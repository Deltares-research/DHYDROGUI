using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Gui.Forms.ViewManager;
using DeltaShell.Plugins.FMSuite.Common.Gui;
using DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.Gui
{
    [TestFixture]
    public class ModelTreeShortcutNodePresenterBaseTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var shortcutNode = new TestModelTreeShortcutNodePresenterBase();

            // Assert
            Assert.That(shortcutNode, Is.InstanceOf<TreeViewNodePresenterBaseForPluginGui<ModelTreeShortcut>>());
        }

        [Test]
        public void OnNodeDoubleClicked_NodeDataNull_ReturnsTrue()
        {
            // Setup
            var shortCutNode = new TestModelTreeShortcutNodePresenterBase();

            // Call
            bool result = shortCutNode.OnNodeDoubleClicked(null);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void OnNodeDoubleClicked_UnsupportedNodeData_ReturnsTrue()
        {
            // Setup
            var shortCutNode = new TestModelTreeShortcutNodePresenterBase();

            // Call
            bool result = shortCutNode.OnNodeDoubleClicked(null);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        [TestCase(ShortCutType.SettingsTab, true)]
        [TestCase(ShortCutType.SpatialCoverage, false)]
        [TestCase(ShortCutType.SpatialCoverageWithView, true)]
        [TestCase(ShortCutType.FeatureSet, true)]
        public void OnNodeDoubleClicked_NodeDataWithDifferentShortCutTypes_ReturnsExpectedResultAndDoesNotOpenGridEditor(
            ShortCutType shortCutType, bool expectedResult)
        {
            // Setup
            var mocks = new MockRepository();
            var documentViews = new ViewList(mocks.Stub<IDockingManager>(), ViewLocation.Top);
            var gui = mocks.Stub<IGui>();
            gui.Stub(g => g.DocumentViews).Return(documentViews);

            var model = mocks.Stub<IModel>();
            var guiPlugin = mocks.Stub<GuiPlugin>();
            mocks.ReplayAll();

            guiPlugin.Gui = gui;

            var nodeData = new TestModelTreeShortcut(model, shortCutType);
            var shortCutNode = new TestModelTreeShortcutNodePresenterBase {GuiPlugin = guiPlugin};

            // Call
            bool result = shortCutNode.OnNodeDoubleClicked(nodeData);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
            Assert.That(shortCutNode.OpenGridEditorShortCut, Is.Null,
                        "ShortCutTypes not equal to grid should not open a grid editor.");
            mocks.VerifyAll();
        }

        [Test]
        public void OnNodeDoubleClicked_NodeDataWithShortCutTypeGrid_ReturnsFalseAndOpensGridEditor()
        {
            // Setup
            var mocks = new MockRepository();
            var model = mocks.Stub<IModel>();
            var commandHandler = mocks.StrictMock<IGuiCommandHandler>();
            commandHandler.Expect(ch => ch.OpenView(model));

            var documentViews = new ViewList(mocks.Stub<IDockingManager>(), ViewLocation.Top);
            var gui = mocks.Stub<IGui>();
            gui.Stub(g => g.DocumentViews).Return(documentViews);

            var guiPlugin = mocks.Stub<GuiPlugin>();
            mocks.ReplayAll();

            gui.CommandHandler = commandHandler;
            guiPlugin.Gui = gui;

            var nodeData = new TestModelTreeShortcut(model, ShortCutType.Grid);
            var shortCutNode = new TestModelTreeShortcutNodePresenterBase {GuiPlugin = guiPlugin};

            // Call
            bool result = shortCutNode.OnNodeDoubleClicked(nodeData);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(shortCutNode.OpenGridEditorShortCut, Is.SameAs(nodeData),
                        "Grid Editor should be opened with the same data as the shortcut node.");
            mocks.VerifyAll();
        }

        [Test]
        public void OnNodeDoubleClicked_NodeDataWithUndefinedShortcutType_ReturnsTrue()
        {
            // Setup
            const ShortCutType shortCutType = (ShortCutType) 999;
            var nodeData = new TestModelTreeShortcut(null, shortCutType);
            var shortCutNode = new TestModelTreeShortcutNodePresenterBase();

            // Call
            bool result = shortCutNode.OnNodeDoubleClicked(nodeData);

            // Assert
            Assert.That(result, Is.True);
        }

        private class TestModelTreeShortcut : ModelTreeShortcut
        {
            public TestModelTreeShortcut(IModel model, ShortCutType shortCutType)
                : base(null, null, model, null, shortCutType) {}
        }

        private class TestModelTreeShortcutNodePresenterBase : ModelTreeShortcutNodePresenterBase<ModelTreeShortcut>
        {
            public ModelTreeShortcut OpenGridEditorShortCut { get; private set; }

            protected override void OpenGridEditor(ModelTreeShortcut shortcut)
            {
                OpenGridEditorShortCut = shortcut;
            }
        }
    }
}