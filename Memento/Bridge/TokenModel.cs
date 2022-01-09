using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge
{
    public class TokenModel
    {
        public int UserId { get; set; }
        public string UserToken { get; set; }
        public DateTime Created { get; set; }
    }
}
