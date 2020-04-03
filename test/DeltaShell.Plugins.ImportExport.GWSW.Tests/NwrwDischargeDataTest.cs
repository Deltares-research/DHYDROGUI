using System;
using DelftTools.Hydro;
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
        public void GivenRrModelWithNwrwDryWeatherFlowDefinition_WhenCallingSetCorrectLateralSurface_ThenCorrectValueIsSetForLateralSurface()
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

                // when
                nwrwDischargeData.SetCorrectLateralSurface(rrModel);

                // then
                var expectedValue = dwf.DailyVolumeConstant / 1000 / 86400; // from dm³/day to m³/s 
                Assert.That(nwrwDischargeData.LateralSurface, Is.EqualTo(expectedValue).Within(tolerance));
            }
        }

        [Test]
        public void GivenRrModelWithNwrwDryWeatherFlowDefinitionAndDischargeDataHasNoId_WhenCallingSetCorrectLateralSurface_ThenCorrectValueIsSetForLateralSurface()
        {
            using (var rrModel = new RainfallRunoffModel())
            {
                // given
                var random = new Random(21);
                var lateralSurface = random.NextDouble();
                var dwf = new NwrwDryWeatherFlowDefinition
                {
                    Name = "Test_dwf_def",
                    DailyVolumeConstant = random.NextDouble() // dm³/day
                };
                rrModel.NwrwDryWeatherFlowDefinitions.Add(dwf);

                var nwrwDischargeData = new NwrwDischargeData
                {
                    LateralSurface = lateralSurface
                };

                // when
                nwrwDischargeData.SetCorrectLateralSurface(rrModel);

                // then
                var expectedValue = lateralSurface / 86400; // from dm³/day to m³/s 
                Assert.That(nwrwDischargeData.LateralSurface, Is.EqualTo(expectedValue).Within(tolerance));
            }
        }

        [Test]
        public void GivenRrModelWithoutNwrwDryWeatherFlowDefinition_WhenCallingSetCorrectLateralSurface_ThenCorrectValueIsSetForLateralSurface()
        {
            using (var rrModel = new RainfallRunoffModel())
            {
                // given
                var random = new Random(21);
                var name = "Test_dwf_def";
                var lateralSurface = random.NextDouble(); // m³/day

                var nwrwDischargeData = new NwrwDischargeData
                {
                    DryWeatherFlowId = name,
                    LateralSurface = lateralSurface
                };

                // when
                nwrwDischargeData.SetCorrectLateralSurface(rrModel);

                // then
                var expectedValue = lateralSurface / 86400; // from m³/day to m³/s
                Assert.That(nwrwDischargeData.LateralSurface, Is.EqualTo(expectedValue).Within(tolerance));
            }
        }

        [Test]
        public void SetCorrectLateralSurface_ModelNull_ThrowsArgumentException()
        {
            // Call
            TestDelegate call = () => new NwrwDischargeData().SetCorrectLateralSurface(null);

            // Assert
            Assert.That(call, Throws.ArgumentException);
        }

        [Test]
        public void SetCorrectLaterSurface_ModelNotRRModel_ThrowsArgumentException()
        {
            // Setup
            var mocks = new MockRepository();
            var invalidModel = mocks.Stub<IHydroModel>();
            mocks.ReplayAll();

            // Call
            TestDelegate call = () => new NwrwDischargeData().SetCorrectLateralSurface(invalidModel);

            // Assert
            Assert.That(call, Throws.ArgumentException);
            mocks.VerifyAll();
        }
    }
}