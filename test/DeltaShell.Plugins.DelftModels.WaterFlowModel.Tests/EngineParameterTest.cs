using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Units;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class EngineParameterTest
    {
        [Test]
        public void Clone()
        {
            var engineParameter = new EngineParameter(QuantityType.WaterLevel,ElementSet.Laterals, DataItemRole.Input,"kaas", new Unit("m"));
            engineParameter.AggregationOptions = AggregationOptions.Maximum;
            
            var clone = engineParameter.Clone();
            ReflectionTestHelper.AssertPublicPropertiesAreEqual(engineParameter,clone);
        }
    }
}