using Domain.Models;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Utilities;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using static Domain.Emails.EmailTemplates;
using Domain.Emails;
using Newtonsoft.Json;

namespace Domain.Repository
{
    public class GroupService : BaseService
    {
        private SendEmailService sendEmailService;
        public GroupService(SendEmailService sendEmailService) {
            this.sendEmailService = sendEmailService;
        }
        /// <summary>
        /// Add tag to the system.  If it exists return the existing id.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public long Add(string tagName, int userId, bool setup = false)
        {
            if (string.IsNullOrWhiteSpace(tagName) || tagName.Length > 50 || (tagName.ToLower() == everyone.ToLower() && !setup)) throw new BadRequestException();

            string trimName = TextFormatter.CleanupWhiteSpace(tagName);

            var userTag = Context.UserNetworks.FirstOrDefault(x => x.Name == trimName && x.UserId == userId);
            if (userTag == null)
            {
                userTag = new UserTag
                {
                    Name = trimName,
                    Created = DateTime.UtcNow,
                    UserId = userId,
                    Archived = false,
                    IsTask = false
                };
                Context.UserNetworks.Add(userTag);
                Context.SaveChanges();
            }
            return userTag.UserTagId;
        }

        public async Task<GroupViewersModel> GetNetworkViewersModels(int ownerId, long networkId)
        {
            var userTagViewers = await Context.UserNetworks.Include(i => i.TagViewers)
                .SingleAsync(x => x.UserTagId == networkId);

            return new GroupViewersModel
            {
                OwnerId = ownerId,
                TagViewers = new TagViewersModel
                {
                    Tag = new GroupModel
                    {
                        TagId = userTagViewers.UserTagId,
                        Name = userTagViewers.Name,
                        Archived = userTagViewers.Archived,
                        Foreign = false,
                        IsTask = false,
                        CanNotEdit = userTagViewers.Name == everyone
                    },
                    Viewers = userTagViewers.TagViewers.Select(p => new GroupMember
                    {
                        Name = p.Viewer.UserName,
                        Id = p.Viewer.UserId,
                        Selected = true
                    }).ToList()
                }
            };
        }

        public async Task<HashSet<int>> GetUsersToShareWith(int ownerId, HashSet<long> networkIds)
        {
            var userIds = await Context.UserNetworks
                .Where(x => networkIds.Contains(x.UserTagId))
                .SelectMany(x => x.TagViewers.Select(s => s.Viewer.UserId)).ToListAsync()
                .ConfigureAwait(false);
            return userIds.ToHashSet();
        }

        public async Task<List<GroupViewersModel>> GetNetworksAndViewersModels(int ownerId)
        {
            var userTagViewers = Context.UserNetworks.Include(i => i.TagViewers)
                .Where(x => x.UserId == ownerId);

            var connections = await Context.UserUsers.Where(x => x.OwnerUserId == ownerId).Select(s => new GroupMember {
                Id = s.ReaderUser.UserId, Name = s.ReaderName ?? s.ReaderUser.Name })
                .ToListAsync().ConfigureAwait(false);
            var connectionDictionary = connections.ToDictionary((k => k.Id), v => v.Name);

            var tagViewers = await userTagViewers.Select(x => new GroupViewersModel
            {
                OwnerId = ownerId,
                TagViewers = new TagViewersModel
                {
                    Tag = new GroupModel
                    {
                        TagId = x.UserTagId,
                        Name = x.Name,
                        Archived = x.Archived,
                        Foreign = false,
                        IsTask = false,
                        CanNotEdit = x.Name == everyone
                    },
                    Viewers = x.TagViewers.Select(s => new GroupMember {
                        Name = s.Viewer.Name,
                        Id = s.Viewer.UserId,
                        Selected = true
                    }).OrderBy(o => o.Name)
                }
            }).ToListAsync().ConfigureAwait(false);

            tagViewers.ForEach(tagViewer =>
            {
                var viewers = tagViewer.TagViewers.Viewers.ToList();
                viewers.Where(viewer => connectionDictionary.ContainsKey(viewer.Id)).ToList()
                .ForEach(viewer => viewer.Name = connectionDictionary[viewer.Id]);
                // make a shallow copy so we can remove and then re-add members
                var tempViewers = connections.ToList();
                tempViewers.RemoveAll(x => viewers.Any(a => a.Id == x.Id));
                tempViewers.AddRange(viewers);
                tagViewer.TagViewers.Viewers = tempViewers.OrderBy(x => x.Name);
            });
                
            return tagViewers.OrderByDescending(x => x.TagViewers.Tag.CanNotEdit).ThenBy(x => x.TagViewers.Tag.Name).ToList();
        }

        public async Task<GroupViewersModel> UpdateNetworkViewers(int ownerId, long networkId, List<int> viewerIds)
        {
            List<UserProfile> viewers = await Context.UserProfiles.Where(x => viewerIds.Contains(x.UserId)
            && x.SharedWithUser.Any(s => s.OwnerUserId == ownerId)).ToListAsync();
            if (viewers.Count() != viewerIds.Count)
            {
                throw new NotAuthorizedException("Viewers not connected");
            }
            UserTag userTags = await Context.UserNetworks.Include(t => t.TagViewers.Select(s => s.Viewer))
                .SingleOrDefaultAsync(x => x.UserTagId == networkId);
            if (userTags == null || userTags.Name == everyone)
            {
                throw new NotAuthorizedException("Group not available");
            }
            userTags.TagViewers.RemoveAll(x => !viewerIds.Contains(x.Viewer.UserId));
            foreach (var tagViewer in viewerIds)
            {
                if (!userTags.TagViewers.Any(x => x.Viewer.UserId == tagViewer))
                {
                    UserProfile viewer = viewers.Single(x => x.UserId == tagViewer);
                    userTags.TagViewers.Add(new TagViewer { Viewer = viewer });
                }
            }
            await Context.SaveChangesAsync();
            return await GetNetworkViewersModels(ownerId, networkId);
        }

        public async Task<UserViewerNetworksModel> GetViewerNetworksModels(int ownerId, int userId)
        {
            var userTags = await Context.UserNetworks.Where(x => x.UserId == ownerId)
                .Select(t => new GroupModel
                {
                    Name = t.Name,
                    TagId = t.UserTagId,
                    Selected = t.TagViewers.Any(a => a.Viewer.UserId == userId) || t.Name == everyone,
                    CanNotEdit = t.Name == everyone
                }).OrderByDescending(x => x.CanNotEdit).ThenBy(x => x.Name).ToListAsync().ConfigureAwait(false);
            return new UserViewerNetworksModel
            {
                OwnerId = ownerId,
                ViewerId = userId,
                ViewerTags = userTags
            };
        }

        public async Task<UserViewerNetworksModel> UpdateViewerNetworks(int ownerId, int userId, List<long> networkIds)
        {
            networkIds = networkIds != null ? networkIds : new List<long>();
            UserProfile viewer = await Context.UserProfiles.SingleOrDefaultAsync(x => x.UserId == userId
            && x.SharedWithUser.Any(s => s.OwnerUserId == ownerId));
            if (viewer == null)
            {
                throw new NotAuthorizedException("Tag not available");
            }
            List<UserTag> userTags = await Context.UserNetworks.Include(t => t.TagViewers.Select(s => s.Viewer))
                .Where(x => x.UserId == ownerId).ToListAsync();
            
            var viewersToRemove = userTags.SelectMany(s => 
                s.TagViewers.Where(t => t.Viewer.UserId.Equals(userId) && !networkIds.Contains(t.UserTag.UserTagId)));
            Context.NetworkViewers.RemoveRange(viewersToRemove);
            foreach (var userTag in userTags.Where(x => networkIds.Contains(x.UserTagId)))
            {
                if (!userTag.TagViewers.Any(x => x.Viewer.UserId.Equals(userId)))
                {
                    Context.NetworkViewers.Add(new TagViewer { UserId = viewer.UserId, UserTagId = userTag.UserTagId });
                }
            }
            await Context.SaveChangesAsync();
            return await GetViewerNetworksModels(ownerId, userId);
        }

        public List<GroupModel> DropTags(int dropId, int userId, List<long> networkIds)
        {
            var start = DateTime.Now;
            networkIds = networkIds ?? new List<long>();
            var drop = Context.Drops.Where(x => x.UserId == userId && x.DropId == dropId)
                .Include(i => i.TagDrops).ThenInclude(i => i.UserTag)
                .First();
            networkIds = networkIds != null ? networkIds : new List<long>();
            var tags = drop.TagDrops.Where(x => !networkIds.Contains(x.UserTagId)).Select(s => new GroupModel
            {
                TagId = s.UserTagId,
                Name = s.UserTag.Name,
                Foreign = false
            }).OrderBy(x => x.Name).ToList();
            return tags;
        }

        public List<PersonModelV2> DropPeople(int dropId, int userId, List<string> people)
        {
            var start = DateTime.Now;
            people = people ?? new List<string>();
            var personModels = Context.Drops.Where(x => x.UserId == userId && x.DropId == dropId)
                .SelectMany(s => s.OtherUsersDrops).Where(x => !people.Contains(x.User.UserName.ToLower()))
                .Select(s => new PersonModelV2 { Name = s.User.UserName, Id = s.User.UserId }).Distinct().ToList();
            return personModels;
        }

        public async Task<List<string>> DropViewers(int userId, int dropId) {
            var users = Context.Drops.Where(x => x.UserId == userId && x.DropId == dropId)
                .SelectMany(x => x.TagDrops
                    .SelectMany(s => s.UserTag.TagViewers.Select(v => v.UserId)));
            var usersNames = await Context.UserUsers.Where(x => x.OwnerUserId == userId
            && users.Contains(x.ReaderUserId)).Select(s => s.ReaderName ?? s.ReaderUser.Name).ToListAsync();
            return usersNames.Distinct().OrderBy(x => x).ToList();
        }

        public void ClearTags(int currentUserId)
        {
            var user = Context.UserProfiles.Single(x => x.UserId.Equals(currentUserId));
            user.CurrentPeople = null;
            user.CurrentTagIds = null;
            user.Me = false;
            Context.SaveChanges();
        }

        /// <summary>
        /// Returns all the people the current user shares with.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<PersonModelV2>> People(int userId)
        {
            DateTime start = DateTime.Now;
            return await Context.UserProfiles.Where(x => x.UserId == userId).SelectMany(x => 
                x.ShareWithUser.Select(s => new PersonModelV2 { Name = s.ReaderName ?? s.ReaderUser.Name, Id = s.ReaderUser.UserId }))
                .ToListAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Get the current select tags for a user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<GroupModel> CurrentNetworks(int userId)
        {
            //need to re-write this to handle the person aspect
            var start = DateTime.Now;
            var networkIds = new List<long>();
            var currentTagIds = Context.UserProfiles.First(x => x.UserId == userId).CurrentTagIds;
            if (currentTagIds != null)
            {
                networkIds = JsonConvert.DeserializeObject<List<long>>(currentTagIds);
            }
            var tags = ActiveNetworks(userId).Where(x => networkIds.Contains(x.UserTagId)).Select(s => new GroupModel
            {
                TagId = s.UserTagId,
                Name = s.Name,
                IsTask = false,
                Foreign = false,
                CanNotEdit = s.Name == everyone
            }).OrderByDescending(x => x.CanNotEdit).ThenBy(x => x.Name).ToList();

            Context.SaveChanges();
            return tags;
        }

        private async Task CreateEveryone(int userId) {
            var everyoneGroup = await Context.UserNetworks.Where(x => x.Name == everyone && x.UserId == userId).SingleOrDefaultAsync();
            if (everyoneGroup == null) {
                everyoneGroup = new UserTag { UserId = userId, Name = everyone };
                Context.UserNetworks.Add(everyoneGroup);
                await Context.SaveChangesAsync();
            }
        }

        public async Task PopulateEveryone(int userId) {
            var relationshipUserIdsInEveryone = Context.NetworkViewers.Where(x => x.UserTag.Name == everyone && x.UserTag.UserId == userId).Select(s => s.UserId);
            var missingRelationships = await Context.UserUsers.Where(x => x.OwnerUserId == userId && 
                !relationshipUserIdsInEveryone.Contains(x.ReaderUserId)).ToListAsync().ConfigureAwait(false);
            if (missingRelationships.Any()) {
                var everyoneGroup = await Context.UserNetworks.SingleAsync(x => x.Name == everyone && x.UserId == userId).ConfigureAwait(false);
                foreach (var relationship in missingRelationships) {
                    everyoneGroup.TagViewers.Add(new TagViewer
                    {
                        UserId = relationship.ReaderUserId
                    });
                }
                await Context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<List<string>> GetViewers(int userId, List<long> networkIds) {
            return await Context.NetworkViewers.Where(x => networkIds.Contains(x.UserTag.UserTagId) && x.UserTag.UserId.Equals(userId))
                .SelectMany(s => s.Viewer.SharedWithUser.Where(w => w.OwnerUserId == userId).Select(i => i.ReaderName))
                .Distinct().OrderBy(x => x)
                .ToListAsync().ConfigureAwait(false);
        }

        public async Task<List<int>> GetNetwork(int userId, long networkId)
        {
            return await Context.NetworkViewers.Where(x => networkId == x.UserTag.UserTagId && x.UserTag.UserId.Equals(userId))
                .SelectMany(s => s.Viewer.SharedWithUser.Where(w => w.OwnerUserId == userId).Select(i => i.ReaderUserId))
                .Distinct().OrderBy(x => x)
                .ToListAsync().ConfigureAwait(false);
        }

        public void SaveCurrentPeople(int userId, List<PersonModelV2> people)
        {
            string currentPeople = JsonConvert.SerializeObject(people);
            var user = Context.UserProfiles.First(x => x.UserId == userId);
            user.CurrentPeople = currentPeople;
            user.CurrentTagIds = "[]";
            user.Skip = 0;
            Context.SaveChanges();
        }

        public void AddToCurrentPeople(int CurrentUserId, int targetUserId)
        {
            var userProfile = Context.UserProfiles.Single(x => x.UserId == CurrentUserId);
            var people = MapUserProfile.CurrentPeople(userProfile);
            var personToAdd = People(CurrentUserId).Result.FirstOrDefault(x => x.Id == targetUserId);
            if (personToAdd != null)
            {
                people.Add(personToAdd);
                SaveCurrentPeople(CurrentUserId, people.Distinct().ToList());
            }
        }

        public void RemoveFromCurrentPeople(int CurrentUserId, int personId)
        {
            var userProfile = Context.UserProfiles.Single(x => x.UserId == CurrentUserId);
            var people = MapUserProfile.CurrentPeople(userProfile);
            var personToRemove = people.FirstOrDefault(x => x.Id == personId);
            if (personToRemove != null)
            {
                people.Remove(personToRemove);
            }

            SaveCurrentPeople(CurrentUserId, people.Distinct().ToList());
        }

        public void SaveCurrentNetworks(int userId, List<long> userTagIds)
        {
            string currentTagIds = JsonConvert.SerializeObject(userTagIds);
            var user = Context.UserProfiles.First(x => x.UserId == userId);
            if (userTagIds != null && userTagIds.Any()) {
                // we don't allow tagIds and people filters on it
                user.Me = false;
                user.CurrentPeople = JsonConvert.SerializeObject(new List<string>());
            }
            user.CurrentTagIds = currentTagIds;
            user.Skip = 0;
            Context.SaveChanges();
        }

        public void UpdateCurrentNetwork(int CurrentUserId, long tagId)
        {
            SaveCurrentNetworks(CurrentUserId, new List<long> { tagId });
        }

        public void RemoveFromCurrentNetworks(int currentUserId, int tagId)
        {
            var groups = CurrentNetworks(currentUserId);
            if(groups.Any(a => a.TagId == tagId && a.Name != everyone))
            {
                var tagIds = groups.Select(s => s.TagId).ToList();
                tagIds.Remove(tagId);
                SaveCurrentNetworks(currentUserId, tagIds.Distinct().ToList());
            }
        }

        public void AddHelloWorldNetworks(int userId)
        {
            Add(GetFamily, userId, true);
            Add("Extended Family", userId, true);
            Add(everyone, userId, true);
        }

        private IQueryable<UserTag> ActiveNetworks(int userId)
        {
            return AllNetworks(userId).Where(x => !x.Archived);
        }

        private IQueryable<UserTag> AllNetworks(int userId)
        {
            return Context.UserNetworks.Where(x => x.UserId == userId).OrderBy(x => x.Name);
        }

        public List<GroupModel> AllNetworkModels(int userId)
        {
            return AllNetworks(userId)
                .Select(s => new GroupModel { TagId = s.UserTagId, Name = s.Name, Archived = s.Archived,
                    CanNotEdit = s.Name == everyone }).OrderByDescending(x => x.CanNotEdit).ThenBy(x => x.Name).ToList();
        }

        /// <summary>
        /// This is used to view all groups
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<GroupModel>> EditableGroups(int userId)
        {
            var groups = AllNetworkModels(userId);
            groups.RemoveAll(x => x.Name == everyone);
            return groups;
        }

        /// <summary>
        /// This is used to view all groups
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<GroupModel>> AllGroups(int userId)
        {
            var groups = AllNetworkModels(userId);
            if (!groups.Any(x => x.Name == everyone))
            {
                await CreateEveryone(userId);
                // This is a one off, just recures and don't worry about performance
                return await AllGroups(userId);
            }
            await PopulateEveryone(userId);
            return groups;
        }

        public async Task Archive(long networkId, int userId)
        {
            var group = await Context.UserNetworks.FirstOrDefaultAsync(x => x.UserId == userId && x.UserTagId == networkId).ConfigureAwait(false);
            if (group != null)
            {
                if (group.Name == everyone)
                {
                    throw new InvalidOperationException("You can not delete this group.");
                }
                var userNetworkDrops = Context.NetworkDrops.Where(x => x.UserTagId == networkId);
                Context.NetworkDrops.RemoveRange(userNetworkDrops);
                var networkViewers = Context.NetworkViewers.Where(x => x.UserTagId == networkId);
                Context.NetworkViewers.RemoveRange(networkViewers);
                Context.UserNetworks.Remove(group);
                await Context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task SendEmail(string email, string userName, int dropId, EmailTypes template)
        {
            await sendEmailService.SendAsync(email, template, new { User = userName, DropId = dropId.ToString() });
        }

        public async Task<string> Rename(int currentUserId, int networkId, string name)
        {
            if(name == everyone) throw new InvalidOperationException("You can not delete this group.");
            if (string.IsNullOrWhiteSpace(name) || name.Length > 50) throw new BadRequestException();

            var group = await Context.UserNetworks.SingleOrDefaultAsync(x => x.UserTagId == networkId && x.UserId == currentUserId)
                .ConfigureAwait(false);
            if (group == null) throw new NotFoundException("Group id not found - " + networkId + " by userId " + currentUserId);

            string trimName = TextFormatter.CleanupWhiteSpace(name);

            group.Name = trimName;
            await Context.SaveChangesAsync().ConfigureAwait(false);
            return trimName;
        }

        public static string GetEveryone { get { return everyone; } }

        private static string everyone = "All Connections";

        public static string GetFamily { get { return "Family"; } }
    }
}
