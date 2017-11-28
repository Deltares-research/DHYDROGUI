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
            Compartment compartment1 = new Compartment(compartmentName1)
            {
                Geometry = coordinate13
            };

            Compartment compartment2 = new Compartment(CompartmentName2)
            {
                Geometry = coordinate24
            };
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
            var compartment1 = new Compartment(compartmentName1)
            {
                Geometry = coordinate13
            };
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
            var compartment1 = new Compartment(compartmentName1)
            {
                Geometry = coordinate13
            };
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
        public void GivenManholeWithOneCompartmentThatHasNoGeometryDefined_WhenGettingGeometry_ThenDefaultGeometryIsReturned()
        {
            var defaultCoordinate = new Point(0, 0);
            var manhole = new Manhole("myManhole");
            manhole.Compartments.Add(new Compartment("myCompartment"));
            Assert.That(manhole.Geometry, Is.EqualTo(defaultCoordinate));

            var compartment = manhole.Compartments.FirstOrDefault();
            Assert.NotNull(compartment);
            Assert.That(compartment.Geometry, Is.EqualTo(defaultCoordinate));
        }

        [Test]
        public void GivenManhole_WhenAddingCompartmentWithoutGeometryDefined_ThenCompartmentGeometryIsEqualToManholeGeometry()
        {
            var coords = new Point(3, 4);
            var manhole = new Manhole("myManhole")
            {
                Geometry = coords
            };
            manhole.Compartments.Add(new Compartment("myCompartment"));
            Assert.That(manhole.Geometry, Is.EqualTo(coords));

            var compartment = manhole.Compartments.FirstOrDefault();
            Assert.NotNull(compartment);
            Assert.That(compartment.Geometry, Is.EqualTo(coords));
        }

        [Test]
        public void GivenManholeWithCompartments_WhenChangingManholeGeometry_ThenCompartmentGeometriesAreTransitionedCorrectly()
        {
            var manhole = new Manhole("myManhole")
            {
                Compartments = GetCompartmentList()
            };
            Assert.That(manhole.Geometry, Is.EqualTo(averageInitialCoordinate));
            manhole.Geometry = new Point(11.5, 8.5);

            var translatedComp1 = manhole.Compartments.FirstOrDefault(c => c.Name == compartmentName1);
            var translatedComp2 = manhole.Compartments.FirstOrDefault(c => c.Name == CompartmentName2);
            Assert.NotNull(translatedComp1);
            Assert.NotNull(translatedComp2);
            Assert.That(translatedComp1.Geometry, Is.EqualTo(new Point(11, 8)));
            Assert.That(translatedComp2.Geometry, Is.EqualTo(new Point(12, 9)));
        }

        [Test]
        public void GivenManholeWithCompartments_WhenChangingCompartmentGeometry_ThenManholeGeometryIsUpdated()
        {
            var manhole = new Manhole("myManhole")
            {
                Compartments = GetCompartmentList()
            };
            Assert.That(manhole.Compartments[0].Geometry, Is.EqualTo(coordinate13));
            Assert.That(manhole.Compartments[1].Geometry, Is.EqualTo(coordinate24));
            Assert.That(manhole.Geometry, Is.EqualTo(averageInitialCoordinate));
            
            manhole.Compartments[0].Geometry = coordinate35;
            Assert.That(manhole.Compartments[0].Geometry, Is.EqualTo(coordinate35));
            Assert.That(manhole.Compartments[1].Geometry, Is.EqualTo(coordinate24));
            Assert.That(manhole.Geometry, Is.EqualTo(new Point(2.5, 4.5)));
        }

        [Test]
        public void GivenManhole_WhenReplacingCompartment_ThenUpdatingManholeGeometryStillWorksCorrectly()
        {
            var initialCoordinate = new Point(10, 10);
            var manhole = new Manhole("myManhole")
            {
                Compartments = GetCompartmentList()
            };
            manhole.Compartments[0] = new Compartment("myReplacedCompartment")
            {
                Geometry = initialCoordinate
            };

            // Check that replacement has updated the manhole geometry correctly
            Assert.That(manhole.Compartments[0].Geometry, Is.EqualTo(initialCoordinate));
            Assert.That(manhole.Compartments[1].Geometry, Is.EqualTo(coordinate24));
            Assert.That(manhole.Geometry, Is.EqualTo(new Point(6.0, 7.0)));

            // Check that changing the geometry of the new compartment updates the manhole geometry correctly
            manhole.Compartments[0].Geometry = coordinate35;
            Assert.That(manhole.Compartments[0].Geometry, Is.EqualTo(coordinate35));
            Assert.That(manhole.Compartments[1].Geometry, Is.EqualTo(coordinate24));
            Assert.That(manhole.Geometry, Is.EqualTo(new Point(2.5, 4.5)));

            // Check that changing the geometry of the other compartment updates the manhole geometry correctly
            var newCoordinateCompartment2 = new Point(6, 6);
            manhole.Compartments[1].Geometry = newCoordinateCompartment2;
            Assert.That(manhole.Compartments[0].Geometry, Is.EqualTo(coordinate35));
            Assert.That(manhole.Compartments[1].Geometry, Is.EqualTo(newCoordinateCompartment2));
            Assert.That(manhole.Geometry, Is.EqualTo(new Point(4.5, 5.5)));
        }

        [Test]
        public void GivenManhole_WhenAddingCompartmentWithSameName_ThenReplaceOldCompartmentWithNewCompartment()
        {
            var oldGeometry = coordinate13;
            var newGeometry = coordinate24;
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