using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro.CrossSections
{
    public interface ICrossSection: IBranchFeature, IHydroNetworkFeature
    {
        ICrossSectionDefinition Definition { get; }

        CrossSectionType CrossSectionType { get; }
        bool GeometryBased { get; }

        double LowestPoint { get; }

        void MakeDefinitionLocal();

        void UseSharedDefinition(ICrossSectionDefinition definition);
        void ShareDefinitionAndChangeToProxy();
    }
}