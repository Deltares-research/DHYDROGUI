using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;


namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Boundary
{
    /// <summary>
    /// LateralDischargeConverter is responsible for extracting all LateralDischarges from
    /// a BoundaryConditions data access model.
    /// </summary>
    public static class LateralDischargeConverter
    {
        /// <summary>
        /// Extract all valid LateralDischarges from the specified <paramref name="dataAccessModel"/>.
        /// </summary>
        /// <param name="dataAccessModel">The data access model describing the LateralDischarges. </param>
        /// <param name="errorMessages">List of error messages to which new messages will be added. </param>
        /// <returns>
        /// A Dictionary mapping the NodeNames to the corresponding valid LateralDischarges extracted from
        /// the data access model.
        /// </returns>
        public static IDictionary<string, LateralDischarge> Convert(IList<IDelftBcCategory> dataAccessModel,
                                                                   IList<string> errorMessages)
        {
            var lateralDischarges = new Dictionary<string, LateralDischarge>();

            if (!ValidateDataAccessModel(dataAccessModel, errorMessages))
                return lateralDischarges;

            var relevantCategories = dataAccessModel.Where(p => p.Name == BoundaryRegion.BcLateralHeader);
            foreach (var category in relevantCategories)
                Parse(category, lateralDischarges, errorMessages);

            return lateralDischarges;
        }

        /// <summary>
        /// Validate the provided <paramref name="dataAccessModel"/>.
        /// </summary>
        /// <param name="dataAccessModel">The dataAccessModel to be validated. </param>
        /// <param name="errorMessages">Collection of error messages to which new messages will be added.</param>
        /// <returns>
        /// True if the <paramref name="dataAccessModel"/> is valid and false otherwise.
        /// </returns>
        private static bool ValidateDataAccessModel(IList<IDelftBcCategory> dataAccessModel,
                                                    ICollection<string> errorMessages)
        {
            if (dataAccessModel == null)
            {
                errorMessages.Add("Unable to parse null set of LateralDischarges.");
                return false;
            }

            if (!dataAccessModel.Any())
            {
                errorMessages.Add("Unable to parse empty set of LateralDischarges.");
                return false;
            }

            return true;
        }

        /// <summary> Possible LateralDischarge types ComponentTypes </summary>
        private enum ComponentType
        {
            Water,
            SaltMass,
            SaltConcentration,
            Temp
        };

        /// <summary>
        /// Parse the provided category, adding any encountered to the <paramref name="errorMessages"/>, and adding
        /// the described component, if valid, to the corresponding LateralDischarge.
        /// </summary>
        /// <param name="category"> The category to be parsed. </param>
        /// <param name="boundaryConditions"> The Dictionary describing all LateralDischarges encountered so far. </param>
        /// <param name="errorMessages"> Collection of error messages to which new messages will be added.</param>
        /// <pre-condition>category != null && lateralDischarges != null && errorMessages != null. </pre-condition>
        private static void Parse(IDelftBcCategory category,
                                  Dictionary<string, LateralDischarge> lateralDischarges,
                                  ICollection<string> errorMessages)
        {
            // Validate properties of category.
            if (!BcConverterHelper.ValidateNameProperty(category.Properties, out var name))
            {
                errorMessages.Add(
                    $"Unable to parse name of LateralDischarge: {category.Name} at line {category.LineNumber}.");
                return;
            }

            if (!lateralDischarges.ContainsKey(name))
                lateralDischarges.Add(name, new LateralDischarge(name));
            var lateralDischarge = lateralDischarges[name];

            if (!BcConverterHelper.ValidateFunctionProperty(category.Properties, out var function))
            {
                errorMessages.Add(
                    $"Unable to parse function of LateralDischarge: {category.Name} at line {category.LineNumber}.");
                return;
            }

            var interpolationType = Flow1DInterpolationType.Linear;
            var extrapolationType = Flow1DExtrapolationType.Linear;
            var hasPeriodicity = false;

            if (function != FunctionType.Constant)
            {
                if (!BcConverterHelper.ValidateInterpolation(category.Properties,
                                                             out interpolationType,
                                                             out extrapolationType))
                {
                    errorMessages.Add(
                        $"Unable to parse interpolation of LateralDischarge: {category.Name} at line {category.LineNumber}");
                    return;
                }

                if (!BcConverterHelper.ValidatePeriodicity(category.Properties, out hasPeriodicity))
                {
                    errorMessages.Add(
                        $"Unable to parse periodicity of LateralDischarge: {category.Name} at line {category.LineNumber}");
                    return;
                }
            }

            ComponentType componentType;
            if (!ValidateComponentType(category.Table[function == FunctionType.Constant ? 0 : 1],
                                       out componentType))
                errorMessages.Add
                    ($"Unable to parse Quantity of LateralDischarge: {category.Name} at line {category.LineNumber}.");

            // Check if value has not already been defined.
            switch (componentType)
            {
                case ComponentType.Water:
                    if (lateralDischarge.WaterComponent != null)
                    {
                        errorMessages.Add(
                            $"Could not parse lateral discharge category: {category.Name} at line {category.LineNumber}: Component has already been defined");
                        return;
                    }

                    break;
                case ComponentType.SaltMass:
                case ComponentType.SaltConcentration:
                    if (lateralDischarge.SaltComponent != null)
                    {
                        errorMessages.Add(
                            $"Could not parse lateral discharge category: {category.Name} at line {category.LineNumber}: Component has already been defined");
                        return;
                    }

                    break;
                case ComponentType.Temp:
                    if (lateralDischarge.TemperatureComponent != null)
                    {
                        errorMessages.Add(
                            $"Could not parse lateral discharge category: {category.Name} at line {category.LineNumber}: Component has already been defined");
                        return;
                    }

                    break;
            }

            // Parse actual component from the category.
            switch (function)
            {
                case FunctionType.Constant:
                    ParseConstant(category.Table, lateralDischarge, componentType);
                    break;
                case FunctionType.QhTable:
                    ParseQhTable(category.Table, lateralDischarge, interpolationType, extrapolationType, hasPeriodicity);
                    break;
                case FunctionType.TimeSeries:
                    ParseTimeSeries(category.Table, lateralDischarge, interpolationType, extrapolationType, hasPeriodicity, componentType);
                    break;
            }
        }

        /// <summary>
        /// Validate the component type specified with the <paramref name="data"/>. 
        /// </summary>
        /// <param name="data"> The column from which the componentType should be obtained.</param>
        /// <param name="componentType">If valid, the found ComponentType. </param>
        /// <returns>
        /// True if data describes a valid ComponentType, false otherwise.
        /// If true, then componentType will contain the validated ComponentType.
        /// </returns>
        private static bool ValidateComponentType(IDelftBcQuantityData data, out ComponentType componentType)
        {
            componentType = ComponentType.Water;
            switch (data.Quantity.Value)
            {
                case BoundaryRegion.QuantityStrings.WaterDischarge:
                    componentType = ComponentType.Water;
                    break;
                case BoundaryRegion.QuantityStrings.WaterSalinity:
                    if (data.Unit.Value.Equals(BoundaryRegion.UnitStrings.SaltMass))
                        componentType = ComponentType.SaltMass;
                    else if (data.Unit.Value.Equals(BoundaryRegion.UnitStrings.SaltPpt))
                        componentType = ComponentType.SaltConcentration;
                    else
                        return false;
                break;
                case BoundaryRegion.QuantityStrings.WaterTemperature:
                    componentType = ComponentType.Temp;
                    break;
                default:
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Parse the constant component specified by the parameters and set this in <paramref name="discharge"/>
        /// </summary>
        /// <param name="categoryTable"> The values associated with this constant component. </param>
        /// <param name="discharge"> The LateralDischarge to which this component is added. </param>
        /// <param name="interpolationType"> The InterpolationType of this new LateralDischargeComponent. </param>
        /// <param name="hasPeriodicity"> Whether this new LateralDischargeComponent has periodicity. </param>
        /// <param name="componentType"> The type of LateralDischargeComponent to be created. </param>
        private static void ParseConstant(IList<IDelftBcQuantityData> categoryTable,
                                          LateralDischarge discharge,
                                          ComponentType componentType)
        {
            var val = double.Parse(categoryTable[0].Values[0],
                                   NumberStyles.AllowExponent |
                                   NumberStyles.AllowDecimalPoint |
                                   NumberStyles.AllowLeadingSign,
                                   CultureInfo.InvariantCulture);
            switch (componentType)
            {
                case ComponentType.Water:
                    discharge.WaterComponent = new LateralDischargeWater(WaterFlowModel1DLateralDataType.FlowConstant,
                                                                         val);
                    break;
                case ComponentType.SaltMass:
                    discharge.SaltComponent = new LateralDischargeSalt(SaltLateralDischargeType.MassConstant,
                                                                       val);
                    break;
                case ComponentType.SaltConcentration:
                    discharge.SaltComponent = new LateralDischargeSalt(val == WaterFlowModel1DLateralSourceData.DefaultSalinity ? SaltLateralDischargeType.Default : 
                                                                                                                                  SaltLateralDischargeType.ConcentrationConstant,
                                                                       val);
                    break;
                case ComponentType.Temp:
                    discharge.TemperatureComponent = new LateralDischargeTemperature(TemperatureLateralDischargeType.Constant,
                                                                                     val);
                    break;
            }
        }

        /// <summary>
        /// Parse the QhTable WaterComponent specified by the parameters and set this in <paramref name="discharge"/>
        /// </summary>
        /// <param name="categoryTable"> The values associated with this QhTable component. </param>
        /// <param name="discharge"> The LateralDischarge to which this component is added. </param>
        /// <param name="interpolationType"> The InterpolationType of this new LateralDischargeComponent. </param>
        /// <param name="hasPeriodicity"> Whether this new LateralDischargeComponent has periodicity. </param>
        private static void ParseQhTable(IList<IDelftBcQuantityData> categoryTable, 
                                         LateralDischarge discharge, 
                                         Flow1DInterpolationType interpolationType,
                                         Flow1DExtrapolationType extrapolationType,
                                         bool hasPeriodicity)
        {
            // QhTable is only defined for the water component
            var function = new Function();
            function.Arguments.Add(new Variable<double>(categoryTable[0].Quantity.Value));

            function.SetInterpolationType(interpolationType);
            function.SetExtrapolationType(extrapolationType);
            function.SetPeriodicity(hasPeriodicity);

            function.Components.Add((new Variable<double>(categoryTable[1].Quantity.Value,
                                                          new Unit(categoryTable[1].Unit.Name, categoryTable[1].Unit.Value))));
            function.Arguments[0].SetValues(BcConverterHelper.ParseDoubleValuesFromTableColumn(categoryTable[0]));
            function.Components[0].SetValues(BcConverterHelper.ParseDoubleValuesFromTableColumn(categoryTable[1]));

            discharge.WaterComponent = new LateralDischargeWater(WaterFlowModel1DLateralDataType.FlowWaterLevelTable,
                                                                 interpolationType,
                                                                 extrapolationType,
                                                                 hasPeriodicity,
                                                                 function);
        }

        /// <summary>
        /// Parse the constant component specified by the parameters and set this in <paramref name="discharge"/>
        /// </summary>
        /// <param name="categoryTable"> The values associated with this timeseries component. </param>
        /// <param name="discharge"> The LateralDischarge to which this component is added. </param>
        /// <param name="interpolationType"> The InterpolationType of this new LateralDischargeComponent. </param>
        /// <param name="hasPeriodicity"> Whether this new LateralDischargeComponent has periodicity. </param>
        /// <param name="componentType"> The type of LateralDischargeComponent to be created. </param>
        private static void ParseTimeSeries(IList<IDelftBcQuantityData> categoryTable, 
                                            LateralDischarge discharge,
                                            Flow1DInterpolationType interpolationType,
                                            Flow1DExtrapolationType extrapolationType,
                                            bool hasPeriodicity,
                                            ComponentType componentType)
        {
            var function = new Function();
            function.Arguments.Add(new Variable<DateTime>(categoryTable[0].Quantity.Value));

            function.SetInterpolationType(interpolationType);
            function.SetExtrapolationType(extrapolationType);
            function.SetPeriodicity(hasPeriodicity);

            function.Components.Add((new Variable<double>(categoryTable[1].Quantity.Value,
                                     new Unit(categoryTable[1].Unit.Name, categoryTable[1].Unit.Value))));
            function.Arguments[0].SetValues(BcConverterHelper.ParseDateTimesValuesFromTableColumn(categoryTable[0]));
            function.Components[0].SetValues(BcConverterHelper.ParseDoubleValuesFromTableColumn(categoryTable[1]));

            switch (componentType)
            {
                case ComponentType.Water:
                    discharge.WaterComponent = new LateralDischargeWater(WaterFlowModel1DLateralDataType.FlowTimeSeries,
                                                                         interpolationType,
                                                                         extrapolationType,
                                                                         hasPeriodicity,
                                                                         function);
                    break;
                case ComponentType.SaltMass:
                    discharge.SaltComponent = new LateralDischargeSalt(SaltLateralDischargeType.MassTimeSeries,
                                                                       interpolationType,
                                                                       extrapolationType,
                                                                       hasPeriodicity,
                                                                       function);
                    break;
                case ComponentType.SaltConcentration:
                    discharge.SaltComponent = new LateralDischargeSalt(SaltLateralDischargeType.ConcentrationTimeSeries,
                                                                       interpolationType,
                                                                       extrapolationType,
                                                                       hasPeriodicity,
                                                                       function);
                    break;
                case ComponentType.Temp:
                    discharge.TemperatureComponent = new LateralDischargeTemperature(TemperatureLateralDischargeType.TimeDependent,
                                                                                     interpolationType,
                                                                                     extrapolationType,
                                                                                     hasPeriodicity,
                                                                                     function);
                    break;
            }
        }
    }
}
