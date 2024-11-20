using System.Drawing;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using Netron.GraphLib.UI;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RTCShapes.Tests
{
    [TestFixture]
    public class SignalShapeTests
    {
        private GraphControl graphControl;
        private SignalShape shape;

        [SetUp]
        public void SetUp()
        {
            shape = new SignalShape();
            graphControl = new GraphControl();
            graphControl.AddLibrary(typeof(SignalShape).Module.FullyQualifiedName);
        }

        [Test]
        public void CreateNewSignalShape()
        {
            var signalShape = new SignalShape();
            Assert.IsNotNull(signalShape);
        }

        [Test]
        public void GetThumbnail()
        {
            Assert.That(shape.GetThumbnail(), Is.Null);
        }

        [Test]
        public void InitializedShapeIsRectangle()
        {
            var signalShape = new SignalShape();
            var rectangle = new RectangleF(0, 0, 60, 40);
            Color shapeColor = Color.WhiteSmoke;
            Assert.AreEqual(rectangle, signalShape.Rectangle);
            Assert.AreEqual(shapeColor, signalShape.ShapeColor);
        }
    }
}