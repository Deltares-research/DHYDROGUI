using System.Collections.Generic;
using System.Windows.Forms;
using DelftTools.Controls.Swf;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Forms;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Properties;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    public class HydroModelGuiTest
    {
        private static IEnumerable<TestCaseData> BeforeTagTestCaseData
        {
            get
            {
                var model = new HydroModel() {Name = "Blastoise"};
                yield return new TestCaseData(null, model);
                yield return new TestCaseData(new HydroModel() {Name = "Squirtle"}, model);
                yield return new TestCaseData(model, model);
            }
        }

        /// <summary>
        /// GIVEN a ProjectExplorer with an existing ContextMenu with a Tag
        /// AND a HydroModelGuiPlugin using this ProjectExplorer
        /// AND some other model not equal to the Tag
        /// WHEN GetContextMenu is called with this model
        /// THEN The the ContextMenu Tag is updated with this model
        /// </summary>
        [TestCaseSource(nameof(BeforeTagTestCaseData))]
        public void GivenAHydroModelGuiPlugin_WhenGetContextMenuIsCalledWithAModel_ThenTheContextMenuTagIsUpdated(object beforeTag, HydroModel model)
        {
            var validateItem = new ClonableToolStripMenuItem
            {
                Text = Resources.HydroModelGuiPlugin_GetContextMenu_Validate___,
                Tag = beforeTag,
            };

            HydroModelGuiPlugin plugin = GetConfiguredPlugin(validateItem);

            // Given
            plugin.GetContextMenu(null, model);

            // Then
            Assert.That(validateItem.Tag, Is.EqualTo(model));
        }

        private static HydroModelGuiPlugin GetConfiguredPlugin(ToolStripItem validateItem)
        {
            var gui = MockRepository.GenerateStub<IGui>();
            var application = MockRepository.GenerateStub<IApplication>();
            var activityRunner = MockRepository.GenerateStub<IActivityRunner>();

            application.Stub(a => a.ActivityRunner)
                       .Return(activityRunner);

            gui.Application = application;

            application.Stub(a => a.GetAllModelsInProject())
                       .Return(new List<IModel>());

            var mainWindow = MockRepository.GenerateStub<IMainWindow>();
            var projectExplorer = MockRepository.GenerateStub<IProjectExplorer>();

            var subMenu = new ContextMenuStrip();
            subMenu.Items.Add(validateItem);
            var contextMenuAdapter = new MenuItemContextMenuStripAdapter(subMenu);

            gui.Stub(g => g.MainWindow)
               .Return(mainWindow);
            mainWindow.Stub(mw => mw.ProjectExplorer)
                      .Return(projectExplorer);
            projectExplorer.Stub(pe => pe.GetContextMenu(null, null))
                           .IgnoreArguments()
                           .Return(contextMenuAdapter);

            var plugin = new HydroModelGuiPlugin {Gui = gui};
            return plugin;
        }
    }
}