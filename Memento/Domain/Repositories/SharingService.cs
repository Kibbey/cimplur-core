using Domain.Models;
using Domain.Entities;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Domain.Exceptions;
using static Domain.Emails.EmailTemplates;
using Domain.Emails;
using log4net;
using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage;

namespace Domain.Repository
{
    public class SharingService : BaseService
    {
        private ILog log = LogManager.GetLogger(nameof(SharingService));

        public List<ConnectionModel> GetConnections(int userId)
        {
            return Context.UserUsers.Where(x => x.OwnerUserId == userId).Select(s =>
                    new ConnectionModel
                    {
                        EmailNotifications = s.SendNotificationEmail,
                        Name = s.ReaderName ?? s.ReaderUser.Name,
                        Id = s.ReaderUserId
                    }
                ).ToList().OrderBy(s => s.Name).ToList();
        }

        public List<ConnectionModel> GetConnectionRequests(int userId) {
            var currentUser = Context.UserProfiles.Single(x => x.UserId == userId);
            var requests = Context.ShareRequests.Where(x => (x.TargetsUserId == userId || x.TargetsEmail.Equals(currentUser.Email))
                                                                && !x.Ignored && !x.Used).ToList();
            
            var ids = requests.Select(r => r.RequesterUserId).ToHashSet();

            var existingConnections = Context.UserUsers.Where(x => x.OwnerUserId == userId && ids.Contains(x.ReaderUserId)).
                Select(s => s.ReaderUserId).ToHashSet();
            // eliminate duplicate requstes making it through
            if (existingConnections.Any()) {
                foreach (var connection in existingConnections) {
                    var existingRequests = requests.Where(x => x.RequesterUserId == connection);
                    Context.ShareRequests.RemoveRange(existingRequests);
                }
                Context.SaveChanges();
                requests = requests.Where(x => !existingConnections.Contains(x.RequesterUserId)).ToList();
            }

            return requests.Select(s =>
                    new ConnectionModel
                    {
                        EmailNotifications = true,
                        Name = s.RequestorName,
                        Token = s.RequestKey,
                        Age = s.CreatedAt
                    }).OrderBy(s => s.Name).ToList(); ;
        }

        public async Task<ReturnModel> RequestConnection(int currentUsrId, ConnectionRequestModel connectionRequestModel, bool sharePlan = false)
        {
            var result = new ReturnModel
            {
                Success = false,
                Message = "Your request failed."
            };
            int? premiumPlanId = null;
            using (var planService = new PlanService())
            {
                var plan = await planService.GetPremiumPlanByUserId(currentUsrId);
                if (plan.ExpirationDate > DateTime.Now)
                {
                    var available = await planService.GetAvaliableFamilyPlanCount(currentUsrId);
                    if (available > 0)
                    {
                        premiumPlanId = plan.PlanId;
                    }

                }
            }

            try
            {
                string tags = JsonSerializer.Serialize(connectionRequestModel.Tags);
                var now = DateTime.UtcNow;
                var request = new ShareRequest
                {
                    RequesterUserId = currentUsrId,
                    RequestKey = Guid.NewGuid(),
                    TargetsEmail = connectionRequestModel.Email,
                    Ignored = false,
                    RequestorName = connectionRequestModel.RequestorName,
                    TargetAlias = connectionRequestModel.ContactName,
                    TagsToShare = tags,
                    CreatedAt = now,
                    UpdatedAt = now,
                    PromptId = connectionRequestModel.PromptId,
                    TimelineId = connectionRequestModel.TimelineId,
                    PremiumPlanId = premiumPlanId
                };
                var targetUser = Context.UserProfiles.FirstOrDefault(x => x.Email.Equals(connectionRequestModel.Email));
                if (targetUser == null)
                {
                    //is we don't have a user then check if their alternate email is available
                    targetUser = Context.UserEmails.Where(x => x.Email.Equals(connectionRequestModel.Email)).Select(s => s.User).FirstOrDefault();
                }
                var currentUser = Context.UserProfiles.Single(x => x.UserId == currentUsrId);
                if (string.IsNullOrWhiteSpace(currentUser.Name))
                {
                    // If they haven't set up a name use this.
                    currentUser.Name = connectionRequestModel.RequestorName;
                }
                EmailTypes template = EmailTypes.ConnectionRequestNewUser;
                if (targetUser != null)
                {
                    request.TargetUser = targetUser;
                    var shareRecord = currentUser.ShareWithUser.FirstOrDefault(x => x.ReaderUserId == targetUser.UserId);
                    if (shareRecord != null)
                    {
                        result.Message = string.Format("You are connectected with {0}.", shareRecord.ReaderName);
                        return result;
                    }
                    if (currentUsrId == targetUser.UserId)
                    {
                        result.Message = "You can not make a sharing request with yourself.";
                        return result;
                    }
                    template = EmailTypes.ConnectionRequest;
                    var notificationService = new NotificationService();
                    notificationService.AddNotificationGeneric(currentUsrId, targetUser.UserId, currentUsrId, NotificationType.Share).Wait();
                }
                var question = string.Empty;
                var timelineName = string.Empty;
                if (connectionRequestModel.PromptId.HasValue) {
                    using (var promptService = new PromptService()) {
                        var prompt = await promptService.GetAskedPrompt(currentUsrId, connectionRequestModel.PromptId.Value);
                        question = prompt.Question;
                        template = EmailTypes.ConnectionRequest == template ? EmailTypes.ConnectionRequestQuestion : EmailTypes.ConnectionRequestNewUserQuestion;
                    }
                }
                if (connectionRequestModel.TimelineId.HasValue)
                {
                    using (var timelineService = new TimelineService())
                    {
                        var timeline = await timelineService.GetTimeline(currentUsrId, connectionRequestModel.TimelineId.Value);
                        timelineName = timeline.Name;
                        template = EmailTypes.ConnectionRequest == template ? EmailTypes.TimelineInviteExisting : EmailTypes.TimelineInviteNew;
                    }
                }

                SendEmailService.SendAsync(connectionRequestModel.Email, template,
                    new { User = connectionRequestModel.RequestorName, 
                        Question = question, 
                        TimelineName = timelineName, 
                        Token = request.RequestKey.ToString() });

                SendEmailService.SendAsync(Constants.Email, EmailTypes.InviteNotice,
                    new { Name = connectionRequestModel.RequestorName });

                Context.ShareRequests.RemoveRange(Context.ShareRequests.Where(x => x.TargetsEmail.Equals(request.TargetsEmail) && x.RequesterUserId.Equals(currentUsrId)));

                Context.ShareRequests.Add(request);

                Context.SaveChanges();
                result.Message = string.Format("Your request has been sent to {0}.", connectionRequestModel.ContactName);
                result.Success = true;
            }
            catch(Exception e)
            {
                log.Error("Share Request", e);
            }

            return result;
        }

        private bool CanRemind(DateTime lastSent) {
            var lastSentThreshold = DateTime.UtcNow.AddDays(-3);
            return lastSent < lastSentThreshold;
        }

        public async Task RequestReminder(int currentUserId, int requestId)
        {
            var invitation = await Context.ShareRequests.SingleOrDefaultAsync(x => x.RequesterUserId == currentUserId 
                && x.ShareRequestId == requestId && !x.Ignored);
            if (invitation == null) throw new NotFoundException();
            if (!CanRemind(invitation.UpdatedAt)) throw new NotAuthorizedException("You can not send a reminder yet.");
            var template = invitation.TargetsUserId.HasValue ? EmailTypes.ConnectionRequest : EmailTypes.ConnectionRequestNewUser;
            SendEmailService.SendAsync(invitation.TargetsEmail, template,
                    new { User = invitation.RequestorName, Token = invitation.RequestKey.ToString() });
            invitation.UpdatedAt = DateTime.UtcNow;
            await Context.SaveChangesAsync();
        }

        public async Task IgnoreRequest(string token, int currentUserId)
        {
            Guid requestToken;
            if (Guid.TryParse(token, out requestToken))
            {
                var request = await Context.ShareRequests.Where(x => x.RequestKey.Equals(requestToken) && !x.Used).FirstOrDefaultAsync().ConfigureAwait(false);
                if (request != null)
                {
                    request.Used = true;
                    await Context.SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        public void RemoveConnection(int userId, int toRemoveuserId)
        {
            var currentUser = Context.UserProfiles.Single(x => x.UserId.Equals(userId));
            var target = Context.UserProfiles.FirstOrDefault(x => x.UserId.Equals(toRemoveuserId));
            if (target != null)
            {
                var shareds = currentUser.SharedWithUser.Where(x => x.ReaderUserId.Equals(userId) && x.OwnerUserId.Equals(target.UserId));
                shareds = shareds.Union(currentUser.ShareWithUser.Where(x => x.OwnerUserId.Equals(userId) && x.ReaderUserId.Equals(target.UserId)));
                Context.UserUsers.RemoveRange(shareds);
                var tagViewers = Context.NetworkViewers.Where(x => (x.UserId == userId && x.UserTag.UserId == target.UserId) ||
                    (x.UserId == target.UserId && x.UserTag.UserId == userId));
                Context.NetworkViewers.RemoveRange(tagViewers);
                Context.SaveChanges();
            }

        }

        public ConnectionRequestModel GetSharingRequest(string token)
        {
            Guid requestToken;
            if (Guid.TryParse(token, out requestToken))
            {
                var request = Context.ShareRequests.Where(x => x.RequestKey.Equals(requestToken) && !x.Used).FirstOrDefault();
                if (request != null)
                {
                    List<long> tags = JsonSerializer.Deserialize<List<long>>(request.TagsToShare);
                    return new ConnectionRequestModel
                    {
                        ContactName = request.TargetAlias ?? request.TargetsEmail,
                        Email = request.TargetsEmail,
                        RequestorName = request.RequestorName,
                        TargetUserId = request.TargetsUserId,
                        Tags = tags
                    };
                }
            }
            return null;
        }

        public async Task<string> UpdateName(int currentUserId, int userIdToChange, string alias)
        {
            var userUser = await Context.UserUsers.Include(i => i.ReaderUser).SingleOrDefaultAsync(x => x.OwnerUserId.Equals(currentUserId) && x.ReaderUser.UserId == userIdToChange);
            if (userUser == null) throw new NotFoundException();
            if (string.IsNullOrWhiteSpace(alias))
            {
                // this reverts to using userName...maybe a feature...maybe a security issue
                alias = userUser.ReaderUser.UserName;
            }
            userUser.ReaderName = alias;
            await Context.SaveChangesAsync().ConfigureAwait(false);
            return alias;
        }

        public async Task<List<OutstandingConnectionRequests>> GetExistingRequests(int currentUserId, int promptId, int timelineId) {
            var shareRequest = Context.ShareRequests.Where(x => x.RequesterUserId == currentUserId && !x.Used && !x.Ignored);
            if (promptId > 0) {
                shareRequest = shareRequest.Where(x => x.PromptId == promptId);
            }
            if (timelineId > 0) {
                shareRequest = shareRequest.Where(x => x.TimelineId == timelineId);
            }
            var sharingRequests = await shareRequest
                .Select(s => new OutstandingConnectionRequests
                {
                    ContactName = s.TargetAlias ?? s.TargetsEmail,
                    RequestId = s.ShareRequestId,
                    Email = s.TargetsUserId.HasValue ? null : s.TargetsEmail,
                    PlanInvite = s.PremiumPlanId.HasValue,
                    UpdatedAt = s.UpdatedAt
                }).ToListAsync();
            sharingRequests.ForEach(s => s.CanSend = CanRemind(s.UpdatedAt));
            return sharingRequests;
        }

        public async Task CancelRequest(int currentUserId, int requestId)
        {
            var request = await Context.ShareRequests.SingleOrDefaultAsync(x => x.RequesterUserId == currentUserId && x.ShareRequestId == requestId);
            if (request != null) {
                request.Ignored = true;
                await Context.SaveChangesAsync();
            }
        }

        public async Task<ViewerNetworksModel> ConfirmationSharingRequest(string token, int userId, string alias)
        {
            Guid requestToken;
            if (Guid.TryParse(token, out requestToken))
            {
                var request = await Context.ShareRequests.Where(x => x.RequestKey.Equals(requestToken) && !x.Used).FirstOrDefaultAsync().ConfigureAwait(false);
                var currentUser = await Context.UserProfiles.SingleAsync(u => u.UserId == userId);
                if (request != null)
                {
                    List<long> groupIds = JsonSerializer.Deserialize<List<long>>(request.TagsToShare);
                    List<GroupModel> tagModels = new List<GroupModel>();
                    if (groupIds != null && groupIds.Any())
                    {
                        var userTags = await Context.UserNetworks.Where(x => groupIds.Contains(x.UserTagId) && request.RequesterUserId == x.UserId).ToListAsync().ConfigureAwait(false);
                        Context.NetworkViewers.AddRange(userTags.Select(s => new TagViewer { UserId = request.RequesterUserId, Viewer = currentUser, UserTag = s }));
                        tagModels = userTags.Select(s => new GroupModel { TagId = s.UserTagId, Name = s.Name }).ToList();
                    }
                    // this is for the person accepting the request
                    string name = string.IsNullOrWhiteSpace(alias) ? request.RequestingUser.Name : alias;
                    string targetName = request.TargetAlias ?? currentUser.Name;
                    Context.UserUsers.Add(new UserUser
                    {
                        OwnerUserId = userId,
                        ReaderUserId = request.RequesterUserId,
                        ReaderName = name,
                        SendNotificationEmail = true
                    });
                    // this is for the person making the request 
                    Context.UserUsers.Add(new UserUser
                    {
                        ReaderUserId = userId,
                        OwnerUserId = request.RequesterUserId,
                        ReaderName = targetName,
                        SendNotificationEmail = true
                    });
                    //mark the requests as used
                    request.Used = true;
                    var requestSharing = await Context.SharingSuggestions
                        .FirstOrDefaultAsync(x => x.OwnerUserId == request.RequesterUserId && x.SuggestedUserId == userId)
                        .ConfigureAwait(false);
                    if (requestSharing != null)
                    {
                        requestSharing.Resolution = Resolutions.Accepted;
                        requestSharing.Resolved = DateTime.UtcNow;
                    }

                    if (request.PremiumPlanId.HasValue) {
                        using (var planService = new PlanService()) {
                            var plan = await planService.GetPremiumPlanById(request.PremiumPlanId.Value);
                            if (plan.ExpirationDate > DateTime.Now) {
                                var available = await planService.GetClaimableFamilyPlanCount(request.PremiumPlanId.Value);
                                if (available > 0) {
                                    await planService.AddPremiumPlan(
                                        currentUser.UserId,
                                        PlanTypes.PremiumFriends,
                                        null,
                                        null,
                                        2,
                                        plan.PlanId
                                    );
                                }
                            }
                        }
                    }

                    await Context.SaveChangesAsync().ConfigureAwait(false);
                    if (request.PromptId.HasValue) {
                        using (var promptService = new PromptService()) {
                            await promptService.AskQuestion(request.RequesterUserId , new List<int> { userId }, request.PromptId.Value);
                        }
                    }

                    var requestor = await Context.UserProfiles.SingleAsync(x => x.UserId.Equals(request.RequesterUserId)).ConfigureAwait(false);
                    Task.Run(() =>
                        SendEmailService.SendAsync(requestor.Email,
                        EmailTemplates.EmailTypes.ConnecionSuccess, new { User = targetName })
                        );
                    await ConnectFamilyPlan(userId).ConfigureAwait(false);
                    await ProcessProfile(userId).ConfigureAwait(false);
                    await new GroupService().PopulateEveryone(userId).ConfigureAwait(false);
                    return new ViewerNetworksModel { Tags = tagModels, Viewer = new PersonModelV2 { Name = name, Id = request.RequestingUser.UserId  } };
                }
            }
            throw new NotFoundException();
        }

        public async Task ConnectFamilyPlan(int userId)
        {
            var currentUser = await Context.UserProfiles.Include(i => i.PremiumPlans).SingleAsync(u => u.UserId == userId)
                .ConfigureAwait(false);
            // Only do this at signup - don't want to open this up to (too much) exploitation
            var now = DateTime.UtcNow.AddMinutes(-60);
            if (currentUser.Created > now && currentUser.PremiumPlans.Any()) {
                var currentPlan = currentUser.PremiumPlans.OrderByDescending(x => x.ExpirationDate).First();
                // figure out if there are family plan members they are not connected to
                var othersOnPlan = await Context.UserProfiles
                    .Where(x => x.UserId != userId)
                    .Where(x => x.PremiumPlans.Any(a => a.ParentPremiumPlanId == currentPlan.ParentPremiumPlanId ||
                        a.PremiumPlanId == currentPlan.ParentPremiumPlanId))
                    .Where(x => !x.ShareWithUser.Any(a => a.ReaderUserId == userId))
                    .ToListAsync().ConfigureAwait(false);
                if (othersOnPlan.Any()) {
                    // Going to add them to family group / network
                    var userIds = othersOnPlan.Select(s => s.UserId).ToHashSet();
                    userIds.Add(userId);
                    var familyGroupByUserId = await Context.UserNetworks
                        .Where(x => x.Name == GroupService.GetFamily && userIds.Contains(x.UserId))
                        .ToDictionaryAsync(k => k.UserId, v => v).ConfigureAwait(false);
                    var name = currentUser.Name;
                    foreach (var familyMember in othersOnPlan)
                    {
                        List<GroupModel> tagModels = new List<GroupModel>();
                        if (familyGroupByUserId.ContainsKey(familyMember.UserId) && familyGroupByUserId.ContainsKey(userId))
                        {
                            Context.NetworkViewers.Add(new TagViewer { 
                                UserId = familyMember.UserId, 
                                Viewer = currentUser, 
                                UserTag = familyGroupByUserId[familyMember.UserId] });

                            Context.NetworkViewers.Add(new TagViewer
                            {
                                UserId = userId,
                                Viewer = familyMember,
                                UserTag = familyGroupByUserId[userId]
                            });
                        }

                        Context.UserUsers.Add(new UserUser
                        {
                            OwnerUserId = userId,
                            ReaderUserId = familyMember.UserId,
                            ReaderName = familyMember.Name,
                            SendNotificationEmail = true
                        });

                        Context.UserUsers.Add(new UserUser
                        {
                            ReaderUserId = userId,
                            OwnerUserId = familyMember.UserId,
                            ReaderName = name,
                            SendNotificationEmail = true
                        });
                    }
                    // if we have any sharing suggestion - mark as resolved.
                    var sharingSuggestions = await Context.SharingSuggestions
                        .Where(x => (userIds.Contains(x.OwnerUserId) && x.SuggestedUserId == userId) 
                            || (x.OwnerUserId == userId && userIds.Contains(x.SuggestedUserId)))
                        .ToListAsync().ConfigureAwait(false);
                    sharingSuggestions.ForEach(s =>
                    {
                        s.Resolution = Resolutions.AutoConnected;
                        s.Resolved = DateTime.UtcNow;
                    });
                    // if anyone made the request - mark as used
                    var sharingRequests = await Context.ShareRequests
                        .Where(x => userIds.Contains(x.RequesterUserId) && x.TargetsEmail == currentUser.Email)
                        .ToListAsync().ConfigureAwait(false);
                    sharingRequests.ForEach(s =>
                    {
                        s.Used = true;
                        s.UpdatedAt = DateTime.UtcNow;
                    });

                    await Context.SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }


        public async Task Ignore(int userId, int suggestedUserId) {
            var suggestion = await Context.SharingSuggestions.Where(x => x.OwnerUserId == userId && x.SuggestedUserId == suggestedUserId)
                .FirstOrDefaultAsync().ConfigureAwait(false);
            if (suggestion != null) {
                suggestion.Resolution = Resolutions.Rejected;
                suggestion.Resolved = DateTime.UtcNow;
                await Context.SaveChangesAsync().ConfigureAwait(false);
                await ProcessProfile(userId).ConfigureAwait(false);
            }
        }

        public async Task<ReturnModel> RequestSuggestedConnection(int userId, int suggestedUserId, List<long> groupIds)
        {
            groupIds = groupIds ?? new List<long>();

            var target = await Context.UserProfiles.SingleAsync(x => x.UserId == suggestedUserId);
            var requestor = await Context.UserProfiles.SingleAsync(x => x.UserId == userId);
            // This will throw if they don't have a suggestion - no cheating!
            var suggestion = await Context.SharingSuggestions.SingleAsync(x => x.OwnerUserId == userId && x.SuggestedUserId == suggestedUserId);

            var result = await RequestConnection(userId, new ConnectionRequestModel {
                ContactName = target.Name,
                Email = target.Email,
                RequestorName = requestor.Name,
                TargetUserId = suggestedUserId,
                Tags = groupIds
            });
            suggestion.Resolution = result.Success ? Resolutions.Accepted : Resolutions.AlreadyConnected;
            suggestion.Resolved = DateTime.UtcNow;
            await Context.SaveChangesAsync();

            await ProcessProfile(userId).ConfigureAwait(false);
            return result;           
        }

        public async Task<List<SuggestionModel>> GetSuggestions(int userId) {
            await ProcessProfile(userId);
            var sharingSuggestions = await Context.SharingSuggestions.Include(i => i.SuggestedUser)
                .Where(x => x.OwnerUserId == userId && x.Resolution == Resolutions.NotResolved)
                .OrderByDescending(x => x.Points)
                //.Take(5) eventually we may want to skip take - but for now keep it simple
                .Select(s => new SuggestionModel {
                    Name = s.SuggestedUser.Name,
                    Id = s.SuggestedUserId,
                    SharedContacts = s.SuggestedUser.ShareWithUser.Where(x => s.OwnerUser.ShareWithUser.Any(a => a.ReaderUserId == x.ReaderUserId))
                        .Select(c => c.ReaderName ?? c.ReaderUser.Name),
                    SuggestedTags = Context.UserNetworks.Where(t => t.TagViewers.Any(a => a.Viewer.UserId == s.OwnerUserId) 
                                            && t.TagViewers.Any(a => a.Viewer.UserId == s.SuggestedUserId))
                                            .Where(x => x.Name != GroupService.GetEveryone)
                                            .Select(t => new GroupModel {
                                                TagId = t.UserTagId,
                                                IsTask = false,
                                                Archived = false,
                                                Foreign = true,
                                                Name = t.Name
                                            }).ToList()
                    
                } ).ToListAsync().ConfigureAwait(false);
            if (sharingSuggestions.Any()) {
                var existingConnections = await Context.UserUsers.Where(x => x.OwnerUserId == userId)
                    .Select(s => s.ReaderUserId).ToListAsync().ConfigureAwait(false);
                var alreadyExisting = sharingSuggestions.Where(x => existingConnections.Any(a => a == x.Id))
                    .Select(s => s.Id).ToList();
                if (alreadyExisting.Any()) {
                    sharingSuggestions.RemoveAll(x => alreadyExisting.Any(a => a == x.Id));
                    await MarkAsExisting(userId, alreadyExisting).ConfigureAwait(false);
                }
            }
            return sharingSuggestions;
        }

        public async Task MarkAsExisting(int userId, List<int> targetUserIds) {
            var sharingSuggestions = await Context.SharingSuggestions.Where(x => x.OwnerUserId == userId && targetUserIds.Any(a => a == x.SuggestedUserId))
                .ToListAsync().ConfigureAwait(false);
            sharingSuggestions.ForEach(x => {
                x.Resolution = Resolutions.Accepted;
                x.Resolved = DateTime.UtcNow;
                });
            await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task ProcessSharing() {
            var now = DateTime.UtcNow;
            var lastUpdate = now.AddHours(-6);
            var profiles = await Context.UserProfiles.Where(x => !x.SuggestionUpdated.HasValue || x.SuggestionUpdated < lastUpdate)
                .OrderBy(x => x.SuggestionUpdated).Take(10).ToListAsync().ConfigureAwait(false);
            foreach (var profile in profiles) {
                profile.SuggestionUpdated = now;
            } 
            await Context.SaveChangesAsync().ConfigureAwait(false);
            foreach (var profile in profiles.ToList()) {
                await ProcessProfile(profile.UserId).ConfigureAwait(false);
            }
            await ProcessSendSharingReminders().ConfigureAwait(false);
            await ProcessQuestionReminders().ConfigureAwait(false);
        }

        private async Task ProcessQuestionReminders()
        {
            var now = DateTime.UtcNow;
            var lastUpdate = now.AddDays(-1);
            if (now.DayOfWeek == DayOfWeek.Saturday && now.Hour > 14)
            {
                var profiles = new List<UserProfile>();
                do
                {
                    // multiple servers
                    using (IDbContextTransaction transaction = Context.Database.BeginTransaction(IsolationLevel.RepeatableRead))
                    {
                        try
                        {
                            profiles = await Context.UserProfiles.Where(x => x.QuestionRemindersSent < lastUpdate)
                                .Take(20).ToListAsync().ConfigureAwait(false);
                            profiles.ForEach(x => x.QuestionRemindersSent = now);
                            await Context.SaveChangesAsync().ConfigureAwait(false);
                            transaction.Commit();
                            foreach (var toSend in profiles)
                            {
                                await FindAndEmailQuestions(toSend.UserId, toSend.Email).ConfigureAwait(false);
                            }
                        }
                        catch (Exception ex)
                        {
                            //transaction.Rollback();
                            log.Error(ex);
                        }
                    }
                } while (profiles.Any());
            }
        }

        private async Task ProcessSendSharingReminders() {
            var now = DateTime.UtcNow;
            var lastUpdate = now.AddDays(-1);
            if (now.DayOfWeek == DayOfWeek.Sunday && now.Hour > 15)
            {
                var profiles = new List<UserProfile>();
                do
                {
                    // multiple servers
                    using (IDbContextTransaction transaction = Context.Database.BeginTransaction(IsolationLevel.RepeatableRead))
                    {
                        try
                        {
                            profiles = await Context.UserProfiles.Where(x => x.SuggestionReminderSent < lastUpdate)
                                .Take(20).ToListAsync().ConfigureAwait(false);
                            profiles.ForEach(x => x.SuggestionReminderSent = now);
                            await Context.SaveChangesAsync().ConfigureAwait(false);
                            transaction.Commit();
                            foreach (var toSend in profiles)
                            {
                                await FindAndEmailSuggestions(toSend.UserId, toSend.Email).ConfigureAwait(false);
                            }
                        }
                        catch (Exception ex)
                        {
                            //transaction.Rollback();
                            log.Error(ex);
                        }
                    }
                } while (profiles.Any());
            }
        }

        private async Task FindAndEmailSuggestions(int userId, string email) {
            var expiration = DateTime.UtcNow.AddDays(-30);
            var requests = this.GetConnectionRequests(userId)
                .OrderByDescending(x => x.Age)
                .Where(x => x.Age > expiration);
            var recentRequest = requests.Any(a => a.Age > DateTime.UtcNow.AddDays(-1));
            var suggestions = await this.GetSuggestions(userId).ConfigureAwait(false);
            if (requests.Any() && !recentRequest)
            {
                requests.Select(s => s.Name);
                await SendEmailService.SendAsync(email, EmailTypes.Requests,
                    new { Name = requests.First() }).ConfigureAwait(false);
            }
            else if (suggestions.Any())
            {
                var suggestionName = suggestions.OrderByDescending(o => o.Id).Select(s => s.Name).First();
                await SendEmailService.SendAsync(email, EmailTypes.Suggestions,
                    new { Name = suggestionName }).ConfigureAwait(false);
            }
        }

        private async Task FindAndEmailQuestions(int userId, string email)
        {
            using (var promptService = new PromptService()) {
                var prompts = await promptService.GetPromptsAskedToMe(userId).ConfigureAwait(false);
                if (prompts.Any()) {
                    var prompt = prompts.OrderByDescending(o => o.PromptId).First();
                    await SendEmailService.SendAsync(email, EmailTypes.QuestionReminders,
                     new { Name = prompt.Askers.FirstOrDefault()?.Name ?? "", prompt.Question }).ConfigureAwait(false);
                }
            }
        }

        private async Task UpdateSuggestionNotifications(int userId, bool ignoreChanges = false) {
            var profile = await Context.UserProfiles.SingleAsync(x => x.UserId == userId).ConfigureAwait(false);
            var suggestionIds = await Context.SharingSuggestions.Where(x => x.OwnerUserId == userId && x.Resolution == Resolutions.NotResolved)
                    .Select(s => s.SuggestedUserId).OrderBy(x => x)
                    .ToListAsync().ConfigureAwait(false);
            string suggestionIdStrings = JsonSerializer.Serialize(suggestionIds);
            if (!ignoreChanges)
            {
                profile.NotififySuggestions = suggestionIdStrings != profile.CurrentSuggestedPeople;
            }
            else {
                profile.NotififySuggestions = false;
            }

            profile.CurrentSuggestedPeople = suggestionIdStrings;
            // if they don't match that means we have a new set of
            await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        private async Task ProcessProfile(int userId) {
            var profile = await Context.UserProfiles.SingleAsync(x => x.UserId == userId).ConfigureAwait(false);
            var similarViewersList = await Context.NetworkViewers.Include(i => i.Viewer.ShareWithUser)
                //.Where(x => x.UserTag.Name != GroupService.GetEveryone)
                .Where(x => userId == x.Viewer.UserId).SelectMany(s => s.UserTag.TagViewers)
                .Where(x => userId != x.Viewer.UserId && !x.Viewer.SharedWithUser.Any(a => a.OwnerUserId == userId))
                .ToListAsync().ConfigureAwait(false);
            var similarViewers = similarViewersList
                .GroupBy(x => x.Viewer.UserId);
            var keys = similarViewers.Select(s => s.Key);
            var suggestions = Context.SharingSuggestions.Where(x => x.OwnerUserId == userId &&
                keys.Contains(x.SuggestedUserId)).ToList();
            foreach (var viewer in similarViewers) {
                var suggestion = suggestions.FirstOrDefault(x => x.SuggestedUserId == viewer.Key);
                if (suggestion == null) {
                    suggestion = new SharingSuggestion {
                        CreatedAt = DateTime.UtcNow,
                        OwnerUserId = userId,
                        SuggestedUserId = viewer.Key,
                        Resolution = Resolutions.NotResolved,
                    };
                    Context.SharingSuggestions.Add(suggestion);
                }
                if (suggestion.Resolution == Resolutions.NotResolved) {
                    suggestion.Reason = "TODO";
                    suggestion.Points = viewer.Count();
                }
            }
            var sharingRequestsOutOfDate = await Context.SharingSuggestions
                .Where(x => x.Resolution == Resolutions.NotResolved && x.OwnerUserId == userId)
                .Where(x => x.OwnerUser.ShareWithUser.Any(a => a.ReaderUserId == x.SuggestedUserId)).ToListAsync()
                .ConfigureAwait(false);
            Context.SharingSuggestions.RemoveRange(sharingRequestsOutOfDate);

            try {
                profile.SuggestionUpdated = DateTime.UtcNow;
                await Context.SaveChangesAsync().ConfigureAwait(false);
                await UpdateSuggestionNotifications(userId);
            } catch (Exception e) {
                log.Error(e);
            }
        }
    }
}
