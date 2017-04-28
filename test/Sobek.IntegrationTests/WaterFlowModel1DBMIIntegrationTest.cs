using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using NUnit.Framework;
using Point = NetTopologySuite.Geometries.Point;

// ReSharper disable NotResolvedInText

namespace Sobek.IntegrationTests
{
    [TestFixture]
    public class WaterFlowModel1DBMIIntegrationTest
    {
        private WaterFlowModel1D model;
        private IChannel channel;
        private const double chainage = 45.0;
        
        [SetUp]
        public void SetUp()
        {
            var tmpDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            FileUtils.CreateDirectoryIfNotExists(tmpDir);

            model = new WaterFlowModel1D()
            {
                Network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(0, 100)),
                ExplicitWorkingDirectory = tmpDir
            };
            channel = model.Network.Channels.FirstOrDefault();
            if (channel == null) throw new ArgumentNullException("branch");
            CrossSectionHelper.AddCrossSection(channel, 90.0, -10.0d);
            var boundaryCondition1 = model.BoundaryConditions.FirstOrDefault();
            if (boundaryCondition1 == null) throw new ArgumentNullException("boundaryCondition1");
            boundaryCondition1.DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries;

            var t = DateTime.Now;
            // Round t down to nearest minute: (See TOOLS-23841)
            t = new DateTime(t.Ticks - (t.Ticks % (1000 * 1000 * 10 * 60)));

            DateTime bcStartTime = t;
            boundaryCondition1.Data[bcStartTime] = 1.0;
            boundaryCondition1.Data[bcStartTime.AddDays(1)] = 1.0;

            var boundaryCondition2 = model.BoundaryConditions.ElementAt(1);
            if (boundaryCondition2 == null) throw new ArgumentNullException("boundaryCondition2");
            boundaryCondition2.DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries;
            boundaryCondition2.Data[bcStartTime] = 0.0;
            boundaryCondition2.Data[bcStartTime.AddDays(1)] = 0.0;

            var offsets = new double[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
            HydroNetworkHelper.GenerateDiscretization(model.NetworkDiscretization, channel, offsets);
        }

        [TearDown]
        public void TearDown()
        {
            model.Dispose();

            // Jaap Zeekant: For the time being do not delete data, we need it for analysis
            //var tmpDir = model.ExplicitWorkingDirectory;
            //Thread.Sleep(20); // need this because disposing is done quickly but releasing crap isn't
            //try
            //{
            //    FileUtils.DeleteIfExists(tmpDir);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("Could not delete temp. directory: " + e.Message);
            //    return;
            //}
            //Console.WriteLine("Deleted temp folder: {0}",tmpDir);
        }
        
        #region ObservationPoint
        private const int OBSERVATION_POINT_ID = 1;
        private const string OBSERVATION_POINT_NAME = "observationPoint1";
        

        [Test]
        [Category(TestCategory.Integration)]
        public void ObservationPointDischargeTest()
        {
            FileWriterTestHelper.AddObservationPoint(
                channel, 
                OBSERVATION_POINT_ID, 
                OBSERVATION_POINT_NAME, 
                chainage);
            
            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.ObservationPoints,
                                                    OBSERVATION_POINT_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterDischarge,
                                                    14991.86));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ObservationPointVelocityTest()
        {
            FileWriterTestHelper.AddObservationPoint(
                channel,
                OBSERVATION_POINT_ID,
                OBSERVATION_POINT_NAME,
                chainage);
            
            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.ObservationPoints,
                                                    OBSERVATION_POINT_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterVelocity,
                                                    6.218));

        }

        [Test]
        [Ignore("Not Implemeted")]
        public void ObservationPointFlowAreaTest()
        {
            FileWriterTestHelper.AddObservationPoint(
                channel,
                OBSERVATION_POINT_ID,
                OBSERVATION_POINT_NAME,
                chainage);
            
            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.ObservationPoints,
                                                    OBSERVATION_POINT_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterFlowArea,
                                                    1.667));

        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void ObservationPointWaterDepthTest()
        {
            FileWriterTestHelper.AddObservationPoint(
                channel,
                OBSERVATION_POINT_ID,
                OBSERVATION_POINT_NAME,
                chainage);
            
            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.ObservationPoints,
                                                    OBSERVATION_POINT_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterDepth,
                                                    10.75));

        }
        #endregion

        #region SimpleWeir

        [Test]
        [Category(TestCategory.Integration)]
        public void SimpleWeirDischargeTest()
        {
            channel.AddSimpleWeir(
                StructureFileWriterTestHelper.WEIR_ID,
                StructureFileWriterTestHelper.WEIR_NAME,
                0.5, //StructureFileWriterTestHelper.WEIR_CREST_LEVEL,
                StructureFileWriterTestHelper.WEIR_CREST_WIDTH,
                chainage,
                StructureFileWriterTestHelper.WEIR_FLOW_DIRECTION,
                StructureFileWriterTestHelper.WEIR_DISCHARGE_COEFF,
                StructureFileWriterTestHelper.WEIR_LATERAL_DISCHARGE_COEFF);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterDischarge,
                                                    12.0554));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void SimpleWeirVelocityTest()
        {
            channel.AddSimpleWeir(
                StructureFileWriterTestHelper.WEIR_ID,
                StructureFileWriterTestHelper.WEIR_NAME,
                0.5, //StructureFileWriterTestHelper.WEIR_CREST_LEVEL,
                StructureFileWriterTestHelper.WEIR_CREST_WIDTH,
                chainage,
                StructureFileWriterTestHelper.WEIR_FLOW_DIRECTION,
                StructureFileWriterTestHelper.WEIR_DISCHARGE_COEFF,
                StructureFileWriterTestHelper.WEIR_LATERAL_DISCHARGE_COEFF);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterVelocity,
                                                    1.808));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void SimpleWeirFlowAreaTest()
        {
            channel.AddSimpleWeir(
                StructureFileWriterTestHelper.WEIR_ID,
                StructureFileWriterTestHelper.WEIR_NAME,
                0.5, //StructureFileWriterTestHelper.WEIR_CREST_LEVEL,
                StructureFileWriterTestHelper.WEIR_CREST_WIDTH,
                chainage,
                StructureFileWriterTestHelper.WEIR_FLOW_DIRECTION,
                StructureFileWriterTestHelper.WEIR_DISCHARGE_COEFF,
                StructureFileWriterTestHelper.WEIR_LATERAL_DISCHARGE_COEFF);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterFlowArea,
                                                    6.667));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void SimpleWeirPressureDifferenceTest()
        {
            channel.AddSimpleWeir(
                StructureFileWriterTestHelper.WEIR_ID,
                StructureFileWriterTestHelper.WEIR_NAME,
                0.5, //StructureFileWriterTestHelper.WEIR_CREST_LEVEL,
                StructureFileWriterTestHelper.WEIR_CREST_WIDTH,
                chainage,
                StructureFileWriterTestHelper.WEIR_FLOW_DIRECTION,
                StructureFileWriterTestHelper.WEIR_DISCHARGE_COEFF,
                StructureFileWriterTestHelper.WEIR_LATERAL_DISCHARGE_COEFF);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructurePressureDifference,
                                                    1226.250));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void SimpleWeirCrestLevelTest()
        {
            var crestLevel = 0.5;
            channel.AddSimpleWeir(
                StructureFileWriterTestHelper.WEIR_ID,
                StructureFileWriterTestHelper.WEIR_NAME,
                crestLevel, //StructureFileWriterTestHelper.WEIR_CREST_LEVEL,
                StructureFileWriterTestHelper.WEIR_CREST_WIDTH,
                chainage,
                StructureFileWriterTestHelper.WEIR_FLOW_DIRECTION,
                StructureFileWriterTestHelper.WEIR_DISCHARGE_COEFF,
                StructureFileWriterTestHelper.WEIR_LATERAL_DISCHARGE_COEFF);

            double checkValue = crestLevel - 1.0;
            Assert.IsTrue(CheckGetSetGetAfterModelInitialization(WaterFlowParametersCategories.Weirs,
                                                                    StructureFileWriterTestHelper.WEIR_ID.ToString(),
                                                                    FunctionAttributes.StandardNames.StructureCrestLevel,
                                                                    crestLevel,
                                                                    checkValue));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void SimpleWeirCrestWidthTest()
        {
            channel.AddSimpleWeir(
                StructureFileWriterTestHelper.WEIR_ID,
                StructureFileWriterTestHelper.WEIR_NAME,
                0.5, //StructureFileWriterTestHelper.WEIR_CREST_LEVEL,
                StructureFileWriterTestHelper.WEIR_CREST_WIDTH,
                chainage,
                StructureFileWriterTestHelper.WEIR_FLOW_DIRECTION,
                StructureFileWriterTestHelper.WEIR_DISCHARGE_COEFF,
                StructureFileWriterTestHelper.WEIR_LATERAL_DISCHARGE_COEFF);

            double checkValue = StructureFileWriterTestHelper.WEIR_CREST_WIDTH - 1.0;
            Assert.IsTrue(CheckGetSetGetAfterModelInitialization(WaterFlowParametersCategories.Weirs,
                                                                    StructureFileWriterTestHelper.WEIR_ID.ToString(),
                                                                    FunctionAttributes.StandardNames.StructureCrestWidth,
                                                                    StructureFileWriterTestHelper.WEIR_CREST_WIDTH,
                                                                    checkValue));
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void SimpleWeirWaterLevelUpTest()
        {
            channel.AddSimpleWeir(
                StructureFileWriterTestHelper.WEIR_ID,
                StructureFileWriterTestHelper.WEIR_NAME,
                0.5, //StructureFileWriterTestHelper.WEIR_CREST_LEVEL,
                StructureFileWriterTestHelper.WEIR_CREST_WIDTH,
                chainage,
                StructureFileWriterTestHelper.WEIR_FLOW_DIRECTION,
                StructureFileWriterTestHelper.WEIR_DISCHARGE_COEFF,
                StructureFileWriterTestHelper.WEIR_LATERAL_DISCHARGE_COEFF);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterLevelUpstream,
                                                    1.0));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void SimpleWeirWaterLevelDownTest()
        {
            channel.AddSimpleWeir(
                StructureFileWriterTestHelper.WEIR_ID,
                StructureFileWriterTestHelper.WEIR_NAME,
                0.5, //StructureFileWriterTestHelper.WEIR_CREST_LEVEL,
                StructureFileWriterTestHelper.WEIR_CREST_WIDTH,
                chainage,
                StructureFileWriterTestHelper.WEIR_FLOW_DIRECTION,
                StructureFileWriterTestHelper.WEIR_DISCHARGE_COEFF,
                StructureFileWriterTestHelper.WEIR_LATERAL_DISCHARGE_COEFF);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterLevelDownstream,
                                                    0.00000000));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void SimpleWeirHeadDifferenceTest()
        {
            channel.AddSimpleWeir(
                StructureFileWriterTestHelper.WEIR_ID,
                StructureFileWriterTestHelper.WEIR_NAME,
                0.5, //StructureFileWriterTestHelper.WEIR_CREST_LEVEL,
                StructureFileWriterTestHelper.WEIR_CREST_WIDTH,
                chainage,
                StructureFileWriterTestHelper.WEIR_FLOW_DIRECTION,
                StructureFileWriterTestHelper.WEIR_DISCHARGE_COEFF,
                StructureFileWriterTestHelper.WEIR_LATERAL_DISCHARGE_COEFF);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterHead,
                                                    1.00000000));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void SimpleWeirWaterLevelAtCrestTest()
        {
            channel.AddSimpleWeir(
                StructureFileWriterTestHelper.WEIR_ID,
                StructureFileWriterTestHelper.WEIR_NAME,
                0.5, //StructureFileWriterTestHelper.WEIR_CREST_LEVEL,
                StructureFileWriterTestHelper.WEIR_CREST_WIDTH,
                chainage,
                StructureFileWriterTestHelper.WEIR_FLOW_DIRECTION,
                StructureFileWriterTestHelper.WEIR_DISCHARGE_COEFF,
                StructureFileWriterTestHelper.WEIR_LATERAL_DISCHARGE_COEFF);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterLevelAtCrest,
                                                    1.00000000));

        }

        #endregion

        #region AdvancedWeir

        [Test]
        [Category(TestCategory.Integration)]
        public void AdvancedWeirDischargeTest()
        {
            channel.AddAdvancedWeir(
                StructureFileWriterTestHelper.ADV_WEIR_ID,
                StructureFileWriterTestHelper.ADV_WEIR_NAME,
                chainage,
                0.5,
                StructureFileWriterTestHelper.ADV_WEIR_CREST_WIDTH,
                StructureFileWriterTestHelper.ADV_WEIR_NUM_PIERS,
                StructureFileWriterTestHelper.ADV_WEIR_UPSTREAM_FACE_POS,
                StructureFileWriterTestHelper.ADV_WEIR_DESIGN_HEAD_POS,
                StructureFileWriterTestHelper.ADV_WEIR_PIER_CONTRACTION_POS,
                StructureFileWriterTestHelper.ADV_WEIR_ABUT_CONTRACTION_POS,
                StructureFileWriterTestHelper.ADV_WEIR_UPSTREAM_FACE_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_DESIGN_HEAD_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_PIER_CONTRACTION_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_ABUT_CONTRACTION_NEG);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.ADV_WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterDischarge,
                                                    3.8));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AdvancedWeirVelocityTest()
        {
            channel.AddAdvancedWeir(
                StructureFileWriterTestHelper.ADV_WEIR_ID,
                StructureFileWriterTestHelper.ADV_WEIR_NAME,
                chainage,
                0.5,
                StructureFileWriterTestHelper.ADV_WEIR_CREST_WIDTH,
                StructureFileWriterTestHelper.ADV_WEIR_NUM_PIERS,
                StructureFileWriterTestHelper.ADV_WEIR_UPSTREAM_FACE_POS,
                StructureFileWriterTestHelper.ADV_WEIR_DESIGN_HEAD_POS,
                StructureFileWriterTestHelper.ADV_WEIR_PIER_CONTRACTION_POS,
                StructureFileWriterTestHelper.ADV_WEIR_ABUT_CONTRACTION_POS,
                StructureFileWriterTestHelper.ADV_WEIR_UPSTREAM_FACE_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_DESIGN_HEAD_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_PIER_CONTRACTION_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_ABUT_CONTRACTION_NEG);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.ADV_WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterVelocity,
                                                    1.982));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AdvancedWeirFlowAreaTest()
        {
            channel.AddAdvancedWeir(
                StructureFileWriterTestHelper.ADV_WEIR_ID,
                StructureFileWriterTestHelper.ADV_WEIR_NAME,
                chainage,
                0.5,
                StructureFileWriterTestHelper.ADV_WEIR_CREST_WIDTH,
                StructureFileWriterTestHelper.ADV_WEIR_NUM_PIERS,
                StructureFileWriterTestHelper.ADV_WEIR_UPSTREAM_FACE_POS,
                StructureFileWriterTestHelper.ADV_WEIR_DESIGN_HEAD_POS,
                StructureFileWriterTestHelper.ADV_WEIR_PIER_CONTRACTION_POS,
                StructureFileWriterTestHelper.ADV_WEIR_ABUT_CONTRACTION_POS,
                StructureFileWriterTestHelper.ADV_WEIR_UPSTREAM_FACE_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_DESIGN_HEAD_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_PIER_CONTRACTION_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_ABUT_CONTRACTION_NEG);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.ADV_WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterFlowArea,
                                                    1.917));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AdvancedWeirPressureDifferenceTest()
        {
            channel.AddAdvancedWeir(
                StructureFileWriterTestHelper.ADV_WEIR_ID,
                StructureFileWriterTestHelper.ADV_WEIR_NAME,
                chainage,
                0.5,
                StructureFileWriterTestHelper.ADV_WEIR_CREST_WIDTH,
                StructureFileWriterTestHelper.ADV_WEIR_NUM_PIERS,
                StructureFileWriterTestHelper.ADV_WEIR_UPSTREAM_FACE_POS,
                StructureFileWriterTestHelper.ADV_WEIR_DESIGN_HEAD_POS,
                StructureFileWriterTestHelper.ADV_WEIR_PIER_CONTRACTION_POS,
                StructureFileWriterTestHelper.ADV_WEIR_ABUT_CONTRACTION_POS,
                StructureFileWriterTestHelper.ADV_WEIR_UPSTREAM_FACE_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_DESIGN_HEAD_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_PIER_CONTRACTION_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_ABUT_CONTRACTION_NEG);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.ADV_WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructurePressureDifference,
                                                    1226.250));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AdvancedWeirCrestLevelTest()
        {
            channel.AddAdvancedWeir(
                StructureFileWriterTestHelper.ADV_WEIR_ID,
                StructureFileWriterTestHelper.ADV_WEIR_NAME,
                chainage,
                0.5,
                StructureFileWriterTestHelper.ADV_WEIR_CREST_WIDTH,
                StructureFileWriterTestHelper.ADV_WEIR_NUM_PIERS,
                StructureFileWriterTestHelper.ADV_WEIR_UPSTREAM_FACE_POS,
                StructureFileWriterTestHelper.ADV_WEIR_DESIGN_HEAD_POS,
                StructureFileWriterTestHelper.ADV_WEIR_PIER_CONTRACTION_POS,
                StructureFileWriterTestHelper.ADV_WEIR_ABUT_CONTRACTION_POS,
                StructureFileWriterTestHelper.ADV_WEIR_UPSTREAM_FACE_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_DESIGN_HEAD_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_PIER_CONTRACTION_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_ABUT_CONTRACTION_NEG);

            double checkValue = 0.5 - 1.0;
            Assert.IsTrue(CheckGetSetGetAfterModelInitialization(WaterFlowParametersCategories.Weirs,
                                                                    StructureFileWriterTestHelper.ADV_WEIR_ID.ToString(),
                                                                    FunctionAttributes.StandardNames.StructureCrestLevel,
                                                                    0.5,
                                                                    checkValue));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AdvancedWeirCrestWidthTest()
        {
            channel.AddAdvancedWeir(
                StructureFileWriterTestHelper.ADV_WEIR_ID,
                StructureFileWriterTestHelper.ADV_WEIR_NAME,
                chainage,
                0.5,
                StructureFileWriterTestHelper.ADV_WEIR_CREST_WIDTH,
                StructureFileWriterTestHelper.ADV_WEIR_NUM_PIERS,
                StructureFileWriterTestHelper.ADV_WEIR_UPSTREAM_FACE_POS,
                StructureFileWriterTestHelper.ADV_WEIR_DESIGN_HEAD_POS,
                StructureFileWriterTestHelper.ADV_WEIR_PIER_CONTRACTION_POS,
                StructureFileWriterTestHelper.ADV_WEIR_ABUT_CONTRACTION_POS,
                StructureFileWriterTestHelper.ADV_WEIR_UPSTREAM_FACE_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_DESIGN_HEAD_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_PIER_CONTRACTION_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_ABUT_CONTRACTION_NEG);

            double checkValue = StructureFileWriterTestHelper.ADV_WEIR_CREST_WIDTH - 1.0;
            Assert.IsTrue(CheckGetSetGetAfterModelInitialization(WaterFlowParametersCategories.Weirs,
                                                                    StructureFileWriterTestHelper.ADV_WEIR_ID.ToString(),
                                                                    FunctionAttributes.StandardNames.StructureCrestWidth,
                                                                    StructureFileWriterTestHelper.ADV_WEIR_CREST_WIDTH,
                                                                    checkValue));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AdvancedWeirWaterLevelUpTest()
        {
            channel.AddAdvancedWeir(
                StructureFileWriterTestHelper.ADV_WEIR_ID,
                StructureFileWriterTestHelper.ADV_WEIR_NAME,
                chainage,
                0.5,
                StructureFileWriterTestHelper.ADV_WEIR_CREST_WIDTH,
                StructureFileWriterTestHelper.ADV_WEIR_NUM_PIERS,
                StructureFileWriterTestHelper.ADV_WEIR_UPSTREAM_FACE_POS,
                StructureFileWriterTestHelper.ADV_WEIR_DESIGN_HEAD_POS,
                StructureFileWriterTestHelper.ADV_WEIR_PIER_CONTRACTION_POS,
                StructureFileWriterTestHelper.ADV_WEIR_ABUT_CONTRACTION_POS,
                StructureFileWriterTestHelper.ADV_WEIR_UPSTREAM_FACE_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_DESIGN_HEAD_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_PIER_CONTRACTION_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_ABUT_CONTRACTION_NEG);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.ADV_WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterLevelUpstream,
                                                    1.0));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AdvancedWeirWaterLevelDownTest()
        {
            channel.AddAdvancedWeir(
                StructureFileWriterTestHelper.ADV_WEIR_ID,
                StructureFileWriterTestHelper.ADV_WEIR_NAME,
                chainage,
                0.5,
                StructureFileWriterTestHelper.ADV_WEIR_CREST_WIDTH,
                StructureFileWriterTestHelper.ADV_WEIR_NUM_PIERS,
                StructureFileWriterTestHelper.ADV_WEIR_UPSTREAM_FACE_POS,
                StructureFileWriterTestHelper.ADV_WEIR_DESIGN_HEAD_POS,
                StructureFileWriterTestHelper.ADV_WEIR_PIER_CONTRACTION_POS,
                StructureFileWriterTestHelper.ADV_WEIR_ABUT_CONTRACTION_POS,
                StructureFileWriterTestHelper.ADV_WEIR_UPSTREAM_FACE_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_DESIGN_HEAD_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_PIER_CONTRACTION_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_ABUT_CONTRACTION_NEG);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.ADV_WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterLevelDownstream,
                                                    0.00000000));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AdvancedWeirHeadDifferenceTest()
        {
            channel.AddAdvancedWeir(
                StructureFileWriterTestHelper.ADV_WEIR_ID,
                StructureFileWriterTestHelper.ADV_WEIR_NAME,
                chainage,
                0.5,
                StructureFileWriterTestHelper.ADV_WEIR_CREST_WIDTH,
                StructureFileWriterTestHelper.ADV_WEIR_NUM_PIERS,
                StructureFileWriterTestHelper.ADV_WEIR_UPSTREAM_FACE_POS,
                StructureFileWriterTestHelper.ADV_WEIR_DESIGN_HEAD_POS,
                StructureFileWriterTestHelper.ADV_WEIR_PIER_CONTRACTION_POS,
                StructureFileWriterTestHelper.ADV_WEIR_ABUT_CONTRACTION_POS,
                StructureFileWriterTestHelper.ADV_WEIR_UPSTREAM_FACE_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_DESIGN_HEAD_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_PIER_CONTRACTION_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_ABUT_CONTRACTION_NEG);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.ADV_WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterHead,
                                                    1.00000000));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AdvancedWeirWaterLevelAtCrestTest()
        {
            channel.AddAdvancedWeir(
                StructureFileWriterTestHelper.ADV_WEIR_ID,
                StructureFileWriterTestHelper.ADV_WEIR_NAME,
                chainage,
                0.5,
                StructureFileWriterTestHelper.ADV_WEIR_CREST_WIDTH,
                StructureFileWriterTestHelper.ADV_WEIR_NUM_PIERS,
                StructureFileWriterTestHelper.ADV_WEIR_UPSTREAM_FACE_POS,
                StructureFileWriterTestHelper.ADV_WEIR_DESIGN_HEAD_POS,
                StructureFileWriterTestHelper.ADV_WEIR_PIER_CONTRACTION_POS,
                StructureFileWriterTestHelper.ADV_WEIR_ABUT_CONTRACTION_POS,
                StructureFileWriterTestHelper.ADV_WEIR_UPSTREAM_FACE_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_DESIGN_HEAD_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_PIER_CONTRACTION_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_ABUT_CONTRACTION_NEG);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.ADV_WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterLevelAtCrest,
                                                    1.0));

        }

        #endregion

        #region UniversalWeir

        [Test]
        [Category(TestCategory.Integration)]
        public void UniversalWeirDischargeTest()
        {
            channel.AddUniversalWeir(
                StructureFileWriterTestHelper.UNI_WEIR_ID,
                StructureFileWriterTestHelper.UNI_WEIR_NAME,
                chainage,
                StructureFileWriterTestHelper.UNI_WEIR_FLOW_DIRECTION,
                UNI_WEIR_Y_VALUES.ToArray(),
                UNI_WEIR_Z_VALUES.ToArray(),
                StructureFileWriterTestHelper.UNI_WEIR_DISCHARGE_COEFF);
            
            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.UNI_WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterDischarge,
                                                    1.241));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void UniversalWeirVelocityTest()
        {
            channel.AddUniversalWeir(
                StructureFileWriterTestHelper.UNI_WEIR_ID,
                StructureFileWriterTestHelper.UNI_WEIR_NAME,
                chainage,
                StructureFileWriterTestHelper.UNI_WEIR_FLOW_DIRECTION,
                UNI_WEIR_Y_VALUES.ToArray(),
                UNI_WEIR_Z_VALUES.ToArray(),
                StructureFileWriterTestHelper.UNI_WEIR_DISCHARGE_COEFF);
            
            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.UNI_WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterVelocity,
                                                    1.7925679683367921));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void UniversalWeirFlowAreaTest()
        {
            channel.AddUniversalWeir(
                StructureFileWriterTestHelper.UNI_WEIR_ID,
                StructureFileWriterTestHelper.UNI_WEIR_NAME,
                chainage,
                StructureFileWriterTestHelper.UNI_WEIR_FLOW_DIRECTION,
                UNI_WEIR_Y_VALUES.ToArray(),
                UNI_WEIR_Z_VALUES.ToArray(),
                StructureFileWriterTestHelper.UNI_WEIR_DISCHARGE_COEFF);
            
            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.UNI_WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterFlowArea,
                                                    0.6923));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void UniversalWeirPressureDifferenceTest()
        {
            channel.AddUniversalWeir(
                StructureFileWriterTestHelper.UNI_WEIR_ID,
                StructureFileWriterTestHelper.UNI_WEIR_NAME,
                chainage,
                StructureFileWriterTestHelper.UNI_WEIR_FLOW_DIRECTION,
                UNI_WEIR_Y_VALUES.ToArray(),
                UNI_WEIR_Z_VALUES.ToArray(),
                StructureFileWriterTestHelper.UNI_WEIR_DISCHARGE_COEFF);
            
            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.UNI_WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructurePressureDifference,
                                                    1226.250));

        }

        static readonly List<double> UNI_WEIR_Y_VALUES = new List<double> { -5.0, -2.0, 2.0, 5.0 };
        static readonly List<double> UNI_WEIR_Z_VALUES = new List<double> { 10.0, 0.5, 0.5, 10.0 };


        [Test]
        [Category(TestCategory.Integration)]
        public void UniversalWeirCrestLevelTest()
        {
            channel.AddUniversalWeir(
                StructureFileWriterTestHelper.UNI_WEIR_ID,
                StructureFileWriterTestHelper.UNI_WEIR_NAME,
                chainage,
                StructureFileWriterTestHelper.UNI_WEIR_FLOW_DIRECTION,
                UNI_WEIR_Y_VALUES.ToArray(),
                UNI_WEIR_Z_VALUES.ToArray(),
                StructureFileWriterTestHelper.UNI_WEIR_DISCHARGE_COEFF);
            
            double checkValue = UNI_WEIR_Z_VALUES.ToArray().Min() - 1.0;
            Assert.IsTrue(CheckGetSetGetAfterModelInitialization(WaterFlowParametersCategories.Weirs,
                                                                    StructureFileWriterTestHelper.UNI_WEIR_ID.ToString(),
                                                                    FunctionAttributes.StandardNames.StructureCrestLevel,
                                                                    UNI_WEIR_Z_VALUES.ToArray().Min(),
                                                                    checkValue));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void UniversalWeirWaterLevelUpTest()
        {
            channel.AddUniversalWeir(
                StructureFileWriterTestHelper.UNI_WEIR_ID,
                StructureFileWriterTestHelper.UNI_WEIR_NAME,
                chainage,
                StructureFileWriterTestHelper.UNI_WEIR_FLOW_DIRECTION,
                UNI_WEIR_Y_VALUES.ToArray(),
                UNI_WEIR_Z_VALUES.ToArray(),
                StructureFileWriterTestHelper.UNI_WEIR_DISCHARGE_COEFF);
            
            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.UNI_WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterLevelUpstream,
                                                    1.0));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void UniversalWeirWaterLevelDownTest()
        {
            channel.AddUniversalWeir(
                StructureFileWriterTestHelper.UNI_WEIR_ID,
                StructureFileWriterTestHelper.UNI_WEIR_NAME,
                chainage,
                StructureFileWriterTestHelper.UNI_WEIR_FLOW_DIRECTION,
                UNI_WEIR_Y_VALUES.ToArray(),
                UNI_WEIR_Z_VALUES.ToArray(),
                StructureFileWriterTestHelper.UNI_WEIR_DISCHARGE_COEFF);
            
            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.UNI_WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterLevelDownstream,
                                                    0.00000000));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void UniversalWeirHeadDifferenceTest()
        {
            channel.AddUniversalWeir(
                StructureFileWriterTestHelper.UNI_WEIR_ID,
                StructureFileWriterTestHelper.UNI_WEIR_NAME,
                chainage,
                StructureFileWriterTestHelper.UNI_WEIR_FLOW_DIRECTION,
                UNI_WEIR_Y_VALUES.ToArray(),
                UNI_WEIR_Z_VALUES.ToArray(),
                StructureFileWriterTestHelper.UNI_WEIR_DISCHARGE_COEFF);
            
            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.UNI_WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterHead,
                                                    1.00000000));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void UniversalWeirWaterLevelAtCrestTest()
        {
            channel.AddUniversalWeir(
                StructureFileWriterTestHelper.UNI_WEIR_ID,
                StructureFileWriterTestHelper.UNI_WEIR_NAME,
                chainage,
                StructureFileWriterTestHelper.UNI_WEIR_FLOW_DIRECTION,
                UNI_WEIR_Y_VALUES.ToArray(),
                UNI_WEIR_Z_VALUES.ToArray(),
                StructureFileWriterTestHelper.UNI_WEIR_DISCHARGE_COEFF);
            
            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.UNI_WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterLevelAtCrest,
                                                    1.00000000));
        }

        #endregion

        #region RiverWeir

        [Test]
        [Category(TestCategory.Integration)]
        public void RiverWeirDischargeTest()
        {
            channel.AddRiverWeir(
                StructureFileWriterTestHelper.RIVER_WEIR_ID,
                StructureFileWriterTestHelper.RIVER_WEIR_NAME,
                chainage,
                0.5, //StructureFileWriterTestHelper.RIVER_WEIR_CREST_LEVEL,
                StructureFileWriterTestHelper.RIVER_WEIR_CREST_WIDTH,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_CW_COEFF,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_SLIM_LIMIT,
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_CW_COEFF,
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_SLIM_LIMIT,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_SF.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_POS_RED.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_SF.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_RED.ToArray());

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.RIVER_WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterDischarge,
                                                    4.219));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RiverWeirVelocityTest()
        {
            channel.AddRiverWeir(
                StructureFileWriterTestHelper.RIVER_WEIR_ID,
                StructureFileWriterTestHelper.RIVER_WEIR_NAME,
                chainage,
                0.5, //StructureFileWriterTestHelper.RIVER_WEIR_CREST_LEVEL,
                StructureFileWriterTestHelper.RIVER_WEIR_CREST_WIDTH,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_CW_COEFF,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_SLIM_LIMIT,
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_CW_COEFF,
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_SLIM_LIMIT,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_SF.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_POS_RED.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_SF.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_RED.ToArray());

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.RIVER_WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterVelocity,
                                                    2.5316));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RiverWeirFlowAreaTest()
        {
            channel.AddRiverWeir(
                StructureFileWriterTestHelper.RIVER_WEIR_ID,
                StructureFileWriterTestHelper.RIVER_WEIR_NAME,
                chainage,
                0.5, //StructureFileWriterTestHelper.RIVER_WEIR_CREST_LEVEL,
                StructureFileWriterTestHelper.RIVER_WEIR_CREST_WIDTH,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_CW_COEFF,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_SLIM_LIMIT,
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_CW_COEFF,
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_SLIM_LIMIT,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_SF.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_POS_RED.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_SF.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_RED.ToArray());

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.RIVER_WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterFlowArea,
                                                    1.667));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RiverWeirPressureDifferenceTest()
        {
            channel.AddRiverWeir(
                StructureFileWriterTestHelper.RIVER_WEIR_ID,
                StructureFileWriterTestHelper.RIVER_WEIR_NAME,
                chainage,
                0.5, //StructureFileWriterTestHelper.RIVER_WEIR_CREST_LEVEL,
                StructureFileWriterTestHelper.RIVER_WEIR_CREST_WIDTH,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_CW_COEFF,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_SLIM_LIMIT,
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_CW_COEFF,
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_SLIM_LIMIT,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_SF.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_POS_RED.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_SF.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_RED.ToArray());

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.RIVER_WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructurePressureDifference,
                                                    1226.250));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RiverWeirCrestLevelTest()
        {
            var crestLevel = 0.5;
            channel.AddRiverWeir(
                StructureFileWriterTestHelper.RIVER_WEIR_ID,
                StructureFileWriterTestHelper.RIVER_WEIR_NAME,
                chainage,
                crestLevel, //StructureFileWriterTestHelper.RIVER_WEIR_CREST_LEVEL,
                StructureFileWriterTestHelper.RIVER_WEIR_CREST_WIDTH,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_CW_COEFF,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_SLIM_LIMIT,
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_CW_COEFF,
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_SLIM_LIMIT,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_SF.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_POS_RED.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_SF.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_RED.ToArray());

            double checkValue = crestLevel - 1.0;
            Assert.IsTrue(CheckGetSetGetAfterModelInitialization(WaterFlowParametersCategories.Weirs,
                                                                    StructureFileWriterTestHelper.RIVER_WEIR_ID.ToString(),
                                                                    FunctionAttributes.StandardNames.StructureCrestLevel,
                                                                    crestLevel,
                                                                    checkValue));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RiverWeirCrestWidthTest()
        {
            channel.AddRiverWeir(
                StructureFileWriterTestHelper.RIVER_WEIR_ID,
                StructureFileWriterTestHelper.RIVER_WEIR_NAME,
                chainage,
                0.5, //StructureFileWriterTestHelper.RIVER_WEIR_CREST_LEVEL,
                StructureFileWriterTestHelper.RIVER_WEIR_CREST_WIDTH,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_CW_COEFF,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_SLIM_LIMIT,
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_CW_COEFF,
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_SLIM_LIMIT,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_SF.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_POS_RED.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_SF.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_RED.ToArray());

            double checkValue = StructureFileWriterTestHelper.RIVER_WEIR_CREST_WIDTH - 1.0;
            Assert.IsTrue(CheckGetSetGetAfterModelInitialization(WaterFlowParametersCategories.Weirs,
                                                                    StructureFileWriterTestHelper.RIVER_WEIR_ID.ToString(),
                                                                    FunctionAttributes.StandardNames.StructureCrestWidth,
                                                                    StructureFileWriterTestHelper.RIVER_WEIR_CREST_WIDTH,
                                                                    checkValue));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RiverWeirWaterLevelUpTest()
        {
            channel.AddRiverWeir(
                StructureFileWriterTestHelper.RIVER_WEIR_ID,
                StructureFileWriterTestHelper.RIVER_WEIR_NAME,
                chainage,
                0.5, //StructureFileWriterTestHelper.RIVER_WEIR_CREST_LEVEL,
                StructureFileWriterTestHelper.RIVER_WEIR_CREST_WIDTH,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_CW_COEFF,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_SLIM_LIMIT,
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_CW_COEFF,
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_SLIM_LIMIT,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_SF.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_POS_RED.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_SF.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_RED.ToArray());

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.RIVER_WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterLevelUpstream,
                                                    1.0));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RiverWeirWaterLevelDownTest()
        {
            channel.AddRiverWeir(
                StructureFileWriterTestHelper.RIVER_WEIR_ID,
                StructureFileWriterTestHelper.RIVER_WEIR_NAME,
                chainage,
                0.5, //StructureFileWriterTestHelper.RIVER_WEIR_CREST_LEVEL,
                StructureFileWriterTestHelper.RIVER_WEIR_CREST_WIDTH,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_CW_COEFF,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_SLIM_LIMIT,
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_CW_COEFF,
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_SLIM_LIMIT,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_SF.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_POS_RED.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_SF.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_RED.ToArray());

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.RIVER_WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterLevelDownstream,
                                                    0.00000000));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RiverWeirHeadDifferenceTest()
        {
            channel.AddRiverWeir(
                StructureFileWriterTestHelper.RIVER_WEIR_ID,
                StructureFileWriterTestHelper.RIVER_WEIR_NAME,
                chainage,
                0.5, //StructureFileWriterTestHelper.RIVER_WEIR_CREST_LEVEL,
                StructureFileWriterTestHelper.RIVER_WEIR_CREST_WIDTH,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_CW_COEFF,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_SLIM_LIMIT,
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_CW_COEFF,
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_SLIM_LIMIT,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_SF.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_POS_RED.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_SF.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_RED.ToArray());

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.RIVER_WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterHead,
                                                    1.00000000));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RiverWeirWaterLevelAtCrestTest()
        {
            channel.AddRiverWeir(
                StructureFileWriterTestHelper.RIVER_WEIR_ID,
                StructureFileWriterTestHelper.RIVER_WEIR_NAME,
                chainage,
                0.5, //StructureFileWriterTestHelper.RIVER_WEIR_CREST_LEVEL,
                StructureFileWriterTestHelper.RIVER_WEIR_CREST_WIDTH,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_CW_COEFF,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_SLIM_LIMIT,
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_CW_COEFF,
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_SLIM_LIMIT,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_SF.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_POS_RED.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_SF.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_RED.ToArray());

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.RIVER_WEIR_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterLevelAtCrest,
                                                    1.00000000));

        }

        #endregion
        
        #region Orifice

        [Test]
        [Category(TestCategory.Integration)]
        public void OrificeDischargeTest()
        {
            channel.AddOrifice(
                StructureFileWriterTestHelper.ORIFICE_ID,
                StructureFileWriterTestHelper.ORIFICE_NAME,
                chainage,
                StructureFileWriterTestHelper.ORIFICE_FLOW_DIRECTION,
                0.5,
                StructureFileWriterTestHelper.ORIFICE_CREST_WIDTH,
                StructureFileWriterTestHelper.ORIFICE_GATE_OPENING,
                StructureFileWriterTestHelper.ORIFICE_CONTRACTION_COEFF,
                StructureFileWriterTestHelper.ORIFICE_LAT_CONTRACTION_COEFF,
                StructureFileWriterTestHelper.ORIFICE_USE_LIMIT_FLOW_POS,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_POS,
                false,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_NEG);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs, 
                                                    StructureFileWriterTestHelper.ORIFICE_ID.ToString(), 
                                                    FunctionAttributes.StandardNames.WaterDischarge,
                                                    3.014));

        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void OrificeVelocityTest()
        {
            channel.AddOrifice(
                StructureFileWriterTestHelper.ORIFICE_ID,
                StructureFileWriterTestHelper.ORIFICE_NAME,
                chainage,
                StructureFileWriterTestHelper.ORIFICE_FLOW_DIRECTION,
                0.5,
                StructureFileWriterTestHelper.ORIFICE_CREST_WIDTH,
                StructureFileWriterTestHelper.ORIFICE_GATE_OPENING,
                StructureFileWriterTestHelper.ORIFICE_CONTRACTION_COEFF,
                StructureFileWriterTestHelper.ORIFICE_LAT_CONTRACTION_COEFF,
                StructureFileWriterTestHelper.ORIFICE_USE_LIMIT_FLOW_POS,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_POS,
                false,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_NEG);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs, 
                                                    StructureFileWriterTestHelper.ORIFICE_ID.ToString(), 
                                                    FunctionAttributes.StandardNames.WaterVelocity,
                                                    1.808));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void OrificeFlowAreaTest()
        {
            channel.AddOrifice(
                StructureFileWriterTestHelper.ORIFICE_ID,
                StructureFileWriterTestHelper.ORIFICE_NAME,
                chainage,
                StructureFileWriterTestHelper.ORIFICE_FLOW_DIRECTION,
                0.5,
                StructureFileWriterTestHelper.ORIFICE_CREST_WIDTH,
                StructureFileWriterTestHelper.ORIFICE_GATE_OPENING,
                StructureFileWriterTestHelper.ORIFICE_CONTRACTION_COEFF,
                StructureFileWriterTestHelper.ORIFICE_LAT_CONTRACTION_COEFF,
                StructureFileWriterTestHelper.ORIFICE_USE_LIMIT_FLOW_POS,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_POS,
                false,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_NEG);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs, 
                                                    StructureFileWriterTestHelper.ORIFICE_ID.ToString(), 
                                                    FunctionAttributes.StandardNames.WaterFlowArea,
                                                    1.667));

        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void OrificePressureDifferenceTest()
        {
            channel.AddOrifice(
                StructureFileWriterTestHelper.ORIFICE_ID,
                StructureFileWriterTestHelper.ORIFICE_NAME,
                chainage,
                StructureFileWriterTestHelper.ORIFICE_FLOW_DIRECTION,
                0.5,
                StructureFileWriterTestHelper.ORIFICE_CREST_WIDTH,
                StructureFileWriterTestHelper.ORIFICE_GATE_OPENING,
                StructureFileWriterTestHelper.ORIFICE_CONTRACTION_COEFF,
                StructureFileWriterTestHelper.ORIFICE_LAT_CONTRACTION_COEFF,
                StructureFileWriterTestHelper.ORIFICE_USE_LIMIT_FLOW_POS,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_POS,
                false,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_NEG);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs, 
                                                    StructureFileWriterTestHelper.ORIFICE_ID.ToString(), 
                                                    FunctionAttributes.StandardNames.StructurePressureDifference,
                                                    1226.250));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void OrificeCrestLevelTest()
        {
            channel.AddOrifice(
                StructureFileWriterTestHelper.ORIFICE_ID,
                StructureFileWriterTestHelper.ORIFICE_NAME,
                chainage,
                StructureFileWriterTestHelper.ORIFICE_FLOW_DIRECTION,
                StructureFileWriterTestHelper.ORIFICE_CREST_LEVEL,
                StructureFileWriterTestHelper.ORIFICE_CREST_WIDTH,
                StructureFileWriterTestHelper.ORIFICE_GATE_OPENING,
                StructureFileWriterTestHelper.ORIFICE_CONTRACTION_COEFF,
                StructureFileWriterTestHelper.ORIFICE_LAT_CONTRACTION_COEFF,
                StructureFileWriterTestHelper.ORIFICE_USE_LIMIT_FLOW_POS,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_POS,
                false,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_NEG);

            double checkValue = StructureFileWriterTestHelper.ORIFICE_CREST_LEVEL - 1.0;
            Assert.IsTrue(CheckGetSetGetAfterModelInitialization(WaterFlowParametersCategories.Weirs,
                                                                    StructureFileWriterTestHelper.ORIFICE_ID.ToString(),
                                                                    FunctionAttributes.StandardNames.StructureCrestLevel,
                                                                    StructureFileWriterTestHelper.ORIFICE_CREST_LEVEL,
                                                                    checkValue));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void OrificeCrestWidthTest()
        {
            channel.AddOrifice(
                StructureFileWriterTestHelper.ORIFICE_ID,
                StructureFileWriterTestHelper.ORIFICE_NAME,
                chainage,
                StructureFileWriterTestHelper.ORIFICE_FLOW_DIRECTION,
                StructureFileWriterTestHelper.ORIFICE_CREST_LEVEL,
                StructureFileWriterTestHelper.ORIFICE_CREST_WIDTH,
                StructureFileWriterTestHelper.ORIFICE_GATE_OPENING,
                StructureFileWriterTestHelper.ORIFICE_CONTRACTION_COEFF,
                StructureFileWriterTestHelper.ORIFICE_LAT_CONTRACTION_COEFF,
                StructureFileWriterTestHelper.ORIFICE_USE_LIMIT_FLOW_POS,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_POS,
                false,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_NEG);

            double checkValue = StructureFileWriterTestHelper.ORIFICE_CREST_WIDTH - 1.0;
            Assert.IsTrue(CheckGetSetGetAfterModelInitialization(WaterFlowParametersCategories.Weirs,
                                                                    StructureFileWriterTestHelper.ORIFICE_ID.ToString(),
                                                                    FunctionAttributes.StandardNames.StructureCrestWidth,
                                                                    StructureFileWriterTestHelper.ORIFICE_CREST_WIDTH,
                                                                    checkValue));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void OrificeGLETest()
        {
            channel.AddOrifice(
                StructureFileWriterTestHelper.ORIFICE_ID,
                StructureFileWriterTestHelper.ORIFICE_NAME,
                chainage,
                StructureFileWriterTestHelper.ORIFICE_FLOW_DIRECTION,
                StructureFileWriterTestHelper.ORIFICE_CREST_LEVEL,
                StructureFileWriterTestHelper.ORIFICE_CREST_WIDTH,
                StructureFileWriterTestHelper.ORIFICE_GATE_OPENING,
                StructureFileWriterTestHelper.ORIFICE_CONTRACTION_COEFF,
                StructureFileWriterTestHelper.ORIFICE_LAT_CONTRACTION_COEFF,
                StructureFileWriterTestHelper.ORIFICE_USE_LIMIT_FLOW_POS,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_POS,
                false,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_NEG);

            const double checkValue = StructureFileWriterTestHelper.ORIFICE_CREST_WIDTH + StructureFileWriterTestHelper.ORIFICE_GATE_OPENING - 1.0;
            Assert.IsTrue(CheckGetSetGetAfterModelInitialization(WaterFlowParametersCategories.Weirs,
                                                                    StructureFileWriterTestHelper.ORIFICE_ID.ToString(),
                                                                    FunctionAttributes.StandardNames.StructureGateLowerEdgeLevel,
                                                                    StructureFileWriterTestHelper.ORIFICE_CREST_LEVEL + StructureFileWriterTestHelper.ORIFICE_GATE_OPENING,
                                                                    checkValue));
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void OrificeOpenHTest()
        {
            channel.AddOrifice(
                StructureFileWriterTestHelper.ORIFICE_ID,
                StructureFileWriterTestHelper.ORIFICE_NAME,
                chainage,
                StructureFileWriterTestHelper.ORIFICE_FLOW_DIRECTION,
                StructureFileWriterTestHelper.ORIFICE_CREST_LEVEL,
                StructureFileWriterTestHelper.ORIFICE_CREST_WIDTH,
                StructureFileWriterTestHelper.ORIFICE_GATE_OPENING,
                StructureFileWriterTestHelper.ORIFICE_CONTRACTION_COEFF,
                StructureFileWriterTestHelper.ORIFICE_LAT_CONTRACTION_COEFF,
                StructureFileWriterTestHelper.ORIFICE_USE_LIMIT_FLOW_POS,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_POS,
                false,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_NEG);

            double checkValue =  StructureFileWriterTestHelper.ORIFICE_GATE_OPENING - 1.0;
            Assert.IsTrue(CheckGetSetGetAfterModelInitialization(WaterFlowParametersCategories.Weirs,
                                                                    StructureFileWriterTestHelper.ORIFICE_ID.ToString(),
                                                                    FunctionAttributes.StandardNames.StructureGateOpeningHeight,
                                                                    StructureFileWriterTestHelper.ORIFICE_GATE_OPENING,
                                                                    checkValue));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void OrificeWaterLevelUpTest()
        {
            channel.AddOrifice(
                StructureFileWriterTestHelper.ORIFICE_ID,
                StructureFileWriterTestHelper.ORIFICE_NAME,
                chainage,
                StructureFileWriterTestHelper.ORIFICE_FLOW_DIRECTION,
                0.5,
                StructureFileWriterTestHelper.ORIFICE_CREST_WIDTH,
                StructureFileWriterTestHelper.ORIFICE_GATE_OPENING,
                StructureFileWriterTestHelper.ORIFICE_CONTRACTION_COEFF,
                StructureFileWriterTestHelper.ORIFICE_LAT_CONTRACTION_COEFF,
                StructureFileWriterTestHelper.ORIFICE_USE_LIMIT_FLOW_POS,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_POS,
                false,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_NEG);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.ORIFICE_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterLevelUpstream,
                                                    1.00));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void OrificeWaterLevelDownTest()
        {
            channel.AddOrifice(
                StructureFileWriterTestHelper.ORIFICE_ID,
                StructureFileWriterTestHelper.ORIFICE_NAME,
                chainage,
                StructureFileWriterTestHelper.ORIFICE_FLOW_DIRECTION,
                0.5,
                StructureFileWriterTestHelper.ORIFICE_CREST_WIDTH,
                StructureFileWriterTestHelper.ORIFICE_GATE_OPENING,
                StructureFileWriterTestHelper.ORIFICE_CONTRACTION_COEFF,
                StructureFileWriterTestHelper.ORIFICE_LAT_CONTRACTION_COEFF,
                StructureFileWriterTestHelper.ORIFICE_USE_LIMIT_FLOW_POS,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_POS,
                false,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_NEG);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.ORIFICE_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterLevelDownstream,
                                                    0.00));

        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void OrificeHeadDifferenceTest()
        {
            channel.AddOrifice(
                StructureFileWriterTestHelper.ORIFICE_ID,
                StructureFileWriterTestHelper.ORIFICE_NAME,
                chainage,
                StructureFileWriterTestHelper.ORIFICE_FLOW_DIRECTION,
                0.5,
                StructureFileWriterTestHelper.ORIFICE_CREST_WIDTH,
                StructureFileWriterTestHelper.ORIFICE_GATE_OPENING,
                StructureFileWriterTestHelper.ORIFICE_CONTRACTION_COEFF,
                StructureFileWriterTestHelper.ORIFICE_LAT_CONTRACTION_COEFF,
                StructureFileWriterTestHelper.ORIFICE_USE_LIMIT_FLOW_POS,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_POS,
                false,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_NEG);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.ORIFICE_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterHead,
                                                    1.00000000));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void OrificeWaterLevelAtCrestTest()
        {
            channel.AddOrifice(
                StructureFileWriterTestHelper.ORIFICE_ID,
                StructureFileWriterTestHelper.ORIFICE_NAME,
                chainage,
                StructureFileWriterTestHelper.ORIFICE_FLOW_DIRECTION,
                0.5,
                StructureFileWriterTestHelper.ORIFICE_CREST_WIDTH,
                StructureFileWriterTestHelper.ORIFICE_GATE_OPENING,
                StructureFileWriterTestHelper.ORIFICE_CONTRACTION_COEFF,
                StructureFileWriterTestHelper.ORIFICE_LAT_CONTRACTION_COEFF,
                StructureFileWriterTestHelper.ORIFICE_USE_LIMIT_FLOW_POS,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_POS,
                false,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_NEG);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.ORIFICE_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterLevelAtCrest,
                                                    1.00000000));

        }

        #endregion

        #region GeneralStructure

        [Test]
        [Category(TestCategory.Integration)]
        public void GeneralStructureDischargeTest()
        {
            channel.AddGeneralStructure(
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_ID,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_NAME,
                chainage,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_GATE_OPENING,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_EXTRA_RESISTANCE,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_W1,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_WSDL,
                6.0,  //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_CENTER,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_WSDR,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_W2,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZB1,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZBSL,
                0.5, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_CENTER,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZBSR,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZB2,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_NEG);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.GENERAL_STRUCTURE_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterDischarge,
                                                    4.34));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GeneralStructureVelocityTest()
        {
            channel.AddGeneralStructure(
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_ID,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_NAME,
                chainage,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_GATE_OPENING,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_EXTRA_RESISTANCE,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_W1,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_WSDL,
                6.0,  //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_CENTER,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_WSDR,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_W2,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZB1,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZBSL,
                0.5, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_CENTER,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZBSR,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZB2,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_NEG);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.GENERAL_STRUCTURE_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterVelocity,
                                                    2.17));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GeneralStructureFlowAreaTest()
        {
            channel.AddGeneralStructure(
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_ID,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_NAME,
                chainage,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_GATE_OPENING,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_EXTRA_RESISTANCE,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_W1,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_WSDL,
                6.0,  //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_CENTER,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_WSDR,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_W2,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZB1,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZBSL,
                0.5, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_CENTER,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZBSR,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZB2,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_NEG);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.GENERAL_STRUCTURE_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterFlowArea,
                                                    2.00));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GeneralStructurePressureDifferenceTest()
        {
            channel.AddGeneralStructure(
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_ID,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_NAME,
                chainage,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_GATE_OPENING,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_EXTRA_RESISTANCE,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_W1,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_WSDL,
                6.0,  //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_CENTER,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_WSDR,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_W2,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZB1,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZBSL,
                0.5, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_CENTER,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZBSR,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZB2,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_NEG);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.GENERAL_STRUCTURE_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructurePressureDifference,
                                                    1226.250));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GeneralStructureCrestLevelTest()
        {
            var levelCenter = 0.5;
            channel.AddGeneralStructure(
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_ID,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_NAME,
                chainage,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_GATE_OPENING,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_EXTRA_RESISTANCE,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_W1,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_WSDL,
                6.0,  //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_CENTER,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_WSDR,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_W2,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZB1,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZBSL,
                levelCenter, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_CENTER,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZBSR,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZB2,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_NEG);

            double checkValue = levelCenter - 1.0;
            Assert.IsTrue(CheckGetSetGetAfterModelInitialization(WaterFlowParametersCategories.Weirs,
                                                                    StructureFileWriterTestHelper.GENERAL_STRUCTURE_ID.ToString(),
                                                                    FunctionAttributes.StandardNames.StructureCrestLevel,
                                                                    levelCenter,
                                                                    checkValue));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GeneralStructureCrestWidthTest()
        {
            var widthCenter = 6.0;
            channel.AddGeneralStructure(
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_ID,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_NAME,
                chainage,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_GATE_OPENING,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_EXTRA_RESISTANCE,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_W1,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_WSDL,
                widthCenter,  //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_CENTER,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_WSDR,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_W2,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZB1,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZBSL,
                0.5, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_CENTER,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZBSR,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZB2,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_NEG);

            double checkValue = widthCenter - 1.0;
            Assert.IsTrue(CheckGetSetGetAfterModelInitialization(WaterFlowParametersCategories.Weirs,
                                                                    StructureFileWriterTestHelper.GENERAL_STRUCTURE_ID.ToString(),
                                                                    FunctionAttributes.StandardNames.StructureCrestWidth,
                                                                    widthCenter,
                                                                    checkValue));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GeneralStructureGLETest()
        {
            var levelCenter = 0.5;
            channel.AddGeneralStructure(
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_ID,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_NAME,
                chainage,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_GATE_OPENING,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_EXTRA_RESISTANCE,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_W1,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_WSDL,
                6.0,  //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_CENTER,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_WSDR,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_W2,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZB1,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZBSL,
                levelCenter, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_CENTER,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZBSR,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZB2,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_NEG);

            var checkValue = levelCenter + StructureFileWriterTestHelper.GENERAL_STRUCTURE_GATE_OPENING - 1.0;
            Assert.IsTrue(CheckGetSetGetAfterModelInitialization(WaterFlowParametersCategories.Weirs,
                                                                    StructureFileWriterTestHelper.GENERAL_STRUCTURE_ID.ToString(),
                                                                    FunctionAttributes.StandardNames.StructureGateLowerEdgeLevel,
                                                                    levelCenter + StructureFileWriterTestHelper.GENERAL_STRUCTURE_GATE_OPENING,
                                                                    checkValue));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GeneralStructureOpenHTest()
        {
            channel.AddGeneralStructure(
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_ID,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_NAME,
                chainage,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_GATE_OPENING,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_EXTRA_RESISTANCE,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_W1,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_WSDL,
                6.0,  //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_CENTER,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_WSDR,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_W2,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZB1,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZBSL,
                0.5, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_CENTER,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZBSR,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZB2,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_NEG);

            double checkValue = StructureFileWriterTestHelper.GENERAL_STRUCTURE_GATE_OPENING - 1.0;
            Assert.IsTrue(CheckGetSetGetAfterModelInitialization(WaterFlowParametersCategories.Weirs,
                                                                    StructureFileWriterTestHelper.GENERAL_STRUCTURE_ID.ToString(),
                                                                    FunctionAttributes.StandardNames.StructureGateOpeningHeight,
                                                                    StructureFileWriterTestHelper.GENERAL_STRUCTURE_GATE_OPENING,
                                                                    checkValue));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GeneralStructureWaterLevelUpTest()
        {
            channel.AddGeneralStructure(
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_ID,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_NAME,
                chainage,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_GATE_OPENING,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_EXTRA_RESISTANCE,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_W1,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_WSDL,
                6.0,  //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_CENTER,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_WSDR,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_W2,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZB1,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZBSL,
                0.5, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_CENTER,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZBSR,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZB2,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_NEG);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.GENERAL_STRUCTURE_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterLevelUpstream,
                                                    1.00));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GeneralStructureWaterLevelDownTest()
        {
            channel.AddGeneralStructure(
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_ID,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_NAME,
                chainage,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_GATE_OPENING,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_EXTRA_RESISTANCE,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_W1,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_WSDL,
                6.0,  //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_CENTER,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_WSDR,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_W2,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZB1,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZBSL,
                0.5, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_CENTER,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZBSR,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZB2,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_NEG);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.GENERAL_STRUCTURE_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterLevelDownstream,
                                                    0.00));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GeneralStructureHeadDifferenceTest()
        {
            channel.AddGeneralStructure(
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_ID,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_NAME,
                chainage,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_GATE_OPENING,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_EXTRA_RESISTANCE,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_W1,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_WSDL,
                6.0,  //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_CENTER,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_WSDR,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_W2,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZB1,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZBSL,
                0.5, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_CENTER,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZBSR,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZB2,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_NEG);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.GENERAL_STRUCTURE_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterHead,
                                                    1.000));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GeneralStructureWaterLevelAtCrestTest()
        {
            channel.AddGeneralStructure(
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_ID,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_NAME,
                chainage,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_GATE_OPENING,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_EXTRA_RESISTANCE,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_W1,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_WSDL,
                6.0,  //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_CENTER,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_WSDR,
                10.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_W2,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZB1,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZBSL,
                0.5, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_CENTER,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZBSR,
                0.0, //StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZB2,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_NEG);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Weirs,
                                                    StructureFileWriterTestHelper.GENERAL_STRUCTURE_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterLevelAtCrest,
                                                    0.833));

        }

        #endregion

        #region Culvert

        [Test]
        [Category(TestCategory.Integration)]
        public void CulvertDischargeTest()
        {
            channel.AddCulvert(
                StructureFileWriterTestHelper.CULVERT_ID,
                StructureFileWriterTestHelper.CULVERT_NAME,
                chainage,
                StructureFileWriterTestHelper.CULVERT_FLOW_DIRECTION,
                -2.0, //StructureFileWriterTestHelper.CULVERT_INLET_LEVEL,
                -2.0, //StructureFileWriterTestHelper.CULVERT_OUTLET_LEVEL,
                100.0, //StructureFileWriterTestHelper.CULVERT_LENGTH,
                1.0, //StructureFileWriterTestHelper.CULVERT_INLET_LOSS_COEFF,
                1.0, //StructureFileWriterTestHelper.CULVERT_OUTLET_LOSS_COEFF,
                StructureFileWriterTestHelper.CULVERT_IS_GATED,
                StructureFileWriterTestHelper.CULVERT_GATE_INITIAL_OPENING,
                StructureFileWriterTestHelper.CULVERT_REL_OPENING.ToArray(),
                StructureFileWriterTestHelper.CULVERT_LOSS_COEFF.ToArray(),
                20.0, //StructureFileWriterTestHelper.CULVERT_FRICTION,
                StructureFileWriterTestHelper.CULVERT_GROUNDLAYER_ENABLED,
                StructureFileWriterTestHelper.CULVERT_GROUNDLAYER_ROUGHNESS);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Culverts,
                                                    StructureFileWriterTestHelper.CULVERT_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterDischarge,
                                                    4.147));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CulvertVelocityTest()
        {
            channel.AddCulvert(
                StructureFileWriterTestHelper.CULVERT_ID,
                StructureFileWriterTestHelper.CULVERT_NAME,
                chainage,
                StructureFileWriterTestHelper.CULVERT_FLOW_DIRECTION,
                -2.0, //StructureFileWriterTestHelper.CULVERT_INLET_LEVEL,
                -2.0, //StructureFileWriterTestHelper.CULVERT_OUTLET_LEVEL,
                100.0, //StructureFileWriterTestHelper.CULVERT_LENGTH,
                1.0, //StructureFileWriterTestHelper.CULVERT_INLET_LOSS_COEFF,
                1.0, //StructureFileWriterTestHelper.CULVERT_OUTLET_LOSS_COEFF,
                StructureFileWriterTestHelper.CULVERT_IS_GATED,
                StructureFileWriterTestHelper.CULVERT_GATE_INITIAL_OPENING,
                StructureFileWriterTestHelper.CULVERT_REL_OPENING.ToArray(),
                StructureFileWriterTestHelper.CULVERT_LOSS_COEFF.ToArray(),
                20.0, //StructureFileWriterTestHelper.CULVERT_FRICTION,
                StructureFileWriterTestHelper.CULVERT_GROUNDLAYER_ENABLED,
                StructureFileWriterTestHelper.CULVERT_GROUNDLAYER_ROUGHNESS);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Culverts,
                                                    StructureFileWriterTestHelper.CULVERT_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterVelocity,
                                                    1.326));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CulvertFlowAreaTest()
        {
            channel.AddCulvert(
                StructureFileWriterTestHelper.CULVERT_ID,
                StructureFileWriterTestHelper.CULVERT_NAME,
                chainage,
                StructureFileWriterTestHelper.CULVERT_FLOW_DIRECTION,
                -2.0, //StructureFileWriterTestHelper.CULVERT_INLET_LEVEL,
                -2.0, //StructureFileWriterTestHelper.CULVERT_OUTLET_LEVEL,
                100.0, //StructureFileWriterTestHelper.CULVERT_LENGTH,
                1.0, //StructureFileWriterTestHelper.CULVERT_INLET_LOSS_COEFF,
                1.0, //StructureFileWriterTestHelper.CULVERT_OUTLET_LOSS_COEFF,
                StructureFileWriterTestHelper.CULVERT_IS_GATED,
                StructureFileWriterTestHelper.CULVERT_GATE_INITIAL_OPENING,
                StructureFileWriterTestHelper.CULVERT_REL_OPENING.ToArray(),
                StructureFileWriterTestHelper.CULVERT_LOSS_COEFF.ToArray(),
                20.0, //StructureFileWriterTestHelper.CULVERT_FRICTION,
                StructureFileWriterTestHelper.CULVERT_GROUNDLAYER_ENABLED,
                StructureFileWriterTestHelper.CULVERT_GROUNDLAYER_ROUGHNESS);


            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Culverts,
                                                    StructureFileWriterTestHelper.CULVERT_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterFlowArea,
                                                    3.129));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CulvertValveOpeningTest()
        {
            channel.AddCulvert(
                StructureFileWriterTestHelper.CULVERT_ID,
                StructureFileWriterTestHelper.CULVERT_NAME,
                chainage,
                StructureFileWriterTestHelper.CULVERT_FLOW_DIRECTION,
                -2.0, //StructureFileWriterTestHelper.CULVERT_INLET_LEVEL,
                -2.0, //StructureFileWriterTestHelper.CULVERT_OUTLET_LEVEL,
                100.0, //StructureFileWriterTestHelper.CULVERT_LENGTH,
                1.0, //StructureFileWriterTestHelper.CULVERT_INLET_LOSS_COEFF,
                1.0, //StructureFileWriterTestHelper.CULVERT_OUTLET_LOSS_COEFF,
                StructureFileWriterTestHelper.CULVERT_IS_GATED,
                StructureFileWriterTestHelper.CULVERT_GATE_INITIAL_OPENING,
                StructureFileWriterTestHelper.CULVERT_REL_OPENING.ToArray(),
                StructureFileWriterTestHelper.CULVERT_LOSS_COEFF.ToArray(),
                20.0, //StructureFileWriterTestHelper.CULVERT_FRICTION,
                StructureFileWriterTestHelper.CULVERT_GROUNDLAYER_ENABLED,
                StructureFileWriterTestHelper.CULVERT_GROUNDLAYER_ROUGHNESS);


            double checkValue = StructureFileWriterTestHelper.CULVERT_GATE_INITIAL_OPENING +0.5 ;
            Assert.IsTrue(CheckGetSetGetAfterModelInitialization(WaterFlowParametersCategories.Culverts,
                                                                    StructureFileWriterTestHelper.CULVERT_ID.ToString(),
                                                                    FunctionAttributes.StandardNames.StructureValveOpening,
                                                                    StructureFileWriterTestHelper.CULVERT_GATE_INITIAL_OPENING,
                                                                    checkValue));
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void CulvertWaterLevelUpTest()
        {
            channel.AddCulvert(
                StructureFileWriterTestHelper.CULVERT_ID,
                StructureFileWriterTestHelper.CULVERT_NAME,
                chainage,
                StructureFileWriterTestHelper.CULVERT_FLOW_DIRECTION,
                -2.0, //StructureFileWriterTestHelper.CULVERT_INLET_LEVEL,
                -2.0, //StructureFileWriterTestHelper.CULVERT_OUTLET_LEVEL,
                100.0, //StructureFileWriterTestHelper.CULVERT_LENGTH,
                1.0, //StructureFileWriterTestHelper.CULVERT_INLET_LOSS_COEFF,
                1.0, //StructureFileWriterTestHelper.CULVERT_OUTLET_LOSS_COEFF,
                StructureFileWriterTestHelper.CULVERT_IS_GATED,
                StructureFileWriterTestHelper.CULVERT_GATE_INITIAL_OPENING,
                StructureFileWriterTestHelper.CULVERT_REL_OPENING.ToArray(),
                StructureFileWriterTestHelper.CULVERT_LOSS_COEFF.ToArray(),
                20.0, //StructureFileWriterTestHelper.CULVERT_FRICTION,
                StructureFileWriterTestHelper.CULVERT_GROUNDLAYER_ENABLED,
                StructureFileWriterTestHelper.CULVERT_GROUNDLAYER_ROUGHNESS);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Culverts,
                                                    StructureFileWriterTestHelper.CULVERT_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterLevelUpstream,
                                                    1.00));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CulvertWaterLevelDownTest()
        {
            channel.AddCulvert(
                StructureFileWriterTestHelper.CULVERT_ID,
                StructureFileWriterTestHelper.CULVERT_NAME,
                chainage,
                StructureFileWriterTestHelper.CULVERT_FLOW_DIRECTION,
                -2.0, //StructureFileWriterTestHelper.CULVERT_INLET_LEVEL,
                -2.0, //StructureFileWriterTestHelper.CULVERT_OUTLET_LEVEL,
                100.0, //StructureFileWriterTestHelper.CULVERT_LENGTH,
                1.0, //StructureFileWriterTestHelper.CULVERT_INLET_LOSS_COEFF,
                1.0, //StructureFileWriterTestHelper.CULVERT_OUTLET_LOSS_COEFF,
                StructureFileWriterTestHelper.CULVERT_IS_GATED,
                StructureFileWriterTestHelper.CULVERT_GATE_INITIAL_OPENING,
                StructureFileWriterTestHelper.CULVERT_REL_OPENING.ToArray(),
                StructureFileWriterTestHelper.CULVERT_LOSS_COEFF.ToArray(),
                20.0, //StructureFileWriterTestHelper.CULVERT_FRICTION,
                StructureFileWriterTestHelper.CULVERT_GROUNDLAYER_ENABLED,
                StructureFileWriterTestHelper.CULVERT_GROUNDLAYER_ROUGHNESS);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Culverts,
                                                    StructureFileWriterTestHelper.CULVERT_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterLevelDownstream,
                                                    0.00000000));

        }

        #endregion

        #region Siphon

        [Test]
        [Category(TestCategory.Integration)]
        public void SiphonDischargeTest()
        {
            channel.AddSiphon(
                StructureFileWriterTestHelper.SIPHON_ID,
                StructureFileWriterTestHelper.SIPHON_NAME,
                chainage,
                StructureFileWriterTestHelper.SIPHON_FLOW_DIRECTION,
                -2.0, //StructureFileWriterTestHelper.SIPHON_INLET_LEVEL,
                -2.0, //StructureFileWriterTestHelper.SIPHON_OUTLET_LEVEL,
                StructureFileWriterTestHelper.SIPHON_LENGTH,
                StructureFileWriterTestHelper.SIPHON_INLET_LOSS_COEFF,
                StructureFileWriterTestHelper.SIPHON_OUTLET_LOSS_COEFF,
                StructureFileWriterTestHelper.SIPHON_IS_GATED,
                StructureFileWriterTestHelper.SIPHON_GATE_INITIAL_OPENING,
                StructureFileWriterTestHelper.SIPHON_REL_OPENING.ToArray(),
                StructureFileWriterTestHelper.SIPHON_LOSS_COEFF.ToArray(),
                StructureFileWriterTestHelper.SIPHON_FRICTION,
                StructureFileWriterTestHelper.SIPHON_GROUNDLAYER_ENABLED,
                StructureFileWriterTestHelper.SIPHON_GROUNDLAYER_ROUGHNESS,
                StructureFileWriterTestHelper.SIPHON_BEND_LOSS_COEFF,
                StructureFileWriterTestHelper.SIPHON_TURNON_LEVEL,
                StructureFileWriterTestHelper.SIPHON_TURNOFF_LEVEL);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Culverts,
                                                    StructureFileWriterTestHelper.SIPHON_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterDischarge,
                                                    0.3835));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void SiphonVelocityTest()
        {
            channel.AddSiphon(
                StructureFileWriterTestHelper.SIPHON_ID,
                StructureFileWriterTestHelper.SIPHON_NAME,
                chainage,
                StructureFileWriterTestHelper.SIPHON_FLOW_DIRECTION,
                -2.0, //StructureFileWriterTestHelper.SIPHON_INLET_LEVEL,
                -2.0, //StructureFileWriterTestHelper.SIPHON_OUTLET_LEVEL,
                StructureFileWriterTestHelper.SIPHON_LENGTH,
                StructureFileWriterTestHelper.SIPHON_INLET_LOSS_COEFF,
                StructureFileWriterTestHelper.SIPHON_OUTLET_LOSS_COEFF,
                StructureFileWriterTestHelper.SIPHON_IS_GATED,
                StructureFileWriterTestHelper.SIPHON_GATE_INITIAL_OPENING,
                StructureFileWriterTestHelper.SIPHON_REL_OPENING.ToArray(),
                StructureFileWriterTestHelper.SIPHON_LOSS_COEFF.ToArray(),
                StructureFileWriterTestHelper.SIPHON_FRICTION,
                StructureFileWriterTestHelper.SIPHON_GROUNDLAYER_ENABLED,
                StructureFileWriterTestHelper.SIPHON_GROUNDLAYER_ROUGHNESS,
                StructureFileWriterTestHelper.SIPHON_BEND_LOSS_COEFF,
                StructureFileWriterTestHelper.SIPHON_TURNON_LEVEL,
                StructureFileWriterTestHelper.SIPHON_TURNOFF_LEVEL);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Culverts,
                                                    StructureFileWriterTestHelper.SIPHON_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterVelocity,
                                                    0.1226));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void SiphonFlowAreaTest()
        {
            channel.AddSiphon(
                StructureFileWriterTestHelper.SIPHON_ID,
                StructureFileWriterTestHelper.SIPHON_NAME,
                chainage,
                StructureFileWriterTestHelper.SIPHON_FLOW_DIRECTION,
                -2.0, //StructureFileWriterTestHelper.SIPHON_INLET_LEVEL,
                -2.0, //StructureFileWriterTestHelper.SIPHON_OUTLET_LEVEL,
                StructureFileWriterTestHelper.SIPHON_LENGTH,
                StructureFileWriterTestHelper.SIPHON_INLET_LOSS_COEFF,
                StructureFileWriterTestHelper.SIPHON_OUTLET_LOSS_COEFF,
                StructureFileWriterTestHelper.SIPHON_IS_GATED,
                StructureFileWriterTestHelper.SIPHON_GATE_INITIAL_OPENING,
                StructureFileWriterTestHelper.SIPHON_REL_OPENING.ToArray(),
                StructureFileWriterTestHelper.SIPHON_LOSS_COEFF.ToArray(),
                StructureFileWriterTestHelper.SIPHON_FRICTION,
                StructureFileWriterTestHelper.SIPHON_GROUNDLAYER_ENABLED,
                StructureFileWriterTestHelper.SIPHON_GROUNDLAYER_ROUGHNESS,
                StructureFileWriterTestHelper.SIPHON_BEND_LOSS_COEFF,
                StructureFileWriterTestHelper.SIPHON_TURNON_LEVEL,
                StructureFileWriterTestHelper.SIPHON_TURNOFF_LEVEL);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Culverts,
                                                    StructureFileWriterTestHelper.SIPHON_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterFlowArea,
                                                    3.1287));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void SiphonValveOpeningTest()
        {
            channel.AddSiphon(
                StructureFileWriterTestHelper.SIPHON_ID,
                StructureFileWriterTestHelper.SIPHON_NAME,
                chainage,
                StructureFileWriterTestHelper.SIPHON_FLOW_DIRECTION,
                -2.0, //StructureFileWriterTestHelper.SIPHON_INLET_LEVEL,
                -2.0, //StructureFileWriterTestHelper.SIPHON_OUTLET_LEVEL,
                StructureFileWriterTestHelper.SIPHON_LENGTH,
                StructureFileWriterTestHelper.SIPHON_INLET_LOSS_COEFF,
                StructureFileWriterTestHelper.SIPHON_OUTLET_LOSS_COEFF,
                StructureFileWriterTestHelper.SIPHON_IS_GATED,
                StructureFileWriterTestHelper.SIPHON_GATE_INITIAL_OPENING,
                StructureFileWriterTestHelper.SIPHON_REL_OPENING.ToArray(),
                StructureFileWriterTestHelper.SIPHON_LOSS_COEFF.ToArray(),
                StructureFileWriterTestHelper.SIPHON_FRICTION,
                StructureFileWriterTestHelper.SIPHON_GROUNDLAYER_ENABLED,
                StructureFileWriterTestHelper.SIPHON_GROUNDLAYER_ROUGHNESS,
                StructureFileWriterTestHelper.SIPHON_BEND_LOSS_COEFF,
                StructureFileWriterTestHelper.SIPHON_TURNON_LEVEL,
                StructureFileWriterTestHelper.SIPHON_TURNOFF_LEVEL);

            double checkValue = StructureFileWriterTestHelper.SIPHON_GATE_INITIAL_OPENING - 1.0;
            Assert.IsTrue(CheckGetSetGetAfterModelInitialization(WaterFlowParametersCategories.Culverts,
                                                                    StructureFileWriterTestHelper.SIPHON_ID.ToString(),
                                                                    FunctionAttributes.StandardNames.StructureValveOpening,
                                                                    StructureFileWriterTestHelper.SIPHON_GATE_INITIAL_OPENING,
                                                                    checkValue));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void SiphonWaterLevelUpTest()
        {
            channel.AddSiphon(
                StructureFileWriterTestHelper.SIPHON_ID,
                StructureFileWriterTestHelper.SIPHON_NAME,
                chainage,
                StructureFileWriterTestHelper.SIPHON_FLOW_DIRECTION,
                -2.0, //StructureFileWriterTestHelper.SIPHON_INLET_LEVEL,
                -2.0, //StructureFileWriterTestHelper.SIPHON_OUTLET_LEVEL,
                StructureFileWriterTestHelper.SIPHON_LENGTH,
                StructureFileWriterTestHelper.SIPHON_INLET_LOSS_COEFF,
                StructureFileWriterTestHelper.SIPHON_OUTLET_LOSS_COEFF,
                StructureFileWriterTestHelper.SIPHON_IS_GATED,
                StructureFileWriterTestHelper.SIPHON_GATE_INITIAL_OPENING,
                StructureFileWriterTestHelper.SIPHON_REL_OPENING.ToArray(),
                StructureFileWriterTestHelper.SIPHON_LOSS_COEFF.ToArray(),
                StructureFileWriterTestHelper.SIPHON_FRICTION,
                StructureFileWriterTestHelper.SIPHON_GROUNDLAYER_ENABLED,
                StructureFileWriterTestHelper.SIPHON_GROUNDLAYER_ROUGHNESS,
                StructureFileWriterTestHelper.SIPHON_BEND_LOSS_COEFF,
                StructureFileWriterTestHelper.SIPHON_TURNON_LEVEL,
                StructureFileWriterTestHelper.SIPHON_TURNOFF_LEVEL);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Culverts,
                                                    StructureFileWriterTestHelper.SIPHON_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterLevelUpstream,
                                                    1.00));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void SiphonWaterLevelDownTest()
        {
            channel.AddSiphon(
                StructureFileWriterTestHelper.SIPHON_ID,
                StructureFileWriterTestHelper.SIPHON_NAME,
                chainage,
                StructureFileWriterTestHelper.SIPHON_FLOW_DIRECTION,
                -2.0, //StructureFileWriterTestHelper.SIPHON_INLET_LEVEL,
                -2.0, //StructureFileWriterTestHelper.SIPHON_OUTLET_LEVEL,
                StructureFileWriterTestHelper.SIPHON_LENGTH,
                StructureFileWriterTestHelper.SIPHON_INLET_LOSS_COEFF,
                StructureFileWriterTestHelper.SIPHON_OUTLET_LOSS_COEFF,
                StructureFileWriterTestHelper.SIPHON_IS_GATED,
                StructureFileWriterTestHelper.SIPHON_GATE_INITIAL_OPENING,
                StructureFileWriterTestHelper.SIPHON_REL_OPENING.ToArray(),
                StructureFileWriterTestHelper.SIPHON_LOSS_COEFF.ToArray(),
                StructureFileWriterTestHelper.SIPHON_FRICTION,
                StructureFileWriterTestHelper.SIPHON_GROUNDLAYER_ENABLED,
                StructureFileWriterTestHelper.SIPHON_GROUNDLAYER_ROUGHNESS,
                StructureFileWriterTestHelper.SIPHON_BEND_LOSS_COEFF,
                StructureFileWriterTestHelper.SIPHON_TURNON_LEVEL,
                StructureFileWriterTestHelper.SIPHON_TURNOFF_LEVEL);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Culverts,
                                                    StructureFileWriterTestHelper.SIPHON_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterLevelDownstream,
                                                    0.00000000));

        }

        #endregion

        #region InvertedSiphon

        [Test]
        [Category(TestCategory.Integration)]
        public void InvertedSiphonDischargeTest()
        {
            channel.AddInvertedSiphon(
                StructureFileWriterTestHelper.INV_SIPHON_ID,
                StructureFileWriterTestHelper.INV_SIPHON_NAME,
                chainage,
                StructureFileWriterTestHelper.INV_SIPHON_FLOW_DIRECTION,
                -2.0, //StructureFileWriterTestHelper.INV_SIPHON_INLET_LEVEL,
                -2.0, //StructureFileWriterTestHelper.INV_SIPHON_OUTLET_LEVEL,
                100.0, //StructureFileWriterTestHelper.INV_SIPHON_LENGTH,
                StructureFileWriterTestHelper.INV_SIPHON_INLET_LOSS_COEFF,
                StructureFileWriterTestHelper.INV_SIPHON_OUTLET_LOSS_COEFF,
                StructureFileWriterTestHelper.INV_SIPHON_IS_GATED,
                StructureFileWriterTestHelper.INV_SIPHON_GATE_INITIAL_OPENING,
                StructureFileWriterTestHelper.INV_SIPHON_REL_OPENING.ToArray(),
                StructureFileWriterTestHelper.INV_SIPHON_LOSS_COEFF.ToArray(),
                StructureFileWriterTestHelper.INV_SIPHON_FRICTION,
                StructureFileWriterTestHelper.INV_SIPHON_GROUNDLAYER_ENABLED,
                StructureFileWriterTestHelper.INV_SIPHON_GROUNDLAYER_ROUGHNESS,
                StructureFileWriterTestHelper.INV_SIPHON_BEND_LOSS_COEFF);
            
            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Culverts,
                                                    StructureFileWriterTestHelper.INV_SIPHON_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterDischarge,
                                                    0.0773));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void InvertedSiphonVelocityTest()
        {
            channel.AddInvertedSiphon(
                StructureFileWriterTestHelper.INV_SIPHON_ID,
                StructureFileWriterTestHelper.INV_SIPHON_NAME,
                chainage,
                StructureFileWriterTestHelper.INV_SIPHON_FLOW_DIRECTION,
                -2.0, //StructureFileWriterTestHelper.INV_SIPHON_INLET_LEVEL,
                -2.0, //StructureFileWriterTestHelper.INV_SIPHON_OUTLET_LEVEL,
                100.0, //StructureFileWriterTestHelper.INV_SIPHON_LENGTH,
                StructureFileWriterTestHelper.INV_SIPHON_INLET_LOSS_COEFF,
                StructureFileWriterTestHelper.INV_SIPHON_OUTLET_LOSS_COEFF,
                StructureFileWriterTestHelper.INV_SIPHON_IS_GATED,
                StructureFileWriterTestHelper.INV_SIPHON_GATE_INITIAL_OPENING,
                StructureFileWriterTestHelper.INV_SIPHON_REL_OPENING.ToArray(),
                StructureFileWriterTestHelper.INV_SIPHON_LOSS_COEFF.ToArray(),
                StructureFileWriterTestHelper.INV_SIPHON_FRICTION,
                StructureFileWriterTestHelper.INV_SIPHON_GROUNDLAYER_ENABLED,
                StructureFileWriterTestHelper.INV_SIPHON_GROUNDLAYER_ROUGHNESS,
                StructureFileWriterTestHelper.INV_SIPHON_BEND_LOSS_COEFF);
            
            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Culverts,
                                                    StructureFileWriterTestHelper.INV_SIPHON_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterVelocity,
                                                    0.0247));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void InvertedSiphonFlowAreaTest()
        {
            channel.AddInvertedSiphon(
                StructureFileWriterTestHelper.INV_SIPHON_ID,
                StructureFileWriterTestHelper.INV_SIPHON_NAME,
                chainage,
                StructureFileWriterTestHelper.INV_SIPHON_FLOW_DIRECTION,
                -2.0, //StructureFileWriterTestHelper.INV_SIPHON_INLET_LEVEL,
                -2.0, //StructureFileWriterTestHelper.INV_SIPHON_OUTLET_LEVEL,
                100.0, //StructureFileWriterTestHelper.INV_SIPHON_LENGTH,
                StructureFileWriterTestHelper.INV_SIPHON_INLET_LOSS_COEFF,
                StructureFileWriterTestHelper.INV_SIPHON_OUTLET_LOSS_COEFF,
                StructureFileWriterTestHelper.INV_SIPHON_IS_GATED,
                StructureFileWriterTestHelper.INV_SIPHON_GATE_INITIAL_OPENING,
                StructureFileWriterTestHelper.INV_SIPHON_REL_OPENING.ToArray(),
                StructureFileWriterTestHelper.INV_SIPHON_LOSS_COEFF.ToArray(),
                StructureFileWriterTestHelper.INV_SIPHON_FRICTION,
                StructureFileWriterTestHelper.INV_SIPHON_GROUNDLAYER_ENABLED,
                StructureFileWriterTestHelper.INV_SIPHON_GROUNDLAYER_ROUGHNESS,
                StructureFileWriterTestHelper.INV_SIPHON_BEND_LOSS_COEFF);
            
            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Culverts,
                                                    StructureFileWriterTestHelper.INV_SIPHON_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterFlowArea,
                                                    3.128));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void InvertedSiphonValveOpeningTest()
        {
            channel.AddInvertedSiphon(
                StructureFileWriterTestHelper.INV_SIPHON_ID,
                StructureFileWriterTestHelper.INV_SIPHON_NAME,
                chainage,
                StructureFileWriterTestHelper.INV_SIPHON_FLOW_DIRECTION,
                -2.0, //StructureFileWriterTestHelper.INV_SIPHON_INLET_LEVEL,
                -2.0, //StructureFileWriterTestHelper.INV_SIPHON_OUTLET_LEVEL,
                100.0, //StructureFileWriterTestHelper.INV_SIPHON_LENGTH,
                StructureFileWriterTestHelper.INV_SIPHON_INLET_LOSS_COEFF,
                StructureFileWriterTestHelper.INV_SIPHON_OUTLET_LOSS_COEFF,
                StructureFileWriterTestHelper.INV_SIPHON_IS_GATED,
                StructureFileWriterTestHelper.INV_SIPHON_GATE_INITIAL_OPENING,
                StructureFileWriterTestHelper.INV_SIPHON_REL_OPENING.ToArray(),
                StructureFileWriterTestHelper.INV_SIPHON_LOSS_COEFF.ToArray(),
                StructureFileWriterTestHelper.INV_SIPHON_FRICTION,
                StructureFileWriterTestHelper.INV_SIPHON_GROUNDLAYER_ENABLED,
                StructureFileWriterTestHelper.INV_SIPHON_GROUNDLAYER_ROUGHNESS,
                StructureFileWriterTestHelper.INV_SIPHON_BEND_LOSS_COEFF);

            double checkValue = StructureFileWriterTestHelper.INV_SIPHON_GATE_INITIAL_OPENING - 1.0;
            Assert.IsTrue(CheckGetSetGetAfterModelInitialization(WaterFlowParametersCategories.Culverts,
                                                                    StructureFileWriterTestHelper.INV_SIPHON_ID.ToString(),
                                                                    FunctionAttributes.StandardNames.StructureValveOpening,
                                                                    StructureFileWriterTestHelper.INV_SIPHON_GATE_INITIAL_OPENING,
                                                                    checkValue));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void InvertedSiphonWaterLevelUpTest()
        {
            channel.AddInvertedSiphon(
                StructureFileWriterTestHelper.INV_SIPHON_ID,
                StructureFileWriterTestHelper.INV_SIPHON_NAME,
                chainage,
                StructureFileWriterTestHelper.INV_SIPHON_FLOW_DIRECTION,
                -2.0, //StructureFileWriterTestHelper.INV_SIPHON_INLET_LEVEL,
                -2.0, //StructureFileWriterTestHelper.INV_SIPHON_OUTLET_LEVEL,
                100.0, //StructureFileWriterTestHelper.INV_SIPHON_LENGTH,
                StructureFileWriterTestHelper.INV_SIPHON_INLET_LOSS_COEFF,
                StructureFileWriterTestHelper.INV_SIPHON_OUTLET_LOSS_COEFF,
                StructureFileWriterTestHelper.INV_SIPHON_IS_GATED,
                StructureFileWriterTestHelper.INV_SIPHON_GATE_INITIAL_OPENING,
                StructureFileWriterTestHelper.INV_SIPHON_REL_OPENING.ToArray(),
                StructureFileWriterTestHelper.INV_SIPHON_LOSS_COEFF.ToArray(),
                StructureFileWriterTestHelper.INV_SIPHON_FRICTION,
                StructureFileWriterTestHelper.INV_SIPHON_GROUNDLAYER_ENABLED,
                StructureFileWriterTestHelper.INV_SIPHON_GROUNDLAYER_ROUGHNESS,
                StructureFileWriterTestHelper.INV_SIPHON_BEND_LOSS_COEFF);
            
            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Culverts,
                                                    StructureFileWriterTestHelper.INV_SIPHON_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterLevelUpstream,
                                                    1.0));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void InvertedSiphonWaterLevelDownTest()
        {
            channel.AddInvertedSiphon(
                StructureFileWriterTestHelper.INV_SIPHON_ID,
                StructureFileWriterTestHelper.INV_SIPHON_NAME,
                chainage,
                StructureFileWriterTestHelper.INV_SIPHON_FLOW_DIRECTION,
                -2.0, //StructureFileWriterTestHelper.INV_SIPHON_INLET_LEVEL,
                -2.0, //StructureFileWriterTestHelper.INV_SIPHON_OUTLET_LEVEL,
                100.0, //StructureFileWriterTestHelper.INV_SIPHON_LENGTH,
                StructureFileWriterTestHelper.INV_SIPHON_INLET_LOSS_COEFF,
                StructureFileWriterTestHelper.INV_SIPHON_OUTLET_LOSS_COEFF,
                StructureFileWriterTestHelper.INV_SIPHON_IS_GATED,
                StructureFileWriterTestHelper.INV_SIPHON_GATE_INITIAL_OPENING,
                StructureFileWriterTestHelper.INV_SIPHON_REL_OPENING.ToArray(),
                StructureFileWriterTestHelper.INV_SIPHON_LOSS_COEFF.ToArray(),
                StructureFileWriterTestHelper.INV_SIPHON_FRICTION,
                StructureFileWriterTestHelper.INV_SIPHON_GROUNDLAYER_ENABLED,
                StructureFileWriterTestHelper.INV_SIPHON_GROUNDLAYER_ROUGHNESS,
                StructureFileWriterTestHelper.INV_SIPHON_BEND_LOSS_COEFF);
            
            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Culverts,
                                                    StructureFileWriterTestHelper.INV_SIPHON_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterLevelDownstream,
                                                    0.00000000));

        }

        #endregion

        #region Pump

        public const PumpControlDirection PUMP_CONTROL_DIRECTION = PumpControlDirection.SuctionSideControl;

        [Test]
        [Category(TestCategory.Integration)]
        public void PumpDischargeTest()
        {
            var head = new List<double> { 0.0, 1.0 };
            var reduction = new List<double> { 1.0, 1.0 };

            channel.AddPump(
                StructureFileWriterTestHelper.PUMP_ID,
                StructureFileWriterTestHelper.PUMP_NAME,
                StructureFileWriterTestHelper.PUMP_CAPACITY,
                chainage,
                PUMP_CONTROL_DIRECTION,
                0.5, //StructureFileWriterTestHelper.PUMP_SUCTION_START,
                0.0, //StructureFileWriterTestHelper.PUMP_SUCTION_STOP,
                -1.0, //StructureFileWriterTestHelper.PUMP_DELIVERY_START,
                StructureFileWriterTestHelper.PUMP_DELIVERY_STOP,
                head, //StructureFileWriterTestHelper.PUMP_HEAD_VALUES,
                reduction //StructureFileWriterTestHelper.PUMP_REDUCTION_VALUES
                );

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Pumps,
                                                    StructureFileWriterTestHelper.PUMP_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterDischarge,
                                                    3.0));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void PumpSetPointTest()
        {
            channel.AddPump(
                StructureFileWriterTestHelper.PUMP_ID,
                StructureFileWriterTestHelper.PUMP_NAME,
                StructureFileWriterTestHelper.PUMP_CAPACITY,
                chainage,
                StructureFileWriterTestHelper.PUMP_CONTROL_DIRECTION,
                StructureFileWriterTestHelper.PUMP_SUCTION_START,
                StructureFileWriterTestHelper.PUMP_SUCTION_STOP,
                StructureFileWriterTestHelper.PUMP_DELIVERY_START,
                StructureFileWriterTestHelper.PUMP_DELIVERY_STOP,
                StructureFileWriterTestHelper.PUMP_HEAD_VALUES,
                StructureFileWriterTestHelper.PUMP_REDUCTION_VALUES);

            double checkValue = StructureFileWriterTestHelper.PUMP_CAPACITY - 1.0;
            Assert.IsTrue(CheckGetSetGetAfterModelInitialization(WaterFlowParametersCategories.Pumps,
                                                                    StructureFileWriterTestHelper.PUMP_ID.ToString(),
                                                                    FunctionAttributes.StandardNames.StructureSetPoint,
                                                                    StructureFileWriterTestHelper.PUMP_CAPACITY,
                                                                    checkValue));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void PumpWaterLevelUpTest()
        {
            channel.AddPump(
                StructureFileWriterTestHelper.PUMP_ID,
                StructureFileWriterTestHelper.PUMP_NAME,
                StructureFileWriterTestHelper.PUMP_CAPACITY,
                chainage,
                StructureFileWriterTestHelper.PUMP_CONTROL_DIRECTION,
                StructureFileWriterTestHelper.PUMP_SUCTION_START,
                StructureFileWriterTestHelper.PUMP_SUCTION_STOP,
                StructureFileWriterTestHelper.PUMP_DELIVERY_START,
                StructureFileWriterTestHelper.PUMP_DELIVERY_STOP,
                StructureFileWriterTestHelper.PUMP_HEAD_VALUES,
                StructureFileWriterTestHelper.PUMP_REDUCTION_VALUES);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Pumps,
                                                    StructureFileWriterTestHelper.PUMP_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterLevelUpstream,
                                                    1.00));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void PumpWaterLevelDownTest()
        {
            channel.AddPump(
                StructureFileWriterTestHelper.PUMP_ID,
                StructureFileWriterTestHelper.PUMP_NAME,
                StructureFileWriterTestHelper.PUMP_CAPACITY,
                chainage,
                StructureFileWriterTestHelper.PUMP_CONTROL_DIRECTION,
                StructureFileWriterTestHelper.PUMP_SUCTION_START,
                StructureFileWriterTestHelper.PUMP_SUCTION_STOP,
                StructureFileWriterTestHelper.PUMP_DELIVERY_START,
                StructureFileWriterTestHelper.PUMP_DELIVERY_STOP,
                StructureFileWriterTestHelper.PUMP_HEAD_VALUES,
                StructureFileWriterTestHelper.PUMP_REDUCTION_VALUES);

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Pumps,
                                                    StructureFileWriterTestHelper.PUMP_ID.ToString(),
                                                    FunctionAttributes.StandardNames.StructureWaterLevelDownstream,
                                                    0.000));

        }

        #endregion
        
        #region Retention Area
        [Test]
        [Category(TestCategory.Integration)]
        public void RetentionWaterlevelTest()
        {
            var retention = new Retention()
            {
                Name = "retention1",
                Chainage = chainage,
                Branch = channel,
                Type = RetentionType.Reservoir,
                UseTable = false,
                BedLevel = -1,
                StorageArea = 1000,
                StreetLevel = 2,
                StreetStorageArea = 100000
            };
            channel.BranchFeatures.Add(retention);

            var retentions = model.Network.Retentions;

            var retention1 = retentions.FirstOrDefault();
            if (retention1 == null) throw new ArgumentNullException("retention1");
            retention1.Data[1.0] = 10.0;

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Retentions,
                                                    retention1.Name,
                                                    FunctionAttributes.StandardNames.WaterLevel,
                                                    0.7469));
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void RetentionVolumeTest()
        {
            var retention = new Retention()
            {
                Name = "retention1",
                Chainage = chainage,
                Branch = channel,
                Type = RetentionType.Reservoir,
                UseTable = false,
                BedLevel = 0,
                StorageArea = 10000,
                StreetLevel = 1,
                StreetStorageArea = 100000
            };
            channel.BranchFeatures.Add(retention);

            var retentions = model.Network.Retentions;

            var retention1 = retentions.FirstOrDefault();
            if (retention1 == null) throw new ArgumentNullException("retention1");
            retention1.Data[1.0] = 10.0;

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Retentions,
                                                    retention1.Name,
                                                    FunctionAttributes.StandardNames.WaterVolume,
                                                    7468.58));
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void RetentionWaterlevelWithRetentionOutputCoveragesOnTest()
        {
            var retention = new Retention()
            {
                Name = "retention1",
                Chainage = chainage,
                Branch = channel,
                Type = RetentionType.Reservoir,
                UseTable = false,
                BedLevel = -1,
                StorageArea = 1000,
                StreetLevel = 2,
                StreetStorageArea = 100000
            };
            channel.BranchFeatures.Add(retention);

            var retentions = model.Network.Retentions;

            var retention1 = retentions.FirstOrDefault();
            if (retention1 == null) throw new ArgumentNullException("retention1");
            retention1.Data[1.0] = 10.0;
            var engineParam = model.OutputSettings.EngineParameters.FirstOrDefault(ep => ep.Name == WaterFlowModelParameterNames.RetentionWaterLevel);
            Assert.That(engineParam, Is.Not.Null);
            engineParam.AggregationOptions = AggregationOptions.Current;
            
            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Retentions,
                                                    retention1.Name,
                                                    FunctionAttributes.StandardNames.WaterLevel,
                                                    0.7469));
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void RetentionVolumeWithRetentionOutputCoveragesOnTest()
        {
            var retention = new Retention()
            {
                Name = "retention1",
                Chainage = chainage,
                Branch = channel,
                Type = RetentionType.Reservoir,
                UseTable = false,
                BedLevel = 0,
                StorageArea = 10000,
                StreetLevel = 1,
                StreetStorageArea = 100000
            };
            channel.BranchFeatures.Add(retention);

            var retentions = model.Network.Retentions;

            var retention1 = retentions.FirstOrDefault();
            if (retention1 == null) throw new ArgumentNullException("retention1");
            retention1.Data[1.0] = 10.0;
            var engineParam = model.OutputSettings.EngineParameters.FirstOrDefault(ep => ep.Name == WaterFlowModelParameterNames.RetentionVolume);
            Assert.That(engineParam, Is.Not.Null);
            engineParam.AggregationOptions = AggregationOptions.Current;

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Retentions,
                                                    retention1.Name,
                                                    FunctionAttributes.StandardNames.WaterVolume,
                                                    7468.58));
        }

        #endregion
        
        #region Boundary Conditions
        [Test]
        [Category(TestCategory.Integration)]
        public void BoundaryConditionDischargeTest()
        {
            var boundaryCondition1 = model.BoundaryConditions.FirstOrDefault();
            if (boundaryCondition1 == null) throw new ArgumentNullException("boundaryCondition1");
            boundaryCondition1.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
            boundaryCondition1.Flow = 3.0;

            Assert.IsTrue(CheckGetSetGetAfterModelInitialization(WaterFlowParametersCategories.BoundaryConditions,
                                                    boundaryCondition1.Node.Name,
                                                    FunctionAttributes.StandardNames.WaterDischarge,
                                                    3.0,
                                                    3.0));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void BoundaryConditionWaterLevelTest()
        {
            var boundaryCondition1 = model.BoundaryConditions.FirstOrDefault();
            if (boundaryCondition1 == null) throw new ArgumentNullException("boundaryCondition1");
            boundaryCondition1.DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant;
            boundaryCondition1.WaterLevel = 3.0;

            var boundaryCondition2 = model.BoundaryConditions.ElementAt(1);
            if (boundaryCondition2 == null) throw new ArgumentNullException("boundaryCondition2");
            boundaryCondition2.DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant;
            boundaryCondition2.WaterLevel = 5.0;

            Assert.IsTrue(CheckGetSetGetAfterModelInitialization(WaterFlowParametersCategories.BoundaryConditions,
                                                    boundaryCondition1.Node.Name,
                                                    FunctionAttributes.StandardNames.WaterLevel,
                                                    3.0,
                                                    3.0));

        }
        /*
        [Test]
        public void BoundaryConditionVelocityTest()
        {

            Assert.IsTrue(CheckAfterModelExecution(BoundaryConditions,
                                                    StructureFileWriterTestHelper.INV_SIPHON_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterVelocity,
                                                    1.808));

        }

        [Test]
        public void BoundaryConditionFlowAreaTest()
        {
            
            Assert.IsTrue(CheckAfterModelExecution(Culverts,
                                                    StructureFileWriterTestHelper.INV_SIPHON_ID.ToString(),
                                                    FunctionAttributes.StandardNames.WaterFlowArea,
                                                    1.667));

        }
        */
        #endregion

        #region Laterals
        
        [Test]
        [Category(TestCategory.Integration)]
        public void LateralDischargeTest()
        {
            var lateral = new LateralSource
            {
                Name = "lateralSource1",
                Chainage = chainage,
                Branch = channel
            };
            channel.BranchFeatures.Add(lateral);
             
            var laterals = model.LateralSourceData;

            var lateral1 = laterals.FirstOrDefault();
            if (lateral1 == null) throw new ArgumentNullException("lateral1");
            lateral1.DataType = WaterFlowModel1DLateralDataType.FlowConstant;
            lateral1.Flow = 3.0;

            Assert.IsTrue(CheckGetSetGetAfterModelInitialization(WaterFlowParametersCategories.Laterals,
                                                    lateral1.Feature.Name,
                                                    FunctionAttributes.StandardNames.WaterDischarge,
                                                    3.0,
                                                    3.0));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void LateralWaterlevelTest()
        {
            var lateral = new LateralSource
            {
                Name = "lateralSource1",
                Chainage = chainage,
                Branch = channel
            };
            channel.BranchFeatures.Add(lateral);

            var laterals = model.LateralSourceData;

            var lateral1 = laterals.FirstOrDefault();
            if (lateral1 == null) throw new ArgumentNullException("lateral1");
            lateral1.DataType = WaterFlowModel1DLateralDataType.FlowWaterLevelTable;
            
            lateral1.Data[1.0] = 3.0;
            lateral1.Data[5.0] = 7.0;

            Assert.IsTrue(CheckAfterModelExecution(WaterFlowParametersCategories.Laterals,
                                                    lateral1.Feature.Name,
                                                    FunctionAttributes.StandardNames.WaterLevel,
                                                    0.785));

        }
        #endregion
        
        #region Helpers
        private bool CheckAfterModelExecution(string category, string id, string parameter, double expectation)
        {
            model.StatusChanged += (sender, args) =>
            {
                if (model.Status == ActivityStatus.Done)
                {
                    var array = model.GetVar(category, id, parameter) as double[];
                    Assert.NotNull(array);
                    var value = array[0];
                    Assert.AreEqual(expectation, value, 0.01);
                }
            };
            return RunModel();
        }
        
        private bool CheckGetSetGetAfterModelInitialization(string category, string id, string parameter, double expectation, double newValue)
        {
            model.StatusChanged += (sender, args) =>
            {
                if (model.Status == ActivityStatus.Initialized)
                {
                    var value = ((double[])model.GetVar(category, id, parameter))[0];
                    Assert.AreEqual(expectation, value, 0.001);
                    model.SetVar(new[] {newValue}, category, id, parameter);
                    value = ((double[])model.GetVar(category, id, parameter))[0];
                    Assert.AreEqual(newValue, value, 0.001);

                }
            };
            return RunModel();
        }
       
        private bool RunModel()
        {
            try
            {
                model.Initialize();
                Assert.AreEqual(ActivityStatus.Initialized, model.Status,
                    "Model should be in initialized state after it is created.");

                while (model.Status != ActivityStatus.Done)
                {
                    model.Execute();

                    if (model.Status == ActivityStatus.Failed)
                    {
                        Assert.Fail("Model run has failed");
                    }
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                model.Finish();
                model.Cleanup();
            }
            return true;
        }
        #endregion
         
    }
}