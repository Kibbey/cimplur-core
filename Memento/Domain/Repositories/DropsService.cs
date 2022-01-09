using Domain.Models;
using Domain.Emails;
using Domain.Entities;
using Domain.Exceptions;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Domain.Emails.EmailTemplates;
using Microsoft.EntityFrameworkCore.Query;

namespace Domain.Repository
{
    public class DropsService : BaseService
    {

        public async Task<Tuple<int, bool>> Add(DropModel model, 
            List<long> selectedNetworkIds, 
            int userId,
            List<int> timelineIds,
            int promptId = 0)
        {
            selectedNetworkIds = selectedNetworkIds == null ? new List<long>() : selectedNetworkIds;
            var user = await Context.UserProfiles.SingleAsync(x => x.UserId == userId);
            var now = DateTime.UtcNow;
            var drop = new Drop
            {
                Created = now,
                UserId = userId,
                Date = model.Date,
                DateType = model.DateType,
                Archived = false,
                DayOfYear = model.Date.DayOfYear,
                ContentDrop = new ContentDrop
                {
                    Stuff = model.Content.Stuff
                },
            };
            if (promptId > 0) {
                drop.PromptId = promptId;
            }
            if (timelineIds.Any()) {
                drop.TimelineId = timelineIds.First();
                Context.TimelineDrops.AddRange(timelineIds.Select(s => new TimelineDrop
                {
                    TimelineId = s,
                    Drop = drop,
                    CreatedAt = DateTime.UtcNow,
                    UserId = userId
                }));
            }
            user.Drops.Add(drop);
            
            if (selectedNetworkIds != null && selectedNetworkIds.Any())
            {

                foreach (var tagId in selectedNetworkIds.ToList())
                {
                    drop.TagDrops.Add(new TagDrop
                    {
                        UserTagId = tagId,
                    });
                }
            }

            await Context.SaveChangesAsync();
            if (selectedNetworkIds?.Any() ?? false)
            {
                Task.Run(async () => {
                    using (var notificationService = new NotificationService()) {
                        await notificationService.AddNotificationDropAdded(userId, selectedNetworkIds.ToHashSet(), drop.DropId).ConfigureAwait(false);
                    }
                });
            }
            EventService.EmitEvent(EventService.AddDrop, userId);
            return new Tuple<int, bool>(drop.DropId, false);
        }

        public bool CanView(int userId, int dropId) {
            return GetAllDrops(userId).Any(x => x.DropId == dropId);
        }

        public List<DropPromptOwner> CanViewPrompts(int userId, int promptId) {
            return GetAllDrops(userId).Where(x => x.PromptId == promptId)
                .Select(s => new DropPromptOwner { DropId = s.DropId, PromptId = promptId, OwnerId = s.UserId }).ToList();
        }

        public async Task<bool> Edit(DropModel model, List<long> networkIds, List<int> images, List<int> movies, int userId)
        {
            networkIds = networkIds ?? new List<long>();
            networkIds = networkIds.Distinct().ToList();
            var drop = await Context.Drops.Include(i => i.Images).Include(m => m.Movies)
                .FirstOrDefaultAsync(x => x.DropId == model.DropId && x.UserId == userId).ConfigureAwait(false);

            if (drop == null)
            {
                throw new Exception(string.Format("Drop not found {0} for {1}.", model.DropId.ToString(), userId.ToString()));
            }

            foreach (var tag in drop.TagDrops.ToList())
            {
                Context.NetworkDrops.Remove(tag);
            }

            foreach (var networkId in networkIds)
            {
                drop.TagDrops.Add(new TagDrop
                {
                    UserTagId = networkId
                });

            }

            drop.Date = model.Date;
            drop.DateType = model.DateType;
            drop.DayOfYear = model.Date.DayOfYear;
            drop.ContentDrop.Stuff = model.Content.Stuff;
            drop.Archived = model.Archived;
            if (drop.Images.Any()) {
                var imagesToRemove = drop.Images.Where(x => !images.Contains(x.ImageDropId) && !x.CommentId.HasValue);
                if (imagesToRemove.Any())
                {
                    Context.ImageDrops.RemoveRange(imagesToRemove);
                    var imageService = new ImageService();
                    foreach (var image in imagesToRemove) {
                        await imageService.Delete(drop.DropId, image.ImageDropId.ToString(), userId);
                    }
                }
            }
            if (drop.Movies.Any())
            {
                var moviesToRemove = drop.Movies.Where(x => !movies.Contains(x.MovieDropId) && !x.CommentId.HasValue);
                if (moviesToRemove.Any())
                {
                    Context.MovieDrops.RemoveRange(moviesToRemove);
                    var movieService = new MovieService();
                    List<Task> tasks = new List<Task>();
                    foreach (var movie in moviesToRemove) {
                         await movieService.Delete(drop.DropId, movie.MovieDropId.ToString(), userId);
                    }
                    
                }
            }
            await Context.SaveChangesAsync().ConfigureAwait(false);
            EventService.EmitEvent(EventService.EditDrop, userId);
            return true;
        }

        public async Task<DropModel> Drop(int currentUserId, int dropId)
        {
            if (!CanView(currentUserId, dropId))
            {
                var notificationService = new NotificationService();
                await notificationService.RemoveNotification(currentUserId, dropId);
                throw new NotFoundException("Ooops, this memory has been moved or removed.");
            }
            var drop = Context.Drops.Where(x => x.DropId == dropId);
            var tagIds = new GroupService().AllNetworkModels(currentUserId).Select(s => s.TagId).ToList();
            var dropModel = await MapDrops(drop, 1, currentUserId, 0, tagIds, true);
            return dropModel.Single();
        }

        public async Task<DropModel> GetDropFromNotification(int notificationId, int currentUserId)
        {
            var notification = Context.SharedDropNotifications
                .Where(x => x.TargetUserId == currentUserId && x.SharedDropNotificationId == notificationId).SingleOrDefault();
            if (!notification?.DropId.HasValue ?? true) throw new NotFoundException();
            int dropId = notification.DropId.Value;
            new NotificationService().ViewNotification(currentUserId, dropId);
            return await Drop(currentUserId, dropId);
        }

        public async Task<DropViewModel> GetTimelineDrops(int currentUserId, int timelineId, int skip, bool ascending) {
            int take = 50;
            if (skip <= 0)
            {
                skip = 0;
            }
            var model = new DropViewModel();
            var drops = GetAllDrops(currentUserId);
            //only pull in drops from the specific timeline
            drops = drops.Where(x => x.TimelineDrops.Any(a => a.TimelineId == timelineId));
            model.Drops = await MapDrops(drops, take, currentUserId, skip, new List<long>(), true, ascending);

            model.Skip = model.Drops.Any() ? skip + take : skip;
            model.Done = model.Drops.Count < take;

            return model;
        }

        public async Task<DropViewModel> GetAlbumDrops(int currentUserId, int albumId, int skip, bool ascending, int take = 50)
        {
            if (skip <= 0)
            {
                skip = 0;
            }
            var model = new DropViewModel();
            var drops = GetAllDrops(currentUserId);
            //only pull in drops from the specific timeline
            drops = drops.Where(x => x.AlbumDrops.Any(a => a.AlbumId == albumId));
            model.Drops = await MapDrops(drops, take, currentUserId, skip, new List<long>(), true, ascending);

            model.Skip = model.Drops.Any() ? skip + take : skip;
            model.Done = model.Drops.Count < take;

            return model;
        }

        public async Task<DropViewModel> GetDrops(int currentUserId, List<int> albumIds, List<int> people, bool me, bool chronological, int? dayOfYear, int year, int skip = 0, bool ascending = false)
        {
            people = people ?? new List<int>();
            //only when site is first pulled up is this an issue....maybe don't need this here.
            int take = 15;
            if (skip <= 0)
            {
                skip = 0;
            }
            var model = new DropViewModel();
            var drops = GetAllDrops(currentUserId);
            using (var groupsService = new GroupService())
            {
                if (albumIds.Any())
                {
                    drops = FilterNetwork(albumIds, drops, currentUserId);
                }
                else
                {
                    // filter only on people and me
                    // if we skip this we get everyones
                    drops = FilterPeople(people, currentUserId, drops, me);
                }

                if (dayOfYear.HasValue)
                {
                    // if we use this filter we have to be chronological
                    chronological = true;
                    int window = 7;
                    dayOfYear += 4;
                    var maxDay = dayOfYear.Value + window;
                    var minDay = dayOfYear.Value - window;
                    drops = drops.Where(x => x.DayOfYear > minDay && x.DayOfYear < maxDay);
                }
                var tagIds = groupsService.AllNetworkModels(currentUserId).Select(s => s.TagId).ToList();
                if (chronological) {
                    var greatestDate = new DateTime(year + 1, 1, 1);
                    drops = drops.Where(x => x.Date < greatestDate);
                }
                model.Drops = await MapDrops(drops, take, currentUserId, skip, tagIds, chronological, ascending);
            }

            // TODO - remove tags that are not shared with you...so you don't get jealous
            // if we have drops update skip.  If not leave it where it was 
            model.Skip = model.Drops.Any() ? skip + take : skip;
            model.Done = model.Drops.Count < take;
            
            return model;
        }

        private List<DropModel> MapUserNames(int userId, List<DropModel> dropModels) {
            var  userMap = Context.UserUsers.Where(x => x.OwnerUserId == userId).Select(s => new { s.ReaderUserId, ReaderName = s.ReaderName }) 
                .ToDictionary(k => k.ReaderUserId);
            foreach (var drop in dropModels) {
                if (userMap.ContainsKey(drop.CreatedById)) {
                    drop.CreatedBy = userMap[drop.CreatedById].ReaderName;
                }
                foreach (var comment in drop.Comments)
                {
                    if (userMap.ContainsKey(comment.OwnerId)) {
                        comment.OwnerName = userMap[comment.OwnerId].ReaderName;
                    }
                }
            }
            return dropModels;
        }

        public async Task<DropModel> GetOldestDropYear(int currentUserId)
        {
            int year = DateTime.UtcNow.Year;
            var dropsViewModel = await GetDrops(currentUserId, new List<int>(), new List<int>(), false, true, null, year, 0, true);
            return dropsViewModel.Drops.FirstOrDefault();
        }

        private async Task<List<DropModel>> MapDrops(IQueryable<Drop> drops, int take, int currentUserId, int skip, List<long> tagIds, bool chronological, bool ascending = false)
        {
            using (var userService = new UserService()) {
                var user = await userService.GetProfile(currentUserId).ConfigureAwait(false);
                if (!user.PremiumMember) {
                    throw new NotAuthorizedException("Buy a premium plan");
                }
            }
            //IIncludableQueryable<Drop, Timeline> dropInclude = drops
            IQueryable<Drop> dropInclude = drops
                .Include(i => i.Images)
                .Include(m => m.Movies)
                .Include(t => t.TagDrops.Select(s => s.UserTagId))
                .Include(x => x.Comments.Select(s => s.Owner))
                .Include(x => x.Prompt)
                .Include(x => x.Timeline);
            if (chronological)
            {
                dropInclude = ascending ? dropInclude.OrderBy(o => o.Date) : dropInclude.OrderByDescending(o => o.Date);
            }
            else
            {
                dropInclude = dropInclude.OrderByDescending(o => o.Created);
            }
            

            var returnDrops = dropInclude.Skip(skip).Take(take).Select(s => new DropModel
                {
                    Archived = s.Archived,
                    Completed = s.Completed,
                    CompletedBy = null,
                    CreatedBy = s.CreatedBy.Name,
                    CreatedById = s.CreatedBy.UserId,
                    OrderBy = chronological ? s.Date : s.Created,
                    Date = s.Date,
                    DateType = s.DateType,
                    DropId = s.DropId,
                    HasAlbums = s.AlbumDrops.Any(x => x.Album.UserId == currentUserId),
                    Images = s.Images.Where(x => !x.CommentId.HasValue).Select(t => t.ImageDropId),
                    Movies = s.Movies.Where(x => !x.CommentId.HasValue).Select(t => t.MovieDropId),
                    Editable = s.CreatedBy.UserId == currentUserId,
                    Content = new ContentModel
                    {
                        ContentId = s.DropId,
                        Stuff = s.ContentDrop.Stuff
                    },
                    Tags = s.TagDrops
                    .Where(x => tagIds.Contains(x.UserTagId))
                    .Select(t => new GroupModel {
                        TagId = t.UserTagId,
                        Name = t.UserTag.Name
                    }),
                    IsTask = false,
                    Prompt = s.PromptId.HasValue ? new PromptModel {
                        PromptId = s.PromptId.Value,
                        Question = s.Prompt.Question,
                    } : null,
                    Timeline = s.TimelineId.HasValue ? new TimelineModel { 
                        Name = s.Timeline.Name
                    } : null,
                    Comments = s.Comments.Select(t => new CommentModel {
                        Comment = t.Content,
                        CommentId = t.CommentId,
                        Kind = t.Kind,
                        OwnerId = t.Owner.UserId,
                        OwnerName = t.Owner.Name ?? t.Owner.UserName,
                        Images = s.Images.Where(x => x.CommentId == t.CommentId).Select(i => i.ImageDropId),
                        Movies = s.Movies.Where(x => x.CommentId == t.CommentId).Select(i => i.MovieDropId),
                        Foreign = !t.Owner.UserId.Equals(currentUserId),
                        Created = t.TimeStamp.ToString(),
                        Date = t.TimeStamp
                    }).OrderBy(c => c.CommentId)
                });
            
            EventService.EmitEvent(EventService.ViewDrop, currentUserId);
            var dropModels = MapUserNames(currentUserId, await returnDrops.ToListAsync().ConfigureAwait(false));
            dropModels.ForEach((item) => item.Content.SplitStuff = SplitOnLink(item.Content.Stuff));
            dropModels.ForEach(dropItem => {
                if (dropItem.Prompt != null) dropItem.Prompt.Question = dropItem.Prompt.Question?.Replace("{{name}}", dropItem.Timeline?.Name);
            });
            return dropModels;
        }

        private static Regex url = new Regex(@"http[s]?:\/\/[^\s]+[^\s-]", RegexOptions.Compiled);


        private static List<ContentSplitModel> SplitOnLink(string content) {
            var matches = url.Matches(content);
            var result = new List<ContentSplitModel>();
            int last = 0;

            for (var i = 0; i < matches.Count; i++)
            {
                var index = matches[i].Index;
                var length = matches[i].Length;
                if (index > last) {
                    result.Add(new ContentSplitModel
                    {
                        Content = content.Substring(last, index - last),
                        ContentType = ContentType.Normal
                    });
                }
                last = index + length;
                result.Add(new ContentSplitModel { Content = content.Substring(index, length),
                    ContentType = ContentType.Link
                });
            }
            
            if (last < content.Length) {
                result.Add(new ContentSplitModel
                {
                    Content = content.Substring(last, content.Length - last),
                    ContentType = ContentType.Normal
                });
            }
            return result;
        }

        private IQueryable<Drop> GetAllDrops(int userId)
        {
            DateTime now = DateTime.UtcNow.AddHours(1); // give them an extra hour
            var drops = Context.Drops.Where(x => (x.TagDrops.Any(t => t.UserTag.TagViewers.Any(a => a.UserId == userId))
                || x.UserId == userId));
            return drops;
        }

        private IQueryable<Drop> FilterTag(long networkId, IQueryable<Drop> drops)
        {
            //constrains drops to only tag selected
            return drops.Where(x => x.TagDrops.Any(a => a.UserTagId == networkId));
        }

        /// <summary>
        /// Pulls only drops in these albums with a safety check to make sure the user owns the albums
        /// </summary>
        /// <param name="albumIds"></param>
        /// <param name="drops"></param>
        /// <param name="currentUserId"></param>
        /// <returns></returns>
        private IQueryable<Drop> FilterNetwork(List<int> albumId, IQueryable<Drop> drops, int currentUserId)
        {
            return drops.Where(x => x.AlbumDrops.Any(s => albumId.Contains(s.AlbumId) && s.Album.UserId == currentUserId));
        }

        private IQueryable<Drop> FilterPeople(List<int> people, int currentUserId, IQueryable<Drop> drops, bool me)
        {
            // me being true means to include me
            if (people == null) people = new List<int>();
            //no people include everyone (and me)
            if (!people.Any() && !me) return drops;
            me = !people.Any() || me;
            // filter out drops from these people (previous filter already made sure we had access)
            return drops.Where(x => people.Contains(x.CreatedBy.UserId) || (me && x.UserId == currentUserId));
        }

        private IQueryable<Drop> FilterTagsQ(List<long> networkIds, IQueryable<Drop> drops)
        {
            if (networkIds != null && networkIds.Any())
            {
                //filter drops by tag filters            
                foreach (var networkId in networkIds)
                {
                    //constrains drops to only tags that hava ALL selected tags
                    drops = drops.Where(x => x.TagDrops.Any(a => a.UserTagId == networkId));
                }

            }
            return drops;
        }

        private IQueryable<Drop> FilterPeople(List<string> people, int currentUserId, IQueryable<Drop> drops)
        {

            //ones I shared
            foreach (var person in people)
            {
                drops = drops.Where(x => x.OtherUsersDrops.Any(a => a.User.UserName.ToLower() == person));
            }
            //ones shared with me
            IQueryable<Drop> sharedDrops = Context.UserDrops.Where(x => x.UserId == currentUserId && people.Contains(x.Drop.CreatedBy.UserName.ToLower())).Select(s => s.Drop);

            if (people.Count > 1)
            {
                foreach (var person in people)
                {
                    sharedDrops = sharedDrops.Where(x => x.OtherUsersDrops.Any(a => a.User.UserName.ToLower() == person) || person == x.CreatedBy.UserName.ToLower());
                }

            }
            //return drops.Union(sharedDrops, new DropEqualityComparer());
            return drops.Union(sharedDrops);
        }

        public async Task Delete(int dropId, int userId)
        {
            var drop = await Context.Drops.FirstOrDefaultAsync(x => x.DropId == dropId && x.UserId == userId)
                .ConfigureAwait(false);
            if (drop == null)
            {
                throw new Exception(string.Format("Drop to delete not found {0} for {1}.", dropId.ToString(), userId.ToString()));
            }
            var images = drop.Images.ToList();
            var movies = drop.Movies.ToList();
            foreach (var tag in drop.TagDrops.ToList())
            {
                Context.NetworkDrops.Remove(tag);
            }

            foreach (var person in drop.OtherUsersDrops.ToList())
            {
                Context.UserDrops.Remove(person);
            }

            Context.SharedDropNotifications.RemoveRange(drop.Notifications);

            Context.ContentDrops.Remove(drop.ContentDrop);

            Context.Drops.Remove(drop);

            using (ImageService imageService = new ImageService())
            {
                foreach (var image in images)
                {
                    RemoveImageId(image.ImageDropId.ToString());
                    await imageService.Delete(image.DropId, image.ImageDropId.ToString(), userId);
                }

            }

            using (MovieService movieService = new MovieService())
            {
                foreach (var movie in movies)
                {
                    RemoveMovieId(movie.MovieDropId.ToString());
                    await movieService.Delete(movie.DropId, movie.MovieDropId.ToString(), userId);
                }

            }

            await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        public string DropImageId(int dropId, int userId, int? commentId)
        {
            if (!this.CanView(userId, dropId))
            {
                throw new NotAuthorizedException("You do not have acces to this memory.");
            }
            var drop = Context.Drops.FirstOrDefault(x => x.DropId == dropId);
            if (drop == null)
            {
                return null;
            }
            //grab imageId = imageId;
            //insert next
            var image = new ImageDrop { CommentId = commentId };
            drop.Images.Add(image);
            Context.SaveChanges();
            return image.ImageDropId.ToString();
        }

        public string DropMovieId(int dropId, int userId, int? commentId)
        {
            if (!this.CanView(userId, dropId))
            {
                throw new NotAuthorizedException("You do not have acces to this memory.");
            }
            var drop = Context.Drops.FirstOrDefault(x => x.DropId == dropId);
            if (drop == null)
            {
                return null;
            }
            //grab imageId = imageId;
            //insert next
            var movie = new MovieDrop { CommentId = commentId };
            drop.Movies.Add(movie);
            Context.SaveChanges();
            return movie.MovieDropId.ToString();
        }

        public void RemoveImageId(string imageId)
        {
            // We do NOT do a security check here - this needs done higher up the stack!
            int id = int.Parse(imageId);
            var image = Context.ImageDrops
                .FirstOrDefault(x => x.ImageDropId == id);
            if (image != null)
            {
                Context.ImageDrops.Remove(image);
                Context.SaveChanges();
            }
        }

        public void RemoveMovieId(string imageId)
        {
            int id = int.Parse(imageId);
            var movie = Context.MovieDrops.FirstOrDefault(x => x.MovieDropId == id);
            if (movie != null)
            {
                Context.MovieDrops.Remove(movie);
                Context.SaveChanges();
            }
        }

        public async Task<CommentModel> Thank(int userId, int dropId)
        {
            if (CanView(userId, dropId))
            {
                var drop = await Context.Drops.Include(i => i.CreatedBy).Include(i => i.Comments).FirstOrDefaultAsync(x => x.DropId.Equals(dropId));
                var commentModel = drop.Comments.SingleOrDefault(x => x.UserId == userId && x.Kind != 0);
                if (commentModel == null) {
                    commentModel = new Comment
                    {
                        UserId = userId,
                        TimeStamp = DateTime.Now,
                        Kind = KindOfComments.Thank,
                    };
                    drop.Comments.Add(commentModel);
                    Dictionary<int, string> lookupCommentor = await new UserService().GetInverseNameDictionary(userId, new List<int> { drop.UserId });
                    if (lookupCommentor.ContainsKey(drop.UserId))
                    {
                        Task.Run(() =>
                            SendEmailService.SendAsync(drop.CreatedBy.Email, EmailTypes.ThankEmail, new { User = lookupCommentor[drop.UserId], DropId = dropId.ToString() })
                        );
                    }
                } else {
                    // if it exists - set it opposite whatever it is.
                    // unthank is just remove the "thank"
                    commentModel.Kind = commentModel.Kind == KindOfComments.Thank 
                        ? KindOfComments.UnThank 
                        : KindOfComments.Thank;
                }
                await Context.SaveChangesAsync();
                return new CommentModel
                {
                    Comment = commentModel.Content,
                    CommentId = commentModel.CommentId,
                    OwnerName = "You",
                    Foreign = false,
                    Kind = commentModel.Kind,
                    Created = commentModel.TimeStamp.ToString()
                };
            }
            throw new NotAuthorizedException("You can not comment on this memory");
        }

        public async Task<CommentModel> AddComment(string comment, int userId, int dropId)
        {
            var drop = await Context.Drops.Include(i => i.Comments).FirstOrDefaultAsync(x => x.DropId.Equals(dropId));
            if (drop != null)
            {
                if (CanView(userId, dropId))
                {
                    var commentModel = new Comment
                    {
                        UserId = userId,
                        Content = comment,
                        TimeStamp = DateTime.Now,
                        Kind = KindOfComments.Normal,
                    };
                    drop.Comments.Add(commentModel);
                    await Context.SaveChangesAsync();
                    var commenters = drop.Comments.Where(x => x.UserId != userId).Select(s => s.Owner).ToList();
                    var commenterIds = commenters.Select(x => x.UserId).ToList();
                    commenterIds.Add(drop.UserId);
                    commenterIds = commenterIds.Distinct().ToList();
                    Dictionary<int, string> lookupCommentor = await new UserService().GetInverseNameDictionary(userId, commenterIds);
                    var personWhoCommented = await Context.UserProfiles.SingleAsync(x => x.UserId.Equals(userId));
                    var commentorName = personWhoCommented.Name;
                    if (drop.UserId != userId)
                    {
                        commenters.Add(drop.CreatedBy);
                    }
                    commenters = commenters.Distinct().ToList();
                    var notificationService = new NotificationService();
                    foreach (var commenter in commenters)
                    {
                        var sendEmail = await notificationService.AddNotificationGeneric(userId, commenter.UserId, dropId, NotificationType.Comment);
                        string name = lookupCommentor.ContainsKey(commenter.UserId) ? lookupCommentor[commenter.UserId] : commentorName;
                        if (sendEmail) {
                            Task.Run(() =>
                                SendEmailService.SendAsync(commenter.Email, EmailTypes.CommentEmail, new { User = name, DropId = dropId.ToString() })
                            );
                        }
                    }

                    return new CommentModel
                    {
                        Comment = commentModel.Content,
                        CommentId = commentModel.CommentId,
                        Kind = commentModel.Kind,
                        Created = commentModel.TimeStamp.ToString(),
                        Foreign = false,
                        OwnerName = commentorName
                    };
                }


            }
            throw new KeyNotFoundException();
        }

        /// <summary>
        /// Allows a user to update a comment they made.
        /// </summary>
        /// <param name="comment"></param>
        /// <param name="commentId"></param>
        /// <param name="currentUserId"></param>
        /// <returns></returns>
        public CommentModel UpdateComment(string comment, int commentId, int currentUserId)
        {
            /* This is an awkward corner - we don't always have a comment when we have a picture.
             * Just let them enter a blank comment for now...we can try to sort it out.
            if (string.IsNullOrWhiteSpace(comment))
            {
                throw new BadRequestException();
            }*/
            var commentEntity = Context.Comments.Include(d => d.Drop).FirstOrDefault(x => x.CommentId.Equals(commentId));
            if (commentEntity != null)
            {
                bool canEdit = commentEntity.UserId == currentUserId;
                if (canEdit)
                {
                    commentEntity.Content = comment.Trim();
                    commentEntity.TimeStamp = DateTime.UtcNow;
                    Context.SaveChanges();
                    return new CommentModel {
                        Comment = commentEntity.Content,
                        CommentId = commentEntity.CommentId,
                        Foreign = false,
                        Kind = commentEntity.Kind,
                        Created = commentEntity.TimeStamp.ToString()
                    };
                }
                else {
                    throw new NotAuthorizedException("You can not update a comment you do not own.");
                }
            }
            throw new KeyNotFoundException();
        }

        public async Task<List<CommentModel>> GetComments(int dropId, int currentUserId) {
            if (CanView(dropId, currentUserId)) {
                return await GetCommentsNoCheck(dropId, currentUserId);
            }
            else
            {
                throw new NotAuthorizedException("You can only view comments on drops shared with you.");
            }
        }

        private async Task<List<CommentModel>> GetCommentsNoCheck(int dropId, int currentUserId)
        {
            return await Context.Comments.Where(s => s.DropId == dropId).OrderBy(o => o.CommentId)
                .Select(t => new CommentModel
                {
                    Comment = t.Content,
                    CommentId = t.CommentId,
                    OwnerName = t.Owner.UserName.ToUpper(),
                    Foreign = !t.Owner.UserId.Equals(currentUserId),
                    Kind = t.Kind,
                    Created = t.TimeStamp.ToString()
                })
                .OrderBy(x => x.Created).ToListAsync();
        }

        public async Task RemoveComment(int commentId, int userId)
        {
            var comment = Context.Comments
                .Include(d => d.Drop)
                .Include(d => d.Images)
                .Include(d => d.Movies)
                .FirstOrDefault(x => x.CommentId.Equals(commentId));
            if (comment != null)
            {
                bool canRemove = comment.UserId == userId;
                if (canRemove)
                {
                    var tasks = new List<Task>();
                    using (var imageService = new ImageService()) {
                        foreach (var image in comment.Images) {
                            tasks.Add(imageService.Delete(comment.DropId, image.ImageDropId.ToString(), userId));
                        }
                    }
                    using (var movieService = new MovieService())
                    {
                        foreach (var image in comment.Images)
                        {
                            tasks.Add(movieService.Delete(comment.DropId, image.ImageDropId.ToString(), userId));
                        }
                    }
                    await Task.WhenAll(tasks);
                    Context.ImageDrops.RemoveRange(comment.Images);
                    Context.MovieDrops.RemoveRange(comment.Movies);
                    Context.Comments.Remove(comment);
                    await Context.SaveChangesAsync();
                }
                else {
                    throw new NotAuthorizedException("You can only delete your comments.");
                }

            }
        }

        public async Task AddHelloWorldDrop(int userId)
        {
            var user = Context.UserProfiles.Single(x => x.UserId.Equals(userId));
            await Add(new DropModel
            {
                Date = DateTime.Now,
                Content = new ContentModel(0, @"Welcome to Fyli!" + twoNewLine +
                    "This is a sample memory. To edit or delete it click the blue pencil." + twoNewLine +
                    "Start building your memories by answering questions or asking others questions you always wanted answered." + twoNewLine
                )
            }, null, user.UserId, new List<int>()); ;
        }

        private static string twoNewLine = Environment.NewLine + Environment.NewLine;

        private static void UpdateUserSkip(StreamContext context, int skip, int userId)
        {
            var user = context.UserProfiles.Single(x => x.UserId == userId);
            user.Skip = skip;
            context.SaveChanges();
        }

        private List<string> _readerCurrentUserPeople { get; set; }

        private List<string> readerCurrentUserPeople(int currentUserId)
        {
            if (_readerCurrentUserPeople == null)
            {
                _readerCurrentUserPeople = Context.UserUsers.Where(x => x.ReaderUserId == currentUserId).Select(s => s.OwnerUser.UserName.ToLower()).ToList();
            }
            return _readerCurrentUserPeople;
        }

        private List<string> writerCurrentUserPeople(int currentUserId)
        {
            throw new NotImplementedException("add later");
            //return Context.UserUserTags.Where(x => x.OwnerUserId == currentUserId).Select(s => s.OwnerUser.UserName.ToLower()).ToList();
        }
    }
}
