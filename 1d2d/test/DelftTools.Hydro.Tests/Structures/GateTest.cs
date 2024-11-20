using System;
using DelftTools.Hydro.Structures;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using ValidationAspects;

namespace DelftTools.Hydro.Tests.Structures
{
    [TestFixture]
    public class GateTest
    {
        [Test]
        public void DefaultGate()
        {
            IGate gate = new Gate("Gate");

            Assert.IsTrue(gate.Validate().IsValid);
            Assert.IsFalse(gate.UseSillLevelTimeSeries);
            Assert.IsNotNull(gate.SillLevelTimeSeries, "Time series should be initialized.");
            Assert.IsFalse(gate.UseOpeningWidthTimeSeries);
            Assert.IsNotNull(gate.OpeningWidthTimeSeries, "Time series should be initialized.");
            Assert.IsFalse(gate.UseLowerEdgeLevelTimeSeries);
            Assert.IsNotNull(gate.LowerEdgeLevelTimeSeries, "Time series should be initialized.");
            Assert.AreEqual(0.0, gate.SillWidth);
        }

        [Test]
        public void Clone()
        {
            IGate gate = new Gate("Gate one")
            {
                Geometry = new Point(7, 0),
                OffsetY = 175,
                OpeningWidth = 75,
                LowerEdgeLevel = -3,
                SillWidth = 78,
            };
            var clonedGate = (IGate)gate.Clone();

            Assert.AreEqual(clonedGate.Name, gate.Name);
            Assert.AreEqual(clonedGate.OffsetY, gate.OffsetY);
            Assert.AreEqual(clonedGate.Geometry, gate.Geometry);
            Assert.AreEqual(clonedGate.OpeningWidth, gate.OpeningWidth);
            Assert.AreEqual(clonedGate.LowerEdgeLevel, gate.LowerEdgeLevel);
            Assert.AreEqual(clonedGate.SillWidth, gate.SillWidth);
        }

        [Test]
        public void CopyFromTest()
        {
            //create two different gates 
            //the target should copy all the property values form the source 
            //into the target, but not the name and geometry!! 
            IGate sourceGate = new Gate("Source Gate")
                {
                    Geometry = new Point(7, 0),
                    OffsetY = 175,
                    OpeningWidth = 75,
                    Name = "Source Weir",
                    UseSillLevelTimeSeries = true,
                    UseLowerEdgeLevelTimeSeries = true,
                    UseOpeningWidthTimeSeries = true,
                    SillWidth = 150
                };
            sourceGate.SillLevelTimeSeries[new DateTime(2013, 6, 5, 4, 3, 2, 1)] = 1.2;
            sourceGate.LowerEdgeLevelTimeSeries[new DateTime(2013, 1, 2, 3, 4, 5, 6)] = 7.8;
            sourceGate.OpeningWidthTimeSeries[new DateTime(2013, 9, 10, 11, 12, 13, 14)] = 15.16;
            IGate targetGate = new Gate("Target Gate")
                {
                    Geometry = new Point(42, 0),
                    OffsetY = 571,
                    OpeningWidth = 55,
                    SillLevel = -1,
                    Name = "Target Weir",
                    UseLowerEdgeLevelTimeSeries = false,
                    UseOpeningWidthTimeSeries = false,
                    SillWidth = 130
                };
            targetGate.CopyFrom(sourceGate);

            Assert.AreNotEqual(sourceGate.Name, targetGate.Name);
            Assert.AreEqual(sourceGate.OffsetY, targetGate.OffsetY);
            Assert.AreEqual(sourceGate.OpeningWidth, targetGate.OpeningWidth);
            Assert.AreEqual(sourceGate.UseSillLevelTimeSeries, targetGate.UseSillLevelTimeSeries);
            Assert.AreEqual(sourceGate.SillLevelTimeSeries[new DateTime(2013, 6, 5, 4, 3, 2, 1)],
                            targetGate.SillLevelTimeSeries[new DateTime(2013, 6, 5, 4, 3, 2, 1)]);
            Assert.AreEqual(sourceGate.UseLowerEdgeLevelTimeSeries, targetGate.UseLowerEdgeLevelTimeSeries);
            Assert.AreEqual(sourceGate.LowerEdgeLevelTimeSeries[new DateTime(2013, 1, 2, 3, 4, 5, 6)],
                            targetGate.LowerEdgeLevelTimeSeries[new DateTime(2013, 1, 2, 3, 4, 5, 6)]);
            Assert.AreEqual(sourceGate.UseOpeningWidthTimeSeries, targetGate.UseOpeningWidthTimeSeries);
            Assert.AreEqual(sourceGate.OpeningWidthTimeSeries[new DateTime(2013, 9, 10, 11, 12, 13, 14)],
                            targetGate.OpeningWidthTimeSeries[new DateTime(2013, 9, 10, 11, 12, 13, 14)]);
            Assert.AreEqual(sourceGate.SillWidth, targetGate.SillWidth);
        }
    }
}