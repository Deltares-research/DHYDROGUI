using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Structures
{
    [TestFixture]
    public class ManholeTest
    {
        private static string compartmentName1 = "myName1";
        private static string CompartmentName2 = "myName2";

        private EventedList<Compartment> GetCompartmentList()
        {
            Compartment compartment1 = new Compartment(compartmentName1);
            Compartment compartment2 = new Compartment(CompartmentName2);
            return new EventedList<Compartment> {compartment1, compartment2};
        }

        [Test]
        public void GivenManholeWithTwoCompartmentsWithAGeometry_WhenGettingTheGeometry_ThenDefaultCoordinateIsReturned()
        {
            var manhole = new Manhole("myManhole")
            {
                Compartments = GetCompartmentList()
            };
            Assert.IsTrue(manhole.Geometry.IsValid);
        }

        [Test]
        public void GivenNewManhole_WhenGettingYCoordinate_ThenXCoordinateIsReturned()
        {
            var manhole = new Manhole("myManhole");
            Assert.IsNotNull(manhole.XCoordinate);
        }

        [Test]
        public void GivenNewManhole_WhenGettingYCoordinate_ThenYCoordinateIsReturned()
        {
            var manhole = new Manhole("myManhole");
            Assert.IsNotNull(manhole.YCoordinate);
        }

        [Test]
        public void GivenManhole_WhenInstatiatingWithCompartment_ThenCompartmentHasManholeAsParentManhole()
        {
            const string manholeName = "myManhole";
            var compartment1 = new Compartment(compartmentName1);
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
            var compartment1 = new Compartment(compartmentName1);
            var manhole = new Manhole(manholeName);
            manhole.Compartments.Add(compartment1);

            Assert.NotNull(manhole.Compartments.FirstOrDefault()?.ParentManhole);
            Assert.That(manhole.Compartments.FirstOrDefault()?.ParentManhole.Name, Is.EqualTo(manholeName));
        }

        [Test]
        public void GivenManholeWithoutCompartments_WhenGettingGeometry_ThenDefaultGeometryIsReturned()
        {
            var manhole = new Manhole("myManhole");
            Assert.IsTrue(manhole.Geometry.IsValid);
            Assert.That(manhole.Geometry, Is.EqualTo(new Point(0, 0)));
        }

        [Test]
        public void GivenManhole_WhenGettingGeometry_ThenDefaultGeometryIsReturned()
        {
            var defaultCoordinate = new Point(0, 0);
            var manhole = new Manhole("myManhole");
            Assert.That(manhole.Geometry, Is.EqualTo(defaultCoordinate));
        }

        [Test]
        public void GivenManholeWithCompartment_WhenGettingCompartmentByName_ThenCompartmentIsReturned()
        {
            var manhole = new Manhole("myManhole")
            {
                Compartments = GetCompartmentList()
            };
            Assert.NotNull(manhole.GetCompartmentByName(compartmentName1));
            Assert.NotNull(manhole.GetCompartmentByName(CompartmentName2));
        }

        [Test]
        public void GivenManholeWithCompartment_WhenGettingCompartmentByNameWithWrongName_ThenCompartmentIsReturned()
        {
            var manhole = new Manhole("myManhole")
            {
                Compartments = GetCompartmentList()
            };
            Assert.IsNull(manhole.GetCompartmentByName("NonExistentCompartmentName"));
        }

        [Test]
        public void GivenManholeWithCompartment_WhenInvokingContainsCompartment_ThenCompartmentIsReturned()
        {
            var manhole = new Manhole("myManhole")
            {
                Compartments = GetCompartmentList()
            };
            Assert.IsTrue(manhole.ContainsCompartmentWithName(compartmentName1));
            Assert.IsTrue(manhole.ContainsCompartmentWithName(CompartmentName2));
        }

        [Test]
        public void GivenManholeWithCompartment_WhenInvokingContainsCompartmentWithWrongName_ThenCompartmentIsReturned()
        {
            var manhole = new Manhole("myManhole")
            {
                Compartments = GetCompartmentList()
            };
            Assert.IsFalse(manhole.ContainsCompartmentWithName("NonExistentCompartmentName"));
        }

        [Test]
        public void GivenManholeWithCompartment_RemovingCompartment_UpdatesListOfCompartments()
        {
            var manhole = new Manhole("myManhole")
            {
                Compartments = GetCompartmentList()
            };

            Assert.IsTrue(manhole.ContainsCompartmentWithName(compartmentName1));

            var compartmentToRemove = manhole.GetCompartmentByName(compartmentName1);
            manhole.Compartments.Remove(compartmentToRemove);

            Assert.IsFalse(manhole.ContainsCompartmentWithName(compartmentName1));
            Assert.AreNotEqual(compartmentToRemove.ParentManhole, manhole);
        }

        [Test] 
        public void GivenManholeWithCompartment_MovingCompartmentToNewManhole_UpdatesListOfCompartments()
        {
            var oldManholeName = "oldManhole";
            var compartmentName = "compartmentName";
            var compartmentToMove = new Compartment(compartmentName);
            var oldManhole = new Manhole(oldManholeName)
            {
                Compartments = new EventedList<Compartment>() { compartmentToMove }
            };

            var newManholeName = "newManhole";
            var newManhole = new Manhole(newManholeName);

            Assert.IsTrue(oldManhole.ContainsCompartmentWithName(compartmentName));
            Assert.IsFalse(newManhole.ContainsCompartmentWithName(compartmentName));
            Assert.AreEqual(compartmentToMove.ParentManhole, oldManhole);

            newManhole.Compartments.Add(compartmentToMove);

            Assert.IsFalse(oldManhole.ContainsCompartmentWithName(compartmentName));
            Assert.IsTrue(newManhole.ContainsCompartmentWithName(compartmentName));
            Assert.AreEqual(compartmentToMove.ParentManhole, newManhole);
        }
    }
}