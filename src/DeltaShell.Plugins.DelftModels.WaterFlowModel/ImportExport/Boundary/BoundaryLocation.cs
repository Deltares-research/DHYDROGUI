using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Boundary
{
    /// <summary>
    /// ReadOnly BoundaryLocation, containing the name, boundary type and
    /// thatcher-harlemann coefficient for a single BoundaryLocation.
    /// </summary>
    public class BoundaryLocation
    {
        /// <summary>
        /// Construct a new BoundaryLocation with the specified arguments.
        /// </summary>
        /// <param name="name"> The nodeId with which this BoundaryLocation is associated. </param>
        /// <param name="boundaryType"> The BoundaryType of this BoundaryLocations. </param>
        /// <param name="thatcherHarlemannCoefficient"> Thatcher-Harlemann coefficient time in seconds. </param>
        public BoundaryLocation(string name,
                                BoundaryType boundaryType,
                                double thatcherHarlemannCoefficient)
        {
            Name = name;
            BoundaryType = boundaryType;
            ThatcherHarlemannCoefficient = thatcherHarlemannCoefficient;
        }

        /// <summary> nodeId: Node on which the boundary is located. </summary>
        public readonly string Name;
        /// <summary> type: Boundary type </summary>
        public readonly BoundaryType BoundaryType;
        /// <summary> thatcher-harlemann return time in seconds. </summary>
        public readonly double ThatcherHarlemannCoefficient;
    }
}
