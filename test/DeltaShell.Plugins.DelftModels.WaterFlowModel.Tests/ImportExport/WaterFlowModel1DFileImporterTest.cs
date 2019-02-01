using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using NUnit.Framework;

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
            var compositeActivity = new WaterFlowModel1D();

            // When
            var result = Importer.CanImportOn(compositeActivity);

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
    }
}