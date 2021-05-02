using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Domain.Entities
{
    public class UserEmail
    {
        public int UserEmailId { get; set; }
        [MaxLength(100), Column(TypeName = "varchar")]
        public string Email { get; set; }

        public int UserId { get; set; }
        [MaxLength(70), Column(TypeName = "varchar")]
        public string Token { get; set; }

        public bool Confirmed { get; set; }

        public DateTime TokenExpiration { get; set; }

        [ForeignKey("UserId")]
        public virtual UserProfile User { get; set; }
    }
}
