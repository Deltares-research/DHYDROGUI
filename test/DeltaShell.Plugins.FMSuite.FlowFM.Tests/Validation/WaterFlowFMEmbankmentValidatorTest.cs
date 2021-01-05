using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation
{
    [TestFixture]
    public class WaterFlowFMEmbankmentValidatorTest
    {
        [Test]
        public void NoSelfIntersectionTest()
        {
            var model = new WaterFlowFMModel();

            var pointList = new List<Coordinate>
            {
                new Coordinate
                {
                    X = -30,
                    Y = -70
                },
                new Coordinate
                {
                    X = 10,
                    Y = 10
                },
                new Coordinate
                {
                    X = -40,
                    Y = 50
                },
                new Coordinate
                {
                    X = -20,
                    Y = 110
                },
                new Coordinate
                {
                    X = 30,
                    Y = 90
                }
            };

            var testEmbankment = new Embankment
            {
                Geometry = new LineString(pointList.ToArray()),
                Name = "NoSelfIntersectionTest"
            };

            model.Area.Embankments.Clear();
            model.Area.Embankments.Add(testEmbankment);

            ValidationReport report = WaterFlowFMEmbankmentValidator.Validate(model);

            Assert.NotNull(report);
            Assert.AreEqual(0, report.InfoCount);
            Assert.AreEqual(0, report.WarningCount);
            Assert.AreEqual(0, report.ErrorCount);
        }

        [Test]
        public void SelfIntersectionTest()
        {
            var model = new WaterFlowFMModel();

            var pointList = new List<Coordinate>
            {
                new Coordinate
                {
                    X = -30,
                    Y = -70
                },
                new Coordinate
                {
                    X = 10,
                    Y = 10
                },
                new Coordinate
                {
                    X = -40,
                    Y = 50
                },
                new Coordinate
                {
                    X = -20,
                    Y = 110
                },
                new Coordinate
                {
                    X = 30,
                    Y = 90
                },
                new Coordinate
                {
                    X = -50,
                    Y = 20
                },
                new Coordinate
                {
                    X = -50,
                    Y = -50
                }
            };

            var testEmbankment = new Embankment
            {
                Geometry = new LineString(pointList.ToArray()),
                Name = "SelfIntersectionTest"
            };

            model.Area.Embankments.Clear();
            model.Area.Embankments.Add(testEmbankment);

            ValidationReport report = WaterFlowFMEmbankmentValidator.Validate(model);

            Assert.NotNull(report);
            Assert.AreEqual(0, report.InfoCount);
            Assert.AreEqual(0, report.WarningCount);
            Assert.AreEqual(1, report.ErrorCount);
            Assert.AreEqual("Embankment SelfIntersectionTest intersects with itself", report.Issues.First().Message);
        }

        [Test]
        public void SelfIntersectionTest2()
        {
            var model = new WaterFlowFMModel();

            var pointList = new List<Coordinate>
            {
                new Coordinate
                {
                    X = 60,
                    Y = 20
                },
                new Coordinate
                {
                    X = 10,
                    Y = 110
                },
                new Coordinate
                {
                    X = 90,
                    Y = 110
                },
                new Coordinate
                {
                    X = 40,
                    Y = 20
                }
            };

            var testEmbankment = new Embankment
            {
                Geometry = new LineString(pointList.ToArray()),
                Name = "SelfIntersectionTest"
            };

            model.Area.Embankments.Clear();
            model.Area.Embankments.Add(testEmbankment);

            ValidationReport report = WaterFlowFMEmbankmentValidator.Validate(model);

            Assert.NotNull(report);
            Assert.AreEqual(0, report.InfoCount);
            Assert.AreEqual(0, report.WarningCount);
            Assert.AreEqual(1, report.ErrorCount);
            Assert.AreEqual("Embankment SelfIntersectionTest intersects with itself", report.Issues.First().Message);
        }

        [Test]
        public void NoEmbankmentIntersectionTest()
        {
            var model = new WaterFlowFMModel();

            var pointList1 = new List<Coordinate>
            {
                new Coordinate
                {
                    X = -30,
                    Y = -70
                },
                new Coordinate
                {
                    X = 10,
                    Y = 10
                },
                new Coordinate
                {
                    X = -40,
                    Y = 50
                },
                new Coordinate
                {
                    X = -20,
                    Y = 110
                },
                new Coordinate
                {
                    X = 30,
                    Y = 90
                }
            };

            var testEmbankment1 = new Embankment
            {
                Geometry = new LineString(pointList1.ToArray()),
                Name = "Embankment01"
            };

            var pointList2 = new List<Coordinate>
            {
                new Coordinate
                {
                    X = 30,
                    Y = -70
                },
                new Coordinate
                {
                    X = 70,
                    Y = 20
                },
                new Coordinate
                {
                    X = -10,
                    Y = 40
                },
                new Coordinate
                {
                    X = 60,
                    Y = 80
                }
            };

            var testEmbankment2 = new Embankment
            {
                Geometry = new LineString(pointList2.ToArray()),
                Name = "Embankment02"
            };

            model.Area.Embankments.Clear();
            model.Area.Embankments.Add(testEmbankment1);
            model.Area.Embankments.Add(testEmbankment2);

            ValidationReport report = WaterFlowFMEmbankmentValidator.Validate(model);

            Assert.NotNull(report);
            Assert.AreEqual(0, report.ErrorCount);
        }

        [Test]
        public void EmbankmentIntersectionTest()
        {
            var model = new WaterFlowFMModel();

            var pointList1 = new List<Coordinate>
            {
                new Coordinate
                {
                    X = -30,
                    Y = -70
                },
                new Coordinate
                {
                    X = 50,
                    Y = 40
                },
                new Coordinate
                {
                    X = -40,
                    Y = 50
                },
                new Coordinate
                {
                    X = -20,
                    Y = 110
                },
                new Coordinate
                {
                    X = 30,
                    Y = 90
                }
            };

            var testEmbankment1 = new Embankment
            {
                Geometry = new LineString(pointList1.ToArray()),
                Name = "Embankment01"
            };

            var pointList2 = new List<Coordinate>
            {
                new Coordinate
                {
                    X = 30,
                    Y = -70
                },
                new Coordinate
                {
                    X = 70,
                    Y = 20
                },
                new Coordinate
                {
                    X = -10,
                    Y = 40
                },
                new Coordinate
                {
                    X = 60,
                    Y = 80
                }
            };

            var testEmbankment2 = new Embankment
            {
                Geometry = new LineString(pointList2.ToArray()),
                Name = "Embankment02"
            };

            model.Area.Embankments.Clear();
            model.Area.Embankments.Add(testEmbankment1);
            model.Area.Embankments.Add(testEmbankment2);

            ValidationReport report = WaterFlowFMEmbankmentValidator.Validate(model);

            Assert.NotNull(report);
            Assert.AreEqual(0, report.InfoCount);
            Assert.AreEqual(0, report.WarningCount);
            Assert.AreEqual(2, report.ErrorCount);
            Assert.AreEqual("Embankment Embankment01 intersects with other embankments", report.Issues.First().Message);
            Assert.AreEqual("Embankment Embankment02 intersects with other embankments", report.Issues.Last().Message);
        }
    }
}