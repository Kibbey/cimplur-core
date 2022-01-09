using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Utilities
{
    public sealed class PersonModelEqualityComparer : IEqualityComparer<PersonModelV2>
    {
        public bool Equals(PersonModelV2 x, PersonModelV2 y)
        {
            if (x == null)
                return y == null;
            else if (y == null)
                return false;
            else
                return x.Id == y.Id;
        }

        public int GetHashCode(PersonModelV2 obj)
        {
            return obj.Id;
        }
    }
}
