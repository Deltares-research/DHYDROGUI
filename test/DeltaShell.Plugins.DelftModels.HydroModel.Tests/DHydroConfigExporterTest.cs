using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave;
using DeltaShell.Plugins.FMSuite.Wave.IO.Exporters;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class DHydroConfigExporterTest
    {
        private static IEnumerable<IFileExporter> GetFileExporters()
        {
            yield return new WaterFlowFMFileExporter();
            yield return new WaterFlowModel1DExporter();
            yield return new RealTimeControlModelExporter();
            yield return new WaveModelFileExporter();
        }

        [Test]
        public void ExportCoupledFmModelToRtc()
        {
            var hydroModelBuilder = new HydroModelBuilder();
            var hydroModel = hydroModelBuilder.BuildModel(ModelGroup.FMWaveRtcModels);
            hydroModel.CurrentWorkflow =
                hydroModel.Workflows.First(w => w.Activities.Count == 2 && !w.Activities.OfType<WaveModel>().Any());
            var fmModel = hydroModel.Activities.OfType<WaterFlowFMModel>().FirstOrDefault();
            Assert.NotNull(fmModel);
            var vertices = new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(0, 10),
                    new Coordinate(10, 10),
                    new Coordinate(10, 0)
                };

            var edges = new int[,]
                {
                    {1, 2}, {2, 3}, {3, 4}, {4, 1}, {1, 3}
                };

            fmModel.Grid = UnstructuredGridFactory.CreateFromVertexAndEdgeList(vertices, edges);
            fmModel.ModelDefinition.GetModelProperty(GuiProperties.HisOutputDeltaT).Value = fmModel.TimeStep;
            fmModel.ModelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value = fmModel.TimeStep;
            var observationPointFm = new Feature2DPoint { Name = "ObservationFM" };
            var weirFm = new Weir { Name = "Weir1", Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(0, 100) }) };

            fmModel.Area.ObservationPoints.Add(observationPointFm);
            fmModel.Area.Weirs.Add(weirFm);

            /* Structures definition */
            var input = new Input() { ParameterName = "rtcObsInput", Feature = observationPointFm };
            var output = new Output() { ParameterName = "rtcWeir1Output", Feature = weirFm };
            var rule = new PIDRule { Name = "noot", Inputs = { input }, Outputs = { output } };
            /*  Control groups  */
            var controlGroup = new ControlGroup { Name = "test", Rules = { rule }, };
            controlGroup.Inputs.Add(input);
            controlGroup.Outputs.Add(output);

            var rtcModel = hydroModel.Activities.OfType<RealTimeControlModel>().FirstOrDefault();
            Assert.NotNull(rtcModel);
            rtcModel.ControlGroups.Add(controlGroup);

            /* Coupert 2dToRtc */
            var flowObservationFmOutputDataItem = rtcModel.GetChildDataItemsFromControlledModelsForLocation(observationPointFm).First();
            var rtcInputdataItemFm = rtcModel.AllDataItems.First(di => di.ValueConverter != null && ReferenceEquals(di.ValueConverter.OriginalValue, input));

            // link
            rtcInputdataItemFm.LinkTo(flowObservationFmOutputDataItem);

            var dirPath = Path.GetFullPath("fmrtc");
            if (Directory.Exists(dirPath))
            {
                Directory.Delete(dirPath, true);
            }
            var dirInfo = Directory.CreateDirectory("fmrtc");
            var exporter = new DHydroConfigXmlExporter
            {
                ExportFilePath = Path.Combine(dirInfo.FullName, "dimr.xml")
            };
            exporter.Export(hydroModel, null);

            Assert.IsTrue(File.Exists(Path.Combine(dirInfo.FullName, "dimr.xml")));
            Assert.IsTrue(Directory.Exists(Path.Combine(dirInfo.FullName, "dflowfm")));
            Assert.IsTrue(File.Exists(Path.Combine(dirInfo.FullName, "dflowfm/FlowFM.mdu")));
            Assert.IsTrue(Directory.Exists(Path.Combine(dirInfo.FullName, "rtc")));
            Assert.IsTrue(File.Exists(Path.Combine(dirInfo.FullName, "rtc/rtcToolsConfig.xml")));
        }

        [Test]
        public void ExportCoupledFmModelToRtcAndWave()
        {
            var hydroModelBuilder = new HydroModelBuilder();
            var hydroModel = hydroModelBuilder.BuildModel(ModelGroup.FMWaveRtcModels);
            hydroModel.CurrentWorkflow =
                hydroModel.Workflows.First(w => w.Activities.Count == 3);
            var waveModel = hydroModel.Activities.OfType<WaveModel>().FirstOrDefault();
            Assert.NotNull(waveModel);
            int mSize = 2;
            int nSize = 3;
            var xCoordinates = new[] { 0.1, 2.0, 0.1, 2.0, 0.1, 2.0 };
            var yCoordinates = new[] { 1.0, 1.0, 3.0, 3.0, 5.0, 5.0 };

            //         0          1    = M
            //      
            //   0   (0.1,1)------(2,1)
            //         |          |
            //         |          |
            //   1   (0.1,3)------(2,3)
            //         |          |
            //         |          |
            //   2   (0.1,5)------(2,5)
            //
            //  = N

            var grid = CurvilinearGrid.CreateDefault();
            grid.Resize(nSize, mSize, xCoordinates, yCoordinates);
            grid.IsTimeDependent = false;
            waveModel.OuterDomain.Grid = grid;
            waveModel.ModelDefinition.GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.WriteCOM)
                .Value = true;
            waveModel.ModelDefinition.GetModelProperty(KnownWaveCategories.OutputCategory,
                KnownWaveProperties.COMFile).Value = "file.txt";
            waveModel.GetFlowComFilePath = () => "";
            var fmModel = hydroModel.Activities.OfType<WaterFlowFMModel>().FirstOrDefault();
            Assert.NotNull(fmModel);

            var vertices = new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(0, 10),
                    new Coordinate(10, 10),
                    new Coordinate(10, 0)
                };

            var edges = new int[,]
                {
                    {1, 2}, {2, 3}, {3, 4}, {4, 1}, {1, 3}
                };
            fmModel.Grid = UnstructuredGridFactory.CreateFromVertexAndEdgeList(vertices, edges);
            fmModel.ModelDefinition.GetModelProperty(GuiProperties.HisOutputDeltaT).Value = fmModel.TimeStep;
            fmModel.ModelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value = fmModel.TimeStep;
            var observationPointFm = new Feature2DPoint { Name = "ObservationFM" };
            var weirFm = new Weir { Name = "Weir1", Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(0, 100) }) };

            fmModel.Area.ObservationPoints.Add(observationPointFm);
            fmModel.Area.Weirs.Add(weirFm);

            /* Structures definition */
            var input = new Input() { ParameterName = "rtcObsInput", Feature = observationPointFm };
            var output = new Output() { ParameterName = "rtcWeir1Output", Feature = weirFm };
            var rule = new PIDRule { Name = "noot", Inputs = { input }, Outputs = { output } };
            /*  Control groups  */
            var controlGroup = new ControlGroup { Name = "test", Rules = { rule }, };
            controlGroup.Inputs.Add(input);
            controlGroup.Outputs.Add(output);

            var rtcModel = hydroModel.Activities.OfType<RealTimeControlModel>().FirstOrDefault();
            Assert.NotNull(rtcModel);
            rtcModel.ControlGroups.Add(controlGroup);

            /* Coupert 2dToRtc */
            var flowObservationFmOutputDataItem = rtcModel.GetChildDataItemsFromControlledModelsForLocation(observationPointFm).First();
            var rtcInputdataItemFm = rtcModel.AllDataItems.First(di => di.ValueConverter != null && ReferenceEquals(di.ValueConverter.OriginalValue, input));

            // link
            rtcInputdataItemFm.LinkTo(flowObservationFmOutputDataItem);
            var dirPath = Path.GetFullPath("fmrtcwave");
            if (Directory.Exists(dirPath))
            {
                Directory.Delete(dirPath, true);
            }
            var dirInfo = Directory.CreateDirectory("fmrtcwave");
            var exporter = new DHydroConfigXmlExporter
            {
                ExportFilePath = Path.Combine(dirInfo.FullName, "dimr.xml")
            };
            exporter.Export(hydroModel, null);

            Assert.IsTrue(File.Exists(Path.Combine(dirInfo.FullName, "dimr.xml")));
            Assert.IsTrue(Directory.Exists(Path.Combine(dirInfo.FullName, "dflowfm")));
            Assert.IsTrue(File.Exists(Path.Combine(dirInfo.FullName, "dflowfm/FlowFM.mdu")));
            Assert.IsTrue(Directory.Exists(Path.Combine(dirInfo.FullName, "rtc")));
            Assert.IsTrue(File.Exists(Path.Combine(dirInfo.FullName, "rtc/rtcToolsConfig.xml")));
            Assert.IsTrue(Directory.Exists(Path.Combine(dirInfo.FullName, "wave")));
            Assert.IsTrue(File.Exists(Path.Combine(dirInfo.FullName, "wave/Waves.mdw")));
        }

        [Test]
        public void ExportStandAloneFm()
        {
            var dirPath = Path.GetFullPath("fmdimr");
            if (Directory.Exists(dirPath))
            {
                Directory.Delete(dirPath, true);
            }
            var dirInfo = Directory.CreateDirectory("fmdimr");
            var exporter = new DHydroConfigXmlExporter
            {
                ExportFilePath = Path.Combine(dirInfo.FullName, "dimr.xml")
            };
            var waterFlowFmModel = new WaterFlowFMModel();
            Assert.NotNull(waterFlowFmModel);

            var vertices = new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(0, 10),
                    new Coordinate(10, 10),
                    new Coordinate(10, 0)
                };

            var edges = new int[,]
                {
                    {1, 2}, {2, 3}, {3, 4}, {4, 1}, {1, 3}
                };
            waterFlowFmModel.Grid = UnstructuredGridFactory.CreateFromVertexAndEdgeList(vertices, edges);
            exporter.Export(waterFlowFmModel, null);

            Assert.IsTrue(File.Exists(Path.Combine(dirInfo.FullName, "dimr.xml")));
            Assert.IsTrue(Directory.Exists(Path.Combine(dirInfo.FullName, "dflowfm")));
            Assert.IsTrue(File.Exists(Path.Combine(dirInfo.FullName, "dflowfm/FlowFM.mdu")));
        }

        [Test]
        public void ExportIntegratedModelWithRtcOnly_ReturnsNoDimrModels()
        {
            var hydroModelBuilder = new HydroModelBuilder();
            var hydroModel = hydroModelBuilder.BuildModel(ModelGroup.Empty);
            var rtcModel = new RealTimeControlModel("rtcModel");
            hydroModel.Activities.Add(rtcModel);
            
            var dimrModels = (IEnumerable<IDimrModel>)TypeUtils.CallPrivateStaticMethod(typeof(DHydroConfigXmlExporter), "GetDimrModelsFromItem", hydroModel);
            Assert.IsEmpty(dimrModels);
        }
    }
}
