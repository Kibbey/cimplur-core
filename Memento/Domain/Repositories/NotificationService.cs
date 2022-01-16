using Domain.Models;
using Domain.Emails;
using Domain.Utilities;
using log4net;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using static Domain.Emails.EmailTemplates;

namespace Domain.Repository
{
    public class NotificationService : BaseService
    {
        public NotificationService(SendEmailService sendEmailService, GroupService groupService) {
            this.sendEmailService = sendEmailService;
            this.groupService = groupService;
        }

        private SendEmailService sendEmailService;
        private GroupService groupService;
        private ILog logger = log4net.LogManager.GetLogger(typeof(NotificationService).Name);

        public async Task AddNotificationDropAdded(int userId, HashSet<long> networkIds, int dropId)
        {
            // failure to add a notification shouldn't fubar the system
            try
            {
                var tagUsers = await groupService.GetUsersToShareWith(userId, networkIds)
                    .ConfigureAwait(false);
                var user = await Context.UserProfiles.SingleAsync(x => x.UserId == userId).ConfigureAwait(false);

                foreach (var viewer in tagUsers)
                {
                    await AddNotificationGeneric(userId, viewer, dropId, NotificationType.Memory);
                    var targetUser = await Context.UserUsers.Include(i => i.OwnerUser)
                        .Where(x => x.OwnerUserId == viewer && x.ReaderUserId == userId)
                        .SingleOrDefaultAsync().ConfigureAwait(false);
                    if (targetUser.SendNotificationEmail)
                    {
                        var last = DateTime.UtcNow.AddHours(-2);
                        var lastNotification = await Context.SharedDropNotifications.
                            FirstOrDefaultAsync(x => x.TargetUserId == viewer
                        && x.SharerUserId == userId && x.TimeShared > last).ConfigureAwait(false);
                        //only send an email if they haven't gotten a notification in the last hour
                        if (lastNotification == null)
                        {
                            await groupService.SendEmail(targetUser.OwnerUser.Email, targetUser.ReaderName ?? user.UserName, dropId, EmailTypes.EmailNotification);
                            Context.SharedDropNotifications.Add(new Entities.SharedDropNotification {
                                TargetUserId = viewer,
                                DropId = dropId,
                                SharerUserId = userId,
                                TimeShared = DateTime.UtcNow
                            });
                            await Context.SaveChangesAsync().ConfigureAwait(false);
                        }
                    }
                }
            } catch (Exception e) {
                logger.Error(e);
            }
        }

        /// <summary>
        /// Remove any notifications (comment or memory) for a given drop and user.
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="dropId"></param>
        /// <returns></returns>
        public async Task RemoveNotification(int currentUserId, int dropId)
        {
            var notifications = Notifications(currentUserId);
            int count = notifications.Notifications.Count;
            // remove suggestions (so they don't get added)
            notifications.Notifications.RemoveAll(x => x.DropId == dropId || x.NotificationType == NotificationType.Suggestion);
            if (notifications.Notifications.Count != count) {
                var user = await Context.UserProfiles.SingleAsync(x => x.UserId == currentUserId).ConfigureAwait(false);
                var currentNotifications = JsonSerializer.Serialize(notifications.Notifications);
                user.CurrentNotifications = currentNotifications;
                await Context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public NotificationsModel Notifications(int userId)
        {
            var notifications = new NotificationsModel();
            var user = Context.UserProfiles.Single(x => x.UserId == userId);
            var currentNotifications = user.CurrentNotifications;
            if (!string.IsNullOrWhiteSpace(currentNotifications))
            {
                notifications.Notifications = JsonSerializer.Deserialize<List<NotificationModel>>(currentNotifications);
                notifications.Notifications = RemoveExpired(notifications.Notifications);
                currentNotifications = JsonSerializer.Serialize(notifications.Notifications);
                user.CurrentNotifications = currentNotifications;
                Context.SaveChanges();
                notifications.Notifications.ForEach(x => x.Name.ToUpper());
                notifications.Notifications = notifications.Notifications.OrderByDescending(x => x.CreatedAt).ToList();
                notifications.NotificationHash = currentNotifications;
            }
            if (user.CurrentSuggestedPeople != null && user.CurrentSuggestedPeople.Length > 3) {
                notifications.Notifications.Insert(0, new NotificationModel {
                    CreatedAt = DateTime.UtcNow,
                    DropId = -1,
                    Name = String.Empty,
                    Viewed = !user.NotififySuggestions, //this turns the bell red if false
                    ViewedAt = null,
                    NotificationType = NotificationType.Suggestion
                });
            }
            return notifications;
        }

        public async Task ViewedSuggestions(int userId) {
            var profile = await Context.UserProfiles.SingleAsync(x => x.UserId == userId).ConfigureAwait(false);
            profile.NotififySuggestions = false;
            await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        private List<NotificationModel> RemoveExpired(List<NotificationModel> notifications, bool forceRemove = false)
        {
            var now = DateTime.UtcNow;
            var limit = now.AddDays(-3);
            int previous = notifications.Count;
            notifications.RemoveAll(x => x.Viewed && x.ViewedAt < limit);
            forceRemove = forceRemove && previous == notifications.Count;
            if (forceRemove)
            {
                var notificationToRemove = notifications.ToList().OrderBy(x => x.CreatedAt).First();
                notifications.Remove(notificationToRemove);
            }
            return notifications;
        }

        // TODO - create a bulk endpoint where we can do this more efficiently
        public async Task<bool> AddNotificationGeneric(int userId, int targetUserId, int? typeId, NotificationType notificationType, EmailTypes? emailTypes = null, ExpandoObject payload = null)
        {
            var sendEmail = false;
            try
            {
                var user = await Context.UserUsers.Where(x => x.OwnerUserId == targetUserId && x.ReaderUserId == userId)
                    .Include(i => i.ReaderUser).FirstOrDefaultAsync().ConfigureAwait(false);
                var name = string.Empty;
                if (user == null) {
                    var userProfile = await Context.UserProfiles.SingleOrDefaultAsync(x => x.UserId == userId);
                    name = userProfile.Name;
                }
                else
                {
                    name = user.ReaderName ?? user.ReaderUser.Name;
                }
                //find the user they just added to - person
                var targetUser = await Context.UserProfiles.SingleOrDefaultAsync(x => x.UserId == targetUserId).ConfigureAwait(false);
                //get their notifications
                if (targetUser != null)
                {
                    //add to notifications
                    var notifications = targetUser.CurrentNotifications ?? string.Empty;
                    bool removeOld = notifications.Length > 7000;
                    var currentNotifcations = JsonSerializer.Deserialize<List<NotificationModel>>(notifications) ?? new List<NotificationModel>();

                    currentNotifcations = RemoveExpired(currentNotifcations, removeOld);
                    // if we already told them about this in anothet context and they have not viewed it - then skip the email.
                    sendEmail = !currentNotifcations.Any(a => a.DropId == typeId && !a.Viewed && a.NotificationType == notificationType);
                    var now = DateTime.UtcNow;
                    // We want the latest notification to show - so if they have a similar one from earlier remove it first.
                    currentNotifcations.RemoveAll(x => x.DropId == typeId && x.NotificationType == notificationType);
                    // So this sucks but DropId should be a generic id -> can be an actual drop OR it can be a promptId...and probably more in the future
                    // If you are reading this - don't hate me - stupid short cuts like this got us to where we are now - so smile and get some coffee
                    // And fix my stupid code...or don't for now so someone in the future can hate us both!
                    currentNotifcations.Add(new NotificationModel { Name = name, DropId = typeId ?? 0, CreatedAt = now, Viewed = false, NotificationType = notificationType });
                    
                    targetUser.CurrentNotifications = JsonSerializer.Serialize(currentNotifcations.Distinct(new NotificationModelComparer()).OrderBy(x => x.CreatedAt));                                

                    await Context.SaveChangesAsync().ConfigureAwait(false);
                    if (emailTypes.HasValue) {
                        var payloadExtended = (IDictionary<string, object>)payload;
                        payloadExtended.Add("User", name);
                        await this.sendEmailService.SendAsync(targetUser.Email, emailTypes.Value, payload);
                    }
                }
            } catch (Exception e) {
                logger.Error(e);
            }
            return sendEmail;
        }

        public void ViewNotification(int userId, int dropId)
        {
            var user = Context.UserProfiles.First(x => x.UserId == userId);
            var notificationsString = user.CurrentNotifications;
            if (!string.IsNullOrWhiteSpace(notificationsString))
            {
                var currentNotifcations = JsonSerializer.Deserialize<List<NotificationModel>>(notificationsString);
                var notifications = currentNotifcations?.Where(x => x.DropId == dropId);
                if (notifications != null && notifications.Any())
                {
                    if (notifications.Any(x => x.NotificationType == NotificationType.Share))
                    {
                        currentNotifcations.RemoveAll(x => x.DropId == dropId);
                    }
                    else
                    {
                        foreach (var notification in notifications)
                        {
                            notification.Viewed = true;
                            notification.ViewedAt = DateTime.UtcNow;
                        }
                    }
                    
                    user.CurrentNotifications = JsonSerializer.Serialize(currentNotifcations);
                    Context.SaveChanges();
                }
            }
        }

        public void RemoveAllNotifications(int userId)
        {
            var user = Context.UserProfiles.First(x => x.UserId == userId);
            var notifications = user.CurrentNotifications;
            if (!string.IsNullOrWhiteSpace(notifications))
            {
                user.CurrentNotifications = JsonSerializer.Serialize(new List<NotificationModel>());
                Context.SaveChanges();
            }
        }
    }
}
