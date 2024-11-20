using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    [TestFixture]
    public class TimeConditionTest
    {
        private const string Implicit = "IMPLICIT";
        private const string Name = "Trigger31";
        private const string InputName = "AlarmREGEN";
        private const string InputParameterName = "DeadBandTime";
        private TimeSeries timeSeries;
        private InterpolationType interpolation = InterpolationType.Linear;
        private ExtrapolationType extrapolation = ExtrapolationType.Periodic;

        private TimeCondition timeCondition;

        [SetUp]
        public void SetUp()
        {
            timeSeries = new TimeSeries();
            timeSeries.Components.Add(new Variable<bool> {DefaultValue = false});
            timeSeries[DateTime.Now] = true;
            timeSeries[DateTime.Now + new TimeSpan(1, 0, 0)] = false;
            timeSeries[DateTime.Now + new TimeSpan(2, 0, 0)] = true;
            timeSeries[DateTime.Now + new TimeSpan(3, 0, 0)] = false;
            timeSeries[DateTime.Now + new TimeSpan(4, 0, 0)] = true;

            timeCondition = new TimeCondition
            {
                Name = Name,
                Reference = Implicit,
                Input =
                    new Input
                    {
                        ParameterName = InputParameterName,
                        Feature = new RtcTestFeature {Name = InputName}
                    },
                TimeSeries = timeSeries,
                Extrapolation = extrapolation,
                InterpolationOptionsTime = interpolation
            };
        }

        [Test]
        public void CopyFrom()
        {
            var condition = new TimeCondition();
            condition.CopyFrom(timeCondition);
            Assert.AreEqual(Name, condition.Name);
            Assert.AreEqual(Implicit, condition.Reference);
            Assert.AreEqual(timeSeries, timeCondition.TimeSeries);
            Assert.AreEqual(5, timeCondition.TimeSeries.Arguments[0].Values.Count);
            Assert.AreEqual(interpolation, timeCondition.InterpolationOptionsTime);
            Assert.AreEqual(extrapolation, timeCondition.Extrapolation);
        }
    }
}