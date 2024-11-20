using NetTopologySuite.Extensions.Geometries;

namespace DeltaShell.Plugins.FMSuite.Common.FeatureData
{
    /// <summary>
    /// Synchronizes a list of items with the collection of coordinates of the feature geometry
    /// of a boundary condition.
    /// </summary>
    public class BoundaryConditionsPointsSyncedList : GeometryPointsSyncedList<string>
    {
        public override string ToString()
        {
            return CreationMethod != null ? string.Join(", ", this) : base.ToString();
        }
    }
}