using Domain.Models;
using Domain.Entities;
using Domain.Exceptions;
using Stripe;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using static Domain.Emails.EmailTemplates;

namespace Domain.Repository
{
    public class TimelineService : BaseService
    {
        public async Task<TimelineModel> AddTimeline(int currentUserId, string name, string description)
        {
            var now = DateTime.UtcNow;
            var timelineUser = new TimelineUser
            {
                UserId = currentUserId,
                Active = true,
                CreatedAt = now,
                UpdatedAt = now
            };
            var timeline = new Timeline
            {
                Name = name,
                Description = description,
                UserId = currentUserId,
                CreatedAt = now,
                UpdatedAt = now
            };
            timelineUser.Timeline = timeline;
            Context.TimelineUsers.Add(timelineUser);
            await Context.SaveChangesAsync();

            return MapFromTimeline(timeline, timeline.TimelineUsers.First(), currentUserId);
        }

        public async Task<TimelineModel> SoftDeleteTimeline(int currentUserId, int timelineId) {
            var timelineUsers = await Context.TimelineUsers.Include(i => i.Timeline)
                .Where(x => x.TimelineId == timelineId && x.UserId == currentUserId)
                .ToListAsync();

            var timelineUser = timelineUsers.FirstOrDefault();

            if (timelineUser == null) throw new NotFoundException();

            timelineUser.Active = false;
            timelineUser.UpdatedAt = DateTime.UtcNow;
            await Context.SaveChangesAsync();
            return MapFromTimeline(timelineUser.Timeline, timelineUser, currentUserId);
        }

        public async Task<TimelineModel> FollowTimeline(int currentUserId, int timelineId)
        {
            var timeline = await Context.Timelines.Include(i => i.TimelineUsers)
                .SingleOrDefaultAsync(x => x.TimelineId == timelineId);

            if (timeline == null) throw new NotFoundException();

            var hasAccess = await HasAccess(currentUserId, timelineId);

            if (!hasAccess) throw new NotAuthorizedException("You do not have access to this timeline");
            // only add name and description if they want to change it
            var timelineUser = timeline.TimelineUsers.SingleOrDefault(x => x.UserId == currentUserId);
            if (timelineUser == null) {
                timelineUser = new TimelineUser
                {
                    UserId = currentUserId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
            }
            timelineUser.Active = true;
            timeline.TimelineUsers.Add(timelineUser);
            await Context.SaveChangesAsync();
            return MapFromTimeline(timeline, timelineUser, currentUserId);
        }

        public async Task InviteToTimeline(int currentUserId, int timelineId, List<int> invitedPeopleIds)
        {
            var timeline = await Context.Timelines.Include(i => i.TimelineUsers)
                .SingleOrDefaultAsync(x => x.TimelineId == timelineId);

            if (timeline == null) throw new NotFoundException();

            var hasAccess = await HasAccess(currentUserId, timelineId);

            if (!hasAccess) throw new NotAuthorizedException("You do not have access to this timeline");
            // only add name and description if they want to change it
            var timelineUsers = timeline.TimelineUsers.Select(x => x.UserId);
            var needAdded = invitedPeopleIds.Where(x => !timelineUsers.Contains(x)).ToList();
            foreach (var personId in needAdded) {
                var timelineUser = new TimelineUser
                {
                    TimelineId = timelineId,
                    UserId = personId,
                    Active = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                Context.TimelineUsers.Add(timelineUser);
            }
            await Context.SaveChangesAsync();
            using (var notificationsService = new NotificationService()) {
                foreach (var person in needAdded)
                {
                    dynamic payload = new ExpandoObject();
                    payload.TimelineName = timeline.Name;
                    payload.TimelineId = timeline.TimelineId;
                    await notificationsService.AddNotificationGeneric(currentUserId, person, timelineId, NotificationType.Timeline,
                        EmailTypes.TimelineShare, payload);
                } 
            }
        }

        public async Task<TimelineModel> UpdateTimeline(int currentUserId, int timelineId, string name, string description)
        {
            var timelineUser = await Context.TimelineUsers.Include(i => i.Timeline)
                .FirstOrDefaultAsync(x => x.TimelineId == timelineId
                && x.UserId == currentUserId);

            if (timelineUser == null) throw new NotFoundException();
            if (timelineUser.Timeline.UserId == currentUserId)
            {
                timelineUser.Timeline.Name = name;
                timelineUser.Timeline.Description = description;
            }
            else
            {
                timelineUser.Name = name;
                timelineUser.Description = description;
            }
            timelineUser.UpdatedAt = DateTime.UtcNow;
            await Context.SaveChangesAsync();

            return MapFromTimeline(timelineUser.Timeline, timelineUser, currentUserId);
        }

        public async Task<TimelineModel> GetTimeline(int currentUserId, int timelineId)
        {
            var timeline = await Context.Timelines.Include(i => i.TimelineUsers)
                .SingleOrDefaultAsync(x => x.TimelineId == timelineId);

            if (timeline == null) throw new NotFoundException();
            if (!await HasAccess(currentUserId, timelineId)) throw new NotAuthorizedException("You do not have access to this timeline");

            return MapFromTimeline(timeline, 
                timeline.TimelineUsers.FirstOrDefault(x => x.UserId == currentUserId), currentUserId);
        }

        public async Task<List<TimelineModel>> GetAllTimelines(int currentUserId)
        {
            var connectedUserIds = new List<int>();
            using (var connectionService = new SharingService())
            {
                connectedUserIds = connectionService.GetConnections(currentUserId).Select(s => s.Id).ToList();
            }
            connectedUserIds.Add(currentUserId);
            // we show timelines that your connections have created or are following
            var timelines = await Context.Timelines
                .Where(x => connectedUserIds.Contains(x.UserId)
                || x.TimelineUsers.Any(a => connectedUserIds.Contains(a.UserId))).ToListAsync();
            return timelines.Select(s => MapFromTimeline(s, s.TimelineUsers.Where(x => x.UserId == currentUserId).FirstOrDefault(), currentUserId))
                .OrderBy(x => x.Name).ToList();
        }

        public async Task<List<TimelineModel>> GetTimelinesForDrop(int currentUserId, int dropId)
        {
            // we show timelines that your connections have created or are following
            var timelines = await Context.TimelineUsers.Include(i => i.Timeline.TimelineDrops)
            .Where(x => x.UserId == currentUserId && x.Active).Select(s => new { TimelineUser = s, 
                Selected = s.Timeline.TimelineDrops.Any(x => x.DropId == dropId) }).ToListAsync();
            return timelines.Select(s => MapFromTimeline(s.TimelineUser.Timeline, s.TimelineUser, currentUserId, s.Selected))
                .OrderBy(x => x.Name).ToList();
        }

        public async Task AddDropToTimeline(int currentUserId, int dropId, int timelineId)
        {
            using (var dropService = new DropsService()) {
                if(!dropService.CanView(currentUserId, dropId)) throw new NotAuthorizedException("You can not view this memory");
            }
            // check it is a valid timeline
            if (await HasAccess(currentUserId, timelineId)) {
                Context.TimelineDrops.Add(new TimelineDrop { 
                    TimelineId = timelineId,
                    DropId = dropId,
                    UserId = currentUserId,
                    CreatedAt = DateTime.UtcNow
                });
                await Context.SaveChangesAsync();
            }
        }

        public async Task AddPromptToTimeline(int currentUserId, List<int> promptIds, int timelineId)
        {
            var timeline = await Context.Timelines.Include(i => i.TimelinePrompts).FirstOrDefaultAsync(x => x.UserId == currentUserId && x.TimelineId == timelineId);
            if (timeline == null) throw new NotAuthorizedException("You can not add a question to a timeline you did not create.");
            var existing = timeline.TimelinePrompts.Select(s => s.PromptId).ToHashSet();
            var toRemove = timeline.TimelinePrompts.Where(x => !promptIds.Contains(x.PromptId)).ToList();
            promptIds.RemoveAll(x => existing.Contains(x));
            foreach(var promptId in promptIds) { 
                var timelinePrompt = new PromptTimeline {
                    PromptId = promptId,
                    TimelineId = timelineId,
                    CreatedAt = DateTime.UtcNow
                };
                Context.PromptTimelines.Add(timelinePrompt);
            }
            Context.PromptTimelines.RemoveRange(toRemove);
            await Context.SaveChangesAsync();
        }

        public async Task RemoveDropFromTimeline(int currentUserId, int dropId, int timelineId) 
        {
            // only the person that added a memory to a timeline can remove it - others can mute (TODO)
            var timelineDrop = await Context.TimelineDrops.FirstOrDefaultAsync(x => x.DropId == dropId
                && x.TimelineId == timelineId && x.UserId == currentUserId);
            if (timelineDrop == null) throw new NotAuthorizedException("Only the person that added a memory to a timeline can remove it.");

            Context.TimelineDrops.Remove(timelineDrop);
            await Context.SaveChangesAsync();
        }

        private TimelineModel MapFromTimeline(Timeline timeline, TimelineUser timelineUser, int currentUserId, bool selected = false)
        {
            var timelineCurrentUser = timelineUser ?? new TimelineUser { };
            return new TimelineModel
            {
                Name = timelineCurrentUser.Name ?? timeline.Name,
                Description = timelineCurrentUser.Description ?? timeline.Description,
                Active = timelineCurrentUser.Active,
                Following = timelineCurrentUser.UserId == currentUserId,
                Id = timeline.TimelineId,
                Creator = timeline.UserId == currentUserId,
                Selected = selected
            };
        }

        private async Task<bool> HasAccess(int currentUserId, int timelineId) {
            var timeLines = await GetAllTimelines(currentUserId);
            return timeLines.Any(a => a.Id == timelineId);
        }
    }
}
