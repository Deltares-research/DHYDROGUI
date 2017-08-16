using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    public class Iterative1D2DCouplerLinkTest
    {
        Iterative1D2DCouplerLink coupler1 = new Iterative1D2DCouplerLink
        {
            Name = "MyFirstCoupler"
        };

        [Test]
        public void GivenIterative1D2DCouplerLinkObjectWhenToStringMethodUsedThenReturnName()
        {
            Assert.That(coupler1.ToString(), Is.EqualTo(coupler1.Name));
        }

        [Test]
        public void GivenTwoIterative1D2DCouplerLinkObjectsWithDifferentNamesWhenComparedThenReturnFalse()
        {
            var coupler2 = new Iterative1D2DCouplerLink
            {
                Name = "MySecondCoupler"
            };

            Assert.That(coupler1.CompareTo(coupler2), Is.EqualTo(-1));
        }

        [Test]
        public void GivenTwoIterative1D2DCouplerLinkObjectsWithEqualNamesWhenComparedThenReturnTrue()
        {
            var coupler2 = new Iterative1D2DCouplerLink
            {
                Name = "MyFirstCoupler"
            };

            Assert.That(coupler1.CompareTo(coupler2), Is.EqualTo(0));
        }
    }
}