using System;

namespace Bridge
{
    public class TransactionModel
    {
        public int TransactionId { get; set; }
        public DateTime Created { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }
}
