using System;

namespace Domain.Models
{
    public class ConnectionModel
    {
        public string Name { get; set; }
        public bool EmailNotifications { get; set; }
        public DateTime Age { get; set; }
        public Guid Token { get; set; }
        public int Id { get; set; }
    }
}
