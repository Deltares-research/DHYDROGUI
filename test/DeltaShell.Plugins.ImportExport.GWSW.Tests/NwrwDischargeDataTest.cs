using System;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests
{
    [TestFixture]
    public class NwrwDischargeDataTest
    {
        private const double tolerance = 0.0001;

        [Test]
        public void GivenRrModelWithNwrwDryWeatherFlowDefinition_WhenCallingCalculateLateralFlow_ThenCorrectValueIsReturned()
        {
            using (var rrModel = new RainfallRunoffModel())
            {
                // given
                var random = new Random(21);
                var name = "Test_dwf_def";

                var dwf = new NwrwDryWeatherFlowDefinition
                {
                    Name = name,
                    DailyVolumeConstant = random.NextDouble() // dm³/day
                };
                rrModel.NwrwDryWeatherFlowDefinitions.Add(dwf);

                var nwrwDischargeData = new NwrwDischargeData
                {
                    DryWeatherFlowId = name
                };

                // setup when
                var dwfdByName = rrModel.NwrwDryWeatherFlowDefinitions.ToLookup(dwfd => dwfd.Name, dwfd => dwfd);
                // when
                var actualLateralFlow = nwrwDischargeData.CalculateLateralFlow(dwfdByName);

                // then
                var expectedValue = dwf.DailyVolumeConstant / 1000 / 3600; // from dm³/day to m³/s 
                Assert.That(actualLateralFlow, Is.EqualTo(expectedValue).Within(tolerance));
            }
        }

        [Test]
        public void SetCorrectLateralSurface_DictionaryNull_ThrowsArgumentNullException()
        {
            // Call
            TestDelegate call = () => new NwrwDischargeData().CalculateLateralFlow(null);

            // Assert
            Assert.That(call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void SetCorrectLaterSurface_DictionaryIsNull_ThrowsArgumentNullException()
        {
            // Setup
            var mocks = new MockRepository();
            mocks.ReplayAll();

            // Call
            TestDelegate call = () => new NwrwDischargeData().CalculateLateralFlow(null);

            // Assert
            Assert.That(call, Throws.TypeOf<ArgumentNullException>());
            mocks.VerifyAll();
        }
    }
}