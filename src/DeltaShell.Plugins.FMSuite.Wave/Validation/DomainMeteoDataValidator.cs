using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Common.Wind;

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
        /// <returns>A collection of error messages. Returns an empty collection if there were no validation errors.</returns>
        /// <exception cref="NotSupportedException">
        /// Thrown when an invalid <see cref="WindDefinitionType"/> is set in the <paramref name="meteoData"/>.
        /// </exception>
        public static IEnumerable<string> Validate(WaveMeteoData meteoData)
        {
            Ensure.NotNull(meteoData, nameof(meteoData));

            var errorMessages = new List<string>();

            switch (meteoData.FileType)
            {
                case WindDefinitionType.WindXY:
                    errorMessages.AddRange(ValidateWindXY(meteoData));
                    break;
                case WindDefinitionType.WindXWindY:
                    errorMessages.AddRange(ValidateWindXWindY(meteoData));
                    break;
                case WindDefinitionType.SpiderWebGrid:
                    errorMessages.AddRange(ValidateSpiderWebGrid(meteoData));
                    break;
                case WindDefinitionType.WindXYP:
                    break;
                default:
                    throw new NotSupportedException();
            }

            return errorMessages;
        }

        private static IEnumerable<string> ValidateWindXY(WaveMeteoData meteoData)
        {
            var errorMessages = new Collection<string>();

            if (string.IsNullOrWhiteSpace(meteoData.XYVectorFilePath))
            {
                errorMessages.Add("Missing xy file.");
            }
            else if (!File.Exists(meteoData.XYVectorFilePath))
            {
                errorMessages.Add("XY file does not exist.");
            }

            if (!meteoData.HasSpiderWeb)
            {
                return errorMessages;
            }

            if (string.IsNullOrWhiteSpace(meteoData.SpiderWebFileName))
            {
                errorMessages.Add("Missing spiderweb file");
            }
            else if (!File.Exists(meteoData.SpiderWebFilePath))
            {
                errorMessages.Add("Spiderweb file does not exist.");
            }

            return errorMessages;
        }

        private static IEnumerable<string> ValidateWindXWindY(WaveMeteoData meteoData)
        {
            var errorMessages = new Collection<string>();

            if (string.IsNullOrWhiteSpace(meteoData.XComponentFileName))
            {
                errorMessages.Add("Missing x file.");
            }
            else if (!File.Exists(meteoData.XComponentFilePath))
            {
                errorMessages.Add("X file does not exist.");
            }

            if (string.IsNullOrWhiteSpace(meteoData.YComponentFileName))
            {
                errorMessages.Add("Missing y file.");
            }
            else if (!File.Exists(meteoData.YComponentFilePath))
            {
                errorMessages.Add("Y file does not exist.");
            }

            if (!meteoData.HasSpiderWeb)
            {
                return errorMessages;
            }

            if (string.IsNullOrWhiteSpace(meteoData.SpiderWebFileName))
            {
                errorMessages.Add("Missing spiderweb file");
            }
            else if (!File.Exists(meteoData.SpiderWebFilePath))
            {
                errorMessages.Add("Spiderweb file does not exist.");
            }

            return errorMessages;
        }

        private static IEnumerable<string> ValidateSpiderWebGrid(WaveMeteoData meteoData)
        {
            var errorMessages = new Collection<string>();

            if (string.IsNullOrWhiteSpace(meteoData.SpiderWebFileName))
            {
                errorMessages.Add("Missing spiderweb file");
            }
            else if (!File.Exists(meteoData.SpiderWebFilePath))
            {
                errorMessages.Add("Spiderweb file does not exist.");
            }

            return errorMessages;
        }
    }
}