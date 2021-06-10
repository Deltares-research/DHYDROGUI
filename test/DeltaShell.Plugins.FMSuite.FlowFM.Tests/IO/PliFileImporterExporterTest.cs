using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO.ImportExport;
using DeltaShell.Plugins.FMSuite.Common.Properties;
using DeltaShell.Plugins.FMSuite.Common.Tests.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.ImportersExporters;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;
using Rhino.Mocks;
using FmResources = DeltaShell.Plugins.FMSuite.FlowFM.Properties.Resources;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class PliFileImporterExporterTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            var importer = new PliFileImporterExporter<FixedWeir, FixedWeir>();

            Assert.That(importer.Category, Is.EqualTo("Feature geometries"));
            Assert.IsEmpty(importer.Description);
            Assert.That(importer.FileFilter, Is.EqualTo("Feature polyline files (*.pli)|*.pli|polyline-z files (*.pliz)|*.pliz"));
            Assert.That(BitmapsAreEqual(importer.Image, FmResources.TextDocument));

            Type[] expectedSourceTypes =
            {
                typeof(FixedWeir),
                typeof(IList<FixedWeir>)
            };
            Assert.That(importer.SourceTypes(), Is.EqualTo(expectedSourceTypes));

            Type[] expectedSupportedItemTypes =
            {
                typeof(IList<FixedWeir>)
            };
            Assert.That(importer.SupportedItemTypes, Is.EqualTo(expectedSupportedItemTypes));
        }

        [Test]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.Slow)]
        public void ImportLargeListOfFixedWeirs()
        {
            string path = TestHelper.GetTestFilePath("structures\\testBas2FM_fxw.pliz");

            var importer = new PliFileImporterExporter<FixedWeir, FixedWeir>();

            IList<FixedWeir> resultList = new List<FixedWeir>();

            TestHelper.AssertIsFasterThan(8500, () => importer.ImportItem(path, resultList), false, true);
            Assert.AreEqual(19459, resultList.Count);
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void ImportLargeListOfFixedWeirsInDeltaShell()
        {
            using (var gui = new DeltaShellGui())
            {
                IApplication app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());
                gui.Plugins.Add(new CommonToolsGuiPlugin());

                gui.Run();

                var model = new WaterFlowFMModel();

                gui.Application.Project.RootFolder.Add(model);

                var importer = (PlizFileImporterExporter<FixedWeir, FixedWeir>)gui.Application.FileImporters.First(fi => fi is PlizFileImporterExporter<FixedWeir, FixedWeir>);

                importer.ImportItem(TestHelper.GetTestFilePath("structures\\testBas2FM_fxw.pliz"), model.Area.FixedWeirs);

                importer.EqualityComparer = new GroupableFeatureComparer<FixedWeir>();

                importer.AfterCreateAction = (parent, feature) => feature.UpdateGroupName(model);
                importer.GetEditableObject = parent => model.Area;

                // import the same set twice to include duplicate checking for all items
                TestHelper.AssertIsFasterThan(
                    25000,
                    () => importer.ImportItem(TestHelper.GetTestFilePath("structures\\testBas2FM_fxw.pliz"),
                                              model.Area.FixedWeirs), false, true);
                Assert.AreEqual(19459, model.Area.FixedWeirs.Count);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAPliFileForAPump_WhenImportingThisFile_ThenACorrectPumpShouldBeCreatedWhichCanBeTimeDependent()
        {
            // Given
            var plugin = new FlowFMApplicationPlugin();
            IEnumerable<IFileImporter> fileImporters = plugin.GetFileImporters();
            PliFileImporterExporter<Pump, Pump> importer =
                fileImporters.OfType<PliFileImporterExporter<Pump, Pump>>().Single();

            // Set delegates to null, since they are used for the relation between model en pump
            // and we only want to test the creation of the pump based on the pli file
            importer.AfterCreateAction = null;
            importer.GetEditableObject = null;

            // When
            var pumps =
                (List<Pump>)importer.ImportItem(TestHelper.GetTestFilePath("structures_all_types\\pump01.pli"));

            // Then
            int counter = pumps.Count;
            Assert.AreEqual(1, counter, $"{counter} pumps created instead of 1");

            Pump pump = pumps[0];
            Assert.AreEqual(158031.3362860695, pump.Geometry.Coordinates[0].X, "Geometry of the pump is not correctly imported");
            Assert.AreEqual(578431.3969514973, pump.Geometry.Coordinates[0].Y, "Geometry of the pump is not correctly imported");

            Assert.AreEqual(158372.1368887129, pump.Geometry.Coordinates[4].X, "Geometry of the pump is not correctly imported");
            Assert.AreEqual(578437.8625413019, pump.Geometry.Coordinates[4].Y, "Geometry of the pump is not correctly imported");
        }

        [Test]
        public void GivenAPliFileImporterExporter_WhenImporting_ThenTheNameShouldBeCorrect()
        {
            var importer = new PliFileImporterExporter<Pump, Pump> { Mode = Feature2DImportExportMode.Import };
            Assert.AreEqual("Features from .pli(z) file", ((IFileImporter)importer).Name, "Name of the pli file importer for pumps is not correct");
        }

        [Test]
        public void GivenAPliFileImporterExporter_WhenExporting_ThenTheNameShouldBeCorrect()
        {
            var exporter = new PliFileImporterExporter<Pump, Pump> { Mode = Feature2DImportExportMode.Export };
            Assert.AreEqual("Features to .pli file", ((IFileExporter)exporter).Name, "Name of the pli file exporter for pumps is not correct");
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.DataAccess)]
        public void GivenFmModel_WhenLoadingPlizFileAndWritingIt_ThenFileContentsAreTheSame()
        {
            string filePath = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath("HydroAreaCollection/TwoFixedWeirs_fxw.pliz"));
            string testDir = Path.GetDirectoryName(filePath);
            string exportToFilePath = Path.Combine(testDir, "ExportedFixedWeirs_fxw.pliz");
            try
            {
                var fmModel = new WaterFlowFMModel();
                var importer = new PliFileImporterExporter<FixedWeir, FixedWeir>();
                importer.ImportItem(filePath, fmModel.Area.FixedWeirs);

                // Check imported fixedWeirs
                CheckImportedFixedWeirs(fmModel);
                importer.Export(fmModel.Area.FixedWeirs, exportToFilePath);
                importer.ImportItem(exportToFilePath, fmModel.Area.FixedWeirs);
                CheckImportedFixedWeirs(fmModel);
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }

        [Test]
        public void ImportItem_ThenAfterImportActionShouldBeInvoked()
        {
            // Set-up
            var target = new List<FixedWeir>();

            var importer = new PliFileImporterExporter<FixedWeir, FixedWeir>();
            var afterImportAction = MockRepository.GenerateMock<Action<IList<FixedWeir>>>();
            importer.AfterImportAction = afterImportAction;

            afterImportAction.Expect(a => a.Invoke(target)).Repeat.Once();
            afterImportAction.Replay();

            // Call
            importer.ImportItem("file_path", target);

            // Assert
            afterImportAction.VerifyAllExpectations();
        }

        public static IEnumerable<TestCaseData> GetNonUniqueImporterTestData()
        {
            PliFileImporterExporter<T, T> GetSimpleImporter<T>(WaterFlowFMModel fmModel) where T : class, IFeature, INameable, IGroupableFeature, new() =>
                new PliFileImporterExporter<T, T>
                {
                    EqualityComparer = new GroupableFeatureComparer<T>(),
                    AfterCreateAction = (featureList, feature) =>
                    {
                        feature.UpdateGroupName(fmModel);
                    }
                };

            yield return new TestCaseData((Func<WaterFlowFMModel, PliFileImporterExporter<Structure, Structure>>)GetSimpleImporter<Structure>,
                                          new Func<WaterFlowFMModel, IEventedList<Structure>>(model => model.Area.Structures),
                                          new Func<WaterFlowFMModel, IEventedList<Structure>>(model => model.Area.Structures));
            yield return new TestCaseData((Func<WaterFlowFMModel, PliFileImporterExporter<ThinDam2D, ThinDam2D>>)GetSimpleImporter<ThinDam2D>,
                                          new Func<WaterFlowFMModel, IEventedList<ThinDam2D>>(model => model.Area.ThinDams),
                                          new Func<WaterFlowFMModel, IEventedList<ThinDam2D>>(model => model.Area.ThinDams));
            yield return new TestCaseData((Func<WaterFlowFMModel, PliFileImporterExporter<Pump, Pump>>)GetSimpleImporter<Pump>,
                                          new Func<WaterFlowFMModel, IEventedList<Pump>>(model => model.Area.Pumps),
                                          new Func<WaterFlowFMModel, IEventedList<Pump>>(model => model.Area.Pumps));

            yield return new TestCaseData((Func<WaterFlowFMModel, PliFileImporterExporter<ObservationCrossSection2D, ObservationCrossSection2D>>)GetSimpleImporter<ObservationCrossSection2D>,
                                          new Func<WaterFlowFMModel, IEventedList<ObservationCrossSection2D>>(model => model.Area.ObservationCrossSections),
                                          new Func<WaterFlowFMModel, IEventedList<ObservationCrossSection2D>>(model => model.Area.ObservationCrossSections));

            PliFileImporterExporter<BoundaryConditionSet, Feature2D> GetBoundaryConditionImporter(WaterFlowFMModel _) =>
                new PliFileImporterExporter<BoundaryConditionSet, Feature2D> { CreateFromFeature = f => new BoundaryConditionSet { Feature = f } };

            yield return new TestCaseData((Func<WaterFlowFMModel, PliFileImporterExporter<BoundaryConditionSet, Feature2D>>)GetBoundaryConditionImporter,
                                          new Func<WaterFlowFMModel, IEventedList<BoundaryConditionSet>>(model => model.BoundaryConditionSets),
                                          new Func<WaterFlowFMModel, IEventedList<Feature2D>>(model => model.Boundaries));

            PliFileImporterExporter<SourceAndSink, Feature2D> GetSourceAndSinkImporter(WaterFlowFMModel _) =>
                new PliFileImporterExporter<SourceAndSink, Feature2D>
                {
                    CreateFromFeature = f => new SourceAndSink
                    {
                        Area = 1.0,
                        Feature = f
                    }
                };

            yield return new TestCaseData((Func<WaterFlowFMModel, PliFileImporterExporter<SourceAndSink, Feature2D>>)GetSourceAndSinkImporter,
                                          new Func<WaterFlowFMModel, IEventedList<SourceAndSink>>(model => model.SourcesAndSinks),
                                          new Func<WaterFlowFMModel, IEventedList<SourceAndSink>>(model => model.SourcesAndSinks));
        }

        [Test]
        [Category(TestCategory.Integration)]
        [TestCaseSource(nameof(GetNonUniqueImporterTestData))]
        public void GivenFmModelWithoutFeatures_WhenImportingFeaturesFromAPliFileWithNonUniqueNames_ThenStructuresWithTheSameNameInTheFileWillBeRenamed<TFeat, TParent, TCheck>(Func<WaterFlowFMModel, PliFileImporterExporter<TParent, TFeat>> importerFunc,
                                                                                                                                                                                Func<WaterFlowFMModel, IEventedList<TParent>> featuresImportFunc,
                                                                                                                                                                                Func<WaterFlowFMModel, IEventedList<TCheck>> featuresCheckFunc)
            where TFeat : class, IFeature, INameable, new()
            where TParent : INameable
            where TCheck : INameable
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                string filePath = TestHelper.GetTestFilePath("pli_files/geometries_non_unique_names.pli");
                string tempFilePath = tempDirectory.CopyTestDataFileToTempDirectory(filePath);
                var fmModel = new WaterFlowFMModel();
                PliFileImporterExporter<TParent, TFeat> importer = importerFunc(fmModel);

                // Act and assert
                IEventedList<TParent> featuresImport = featuresImportFunc(fmModel);
                TestHelper.AssertAtLeastOneLogMessagesContains(() => importer.ImportItem(tempFilePath, featuresImport), Resources.Feature2DImportExportBase_AddOrReplace_The_list_of_imported_features_did_not_contain_unique_names_Names_were_made_unique_during_import);

                IEventedList<TCheck> featuresCheck = featuresCheckFunc(fmModel);
                Assert.AreEqual(10, featuresCheck.Count);
                Assert.AreEqual(10, featuresCheck.Select(w => w.Name).Distinct().Count(), "All names should be unique");
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenFmModelWithStructuresWithTheSameNamesAndTheSameGroupNames_WhenImportingACorrectPliFile_ThenNoCriticalErrorShouldBeGivenAndImportFailed()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                string filePath = TestHelper.GetTestFilePath("pli_files/geometries_unique_names.pli");
                string tempFilePath = tempDirectory.CopyTestDataFileToTempDirectory(filePath);
                var fmModel = new WaterFlowFMModel();

                var structure1 = new Structure
                { Name = "test" };
                structure1.UpdateGroupName(fmModel);

                var structure2 = new Structure
                { Name = "test" };
                structure2.UpdateGroupName(fmModel);

                fmModel.Area.Structures.Add(structure1);
                fmModel.Area.Structures.Add(structure2);
                var importer = new PliFileImporterExporter<Structure, Structure>
                {
                    EqualityComparer = new GroupableFeatureComparer<Structure>(),
                    AfterCreateAction = (featureList, feature) =>
                    {
                        feature.UpdateGroupName(fmModel);
                    }
                };

                // Act and assert
                TestHelper.AssertAtLeastOneLogMessagesContains(() => importer.ImportItem(tempFilePath, fmModel.Area.Structures), Resources.Feature2DImportExportBase_AddOrReplace_Import_failed_Current_project_does_not_contain_unique_names_for_features);
                Assert.AreEqual(2, fmModel.Area.Structures.Count);
            }
        }

        private static bool BitmapsAreEqual(Bitmap bitmap1, Bitmap bitmap2)
        {
            if (bitmap1 == null || bitmap2 == null)
            {
                return false;
            }

            if (Equals(bitmap1, bitmap2))
            {
                return true;
            }

            if (!bitmap1.Size.Equals(bitmap2.Size) || !bitmap1.PixelFormat.Equals(bitmap2.PixelFormat))
            {
                return false;
            }

            for (var x = 0; x < bitmap1.Width; x++)
            {
                for (var y = 0; y < bitmap1.Height; y++)
                {
                    if (!bitmap1.GetPixel(x, y).Equals(bitmap2.GetPixel(x, y)))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static void CheckImportedFixedWeirs(WaterFlowFMModel fmModel)
        {
            IEventedList<FixedWeir> fixedWeirs = fmModel.Area.FixedWeirs;
            Assert.That(fixedWeirs.Count, Is.EqualTo(2));

            // Check first weir's properties
            FixedWeir firstWeir = fixedWeirs[0];
            IFeatureAttributeCollection attributes = firstWeir.Attributes;
            attributes.CheckDoubleValuesForColumn("Column3", 1.2, 6.4);
            attributes.CheckDoubleValuesForColumn("Column4", 3.5, 3.0);
            attributes.CheckDoubleValuesForColumn("Column5", 3.2, 3.3);
            attributes.CheckDoubleValuesForColumn("Column6", 4.0, 3.8);
            attributes.CheckDoubleValuesForColumn("Column7", 4.0, 4.0);
            attributes.CheckDoubleValuesForColumn("Column8", 4.0, 4.0);
            attributes.CheckDoubleValuesForColumn("Column9", 0.0, 0.0);
            attributes.CheckStringValuesForColumn("WeirType", "V", "V");

            // Check second weir's properties
            FixedWeir secondWeir = fixedWeirs[1];
            attributes = secondWeir.Attributes;
            attributes.CheckDoubleValuesForColumn("Column3", 1.7, 6.1);
            attributes.CheckDoubleValuesForColumn("Column4", 4.5, 4.0);
            attributes.CheckDoubleValuesForColumn("Column5", 4.2, 4.3);
            attributes.CheckDoubleValuesForColumn("Column6", 5.0, 4.8);
            attributes.CheckDoubleValuesForColumn("Column7", 5.0, 5.0);
            attributes.CheckDoubleValuesForColumn("Column8", 5.0, 5.0);
            attributes.CheckDoubleValuesForColumn("Column9", 0.0, 0.0);
            attributes.CheckStringValuesForColumn("WeirType", "T", "T");
        }
    }
}