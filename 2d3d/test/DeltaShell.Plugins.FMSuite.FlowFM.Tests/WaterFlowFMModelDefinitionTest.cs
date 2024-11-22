﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.NetCdf;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.SpatialOperations;
using SharpMap.Data.Providers;
using SharpMap.Extensions.CoordinateSystems;
using SharpMap.SpatialOperations;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class WaterFlowFMModelDefinitionTest
    {
        private const string testFilePath = @"fm_files\fm_files.mdu";

        [Test]
        public void FilePropertiesReturnOnlyFileBasedProperties()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            Assert.That(modelDefinition.FileProperties, Has.All.Matches<WaterFlowFMProperty>(x => x.PropertyDefinition.IsFile));
        }

        [Test]
        public void SetGuiTimePropertiesFromMduPropertiesTest()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            modelDefinition.GetModelProperty(KnownProperties.MapInterval).Value = new List<double>();
            modelDefinition.GetModelProperty(KnownProperties.HisInterval).Value = new List<double>();
            modelDefinition.GetModelProperty(KnownProperties.RstInterval).Value = new List<double>();
            modelDefinition.GetModelProperty(KnownProperties.WaqInterval).Value = new List<double>();

            modelDefinition.SetGuiTimePropertiesFromMduProperties();

            Assert.IsTrue((bool) modelDefinition.GetModelProperty(GuiProperties.WriteMapFile).Value);
            Assert.NotNull(modelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value);

            Assert.IsTrue((bool) modelDefinition.GetModelProperty(GuiProperties.WriteHisFile).Value);
            Assert.NotNull(modelDefinition.GetModelProperty(GuiProperties.HisOutputDeltaT).Value);

            Assert.IsTrue((bool) modelDefinition.GetModelProperty(GuiProperties.WriteRstFile).Value);
            Assert.NotNull(modelDefinition.GetModelProperty(GuiProperties.RstOutputDeltaT).Value);

            Assert.IsTrue((bool) modelDefinition.GetModelProperty(GuiProperties.SpecifyWaqOutputInterval).Value);
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

            var writeMapFile = (bool) modelDefinition.GetModelProperty(GuiProperties.WriteMapFile).Value;
            Assert.IsTrue(writeMapFile);
            var mapOutputInterval = (TimeSpan) modelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value;
            Assert.IsTrue(new TimeSpan(0, 0, 20, 0).Equals(mapOutputInterval));
        }

        [Test]
        public void WhenCreatingANewWaterFlowFMModelDefinition_GuiTimeIntervalsForWritingHisFilesAreSetAccordingToPropertiesFile()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            var writeHisFile = (bool) modelDefinition.GetModelProperty(GuiProperties.WriteHisFile).Value;
            Assert.IsTrue(writeHisFile);
            var hisOutputInterval = (TimeSpan) modelDefinition.GetModelProperty(GuiProperties.HisOutputDeltaT).Value;
            Assert.IsTrue(new TimeSpan(0, 0, 5, 0).Equals(hisOutputInterval));
        }

        [Test]
        public void WhenCreatingANewWaterFlowFMModelDefinition_GuiTimeIntervalsForWritingRestartFilesAreSetAccordingToPropertiesFile()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            var writeRestartFile = (bool) modelDefinition.GetModelProperty(GuiProperties.WriteRstFile).Value;
            Assert.IsFalse(writeRestartFile);
            var restartOutputInterval = (TimeSpan) modelDefinition.GetModelProperty(GuiProperties.RstOutputDeltaT).Value;
            Assert.IsTrue(new TimeSpan(1, 0, 0, 0).Equals(restartOutputInterval));
        }

        [Test]
        public void WhenCreatingANewWaterFlowFMModelDefinition_GuiTimeIntervalsForWritingWaqFilesAreSetAccordingToPropertiesFile()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            var writeWaqFile = (bool) modelDefinition.GetModelProperty(GuiProperties.SpecifyWaqOutputInterval).Value;
            Assert.IsFalse(writeWaqFile);
            var waqOutputInterval = (TimeSpan) modelDefinition.GetModelProperty(GuiProperties.WaqOutputDeltaT).Value;
            Assert.IsTrue(new TimeSpan(0, 0, 0, 0).Equals(waqOutputInterval));
        }
        
        [Test]
        public void WhenCreatingANewWaterFlowFMModelDefinition_PropertySortingIndicesHaveDefaultValue()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            Assert.IsTrue(modelDefinition.Properties.All(p => p.PropertyDefinition.SortIndex == -1));
        }
        
        [Test]
        public void WhenCreatingANewWaterFlowFMModelDefinition_PropertySortingIndicesAreReset()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();
            
            modelDefinition.Properties.ForEach(p => p.PropertyDefinition.SortIndex = 10);
            modelDefinition = new WaterFlowFMModelDefinition();
            
            Assert.IsTrue(modelDefinition.Properties.All(p => p.PropertyDefinition.SortIndex == -1));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteMduFile_UpdatesCoordinateSystemInNetFileIfNecessary()
        {
            const string netFileName = "bendprof_map.nc";
            const string outputDirName = "readWriteMdu";
            if (Directory.Exists(outputDirName))
            {
                Directory.Delete(outputDirName, true);
            }

            // setup
            string mduFilePath = TestHelper.GetTestFilePath(testFilePath);
            string modelName = Path.GetFileName(mduFilePath);

            string testDataFilePath = TestHelper.GetTestFilePath(@"output_mapfiles");
            var zmDfmZipFileName = "zm_dfm_map.zip";
            string zmDfmZipFilePath = Path.Combine(testDataFilePath, zmDfmZipFileName);

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                var area = new HydroArea();
                var modelDefinition = new WaterFlowFMModelDefinition(modelName);
                var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
                var mduFile = new MduFile();
                mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

                // set coordinate system
                ICoordinateSystem coordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(4326); //wsg84
                modelDefinition.CoordinateSystem = coordinateSystem;

                // setup test netfile
                modelDefinition.Properties.First(p => p.PropertyDefinition.MduPropertyName == "NetFile").Value = netFileName;

                FileUtils.CopyDirectory(testDataFilePath, tempDir);
                ZipFileUtils.Extract(zmDfmZipFilePath, tempDir);

                var simpleBoxMapFileName = "bendprof_map.nc";
                string mapFilePath = Path.Combine(tempDir, simpleBoxMapFileName);

                DirectoryInfo outputDir = Directory.CreateDirectory(outputDirName);
                string netFile = Path.Combine(outputDir.FullName, netFileName);
                File.Copy(mapFilePath, netFile);

                // write mdu file
                mduFile.Write(outputDirName + @"/fm_files.mdu", modelDefinition, area, allFixedWeirsAndCorrespondingProperties.Values);

                // read coordinate system from file
                ICoordinateSystem fileCoordinateSystem = NetFile.ReadCoordinateSystem(netFile);
                Assert.AreEqual(coordinateSystem.AuthorityCode, fileCoordinateSystem.AuthorityCode);
            });
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteMduFile_UpdatesCoordinateSystemInNetFileIfNecessary_UGrid()
        {
            const string netFileName = "Custom_Ugrid.nc";
            const string outputDirName = "readWriteMdu";
            if (Directory.Exists(outputDirName))
            {
                Directory.Delete(outputDirName, true);
            }

            // setup
            string mduFilePath = TestHelper.GetTestFilePath(testFilePath);
            string modelName = Path.GetFileName(mduFilePath);

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            // set coordinate system
            ICoordinateSystem coordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(28992); //rd new
            modelDefinition.CoordinateSystem = coordinateSystem;

            // setup test netfile
            modelDefinition.Properties.First(p => p.PropertyDefinition.MduPropertyName == "NetFile").Value = netFileName;
            string existingNetFile = TestHelper.GetTestFilePath(@"ugrid\" + netFileName);
            DirectoryInfo outputDir = Directory.CreateDirectory(outputDirName);
            string netFile = Path.Combine(outputDir.FullName, netFileName);
            File.Copy(existingNetFile, netFile);

            // write mdu file
            mduFile.Write(outputDirName + @"/fm_files.mdu", modelDefinition, area, allFixedWeirsAndCorrespondingProperties.Values);

            // read coordinate system from file
            ICoordinateSystem fileCoordinateSystem = NetFile.ReadCoordinateSystem(netFile);
            Assert.AreEqual(coordinateSystem.AuthorityCode, fileCoordinateSystem.AuthorityCode);

            // Additional step for UGrid files...
            // read node_z.grid_mapping
            NetCdfFile netCdfFile = NetCdfFile.OpenExisting(netFile);
            NetCdfVariable netCdfVariable = netCdfFile.GetVariableByName("mesh2d_node_z");

            string gridMapping = netCdfFile.GetAttributeValue(netCdfVariable, "grid_mapping");

            // Attribute Value: "projected_coordinate_system" - relates to the variable name in NetCdfFile
            Assert.AreEqual("projected_coordinate_system", gridMapping);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadWriteMduFilesWithDifferentUnknownProperties()
        {
            // read model A
            string mduFilePathA = TestHelper.GetTestFilePath(@"mdu_examples\modelA.mdu");
            string modelNameA = Path.GetFileName(mduFilePathA);
            var modelDefinitionA = new WaterFlowFMModelDefinition(modelNameA);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFileInA = new MduFile();
            mduFileInA.Read(mduFilePathA, modelDefinitionA, new HydroArea(), allFixedWeirsAndCorrespondingProperties);
            IEnumerable<string> fileSectionNames = modelDefinitionA.Properties.Select(p => p.PropertyDefinition.FileSectionName);
            Assert.IsTrue(fileSectionNames.Contains("group_A"));
            Assert.AreEqual("A", modelDefinitionA.GetModelProperty("parametera").Value);

            // read model B, should not affect model A properties and custom groups...
            string mduFilePathB = TestHelper.GetTestFilePath(@"mdu_examples\modelB.mdu");
            string modelNameB = Path.GetFileName(mduFilePathB);
            var modelDefinitionB = new WaterFlowFMModelDefinition(modelNameB);
            var mduFileInB = new MduFile();
            mduFileInB.Read(mduFilePathB, modelDefinitionB, new HydroArea(), new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>());
            Assert.AreEqual("B", modelDefinitionB.GetModelProperty("parameterb").Value);

            // write and confirm output model A
            string mduFilePathOutA = Path.Combine(modelNameA + "_out" + ".mdu");
            var mduFileOutA = new MduFile();

            var mduFileWriteConfig = new MduFileWriteConfig
            {
                WriteExtForcings = false,
                WriteFeatures = false
            };

            mduFileOutA.Write(mduFilePathOutA,
                              modelDefinitionA,
                              new HydroArea(),
                              new List<ModelFeatureCoordinateData<FixedWeir>>(),
                              mduFileWriteConfig,
                              false);

            List<string> linesOutA = File.ReadAllLines(mduFilePathOutA).ToList();
            Assert.IsFalse(linesOutA.Any(l => l.Contains("parameterB"))); // models shouldn't mix their custom props!

            // write and confirm output model B
            string mduFilePathOutB = Path.Combine(modelNameB + "_out" + ".mdu");
            var mduFileOutB = new MduFile();
            mduFileOutB.Write(mduFilePathOutB,
                              modelDefinitionB,
                              new HydroArea(),
                              new List<ModelFeatureCoordinateData<FixedWeir>>(),
                              mduFileWriteConfig,
                              false);

            List<string> linesOutB = File.ReadAllLines(mduFilePathOutB).ToList();
            Assert.IsFalse(linesOutB.Any(l => l.Contains("parameterA")));
            Assert.IsFalse(linesOutB.Any(l => l.Contains("[group_A]")));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadLandBoundaryAndObservationsFile()
        {
            string mduFilePath = TestHelper.GetTestFilePath(testFilePath);
            string mduDir = Path.GetDirectoryName(mduFilePath);
            string modelName = Path.GetFileName(mduFilePath);

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            WaterFlowFMProperty obsFileProperty =
                modelDefinition.GetModelProperty(KnownProperties.ObsFile);
            string obsFilePath = MduFileHelper.GetSubfilePath(mduFilePath, obsFileProperty);
            Assert.AreEqual(Path.Combine(mduDir, "fm_files_obs.xyn"), obsFilePath, "obs file path");
            var obsFile = new ObsFile<GroupableFeature2DPoint>();
            IList<GroupableFeature2DPoint> observationPoints = obsFile.Read(obsFilePath);
            Assert.AreEqual(51, observationPoints.Count, "#observationPoints");
            Assert.AreEqual("2241", observationPoints[11].Name, "name of 12th obs");
            Assert.AreEqual("1853", observationPoints[40].Name, "name of 41st obs");
            Assert.AreEqual(26597.0, observationPoints[46].X, "X for obs with id 1769");
            Assert.AreEqual(33609.45, observationPoints[50].Y, "Y for last obs, id 47");

            WaterFlowFMProperty lbdFileProperty =
                modelDefinition.GetModelProperty(KnownProperties.LandBoundaryFile);
            string lbdFilePath = MduFileHelper.GetSubfilePath(mduFilePath, lbdFileProperty);
            Assert.AreEqual(Path.Combine(mduDir, "tst.ldb"), lbdFilePath, "ldb file path");
            var ldbFile = new LdbFile();
            IList<LandBoundary2D> landBoundaries = ldbFile.Read(lbdFilePath);
            Assert.AreEqual(2, landBoundaries.Count, "#landBoundaries");
            Assert.AreEqual("L2", landBoundaries[1].Name, "name of 2nd land bound part");
            Assert.AreEqual(172.002274, ((ILineString) landBoundaries[0].Geometry).GetPointN(0).X, "X for first point of L1");
            Assert.AreEqual(160.002625, ((ILineString) landBoundaries[1].Geometry).GetPointN(2).Y, "Y for last point of L2");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFixedWeirsWithMissingValuesFile()
        {
            string mduFilePath = TestHelper.GetTestFilePath(testFilePath);
            string mduDir = Path.GetDirectoryName(mduFilePath);
            string modelName = Path.GetFileName(mduFilePath);

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            WaterFlowFMProperty fixedWeirFileProperty =
                modelDefinition.GetModelProperty(KnownProperties.FixedWeirFile);
            string fixedWeirFilePath = MduFileHelper.GetSubfilePath(mduFilePath, fixedWeirFileProperty);
            Assert.AreEqual(Path.Combine(mduDir, "miss_val_fxw.pli"), fixedWeirFilePath, "obs file path");
            var fixedWeirFile = new PliFile<FixedWeir>();
            IList<FixedWeir> fixedWeirs = fixedWeirFile.Read(fixedWeirFilePath);
            Assert.AreEqual(6, fixedWeirs.Count, "#fixedWeirs");
            Assert.AreEqual("bl01-3", fixedWeirs[2].Name, "name of 3th fixed weir");
            var polyline = (ILineString) fixedWeirs[5].Geometry;
            Assert.AreEqual(188676.078, polyline.GetPointN(1).X, "X for last poly line part");
            Assert.AreEqual(428853.531, polyline.GetPointN(1).Y, "Y for last poly line part");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadExtForceFileAndPlifiles()
        {
            string mduFilePath = TestHelper.GetTestFilePath(testFilePath);
            string modelName = Path.GetFileName(mduFilePath);

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            Assert.AreEqual(4, modelDefinition.BoundaryConditions.Count(), "#ext_force_items");
            foreach (BoundaryCondition bc in modelDefinition.BoundaryConditions.OfType<BoundaryCondition>())
            {
                if (bc.Name.Contains("bndB"))
                {
                    Assert.AreEqual(BoundaryConditionDataType.TimeSeries, bc.DataType, "bound.type for bound A and B");
                    Assert.AreEqual(2, bc.DataPointIndices.Count());
                    Assert.AreEqual(5, ((ITimeSeries) bc.GetDataAtPoint(0)).Time.Values.Count);
                    Assert.AreEqual(5, ((ITimeSeries) bc.GetDataAtPoint(1)).Time.Values.Count);
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
            string mduFilePath = TestHelper.GetTestFilePath(testFilePath);
            string modelName = Path.GetFileName(mduFilePath);

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            Assert.AreEqual(2, area.Pumps.Count);
            Assert.AreEqual(3, area.Structures.Count);
            Assert.AreEqual(1, area.Structures.Where(w => w.Formula.GetType() == typeof(SimpleGateFormula)).ToList().Count);
            Assert.AreEqual(2, area.Structures.Where(w => w.Formula.GetType() == typeof(SimpleWeirFormula)).ToList().Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadAndWriteOutputSettings()
        {
            string mduDir = Path.Combine(TestHelper.GetTestDataDirectory(), "simpleBox");

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition("simplebox");
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            mduFile.Read(Path.Combine(mduDir, "simplebox.mdu"), modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            var modelStartTime = (DateTime) modelDefinition.GetModelProperty(KnownProperties.StartDateTime).Value;
            var modelStopTime = (DateTime) modelDefinition.GetModelProperty(KnownProperties.StopDateTime).Value;

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
            string fullDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), saveToDir);

            string mduFileSaveToPath = Path.Combine(fullDirectoryPath, "simplebox.mdu");
            mduFile.Write(mduFileSaveToPath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties.Values);

            var savedModelDefinition = new WaterFlowFMModelDefinition("simplebox");
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
            string mduDir = Path.Combine(TestHelper.GetTestDataDirectory(), "mdu_ivoorkust");

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition("ivk");
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            mduFile.Read(Path.Combine(mduDir, "ivk.mdu"), modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            const string saveToDir = "readWriteIvk";
            Directory.CreateDirectory(saveToDir);

            string mduFileSaveToPath = Path.Combine(saveToDir, "ivk.mdu");
            mduFile.Write(mduFileSaveToPath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties.Values);

            string mduContent = File.ReadAllText(mduFileSaveToPath);
            WaterFlowFMMduFileTestHelper.AssertContainsMduLine(mduContent, "StartDateTime", "19830122");
            WaterFlowFMMduFileTestHelper.AssertContainsMduLine(mduContent, "HisInterval", "600");

            string pliFileSaveToPath = NGHSFileBase.GetOtherFilePathInSameDirectory(mduFileSaveToPath, "versie2_01.pli");
            string pliFileContent = File.ReadAllText(pliFileSaveToPath);
            Assert.IsTrue(pliFileContent.Contains("versie2_01"));
            Assert.IsTrue(pliFileContent.Contains("    16    2"));
            Assert.IsTrue(pliFileContent.Contains("-3.645997829983308E+000  4.676944264305948E+000"));

            string extFileSaveToPath = NGHSFileBase.GetOtherFilePathInSameDirectory(mduFileSaveToPath, "ivk_wet.ext");
            string extFileContent = File.ReadAllText(extFileSaveToPath);
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
                              "* this one too"));
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
            var modelDefinition = new WaterFlowFMModelDefinition("har");
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
                bool isMduFile = generatedResultsFilePath.EndsWith(".mdu", StringComparison.Ordinal);
                int skipNLines = isMduFile ? 8 : 0; // skip date/program/version lines
                List<string> expectedResultsContent = File.ReadAllLines(expectedResultsFilePath).Skip(skipNLines).ToList();
                List<string> generatedResultsContent = File.ReadAllLines(generatedResultsFilePath).Skip(skipNLines).ToList();

                Assert.IsNotNull(generatedResultsContent);
                Assert.IsNotNull(expectedResultsContent);

                if (isMduFile)
                {
                    string generatedContent = File.ReadAllText(generatedResultsFilePath);
                    IEnumerable<KeyValuePair<string, string>> expectedContent =
                        GetExpectedKeyValuePairs(expectedResultsContent);

                    foreach (KeyValuePair<string, string> expectedContentItem in expectedContent)
                    {
                        WaterFlowFMMduFileTestHelper.AssertContainsMduLine(
                            generatedContent, expectedContentItem.Key, expectedContentItem.Value);
                    }
                }
                else
                {
                    // Added first check if the strings are the same at the same index, because the contain method is not efficient for big files with a lot of lines.  
                    if (expectedResultsContent.Count == generatedResultsContent.Count)
                    {
                        for (var i = 0; i < expectedResultsContent.Count; i++)
                        {
                            if (expectedResultsContent[i] == generatedResultsContent[i])
                            {
                                continue;
                            }

                            expectedResultsContent.ForEach(er => Assert.IsTrue(generatedResultsContent.Contains(er),
                                                                               $"Expected {er} in File {Path.GetFileName(expectedResultsFilePath)} but not found."));
                            break;
                        }
                    }
                    else
                    {
                        expectedResultsContent.ForEach(er => Assert.IsTrue(generatedResultsContent.Contains(er),
                                                                           $"Expected {er} in File {Path.GetFileName(expectedResultsFilePath)} but not found."));
                    }
                }
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadAndWriteModelDefinitionC010TimeSeries()
        {
            string mduDir =
                Path.Combine(TestHelper.GetTestDataDirectory(), @"data\f05_boundary_conditions\c010_time_series\input");

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition("boundcond_test");
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            mduFile.Read(Path.Combine(mduDir, "boundcond_test.mdu"), modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            const string saveToDir = "readWriteC010";
            Directory.CreateDirectory(saveToDir);

            string mduFileSaveToPath = Path.Combine(saveToDir, "boundcond_test.mdu");
            mduFile.Write(mduFileSaveToPath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties.Values);

            string mduContent = File.ReadAllText(mduFileSaveToPath);
            WaterFlowFMMduFileTestHelper.AssertContainsMduLine(mduContent, "NetFile", "boundcond_test_net.nc");
            WaterFlowFMMduFileTestHelper.AssertContainsMduLine(mduContent, "CFLWaveFrac", "0.1");

            string pliFileSaveToPath = NGHSFileBase.GetOtherFilePathInSameDirectory(mduFileSaveToPath, "north.pli");
            string pliFileContent = File.ReadAllText(pliFileSaveToPath);
            Assert.IsTrue(pliFileContent.Contains("north"));
            Assert.IsTrue(pliFileContent.Contains("    7    2"));
            Assert.IsTrue(pliFileContent.Contains("1.000000000000000E+002  1.050000000000000E+002"));

            string extFileSaveToPath = NGHSFileBase.GetOtherFilePathInSameDirectory(mduFileSaveToPath, "boundcond_test.ext");
            string extFileContent = File.ReadAllText(extFileSaveToPath);
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
            string mduDir =
                Path.Combine(TestHelper.GetTestDataDirectory(), @"c075_Frictiontypes");

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition("frictiontypes");
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
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition("original");
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            mduFile.Read(mduPath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            IBoundaryCondition exportedBoundary =
                modelDefinition.BoundaryConditions.First(
                    bc => bc.DataType == BoundaryConditionDataType.Harmonics);

            int firstPoint = exportedBoundary.DataPointIndices.FirstOrDefault();

            IFunction exportedHarmonics = exportedBoundary.GetDataAtPoint(firstPoint);

            var mduExportPath = "har_export.mdu";
            mduExportPath = Path.Combine(Directory.GetCurrentDirectory(), mduExportPath);
            mduFile.Write(mduExportPath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties.Values);

            var modelDefinitionReimport = new WaterFlowFMModelDefinition("exported");

            new MduFile().Read(mduExportPath, modelDefinitionReimport, area, allFixedWeirsAndCorrespondingProperties);

            IBoundaryCondition boundaryCondition = modelDefinitionReimport.BoundaryConditions.First(bc => bc.DataType == BoundaryConditionDataType.Harmonics && bc.ProcessName == "Flow");

            firstPoint = boundaryCondition.DataPointIndices.FirstOrDefault();

            IFunction importedHarmonics = boundaryCondition.GetDataAtPoint(firstPoint);

            Assert.AreEqual(exportedHarmonics.Arguments[0].Values[0], importedHarmonics.Arguments[0].Values[0], "period");
            Assert.AreEqual(exportedHarmonics.Components[0].Values[0], importedHarmonics.Components[0].Values[0], "amplitude");
            Assert.AreEqual(exportedHarmonics.Components[1].Values[0], importedHarmonics.Components[1].Values[0], "phase");
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ReadMduAndVerifyIsEnabled()
        {
            string mduFilePath = TestHelper.GetTestFilePath(testFilePath);
            string modelName = Path.GetFileName(mduFilePath);

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            WaterFlowFMProperty useSalinity = modelDefinition.GetModelProperty(KnownProperties.UseSalinity);
            useSalinity.Value = true;

            WaterFlowFMProperty limtypsa = modelDefinition.GetModelProperty(KnownProperties.Limtypsa);
            Assert.IsTrue(limtypsa.IsEnabled(modelDefinition.Properties));

            useSalinity.Value = false;
            Assert.IsFalse(limtypsa.IsEnabled(modelDefinition.Properties));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void PropertyChangedEventsAreBubbledForModelProperties()
        {
            string mduFilePath = TestHelper.GetTestFilePath(testFilePath);
            string modelName = Path.GetFileName(mduFilePath);

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            WaterFlowFMProperty useSalinity = modelDefinition.GetModelProperty(KnownProperties.UseSalinity);
            useSalinity.Value = true;

            WaterFlowFMProperty limtypsa = modelDefinition.GetModelProperty(KnownProperties.Limtypsa);
            Assert.IsTrue(limtypsa.IsEnabled(modelDefinition.Properties));

            useSalinity.Value = false;
            Assert.IsFalse(limtypsa.IsEnabled(modelDefinition.Properties));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void SettingUseMorSedShouldWriteSedimentSection()
        {
            string testData = TestHelper.GetTestFilePath(testFilePath);
            
            using (var tempDir = new TemporaryDirectory())
            {
                string mduFilePath = tempDir.CopyTestDataFileAndDirectoryToTempDirectory(testData);

                var model = new WaterFlowFMModel();
                model.ImportFromMdu(mduFilePath);

                WaterFlowFMProperty useMorSed = model.ModelDefinition.GetModelProperty("UseMorSed");
                Assert.IsFalse((bool)useMorSed.Value);

                string readAllText = File.ReadAllText(mduFilePath);
                Assert.IsFalse(readAllText.Contains("[sediment]"));
                Assert.IsFalse(readAllText.Contains(KnownProperties.SedFile));
                Assert.IsFalse(readAllText.Contains(model.Name + ".sed"));
                Assert.IsFalse(readAllText.Contains(KnownProperties.MorFile));
                Assert.IsFalse(readAllText.Contains(model.Name + ".mor"));
                Assert.IsFalse(readAllText.Contains("Sedimentmodelnr"));

                useMorSed.Value = true;
                model.ExportTo(mduFilePath);

                var otherModel = new WaterFlowFMModel();
                otherModel.ImportFromMdu(mduFilePath);

                useMorSed = otherModel.ModelDefinition.GetModelProperty("UseMorSed");
                Assert.IsTrue((bool)useMorSed.Value);
                
                readAllText = File.ReadAllText(mduFilePath);
                Assert.IsTrue(readAllText.Contains("[sediment]"));
                Assert.IsTrue(readAllText.Contains(KnownProperties.SedFile));
                Assert.IsTrue(readAllText.Contains(otherModel.Name + ".sed"));
                Assert.IsTrue(readAllText.Contains(KnownProperties.MorFile));
                Assert.IsTrue(readAllText.Contains(otherModel.Name + ".mor"));
                Assert.IsTrue(readAllText.Contains("Sedimentmodelnr"));
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void MduSubFilesShouldChangeNamesAfterModelRename()
        {
            var model = new WaterFlowFMModel();

            Assert.AreEqual(model.ModelDefinition.ModelName, model.Name);
            Assert.AreEqual(model.Name + FileConstants.ExternalForcingFileExtension,
                            model.ModelDefinition.GetModelProperty(KnownProperties.ExtForceFile).Value);

            model.Name = "newname";
            var boundary = new Feature2D
            {
                Name = "bound",
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(0, 100)
                })
            };
            model.Boundaries.Add(boundary);
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                FlowBoundaryConditionFactory.CreateBoundaryCondition(boundary));

            const string newNameMdu = "RenameIO/newname.mdu";
            model.ExportTo(newNameMdu);

            var newModel = new WaterFlowFMModel();
            newModel.ImportFromMdu(newNameMdu);

            Assert.AreEqual(model.Name, newModel.ModelDefinition.ModelName);
            Assert.AreEqual(model.Name + FileConstants.BoundaryExternalForcingFileExtension,
                            newModel.ModelDefinition.GetModelProperty(KnownProperties.BndExtForceFile).Value);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void IrregularlyNamedMduSubfilesShouldKeepNames()
        {
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            Assert.AreEqual(model.ModelDefinition.ModelName, model.Name);
            Assert.AreNotEqual(model.Name + FileConstants.ExternalForcingFileExtension,
                               model.ModelDefinition.GetModelProperty(KnownProperties.ExtForceFile).Value);

            model.Name = "newname";
            model.Boundaries.Add(new Feature2D
            {
                Name = "bound",
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(0, 100)
                })
            });

            const string newnameMdu = "RenameIO/newname.mdu";
            string fullDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), newnameMdu);
            model.ExportTo(fullDirectoryPath);

            var newModel = new WaterFlowFMModel();
            newModel.ImportFromMdu(fullDirectoryPath);

            Assert.AreEqual(model.Name, newModel.ModelDefinition.ModelName);
            Assert.AreNotEqual(model.Name + FileConstants.ExternalForcingFileExtension,
                               newModel.ModelDefinition.GetModelProperty(KnownProperties.ExtForceFile).Value);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void SaveLoadTracerTest()
        {
            var model = new WaterFlowFMModel {Name = "feest"};

            var geometry = new LineString(
                new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(1, 0)
                });

            model.Boundaries.Add(new Feature2D
            {
                Name = "polygon",
                Geometry = geometry
            });

            var condition = new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer,
                                                      BoundaryConditionDataType.TimeSeries)
            {
                Feature = model.Boundaries[0],
                TracerName = "feest"
            };
            condition.AddPoint(0);
            condition.AddPoint(1);
            model.BoundaryConditionSets[0].BoundaryConditions.Add(condition);

            model.ExportTo(@"tracer\feest.mdu");

            Assert.IsTrue(File.Exists(@"tracer\feest.mdu"));

            var loadedModel = new WaterFlowFMModel();
            loadedModel.ImportFromMdu(@"tracer\feest.mdu");

            Assert.AreEqual(1, loadedModel.BoundaryConditions.Count());
            Assert.AreEqual("feest", ((FlowBoundaryCondition) loadedModel.BoundaryConditions.First()).TracerName);
        }

        [Test]
        public void SelectSpatialOperationsSetsUniqueOperationNameTest()
        {
            var model = new WaterFlowFMModel();
            var importSamplesOperation = new ImportSamplesSpatialOperation { Name = "some_name" };
            
            var valueConverter = SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(
                model.SpatialData.DataItems.First(x => x.Name == WaterFlowFMModelDefinition.InitialWaterLevelDataItemName));
            valueConverter.SpatialOperationSet.Operations.Add(importSamplesOperation.CreateOperations().Item2);
            valueConverter.SpatialOperationSet.Operations.Add(importSamplesOperation.CreateOperations().Item2);
                
            model.ModelDefinition.SelectSpatialOperations(model.SpatialData.DataItems, model.TracerDefinitions);

            var spatialOperations = model.ModelDefinition.GetSpatialOperations(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName);

            Assert.That(spatialOperations, Has.One.Matches<ISpatialOperation>(x => x.Name == "some_name"));
            Assert.That(spatialOperations, Has.One.Matches<ISpatialOperation>(x => x.Name == "some_name_1"));
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

            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            // update model definition (called during export)
            model.ModelDefinition.SelectSpatialOperations(model.AllDataItems.Where(d => d.Value is UnstructuredGridCoverage).ToList(), model.TracerDefinitions);

            IDictionary<string, IList<ISpatialOperation>> mduSpatialOperations = model.ModelDefinition.SpatialOperations;
            Assert.AreEqual(3, mduSpatialOperations.Count);

            // retrieve value converter for InitialWaterLevel dataitem
            SpatialOperationSetValueConverter valueConverter = SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(
                model.AllDataItems.First(d => d.Value == model.SpatialData.InitialWaterLevel), model.SpatialData.InitialWaterLevel.Name);

            // Generate samples to add
            var samples = new AddSamplesOperation(false);
            samples.SetInputData(AddSamplesOperation.SamplesInputName, new PointCloudFeatureProvider
            {
                PointCloud = new PointCloud
                {
                    PointValues = new List<IPointValue>
                    {
                        new PointValue
                        {
                            X = model.Grid.Cells[0].CenterX,
                            Y = model.Grid.Cells[0].CenterY,
                            Value = 45
                        },
                        new PointValue
                        {
                            X = model.Grid.Cells[1].CenterX,
                            Y = model.Grid.Cells[1].CenterY,
                            Value = 67
                        },
                        new PointValue
                        {
                            X = model.Grid.Cells[2].CenterX,
                            Y = model.Grid.Cells[2].CenterY,
                            Value = 78
                        },
                        new PointValue
                        {
                            X = model.Grid.Cells[3].CenterX,
                            Y = model.Grid.Cells[3].CenterY,
                            Value = 58
                        },
                        new PointValue
                        {
                            X = model.Grid.Cells[4].CenterX,
                            Y = model.Grid.Cells[4].CenterY,
                            Value = 39
                        }
                    }
                }
            });

            // add samples
            Assert.IsNotNull(valueConverter.SpatialOperationSet.AddOperation(samples));

            // update model definition (called during export)
            model.ModelDefinition.SelectSpatialOperations(model.AllDataItems.Where(d => d.Value is UnstructuredGridCoverage).ToList(), model.TracerDefinitions);

            // assert that the incomplete spatial operation has not been added
            Assert.AreEqual(3, mduSpatialOperations.Count);
            Assert.IsFalse(mduSpatialOperations.Any(o => o.Key == model.SpatialData.InitialWaterLevel.Name));

            // create an interpolate operation using the samples added earlier
            var interpolateOperation = new InterpolateOperation();
            interpolateOperation.SetInputData(InterpolateOperation.InputSamplesName, samples.Output.Provider);
            Assert.IsNotNull(valueConverter.SpatialOperationSet.AddOperation(interpolateOperation));

            // update model definition (called during export)
            model.ModelDefinition.SelectSpatialOperations(model.AllDataItems.Where(d => d.Value is UnstructuredGridCoverage).ToList(), model.TracerDefinitions);

            // assert that the complete spatial operation has now been added
            Assert.AreEqual(4, mduSpatialOperations.Count);
            Assert.IsTrue(mduSpatialOperations.Any(o => o.Key == model.SpatialData.InitialWaterLevel.Name));
        }

        [Test]
        public void SelectSpatialOperations_DataItemHasValueConverter_CreatedAddSamplesOperationForOriginalValue()
        {
            // Setup
            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(3, 3, 1, 1);

            var originalCoverage = new UnstructuredGridCellCoverage(grid, false);
            originalCoverage.Components[0].NoDataValue = -999d;
            originalCoverage.SetValues(new[]
            {
                1d,
                2d,
                3d
            });

            var setValueOperation = new SetValueOperation();
            var valueConverter = Substitute.For<SpatialOperationSetValueConverter>();
            valueConverter.OriginalValue = originalCoverage;
            valueConverter.SpatialOperationSet.Operations.Returns(new EventedList<ISpatialOperation> {setValueOperation});

            var dataItem = new DataItem(new UnstructuredGridCellCoverage(grid, false), "Some spatial coverage") {ValueConverter = valueConverter};

            var modelDefinition = new WaterFlowFMModelDefinition();

            // Call
            modelDefinition.SelectSpatialOperations(new List<IDataItem> {dataItem}, Enumerable.Empty<string>());

            // Assert
            IList<ISpatialOperation> spatialOperations = modelDefinition.SpatialOperations["Some spatial coverage"];
            Assert.That(spatialOperations, Has.Count.EqualTo(2));
            Assert.That(spatialOperations[0], Is.TypeOf<AddSamplesOperation>());
            Assert.That(spatialOperations[1], Is.SameAs(setValueOperation));
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void ImportSpatialOperationsTest()
        {
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            // Set values in coverage
            model.SpatialData.Viscosity.SetValues(new[]
            {
                500.0
            });

            // create a spatial operation set on bathymetry
            SpatialOperationSetValueConverter valueConverter = SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(
                model.AllDataItems.First(d => d.Value == model.SpatialData.Viscosity), model.SpatialData.Viscosity.Name);

            // add an unsupported operation (erase for example)
            var polygons = new[]
            {
                new Feature
                {
                    Geometry = new[]
                    {
                        new Coordinate(-5, -5),
                        new Coordinate(5, -5),
                        new Coordinate(5, 5),
                        new Coordinate(-5, 5)
                    }.ToPolygon()
                }
            };

            var mask = new FeatureCollection(polygons, typeof(Feature));
            var eraseOperation = new EraseOperation();
            eraseOperation.SetInputData(SpatialOperation.MaskInputName, mask);
            Assert.IsNotNull(valueConverter.SpatialOperationSet.AddOperation(eraseOperation));

            model.ModelDefinition.SelectSpatialOperations(model.AllDataItems.Where(d => d.Value is UnstructuredGridCoverage).ToList(), model.TracerDefinitions);

            IList<ISpatialOperation> viscosityOperations = model.ModelDefinition.SpatialOperations.First(kvp => kvp.Key == model.SpatialData.Viscosity.Name).Value;

            Assert.AreEqual(1, viscosityOperations.Count);
            Assert.IsTrue(viscosityOperations[0] is AddSamplesOperation);

            IList<ISpatialOperation> salinityOperations = model.ModelDefinition.SpatialOperations.First(kvp => kvp.Key == model.SpatialData.InitialSalinity.Name).Value;

            Assert.AreEqual(2, salinityOperations.Count);
            Assert.IsTrue(salinityOperations[0] is SetValueOperation);
            Assert.IsTrue(salinityOperations[1] is SetValueOperation);
        }

        [Test]
        public void ChangeHeatFluxModelTypeShouldChangeUseTemperature()
        {
            var waterFlowFMModel = new WaterFlowFMModel();

            waterFlowFMModel.ModelDefinition.GetModelProperty(KnownProperties.Temperature).SetValueFromString("1");
            Assert.AreEqual(HeatFluxModelType.TransportOnly, waterFlowFMModel.HeatFluxModelType);

            waterFlowFMModel.ModelDefinition.GetModelProperty(KnownProperties.Temperature).SetValueFromString("3");
            Assert.AreEqual(HeatFluxModelType.ExcessTemperature, waterFlowFMModel.HeatFluxModelType);

            waterFlowFMModel.ModelDefinition.GetModelProperty(KnownProperties.Temperature).SetValueFromString("5");
            Assert.AreEqual(HeatFluxModelType.Composite, waterFlowFMModel.HeatFluxModelType);

            waterFlowFMModel.ModelDefinition.GetModelProperty(KnownProperties.Temperature).SetValueFromString("0");
            Assert.AreEqual(HeatFluxModelType.None, waterFlowFMModel.HeatFluxModelType);
        }

        [Test]
        [TestCase(MapFormatType.NetCdf, true, MapFormatType.Ugrid)]
        [TestCase(MapFormatType.NetCdf, false, MapFormatType.NetCdf)]
        [TestCase(MapFormatType.Tecplot, false, MapFormatType.Tecplot)]
        [TestCase(MapFormatType.Both, true, MapFormatType.Ugrid)]
        [TestCase(MapFormatType.Ugrid, false, MapFormatType.Ugrid)]
        [TestCase(MapFormatType.Ugrid, true, MapFormatType.Ugrid)]
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
        [TestCase(MapFormatType.NetCdf, true, MapFormatType.Ugrid)]
        [TestCase(MapFormatType.NetCdf, false, MapFormatType.NetCdf)]
        [TestCase(MapFormatType.Ugrid, true, MapFormatType.Ugrid)]
        [TestCase(MapFormatType.Ugrid, false, MapFormatType.Ugrid)]
        public void GivenModelDefinitionWhenSettingUseMorSedValueThenMapFormatHasExpectedValue(MapFormatType mapFormatType, bool useMorSed, MapFormatType expectedMapFormatType)
        {
            var modelDefinition = new WaterFlowFMModelDefinition {MapFormat = mapFormatType};
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
            string mduFilePath = TestHelper.GetTestFilePath(relativeMduFilepath);
            string modelName = Path.GetFileName(mduFilePath);

            // Read the mdu file for modelDefinition properties
            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(modelName);
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
            WaterFlowFMModelDefinition md = model.ModelDefinition;

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
            WaterFlowFMModelDefinition md = model.ModelDefinition;

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
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void UpdateWriteOutputSnappedFeaturesWaterfallFromFileTest()
        {
            string mduPath = TestHelper.GetTestFilePath(@"outputSnappedFeatures\outputSnappedFeatures.dsproj_data\FlowFM\FlowFM.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            WaterFlowFMModelDefinition md = model.ModelDefinition;

            Assert.IsTrue(md.WriteSnappedFeatures);
            CheckOutputSnappedFeaturesValue(true, md);
        }

        [Test]
        public void ReadEnclosureFile()
        {
            string mduFilePath = TestHelper.GetTestFilePath(@"enclosureFiles\FlowFM.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);

            string modelName = Path.GetFileName(mduFilePath);

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            Assert.IsTrue(area.Enclosures.Count == 0);

            var mduFile = new MduFile();
            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);
            Assert.IsTrue(area.Enclosures.Count == 1);
        }

        [Test]
        public void Read3EnclosuresWithSameNameFromFile()
        {
            string mduFilePath = TestHelper.GetTestFilePath(@"enclosureFiles\threeEnclosuresDifferentName.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);

            string modelName = Path.GetFileName(mduFilePath);

            var area = new HydroArea();
            area.Enclosures.Add(
                FlowFMTestHelper.CreateFeature2DPolygonFromGeometry(
                    "Enclosure01",
                    FlowFMTestHelper.GetValidGeometryForEnclosureExample()));
            var modelDefinition = new WaterFlowFMModelDefinition(modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            Assert.IsTrue(area.Enclosures.Count == 1);

            var mduFile = new MduFile();
            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);
            Assert.IsTrue(area.Enclosures.Count == 4);
        }

        [Test]
        public void Read3EnclosuresWithDifferentNameFromFile()
        {
            string mduFilePath = TestHelper.GetTestFilePath(@"enclosureFiles\threeEnclosuresDifferentName.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);

            string modelName = Path.GetFileName(mduFilePath);

            var area = new HydroArea();
            area.Enclosures.Add(
                FlowFMTestHelper.CreateFeature2DPolygonFromGeometry(
                    "Enclosure01",
                    FlowFMTestHelper.GetValidGeometryForEnclosureExample()));
            var modelDefinition = new WaterFlowFMModelDefinition(modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            Assert.IsTrue(area.Enclosures.Count == 1);

            var mduFile = new MduFile();
            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);
            Assert.IsTrue(area.Enclosures.Count == 4);
        }

        [Test]
        public void WriteEnclosureFile()
        {
            string nameWithoutExtension = Path.GetTempFileName();
            string mduFilePath = string.Concat(nameWithoutExtension, ".mdu");
            string encFilePath = TestHelper.GetTestFilePath(string.Concat(nameWithoutExtension, "_enc.pol"));

            try
            {
                var featureName = "EnclosureFeature";
                var mduFile = new MduFile();

                var area = new HydroArea();
                var modelDefinition = new WaterFlowFMModelDefinition(Path.GetFileName(mduFilePath));
                var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
                //Add an enclosure.
                Polygon enclosureGeom = FlowFMTestHelper.GetValidGeometryForEnclosureExample();
                GroupableFeature2DPolygon newEnclosure =
                    FlowFMTestHelper.CreateFeature2DPolygonFromGeometry(featureName, enclosureGeom);
                area.Enclosures.Add(newEnclosure);
                Assert.AreEqual(1, area.Enclosures.Count);

                mduFile.Write(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties.Values);

                Assert.IsTrue(File.Exists(mduFilePath));
                Assert.IsTrue(File.Exists(encFilePath));

                string writtenEncFile = File.ReadAllText(encFilePath);
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
            string nameWithoutExtension = Path.GetTempFileName();
            string mduFilePath = string.Concat(nameWithoutExtension, ".mdu");
            string encFilePath = TestHelper.GetTestFilePath(string.Concat(nameWithoutExtension, "_enc.pol"));

            try
            {
                var featureName = "EnclosureFeature";
                var mduFile = new MduFile();

                Assert.IsFalse(File.Exists(mduFilePath));
                Assert.IsFalse(File.Exists(encFilePath));

                var area = new HydroArea();
                var modelDefinition = new WaterFlowFMModelDefinition(Path.GetFileName(mduFilePath));
                var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
                //Add an enclosure.
                Polygon enclosureGeom = FlowFMTestHelper.GetValidGeometryForEnclosureExample();
                GroupableFeature2DPolygon newEnclosure =
                    FlowFMTestHelper.CreateFeature2DPolygonFromGeometry(featureName, enclosureGeom);
                area.Enclosures.Add(newEnclosure);
                Assert.AreEqual(1, area.Enclosures.Count);

                mduFile.Write(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties.Values);

                Assert.IsTrue(File.Exists(mduFilePath));
                Assert.IsTrue(File.Exists(encFilePath));

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
            string nameWithoutExtension = Path.GetTempFileName();
            string mduFilePath = string.Concat(nameWithoutExtension, ".mdu");
            var modelDefinition = new WaterFlowFMModelDefinition(Path.GetFileName(mduFilePath));

            WaterFlowFMProperty property = modelDefinition.GetModelProperty(hydroAreaFileProperty);
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

            IFunction meteoData = fmModel.ModelDefinition.HeatFluxModel.MeteoData;
            Assert.IsNotNull(meteoData);

            // Check arguments
            Assert.That(meteoData.Arguments.Count, Is.EqualTo(1));
            Assert.That(meteoData.Arguments.FirstOrDefault()?.Name, Is.EqualTo("Time"));

            // Check components
            IEventedList<IVariable> meteoDataComponents = meteoData.Components;
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
        [TestCase(KnownProperties.SedFile, "Sediment")]
        [TestCase(KnownProperties.morphology, "Morphology")]
        public void Test_GetTabName_WithValidKeysAndModel_ExpectedTabNamesAreGiven(string key, string expectedName)
        {
            var tabName = string.Empty;
            Assert.DoesNotThrow(() =>
                {
                    TestHelper.AssertLogMessagesCount(
                        () => tabName = WaterFlowFMModelDefinition.GetTabName(key, fmModel: new WaterFlowFMModel()), 0);
                }
            );
            Assert.NotNull(tabName);
            Assert.AreEqual(tabName, expectedName);
        }

        [Test]
        public void Test_GetTabName_WithValidSedimentKeyAndWithoutModel_EmptyStringIsGivenAndNoLogMessages()
        {
            string key = KnownProperties.SedFile;
            var expectedName = string.Empty;
            var tabName = "Not Empty";

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
            string expectedMessage = string.Format(
                Resources.WaterFlowFMModelDefinition_GetTabName_Invalid_gui_group_id_for___0___in_the_scheme_of_dflowfmmorpropertiescsv___1_,
                message,
                key);

            var expectedName = string.Empty;
            var tabName = "Not Empty";

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
            Dictionary<string, ModelPropertyGroup> _;
            Assert.DoesNotThrow(() => _ = WaterFlowFMModelDefinition.GuiPropertyGroups);
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
            modelDefinition.GetModelProperty(WaterFlowFMModelDefinition.ClassMapFilePropertyName).SetValueFromString(propertyValue);

            // When
            string resultedFileName = modelDefinition.ClassMapFileName;

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
            var modelDefinition = new WaterFlowFMModelDefinition {ModelName = "FlowFM1"};
            modelDefinition.GetModelProperty(WaterFlowFMModelDefinition.MapFilePropertyName).SetValueFromString(propertyValue);

            // When
            string resultedFileName = modelDefinition.MapFileName;

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
            modelDefinition.GetModelProperty(WaterFlowFMModelDefinition.HisFilePropertyName).SetValueFromString(propertyValue);

            // When
            string resultedFileName = modelDefinition.HisFileName;

            //Then
            Assert.AreEqual(expectedString, resultedFileName);
        }

        [Test]
        public void GivenAModelDefinitionWithClassMapIntervalProperty_WhensSetGuiTimePropertiesFromMduPropertiesIsCalled_ThenCorrectIntervalGuiIsSet()
        {
            // Given
            var modelDefinition = new WaterFlowFMModelDefinition();
            WaterFlowFMProperty classMapOutputDeltaTProperty = modelDefinition.GetModelProperty(GuiProperties.ClassMapOutputDeltaT);
            WaterFlowFMProperty classMapIntervalProperty = modelDefinition.GetModelProperty(KnownProperties.ClassMapInterval);

            classMapIntervalProperty.Value = new List<double> {120};
            Assert.IsTrue(new TimeSpan(0, 0, 5, 0).Equals((TimeSpan) classMapOutputDeltaTProperty.Value));

            // When 
            modelDefinition.SetGuiTimePropertiesFromMduProperties();

            // Then
            Assert.IsTrue(new TimeSpan(0, 0, 2, 0).Equals((TimeSpan) classMapOutputDeltaTProperty.Value));
            object writeClassMapFilePropertyValue = modelDefinition.GetModelProperty(GuiProperties.WriteClassMapFile).Value;
            Assert.AreEqual(true, (bool) writeClassMapFilePropertyValue);
        }

        [Test]
        public void GivenAModelDefinitionWithAClassMapIntervalPropertyWithEmptyValue_WhensSetGuiTimePropertiesFromMduPropertiesIsCalled_ThenCorrectDefaultValuesForGuiAreSet()
        {
            // Given
            var modelDefinition = new WaterFlowFMModelDefinition();

            WaterFlowFMProperty classMapIntervalProperty = modelDefinition.GetModelProperty(KnownProperties.ClassMapInterval);
            WaterFlowFMProperty classMapOutputDeltaTProperty = modelDefinition.GetModelProperty(GuiProperties.ClassMapOutputDeltaT);
            WaterFlowFMProperty writeClassMapFileProperty = modelDefinition.GetModelProperty(GuiProperties.WriteClassMapFile);

            classMapIntervalProperty.Value = new List<double>();
            classMapOutputDeltaTProperty.Value = new TimeSpan(0, 0, 10, 0);
            writeClassMapFileProperty.Value = false;

            // When 
            modelDefinition.SetGuiTimePropertiesFromMduProperties();

            // Then
            Assert.IsEmpty((IList<double>) classMapIntervalProperty.Value);
            Assert.AreEqual(0, new TimeSpan(0, 0, 5, 0).CompareTo((TimeSpan) classMapOutputDeltaTProperty.Value));
            Assert.AreEqual(true, (bool) writeClassMapFileProperty.Value);
        }

        [Test]
        [TestCase("", DirectoryNameConstants.OutputDirectoryName)]
        [TestCase(null, DirectoryNameConstants.OutputDirectoryName)]
        [TestCase(".", "")]
        [TestCase("custom", "custom")]
        [TestCase(DirectoryNameConstants.OutputDirectoryName, DirectoryNameConstants.OutputDirectoryName)]
        public void GivenAWaterFlowFMModelDefinitionWithAnOutputDirectoryProperty_WhenOutputDirectoryNameIsCalled_ThenCorrectStringIsReturned(string propertyValue, string expectedString)
        {
            // given
            var modelDefinition = new WaterFlowFMModelDefinition();
            modelDefinition.GetModelProperty(KnownProperties.OutputDir).SetValueFromString(propertyValue);

            // When
            string resultedString = modelDefinition.OutputDirectoryName;

            // Then
            Assert.AreEqual(expectedString, resultedString,
                            $"When the value of the 'OutputDir' property is \"{propertyValue}\" then the expected OutputDirectoryName is \"{expectedString}\", but it was \"{resultedString}\".");
        }

        [Test]
        public void GivenAWaterFlowFMModelDefinitionWithoutAnOutputDirectoryProperty_WhenOutputDirectoryNameIsCalled_ThenCorrectStringIsReturned()
        {
            // given
            var modelDefinition = new WaterFlowFMModelDefinition();
            WaterFlowFMProperty property = modelDefinition.GetModelProperty(KnownProperties.OutputDir);
            modelDefinition.Properties.Remove(property);
            const string expectedString = DirectoryNameConstants.OutputDirectoryName;

            // When
            string resultedString = modelDefinition.OutputDirectoryName;

            // Then
            Assert.AreEqual(expectedString, resultedString,
                            $"When the model definition does not contain the 'OutputDir' property, then the expected OutputDirectoryName is \"{expectedString}\", but it was \"{resultedString}\".");
        }
        
        [Test]
        public void SetUseMorphologySediment_True_ThenSedimentModelNumberIsEqualTo4()
        {
            // Setup
            var modelDefinition = new WaterFlowFMModelDefinition();

            // Precondition
            Assert.That(modelDefinition.GetModelProperty(KnownProperties.SedimentModelNumber).GetValueAsString(), Is.EqualTo("0"));

            // Call
            modelDefinition.UseMorphologySediment = true;

            // Assert
            Assert.That(modelDefinition.GetModelProperty(KnownProperties.SedimentModelNumber).GetValueAsString(), Is.EqualTo("4"));
        }

        [Test]
        public void SetUseMorphologySediment_False_ThenSedimentModelNumberIsEqualTo0()
        {
            // Setup
            var modelDefinition = new WaterFlowFMModelDefinition {UseMorphologySediment = true};

            // Precondition
            Assert.That(modelDefinition.GetModelProperty(KnownProperties.SedimentModelNumber).GetValueAsString(), Is.EqualTo("4"));

            // Call
            modelDefinition.UseMorphologySediment = false;

            // Assert
            Assert.That(modelDefinition.GetModelProperty(KnownProperties.SedimentModelNumber).GetValueAsString(), Is.EqualTo("0"));
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void SelectSpatialOperations_OriginalSamplesEqualImportedSamples_DoesNotAddOriginalSamples()
        {
            using (var temp = new TemporaryDirectory())
            {
                var modelDefinition = new WaterFlowFMModelDefinition();
            
                UnstructuredGridCellCoverage coverage = CreateGridCoverageWithValue(7d);
                InterpolateOperation interpolateOperation = CreateInterpolateOperationWithValue(temp, 7d);

                IDataItem dataItem = CreateDataItem(coverage, interpolateOperation);
                
                // Call
                modelDefinition.SelectSpatialOperations(new List<IDataItem> {dataItem}, 
                                                        Enumerable.Empty<string>(), 
                                                        Enumerable.Empty<string>());
                
                // Assert
                Assert.That(modelDefinition.SpatialOperations.ContainsKey("initial_condition"));
                IList<ISpatialOperation> operation = modelDefinition.SpatialOperations["initial_condition"];
                Assert.That(operation, Has.Count.EqualTo(1));
                Assert.That(operation[0], Is.InstanceOf<ImportSamplesSpatialOperation>());
            }
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void SelectSpatialOperations_OriginalSamplesDoNotEqualImportedSamples_AddsOriginalSamples()
        {
            using (var temp = new TemporaryDirectory())
            {
                var modelDefinition = new WaterFlowFMModelDefinition();
            
                UnstructuredGridCellCoverage coverage = CreateGridCoverageWithValue(7d);
                InterpolateOperation interpolateOperation = CreateInterpolateOperationWithValue(temp, 6d);

                IDataItem dataItem = CreateDataItem(coverage, interpolateOperation);

                // Call
                modelDefinition.SelectSpatialOperations(new List<IDataItem> {dataItem}, 
                                                        Enumerable.Empty<string>(), 
                                                        Enumerable.Empty<string>());
                
                // Assert
                Assert.That(modelDefinition.SpatialOperations.ContainsKey("initial_condition"));
                IList<ISpatialOperation> operations = modelDefinition.SpatialOperations["initial_condition"];
                Assert.That(operations, Has.Count.EqualTo(2));
                Assert.That(operations[0], Is.InstanceOf<AddSamplesOperation>());
                Assert.That(operations[1], Is.InstanceOf<ImportSamplesSpatialOperation>());
            }
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void SelectSpatialOperations_DataItemOnlyContainsSpatialOperationSet_SpatialOperationsEmpty()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            UnstructuredGridCellCoverage coverage = CreateGridCoverageWithValue(7d);
            var operationSet = Substitute.For<ISpatialOperationSet>();
            operationSet.Operations.Returns(new EventedList<ISpatialOperation> {new ImportSamplesOperation(false)});
            IDataItem dataItem = CreateDataItem(coverage, operationSet);

            // Call
            modelDefinition.SelectSpatialOperations(new List<IDataItem> {dataItem},
                                                    Enumerable.Empty<string>(),
                                                    Enumerable.Empty<string>()); 
            
            // Assert
            Assert.That(modelDefinition.SpatialOperations.ContainsKey("initial_condition")); 
            IList<ISpatialOperation> operations = modelDefinition.SpatialOperations["initial_condition"];
            Assert.That(operations, Is.Empty);
        }

        [Test]
        [TestCaseSource(nameof(GetAllConditional3DLayerPropertiesCases))]
        public void WhenKmxIsZero_3DLayerPropertiesAreDisabledAndInvisible(string propertyName)
        {
            // Setup
            var modelDefinition = new WaterFlowFMModelDefinition();

            // Call
            modelDefinition.GetModelProperty(KnownProperties.Kmx).SetValueFromString("0");
            
            WaterFlowFMProperty property = modelDefinition.GetModelProperty(propertyName);

            // Assert
            Assert.That(property.IsEnabled(modelDefinition.Properties), Is.False);
            Assert.That(property.IsVisible(modelDefinition.Properties), Is.False);
        }
        
        [Test]
        [TestCaseSource(nameof(GetAllConditional3DLayerPropertiesCases))]

        public void WhenKmxIsLargerThanZero_LayerTypeAllZ_AllPropertiesAreEnabledAndVisible(string propertyName)
        {
            // Setup
            var modelDefinition = new WaterFlowFMModelDefinition();

            // Call
            modelDefinition.GetModelProperty(KnownProperties.Kmx).SetValueFromString("1");
            modelDefinition.GetModelProperty(KnownProperties.LayerType).SetValueFromString("2"); // all-z

            WaterFlowFMProperty property = modelDefinition.GetModelProperty(propertyName);

            // Assert
            Assert.That(property.IsEnabled(modelDefinition.Properties), Is.True);
            Assert.That(property.IsVisible(modelDefinition.Properties), Is.True);
        }

        [Test]
        [TestCaseSource(nameof(GetAllConditional3DLayerPropertiesCases))]

        public void WhenKmxIsLargerThanZero_LayerTypeAllSigma_NoPropertiesAreEnabledOrVisible(string propertyName)
        {
            // Setup
            var modelDefinition = new WaterFlowFMModelDefinition();

            // Call
            modelDefinition.GetModelProperty(KnownProperties.Kmx).SetValueFromString("1");
            modelDefinition.GetModelProperty(KnownProperties.LayerType).SetValueFromString("1"); // all-sigma
            
            WaterFlowFMProperty property = modelDefinition.GetModelProperty(propertyName);

            // Assert
            Assert.That(property.IsEnabled(modelDefinition.Properties), Is.False);
            Assert.That(property.IsVisible(modelDefinition.Properties), Is.False);
        }

        private static IEnumerable<TestCaseData> GetAllConditional3DLayerPropertiesCases()

        {
            yield return new TestCaseData(KnownProperties.DzTop);
            yield return new TestCaseData(KnownProperties.FloorLevTopLay);
            yield return new TestCaseData(KnownProperties.DzTopUniAboveZ);
            yield return new TestCaseData(KnownProperties.SigmaGrowthFactor);
            yield return new TestCaseData(KnownProperties.NumTopSig);
            yield return new TestCaseData(KnownProperties.NumTopSigUniform);
        }

        [Test]
        [TestCaseSource(nameof(GetOnlyZPropertiesAreEnabledAndVisibleCases))]
        public void WhenKmxIsLargerThanZero_LayerTypeAllZ_ZPropertiesAreEnabledAndVisible(string propertyName,
                                                                                         bool enabledAndVisible)
        {
            // Setup
            var modelDefinition = new WaterFlowFMModelDefinition();

            // Call
            modelDefinition.GetModelProperty(KnownProperties.Kmx).SetValueFromString("1");
            modelDefinition.GetModelProperty(KnownProperties.LayerType).SetValueFromString("2"); // all-z

            WaterFlowFMProperty property = modelDefinition.GetModelProperty(propertyName);

            // Assert
            Assert.That(property.IsEnabled(modelDefinition.Properties), Is.EqualTo(enabledAndVisible));
            Assert.That(property.IsVisible(modelDefinition.Properties), Is.EqualTo(enabledAndVisible));
        }
        
        private static IEnumerable<TestCaseData> GetOnlyZPropertiesAreEnabledAndVisibleCases()
        {
            yield return new TestCaseData(KnownProperties.DzTop, true);
            yield return new TestCaseData(KnownProperties.FloorLevTopLay, true);
            yield return new TestCaseData(KnownProperties.DzTopUniAboveZ, true);
            yield return new TestCaseData(KnownProperties.SigmaGrowthFactor, true);
            yield return new TestCaseData(KnownProperties.NumTopSig, true);
            yield return new TestCaseData(KnownProperties.NumTopSigUniform, true);
        }

        private static UnstructuredGridCellCoverage CreateGridCoverageWithValue(double value)
        {
            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 1, 1);
            UnstructuredGridCellCoverage coverage = UnstructuredGridCoverageFactory.CreateCellCoverage("initial_condition", grid, defaultValue: value);
            
            return coverage;
        }

        private static InterpolateOperation CreateInterpolateOperationWithValue(TemporaryDirectory temp, double value)
        {
            string filePath = Path.Combine(temp.Path, "initial_condition.xyz");
            File.WriteAllLines(filePath, new []
            {
                $"0.5 0.5 {value}",
                $"1.5 0.5 {value}",
                $"0.5 1.5 {value}",
                $"1.5 1.5 {value}",
            });
            
            
            var importOperation = new ImportSamplesOperation(true) {FilePath = filePath};
            var interpolateOperation = new InterpolateOperation();
            interpolateOperation.Mask.Provider = new FeatureCollection(new List<Feature>(), typeof(Feature));
            interpolateOperation.LinkInput(InterpolateOperation.InputSamplesName, importOperation.Output);
            
            return interpolateOperation;
        }
        
        private static IDataItem CreateDataItem(UnstructuredGridCellCoverage coverage, ISpatialOperation operation)
        {
            IDataItem dataItem = new DataItem(coverage, DataItemRole.Input) {Name = "initial_condition"};
            var valueConverter = Substitute.For<SpatialOperationSetValueConverter>();
            dataItem.ValueConverter = valueConverter;
            var set = Substitute.For<ISpatialOperationSet>();
            valueConverter.OriginalValue = coverage;
            valueConverter.SpatialOperationSet.Returns(set);
            set.Operations = new EventedList<ISpatialOperation> {operation};
            
            return dataItem;
        }
        
        private static IEnumerable<KeyValuePair<string, string>> GetExpectedKeyValuePairs(IEnumerable<string> lines)
        {
            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                if (trimmedLine == string.Empty ||
                    trimmedLine.StartsWith("[") ||
                    trimmedLine.StartsWith("#"))
                {
                    continue;
                }

                string[] segments = trimmedLine.Split('=', '#');

                string key = segments[0].Trim();
                string value = segments[1].Trim();

                yield return new KeyValuePair<string, string>(key, value);
            }
        }

        private void CheckOutputSnappedFeaturesValue(bool expectedValue, WaterFlowFMModelDefinition modelDefinition)
        {
            foreach (string testProp in modelDefinition.KnownWriteOutputSnappedFeatures)
            {
                Assert.AreEqual(expectedValue, modelDefinition.GetModelProperty(testProp).Value);
            }
        }
    }
}