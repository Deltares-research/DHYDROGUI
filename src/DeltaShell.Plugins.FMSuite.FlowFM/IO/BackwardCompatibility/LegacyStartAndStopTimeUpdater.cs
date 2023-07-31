using System;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Common;
using DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.Extensions;
using DHYDRO.Common.Logging;
using CommonResources = DeltaShell.Plugins.FMSuite.Common.Properties.Resources;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.BackwardCompatibility
{
    /// <summary>
    /// Class responsible for updating the legacy TStart and TStop mdu properties to their latest version.
    /// </summary>
    public class LegacyStartAndStopTimeUpdater : IPropertyUpdater
    {
        public void UpdateProperty(DelftIniProperty legacyProperty,
                                   string newPropertyName,
                                   DelftIniCategory legacyPropertyCategory,
                                   ILogHandler logHandler)
        {
            Ensure.NotNull(legacyProperty, nameof(legacyProperty));
            Ensure.NotNull(newPropertyName, nameof(newPropertyName));
            Ensure.NotNull(legacyPropertyCategory, nameof(legacyPropertyCategory));
            Ensure.NotNull(logHandler, nameof(logHandler));

            if (!IsValidLegacyPropertyForThisUpdater(legacyProperty))
            {
                return;
            }

            EnsureRequiredPropertiesForUpdatingLegacyPropertyArePresent(legacyPropertyCategory);

            UpdateLegacyProperty(legacyProperty, newPropertyName, legacyPropertyCategory, logHandler);
        }

        private static bool IsValidLegacyPropertyForThisUpdater(DelftIniProperty legacyProperty)
        {
            return legacyProperty.Name.EqualsCaseInsensitive(KnownLegacyProperties.TStart)
                   || legacyProperty.Name.EqualsCaseInsensitive(KnownLegacyProperties.TStop);
        }

        private static void EnsureRequiredPropertiesForUpdatingLegacyPropertyArePresent(DelftIniCategory legacyPropertyCategory)
        {
            EnsureRefDateIsPresentInCategoryAndHasValue(legacyPropertyCategory);
            EnsureTUnitIsPresentInCategoryAndHasValueOrUseDefault(legacyPropertyCategory);
        }

        private static void EnsureRefDateIsPresentInCategoryAndHasValue(DelftIniCategory legacyPropertyCategory)
        {
            EnsurePropertyIsPresentInCategory(legacyPropertyCategory, KnownProperties.RefDate);
            EnsurePropertyHasValue(legacyPropertyCategory, KnownProperties.RefDate);
        }

        private static DelftIniProperty EnsurePropertyIsPresentInCategory(DelftIniCategory legacyPropertyCategory, string propertyName)
        {
            DelftIniProperty requiredProperty = legacyPropertyCategory.Properties.FirstOrDefault(property => property.Name.EqualsCaseInsensitive(propertyName));
            if (requiredProperty is null)
            {
                throw new InvalidOperationException(string.Format(Resources.PropertyUpdater_Required_keyword_0_is_missing, propertyName));
            }

            return requiredProperty;
        }

        private static void EnsurePropertyHasValue(DelftIniCategory legacyPropertyCategory, string propertyName)
        {
            string requiredPropertyValue = GetPropertyValue(propertyName, legacyPropertyCategory);
            if (string.IsNullOrWhiteSpace(requiredPropertyValue))
            {
                throw new InvalidOperationException(string.Format(Resources.PropertyUpdater_Required_value_for_keyword_0_is_missing, propertyName));
            }
        }

        private static void EnsureTUnitIsPresentInCategoryAndHasValueOrUseDefault(DelftIniCategory legacyPropertyCategory)
        {
            DelftIniProperty requiredProperty = EnsurePropertyIsPresentInCategory(legacyPropertyCategory, KnownProperties.Tunit);

            string requiredPropertyValue = GetPropertyValue(KnownProperties.Tunit, legacyPropertyCategory);
            if (requiredPropertyValue is null)
            {
                const string defaultTUnitValue = "S";
                requiredProperty.Value = defaultTUnitValue;
            }
        }

        private static void UpdateLegacyProperty(DelftIniProperty legacyProperty,
                                                 string newPropertyName,
                                                 DelftIniCategory legacyPropertyCategory,
                                                 ILogHandler logHandler)
        {
            UpdatePropertyName(legacyProperty, newPropertyName, logHandler);
            UpdatePropertyValue(legacyProperty, legacyPropertyCategory, logHandler);
        }

        private static void UpdatePropertyName(DelftIniProperty legacyProperty, string newPropertyName, ILogHandler logHandler)
        {
            LogWarningAboutUpdatedName(legacyProperty, newPropertyName, logHandler);

            legacyProperty.Name = newPropertyName;
        }

        private static void UpdatePropertyValue(DelftIniProperty legacyProperty, DelftIniCategory legacyPropertyCategory, ILogHandler logHandler)
        {
            string tUnit = GetTUnitFromCategory(legacyPropertyCategory);
            string refDateAsString = GetRefDateFromCategory(legacyPropertyCategory);

            string newValue = GetNewValue(legacyProperty, refDateAsString, tUnit);

            ReportWarningAboutUpdatedValue(legacyProperty, newValue, logHandler);

            legacyProperty.Value = newValue;
        }

        private static string GetTUnitFromCategory(DelftIniCategory legacyPropertyCategory)
        {
            return GetPropertyValue(KnownProperties.Tunit, legacyPropertyCategory);
        }

        private static string GetRefDateFromCategory(DelftIniCategory legacyPropertyCategory)
        {
            return GetPropertyValue(KnownProperties.RefDate, legacyPropertyCategory);
        }

        private static string GetPropertyValue(string propertyName, DelftIniCategory legacyPropertyCategory)
        {
            return legacyPropertyCategory.Properties
                                         .FirstOrDefault(property => property.Name.EqualsCaseInsensitive(propertyName))?
                                         .Value;
        }

        private static string GetNewValue(DelftIniProperty legacyProperty, string refDateAsString, string tUnit)
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

        private static void LogWarningAboutUpdatedName(DelftIniProperty legacyProperty, string newPropertyName, ILogHandler logHandler)
        {
            logHandler.ReportWarningFormat(
                CommonResources.DelftIniBackwardsCompatibilityHelper_GetUpdatedName_Backwards_Compatibility____0___has_been_updated_to___1__,
                legacyProperty.Name,
                newPropertyName);
        }

        private static void ReportWarningAboutUpdatedValue(DelftIniProperty legacyProperty, string newValue, ILogHandler logHandler)
        {
            logHandler.ReportWarningFormat(
                CommonResources.DelftIniBackwardsCompatibilityHelper_Value_for_0_has_been_updated_from_1_to_2,
                legacyProperty.Name,
                legacyProperty.Value,
                newValue);
        }
    }
}