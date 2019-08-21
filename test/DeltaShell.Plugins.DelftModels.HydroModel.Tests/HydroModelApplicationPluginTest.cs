using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui.Swf;
using DelftTools.TestUtils;
using DeltaShell.Core;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.WaterQualityModel;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.Wave;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class HydroModelApplicationPluginTest
    {
        private void SetUpApplication(DeltaShellApplication app, ApplicationPlugin appPlugin)
        {
            app.Plugins.Add(appPlugin);
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
        public void GetParentProjectItemTest_HydroModel()
        {
            using (var app = new DeltaShellApplication())
            {
                var appPlugin = new HydroModelApplicationPlugin();

                SetUpApplication(app, appPlugin);
                SetupIntegratedModelWithFmModelInsideProjectTree(app, out HydroModel integratedModel, out WaterFlowFMModel fmModel, out TreeFolder modelsFolder);

                CheckParentOfDifferentProjectTreeItems(appPlugin, app, integratedModel, modelsFolder, fmModel);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetParentProjectItemTest_RealTimeControl()
        {
            using (var app = new DeltaShellApplication())
            {
                var appPlugin = new RealTimeControlApplicationPlugin();

                SetUpApplication(app, appPlugin);
                SetupIntegratedModelWithFmModelInsideProjectTree(app, out HydroModel integratedModel, out WaterFlowFMModel fmModel, out TreeFolder modelsFolder);

                CheckParentOfDifferentProjectTreeItems(appPlugin, app, integratedModel, modelsFolder, fmModel);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetParentProjectItemTest_WaterQuality()
        {
            using (var app = new DeltaShellApplication())
            {
                var appPlugin = new WaterQualityModelApplicationPlugin();

                SetUpApplication(app, appPlugin);
                SetupIntegratedModelWithFmModelInsideProjectTree(app, out HydroModel integratedModel, out WaterFlowFMModel fmModel, out TreeFolder modelsFolder);
                
                CheckParentOfDifferentProjectTreeItems(appPlugin, app, integratedModel, modelsFolder, fmModel);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetParentProjectItemTest_FlowFM()
        {
            using (var app = new DeltaShellApplication())
            {
                var appPlugin = new FlowFMApplicationPlugin();

                SetUpApplication(app, appPlugin);
                SetupIntegratedModelWithFmModelInsideProjectTree(app, out HydroModel integratedModel, out WaterFlowFMModel fmModel, out TreeFolder modelsFolder);
                
                CheckParentOfDifferentProjectTreeItems(appPlugin, app, integratedModel, modelsFolder, fmModel);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetParentProjectItemTest_Wave()
        {
            using (var app = new DeltaShellApplication())
            {
                var appPlugin = new WaveApplicationPlugin();

                SetUpApplication(app, appPlugin);
                SetupIntegratedModelWithFmModelInsideProjectTree(app, out HydroModel integratedModel, out WaterFlowFMModel fmModel, out TreeFolder modelsFolder);

                CheckParentOfDifferentProjectTreeItems(appPlugin, app, integratedModel, modelsFolder, fmModel);
            }
        }

        private static void CheckParentOfDifferentProjectTreeItems(ApplicationPlugin appPlugin, IApplication app,
                                                                   HydroModel integratedModel, TreeFolder modelsFolder,
                                                                   WaterFlowFMModel fmModel)
        {
            ModelInfo modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
            Assert.NotNull(modelInfos);
            Assert.IsNull(modelInfos.GetParentProjectItem(app.Project.RootFolder));
            Assert.AreSame(modelInfos.GetParentProjectItem(integratedModel), integratedModel);
            Assert.AreSame(integratedModel, modelInfos.GetParentProjectItem(modelsFolder));
            Assert.AreSame(integratedModel, modelInfos.GetParentProjectItem(fmModel));
        }

        private static void SetupIntegratedModelWithFmModelInsideProjectTree(IApplication app,
                                                                             out HydroModel integratedModel,
                                                                             out WaterFlowFMModel fmModel,
                                                                             out TreeFolder modelsFolder)
        {
            integratedModel = new HydroModel();
            app.Project.RootFolder.Add(integratedModel);
            fmModel = new WaterFlowFMModel();
            integratedModel.Activities.Add(fmModel);
            modelsFolder = new TreeFolder(integratedModel, null, "models", FolderImageType.Input);
        }
    }
}
