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

        /// <summary>
        /// Updates the name of the cross-section without updating the cross-section definition name. 
        /// </summary>
        /// <param name="name">The name to set for the cross-section.</param>
        void SetNameWithoutUpdatingDefinition(string name);
    }
}