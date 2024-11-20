using System.Drawing;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using Netron.GraphLib.UI;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.RTCShapes.Tests
{
    [TestFixture]
    public class OutputItemShapeTests
    {
        private GraphControl graphControl;
        private OutputItemShape shape;
        private Graphics graphic;

        [SetUp]
        public void SetUp()
        {
            shape = new OutputItemShape();
            graphic = MockRepository.GenerateMock<Graphics>();
            graphControl = new GraphControl();
            graphControl.AddLibrary(typeof(RuleShape).Module.FullyQualifiedName);
        }

        [Test]
        public void CreateNewOutputItemShape()
        {
            var outputItemShape = new OutputItemShape();
            Assert.IsNotNull(outputItemShape);
        }

        [Test]
        public void GetThumbNail()
        {
            Assert.That(shape.GetThumbnail(), Is.TypeOf(typeof(Bitmap)));
        }

        [Test]
        public void Paint()
        {
            shape.Paint(graphic);
        }
    }
}