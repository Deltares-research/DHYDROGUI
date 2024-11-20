using System.Drawing;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.ActivityShapes;
using Netron.GraphLib.UI;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Forms.ActivityShapes
{
    [TestFixture]
    public class ShapeFactoryTest
    {
        [Test]
        public void CreateSimpleActivityShape()
        {
            var graphControl = new GraphControl {AllowDrop = false};
            ActivityShapeBase shape = ShapeFactory.CreateShapeFromActivity(ActivityShapeTestHelper.CreateSimpleActivity(), graphControl);
            Assert.IsInstanceOf<SimpleActivityShape>(shape);
            AssertColor(Color.LightGray, shape.ShapeColor);
            Assert.AreSame(graphControl, shape.Site);
            Assert.Greater(shape.Rectangle.Width, 5,
                           "Size of element should be updated");
            Assert.Greater(shape.Rectangle.Height, 5,
                           "Size of element should be updated");
        }

        [Test]
        public void CreateParallelActivityShapeSimple()
        {
            var graphControl = new GraphControl {AllowDrop = false};
            ActivityShapeBase shape = ShapeFactory.CreateShapeFromActivity(new ParallelActivity {Name = "Test"}, graphControl);
            Assert.IsInstanceOf<ParallelActivityShape>(shape);
            AssertColor(Color.SandyBrown, shape.ShapeColor);
            Assert.AreSame(graphControl, shape.Site);
            Assert.Greater(shape.Rectangle.Width, 5,
                           "Size of element should be updated");
            Assert.Greater(shape.Rectangle.Height, 5,
                           "Size of element should be updated");
        }

        [Test]
        public void CreateSequentialActivityShapeSimple()
        {
            var graphControl = new GraphControl {AllowDrop = false};
            ActivityShapeBase shape = ShapeFactory.CreateShapeFromActivity(new SequentialActivity {Name = "Test"}, graphControl);
            Assert.IsInstanceOf<SequentialActivityShape>(shape);
            AssertColor(Color.LightSkyBlue, shape.ShapeColor);
            Assert.AreSame(graphControl, shape.Site);
            Assert.Greater(shape.Rectangle.Width, 5,
                           "Size of element should be updated");
            Assert.Greater(shape.Rectangle.Height, 5,
                           "Size of element should be updated");
        }

        [Test]
        public void CreateICompositeActivityShapeSimple()
        {
            var graphControl = new GraphControl {AllowDrop = false};
            ActivityShapeBase shape = ShapeFactory.CreateShapeFromActivity(ActivityShapeTestHelper.CreateSimpleCompositeActivity(), graphControl);
            Assert.IsInstanceOf<CompositeActivityShape>(shape);
            AssertColor(Color.GreenYellow, shape.ShapeColor);
            Assert.AreSame(graphControl, shape.Site);
            Assert.Greater(shape.Rectangle.Width, 5,
                           "Size of element should be updated");
            Assert.Greater(shape.Rectangle.Height, 5,
                           "Size of element should be updated");
        }

        private void AssertColor(Color reference, Color actual)
        {
            Assert.AreEqual(reference.A, actual.A);
            Assert.AreEqual(reference.R, actual.R);
            Assert.AreEqual(reference.G, actual.G);
            Assert.AreEqual(reference.B, actual.B);
        }
    }
}