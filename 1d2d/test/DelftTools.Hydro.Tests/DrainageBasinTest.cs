using NUnit.Framework;
using SharpTestsEx;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class DrainageBasinTest
    {
        [Test]
        public void LinkCatchmentsOfTheSameBasin()
        {
            var c1 = new Catchment();
            var c2 = new Catchment();

            var b1 = new DrainageBasin { Catchments = { c1, c2 } };

            c1.LinkTo(c2);

            b1.Links.Count
                .Should().Be.EqualTo(1);
        }
    }
}