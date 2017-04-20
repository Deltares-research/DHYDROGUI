using System;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using NetTopologySuite.Extensions.Networks;
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

                var conveyanceTable = calculator.GetConveyance(flowModel.Network.CrossSections.ToArray()[0]);

                Assert.Greater(conveyanceTable.GetValues().Count,0);
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Conveyance calculation is only available for YZ and geometry based cross sections")]
        public void GetConveyanceTableFromNonYZCrossSectionShouldThrow()
        {
            var calculator = new WaterFlowModel1DConveyanceCalculator(new WaterFlowModel1D());
            calculator.GetConveyance(CrossSection.CreateDefault(CrossSectionType.ZW, new Branch()));
        }
    }
}
