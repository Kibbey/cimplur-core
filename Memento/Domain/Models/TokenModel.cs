using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class TokenModel
    {
        public int UserId { get; set; }
        public string UserToken { get; set; }
        public DateTime Created { get; set; }
    }
}
