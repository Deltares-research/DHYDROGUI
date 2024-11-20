using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.DataObjects.SubstanceProcessLibrary
{
    [TestFixture]
    public class WaterQualityProcessTest
    {
        [Test]
        public void TestClone()
        {
            var waterQualityProcess = new WaterQualityProcess
            {
                Id = 2,
                Name = "Name",
                Description = "Description"
            };

            var waterQualityProcessClone = waterQualityProcess.Clone() as WaterQualityProcess;

            Assert.IsNotNull(waterQualityProcessClone);
            Assert.AreNotSame(waterQualityProcess, waterQualityProcessClone);
            Assert.AreEqual(0, waterQualityProcessClone.Id);
            Assert.AreEqual(waterQualityProcess.Name, waterQualityProcessClone.Name);
            Assert.AreEqual(waterQualityProcess.Description, waterQualityProcessClone.Description);
        }
    }
}