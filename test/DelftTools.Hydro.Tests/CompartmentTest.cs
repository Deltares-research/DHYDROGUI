using System.Linq;
using DelftTools.Hydro.Structures;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class CompartmentTest
    {
        [Test]
        public void GivenCompartment_WhenAddingParentManhole_ThenCompartmentIsAlsoContainedInManhole()
        {
            var compartment = new Compartment("myName");
            var manhole = new Manhole("myManhole");
            compartment.ParentManhole = manhole;

            Assert.That(compartment.ParentManhole.Compartments.Count, Is.EqualTo(1));
            Assert.That(manhole.Compartments.FirstOrDefault()?.Name, Is.EqualTo(compartment.Name));
        }

        [Test]
        public void GivenCompartment_WhenAddingParentManholeInConstructor_ThenCompartmentIsAlsoContainedInManhole()
        {
            var manhole = new Manhole("myManhole");
            var compartment = new Compartment("myName") {ParentManhole = manhole};

            Assert.That(compartment.ParentManhole.Compartments.Count, Is.EqualTo(1));
            Assert.That(manhole.Compartments.FirstOrDefault()?.Name, Is.EqualTo(compartment.Name));
        }

        [Test]
        public void GivenCompartment_WhenAssigningTheSameManholeAsParentManholeTwice_ThenTheAmountOfCompartmentDoesNotChange()
        {
            var compartment = new Compartment("myName");
            var manhole = new Manhole("myManhole");
            compartment.ParentManhole = manhole;
            compartment.ParentManhole = manhole;

            Assert.That(compartment.ParentManhole.Compartments.Count, Is.EqualTo(1));
            Assert.That(manhole.Compartments.FirstOrDefault()?.Name, Is.EqualTo(compartment.Name));
        }
    }
}