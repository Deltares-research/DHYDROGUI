using System.ComponentModel;
using System.IO;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    [Entity]
    public class WaveDomainData : IWaveDomainData
    {
        public WaveDomainData(string name)
        {
            SpectralDomainData = new SpectralDomainData
            {
                UseDefaultDirectionalSpace = true,
                UseDefaultFrequencySpace = true
            };
            HydroFromFlowData = new HydroFromFlowSettings {UseDefaultHydroFromFlowSettings = true};
            MeteoData = new WaveMeteoData();
            UseGlobalMeteoData = true;

            SubDomains = new EventedList<IWaveDomainData>();

            Grid = CurvilinearGrid.CreateDefault();
            Grid.Name = "Grid (" + name + ")";

            Bathymetry = new CurvilinearCoverage() {Name = "Bed Level (" + name + ")"};
            Bathymetry.Components[0].NoDataValue = -999.0;
            Bathymetry.Components[0].DefaultValue = Bathymetry.Components[0].NoDataValue;
            Bathymetry.Resize(Grid.Size1, Grid.Size2, Grid.X.Values, Grid.Y.Values);

            GridFileName = name + ".grd";
            BedLevelFileName = name + ".dep";
            BedLevelGridFileName = null;

            Output = true;
        }

        public int NestedInDomain { get; set; }
        public IEventedList<IWaveDomainData> SubDomains { get; private set; }

        public string GridFileName { get; set; }
        public CurvilinearGrid Grid { get; set; }
        public string BedLevelGridFileName { get; set; }

        public CurvilinearCoverage Bathymetry { get; set; }

        public string BedLevelFileName { get; set; }

        public SpectralDomainData SpectralDomainData { get; set; }
        public HydroFromFlowSettings HydroFromFlowData { get; set; }
        public WaveMeteoData MeteoData { get; set; }
        public bool UseGlobalMeteoData { get; set; }
        public bool Output { get; set; }

        [Aggregation]
        public IWaveDomainData SuperDomain { get; set; }

        public string Name => Path.GetFileNameWithoutExtension(GridFileName);
    }

    [Entity]
    public class SpectralDomainData
    {
        public bool UseDefaultDirectionalSpace { get; set; }
        public WaveDirectionalSpaceType DirectionalSpaceType { get; set; }
        public int NDir { get; set; }
        public double StartDir { get; set; }
        public double EndDir { get; set; }
        public bool UseDefaultFrequencySpace { get; set; }
        public int NFreq { get; set; } // double according to manual ??
        public double FreqMin { get; set; }
        public double FreqMax { get; set; }
    }

    [Entity]
    public class HydroFromFlowSettings
    {
        public bool UseDefaultHydroFromFlowSettings { get; set; }
        public UsageFromFlowType BedLevelUsage { get; set; }
        public UsageFromFlowType WaterLevelUsage { get; set; }
        public UsageFromFlowType VelocityUsage { get; set; }
        public VelocityComputationType VelocityUsageType { get; set; }
        public UsageFromFlowType WindUsage { get; set; }
    }

    public enum WaveDirectionalSpaceType
    {
        [Description("Circle")]
        Circle,

        [Description("Sector")]
        Sector
    }

    public enum UsageFromFlowType
    {
        [Description("Do not use")]
        DoNotUse,

        [Description("Use, don't extend")]
        UseDoNotExtend,

        [Description("Use and extend")]
        UseAndExtend
    }

    public enum VelocityComputationType
    {
        [Description("depth-averaged")]
        DepthAveraged,

        [Description("surface-layer")]
        SurfaceLayer,

        [Description("wave-dependent")]
        WaveDependent
    }
}