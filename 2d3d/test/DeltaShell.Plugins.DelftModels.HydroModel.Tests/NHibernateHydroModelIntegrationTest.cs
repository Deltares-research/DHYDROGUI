using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.IntegrationTestUtils.NHibernate;
using DeltaShell.NGHS.TestUtils.Builders;
using DeltaShell.Plugins.Data.NHibernate.DelftTools.Shell.Core.Dao;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
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
            HydroModel hydroModel = hydroModelBuilder.BuildModel(ModelGroup.All);

            HydroModel retrievedHydroModel = SaveAndRetrieveObject(hydroModel);

            Assert.That(retrievedHydroModel.Activities, Has.Count.EqualTo(3),
                        "3 activities (fm, waves, rtc) were expected in hydro model");
            Assert.That(retrievedHydroModel.Region.AllRegions.Count(), Is.EqualTo(2),
                        "2 regions were expected in hydro model");
        }

        [Test]
        public void SaveLoadHydroModelWithCurrentWorkflowData()
        {
            var hydroModelBuilder = new HydroModelBuilder();
            HydroModel hydroModel = hydroModelBuilder.BuildModel(ModelGroup.FMWaveRtcModels);

            IEventedList<IActivity> activities = hydroModel.CurrentWorkflow.Activities;
            ValidateWorkflow(activities);

            HydroModel retrievedHydroModel = SaveAndRetrieveObject(hydroModel);

            IEventedList<IActivity> retrievedWorkflowActivities = retrievedHydroModel.CurrentWorkflow.Activities;
            ValidateWorkflow(retrievedWorkflowActivities);
        }

        [Test]
        public void SaveLoadAndReSaveCompositeHydroModelWorkFlowData()
        {
            var compositeHydroModelWorkFlowData = new CompositeHydroModelWorkFlowData
            {
                HydroModelWorkFlowDataLookUp = new Dictionary<IHydroModelWorkFlowData, IList<int>>
                {
                    {
                        new CompositeHydroModelWorkFlowData(), new List<int>(new[]
                        {
                            1,
                            4,
                            7,
                            2
                        })
                    }
                }
            };

            CompositeHydroModelWorkFlowData retrievedcompositeHydroModelWorkFlowData = SaveAndRetrieveObject(compositeHydroModelWorkFlowData);
            var innerCompositeHydroModelWorkFlowData = (CompositeHydroModelWorkFlowData) retrievedcompositeHydroModelWorkFlowData.WorkFlowDatas.First();

            // resave
            ProjectRepository.SaveOrUpdate(ProjectRepository.GetProject());

            Assert.AreEqual(new List<int>(new[]
            {
                1,
                4,
                7,
                2
            }), retrievedcompositeHydroModelWorkFlowData.HydroModelWorkFlowDataLookUp[innerCompositeHydroModelWorkFlowData]);
        }

        [Test]
        public void SaveLoadHydroModelWithCurrentWorkflow()
        {
            var hydroModelBuilder = new HydroModelBuilder();
            HydroModel hydroModel = hydroModelBuilder.BuildModel(ModelGroup.All);

            hydroModel.CurrentWorkflow = hydroModel.Workflows.ElementAt(2);

            HydroModel retrievedHydroModel = SaveAndRetrieveObject(hydroModel);
            Assert.AreEqual(2, retrievedHydroModel.Workflows.IndexOf(retrievedHydroModel.CurrentWorkflow));
        }

        [Test]
        public void SaveHydroModelDoesNotCausePropertyChangeOnWorkflowTools9667()
        {
            var hydroModelBuilder = new HydroModelBuilder();
            HydroModel hydroModel = hydroModelBuilder.BuildModel(ModelGroup.All);

            hydroModel.CurrentWorkflow = hydroModel.Workflows.ElementAt(2);

            string path = TestHelper.GetCurrentMethodName() + "1.dsproj";
            var project = new Project();
            project.RootFolder.Add(hydroModel);

            ProjectRepository.Create(path);

            try
            {
                ICompositeActivity workFlowBefore = hydroModel.CurrentWorkflow;
                ProjectRepository.SaveOrUpdate(project);
                ICompositeActivity workFlowAfter = hydroModel.CurrentWorkflow;

                Assert.AreSame(workFlowBefore, workFlowAfter);
            }
            finally
            {
                ProjectRepository.Close();
            }
        }

        private static void ValidateWorkflow(IEventedList<IActivity> activities)
        {
            Assert.That(activities, Has.Count.EqualTo(2),
                        "2 activities were expected in the current workflow of the hydro model.");
            var rtcModel = (activities.First() as ActivityWrapper)?.Activity as RealTimeControlModel;
            Assert.That(rtcModel, Is.Not.Null,
                        "Real time control model was expected to be in the current workflow of the hydro model");
            var fmModel = (activities.Last() as ActivityWrapper)?.Activity as WaterFlowFMModel;
            Assert.That(fmModel, Is.Not.Null,
                        "WaterFlow FM model was expected to be in the current workflow of the hydro model");
        }
        
        protected override NHibernateProjectRepository CreateProjectRepository()
        {
            return new DHYDRONHibernateProjectRepositoryBuilder().WithRealTimeControl()
                                                                 .WithWaterQuality()
                                                                 .WithFlowFM()
                                                                 .WithWaves()
                                                                 .Build();
        }
    }
}