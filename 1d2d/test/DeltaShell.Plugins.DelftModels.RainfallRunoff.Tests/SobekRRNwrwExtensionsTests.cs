using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Sobek.Readers.Properties;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    public class SobekRRNwrwExtensionsTests
    {

        [Test]
        public void GivenCustomNwrwSurfaceDataWhenUpdatingNwrwSettingsThenValuesAreUpdatedCorrectlyInNwrwSettingsObject()
        {
            // arrange
            var line = "PLVG id '-1' rf 0.5 0.2 0.1 0.5 0.2 0.1 0.5 0.2 0.1 0.5 0.2 0.1 ms 0 0.5 1 0 0.5 1 0 2 4 2 4 6 ix 0 2 0 5 im 0 0.5 0 1 ic 0 3 0 3 dc 0 0.1 0 0.1 od 1 or 0 plvg";

            var sobekRRNwrwSettings = new SobekRRNwrwSettingsReader().Parse(line).ToArray();
            var nwrwSettings = NwrwDefinition.CreateDefaultNwrwDefinitions().ToArray();
            ILogHandler logHandler = Substitute.For<ILogHandler>();

            // act
            sobekRRNwrwSettings.UpdateNwrwSettings(nwrwSettings,logHandler);

            // assert
            Assert.That(nwrwSettings.Last().SurfaceStorage, Is.EqualTo(6));
        }
        
        [Test]
        public void GivenCustomNwrwSurfaceDataWithOldRuWhenUpdatingNwrwSettingsThenValuesAreUpdatedCorrectlyInNwrwSettingsObject()
        {
            // arrange
            var line = "PLVG id '-1' ru 0.5 0.2 0.1 ms 0 0.5 1 0 0.5 1 0 2 4 2 4 6 ix 0 2 0 5 im 0 0.5 0 1 ic 0 3 0 3 dc 0 0.1 0 0.1 od 0 or 1 plvg";

            var sobekRRNwrwSettings = new SobekRRNwrwSettingsReader().Parse(line).ToArray();
            var nwrwSettings = NwrwDefinition.CreateDefaultNwrwDefinitions().ToArray();
            ILogHandler logHandler = Substitute.For<ILogHandler>();

            // act
            sobekRRNwrwSettings.UpdateNwrwSettings(nwrwSettings,logHandler);

            // assert
            Assert.That(nwrwSettings.Last().SurfaceStorage, Is.EqualTo(6));
        }
        
        [Test]
        public void GivenEmptyNwrwSettingsWhenUpdateNwrwSettingsThenLogErrorIsTriggered()
        {
            // arrange
            ISobekRRNwrwSettings sobekRRNwrwSetting = Substitute.For<ISobekRRNwrwSettings>();
            ISobekRRNwrwSettings[] sobekRRNwrwSettings = { sobekRRNwrwSetting };
            ICollection<NwrwDefinition> nwrwSettings = Substitute.For<ICollection<NwrwDefinition>>();
            ILogHandler logHandler = Substitute.For<ILogHandler>();

            // act
            sobekRRNwrwSettings.UpdateNwrwSettings(nwrwSettings, logHandler);
            
            // assert
            logHandler.Received().ReportError(Properties.Resources.SobekRRNwrwSettingsExtensions_UpdateNwrwSettings_Nwrw_Definitions_in_RR_model_are_not_configured_as_expected__Cannot_load_default_data_in_unexpected_configured_NWRW_surface_settings_object);
        }
        
        [Test]
        public void GivenIncorrectNwrwSettingsWhenUpdateNwrwSettingsThenArgumentException()
        {
            // arrange
            ISobekRRNwrwSettings sobekRRNwrwSetting = Substitute.For<ISobekRRNwrwSettings>();
            ISobekRRNwrwSettings[] sobekRRNwrwSettings = { sobekRRNwrwSetting };
            ICollection<NwrwDefinition> nwrwSettings = NwrwDefinition.CreateDefaultNwrwDefinitions();
            nwrwSettings.Remove(nwrwSettings.Last());
            ILogHandler logHandler = Substitute.For<ILogHandler>();

            // act
            sobekRRNwrwSettings.UpdateNwrwSettings(nwrwSettings, logHandler);

            // assert
            logHandler.Received().ReportError(Properties.Resources.SobekRRNwrwSettingsExtensions_UpdateNwrwSettings_Nwrw_Definitions_in_RR_model_are_not_configured_as_expected__Cannot_load_default_data_in_unexpected_configured_NWRW_surface_settings_object);
        }
        
        [Test]
        public void GivenCorrectNwrwSettingsButNoReadDataWhenUpdateNwrwSettingsThenLogWarningNoUpdatePossibleIsTriggered()
        {
            // arrange
            ISobekRRNwrwSettings[] sobekRRNwrwSettings = Array.Empty<ISobekRRNwrwSettings>();
            ICollection<NwrwDefinition> nwrwSettings = NwrwDefinition.CreateDefaultNwrwDefinitions();
            ILogHandler logHandler = Substitute.For<ILogHandler>();

            // act
            sobekRRNwrwSettings.UpdateNwrwSettings(nwrwSettings, logHandler);

            // assert
            logHandler.Received().ReportWarning(Properties.Resources.SobekRRNwrwSettingsExtensions_UpdateNwrwSettings_No_nwrw_settings_were_found);
        }
        
        [Test]
        public void GivenCorrectNwrwSettingsButMultipleReadDataWhenUpdateNwrwSettingsThenCheckLogWarningAreTriggered()
        {
            // arrange
            ISobekRRNwrwSettings sobekRRNwrwSetting = Substitute.For<ISobekRRNwrwSettings>();
            ISobekRRNwrwSettings[] sobekRRNwrwSettings = { sobekRRNwrwSetting, sobekRRNwrwSetting };
            ICollection<NwrwDefinition> nwrwSettings = NwrwDefinition.CreateDefaultNwrwDefinitions();
            ILogHandler logHandler = Substitute.For<ILogHandler>();

            // act
            sobekRRNwrwSettings.UpdateNwrwSettings(nwrwSettings, logHandler);

            // assert
            logHandler.Received().ReportWarning(Properties.Resources.SobekRRNwrwSettingsExtensions_UpdateNwrwSettings_Found_multiple_nwrw_settings__Importing_the_first_settings_and_ignoring_the_others);
            
            logHandler.Received().ReportWarning(Properties.Resources.SobekRRNwrwSettingsExtensions_UpdateRunoffDelayFactors_Could_not_find_any_runoff_factors);
            logHandler.Received().ReportWarning(Properties.Resources.SobekRRNwrwSettingsExtensions_UpdateMaximumStorages_No_settings_found_for_maximum_storages);
            logHandler.Received().ReportWarning(Properties.Resources.SobekRRNwrwSettingsExtensions_UpdateMaximumInfiltrationCapacities_No_settings_found_for_maximum_infiltration_capacities);
            logHandler.Received().ReportWarning(Properties.Resources.SobekRRNwrwSettingsExtensions_UpdateMinimumInfiltrationCapacities_No_settings_found_for_minimum_infiltration_capacities);
            logHandler.Received().ReportWarning(Properties.Resources.SobekRRNwrwSettingsExtensions_UpdateInfiltrationCapacityDecrease_No_settings_found_for_infiltration_capacity_reduction);
            logHandler.Received().ReportWarning(Properties.Resources.SobekRRNwrwSettingsExtensions_UpdateInfiltrationCapacityIncrease_No_settings_found_for_infiltration_capacity_recovery);
        }
    }
}