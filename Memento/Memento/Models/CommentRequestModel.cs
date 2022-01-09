

namespace Memento.Web.Models
{
    public class CommentRequestModel
    {
        public string Comment { get; set; }
        public int DropId { get; set; }
        public int Kind { get; internal set; }
    }
}