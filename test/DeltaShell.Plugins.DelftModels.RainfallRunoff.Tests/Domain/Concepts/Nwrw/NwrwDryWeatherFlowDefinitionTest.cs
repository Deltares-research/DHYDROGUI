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
                // Call
                dryWeatherFlowDefinition.AddNwrwCatchmentModelDataToModel(model, new NwrwImporterHelper());
                
                // Assert
                IEventedList<NwrwDryWeatherFlowDefinition> definitions = model.NwrwDryWeatherFlowDefinitions;
                Assert.That(definitions.Count, Is.EqualTo(1));
                Assert.That(definitions.Single(), Is.EqualTo(dryWeatherFlowDefinition));
            }
            
        }
    }
}