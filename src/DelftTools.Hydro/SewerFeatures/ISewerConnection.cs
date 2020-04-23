using DelftTools.Hydro.Structures;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro.SewerFeatures
{
    public interface ISewerConnection : IBranch, IHydroNetworkFeature, ISewerFeature
    {
        double Length { get; set; }

        double LevelSource { get; set; }

        double LevelTarget { get; set; }

        SewerConnectionWaterType WaterType { get; set; }

        ICompartment SourceCompartment { get; set; }

        ICompartment TargetCompartment { get; set; }

        string SourceCompartmentName { get; set; }

        string TargetCompartmentName { get; set; }

        void UpdateBranchFeatureGeometries();
        void AddOrUpdateGeometry(IHydroNetwork hydroNetwork, SewerImporterHelper helper);
    }
}