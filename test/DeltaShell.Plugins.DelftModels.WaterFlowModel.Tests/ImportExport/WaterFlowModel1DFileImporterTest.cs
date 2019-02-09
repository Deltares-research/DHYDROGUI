using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using NUnit.Framework;
using Rhino.Mocks;


namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport
{
    [TestFixture]
    public class WaterFlowModel1DFileImporterTest
    {
        private WaterFlowModel1DFileImporter Importer;

        [TestFixtureSetUp]
        public void SetUpFixture()
        {
            Importer = new WaterFlowModel1DFileImporter();
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void GivenAnMd1dFile_WhenImportingFileAndExporting_ThenTheInputFilesAreTheSameAsTheOutputFiles11()
        {
            // TODO: Add BoundaryConditions.bc, CrossSectionDefinitions.ini, CrossSectionLocations.ini, NetworkDefinition.ini, ObservationPoints.ini & the md1d file
            var md1dFilePath = TestHelper.GetTestFilePath(@"ImportSpatialData\water flow 1d.md1d");
            var testDirectory = FileUtils.CreateTempDirectory();
            var errorMessage = "Files not equal";
            var targetFilePath = Path.Combine(testDirectory, "FileWriters");
            var fileCollection = new List<string>
            {
                "BoundaryLocations.ini",
                "Dispersion.ini",
                "DispersionF3.ini",
                "DispersionF4.ini",
                "InitialDischarge.ini",
                "InitialSalinity.ini",
                "InitialTemperature.ini",
                "InitialWaterLevel.ini",
                "LateralDischargeLocations.ini",
                "Retention.ini",
                "roughness-FloodPlain1 (Reversed).ini",
                "roughness-FloodPlain1.ini",
                "roughness-Main (Reversed).ini",
                "roughness-Main.ini",
                "Salinity.ini",
                "sobeksim.fnm",
                "SobekSim.ini",
                "Structures.ini",
                "WindShielding.ini"
            };

            try
            {
                var importer = new WaterFlowModel1DFileImporter
                {
                    ProgressChanged = (name, step, steps) => { }
                };
                var model = importer.ImportItem(md1dFilePath) as WaterFlowModel1D;
                Assert.IsNotNull(model);

                WaterFlowModel1DFileWriter.Write(Path.Combine(targetFilePath, ModelFileNames.ModelDefinitionFilename), model);

                foreach (var file in fileCollection)
                {
                    var iniFilePath = Path.Combine(targetFilePath, file);
                    Assert.That(() => FileComparer.Compare(iniFilePath,
                            TestHelper.GetTestFilePath($@"ImportSpatialData\{file}"), out errorMessage), Is.True,
                        $"processed file:{file} not equal to the model counterpart. This means that the file has not been correctly imported or written");
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(testDirectory);
            }
        }

        #region PropertyTests
        /// <summary>
        /// WHEN SupportedItemTypes is retrieved
        /// THEN a set containing only IHydroModel is returned
        /// </summary>
        [Test]
        public void WhenSupportedItemTypesIsRetrieved_ThenASetContainingOnlyIHydroModelIsReturned()
        {
            // When
            var supportedItemTypes = Importer.SupportedItemTypes.ToList();

            // Then
            Assert.That(supportedItemTypes, Is.Not.Null, "Expected the supported item types not to be null.");
            Assert.That(supportedItemTypes.Count, Is.EqualTo(1), "Expected a different number of of supported item types:");
            Assert.That(supportedItemTypes.First(), Is.EqualTo(typeof(IHydroModel)), "Expected the supported item type to be different:");
        }

        /// <summary>
        /// WHEN CanImportOnRootLevelIsRetrieved
        /// THEN True is returned
        /// </summary>
        [Test]
        public void WhenCanImportOnRootLevelIsRetrieved_ThenTrueIsReturned()
        {
            // When
            var result = Importer.CanImportOnRootLevel;

            // Then
            Assert.That(result, Is.True, "Expected CanImportOnRootLevel to be true:");

        }

        /// <summary>
        /// GIVEN an ICompositeActivityObject
        /// WHEN CanImportOn this object is called
        /// THEN true is returned
        /// </summary>
        [Test]
        public void GivenAnICompositeActivityObject_WhenCanImportOnThisObjectIsCalled_ThenTrueIsReturned()
        {
            var compositeActivity = Rhino.Mocks.MockRepository.GenerateStub<ICompositeActivity>();

            // When
            var result = Importer.CanImportOn(compositeActivity);

            Assert.That(result, Is.True);
        }

        /// <summary>
        /// GIVEN a WaterFlowModel1D
        /// WHEN CanImportOn this object is called
        /// THEN true is returned
        /// </summary>
        [Test]
        public void GivenAWaterFlowModel1D_WhenCanImportOnThisObjectIsCalled_ThenTrueIsReturned()
        {
            var currentModel = new WaterFlowModel1D();

            // When
            var result = Importer.CanImportOn(currentModel);

            Assert.That(result, Is.True);
        }

        /// <summary>
        /// GIVEN an Object not WaterFlowModel1D or ICompositeActivityObject
        /// WHEN CanImportOn this object is called
        /// THEN false is returned
        /// </summary>
        [Test]
        public void GivenAnObjectNotWaterFlowModel1DOrICompositeActivityObject_WhenCanImportOnThisObjectIsCalled_ThenFalseIsReturned()
        {
            // When
            var result = Importer.CanImportOn(this);

            // Then
            Assert.That(result, Is.False);
        }

        /// <summary>
        /// WHEN FileFilter is retrieved
        /// THEN the expected FileFilter is returned
        /// </summary>
        [Test]
        public void WhenFileFilterIsRetrieved_ThenTheExpectedFileFilterIsReturned()
        {
            const string expectedValue = "md1d|*.md1d";

            // When
            var result = Importer.FileFilter;

            // Then
            Assert.That(result, Is.EqualTo(expectedValue), "Expected a different FileFilter:");
        }

        /// <summary>
        /// WHEN OpenViewAfterImport is retrieved
        /// THEN true should be returned
        /// </summary>
        [Test]
        public void WhenOpenViewAfterImportIsRetrieved_ThenTrueShouldBeReturned()
        {
            // When
            var result = Importer.OpenViewAfterImport;

            // Then
            Assert.That(result, Is.True);
        }

        /// <summary>
        /// WHEN MasterFileExtension is retrieved
        /// THEN the expected MasterFileExtension should be returned
        /// </summary>
        [Test]
        public void WhenMasterFileExtensionIsRetrieved_ThenTheExpectedMasterFileExtensionShouldBeReturned()
        {
            const string expectedValue = "md1d";

            // When
            var result = Importer.MasterFileExtension;

            // Then
            Assert.That(result, Is.EqualTo(expectedValue), "Expected a different MasterFileExtension:");
        }

        /// <summary>
        /// WHEN the subfolders are retrieved
        /// THEN a set containing dflow1d should be returned
        /// </summary>
        [Test]
        public void WhenTheSubfoldersAreRetrieved_ThenASetContainingDflow1dShouldBeReturned()
        {
            const string expectedValue = "dflow1d";

            // When
            var result = Importer.SubFolders.ToList();

            // Then
            Assert.That(result, Is.Not.Null, "Expected the subfolders to not be null.");
            Assert.That(result.Count, Is.EqualTo(1), "Expected a different number of subfolders:");
            Assert.That(result.First(), Is.EqualTo(expectedValue), "Expected a different subfolder:");
        }
        #endregion

        #region ImportItem Tests
        /// <summary>
        /// WHEN ImportItem is called
        ///  AND an expected error is thrown
        /// THEN a message is logged
        ///  AND null is returned
        /// </summary>
        [TestCase(typeof(ArgumentException))]
        [TestCase(typeof(PathTooLongException))]
        [TestCase(typeof(FormatException))]
        [TestCase(typeof(OutOfMemoryException))]
        [TestCase(typeof(IOException))]
        [TestCase(typeof(InvalidOperationException))]
        public void WhenImportItemIsCalledAndAnExpectedErrorIsThrown_ThenAMessageIsLoggedAndNullIsReturned(Type exceptionType)
        {
            // Given
            const string path = "somePath";

            var readFunc = 
                MockRepository.GenerateStrictMock<Func<string, Action<string, int, int>, WaterFlowModel1D>>();
            readFunc.Expect(f => f.Invoke(null, null))
                    .IgnoreArguments()
                    .Throw((Exception) Activator.CreateInstance(exceptionType))
                    .Repeat.Any();

            var importer = new WaterFlowModel1DFileImporter(readFunc);

            // When | Then

            WaterFlowModel1D result = null;
            TestHelper.AssertLogMessageIsGenerated(() =>
            {
                result = (WaterFlowModel1D) importer.ImportItem(path);
            }, string.Format(Resources.WaterFlowModel1DFileImporter_ImportItem_An_error_occurred_while_trying_to_import_a__0___, importer.Name), 1);

            readFunc.VerifyAllExpectations();
            Assert.That(result, Is.Null, "Expected ImportItem to file upon reading and return null:");
        }

        /// <summary>
        /// WHEN ImportItem is called
        ///  AND an unexpected error is thrown
        /// THEN this error is propagated
        /// </summary>
        [Test]
        public void WhenImportItemIsCalledAndAnUnexpectedErrorIsThrown_ThenThisErrorIsPropagated()
        {
            // Given
            const string path = "somePath";
            const string errorMsg = "Uncaught Exception";

            var readFunc =
                MockRepository.GenerateStrictMock<Func<string, Action<string, int, int>, WaterFlowModel1D>>();
            readFunc.Expect(f => f.Invoke(null, null))
                    .IgnoreArguments()
                    .Throw(new Exception(errorMsg))
                    .Repeat.Any();

            var importer = new WaterFlowModel1DFileImporter(readFunc);

            Assert.Throws<Exception>(() => importer.ImportItem(path), errorMsg);
            readFunc.VerifyAllExpectations();
        }


        /// <summary>
        /// GIVEN a WaterFlowModel1D
        ///   AND some path
        ///   AND a null target
        /// WHEN ImportItem is called with these parameters
        ///  AND this model is read
        /// THEN this model is returned
        /// </summary>
        [Test]
        public void GivenAWaterFlowModel1DAndSomePathAndANullTarget_WhenImportItemIsCalledWithTheseParametersAndThisModelIsRead_ThenThisModelIsReturned()
        {
            // Given
            var model = new WaterFlowModel1D("potato");

            const string path = "somePath";

            var readFunc =
                MockRepository.GenerateStrictMock<Func<string, Action<string, int, int>, WaterFlowModel1D>>();
            readFunc.Expect(f => f.Invoke(null, null))
                    .IgnoreArguments()
                    .Return(model)
                    .Repeat.AtLeastOnce();
            var importer = new WaterFlowModel1DFileImporter(readFunc);

            // When
            var result = importer.ImportItem(path, null);

            // Then
            readFunc.VerifyAllExpectations();

            Assert.That(result, Is.EqualTo(model), "Expected returned model to be equal to read function result:");
        }

        /// <summary>
        /// GIVEN a WaterFlowModel1D
        ///   AND some path
        ///   AND some target WaterModel1D with a folder owner
        /// WHEN ImportItem is called with these parameters
        ///  AND this new model is read
        /// THEN this new model is returned
        ///  AND the target model has been replaced in the folder
        /// </summary>
        [Test]
        public void GivenAWaterFlowModel1DAndSomePathAndSomeTargetWaterModel1DWithAFolderOwner_WhenImportItemIsCalledWithTheseParametersAndThisNewModelIsRead_ThenThisNewModelIsReturnedAndTheTargetModelHasBeenReplacedInTheFolder()
        {
            // Given
            var folder = new Folder();
            var prevModel = new WaterFlowModel1D("definitely-not-a-potato") { Owner = folder };

            var model = new WaterFlowModel1D("potato");

            const string path = "somePath";

            var readFunc =
                MockRepository.GenerateStrictMock<Func<string, Action<string, int, int>, WaterFlowModel1D>>();
            readFunc.Expect(f => f.Invoke(null, null))
                    .IgnoreArguments()
                    .Return(model)
                    .Repeat.AtLeastOnce();
            var importer = new WaterFlowModel1DFileImporter(readFunc);

            // When
            var result = (WaterFlowModel1D) importer.ImportItem(path, prevModel);

            // Then
            readFunc.VerifyAllExpectations();

            Assert.That(result, Is.EqualTo(model), "Expected returned model to be equal to read function result:");
            Assert.That(result.Owner(), Is.EqualTo(folder), "Expected the owner of the returned model to be equal to the provided target:");
            Assert.That(folder.Items.Contains(prevModel), Is.False, "Expected folder not to contain the previous WaterFlowModel1D");
        }

        /// <summary>
        /// GIVEN a WaterFlowModel1D
        ///   AND some path
        ///   AND some target Folder
        /// WHEN ImportItem is called with these parameters
        ///  AND this new model is read
        /// THEN this new model is returned
        ///  AND the target model is in the Folder
        /// </summary>
        [Test]
        public void GivenAWaterFlowModel1DAndSomePathAndSomeTargetFolder_WhenImportItemIsCalledWithTheseParametersAndThisNewModelIsRead_ThenThisNewModelIsReturnedAndTheTargetModelIsInTheFolder()
        {
            // Given
            var folder = new Folder();
            var model = new WaterFlowModel1D("potato");

            const string path = "somePath";

            var readFunc =
                MockRepository.GenerateStrictMock<Func<string, Action<string, int, int>, WaterFlowModel1D>>();
            readFunc.Expect(f => f.Invoke(null, null))
                    .IgnoreArguments()
                    .Return(model)
                    .Repeat.AtLeastOnce();
            var importer = new WaterFlowModel1DFileImporter(readFunc);

            // When
            var result = (WaterFlowModel1D)importer.ImportItem(path, folder);

            // Then
            readFunc.VerifyAllExpectations();

            Assert.That(result, Is.EqualTo(model), "Expected returned model to be equal to read function result:");
            Assert.That(result.Owner(), Is.EqualTo(folder), "Expected the owner of the returned model to be equal to the provided target:");
        }


        /// <summary>
        /// GIVEN a WaterFlowModel1D
        ///   AND some path
        /// WHEN ShouldCancel is set
        ///  AND ImportItem is called with these parameters
        /// THEN null is returned
        /// </summary>
        [Test]
        public void GivenAWaterFlowModel1DAndSomePath_WhenShouldCancelIsSetAndImportItemIsCalledWithTheseParameters_ThenNullIsReturned()
        {
            // Given
            var model = new WaterFlowModel1D("potato");

            const string path = "somePath";

            var readFunc =
                MockRepository.GenerateStrictMock<Func<string, Action<string, int, int>, WaterFlowModel1D>>();
            readFunc.Expect(f => f.Invoke(null, null))
                    .IgnoreArguments()
                    .Return(model)
                    .Repeat.AtLeastOnce();
            var importer = new WaterFlowModel1DFileImporter(readFunc)
            {
                ShouldCancel = true
            };

            // When
            var result = importer.ImportItem(path, null);

            // Then
            readFunc.VerifyAllExpectations();

            Assert.That(result, Is.EqualTo(null), "Expected returned model to be null:");

        }
        #endregion
    }
}