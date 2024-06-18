using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.InitialField;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
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
    public class ExtForceFileTest
    {
        private void CheckInternalTidesFrictionCoefficient(WaterFlowFMModelDefinition def)
        {
            Assert.AreEqual(1, def.UnsupportedFileBasedExtForceFileItems.Count);
            Assert.AreEqual("surroundingDomain.pol",
                def.UnsupportedFileBasedExtForceFileItems[0].UnsupportedExtForceFileItem.FileName);
            Assert.AreEqual("surroundingDomain.pol",
                def.UnsupportedFileBasedExtForceFileItems[0].UnsupportedExtForceFileItem.FileName);
            Assert.AreEqual(10, def.UnsupportedFileBasedExtForceFileItems[0].UnsupportedExtForceFileItem.FileType);
            Assert.AreEqual(4, def.UnsupportedFileBasedExtForceFileItems[0].UnsupportedExtForceFileItem.Method);
            Assert.AreEqual("*", def.UnsupportedFileBasedExtForceFileItems[0].UnsupportedExtForceFileItem.Operand);
            Assert.AreEqual(0.0125, def.UnsupportedFileBasedExtForceFileItems[0].UnsupportedExtForceFileItem.Value);
        }

        private static void AddBoundaryCondition(WaterFlowFMModel model, FlowBoundaryCondition bc)
        {
            var modelDefinition = model.ModelDefinition;
            var set =
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

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ReadPolygonForcings()
        {
            var def = new WaterFlowFMModelDefinition();
            var extPath = TestHelper.GetTestFilePath(@"harlingen\001.ext");

            var extForceFile = new ExtForceFile();
            extForceFile.Read(extPath, def);

            //extForceFile.ImportSpatialOperations(extPath, def);

            Assert.IsNull(def.GetSpatialOperations(WaterFlowFMModelDefinition.ViscosityDataItemName));
            Assert.IsNull(def.GetSpatialOperations(WaterFlowFMModelDefinition.DiffusivityDataItemName));
            Assert.IsNull(def.GetSpatialOperations(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName));
            Assert.AreEqual(2, def.GetSpatialOperations(WaterFlowFMModelDefinition.InitialSalinityDataItemName).Count);

            var roughnessOperations = def.GetSpatialOperations(WaterFlowFMModelDefinition.RoughnessDataItemName);
            Assert.IsNotNull(roughnessOperations);
            Assert.AreEqual(1, roughnessOperations.Count);

            var firstOperation = roughnessOperations.First() as SetValueOperation;
            Assert.IsNotNull(firstOperation);

            Assert.AreEqual(PointwiseOperationType.Overwrite, firstOperation.OperationType);
            Assert.AreEqual(0.04, firstOperation.Value); //undefined

            var secondInitialSalinityOperation = (SetValueOperation)def.GetSpatialOperations(WaterFlowFMModelDefinition.InitialSalinityDataItemName)[1];
            Assert.AreEqual(PointwiseOperationType.Add, secondInitialSalinityOperation.OperationType);
            Assert.AreEqual(10.0, secondInitialSalinityOperation.Value); //undefined
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadSampleForcings()
        {
            var def = new WaterFlowFMModelDefinition();
            var extPath = TestHelper.GetTestFilePath(@"chezy_samples\chezy.ext");

            var extForceFile = new ExtForceFile();
            extForceFile.Read(extPath, def);
            
            Assert.IsNull(def.GetSpatialOperations(WaterFlowFMModelDefinition.ViscosityDataItemName));
            Assert.IsNull(def.GetSpatialOperations(WaterFlowFMModelDefinition.DiffusivityDataItemName));
            Assert.IsNull(def.GetSpatialOperations(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName));
            Assert.AreEqual(1, def.GetSpatialOperations(WaterFlowFMModelDefinition.RoughnessDataItemName).Count);

            IList<ISpatialOperation> roughnessOperations = def.GetSpatialOperations(WaterFlowFMModelDefinition.RoughnessDataItemName);
            
            Assert.IsTrue(roughnessOperations[0] is ImportSamplesOperation);

            var sampleDef = (ImportSamplesOperation)roughnessOperations[0];
            Assert.AreEqual("chezy", sampleDef.Name);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadXyzFile_WithUnknownSpatiallyVaryingProperties_ShouldGiveAWarningMessage()
        {
            var def = new WaterFlowFMModelDefinition();
            var extPath = TestHelper.GetTestFilePath(@"SpatialVaryingPrefix\incorrect_prefix.ext");
            extPath = TestHelper.CreateLocalCopy(extPath);
            Assert.IsTrue(File.Exists(extPath));

            var extForceFile = new ExtForceFile();

            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => extForceFile.Read(extPath, def),
                String.Format(
                    Resources
                        .ExtForceFile_ReadSpatialData_The_model_may_not_run__Spatial_varying_quantity__0__could_not_be_imported_because_the_prefix_does_not_match__1__for_Tracers_or__2__for_Spatial_Varying_Sediments_,
                    "initialspatialvaryingsedimentSediment_sand_SedConc", 
                    ExtForceFile.InitialTracerPrefix,ExtForceFile.InitialSpatialVaryingSedimentPrefix));

            FileUtils.DeleteIfExists(extPath);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadXyzFile_WithKnownSpatiallyVaryingProperties_ShouldNotGiveAWarningMessage()
        {
            var def = new WaterFlowFMModelDefinition();
            var extPath = TestHelper.GetTestFilePath(@"SpatialVaryingPrefix\correctKnownQuantity.ext");
            extPath = TestHelper.CreateLocalCopy(extPath);
            Assert.IsTrue(File.Exists(extPath));

            var extForceFile = new ExtForceFile();
            Assert.Throws<AssertionException>(
                () => TestHelper.AssertAtLeastOneLogMessagesContains(
                    () => extForceFile.Read(extPath, def),
                    String.Format(
                        Resources
                            .ExtForceFile_ReadSpatialData_The_model_may_not_run__Spatial_varying_quantity__0__could_not_be_imported_because_the_prefix_does_not_match__1__for_Tracers_or__2__for_Spatial_Varying_Sediments_,
                        ExtForceQuantNames.FrictCoef, 
                        ExtForceFile.InitialTracerPrefix,ExtForceFile.InitialSpatialVaryingSedimentPrefix)),
                "The warn message was logged, but we were not expecting it to appear");
            

            FileUtils.DeleteIfExists(extPath);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadExtFileWithUnknownQuantityShowsLogMessage()
        {
            var def = new WaterFlowFMModelDefinition();
            var extPath = TestHelper.GetTestFilePath(@"ExtFileTest\withOnlyUnknownQuantity.ext");
            Assert.IsTrue(File.Exists(extPath));

            extPath = TestHelper.CreateLocalCopy(extPath);
            Assert.IsTrue(File.Exists(extPath));

            var expectedMessage = string.Format(Resources.ExtForceFile_ReadPolyLineData_Unsupported_quantity_type___0___in_the__ext_file__1__detected__It_will_not_be_imported_, "generalstructure", extPath);
            var extForceFile = new ExtForceFile();
            TestHelper.AssertAtLeastOneLogMessagesContains(() => extForceFile.Read(extPath, def), expectedMessage);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadExtFileWithUnknownQuantityImportsTheOtherQuantities()
        {
            var def = new WaterFlowFMModelDefinition();
            var extPath = TestHelper.GetTestFilePath(@"ExtFileTest\withUnknownAndKnownQuantities.ext");
            Assert.IsTrue(File.Exists(extPath));

            extPath = TestHelper.CreateLocalCopy(extPath);
            Assert.IsTrue(File.Exists(extPath));

            var expectedMessage = string.Format(Resources.ExtForceFile_ReadPolyLineData_Unsupported_quantity_type___0___in_the__ext_file__1__detected__It_will_not_be_imported_, "generalstructure", extPath);
            var extForceFile = new ExtForceFile();

            Assert.IsFalse(def.BoundaryConditions.Any());
            TestHelper.AssertAtLeastOneLogMessagesContains(() => extForceFile.Read(extPath, def), expectedMessage);
            Assert.IsTrue(def.BoundaryConditions.Any());

            /* Just check the boundary has been imported. */
            var boundaryCondition = def.BoundaryConditions.First();
            Assert.AreEqual("WaterLevel", boundaryCondition.VariableName);
            Assert.AreEqual("OB_001_orgsize-Water level", boundaryCondition.Name);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExtFileWithInternalTidesFrictionCoefficient_WhenImportingItAndExportingIt_ThenThisQuantityShouldBeReadAndWritten()
        {
            var def = new WaterFlowFMModelDefinition();
            var extPath = TestHelper.GetTestFilePath(@"ExtFileTest\withInternalTidesFrictionCoefficientAndKnownQuantities.ext");
            Assert.IsTrue(File.Exists(extPath));

            extPath = TestHelper.CreateLocalCopy(extPath);
            Assert.IsTrue(File.Exists(extPath));

            var extForceFile = new ExtForceFile();
            var expectedMessage =
                string.Format(
                    "Spatial varying quantity {0} detected in the external force file and will be passed to the computational core. This may affect your simulation.",
                    extForceFile.UnsupportedQuantityInMemory);
           

            Assert.IsFalse(def.BoundaryConditions.Any());
            TestHelper.AssertAtLeastOneLogMessagesContains(() => extForceFile.Read(extPath, def), expectedMessage);
            Assert.IsTrue(def.BoundaryConditions.Any());

            /* Just check the boundary has been imported. */
            var boundaryCondition = def.BoundaryConditions.First();
            Assert.AreEqual("WaterLevel", boundaryCondition.VariableName);
            Assert.AreEqual("OB_001_orgsize-Water level", boundaryCondition.Name);

            // Check if the internaltidesfrictioncoefficient is in memory
            CheckInternalTidesFrictionCoefficient(def);

            Assert.That(File.Exists(Path.Combine(Path.GetDirectoryName(extPath),def.UnsupportedFileBasedExtForceFileItems[0].UnsupportedExtForceFileItem.FileName)));

            string newPath = Path.Combine(Path.GetDirectoryName(extPath),"NewExtFileDirectory","NewExtFile");
            extForceFile.Write(newPath, def); // write loaded definition to new location

            //Check
            Assert.That(File.Exists(Path.Combine(Path.GetDirectoryName(newPath), def.UnsupportedFileBasedExtForceFileItems[0].UnsupportedExtForceFileItem.FileName)));

            var newExtFile = new ExtForceFile();
            var newDef = new WaterFlowFMModelDefinition();

            newExtFile.Read(newPath, newDef); // load written definition back
            // Check if the internaltidesfrictioncoefficient is in memory again, so that the write method is correct.
            CheckInternalTidesFrictionCoefficient(newDef);

            
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExtFileWithInternalTidesFrictionCoefficient_WhenImportingItAndCorrespondingFileIsMissing_ThenThisQuantityShouldNotBeImported()
        {
            var def = new WaterFlowFMModelDefinition();
            var extPath =
                TestHelper.GetTestFilePath(@"ExtFileTest\ExtFileWithInternalTidesFrictionCoefficientAndMissingFile\withInternalTidesFrictionCoefficientAndKnownQuantities.ext");
            Assert.IsTrue(File.Exists(extPath));

            extPath = TestHelper.CreateLocalCopy(extPath);
            Assert.IsTrue(File.Exists(extPath));

            var extForceFile = new ExtForceFile();
            var expectedMessage =
                string.Format(
                    "Spatial varying quantity {0} detected in the external force file and will be passed to the computational core. This may affect your simulation.",
                    extForceFile.UnsupportedQuantityInMemory);

            var correspondingFile = Path.Combine(Path.GetDirectoryName(extPath), "surroundingDomain.pol");
            var expectedMessage2 = string.Format("File {0} could not be found for an internaltidesfrictioncoefficient quantity in the external force file", correspondingFile);

            List< string> messages = new List<string>();
            messages.Add(expectedMessage);
            messages.Add(expectedMessage2);

            IEnumerable<string> messagesExpected = messages;

            Assert.IsFalse(def.BoundaryConditions.Any());
            TestHelper.AssertLogMessagesAreGenerated(() => extForceFile.Read(extPath, def), messagesExpected);
            
            Assert.IsTrue(def.BoundaryConditions.Any());

            /* Just check the boundary has been imported. */
            var boundaryCondition = def.BoundaryConditions.First();
            Assert.AreEqual("WaterLevel", boundaryCondition.VariableName);
            Assert.AreEqual("OB_001_orgsize-Water level", boundaryCondition.Name);

            // Check if the internaltidesfrictioncoefficient is not in memory
            Assert.AreEqual(0, def.UnsupportedFileBasedExtForceFileItems.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadCorrectSpatialVaryingPropertiesShouldBeOk()
        {
            var def = new WaterFlowFMModelDefinition();
            var extPath = TestHelper.GetTestFilePath(@"SpatialVaryingPrefix\correct_prefix.ext");
            var extForceFile = new ExtForceFile();
            TestHelper.AssertLogMessagesCount(() => extForceFile.Read(extPath, def), 0);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ExtFileDoesNotSaveSedimentSpatiallyVaryingOperationsButSedConc()
        {
            //define model
            var sedFile = Path.GetTempFileName();
            var extForceFile = Path.Combine(Path.GetDirectoryName(sedFile), "extForceFileTest.ext");
            var sedConcXyzFile = Path.Combine(Path.GetDirectoryName(sedFile), "mysedimentName_SedConc." + XyzFile.Extension);
            var customPropXyzFile = Path.Combine(Path.GetDirectoryName(sedFile), "mysedimentName_IniSedThick." + XyzFile.Extension);
            var fileCopyName = string.Empty;
            try
            {
                /* Define new model */
                var fmModel = new WaterFlowFMModel(sedFile);
                fmModel.ModelDefinition.UseMorphologySediment = true;
                var grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
                fmModel.Grid = grid;

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
                    Properties = new EventedList<ISedimentProperty> { doubleSpatProp, thickProp }
                };

                var overallProp = new SedimentProperty<double>("Cref", 0, 0, true, 0, false, "km", "myoveralldescription", false)
                {
                    Value = 80.1
                };

                fmModel.SedimentOverallProperties = new EventedList<ISedimentProperty>() { overallProp };

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
                var dataItem = fmModel.AllDataItems.FirstOrDefault(di => di.Name == "mysedimentName_SedConc");

                // retrieve / create value converter for mysedimentName_SedConc dataitem
                var valueConverter = SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(dataItem, "mysedimentName_SedConc");
                var samplesSedConc = new AddSamplesOperation(false);
                samplesSedConc.SetInputData(AddSamplesOperation.SamplesInputName, new PointCloudFeatureProvider
                {
                    PointCloud = new PointCloud
                    {
                        PointValues = new List<IPointValue>
                        {
                            new PointValue { X = fmModel.Grid.Cells[0].CenterX, Y = fmModel.Grid.Cells[0].CenterY, Value = 12},
                            new PointValue { X = fmModel.Grid.Cells[1].CenterX, Y = fmModel.Grid.Cells[1].CenterY, Value = 30},
                            new PointValue { X = fmModel.Grid.Cells[2].CenterX, Y = fmModel.Grid.Cells[2].CenterY, Value = 31},
                        },
                    },

                });
                valueConverter.SpatialOperationSet.AddOperation(samplesSedConc);
                valueConverter.SpatialOperationSet.Execute();

                /* Create coverage for CustomProp */
                var thickDataItem = fmModel.AllDataItems.FirstOrDefault(di => di.Name == "mysedimentName_IniSedThick");

                // retrieve / create value converter for mysedimentName_SedConc dataitem
                var valueConverThick = SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(thickDataItem, "mysedimentName_IniSedThick");
                var samplesThick = new AddSamplesOperation(false);
                samplesThick.SetInputData(AddSamplesOperation.SamplesInputName, new PointCloudFeatureProvider
                {
                    PointCloud = new PointCloud
                    {
                        PointValues = new List<IPointValue>
                        {
                            new PointValue { X = fmModel.Grid.Cells[0].CenterX, Y = fmModel.Grid.Cells[0].CenterY, Value = 2},
                            new PointValue { X = fmModel.Grid.Cells[1].CenterX, Y = fmModel.Grid.Cells[1].CenterY, Value = 15},
                            new PointValue { X = fmModel.Grid.Cells[2].CenterX, Y = fmModel.Grid.Cells[2].CenterY, Value = 28},
                        },
                    },

                });
                valueConverThick.SpatialOperationSet.AddOperation(samplesThick);
                valueConverThick.SpatialOperationSet.Execute();

                // update model definition (called during export)
                var initialSpatialOps = new List<string>() { doubleSpatProp.SpatiallyVaryingName, thickProp.SpatiallyVaryingName };
                fmModel.ModelDefinition.SelectSpatialOperations(fmModel.DataItems, fmModel.TracerDefinitions, initialSpatialOps);
                
                // create an interpolate operation using the samples added earlier
                var intOpSedConc = new InterpolateOperation();
                intOpSedConc.SetInputData(InterpolateOperation.InputSamplesName, samplesSedConc.Output.Provider);
                Assert.IsNotNull(valueConverter.SpatialOperationSet.AddOperation(intOpSedConc));
                var intOpThick = new InterpolateOperation();
                intOpThick.SetInputData(InterpolateOperation.InputSamplesName, samplesThick.Output.Provider);
                Assert.IsNotNull(valueConverThick.SpatialOperationSet.AddOperation(intOpThick));

                // update model definition (called during export)
                fmModel.ModelDefinition.SelectSpatialOperations(fmModel.DataItems, fmModel.TracerDefinitions, initialSpatialOps);

                /* Save ext file */
                var extFile = new ExtForceFile();
                extFile.Write(extForceFile, fmModel.ModelDefinition);
                Assert.IsTrue(File.Exists(extForceFile));

                /* Check SedConc has generated only one Xyz File and one entry in the Ext file
                 * for SedConc but not for CustomProp.       */
                var extWritten = File.ReadAllText(extForceFile);
                Assert.That(extWritten.Contains("QUANTITY=initialsedfracmysedimentName"));
                Assert.That(extWritten.Contains("FILENAME=mysedimentName_SedConc.xyz"));
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

            //save model
            //check no custom spatially varying op is saved.
            //check sed conc spatially varying is saved.
        }

        [Test]
        public void SaveExtFileWithSpatiallyVaryingSedConcButNoOperationsGeneratesWarningMessage()
        {
            var sedFile = Path.GetTempFileName();
            var generatedXyzFile = Path.Combine(Path.GetDirectoryName(sedFile), "mysedimentName_SedConc." + XyzFile.Extension);
            var extForceFile = Path.Combine(Path.GetDirectoryName(sedFile), "extForceFileTest.ext");

            try
            {
                /* Define new model */
                var fmModel = new WaterFlowFMModel(sedFile);
                fmModel.ModelDefinition.UseMorphologySediment = true;
                var grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
                fmModel.Grid = grid;

                var fraction = new SedimentFraction() { Name = "Frac1" };
                fmModel.SedimentFractions.Add(fraction);

                /* Save ext file */
                var extFile = new ExtForceFile();
                //Save SedFile with no fractions. No warnings should be given.
                TestHelper.AssertLogMessagesCount(() => extFile.Write(extForceFile, fmModel.ModelDefinition), 0);

                //Update model , we need to force it as we are not saving directly from the model but from the ModelDefinition
                var sedConcProp = fraction.CurrentSedimentType.Properties.OfType<ISpatiallyVaryingSedimentProperty>().FirstOrDefault(p => p.Name == "SedConc");
                var initialSpatialOps = new List<string>() { sedConcProp.SpatiallyVaryingName};
                //Add another spatially varying prop -> Warning should be given.
                fmModel.ModelDefinition.SelectSpatialOperations(fmModel.DataItems, fmModel.TracerDefinitions, initialSpatialOps);
                TestHelper.AssertAtLeastOneLogMessagesContains(
                    () => extFile.Write(extForceFile, fmModel.ModelDefinition),
                        String.Format(
                            Resources.SedimentFile_WriteSpatiallyVaryingSedimentPropertySubFiles_No_spatial_operations_of_type_Import__Add_or_Value_found_for_spatially_varying_property__0___Remember_to_interpolate_them_to_generate_the_xyz_file__Otherwise_the_model_might_not_run_as_expected_, 
                            sedConcProp.SpatiallyVaryingName));

                
                //Add a 'value' operation, another warning should be given.
                var dataItem = fmModel.AllDataItems.FirstOrDefault(di => di.Name == sedConcProp.SpatiallyVaryingName);

                // retrieve / create value converter for mysedimentName_SedConc dataitem
                var valueConverter = SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(dataItem, sedConcProp.SpatiallyVaryingName);
                var samples = new SetValueOperation();
                samples.SetInputData(ValueOperationBase.MainInputName, new PointCloudFeatureProvider
                {
                    PointCloud = new PointCloud
                    {
                        PointValues = new List<IPointValue>
                        {
                            new PointValue { X = fmModel.Grid.Cells[0].CenterX, Y = fmModel.Grid.Cells[0].CenterY, Value = 12},
                            new PointValue { X = fmModel.Grid.Cells[1].CenterX, Y = fmModel.Grid.Cells[1].CenterY, Value = 30},
                            new PointValue { X = fmModel.Grid.Cells[2].CenterX, Y = fmModel.Grid.Cells[2].CenterY, Value = 31},
                        },
                    },

                });
                valueConverter.SpatialOperationSet.AddOperation(samples);
                valueConverter.SpatialOperationSet.Execute();

                // update model definition (called during export)
                fmModel.ModelDefinition.SelectSpatialOperations(fmModel.DataItems, fmModel.TracerDefinitions, initialSpatialOps);
                //New warning should be given.
                TestHelper.AssertAtLeastOneLogMessagesContains(
                    () => extFile.Write(extForceFile, fmModel.ModelDefinition),
                        String.Format(
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
        [Category(TestCategory.DataAccess)]
        public void ReadWriteSampleForcingsWaterLevel()
        {
            //is not done via external forcing file
            var def = new WaterFlowFMModelDefinition();
            var initialFieldFile = new InitialFieldFile();

            string validInitial2DWaterLevelIniSectionsFile = TestHelper.GetTestFilePath(@"IO\Initial2DWaterLevel_expected.ini");
            initialFieldFile.Read(validInitial2DWaterLevelIniSectionsFile, validInitial2DWaterLevelIniSectionsFile, def);

            Assert.AreEqual(1, def.GetSpatialOperations(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName).Count);

            // add polygon
            var geometry = new Polygon(new LinearRing(new[]
            {
                new Coordinate(-135, -105),
                new Coordinate(-85, -100),
                new Coordinate(-75, -205),
                new Coordinate(-125, -200),
                new Coordinate(-135, -105)
            }));
            var f = new Feature
            {
                Geometry = geometry,
            };
            var maskCollection = new FeatureCollection(new[] { f }, typeof(Feature));
            var operation = new SetValueOperation
            {
                Name = "poly",
            };
            operation.SetInputData(SpatialOperation.MaskInputName, maskCollection);
            def.GetSpatialOperations(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName).Add(operation);

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
                        },
                    },
                },
            });

            def.GetSpatialOperations(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName).Add(samples);

            var newDef = new WaterFlowFMModelDefinition();

            using (var tempDir = new TemporaryDirectory())
            {
                string newExtPath = Path.Combine(tempDir.Path, "test.ini");

                initialFieldFile.Write(newExtPath, newExtPath, true, def);
                initialFieldFile.Read(newExtPath, newExtPath, newDef);

                Assert.AreEqual(3, newDef.GetSpatialOperations(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName).Count);
                Assert.AreEqual(3, ((ImportSamplesOperation)newDef.GetSpatialOperations(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName)[2]).GetPoints().Count());
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ExportImportBoundaryConditionWithOffsetAndFactor()
        {
            var model = new WaterFlowFMModel();

            var feature = new Feature2D
            {
                Name = "boundary",
                Geometry =
                    new LineString(new [] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0) })
            };

            var bc1 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                BoundaryConditionDataType.AstroComponents)
            {
                Feature = feature,
                Offset = -0.3,
                Factor = 2.5
            };

            bc1.AddPoint(1);
            var data = bc1.GetDataAtPoint(1);
            data["M1"] = new[] { 0.5, 120 };

            bc1.AddPoint(2);
            data = bc1.GetDataAtPoint(2);
            data["M2"] = new[] { 0.7, 60 };
            AddBoundaryCondition(model, bc1);

            var mduPath = Path.GetFullPath(@"exportbc.mdu");

            model.ExportTo(mduPath);

            var importedModel = new WaterFlowFMModel(mduPath);
            var boundaries = importedModel.Boundaries;
            Assert.AreEqual(1, boundaries.Count);
            Assert.AreEqual(feature.Geometry, boundaries.First().Geometry);

            var boundaryConditions = importedModel.BoundaryConditions.ToList();
            Assert.AreEqual(1, boundaryConditions.Count);

            Assert.AreEqual(-0.3, ((FlowBoundaryCondition)boundaryConditions[0]).Offset);
            Assert.AreEqual(2.5, ((FlowBoundaryCondition)boundaryConditions[0]).Factor);

            var pointData1 = ((BoundaryCondition)boundaryConditions[0]).GetDataAtPoint(1);
            Assert.AreEqual(pointData1.Arguments[0].Values.OfType<string>().ToArray(), new[] { "M1" });
            Assert.AreEqual(pointData1.Components[0].Values.OfType<double>().ToArray(), new[] { 0.5 });
            Assert.AreEqual(pointData1.Components[1].Values.OfType<double>().ToArray(), new[] { 120 });

            var pointData2 = ((BoundaryCondition)boundaryConditions[0]).GetDataAtPoint(2);
            Assert.AreEqual(pointData2.Arguments[0].Values.OfType<string>().ToArray(), new[] { "M2" });
            Assert.AreEqual(pointData2.Components[0].Values.OfType<double>().ToArray(), new[] { 0.7 });
            Assert.AreEqual(pointData2.Components[1].Values.OfType<double>().ToArray(), new[] { 60 });
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ExportImportMultipleBoundaryConditionsOnSameFeature()
        {
            var model = new WaterFlowFMModel();
            
            var feature = new Feature2D
                {
                    Name = "boundary",
                    Geometry =
                        new LineString(new [] {new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0)})
                };

            var bc1 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.AstroComponents)
                {
                    Feature = feature
                };

            bc1.AddPoint(1);
            var data = bc1.GetDataAtPoint(1);
            data["M1"] = new[] {0.5, 120};
            
            bc1.AddPoint(2);
            data = bc1.GetDataAtPoint(2);
            data["M2"] = new[] {0.7, 60};
            AddBoundaryCondition(model, bc1);

            var bc2 = new FlowBoundaryCondition(FlowBoundaryQuantityType.NormalVelocity, BoundaryConditionDataType.AstroComponents)
                {
                    Feature = feature
                };
            bc2.AddPoint(1);
            data = bc2.GetDataAtPoint(1);
            data["M1"] = new[] {0.6, 0};
            bc2.AddPoint(2);
            data = bc2.GetDataAtPoint(2);
            data["M2"] = new[] {0.8, 30};
            AddBoundaryCondition(model, bc2);

            var mduPath = Path.GetFullPath(@"exportbcs.mdu");

            model.ExportTo(mduPath);

            var importedModel = new WaterFlowFMModel(mduPath);
            var boundaries = importedModel.Boundaries;
            Assert.AreEqual(1, boundaries.Count);
            Assert.AreEqual(feature.Geometry, boundaries.First().Geometry);

            var boundaryConditions = importedModel.BoundaryConditions.ToList();
            Assert.AreEqual(2, boundaryConditions.Count);

            var pointData1 = ((BoundaryCondition)boundaryConditions[0]).GetDataAtPoint(1);
            Assert.AreEqual(pointData1.Arguments[0].Values.OfType<string>().ToArray(), new[] {"M1"});
            Assert.AreEqual(pointData1.Components[0].Values.OfType<double>().ToArray(), new[] {0.5});
            Assert.AreEqual(pointData1.Components[1].Values.OfType<double>().ToArray(), new[] {120});

            var pointData2 = ((BoundaryCondition)boundaryConditions[0]).GetDataAtPoint(2);
            Assert.AreEqual(pointData2.Arguments[0].Values.OfType<string>().ToArray(), new[] { "M2" });
            Assert.AreEqual(pointData2.Components[0].Values.OfType<double>().ToArray(), new[] { 0.7 });
            Assert.AreEqual(pointData2.Components[1].Values.OfType<double>().ToArray(), new[] { 60 });

            var pointData3 = ((BoundaryCondition)boundaryConditions[1]).GetDataAtPoint(1);
            Assert.AreEqual(pointData3.Arguments[0].Values.OfType<string>().ToArray(), new[] { "M1" });
            Assert.AreEqual(pointData3.Components[0].Values.OfType<double>().ToArray(), new[] { 0.6 });
            Assert.AreEqual(pointData3.Components[1].Values.OfType<double>().ToArray(), new[] { 0 });

            var pointData4 = ((BoundaryCondition)boundaryConditions[1]).GetDataAtPoint(2);
            Assert.AreEqual(pointData4.Arguments[0].Values.OfType<string>().ToArray(), new[] { "M2" });
            Assert.AreEqual(pointData4.Components[0].Values.OfType<double>().ToArray(), new[] { 0.8 });
            Assert.AreEqual(pointData4.Components[1].Values.OfType<double>().ToArray(), new[] { 30 });
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ExportImportSummedWaterLevelsOnSameFeature()
        {
            var model = new WaterFlowFMModel() {Name = "test"};

            var feature = new Feature2D
            {
                Name = "boundary",
                Geometry =
                    new LineString(new [] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0) })
            };

            var bc1 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.AstroComponents)
            {
                Feature = feature
            };

            bc1.AddPoint(1);
            var data = bc1.GetDataAtPoint(1);
            data["M1"] = new[] { 0.5, 120 };

            bc1.AddPoint(2);
            data = bc1.GetDataAtPoint(2);
            data["M2"] = new[] { 0.7, 60 };
            AddBoundaryCondition(model, bc1);

            var bc2 = new FlowBoundaryCondition(FlowBoundaryQuantityType.Discharge, BoundaryConditionDataType.Harmonics)
            {
                Feature = feature
            };
            bc2.AddPoint(1);
            data = bc2.GetDataAtPoint(1);
            data[250.0] = new[] { 0.6, 0 };
            bc2.AddPoint(2);
            data = bc2.GetDataAtPoint(2);
            data[360.0] = new[] { 0.8, 30 };
            AddBoundaryCondition(model, bc2);

            var mduPath = Path.GetFullPath(@"exportwls.mdu");

            model.ExportTo(mduPath);

            
            var path = model.BndExtFilePath;
            var blocks = new IniReader().ReadIniFile(path);
            Assert.AreEqual(2+1, blocks.Count()); // 2 boundaries plus a general block
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadAndWriteSourcesAndSinksTest()
        {
            var def = new WaterFlowFMModelDefinition();

            var extPath = TestHelper.GetTestFilePath(@"c070_sourcesink_2D\sourcesink_2D.ext");

            var extForceFile = new ExtForceFile();
            extForceFile.Read(extPath, def);

            Assert.AreEqual(6,def.Pipes.Count);
            Assert.AreEqual(6, def.SourcesAndSinks.Count);

            Assert.AreEqual(1.5d, def.SourcesAndSinks[0].Area);

            extForceFile.Write("sourcesink.ext", def);

            Assert.IsTrue(File.Exists("sourcesink.ext"));

            Assert.IsTrue(File.Exists("chan2_east_outflow.pli"));
            Assert.IsTrue(File.Exists("chan2_east_outflow.tim"));
        }
    }
}