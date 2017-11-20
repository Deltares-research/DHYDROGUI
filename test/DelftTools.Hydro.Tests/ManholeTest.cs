using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class ManholeTest
    {
        [Test]
        public void GivenManholeWithTwoCompartmentsWithAGeometry_WhenGettingTheGeometry_ThenTheAverageCoordinateIsReturned()
        {
            var compartment1 = new Compartment("myName1")
            {
                Geometry = new Point(1, 3)
            };

            var compartment2 = new Compartment("myName2")
            {
                Geometry = new Point(2, 4)
            };
            
            var manhole = new Manhole("myManhole")
            {
                Compartments = new EventedList<Compartment> { compartment1, compartment2 }
            };
            Assert.That(manhole.Geometry, Is.EqualTo(new Point(1.5, 3.5)));
        }

        [Test]
        public void GivenManhole_WhenInstatiatingWithCompartment_ThenCompartmentHasManholeAsParentManhole()
        {
            const string manholeName = "myManhole";
            var compartment = new Compartment("myName")
            {
                Geometry = new Point(1, 3)
            };

            var manhole = new Manhole(manholeName)
            {
                Compartments = new EventedList<Compartment> { compartment }
            };

            Assert.NotNull(manhole.Compartments.FirstOrDefault()?.ParentManhole);
            Assert.That(manhole.Compartments.FirstOrDefault()?.ParentManhole.Name, Is.EqualTo(manholeName));
        }

        [Test]
        public void GivenManhole_WhenAddingCompartment_ThenCompartmentHasManholeAsParentManhole()
        {
            const string manholeName = "myManhole";
            var compartment = new Compartment("myName")
            {
                Geometry = new Point(1, 3)
            };

            var manhole = new Manhole(manholeName);
            manhole.Compartments.Add(compartment);

            Assert.NotNull(manhole.Compartments.FirstOrDefault()?.ParentManhole);
            Assert.That(manhole.Compartments.FirstOrDefault()?.ParentManhole.Name, Is.EqualTo(manholeName));
        }

        [Test]
        public void GivenManhole_WhenAddingCompartmentWithSameName_ThenReplaceOldCompartmentWithNewCompartment()
        {
            var oldGeometry = new Point(1, 3);
            var newGeometry = new Point(2, 4);
            var oldCompartment = new Compartment("myCompartment")
            {
                Geometry = oldGeometry
            };

            var newCompartment = new Compartment("myCompartment")
            {
                Geometry = newGeometry
            };
            var manhole = new Manhole("myManhole")
            {
                Compartments = new EventedList<Compartment> { oldCompartment }
            };
            Assert.That(manhole.Compartments.Count, Is.EqualTo(1));
            Assert.That(manhole.Compartments.FirstOrDefault()?.Name, Is.EqualTo("myCompartment"));
            Assert.That(manhole.Compartments.FirstOrDefault()?.Geometry, Is.EqualTo(oldGeometry));

            manhole.Compartments.Add(newCompartment);
            Assert.That(manhole.Compartments.Count, Is.EqualTo(1));
            Assert.That(manhole.Compartments.FirstOrDefault()?.Name, Is.EqualTo("myCompartment"));
            Assert.That(manhole.Compartments.FirstOrDefault()?.Geometry, Is.EqualTo(newGeometry));
        }
    }
}