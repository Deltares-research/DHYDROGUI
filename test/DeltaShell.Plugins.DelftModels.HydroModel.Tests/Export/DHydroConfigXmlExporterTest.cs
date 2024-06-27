using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.Core.Services;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Geometries;
using log4net.Core;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Export
{
    [TestFixture]
    public class DHydroConfigXmlExporterTest
    {
        [Test]
        public void Constructor_FileExportServiceIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => new DHydroConfigXmlExporter(null), Throws.ArgumentNullException);
        }

        [Test]
        public void FileExportService_SetToNull_ThrowsArgumentNullException()
        {
            DHydroConfigXmlExporter exporter = CreateExporter(string.Empty);

            Assert.That(() => exporter.FileExportService = null, Throws.ArgumentNullException);
        }

        [Test]
        public void Export_NoFileExporterFound_LogsErrorMessage()
        {
            string dirPath = Path.GetFullPath("fmdimr");
            if (Directory.Exists(dirPath))
            {
                Directory.Delete(dirPath, true);
            }

            DirectoryInfo dirInfo = Directory.CreateDirectory("fmdimr");
            DHydroConfigXmlExporter exporter = CreateExporter(Path.Combine(dirInfo.FullName, "dimr.xml"));
            exporter.FileExportService = new FileExportService();

            WaterFlowFMModel waterFlowFmModel = SetupWaterFlowFmModel();

            var expectedMessage = $"Export failed: No file exporter found for model '{waterFlowFmModel.Name}'.";
            TestHelper.AssertLogMessageIsGenerated(() => exporter.Export(waterFlowFmModel, null), expectedMessage, Level.Error);
        }

        [Test]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.DataAccess)]
        public void ExportCoupledFmModelToRtc()
        {
            var hydroModelBuilder = new HydroModelBuilder();
            HydroModel hydroModel = hydroModelBuilder.BuildModel(ModelGroup.FMWaveRtcModels);
            hydroModel.CurrentWorkflow = hydroModel.Workflows.First(w => w.Activities.Count == 2);
            WaterFlowFMModel fmModel = hydroModel.Activities.OfType<WaterFlowFMModel>().FirstOrDefault();
            Assert.NotNull(fmModel);
            var vertices = new[]
            {
                new Coordinate(0, 0),
                new Coordinate(0, 10),
                new Coordinate(10, 10),
                new Coordinate(10, 0)
            };

            var edges = new[,]
            {
                { 1, 2 },
                { 2, 3 },
                { 3, 4 },
                { 4, 1 },
                { 1, 3 }
            };

            var cells = new[,]
            {
                { 1, 2, 3 }, 
                { 1, 3, 4 }
            };

            fmModel.Grid = UnstructuredGridFactory.CreateFromVertexAndEdgeList(vertices, edges, cells);
            fmModel.ModelDefinition.GetModelProperty(GuiProperties.HisOutputDeltaT).Value = fmModel.TimeStep;
            fmModel.ModelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value = fmModel.TimeStep;
            
            var observationPointFm = new ObservationPoint2D{ Name = "ObservationFM" };
            var weirFm = new Weir2D
            {
                Name = "Weir1",
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(0, 100)
                })
            };

            fmModel.Area.ObservationPoints.Add(observationPointFm);
            fmModel.Area.Weirs.Add(weirFm);

            /* Structures definition */
            var input = new Input()
            {
                ParameterName = "rtcObsInput",
                Feature = observationPointFm
            };
            var output = new Output()
            {
                ParameterName = "rtcWeir1Output",
                Feature = weirFm
            };
            var rule = new PIDRule
            {
                Name = "noot",
                Inputs = { input },
                Outputs = { output }
            };
            /*  Control groups  */
            var controlGroup = new ControlGroup
            {
                Name = "test",
                Rules = { rule }
            };
            controlGroup.Inputs.Add(input);
            controlGroup.Outputs.Add(output);

            RealTimeControlModel rtcModel = hydroModel.Activities.OfType<RealTimeControlModel>().FirstOrDefault();
            Assert.NotNull(rtcModel);
            rtcModel.ControlGroups.Add(controlGroup);

            /* Coupert 2dToRtc */
            IDataItem flowObservationFmOutputDataItem = rtcModel.GetChildDataItemsFromControlledModelsForLocation(observationPointFm).First();
            IDataItem rtcInputdataItemFm = rtcModel.AllDataItems.First(di => di.ValueConverter != null && ReferenceEquals(di.ValueConverter.OriginalValue, input));

            // link
            rtcInputdataItemFm.LinkTo(flowObservationFmOutputDataItem);

            string dirPath = Path.GetFullPath("fmrtc");
            if (Directory.Exists(dirPath))
            {
                Directory.Delete(dirPath, true);
            }

            DirectoryInfo dirInfo = Directory.CreateDirectory("fmrtc");
            DHydroConfigXmlExporter exporter = CreateExporter(Path.Combine(dirInfo.FullName, "dimr.xml"));
            exporter.Export(hydroModel, null);

            Assert.That(Path.Combine(dirInfo.FullName, "dimr.xml"), Does.Exist);
            Assert.That(Path.Combine(dirInfo.FullName, "dflowfm"), Does.Exist);
            Assert.That(Path.Combine(dirInfo.FullName, "dflowfm/FlowFM.mdu"), Does.Exist);
            Assert.That(Path.Combine(dirInfo.FullName, "rtc"), Does.Exist);
            Assert.That(Path.Combine(dirInfo.FullName, "rtc/rtcToolsConfig.xml"), Does.Exist);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ExportStandAloneFm()
        {
            string dirPath = Path.GetFullPath("fmdimr");
            if (Directory.Exists(dirPath))
            {
                Directory.Delete(dirPath, true);
            }

            DirectoryInfo dirInfo = Directory.CreateDirectory("fmdimr");
            DHydroConfigXmlExporter exporter = CreateExporter(Path.Combine(dirInfo.FullName, "dimr.xml"));

            WaterFlowFMModel waterFlowFmModel = SetupWaterFlowFmModel();
            exporter.Export(waterFlowFmModel, null);

            Assert.That(Path.Combine(dirInfo.FullName, "dimr.xml"), Does.Exist);
            Assert.That(Path.Combine(dirInfo.FullName, "dflowfm"), Does.Exist);
            Assert.That(Path.Combine(dirInfo.FullName, "dflowfm/FlowFM.mdu"), Does.Exist);
        }

        private WaterFlowFMModel SetupWaterFlowFmModel()
        {
            var waterFlowFmModel = new WaterFlowFMModel();

            var vertices = new[]
            {
                new Coordinate(0, 0),
                new Coordinate(0, 10),
                new Coordinate(10, 10),
                new Coordinate(10, 0)
            };

            var edges = new[,]
            {
                { 1, 2 },
                { 2, 3 },
                { 3, 4 },
                { 4, 1 },
                { 1, 3 }
            };
            
            var cells = new[,]
            {
                { 1, 2, 3 }, 
                { 1, 3, 4 }
            };

            waterFlowFmModel.Grid = UnstructuredGridFactory.CreateFromVertexAndEdgeList(vertices, edges, cells);

            return waterFlowFmModel;
        }

        [Test]
        public void ExportIntegratedModelWithRtcOnly_ReturnsNoDimrModels()
        {
            var hydroModelBuilder = new HydroModelBuilder();
            HydroModel hydroModel = hydroModelBuilder.BuildModel(ModelGroup.Empty);
            var rtcModel = new RealTimeControlModel("rtcModel");
            hydroModel.Activities.Add(rtcModel);

            var dimrModels = (IEnumerable<IDimrModel>)TypeUtils.CallPrivateStaticMethod(typeof(DHydroConfigXmlExporter), "GetDimrModelsFromItem", hydroModel);
            Assert.IsEmpty(dimrModels);
        }

        private static DHydroConfigXmlExporter CreateExporter(string exportPath)
        {
            var fileExportService = new FileExportService();
            fileExportService.RegisterFileExporter(new WaterFlowFMFileExporter());
            fileExportService.RegisterFileExporter(new RealTimeControlModelExporter
            {
                XmlWriters =
                {
                    new RealTimeControlXmlWriter(),
                    new RealTimeControlRestartXmlWriter()
                }
            });

            return new DHydroConfigXmlExporter(fileExportService) { ExportFilePath = exportPath };
        }
    }
}