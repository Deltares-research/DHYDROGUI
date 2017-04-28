using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DeltaShell.Gui;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterQualityModel;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using NUnit.Framework;

namespace Sobek.IntegrationTests
{
    [TestFixture, Category(TestCategory.DataAccess)]
    public class NHibernateHydroGuiIntegrationTest : NHibernateIntegrationTestBase
    {
        [TestFixtureSetUp]
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
            factory.AddPlugin(new WaterQualityModelApplicationPlugin());
            factory.AddPlugin(new WaterFlowModel1DApplicationPlugin());
            factory.AddPlugin(new NetworkEditorApplicationPlugin());
            factory.AddPlugin(new RealTimeControlApplicationPlugin());
            factory.AddPlugin(new HydroModelApplicationPlugin());
            factory.AddPlugin(new CommonToolsApplicationPlugin());
            factory.AddPlugin(new SharpMapGisApplicationPlugin());
            factory.AddPlugin(new NetCdfApplicationPlugin());
        }
        
        [Test]
        [Category(TestCategory.Slow)]
        public void SaveLoadHydroRegionCheckOwner()
        {
            var hydroRegion = new HydroRegion();

            var retrievedProject = SaveAndRetrieveObjectCore(hydroRegion);
            var retrievedHydroRegionDataItem = (IDataItem) retrievedProject.RootFolder.Items[0];
            Assert.IsNotNull(retrievedHydroRegionDataItem.Owner);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void SaveLoadHydroRegionCheckCoordinateSystem()
        {
            var coordinateSystem = SharpMap.Map.CoordinateSystemFactory.SupportedCoordinateSystems.First();
            var coordinateSystemWKT = coordinateSystem.WKT;

            var hydroRegion = new HydroRegion { CoordinateSystem = coordinateSystem };

            var retrievedProject = SaveAndRetrieveObjectCore(hydroRegion);
            var retrievedHydroRegion = ((IDataItem)retrievedProject.RootFolder.Items[0]).Value as HydroRegion;

            Assert.IsNotNull(retrievedHydroRegion); 
            Assert.IsNotNull(retrievedHydroRegion.CoordinateSystem);
            Assert.AreEqual(coordinateSystemWKT, retrievedHydroRegion.CoordinateSystem.WKT);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void SaveLoadHydroRegionCheckSubRegionOwner()
        {
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Plugins.Add(new WaterQualityModelApplicationPlugin());
                app.Plugins.Add(new RainfallRunoffApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Run();

                var path = TestHelper.GetCurrentMethodName() + ".dsproj";

                var hydroRegion = new HydroRegion();
                hydroRegion.SubRegions.Add(new HydroNetwork());
                app.Project.RootFolder.Add(hydroRegion);

                app.SaveProjectAs(path);
                app.CloseProject();
                app.OpenProject(path);

                var retrievedProject = app.Project;
                var retrievedHydroRegionDataItem = (IDataItem) retrievedProject.RootFolder.Items[0];
                var retrievedNetworkDataItem = retrievedHydroRegionDataItem.Children.First();
                Assert.IsNotNull(retrievedNetworkDataItem.Owner, "child data item owner not set");
            }
        }
    }
}