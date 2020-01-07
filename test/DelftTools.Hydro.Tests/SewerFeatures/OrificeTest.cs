using DelftTools.Hydro.SewerFeatures;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.SewerFeatures
{
    [TestFixture]
    public class OrificeTest
    {
        [Test]
        public void GivenOrifice_WhenGettingStructureType_ThenOrificeTypeIsReturned()
        {
            var connectionOrifice = new Orifice("myOrifice");
            Assert.That(connectionOrifice.GetStructureType(), Is.EqualTo(StructureType.Orifice));
        }
    }
}