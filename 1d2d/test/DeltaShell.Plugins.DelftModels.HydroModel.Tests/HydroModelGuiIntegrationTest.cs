using System;
using System.Linq;
using System.Threading;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DeltaShell.Gui.Forms.MainWindow;
using DeltaShell.NGHS.TestUtils.Builders;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    [Category(TestCategory.WindowsForms)]
    public class HydroModelGuiIntegrationTest
    {
        private IGui gui;
        private Project project;

        [SetUp]
        public void SetUp()
        {
            InitializeGui();
        }

        [TearDown]
        public void TearDown()
        {
            DisposeGui();
        }

        private void InitializeGui()
        {
            gui = new DHYDROGuiBuilder().WithRealTimeControl().WithFlowFM().WithRainfallRunoff().Build();
            gui.Run();
            project = gui.Application.ProjectService.CreateProject();
        }

        private void DisposeGui()
        {
            gui.Dispose();

            gui = null;
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void Add2D3DIntegratedModelAddFMModelRemoveIntegratedModel()
        {
            var mainWindow = (MainWindow) gui.MainWindow;

            if (!gui.DocumentViewsResolver.DefaultViewTypes.ContainsKey(typeof(WaterFlowFMModel)))
                gui.DocumentViewsResolver.DefaultViewTypes.Add(typeof(WaterFlowFMModel), typeof(WaterFlowFMFileStructureView));

            Action mainWindowShown = delegate
            {
                var hydroModelBuilder = new HydroModelBuilder();
                using (var integratedModel2D3D = hydroModelBuilder.BuildModel(ModelGroup.FMWaveRtcModels))
                {
                    using (var waterFlowFMModel = new WaterFlowFMModel())
                    {
                        gui.CommandHandler.AddItemToProject(integratedModel2D3D);
                        gui.Selection = integratedModel2D3D;
                        gui.CommandHandler.OpenViewForSelection();
                        gui.CommandHandler.AddItemToProject(waterFlowFMModel);
                        gui.Selection = waterFlowFMModel;
                        gui.CommandHandler.OpenViewForSelection();
                        project.RootFolder.Items.Remove(integratedModel2D3D);
                        Assert.IsTrue(project.RootFolder.GetAllModelsRecursive().SequenceEqual(new[] {waterFlowFMModel}));
                    }
                }
            };
            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }
        
        [Test]
        [Category(TestCategory.Slow)]
        public void FmModelShouldBeReplacedWhenImportedInIntegratedModel()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            /* create a integrated model */
            var hydroModel = HydroModel.BuildModel(ModelGroup.FMWaveRtcModels);

            /* add it to you project */
            project.RootFolder.Add(hydroModel);

            // wait until gui starts
            Action mainWindowShown = delegate
            {
                /* get the water flow fm model */
                var waterFlowFmModel = hydroModel.Activities.OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.NotNull(waterFlowFmModel);
                Assert.IsTrue(waterFlowFmModel.Name.StartsWith("FlowFM"));

                var fmImporter = gui.Application.FileImporters.OfType<WaterFlowFMFileImporter>().FirstOrDefault();
                Assert.IsNotNull(fmImporter);
                fmImporter.ImportItem(mduPath, waterFlowFmModel);

                var targetFmModel = hydroModel.Activities.OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.IsNotNull(targetFmModel);
                Assert.IsTrue(targetFmModel.Name.StartsWith("har"));
            };
            WpfTestHelper.ShowModal((MainWindow)gui.MainWindow, mainWindowShown);
        }
    }
}