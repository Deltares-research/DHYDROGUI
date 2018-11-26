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
    /// BoundaryConditionConverter is responsible for extracting all BoundaryConditions from
    /// a BoundaryConditions data access model.
    /// </summary>
    public static class BoundaryConditionConverter
    {
        /// <summary>
        /// Extract all valid BoundaryConditions from the specified <paramref name="dataAccessModel"/>.
        /// </summary>
        /// <param name="dataAccessModel">The data access model describing the BoundaryConditions. </param>
        /// <param name="errorMessages">List of error messages to which new messages will be added. </param>
        /// <returns>
        /// A Dictionary mapping the NodeNames to the corresponding valid BoundaryConditions extracted from
        /// the data access model.
        /// </returns>
        public static Dictionary<string, BoundaryCondition> Convert(IList<IDelftBcCategory> dataAccessModel,
                                                                    IList<string> errorMessages)
        {
            var boundaryConditions = new Dictionary<string, BoundaryCondition>();

            if (!ValidateDataAccessModel(dataAccessModel, errorMessages))
                return boundaryConditions;

            var relevantCategories = dataAccessModel.Where(p => p.Name == BoundaryRegion.BcBoundaryHeader && 
                                                                p.ReadProperty<string>(BoundaryRegion.Name.Key) != FunctionAttributes.StandardFeatureNames.ModelWide);
            foreach (var category in relevantCategories)
                Parse(category, boundaryConditions, errorMessages);

            return boundaryConditions;
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
                errorMessages.Add("Unable to parse null set of BoundaryConditions.");
                return false;
            }

            if (!dataAccessModel.Any())
            {
                errorMessages.Add("Unable to parse empty set of BoundaryConditions.");
                return false;
            }

            return true;
        }

        /// <summary> Possible BoundaryCondition ComponentTypes </summary>
        private enum ComponentType { Level, Flow, Salt, Temp };

        /// <summary>
        /// Parse the provided category, adding any encountered to the <paramref name="errorMessages"/>, and adding
        /// the described component, if valid, to the corresponding BoundaryCondition.
        /// </summary>
        /// <param name="category"> The category to be parsed. </param>
        /// <param name="boundaryConditions"> The Dictionary describing all BoundaryConditions encountered so far. </param>
        /// <param name="errorMessages"> Collection of error messages to which new messages will be added.</param>
        /// <pre-condition>category != null && boundaryConditions != null && errorMessages != null. </pre-condition>
        private static void Parse(IDelftBcCategory category,
                                  Dictionary<string, BoundaryCondition> boundaryConditions,
                                  ICollection<string> errorMessages)
        {
            // Validate properties of category.
            string name;
            if (!BcConverterHelper.ValidateNameProperty(category.Properties, out name))
            {
                errorMessages.Add(
                    $"Unable to parse name of BoundaryCondition: {category.Name} at line {category.LineNumber}.");
                return;
            }

            if (!boundaryConditions.ContainsKey(name))
                boundaryConditions.Add(name, new BoundaryCondition(name));
            var boundaryCondition = boundaryConditions[name];

            FunctionType function;
            if (!BcConverterHelper.ValidateFunctionProperty(category.Properties, out function))
            {
                errorMessages.Add(
                    $"Unable to parse function of BoundaryCondition: {category.Name} at line {category.LineNumber}.");
                return;
            }

            InterpolationType interpolationType;
            if (!BcConverterHelper.ValidateInterpolation(category.Properties, out interpolationType))
            {
                errorMessages.Add(
                    $"Unable to parse interpolation of BoundaryCondition: {category.Name} at line {category.LineNumber}");
                return;
            }

            bool hasPeriodicity;
            if (!BcConverterHelper.ValidatePeriodicity(category.Properties, out hasPeriodicity))
            {
                errorMessages.Add(
                    $"Unable to parse periodicity of BoundaryCondition: {category.Name} at line {category.LineNumber}");
                return;
            }

            ComponentType componentType;
            if (!ValidateComponentType(category.Table[function == FunctionType.Constant ? 0 : 1].Quantity.Value, out componentType))
                errorMessages.Add
                    ($"Unable to parse Quantity of BoundaryCondition: {category.Name} at line {category.LineNumber}.");

            // Check if value has not already been defined.
            switch (componentType)
            {
                case ComponentType.Flow:
                case ComponentType.Level:
                    if (boundaryCondition.WaterComponent != null)
                    {
                        errorMessages.Add(
                            $"Could not parse boundary condition category: {category.Name} at line {category.LineNumber}: Component has already been defined");
                        return;
                    }
                    break;
                case ComponentType.Salt:
                    if (boundaryCondition.SaltComponent != null)
                    {
                        errorMessages.Add(
                            $"Could not parse boundary condition category: {category.Name} at line {category.LineNumber}: Component has already been defined");
                        return;
                    }
                    break;
                case ComponentType.Temp:
                    if (boundaryCondition.TemperatureComponent != null)
                    {
                        errorMessages.Add(
                            $"Could not parse boundary condition category: {category.Name} at line {category.LineNumber}: Component has already been defined");
                        return;
                    }
                    break;
            }

            // Parse actual component from the category.
            switch (function)
            {
                case FunctionType.Constant:
                    ParseConstant(category.Table, boundaryCondition, interpolationType, hasPeriodicity, componentType);
                    break;
                case FunctionType.QhTable:
                    ParseQhTable(category.Table, boundaryCondition, interpolationType, hasPeriodicity);
                    break;
                case FunctionType.TimeSeries:
                    ParseTimeSeries(category.Table, boundaryCondition, interpolationType, hasPeriodicity, componentType);
                    break;
            }
        }

        /// <summary>
        /// Validate the component type specified with the <paramref name="quantity"/>. 
        /// </summary>
        /// <param name="quantity"> The quantity from which the componentType should be obtained.</param>
        /// <param name="componentType">If valid, the found ComponentType. </param>
        /// <returns>
        /// True if quantity describes a valid ComponentType, false otherwise.
        /// If true, then componentType will contain the validated ComponentType.
        /// </returns>
        private static bool ValidateComponentType(string quantity, out ComponentType componentType)
        {
            componentType = ComponentType.Level;
            switch (quantity)
            {
                case BoundaryRegion.QuantityStrings.WaterDischarge:
                    componentType = ComponentType.Flow;
                    break;
                case BoundaryRegion.QuantityStrings.WaterLevel:
                    componentType = ComponentType.Level;
                    break;
                case BoundaryRegion.QuantityStrings.WaterSalinity:
                    componentType = ComponentType.Salt;
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
        /// Parse the constant component specified by the parameters and set this in <paramref name="condition"/>
        /// </summary>
        /// <param name="categoryTable"> The values associated with this constant component. </param>
        /// <param name="condition"> The BoundaryCondition to which this component is added. </param>
        /// <param name="interpolationType"> The InterpolationType of this new BoundaryConditionComponent. </param>
        /// <param name="hasPeriodicity"> Whether this new BoundaryConditionComponent has periodicity. </param>
        /// <param name="componentType"> The type of BoundaryConditionComponent to be created. </param>
        private static void ParseConstant(IList<IDelftBcQuantityData> categoryTable, BoundaryCondition condition,
                                          InterpolationType interpolationType, bool hasPeriodicity, 
                                          ComponentType componentType)
        {
            var val = double.Parse(categoryTable[0].Values[0],
                                   NumberStyles.AllowExponent |
                                   NumberStyles.AllowDecimalPoint |
                                   NumberStyles.AllowLeadingSign,
                                   CultureInfo.InvariantCulture);
            switch (componentType)
            {
                case ComponentType.Flow:
                    condition.WaterComponent = new BoundaryConditionWater(WaterFlowModel1DBoundaryNodeDataType.FlowConstant, 
                                                                          interpolationType, 
                                                                          hasPeriodicity, 
                                                                          val);
                    break;
                case ComponentType.Level:
                    condition.WaterComponent = new BoundaryConditionWater(WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant,
                                                                          interpolationType,
                                                                          hasPeriodicity,
                                                                          val);
                    break;
                case ComponentType.Salt:
                    condition.SaltComponent = new BoundaryConditionSalt(SaltBoundaryConditionType.Constant,
                                                                        interpolationType,
                                                                        hasPeriodicity,
                                                                        val);
                    break;
                case ComponentType.Temp:
                    condition.TemperatureComponent = new BoundaryConditionTemperature(TemperatureBoundaryConditionType.Constant,
                                                                                      interpolationType,
                                                                                      hasPeriodicity,
                                                                                      val);
                    break;
            }
        }

        /// <summary>
        /// Parse the QhTable WaterComponent specified by the parameters and set this in <paramref name="condition"/>
        /// </summary>
        /// <param name="categoryTable"> The values associated with this QhTable component. </param>
        /// <param name="condition"> The BoundaryCondition to which this component is added. </param>
        /// <param name="interpolationType"> The InterpolationType of this new BoundaryConditionComponent. </param>
        /// <param name="hasPeriodicity"> Whether this new BoundaryConditionComponent has periodicity. </param>
        private static void ParseQhTable(IList<IDelftBcQuantityData> categoryTable, BoundaryCondition condition, InterpolationType interpolationType, bool hasPeriodicity)
        {
            // QhTable is only defined for the water component
            var function = new Function();
            function.Arguments.Add(new Variable<double>(categoryTable[0].Quantity.Value)
            {
                InterpolationType = interpolationType,
                ExtrapolationType = hasPeriodicity ? ExtrapolationType.Periodic : ExtrapolationType.Constant,
            });

            function.Components.Add((new Variable<double>(categoryTable[1].Quantity.Value,
                new Unit(categoryTable[1].Unit.Name, categoryTable[1].Unit.Value))));
            function.Arguments[0].SetValues(BcConverterHelper.ParseDoubleValuesFromTableColumn(categoryTable[0]));
            function.Components[0].SetValues(BcConverterHelper.ParseDoubleValuesFromTableColumn(categoryTable[1]));

            condition.WaterComponent = new BoundaryConditionWater(WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable,
                                                                  interpolationType,
                                                                  hasPeriodicity,
                                                                  function);
        }

        /// <summary>
        /// Parse the timeseries component specified by the parameters and set this in <paramref name="condition"/>
        /// </summary>
        /// <param name="categoryTable"> The values associated with this timeseries component. </param>
        /// <param name="condition"> The BoundaryCondition to which this component is added. </param>
        /// <param name="interpolationType"> The InterpolationType of this new BoundaryConditionComponent. </param>
        /// <param name="hasPeriodicity"> Whether this new BoundaryConditionComponent has periodicity. </param>
        /// <param name="componentType"> The type of BoundaryConditionComponent to be created. </param>
        private static void ParseTimeSeries(IList<IDelftBcQuantityData> categoryTable, BoundaryCondition condition,
            InterpolationType interpolationType, bool hasPeriodicity,
            ComponentType componentType)
        {
            var function = new Function();
            function.Arguments.Add(new Variable<DateTime>(categoryTable[0].Quantity.Value)
            {
                InterpolationType = interpolationType,
                ExtrapolationType = hasPeriodicity ? ExtrapolationType.Periodic : ExtrapolationType.Constant,
            });

            function.Components.Add((new Variable<double>(categoryTable[1].Quantity.Value,
                new Unit(categoryTable[1].Unit.Name, categoryTable[1].Unit.Value))));
            function.Arguments[0].SetValues(BcConverterHelper.ParseDateTimesValuesFromTableColumn(categoryTable[0]));
            function.Components[0].SetValues(BcConverterHelper.ParseDoubleValuesFromTableColumn(categoryTable[1]));

            switch (componentType)
            {
                case ComponentType.Flow:
                    condition.WaterComponent = new BoundaryConditionWater(WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries,
                        interpolationType,
                        hasPeriodicity,
                        function);
                    break;
                case ComponentType.Level:
                    condition.WaterComponent = new BoundaryConditionWater(WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries,
                        interpolationType,
                        hasPeriodicity,
                        function);
                    break;
                case ComponentType.Salt:
                    condition.SaltComponent = new BoundaryConditionSalt(SaltBoundaryConditionType.TimeDependent,
                        interpolationType,
                        hasPeriodicity,
                        function);
                    break;
                case ComponentType.Temp:
                    condition.TemperatureComponent = new BoundaryConditionTemperature(TemperatureBoundaryConditionType.TimeDependent,
                        interpolationType,
                        hasPeriodicity,
                        function);
                    break;
            }
        }
    }
}

