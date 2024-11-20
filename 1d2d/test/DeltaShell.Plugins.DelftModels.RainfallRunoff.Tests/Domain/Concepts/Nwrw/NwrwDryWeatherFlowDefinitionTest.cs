using System;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FixedFiles;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Concepts.Nwrw
{
    [TestFixture]
    public class NwrwDryWeatherFlowDefinitionTest
    {
        [Test]
        public void GivenAModelWithAnExistingDryWeatherFlowDefinition_WhenAddingNewDefaultDefinition_ThenNewDefaultDefinitionIsSet()
        {
            using (var model = new RainfallRunoffModel())
            {
                // Setup
                ILogHandler logHandler = Substitute.For<ILogHandler>();
                var dryWeatherFlowDefinition = new NwrwDryWeatherFlowDefinition()
                {
                    Name = NwrwDryWeatherFlowDefinition.DefaultDwaId,
                    DayNumber = 123,
                    DistributionType = DryweatherFlowDistributionType.Daily,
                    DailyVolumeConstant = 456,
                    DailyVolumeVariable = 789,
                    HourlyPercentageDailyVolume = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 }
                };

                // Precondition
                Assert.That(model.NwrwDryWeatherFlowDefinitions.Count(), Is.EqualTo(2));
                NwrwDryWeatherFlowDefinition defaultDefinition = model.NwrwDryWeatherFlowDefinitions.First();
                Assert.That(defaultDefinition.Name, Is.EqualTo(NwrwDryWeatherFlowDefinition.DefaultDwaId));
                
                // Call
                dryWeatherFlowDefinition.AddNwrwCatchmentModelDataToModel(model, new NwrwImporterHelper(), logHandler);
                
                // Assert
                IEventedList<NwrwDryWeatherFlowDefinition> definitions = model.NwrwDryWeatherFlowDefinitions;
                Assert.That(definitions.Count, Is.EqualTo(2));

                NwrwDryWeatherFlowDefinition newDefaultDefinition = definitions.First(d => d.Name.Equals(NwrwDryWeatherFlowDefinition.DefaultDwaId, StringComparison.InvariantCultureIgnoreCase));
                Assert.That(newDefaultDefinition, Is.EqualTo(dryWeatherFlowDefinition));

                Assert.That(newDefaultDefinition, Is.Not.SameAs(defaultDefinition)); // Check if old default definition has really been removed and not just updated.
            }
        }

        [Test]
        public void Constructor_NameIsNotEmptyUponInitialization()
        {
            var definition = new NwrwDryWeatherFlowDefinition();

            Assert.That(string.IsNullOrWhiteSpace(definition.Name), Is.False);
        }

        [Test]
        public void Constructor_HourlyPercentageDailyVolumeIsCorrectlyInitialized()
        {
            // Arrange
            var defaultDwaFile = RainfallRunoffModelFixedFiles.ReadFixedFileFromResource("PLUVIUS.DWA");
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            NwrwDryWeatherFlowDefinitionBuilder nwrwDryWeatherFlowDefinitionBuilder = new NwrwDryWeatherFlowDefinitionBuilder();
            SobekRRDryWeatherFlow sobekRRDryWeatherFlow = new SobekRRDryWeatherFlowReader().Parse(defaultDwaFile).First();
            double[] hourlyPercentageDailyVolume = nwrwDryWeatherFlowDefinitionBuilder.Build(sobekRRDryWeatherFlow, logHandler).HourlyPercentageDailyVolume;
            var rrModel = new RainfallRunoffModel();

            // Call
            var definition = new NwrwDryWeatherFlowDefinition();

            // Assert
            Assert.That(definition.HourlyPercentageDailyVolume, Is.EqualTo(hourlyPercentageDailyVolume));
        }

        [TearDown]
        public void CleanUp()
        {
            if (!string.IsNullOrWhiteSpace(NwrwDryWeatherFlowDefinition.DefaultDwaId))
            {
                TypeUtils.SetPrivatePropertyValue(new NwrwDryWeatherFlowDefinition(), "DefaultDwaId",string.Empty);
            }
        }

        
    }
}