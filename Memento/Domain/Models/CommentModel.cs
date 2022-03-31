using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public class CommentModel
    {
        public CommentModel() {
            Images = new List<int>();
            ImageLinks = new List<ImageModel>();
            Movies = new List<int>();
            MovieLinks = new List<MovieModel>();
        }

        public int CommentId { get; set; }
        public string OwnerName { get; set; }
        public string Comment { get; set; }
        public bool Foreign { get; set; }
        public KindOfComments Kind { get; set; }
        public string Created { get; set; }
        public int OwnerId { get; set; }
        public DateTime Date { get; set; }
        public IEnumerable<int> Images { get; set; }
        public List<ImageModel> ImageLinks { get; set; }
        public IEnumerable<int> Movies { get; set; }
        public List<MovieModel> MovieLinks { get; set; }
    }

    public enum KindOfComments { 
        Normal = 0,
        Thank = 1,
        UnThank = 2,
    }
}