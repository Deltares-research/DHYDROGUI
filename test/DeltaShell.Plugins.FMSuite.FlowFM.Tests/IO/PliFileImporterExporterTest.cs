using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using NetTopologySuite.Extensions.Features;
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
            
            TestHelper.AssertIsFasterThan(8500, () => importer.ImportItem(path, resultList));
            Assert.AreEqual(19459, resultList.Count);
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void ImportLargeListOfFixedWeirsInDeltaShell()
        {
            using (var gui = new DeltaShellGui())
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

                WaterFlowFMModel model = new WaterFlowFMModel();

                gui.Application.Project.RootFolder.Add(model);

                var importer = gui.Application.FileImporters.First(fi => fi is PliFileImporterExporter<FixedWeir, FixedWeir>);

                TestHelper.AssertIsFasterThan(9000, () => importer.ImportItem(TestHelper.GetTestFilePath("structures\\testBas2FM_fxw.pliz"), model.Area.FixedWeirs));
                Assert.AreEqual(19459, model.Area.FixedWeirs.Count);
            }
        }
    }
}
