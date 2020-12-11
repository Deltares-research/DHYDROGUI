using System;
using DelftTools.Functions;
using DelftTools.Hydro.Structures;
using NUnit.Framework;
using ValidationAspects;

namespace DelftTools.Hydro.Tests.Structures
{
    [TestFixture]
    public class PumpTest
    {
        [Test]
        public void DefaultPump()
        {
            var pump = (IPump) new Pump(false);
            Assert.IsTrue(pump.Validate().IsValid);
            Assert.IsFalse(pump.CanBeTimedependent);
            Assert.IsFalse(pump.UseCapacityTimeSeries);
            Assert.IsNull(pump.CapacityTimeSeries);
        }

        [Test]
        public void DefaultTimeDependentPump()
        {
            var pump = (IPump) new Pump(true);
            Assert.IsTrue(pump.Validate().IsValid);
            Assert.IsTrue(pump.CanBeTimedependent);
            Assert.IsFalse(pump.UseCapacityTimeSeries);
            Assert.IsNotNull(pump.CapacityTimeSeries, "Time series should be initialized.");
        }

        [Test]
        public void UseTimeDepententPreconditionThrows()
        {
            var pump = (IPump) new Pump(false);

            // Always allow setting to false:
            pump.UseCapacityTimeSeries = false;

            Assert.Throws<InvalidOperationException>(() => pump.UseCapacityTimeSeries = true);
        }

        [Test]
        public void Clone()
        {
            var pump = new Pump("Kees") {LongName = "Long"};
            var clone = (Pump) pump.Clone();

            Assert.AreEqual(clone.LongName, pump.LongName);
        }

        [Test]
        public void CopyFrom()
        {
            var targetPump = new Pump();
            var sourcePump = new Pump(true)
            {
                Name = "target",
                Capacity = 42.0,
                StartDelivery = 1,
                StopDelivery = 0,
                StartSuction = 4.0,
                StopSuction = 3.0,
                DirectionIsPositive = false,
                ControlDirection = PumpControlDirection.DeliverySideControl,
                ReductionTable =
                    FunctionHelper.Get1DFunction<double, double>("reduction", "difference",
                                                                 "factor")
            };
            sourcePump.CapacityTimeSeries[new DateTime(2010, 1, 2, 4, 5, 6)] = 7.8;

            targetPump.CopyFrom(sourcePump);
            Assert.AreEqual(sourcePump.Attributes, targetPump.Attributes);
            Assert.AreEqual(sourcePump.Capacity, targetPump.Capacity);
            Assert.AreEqual(sourcePump.StopDelivery, targetPump.StopDelivery);
            Assert.AreEqual(sourcePump.StartDelivery, targetPump.StartDelivery);
            Assert.AreEqual(sourcePump.StartSuction, targetPump.StartSuction);
            Assert.AreEqual(sourcePump.StopSuction, targetPump.StopSuction);
            Assert.AreEqual(sourcePump.ControlDirection, targetPump.ControlDirection);
            Assert.AreEqual(sourcePump.DirectionIsPositive, targetPump.DirectionIsPositive);
            for (var i = 0; i < sourcePump.ReductionTable.Components[0].Values.Count; i++)
            {
                Assert.AreEqual(sourcePump.ReductionTable.Components[0].Values[i], targetPump.ReductionTable.Components[0].Values[i]);
            }

            Assert.AreNotEqual(sourcePump.Name, targetPump.Name);

            Assert.AreEqual(sourcePump.CanBeTimedependent, targetPump.CanBeTimedependent);
            Assert.AreEqual(sourcePump.UseCapacityTimeSeries, targetPump.UseCapacityTimeSeries);
            for (var i = 0; i < sourcePump.CapacityTimeSeries.Components[0].Values.Count; i++)
            {
                Assert.AreEqual(sourcePump.CapacityTimeSeries.Components[0].Values[i], targetPump.CapacityTimeSeries.Components[0].Values[i]);
            }
        }
    }
}