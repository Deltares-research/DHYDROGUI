using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    [TestFixture]
    public class RtcDataAccessListenerTest
    {
        [Test]
        [TestCase(InterpolationType.None, InterpolationType.Linear, TestName = "If interpolation type for time rules is none in the database, it should be changed to linear")]
        [TestCase(InterpolationType.Linear, InterpolationType.Linear, TestName = "If interpolation type for time rules is linear in the database, nothing will happen")]
        [TestCase(InterpolationType.Constant, InterpolationType.Constant, TestName = "If interpolation type for time rules is constant in the database, nothing will happen")]
        public void TestRemovingInterpolationNoneForTimeRulesIfSetInDatabase(InterpolationType before, InterpolationType after)
        {
            var entity = new TimeRule();
            var loadedState = new object[3];
            loadedState[0] = "bla";
            loadedState[1] = 3;

            var timeSeries = new TimeSeries();
            timeSeries.Arguments[0].InterpolationType = before;
            loadedState[2] = timeSeries;

            
            Assert.AreEqual(before, timeSeries.Time.InterpolationType);

            Assert.DoesNotThrow(() =>
                TypeUtils.CallPrivateStaticMethod(typeof(RtcDataAccessListener),
                    "RemovingInterpolationNoneForTimeRulesIfSetInDatabase", entity, loadedState));

            Assert.AreEqual(after, timeSeries.Time.InterpolationType);
        }
    }
}