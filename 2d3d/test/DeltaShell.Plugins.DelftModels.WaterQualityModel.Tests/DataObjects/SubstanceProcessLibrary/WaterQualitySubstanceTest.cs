using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.DataObjects.SubstanceProcessLibrary
{
    [TestFixture]
    public class WaterQualitySubstanceTest
    {
        [Test]
        public void TestClone()
        {
            var waterQualitySubstance = new WaterQualitySubstance
            {
                Id = 2,
                Name = "Name",
                Description = "Description",
                Active = true,
                InitialValue = 1.4,
                ConcentrationUnit = "Concentration unit",
                WasteLoadUnit = "Waste load unit"
            };

            var waterQualitySubstanceClone = waterQualitySubstance.Clone() as WaterQualitySubstance;

            Assert.IsNotNull(waterQualitySubstanceClone);
            Assert.AreNotSame(waterQualitySubstance, waterQualitySubstanceClone);
            Assert.AreEqual(0, waterQualitySubstanceClone.Id);
            Assert.AreEqual(waterQualitySubstance.Name, waterQualitySubstanceClone.Name);
            Assert.AreEqual(waterQualitySubstance.Description, waterQualitySubstanceClone.Description);
            Assert.AreEqual(waterQualitySubstance.Active, waterQualitySubstanceClone.Active);
            Assert.AreEqual(waterQualitySubstance.InitialValue, waterQualitySubstanceClone.InitialValue);
            Assert.AreEqual(waterQualitySubstance.ConcentrationUnit, waterQualitySubstanceClone.ConcentrationUnit);
            Assert.AreEqual(waterQualitySubstance.WasteLoadUnit, waterQualitySubstanceClone.WasteLoadUnit);
        }

        [Test]
        public void TestCompareTo()
        {
            var waterQualitySubstance1 = new WaterQualitySubstance {Name = "a"};
            var waterQualitySubstance2 = new WaterQualitySubstance {Name = "b"};
            var waterQualitySubstance3 = new WaterQualitySubstance {Name = "a"};

            Assert.AreEqual(1, waterQualitySubstance2.CompareTo(waterQualitySubstance1));
            Assert.AreEqual(-1, waterQualitySubstance1.CompareTo(waterQualitySubstance2));
            Assert.AreEqual(0, waterQualitySubstance1.CompareTo(waterQualitySubstance3));
        }
    }
}