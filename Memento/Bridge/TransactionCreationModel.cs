using System;

namespace Bridge
{
    class TransactionCreationModel
    {
        public int UserId { get; set; }
        public DateTime Created { get; set; }
        public int AmountCents { get; set; }
        public string Description { get; set; }
        public string ChargeId { get; set; }
    }
}
