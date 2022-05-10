using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class CatchmentTest
    {
        [Test]
        public void DefaultCatchment()
        {
            var catchment = new Catchment();
            Assert.IsNotNull(catchment);
            Assert.IsTrue(catchment.IsGeometryDerivedFromAreaSize, "Catchment should have IsGeometryDerivedFromAreaSize set to true by default");
        }

        [Test]
        public void CloneWithNoGeometry()
        {
            var catchment = new Catchment();
            var clone = (Catchment)catchment.Clone();

            Assert.IsNotNull(clone);
        }
        
        [Test]
        public void DefaultGeometryForArea()
        {
            var catchment = new Catchment { IsGeometryDerivedFromAreaSize = true };
            var expected = 500;
            catchment.SetAreaSize(expected);

            Assert.AreEqual(expected, catchment.Geometry.Area, 0.01);
            Assert.AreEqual(expected, catchment.GeometryArea, 0.01);
        }

        [Test]
        public void GivenCatchment_SettingPointGeometry_ShouldResultInDefaultPolygon()
        {
            //Arrange
            var catchment = new Catchment { IsGeometryDerivedFromAreaSize = true };
            var expected = 500;
            catchment.SetAreaSize(expected);

            // Act
            catchment.Geometry = new Point(0, 0);

            // Assert
            Assert.AreEqual(expected, catchment.GeometryArea, 0.01);
            Assert.IsInstanceOf<IPolygon>(catchment.Geometry);
        }

        [Test]
        public void GivenCatchment_ChangingComputingArea_ShouldNotChangeGeometryIfUserSet()
        {
            //Arrange
            var catchment = new Catchment
            {
                Geometry = new Polygon(new LinearRing(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(5, 5),
                    new Coordinate(10, 0),
                    new Coordinate(0, 0)
                }))
            };

            var expected = 500;
            Assert.IsFalse(catchment.IsGeometryDerivedFromAreaSize, "If user sets geometry it should not be set as derived");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => catchment.SetAreaSize(expected));
            Assert.AreNotEqual(expected, catchment.GeometryArea);
        }

        [Test]
        public void DefaultGeometryForLargeArea()
        {
            var catchment = new Catchment {IsGeometryDerivedFromAreaSize = true};
            var expected = 9000;
            catchment.SetAreaSize(expected);

            Assert.AreEqual(expected, catchment.Geometry.Area, 0.01);
            Assert.AreEqual(expected, catchment.GeometryArea, 0.01);
        }

        [Test]
        public void DefaultGeometryForEmptyArea()
        {
            var catchment = new Catchment { IsGeometryDerivedFromAreaSize = true };
            var expected = 0;
            catchment.SetAreaSize(expected);

            Assert.AreEqual(expected, catchment.Geometry.Area, 1.0);
            Assert.AreEqual(expected, catchment.GeometryArea);
        }

        [Test]
        public void Clone()
        {
            var catchment = new Catchment();
            catchment.Geometry = new Point(15d, 15d);
            var clone = (Catchment)catchment.Clone();

            Assert.AreEqual(catchment.Geometry, clone.Geometry);
            Assert.AreNotSame(catchment.Geometry, clone.Geometry);
            Assert.AreEqual(catchment.Name, clone.Name);
        }

        [Test]
        public void CopyFrom()
        {
            var catchment1 = new Catchment();
            var catchment2 = new Catchment
                                 {
                                     Name = "Aapje",
                                     Geometry =
                                         new Polygon(
                                         new LinearRing(new[]
                                                            {
                                                                new Coordinate(10d, 10d), new Coordinate(20d, 10d),
                                                                new Coordinate(15d, 15d), new Coordinate(10d, 10d)
                                                            })),
                                     Description = "Komt uit de mouw",
                                     Basin = new DrainageBasin()
                                 };
            catchment2.Attributes.Add("gras", 15);

            catchment1.CopyFrom(catchment2);

            Assert.AreNotEqual(catchment1.Geometry, catchment2.Geometry);
            Assert.AreNotEqual(catchment1.Name, catchment2.Name);
            Assert.AreEqual(catchment1.Attributes, catchment2.Attributes);
            Assert.AreEqual(catchment1.Description, catchment2.Description);
            Assert.AreSame(catchment1.Basin, catchment2.Basin);
            Assert.AreNotSame(catchment1.Attributes, catchment2.Attributes);
        }

        [Test]
        public void SettingGeometryShouldChangeInteriorPointAccordingly()
        {
            var catchment = new Catchment
                {
                    Geometry = new Polygon(new LinearRing(new[]
                        {
                            new Coordinate(0,0), 
                            new Coordinate(10,0), 
                            new Coordinate(10,10), 
                            new Coordinate(0,10), 
                            new Coordinate(0,0)
                        }))
                };

            Assert.AreEqual(5, catchment.InteriorPoint.X);
            Assert.AreEqual(5, catchment.InteriorPoint.Y);

            catchment.Geometry = new Polygon(new LinearRing(new []
                {
                    new Coordinate(0, 0),
                    new Coordinate(20, 0),
                    new Coordinate(20, 20),
                    new Coordinate(0, 20),
                    new Coordinate(0, 0)
                }));
            
            Assert.AreEqual(10, catchment.InteriorPoint.X);
            Assert.AreEqual(10, catchment.InteriorPoint.Y);
        }

        [Test]
        public void PavedCatchmentWithLinkToWasteWaterTreatmentPlant_CannotBeLinkedToAnotherWasteWaterTreatmentPlant()
        {
            var catchment = new Catchment()
            {
                CatchmentType = CatchmentType.Paved
            };

            var treatmentPlant1 = new WasteWaterTreatmentPlant();
            var link = new HydroLink(catchment, treatmentPlant1);
            catchment.Links.Add(link);

            var treatmentPlant2 = new WasteWaterTreatmentPlant();

            // Call
            bool canLinkTo = catchment.CanLinkTo(treatmentPlant2);

            // Assert
            Assert.That(canLinkTo, Is.False);
        }
        
        [Test]
        [TestCaseSource(nameof(CanLinkPavedCatchmentTestCases))]
        public void PavedCatchmentWithLinkToLateralSourceOrRunoffBoundary_CannotBeLinkedToAnotherLateralSourceOrRunoffBoundary(
            IHydroObject target1,
            IHydroObject target2)
        {
            var catchment = new Catchment()
            {
                CatchmentType = CatchmentType.Paved,
            };

            var link = new HydroLink(catchment, target1);
            catchment.Links.Add(link);
 
            // Call
            bool canLinkTo = catchment.CanLinkTo(target2);

            // Assert
            Assert.That(canLinkTo, Is.False);
        }

        private static IEnumerable<TestCaseData> CanLinkPavedCatchmentTestCases()
        {
            yield return new TestCaseData(new LateralSource(), new LateralSource())
                .SetName("Cannot link paved catchment to 2 lateral sources.");
            yield return new TestCaseData(new LateralSource(), new RunoffBoundary())
                .SetName("Cannot link paved catchment to a lateral source and a runoff boundary.");
            yield return new TestCaseData(new RunoffBoundary(), new LateralSource())
                .SetName("Cannot link paved catchment to a runoff boundary and a lateral source.");
            yield return new TestCaseData(new RunoffBoundary(), new RunoffBoundary())
                .SetName("Cannot link paved catchment to 2 runoff boundaries.");
        }

    }
}
