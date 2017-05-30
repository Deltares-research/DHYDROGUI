using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.FeatureData
{
    [TestFixture]
    public class FlowBoundaryConditionFactoryTest
    {
        [TestCase("WaterLevel", "Water level", true, FlowBoundaryQuantityType.WaterLevel)]
        [TestCase("Riemann", "Riemann invariant", true, FlowBoundaryQuantityType.Riemann)]
        [TestCase("MorphologyBedLevelPrescribed", "Bed level prescribed", true, FlowBoundaryQuantityType.MorphologyBedLevelPrescribed)]
        [TestCase("Tracer1", "Tracer", false, FlowBoundaryQuantityType.Tracer)]
        [TestCase("Fraction1", "Sediment concentration", false, FlowBoundaryQuantityType.SedimentConcentration)]
        public void TestTryParseRegularFlowBoundaryQuantityType(string boundaryConditionName, string quantityType, bool expectedResult, FlowBoundaryQuantityType expectedFlowBoundaryQuantityType)
        {
            FlowBoundaryQuantityType flowBoundaryQuantityType;
            var result = FlowBoundaryConditionFactory.TryParseRegularFlowBoundaryQuantityType(boundaryConditionName, quantityType, out flowBoundaryQuantityType);

            Assert.AreEqual(expectedResult, result);

            if (result) Assert.AreEqual(expectedFlowBoundaryQuantityType, flowBoundaryQuantityType);
        }
    }
}
