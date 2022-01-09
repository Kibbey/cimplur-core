using System.Collections.Generic;


namespace Memento.Web.Models
{
    public class SelectedUserModel
    {
        public SelectedUserModel() {
            UserIds = new List<int>();
        }

        public List<int> UserIds { get; set; }
    }
}