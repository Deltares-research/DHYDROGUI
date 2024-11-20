using System.IO;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Exporters
{
    /// <summary>
    /// Interface for an <see cref="EvaporationMeteoData"/> exporter.
    /// </summary>
    public interface IEvaporationExporter
    {
        /// <summary>
        /// Exports the evaporation meteo data to the specified file.
        /// </summary>
        /// <param name="evaporationMeteoData"> The evaporation meteo data. </param>
        /// <param name="file"> The file location to export the evaporation to. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="evaporationMeteoData"/> or <paramref name="file"/> is <c>null</c>.
        /// </exception>
        void Export(EvaporationMeteoData evaporationMeteoData, FileInfo file);

        /// <summary>
        /// Exports the evaporation meteo data to the specified directory.
        /// </summary>
        /// <param name="evaporationMeteoData"> The evaporation meteo data. </param>
        /// <param name="directory"> The directory location to export the evaporation to. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="evaporationMeteoData"/> or <paramref name="directory"/> is <c>null</c>.
        /// </exception>
        void Export(EvaporationMeteoData evaporationMeteoData, DirectoryInfo directory);
    }
}