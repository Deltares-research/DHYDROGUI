using System.Collections.Generic;
using DelftTools.Utils;

namespace DelftTools.Hydro
{
    public class NameableFeatureComparer<T> : EqualityComparer<T> where T : INameable
    {
        public override bool Equals(T x, T y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return x.Name == y.Name;
        }

        public override int GetHashCode(T obj)
        {
            return obj != null && obj.Name != null ? obj.Name.GetHashCode() : 0;
        }
    }
}