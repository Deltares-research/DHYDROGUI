using System;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils;
using NUnit.Framework;
using Rhino.Mocks;

namespace DelftTools.OpenMI2.Tests
{
    
    [TestFixture]
    public class TimeSpaceComponentWrapperTest
    {
        private static readonly MockRepository Mocks = new MockRepository();
        [Test]
        public void TimeExtendIsBasedOnStarAndStopTime()
        {
            var model = Mocks.Stub<ITimeDependentModel>();
            model.StartTime = new DateTime(2001, 1, 1);
            model.StopTime= new DateTime(2002, 10, 10);

            var tsc = new TimeSpaceComponentWrapper(model);

            var durationInDays = (model.StopTime - model.StartTime).TotalDays;
            var julianStartDay = model.StartTime.ToModifiedJulianDay();

            Assert.AreEqual(durationInDays, tsc.TimeExtent.TimeHorizon.DurationInDays);
            Assert.AreEqual(julianStartDay, tsc.TimeExtent.TimeHorizon.StampAsModifiedJulianDay);
        }
    }
}