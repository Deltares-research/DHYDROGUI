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

        private readonly Point coordinate13 = new Point(1, 3);
        private readonly Point coordinate24 = new Point(2, 4);
        private readonly Point coordinate35 = new Point(3, 5);
        private readonly Point averageInitialCoordinate = new Point(1.5, 3.5);

        private EventedList<Compartment> GetCompartmentList()
        {
            Compartment compartment1 = new Compartment(compartmentName1);
            Compartment compartment2 = new Compartment(CompartmentName2);
            return new EventedList<Compartment> {compartment1, compartment2};
        }

        [Test]
        public void GivenManholeWithTwoCompartmentsWithAGeometry_WhenGettingTheGeometry_ThenTheAverageCoordinateIsReturned()
        {
            var manhole = new Manhole("myManhole")
            {
                Compartments = GetCompartmentList()
            };
            Assert.That(manhole.Geometry, Is.EqualTo(averageInitialCoordinate));
        }

        [Test]
        public void GivenManholeWithTwoCompartmentsWithAGeometry_WhenGettingXCoordinate_ThenTheAverageXCoordinateIsReturned()
        {
            var manhole = new Manhole("myManhole")
            {
                Compartments = GetCompartmentList()
            };
            Assert.That(manhole.XCoordinate, Is.EqualTo(1.5));
        }

        [Test]
        public void GivenManholeWithTwoCompartmentsWithAGeometry_WhenGettingYCoordinate_ThenTheAverageYCoordinateIsReturned()
        {
            var manhole = new Manhole("myManhole")
            {
                Compartments = GetCompartmentList()
            };
            Assert.That(manhole.YCoordinate, Is.EqualTo(3.5));
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

            Assert.That(manhole.Geometry, Is.EqualTo(new Point(1.0, 3.0)));
            Assert.NotNull(manhole.Compartments.FirstOrDefault()?.ParentManhole);
            Assert.That(manhole.Compartments.FirstOrDefault()?.ParentManhole.Name, Is.EqualTo(manholeName));
        }

        [Test]
        public void GivenManholeWithTwoCompartments_WhenRemovingCompartment_ThenGeometryIsCorrectlyUpdated()
        {
            var manhole = new Manhole("myManhole")
            {
                Compartments = GetCompartmentList()
            };
            Assert.That(manhole.Geometry, Is.EqualTo(averageInitialCoordinate));

            manhole.Compartments.RemoveAt(0);
            Assert.That(manhole.Geometry, Is.EqualTo(coordinate24));
        }

        [Test]
        public void GivenManholeWithoutCompartments_WhenGettingGeometry_ThenDefaultGeometryIsReturned()
        {
            var manhole = new Manhole("myManhole");
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
        public void GivenManhole_WhenAddingCompartmentWithSameName_ThenReplaceOldCompartmentWithNewCompartment()
        {
            var oldBottomLevel = 10;
            var oldCompartment = new Compartment("myCompartment"){BottomLevel = oldBottomLevel};
            var newBottomLevel = 23;
            var newCompartment = new Compartment("myCompartment"){BottomLevel = newBottomLevel};
            var manhole = new Manhole("myManhole")
            {
                Compartments = new EventedList<Compartment> { oldCompartment }
            };
            Assert.That(manhole.Compartments.Count, Is.EqualTo(1));

            Assert.AreEqual(oldCompartment, manhole.Compartments.FirstOrDefault());
            Assert.AreEqual(oldBottomLevel, manhole.Compartments.FirstOrDefault()?.BottomLevel);

            manhole.Compartments.Add(newCompartment);
            Assert.That(manhole.Compartments.Count, Is.EqualTo(1));
            Assert.AreEqual(newCompartment, manhole.Compartments.FirstOrDefault());
            Assert.AreEqual(newBottomLevel, manhole.Compartments.FirstOrDefault()?.BottomLevel);
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
            Assert.IsTrue(manhole.ContainsCompartment(compartmentName1));
            Assert.IsTrue(manhole.ContainsCompartment(CompartmentName2));
        }

        [Test]
        public void GivenManholeWithCompartment_WhenInvokingContainsCompartmentWithWrongName_ThenCompartmentIsReturned()
        {
            var manhole = new Manhole("myManhole")
            {
                Compartments = GetCompartmentList()
            };
            Assert.IsFalse(manhole.ContainsCompartment("NonExistentCompartmentName"));
        }
    }
}