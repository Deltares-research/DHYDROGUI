using DelftTools.Hydro.Structures;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
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