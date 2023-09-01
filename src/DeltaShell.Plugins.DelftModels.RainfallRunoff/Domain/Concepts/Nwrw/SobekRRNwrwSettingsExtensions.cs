using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Properties;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public static class SobekRRNwrwSettingsExtensions
    {
        public static void UpdateNwrwSettings(this ISobekRRNwrwSettings[] readNwrwSettings, ICollection<NwrwDefinition> nwrwDefinitions, ILogHandler logHandler)
        {
            const int nwrwNumberOfAreaTypes = 12;
            // (12 types) as combination of 3 kind of slopes(with a slope, flat, flat stretched)
            // and 4 types of surfaces(closed paved, open paved, roofs, unpaved)
            // see SOBEK 2 User Manual => chapter D.19.6 : NWRW Layer
            if (nwrwDefinitions.Count != nwrwNumberOfAreaTypes)
            {
                logHandler.ReportError(Resources.SobekRRNwrwSettingsExtensions_UpdateNwrwSettings_Nwrw_Definitions_in_RR_model_are_not_configured_as_expected__Cannot_load_default_data_in_unexpected_configured_NWRW_surface_settings_object);
                return;
            }
            ISobekRRNwrwSettings readNwrwSetting = readNwrwSettings.FirstOrDefault();
            if (readNwrwSetting == null)
            {
                logHandler.ReportWarning(Resources.SobekRRNwrwSettingsExtensions_UpdateNwrwSettings_No_nwrw_settings_were_found);
                return;
            }

            if (readNwrwSettings.Length > 1)
            {
                logHandler.ReportWarning(Resources.SobekRRNwrwSettingsExtensions_UpdateNwrwSettings_Found_multiple_nwrw_settings__Importing_the_first_settings_and_ignoring_the_others);
            }

            NwrwDefinition[] nwrwDefinitionArray = nwrwDefinitions.ToArray();

            UpdateRunoffDelayFactors(nwrwDefinitionArray, readNwrwSetting, logHandler);
            UpdateMaximumStorages(nwrwDefinitionArray, readNwrwSetting, logHandler);
            UpdateMaximumInfiltrationCapacities(nwrwDefinitionArray, readNwrwSetting, logHandler);
            UpdateMinimumInfiltrationCapacities(nwrwDefinitionArray, readNwrwSetting, logHandler);
            UpdateInfiltrationCapacityDecrease(nwrwDefinitionArray, readNwrwSetting, logHandler);
            UpdateInfiltrationCapacityIncrease(nwrwDefinitionArray, readNwrwSetting, logHandler);
        }

        private static void UpdateRunoffDelayFactors(NwrwDefinition[] nwrwDefinitions, ISobekRRNwrwSettings readSettings, ILogHandler logHandler)
        {
            if (readSettings.RunoffDelayFactors == null || readSettings.RunoffDelayFactors.Length == 0)
            {
                logHandler.ReportWarning(Resources.SobekRRNwrwSettingsExtensions_UpdateRunoffDelayFactors_Could_not_find_any_runoff_factors);
                return;
            }

            if (readSettings.IsOldFormatData)
            {
                for (var i = 0; i < readSettings.RunoffDelayFactors.Length; i++)
                {
                    nwrwDefinitions[i].RunoffDelay = readSettings.RunoffDelayFactors[i];
                    nwrwDefinitions[i + 3].RunoffDelay = readSettings.RunoffDelayFactors[i];
                    nwrwDefinitions[i + 6].RunoffDelay = readSettings.RunoffDelayFactors[i];
                    nwrwDefinitions[i + 9].RunoffDelay = readSettings.RunoffDelayFactors[i];
                }
            }
            else
            {
                for (var i = 0; i < readSettings.RunoffDelayFactors.Length; i++)
                {
                    nwrwDefinitions[i].RunoffDelay = readSettings.RunoffDelayFactors[i];
                }
            }
        }

        private static void UpdateMaximumStorages(NwrwDefinition[] nwrwDefinitionArray, ISobekRRNwrwSettings readSettings, ILogHandler logHandler)
        {
            if (readSettings.MaximumStorages == null || readSettings.MaximumStorages.Length == 0)
            {
                logHandler.ReportWarning(Resources.SobekRRNwrwSettingsExtensions_UpdateMaximumStorages_No_settings_found_for_maximum_storages);
                return;
            }

            for (var i = 0; i < readSettings.MaximumStorages.Length; i++)
            {
                nwrwDefinitionArray[i].SurfaceStorage = readSettings.MaximumStorages[i];
            }
        }

        private static void UpdateMaximumInfiltrationCapacities(NwrwDefinition[] nwrwDefinitionArray, ISobekRRNwrwSettings readSettings, ILogHandler logHandler)
        {
            if (readSettings.MaximumInfiltrationCapacities == null || readSettings.MaximumInfiltrationCapacities.Length == 0)
            {
                logHandler.ReportWarning(Resources.SobekRRNwrwSettingsExtensions_UpdateMaximumInfiltrationCapacities_No_settings_found_for_maximum_infiltration_capacities);
                return;
            }

            for (var i = 0; i < readSettings.MaximumInfiltrationCapacities.Length - 1; i++)
            {
                nwrwDefinitionArray[i].InfiltrationCapacityMax = readSettings.MaximumInfiltrationCapacities[0];
                nwrwDefinitionArray[i + 3].InfiltrationCapacityMax = readSettings.MaximumInfiltrationCapacities[1];
                nwrwDefinitionArray[i + 6].InfiltrationCapacityMax = readSettings.MaximumInfiltrationCapacities[2];
                nwrwDefinitionArray[i + 9].InfiltrationCapacityMax = readSettings.MaximumInfiltrationCapacities[3];
            }
        }

        private static void UpdateMinimumInfiltrationCapacities(NwrwDefinition[] nwrwDefinitionArray, ISobekRRNwrwSettings readSettings, ILogHandler logHandler)
        {
            if (readSettings.MinimumInfiltrationCapacities == null || readSettings.MinimumInfiltrationCapacities.Length == 0)
            {
                logHandler.ReportWarning(Resources.SobekRRNwrwSettingsExtensions_UpdateMinimumInfiltrationCapacities_No_settings_found_for_minimum_infiltration_capacities);
                return;
            }

            for (var i = 0; i < readSettings.MinimumInfiltrationCapacities.Length - 1; i++)
            {
                nwrwDefinitionArray[i].InfiltrationCapacityMin = readSettings.MinimumInfiltrationCapacities[0];
                nwrwDefinitionArray[i + 3].InfiltrationCapacityMin = readSettings.MinimumInfiltrationCapacities[1];
                nwrwDefinitionArray[i + 6].InfiltrationCapacityMin = readSettings.MinimumInfiltrationCapacities[2];
                nwrwDefinitionArray[i + 9].InfiltrationCapacityMin = readSettings.MinimumInfiltrationCapacities[3];
            }
        }

        private static void UpdateInfiltrationCapacityDecrease(NwrwDefinition[] nwrwDefinitionArray, ISobekRRNwrwSettings readSettings, ILogHandler logHandler)
        {
            if (readSettings.InfiltrationCapacityDecreases == null || readSettings.InfiltrationCapacityDecreases.Length == 0)
            {
                logHandler.ReportWarning(Resources.SobekRRNwrwSettingsExtensions_UpdateInfiltrationCapacityDecrease_No_settings_found_for_infiltration_capacity_reduction);
                return;
            }

            for (var i = 0; i < readSettings.InfiltrationCapacityDecreases.Length - 1; i++)
            {
                nwrwDefinitionArray[i].InfiltrationCapacityReduction = readSettings.InfiltrationCapacityDecreases[0];
                nwrwDefinitionArray[i + 3].InfiltrationCapacityReduction = readSettings.InfiltrationCapacityDecreases[1];
                nwrwDefinitionArray[i + 6].InfiltrationCapacityReduction = readSettings.InfiltrationCapacityDecreases[2];
                nwrwDefinitionArray[i + 9].InfiltrationCapacityReduction = readSettings.InfiltrationCapacityDecreases[3];
            }
        }

        private static void UpdateInfiltrationCapacityIncrease(NwrwDefinition[] nwrwDefinitionArray, ISobekRRNwrwSettings readSettings, ILogHandler logHandler)
        {
            if (readSettings.InfiltrationCapacityIncreases == null || readSettings.InfiltrationCapacityIncreases.Length == 0)
            {
                logHandler.ReportWarning(Resources.SobekRRNwrwSettingsExtensions_UpdateInfiltrationCapacityIncrease_No_settings_found_for_infiltration_capacity_recovery);
                return;
            }

            for (var i = 0; i < readSettings.InfiltrationCapacityIncreases.Length - 1; i++)
            {
                nwrwDefinitionArray[i].InfiltrationCapacityRecovery = readSettings.InfiltrationCapacityIncreases[0];
                nwrwDefinitionArray[i + 3].InfiltrationCapacityRecovery = readSettings.InfiltrationCapacityIncreases[1];
                nwrwDefinitionArray[i + 6].InfiltrationCapacityRecovery = readSettings.InfiltrationCapacityIncreases[2];
                nwrwDefinitionArray[i + 9].InfiltrationCapacityRecovery = readSettings.InfiltrationCapacityIncreases[3];
            }
        }
    }
}