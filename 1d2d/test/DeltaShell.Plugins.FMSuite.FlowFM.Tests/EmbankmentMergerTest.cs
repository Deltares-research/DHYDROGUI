using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    class EmbankmentMergerTest
    {

        private static Embankment createEmbankment(string name, Coordinate[] lstCoordinates)
        {
            return new Embankment()
            {
                Name = name,
                Geometry = new LineString(lstCoordinates)
            };
        }

        [Test]
        public void MergeDoubleIntersectingTest()
        {
            var pointList1 = new List<Coordinate>
            {
                new Coordinate {X = 110, Y = 140},
                new Coordinate {X = 90, Y = 110},
                new Coordinate {X = 70, Y = 100},
                new Coordinate {X = 110, Y = 65},
                new Coordinate {X = 175, Y = 75},
            };

            var testEmbankment1 = new Embankment
            {
                Geometry = new LineString(pointList1.ToArray()),
                Name = "Embankment01"
            };

            var pointList2 = new List<Coordinate>
            {
                new Coordinate {X = 145, Y = 50},
                new Coordinate {X = 60, Y = 100},
                new Coordinate {X = 130, Y = 130},
                new Coordinate {X = 100, Y = 120},
                new Coordinate {X = 80, Y = 130},
            };

            var testEmbankment2 = new Embankment
            {
                Geometry = new LineString(pointList2.ToArray()),
                Name = "Embankment02"
            };

            IList<Embankment> embankmentDefinitions = new List<Embankment>();
            embankmentDefinitions.Add(testEmbankment1);
            embankmentDefinitions.Add(testEmbankment2);

            var result = EmbankmentMerger.MergeSelectedEmbankments(embankmentDefinitions, testEmbankment1, testEmbankment2);

            Assert.Null(result);
        }

        [Test]
        public void EmbankmentMergeNoIntersectionTest()
        {

            var embankment1 = createEmbankment("Embankment01", new[]
            {
                new Coordinate(10, 20),
                new Coordinate(10, 100)
            });

            var embankment2 = createEmbankment("Embankment02", new[]
            {
                new Coordinate(20, 10),
                new Coordinate(100, 10)
            });

            var embankment3 = createEmbankment("Embankment03", new[]
            {
                new Coordinate(0, 0),
                new Coordinate(999, 999)
            });

            var embankmentDefinitions = new List<Embankment>()
            {
                embankment1, embankment2, embankment3
            };

            var mergedEmbankment = EmbankmentMerger.MergeSelectedEmbankments(embankmentDefinitions, embankment1, embankment2);

            Assert.That(mergedEmbankment.Geometry.NumPoints == 4);
            Assert.That(mergedEmbankment.Geometry.Coordinates[0].Equals(new Coordinate(10, 100)));
            Assert.That(mergedEmbankment.Geometry.Coordinates[1].Equals(new Coordinate(10, 20)));
            Assert.That(mergedEmbankment.Geometry.Coordinates[2].Equals(new Coordinate(20, 10)));
            Assert.That(mergedEmbankment.Geometry.Coordinates[3].Equals(new Coordinate(100, 10)));
        }

        [Test]
        public void EmbankmentMergeNoIntersectionReversedLineStringTest()
        {

            var embankment1 = createEmbankment("Embankment01", new[]
            {
                new Coordinate(10, 20),
                new Coordinate(10, 100)
            });

            var embankment2 = createEmbankment("Embankment02", new[]
            {
                new Coordinate(100, 10),
                new Coordinate(20, 10)
            });

            var embankment3 = createEmbankment("Embankment03", new[]
            {
                new Coordinate(0, 0),
                new Coordinate(999, 999)
            });

            var embankmentDefinitions = new List<Embankment>()
            {
                embankment1, embankment2, embankment3
            };

            var mergedEmbankment = EmbankmentMerger.MergeSelectedEmbankments(embankmentDefinitions, embankment1, embankment2);

            Assert.That(mergedEmbankment.Geometry.NumPoints == 4);
            Assert.That(mergedEmbankment.Geometry.Coordinates[0].Equals(new Coordinate(10, 100)));
            Assert.That(mergedEmbankment.Geometry.Coordinates[1].Equals(new Coordinate(10, 20)));
            Assert.That(mergedEmbankment.Geometry.Coordinates[2].Equals(new Coordinate(20, 10)));
            Assert.That(mergedEmbankment.Geometry.Coordinates[3].Equals(new Coordinate(100, 10)));
        }

        [Test]
        public void EmbankmentMergeOneIntersectionTest()
        {

            var embankment1 = createEmbankment("Embankment01", new[]
            {
                new Coordinate(10, 0),
                new Coordinate(10, 100)
            });

            var embankment2 = createEmbankment("Embankment02", new[]
            {
                new Coordinate(0, 10),
                new Coordinate(100, 10)
            });

            var embankment3 = createEmbankment("Embankment03", new[]
            {
                new Coordinate(0, 0),
                new Coordinate(999, 999)
            });

            var embankmentDefinitions = new List<Embankment>()
            {
                embankment1, embankment2, embankment3
            };

            var mergedEmbankment = EmbankmentMerger.MergeSelectedEmbankments(embankmentDefinitions, embankment1, embankment2);

            Assert.That(mergedEmbankment.Geometry.NumPoints == 3);
            Assert.That(mergedEmbankment.Geometry.Coordinates[0].Equals(new Coordinate(10, 100)));
            Assert.That(mergedEmbankment.Geometry.Coordinates[1].Equals(new Coordinate(10, 10)));  // The intersection
            Assert.That(mergedEmbankment.Geometry.Coordinates[2].Equals(new Coordinate(100, 10)));
        }

        [Test]
        public void EmbankmentMergeOneIntersectionReversedLineStringTest()
        {

            // This embankment has different different direction that previous test. Should give exact same result. 
            var embankment1 = createEmbankment("Embankment01", new[]
            {
                new Coordinate(10, 100),  
                new Coordinate(10, 0)
            });

            var embankment2 = createEmbankment("Embankment02", new[]
            {
                new Coordinate(0, 10),
                new Coordinate(100, 10)
            });

            var embankment3 = createEmbankment("Embankment03", new[]
            {
                new Coordinate(0, 0),
                new Coordinate(999, 999)
            });

            var embankmentDefinitions = new List<Embankment>()
            {
                embankment1, embankment2, embankment3
            };

            var mergedEmbankment = EmbankmentMerger.MergeSelectedEmbankments(embankmentDefinitions, embankment1, embankment2);

            Assert.That(mergedEmbankment.Geometry.NumPoints == 3);
            Assert.That(mergedEmbankment.Geometry.Coordinates[0].Equals(new Coordinate(10, 100)));
            Assert.That(mergedEmbankment.Geometry.Coordinates[1].Equals(new Coordinate(10, 10)));  // The intersection. 
            Assert.That(mergedEmbankment.Geometry.Coordinates[2].Equals(new Coordinate(100, 10)));
        }

        [Test]
        public void MergePlus()
        {
            // No crash when the embankments are shaped in an exact plus sign, that is: all endpoints have equal distances to each other. 

            var pointList1 = new List<Coordinate>
            {
                new Coordinate {X = 10, Y = 20},
                new Coordinate {X = 30, Y = 20}
            };

            var testEmbankment1 = new Embankment
            {
                Geometry = new LineString(pointList1.ToArray()),
                Name = "Embankment01"
            };

            var pointList2 = new List<Coordinate>
            {
                new Coordinate {X = 20, Y = 10},
                new Coordinate {X = 20, Y = 30}
            };

            var testEmbankment2 = new Embankment
            {
                Geometry = new LineString(pointList2.ToArray()),
                Name = "Embankment02"
            };

            IList<Embankment> embankmentDefinitions = new List<Embankment>();
            embankmentDefinitions.Add(testEmbankment1);
            embankmentDefinitions.Add(testEmbankment2);

            var result = EmbankmentMerger.MergeSelectedEmbankments(embankmentDefinitions, testEmbankment1, testEmbankment2);

            Assert.NotNull(result);
            Assert.AreEqual(3, result.Geometry.Coordinates.Count());

            // Result: undetermined. 

            Assert.AreEqual("MergedEmbankment01", result.Name);
        }


        [Test]
        public void MergeByAddingTest()
        {
            var pointList1 = new List<Coordinate>
            {
                new Coordinate {X = 90, Y = 110},
                new Coordinate {X = 70, Y = 100},
                new Coordinate {X = 50, Y = 50},
                new Coordinate {X = 20, Y = 20},
            };

            var testEmbankment1 = new Embankment
            {
                Geometry = new LineString(pointList1.ToArray()),
                Name = "Embankment01"
            };

            var pointList2 = new List<Coordinate>
            {
                new Coordinate {X = 240, Y = 60},
                new Coordinate {X = 200, Y = 110},
                new Coordinate {X = 190, Y = 140},
                new Coordinate {X = 130, Y = 130},
                new Coordinate {X = 100, Y = 120},
            };

            var testEmbankment2 = new Embankment
            {
                Geometry = new LineString(pointList2.ToArray()),
                Name = "Embankment02"
            };

            IList<Embankment> embankmentDefinitions = new List<Embankment>();
            embankmentDefinitions.Add(testEmbankment1);
            embankmentDefinitions.Add(testEmbankment2);

            var result = EmbankmentMerger.MergeSelectedEmbankments(embankmentDefinitions, testEmbankment1, testEmbankment2);

            Assert.NotNull(result);
            Assert.AreEqual(9, result.Geometry.Coordinates.Count());

            Assert.AreEqual(20.0, result.Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(20.0, result.Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(50.0, result.Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(50.0, result.Geometry.Coordinates[1].Y, 0.00000000001);
            Assert.AreEqual(70.0, result.Geometry.Coordinates[2].X, 0.00000000001);
            Assert.AreEqual(100.0, result.Geometry.Coordinates[2].Y, 0.00000000001);
            Assert.AreEqual(90.0, result.Geometry.Coordinates[3].X, 0.00000000001);
            Assert.AreEqual(110.0, result.Geometry.Coordinates[3].Y, 0.00000000001);
            Assert.AreEqual(100.0, result.Geometry.Coordinates[4].X, 0.00000000001);
            Assert.AreEqual(120.0, result.Geometry.Coordinates[4].Y, 0.00000000001);
            Assert.AreEqual(130.0, result.Geometry.Coordinates[5].X, 0.00000000001);
            Assert.AreEqual(130.0, result.Geometry.Coordinates[5].Y, 0.00000000001);
            Assert.AreEqual(190.0, result.Geometry.Coordinates[6].X, 0.00000000001);
            Assert.AreEqual(140.0, result.Geometry.Coordinates[6].Y, 0.00000000001);
            Assert.AreEqual(200.0, result.Geometry.Coordinates[7].X, 0.00000000001);
            Assert.AreEqual(110.0, result.Geometry.Coordinates[7].Y, 0.00000000001);
            Assert.AreEqual(240.0, result.Geometry.Coordinates[8].X, 0.00000000001);
            Assert.AreEqual(60.0, result.Geometry.Coordinates[8].Y, 0.00000000001);

            Assert.AreEqual("MergedEmbankment01", result.Name);
        }

        [Test]
        public void MergeIntersectingTest()
        {
            var pointList1 = new List<Coordinate>
            {
                new Coordinate {X = 130, Y = 150},
                new Coordinate {X = 110, Y = 140},
                new Coordinate {X = 90, Y = 110},
                new Coordinate {X = 70, Y = 100},
                new Coordinate {X = 50, Y = 50},
                new Coordinate {X = 20, Y = 20},
            };

            var testEmbankment1 = new Embankment
            {
                Geometry = new LineString(pointList1.ToArray()),
                Name = "Embankment01"
            };

            var pointList2 = new List<Coordinate>
            {
                new Coordinate {X = 240, Y = 60},
                new Coordinate {X = 200, Y = 110},
                new Coordinate {X = 190, Y = 140},
                new Coordinate {X = 130, Y = 130},
                new Coordinate {X = 100, Y = 120},
                new Coordinate {X = 80, Y = 130},
                new Coordinate {X = 40, Y = 140},
            };

            var testEmbankment2 = new Embankment
            {
                Geometry = new LineString(pointList2.ToArray()),
                Name = "Embankment02"
            };

            IList<Embankment> embankmentDefinitions = new List<Embankment>();
            embankmentDefinitions.Add(testEmbankment1);
            embankmentDefinitions.Add(testEmbankment2);

            var result = EmbankmentMerger.MergeSelectedEmbankments(embankmentDefinitions, testEmbankment1, testEmbankment2);

            Assert.NotNull(result);
            Assert.AreEqual(10, result.Geometry.Coordinates.Count());

            Assert.AreEqual(20.0, result.Geometry.Coordinates[0].X, 0.00000000001);
            Assert.AreEqual(20.0, result.Geometry.Coordinates[0].Y, 0.00000000001);
            Assert.AreEqual(50.0, result.Geometry.Coordinates[1].X, 0.00000000001);
            Assert.AreEqual(50.0, result.Geometry.Coordinates[1].Y, 0.00000000001);
            Assert.AreEqual(70.0, result.Geometry.Coordinates[2].X, 0.00000000001);
            Assert.AreEqual(100.0, result.Geometry.Coordinates[2].Y, 0.00000000001);
            Assert.AreEqual(90.0, result.Geometry.Coordinates[3].X, 0.00000000001);
            Assert.AreEqual(110.0, result.Geometry.Coordinates[3].Y, 0.00000000001);
            Assert.AreEqual(97.5, result.Geometry.Coordinates[4].X, 0.00000000001);
            Assert.AreEqual(121.25, result.Geometry.Coordinates[4].Y, 0.00000000001);
            Assert.AreEqual(100.0, result.Geometry.Coordinates[5].X, 0.00000000001);
            Assert.AreEqual(120.0, result.Geometry.Coordinates[5].Y, 0.00000000001);
            Assert.AreEqual(130.0, result.Geometry.Coordinates[6].X, 0.00000000001);
            Assert.AreEqual(130.0, result.Geometry.Coordinates[6].Y, 0.00000000001);
            Assert.AreEqual(190.0, result.Geometry.Coordinates[7].X, 0.00000000001);
            Assert.AreEqual(140.0, result.Geometry.Coordinates[7].Y, 0.00000000001);
            Assert.AreEqual(200.0, result.Geometry.Coordinates[8].X, 0.00000000001);
            Assert.AreEqual(110.0, result.Geometry.Coordinates[8].Y, 0.00000000001);
            Assert.AreEqual(240.0, result.Geometry.Coordinates[9].X, 0.00000000001);
            Assert.AreEqual(60.0, result.Geometry.Coordinates[9].Y, 0.00000000001);

            Assert.AreEqual("MergedEmbankment01", result.Name);
        }


    }
}
