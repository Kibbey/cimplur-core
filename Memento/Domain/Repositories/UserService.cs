using Domain.Models;
using Domain.Entities;
using Domain.Utilities;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Domain.Exceptions;
using static Domain.Emails.EmailTemplates;
using System.Collections.Generic;
using log4net;
using Domain.Emails;
using Newtonsoft.Json;

namespace Domain.Repository
{
    public class UserService : BaseService
    {
        private NotificationService notificationService;
        private SendEmailService sendEmailService;
        private DropsService dropService;
        private AlbumService albumService;
        private TokenService tokenService;


        public UserService(
            NotificationService notificationService,
            SendEmailService sendEmailService,
            DropsService dropsService,
            AlbumService albumService,
            TokenService tokenService)
        {
            this.notificationService = notificationService;
            this.albumService = albumService;
            this.dropService = dropsService;
            this.sendEmailService = sendEmailService;
            this.tokenService = tokenService;
        }
        public async Task<int> AddUser(string email, string userName, 
            string token, bool acceptTerms, string name, List<ReasonModel> reasons)
        {
            var user = await Context.UserProfiles.SingleOrDefaultAsync(x => x.UserName == userName);
            if (user != null) {
                throw new BadRequestException("An account already exists with this email.  Please login.");
            }
            user = new UserProfile();
            user.UserName = userName;
            user.Email = email;
            user.Created = DateTime.UtcNow;
            user.SuggestionReminderSent = DateTime.UtcNow;
            user.QuestionRemindersSent = DateTime.UtcNow;
            user.Name = name;
            if (reasons != null) {
                user.Reasons = JsonConvert.SerializeObject(reasons);
            }
            if (acceptTerms) {
                user.AcceptedTerms = DateTime.UtcNow;
                // Free premium for a 60 days
                user.PremiumExpiration = DateTime.UtcNow.AddYears(10);
            }
            if (string.IsNullOrWhiteSpace(token))
            {
                Guid userToken;
                if (Guid.TryParse(token, out userToken))
                {
                    var request = Context.ShareRequests.FirstOrDefault(x => x.RequestKey == userToken);
                    if (request != null)
                    {
                        request.TargetsUserId = user.UserId;
                    }
                }
            }
            Context.UserProfiles.Add(user);
            await Context.SaveChangesAsync();
            return user.UserId;
        }

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

        public async Task<UserModel> ChangeName(int currentUserId, string name)
        {
            var user = await Context.UserProfiles.SingleOrDefaultAsync(x => x.UserId == currentUserId);
            if (user == null) throw new NotFoundException();
            if (string.IsNullOrWhiteSpace(name)) {
                name = user.UserName;
            }
            user.Name = name;
            await Context.SaveChangesAsync();
            return new UserModel { Name = user.Name };
        }

        public static Dictionary<string, string> GetVariants(int currentUserId) {
            var variants = new Dictionary<string, string>();
            bool even = currentUserId % 2 == 0;
            variants.Add("customQuestion", even ? "variant" : "");
            variants.Add("gettingStarted", even ? "gettingStarted" : "gettingStarted");
            // variants.Add("relationships", even ? "gettingStarted" : "gettingStarted");
            return variants;
        }

        public async Task<UserModel> GetUser(int currentUserId)
        {
            var now = DateTime.UtcNow;
            var user = await Context.UserProfiles.Include(i => i.PremiumPlans).SingleOrDefaultAsync(x => x.UserId == currentUserId);
            if (user == null) throw new NotFoundException();
            var userModel = new UserModel {
                Name = user.Name ?? user.UserName,
                CanShareDate = user.PremiumExpiration.HasValue && user.PremiumExpiration.Value > now ? user.PremiumExpiration : null,
                PremiumMember = user.PremiumExpiration.HasValue && user.PremiumExpiration.Value > now,
                Variants = GetVariants(currentUserId),
                PrivateMode = user.PrivateMode,
                Email = user.Email
            };
            return userModel;
        }

        public async Task<ProfileModel> UpdatePrivateMode(int currentUserId, bool privateMode) {
            var user = await Context.UserProfiles.SingleOrDefaultAsync(x => x.UserId == currentUserId);
            if (user == null) throw new NotFoundException();
            user.PrivateMode = privateMode;
            await Context.SaveChangesAsync();
            return await GetProfile(currentUserId);
        }

        public async Task<ProfileModel> GetProfile(int currentUserId)
        {
            var user = await Context.UserProfiles.SingleOrDefaultAsync(x => x.UserId == currentUserId)
                .ConfigureAwait(false);
            if (user == null) throw new NotFoundException();
            var premium = user.PremiumExpiration.HasValue && user.PremiumExpiration > DateTime.UtcNow;
            return new ProfileModel { 
                Name = user.Name ?? user.UserName, 
                Id = user.UserId, 
                Email = user.Email, 
                PremiumMember = premium, 
                PrivateMode = user.PrivateMode
            };
        }

        public void IgnoreConnectionRequest(int userId, string email)
        {
            var currentUser = Context.UserProfiles.Single(x => x.UserId.Equals(userId));
            var requester = Context.UserProfiles.FirstOrDefault(x => x.Email.Equals(email));
            if (requester != null)
            {
                //find all request
                var requests = Context.ShareRequests.Where(x => x.RequesterUserId.Equals(requester.UserId)
                    && (x.TargetsUserId.Equals(userId) || x.TargetsEmail.Equals(currentUser.Email)));
                //make sure we have some 
                if (requests.Any())
                {
                    foreach (var request in requests)
                    {
                        request.Ignored = true;
                    }
                    Context.SaveChanges();
                }
            }
            //throw new NotImplementedException();
        }

        public void EnableEmailNotificationsOfPosts(int userId, int targetUserId)
        {
            var currentUser = Context.UserProfiles.Single(x => x.UserId.Equals(userId));
            var connection = currentUser.ShareWithUser.FirstOrDefault(x => x.ReaderUser.UserId == targetUserId);
            if (connection != null)
            {
                connection.SendNotificationEmail = !connection.SendNotificationEmail;
                Context.SaveChanges();
            }
        }

        public string GetEmail(string userName)
        {
            return Context.UserProfiles.Single(x => x.UserName == userName).Email;
        }

        public async Task<string> GetEmail(int userId)
        {
            var user = await Context.UserProfiles.SingleAsync(x => x.UserId == userId);
            return user.Email;
        }

        public void ViewNotification(int userId, int notificationId)
        {
            //remove all tags and remove all people and set people == to person
            var notification = Context.SharedDropNotifications.FirstOrDefault(x => x.TargetUserId.Equals(userId) && x.SharedDropNotificationId.Equals(notificationId));
            if (notification != null)
            {
                if (notification.DropId.HasValue) {
                    notificationService.ViewNotification(userId, notification.DropId.Value);
                    notificationService.Dispose();
                }
                Context.SharedDropNotifications.Remove(notification);
                Context.SaveChanges();
            }
        }

        public bool CheckEmail(string email)
        {
            return Context.UserProfiles.Any(x => x.Email.Equals(email));
        }

        public string GetUserName(string email)
        {
            return Context.UserProfiles.Where(x => x.Email.Equals(email)).Select(s => s.UserName).FirstOrDefault();
        }

        public async Task<string> SendClaimEmail(int CurrentUserId, string email)
        {
            //check for email already used / claimed
            if (Context.UserProfiles.Any(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                || Context.UserEmails.Any(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && x.Confirmed))
            {
                return "Please check that this is your email and you have not already claimed it.";
            }
            //if not then save it
            var userEmail = new UserEmail
            {
                UserId = CurrentUserId,
                Token = Guid.NewGuid().ToString(),
                Confirmed = false,
                Email = email,
                TokenExpiration = DateTime.Now.AddHours(24)
            };
            Context.UserEmails.Add(userEmail);
            Context.SaveChanges();
            //send it 
            var currentUserEmail = Context.UserProfiles.Single(x => x.UserId.Equals(CurrentUserId)).Email;
            await sendEmailService.SendAsync(email, EmailTypes.ClaimEmail,
                   new { User = currentUserEmail, Token = userEmail.Token });

            return string.Format("Email has been sent to {0}.", email);
        }

        public string ClaimEmail(string token)
        {
            var now = DateTime.Now;
            var userEmail = Context.UserEmails.FirstOrDefault(x => x.Token.Equals(token) && !x.Confirmed && x.TokenExpiration >= now);
            if (userEmail == null)
            {
                return "This request was not found or has expired.";
            }
            string email = userEmail.Email;
            //check for email already used / claimed
            if (Context.UserProfiles.Any(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                || Context.UserEmails.Any(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && x.Confirmed))
            {
                //be vague for security reasons.
                return "This request can not be completed.";
            }
            else
            {
                userEmail.Confirmed = true;
                Context.UserEmails.RemoveRange(Context.UserEmails.Where(x => x.Email.Equals(email) && !x.Token.Equals(token)));
                Context.SaveChanges();
                return string.Format("You have successfully claimed {0}.", email);
            }
        }

        public async Task<FiltersModel> GetFilter(int currentUserId)
        {
            // this is a pretty expensive call and should be optimized later
            UserProfile userProfile = Context.UserProfiles.Single(x => x.UserId == currentUserId);
            var albums = new List<AlbumModel>();

            albums = await albumService.GetActive(currentUserId);

            int year = DateTime.UtcNow.Year;
            int oldestYear = 0;
            var oldestDrop = await this.GetOldestDropYear(currentUserId);
            if (oldestDrop != null) {
                oldestYear = oldestDrop.Date.Year;
            }
            else
            {
                oldestYear = year;
            }
            
            var years = new List<int>();
            for (var i = year; i >= oldestYear; i--) {
                years.Add(i);
            }
            var currentPeople = MapUserProfile.CurrentPeople(userProfile).Select(s => s.Id).ToHashSet();
            var tagModels = new FiltersModel
            {
                AllPeople = Context.UserUsers.Where(x => x.OwnerUserId == currentUserId).Select(s =>
                    new PersonModelV2 { Name = s.ReaderName ?? s.ReaderUser.Name, Id = s.ReaderUser.UserId })
                    .ToList()
                    .Select(s => new PersonSelectedModel { Name = s.Name, Id = s.Id, Selected = currentPeople.Contains(s.Id) }).ToList(),
                Albums = albums.Select(s => new AlbumViewModel { AlbumId = s.AlbumId, Name = s.Name, Selected = false }).ToList(),
                Me = userProfile.Me,
                Years = years
            };
            return tagModels;
        }


        public async Task UpdateMe(int currentUserId, bool me)
        {
            var userProfile = await Context.UserProfiles.SingleAsync(x => x.UserId == currentUserId);
            if (me != userProfile.Me) {
                userProfile.Me = me;
                await Context.SaveChangesAsync();
            }
        }

        public async Task<string> GiveFeedbackReceived(string token)
        {
            try
            {
                var validatedToken = await this.tokenService.GetTokenValue(token);
                var user = await Context.UserProfiles.SingleAsync(x => x.UserId == validatedToken.UserId);
                user.GiveFeedback = DateTime.UtcNow;
                await Context.SaveChangesAsync();
                return user.Name;
            }
            catch (Exception e)
            {
                logger.Error($"Welcome Issue - {token}", e);
            }
            return null;
        }

        /// <summary>
        /// How this user sees others
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <returns></returns>
        public async Task<Dictionary<int, string>> GetNameDictionary(int currentUserId) {
            return await Context.UserUsers.Where(x => x.OwnerUserId == currentUserId).ToDictionaryAsync(k => k.ReaderUserId,
                v => v.ReaderName ?? v.ReaderUser.Name);
        }


        /// <summary>
        /// Get all relationships selected
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <returns></returns>
        public async Task<List<SelectModel>> GetRelationships(int currentUserId) {
            var relationships = await Context.UserRelationships.Where(x => x.UserId == currentUserId)
                .ToDictionaryAsync(k => k.Relationship, v => true);
            var relationshipList = EnumIterator.GetValues<Relationships>();
            relationshipList.ToList().ForEach(x => { if (!relationships.ContainsKey(x)) {
                    relationships.Add(x, false);
                } } );
            return relationships.Select(s => new SelectModel {
                Id = (int)s.Key,
                Name = s.Key.ToString().Replace('_',' '),
                Selected = s.Value
            }).OrderBy(x => x.Id).ToList();
        }

        public async Task<List<SelectModel>> UpdateRelationships(List<int> selected, int currentUserId) {
            var relationships = await Context.UserRelationships.Where(x => x.UserId == currentUserId).ToListAsync();
            foreach(var existing in relationships) {
                if (!selected.Contains((int)existing.Relationship)) {
                    Context.UserRelationships.Remove(existing);
                }
                selected.RemoveAll(x => x == (int)existing.Relationship);
            }
            foreach (var relationship in selected) {
                Context.UserRelationships.Add(new UserRelationship
                {
                    Relationship = (Relationships)relationship,
                    UserId = currentUserId,
                    CreatedAt = DateTime.UtcNow
                });
            }
            await Context.SaveChangesAsync();
            return await GetRelationships(currentUserId);
        }

        private async Task<DropModel> GetOldestDropYear(int currentUserId)
        {
            int year = DateTime.UtcNow.Year;
            var dropsViewModel = await dropService.GetDrops(currentUserId, new List<int>(), new List<int>(), false, true, null, year, 0, true);
            return dropsViewModel.Drops.FirstOrDefault();
        }

        /*
        public async Task<OneTimePasswordModel> CreateOneTimePassword(string email) {
            var user = await Context.UserProfiles.SingleOrDefaultAsync(x => x.Email.Equals(email));
            var result = new OneTimePasswordModel { Success = false };
            if (user == null) return result;
            var now = DateTime.UtcNow;
            if (user.TokenCreation.HasValue) {
                // Keep people from double tapping and invalidating the one just sent.
                // This also makes it harder to brute force.
                if (user.TokenCreation.Value > now.AddSeconds(-300))
                {
                    return result;
                }
            }
            var userToken = new OneTimePasswordModel { Success = true, Password = CreatePassword() };
            user.Token = userToken.Password;
            user.TokenCreation = now;
            user.TokenAttempts = 0;
            await Context.SaveChangesAsync();
            return userToken;
        }

        public async Task<string> ValidatePassword(string token, string email) {
            var validCreation = DateTime.UtcNow.AddMinutes(-expirationInMinutes);
            var user = await Context.UserProfiles.SingleOrDefaultAsync(x => x.Email.Equals(email) 
                && x.TokenCreation > validCreation
                && x.TokenAttempts < 9);
            if (user == null) { return null; }
            bool isValid = user.Token == token;
            user.TokenAttempts = isValid ? 999 : ++user.TokenAttempts;
            user.TokenCreation = isValid ? null : user.TokenCreation;
            await Context.SaveChangesAsync();
            return isValid ? user.UserName : null;
        }
        */


        private static string CreateToken()
        {
            var password = "";
            for (var i = 0; i < 6; i++) {
                password += random.Next(0, 10).ToString();
                password += GetLetter();
            }
            return password;
        }

        private static Random random = new Random();
        private static string GetLetter()
        {
            int num = random.Next(0, 26);
            char let = (char)('a' + num);
            return let.ToString();
        }

        private ILog logger = LogManager.GetLogger(nameof(UserService));
    }
}
