using System;
using System.Linq;
using DelftTools.Hydro.SewerFeatures;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.SewerFeatures
{
    [TestFixture]
    public class CompartmentTest
    {
        [Test]
        public void GivenCompartment_DefaultShapeTypeIsEnumUnknown()
        {
            var compartment = new Compartment("myName");

            Assert.IsNotNull(compartment.Shape);
            Assert.AreEqual(default(CompartmentShape), compartment.Shape);
            Assert.AreEqual(CompartmentShape.Unknown, compartment.Shape);
        }

        [Test]
        [TestCase(CompartmentShape.Unknown)]
        [TestCase(CompartmentShape.Square)]
        [TestCase(CompartmentShape.Rectangular)]
        public void GivenCompartment_DefaultEnumTypeIsUnknown(CompartmentShape shapeType)
        {
            var compartment = new Compartment("myName"){ Shape = shapeType};

            Assert.IsNotNull(compartment.Shape);
            Assert.AreEqual(shapeType, compartment.Shape);
        }

        [Test]
        public void CheckCompartmentShapeTypes()
        {
            /*
             * Test made to keep track of how many compartment shapes do we have
             * If the types change the test above will fail. This is only for the NUMBER
             * of shape types.
             */
            var shapeTypeCount = Enum.GetNames(typeof(CompartmentShape)).Length;
            Assert.AreEqual(3, shapeTypeCount);
        }

        [Test]
        public void GivenCompartment_ParentManholeCanBeSet()
        {
            var compartment = new Compartment("myName");
            var manhole = new Manhole("myManhole");
            compartment.ParentManhole = manhole;

            //If we really want to add a compartment to the manhole we have to do it through the manhole,
            // not through the compartment. However with this test we also ensure the set property.
            Assert.IsFalse(manhole.Compartments.Any());
            Assert.AreEqual(manhole, compartment.ParentManhole);
        }
    }
}