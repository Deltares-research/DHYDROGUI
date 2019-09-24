using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.NetCdf;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap.Api.SpatialOperations;
using SharpMap.Data.Providers;
using SharpMap.Extensions.CoordinateSystems;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class WaterFlowFMModelDefinitionTest
    {
        private const string TestFilePath = @"fm_files\fm_files.mdu";

        [Test]
        public void SetGuiTimePropertiesFromMduPropertiesTest()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            modelDefinition.GetModelProperty(KnownProperties.MapInterval).Value = new List<double>();
            modelDefinition.GetModelProperty(KnownProperties.HisInterval).Value = new List<double>();
            modelDefinition.GetModelProperty(KnownProperties.RstInterval).Value = new List<double>();
            modelDefinition.GetModelProperty(KnownProperties.WaqInterval).Value = new List<double>();

            modelDefinition.SetGuiTimePropertiesFromMduProperties();

            Assert.IsTrue((bool)modelDefinition.GetModelProperty(GuiProperties.WriteMapFile).Value);
            Assert.NotNull(modelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value);

            Assert.IsTrue((bool)modelDefinition.GetModelProperty(GuiProperties.WriteHisFile).Value);
            Assert.NotNull(modelDefinition.GetModelProperty(GuiProperties.HisOutputDeltaT).Value);

            Assert.IsTrue((bool)modelDefinition.GetModelProperty(GuiProperties.WriteRstFile).Value);
            Assert.NotNull(modelDefinition.GetModelProperty(GuiProperties.RstOutputDeltaT).Value);

            Assert.IsTrue((bool)modelDefinition.GetModelProperty(GuiProperties.SpecifyWaqOutputInterval).Value);
            Assert.NotNull(modelDefinition.GetModelProperty(GuiProperties.WaqOutputDeltaT).Value);
        }

        [Test]
        public void WhenCreatingANewWaterFlowFMModelDefinition_GuiTimeIntervalsForWritingClassMapFilesAreSetAccordingToPropertiesFile()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            var writeClassMapFile = (bool) modelDefinition.GetModelProperty(GuiProperties.WriteClassMapFile).Value;
            Assert.IsFalse(writeClassMapFile);
            var classMapOutputInterval = (TimeSpan) modelDefinition.GetModelProperty(GuiProperties.ClassMapOutputDeltaT).Value;
            Assert.IsTrue(new TimeSpan(0, 0, 5, 0).Equals(classMapOutputInterval));
        }

        [Test]
        public void WhenCreatingANewWaterFlowFMModelDefinition_GuiTimeIntervalsForWritingMapFilesAreSetAccordingToPropertiesFile()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            var writeMapFile = (bool)modelDefinition.GetModelProperty(GuiProperties.WriteMapFile).Value;
            Assert.IsTrue(writeMapFile);
            var mapOutputInterval = (TimeSpan)modelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value;
            Assert.IsTrue(new TimeSpan(0, 0, 20, 0).Equals(mapOutputInterval));
        }

        [Test]
        public void WhenCreatingANewWaterFlowFMModelDefinition_GuiTimeIntervalsForWritingHisFilesAreSetAccordingToPropertiesFile()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            var writeHisFile = (bool)modelDefinition.GetModelProperty(GuiProperties.WriteHisFile).Value;
            Assert.IsTrue(writeHisFile);
            var hisOutputInterval = (TimeSpan)modelDefinition.GetModelProperty(GuiProperties.HisOutputDeltaT).Value;
            Assert.IsTrue(new TimeSpan(0, 0, 5, 0).Equals(hisOutputInterval));
        }

        [Test]
        public void WhenCreatingANewWaterFlowFMModelDefinition_GuiTimeIntervalsForWritingRestartFilesAreSetAccordingToPropertiesFile()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            var writeRestartFile = (bool)modelDefinition.GetModelProperty(GuiProperties.WriteRstFile).Value;
            Assert.IsFalse(writeRestartFile);
            var restartOutputInterval = (TimeSpan)modelDefinition.GetModelProperty(GuiProperties.RstOutputDeltaT).Value;
            Assert.IsTrue(new TimeSpan(1, 0, 0, 0).Equals(restartOutputInterval));
        }

        [Test]
        public void WhenCreatingANewWaterFlowFMModelDefinition_GuiTimeIntervalsForWritingWaqFilesAreSetAccordingToPropertiesFile()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            var writeWaqFile = (bool)modelDefinition.GetModelProperty(GuiProperties.SpecifyWaqOutputInterval).Value;
            Assert.IsFalse(writeWaqFile);
            var waqOutputInterval = (TimeSpan)modelDefinition.GetModelProperty(GuiProperties.WaqOutputDeltaT).Value;
            Assert.IsTrue(new TimeSpan(0, 0, 0, 0).Equals(waqOutputInterval));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteMduFile_UpdatesCoordinateSystemInNetFileIfNecessary()
        {
            const string netFileName = "bendprof_map.nc";
            const string outputDirName = "readWriteMdu";
            if (Directory.Exists(outputDirName)) Directory.Delete(outputDirName, true);

            // setup
            var mduFilePath = TestHelper.GetTestFilePath(TestFilePath);
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(mduFilePath);

            var testDataFilePath = TestHelper.GetTestFilePath(@"output_mapfiles");
            var zmDfmZipFileName = "zm_dfm_map.zip";
            var zmDfmZipFilePath = Path.Combine(testDataFilePath, zmDfmZipFileName);

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                var area = new HydroArea();
                var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
                var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
                var mduFile = new MduFile();
                mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

                // set coordinate system
                var coordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(4326); //wsg84
                modelDefinition.CoordinateSystem = coordinateSystem;

                // setup test netfile
                modelDefinition.Properties.First(p => p.PropertyDefinition.MduPropertyName == "NetFile").Value = netFileName;

                FileUtils.CopyDirectory(testDataFilePath, tempDir);
                ZipFileUtils.Extract(zmDfmZipFilePath, tempDir);

                var simpleBoxMapFileName = "bendprof_map.nc";
                var mapFilePath = Path.Combine(tempDir, simpleBoxMapFileName);

                var outputDir = Directory.CreateDirectory(outputDirName);
                var netFile = Path.Combine(outputDir.FullName, netFileName);
                File.Copy(mapFilePath, netFile);

                // write mdu file
                mduFile.Write(outputDirName + @"/fm_files.mdu", modelDefinition, area, allFixedWeirsAndCorrespondingProperties.Values);

                // read coordinate system from file
                var fileCoordinateSystem = NetFile.ReadCoordinateSystem(netFile);
                Assert.AreEqual(coordinateSystem.AuthorityCode, fileCoordinateSystem.AuthorityCode);
            });



        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteMduFile_UpdatesCoordinateSystemInNetFileIfNecessary_UGrid()
        {
            const string netFileName = "Custom_Ugrid.nc";
            const string outputDirName = "readWriteMdu";
            if (Directory.Exists(outputDirName)) Directory.Delete(outputDirName, true);

            // setup
            var mduFilePath = TestHelper.GetTestFilePath(TestFilePath);
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(mduFilePath);

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            // set coordinate system
            var coordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(28992); //rd new
            modelDefinition.CoordinateSystem = coordinateSystem;

            // setup test netfile
            modelDefinition.Properties.First(p => p.PropertyDefinition.MduPropertyName == "NetFile").Value = netFileName;
            var existingNetFile = TestHelper.GetTestFilePath(@"ugrid\" + netFileName);
            var outputDir = Directory.CreateDirectory(outputDirName);
            var netFile = Path.Combine(outputDir.FullName, netFileName);
            File.Copy(existingNetFile, netFile);

            // write mdu file
            mduFile.Write(outputDirName+@"/fm_files.mdu", modelDefinition, area, allFixedWeirsAndCorrespondingProperties.Values);

            // read coordinate system from file
            var fileCoordinateSystem = NetFile.ReadCoordinateSystem(netFile);
            Assert.AreEqual(coordinateSystem.AuthorityCode, fileCoordinateSystem.AuthorityCode);

            // Additional step for UGrid files...
            // read node_z.grid_mapping
            var netCdfFile = NetCdfFile.OpenExisting(netFile);
            var netCdfVariable = netCdfFile.GetVariableByName("mesh2d_node_z");

            var gridMapping = netCdfFile.GetAttributeValue(netCdfVariable, "grid_mapping");

            // Attribute Value: "projected_coordinate_system" - relates to the variable name in NetCdfFile
            Assert.AreEqual("projected_coordinate_system", gridMapping);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadAndWriteMduFile()
        {
            var mduFilePath = TestHelper.GetTestFilePath(TestFilePath);
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(mduFilePath);

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            Directory.CreateDirectory("readWriteMdu");
            mduFile.Write("readWriteMdu/fm_files.mdu", modelDefinition, area, allFixedWeirsAndCorrespondingProperties.Values);

            var mduContent = File.ReadAllText("readWriteMdu/fm_files.mdu");
            var extForceFileContent = File.ReadAllText("readWriteMdu/fm_files.ext");
            Assert.IsTrue(extForceFileContent.Contains(
                "* FACTOR  =   : Conversion factor for this provider"));
            Assert.IsTrue(extForceFileContent.Contains(
                "* This comment line will not be removed, eventhough shiptxy is not yet supported."));
            Assert.IsTrue(mduContent.Contains(
                "! comment line on initial water level"));
            Assert.IsTrue(mduContent.Contains(
                "SomeNewFactor     = 3.7                 # new factor that should be read and written, but is not known"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadWriteMduFilesWithDifferentUnknownProperties()
        {
            // read model A
            var mduFilePathA = TestHelper.GetTestFilePath(@"mdu_examples\modelA.mdu");
            var mduDirA = Path.GetDirectoryName(mduFilePathA);
            var modelNameA = Path.GetFileName(mduFilePathA);
            var modelDefinitionA = new WaterFlowFMModelDefinition(mduDirA, modelNameA);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFileInA = new MduFile();
            mduFileInA.Read(mduFilePathA, modelDefinitionA, new HydroArea(), allFixedWeirsAndCorrespondingProperties);
            var fileCategories = modelDefinitionA.Properties.Select(p => p.PropertyDefinition.FileCategoryName);
            Assert.IsTrue(fileCategories.Contains("group_A"));
            Assert.AreEqual("A", modelDefinitionA.GetModelProperty("parametera").Value);


            // read model B, should not affect model A properties and custom groups...
            var mduFilePathB = TestHelper.GetTestFilePath(@"mdu_examples\modelB.mdu");
            var mduDirB = Path.GetDirectoryName(mduFilePathB);
            var modelNameB = Path.GetFileName(mduFilePathB);
            var modelDefinitionB = new WaterFlowFMModelDefinition(mduDirB, modelNameB);
            var mduFileInB = new MduFile();
            mduFileInB.Read(mduFilePathB, modelDefinitionB, new HydroArea(), new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>());
            Assert.AreEqual("B", modelDefinitionB.GetModelProperty("parameterb").Value);


            // write and confirm output model A
            var mduFilePathOutA = Path.Combine(modelNameA + "_out" + ".mdu");
            var mduFileOutA = new MduFile();

            var mduFileWriteConfig = new MduFileWriteConfig
            {
                WriteExtForcings = false,
                WriteFeatures = false,
            };

            mduFileOutA.Write(mduFilePathOutA, 
                              modelDefinitionA, 
                              new HydroArea(), 
                              new List<ModelFeatureCoordinateData<FixedWeir>>(), 
                              mduFileWriteConfig, 
                              switchTo: false);
            var originalLinesA = File.ReadAllLines(mduFilePathA).Where(l => !l.StartsWith("#")).ToList();
            var linesOutA = File.ReadAllLines(mduFilePathOutA).ToList();
            foreach (var line in originalLinesA.Where(l => !l.Contains("Version")))
            {
                Assert.IsTrue(linesOutA.Contains(line));
            }
            Assert.IsFalse(linesOutA.Any(l => l.Contains("parameterB"))); // models shouldn't mix their custom props!


            // write and confirm output model B
            var mduFilePathOutB = Path.Combine(modelNameB + "_out" + ".mdu");
            var mduFileOutB = new MduFile();
            mduFileOutB.Write(mduFilePathOutB, 
                              modelDefinitionB, 
                              new HydroArea(),
                              new List<ModelFeatureCoordinateData<FixedWeir>>(),
                              mduFileWriteConfig, 
                              switchTo: false);
            var originalLinesB = File.ReadAllLines(mduFilePathB).Where(l => !l.StartsWith("#")).ToList();
            var linesOutB = File.ReadAllLines(mduFilePathOutB).ToList();
            foreach (var line in originalLinesB.Where(l => !l.Contains("Version")))
            {
                Assert.IsTrue(linesOutB.Contains(line));
            }
            Assert.IsFalse(linesOutB.Any(l => l.Contains("parameterA")));
            Assert.IsFalse(linesOutB.Any(l => l.Contains("[group_A]")));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadLandBoundaryAndObservationsFile()
        {
            var mduFilePath = TestHelper.GetTestFilePath(TestFilePath);
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(mduFilePath);

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            var obsFileProperty =
                modelDefinition.GetModelProperty(KnownProperties.ObsFile);
            var obsFilePath = MduFileHelper.GetSubfilePath(mduFilePath, obsFileProperty);
            Assert.AreEqual(Path.Combine(mduDir, "fm_files_obs.xyn"), obsFilePath, "obs file path");
            var obsFile = new ObsFile<GroupableFeature2DPoint>();
            var observationPoints = obsFile.Read(obsFilePath);
            Assert.AreEqual(51, observationPoints.Count, "#observationPoints");
            Assert.AreEqual("2241", observationPoints[11].Name, "name of 12th obs");
            Assert.AreEqual("1853", observationPoints[40].Name, "name of 41st obs");
            Assert.AreEqual(26597.0, observationPoints[46].X, "X for obs with id 1769");
            Assert.AreEqual(33609.45, observationPoints[50].Y, "Y for last obs, id 47");

            var lbdFileProperty =
                modelDefinition.GetModelProperty(KnownProperties.LandBoundaryFile);
            var lbdFilePath = MduFileHelper.GetSubfilePath(mduFilePath, lbdFileProperty);
            Assert.AreEqual(Path.Combine(mduDir, "tst.ldb"), lbdFilePath, "ldb file path");
            var ldbFile = new LdbFile();
            var landBoundaries = ldbFile.Read(lbdFilePath);
            Assert.AreEqual(2, landBoundaries.Count, "#landBoundaries");
            Assert.AreEqual("L2", landBoundaries[1].Name, "name of 2nd land bound part");
            Assert.AreEqual(172.002274, ((ILineString)landBoundaries[0].Geometry).GetPointN(0).X, "X for first point of L1");
            Assert.AreEqual(160.002625, ((ILineString)landBoundaries[1].Geometry).GetPointN(2).Y, "Y for last point of L2");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFixedWeirsWithMissingValuesFile()
        {
            var mduFilePath = TestHelper.GetTestFilePath(TestFilePath);
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(mduFilePath);

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            var fixedWeirFileProperty =
                modelDefinition.GetModelProperty(KnownProperties.FixedWeirFile);
            var fixedWeirFilePath = MduFileHelper.GetSubfilePath(mduFilePath, fixedWeirFileProperty);
            Assert.AreEqual(Path.Combine(mduDir, "miss_val_fxw.pli"), fixedWeirFilePath, "obs file path");
            var fixedWeirFile = new PliFile<FixedWeir>();
            var fixedWeirs = fixedWeirFile.Read(fixedWeirFilePath);
            Assert.AreEqual(6, fixedWeirs.Count, "#fixedWeirs");
            Assert.AreEqual("bl01-3", fixedWeirs[2].Name, "name of 3th fixed weir");
            var polyline = ((ILineString)fixedWeirs[5].Geometry);
            Assert.AreEqual(188676.078, polyline.GetPointN(1).X, "X for last poly line part");
            Assert.AreEqual(428853.531, polyline.GetPointN(1).Y, "Y for last poly line part");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadExtForceFileAndPlifiles()
        {
            var mduFilePath = TestHelper.GetTestFilePath(TestFilePath);
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(mduFilePath);

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            Assert.AreEqual(4, modelDefinition.BoundaryConditions.Count(), "#ext_force_items");
            foreach (var bc in modelDefinition.BoundaryConditions.OfType<BoundaryCondition>())
            {
                if (bc.Name.Contains("bndB"))
                {
                    Assert.AreEqual(BoundaryConditionDataType.TimeSeries, bc.DataType, "bound.type for bound A and B");
                    Assert.AreEqual(2, bc.DataPointIndices.Count());
                    Assert.AreEqual(5, ((ITimeSeries)bc.GetDataAtPoint(0)).Time.Values.Count);
                    Assert.AreEqual(5, ((ITimeSeries)bc.GetDataAtPoint(1)).Time.Values.Count);
                }
                else if (bc.Name.Contains("left"))
                {
                    Assert.AreEqual(BoundaryConditionDataType.Harmonics, bc.DataType, "bound.type for bound left");
                    Assert.AreEqual(2, bc.DataPointIndices.Count());
                    Assert.AreEqual(1, bc.GetDataAtPoint(0).Arguments[0].Values.Count);
                    Assert.AreEqual(1, bc.GetDataAtPoint(1).Arguments[0].Values.Count);
                    Assert.AreEqual(720.0, bc.GetDataAtPoint(0).Arguments[0].Values[0]);
                    Assert.AreEqual(0.2, bc.GetDataAtPoint(1).Components[0].Values[0]);
                    Assert.AreEqual(0.11, bc.GetDataAtPoint(0).Components[1].Values[0]);
                }
                else if (bc.Name.Contains("right"))
                {
                    Assert.AreEqual(BoundaryConditionDataType.Harmonics, bc.DataType, "bound.type for bound right");
                    Assert.AreEqual(2, bc.DataPointIndices.Count());
                    Assert.AreEqual(1, bc.GetDataAtPoint(0).Arguments[0].Values.Count);
                    Assert.AreEqual(1, bc.GetDataAtPoint(1).Arguments[0].Values.Count);
                }
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadStructuresFile()
        {
            var mduFilePath = TestHelper.GetTestFilePath(TestFilePath);
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(mduFilePath);

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            Assert.AreEqual(2, area.Pumps.Count);
            Assert.AreEqual(3, area.Weirs.Count);
            Assert.AreEqual(1,area.Weirs.Where(w =>w.WeirFormula.GetType() == typeof(GatedWeirFormula)).ToList().Count);
            Assert.AreEqual(2, area.Weirs.Where(w => w.WeirFormula.GetType() == typeof(SimpleWeirFormula)).ToList().Count);
        }

        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadAndWriteOutputSettings()
        {
            var mduDir =
                Path.Combine(TestHelper.GetTestDataDirectory(), "simpleBox");

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, "simplebox");
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            mduFile.Read(Path.Combine(mduDir, "simplebox.mdu"), modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            var modelStartTime = (DateTime) modelDefinition.GetModelProperty(GuiProperties.StartTime).Value;
            var modelStopTime = (DateTime) modelDefinition.GetModelProperty(GuiProperties.StopTime).Value;
            
            Assert.AreEqual(new DateTime(2001, 01, 01, 0, 2, 0), modelStartTime);
            Assert.AreEqual(new DateTime(2001, 01, 01, 0, 12, 0), modelStopTime);

            Assert.AreEqual(new TimeSpan(0, 0, 8), modelDefinition.GetModelProperty(GuiProperties.HisOutputDeltaT).Value);
            Assert.AreEqual(new DateTime(2001, 01, 01, 0, 2, 12), modelDefinition.GetModelProperty(GuiProperties.HisOutputStartTime).Value);
            Assert.AreEqual(modelStopTime, modelDefinition.GetModelProperty(GuiProperties.HisOutputStopTime).Value);

            Assert.AreEqual(new TimeSpan(0, 0, 5), modelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value);
            Assert.AreEqual(modelStartTime, modelDefinition.GetModelProperty(GuiProperties.MapOutputStartTime).Value);
            Assert.AreEqual(modelStopTime, modelDefinition.GetModelProperty(GuiProperties.MapOutputStopTime).Value);

            Assert.AreEqual(new TimeSpan(0, 0, 5), modelDefinition.GetModelProperty(GuiProperties.RstOutputDeltaT).Value);
            Assert.AreEqual(new DateTime(2001, 01, 01, 0, 3, 0), modelDefinition.GetModelProperty(GuiProperties.RstOutputStartTime).Value);
            Assert.AreEqual(modelStopTime, modelDefinition.GetModelProperty(GuiProperties.RstOutputStopTime).Value);

            modelDefinition.GetModelProperty(GuiProperties.HisOutputDeltaT).Value = new TimeSpan(0, 0, 15);
            modelDefinition.GetModelProperty(GuiProperties.SpecifyHisStart).Value = true;
            modelDefinition.GetModelProperty(GuiProperties.HisOutputStartTime).Value = new DateTime(2001, 01, 01, 0, 2, 30);
            modelDefinition.GetModelProperty(GuiProperties.SpecifyHisStop).Value = true;
            modelDefinition.GetModelProperty(GuiProperties.HisOutputStopTime).Value = new DateTime(2001, 01, 01, 0, 3, 30);

            modelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value = new TimeSpan(0, 0, 6);
            modelDefinition.GetModelProperty(GuiProperties.SpecifyMapStart).Value = true;
            modelDefinition.GetModelProperty(GuiProperties.MapOutputStartTime).Value = new DateTime(2001, 01, 01, 0, 3, 0);

            modelDefinition.GetModelProperty(GuiProperties.RstOutputDeltaT).Value = new TimeSpan(0, 1, 0);
            modelDefinition.GetModelProperty(GuiProperties.SpecifyRstStart).Value = false;

            const string saveToDir = "readWriteSimpleBox";
            Directory.CreateDirectory(saveToDir);
            var fullDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), saveToDir);

            var mduFileSaveToPath = Path.Combine(fullDirectoryPath, "simplebox.mdu");
            mduFile.Write(mduFileSaveToPath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties.Values);

            var savedModelDefinition = new WaterFlowFMModelDefinition(saveToDir, "simplebox");
            var savedMduFile = new MduFile();
            savedMduFile.Read(Path.Combine(saveToDir, "simplebox.mdu"), savedModelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            Assert.AreEqual(new TimeSpan(0, 0, 15), savedModelDefinition.GetModelProperty(GuiProperties.HisOutputDeltaT).Value);
            Assert.AreEqual(new DateTime(2001, 01, 01, 0, 2, 30), savedModelDefinition.GetModelProperty(GuiProperties.HisOutputStartTime).Value);
            Assert.AreEqual(new DateTime(2001, 01, 01, 0, 3, 30), savedModelDefinition.GetModelProperty(GuiProperties.HisOutputStopTime).Value);

            Assert.AreEqual(new TimeSpan(0, 0, 6), savedModelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value);
            Assert.AreEqual(new DateTime(2001, 01, 01, 0, 3, 0), savedModelDefinition.GetModelProperty(GuiProperties.MapOutputStartTime).Value);
            Assert.AreEqual(modelStopTime, savedModelDefinition.GetModelProperty(GuiProperties.MapOutputStopTime).Value);

            Assert.AreEqual(new TimeSpan(0, 1, 0), savedModelDefinition.GetModelProperty(GuiProperties.RstOutputDeltaT).Value);
            Assert.AreEqual(modelStartTime, savedModelDefinition.GetModelProperty(GuiProperties.RstOutputStartTime).Value);
            Assert.AreEqual(modelStopTime, savedModelDefinition.GetModelProperty(GuiProperties.RstOutputStopTime).Value);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadAndWriteModelDefinitionIvkModel()
        {
            var mduDir =
                Path.Combine(TestHelper.GetTestDataDirectory(), "mdu_ivoorkust");

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, "ivk");
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            mduFile.Read(Path.Combine(mduDir, "ivk.mdu"), modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            const string saveToDir = "readWriteIvk";
            Directory.CreateDirectory(saveToDir);

            var mduFileSaveToPath = Path.Combine(saveToDir,"ivk.mdu");
            mduFile.Write(mduFileSaveToPath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties.Values);

            var mduContent = File.ReadAllText(mduFileSaveToPath);
            Assert.IsTrue(mduContent.Contains(
                "TStart            = 504                 # Start time w.r.t. RefDate (in TUnit)"));
            Assert.IsTrue(mduContent.Contains(
                "HisInterval       = 600                 # Interval (s) between history outputs"));
            Assert.IsTrue(mduContent.Contains(
                "! for now, no Smag."));

            var pliFileSaveToPath = FMSuiteFileBase.GetOtherFilePathInSameDirectory(mduFileSaveToPath, "versie2_01.pli");
            var pliFileContent = File.ReadAllText(pliFileSaveToPath);
            Assert.IsTrue(pliFileContent.Contains("versie2_01"));
            Assert.IsTrue(pliFileContent.Contains("    16    2"));
            Assert.IsTrue(pliFileContent.Contains("-3.645997829983308E+000  4.676944264305948E+000"));
            
            var extFileSaveToPath = FMSuiteFileBase.GetOtherFilePathInSameDirectory(mduFileSaveToPath, "ivk_wet.ext");
            var extFileContent = File.ReadAllText(extFileSaveToPath);
            Assert.IsTrue(extFileContent.Contains("QUANTITY=waterlevelbnd"));
            Assert.IsTrue(extFileContent.Contains("FILENAME=versie2_02.pli"));
            Assert.IsTrue(extFileContent.Contains("FILENAME=versie2_03.pli"));
            Assert.IsTrue(extFileContent.Contains("QUANTITY=dischargebnd"));
            Assert.IsTrue(extFileContent.Contains("FILENAME=river_me_wet.pli"));
            Assert.IsTrue(extFileContent.Contains("FILETYPE=9"));
            Assert.IsTrue(extFileContent.Contains("METHOD=3"));
            Assert.IsTrue(extFileContent.Contains("OPERAND=O"));
            Assert.IsFalse(extFileContent.Contains(
                "! this comment inside the block will not be kept"));
            Assert.IsTrue(extFileContent.Contains(
                "* this comment block should still be there"));
            Assert.IsTrue(extFileContent.Contains(
                "# this one too"));
            Assert.IsTrue(extFileContent.Contains(
                "* and this one too"));
            Assert.IsTrue(extFileContent.Contains(
                "* and finally this one"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadAndWriteModelDefinitionHarlingenModel()
        {
            string mduDir = Path.Combine(TestHelper.GetTestDataDirectory(), "harlingen");

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, "har");
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();

            var mduFile = new MduFile();
            mduFile.Read(Path.Combine(mduDir, "harWithoutOutputDir.mdu"), modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            const string saveToDir = "readWriteHarlingen";
            Directory.CreateDirectory(saveToDir);

            string mduFileSaveToPath = Path.Combine(saveToDir, "har.mdu");
            mduFile.Write(mduFileSaveToPath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties.Values);

            string expectedResultsDir = Path.Combine(mduDir, "expectedResults");
            foreach (string expectedResultsFilePath in Directory.GetFiles(expectedResultsDir))
            {
                string generatedResultsFilePath = Path.Combine(saveToDir, Path.GetFileName(expectedResultsFilePath));
                int skipNLines = generatedResultsFilePath.EndsWith(".mdu") ? 8 : 0; // skip date/program/version lines
                List<string> expectedResultsContent = File.ReadAllLines(expectedResultsFilePath).Skip(skipNLines).ToList();
                List<string> generatedResultsContent = File.ReadAllLines(generatedResultsFilePath).Skip(skipNLines).ToList();
                
                Assert.IsNotNull(generatedResultsContent);
                Assert.IsNotNull(expectedResultsContent);

                // Added first check if the strings are the same at the same index, because the contain method is not efficient for big files with a lot of lines.  
                if (expectedResultsContent.Count == generatedResultsContent.Count)
                {
                    for (var i = 0; i < expectedResultsContent.Count; i++)
                    {
                        if (expectedResultsContent[i] == generatedResultsContent[i])
                        {
                            continue;
                        }

                        expectedResultsContent.ForEach(er =>
                                                           Assert.IsTrue(generatedResultsContent.Contains(er),
                                                                         $"Expected {er} in File {Path.GetFileName(expectedResultsFilePath)} but not found."));
                        break;
                    }
                }
                else
                {
                    expectedResultsContent.ForEach(er =>
                        Assert.IsTrue(generatedResultsContent.Contains(er),
                            $"Expected {er} in File {Path.GetFileName(expectedResultsFilePath)} but not found."));

                }
            }
        }

        [Test]
        [Ignore("Run this test to generate expected model definition files")]
        public void GenerateExpectedResultsFolder()
        {
            var mduDir = Path.Combine(TestHelper.GetTestDataDirectory(), "harlingen");
            var expectedResultsDir = Path.Combine(mduDir, "expectedResults");
            
            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, "har");
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            mduFile.Read(Path.Combine(mduDir, "har.mdu"), modelDefinition, area, allFixedWeirsAndCorrespondingProperties);
            mduFile.Write(Path.Combine(expectedResultsDir, "har.mdu"), modelDefinition, area, allFixedWeirsAndCorrespondingProperties.Values);
        } 

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadAndWriteModelDefinitionC010TimeSeries()
        {
            var mduDir =
                Path.Combine(TestHelper.GetTestDataDirectory(), @"data\f05_boundary_conditions\c010_time_series\input");

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, "boundcond_test");
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            mduFile.Read(Path.Combine(mduDir, "boundcond_test.mdu"), modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            const string saveToDir = "readWriteC010";
            Directory.CreateDirectory(saveToDir);

            var mduFileSaveToPath = Path.Combine(saveToDir, "boundcond_test.mdu");
            mduFile.Write(mduFileSaveToPath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties.Values);

            var mduContent = File.ReadAllText(mduFileSaveToPath);
            Assert.IsTrue(mduContent.Contains(
                "NetFile           = boundcond_test_net.nc# *_net.nc"));
            Assert.IsTrue(mduContent.Contains(
                "CFLWaveFrac       = 0.1                 # Wave velocity fraction, total courant vel = u + cflw*wavevelocity"));

            var pliFileSaveToPath = FMSuiteFileBase.GetOtherFilePathInSameDirectory(mduFileSaveToPath, "north.pli");
            var pliFileContent = File.ReadAllText(pliFileSaveToPath);
            Assert.IsTrue(pliFileContent.Contains("north"));
            Assert.IsTrue(pliFileContent.Contains("    7    2"));
            Assert.IsTrue(pliFileContent.Contains("1.000000000000000E+002  1.050000000000000E+002"));

            var extFileSaveToPath = FMSuiteFileBase.GetOtherFilePathInSameDirectory(mduFileSaveToPath, "boundcond_test.ext");
            var extFileContent = File.ReadAllText(extFileSaveToPath);
            Assert.IsTrue(extFileContent.Contains("QUANTITY=waterlevelbnd"));
            Assert.IsTrue(extFileContent.Contains("FILENAME=north.pli"));
            Assert.IsTrue(extFileContent.Contains("QUANTITY=waterlevelbnd"));
            Assert.IsTrue(extFileContent.Contains("FILENAME=east_concave.pli"));
            Assert.IsTrue(extFileContent.Contains("FILETYPE=9"));
            Assert.IsTrue(extFileContent.Contains("METHOD=3"));
            Assert.IsTrue(extFileContent.Contains("OPERAND=O"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadModelDefinitionC075Frictiontypes()
        {
            var mduDir =
                Path.Combine(TestHelper.GetTestDataDirectory(), @"c075_Frictiontypes");

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, "frictiontypes");
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            mduFile.Read(Path.Combine(mduDir, "frictiontypes.mdu"), modelDefinition, area, allFixedWeirsAndCorrespondingProperties);
            Assert.IsTrue(modelDefinition.SpatialOperations.ContainsKey(WaterFlowFMModelDefinition.RoughnessDataItemName));
            Assert.AreEqual(2, modelDefinition.SpatialOperations[WaterFlowFMModelDefinition.RoughnessDataItemName].Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ReadWriteModelDefinitionHarlingenAndCheckAstroComponents()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(Path.GetDirectoryName(mduPath), "original");
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            mduFile.Read(mduPath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            var exportedBoundary =
                modelDefinition.BoundaryConditions.First(
                    bc => bc.DataType == BoundaryConditionDataType.Harmonics);

            var firstPoint = exportedBoundary.DataPointIndices.FirstOrDefault();

            var exportedHarmonics = exportedBoundary.GetDataAtPoint(firstPoint);

            string mduExportPath = "har_export.mdu";
            mduExportPath = Path.Combine(Directory.GetCurrentDirectory(), mduExportPath);
            mduFile.Write(mduExportPath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties.Values);

            var modelDefinitionReimport = new WaterFlowFMModelDefinition(Path.GetDirectoryName(mduExportPath), "exported");
            
            new MduFile().Read(mduExportPath, modelDefinitionReimport, area, allFixedWeirsAndCorrespondingProperties);

            var boundaryCondition = modelDefinitionReimport.BoundaryConditions.First(bc => bc.DataType == BoundaryConditionDataType.Harmonics && bc.ProcessName == "Flow");
            
            firstPoint = boundaryCondition.DataPointIndices.FirstOrDefault();

            var importedHarmonics = boundaryCondition.GetDataAtPoint(firstPoint);

            Assert.AreEqual(exportedHarmonics.Arguments[0].Values[0], importedHarmonics.Arguments[0].Values[0], "period");
            Assert.AreEqual(exportedHarmonics.Components[0].Values[0], importedHarmonics.Components[0].Values[0], "amplitude");
            Assert.AreEqual(exportedHarmonics.Components[1].Values[0], importedHarmonics.Components[1].Values[0], "phase");
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ReadMduAndVerifyIsEnabled()
        {
            var mduFilePath = TestHelper.GetTestFilePath(TestFilePath);
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(mduFilePath);

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            var useSalinity = modelDefinition.GetModelProperty(KnownProperties.UseSalinity);
            useSalinity.Value = true;

            var limtypsa = modelDefinition.GetModelProperty(KnownProperties.Limtypsa);
            Assert.IsTrue(limtypsa.IsEnabled(modelDefinition.Properties));

            useSalinity.Value = false;
            Assert.IsFalse(limtypsa.IsEnabled(modelDefinition.Properties));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void PropertyChangedEventsAreBubbledForModelProperties()
        {
            var mduFilePath = TestHelper.GetTestFilePath(TestFilePath);
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(mduFilePath);

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            mduFile.Read(mduFilePath, modelDefinition, area,allFixedWeirsAndCorrespondingProperties);

             var useSalinity = modelDefinition.GetModelProperty(KnownProperties.UseSalinity);
            useSalinity.Value = true;

            var limtypsa = modelDefinition.GetModelProperty(KnownProperties.Limtypsa);
            Assert.IsTrue(limtypsa.IsEnabled(modelDefinition.Properties));

            useSalinity.Value = false;
            Assert.IsFalse(limtypsa.IsEnabled(modelDefinition.Properties));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void SettingUseMorSedShouldWriteSedimentSection()
        {
            var mduFilePath = TestHelper.GetTestFilePath(TestFilePath);
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            var mduDir = Path.GetDirectoryName(mduFilePath);

            try
            {
                var modelName = Path.GetFileName(mduFilePath);
                var justModelName = Path.GetFileNameWithoutExtension(modelName);
                var area = new HydroArea();
                var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
                var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
                var mduFile = new MduFile();
                mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

                var useMorSed = modelDefinition.GetModelProperty("UseMorSed");
                Assert.IsFalse(bool.Parse(useMorSed.Value.ToString()));
                var readAllText = File.ReadAllText(mduFilePath);
                Assert.IsFalse(readAllText.Contains("[sediment]"));
                Assert.IsFalse(readAllText.Contains(KnownProperties.SedFile));
                Assert.IsFalse(readAllText.Contains(justModelName + ".sed"));
                Assert.IsFalse(readAllText.Contains(KnownProperties.MorFile));
                Assert.IsFalse(readAllText.Contains(justModelName + ".mor"));
                Assert.IsFalse(readAllText.Contains("Sedimentmodelnr"));
                useMorSed.Value = true;
                mduFile.Write(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties.Values);
                var otherArea = new HydroArea();
                var otherModelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
                var otherMduFile = new MduFile();
                otherMduFile.Read(mduFilePath, otherModelDefinition, otherArea, allFixedWeirsAndCorrespondingProperties);

                useMorSed = otherModelDefinition.GetModelProperty("UseMorSed");
                Assert.IsTrue(bool.Parse(useMorSed.Value.ToString()));
                readAllText = File.ReadAllText(mduFilePath);
                Assert.IsTrue(readAllText.Contains("[sediment]"));
                Assert.IsTrue(readAllText.Contains(KnownProperties.SedFile));
                Assert.IsTrue(readAllText.Contains(justModelName + ".sed"));
                Assert.IsTrue(readAllText.Contains(KnownProperties.MorFile));
                Assert.IsTrue(readAllText.Contains(justModelName + ".mor"));
                Assert.IsTrue(readAllText.Contains("Sedimentmodelnr"));
            }
            finally
            {
                FileUtils.DeleteIfExists(mduDir);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void MduSubFilesShouldChangeNamesAfterModelRename()
        {
            var model = new WaterFlowFMModel();

            Assert.AreEqual(model.ModelDefinition.ModelName, model.Name);
            Assert.AreEqual(model.Name + ExtForceFile.Extension,
                            model.ModelDefinition.GetModelProperty(KnownProperties.ExtForceFile).Value);

            model.Name = "newname";
            var boundary = new Feature2D
            {
                Name = "bound",
                Geometry = new LineString(new [] {new Coordinate(0, 0), new Coordinate(0, 100)})
            };
            model.Boundaries.Add(boundary);
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                FlowBoundaryConditionFactory.CreateBoundaryCondition(boundary));

            const string newnameMdu = "RenameIO/newname.mdu";
            model.ExportTo(newnameMdu);

            var newModel = new WaterFlowFMModel(newnameMdu);

            Assert.AreEqual(model.Name, newModel.ModelDefinition.ModelName);
            Assert.AreEqual(model.Name + "_bnd" + ExtForceFile.Extension,
                newModel.ModelDefinition.GetModelProperty(KnownProperties.BndExtForceFile).Value);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void IrregularlyNamedMduSubfilesShouldKeepNames()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            var model = new WaterFlowFMModel(mduPath);

            Assert.AreEqual(model.ModelDefinition.ModelName, model.Name);
            Assert.AreNotEqual(model.Name + ExtForceFile.Extension,
                            model.ModelDefinition.GetModelProperty(KnownProperties.ExtForceFile).Value);

            model.Name = "newname";
            model.Boundaries.Add(new Feature2D
            {
                Name = "bound",
                Geometry = new LineString(new [] { new Coordinate(0, 0), new Coordinate(0, 100) })
            });

            const string newnameMdu = "RenameIO/newname.mdu";
            var fullDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), newnameMdu);
            model.ExportTo(fullDirectoryPath);

            var newModel = new WaterFlowFMModel(fullDirectoryPath);

            Assert.AreEqual(model.Name, newModel.ModelDefinition.ModelName);
            Assert.AreNotEqual(model.Name + ExtForceFile.Extension,
                            newModel.ModelDefinition.GetModelProperty(KnownProperties.ExtForceFile).Value);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void SaveLoadTracerTest()
        {
            var model = new WaterFlowFMModel {Name = "feest"};

            var geometry = new LineString(
                new []
                    {
                        new Coordinate(0, 0),
                        new Coordinate(1, 0),
                    });

            model.Boundaries.Add(new Feature2D
            {
                    Name = "polygon",
                    Geometry = geometry,
                });

            var condition = new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer,
                                                      BoundaryConditionDataType.TimeSeries)
                {
                    Feature = model.Boundaries[0],
                    TracerName = "feest",
                };
            condition.AddPoint(0);
            condition.AddPoint(1);
            model.BoundaryConditionSets[0].BoundaryConditions.Add(condition);

            model.ExportTo(@"tracer\feest.mdu");

            Assert.IsTrue(File.Exists(@"tracer\feest.mdu"));

            var loadedModel = new WaterFlowFMModel(@"tracer\feest.mdu");

            Assert.AreEqual(1, loadedModel.BoundaryConditions.Count());
            Assert.AreEqual("feest", ((FlowBoundaryCondition)loadedModel.BoundaryConditions.First()).TracerName);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void SelectSpatialOperationsOnlySelectsCompletedOperationsTest()
        {
            // Issue#: DELFT3DFM-508
            // This test is to ensure that we ignore spatial operation coverages that are composed of entirely of non-data values
            // (This can happen when exporting spatial operations that comprise of added points but no interpolation
            // - we're not interested in these for the mdu, they will be saved as dataitems to the dsproj)

            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            var model = new WaterFlowFMModel(mduPath);
            
            // update model definition (called during export)
            model.ModelDefinition.SelectSpatialOperations(model.DataItems, model.TracerDefinitions);

            var mduSpatialOperations = model.ModelDefinition.SpatialOperations;
            Assert.AreEqual(3, mduSpatialOperations.Count);

            // retrieve value converter for InitialWaterLevel dataitem
            var valueConverter = SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(
                model.GetDataItemByValue(model.InitialWaterLevel), model.InitialWaterLevel.Name);

            // Generate samples to add
            var samples = new AddSamplesOperation(false);
            samples.SetInputData(AddSamplesOperation.SamplesInputName, new PointCloudFeatureProvider
            {
                PointCloud = new PointCloud
                {
                    PointValues = new List<IPointValue>
                    {
                        new PointValue { X = model.Grid.Cells[0].CenterX, Y = model.Grid.Cells[0].CenterY, Value = 45},
                        new PointValue { X = model.Grid.Cells[1].CenterX, Y = model.Grid.Cells[1].CenterY, Value = 67},
                        new PointValue { X = model.Grid.Cells[2].CenterX, Y = model.Grid.Cells[2].CenterY, Value = 78},
                        new PointValue { X = model.Grid.Cells[3].CenterX, Y = model.Grid.Cells[3].CenterY, Value = 58},
                        new PointValue { X = model.Grid.Cells[4].CenterX, Y = model.Grid.Cells[4].CenterY, Value = 39}
                    }
                }
            });

            // add samples
            Assert.IsNotNull(valueConverter.SpatialOperationSet.AddOperation(samples));

            // update model definition (called during export)
            model.ModelDefinition.SelectSpatialOperations(model.DataItems, model.TracerDefinitions);

            // assert that the incomplete spatial operation has not been added
            Assert.AreEqual(3, mduSpatialOperations.Count);
            Assert.IsFalse(mduSpatialOperations.Any(o => o.Key == model.InitialWaterLevel.Name));

            // create an interpolate operation using the samples added earlier
            var interpolateOperation = new InterpolateOperation();
            interpolateOperation.SetInputData(InterpolateOperation.InputSamplesName, samples.Output.Provider);
            Assert.IsNotNull(valueConverter.SpatialOperationSet.AddOperation(interpolateOperation));
            
            // update model definition (called during export)
            model.ModelDefinition.SelectSpatialOperations(model.DataItems, model.TracerDefinitions);

            // assert that the complete spatial operation has now been added
            Assert.AreEqual(4, mduSpatialOperations.Count);
            Assert.IsTrue(mduSpatialOperations.Any(o => o.Key == model.InitialWaterLevel.Name));
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void ImportSpatialOperationsTest()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            var model = new WaterFlowFMModel(mduPath);

            // Set values in coverage
            model.Viscosity.SetValues(new[]{500.0});       
    
            // create a spatial operation set on bathymetry
            var valueConverter = SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(
                model.GetDataItemByValue(model.Viscosity), model.Viscosity.Name);

            // add an unsupported operation (erase for example)
            var polygons = new[] { new Feature
            {
                Geometry = new[]{new Coordinate(-5,-5), new Coordinate(5,-5), new Coordinate(5,5), new Coordinate(-5,5)}.ToPolygon()
            }};

            var mask = new FeatureCollection(polygons, typeof(Feature));
            var eraseOperation = new EraseOperation();
            eraseOperation.SetInputData(SpatialOperation.MaskInputName, mask);
            Assert.IsNotNull(valueConverter.SpatialOperationSet.AddOperation(eraseOperation));

            model.ModelDefinition.SelectSpatialOperations(model.DataItems, model.TracerDefinitions);

            var viscosityOperations = model.ModelDefinition.SpatialOperations.First(kvp => kvp.Key == model.Viscosity.Name).Value;

            Assert.AreEqual(1, viscosityOperations.Count);
            Assert.IsTrue(viscosityOperations[0] is AddSamplesOperation);

            var salinityOperations = model.ModelDefinition.SpatialOperations.First(kvp => kvp.Key == model.InitialSalinity.Name).Value;

            Assert.AreEqual(2, salinityOperations.Count);
            Assert.IsTrue(salinityOperations[0] is SetValueOperation);
            Assert.IsTrue(salinityOperations[1] is SetValueOperation);
        }

        [Test]
        public void ChangeHeatFluxModelTypeShouldChangeUseTemperature()
        {
            var waterFlowFMModel = new WaterFlowFMModel();

            waterFlowFMModel.ModelDefinition.GetModelProperty(KnownProperties.Temperature).SetValueAsString("1");
            Assert.AreEqual(HeatFluxModelType.TransportOnly, waterFlowFMModel.HeatFluxModelType);
            
            waterFlowFMModel.ModelDefinition.GetModelProperty(KnownProperties.Temperature).SetValueAsString("3");
            Assert.AreEqual(HeatFluxModelType.ExcessTemperature, waterFlowFMModel.HeatFluxModelType);
            
            waterFlowFMModel.ModelDefinition.GetModelProperty(KnownProperties.Temperature).SetValueAsString("5");
            Assert.AreEqual(HeatFluxModelType.Composite, waterFlowFMModel.HeatFluxModelType);
            
            waterFlowFMModel.ModelDefinition.GetModelProperty(KnownProperties.Temperature).SetValueAsString("0");
            Assert.AreEqual(HeatFluxModelType.None, waterFlowFMModel.HeatFluxModelType);
        }
        
        [Test]
        [TestCase(MapFormatType.NetCdf, true, false, MapFormatType.Ugrid)]
        [TestCase(MapFormatType.NetCdf, false, false, MapFormatType.NetCdf)]
        [TestCase(MapFormatType.Tecplot, false, false, MapFormatType.Tecplot)]
        [TestCase(MapFormatType.Both, true, false, MapFormatType.Ugrid)]
        [TestCase(MapFormatType.Ugrid, false, false, MapFormatType.Ugrid)]
        [TestCase(MapFormatType.Ugrid, true, false, MapFormatType.Ugrid)]
        public void SetMapFormatPropertyValueTest(MapFormatType mapFormatType, bool useMorSed, MapFormatType expectedMapFormatType)
        {
            Assert.NotNull(mapFormatType);
            Assert.NotNull(expectedMapFormatType);

            var modelDefinition = new WaterFlowFMModelDefinition
            {
                MapFormat = mapFormatType,
                UseMorphologySediment = useMorSed
            };

            modelDefinition.SetMapFormatPropertyValue();

            // Check that MapFormat property value has been changed accordingly
            Assert.AreEqual(expectedMapFormatType, modelDefinition.MapFormat);
        }

        [Test]
        [TestCase(MapFormatType.NetCdf, true, false, MapFormatType.Ugrid)]
        [TestCase(MapFormatType.NetCdf, false, false, MapFormatType.NetCdf)]
        [TestCase(MapFormatType.Ugrid, true, false, MapFormatType.Ugrid)]
        [TestCase(MapFormatType.Ugrid, false, false, MapFormatType.Ugrid)]

        public void GivenModelDefinitionWhenSettingUseMorSedValueThenMapFormatHasExpectedValue(MapFormatType mapFormatType, bool useMorSed, MapFormatType expectedMapFormatType)
        {
            var modelDefinition = new WaterFlowFMModelDefinition
            {
                MapFormat = mapFormatType
            };
            modelDefinition.UseMorphologySediment = useMorSed;

            // Check that MapFormat property value has been changed accordingly
            Assert.AreEqual(useMorSed, modelDefinition.UseMorphologySediment);
            Assert.AreEqual(expectedMapFormatType, modelDefinition.MapFormat);
        }

        [Test]
        [TestCase(@"morphology\MorphologyActiveAndMapFormatEqualTo1.dsproj_data\FlowFM\FlowFM.mdu", MapFormatType.Ugrid, true)]
        [TestCase(@"morphology\MorphologyActiveAndMapFormatEqualTo4.dsproj_data\FlowFM\FlowFM.mdu", MapFormatType.Ugrid, true)]
        [TestCase(@"morphology\NoMorphologyActiveAndMapFormatEqualTo1.dsproj_data\FlowFM\FlowFM.mdu", MapFormatType.NetCdf, false)]
        [TestCase(@"morphology\NoMorphologyButMapFormatEqualTo4.dsproj_data\FlowFM\FlowFM.mdu", MapFormatType.Ugrid, false)]
        [Category(TestCategory.Integration)]
        public void GivenMduFileWhenImportingThenMapFormatHasExpectedValue(string relativeMduFilepath, MapFormatType expectedMapFormatType, bool expectedUseMorSedValue)
        {
            // setup
            var mduFilePath = TestHelper.GetTestFilePath(relativeMduFilepath);
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(mduFilePath);

            // Read the mdu file for modelDefinition properties
            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();

            var mduFile = new MduFile();
            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            // Check that MapFormat property value has been changed accordingly in modelDefinition
            Assert.AreEqual(expectedUseMorSedValue, modelDefinition.UseMorphologySediment);
            Assert.AreEqual(expectedMapFormatType, modelDefinition.MapFormat);
        }
        
        [Test]
        public void WriteSnappedFeaturesTest()
        {
            var model = new WaterFlowFMModel();
            var md = model.ModelDefinition;

            /* Default is false */
            Assert.IsFalse(md.WriteSnappedFeatures);
            CheckOutputSnappedFeaturesValue(md.WriteSnappedFeatures, md);

            md.WriteSnappedFeatures = true;
            Assert.IsTrue(md.WriteSnappedFeatures);
            CheckOutputSnappedFeaturesValue(md.WriteSnappedFeatures, md);
        }

        [Test]
        public void UpdateWriteOutputSnappedFeaturesWaterfallTest()
        {
            var model = new WaterFlowFMModel();
            var md = model.ModelDefinition;

            /* Default is false */
            Assert.IsFalse(md.WriteSnappedFeatures);
            CheckOutputSnappedFeaturesValue(false, md);

            /* Set one of the properties to true (try to use the last one in the UpdateWriteOutputSnappedFeatures conditional check) */
            md.GetModelProperty(KnownProperties.Wrishp_emb).Value = true;
            md.UpdateWriteOutputSnappedFeatures();

            /* The rest should be waterfall updated */
            Assert.IsTrue(md.WriteSnappedFeatures);
            CheckOutputSnappedFeaturesValue(md.WriteSnappedFeatures, md);
        }

        [Test]
        [TestCase(KnownProperties.Wrishp_crs)]
        [TestCase(KnownProperties.Wrishp_weir)]
        [TestCase(KnownProperties.Wrishp_gate)]
        [TestCase(KnownProperties.Wrishp_fxw)]
        [TestCase(KnownProperties.Wrishp_thd)]
        [TestCase(KnownProperties.Wrishp_obs)]
        [TestCase(KnownProperties.Wrishp_emb)]
        [TestCase(KnownProperties.Wrishp_dryarea)]
        [TestCase(KnownProperties.Wrishp_enc)]
        [TestCase(KnownProperties.Wrishp_src)]
        [TestCase(KnownProperties.Wrishp_pump)] 
        [Category(TestCategory.Jira)] // D3DFMIQ-278
        public void UpdateMduFileAfterSettingOptionWriteShapeFileTest(string property)
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"outputKnownProperties\FlowFM.mdu");
            var mduFileInfo = new FileInfo(mduFilePath);
            Assert.IsTrue(mduFileInfo.Exists);

            var workingDirectory = FileUtils.CreateTempDirectory();
            var workingMduFilePath = TestHelper.GetTestFilePath(Path.Combine(workingDirectory, mduFileInfo.Name));
            FileUtils.CopyFile(mduFileInfo.FullName, workingMduFilePath);
            var workingMduFileInfo = new FileInfo(workingMduFilePath);
            Assert.IsTrue(workingMduFileInfo.Exists);

            var model = new WaterFlowFMModel(workingMduFilePath);
            var md = model.ModelDefinition;

            Assert.IsFalse(md.WriteSnappedFeatures);
            CheckOutputSnappedFeaturesValue(false, md);

            md.WriteSnappedFeatures = true;
            Assert.IsTrue(md.WriteSnappedFeatures);
            CheckOutputSnappedFeaturesValue(md.WriteSnappedFeatures, md);

            md.WriteSnappedFeatures = false;
            Assert.IsFalse(md.WriteSnappedFeatures);
            CheckOutputSnappedFeaturesValue(md.WriteSnappedFeatures, md);

            var checkedProperty = md.KnownWriteOutputSnappedFeatures.Where(sf => sf.Equals(property)).FirstOrDefault();
            Assert.IsNotNull(checkedProperty);

            md.GetModelProperty(checkedProperty).Value = true;
            Assert.AreEqual(true, md.GetModelProperty(checkedProperty).Value);

            var uncheckedProperties = md.KnownWriteOutputSnappedFeatures.Where(sf => sf != checkedProperty).ToList();
            Assert.IsTrue(uncheckedProperties.TrueForAll(p => md.GetModelProperty(p).Value.Equals(false)));

            var mduFile = new MduFile(); 
            
            string saveToPath = Path.Combine(workingDirectory, "saved.mdu");
            var saveToFileInfo = new FileInfo(saveToPath);
            mduFile.Write(saveToPath, md, new HydroArea(), new List<ModelFeatureCoordinateData<FixedWeir>>());
            Assert.IsTrue(saveToFileInfo.Exists);

            var modelFromSavedMduFile = new WaterFlowFMModel();        
            var mdFromSavedMduFile = modelFromSavedMduFile.ModelDefinition;
            mduFile.Read(saveToPath, mdFromSavedMduFile, new HydroArea(), new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>());

            Assert.AreEqual(true, mdFromSavedMduFile.GetModelProperty(checkedProperty).Value);
            uncheckedProperties = mdFromSavedMduFile.KnownWriteOutputSnappedFeatures.Where(sf => sf != checkedProperty).ToList();
            Assert.IsTrue(uncheckedProperties.TrueForAll(p => mdFromSavedMduFile.GetModelProperty(p).Value.Equals(false)));

            FileUtils.DeleteIfExists(workingDirectory);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void UpdateWriteOutputSnappedFeaturesWaterfallFromFileTest()
        {
            var mduPath = TestHelper.GetTestFilePath(@"outputSnappedFeatures\outputSnappedFeatures.dsproj_data\FlowFM\FlowFM.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);
            var md = model.ModelDefinition;

            Assert.IsTrue(md.WriteSnappedFeatures);
            CheckOutputSnappedFeaturesValue(true, md);
        }

        private void CheckOutputSnappedFeaturesValue(bool expectedValue, WaterFlowFMModelDefinition modelDefinition)
        {
            foreach (var testProp in modelDefinition.KnownWriteOutputSnappedFeatures)
            {
                Assert.AreEqual(expectedValue, modelDefinition.GetModelProperty(testProp).Value);
            }
        }

        [Test]
        public void ReadEnclosureFile()
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"enclosureFiles\FlowFM.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);

            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(mduFilePath);

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            Assert.IsTrue(area.Enclosures.Count == 0);

            var mduFile = new MduFile();
            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);
            Assert.IsTrue(area.Enclosures.Count == 1);
        }

        [Test]
        public void Read3EnclosuresWithSameNameFromFile()
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"enclosureFiles\threeEnclosuresDifferentName.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);

            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(mduFilePath);

            var area = new HydroArea();
            area.Enclosures.Add(
                FlowFMTestHelper.CreateFeature2DPolygonFromGeometry(
                    "Enclosure01", 
                    FlowFMTestHelper.GetValidGeometryForEnclosureExample()));
            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            Assert.IsTrue(area.Enclosures.Count == 1);

            var mduFile = new MduFile();
            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);
            Assert.IsTrue(area.Enclosures.Count == 4);
        }

        [Test]
        public void Read3EnclosuresWithDifferentNameFromFile()
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"enclosureFiles\threeEnclosuresDifferentName.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);

            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(mduFilePath);

            var area = new HydroArea();
            area.Enclosures.Add(
                FlowFMTestHelper.CreateFeature2DPolygonFromGeometry(
                    "Enclosure01",
                    FlowFMTestHelper.GetValidGeometryForEnclosureExample()));
            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            Assert.IsTrue(area.Enclosures.Count == 1);

            var mduFile = new MduFile();
            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);
            Assert.IsTrue(area.Enclosures.Count == 4);
        }

        [Test]
        public void WriteEnclosureFile()
        {
            var nameWithoutExtension = Path.GetTempFileName();
            var mduFilePath = String.Concat(nameWithoutExtension, ".mdu");
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var encFilePath = TestHelper.GetTestFilePath(String.Concat(nameWithoutExtension,"_enc.pol"));

            try
            {
                var featureName = "EnclosureFeature";
                var mduFile = new MduFile();

                var area = new HydroArea();
                var modelDefinition = new WaterFlowFMModelDefinition(mduDir, Path.GetFileName(mduFilePath));
                var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
                //Add an enclosure.
                var enclosureGeom = FlowFMTestHelper.GetValidGeometryForEnclosureExample();
                var newEnclosure =
                    FlowFMTestHelper.CreateFeature2DPolygonFromGeometry(featureName, enclosureGeom);
                area.Enclosures.Add(newEnclosure);
                Assert.AreEqual(1, area.Enclosures.Count);

                mduFile.Write(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties.Values);

                Assert.IsTrue(File.Exists(mduFilePath));
                Assert.IsTrue(File.Exists(encFilePath));

                var writtenEncFile = File.ReadAllText(encFilePath);
                Assert.NotNull(writtenEncFile);
                Assert.IsNotEmpty(writtenEncFile);

                Assert.AreEqual(FlowFMTestHelper.GetExpectedEnclosurePolFileContent(featureName), writtenEncFile);
            }
            finally
            {
                FileUtils.DeleteIfExists(mduFilePath);
                FileUtils.DeleteIfExists(encFilePath);
            }
        }

        [Test]
        public void WriteAndReadEnclosureFileTest()
        {
            var nameWithoutExtension = Path.GetTempFileName();
            var mduFilePath = String.Concat(nameWithoutExtension, ".mdu");
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var encFilePath = TestHelper.GetTestFilePath(String.Concat(nameWithoutExtension, "_enc.pol"));

            try
            {
                var featureName = "EnclosureFeature";
                var mduFile = new MduFile();

                Assert.IsFalse(File.Exists(mduFilePath));
                Assert.IsFalse(File.Exists(encFilePath));

                /**/
                var area = new HydroArea();
                var modelDefinition = new WaterFlowFMModelDefinition(mduDir, Path.GetFileName(mduFilePath));
                var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
                //Add an enclosure.
                var enclosureGeom = FlowFMTestHelper.GetValidGeometryForEnclosureExample();
                var newEnclosure =
                    FlowFMTestHelper.CreateFeature2DPolygonFromGeometry(featureName, enclosureGeom);
                area.Enclosures.Add(newEnclosure);
                Assert.AreEqual(1, area.Enclosures.Count);

                mduFile.Write(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties.Values);

                Assert.IsTrue(File.Exists(mduFilePath));
                Assert.IsTrue(File.Exists(encFilePath));
                /**/

                var readModelDefinition = new WaterFlowFMModelDefinition();
                var readArea = new HydroArea();
               

                mduFile.Read(mduFilePath, readModelDefinition, readArea, allFixedWeirsAndCorrespondingProperties);

                Assert.AreEqual(area.Enclosures.Count, readArea.Enclosures.Count);
                Assert.AreEqual(enclosureGeom, readArea.Enclosures[0].Geometry);
            }
            finally
            {
                FileUtils.DeleteIfExists(mduFilePath);
                FileUtils.DeleteIfExists(encFilePath);
            }
        }

        [Test]
        [TestCase(KnownProperties.EnclosureFile)]
        [TestCase(KnownProperties.ObsFile)]
        [TestCase(KnownProperties.LandBoundaryFile)]
        [TestCase(KnownProperties.ThinDamFile)]
        [TestCase(KnownProperties.FixedWeirFile)]
        [TestCase(KnownProperties.StructuresFile)]
        [TestCase(KnownProperties.ObsCrsFile)]
        [TestCase(KnownProperties.DryPointsFile)]    
        public void HydroAreaPropertyIsMultipleEntriesFileName(string hydroAreaFileProperty)
        {
            var nameWithoutExtension = Path.GetTempFileName();
            var mduFilePath = String.Concat(nameWithoutExtension, ".mdu");
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, Path.GetFileName(mduFilePath));

            var property = modelDefinition.GetModelProperty(hydroAreaFileProperty);
            Assert.IsNotNull(property);
            Assert.AreEqual(typeof(List<string>), property.Value.GetType());
            Assert.AreEqual(true, MduFileHelper.IsFileValued(property));
            Assert.AreEqual(true, MduFileHelper.IsMultipleFileValued(property));
            Assert.AreEqual(typeof(IList<string>), property.PropertyDefinition.DataType);

            FileUtils.DeleteIfExists(mduFilePath);
        }

        [Test]
        public void GivenWaterFlowFmModel_WhenChangingHeatFluxModelToComposite_ThenTimeSeriesArgumentsAndComponentsAreInTheRightOrder()
        {
            var humidity = "Humidity";
            var airtemperature = "Air temperature";
            var cloudCoverage = "Cloud coverage";
            var solarRadiation = "Solar radiation";

            var fmModel = new WaterFlowFMModel();
            Assert.IsNull(fmModel.ModelDefinition.HeatFluxModel.MeteoData);
            fmModel.ModelDefinition.HeatFluxModel.Type = HeatFluxModelType.Composite;

            var meteoData = fmModel.ModelDefinition.HeatFluxModel.MeteoData;
            Assert.IsNotNull(meteoData);

            // Check arguments
            Assert.That(meteoData.Arguments.Count, Is.EqualTo(1));
            Assert.That(meteoData.Arguments.FirstOrDefault()?.Name, Is.EqualTo("Time"));

            // Check components
            var meteoDataComponents = meteoData.Components;
            Assert.That(meteoDataComponents.Count, Is.EqualTo(3));
            Assert.That(meteoDataComponents[0].Name, Is.EqualTo(humidity));
            Assert.That(meteoDataComponents[1].Name, Is.EqualTo(airtemperature));
            Assert.That(meteoDataComponents[2].Name, Is.EqualTo(cloudCoverage));

            // Add Solar Radiation to the Heat Flux Model
            fmModel.ModelDefinition.HeatFluxModel.ContainsSolarRadiation = true;

            // Check components
            Assert.That(meteoDataComponents.Count, Is.EqualTo(4));
            Assert.That(meteoDataComponents[0].Name, Is.EqualTo(humidity));
            Assert.That(meteoDataComponents[1].Name, Is.EqualTo(airtemperature));
            Assert.That(meteoDataComponents[2].Name, Is.EqualTo(cloudCoverage));
            Assert.That(meteoDataComponents[3].Name, Is.EqualTo(solarRadiation));
        }

        [Test]
        [TestCase(KnownProperties.SedFile,"Sediment")]
        [TestCase(KnownProperties.morphology,"Morphology")]

        public void Test_GetTabName_WithValidKeysAndModel_ExpectedTabNamesAreGiven(string key, string expectedName)
        {
            var tabName = string.Empty;
            Assert.DoesNotThrow(() =>
                {
                    TestHelper.AssertLogMessagesCount(
                        () => tabName = WaterFlowFMModelDefinition.GetTabName(key, fmModel:new WaterFlowFMModel()), 0);
                }
            );
            Assert.NotNull(tabName);
            Assert.AreEqual(tabName, expectedName);
        }

        [Test]
        public void Test_GetTabName_WithValidSedimentKeyAndWithoutModel_EmptyStringIsGivenAndNoLogMessages()
        {
            var key = KnownProperties.SedFile;
            var expectedName = string.Empty;
            string tabName = "Not Empty";

            Assert.DoesNotThrow(() =>
                {
                    TestHelper.AssertLogMessagesCount(
                        () => tabName = WaterFlowFMModelDefinition.GetTabName(key), 0);
                }
            );

            Assert.NotNull(tabName);
            Assert.AreEqual(tabName, expectedName);
        }

        [Test]
        public void Test_GetTabName_WithInvalidKey_LogErrorIsGivenAndNoExceptionThrown()
        {
            var key = "invalid";
            var message = "non-existent file";
            var expectedMessage = String.Format(
                Resources.WaterFlowFMModelDefinition_GetTabName_Invalid_gui_group_id_for___0___in_the_scheme_of_dflowfmmorpropertiescsv___1_,
                message,
                key);

            var expectedName = string.Empty;
            string tabName = "Not Empty";

            Assert.DoesNotThrow(() =>
            {
                TestHelper.AssertAtLeastOneLogMessagesContains(
                    () => tabName = WaterFlowFMModelDefinition.GetTabName(key, message),
                    expectedMessage);
            });

            Assert.NotNull(tabName);
            Assert.AreEqual(tabName, expectedName);
        }

        [Test]
        public void Test_GuiPropertyGroups_GetUniqueGuiPropertyGroupsFromModelAndMorphologyPropertyGroups()
        {
            Dictionary<string, ModelPropertyGroup> dummyVar;
            Assert.DoesNotThrow(() => dummyVar = WaterFlowFMModelDefinition.GuiPropertyGroups );
        }

        [Test]
        [TestCase("file_name", "file_name")]
        [TestCase("", "FlowFM1_clm.nc")]
        [TestCase(null, "FlowFM1_clm.nc")]
        [TestCase("FlowFM1_clm.nc", "FlowFM1_clm.nc")]
        public void GivenAModelDefinition_WhenClassMapFileNameIsCalled_ThenCorrectStringIsReturned(string propertyValue, string expectedString)
        {
            // Given
            var modelDefinition = new WaterFlowFMModelDefinition {ModelName = "FlowFM1"};
            modelDefinition.GetModelProperty(WaterFlowFMModelDefinition.ClassMapFilePropertyName).SetValueAsString(propertyValue);

            // When
            var resultedFileName = modelDefinition.ClassMapFileName;

            //Then
            Assert.AreEqual(expectedString, resultedFileName);
        }

        [Test]
        [TestCase("file_name", "file_name")]
        [TestCase("", "FlowFM1_map.nc")]
        [TestCase(null, "FlowFM1_map.nc")]
        [TestCase("FlowFM1_map.nc", "FlowFM1_map.nc")]
        public void GivenAModelDefinition_WhenMapFileNameIsCalled_ThenCorrectStringIsReturned(string propertyValue, string expectedString)
        {
            // Given
            var modelDefinition = new WaterFlowFMModelDefinition { ModelName = "FlowFM1" };
            modelDefinition.GetModelProperty(WaterFlowFMModelDefinition.MapFilePropertyName).SetValueAsString(propertyValue);

            // When
            var resultedFileName = modelDefinition.MapFileName;

            //Then
            Assert.AreEqual(expectedString, resultedFileName);
        }

        [Test]
        [TestCase("file_name", "file_name")]
        [TestCase("", "FlowFM1_his.nc")]
        [TestCase(null, "FlowFM1_his.nc")]
        [TestCase("FlowFM1_his.nc", "FlowFM1_his.nc")]
        public void GivenAModelDefinition_WhenHisFileNameIsCalled_ThenCorrectStringIsReturned(string propertyValue, string expectedString)
        {
            // Given
            var modelDefinition = new WaterFlowFMModelDefinition {ModelName = "FlowFM1"};
            modelDefinition.GetModelProperty(WaterFlowFMModelDefinition.HisFilePropertyName).SetValueAsString(propertyValue);

            // When
            var resultedFileName = modelDefinition.HisFileName;

            //Then
            Assert.AreEqual(expectedString, resultedFileName);
        }

        [Test]
        public void GivenAModelDefinitionWithClassMapIntervalProperty_WhensSetGuiTimePropertiesFromMduPropertiesIsCalled_ThenCorrectIntervalGuiIsSet()
        {
            // Given
            var modelDefinition = new WaterFlowFMModelDefinition();
            var classMapOutputDeltaTProperty = modelDefinition.GetModelProperty(GuiProperties.ClassMapOutputDeltaT);
            var classMapIntervalProperty = modelDefinition.GetModelProperty(KnownProperties.ClassMapInterval);

            classMapIntervalProperty.Value = new List<double> {120};
            Assert.IsTrue(new TimeSpan(0, 0, 5, 0).Equals((TimeSpan)classMapOutputDeltaTProperty.Value));

            // When 
            modelDefinition.SetGuiTimePropertiesFromMduProperties();

            // Then
            Assert.IsTrue(new TimeSpan(0, 0, 2, 0).Equals((TimeSpan)classMapOutputDeltaTProperty.Value));
            var writeClassMapFilePropertyValue = modelDefinition.GetModelProperty(GuiProperties.WriteClassMapFile).Value;
            Assert.AreEqual(true, (bool)writeClassMapFilePropertyValue);
        }

        [Test]
        public void GivenAModelDefinitionWithAClassMapIntervalPropertyWithEmptyValue_WhensSetGuiTimePropertiesFromMduPropertiesIsCalled_ThenCorrectDefaultValuesForGuiAreSet()
        {
            // Given
            var modelDefinition = new WaterFlowFMModelDefinition();

            var classMapIntervalProperty = modelDefinition.GetModelProperty(KnownProperties.ClassMapInterval);
            var classMapOutputDeltaTProperty = modelDefinition.GetModelProperty(GuiProperties.ClassMapOutputDeltaT);
            var writeClassMapFileProperty = modelDefinition.GetModelProperty(GuiProperties.WriteClassMapFile);

            classMapIntervalProperty.Value = new List<double>();          
            classMapOutputDeltaTProperty.Value = new TimeSpan(0,0,10,0);        
            writeClassMapFileProperty.Value = false;

            // When 
            modelDefinition.SetGuiTimePropertiesFromMduProperties();

            // Then
            Assert.IsEmpty((IList<double>)classMapIntervalProperty.Value);
            Assert.AreEqual(0, new TimeSpan(0,0,5,0).CompareTo((TimeSpan)classMapOutputDeltaTProperty.Value));
            Assert.AreEqual(true, (bool) writeClassMapFileProperty.Value);
        }

        [Test]
        [TestCase("", WaterFlowFMModelDefinition.DefaultOutputDirectoryName)]
        [TestCase(null, WaterFlowFMModelDefinition.DefaultOutputDirectoryName)]
        [TestCase(".", "")]
        [TestCase("custom", "custom")]
        [TestCase(WaterFlowFMModelDefinition.DefaultOutputDirectoryName, WaterFlowFMModelDefinition.DefaultOutputDirectoryName)]
        public void GivenAWaterFlowFMModelDefinitionWithAnOutputDirectoryProperty_WhenOutputDirectoryNameIsCalled_ThenCorrectStringIsReturned(string propertyValue, string expectedString)
        {
            // given
            var modelDefinition = new WaterFlowFMModelDefinition();
            modelDefinition.GetModelProperty(KnownProperties.OutputDir).SetValueAsString(propertyValue);

            // When
            var resultedString = modelDefinition.OutputDirectoryName;

            // Then
            Assert.AreEqual(expectedString, resultedString,
                $"When the value of the 'OutputDir' property is \"{propertyValue}\" then the expected OutputDirectoryName is \"{expectedString}\", but it was \"{resultedString}\".");
        }

        [Test]
        public void GivenAWaterFlowFMModelDefinitionWithoutAnOutputDirectoryProperty_WhenOutputDirectoryNameIsCalled_ThenCorrectStringIsReturned()
        {
            // given
            var modelDefinition = new WaterFlowFMModelDefinition();
            var property = modelDefinition.GetModelProperty(KnownProperties.OutputDir);
            modelDefinition.Properties.Remove(property);
            const string expectedString = WaterFlowFMModelDefinition.DefaultOutputDirectoryName;

            // When
            var resultedString = modelDefinition.OutputDirectoryName;

            // Then
            Assert.AreEqual(expectedString, resultedString,
                $"When the model definition does not contain the 'OutputDir' property, then the expected OutputDirectoryName is \"{expectedString}\", but it was \"{resultedString}\".");
        }

        [Test]
        public void
            SelectSpatialOperations_WithTwoDataItemsWithSameName_OneWithASpatialOperation_ThenOnlyThisOneIsTakenIntoAccountAndNoWarningIsGiven()
        {
            // Set-up
            const string name = "tracer";
            IDataItem dataItemWithoutConverter = CreateCoverageDataItem(name, false);
            IDataItem dataItemWithConverter = CreateCoverageDataItem(name, true);

            var modelDefinition = new WaterFlowFMModelDefinition();

            // Pre-condition
            Assert.That(modelDefinition.SpatialOperations, Is.Empty);

            // Action
            void TestAction()
            {
                modelDefinition.SelectSpatialOperations(
                    new[] {dataItemWithConverter, dataItemWithoutConverter},
                    new[] {name}
                );
            }

            IEnumerable<string> renderedMessages = TestHelper.GetAllRenderedMessages(TestAction);
            Assert.That(renderedMessages, Is.Empty);
            Assert.That(modelDefinition.SpatialOperations, Has.Count.EqualTo(1));
        }

        private static IDataItem CreateCoverageDataItem(string name, bool withValueConverter)
        {
            var grid = new UnstructuredGrid
            {
                Cells = new List<Cell>
                {
                    new Cell(new int[] {})
                }
            };

            var coverage = new UnstructuredGridCellCoverage(grid, false) {Name = name};
            var dataItem = MockRepository.GenerateStub<IDataItem>();
            dataItem.Value = coverage;
            dataItem.Name = name;

            if (withValueConverter)
            {
                dataItem.ValueConverter = GetStubbedValueConverter();
            }

            return dataItem;
        }

        private static SpatialOperationSetValueConverter GetStubbedValueConverter()
        {
            var operation = new SetValueOperation {OperationType = PointwiseOperationType.Overwrite};
            var converter = MockRepository.GenerateStub<SpatialOperationSetValueConverter>();
            var operationSet = MockRepository.GenerateStub<ISpatialOperationSet>();
            operationSet.Operations = new EventedList<ISpatialOperation> {operation};
            converter.Stub(c => c.SpatialOperationSet).Return(operationSet);
            return converter;
        }
    }
}
