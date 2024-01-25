using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Serialization;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using DHYDRO.Common.IO.Ini;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap;
using SharpMap.Api.SpatialOperations;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class ExtForceFileTest
    {
        [Test]
        [Category(TestCategory.Slow)]
        public void ReadPolygonForcings()
        {
            var def = new WaterFlowFMModelDefinition();
            string extPath = TestHelper.GetTestFilePath(@"harlingen\001.ext");
            string extSubFilesReferenceFilePath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            var extForceFile = new ExtForceFile();
            extForceFile.Read(extPath, def, extSubFilesReferenceFilePath);

            //extForceFile.ImportSpatialOperations(extPath, def);

            Assert.IsNull(def.GetSpatialOperations(WaterFlowFMModelDefinition.ViscosityDataItemName));
            Assert.IsNull(def.GetSpatialOperations(WaterFlowFMModelDefinition.DiffusivityDataItemName));
            Assert.IsNull(def.GetSpatialOperations(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName));
            Assert.AreEqual(2, def.GetSpatialOperations(WaterFlowFMModelDefinition.InitialSalinityDataItemName).Count);

            IList<ISpatialOperation> roughnessOperations = def.GetSpatialOperations(WaterFlowFMModelDefinition.RoughnessDataItemName);
            Assert.IsNotNull(roughnessOperations);
            Assert.AreEqual(1, roughnessOperations.Count);

            var firstOperation = roughnessOperations.First() as SetValueOperation;
            Assert.IsNotNull(firstOperation);

            Assert.AreEqual(PointwiseOperationType.Overwrite, firstOperation.OperationType);
            Assert.AreEqual(0.04, firstOperation.Value); //undefined

            var secondInitialSalinityOperation = (SetValueOperation) def.GetSpatialOperations(WaterFlowFMModelDefinition.InitialSalinityDataItemName)[1];
            Assert.AreEqual(PointwiseOperationType.Add, secondInitialSalinityOperation.OperationType);
            Assert.AreEqual(10.0, secondInitialSalinityOperation.Value); //undefined
        }

        [Test]
        public void ReadSampleForcings()
        {
            var def = new WaterFlowFMModelDefinition();
            string extPath = TestHelper.GetTestFilePath(@"chezy_samples\chezy.ext");
            string extSubFilesReferenceFilePath = TestHelper.GetTestFilePath(@"chezy_samples\chezy.mdu");

            var extForceFile = new ExtForceFile();
            extForceFile.Read(extPath, def, extSubFilesReferenceFilePath);

            Assert.IsNull(def.GetSpatialOperations(WaterFlowFMModelDefinition.ViscosityDataItemName));
            Assert.IsNull(def.GetSpatialOperations(WaterFlowFMModelDefinition.DiffusivityDataItemName));
            Assert.IsNull(def.GetSpatialOperations(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName));
            Assert.AreEqual(1, def.GetSpatialOperations(WaterFlowFMModelDefinition.RoughnessDataItemName).Count);

            IList<ISpatialOperation> roughnessOperations = def.GetSpatialOperations(WaterFlowFMModelDefinition.RoughnessDataItemName);

            Assert.IsTrue(roughnessOperations[0] is ImportSamplesOperation);

            var sampleDef = (ImportSamplesOperation) roughnessOperations[0];
            Assert.AreEqual("chezy", sampleDef.Name);
        }

        [Test]
        public void GivenAnExtForceFileWithUnknownSpatiallyVaryingProperties_WhenRead_ThenCorrectWarningMessageIsGiven()
        {
            // Given
            string extPath = TestHelper.GetTestFilePath(@"SpatialVaryingPrefix\incorrect_prefix.ext");
            extPath = TestHelper.CreateLocalCopy(extPath);
            string extSubFilesReferenceFilePath = Path.Combine(Path.GetDirectoryName(extPath), "incorrect_prefix.mdu");
            Assert.IsTrue(File.Exists(extPath));

            // When, Then
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => new ExtForceFile().Read(extPath, new WaterFlowFMModelDefinition(), extSubFilesReferenceFilePath),
                string.Format(Resources.ExtForceFile_StoreUnknownQuantities_Quantity___0___detected_in_the_external_force_file_and_will_be_passed_to_the_computational_core__This_may_affect_your_simulation_,
                              "initialspatialvaryingsedimentSediment_sand_SedConc"));

            FileUtils.DeleteIfExists(extPath);
        }

        [Test]
        public void ReadExtFileWithUnknownQuantityShowsLogMessage()
        {
            var def = new WaterFlowFMModelDefinition();
            string extPath = TestHelper.GetTestFilePath(@"ExtFileTest\withOnlyUnknownQuantity.ext");
            string extSubFilesReferenceFilePath = Path.Combine(Path.GetDirectoryName(extPath), "withOnlyUnknownQuantity.mdu");
            Assert.IsTrue(File.Exists(extPath));

            extPath = TestHelper.CreateLocalCopy(extPath);
            Assert.IsTrue(File.Exists(extPath));

            string expectedMessage = string.Format(Resources.ExtForceFile_StoreUnknownQuantities_Quantity___0___detected_in_the_external_force_file_and_will_be_passed_to_the_computational_core__This_may_affect_your_simulation_, "generalstructure");
            var extForceFile = new ExtForceFile();
            TestHelper.AssertAtLeastOneLogMessagesContains(() => extForceFile.Read(extPath, def, extSubFilesReferenceFilePath), expectedMessage);
        }

        [Test]
        public void ReadExtFileWithUnknownQuantityImportsTheOtherQuantities()
        {
            var def = new WaterFlowFMModelDefinition();
            string extPath = TestHelper.GetTestFilePath(@"ExtFileTest\withUnknownAndKnownQuantities.ext");
            string extSubFilesReferenceFilePath = Path.Combine(Path.GetDirectoryName(extPath), "withUnknownAndKnownQuantities.mdu");
            Assert.IsTrue(File.Exists(extPath));

            extPath = TestHelper.CreateLocalCopy(extPath);
            Assert.IsTrue(File.Exists(extPath));

            string expectedMessage = string.Format(Resources.ExtForceFile_StoreUnknownQuantities_Quantity___0___detected_in_the_external_force_file_and_will_be_passed_to_the_computational_core__This_may_affect_your_simulation_, "generalstructure");
            var extForceFile = new ExtForceFile();

            Assert.IsFalse(def.BoundaryConditions.Any());
            TestHelper.AssertAtLeastOneLogMessagesContains(() => extForceFile.Read(extPath, def, extSubFilesReferenceFilePath), expectedMessage);
            Assert.IsTrue(def.BoundaryConditions.Any());

            /* Just check the boundary has been imported. */
            IBoundaryCondition boundaryCondition = def.BoundaryConditions.First();
            Assert.AreEqual("WaterLevel", boundaryCondition.VariableName);
            Assert.AreEqual("OB_001_orgsize-Water level", boundaryCondition.Name);
        }

        [Test]
        public void ReadExtFileWithFileReferencesInDifferentFoldersWritesToExtFileFolder()
        {
            var extForceFile = new ExtForceFile();
            var modelDefinition = new WaterFlowFMModelDefinition();

            using (var sourceDir = new TemporaryDirectory())
            using (var targetDir = new TemporaryDirectory())
            {
                string testFilesDir = TestHelper.GetTestFilePath(@"ExtFileTest\ExtFileReferencesInDifferentFolders");
                string sourceFilesDir = sourceDir.CopyDirectoryToTempDirectory(testFilesDir);

                string sourceExtFilePath = Path.Combine(sourceFilesDir, @"computations\WithKnownAndUnknownQuantities.ext");
                string sourceMduFilePath = Path.Combine(sourceFilesDir, @"computations\EmptyMduFile.mdu");

                extForceFile.Read(sourceExtFilePath, modelDefinition, sourceMduFilePath);

                string targetFilesDir = targetDir.Path;
                string targetExtFilePath = Path.Combine(targetFilesDir, @"WithKnownAndUnknownQuantities.ext");
                string targetPliFilePath = Path.Combine(targetFilesDir, @"OB_001_orgsize.pli");
                string targetNcFilePath = Path.Combine(targetFilesDir, @"RAD_NL25_RAC_MFBS_5min.nc");
                string targetPolFilePath = Path.Combine(targetFilesDir, @"surroundingDomain.pol");

                extForceFile.Write(targetExtFilePath, modelDefinition, true, true);

                Assert.That(targetExtFilePath, Does.Exist);
                Assert.That(targetPliFilePath, Does.Exist);
                Assert.That(targetNcFilePath, Does.Exist);
                Assert.That(targetPolFilePath, Does.Exist);
            }
        }

        [Test]
        public void GivenAnExtFileWithAnUnknownQuantity_WhenImportingItAndExportingIt_ThenThisQuantityShouldBeReadAndWritten()
        {
            var def = new WaterFlowFMModelDefinition();
            string extPath = TestHelper.GetTestFilePath(@"ExtFileTest\withKnownAndUnknownQuantities.ext");
            string extSubFilesReferenceFilePath = Path.Combine(Path.GetDirectoryName(extPath), "withKnownAndUnknownQuantities.mdu");
            Assert.IsTrue(File.Exists(extPath));

            extPath = TestHelper.CreateLocalCopy(extPath);
            Assert.IsTrue(File.Exists(extPath));

            var extForceFile = new ExtForceFile();
            string expectedMessage =
                string.Format(Resources.ExtForceFile_StoreUnknownQuantities_Quantity___0___detected_in_the_external_force_file_and_will_be_passed_to_the_computational_core__This_may_affect_your_simulation_, "internaltidesfrictioncoefficient");

            Assert.IsFalse(def.BoundaryConditions.Any());
            TestHelper.AssertAtLeastOneLogMessagesContains(() => extForceFile.Read(extPath, def, extSubFilesReferenceFilePath), expectedMessage);
            Assert.IsTrue(def.BoundaryConditions.Any());

            /* Just check the boundary has been imported. */
            IBoundaryCondition boundaryCondition = def.BoundaryConditions.First();
            Assert.AreEqual("WaterLevel", boundaryCondition.VariableName);
            Assert.AreEqual("OB_001_orgsize-Water level", boundaryCondition.Name);

            ValidateUnknownQuantities(def);

            Assert.That(File.Exists(Path.Combine(Path.GetDirectoryName(extPath), def.UnsupportedFileBasedExtForceFileItems[0].UnsupportedExtForceFileItem.FileName)));

            string newPath = Path.Combine(Path.GetDirectoryName(extPath), "NewExtFileDirectory", "NewExtFile");
            string newExtSubFilesReferenceFilePath = Path.Combine(Path.GetDirectoryName(extPath), "NewExtFileDirectory", "NewMduFile");

            extForceFile.Write(newPath, def, true, true); // write loaded definition to new location

            //Check
            Assert.That(File.Exists(Path.Combine(Path.GetDirectoryName(newPath), def.UnsupportedFileBasedExtForceFileItems[0].UnsupportedExtForceFileItem.FileName)));

            var newExtFile = new ExtForceFile();
            var newDef = new WaterFlowFMModelDefinition();

            newExtFile.Read(newPath, newDef, newExtSubFilesReferenceFilePath); // load written definition back
            ValidateUnknownQuantities(newDef);
        }

        [Test]
        public void GivenAnExtFileWithAnUnknownQuantity_WhenImportingAndCorrespondingFileIsMissing_ThenThisQuantityShouldBeImported()
        {
            // Given
            var modelDefinition = new WaterFlowFMModelDefinition();
            string extPath =
                TestHelper.GetTestFilePath(@"ExtFileTest\ExtFileWithInternalTidesFrictionCoefficientAndMissingFile\withKnownAndUnknownQuantities.ext");
            string extSubFilesReferenceFilePath = Path.Combine(Path.GetDirectoryName(extPath), "withKnownAndUnknownQuantities.mdu");
            Assert.IsTrue(File.Exists(extPath));

            extPath = TestHelper.CreateLocalCopy(extPath);
            Assert.IsTrue(File.Exists(extPath));

            var extForceFile = new ExtForceFile();
            string expectedMessage = string.Format(Resources.ExtForceFile_StoreUnknownQuantities_Quantity___0___detected_in_the_external_force_file_and_will_be_passed_to_the_computational_core__This_may_affect_your_simulation_, "internaltidesfrictioncoefficient");

            Assert.IsFalse(modelDefinition.BoundaryConditions.Any());

            // When
            TestHelper.AssertLogMessageIsGenerated(() => extForceFile.Read(extPath, modelDefinition, extSubFilesReferenceFilePath), expectedMessage);

            // Then
            ValidateUnknownQuantities(modelDefinition);
            Assert.IsTrue(modelDefinition.BoundaryConditions.Any());
            IBoundaryCondition boundaryCondition = modelDefinition.BoundaryConditions.First();
            Assert.AreEqual("WaterLevel", boundaryCondition.VariableName);
            Assert.AreEqual("OB_001_orgsize-Water level", boundaryCondition.Name);
        }
        
        [Test]
        public void Read_ExtFileWithVarName()
        {
            // Given
            var modelDefinition = new WaterFlowFMModelDefinition();
            string extPath =
                TestHelper.GetTestFilePath(@"ExtFileTest\with_varname.ext");
            Assert.IsTrue(File.Exists(extPath));

            extPath = TestHelper.CreateLocalCopy(extPath);
            Assert.IsTrue(File.Exists(extPath));
            
            Assert.IsFalse(modelDefinition.UnsupportedFileBasedExtForceFileItems.Any());
            
            //action read
            var extForceFile = new ExtForceFile();
            var expectedMessage = @"Quantity 'solarradiation' detected in the external force file and will be passed to the computational core. This may affect your simulation.";
            TestHelper.AssertLogMessageIsGenerated(() => extForceFile.Read(extPath, modelDefinition, ""), expectedMessage);

            // check read
            Assert.IsTrue(modelDefinition.UnsupportedFileBasedExtForceFileItems.Any());
            var unsupportedExternalForceItem = modelDefinition.UnsupportedFileBasedExtForceFileItems.First();
            Assert.IsNotNull(unsupportedExternalForceItem.UnsupportedExtForceFileItem);
            Assert.AreEqual("solarradiation", unsupportedExternalForceItem.UnsupportedExtForceFileItem.Quantity);
            Assert.AreEqual("ssr", unsupportedExternalForceItem.UnsupportedExtForceFileItem.VarName);
        }

        [Test]
        public void Write_ExtFileWithVarName()
        {
            //action write
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                //given
               
                var quantity = "solarradiation";
                var filename = @"/p/1204257-dcsmzuno/data/meteo/ERA5/nc/ERA5_2005-2021_dfm_ssr_strd.nc";
                var varname = "ssr";
                var filetype = 11;
                var method = 3;
                var operation = "O";
                
                var extForceFileItem = new ExtForceFileItem(quantity)
                {
                    FileName = filename,
                    VarName = varname,
                    FileType = filetype,
                    Method = method,
                    Operand = operation
                };
                
                //setup model
                var writeExtPath = Path.Combine(temporaryDirectory.Path, Path.GetFileName("with_varname_written.ext"));
                var modelDefinition = new WaterFlowFMModelDefinition();
                var extForceFile = new ExtForceFile();
                modelDefinition.UnsupportedFileBasedExtForceFileItems.Add(new UnsupportedFileBasedExtForceFileItem(writeExtPath,extForceFileItem));

                //write
                extForceFile.Write(writeExtPath, modelDefinition, false, false);

                // check written file
                var writtenModelDefinition = new WaterFlowFMModelDefinition();
                var writtenExtForceFile = new ExtForceFile();
                writtenExtForceFile.Read(writeExtPath, writtenModelDefinition, "");

                Assert.IsTrue(writtenModelDefinition.UnsupportedFileBasedExtForceFileItems.Any());
                var writtenUnsupportedExternalForceItem = writtenModelDefinition.UnsupportedFileBasedExtForceFileItems.First();
                Assert.IsNotNull(writtenUnsupportedExternalForceItem.UnsupportedExtForceFileItem);
                Assert.AreEqual(quantity, writtenUnsupportedExternalForceItem.UnsupportedExtForceFileItem.Quantity);
                Assert.AreEqual(varname, writtenUnsupportedExternalForceItem.UnsupportedExtForceFileItem.VarName);
            }
        }

        [Test]
        public void ReadCorrectSpatialVaryingPropertiesShouldBeOk()
        {
            //LogHelper.ConfigureLogging(|Level);
            var def = new WaterFlowFMModelDefinition();
            string extPath = TestHelper.GetTestFilePath(@"SpatialVaryingPrefix\correct_prefix.ext");
            string extSubFilesReferenceFilePath = Path.Combine(Path.GetDirectoryName(extPath), "correct_prefix.mdu");
            var extForceFile = new ExtForceFile();
            TestHelper.AssertLogMessagesCount(() => extForceFile.Read(extPath, def, extSubFilesReferenceFilePath), 0);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ExtFileDoesNotSaveSedimentSpatiallyVaryingOperationsButSedConc()
        {
            //define model
            string sedFile = Path.GetTempFileName();
            string extForceFile = Path.Combine(Path.GetDirectoryName(sedFile), "extForceFileTest.ext");
            string sedConcXyzFile = Path.Combine(Path.GetDirectoryName(sedFile), "mysedimentName_SedConc." + XyzFile.Extension);
            string customPropXyzFile = Path.Combine(Path.GetDirectoryName(sedFile), "mysedimentName_IniSedThick." + XyzFile.Extension);
            var fileCopyName = string.Empty;
            try
            {
                /* Define new model */
                var fmModel = new WaterFlowFMModel();
                fmModel.ImportFromMdu(sedFile);

                fmModel.ModelDefinition.UseMorphologySediment = true;
                fmModel.Grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);

                /* Define test properties */
                var doubleSpatProp = new SpatiallyVaryingSedimentProperty<double>("SedConc", 0, 0, false, 0, true, "cc", "mydoubledescription", true, false)
                {
                    SpatiallyVaryingName = "mysedimentName_SedConc",
                    Value = 12.3
                };
                var thickProp = new SpatiallyVaryingSedimentProperty<double>("IniSedThick", 5, 0, false, 0, true, "cc", "mydoubledescription", true, false)
                {
                    SpatiallyVaryingName = "mysedimentName_IniSedThick",
                    Value = 12.3
                };
                thickProp.IsSpatiallyVarying = true;

                /* Set sediment and formula properties */
                var testSedimentType = new SedimentType
                {
                    Key = "sand",
                    Properties = new EventedList<ISedimentProperty>
                    {
                        doubleSpatProp,
                        thickProp
                    }
                };

                var overallProp = new SedimentProperty<double>("Cref", 0, 0, true, 0, false, "km", "myoveralldescription", false) {Value = 80.1};

                fmModel.SedimentOverallProperties = new EventedList<ISedimentProperty> {overallProp};

                /*Add the fraction to the model*/
                var fraction = new SedimentFraction
                {
                    Name = "mysedimentName",
                    CurrentSedimentType = testSedimentType
                };

                fileCopyName = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"harlingen_model_3d\har_V3.xyz"));

                fmModel.SedimentOverallProperties = new EventedList<ISedimentProperty>();
                fmModel.SedimentFractions = new EventedList<ISedimentFraction>();
                fmModel.SedimentFractions.Add(fraction);

                /* Coverage for SedConc */
                IDataItem dataItem = fmModel.AllDataItems.FirstOrDefault(di => di.Name == "mysedimentName_SedConc");

                // retrieve / create value converter for mysedimentName_SedConc dataitem
                SpatialOperationSetValueConverter valueConverter = SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(dataItem, "mysedimentName_SedConc");
                var samplesSedConc = new AddSamplesOperation(false);
                samplesSedConc.SetInputData(AddSamplesOperation.SamplesInputName, new PointCloudFeatureProvider
                {
                    PointCloud = new PointCloud
                    {
                        PointValues = new List<IPointValue>
                        {
                            new PointValue
                            {
                                X = fmModel.Grid.Cells[0].CenterX,
                                Y = fmModel.Grid.Cells[0].CenterY,
                                Value = 12
                            },
                            new PointValue
                            {
                                X = fmModel.Grid.Cells[1].CenterX,
                                Y = fmModel.Grid.Cells[1].CenterY,
                                Value = 30
                            },
                            new PointValue
                            {
                                X = fmModel.Grid.Cells[2].CenterX,
                                Y = fmModel.Grid.Cells[2].CenterY,
                                Value = 31
                            }
                        }
                    }
                });
                valueConverter.SpatialOperationSet.AddOperation(samplesSedConc);
                valueConverter.SpatialOperationSet.Execute();

                /* Create coverage for CustomProp */
                IDataItem thickDataItem = fmModel.AllDataItems.FirstOrDefault(di => di.Name == "mysedimentName_IniSedThick");

                // retrieve / create value converter for mysedimentName_SedConc data item
                SpatialOperationSetValueConverter valueConvertThick = SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(thickDataItem, "mysedimentName_IniSedThick");
                var samplesThick = new AddSamplesOperation(false);
                samplesThick.SetInputData(AddSamplesOperation.SamplesInputName, new PointCloudFeatureProvider
                {
                    PointCloud = new PointCloud
                    {
                        PointValues = new List<IPointValue>
                        {
                            new PointValue
                            {
                                X = fmModel.Grid.Cells[0].CenterX,
                                Y = fmModel.Grid.Cells[0].CenterY,
                                Value = 2
                            },
                            new PointValue
                            {
                                X = fmModel.Grid.Cells[1].CenterX,
                                Y = fmModel.Grid.Cells[1].CenterY,
                                Value = 15
                            },
                            new PointValue
                            {
                                X = fmModel.Grid.Cells[2].CenterX,
                                Y = fmModel.Grid.Cells[2].CenterY,
                                Value = 28
                            }
                        }
                    }
                });
                valueConvertThick.SpatialOperationSet.AddOperation(samplesThick);
                valueConvertThick.SpatialOperationSet.Execute();

                // update model definition (called during export)
                var initialSpatialOps = new List<string>
                {
                    doubleSpatProp.SpatiallyVaryingName,
                    thickProp.SpatiallyVaryingName
                };
                fmModel.ModelDefinition.SelectSpatialOperations(fmModel.AllDataItems.ToList(), fmModel.TracerDefinitions, initialSpatialOps);

                // create an interpolate operation using the samples added earlier
                var intOpSedConc = new InterpolateOperation();
                intOpSedConc.SetInputData(InterpolateOperation.InputSamplesName, samplesSedConc.Output.Provider);
                Assert.IsNotNull(valueConverter.SpatialOperationSet.AddOperation(intOpSedConc));
                var intOpThick = new InterpolateOperation();
                intOpThick.SetInputData(InterpolateOperation.InputSamplesName, samplesThick.Output.Provider);
                Assert.IsNotNull(valueConvertThick.SpatialOperationSet.AddOperation(intOpThick));

                // update model definition (called during export)
                fmModel.ModelDefinition.SelectSpatialOperations(fmModel.AllDataItems.ToList(), fmModel.TracerDefinitions, initialSpatialOps);

                /* Save ext file */
                var extFile = new ExtForceFile();
                extFile.Write(extForceFile, fmModel.ModelDefinition, true, true);
                Assert.IsTrue(File.Exists(extForceFile));

                /* Check SedConc has generated only one Xyz File and one entry in the Ext file
                 * for SedConc but not for CustomProp.       */
                string extWritten = File.ReadAllText(extForceFile);
                Assert.That(extWritten, Does.Contain("QUANTITY=initialsedfracmysedimentName"));
                Assert.That(extWritten, Does.Contain("FILENAME=mysedimentName_SedConc.xyz"));
                /* Nothing related to the customProp */
                Assert.That(extWritten, Is.Not.Contains("mysedimentName_IniSedThick"));
                Assert.That(extWritten, Is.Not.Contains("IniSedThick"));

                Assert.IsTrue(File.Exists(sedConcXyzFile));
                Assert.IsFalse(File.Exists(customPropXyzFile));

                /* Save the sediments now and check for the xyz */
                SedimentFile.Save(sedFile, fmModel.ModelDefinition, fmModel);
                Assert.IsTrue(File.Exists(customPropXyzFile));
            }
            finally
            {
                FileUtils.DeleteIfExists(sedFile);
                FileUtils.DeleteIfExists(fileCopyName);
                FileUtils.DeleteIfExists(extForceFile);
                FileUtils.DeleteIfExists(sedConcXyzFile);
                FileUtils.DeleteIfExists(customPropXyzFile);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void SaveExtFileWithSpatiallyVaryingSedConcButNoOperationsGeneratesWarningMessage()
        {
            string sedFile = Path.GetTempFileName();
            string generatedXyzFile = Path.Combine(Path.GetDirectoryName(sedFile), "mysedimentName_SedConc." + XyzFile.Extension);
            string extForceFile = Path.Combine(Path.GetDirectoryName(sedFile), "extForceFileTest.ext");

            try
            {
                /* Define new model */
                UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);

                var fmModel = new WaterFlowFMModel();
                fmModel.ImportFromMdu(sedFile);

                fmModel.ModelDefinition.UseMorphologySediment = true;
                fmModel.Grid = grid;

                var fraction = new SedimentFraction {Name = "Frac1"};
                fmModel.SedimentFractions.Add(fraction);

                /* Save ext file */
                var extFile = new ExtForceFile();
                //Save SedFile with no fractions. No warnings should be given.
                TestHelper.AssertLogMessagesCount(() => extFile.Write(extForceFile, fmModel.ModelDefinition, true, true), 0);

                //Update model , we need to force it as we are not saving directly from the model but from the ModelDefinition
                ISpatiallyVaryingSedimentProperty sedConcProp = fraction.CurrentSedimentType.Properties.OfType<ISpatiallyVaryingSedimentProperty>().FirstOrDefault(p => p.Name == "SedConc");
                var initialSpatialOps = new List<string> {sedConcProp.SpatiallyVaryingName};
                //Add another spatially varying prop -> Warning should be given.
                fmModel.ModelDefinition.SelectSpatialOperations(fmModel.AllDataItems.ToList(), fmModel.TracerDefinitions, initialSpatialOps);
                TestHelper.AssertAtLeastOneLogMessagesContains(
                    () => extFile.Write(extForceFile, fmModel.ModelDefinition, true, true),
                    string.Format(
                        Resources.SedimentFile_WriteSpatiallyVaryingSedimentPropertySubFiles_No_spatial_operations_of_type_Import__Add_or_Value_found_for_spatially_varying_property__0___Remember_to_interpolate_them_to_generate_the_xyz_file__Otherwise_the_model_might_not_run_as_expected_,
                        sedConcProp.SpatiallyVaryingName));

                //Add a 'value' operation, another warning should be given.
                IDataItem dataItem = fmModel.AllDataItems.FirstOrDefault(di => di.Name == sedConcProp.SpatiallyVaryingName);

                // retrieve / create value converter for mysedimentName_SedConc dataitem
                SpatialOperationSetValueConverter valueConverter = SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(dataItem, sedConcProp.SpatiallyVaryingName);
                var samples = new SetValueOperation();
                samples.SetInputData(SpatialOperation.MainInputName, new PointCloudFeatureProvider
                {
                    PointCloud = new PointCloud
                    {
                        PointValues = new List<IPointValue>
                        {
                            new PointValue
                            {
                                X = fmModel.Grid.Cells[0].CenterX,
                                Y = fmModel.Grid.Cells[0].CenterY,
                                Value = 12
                            },
                            new PointValue
                            {
                                X = fmModel.Grid.Cells[1].CenterX,
                                Y = fmModel.Grid.Cells[1].CenterY,
                                Value = 30
                            },
                            new PointValue
                            {
                                X = fmModel.Grid.Cells[2].CenterX,
                                Y = fmModel.Grid.Cells[2].CenterY,
                                Value = 31
                            }
                        }
                    }
                });
                valueConverter.SpatialOperationSet.AddOperation(samples);
                valueConverter.SpatialOperationSet.Execute();

                // update model definition (called during export)
                fmModel.ModelDefinition.SelectSpatialOperations(fmModel.AllDataItems.ToList(), fmModel.TracerDefinitions, initialSpatialOps);
                //New warning should be given.
                TestHelper.AssertAtLeastOneLogMessagesContains(
                    () => extFile.Write(extForceFile, fmModel.ModelDefinition, true, true),
                    string.Format(
                        Resources.SedimentFile_WriteSpatiallyVaryingSedimentPropertySubFiles_Cannot_create_xyz_file_for_spatial_varying_initial_condition__0__because_it_is_a_value_spatial_operation__please_interpolate_the_operation_to_the_grid_or,
                        sedConcProp.SpatiallyVaryingName));
            }
            finally
            {
                FileUtils.DeleteIfExists(sedFile);
                FileUtils.DeleteIfExists(extForceFile);
                FileUtils.DeleteIfExists(generatedXyzFile);
            }
        }

        [Test]
        public void CheckReadWriteOfSampleForcingsWithAOperator()
        {
            var def = new WaterFlowFMModelDefinition();
            string extPath = TestHelper.GetTestFilePath(@"chezy_samples\chezy_A.ext");
            string extSubFilesReferenceFilePath = Path.Combine(Path.GetDirectoryName(extPath), "chezy_A.mdu");

            var extForceFile = new ExtForceFile();
            extForceFile.Read(extPath, def, extSubFilesReferenceFilePath);

            Assert.IsNull(def.GetSpatialOperations(WaterFlowFMModelDefinition.ViscosityDataItemName));
            Assert.IsNull(def.GetSpatialOperations(WaterFlowFMModelDefinition.DiffusivityDataItemName));
            Assert.IsNull(def.GetSpatialOperations(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName));
            Assert.IsNotNull(def.GetSpatialOperations(WaterFlowFMModelDefinition.RoughnessDataItemName));

            IList<ISpatialOperation> roughnessOperations = def.GetSpatialOperations(WaterFlowFMModelDefinition.RoughnessDataItemName);
            Assert.AreEqual(1, roughnessOperations.Count);

            var samplesOperation = (ImportSamplesOperation) roughnessOperations[0];
            Assert.AreEqual("chezy", samplesOperation.Name);
            Assert.AreEqual(4, samplesOperation.GetPoints().Count());

            using (var temp = new TemporaryDirectory())
            {
                string newPath =Path.Combine(temp.Path, "initialFields.ini");
                var initialFieldFileWriter = new InitialFieldFileWriter(new FileSystem(), new SpatialDataFileWriter());
                initialFieldFileWriter.Write(newPath, def); // write loaded definition to new location

                var initialFieldFileReader = new InitialFieldFileReader(new FileSystem());
                var newDef = new WaterFlowFMModelDefinition();

                initialFieldFileReader.Read(newPath, newPath, newDef); // load written definition back
                IList<ISpatialOperation> newRoughnessOperations = newDef.GetSpatialOperations(WaterFlowFMModelDefinition.RoughnessDataItemName);
                Assert.AreEqual(4, ((ImportSamplesOperation) newRoughnessOperations[0]).GetPoints().Count());
            }
        }

        [Test]
        public void ReadWriteSampleForcingsInitialSalinity()
        {
            var def = new WaterFlowFMModelDefinition();
            string extPath = TestHelper.GetTestFilePath(@"chezy_samples\initialsalinity.ext");
            string extSubFilesReferenceFilePath = Path.Combine(Path.GetDirectoryName(extPath), "waterlevel.mdu");

            var extForceFile = new ExtForceFile();
            extForceFile.Read(extPath, def, extSubFilesReferenceFilePath);

            Assert.AreEqual(1, def.GetSpatialOperations(WaterFlowFMModelDefinition.InitialSalinityDataItemName).Count);

            // add polygon
            var geometry = new Polygon(new LinearRing(new[]
            {
                new Coordinate(-135, -105),
                new Coordinate(-85, -100),
                new Coordinate(-75, -205),
                new Coordinate(-125, -200),
                new Coordinate(-135, -105)
            }));
            var f = new Feature {Geometry = geometry};
            var maskCollection = new FeatureCollection(new[]
            {
                f
            }, typeof(Feature));
            var operation = new SetValueOperation {Name = "poly"};
            operation.SetInputData(SpatialOperation.MaskInputName, maskCollection);
            def.GetSpatialOperations(WaterFlowFMModelDefinition.InitialSalinityDataItemName).Add(operation);

            // add samples
            var samples = new AddSamplesOperation(false);
            samples.SetInputData(AddSamplesOperation.SamplesInputName, new PointCloudFeatureProvider
            {
                PointCloud = new PointCloud
                {
                    PointValues = new List<IPointValue>
                    {
                        new PointValue
                        {
                            X = 5,
                            Y = 5,
                            Value = 12
                        },
                        new PointValue
                        {
                            X = 10,
                            Y = 10,
                            Value = 30
                        },
                        new PointValue
                        {
                            X = 20,
                            Y = 10,
                            Value = 31
                        }
                    }
                }
            });

            def.GetSpatialOperations(WaterFlowFMModelDefinition.InitialSalinityDataItemName).Add(samples);

            const string newExtPath = "test.ext";
            string newExtSubFilesReferenceFilePath = Path.Combine(Path.GetDirectoryName(newExtPath), "test.mdu");

            extForceFile.Write(newExtPath, def, true, true);

            var newDef = new WaterFlowFMModelDefinition();
            var newExtFile = new ExtForceFile();
            newExtFile.Read(newExtPath, newDef, newExtSubFilesReferenceFilePath);

            Assert.AreEqual(3, newDef.GetSpatialOperations(WaterFlowFMModelDefinition.InitialSalinityDataItemName).Count);
            Assert.AreEqual(3,
                            ((ImportSamplesOperation)
                                newDef.GetSpatialOperations(WaterFlowFMModelDefinition.InitialSalinityDataItemName)[2]).GetPoints()
                                                                                                                       .Count());
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ExportImportBoundaryConditionWithOffsetAndFactor()
        {
            var model = new WaterFlowFMModel();

            var feature = new Feature2D
            {
                Name = "boundary",
                Geometry =
                    new LineString(new[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(1, 0),
                        new Coordinate(2, 0)
                    })
            };

            var bc1 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                BoundaryConditionDataType.AstroComponents)
            {
                Feature = feature,
                Offset = -0.3,
                Factor = 2.5
            };

            bc1.AddPoint(1);
            IFunction data = bc1.GetDataAtPoint(1);
            data["M1"] = new[]
            {
                0.5,
                120
            };

            bc1.AddPoint(2);
            data = bc1.GetDataAtPoint(2);
            data["M2"] = new[]
            {
                0.7,
                60
            };
            AddBoundaryCondition(model, bc1);

            string mduPath = Path.GetFullPath(@"exportbc.mdu");

            model.ExportTo(mduPath);

            var importedModel = new WaterFlowFMModel();
            importedModel.ImportFromMdu(mduPath);

            IEventedList<Feature2D> boundaries = importedModel.Boundaries;
            Assert.AreEqual(1, boundaries.Count);
            Assert.AreEqual(feature.Geometry, boundaries.First().Geometry);

            List<IBoundaryCondition> boundaryConditions = importedModel.BoundaryConditions.ToList();
            Assert.AreEqual(1, boundaryConditions.Count);

            Assert.AreEqual(-0.3, ((FlowBoundaryCondition) boundaryConditions[0]).Offset);
            Assert.AreEqual(2.5, ((FlowBoundaryCondition) boundaryConditions[0]).Factor);

            IFunction pointData1 = ((BoundaryCondition) boundaryConditions[0]).GetDataAtPoint(1);
            Assert.AreEqual(pointData1.Arguments[0].Values.OfType<string>().ToArray(), new[]
            {
                "M1"
            });
            Assert.AreEqual(pointData1.Components[0].Values.OfType<double>().ToArray(), new[]
            {
                0.5
            });
            Assert.AreEqual(pointData1.Components[1].Values.OfType<double>().ToArray(), new[]
            {
                120
            });

            IFunction pointData2 = ((BoundaryCondition) boundaryConditions[0]).GetDataAtPoint(2);
            Assert.AreEqual(pointData2.Arguments[0].Values.OfType<string>().ToArray(), new[]
            {
                "M2"
            });
            Assert.AreEqual(pointData2.Components[0].Values.OfType<double>().ToArray(), new[]
            {
                0.7
            });
            Assert.AreEqual(pointData2.Components[1].Values.OfType<double>().ToArray(), new[]
            {
                60
            });
        }

        [Test]
        public void ExportImportMultipleBoundaryConditionsOnSameFeature()
        {
            var model = new WaterFlowFMModel();

            var feature = new Feature2D
            {
                Name = "boundary",
                Geometry =
                    new LineString(new[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(1, 0),
                        new Coordinate(2, 0)
                    })
            };

            var bc1 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.AstroComponents) {Feature = feature};

            bc1.AddPoint(1);
            IFunction data = bc1.GetDataAtPoint(1);
            data["M1"] = new[]
            {
                0.5,
                120
            };

            bc1.AddPoint(2);
            data = bc1.GetDataAtPoint(2);
            data["M2"] = new[]
            {
                0.7,
                60
            };
            AddBoundaryCondition(model, bc1);

            var bc2 = new FlowBoundaryCondition(FlowBoundaryQuantityType.NormalVelocity, BoundaryConditionDataType.AstroComponents) {Feature = feature};
            bc2.AddPoint(1);
            data = bc2.GetDataAtPoint(1);
            data["M1"] = new[]
            {
                0.6,
                0
            };
            bc2.AddPoint(2);
            data = bc2.GetDataAtPoint(2);
            data["M2"] = new[]
            {
                0.8,
                30
            };
            AddBoundaryCondition(model, bc2);

            string mduPath = Path.GetFullPath(@"exportbcs.mdu");

            model.ExportTo(mduPath);

            var importedModel = new WaterFlowFMModel();
            importedModel.ImportFromMdu(mduPath);

            IEventedList<Feature2D> boundaries = importedModel.Boundaries;
            Assert.AreEqual(1, boundaries.Count);
            Assert.AreEqual(feature.Geometry, boundaries.First().Geometry);

            List<IBoundaryCondition> boundaryConditions = importedModel.BoundaryConditions.ToList();
            Assert.AreEqual(2, boundaryConditions.Count);

            IFunction pointData1 = ((BoundaryCondition) boundaryConditions[0]).GetDataAtPoint(1);
            Assert.AreEqual(pointData1.Arguments[0].Values.OfType<string>().ToArray(), new[]
            {
                "M1"
            });
            Assert.AreEqual(pointData1.Components[0].Values.OfType<double>().ToArray(), new[]
            {
                0.5
            });
            Assert.AreEqual(pointData1.Components[1].Values.OfType<double>().ToArray(), new[]
            {
                120
            });

            IFunction pointData2 = ((BoundaryCondition) boundaryConditions[0]).GetDataAtPoint(2);
            Assert.AreEqual(pointData2.Arguments[0].Values.OfType<string>().ToArray(), new[]
            {
                "M2"
            });
            Assert.AreEqual(pointData2.Components[0].Values.OfType<double>().ToArray(), new[]
            {
                0.7
            });
            Assert.AreEqual(pointData2.Components[1].Values.OfType<double>().ToArray(), new[]
            {
                60
            });

            IFunction pointData3 = ((BoundaryCondition) boundaryConditions[1]).GetDataAtPoint(1);
            Assert.AreEqual(pointData3.Arguments[0].Values.OfType<string>().ToArray(), new[]
            {
                "M1"
            });
            Assert.AreEqual(pointData3.Components[0].Values.OfType<double>().ToArray(), new[]
            {
                0.6
            });
            Assert.AreEqual(pointData3.Components[1].Values.OfType<double>().ToArray(), new[]
            {
                0
            });

            IFunction pointData4 = ((BoundaryCondition) boundaryConditions[1]).GetDataAtPoint(2);
            Assert.AreEqual(pointData4.Arguments[0].Values.OfType<string>().ToArray(), new[]
            {
                "M2"
            });
            Assert.AreEqual(pointData4.Components[0].Values.OfType<double>().ToArray(), new[]
            {
                0.8
            });
            Assert.AreEqual(pointData4.Components[1].Values.OfType<double>().ToArray(), new[]
            {
                30
            });
        }

        [Test]
        public void ExportImportSummedWaterLevelsOnSameFeature()
        {
            var model = new WaterFlowFMModel {Name = "test"};

            var feature = new Feature2D
            {
                Name = "boundary",
                Geometry =
                    new LineString(new[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(1, 0),
                        new Coordinate(2, 0)
                    })
            };

            var bc1 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.AstroComponents) {Feature = feature};

            bc1.AddPoint(1);
            IFunction data = bc1.GetDataAtPoint(1);
            data["M1"] = new[]
            {
                0.5,
                120
            };

            bc1.AddPoint(2);
            data = bc1.GetDataAtPoint(2);
            data["M2"] = new[]
            {
                0.7,
                60
            };
            AddBoundaryCondition(model, bc1);

            var bc2 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.Harmonics) {Feature = feature};
            bc2.AddPoint(1);
            data = bc2.GetDataAtPoint(1);
            data[250.0] = new[]
            {
                0.6,
                0
            };
            bc2.AddPoint(2);
            data = bc2.GetDataAtPoint(2);
            data[360.0] = new[]
            {
                0.8,
                30
            };
            AddBoundaryCondition(model, bc2);

            string mduPath = Path.GetFullPath(@"exportwls.mdu");

            model.ExportTo(mduPath);

            string path = model.BndExtFilePath;
            IniData iniData;
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                iniData = new IniReader().ReadIniFile(fileStream, path);
            }

            Assert.AreEqual(3, iniData.Sections.Count());
        }

        [Test]
        public void ReadAndWriteSourcesAndSinksTest()
        {
            var def = new WaterFlowFMModelDefinition();

            string extPath = TestHelper.GetTestFilePath(@"c070_sourcesink_2D\sourcesink_2D.ext");
            string extSubFilesReferenceFilePath = Path.Combine(Path.GetDirectoryName(extPath), "sourcesink_2D.mdu");

            var extForceFile = new ExtForceFile();
            extForceFile.Read(extPath, def, extSubFilesReferenceFilePath);

            Assert.AreEqual(7, def.Pipes.Count);
            Assert.AreEqual(7, def.SourcesAndSinks.Count);

            Assert.AreEqual(1.5d, def.SourcesAndSinks[0].Area);

            extForceFile.Write("sourcesink.ext", def, true, true);

            Assert.IsTrue(File.Exists("sourcesink.ext"));

            Assert.IsTrue(File.Exists("chan2_east_outflow.pli"));
            Assert.IsTrue(File.Exists("chan2_east_outflow.tim"));
        }

        [Test]
        [TestCase("uniform", @"heatFluxFiles\UniformHeatFluxModel\htccase.ext", @"heatFluxFiles\UniformHeatFluxModel\meteo.tim")]
        [TestCase("uniform", @"heatFluxFiles\UniformHeatFluxModel\htccase-foo.ext", @"heatFluxFiles\UniformHeatFluxModel\meteo.foo")] // unconventional file extension
        [TestCase("gridded", @"heatFluxFiles\GriddedHeatFluxModel\htccase.ext", @"heatFluxFiles\GriddedHeatFluxModel\meteo.htc", @"heatFluxFiles\GriddedHeatFluxModel\meteo.grd")]
        public void GivenAHeatFluxModel_WhenReadingAndWriting_ThenAllDataShouldRemain(string heatFluxModelVersion, params string[] relativeTestDataFilePaths)
        {
            // Given
            using (var temp = new TemporaryDirectory())
            {
                List<string> copiesInTempFilePaths = temp.CopyAllTestDataToTempDirectory(relativeTestDataFilePaths);

                string copyInTempOfExtFilePath = copiesInTempFilePaths[0];

                var def = new WaterFlowFMModelDefinition();
                def.HeatFluxModel.Type = HeatFluxModelType.Composite;

                var extForceFile = new ExtForceFile();

                // When
                extForceFile.Read(copyInTempOfExtFilePath, def, copyInTempOfExtFilePath);

                // Then
                if (heatFluxModelVersion == "gridded")
                {
                    Assert.IsNotNull(def.HeatFluxModel.GriddedHeatFluxFilePath, "The gridded heat flux model is not correctly imported");
                    Assert.IsNotNull(def.HeatFluxModel.GridFilePath, "The gridded heat flux model is not correctly imported");
                }
                else
                {
                    Assert.IsNull(def.HeatFluxModel.GriddedHeatFluxFilePath, "The uniform heat flux model is not correctly imported");
                    Assert.IsNull(def.HeatFluxModel.GridFilePath, "The uniform heat flux model is not correctly imported");
                }

                // Given
                string absoluteSaveFolderInTempPath = Path.Combine(temp.Path, "save");

                var saveLocations = new List<string>();

                foreach (string relativeFilePath in relativeTestDataFilePaths)
                {
                    string fileName = Path.GetFileName(relativeFilePath);
                    string saveFilePath = Path.Combine(absoluteSaveFolderInTempPath, fileName);
                    saveLocations.Add(saveFilePath);
                }

                string savedExtFile = saveLocations[0];

                string saveDirectory = Path.GetDirectoryName(savedExtFile);
                FileUtils.CreateDirectoryIfNotExists(saveDirectory);

                // When
                extForceFile.Write(savedExtFile, def, true, true);

                // Then
                foreach (string file in saveLocations)
                {
                    Assert.IsTrue(File.Exists(file));
                }
            }
        }

        [Test]
        public void GivenExtForceFileReferencingPliFileForSourceSink_WhenReading_ThenSourceAndSinkIsImported()
        {
            // Given
            var modelDefinition = new WaterFlowFMModelDefinition();
            var extForceFile = new ExtForceFile();

            using (var tempDirectory = new TemporaryDirectory())
            {
                string originalTestDirectory = TestHelper.GetTestFilePath(Path.Combine("ExtFileTest", "SourcesAndSinks"));
                FileUtils.CopyDirectory(originalTestDirectory, tempDirectory.Path);

                string extForceFilePath = Path.Combine(tempDirectory.Path, "SourcesAndSinks.ext");
                string dummyMduFilePath = Path.Combine(tempDirectory.Path, "nonExisting.mdu");

                // When
                extForceFile.Read(extForceFilePath, modelDefinition, dummyMduFilePath);

                // Then
                Assert.IsNotEmpty(modelDefinition.SourcesAndSinks, "Reading source and sink was unsuccessful.");
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenExternalForcingsFileWithComments_WhenReadingAndWriting_ThenCommentsHaveBeenPreserved()
        {
            // Setup
            string mduFilePath = TestHelper.GetTestFilePath(@"fm_files\fm_files.mdu");
            var modelDefinition = new WaterFlowFMModelDefinition(Path.GetDirectoryName(mduFilePath), Path.GetFileName(mduFilePath));

            var extForceFile = new ExtForceFile();
            const string relativeExtForceFilePath = @"fm_files\fm_files.ext";
            string extForceFilePath = TestHelper.GetTestFilePath(relativeExtForceFilePath);
            extForceFile.Read(extForceFilePath, modelDefinition, mduFilePath);

            using (var temporaryDirectory = new TemporaryDirectory())
            {
                // When
                string newExtForceFilePath = Path.Combine(temporaryDirectory.Path, Path.GetFileName(relativeExtForceFilePath));
                extForceFile.Write(newExtForceFilePath, modelDefinition, true, true);

                // Assert
                string extForceFileContent = File.ReadAllText(newExtForceFilePath);
                Assert.IsTrue(extForceFileContent.Contains(
                                  "* FACTOR  =   : Conversion factor for this provider"));
                Assert.IsTrue(extForceFileContent.Contains(
                                  "* This comment line will not be removed, eventhough shiptxy is not yet supported."));
            }
        }

        [Test]
        public void GivenOldExtForceFileWithExtraPolTol_WhenReadOldExtForceFile_ThenWrittenDataContainsExpectedDataWithExtraPolTol()
        {
            double expectedExtraPolTol = 40;
            var def = new WaterFlowFMModelDefinition();
            string originalTestDirectory = TestHelper.GetTestFilePath(Path.Combine("ExtFileTest", "OptionalExtraPolTol"));
            string extPath = Path.Combine(originalTestDirectory, "WithExtraPolTol.ext");
            string extSubFilesReferenceFilePath = Path.Combine(originalTestDirectory, "FlowFM.mdu");

            var extForceFile = new ExtForceFile();
            extForceFile.Read(extPath, def, extSubFilesReferenceFilePath);

            var supportedExtForceFileItem = extForceFile.ExistingForceFileItems.First().Key;

            Assert.That(supportedExtForceFileItem, Is.Not.Null);
            Assert.That(supportedExtForceFileItem.ExtraPolTol, Is.EqualTo(expectedExtraPolTol));
        }

        [Test]
        public void GivenOldExtForceFileWithExtraPolTol_WhenReadAndWriteOldExtForceFile_ThenWrittenDataContainsExpectedDataWithExtraPolTol()
        {
            const string quantity = "initialvelocityx";
            const string filename = @"rijn_beno19_6_v2a_waal_ucx_S16000.xyz";
            const int filetype = 7;
            const int method = 5;
            const string operation = "O";
            const int extrapoltol = 40;

            var extForceFileItem = new ExtForceFileItem(quantity)
            {
                FileName = filename,
                FileType = filetype,
                Method = method,
                Operand = operation,
                ExtraPolTol = extrapoltol
            };

            using (var temporaryDirectory = new TemporaryDirectory())
            {
                var writeExtPath = Path.Combine(temporaryDirectory.Path, Path.GetFileName("with_extrapoltol_written.ext"));
                var modelDefinition = new WaterFlowFMModelDefinition();
                var extForceFile = new ExtForceFile();
                modelDefinition.UnsupportedFileBasedExtForceFileItems.Add(new UnsupportedFileBasedExtForceFileItem(writeExtPath,extForceFileItem));
                
                //write
                extForceFile.Write(writeExtPath, modelDefinition, false, false);

                // check written file
                string extForceFileContent = File.ReadAllText(writeExtPath);
                Assert.IsTrue(extForceFileContent.Contains($"QUANTITY={quantity}"), $"QUANTITY={quantity} not found");
                Assert.IsTrue(extForceFileContent.Contains($"FILENAME={filename}"), $"FILENAME={filename} not found");
                Assert.IsTrue(extForceFileContent.Contains($"FILETYPE={filetype}"), $"FILETYPE={filetype} not found");
                Assert.IsTrue(extForceFileContent.Contains($"METHOD={method}"), $"METHOD={method} not found");
                Assert.IsTrue(extForceFileContent.Contains($"OPERAND={operation}"), $"OPERAND={operation} not found");
                Assert.IsTrue(extForceFileContent.Contains($"EXTRAPOLTOL={extrapoltol}"), $"EXTRAPOLTOL={extrapoltol} not found");
            }
        }

        [Test]
        [TestCase(@"ExtFileTest\InitialVelocity\beno19_6_20m_initial_velocity_S16000.ext")]
        [TestCase(@"ExtFileTest\InitialVelocity\beno19_6_20m_initial_velocity_S16000_Case_Insensitive_Quantity.ext")]
        public void GivenOldExternalFileWithInitialVelocities_WhenReadingFiles_ThenExtForceFileItemsWithExpectedNamesAndQuantitiesExist(string extForcingFile)
        {
            const string expectedFileNameX = "rijn_beno19_6_v2a_waal_ucx_S16000.xyz";
            const string expectedQuantityX = "initialvelocityx";
            const string expectedFileNameY = "rijn_beno19_6_v2a_waal_ucy_S16000.xyz";
            const string expectedQuantityY = "initialvelocityy";
            
            var def = new WaterFlowFMModelDefinition();
            string extPath = TestHelper.GetTestFilePath(@"ExtFileTest\InitialVelocity\beno19_6_20m_initial_velocity_S16000.ext");
            string extSubFilesReferenceFilePath = TestHelper.GetTestFilePath(@"ExtFileTest\InitialVelocity\flowFM.mdu");

            var extForceFile = new ExtForceFile();
            extForceFile.Read(extPath, def, extSubFilesReferenceFilePath);
            
            var extForceFileItems = extForceFile.ExistingForceFileItems.ToList();
            var extForceFileItemX = extForceFileItems[0].Key;
            var extForceFileItemY = extForceFileItems[1].Key;

            Assert.That(extForceFileItemX, Is.Not.Null);
            Assert.That(extForceFileItemX.Quantity, Is.EqualTo(expectedQuantityX));
            Assert.That(extForceFileItemX.FileName, Is.EqualTo(expectedFileNameX));
            
            Assert.That(extForceFileItemY, Is.Not.Null);
            Assert.That(extForceFileItemY.Quantity, Is.EqualTo(expectedQuantityY));
            Assert.That(extForceFileItemY.FileName, Is.EqualTo(expectedFileNameY));
        }
        
        [Test]
        public void GivenInitialVelocity_WhenWritingFiles_ThenOldExtForceFileAndSubFileExist()
        {
            const string quantity = "initialvelocityx";
            const string filename = @"rijn_beno19_6_v2a_waal_ucx_S16000.xyz";
            const int filetype = 7;
            const int method = 5;
            const string operation = "O";

            var extForceFileItem = new ExtForceFileItem(quantity)
            {
                FileName = filename,
                FileType = filetype,
                Method = method,
                Operand = operation,
            };

            using (var temporaryDirectory = new TemporaryDirectory())
            {
                string writeExtPath = Path.Combine(temporaryDirectory.Path, Path.GetFileName("with_initialvelocity_written.ext"));
                var modelDefinition = new WaterFlowFMModelDefinition();
                modelDefinition.InitialVelocityX.SetPointValues(new[]
                {
                    new PointValue
                    {
                        X = 1.23,
                        Y = 2.34,
                        Value = 3.45
                    }
                });
                modelDefinition.InitialVelocityX.SourceFileName = filename;
                modelDefinition.InitialVelocityX.InterpolationMethod = SpatialInterpolationMethod.Triangulation;

                var extForceFile = new ExtForceFile();
                extForceFile.ExistingForceFileItems[extForceFileItem] = modelDefinition.InitialVelocityX;
                
                //write
                extForceFile.Write(writeExtPath, modelDefinition, false, false);

                // check written file
                string extForceFileContent = File.ReadAllText(writeExtPath);
                Assert.IsTrue(extForceFileContent.Contains($"QUANTITY={quantity}"), $"QUANTITY={quantity} not found");
                Assert.IsTrue(extForceFileContent.Contains($"FILENAME={filename}"), $"FILENAME={filename} not found");
                Assert.IsTrue(extForceFileContent.Contains($"FILETYPE={filetype}"), $"FILETYPE={filetype} not found");
                Assert.IsTrue(extForceFileContent.Contains($"METHOD={method}"), $"METHOD={method} not found");
                Assert.IsTrue(extForceFileContent.Contains($"OPERAND={operation}"), $"OPERAND={operation} not found");

                string expectedVelocityPath = Path.Combine(temporaryDirectory.Path, filename);
                Assert.That(File.Exists(expectedVelocityPath), Is.True);
            }
        }
        
        

        private static void ValidateUnknownQuantities(WaterFlowFMModelDefinition def)
        {
            Assert.AreEqual(2, def.UnsupportedFileBasedExtForceFileItems.Count,
                            "Two unknown quantities were expected to be stored on the model definition.");

            ExtForceFileItem unsupportedQuantity1 = def.UnsupportedFileBasedExtForceFileItems.First().UnsupportedExtForceFileItem;

            Assert.AreEqual("internaltidesfrictioncoefficient", unsupportedQuantity1.Quantity,
                            "Quantity name was not as expected.");
            Assert.AreEqual("surroundingDomain.pol", unsupportedQuantity1.FileName,
                            "File name of quantity was not as expected.");
            Assert.AreEqual(10, unsupportedQuantity1.FileType,
                            "File type of quantity was not as expected.");
            Assert.AreEqual(4, unsupportedQuantity1.Method,
                            "Method type of quantity was not as expected.");
            Assert.AreEqual("*", unsupportedQuantity1.Operand,
                            "Operand of quantity was not as expected.");
            Assert.AreEqual(0.0125, unsupportedQuantity1.Value,
                            "Value of quantity was not as expected.");

            ExtForceFileItem unsupportedQuantity2 = def.UnsupportedFileBasedExtForceFileItems.Last().UnsupportedExtForceFileItem;

            Assert.AreEqual("rainfall_rate", unsupportedQuantity2.Quantity,
                            "Quantity name was not as expected.");
            Assert.AreEqual("RAD_NL25_RAC_MFBS_5min.nc", unsupportedQuantity2.FileName,
                            "File name of quantity was not as expected.");
            Assert.AreEqual(11, unsupportedQuantity2.FileType,
                            "File type of quantity was not as expected.");
            Assert.AreEqual(3, unsupportedQuantity2.Method,
                            "Method type of quantity was not as expected.");
            Assert.AreEqual("O", unsupportedQuantity2.Operand,
                            "Operand of quantity was not as expected.");
        }

        private static void AddBoundaryCondition(WaterFlowFMModel model, FlowBoundaryCondition bc)
        {
            WaterFlowFMModelDefinition modelDefinition = model.ModelDefinition;
            BoundaryConditionSet set =
                modelDefinition.BoundaryConditionSets.FirstOrDefault(
                    bcs => bcs.Feature == ((IBoundaryCondition) bc).Feature);
            if (set != null)
            {
                set.BoundaryConditions.Add(bc);
            }
            else
            {
                modelDefinition.BoundaryConditionSets.Add(new BoundaryConditionSet
                {
                    Feature = ((IBoundaryCondition) bc).Feature as Feature2D,
                    BoundaryConditions = new EventedList<IBoundaryCondition> {bc}
                });
            }
        }
    }
}