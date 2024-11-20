using System;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.API.Logging;
using Deltares.Infrastructure.Extensions;
using Deltares.Infrastructure.IO.Ini;
using Deltares.Infrastructure.IO.Ini.BackwardCompatibility;
using DeltaShell.Plugins.FMSuite.Common;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.BackwardCompatibility
{
    /// <summary>
    /// Class responsible for updating the legacy TStart and TStop mdu properties to their latest version.
    /// </summary>
    public class LegacyStartAndStopTimeUpdater : IPropertyUpdater
    {
        public void UpdateProperty(string oldPropertyKey,
                                   string newPropertyKey,
                                   IniSection section,
                                   ILogHandler logHandler)
        {
            Ensure.NotNull(oldPropertyKey, nameof(oldPropertyKey));
            Ensure.NotNull(newPropertyKey, nameof(newPropertyKey));
            Ensure.NotNull(section, nameof(section));
            Ensure.NotNull(logHandler, nameof(logHandler));

            if (!IsValidLegacyPropertyForThisUpdater(oldPropertyKey))
            {
                return;
            }

            EnsureRequiredPropertiesForUpdatingLegacyPropertyArePresent(section, oldPropertyKey);

            UpdateLegacyProperty(oldPropertyKey, newPropertyKey, section, logHandler);
        }

        private static bool IsValidLegacyPropertyForThisUpdater(string legacyPropertyKey)
        {
            return legacyPropertyKey.EqualsCaseInsensitive(KnownLegacyProperties.TStart) || 
                   legacyPropertyKey.EqualsCaseInsensitive(KnownLegacyProperties.TStop);
        }

        private static void EnsureRequiredPropertiesForUpdatingLegacyPropertyArePresent(IniSection section, string oldPropertyKey)
        {
            EnsurePropertyIsPresentInSection(section, oldPropertyKey);
            EnsureRefDateIsPresentInSectionAndHasValue(section);
            EnsureTUnitIsPresentInSectionAndHasValueOrUseDefault(section);
        }

        private static void EnsureRefDateIsPresentInSectionAndHasValue(IniSection section)
        {
            EnsurePropertyIsPresentInSection(section, KnownProperties.RefDate);
            EnsurePropertyHasValue(section, KnownProperties.RefDate);
        }

        private static IniProperty EnsurePropertyIsPresentInSection(IniSection section, string propertyKey)
        {
            IniProperty requiredProperty = section.FindProperty(propertyKey);
            if (requiredProperty is null)
            {
                throw new InvalidOperationException(string.Format(Resources.PropertyUpdater_Required_keyword_0_is_missing, propertyKey));
            }

            return requiredProperty;
        }

        private static void EnsurePropertyHasValue(IniSection section, string propertyKey)
        {
            string requiredPropertyValue = GetPropertyValue(propertyKey, section);
            if (string.IsNullOrWhiteSpace(requiredPropertyValue))
            {
                throw new InvalidOperationException(string.Format(Resources.PropertyUpdater_Required_value_for_keyword_0_is_missing, propertyKey));
            }
        }

        private static void EnsureTUnitIsPresentInSectionAndHasValueOrUseDefault(IniSection section)
        {
            IniProperty requiredProperty = EnsurePropertyIsPresentInSection(section, KnownProperties.Tunit);

            string requiredPropertyValue = GetPropertyValue(KnownProperties.Tunit, section);
            if (requiredPropertyValue is null)
            {
                const string defaultTUnitValue = "S";
                requiredProperty.Value = defaultTUnitValue;
            }
        }

        private static void UpdateLegacyProperty(string oldPropertyKey,
                                                 string newPropertyKey,
                                                 IniSection section,
                                                 ILogHandler logHandler)
        {
            UpdatePropertyKey(oldPropertyKey, newPropertyKey, section, logHandler);
            UpdatePropertyValue(newPropertyKey, section, logHandler);
        }

        private static void UpdatePropertyKey(string oldPropertyKey, string newPropertyKey, IniSection section, ILogHandler logHandler)
        {
            LogWarningAboutUpdatedKey(oldPropertyKey, newPropertyKey, logHandler);

            section.RenameProperties(oldPropertyKey, newPropertyKey);
        }

        private static void UpdatePropertyValue(string newPropertyKey, IniSection section, ILogHandler logHandler)
        {
            IniProperty legacyProperty = section.FindProperty(newPropertyKey);
            
            string tUnit = GetTUnitFromSection(section);
            string refDateAsString = GetRefDateFromSection(section);

            string newValue = GetNewValue(legacyProperty, refDateAsString, tUnit);

            ReportWarningAboutUpdatedValue(legacyProperty, newValue, logHandler);

            legacyProperty.Value = newValue;
        }

        private static string GetTUnitFromSection(IniSection section)
        {
            return GetPropertyValue(KnownProperties.Tunit, section);
        }

        private static string GetRefDateFromSection(IniSection section)
        {
            return GetPropertyValue(KnownProperties.RefDate, section);
        }

        private static string GetPropertyValue(string propertyKey, IniSection section)
        {
            return section.GetPropertyValue(propertyKey);
        }

        private static string GetNewValue(IniProperty legacyProperty, string refDateAsString, string tUnit)
        {
            var refDate = FMParser.FromString<DateOnly>(refDateAsString);
            var offset = FMParser.FromString<double>(legacyProperty.Value);

            return CalculateNewValueAsString(tUnit, offset, refDate);
        }

        private static string CalculateNewValueAsString(string tUnit, double offset, DateOnly refDate)
        {
            double timeUnitInSeconds = 1;
            string tUnitLower = tUnit.ToLower();

            switch (tUnitLower)
            {
                case "m":
                    timeUnitInSeconds = 60d;
                    break;
                case "h":
                    timeUnitInSeconds = 3600d;
                    break;
            }

            var ticks = (long)(TimeSpan.TicksPerSecond * offset * timeUnitInSeconds);
            
            DateTime newValue = refDate.ToDateTime(TimeOnly.MinValue)
                                       .AddTicks(ticks);

            return FMParser.ToString(newValue, typeof(DateTime));
        }

        private static void LogWarningAboutUpdatedKey(string oldPropertyKey, string newPropertyKey, ILogHandler logHandler)
        {
            logHandler.ReportWarningFormat(
                Resources.Backwards_Compatibility____0___has_been_updated_to___1__,
                oldPropertyKey,
                newPropertyKey);
        }

        private static void ReportWarningAboutUpdatedValue(IniProperty legacyProperty, string newValue, ILogHandler logHandler)
        {
            logHandler.ReportWarningFormat(
                Resources.Backwards_Compatibility__Value_for___0___has_been_updated_from___1___to___2__,
                legacyProperty.Key,
                legacyProperty.Value,
                newValue);
        }
    }
}