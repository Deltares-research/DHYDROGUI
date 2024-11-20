using System.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
{
    /// <summary>
    /// Compares <see cref="IDataColumn"/> by name
    /// </summary>
    public class DataColumnsNameComparer : IEqualityComparer<IDataColumn>
    {
        public bool Equals(IDataColumn x, IDataColumn y)
        {
            if (x == null) return false;
            if (y == null) return false;

            return Equals(x.Name, y.Name);
        }

        public int GetHashCode(IDataColumn obj)
        {
            return obj?.Name.GetHashCode() ?? -1;
        }
    }
}