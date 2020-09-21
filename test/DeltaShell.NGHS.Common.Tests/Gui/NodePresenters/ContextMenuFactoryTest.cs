using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DeltaShell.NGHS.Common.Gui.NodePresenters;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.Gui.NodePresenters
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
                if (toolStripItem.Text == "Import ...")
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
                if (toolStripItem.Text == "Export ...")
                {
                    importToolStripItem = toolStripItem;
                }
            }

            Assert.IsNotNull(importToolStripItem);
            Assert.That(importToolStripItem.Enabled, Is.EqualTo(canExportFrom));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GivenItem_WhenCanOpenSelectViewDialogAndGetViewInfosGreaterThanOne_ThenOpenAndOpenWithIsVisibleAndEnabled(bool canOpen)
        {
            // Given
            var item = Substitute.For<object>();
            var commandHandler = Substitute.For<IGuiCommandHandler>();
            commandHandler.CanOpenSelectViewDialog().Returns(canOpen);

            var documentViewsResolver = Substitute.For<IViewResolver>();
            var viewInfo = new ViewInfo();
            documentViewsResolver.GetViewInfosFor(item).Returns(new[]
            {
                viewInfo,
                viewInfo
            });

            var gui = Substitute.For<IGui>();
            gui.CommandHandler.Returns(commandHandler);
            gui.DocumentViewsResolver.Returns(documentViewsResolver);

            // When
            ContextMenuStrip menu = ContextMenuFactory.CreateMenuFor(item, gui, Substitute.For<ITreeNodePresenter>(), Substitute.For<ITreeNode>());

            // Then
            ToolStripItem openAndOpenWithItem = null;
            foreach (ToolStripItem toolStripItem in menu.Items)
            {
                if (toolStripItem.Text == "Open with ...")
                {
                    openAndOpenWithItem = toolStripItem;
                }
            }

            Assert.That(openAndOpenWithItem != null, Is.EqualTo(canOpen));
            if (!canOpen)
            {
                return;
            }

            Assert.That(openAndOpenWithItem.Enabled);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GivenItem_WhenCanOpenSelectViewDialogAndGetViewInfosLesserOrEqualThanOne_ThenOpenAndOpenWithIsNotVisibleOrEnabled(bool canOpen)
        {
            // Given
            var item = Substitute.For<object>();
            var commandHandler = Substitute.For<IGuiCommandHandler>();
            commandHandler.CanOpenSelectViewDialog().Returns(canOpen);

            var documentViewsResolver = Substitute.For<IViewResolver>();
            var viewInfo = new ViewInfo();
            documentViewsResolver.GetViewInfosFor(item).Returns(new[]
            {
                viewInfo
            });

            var gui = Substitute.For<IGui>();
            gui.CommandHandler.Returns(commandHandler);
            gui.DocumentViewsResolver.Returns(documentViewsResolver);

            // When
            ContextMenuStrip menu = ContextMenuFactory.CreateMenuFor(item, gui, Substitute.For<ITreeNodePresenter>(), Substitute.For<ITreeNode>());

            // Then
            ToolStripItem openAndOpenWithItem = null;
            foreach (ToolStripItem toolStripItem in menu.Items)
            {
                if (toolStripItem.Text == "Open with ...")
                {
                    openAndOpenWithItem = toolStripItem;
                }
            }

            Assert.That(openAndOpenWithItem, Is.Null);
        }
    }
}