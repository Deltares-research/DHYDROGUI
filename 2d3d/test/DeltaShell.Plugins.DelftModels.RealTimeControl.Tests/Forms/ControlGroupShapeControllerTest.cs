using System.Collections.Generic;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Forms
{
    [TestFixture]
    public class ControlGroupShapeControllerTest
    {
        private IGuiContextManager guiContextManager;

        [SetUp]
        public void SetUp()
        {
            guiContextManager = Substitute.For<IGuiContextManager>();
        }

        [Test]
        public void Constructor_GuiContextManagerIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => new ControlGroupShapeController(null), Throws.ArgumentNullException);
        }

        [Test]
        public void GetShapes_ControlGroupIsNull_ThrowsArgumentNullException()
        {
            ControlGroupShapeController controller = CreateController();

            Assert.That(() => controller.GetShapes(null), Throws.ArgumentNullException);
        }

        [Test]
        public void GetShapes_NoViewContextRegistered_ReturnsEmptyCollection()
        {
            ControlGroup controlGroup = CreateControlGroup();
            ControlGroupShapeController controller = CreateController();

            IEnumerable<ShapeBase> shapes = controller.GetShapes(controlGroup);

            Assert.That(shapes, Is.Empty);
        }

        [Test]
        public void GetShapes_ViewContextRegistered_ReturnsShapesCollection()
        {
            ControlGroup controlGroup = CreateControlGroup();
            ControlGroupEditorViewContext viewContext = CreateViewContext();
            ControlGroupShapeController controller = CreateController();

            viewContext.ControlGroup = controlGroup;
            viewContext.ShapeList = CreateShapes();

            RegisterViewContext(viewContext);

            IEnumerable<ShapeBase> shapes = controller.GetShapes(controlGroup);

            Assert.That(shapes, Is.EquivalentTo(viewContext.ShapeList));
        }

        [Test]
        public void SetShapes_ControlGroupIsNull_ThrowsArgumentNullException()
        {
            ControlGroupShapeController controller = CreateController();

            Assert.That(() => controller.SetShapes(null, CreateShapes()), Throws.ArgumentNullException);
        }

        [Test]
        public void SetShapes_ShapesIsNull_ThrowsArgumentNullException()
        {
            ControlGroupShapeController controller = CreateController();

            Assert.That(() => controller.SetShapes(CreateControlGroup(), null), Throws.ArgumentNullException);
        }

        [Test]
        public void SetShapes_ViewContextRegistered_UpdatesShapesList()
        {
            IEnumerable<ShapeBase> shapes = CreateShapes();
            ControlGroup controlGroup = CreateControlGroup();
            ControlGroupEditorViewContext viewContext = CreateViewContext();
            ControlGroupShapeController controller = CreateController();

            viewContext.ControlGroup = controlGroup;
            RegisterViewContext(viewContext);

            controller.SetShapes(controlGroup, shapes);

            Assert.That(shapes, Is.EquivalentTo(viewContext.ShapeList));
        }
        
        [Test]
        public void SetShapes_NoViewContextRegistered_RegistersViewContextWithShapesList()
        {
            IEnumerable<ShapeBase> shapes = CreateShapes();
            ControlGroup controlGroup = CreateControlGroup();
            ControlGroupShapeController controller = CreateController();

            controller.SetShapes(controlGroup, shapes);

            guiContextManager.Received(1).AddViewContext(
                Arg.Is<ControlGroupEditorViewContext>(c => c.ShapeList.SequenceEqual(shapes)));
        }

        private ControlGroupShapeController CreateController()
            => new ControlGroupShapeController(guiContextManager);

        private static ControlGroupEditorViewContext CreateViewContext()
            => new ControlGroupEditorViewContext();

        private static ControlGroup CreateControlGroup()
            => new ControlGroup();

        private static IList<ShapeBase> CreateShapes()
            => new List<ShapeBase>
            {
                new InputItemShape(),
                new OutputItemShape()
            };

        private void RegisterViewContext(IViewContext viewContext)
            => guiContextManager.ProjectViewContexts.Returns(new[] { viewContext });
    }
}