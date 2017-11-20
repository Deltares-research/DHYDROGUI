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
        Compartment compartment1 = new Compartment("myName1")
        {
            Geometry = new Point(1, 3)
        };

        Compartment compartment2 = new Compartment("myName2")
        {
            Geometry = new Point(2, 4)
        };

        [Test]
        public void GivenManholeWithTwoCompartmentsWithAGeometry_WhenGettingTheGeometry_ThenTheAverageCoordinateIsReturned()
        {
            var manhole = new Manhole("myManhole")
            {
                Compartments = new EventedList<Compartment> { compartment1, compartment2 }
            };
            Assert.That(manhole.Geometry, Is.EqualTo(new Point(1.5, 3.5)));
        }

        [Test]
        public void GivenManholeWithTwoCompartmentsWithAGeometry_WhenGettingXCoordinate_ThenTheAverageXCoordinateIsReturned()
        {
            var manhole = new Manhole("myManhole")
            {
                Compartments = new EventedList<Compartment> { compartment1, compartment2 }
            };
            Assert.That(manhole.XCoordinate, Is.EqualTo(1.5));
        }

        [Test]
        public void GivenManholeWithTwoCompartmentsWithAGeometry_WhenGettingYCoordinate_ThenTheAverageYCoordinateIsReturned()
        {
            var manhole = new Manhole("myManhole")
            {
                Compartments = new EventedList<Compartment> { compartment1, compartment2 }
            };
            Assert.That(manhole.YCoordinate, Is.EqualTo(3.5));
        }

        [Test]
        public void GivenManhole_WhenInstatiatingWithCompartment_ThenCompartmentHasManholeAsParentManhole()
        {
            const string manholeName = "myManhole";
            var manhole = new Manhole(manholeName)
            {
                Compartments = new EventedList<Compartment> { compartment1 }
            };

            Assert.NotNull(manhole.Compartments.FirstOrDefault()?.ParentManhole);
            Assert.That(manhole.Compartments.FirstOrDefault()?.ParentManhole.Name, Is.EqualTo(manholeName));
        }

        [Test]
        public void GivenManhole_WhenAddingCompartment_ThenCompartmentHasManholeAsParentManhole()
        {
            const string manholeName = "myManhole";
            var manhole = new Manhole(manholeName);
            manhole.Compartments.Add(compartment1);

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

        [Test]
        public void GivenManholeWithCompartment_WhenGettingCompartmentByName_ThenCompartmentIsReturned()
        {
            var manhole = new Manhole("myManhole")
            {
                Compartments = new EventedList<Compartment> { compartment1, compartment2 }
            };
            Assert.NotNull(manhole.GetCompartmentByName(compartment1.Name));
            Assert.NotNull(manhole.GetCompartmentByName(compartment2.Name));
        }

        [Test]
        public void GivenManholeWithCompartment_WhenGettingCompartmentByNameWithWrongName_ThenCompartmentIsReturned()
        {
            var manhole = new Manhole("myManhole")
            {
                Compartments = new EventedList<Compartment> { compartment1, compartment2 }
            };
            Assert.IsNull(manhole.GetCompartmentByName("NonExistentCompartmentName"));
        }

        [Test]
        public void GivenManholeWithCompartment_WhenInvokingContainsCompartment_ThenCompartmentIsReturned()
        {
            var manhole = new Manhole("myManhole")
            {
                Compartments = new EventedList<Compartment> { compartment1, compartment2 }
            };
            Assert.IsTrue(manhole.ContainsCompartment(compartment1.Name));
            Assert.IsTrue(manhole.ContainsCompartment(compartment2.Name));
        }

        [Test]
        public void GivenManholeWithCompartment_WhenInvokingContainsCompartmentWithWrongName_ThenCompartmentIsReturned()
        {
            var manhole = new Manhole("myManhole")
            {
                Compartments = new EventedList<Compartment> { compartment1, compartment2 }
            };
            Assert.IsFalse(manhole.ContainsCompartment("NonExistentCompartmentName"));
        }
    }
}