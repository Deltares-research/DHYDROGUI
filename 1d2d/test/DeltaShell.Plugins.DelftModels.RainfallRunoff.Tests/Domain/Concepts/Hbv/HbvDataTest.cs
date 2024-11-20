using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Concepts.Hbv
{
    [TestFixture]
    public class HbvDataTest
    {
        [Test]
        public void Constructor_SetsCatchmentModelDataOnCatchment()
        {
            // Setup
            var catchment = new Catchment();

            // Call
            var data = new HbvData(catchment);

            // Assert
            Assert.That(catchment.ModelData, Is.SameAs(data));
        }
        
        [Test]
        public void CloneHbvData()
        {
            var hbvData = new HbvData(new Catchment {Name = "catchment"});
            ReflectionTestHelper.FillRandomValuesForValueTypeProperties(hbvData);

            var hbvDataClone = hbvData.Clone() as HbvData;

            Assert.IsNotNull(hbvDataClone);
            ReflectionTestHelper.AssertPublicPropertiesAreEqual(hbvData, hbvDataClone);
            
        }
    }
}
