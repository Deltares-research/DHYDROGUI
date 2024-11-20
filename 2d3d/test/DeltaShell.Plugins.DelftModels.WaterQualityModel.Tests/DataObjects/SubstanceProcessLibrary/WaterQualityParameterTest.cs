using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.DataObjects.SubstanceProcessLibrary
{
    [TestFixture]
    public class WaterQualityParameterTest
    {
        [Test]
        public void TestClone()
        {
            var waterQualityParameter = new WaterQualityParameter
            {
                Id = 2,
                Name = "Name",
                Description = "Description",
                Unit = "Unit",
                DefaultValue = 1.2
            };

            var waterQualityParameterClone = waterQualityParameter.Clone() as WaterQualityParameter;

            Assert.IsNotNull(waterQualityParameterClone);
            Assert.AreNotSame(waterQualityParameter, waterQualityParameterClone);
            Assert.AreEqual(0, waterQualityParameterClone.Id);
            Assert.AreEqual(waterQualityParameter.Name, waterQualityParameterClone.Name);
            Assert.AreEqual(waterQualityParameter.Description, waterQualityParameterClone.Description);
            Assert.AreEqual(waterQualityParameter.Unit, waterQualityParameterClone.Unit);
            Assert.AreEqual(waterQualityParameter.DefaultValue, waterQualityParameterClone.DefaultValue);
        }
    }
}