
using System;

namespace Domain.Models
{
    public class PurchaseOptionModel
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
