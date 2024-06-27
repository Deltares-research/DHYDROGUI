using System;
using System.Linq;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.ImportExport.Sobek.Builders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.Builders
{
    [TestFixture]
    public class NwrwDryWeatherFlowDefinitionBuilderTest
    {
        [Test]
        public void Build_IncorrectWaterCapacityPerHourLength_LogsWarning()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var builder = new NwrwDryWeatherFlowDefinitionBuilder();

            var readDefinition = new SobekRRDryWeatherFlow
            {
                Id = "some_definition_name",
                ComputationOption = DWAComputationOption.ConstantDWAPerHour,
                WaterUsePerHourForConstant = 2,
                WaterUsePerDayForVariable = 3,
                WaterCapacityPerHour = new double[25]
            };

            // Call
            NwrwDryWeatherFlowDefinition definition = builder.Build(readDefinition, logHandler);

            // Assert
            logHandler.Received().ReportWarningFormat("Expected 24 values but got {0} values. Skipping import of water use per capita per hour.", 25);
            Assert.That(definition.HourlyPercentageDailyVolume, Has.Length.EqualTo(24));
        }

        [TestCase(DWAComputationOption.NrPeopleTimesConstantPerHour, DryweatherFlowDistributionType.Constant)]
        [TestCase(DWAComputationOption.ConstantDWAPerHour, DryweatherFlowDistributionType.Constant)]
        [TestCase(DWAComputationOption.NrPeopleTimesVariablePerHour, DryweatherFlowDistributionType.Daily)]
        [TestCase(DWAComputationOption.VariablePerHour, DryweatherFlowDistributionType.Daily)]
        public void Build_ReturnsCorrectNwrwDryWeatherFlowDefinition(DWAComputationOption computationOption, DryweatherFlowDistributionType expDistributionType)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var builder = new NwrwDryWeatherFlowDefinitionBuilder();

            double[] hourlyVolume = CreateArray();
            var readDefinition = new SobekRRDryWeatherFlow
            {
                Id = "some_definition_name",
                ComputationOption = computationOption,
                WaterUsePerHourForConstant = 2,
                WaterUsePerDayForVariable = 3,
                WaterCapacityPerHour = hourlyVolume
            };

            // Call
            NwrwDryWeatherFlowDefinition definition = builder.Build(readDefinition, logHandler);

            // Assert
            Assert.That(logHandler.ReceivedCalls(), Is.Empty);
            Assert.That(definition.Name, Is.EqualTo("some_definition_name"));
            Assert.That(definition.DistributionType, Is.EqualTo(expDistributionType));
            Assert.That(definition.DailyVolumeConstant, Is.EqualTo(48));
            Assert.That(definition.DailyVolumeVariable, Is.EqualTo(3));
            Assert.That(definition.HourlyPercentageDailyVolume, Is.EqualTo(hourlyVolume));
        }

        [TestCase(DWAComputationOption.UseTable)]
        [TestCase(99)]
        public void Build_UnsupportedDWAComputationOption_ThrowsNotSupportedException(DWAComputationOption computationOption)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var builder = new NwrwDryWeatherFlowDefinitionBuilder();

            double[] hourlyVolume = CreateArray();
            var readDefinition = new SobekRRDryWeatherFlow
            {
                Id = "some_definition_name",
                ComputationOption = computationOption,
                WaterUsePerHourForConstant = 2,
                WaterUsePerDayForVariable = 3,
                WaterCapacityPerHour = hourlyVolume
            };

            // Call
            void Call() => builder.Build(readDefinition, logHandler);

            // Assert
            Assert.Throws<NotSupportedException>(Call);
        }

        private static double[] CreateArray() => Enumerable.Range(0, 24).Select(i => (double)i).ToArray();
    }
}