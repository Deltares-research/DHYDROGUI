using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    [TestFixture]
    public class RealTimeControlUserInterfaceIntegrationTests
    {

        private static IGui CreateGui()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new RealTimeControlApplicationPlugin(),
                new ProjectExplorerGuiPlugin(),
            };
            return new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build();
        }
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void SaveAndLoadModelResultsInValidModel()
        {
            // next line is a hack to force loading of RTCShapes.dll
            var _ = new InputItemShape();

            var path = "SaveAndLoad.dsproj";
            using (var gui = CreateGui())
            {
                IApplication app = gui.Application;
                gui.Run();

                app.CreateNewProject();

                Project project = app.Project;

                project.RootFolder.Add(RealTimeControlTestHelper.GenerateTestModel(false));

                app.SaveProjectAs(path);
                app.CloseProject();

                app.OpenProject(path);
                project = app.Project;

                IModel model = project.RootFolder.Models.First();

                var retrievedModel = (RealTimeControlModel) model;
                ControlGroup retrievedControlGroup = retrievedModel.ControlGroups.First();
                ControlGroup resultControlGroup =
                    RealTimeControlTestHelper.GenerateTestModel(false).ControlGroups.First();

                Assert.AreEqual(RealTimeControlTestHelper.GenerateTestModel(false).Name, retrievedModel.Name);
                Assert.AreEqual(resultControlGroup.Name, retrievedControlGroup.Name);
                Assert.AreEqual(resultControlGroup.Inputs.First().Name,
                                retrievedControlGroup.Inputs.First().Name);
                Assert.AreEqual(resultControlGroup.Outputs.First().Name,
                                retrievedControlGroup.Outputs.First().Name);
                Assert.AreEqual(resultControlGroup.Rules.First().Name,
                                retrievedControlGroup.Rules.First().Name);
                Assert.AreEqual(resultControlGroup.Conditions.First().Name,
                                retrievedControlGroup.Conditions.First().Name);
            }
        }
    }
}