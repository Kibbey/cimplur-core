using System.Collections.Generic;
using Domain.Entities;

namespace Domain.Utilities
{
    public sealed class DropEqualityComparer : IEqualityComparer<Drop>
    {
        public bool Equals(Drop x, Drop y)
        {
            if (x == null)
                return y == null;
            else if (y == null)
                return false;
            else
                return x.DropId == y.DropId;
        }

        public int GetHashCode(Drop obj)
        {
            return obj.DropId;
        }
    }
}
