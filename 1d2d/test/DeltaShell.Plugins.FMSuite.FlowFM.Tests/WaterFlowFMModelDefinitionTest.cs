﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.NetCdf;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.SpatialOperations;
using SharpMap.Data.Providers;
using SharpMap.Extensions.CoordinateSystems;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class WaterFlowFMModelDefinitionTest
    {
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

            Assert.IsFalse((bool)modelDefinition.GetModelProperty(GuiProperties.WriteRstFile).Value);
            Assert.NotNull(modelDefinition.GetModelProperty(GuiProperties.RstOutputDeltaT).Value);

            Assert.IsTrue((bool)modelDefinition.GetModelProperty(GuiProperties.SpecifyWaqOutputInterval).Value);
            Assert.NotNull(modelDefinition.GetModelProperty(GuiProperties.WaqOutputDeltaT).Value);
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
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void WriteMduFile_UpdatesCoordinateSystemInNetFileIfNecessary()
        {
            const string netFileName = "bendprof_map.nc";
            const string outputDirName = "readWriteMdu";
            if (Directory.Exists(outputDirName))
            {
                Directory.Delete(outputDirName, true);
            }

            // setup
            var mduFilePath = TestHelper.GetTestFilePath(@"fm_files\fm_files.mdu");
            var modelName = Path.GetFileName(mduFilePath);

            string testDataFilePath = TestHelper.GetTestFilePath(@"output_mapfiles");
            var zmDfmZipFileName = "zm_dfm_map.zip";
            string zmDfmZipFilePath = Path.Combine(testDataFilePath, zmDfmZipFileName);

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                var mduFile = new MduFile();
                var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel(modelName);
                var area = convertedFileObjectsForFMModel.HydroArea;
                var modelDefinition = convertedFileObjectsForFMModel.ModelDefinition;
                var allFixedWeirsAndCorrespondingProperties = convertedFileObjectsForFMModel.AllFixedWeirsAndCorrespondingProperties;
                
                mduFile.Read(mduFilePath, convertedFileObjectsForFMModel);

                // set coordinate system
                var coordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(4326); //wsg84
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
                mduFile.Write(outputDirName + @"/fm_files.mdu", modelDefinition, area, null, null, null, null, null, null, allFixedWeirsAndCorrespondingProperties);

                // read coordinate system from file
                var fileCoordinateSystem = NetFile.ReadCoordinateSystem(netFile);
                Assert.AreEqual(coordinateSystem.AuthorityCode, fileCoordinateSystem.AuthorityCode);
            });
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void WriteMduFile_UpdatesCoordinateSystemInNetFileIfNecessary_UGrid()
        {
            const string netFileName = "Custom_Ugrid.nc";
            const string outputDirName = "readWriteMdu";
            if (Directory.Exists(outputDirName)) Directory.Delete(outputDirName, true);

            // setup
            var mduFilePath = TestHelper.GetTestFilePath(@"fm_files\fm_files.mdu");
            var modelName = Path.GetFileName(mduFilePath);

            var mduFile = new MduFile();
            var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel(modelName);
            var modelDefinition = convertedFileObjectsForFMModel.ModelDefinition;
            var area = convertedFileObjectsForFMModel.HydroArea;
            var allFixedWeirsAndCorrespondingProperties = convertedFileObjectsForFMModel.AllFixedWeirsAndCorrespondingProperties;
            
            mduFile.Read(mduFilePath, convertedFileObjectsForFMModel);

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
            mduFile.Write(outputDirName+@"/fm_files.mdu", modelDefinition, area, null, null, null, null, null, null, allFixedWeirsAndCorrespondingProperties);

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
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void ReadAndWriteMduFile()
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"fm_files\fm_files.mdu");
            var modelName = Path.GetFileName(mduFilePath);

            var mduFile = new MduFile();
            var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel(modelName);
            var modelDefinition = convertedFileObjectsForFMModel.ModelDefinition;
            var area = convertedFileObjectsForFMModel.HydroArea;
            var allFixedWeirsAndCorrespondingProperties = convertedFileObjectsForFMModel.AllFixedWeirsAndCorrespondingProperties;
            
            mduFile.Read(mduFilePath, convertedFileObjectsForFMModel);

            Directory.CreateDirectory("readWriteMdu");
            mduFile.Write("readWriteMdu/fm_files.mdu", modelDefinition, area, null, null, null, null, null, null, allFixedWeirsAndCorrespondingProperties);

            var mduContent = File.ReadAllText("readWriteMdu/fm_files.mdu");
            var extForceFileContent = File.ReadAllText("readWriteMdu/fm_files.ext");
            Assert.IsTrue(extForceFileContent.Contains(
                "* FACTOR  =   : Conversion factor for this provider"));
            Assert.IsFalse(extForceFileContent.Contains(
                "* This comment line will be removed (shiptxy not yet supported and thus not read)"));
            Assert.IsTrue(mduContent.Contains(
                "! comment line on initial water level"));
            Assert.IsTrue(mduContent.Contains(
                "SomeNewFactor     = 3.7                 # new factor that should be read and written, but is not known"));
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void ReadLandBoundaryAndObservationsFile()
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"fm_files\fm_files.mdu");
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(mduFilePath);

            var mduFile = new MduFile();
            var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel(modelName);
            var modelDefinition = convertedFileObjectsForFMModel.ModelDefinition;
            
            mduFile.Read(mduFilePath, convertedFileObjectsForFMModel);

            var obsFileProperty = modelDefinition.GetModelProperty(KnownProperties.ObsFile);
            var obsFilePath = MduFileHelper.GetSubfilePath(mduFilePath, obsFileProperty);
            Assert.AreEqual(Path.Combine(mduDir, "fm_files_obs.xyn"), obsFilePath, "obs file path");
            var obsFile = new Feature2DPointFile<GroupableFeature2DPoint>();
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
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void ReadFixedWeirsWithMissingValuesFile()
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"fm_files\fm_files.mdu");
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(mduFilePath);

            var mduFile = new MduFile();
            var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel(modelName);
            var modelDefinition = convertedFileObjectsForFMModel.ModelDefinition;
            
            mduFile.Read(mduFilePath, convertedFileObjectsForFMModel);

            var fixedWeirFileProperty = modelDefinition.GetModelProperty(KnownProperties.FixedWeirFile);
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
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void ReadExtForceFileAndPlifiles()
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"fm_files\fm_files.mdu");
            var modelName = Path.GetFileName(mduFilePath);

            var mduFile = new MduFile();
            var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel(modelName);
            var modelDefinition = convertedFileObjectsForFMModel.ModelDefinition;
            
            mduFile.Read(mduFilePath, convertedFileObjectsForFMModel);

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
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void ReadAndWriteOutputSettings()
        {
            var mduDir =
                Path.Combine(TestHelper.GetTestDataDirectory(), "simpleBox");

            var mduFile = new MduFile();
            var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel("simplebox");
            var modelDefinition = convertedFileObjectsForFMModel.ModelDefinition;
            var area = convertedFileObjectsForFMModel.HydroArea;
            var network = convertedFileObjectsForFMModel.HydroNetwork;
            var allFixedWeirsAndCorrespondingProperties = convertedFileObjectsForFMModel.AllFixedWeirsAndCorrespondingProperties;
            
            mduFile.Read(Path.Combine(mduDir, "simplebox.mdu"), convertedFileObjectsForFMModel);

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
            mduFile.Write(mduFileSaveToPath, modelDefinition, area, null, null, null, null, null, null, allFixedWeirsAndCorrespondingProperties);

            var savedMduFile = new MduFile();
            var convertedFileObjectsForFMModelSaved = CreateConvertedFileObjectsForFMModel("simplebox");
            var savedModelDefinition = convertedFileObjectsForFMModelSaved.ModelDefinition;
            convertedFileObjectsForFMModelSaved.HydroArea = area;
            convertedFileObjectsForFMModelSaved.HydroNetwork = network;
            convertedFileObjectsForFMModelSaved.AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties;
            
            savedMduFile.Read(Path.Combine(saveToDir, "simplebox.mdu"), convertedFileObjectsForFMModelSaved);

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
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void ReadAndWriteModelDefinitionIvkModel()
        {
            var mduDir = Path.Combine(TestHelper.GetTestDataDirectory(), "mdu_ivoorkust");

            var mduFile = new MduFile();
            var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel("ivk");
            var modelDefinition = convertedFileObjectsForFMModel.ModelDefinition;
            var area = convertedFileObjectsForFMModel.HydroArea;
            var allFixedWeirsAndCorrespondingProperties = convertedFileObjectsForFMModel.AllFixedWeirsAndCorrespondingProperties;
            
            mduFile.Read(Path.Combine(mduDir, "ivk.mdu"), convertedFileObjectsForFMModel);

            const string saveToDir = "readWriteIvk";
            Directory.CreateDirectory(saveToDir);

            var mduFileSaveToPath = Path.Combine(saveToDir,"ivk.mdu");
            mduFile.Write(mduFileSaveToPath, modelDefinition, area, null, null, null, null, null, null, allFixedWeirsAndCorrespondingProperties);

            var mduContent = File.ReadAllText(mduFileSaveToPath);
            Assert.IsTrue(mduContent.Contains("TStart            = 504                 # Start time w.r.t. RefDate (in TUnit)"));
            Assert.IsTrue(mduContent.Contains("HisInterval       = 600                 # Interval (s) between history outputs"));
            Assert.IsTrue(mduContent.Contains("! for now, no Smag."));

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
            Assert.IsFalse(extFileContent.Contains("! this comment inside the block will not be kept"));
            Assert.IsTrue(extFileContent.Contains("* this comment block should still be there"));
            Assert.IsTrue(extFileContent.Contains("* this one too"));
            Assert.IsTrue(extFileContent.Contains("* and this one too"));
            Assert.IsTrue(extFileContent.Contains("* and finally this one"));
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void ReadAndWriteModelDefinitionC010TimeSeries()
        {
            var mduDir = Path.Combine(TestHelper.GetTestDataDirectory(), @"data\f05_boundary_conditions\c010_time_series\input");

            var mduFile = new MduFile();
            var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel("boundcond_test");
            var modelDefinition = convertedFileObjectsForFMModel.ModelDefinition;
            var area = convertedFileObjectsForFMModel.HydroArea;
            var allFixedWeirsAndCorrespondingProperties = convertedFileObjectsForFMModel.AllFixedWeirsAndCorrespondingProperties;
            
            mduFile.Read(Path.Combine(mduDir, "boundcond_test.mdu"), convertedFileObjectsForFMModel);

            const string saveToDir = "readWriteC010";
            Directory.CreateDirectory(saveToDir);

            var mduFileSaveToPath = Path.Combine(saveToDir, "boundcond_test.mdu");
            mduFile.Write(mduFileSaveToPath, modelDefinition, area, null, null, null, null, null, null, allFixedWeirsAndCorrespondingProperties);

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
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void ReadModelDefinitionC075Frictiontypes()
        {
            var mduDir =
                Path.Combine(TestHelper.GetTestDataDirectory(), @"c075_Frictiontypes");

            var mduFile = new MduFile();
            var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel("frictiontypes");
            var modelDefinition = convertedFileObjectsForFMModel.ModelDefinition;
            
            mduFile.Read(Path.Combine(mduDir, "frictiontypes.mdu"), convertedFileObjectsForFMModel);
            Assert.IsTrue(modelDefinition.SpatialOperations.ContainsKey(WaterFlowFMModelDefinition.RoughnessDataItemName));
            Assert.AreEqual(2, modelDefinition.SpatialOperations[WaterFlowFMModelDefinition.RoughnessDataItemName].Count);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void ReadWriteModelDefinitionHarlingenAndCheckAstroComponents()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            var mduFile = new MduFile();
            var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel("original");
            var modelDefinition = convertedFileObjectsForFMModel.ModelDefinition;
            var area = convertedFileObjectsForFMModel.HydroArea;
            var allFixedWeirsAndCorrespondingProperties = convertedFileObjectsForFMModel.AllFixedWeirsAndCorrespondingProperties;
            
            mduFile.Read(mduPath, convertedFileObjectsForFMModel);

            var exportedBoundary =
                modelDefinition.BoundaryConditions.First(
                    bc => bc.DataType == BoundaryConditionDataType.Harmonics);

            var firstPoint = exportedBoundary.DataPointIndices.FirstOrDefault();

            var exportedHarmonics = exportedBoundary.GetDataAtPoint(firstPoint);

            string mduExportPath = "har_export.mdu";
            mduExportPath = Path.Combine(Directory.GetCurrentDirectory(), mduExportPath);
            mduFile.Write(mduExportPath, modelDefinition, area, null, null, null,null, null, null, allFixedWeirsAndCorrespondingProperties);

            var convertedFileObjectsForFMModelReimport = CreateConvertedFileObjectsForFMModel("exported");
            var modelDefinitionReimport = convertedFileObjectsForFMModel.ModelDefinition;
            
            new MduFile().Read(mduExportPath, convertedFileObjectsForFMModelReimport);

            var boundaryCondition = modelDefinitionReimport.BoundaryConditions.First(bc => bc.DataType == BoundaryConditionDataType.Harmonics && bc.ProcessName == "Flow");
            
            firstPoint = boundaryCondition.DataPointIndices.FirstOrDefault();

            var importedHarmonics = boundaryCondition.GetDataAtPoint(firstPoint);

            Assert.AreEqual(exportedHarmonics.Arguments[0].Values[0], importedHarmonics.Arguments[0].Values[0], "period");
            Assert.AreEqual(exportedHarmonics.Components[0].Values[0], importedHarmonics.Components[0].Values[0], "amplitude");
            Assert.AreEqual(exportedHarmonics.Components[1].Values[0], importedHarmonics.Components[1].Values[0], "phase");
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ReadMduAndVerifyIsEnabled()
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"fm_files\fm_files.mdu");
            var modelName = Path.GetFileName(mduFilePath);

            var mduFile = new MduFile();
            var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel(modelName);
            var modelDefinition = convertedFileObjectsForFMModel.ModelDefinition;
            
            mduFile.Read(mduFilePath, convertedFileObjectsForFMModel);

            var useSalinity = modelDefinition.GetModelProperty(KnownProperties.UseSalinity);
            useSalinity.Value = true;

            var limtypsa = modelDefinition.GetModelProperty(KnownProperties.Limtypsa);
            Assert.IsTrue(limtypsa.IsEnabled(modelDefinition.Properties));

            useSalinity.Value = false;
            Assert.IsFalse(limtypsa.IsEnabled(modelDefinition.Properties));
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void PropertyChangedEventsAreBubbledForModelProperties()
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"fm_files\fm_files.mdu");
            var modelName = Path.GetFileName(mduFilePath);

            var mduFile = new MduFile();
            var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel(modelName);
            var modelDefinition = convertedFileObjectsForFMModel.ModelDefinition;
            
            mduFile.Read(mduFilePath, convertedFileObjectsForFMModel);

             var useSalinity = modelDefinition.GetModelProperty(KnownProperties.UseSalinity);
            useSalinity.Value = true;

            var limtypsa = modelDefinition.GetModelProperty(KnownProperties.Limtypsa);
            Assert.IsTrue(limtypsa.IsEnabled(modelDefinition.Properties));

            useSalinity.Value = false;
            Assert.IsFalse(limtypsa.IsEnabled(modelDefinition.Properties));
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void SettingUseMorSedShouldWriteSedimentSection()
        {
            string testData = TestHelper.GetTestFilePath(@"fm_files\fm_files.mdu");
            
            using (var tempDir = new TemporaryDirectory())
            {
                string mduFilePath = tempDir.CopyTestDataFileAndDirectoryToTempDirectory(testData);

                var model = new WaterFlowFMModel(mduFilePath);

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

                var otherModel = new WaterFlowFMModel(mduFilePath);

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
        [NUnit.Framework.Category(TestCategory.Integration)]
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
        [NUnit.Framework.Category(TestCategory.Integration)]
        [NUnit.Framework.Category(TestCategory.Slow)]
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
        [NUnit.Framework.Category(TestCategory.DataAccess)]
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
        public void SelectSpatialOperationsSetsUniqueOperationNameTest()
        {
            var model = new WaterFlowFMModel();
            
            var importSamplesOperation = new ImportSamplesOperationImportData { Name = "some_name" };

            var valueConverter = SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(
                model.DataItems.First(x => x.Name == "Bed Level"));
            valueConverter.SpatialOperationSet.Operations.Add(importSamplesOperation.CreateOperations().Second);
            valueConverter.SpatialOperationSet.Operations.Add(importSamplesOperation.CreateOperations().Second);
                
            model.ModelDefinition.SelectSpatialOperations(model.DataItems, model.TracerDefinitions);

            var spatialOperations = model.ModelDefinition.GetSpatialOperations("Bed Level");

            Assert.That(spatialOperations, Has.One.Matches<ISpatialOperation>(x => x.Name == "some_name"));
            Assert.That(spatialOperations, Has.One.Matches<ISpatialOperation>(x => x.Name == "some_name_1"));
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
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
        [NUnit.Framework.Category(TestCategory.Integration)]
        [NUnit.Framework.Category(TestCategory.Slow)]
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

            var modelName = Path.GetFileName(mduFilePath);

            var mduFile = new MduFile();
            var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel(modelName);
            var area = convertedFileObjectsForFMModel.HydroArea;
            
            mduFile.Read(mduFilePath, convertedFileObjectsForFMModel);
            Assert.IsTrue(area.Enclosures.Count == 1);
        }

        [Test]
        public void Read3EnclosuresWithSameNameFromFile()
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"enclosureFiles\threeEnclosuresDifferentName.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);

            var modelName = Path.GetFileName(mduFilePath);

            var mduFile = new MduFile();
            var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel(modelName);
            var area = convertedFileObjectsForFMModel.HydroArea;
            
            area.Enclosures.Add(
                FlowFMTestHelper.CreateFeature2DPolygonFromGeometry(
                    "Enclosure01", 
                    FlowFMTestHelper.GetValidGeometryForEnclosureExample()));
            
            mduFile.Read(mduFilePath, convertedFileObjectsForFMModel);
            Assert.IsTrue(area.Enclosures.Count == 4);
        }

        [Test]
        public void Read3EnclosuresWithDifferentNameFromFile()
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"enclosureFiles\threeEnclosuresDifferentName.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);

            var modelName = Path.GetFileName(mduFilePath);

            var mduFile = new MduFile();
            var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel(modelName);
            var area = convertedFileObjectsForFMModel.HydroArea;
            
            area.Enclosures.Add(
                FlowFMTestHelper.CreateFeature2DPolygonFromGeometry(
                    "Enclosure01",
                    FlowFMTestHelper.GetValidGeometryForEnclosureExample()));
            
            mduFile.Read(mduFilePath, convertedFileObjectsForFMModel);
            Assert.IsTrue(area.Enclosures.Count == 4);
        }

        [Test]
        public void WriteEnclosureFile()
        {
            var nameWithoutExtension = Path.GetTempFileName();
            var mduFilePath = String.Concat(nameWithoutExtension, ".mdu");
            var encFilePath = TestHelper.GetTestFilePath(String.Concat(nameWithoutExtension,"_enc.pol"));

            try
            {
                var featureName = "EnclosureFeature";
                var mduFile = new MduFile();

                var area = new HydroArea();
                var modelDefinition = new WaterFlowFMModelDefinition(Path.GetFileName(mduFilePath));
                var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
                //Add an enclosure.
                var enclosureGeom = FlowFMTestHelper.GetValidGeometryForEnclosureExample();
                var newEnclosure =
                    FlowFMTestHelper.CreateFeature2DPolygonFromGeometry(featureName, enclosureGeom);
                area.Enclosures.Add(newEnclosure);
                Assert.AreEqual(1, area.Enclosures.Count);

                mduFile.Write(mduFilePath, modelDefinition, area, null, null, null, null, null,null, allFixedWeirsAndCorrespondingProperties);

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
            var modelName = Path.GetFileName(mduFilePath);
            var encFilePath = TestHelper.GetTestFilePath(String.Concat(nameWithoutExtension, "_enc.pol"));

            try
            {
                var featureName = "EnclosureFeature";
                var mduFile = new MduFile();

                Assert.IsFalse(File.Exists(mduFilePath));
                Assert.IsFalse(File.Exists(encFilePath));

                /**/
                var area = new HydroArea();
                var modelDefinition = new WaterFlowFMModelDefinition(Path.GetFileName(mduFilePath));
                var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
                //Add an enclosure.
                var enclosureGeom = FlowFMTestHelper.GetValidGeometryForEnclosureExample();
                var newEnclosure =
                    FlowFMTestHelper.CreateFeature2DPolygonFromGeometry(featureName, enclosureGeom);
                area.Enclosures.Add(newEnclosure);
                Assert.AreEqual(1, area.Enclosures.Count);

                mduFile.Write(mduFilePath, modelDefinition, area, null, null, null, null, null,null, allFixedWeirsAndCorrespondingProperties);

                Assert.IsTrue(File.Exists(mduFilePath));
                Assert.IsTrue(File.Exists(encFilePath));
                /**/

                var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel(modelName);
                var readArea = convertedFileObjectsForFMModel.HydroArea;
                
                mduFile.Read(mduFilePath, convertedFileObjectsForFMModel);

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
            var modelDefinition = new WaterFlowFMModelDefinition(Path.GetFileName(mduFilePath));

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

        [TestCase(KnownProperties.SedFile,"Sediment")]
        [TestCase(KnownProperties.morphology,"Morphology")]

        public void Test_GetTabName_WithValidKeysAndModel_ExpectedTabNamesAreGiven(string key, string expectedName)
        {
            var tabName = string.Empty;
            var modelDefinition = new WaterFlowFMModelDefinition();
            Assert.DoesNotThrow(() =>
                {
                    TestHelper.AssertLogMessagesCount(
                        () => tabName = modelDefinition.GetTabName(key, fmModel:new WaterFlowFMModel()), 0);
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
            var modelDefinition = new WaterFlowFMModelDefinition();

            Assert.DoesNotThrow(() =>
                {
                    TestHelper.AssertLogMessagesCount(
                        () => tabName = modelDefinition.GetTabName(key), 0);
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
            var modelDefinition = new WaterFlowFMModelDefinition();

            Assert.DoesNotThrow(() =>
            {
                TestHelper.AssertAtLeastOneLogMessagesContains(
                    () => tabName = modelDefinition.GetTabName(key, message),
                    expectedMessage);
            });

            Assert.NotNull(tabName);
            Assert.AreEqual(tabName, expectedName);
        }

        [Test]
        public void Test_GuiPropertyGroups_GetUniqueGuiPropertyGroupsFromModelAndMorphologyPropertyGroups()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();
            Dictionary<string, ModelPropertyGroup> dummyVar;
            Assert.DoesNotThrow(() => dummyVar = modelDefinition.GuiPropertyGroups );
         }
        
        [Test]
        public void ConvertSpatialOperation_InterpolateOperation_ReturnsCorrectImportSamplesSpatialOperationExtension(
            [Values] bool enabled,
            [Values] SpatialInterpolationMethod interpolationMethod,
            [Values] GridCellAveragingMethod averagingMethod,
            [Values] PointwiseOperationType operand)
        {
            const string name = "some_name";
            const string filePath = "some_file_path";
            const double relativeSearchSize = 1.23;
            const int minSamplePoints = 4;

            var interpolateOperation = new InterpolateOperation
            {
                InterpolationMethod = interpolationMethod,
                GridCellAveragingMethod = averagingMethod,
                RelativeSearchCellSize = relativeSearchSize,
                MinNumSamples = minSamplePoints,
                OperationType = operand
            };
            var importSamplesOperation = new ImportSamplesOperation(false)
            {
                Name = name,
                FilePath = filePath,
                Enabled = enabled
            };
            
            var source = Substitute.For<ISpatialOperationData, INotifyPropertyChanged>();
            source.Operation = importSamplesOperation;
            
            var input = interpolateOperation.GetInput(InterpolateOperation.InputSamplesName);
            input.Source = source;
            interpolateOperation.Inputs.Add(input);

            // Call
            var convertedOperation = WaterFlowFMModelDefinition.ConvertSpatialOperation(interpolateOperation) as ImportSamplesOperationImportData;
            
            // Assert
            Assert.That(convertedOperation, Is.Not.Null);
            Assert.That(convertedOperation.Name, Is.EqualTo(name));
            Assert.That(convertedOperation.FilePath, Is.EqualTo(filePath));
            Assert.That(convertedOperation.Enabled, Is.EqualTo(enabled));
            Assert.That(convertedOperation.InterpolationMethod, Is.EqualTo(interpolationMethod));
            Assert.That(convertedOperation.AveragingMethod, Is.EqualTo(averagingMethod));
            Assert.That(convertedOperation.RelativeSearchCellSize, Is.EqualTo(relativeSearchSize));
            Assert.That(convertedOperation.MinSamplePoints, Is.EqualTo(minSamplePoints));
            Assert.That(convertedOperation.Operand, Is.EqualTo(operand));
        }
        
        [TestCase(WaterFlowFMModelDefinition.BathymetryDataItemName, "Bed Level")]
        [TestCase(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName, "Initial Water Level")]
        [TestCase(WaterFlowFMModelDefinition.InitialWaterDepthDataItemName, "Initial Water Depth")]
        [TestCase(WaterFlowFMModelDefinition.InitialSalinityDataItemName, "Initial Salinity")]
        [TestCase(WaterFlowFMModelDefinition.InitialTemperatureDataItemName, "Initial Temperature")]
        [TestCase(WaterFlowFMModelDefinition.RoughnessDataItemName, "Roughness")]
        [TestCase(WaterFlowFMModelDefinition.ViscosityDataItemName, "Viscosity")]
        [TestCase(WaterFlowFMModelDefinition.DiffusivityDataItemName, "Diffusivity")]
        [TestCase(WaterFlowFMModelDefinition.InfiltrationDataItemName, "Infiltration")]
        public void ConstantFields(string actual, string expected)
        {
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void SpatialDataItemNames_ContainsCorrectValues()
        {
            // Call
            string[] names = WaterFlowFMModelDefinition.SpatialDataItemNames;
            
            // Assert
            Assert.That(names, Has.Length.EqualTo(9));
            Assert.That(names.Contains("Bed Level"));
            Assert.That(names.Contains("Initial Water Level"));
            Assert.That(names.Contains("Initial Water Depth"));
            Assert.That(names.Contains("Initial Salinity"));
            Assert.That(names.Contains("Initial Salinity"));
            Assert.That(names.Contains("Initial Temperature"));
            Assert.That(names.Contains("Viscosity"));
            Assert.That(names.Contains("Diffusivity"));
            Assert.That(names.Contains("Infiltration"));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void SetModelProperty_PropertyNameNull_ThrowsArgumentNullException(string propertyName)
        {
            // Setup
            var modelDefinition = new WaterFlowFMModelDefinition();

            // Call
            void Call() => modelDefinition.SetModelProperty(propertyName, 1.23d);

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("propertyName"));

        }
        
        [Test]
        public void SetModelProperty_SetsPropertyValue()
        {
            // Setup
            var propertyDefinition = new WaterFlowFMPropertyDefinition
            {
                MduPropertyName = "some_property",
                DataType = typeof(double)
            };
            var property = new WaterFlowFMProperty(propertyDefinition, "1.23");
            var modelDefinition = new WaterFlowFMModelDefinition();
            modelDefinition.AddProperty(property);
            
            // Call
            modelDefinition.SetModelProperty("some_property", 4.56d);

            // Assert
            Assert.That(property.Value, Is.EqualTo(4.56d));
        }

        [Test]
        public void SetRefDateFromDateTime_ExtractsDatePart()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();
            var dateTime = new DateTime(2023, 05, 23, 01, 02, 03);
            var datePart = DateOnly.FromDateTime(dateTime).ToDateTime(TimeOnly.MinValue);

            modelDefinition.SetReferenceDateFromDatePartOfDateTime(dateTime);
            
            Assert.That( modelDefinition.GetReferenceDateAsDateTime(), Is.EqualTo(datePart) );
        }
        
        private static ConvertedFileObjectsForFMModel CreateConvertedFileObjectsForFMModel(string modelName)
        {
            return new ConvertedFileObjectsForFMModel
            {
                HydroArea = new HydroArea(),
                HydroNetwork = new HydroNetwork(),
                ModelDefinition = new WaterFlowFMModelDefinition(modelName),
                BoundaryConditions1D = new EventedList<Model1DBoundaryNodeData>(),
                LateralSourcesData = new EventedList<Model1DLateralSourceData>(),
                AllFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>(),
                AllBridgePillarsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<BridgePillar>>(),
                RoughnessSections = new EventedList<RoughnessSection>(),
                ChannelFrictionDefinitions = new EventedList<ChannelFrictionDefinition>(),
                ChannelInitialConditionDefinitions = new EventedList<ChannelInitialConditionDefinition>()
            };
        }
    }
}
