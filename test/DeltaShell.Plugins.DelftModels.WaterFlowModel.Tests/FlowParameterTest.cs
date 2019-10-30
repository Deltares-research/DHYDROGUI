using DeltaShell.NGHS.IO.DataObjects;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class FlowParameterTest
    {
        [Test]
        public void Clone()
        {
            FlowParameter flowParameter = new FlowParameter {Value = 5.0};
            var clonedFlowParameter = (FlowParameter)flowParameter.Clone();
            Assert.AreEqual(5,clonedFlowParameter.Value);
        }
    }
}
