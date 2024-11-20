using System.Drawing;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.ActivityShapes;
using Netron.GraphLib.UI;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Forms.ActivityShapes
{
    [TestFixture]
    public class ParallelActivityShapeTest
    {
        [Test]
        public void ParallelActivityShapeConstructorTest()
        {
            var shape = new ParallelActivityShape(null);

            Assert.AreEqual("Parallel activity", shape.Text);
            Assert.IsNull(shape.Activity);
        }

        [Test]
        public void GetRequiredSizeWithoutChildActivities()
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

            var shape = new ParallelActivityShape(null) {Activity = null};
            var size = TypeUtils.CallPrivateMethod<SizeF>(shape, "GetRequiredSize", graphics);

            Assert.Greater(size.Width, 1,
                           "Should require more width than 1 pixel.");
            Assert.Greater(size.Height, 1,
                           "Should require mode height than 1 pixel.");
            Assert.AreEqual(new RectangleF(0, 0, 1, 1), shape.Rectangle,
                            "GetRequiredSize should not set actual size.");

            shape.Activity = new ParallelActivity();
            size = TypeUtils.CallPrivateMethod<SizeF>(shape, "GetRequiredSize", graphics);

            Assert.Greater(size.Width, 1,
                           "Should require more width than 1 pixel.");
            Assert.Greater(size.Height, 1,
                           "Should require mode height than 1 pixel.");
            Assert.AreEqual(new RectangleF(0, 0, 1, 1), shape.Rectangle,
                            "GetRequiredSize should not set actual size.");
        }

        [Test]
        public void GetRequiredSizeWithChildActivities()
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

            var shape = new ParallelActivityShape(graphControl) {Activity = null};
            var emptySize = TypeUtils.CallPrivateMethod<SizeF>(shape, "GetRequiredSize", graphics);

            var parallelActivity = new ParallelActivity();
            parallelActivity.Activities.AddRange(new[]
            {
                ActivityShapeTestHelper.CreateSimpleActivity("Simple Activity 1"),
                ActivityShapeTestHelper.CreateSimpleActivity("Simple Activity 2"),
                ActivityShapeTestHelper.CreateSimpleActivity("Simple Activity 3")
            });
            shape = new ParallelActivityShape(graphControl) {Activity = parallelActivity};
            var size = TypeUtils.CallPrivateMethod<SizeF>(shape, "GetRequiredSize", graphics);

            Assert.Greater(size.Width, emptySize.Width,
                           "Should require more width than when it was empty.");
            Assert.Greater(size.Height, emptySize.Height,
                           "Should require mode height than when it was empty.");
            Assert.AreEqual(new RectangleF(0, 0, 1, 1), shape.Rectangle,
                            "GetRequiredSize should not set actual size.");
        }

        [Test]
        public void ThrowWhenAssigningIncompatibleActivity()
        {
            Assert.That(() => new ParallelActivityShape(null) {Activity = ActivityShapeTestHelper.CreateSimpleActivity()},
                        Throws.ArgumentException.With.Message.StartsWith("Value must be a ParallelActivity"));
        }
    }
}