using System.Linq;
using DelftTools.Hydro.CrossSections;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class WaterFlowModel1DConveyanceCalculatorTest
    {
        [Test]
        public void GetConveyanceTableFromBasicYZCrossSection()
        {
            using (var flowModel = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {
                var calculator = new WaterFlowModel1DConveyanceCalculator(flowModel);

                Assert.IsTrue(flowModel.Network.CrossSections.Any());
                Assert.AreEqual(CrossSectionType.GeometryBased,flowModel.Network.CrossSections.ToArray()[0].CrossSectionType);

                // TODO: This feature needs to be re-implemented, for now Default conveyance is returned
                Assert.DoesNotThrow(() => calculator.GetConveyance(flowModel.Network.CrossSections.ToArray()[0]));
            }
        }
    }
}
