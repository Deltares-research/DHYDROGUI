using System;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests
{
    [TestFixture]
    public class NwrwDischargeDataTest
    {
        [Test]
        public void GivenRrModelWithNwrwDryWeatherFlowDefinition_WhenCallingSetCorrectLateralSurface_ThenCorrectValueIsSetForLateralSurface()
        {
            using (var rrModel = new RainfallRunoffModel())
            {
                // given
                string name = "Test_dwf_def";
                double volume = 123; // dm³/day

                var dwf = new NwrwDryWeatherFlowDefinition
                {
                    Name = name,
                    DailyVolumeConstant = volume
                };
                rrModel.NwrwDryWeatherFlowDefinitions.Add(dwf);

                var nwrwDischargeData = new NwrwDischargeData()
                {
                    DryWeatherFlowId = name,
                    LateralSurface = 0.0,
                    DischargeType = DischargeType.Lateral
                };

                // when
                nwrwDischargeData.SetCorrectLateralSurface(rrModel);

                // then
                double expectedValue = volume / 1000 / 3600; // from dm³/day to m³/s 
                Assert.That(nwrwDischargeData.LateralSurface, Is.EqualTo(expectedValue).Within(0.0001));

            }
        }

        [Test]
        public void GivenRrModelWithoutNwrwDryWeatherFlowDefinition_WhenCallingSetCorrectLateralSurface_ThenCorrectValueIsSetForLateralSurface()
        {
            using (var rrModel = new RainfallRunoffModel())
            {
                // given
                string name = "Test_dwf_def";
                double lateralSurface = 12; // m³/day

                var nwrwDischargeData = new NwrwDischargeData()
                {
                    DryWeatherFlowId = name,
                    LateralSurface = lateralSurface,
                    DischargeType = DischargeType.Lateral
                };

                // when
                nwrwDischargeData.SetCorrectLateralSurface(rrModel);

                // then
                double expectedValue = lateralSurface / 86400; // from m³/day to m³/s
                Assert.That(nwrwDischargeData.LateralSurface, Is.EqualTo(expectedValue).Within(0.0001));

            }
        }
    }
}