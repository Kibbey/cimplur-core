using Domain.Models;
using System.Collections.Generic;
using System.Linq;

namespace Memento.Web.Models
{
    public class TagsViewModel
    {
        public TagsViewModel(List<GroupModel> tags) 
        {
            ActiveTags = tags.Where(x => !x.Archived).Select(s => new GroupModel { Name = s.Name, TagId = s.TagId, CanNotEdit = s.CanNotEdit }).ToList();
            ArchivedTags = tags.Where(x => x.Archived).Select(s => new GroupModel { Name = s.Name, TagId = s.TagId, CanNotEdit = s.CanNotEdit }).ToList();
        }
        public List<GroupModel> ArchivedTags { get; set; }
        public List<GroupModel> ActiveTags { get; set; }
    }
}