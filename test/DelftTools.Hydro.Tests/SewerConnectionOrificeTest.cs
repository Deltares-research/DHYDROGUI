using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class SewerConnectionOrificeTest
    {
        [Test]
        public void OrificeIsSewerConnection()
        {
            var orifice = new SewerConnectionOrifice();
            Assert.IsNotNull(orifice);
            Assert.IsTrue(orifice is SewerConnection);
            Assert.IsFalse(orifice.IsPipe());
            Assert.IsTrue(orifice.IsOrifice());
        }
    }
}