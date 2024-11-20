using System.Drawing;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using Netron.GraphLib.UI;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RTCShapes.Tests
{
    [TestFixture]
    public class InputItemShapeTests
    {
        private GraphControl graphControl;
        private InputItemShape shape;

        [SetUp]
        public void SetUp()
        {
            shape = new InputItemShape();
            graphControl = new GraphControl();
            graphControl.AddLibrary(typeof(RuleShape).Module.FullyQualifiedName);
        }

        [Test]
        public void CreateNewInputItemShape()
        {
            var inputItemShape = new InputItemShape();
            Assert.IsNotNull(inputItemShape);
            Assert.That(shape.GradientStartColor, Is.EqualTo(Color.LemonChiffon));
            Assert.That(shape.GradientEndColor, Is.EqualTo(Color.White));
        }

        [Test]
        public void GetThumbNail()
        {
            Assert.That(shape.GetThumbnail(), Is.TypeOf(typeof(Bitmap)));
        }
    }
}