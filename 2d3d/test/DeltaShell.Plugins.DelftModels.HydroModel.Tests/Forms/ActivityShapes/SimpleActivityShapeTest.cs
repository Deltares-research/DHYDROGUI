using System.Drawing;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.ActivityShapes;
using Netron.GraphLib.UI;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Forms.ActivityShapes
{
    [TestFixture]
    public class SimpleActivityShapeTest
    {
        [Test]
        public void ActivityShapeConstructorTest()
        {
            var shape = new SimpleActivityShape(null);
            Assert.AreEqual("[Not set]", shape.Text);
        }

        [Test]
        public void GetRequiredSize()
        {
            var graphControl = new GraphControl
            {
                AllowDrop = false,
                AllowAddConnection = false,
                AllowAddShape = false,
                AllowDeleteShape = false,
                AllowMoveShape = false
            };
            Graphics graphics = graphControl.Graphics;

            var shape = new SimpleActivityShape(graphControl);
            var size = TypeUtils.CallPrivateMethod<SizeF>(shape, "GetRequiredSize", graphics);

            Assert.Greater(size.Width, 1,
                           "Should require more width than 1 pixel.");
            Assert.Greater(size.Height, 1,
                           "Should require mode height than 1 pixel.");
            Assert.AreEqual(new RectangleF(0, 0, 1, 1), shape.Rectangle,
                            "GetRequiredSize should not set actual size.");
        }

        [Test]
        public void GetTextFromActivityName()
        {
            var shape = new SimpleActivityShape(null);
            Assert.AreEqual("[Not set]", shape.Text);

            shape.Activity = ActivityShapeTestHelper.CreateSimpleActivity("Test");
            Assert.AreEqual("Test", shape.Text);

            shape.Activity = null;
            Assert.AreEqual("[Not set]", shape.Text);
        }
    }
}