using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Concepts.Nwrw
{
    [TestFixture]
    public class NwrwDryWeatherFlowDefinitionTest
    {
        [Test]
        public void GivenAModelWithAnExistingDryWeatherFlowDefinition_WhenAddingNewDefaultDefinition_ThenNewDefaultDefinitionIsSet()
        {
            // Setup
            var dryWeatherFlowDefinition = new NwrwDryWeatherFlowDefinition()
            {
                Name = NwrwData.DEFAULT_DWA_ID,
                DayNumber = 123,
                DistributionType = DryweatherFlowDistributionType.Daily,
                DailyVolumeConstant = 456,
                DailyVolumeVariable = 789,
                HourlyPercentageDailyVolume = new double []{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24}
            };
            
            using (var model = new RainfallRunoffModel())
            {
                // Precondition
                Assert.That(model.NwrwDryWeatherFlowDefinitions.Count(), Is.EqualTo(1));
                NwrwDryWeatherFlowDefinition defaultDefinition = model.NwrwDryWeatherFlowDefinitions.Single();
                Assert.That(defaultDefinition.Name, Is.EqualTo(NwrwData.DEFAULT_DWA_ID));
                
                // Call
                dryWeatherFlowDefinition.AddNwrwCatchmentModelDataToModel(model, new NwrwImporterHelper());
                
                // Assert
                IEventedList<NwrwDryWeatherFlowDefinition> definitions = model.NwrwDryWeatherFlowDefinitions;
                Assert.That(definitions.Count, Is.EqualTo(1));

                NwrwDryWeatherFlowDefinition newDefaultDefinition = definitions.Single();
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
            // Call
            var definition = new NwrwDryWeatherFlowDefinition();

            // Assert
            Assert.That(definition.HourlyPercentageDailyVolume, Is.EqualTo(GetDefaultHourlyPercentageDailyVolume()));
        }

        [Test]
        public void CreateDefaultDwaDefinition_ReturnsCorrectInstance()
        {
            // Call
            var definition = NwrwDryWeatherFlowDefinition.CreateDefaultDwaDefinition();

            // Assert
            Assert.That(definition.Name, Is.EqualTo("Default_DWA"));
            Assert.That(definition.DistributionType, Is.EqualTo(DryweatherFlowDistributionType.Constant));
            Assert.That(definition.DailyVolumeConstant, Is.EqualTo(240));
            Assert.That(definition.DailyVolumeVariable, Is.EqualTo(120));
            Assert.That(definition.HourlyPercentageDailyVolume, Is.EqualTo(GetDefaultHourlyPercentageDailyVolume()));
        }

        private static double[] GetDefaultHourlyPercentageDailyVolume()
        {
            return new[]
            {
                1.5,
                1.5,
                1.5,
                1.5,
                1.5,
                3.0,
                4.0,
                5.0,
                6.0,
                6.5,
                7.5,
                8.5,
                7.5,
                6.5,
                6.0,
                5.0,
                5.0,
                5.0,
                4.0,
                3.5,
                3.0,
                2.5,
                2.0,
                2.0
            };
        }
    }
}