using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.DataObjects.SubstanceProcessLibrary
{
    [TestFixture]
    public class WaterQualityOutputParameterTest
    {
        [Test]
        public void TestClone()
        {
            var waterQualityOutputParameter = new WaterQualityOutputParameter
            {
                Id = 2,
                Name = "Name",
                Description = "Description",
                ShowInHis = true,
                ShowInMap = true
            };

            var waterQualityOutputParameterClone = waterQualityOutputParameter.Clone() as WaterQualityOutputParameter;

            Assert.IsNotNull(waterQualityOutputParameterClone);
            Assert.AreNotSame(waterQualityOutputParameter, waterQualityOutputParameterClone);
            Assert.AreEqual(0, waterQualityOutputParameterClone.Id);
            Assert.AreEqual(waterQualityOutputParameter.Name, waterQualityOutputParameterClone.Name);
            Assert.AreEqual(waterQualityOutputParameter.Description, waterQualityOutputParameterClone.Description);
            Assert.AreEqual(waterQualityOutputParameter.ShowInHis, waterQualityOutputParameterClone.ShowInHis);
            Assert.AreEqual(waterQualityOutputParameter.ShowInMap, waterQualityOutputParameterClone.ShowInMap);
        }
    }
}