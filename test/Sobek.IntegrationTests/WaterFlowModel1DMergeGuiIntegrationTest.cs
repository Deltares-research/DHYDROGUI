using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DeltaShell.Gui;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using NUnit.Framework;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class WaterFlowModel1DMergeGuiIntegrationTest
    {
        [Test]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void TestMergeTwoFlow1DModelsRequiringBranchRefittingAndWithOutputDataOnSourceModelDoesNotThrowException()
        {
            /*
            SOBEK3-595:
            Merging two 1D flow models with output data on the source model gives an unhandled exception:
            System.IO.IOException: The process cannot access the file '[...]\SOBEK3-590\Project1_withOutput.dsproj_data\Water level-2e763725-7e5f-4f9a-847a-ba807fea5203.nc' because it is being used by another process.
        
            This was due to the branch of the source model needing to be resized.
            When cloning the source model, the function stores are in a 'Defined' state, which results in the cloned model linking to the same file stores as the source model.
            When setting channel.IsLengthCustom = true, the output of the cloned model is cleared, which then gives the exception above.
            */
            using (var gui = new DeltaShellGui())
            {
                using (var app = gui.Application)
                {
                    SetupPluginsForApp(app);
                    SetupPluginsForGui(gui);
                    gui.Run();

                    //Setup
                    var destinationWFM1D = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 100, "Destination");
                    var sourceWFM1D = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(105, 250, "Source"); // Ensure that the branch in the source model needs to be refitted

                    app.Project.RootFolder.Add(sourceWFM1D);
                    app.Project.RootFolder.Add(destinationWFM1D);

                    InitializeModelsBeforeMerge(sourceWFM1D);
                    RunModelsBeforeMerge(sourceWFM1D); // Run Model to generate output data and ensure NetCdfFunctionStore.State == 'Defined'

                    //merge
                    destinationWFM1D.Merge(sourceWFM1D, null);
                }
            }
        }

        [Test]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void Given2ModelWithNetworkWithOutputCreatedOnSourceModelWhenMergeThenAfterMergeObservationPointDeleteShouldNotCrash()
        {
            using (var gui = new DeltaShellGui())
            {
                using (var app = gui.Application)
                {
                    SetupPluginsForApp(app);
                    SetupPluginsForGui(gui);
                    gui.Run();

                    //Setup
                    var destinationWFM1D = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(0, 100, "Destination");
                    var sourceWFM1D = WaterFlowModel1DModelMergeTestHelper.SetupWFM1D(100, 250, "Source");
                    app.Project.RootFolder.Add(sourceWFM1D);
                    app.Project.RootFolder.Add(destinationWFM1D);

                    InitializeModelsBeforeMerge(sourceWFM1D);

                    RunModelsBeforeMerge(sourceWFM1D);

                    //merge
                    destinationWFM1D.Merge(sourceWFM1D, null);

                    var obsPnt2 = destinationWFM1D.Network.ObservationPoints.FirstOrDefault(op => op.Name == "2"); // the connected node
                    Assert.That(obsPnt2, Is.Not.Null);

                    using (var centralMapView = gui.DocumentViewsResolver.CreateViewForData(destinationWFM1D, (vi) => vi.ViewType == typeof(ProjectItemMapView)) as ProjectItemMapView)
                    {
                        Assert.IsNotNull(centralMapView);

                        var mapControl = centralMapView.MapView.MapControl;
                        mapControl.SelectTool.Select(obsPnt2);
                        Assert.That(mapControl.SelectTool.Selection.Count(), Is.EqualTo(1));
                        var selectedFeature = mapControl.SelectTool.Selection.FirstOrDefault();
                        Assert.That(selectedFeature, Is.Not.Null);
                        Assert.IsTrue(selectedFeature is IObservationPoint);

                        //delete ObsPnt2 in destination model... now no crash!
                        Assert.DoesNotThrow(() => mapControl.DeleteTool.DeleteSelection());
                    }
                }
            }
        }

        private void SetupPluginsForGui(IGui gui)
        {
            gui.Plugins.Add(new ProjectExplorerGuiPlugin());
            gui.Plugins.Add(new NetworkEditorGuiPlugin());
            gui.Plugins.Add(new WaterFlowModel1DGuiPlugin());
            gui.Plugins.Add(new SharpMapGisGuiPlugin());
            gui.Plugins.Add(new CommonToolsGuiPlugin());
        }

        private static void SetupPluginsForApp(IApplication app)
        {
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new NetCdfApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
            app.Plugins.Add(new HydroModelApplicationPlugin());
        }

        private void RunModelsBeforeMerge(WaterFlowModel1D sourceWFM1D)
        {
            //create output on source model
            Assert.That(sourceWFM1D.OutputFunctions.First().Components[0].Values.Count, Is.EqualTo(0),
                "Coverages not empty initially, unexpected!");
            ActivityRunner.RunActivity(sourceWFM1D);
            Assert.That(sourceWFM1D.Status, Is.Not.EqualTo(ActivityStatus.Failed), "Model run has failed");
            Assert.That(sourceWFM1D.OutputFunctions.First().Components[0].Values.Count, Is.GreaterThan(0),
                "Coverages empty initially, unexpected!");
        }

        private void InitializeModelsBeforeMerge(WaterFlowModel1D sourceWFM1D)
        {
            sourceWFM1D.NetworkDiscretization = WaterFlowModel1DModelMergeTestHelper.SetupUniformNetworkDiscretization(sourceWFM1D.Network, 11);
            var channel = sourceWFM1D.Network.Channels.FirstOrDefault();
            Assert.That(channel, Is.Not.Null);

            FileWriterTestHelper.AddObservationPoint(channel, 2, "ObsPnt2", (250 - 100)/2);
            var validationreport = sourceWFM1D.Validate();
            Assert.That(validationreport.AllErrors.Count(), Is.EqualTo(0));
        }

    }
}