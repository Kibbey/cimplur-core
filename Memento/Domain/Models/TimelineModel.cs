namespace Domain.Models
{
    public class TimelineModel : SelectModel
    {
        public string Description { get; set; }
        public bool Active { get; set; }
        public bool Following { get; set; }
        public bool Creator { get; set; }
    }
}
