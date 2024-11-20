using DelftTools.Controls.Swf;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.NodePresenters;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class RainfallRunoffAreasTreeFolderNodePresenterTest
    {     
        private MockRepository mocks = new MockRepository();
       
        [Test]
        public void ShowContextMenu()
        {
            var model = new RainfallRunoffModel();
            var pluginGui = mocks.StrictMock<GuiPlugin>();

            var nodePresenter = new CatchmentModelDataTreeFolderProjectNodePresenter(pluginGui);
            var menuItem = nodePresenter.GetContextMenu(null,
                                                           new TreeFolder(model, new object[0], "areas",
                                                                          FolderImageType.None));

            var adapter = (MenuItemContextMenuStripAdapter)menuItem;
            WindowsFormsTestHelper.ShowModal(adapter.ContextMenuStrip);
        }

        [Test]
        public void ShowContextMenuAndClickImport()
        {
            var model = new RainfallRunoffModel();
            var pluginGui = mocks.StrictMock<GuiPlugin>();
            var gui = mocks.StrictMock<IGui>();
            var commandHandler = mocks.StrictMock<IGuiCommandHandler>();

            pluginGui.Expect(pg => pg.Gui).Return(gui).IgnoreArguments().Repeat.Any();
            gui.Expect(g => g.CommandHandler).Return(commandHandler).IgnoreArguments().Repeat.Any();
            gui.Expect(g => g.Selection).SetPropertyAndIgnoreArgument().Repeat.Any();
            commandHandler.Expect(ch => ch.ImportToGuiSelection()).IgnoreArguments().Repeat.Once();

            mocks.ReplayAll();

            var nodePresenter = new CatchmentModelDataTreeFolderProjectNodePresenter(pluginGui);
            var menuItem = nodePresenter.GetContextMenu(null,
                                                           new TreeFolder(model, new object[0], "areas",
                                                                          FolderImageType.None));

            var adapter = (MenuItemContextMenuStripAdapter)menuItem;
            WindowsFormsTestHelper.ShowModal(adapter.ContextMenuStrip,
                                             f => adapter.ContextMenuStrip.Items[0].PerformClick());

            mocks.VerifyAll();
        }
    }
}
