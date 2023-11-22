using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.IntegrationTestUtils;
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
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void SaveAndLoadModelResultsInValidModel()
        {
            // next line is a hack to force loading of RTCShapes.dll
            var _ = new InputItemShape();

            var path = "SaveAndLoad.dsproj";
            using (var gui = DeltaShellCoreFactory.CreateGui())
            {
                IApplication application = gui.Application;
                application.Plugins.Add(new NHibernateDaoApplicationPlugin());
                application.Plugins.Add(new CommonToolsApplicationPlugin());
                application.Plugins.Add(new SharpMapGisApplicationPlugin());
                application.Plugins.Add(new NetworkEditorApplicationPlugin());
                application.Plugins.Add(new RealTimeControlApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Run();

                application.CreateNewProject();

                Project project = application.Project;

                project.RootFolder.Add(RealTimeControlTestHelper.GenerateTestModel(false));

                application.SaveProjectAs(path);
                application.CloseProject();

                application.OpenProject(path);
                project = application.Project;

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