
namespace Domain.Models
{
    public class DropPromptOwner
    {
        public int DropId { get; set; }
        public int PromptId { get; set; }
        public int OwnerId { get; set; }
    }
}
