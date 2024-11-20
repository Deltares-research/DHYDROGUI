using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Domain
{
    [TestFixture]
    public class TimeRuleTests
    {
        [Test]
        public void CopyFromAndClone()
        {
            var source = new TimeRule()
            {
                Name = "test",
                InterpolationOptionsTime = InterpolationType.Linear,
                Periodicity = ExtrapolationType.Constant
                //TimeSeries = new TimeSeries()
            };
            var timeSeries = new TimeSeries();
            timeSeries.Arguments[0].DefaultValue = new DateTime(2000, 1, 1);
            timeSeries.Components.Add(new Variable<double>("someThing"));
            timeSeries.Time.ExtrapolationType = ExtrapolationType.Constant;
            DateTime time = DateTime.Now;
            timeSeries[time] = 1.0;
            source.TimeSeries = timeSeries;
            var newRule = new TimeRule();

            newRule.CopyFrom(source);

            Assert.AreEqual(source.Name, newRule.Name);
            Assert.AreEqual(source.InterpolationOptionsTime, newRule.InterpolationOptionsTime);
            Assert.AreEqual(source.Periodicity, newRule.Periodicity);
            for (var i = 0; i < source.TimeSeries.Arguments[0].Values.Count; i++)
            {
                Assert.AreEqual(source.TimeSeries.Arguments[0].Values[i], newRule.TimeSeries.Arguments[0].Values[i]);
                Assert.AreEqual(source.TimeSeries.Components[0].Values[i], newRule.TimeSeries.Components[0].Values[i]);
            }

            var clone = (TimeRule) source.Clone();
            Assert.IsFalse(ReferenceEquals(source, clone));
            Assert.AreEqual(source.Name, clone.Name);
        }

        [Test]
        public void DoNotSupportNoneExtrapolation()
        {
            Assert.That(() => new TimeRule
            {
                Name = "test",
                InterpolationOptionsTime = InterpolationType.Linear,
                Periodicity = ExtrapolationType.None
            }, Throws.ArgumentException);
        }

        [Test]
        public void DoNotSupportLinearExtrapolation()
        {
            Assert.That(() => new TimeRule
            {
                Name = "test",
                InterpolationOptionsTime = InterpolationType.Linear,
                Periodicity = ExtrapolationType.Linear
            }, Throws.ArgumentException);
        }
    }
}