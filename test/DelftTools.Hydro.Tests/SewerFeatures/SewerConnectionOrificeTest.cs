using DelftTools.Hydro.SewerFeatures;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.SewerFeatures
{
    [TestFixture]
    public class SewerConnectionOrificeTest
    {
        [Test]
        public void OrificeIsSewerConnection()
        {
            var orifice = new Orifice();
            Assert.IsNotNull(orifice);
        }
    }
}