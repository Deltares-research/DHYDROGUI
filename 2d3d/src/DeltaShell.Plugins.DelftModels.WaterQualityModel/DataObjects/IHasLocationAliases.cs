using DeltaShell.Plugins.DelftModels.WaterQualityModel.Model;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects
{
    /// <summary>
    /// Implement this interface if you want <see cref="WaqInitializationSettingsBuilder"/>
    /// to be able to write the USEDATA_ITEM ... FORITEM ... block with this type of feature.
    /// </summary>
    public interface IHasLocationAliases
    {
        /// <summary>
        /// The list of comma separated aliases that is used for this feature (location)
        /// </summary>
        string LocationAliases { get; }

        /// <summary>
        /// The name of the feature (location) to write.
        /// </summary>
        string Name { get; }
    }
}