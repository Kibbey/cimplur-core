using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Utilities
{
    public static class CacheHelper
    {
        public static string CacheName(string type, params string[] variables)
        {
            string name = type.ToString();
            foreach (var n in variables) {
                name += "_" + n;
            }
            return name;
        }
    }
}
