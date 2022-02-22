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

namespace Domain.Repository
{
    public class PermissionService : BaseService
    {


        public bool CanView(int userId, int dropId) {
            return GetAllDrops(userId).Any(x => x.DropId == dropId);
        }

        public List<DropPromptOwner> CanViewPrompts(int userId, int promptId) {
            return GetAllDrops(userId).Where(x => x.PromptId == promptId)
                .Select(s => new DropPromptOwner { DropId = s.DropId, PromptId = promptId, OwnerId = s.UserId }).ToList();
        }

        private IQueryable<Drop> GetAllDrops(int userId)
        {
            DateTime now = DateTime.UtcNow.AddHours(1); // give them an extra hour
            var drops = Context.Drops.Where(x => (x.TagDrops.Any(t => t.UserTag.TagViewers.Any(a => a.UserId == userId))
                || x.UserId == userId));
            return drops;
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
    }
}
