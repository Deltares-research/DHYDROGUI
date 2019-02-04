using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using NUnit.Framework;
using Rhino.Mocks;
using Is = NUnit.Framework.Is;


namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Readers
{
    [TestFixture]
    public class DHydroConfigXmlImporterTest
    {
        [Test]
        public void ImportTestWithoutActivities()
        {
            var dimrXmlPath = TestHelper.GetTestFilePath(Path.Combine("FileReader", "dimr.xml"));

            var mocks = new MockRepository();
            var importers = mocks.DynamicMock(typeof(Func<List<IDimrModelFileImporter>>));
            var importer = mocks.DynamicMock<DHydroConfigXmlImporter>(importers);
       
            var model =importer.ImportItem(dimrXmlPath);

            Assert.IsNotNull(model);
            Assert.That(model, Is.TypeOf<HydroModel>());
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
            var importer = new DHydroConfigXmlImporter(() => importers);

            // When
            var result = importer.CanImportOnRootLevel;

            // Then
            Assert.That(result, Is.False);
        }

        /// <summary>
        /// GIVEN a DimrModelFileImporter that cannot import on root level
        ///   AND an importer containing this importer
        /// WHEN CanImportOnRootLevel is called
        /// THEN False is returned
        /// </summary>
        [Test]
        public void GivenADimrModelFileImporterThatCannotImportOnRootLevelAndAnImporterContainingThisImporter_WhenCanImportOnRootLevelIsCalled_ThenFalseIsReturned()
        {
            // Given
            var cannotImportRootLevelImporter = MockRepository.GenerateStrictMock<IDimrModelFileImporter>();
            cannotImportRootLevelImporter.Expect(imp => imp.CanImportOnRootLevel).Return(false).Repeat.Any();

            var importers = new List<IDimrModelFileImporter>() { cannotImportRootLevelImporter };
            var importer = new DHydroConfigXmlImporter(() => importers);

            // When
            var result = importer.CanImportOnRootLevel;

            // Then
            cannotImportRootLevelImporter.VerifyAllExpectations();
            Assert.That(result, Is.False);
        }

        /// <summary>
        /// GIVEN a DimrModelFileImporter that can import on root level
        ///   AND an importer containing this importer
        /// WHEN CanImportOnRootLevel is called
        /// THEN True is returned
        /// </summary>
        [Test]
        public void GivenADimrModelFileImporterThatCanImportOnRootLevelAndAnImporterContainingThisImporter_WhenCanImportOnRootLevelIsCalled_ThenTrueIsReturned()
        {
            // Given
            var canImportRootLevelImporter = MockRepository.GenerateStrictMock<IDimrModelFileImporter>();
            canImportRootLevelImporter.Expect(imp => imp.CanImportOnRootLevel).Return(true).Repeat.Any();

            var importers = new List<IDimrModelFileImporter>() { canImportRootLevelImporter };
            var importer = new DHydroConfigXmlImporter(() => importers);

            // When
            var result = importer.CanImportOnRootLevel;

            // Then
            canImportRootLevelImporter.VerifyAllExpectations();
            Assert.That(result, Is.True);
        }

        /// <summary>
        /// GIVEN a set of DimrModelFileImporter that can and cannot import on root level
        ///   AND an importer containing these importers
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
                cannotImportRootLevelImporter2,
            };

            var importer = new DHydroConfigXmlImporter(() => importers);

            // When
            var result = importer.CanImportOnRootLevel;

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
            var importer = new DHydroConfigXmlImporter(() => importers);

            // When
            var result = importer.CanImportOn(project);

            // Then
            Assert.That(result, Is.False);
        }

        /// <summary>
        /// GIVEN a project
        ///   AND a subimporter that can import on this project
        ///   AND an importer containing this importer
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

            var importers = new List<IDimrModelFileImporter>() { canImportOnImporter };
            var importer = new DHydroConfigXmlImporter(() => importers);

            // When
            var result = importer.CanImportOn(project);

            // Then
            canImportOnImporter.VerifyAllExpectations();
            Assert.That(result, Is.True);
        }

        /// <summary>
        /// GIVEN a project
        ///   AND a subimporter that cannot import on this project
        ///   AND an importer containing this importer
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

            var importers = new List<IDimrModelFileImporter>() { cannotImportOnImporter };
            var importer = new DHydroConfigXmlImporter(() => importers);

            // When
            var result = importer.CanImportOn(project);

            // Then
            cannotImportOnImporter.VerifyAllExpectations();
            Assert.That(result, Is.False);
        }

        /// <summary>
        /// GIVEN a project
        ///   AND a set of subimporters that can and cannot import on this project
        ///   AND an importer containing this importer
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
                cannotImportImporter2,
            };

            var importer = new DHydroConfigXmlImporter(() => importers);

            // When
            var result = importer.CanImportOn(project);

            // Then
            canImportImporter1.VerifyAllExpectations();
            canImportImporter2.VerifyAllExpectations();
            cannotImportImporter1.VerifyAllExpectations();
            cannotImportImporter2.VerifyAllExpectations();

            Assert.That(result, Is.True);
        }
    }
}
