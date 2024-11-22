﻿using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.NGHS.Common;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Importers
{
    [TestFixture]
    public class RainfallRunoffModelImporterTest
    {
        [TearDown]
        public void TearDown()
        {
            Sobek2ModelImporters.ClearRegisteredImporters();
        }
        
        [Test]
        public void Constructor_ExpectedResults()
        {
            var importer = new RainfallRunoffModelImporter();

            Assert.That(importer.Name, Is.EqualTo("Rainfall Runoff Model importer"));
            Assert.That(importer.Description, Is.EqualTo("Rainfall Runoff Model importer"));
            Assert.That(importer.Category, Is.EqualTo(ProductCategories.OneDTwoDModelImportCategory));
            Assert.That(importer.SupportedItemTypes, Is.EqualTo(new [] { typeof(IHydroModel) }));
            Assert.That(importer.FileFilter, Is.EqualTo("RR Sobek_3b.fnm file model import|Sobek_3b.fnm"));
            Assert.That(importer.CanImportOnRootLevel, Is.True);
            Assert.That(importer.OpenViewAfterImport, Is.True);
        }

        public static IEnumerable<TestCaseData> CanImportOnData()
        {
            yield return new TestCaseData(null, false);
            yield return new TestCaseData(new object(), false);
            yield return new TestCaseData(Substitute.For<ICompositeActivity>(), true);
            yield return new TestCaseData(new RainfallRunoffModel(), true);
        }

        [Test]
        [TestCaseSource(nameof(CanImportOnData))]
        public void CanImportOn_ExpectedResults(object targetObject, bool expectedResult)
        {
            var importer = new RainfallRunoffModelImporter();
            Assert.That(importer.CanImportOn(targetObject), Is.EqualTo(expectedResult));
        }
        
        [Test]
        [TestCase(null, ExpectedResult = false)]
        [TestCase("", ExpectedResult = false)]
        [TestCase(".", ExpectedResult = false)]
        [TestCase("settings.json", ExpectedResult = false)]
        [TestCase("settings.xml", ExpectedResult = false)]
        [TestCase("flowfm.mdu", ExpectedResult = false)]
        [TestCase("Sobek_3b.fnm", ExpectedResult = true)]
        [TestCase("SOBEK_3B.FNM", ExpectedResult = true)]
        public bool CanImportDimrFile_WithInputFile_ThenExpectedIsReturned(string path)
        {
            // Setup
            var importer = new RainfallRunoffModelImporter();

            // Call
            return importer.CanImportDimrFile(path);
        }

        [Test]
        public void GivenRainfallRunoffModelImporter_Import_ShouldConnectOutput()
        {
            //Arrange
            var stubImporter = Substitute.For<IFileImporter>();
            var model = Substitute.For<IRainfallRunoffModel>();

            stubImporter.SupportedItemTypes.Returns(new []{typeof(RainfallRunoffModel) });

            Sobek2ModelImporters.RegisterSobek2Importer(() => stubImporter);
            var importer = new RainfallRunoffModelImporter(() => model);

            // Act
            var importedModel = importer.ImportItem("D:\\Temp\\Test.fnm");

            // Assert
            Assert.AreEqual(model, importedModel);
            model.Received(1).ConnectOutput("D:\\Temp");
        }
    }
}