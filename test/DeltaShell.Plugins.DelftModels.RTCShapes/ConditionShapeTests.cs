using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using Netron.GraphLib.UI;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RTCShapes.Tests
{
    [TestFixture]
    public class ConditionShapeTests
    {
        private GraphControl graphControl;

        [SetUp]
        public void SetUp()
        {
            graphControl = new GraphControl();
            graphControl.AddLibrary(typeof(RuleShape).Module.FullyQualifiedName);
        }

        [Test]
        public void CreateNewConditionShape()
        {
            var conditionShape = new ConditionShape();
            Assert.IsNotNull(conditionShape);
        }
    }
}
