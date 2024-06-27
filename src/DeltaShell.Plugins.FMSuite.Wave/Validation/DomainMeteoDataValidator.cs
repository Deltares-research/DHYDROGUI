using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Common.Wind;
using DeltaShell.Plugins.FMSuite.Wave.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Validation
{
    /// <summary>
    /// Class to validate <see cref="WaveMeteoData"/>.
    /// </summary>
    public static class DomainMeteoDataValidator
    {
        /// <summary>
        /// Validates the provided <see cref="WaveMeteoData"/>.
        /// </summary>
        /// <param name="meteoData">The <see cref="WaveMeteoData"/> to validate.</param>
        /// <returns>A collection of validation messages. Returns an empty collection if there were no validation messages.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="meteoData"/> is <c>null</c>.</exception>
        /// <exception cref="NotSupportedException">
        /// Thrown when an invalid <see cref="WindDefinitionType"/> is set in the <paramref name="meteoData"/>.
        /// </exception>
        public static IEnumerable<string> Validate(WaveMeteoData meteoData)
        {
            Ensure.NotNull(meteoData, nameof(meteoData));

            var validationMessages = new List<string>();

            switch (meteoData.FileType)
            {
                case WindDefinitionType.WindXY:
                    validationMessages.AddRange(ValidateWindXY(meteoData));
                    break;
                case WindDefinitionType.WindXWindY:
                    validationMessages.AddRange(ValidateWindXWindY(meteoData));
                    break;
                case WindDefinitionType.SpiderWebGrid:
                    validationMessages.AddRange(ValidateSpiderWebGrid(meteoData));
                    break;
                case WindDefinitionType.WindXYP:
                    break;
                default:
                    throw new NotSupportedException();
            }

            return validationMessages;
        }

        private static IEnumerable<string> ValidateWindXY(WaveMeteoData meteoData)
        {
            var validationMessages = new List<string>();

            if (string.IsNullOrWhiteSpace(meteoData.XYVectorFilePath))
            {
                validationMessages.Add(Resources.DomainMeteoDataValidator_Validate_Use_custom_wind_file_option_selected_but_no_file_provided);
            }
            else if (!File.Exists(meteoData.XYVectorFilePath))
            {
                validationMessages.Add(Resources.DomainMeteoDataValidator_Validate_Use_custom_wind_file_option_selected_but_selected_file_does_not_exist);
            }

            if (meteoData.HasSpiderWeb)
            {
                validationMessages.AddRange(ValidateSpiderWebGrid(meteoData));
            }

            return validationMessages;
        }

        private static IEnumerable<string> ValidateWindXWindY(WaveMeteoData meteoData)
        {
            var validationMessages = new List<string>();

            if (string.IsNullOrWhiteSpace(meteoData.XComponentFileName))
            {
                validationMessages.Add(Resources.DomainMeteoDataValidator_Validate_Use_custom_wind_file_option_selected_but_no_x_component_file_provided);
            }
            else if (!File.Exists(meteoData.XComponentFilePath))
            {
                validationMessages.Add(Resources.DomainMeteoDataValidator_Validate_Use_custom_wind_file_option_selected_but_selected_x_component_file_does_not_exist);
            }

            if (string.IsNullOrWhiteSpace(meteoData.YComponentFileName))
            {
                validationMessages.Add(Resources.DomainMeteoDataValidator_Validate_Use_custom_wind_file_option_selected_but_no_y_component_file_provided);
            }
            else if (!File.Exists(meteoData.YComponentFilePath))
            {
                validationMessages.Add(Resources.DomainMeteoDataValidator_Validate_Use_custom_wind_file_option_selected_but_selected_y_component_file_does_not_exist);
            }

            if (meteoData.HasSpiderWeb)
            {
                validationMessages.AddRange(ValidateSpiderWebGrid(meteoData));
            }

            return validationMessages;
        }

        private static IEnumerable<string> ValidateSpiderWebGrid(WaveMeteoData meteoData)
        {
            var validationMessages = new Collection<string>();

            if (string.IsNullOrWhiteSpace(meteoData.SpiderWebFileName))
            {
                validationMessages.Add(Resources.DomainMeteoDataValidator_Validate_Use_spider_web_file_selected_but_no_file_provided);
            }
            else if (!File.Exists(meteoData.SpiderWebFilePath))
            {
                validationMessages.Add(Resources.DomainMeteoDataValidator_Validate_Use_spider_web_file_selected_but_selected_file_does_not_exist);
            }

            return validationMessages;
        }
    }
}