using DelftTools.Hydro.Structures;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro.SewerFeatures
{
    public interface ISewerConnection : IBranch, IHydroNetworkFeature, ISewerFeature
    {
        /// <summary>
        /// Length of the connection
        /// </summary>
        double Length { get; set; }

        /// <summary>
        /// Level at the source (compartment)
        /// </summary>
        double LevelSource { get; set; }

        /// <summary>
        /// Level at the target (compartment)
        /// </summary>
        double LevelTarget { get; set; }

        /// <summary>
        /// Type of water in this connection (<seealso cref="SewerConnectionWaterType"/>
        /// </summary>
        SewerConnectionWaterType WaterType { get; set; }

        /// <summary>
        /// Sources compartment (compartment at the start of connection)
        /// </summary>
        ICompartment SourceCompartment { get; set; }

        /// <summary>
        /// Target compartment (compartment at the end of connection)
        /// </summary>
        ICompartment TargetCompartment { get; set; }

        /// <summary>
        /// Name of the <see cref="SourceCompartment"/>
        /// </summary>
        string SourceCompartmentName { get; set; }

        /// <summary>
        /// Name of the <see cref="TargetCompartment"/>
        /// </summary>
        string TargetCompartmentName { get; set; }
        
        /// <summary>
        /// Updates the geometries of the branch features
        /// </summary>
        void UpdateBranchFeatureGeometries();
        
        void AddOrUpdateGeometry(IHydroNetwork hydroNetwork, SewerImporterHelper helper);

        void SetLengthOfConnectionBasedOnConnectedCompartmentsOrSetAFake();
    }
}