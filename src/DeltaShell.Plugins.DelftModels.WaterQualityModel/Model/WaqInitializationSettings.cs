using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Model
{
    public class WaqInitializationSettings
    {
        public WaqInitializationSettings()
        {
            Settings = new WaterQualityModelSettings();
        }

        public IWaterQualityModelSettings Settings { get; set; }

        /// <summary>
        /// Input file used for preprocessing with delwaq.exe
        /// </summary>
        public TextDocument InputFile { get; set; }

        /// <summary>
        /// SubstanceProcessLibrary defining the substanceVariables, processes
        /// and parameters to use
        /// </summary>
        public SubstanceProcessLibrary SubstanceProcessLibrary { get; set; }

        /// <summary>
        /// The simulation start time
        /// </summary>
        public DateTime SimulationStartTime { get; set; }

        /// <summary>
        /// The simulation stop time
        /// </summary>
        public DateTime SimulationStopTime { get; set; }

        /// <summary>
        /// The simulation time step
        /// </summary>
        public TimeSpan SimulationTimeStep { get; set; }

        /// <summary>
        /// Collection of initial conditions
        /// </summary>
        public ICollection<IFunction> InitialConditions { get; set; }

        /// <summary>
        /// Collection of process coefficients
        /// </summary>
        public ICollection<IFunction> ProcessCoefficients { get; set; }

        /// <summary>
        /// Collection with one dispersion function
        /// </summary>
        public ICollection<IFunction> Dispersion { get; set; }

        /// <summary>
        /// Restart from *.map file (containing initial conditions)
        /// </summary>
        public bool UseRestart { get; set; }

        public int SegmentsPerLayer { get; set; }

        public int NumberOfLayers { get; set; }

        /// <summary>
        /// Gets or sets the grid file path.
        /// </summary>
        /// <value>
        /// The absolute grid file path.
        /// </value>
        public string GridFile { get; set; }

        public string AttributesFile { get; set; }

        public string VolumesFile { get; set; }

        public int HorizontalExchanges { get; set; }

        public int VerticalExchanges { get; set; }

        public string PointersFile { get; set; }

        public double HorizontalDispersion { get; set; }

        public double VerticalDispersion { get; set; }

        public bool UseAdditionalVerticalDiffusion { get; set; }

        public string AreasFile { get; set; }

        public string FlowsFile { get; set; }

        public string LengthsFile { get; set; }

        public string SurfacesFile { get; set; }

        public string VerticalDiffusionFile { get; set; }

        public string VelocitiesFile { get; set; }

        public string WidthsFile { get; set; }

        public string ChezyCoefficientsFile { get; set; }

        public IDictionary<WaterQualityBoundary, int[]> BoundaryNodeIds { get; set; }
        public IDictionary<WaterQualityLoad, int> LoadAndIds { get; set; }
        public IDictionary<string, IList<int>> OutputLocations { get; set; }
        public DataTableManager BoundaryDataManager { get; set; }
        public DataTableManager LoadsDataManager { get; set; }
        public string ModelWorkDirectory { get; set; }
        public IDictionary<string, IList<string>> BoundaryAliases { get; set; }
        public IDictionary<string, IList<string>> LoadsAliases { get; set; }
        public DateTime ReferenceTime { get; set; }
    }
}