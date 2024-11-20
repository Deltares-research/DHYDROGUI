using DelftTools.Hydro;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters
{
    /// <summary>
    /// Serializes the geometry of the basin catchments
    /// </summary>
    public interface IBasinGeometrySerializer
    {
        /// <summary>
        /// Writes the geometry of the basin catchments to the specified <paramref name="path"/>
        /// </summary>
        /// <param name="basin">Basin to use</param>
        /// <param name="path">Path to write the geometry to</param>
        /// <returns>If writing was successful</returns>
        bool WriteCatchmentGeometry(IDrainageBasin basin, string path);

        /// <summary>
        /// Reads the geometry of the basin catchments to the specified <paramref name="path"/>
        /// </summary>
        /// <param name="basin">Basin to add the geometries to</param>
        /// <param name="path">Path to read the geometry from</param>
        /// <returns>If reading was successful</returns>
        bool ReadCatchmentGeometry(IDrainageBasin basin, string path);
    }
}