using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Dimr.Gui;
using DeltaShell.Dimr.Gui.Properties;
using DeltaShell.IntegrationTestUtils.Builders;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Dimr.Tests
{
    [TestFixture]
    public class DimrGuiPluginTests
    {
        [Test]
        public void TestDimrGuiPlugin()
        {
            var dimrGuiPlugin = new DimrGuiPlugin();
            var pluginsToAdd = new List<IPlugin>()
            {
                dimrGuiPlugin
            };
            using (var gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build())
            {
                gui.Run();
                Assert.AreEqual(dimrGuiPlugin, DimrGuiPlugin.Instance);
                Assert.AreEqual("Dimr (UI)", DimrGuiPlugin.Instance.Name);
                Assert.AreEqual(Resources.DimrGuiPlugin_Description_Provides_possibilities_to_configure_DIMR_settings, DimrGuiPlugin.Instance.Description);
                Assert.That(DimrGuiPlugin.Instance.RibbonCommandHandler.GetType().Namespace, Does.StartWith("DeltaShell.Dimr.Gui"));
            }

            Assert.IsNull(DimrGuiPlugin.Instance);
        }

        [Test]
        public void TestIsOnlyDimrModelSelected()
        {
            var mocks = new MockRepository();

            var viewlist = mocks.DynamicMock<IViewList>();
            var gui = mocks.DynamicMock<IGui>();

            gui.Expect(g => g.DocumentViews).Return(viewlist).Repeat.Any();

            mocks.ReplayAll();

            using (var guiPlugin = new DimrGuiPlugin())
            {
                Assert.False(guiPlugin.IsOnlyDimrModelSelected);
                guiPlugin.Gui = gui;

                Assert.False(guiPlugin.IsOnlyDimrModelSelected);

                mocks.BackToRecordAll();

                var dimrModel = mocks.DynamicMock<IDimrModel>();
                gui.Expect(g => g.SelectedModel).Return(dimrModel).Repeat.Any();
                mocks.ReplayAll();

                Assert.True(guiPlugin.IsOnlyDimrModelSelected);

                mocks.BackToRecordAll();

                var workflow = mocks.DynamicMock<ICompositeActivity>();
                workflow.Expect(wf => wf.Activities).Return(new EventedList<IActivity>() {dimrModel}).Repeat.Any();

                var compositeActivity = mocks.DynamicMultiMock<IModel>(typeof(ICompositeActivity));
                ((ICompositeActivity) compositeActivity).Expect(ca => ca.CurrentWorkflow).Return(workflow).Repeat.Any();
                gui.Expect(g => g.SelectedModel).Return(compositeActivity).Repeat.Any();

                mocks.ReplayAll();

                Assert.True(guiPlugin.IsOnlyDimrModelSelected);
            }

            mocks.VerifyAll();
        }

        [Test]
        public void TestGetPersistentAssemblies()
        {
            using (var dimrGuiPlugin = new DimrGuiPlugin())
            {
                Assert.True(dimrGuiPlugin.GetPersistentAssemblies().Contains(typeof(DimrGuiPlugin).Assembly));
                Assert.AreEqual(1, dimrGuiPlugin.GetPersistentAssemblies().Count());
            }
        }
    }
}