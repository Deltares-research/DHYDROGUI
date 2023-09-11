using System;
using System.Linq;
using DelftTools.Shell.Core.Properties;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DHYDRO.Common.Logging;
using NSubstitute;
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
                ILogHandler logHandler = Substitute.For<ILogHandler>();

                var name = "Test_dwf_def";

                var dwf = new NwrwDryWeatherFlowDefinition(logHandler)
                {
                    Name = name,
                    DailyVolumeConstant = random.NextDouble() // dm³/day
                };
                rrModel.NwrwDryWeatherFlowDefinitions.Add(dwf);
                
                var nwrwDischargeData = new NwrwDischargeData(logHandler)
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
            // Arrange
            ILogHandler logHandler = Substitute.For<ILogHandler>();

            // Call
            TestDelegate call = () => new NwrwDischargeData(logHandler).CalculateLateralFlow(null);

            // Assert
            Assert.That(call, Throws.Nothing);
            logHandler.Received().ReportError(Properties.Resources.NwrwDischargeData_CalculateLateralFlow_In_CalculateLateralFlow_parameter_nwrwDryWeatherFlowDefinitionByName_is_null);
        }

        [Test]
        public void SetCorrectLaterSurface_DictionaryIsNull_ThrowsArgumentNullException()
        {
            // Setup
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            ILookup<string, NwrwDryWeatherFlowDefinition> nwrwDryWeatherFlowDefinitionByName = Substitute.For<ILookup<string, NwrwDryWeatherFlowDefinition>>();
            const string dryWeatherFlowId = "myId";
            
            // Call
            TestDelegate call = () => new NwrwDischargeData(logHandler) { DryWeatherFlowId = dryWeatherFlowId }.CalculateLateralFlow(nwrwDryWeatherFlowDefinitionByName);

            // Assert
            Assert.That(call, Throws.Nothing);
            logHandler.Received().ReportError(string.Format(Properties.Resources.NwrwDischargeData_CalculateLateralFlow_Cannot_find_NwrwDryWeatherFlowDefinition_in_RR_model_by_name___0, dryWeatherFlowId));
        }
    }
}