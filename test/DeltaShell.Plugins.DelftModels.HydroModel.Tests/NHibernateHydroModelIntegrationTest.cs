using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.NetworkEditor;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class NHibernateHydroModelIntegrationTest : NHibernateIntegrationTestBase
    {
        [OneTimeSetUp]
        public override void OneTimeSetUp()
        {
            base.OneTimeSetUp();
            factory.AddPlugin(new NetworkEditorApplicationPlugin());
            factory.AddPlugin(new HydroModelApplicationPlugin());
            factory.AddPlugin(new RainfallRunoffApplicationPlugin());
            factory.AddPlugin(new RealTimeControlApplicationPlugin());
            factory.AddPlugin(new FlowFMApplicationPlugin());
        }

        [Test]
        public void SaveLoadHydroModelWithSeveralSubActivities()
        {
            var hydroModelBuilder = new HydroModelBuilder();
            var hydroModel = hydroModelBuilder.BuildModel(ModelGroup.All);

            int expectedActivitiesCount = hydroModel.Activities.Count;
            int expectedRegionsCount = hydroModel.Region.AllRegions.Count();

            var retrievedHydroModel = SaveAndRetrieveObject(hydroModel);

            Assert.AreEqual(expectedActivitiesCount, retrievedHydroModel.Activities.Count);
            Assert.AreEqual(expectedRegionsCount, retrievedHydroModel.Region.AllRegions.Count());
        }

        [Test]
        public void SaveLoadHydroModelWithCurrentWorkflow()
        {
            var hydroModelBuilder = new HydroModelBuilder();
            var hydroModel = hydroModelBuilder.BuildModel(ModelGroup.All);

            hydroModel.CurrentWorkflow = hydroModel.Workflows.ElementAt(2);

            var retrievedHydroModel = SaveAndRetrieveObject(hydroModel);
            Assert.AreEqual(2, retrievedHydroModel.Workflows.IndexOf(retrievedHydroModel.CurrentWorkflow));
        }

        [Test]
        public void SaveHydroModelDoesNotCausePropertyChangeOnWorkflowTools9667()
        {
            var hydroModelBuilder = new HydroModelBuilder();
            var hydroModel = hydroModelBuilder.BuildModel(ModelGroup.All);

            hydroModel.CurrentWorkflow = hydroModel.Workflows.ElementAt(2);

            var path = TestHelper.GetCurrentMethodName() + "1.dsproj";
            var project = new Project();
            project.RootFolder.Add(hydroModel);

            ProjectRepository.Create(path);

            try
            {
                var workFlowBefore = hydroModel.CurrentWorkflow;
                ProjectRepository.SaveOrUpdate(project);
                var workFlowAfter = hydroModel.CurrentWorkflow;

                Assert.AreSame(workFlowBefore, workFlowAfter);
            }
            finally
            {
                ProjectRepository.Close();
            }
        }
    }
}