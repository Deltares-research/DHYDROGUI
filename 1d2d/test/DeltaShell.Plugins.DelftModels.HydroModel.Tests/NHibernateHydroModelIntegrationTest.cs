using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.IntegrationTestUtils.NHibernate;
using DeltaShell.NGHS.TestUtils.Builders;
using DeltaShell.Plugins.Data.NHibernate.DelftTools.Shell.Core.Dao;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class NHibernateHydroModelIntegrationTest : NHibernateIntegrationTestBase
    {
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

        [Test]
        public void GivenAProjectWithAnIntegratedModelWithHydroLinks_FromVersion202204_MigratesAndLoadsCorrectly()
        {
            // Setup
            using (var temp = new TemporaryDirectory())
            using (var app = new DHYDROApplicationBuilder().WithRainfallRunoff().WithFlowFM().WithHydroModel().Build())
            {
                app.Run();
                
                string file = temp.CopyTestDataFileAndDirectoryToTempDirectory(Path.Combine("BackwardCompatibility", "ProjectWithHydrolinksCreatedWith2022.04.dsproj"));

                // Call
                Project project = app.ProjectService.OpenProject(file);
                
                // Assert
                HydroModel integratedModel = project.GetAllItemsRecursive().OfType<HydroModel>().FirstOrDefault();
                Assert.That(integratedModel, Is.Not.Null);
                
                IEventedList<HydroLink> links = integratedModel.Region.Links;
                Assert.That(links.Count, Is.EqualTo(2));
                Assert.That(links[0].Source, Is.TypeOf<Catchment>());
                Assert.That(links[0].Target, Is.TypeOf<LateralSource>());
                Assert.That(links[1].Source, Is.TypeOf<Catchment>());
                Assert.That(links[1].Target, Is.TypeOf<LateralSource>());
            }
        }

        protected override NHibernateProjectRepository CreateProjectRepository()
        {
            return new DHYDRONHibernateProjectRepositoryBuilder().WithRainfallRunoff().WithRealTimeControl().WithFlowFM().Build();
        }
    }
}