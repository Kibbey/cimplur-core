

namespace Domain.Entities
{
    
    public partial class Log
    {
        public int ID { get; set; }
        public System.DateTime Date { get; set; }
        public string Thread { get; set; }
        public string Level { get; set; }
        public string Logger { get; set; }
        public string Message { get; set; }
    }
}
