using System;
using System.Linq;
using System.Threading;
using DelftTools.TestUtils;
using DelftTools.Utils.Editing;
using DeltaShell.Core;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using log4net.Core;
using NUnit.Framework;
using SharpTestsEx;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class WaterFlowModel1DSaveAndLoadIntegrationTest
    {
        private DeltaShellApplication app;

        [SetUp]
        public void SetUp()
        {
            LogHelper.ConfigureLogging();
            LogHelper.SetLoggingLevel(Level.Info);

            app = new DeltaShellApplication();
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
            
            app.Run();

            // use a valid network for the calculation
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            app.Project.RootFolder.Add(model);
        }

        [TearDown]  
        public void TearDown()
        {
            app.Dispose();

            LogHelper.SetLoggingLevel(Level.Error);
        }

        [Test]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.WorkInProgress)] //ToDo Noort: fails on build server. please remove WIP when fixed in Flow dll
        public void ModelOutputIsEmptyWhenProjectIsNotSaved()
        {
            var path = TestHelper.GetCurrentMethodName() + ".dsproj";

            app.SaveProjectAs(path);

            var model = (WaterFlowModel1D)app.Project.RootFolder.Models.First();

            model.OutputFlow.Time.Values.Count
                .Should().Be.EqualTo(0);

            app.RunActivityInBackground(model);
            while (app.IsActivityRunning())
            {
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(0);
            }

            model.OutputFlow.Time.Values.Count
                .Should().Not.Be.EqualTo(0);

            app.CloseProject();

            app.OpenProject(path);

            model = (WaterFlowModel1D)app.Project.RootFolder.Models.First();

            model.OutputFlow.Time.Values.Count
                .Should().Be.EqualTo(0);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ModelOutputIsClearedOnCrossSectionMoveAfterSave()
        {
            var model = (WaterFlowModel1D)app.Project.RootFolder.Models.First();
            model.StartTime = new DateTime(2000, 1, 1);
            model.StopTime = new DateTime(2000, 1, 1, 0, 0, 30);
            model.TimeStep = new TimeSpan(0, 0, 30);
            app.RunActivity(model);

            model.OutputFlow.Time.Values.Count
                .Should("model is finished and has output").Not.Be.EqualTo(0);

            app.SaveProjectAs("ModelOutputIsCleared.dsproj");

            // change offset in the 1st cross-section (clears output, in memory)
            var cs = model.Network.CrossSections.First();
            cs.Chainage += 1.0;

            model.OutputFlow.Time.Values.Count
                .Should("output is cleared").Be.EqualTo(0);
        }

        [Test]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.WorkInProgress)] //ToDo Noort: fails on build server. please remove WIP when fixed in Flow dll
        public void ModelOutputIsNotDamagedIfProjectIsChangedAndNotSaved()
        {
            const string path = "ModelOutputIsNotDamaged.dsproj";
            app.SaveProjectAs(path);

            // run model
            var model = (WaterFlowModel1D)app.Project.RootFolder.Models.First();
            app.RunActivity(model);

            // save
            app.SaveProject();

            // change offset in the 1st cross-section (clears output)
            var cs = model.Network.CrossSections.First();
            cs.Chainage += 1.0;

            // close project without saving it and then reopen
            app.CloseProject(); 
            
            // output in saved model should still exist            
            app.OpenProject(path);

            model = (WaterFlowModel1D)app.Project.RootFolder.Models.First();

            model.OutputFlow.Time.Values.Count
                .Should("3. model output has values after project is reloaded").Not.Be.EqualTo(0);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ModelOutputIsUpdatedAfterBranchIsDeletedAndProjectIsSavedTools7319()
        {
            // save
            const string Path = "ModelOutputIsSaved.dsproj";
            app.SaveProjectAs(Path);

            var model = (WaterFlowModel1D)app.Project.RootFolder.Models.First();
            WaterFlowModel1DDemoModelTestHelper.ReplaceStoreForOutputCoverages(model, false, false);
            // run model
            app.RunActivity(model);

            // save
            app.SaveProject();

            // delete branch - will delete output
            var network = model.Network;
            network.BeginEdit("Delete branch");
            network.Branches.RemoveAt(0);
            network.Nodes.RemoveAt(0);
            network.EndEdit();

            // close project without saving it and then reopen
            app.SaveProject();
            app.CloseProject();

            // output in saved model should still exist            
            app.OpenProject(Path);

            model = (WaterFlowModel1D)app.Project.RootFolder.Models.First();
            model.OutputFlow.Time.Values.Count
                .Should("model is cleared since network was modified").Be.EqualTo(0);

            // run, save, open
            app.RunActivity(model);
            app.SaveProject();
            app.OpenProject(Path);

            model = (WaterFlowModel1D)app.Project.RootFolder.Models.First();
            model.OutputFlow.Time.Values.Count.Should("model is saved after model run").Not.Be.EqualTo(0);
        }
    }
}
 