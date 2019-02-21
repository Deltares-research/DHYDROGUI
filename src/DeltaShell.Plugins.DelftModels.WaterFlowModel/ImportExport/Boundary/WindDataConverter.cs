using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.Helpers;


namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Boundary
{
    /// <summary>
    /// WindDataConverter is responsible for extracting a WindFunction from
    /// a BoundaryConditions data access model.
    /// </summary>
    public static class WindDataConverter
    {
        /// <summary>
        /// Convert the specified dataAccessModel to a new WindFunction if the
        /// dataAccessModel is valid, return null otherwise.
        ///
        /// Any errors will be logged in errorMessages.
        /// </summary>
        /// <param name="dataAccessModel">The data access model describing the WindFunction</param>
        /// <param name="errorMessages">List of error messages to which new messages will be added</param>
        /// <returns>
        /// If the <paramref name="dataAccessModel"/> describes a valid WindFunction
        /// then the corresponding WindFunction, else null.
        /// </returns>
        public static WindFunction Convert(IList<IDelftBcCategory> dataAccessModel,
                                           IList<string> errorMessages)
        {
            if (!Validate(dataAccessModel, errorMessages))
                return null;

            var relevantCategories = dataAccessModel.Where(IsWindFunctionAttribute).ToList();
            return Parse(relevantCategories, errorMessages);
        }

        private static bool IsWindFunctionAttribute(IDelftBcCategory category)
        {
            return category.Name.Equals(BoundaryRegion.BcBoundaryHeader) &&
                   category.ReadProperty<string>(BoundaryRegion.Name.Key).Equals(FunctionAttributes.StandardFeatureNames.ModelWide) &&
                   (category.Table[1].Quantity.Value.Equals(BoundaryRegion.QuantityStrings.WindSpeed) ||
                    category.Table[1].Quantity.Value.Equals(BoundaryRegion.QuantityStrings.WindDirection));
        }

        /// <summary>
        /// Validate the provided dataAccessModel whether it describes a valid
        /// wind function.
        /// </summary>
        /// <remarks>
        /// This function needs to be further extended as follow up of issue
        /// SOBEK3-1535. 
        /// </remarks>
        /// <param name="dataAccessModel">The dataAccessModel to be validated.</param>
        /// <param name="errorMessages">List of error messages to be extended.</param>
        /// <returns>True if dataAccessModel can be parsed, false otherwise.</returns>
        private static bool Validate(IList<IDelftBcCategory> dataAccessModel,
                                     IList<string> errorMessages)
        {
            if (dataAccessModel == null)
            {
                errorMessages.Add("Unable to parse null wind data function.");
                return false;
            }

            if (!dataAccessModel.Any())
            {
                errorMessages.Add("Unable to parse empty set of wind data.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Parse the provided valid dataAccessModel to obtain a valid
        /// WindFunction.
        /// </summary>
        /// <param name="dataAccessModel">The wind_speed and wind_direction categories (in any order).</param>
        /// <pre-condition>this.Validate(dataAccessModel, _)</pre-condition>
        /// <returns>A new WindFunction corresponding with the description in the dataAccessModel</returns>
        private static WindFunction Parse(IList<IDelftBcCategory> dataAccessModel, 
                                          IList<string> errorMessages)
        {
            var windFunction = new WindFunction();

            // Obtain DateTime values
            var dateTimeValues = BcConverterHelper.ParseDateTimesValuesFromTableColumn(dataAccessModel[0].Table[0]);

            // Determine which index corresponds with which value set
            windFunction.Arguments[0].SetValues(dateTimeValues); // set time column of the function

            foreach (var category in dataAccessModel)
            {
                var values = BcConverterHelper.ParseDoubleValuesFromTableColumn(category.Table[1]).ToList();
                if (category.Table[1].Quantity.Value.Equals(BoundaryRegion.QuantityStrings.WindSpeed))
                    windFunction.Velocity.SetValues(values);
                else
                    windFunction.Direction.SetValues(values);
            }

            // Determine Interpolation | Extrapolation | Periodicity
            if (!BcConverterHelper.ValidateInterpolation(dataAccessModel.First().Properties,
                                                         out var interpolationType,
                                                         out var extrapolationType))
            {
                errorMessages.Add("Unable to parse WindFunction interpolation, defaulting to linear-extrapolate.");
                interpolationType = Flow1DInterpolationType.Linear;
                extrapolationType = Flow1DExtrapolationType.Linear;
            }

            windFunction.SetInterpolationType(interpolationType);
            windFunction.SetExtrapolationType(extrapolationType);

            if (!BcConverterHelper.ValidatePeriodicity(dataAccessModel.First().Properties,
                                                       out var hasPeriodicity))
            {
                errorMessages.Add("Unable to parse WindFunction periodicity, defaulting to false.");
                hasPeriodicity = false;
            }

            windFunction.SetPeriodicity(hasPeriodicity);

            return windFunction;
        }
    }
}
