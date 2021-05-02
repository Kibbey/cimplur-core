
namespace Domain.Entities
{
    
    public partial class UserActivityLog
    {
        public int UserActivityLogId { get; set; }
        public string TableName { get; set; }
        public string KeyColumn { get; set; }
        public string KeyValue { get; set; }
        public System.DateTime DateTime { get; set; }
        public string UserId { get; set; }
        public string Data { get; set; }
        public int TransactionType { get; set; }
    }
}
