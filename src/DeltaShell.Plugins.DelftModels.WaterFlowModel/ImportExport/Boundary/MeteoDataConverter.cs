using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters;


namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Boundary
{
    /// <summary>
    /// MeteoDataConverter is responsible for extracting a MeteoFunction from a
    /// BoundaryConditions data access model.
    /// </summary>
    public static class MeteoDataConverter
    {
        /// <summary>
        /// Convert the specified dataAccessmodel to a new MeteoFunction if the
        /// data access model is valid, return null otherwise.
        ///
        /// Any errors will be logged in errorMessages.
        /// </summary>
        /// <param name="dataAccessModel">The data access model describing the MeteoFunction</param>
        /// <param name="errorMessages">List of error messages to which new messages will be added</param>
        /// <returns>
        /// If the <paramref name="dataAccessModel"/> describes a valid MeteoFunction,
        /// then the corresponding <see cref="MeteoFunction"/>, else null.
        /// </returns>
        public static MeteoFunction Convert(IList<IDelftBcCategory> dataAccessModel,
                                            IList<string> errorMessages)
        {
            if (!Validate(dataAccessModel, errorMessages))
                return null;

            var relevantCategories = dataAccessModel.Where(IsMeteoFunctionAttribute).ToList();
            return Parse(relevantCategories, errorMessages);
        }


        private static bool IsMeteoFunctionAttribute(IDelftBcCategory category)
        {
            return category.Name.Equals(BoundaryRegion.BcBoundaryHeader) &&
                   category.ReadProperty<string>(BoundaryRegion.Name.Key).Equals(FunctionAttributes.StandardFeatureNames.ModelWide) &&
                   (category.Table[1].Quantity.Value.Equals(BoundaryRegion.QuantityStrings.MeteoDataAirTemperature) ||
                    category.Table[1].Quantity.Value.Equals(BoundaryRegion.QuantityStrings.MeteoDataHumidity) ||
                    category.Table[1].Quantity.Value.Equals(BoundaryRegion.QuantityStrings.MeteoDataCloudiness));
        }

        /// <summary>
        /// Validate the provided dataAccessModel whether it describes a valid MeteoFunction.
        /// </summary>
        /// <remarks>
        /// This function needs to be further extended as follow up of issue
        /// SOBEK3-1535. 
        /// </remarks>
        /// <param name="dataAccessModel"> The dataAccessModel to be validated.</param>
        /// <param name="errorMessages"> List of error messages to be extended.</param>
        /// <returns>True if dataAccessModel can be parsed, false otherwise.</returns>
        private static bool Validate(IList<IDelftBcCategory> dataAccessModel,
                                     ICollection<string> errorMessages)
        {
            if (dataAccessModel == null)
            {
                errorMessages.Add("Unable to parse null meteo data function.");
                return false;
            }

            if (!dataAccessModel.Any())
            {
                errorMessages.Add("Unable to parse empty set of meteo data.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Parse the provided valid dataAccessModel to obtain a valid MeteoFunction.
        /// </summary>
        /// <param name="dataAccessModel">The air temperature, humidity and cloudiness categories (in any order).</param>
        /// <param name="errorMessages">Collection of error messages to which new messages will be added.</param>
        /// <pre-condition> this.Validate(dataAccessModel, _)</pre-condition>
        /// <returns>A new MeteoFunction corresponding with the description in the dataAccessModel</returns>
        private static MeteoFunction Parse(IList<IDelftBcCategory> dataAccessModel,
                                           ICollection<string> errorMessages)
        {
            var meteoFunction = new MeteoFunction();

            // Obtain DateTime values
            var dateTimeValues = BcConverterHelper.ParseDateTimesValuesFromTableColumn(dataAccessModel[0].Table[0]);
            meteoFunction.Arguments[0].SetValues(dateTimeValues); // set time column of the function

            // Determine which index corresponds with which value set
            foreach (var component in dataAccessModel)
            {
                var values = BcConverterHelper.ParseDoubleValuesFromTableColumn(component.Table[1]).ToList();
                if (component.Table[1].Quantity.Value.Equals(BoundaryRegion.QuantityStrings.MeteoDataAirTemperature))
                    meteoFunction.AirTemperature.SetValues(values);
                else if (component.Table[1].Quantity.Value.Equals(BoundaryRegion.QuantityStrings.MeteoDataHumidity))
                    meteoFunction.RelativeHumidity.SetValues(values);
                else // Quantity.Values.Equals(cloudiness)
                    meteoFunction.Cloudiness.SetValues(values);
            }

            // Determine Interpolation | Extrapolation | Periodicity
            if (!BcConverterHelper.ValidateInterpolation(dataAccessModel.First().Properties,
                                                         out var interpolationType,
                                                         out var extrapolationType))
            {
                errorMessages.Add("Unable to parse MeteoFunction interpolation, defaulting to linear-extrapolate.");
            }

            meteoFunction.SetInterpolationType(interpolationType);
            meteoFunction.SetExtrapolationType(extrapolationType);

            if (!BcConverterHelper.ValidatePeriodicity(dataAccessModel.First().Properties,
                                                       out var hasPeriodicity))
            {
                errorMessages.Add("Unable to parse MeteoFunction periodicity, defaulting to false.");
            }

            meteoFunction.SetPeriodicity(hasPeriodicity);

            return meteoFunction;
        }
    }
}
