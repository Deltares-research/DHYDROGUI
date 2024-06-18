using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Validation
{
    /// <summary>
    /// Validator class for a <see cref="LateralDTO"/> that checks for correct user input.
    /// </summary>
    public sealed class LateralValidator
    {
        private readonly LateralFileValidator fileValidator;
        private readonly ILogHandler logHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="LateralValidator"/> class.
        /// </summary>
        /// <param name="logHandler"> The log handler to report user messages with. </param>
        /// <param name="fileSystem"> Provides access to the file system. </param>
        /// <param name="referencePath">
        /// The reference path, which is the external forcing file or MDU file dependent on the
        /// PathsRelativeToParent option in the MDU.
        /// </param>
        /// <param name="parentFilePath"> The external forcing file path. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="logHandler"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="referencePath"/> or <paramref name="parentFilePath"/> is <c>null</c>.
        /// </exception>
        public LateralValidator(ILogHandler logHandler, IFileSystem fileSystem, string referencePath, string parentFilePath)
        {
            Ensure.NotNull(logHandler, nameof(logHandler));
            Ensure.NotNull(fileSystem, nameof(fileSystem));
            Ensure.NotNullOrWhiteSpace(referencePath, nameof(referencePath));
            Ensure.NotNullOrWhiteSpace(parentFilePath, nameof(parentFilePath));

            this.logHandler = logHandler;

            fileValidator = new LateralFileValidator(referencePath, parentFilePath, logHandler, fileSystem);
        }

        /// <summary>
        /// Validates the provided <see cref="LateralDTO"/> object.
        /// Validation rules:
        /// - id must be provided.
        /// - forcing type must be either be provided and supported or not provided at all.
        /// - location type must be either be provided and supported or not provided at all.
        /// - a correct location specification must be provided, which is the following:
        /// - the number of coordinates, the x-coordinates and y-coordinates are provided.
        /// - the correct number of coordinates is either 1 or >2.
        /// - the number of coordinates should correspond with the x-coordinates and y-coordinates.
        /// - if discharge is specified as time series, the time series file must exist.
        /// </summary>
        /// <param name="lateralDTO"> The object to validate. </param>
        /// <returns>
        /// A boolean indication whether or not the validation was successful.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="lateralDTO"/> is <c>null</c>.
        /// </exception>
        public bool Validate(LateralDTO lateralDTO)
        {
            Ensure.NotNull(lateralDTO, nameof(lateralDTO));

            var hasErrors = false;

            if (string.IsNullOrWhiteSpace(lateralDTO.Id))
            {
                ReportError(Resources.Property_id_must_be_provided, lateralDTO.LineNumber);
                hasErrors = true;
            }

            if (lateralDTO.Type == LateralForcingType.Unsupported)
            {
                ReportErrorUnsupportedValue<LateralForcingType>(BndExtForceFileConstants.TypeKey, lateralDTO.LineNumber);
                hasErrors = true;
            }

            if (lateralDTO.LocationType == LateralLocationType.Unsupported)
            {
                ReportErrorUnsupportedValue<LateralLocationType>(BndExtForceFileConstants.LocationTypeKey, lateralDTO.LineNumber);
                hasErrors = true;
            }

            if (!HasCompleteLocationSpecification(lateralDTO))
            {
                ReportError(Resources.Properties_numCoordinates_xCoordinates_yCoordinates_must_be_provided, lateralDTO.LineNumber);
                hasErrors = true;
            }

            else if (!ValidateCoordinates(lateralDTO, lateralDTO.LineNumber))
            {
                hasErrors = true;
            }

            if (!fileValidator.Validate(lateralDTO))
            {
                hasErrors = true;
            }

            return !hasErrors;
        }

        private bool ValidateCoordinates(LateralDTO lateralDTO, int lineNumber)
        {
            var hasErrors = false;

            if (lateralDTO.NumCoordinates < 1 || lateralDTO.NumCoordinates == 2)
            {
                ReportError(Resources.Property_numCoordinates_must_either_be_1_point_or_any_value_greater_than_2_polygon, lineNumber);
                hasErrors = true;
            }

            if (HasCorrectCoordinatesCount(lateralDTO, lateralDTO.XCoordinates))
            {
                ReportErrorCoordinatesCount(BndExtForceFileConstants.XCoordinatesKey, lineNumber);
                hasErrors = true;
            }

            if (HasCorrectCoordinatesCount(lateralDTO, lateralDTO.YCoordinates))
            {
                ReportErrorCoordinatesCount(BndExtForceFileConstants.YCoordinatesKey, lineNumber);
                hasErrors = true;
            }

            return !hasErrors;
        }

        private void ReportErrorUnsupportedValue<TEnum>(string propertyName, int lineNumber) where TEnum : Enum
        {
            string supportedValuesStr = GetSupportedEnumValuesString<TEnum>();
            ReportError(string.Format(Resources.Property_0_contains_an_unsupported_value_Supported_values_1_, propertyName, supportedValuesStr), lineNumber);
        }

        private static string GetSupportedEnumValuesString<TEnum>() where TEnum : Enum
        {
            IEnumerable<TEnum> values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
            IEnumerable<string> descriptions = values.Select(v => v.GetDescription()).Where(d => !string.IsNullOrEmpty(d));
            return string.Join(", ", descriptions);
        }

        private void ReportErrorCoordinatesCount(string propertyName, int lineNumber)
        {
            ReportError(string.Format(Resources.The_number_of_values_of_property_0_must_be_equal_to_the_value_of_property_numCoordinates_, propertyName), lineNumber);
        }

        private static bool HasCorrectCoordinatesCount(LateralDTO lateralDTO, IEnumerable<double> coordinates)
        {
            return lateralDTO.NumCoordinates != coordinates.Count();
        }

        private void ReportError(string message, int lineNumber)
        {
            logHandler.ReportError(string.Format(Resources._0_Line_1_, message, lineNumber));
        }

        private static bool HasCompleteLocationSpecification(LateralDTO lateralDTO)
        {
            return lateralDTO.NumCoordinates != null &&
                   lateralDTO.XCoordinates != null &&
                   lateralDTO.YCoordinates != null;
        }
    }
}