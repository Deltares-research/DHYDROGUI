using DelftTools.Utils.Collections.Generic;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    /// <summary>
    /// Data on a Waves Domain
    /// </summary>
    public interface IWaveDomainData
    {
        /// <summary>
        /// Gets the sub domains of the domain.
        /// </summary>
        IEventedList<IWaveDomainData> SubDomains { get; }

        /// <summary>
        /// Gets or sets the name of the grid file.
        /// </summary>
        string GridFileName { get; set; }

        /// <summary>
        /// Gets or sets the grid.
        /// </summary>
        CurvilinearGrid Grid { get; set; }

        /// <summary>
        /// Gets or sets the name of the bed level grid file.
        /// </summary>
        string BedLevelGridFileName { get; set; }

        /// <summary>
        /// Gets or sets the bathymetry.
        /// </summary>
        CurvilinearCoverage Bathymetry { get; set; }

        /// <summary>
        /// Gets or sets the name of the bed level file.
        /// </summary>
        string BedLevelFileName { get; set; }

        /// <summary>
        /// Gets the spectral domain data.
        /// </summary>
        SpectralDomainData SpectralDomainData { get; }

        /// <summary>
        /// Gets the hydro from flow data settings.
        /// </summary>
        HydroFromFlowSettings HydroFromFlowData { get; }

        /// <summary>
        /// Gets the meteo data.
        /// </summary>
        WaveMeteoData MeteoData { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this domain uses global meteo data.
        /// </summary>
        bool UseGlobalMeteoData { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IWaveDomainData"/> has output.
        /// </summary>
        bool Output { get; }

        /// <summary>
        /// Gets or sets the super domain.
        /// </summary>
        IWaveDomainData SuperDomain { get; set; }

        /// <summary>
        /// Gets the name of the domain.
        /// </summary>
        string Name { get; }
    }
}