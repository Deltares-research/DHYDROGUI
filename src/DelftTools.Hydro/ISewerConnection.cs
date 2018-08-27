using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro
{
    public interface ISewerConnection : IBranch, IHydroNetworkFeature, ISewerFeature
    {
        double Length { get; set; }

        double LevelSource { get; set; }

        double LevelTarget { get; set; }

        SewerConnectionWaterType WaterType { get; set; }

        Compartment SourceCompartment { get; set; }

        Compartment TargetCompartment { get; set; }

        string SourceCompartmentName { get; set; }

        string TargetCompartmentName { get; set; }
        void UpdateBranchFeatureGeometries();
    }
}