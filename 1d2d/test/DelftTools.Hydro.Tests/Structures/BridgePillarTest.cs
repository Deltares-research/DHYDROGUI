using DelftTools.Hydro.Structures;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Structures
{
    [TestFixture]
    public class BridgePillarTest
    {
        [Test]
        public void Test_Structures_BridgePillar_Constructor()
        {
            //Instantiate BridgePillar
            var bridgePillar = new BridgePillar();
            Assert.IsNotNull(bridgePillar);
        }
    }
}
