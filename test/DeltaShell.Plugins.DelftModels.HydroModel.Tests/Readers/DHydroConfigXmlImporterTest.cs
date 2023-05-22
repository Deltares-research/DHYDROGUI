using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using log4net.Core;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Readers
{
    [TestFixture]
    public class DHydroConfigXmlImporterTest
    {
        [Test]
        public void ImportTestWithoutActivities()
        {
            string dimrXmlPath = TestHelper.GetTestFilePath(Path.Combine("FileReader", "dimr.xml"));

            var mocks = new MockRepository();
            object importers = mocks.DynamicMock(typeof(Func<List<IDimrModelFileImporter>>));
            object workingDirectory = mocks.DynamicMock(typeof(Func<string>));
            var importer = mocks.DynamicMock<DHydroConfigXmlImporter>(importers, workingDirectory);

            object model = importer.ImportItem(dimrXmlPath);

            Assert.IsNotNull(model);
            Assert.That(model, Is.TypeOf<HydroModel>());
        }

        [Test]
        public void Constructor_WhenGetWorkingDirectoryPathFuncIsNull_ShouldThrownArgumentNullException()
        {
            void Call() => new DHydroConfigXmlImporter(() => new List<IDimrModelFileImporter>(), null);
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("getWorkingDirectoryPathFunc"));
        }

        /// <summary>
        /// WHEN SupportedItemTypes is retrieved
        /// THEN a set containing ICompositeActivity is returned
        /// </summary>
        [Test]
        public void WhenSupportedItemTypesIsRetrieved_ThenASetContainingICompositeActivityIsReturned()
        {
            // Given
            var importers = new List<IDimrModelFileImporter>();
            var importer = new DHydroConfigXmlImporter(() => importers, () => null);

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
            var importers = new List<IDimrModelFileImporter>();
            var importer = new DHydroConfigXmlImporter(() => importers, () => null);

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
            var cannotImportRootLevelImporter = MockRepository.GenerateStrictMock<IDimrModelFileImporter>();
            cannotImportRootLevelImporter.Expect(imp => imp.CanImportOnRootLevel).Return(false).Repeat.Any();

            var importers = new List<IDimrModelFileImporter>() {cannotImportRootLevelImporter};
            var importer = new DHydroConfigXmlImporter(() => importers, () => null);

            // When
            bool result = importer.CanImportOnRootLevel;

            // Then
            cannotImportRootLevelImporter.VerifyAllExpectations();
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
            var canImportRootLevelImporter = MockRepository.GenerateStrictMock<IDimrModelFileImporter>();
            canImportRootLevelImporter.Expect(imp => imp.CanImportOnRootLevel).Return(true).Repeat.Any();

            var importers = new List<IDimrModelFileImporter>() {canImportRootLevelImporter};
            var importer = new DHydroConfigXmlImporter(() => importers, () => null);

            // When
            bool result = importer.CanImportOnRootLevel;

            // Then
            canImportRootLevelImporter.VerifyAllExpectations();
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
            var canImportRootLevelImporter1 = MockRepository.GenerateStrictMock<IDimrModelFileImporter>();
            canImportRootLevelImporter1.Expect(imp => imp.CanImportOnRootLevel).Return(true).Repeat.Any();
            var canImportRootLevelImporter2 = MockRepository.GenerateStrictMock<IDimrModelFileImporter>();
            canImportRootLevelImporter2.Expect(imp => imp.CanImportOnRootLevel).Return(true).Repeat.Any();

            var cannotImportRootLevelImporter1 = MockRepository.GenerateStrictMock<IDimrModelFileImporter>();
            cannotImportRootLevelImporter1.Expect(imp => imp.CanImportOnRootLevel).Return(false).Repeat.Any();
            var cannotImportRootLevelImporter2 = MockRepository.GenerateStrictMock<IDimrModelFileImporter>();
            cannotImportRootLevelImporter2.Expect(imp => imp.CanImportOnRootLevel).Return(false).Repeat.Any();

            var importers = new List<IDimrModelFileImporter>()
            {
                canImportRootLevelImporter1,
                canImportRootLevelImporter2,
                cannotImportRootLevelImporter1,
                cannotImportRootLevelImporter2
            };

            var importer = new DHydroConfigXmlImporter(() => importers, () => null);

            // When
            bool result = importer.CanImportOnRootLevel;

            // Then
            canImportRootLevelImporter1.VerifyAllExpectations();
            canImportRootLevelImporter2.VerifyAllExpectations();
            cannotImportRootLevelImporter1.VerifyAllExpectations();
            cannotImportRootLevelImporter2.VerifyAllExpectations();

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
            var project = new Project();

            var importers = new List<IDimrModelFileImporter>();
            var importer = new DHydroConfigXmlImporter(() => importers, () => null);

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
        public void GivenAProjectAndASubimporterThatCanImportOnThisProjectAndAnImporterContainingThisImporter_WhenCanImportOnIsCalledOnThisProject_ThenTrueIsReturned()
        {
            // Given
            var project = new Project();

            var canImportOnImporter = MockRepository.GenerateStrictMock<IDimrModelFileImporter>();
            canImportOnImporter.Expect(imp => imp.CanImportOn(Arg<Project>.Is.Equal(project))).Return(true).Repeat.AtLeastOnce();

            var importers = new List<IDimrModelFileImporter>() {canImportOnImporter};
            var importer = new DHydroConfigXmlImporter(() => importers, () => null);

            // When
            bool result = importer.CanImportOn(project);

            // Then
            canImportOnImporter.VerifyAllExpectations();
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
        public void GivenAProjectAndASubimporterThatCannotImportOnThisProjectAndAnImporterContainingThisImporter_WhenCanImportOnIsCalledOnThisProject_ThenFalseIsReturned()
        {
            // Given
            var project = new Project();

            var cannotImportOnImporter = MockRepository.GenerateStrictMock<IDimrModelFileImporter>();
            cannotImportOnImporter.Expect(imp => imp.CanImportOn(Arg<Project>.Is.Equal(project))).Return(false).Repeat.AtLeastOnce();

            var importers = new List<IDimrModelFileImporter>() {cannotImportOnImporter};
            var importer = new DHydroConfigXmlImporter(() => importers, () => null);

            // When
            bool result = importer.CanImportOn(project);

            // Then
            cannotImportOnImporter.VerifyAllExpectations();
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
        public void GivenAProjectAndASetOfSubimportersThatCanAndCannotImportOnThisProjectAndAnImporterContainingThisImporter_WhenCanImportOnIsCalledOnThisProject_ThenTrueIsReturned()
        {
            // Given
            var project = new Project();

            var canImportImporter1 = MockRepository.GenerateStrictMock<IDimrModelFileImporter>();
            canImportImporter1.Expect(imp => imp.CanImportOn(Arg<Project>.Is.Equal(project))).Return(true).Repeat.Any();

            var canImportImporter2 = MockRepository.GenerateStrictMock<IDimrModelFileImporter>();
            canImportImporter2.Expect(imp => imp.CanImportOn(Arg<Project>.Is.Equal(project))).Return(true).Repeat.Any();

            var cannotImportImporter1 = MockRepository.GenerateStrictMock<IDimrModelFileImporter>();
            cannotImportImporter1.Expect(imp => imp.CanImportOn(Arg<Project>.Is.Equal(project))).Return(false).Repeat.Any();
            var cannotImportImporter2 = MockRepository.GenerateStrictMock<IDimrModelFileImporter>();
            cannotImportImporter2.Expect(imp => imp.CanImportOn(Arg<Project>.Is.Equal(project))).Return(false).Repeat.Any();

            var importers = new List<IDimrModelFileImporter>()
            {
                canImportImporter1,
                canImportImporter2,
                cannotImportImporter1,
                cannotImportImporter2
            };

            var importer = new DHydroConfigXmlImporter(() => importers, () => null);

            // When
            bool result = importer.CanImportOn(project);

            // Then
            canImportImporter1.VerifyAllExpectations();
            canImportImporter2.VerifyAllExpectations();
            cannotImportImporter1.VerifyAllExpectations();
            cannotImportImporter2.VerifyAllExpectations();

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
            var importer = new DHydroConfigXmlImporter(() => new List<IDimrModelFileImporter>(), () => null);

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
            var importer = new DHydroConfigXmlImporter(() => new List<IDimrModelFileImporter>(), () => null);

            // When
            bool result = importer.OpenViewAfterImport;

            // Then
            Assert.That(result, Is.True);
        }

        #region ImportItem Tests

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

            var readFunc =
                MockRepository.GenerateStrictMock<DHydroConfigXmlImporter.ReadDimrModelFunction>();
            readFunc.Expect(f => f.Invoke(null, null))
                    .IgnoreArguments()
                    .Throw((Exception) Activator.CreateInstance(exceptionType))
                    .Repeat.Any();

            var importer = new DHydroConfigXmlImporter(readFunc,
                                                       () => new List<IDimrModelFileImporter>(), () => null);

            // When | Then
            void Call() => importer.ImportItem(path);

            string error = TestHelper.GetAllRenderedMessages(Call, Level.Error).Single();
            Assert.That(error, Does.StartWith($"An error occurred while trying to import a {importer.Name}:"));
            readFunc.VerifyAllExpectations();
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

            var readFunc =
                MockRepository.GenerateStrictMock<DHydroConfigXmlImporter.ReadDimrModelFunction>();
            readFunc.Expect(f => f.Invoke(null, null))
                    .IgnoreArguments()
                    .Throw(new Exception(errorMsg))
                    .Repeat.Any();

            var importer = new DHydroConfigXmlImporter(readFunc,
                                                       () => new List<IDimrModelFileImporter>(), () => null);

            Assert.Throws<Exception>(() => importer.ImportItem(path), errorMsg);
            readFunc.VerifyAllExpectations();
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
            var model = new HydroModel();

            const string path = "somePath";

            var readFunc =
                MockRepository.GenerateStrictMock<DHydroConfigXmlImporter.ReadDimrModelFunction>();

            readFunc.Expect(f => f.Invoke(null, null))
                    .IgnoreArguments()
                    .Return(model)
                    .Repeat.Once();

            var importer = new DHydroConfigXmlImporter(readFunc,
                                                       () => new List<IDimrModelFileImporter>(), () => null);

            // When
            object result = importer.ImportItem(path, null);

            // Then
            readFunc.VerifyAllExpectations();

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
            var folder = new Folder();
            var model = new HydroModel();

            const string path = "somePath";

            var readFunc =
                MockRepository.GenerateStrictMock<DHydroConfigXmlImporter.ReadDimrModelFunction>();

            readFunc.Expect(f => f.Invoke(null, null))
                    .IgnoreArguments()
                    .Return(model)
                    .Repeat.Once();

            var importer = new DHydroConfigXmlImporter(readFunc,
                                                       () => new List<IDimrModelFileImporter>(), () => null);

            // When
            var result = (HydroModel) importer.ImportItem(path, folder);

            // Then
            readFunc.VerifyAllExpectations();

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
            var folder = new Folder();
            var prevModel = new HydroModel();
            folder.Add(prevModel);

            var model = new HydroModel();

            const string path = "somePath";

            var readFunc =
                MockRepository.GenerateStrictMock<DHydroConfigXmlImporter.ReadDimrModelFunction>();

            readFunc.Expect(f => f.Invoke(null, null))
                    .IgnoreArguments()
                    .Return(model)
                    .Repeat.Once();

            var importer = new DHydroConfigXmlImporter(readFunc,
                                                       () => new List<IDimrModelFileImporter>(), () => null);

            // When
            var result = (HydroModel) importer.ImportItem(path, prevModel);

            // Then
            readFunc.VerifyAllExpectations();

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

            var readFunc =
                MockRepository.GenerateStrictMock<DHydroConfigXmlImporter.ReadDimrModelFunction>();

            readFunc.Expect(f => f.Invoke(null, null))
                    .IgnoreArguments()
                    .Return(model)
                    .Repeat.Once();

            var importer = new DHydroConfigXmlImporter(readFunc,
                                                       () => new List<IDimrModelFileImporter>(), () => null) {ShouldCancel = true};

            // When
            object result = importer.ImportItem(path, null);

            // Then
            readFunc.VerifyAllExpectations();

            Assert.That(result, Is.EqualTo(null), "Expected returned model to be null:");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportItem_ShouldSetWorkingDirectoryPathFunInImportedHydroModel()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                string dimrFilePathInTemp = tempDirectory.CopyTestDataFileAndDirectoryToTempDirectory(Path.Combine("FileReader", "dimr.xml"));
                const string applicationWorkingDirectory = "TestWorkingDirectory";

                var importer = new DHydroConfigXmlImporter(() => new List<IDimrModelFileImporter>(),
                                                           () => applicationWorkingDirectory);

                // Act
                object importedModel = importer.ImportItem(dimrFilePathInTemp, null);

                // Assert
                Assert.AreEqual(applicationWorkingDirectory, ((HydroModel) importedModel).WorkingDirectoryPathFunc());
            }
        }

        #endregion
    }
}