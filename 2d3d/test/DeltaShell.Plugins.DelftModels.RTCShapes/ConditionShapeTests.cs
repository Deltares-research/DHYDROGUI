using System.Drawing;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using Netron.GraphLib;
using Netron.GraphLib.UI;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.RTCShapes.Tests
{
    [TestFixture]
    public class ConditionShapeTests : ConditionShape
    {
        private GraphControl graphControl;
        private ConditionShape shape;
        private Graphics graphic;
        private ConditionBase condition;

        [SetUp]
        public void SetUp()
        {
            condition = new DirectionalCondition();
            graphic = MockRepository.GenerateMock<Graphics>();
            //  shape = MockRepository.GeneratePartialMock<ConditionShape>();
            shape = new ConditionShape();
            graphControl = new GraphControl();
            graphControl.AddLibrary(typeof(RuleShape).Module.FullyQualifiedName);
        }

        [Test]
        public void CreateNewConditionShape()
        {
            var conditionShape = new ConditionShape();
            Assert.IsNotNull(conditionShape);
        }

        [Test]
        public void GivenConditionShapeWhenGettingThumbNailThenVerifyIfTypeIsOfBitMap()
        {
            Assert.That(shape.GetThumbnail(), Is.TypeOf(typeof(Bitmap)));
        }

        [Test]
        public void GivenConditionShapeWhenDisablingInputConnectionsThenNewConnectionsFromLeftAndTopNodeAreNotAllowed()
        {
            shape.DisableInputConnections();
            Connector connector = shape.Connectors[0];
            Assert.That(connector.AllowNewConnectionsTo, Is.EqualTo(false));
        }

        [Test]
        public void GivenConditionShapeWhenEnablingInputConnectionsThenNewConnectionsFromLeftAndTopNodeAreAllowed()
        {
            shape.EnableInputConnections();
            Connector connector = shape.Connectors[0];
            Assert.That(connector.AllowNewConnectionsTo, Is.EqualTo(true));
        }

        [Test]
        public void GivenResizableConditionShapeWhenDrawingTheShapeThenShapeGetsRecalculated()
        {
            shape.AutoResize = true;
            shape.Paint(graphic);
        }

        [Test]
        public void GivenNonResizableConditionShapeWhenDrawingTheShapeThenShapeIsNotRecalculated()
        {
            shape.AutoResize = false;
            shape.Paint(graphic);
        }

        [Test]
        public void Image()
        {
            shape.Image = new Bitmap(2, 2);
            shape.Paint(graphic);
        }

        [Test]
        public void Description()
        {
            shape.GetDescriptionDelegate = condition.GetDescription;
            shape.Paint(graphic);
        }
    }
}