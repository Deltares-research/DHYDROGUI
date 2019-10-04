using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Core;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.WaterQualityModel;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.Wave;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class HydroModelApplicationPluginTest
    {
        private static void SetUpApplication(DeltaShellApplication app, ApplicationPlugin appPlugin)
        {
            app.Project = new Project();
            appPlugin.Application = app;
        }

        [Test]
        public void AdditionalOwnerCheckTest_HydroModel()
        {
            using (var app = new DeltaShellApplication())
            {
                var appPlugin = new HydroModelApplicationPlugin();
                SetUpApplication(app, appPlugin);

                var modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(app.Project.RootFolder), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new HydroModel()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new ParallelActivity()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new SequentialActivity()), false);
            }
        }

        [Test]
        public void AdditionalOwnerCheckTest_RealTimeControl()
        {
            using (var app = new DeltaShellApplication())
            {
                var appPlugin = new RealTimeControlApplicationPlugin();
                SetUpApplication(app, appPlugin);

                var modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(app.Project.RootFolder), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new HydroModel()), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new ParallelActivity()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new SequentialActivity()), false);
            }
        }

        [Test]
        public void AdditionalOwnerCheckTest_WaterQuality()
        {
            using (var app = new DeltaShellApplication())
            {
                var appPlugin = new WaterQualityModelApplicationPlugin();
                SetUpApplication(app, appPlugin);

                var modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(app.Project.RootFolder), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new HydroModel()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new ParallelActivity()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new SequentialActivity()), false);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void AdditionalOwnerCheckTest_FlowFM()
        {
            using (var app = new DeltaShellApplication())
            {
                var appPlugin = new FlowFMApplicationPlugin();
                SetUpApplication(app, appPlugin);

                var modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(app.Project.RootFolder), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new HydroModel()), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new ParallelActivity()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new SequentialActivity()), false);
            }
        }

        [Test]
        public void AdditionalOwnerCheckTest_Wave()
        {
            using (var app = new DeltaShellApplication())
            {
                var appPlugin = new WaveApplicationPlugin();
                SetUpApplication(app, appPlugin);

                var modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(app.Project.RootFolder), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new HydroModel()), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new ParallelActivity()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new SequentialActivity()), false);
            }
        }


        [Test]
        [Category(TestCategory.Integration)]
        public void GetParentProjectItem_WhenSelectionIsCompositeActivity_ThenHelperMethodReturnsCompositeActivityAndThisWillBeUsed()
        {
            var hydroModelApplicationPlugin = new HydroModelApplicationPlugin();
            ApplicationPluginTestHelper.TestForGetParentProjectItemDelegateSetByApplicationPlugins_WhenApplicationPluginHelperReturnsNotNull(hydroModelApplicationPlugin);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetParentProjectItem_WhenSelectionIsNull_ThenHelperMethodReturnsNullAndRootFolderWillBeUsed()
        {
            var hydroModelApplicationPlugin = new HydroModelApplicationPlugin();
            ApplicationPluginTestHelper.TestForGetParentProjectItemDelegateSetByApplicationPlugins_WhenApplicationPluginHelperReturnsNull(hydroModelApplicationPlugin);
        }

        [Test]
        public void GivenAnApplicationWithOnlyHydroModelAndFlowFmPlugins_WhenCallingCanImportOnRootLevelForDIMRImporter_TrueShouldBeReturnedSinceFmIsAdded()
        {
            using (var app = new DeltaShellApplication())
            {
                // Given
                var hydroModelAppPlugin = new HydroModelApplicationPlugin();
                var fmModelAppPlugin = new FlowFMApplicationPlugin();
                
                app.Plugins.Add(hydroModelAppPlugin);
                app.Plugins.Add(fmModelAppPlugin);

                app.Run();

                // When Then
                var dimrImporter = (DHydroConfigXmlImporter)hydroModelAppPlugin.GetFileImporters().ToList().FirstOrDefault();
                Assert.IsNotNull(dimrImporter);
                Assert.IsTrue(dimrImporter.CanImportOnRootLevel);
            }
        }
    }
}
