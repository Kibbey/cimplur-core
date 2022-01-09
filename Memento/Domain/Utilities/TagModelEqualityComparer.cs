using System;
using System.Collections.Generic;
using Domain.Models;


namespace Domain.Utilities
{
    public sealed class TagModelEqualityComparer : IEqualityComparer<GroupModel>
    {
        public bool Equals(GroupModel x, GroupModel y)
        {
            if (x == null)
                return y == null;
            else if (y == null)
                return false;
            else
                return x.TagId == y.TagId;// && x.IsTask == y.IsTask; don't care because the ones you add behave like your item - not the other persons
        }

        public int GetHashCode(GroupModel obj)
        {
            return obj.TagId.GetHashCode();
        }
    }

}
