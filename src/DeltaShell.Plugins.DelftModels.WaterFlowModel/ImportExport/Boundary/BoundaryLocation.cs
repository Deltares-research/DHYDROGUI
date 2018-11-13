using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Boundary
{
    /// <summary>
    /// ReadOnly BoundaryLocation, containing the name, boundary type and
    /// thatcher-harlemann coefficient for a single BoundaryLocation.
    /// </summary>
    public class BoundaryLocation
    {
        public BoundaryLocation(string name,
                                BoundaryType boundaryType,
                                double thatcherHarlemannCoefficient)
        {
            Name = name;
            BoundaryType = boundaryType;
            ThatcherHarlemannCoefficient = thatcherHarlemannCoefficient;
        }

        public readonly string Name;
        public readonly BoundaryType BoundaryType;
        public readonly double ThatcherHarlemannCoefficient;
    }
}
