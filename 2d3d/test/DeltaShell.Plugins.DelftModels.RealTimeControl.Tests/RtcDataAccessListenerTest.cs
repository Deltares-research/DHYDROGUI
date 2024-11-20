using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    [TestFixture]
    public class RtcDataAccessListenerTest
    {
        private static IEnumerable<TestCaseData> DifferentTestCaseData
        {
            get
            {
                yield return new TestCaseData(new RelativeTimeRule(), InterpolationType.None, InterpolationType.Linear).SetName("If interpolation type for relative time rules is none in the database, it should be changed to linear");
                yield return new TestCaseData(new RelativeTimeRule(), InterpolationType.Linear, InterpolationType.Linear).SetName("If interpolation type for relative time rules is linear in the database, nothing will happen");
                yield return new TestCaseData(new RelativeTimeRule(), InterpolationType.Constant, InterpolationType.Constant).SetName("If interpolation type for relative time rules is constant in the database, nothing will happen");
                yield return new TestCaseData(new TimeRule(), InterpolationType.None, InterpolationType.Linear).SetName("If interpolation type for time rules is none in the database, it should be changed to linear");
                yield return new TestCaseData(new TimeRule(), InterpolationType.Linear, InterpolationType.Linear).SetName("If interpolation type for time rules is linear in the database, nothing will happen");
                yield return new TestCaseData(new TimeRule(), InterpolationType.Constant, InterpolationType.Constant).SetName("If interpolation type for time rules is constant in the database, nothing will happen");
            }
        }

        [TestCaseSource(nameof(DifferentTestCaseData))]
        public void TestRemovingInterpolationNoneForTimeRulesIfSetInDatabase(object entity, InterpolationType before, InterpolationType after)
        {
            var loadedState = new object[3];
            loadedState[0] = "bla";
            loadedState[1] = 3;

            var timeSeries = new TimeSeries();
            timeSeries.Arguments[0].InterpolationType = before;
            loadedState[2] = timeSeries;
            Assert.AreEqual(before, timeSeries.Time.InterpolationType);

            var propertyNames = new string[1];
            var rtcDataAccessListener = new RtcDataAccessListener(null);

            rtcDataAccessListener.OnPreLoad(entity, loadedState, propertyNames);

            Assert.AreEqual(after, timeSeries.Time.InterpolationType);
        }
    }
}