using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using NetTopologySuite.Extensions.Grids;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    /// <summary>
    /// Creates a stubbed IHydroData class for a 2x2 square grid model.
    /// </summary>
    public class TestHydroDataStub : IHydroData
    {
        private readonly UnstructuredGrid grid;
        private DateTime startTime;
        private DateTime stopTime;
        private TimeSpan timeStep;
        private DateTime referenceTime;
        private string areasPath;
        private string volumesPath;
        private string flowsPath;
        private string pointersPath;
        private string lengthsPath;
        private string salinityPath;
        private string temperaturesPath;
        private string verticalDiffusionPath;
        private string surfacesPath;
        private string shearStressesPath;
        private string gridFilePath;
        private string attributesPath;
        private string velocitiesRelativePath;
        private string widthsRelativePath;
        private string chezyCoefficientsRelativePath;
        private int numberOfHydrodynamicLayers;
        private int numberOfWaqSegmentLayers;
        private double[] hydrodynamicLayerThicknesses;
        private int[] numberOfHydrodynamicLayersPerWaqLayer;
        private EventedList<WaterQualityBoundary> boundaries;
        private Dictionary<WaterQualityBoundary, int[]> boundaryNodeIds;

        private IDictionary<string, Func<string>> delwaqDataToFilePathMapping;

        public event EventHandler<EventArgs<string>> DataChanged;

        public TestHydroDataStub()
        {
            FilePath = TestHelper.GetTestFilePath("notExistingHydFile.hyd");
            grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 10, 10);

            LayerType = LayerType.Sigma;

            startTime = new DateTime(2015, 3, 2);
            stopTime = new DateTime(2015, 4, 5);
            timeStep = new TimeSpan(0, 1, 0, 0);
            referenceTime = new DateTime(2015, 3, 2);

            areasPath = "";
            volumesPath = "";
            flowsPath = "";
            pointersPath = "";
            lengthsPath = "";
            salinityPath = "";
            temperaturesPath = "";
            verticalDiffusionPath = "";
            surfacesPath = "";
            shearStressesPath = "";
            velocitiesRelativePath = "";
            widthsRelativePath = "";
            chezyCoefficientsRelativePath = "";

            numberOfHydrodynamicLayers = 1;
            hydrodynamicLayerThicknesses = new[]
            {
                1.0
            };

            numberOfWaqSegmentLayers = 1;
            numberOfHydrodynamicLayersPerWaqLayer = new[]
            {
                1
            };

            boundaries = new EventedList<WaterQualityBoundary>();
            boundaryNodeIds = new Dictionary<WaterQualityBoundary, int[]>();

            InitializeDataMapping();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestHydroDataStub"/> class
        /// using values specified in a <see cref="HydFileData"/> object.
        /// </summary>
        /// <remarks>
        /// Note that the following items shall not be copied and remain the stubbed value provided by this class:
        /// <list type="bullet">
        ///     <item>
        ///         <see cref="GetGrid"/>
        ///     </item>
        ///     <item>
        ///         <see cref="GetAttributesRelativeFilePath"/>
        ///     </item>
        ///     <item>Grid related meta data (i.e. exchange counts)</item>
        /// </list>
        /// </remarks>
        public TestHydroDataStub(HydFileData hydFileData)
        {
            FilePath = hydFileData.Path == null
                           ? TestHelper.GetTestFilePath("dummy.hyd")
                           : hydFileData.Path.FullName;
            grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 10, 10);
            ModelType = hydFileData.HydroDynamicModelType;
            LayerType = hydFileData.LayerType;
            //Ztop = hydFileData.ZTop;
            //Zbot = hydFileData.ZBot;

            startTime = hydFileData.ConversionStartTime;
            stopTime = hydFileData.ConversionStopTime;
            timeStep = hydFileData.ConversionTimeStep;
            referenceTime = hydFileData.ConversionReferenceTime;

            gridFilePath = hydFileData.GridRelativePath;
            attributesPath = hydFileData.AttributesRelativePath;
            areasPath = hydFileData.AreasRelativePath;
            volumesPath = hydFileData.VolumesRelativePath;
            flowsPath = hydFileData.FlowsRelativePath;
            pointersPath = hydFileData.PointersRelativePath;
            lengthsPath = hydFileData.LengthsRelativePath;
            salinityPath = hydFileData.SalinityRelativePath;
            temperaturesPath = hydFileData.TemperatureRelativePath;
            verticalDiffusionPath = hydFileData.VerticalDiffusionRelativePath;
            surfacesPath = hydFileData.SurfacesRelativePath;
            shearStressesPath = hydFileData.ShearStressesRelativePath;
            velocitiesRelativePath = hydFileData.VelocitiesRelativePath;
            widthsRelativePath = hydFileData.WidthsRelativePath;
            chezyCoefficientsRelativePath = hydFileData.ChezyCoefficientsRelativePath;

            numberOfHydrodynamicLayers = hydFileData.NumberOfHydrodynamicLayers == 0 ? 1 : hydFileData.NumberOfHydrodynamicLayers;
            hydrodynamicLayerThicknesses = hydFileData.HydrodynamicLayerThicknesses.Length == 0
                                               ? new[]
                                               {
                                                   1.0d
                                               }
                                               : hydFileData.HydrodynamicLayerThicknesses;

            numberOfWaqSegmentLayers = hydFileData.NumberOfWaqSegmentLayers == 0 ? 1 : hydFileData.NumberOfWaqSegmentLayers;
            numberOfHydrodynamicLayersPerWaqLayer = hydFileData.NumberOfHydrodynamicLayersPerWaqSegmentLayer.Length == 0
                                                        ? new[]
                                                        {
                                                            1
                                                        }
                                                        : hydFileData.NumberOfHydrodynamicLayersPerWaqSegmentLayer;

            boundaries = new EventedList<WaterQualityBoundary>();
            boundaryNodeIds = new Dictionary<WaterQualityBoundary, int[]>();

            InitializeDataMapping();
        }

        public HydroDynamicModelType ModelType { get; set; }
        public double Ztop { get; set; }
        public double Zbot { get; set; }
        public bool ThirdCellIsInactive { get; set; }

        public Func<string, bool> HasDataForInjection { get; set; }
        public Func<string, string> GetFilePathForInjection { get; set; }

        public string FilePath { get; set; }

        public UnstructuredGrid Grid
        {
            get
            {
                return grid;
            }
        }

        public DateTime ConversionStartTime
        {
            get
            {
                return startTime;
            }
        }

        public DateTime ConversionStopTime
        {
            get
            {
                return stopTime;
            }
        }

        public TimeSpan ConversionTimeStep
        {
            get
            {
                return timeStep;
            }
        }

        public DateTime ConversionReferenceTime
        {
            get
            {
                return referenceTime;
            }
        }

        public string GridRelativePath
        {
            get
            {
                return gridFilePath;
            }
        }

        public string AreasRelativePath
        {
            get
            {
                return areasPath;
            }
        }

        public string VolumesRelativePath
        {
            get
            {
                return volumesPath;
            }
        }

        public string FlowsRelativePath
        {
            get
            {
                return flowsPath;
            }
        }

        public string PointersRelativePath
        {
            get
            {
                return pointersPath;
            }
        }

        public string LengthsRelativePath
        {
            get
            {
                return lengthsPath;
            }
        }

        public string SalinityRelativePath
        {
            get
            {
                return salinityPath;
            }
        }

        public string TemperatureRelativePath
        {
            get
            {
                return temperaturesPath;
            }
        }

        public string VerticalDiffusionRelativePath
        {
            get
            {
                return verticalDiffusionPath;
            }
        }

        public string SurfacesRelativePath
        {
            get
            {
                return surfacesPath;
            }
        }

        public string ShearStressesRelativePath
        {
            get
            {
                return shearStressesPath;
            }
        }

        public string AttributesRelativePath
        {
            get
            {
                string commonPath = Path.Combine("IO", "attribute files");

                if (!string.IsNullOrWhiteSpace(attributesPath))
                {
                    return attributesPath;
                }

                return ThirdCellIsInactive
                           ? Path.Combine(commonPath, "TestHydroDataImporterStub_3rdCellInactive.atr")
                           : Path.Combine(commonPath, "TestHydroDataImporterStub_AllActive.atr");
            }
        }

        public string VelocitiesRelativePath
        {
            get
            {
                return velocitiesRelativePath;
            }
        }

        public string WidthsRelativePath
        {
            get
            {
                return widthsRelativePath;
            }
        }

        public string ChezyCoefficientsRelativePath
        {
            get
            {
                return chezyCoefficientsRelativePath;
            }
        }

        public HydroDynamicModelType HydroDynamicModelType
        {
            get
            {
                return ModelType;
            }
        }

        public LayerType LayerType { get; set; }

        public double ZTop
        {
            get
            {
                return Ztop;
            }
        }

        public double ZBot
        {
            get
            {
                return Zbot;
            }
        }

        public int NumberOfHorizontalExchanges
        {
            get
            {
                return 8;
            }
        }

        public int NumberOfVerticalExchanges
        {
            get
            {
                return (numberOfWaqSegmentLayers - 1) * 4;
            }
        }

        public int NumberOfHydrodynamicLayers
        {
            get
            {
                return numberOfHydrodynamicLayers;
            }
        }

        public int NumberOfDelwaqSegmentsPerHydrodynamicLayer
        {
            get
            {
                return 4;
            }
        }

        public int NumberOfWaqSegmentLayers
        {
            get
            {
                return numberOfWaqSegmentLayers;
            }
        }

        public double[] HydrodynamicLayerThicknesses
        {
            get
            {
                return hydrodynamicLayerThicknesses;
            }
        }

        public int[] NumberOfHydrodynamicLayersPerWaqSegmentLayer
        {
            get
            {
                return numberOfHydrodynamicLayersPerWaqLayer;
            }
        }

        public long Id
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IEventedList<WaterQualityBoundary> GetBoundaries()
        {
            return boundaries;
        }

        public IDictionary<WaterQualityBoundary, int[]> GetBoundaryNodeIds()
        {
            return boundaryNodeIds;
        }

        public bool HasDataFor(string functionName)
        {
            if (HasDataForInjection != null)
            {
                return HasDataForInjection(functionName);
            }

            return !string.IsNullOrWhiteSpace(GetFilePathForFunctionName(functionName));
        }

        public string GetFilePathFor(string functionName)
        {
            if (GetFilePathForInjection != null)
            {
                return GetFilePathForInjection(functionName);
            }

            return "";
        }

        public bool HasSameSchematization(IHydroData data)
        {
            var dataStub = data as TestHydroDataStub;
            if (dataStub != null)
            {
                return NumberOfHydrodynamicLayers == dataStub.NumberOfHydrodynamicLayers &&
                       NumberOfWaqSegmentLayers == dataStub.NumberOfWaqSegmentLayers &&
                       CollectionsAreEqual(HydrodynamicLayerThicknesses, dataStub.HydrodynamicLayerThicknesses) &&
                       CollectionsAreEqual(NumberOfHydrodynamicLayersPerWaqSegmentLayer, dataStub.NumberOfHydrodynamicLayersPerWaqSegmentLayer);
            }

            return false;
        }

        public void Dispose() {}

        public Type GetEntityType()
        {
            throw new NotImplementedException();
        }

        private void InitializeDataMapping()
        {
            delwaqDataToFilePathMapping = new Dictionary<string, Func<string>>
            {
                {"salinity", () => SalinityRelativePath},
                {"temp", () => TemperatureRelativePath},
                {"tau", () => ShearStressesRelativePath}
            };
        }

        private string GetFilePathForFunctionName(string functionName)
        {
            string name = functionName.ToLower();
            if (delwaqDataToFilePathMapping.ContainsKey(name))
            {
                return delwaqDataToFilePathMapping[name]();
            }

            return null;
        }

        private static bool CollectionsAreEqual<T>(T[] a, T[] b) where T : IComparable
        {
            return !a.Where((t, i) => t.CompareTo(b[i]) != 0).Any();
        }
    }
}