using System.Drawing;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using Netron.GraphLib.UI;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RTCShapes.Tests
{
    [TestFixture]
    public class RuleShapeTests
    {
        private GraphControl graphControl;
        private RuleShape shape;

        [SetUp]
        public void SetUp()
        {
            shape = new RuleShape();
            graphControl = new GraphControl();
            graphControl.AddLibrary(typeof(RuleShape).Module.FullyQualifiedName);
        }

        [Test]
        public void CreateNewRuleShape()
        {
            var ruleShape = new RuleShape();
            Assert.IsNotNull(ruleShape);
        }

        [Test]
        public void InitializedShapeIsRectangle()
        {
            var ruleShape = new RuleShape();
            var rectangle = new RectangleF(0, 0, 60, 40);
            Color shapeColor = Color.WhiteSmoke;
            Assert.AreEqual(rectangle, ruleShape.Rectangle);
            Assert.AreEqual(shapeColor, ruleShape.ShapeColor);
        }

        [Test]
        public void GetThumbNail()
        {
            Assert.That(shape.GetThumbnail(), Is.TypeOf(typeof(Bitmap)));
        }
    }
}