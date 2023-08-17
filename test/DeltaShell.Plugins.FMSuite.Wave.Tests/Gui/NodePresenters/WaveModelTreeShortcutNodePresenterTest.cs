using System.Drawing;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.FMSuite.Common.Gui;
using DeltaShell.Plugins.FMSuite.Wave.Gui;
using DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters;
using NetTopologySuite.Extensions.Grids;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.NodePresenters
{
    [TestFixture]
    public class WaveModelTreeShortcutNodePresenterTest
    {
        [Test]
        public void OnDoubleClickingTheWaveModelTreeShortcut_GridNotFoundInOuterDomain_DoesNotOpenGridEditor()
        {
            // Setup
            using (var model = new WaveModel())
            {
                CurvilinearGrid originalGrid = model.OuterDomain.Grid;
                var otherGrid = CurvilinearGrid.CreateDefault();

                const string random = "random";
                var randomBitmap = new Bitmap(1, 1);
                var shortcut = new WaveModelTreeShortcut(random, randomBitmap, model, otherGrid, ShortCutType.Grid);

                GuiPlugin guiPlugin = SetUpGuiPlugin();

                var gridShortcutNodePresenter = new WaveModelTreeShortcutNodePresenter() { GuiPlugin = guiPlugin };

                // Precondition
                Assert.That(originalGrid, Is.Not.SameAs(otherGrid));

                // Call
                void Call() => gridShortcutNodePresenter.OnNodeDoubleClicked(shortcut);

                // Assert
                Assert.That(Call, Throws.Nothing);
                guiPlugin.Gui.CommandHandler.Received(1).OpenView(model);
            }
        }

        private static GuiPlugin SetUpGuiPlugin()
        {
            var guiPlugin = Substitute.For<GuiPlugin>();
            var gui = Substitute.For<IGui>();
            guiPlugin.Gui.Returns(gui);
            var commandHandler = Substitute.For<IGuiCommandHandler>();
            gui.CommandHandler.Returns(commandHandler);

            return guiPlugin;
        }
    }
}