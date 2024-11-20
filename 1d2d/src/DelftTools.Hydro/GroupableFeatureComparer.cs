using System.Collections.Generic;
using DelftTools.Utils;

namespace DelftTools.Hydro
{
    public class GroupableFeatureComparer<T> : EqualityComparer<T> where T : IGroupableFeature, INameable
    {
        public override bool Equals(T x, T y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;
            return x.Name == y.Name && x.GroupName == y.GroupName;
        }

        public override int GetHashCode(T obj)
        {
            if (obj == null) return 0;
            var nameHash = obj.Name != null? obj.Name.GetHashCode() : 0;
            var groupNameHash = obj.GroupName != null ? obj.GroupName.GetHashCode() : 0;

            return nameHash ^ groupNameHash;
        }
    }
}