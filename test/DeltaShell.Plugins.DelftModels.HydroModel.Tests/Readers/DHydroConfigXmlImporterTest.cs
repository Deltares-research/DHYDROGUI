using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Services;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Readers
{
    [TestFixture]
    public class DHydroConfigXmlImporterTest
    {
        private IFileImportService fileImportService;
        private IHydroModelReader hydroModelReader;
        private Func<string> workingDirectoryPathFunc;

        [SetUp]
        public void SetUp()
        {
            fileImportService = Substitute.For<IFileImportService>();
            hydroModelReader = Substitute.For<IHydroModelReader>();
            workingDirectoryPathFunc = () => string.Empty;
        }

        [Test]
        public void Constructor_FileImportServiceIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => _ = new DHydroConfigXmlImporter(null, hydroModelReader, workingDirectoryPathFunc), Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_HydroModelReaderIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => _ = new DHydroConfigXmlImporter(fileImportService, null, workingDirectoryPathFunc), Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_WorkingDirectoryPathFuncIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => _ = new DHydroConfigXmlImporter(fileImportService, hydroModelReader, null), Throws.ArgumentNullException);
        }

        /// <summary>
        /// WHEN SupportedItemTypes is retrieved
        /// THEN a set containing ICompositeActivity is returned
        /// </summary>
        [Test]
        public void WhenSupportedItemTypesIsRetrieved_ThenASetContainingICompositeActivityIsReturned()
        {
            // Given
            DHydroConfigXmlImporter importer = CreateImporter();

            // When
            List<Type> result = importer.SupportedItemTypes.ToList();

            // Then
            Assert.That(result, Is.Not.Null, "Expected the retrieved item types not to be null:");
            Assert.That(result.Count, Is.EqualTo(1), "Expected the supported item types to contain a different number of items:");
            Assert.That(result.Contains(typeof(ICompositeActivity)), Is.True, "Expected supported item types to contain ICompositeActivity");
        }

        /// <summary>
        /// GIVEN an importer with no sub importers
        /// WHEN CanImportOnRootLevel is called
        /// THEN False is returned
        /// </summary>
        [Test]
        public void GivenAnImporterWithNoSubImporters_WhenCanImportOnRootLevelIsCalled_ThenFalseIsReturned()
        {
            // Given
            DHydroConfigXmlImporter importer = CreateImporter();

            // When
            bool result = importer.CanImportOnRootLevel;

            // Then
            Assert.That(result, Is.False);
        }

        /// <summary>
        /// GIVEN a DimrModelFileImporter that cannot import on root level
        /// AND an importer containing this importer
        /// WHEN CanImportOnRootLevel is called
        /// THEN False is returned
        /// </summary>
        [Test]
        public void GivenADimrModelFileImporterThatCannotImportOnRootLevelAndAnImporterContainingThisImporter_WhenCanImportOnRootLevelIsCalled_ThenFalseIsReturned()
        {
            // Given
            var cannotImportRootLevelImporter = Substitute.For<IDimrModelFileImporter>();
            cannotImportRootLevelImporter.CanImportOnRootLevel.Returns(false);

            var importers = new List<IDimrModelFileImporter> { cannotImportRootLevelImporter };
            fileImportService.FileImporters.Returns(importers);

            DHydroConfigXmlImporter importer = CreateImporter();

            // When
            bool result = importer.CanImportOnRootLevel;

            // Then
            Assert.That(result, Is.False);
        }

        /// <summary>
        /// GIVEN a DimrModelFileImporter that can import on root level
        /// AND an importer containing this importer
        /// WHEN CanImportOnRootLevel is called
        /// THEN True is returned
        /// </summary>
        [Test]
        public void GivenADimrModelFileImporterThatCanImportOnRootLevelAndAnImporterContainingThisImporter_WhenCanImportOnRootLevelIsCalled_ThenTrueIsReturned()
        {
            // Given
            var canImportRootLevelImporter = Substitute.For<IDimrModelFileImporter>();
            canImportRootLevelImporter.CanImportOnRootLevel.Returns(true);

            var importers = new List<IDimrModelFileImporter> { canImportRootLevelImporter };
            fileImportService.FileImporters.Returns(importers);

            DHydroConfigXmlImporter importer = CreateImporter();

            // When
            bool result = importer.CanImportOnRootLevel;

            // Then
            Assert.That(result, Is.True);
        }

        /// <summary>
        /// GIVEN a set of DimrModelFileImporter that can and cannot import on root level
        /// AND an importer containing these importers
        /// WHEN CanImportOnRootLevel is called
        /// THEN True is returned
        /// </summary>
        [Test]
        public void GivenASetOfDimrModelFileImporterThatCanAndCannotImportOnRootLevelAndAnImporterContainingTheseImporters_WhenCanImportOnRootLevelIsCalled_ThenTrueIsReturned()
        {
            // Given
            var canImportRootLevelImporter1 = Substitute.For<IDimrModelFileImporter>();
            canImportRootLevelImporter1.CanImportOnRootLevel.Returns(true);
            var canImportRootLevelImporter2 = Substitute.For<IDimrModelFileImporter>();
            canImportRootLevelImporter2.CanImportOnRootLevel.Returns(true);

            var cannotImportRootLevelImporter1 = Substitute.For<IDimrModelFileImporter>();
            cannotImportRootLevelImporter1.CanImportOnRootLevel.Returns(false);
            var cannotImportRootLevelImporter2 = Substitute.For<IDimrModelFileImporter>();
            cannotImportRootLevelImporter2.CanImportOnRootLevel.Returns(false);

            var importers = new List<IDimrModelFileImporter>
            {
                canImportRootLevelImporter1,
                canImportRootLevelImporter2,
                cannotImportRootLevelImporter1,
                cannotImportRootLevelImporter2
            };

            fileImportService.FileImporters.Returns(importers);

            DHydroConfigXmlImporter importer = CreateImporter();

            // When
            bool result = importer.CanImportOnRootLevel;

            // Then
            Assert.That(result, Is.True);
        }

        /// <summary>
        /// GIVEN an importer with no sub importers
        /// WHEN CanImportOn is called on anything
        /// THEN False is returned
        /// </summary>
        [Test]
        public void GivenAnImporterWithNoSubImporters_WhenCanImportOnIsCalledOnAnything_ThenFalseIsReturned()
        {
            // Given
            DHydroConfigXmlImporter importer = CreateImporter();

            var project = new Project();

            // When
            bool result = importer.CanImportOn(project);

            // Then
            Assert.That(result, Is.False);
        }

        /// <summary>
        /// GIVEN a project
        /// AND a subimporter that can import on this project
        /// AND an importer containing this importer
        /// WHEN CanImportOn is called on this project
        /// THEN True is returned
        /// </summary>
        [Test]
        public void GivenAProjectAndASubImporterThatCanImportOnThisProjectAndAnImporterContainingThisImporter_WhenCanImportOnIsCalledOnThisProject_ThenTrueIsReturned()
        {
            // Given
            var project = new Project();

            var canImportOnImporter = Substitute.For<IDimrModelFileImporter>();
            canImportOnImporter.CanImportOn(project).Returns(true);

            var importers = new List<IDimrModelFileImporter> { canImportOnImporter };
            fileImportService.FileImporters.Returns(importers);

            DHydroConfigXmlImporter importer = CreateImporter();

            // When
            bool result = importer.CanImportOn(project);

            // Then
            Assert.That(result, Is.True);
        }

        /// <summary>
        /// GIVEN a project
        /// AND a subimporter that cannot import on this project
        /// AND an importer containing this importer
        /// WHEN CanImportOn is called on this project
        /// THEN False is returned
        /// </summary>
        [Test]
        public void GivenAProjectAndASubImporterThatCannotImportOnThisProjectAndAnImporterContainingThisImporter_WhenCanImportOnIsCalledOnThisProject_ThenFalseIsReturned()
        {
            // Given
            var project = new Project();

            var cannotImportOnImporter = Substitute.For<IDimrModelFileImporter>();
            cannotImportOnImporter.CanImportOn(project).Returns(false);

            var importers = new List<IDimrModelFileImporter> { cannotImportOnImporter };
            fileImportService.FileImporters.Returns(importers);

            DHydroConfigXmlImporter importer = CreateImporter();

            // When
            bool result = importer.CanImportOn(project);

            // Then
            Assert.That(result, Is.False);
        }

        /// <summary>
        /// GIVEN a project
        /// AND a set of subimporters that can and cannot import on this project
        /// AND an importer containing this importer
        /// WHEN CanImportOn is called on this project
        /// THEN True is returned
        /// </summary>
        [Test]
        public void GivenAProjectAndASetOfSubImportersThatCanAndCannotImportOnThisProjectAndAnImporterContainingThisImporter_WhenCanImportOnIsCalledOnThisProject_ThenTrueIsReturned()
        {
            // Given
            var project = new Project();

            var canImportImporter1 = Substitute.For<IDimrModelFileImporter>();
            canImportImporter1.CanImportOn(project).Returns(true);

            var canImportImporter2 = Substitute.For<IDimrModelFileImporter>();
            canImportImporter2.CanImportOn(project).Returns(true);

            var cannotImportImporter1 = Substitute.For<IDimrModelFileImporter>();
            cannotImportImporter1.CanImportOn(project).Returns(false);
            var cannotImportImporter2 = Substitute.For<IDimrModelFileImporter>();
            cannotImportImporter2.CanImportOn(project).Returns(false);

            var importers = new List<IDimrModelFileImporter>
            {
                canImportImporter1,
                canImportImporter2,
                cannotImportImporter1,
                cannotImportImporter2
            };

            fileImportService.FileImporters.Returns(importers);

            DHydroConfigXmlImporter importer = CreateImporter();

            // When
            bool result = importer.CanImportOn(project);

            // Then
            Assert.That(result, Is.True);
        }

        /// <summary>
        /// WHEN FileFilter is retrieved
        /// THEN the expected FileFilter is returned
        /// </summary>
        [Test]
        public void WhenFileFilterIsRetrieved_ThenTheExpectedFileFilterIsReturned()
        {
            // Given
            const string expectedValue = "xml|*.xml";

            DHydroConfigXmlImporter importer = CreateImporter();

            // When
            string result = importer.FileFilter;

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
            // Given
            DHydroConfigXmlImporter importer = CreateImporter();

            // When
            bool result = importer.OpenViewAfterImport;

            // Then
            Assert.That(result, Is.True);
        }

        /// <summary>
        /// WHEN ImportItem is called
        /// AND an expected error is thrown
        /// THEN a message is logged
        /// AND null is returned
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

            hydroModelReader.Read(path).Throws((Exception)Activator.CreateInstance(exceptionType));

            DHydroConfigXmlImporter importer = CreateImporter();

            // When | Then
            HydroModel result = null;

            var expectedMessage = $"An error occurred while trying to import a {importer.Name}:";

            TestHelper.AssertAtLeastOneLogMessagesContains(() => result = (HydroModel)importer.ImportItem(path), expectedMessage);
            Assert.That(result, Is.Null, "Expected ImportItem to file upon reading and return null:");
        }

        /// <summary>
        /// WHEN ImportItem is called
        /// AND an unexpected error is thrown
        /// THEN this error is propagated
        /// </summary>
        [Test]
        public void WhenImportItemIsCalledAndAnUnexpectedErrorIsThrown_ThenThisErrorIsPropagated()
        {
            // Given
            const string path = "somePath";
            const string errorMsg = "Uncaught Exception";

            hydroModelReader.Read(path).Throws(new Exception(errorMsg));

            DHydroConfigXmlImporter importer = CreateImporter();

            Assert.Throws<Exception>(() => importer.ImportItem(path), errorMsg);
        }

        /// <summary>
        /// GIVEN a HydroModel
        /// AND some path
        /// AND a null target
        /// WHEN ImportItem is called with these parameters
        /// AND this model is read
        /// THEN this model is returned
        /// </summary>
        [Test]
        public void GivenSomePathAndANullTarget_WhenImportItemIsCalledWithTheseParameters_ThenThisModelIsReturned()
        {
            // Given
            const string path = "somePath";

            var model = new HydroModel();
            hydroModelReader.Read(path).Returns(model);

            DHydroConfigXmlImporter importer = CreateImporter();

            // When
            object result = importer.ImportItem(path);

            // Then
            Assert.That(result, Is.EqualTo(model), "Expected returned model to be equal to the read function result:");
        }

        /// <summary>
        /// GIVEN a HydroModel
        /// AND some path
        /// AND some target folder
        /// WHEN ImportItem is called with these parameters
        /// AND this new model is read
        /// THEN this new model is returned
        /// AND the folder contains the model
        /// </summary>
        [Test]
        public void GivenSomePathAndSomeTargetFolder_WhenImportItemIsCalledWithTheseParameters_ThenThisNewModelIsReturnedAndTheFolderContainsTheModel()
        {
            // Given
            const string path = "somePath";

            var folder = new Folder();
            var model = new HydroModel();

            hydroModelReader.Read(path).Returns(model);

            DHydroConfigXmlImporter importer = CreateImporter();

            // When
            var result = (HydroModel)importer.ImportItem(path, folder);

            // Then
            Assert.That(result, Is.EqualTo(model), "Expected returned model to be equal to the read function result:");
            Assert.That(result.Owner(), Is.EqualTo(folder), "Expected the owner of the returned model to be equal to the provided target:");
            Assert.That(folder.Items.Contains(result), "Expected the folder to contain the newly read model");
        }

        /// <summary>
        /// GIVEN a HydroModel
        /// AND some path
        /// AND some target HydroModel with a folder owner
        /// WHEN ImportItem is called with these parameters
        /// AND this new model is read
        /// THEN this new model is returned
        /// AND the target model has been replaced in the folder
        /// </summary>
        [Test]
        public void GivenSomePathAndSomeTargetHydroModelWithAFolderOwner_WhenImportItemIsCalledWithTheseParameters_ThenThisNewModelIsReturnedAndTheTargetModelHasBeenReplacedInTheFolder()
        {
            // Given
            const string path = "somePath";

            var folder = new Folder();
            var model = new HydroModel();
            var prevModel = new HydroModel();
            folder.Add(prevModel);

            hydroModelReader.Read(path).Returns(model);

            DHydroConfigXmlImporter importer = CreateImporter();

            // When
            var result = (HydroModel)importer.ImportItem(path, prevModel);

            // Then
            Assert.That(result, Is.EqualTo(model), "Expected returned model to be equal to the read function result:");
            Assert.That(result.Owner(), Is.EqualTo(folder), "Expected the owner of the returned model to be equal to the provided target:");
            Assert.That(folder.Items.Contains(prevModel), Is.False, "Expected folder not to contain the previous WaterFlowModel1D");
            Assert.That(folder.Items.Contains(result), "Expected the folder to contain the newly read model");
        }

        /// <summary>
        /// GIVEN a HydroModel
        /// AND some path
        /// WHEN ShouldCancel is set
        /// AND ImportItem is called with these parameters
        /// THEN null is returned
        /// </summary>
        [Test]
        public void GivenSomePath_WhenShouldCancelIsSetAndImportItemIsCalledWithTheseParameters_ThenNullIsReturned()
        {
            // Given
            var model = new HydroModel();

            const string path = "somePath";

            hydroModelReader.Read(path).Returns(model);

            DHydroConfigXmlImporter importer = CreateImporter();
            importer.ShouldCancel = true;

            // When
            object result = importer.ImportItem(path);

            // Then
            Assert.That(result, Is.EqualTo(null), "Expected returned model to be null:");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportItem_ShouldSetWorkingDirectoryPathFunInImportedHydroModel()
        {
            // Arrange
            workingDirectoryPathFunc = () => "TestWorkingDirectory";
            hydroModelReader = new HydroModelReader(fileImportService);

            DHydroConfigXmlImporter importer = CreateImporter();

            using (var tempDirectory = new TemporaryDirectory())
            {
                string dimrFilePathInTemp = tempDirectory.CopyTestDataFileAndDirectoryToTempDirectory(Path.Combine("FileReader", "dimr.xml"));

                // Act
                object importedModel = importer.ImportItem(dimrFilePathInTemp);

                // Assert
                Assert.AreEqual("TestWorkingDirectory", ((HydroModel)importedModel).WorkingDirectoryPathFunc());
            }
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportTestWithoutActivities()
        {
            string dimrXmlPath = TestHelper.GetTestFilePath(Path.Combine("FileReader", "dimr.xml"));

            hydroModelReader = new HydroModelReader(fileImportService);
            DHydroConfigXmlImporter importer = CreateImporter();

            object model = importer.ImportItem(dimrXmlPath);

            Assert.IsNotNull(model);
            Assert.That(model, Is.TypeOf<HydroModel>());
        }

        private DHydroConfigXmlImporter CreateImporter()
        {
            return new DHydroConfigXmlImporter(fileImportService, hydroModelReader, workingDirectoryPathFunc);
        }
    }
}