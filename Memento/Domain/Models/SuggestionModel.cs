using System.Collections.Generic;

namespace Domain.Models
{
    public class SuggestionModel
    {
        public SuggestionModel() {
            SharedContacts = new List<string>();
            SuggestedTags = new List<GroupModel>();
        }

        public string Name { get; set; }
        public int Id { get; set; }
        public IEnumerable<string> SharedContacts { get; set; }
        public List<GroupModel> SuggestedTags { get; set; }
    }
}
