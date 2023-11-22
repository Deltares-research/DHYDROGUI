using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.Common.Tests.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using NUnit.Framework;


namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class PliFileImporterExporterTest 
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.Slow)]
        public void ImportLargeListOfFixedWeirs()
        {
            string path = TestHelper.GetTestFilePath("structures\\testBas2FM_fxw.pliz");

            var importer = new PliFileImporterExporter<FixedWeir, FixedWeir>();

            IList<FixedWeir> resultList = new List<FixedWeir>();
            
            TestHelper.AssertIsFasterThan(12500, () => importer.ImportItem(path, resultList));
            Assert.AreEqual(19459, resultList.Count);
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void ImportLargeListOfFixedWeirsInDeltaShell()
        {
            using (var gui = DeltaShellCoreFactory.CreateGui())
            {
                var app = gui.Application;
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

                app.CreateNewProject();

                var model = new WaterFlowFMModel();

                gui.Application.Project.RootFolder.Add(model);

                var importer = (PliFileImporterExporter<FixedWeir, FixedWeir>) gui.Application.FileImporters.First(fi => fi is PliFileImporterExporter<FixedWeir, FixedWeir>);

                importer.ImportItem(TestHelper.GetTestFilePath("structures\\testBas2FM_fxw.pliz"), model.Area.FixedWeirs);

                importer.EqualityComparer = new GroupableFeatureComparer<FixedWeir>();

                importer.AfterCreateAction = (parent, feature) => feature.UpdateGroupName(model);
                importer.GetEditableObject = parent => model.Area;

                // import the same set twice to include duplicate checking for all items
                TestHelper.AssertIsFasterThan(400000, () => importer.ImportItem(TestHelper.GetTestFilePath("structures\\testBas2FM_fxw.pliz"), model.Area.FixedWeirs));
                Assert.AreEqual(19459, model.Area.FixedWeirs.Count);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.DataAccess)]
        public void GivenFmModel_WhenLoadingPlizFileAndWritingIt_ThenFileContentsAreTheSame()
        {
            var filePath = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath("HydroAreaCollection/TwoFixedWeirs_fxw.pliz"));
            var testDir = Path.GetDirectoryName(filePath);
            var exportToFilePath = Path.Combine(testDir, "ExportedFixedWeirs_fxw.pliz");
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
        [Category(TestCategory.Integration)]
        [Category(TestCategory.DataAccess)]
        public void GivenFmModel_WhenImportingTheSameFileTwice_TheFeaturesWithTheSameNameAreReplaced()
        {
            var filePath = TestHelper.GetTestFilePath("PliFileImporter/structures.pli");
            
            var fmModel = new WaterFlowFMModel();
            var importer = new PliFileImporterExporter<FixedWeir, FixedWeir>();
            importer.ImportItem(filePath, fmModel.Area.FixedWeirs);

            Assert.AreEqual(10, fmModel.Area.FixedWeirs.Count);
            CollectionAssert.AllItemsAreUnique(fmModel.Area.FixedWeirs.Select(w => w.Name), "All names should be unique");

            var firstWeir = fmModel.Area.FixedWeirs[0];
            Assert.NotNull(firstWeir);

            importer.ImportItem(filePath, fmModel.Area.FixedWeirs);

            var newFirstWeir = fmModel.Area.FixedWeirs[0];

            Assert.AreEqual(10, fmModel.Area.FixedWeirs.Count);
            Assert.AreNotEqual(firstWeir, newFirstWeir);
        }

        private static void CheckImportedFixedWeirs(WaterFlowFMModel fmModel)
        {
            var fixedWeirs = fmModel.Area.FixedWeirs;
            Assert.That(fixedWeirs.Count, Is.EqualTo(2));

            // Check first weir's properties
            var firstWeir = fixedWeirs[0];
            var attributes = firstWeir.Attributes;
            attributes.CheckDoubleValuesForColumn("Column3", 1.2, 6.4);
            attributes.CheckDoubleValuesForColumn("Column4", 3.5, 3.0);
            attributes.CheckDoubleValuesForColumn("Column5", 3.2, 3.3);
            attributes.CheckDoubleValuesForColumn("Column6", 4.0, 3.8);
            attributes.CheckDoubleValuesForColumn("Column7", 4.0, 4.0);
            attributes.CheckDoubleValuesForColumn("Column8", 4.0, 4.0);
            attributes.CheckDoubleValuesForColumn("Column9", 0.0, 0.0);
            attributes.CheckStringValuesForColumn("WeirType", "V", "V");

            // Check second weir's properties
            var secondWeir = fixedWeirs[1];
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
