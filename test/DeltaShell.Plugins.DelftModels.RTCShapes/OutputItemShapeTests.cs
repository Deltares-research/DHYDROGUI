using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using Netron.GraphLib.UI;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RTCShapes.Tests
{
    [TestFixture]
    public class OutputItemShapeTests
    {
        private GraphControl graphControl;

        [SetUp]
        public void SetUp()
        {
            graphControl = new GraphControl();
            graphControl.AddLibrary(typeof(RuleShape).Module.FullyQualifiedName);
        }

        [Test]
        public void CreateNewOutputItemShape()
        {
            var outputItemShape = new OutputItemShape();
            Assert.IsNotNull(outputItemShape);
        }
    }
}
