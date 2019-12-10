using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using NUnit.Framework;
using DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters;
using NSubstitute;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.Gui.NodePresenters
{
    [TestFixture]
    public class ContextMenuFactoryTest
    {
        [TestCase(true)]
        [TestCase(false)]
        public void CreateMenuFor_WithDataObject_ReturnImportToolStripMenu_CheckEnabled(bool canImportOn)
        {
            // Arrange
            var data = Substitute.For<object>();

            var commandHandler = Substitute.For<IGuiCommandHandler>();
            commandHandler.CanImportOn(data).Returns(canImportOn);

            var gui = Substitute.For<IGui>();
            gui.CommandHandler.Returns(commandHandler);

            // Act
            ContextMenuStrip menu = ContextMenuFactory.CreateMenuFor(data, gui, Substitute.For<ITreeNodePresenter>(), Substitute.For<ITreeNode>());

            // Assert
            ToolStripItem importToolStripItem = null;
            foreach (ToolStripItem toolStripItem in menu.Items)
            {
                if (toolStripItem.Text == "&Import...")
                {
                    importToolStripItem = toolStripItem;
                }
            }

            Assert.IsNotNull(importToolStripItem);
            Assert.That(importToolStripItem.Enabled, Is.EqualTo(canImportOn));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void CreateMenuFor_WithDataObject_ReturnExportToolStripMenu_CheckEnabled(bool canExportFrom)
        {
            // Arrange
            var data = Substitute.For<object>();

            var commandHandler = Substitute.For<IGuiCommandHandler>();
            commandHandler.CanExportFrom(data).Returns(canExportFrom);

            var gui = Substitute.For<IGui>();
            gui.CommandHandler.Returns(commandHandler);

            // Act
            ContextMenuStrip menu = ContextMenuFactory.CreateMenuFor(data, gui, Substitute.For<ITreeNodePresenter>(), Substitute.For<ITreeNode>());

            // Assert
            ToolStripItem importToolStripItem = null;
            foreach (ToolStripItem toolStripItem in menu.Items)
            {
                if (toolStripItem.Text == "&Export...")
                {
                    importToolStripItem = toolStripItem;
                }
            }

            Assert.IsNotNull(importToolStripItem);
            Assert.That(importToolStripItem.Enabled, Is.EqualTo(canExportFrom));
        }
    }
}