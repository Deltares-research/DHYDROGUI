using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils.Globalization;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel
{
    public static class WaterQualityFunctionFactory
    {
        public const string DESCRIPTION_ATTRIBUTE = "Description";

        /// <summary>
        /// Returns a function one component(double) and no arguments
        /// </summary>
        /// <param name="name"> Name of the new function </param>
        /// <param name="defaultValue"> Default value of the component </param>
        /// <param name="componentName"> Name of the component </param>
        /// <param name="componentUnitName"> Unit name/symbol of the component </param>
        /// <param name="description"> The description as found in the process library. </param>
        public static IFunction CreateConst(string name, double defaultValue, string componentName,
                                            string componentUnitName, string description, string url = null)
        {
            var function = new Function(name);
            function.Attributes.Add(DESCRIPTION_ATTRIBUTE, description);
            var variable = new Variable<double>
            {
                Name = componentName,
                DefaultValue = defaultValue,
                Unit = new Unit(componentUnitName, componentUnitName)
            };

            function.Components.Add(variable);

            // HACK: Add a dummy argument variable to make the function dependent (=> property changes are notified then...)
            function.Arguments.Add(new Variable<double>());

            return function;
        }

        /// <summary>
        /// Returns a function with one time argument and one component(double)
        /// </summary>
        /// <param name="name"> Name of the new function </param>
        /// <param name="defaultValue"> Default value of the component </param>
        /// <param name="componentName"> Name of the component </param>
        /// <param name="componentUnitName"> Unit name/symbol of the component </param>
        /// <param name="description"> The description as found in the process library </param>
        public static IFunction CreateTimeSeries(string name, double defaultValue, string componentName,
                                                 string componentUnitName, string description, string url = null)
        {
            var function = new Function(name);
            function.Attributes.Add(DESCRIPTION_ATTRIBUTE, description);
            var variable = new Variable<double>
            {
                Name = componentName,
                DefaultValue = defaultValue,
                Unit = new Unit(componentUnitName, componentUnitName)
            };

            function.Components.Add(variable);
            function.Arguments.Add(new Variable<DateTime>("date time")
            {
                Unit = new Unit(RegionalSettingsManager.DateTimeFormat,
                                RegionalSettingsManager.DateTimeFormat)
            });

            return function;
        }

        /// <summary>
        /// Returns a function with one time argument and one component(double)
        /// </summary>
        /// <param name="name"> Name of the new function </param>
        /// <param name="defaultValue"> Default value of the component </param>
        /// <param name="componentName"> Name of the component </param>
        /// <param name="componentUnitName"> Unit name/symbol of the component </param>
        /// <param name="description"> The description as found in the process library </param>
        /// <param name="urlPath"> The URL path. </param>
        public static SegmentFileFunction CreateSegmentFunction(string name, double defaultValue, string componentName,
                                                                string componentUnitName, string description,
                                                                string urlPath)
        {
            var function = new SegmentFileFunction
            {
                Name = name,
                UrlPath = urlPath
            };
            function.Attributes.Add(DESCRIPTION_ATTRIBUTE, description);
            var variable = new Variable<double>
            {
                Name = componentName,
                DefaultValue = defaultValue,
                Unit = new Unit(componentUnitName, componentUnitName)
            };

            function.Components.Add(variable);

            return function;
        }

        /// <summary>
        /// Returns a <see cref="NetworkCoverage"/> (as a function) wtih the supplied parameters
        /// </summary>
        /// <param name="name"> Name of the new function </param>
        /// <param name="defaultValue"> Default value of the component </param>
        /// <param name="componentName"> Name of the component </param>
        /// <param name="componentUnitName"> Unit name/symbol of the component </param>
        /// <param name="description"> The description as found in the process library. </param>
        public static IFunction CreateNetworkCoverage(string name, double defaultValue, string componentName,
                                                      string componentUnitName, string description, string url = null)
        {
            var networkCoverage = new NetworkCoverage(name, false, componentName, componentUnitName);
            networkCoverage.Attributes.Add(DESCRIPTION_ATTRIBUTE, description);
            networkCoverage.Components[0].DefaultValue = defaultValue;

            return networkCoverage;
        }

        /// <summary>
        /// Returns a <see cref="UnstructuredGridCellCoverage"/> (as a function) wtih the supplied parameters
        /// </summary>
        /// <param name="name"> Name of the new function </param>
        /// <param name="defaultValue"> Default value of the component </param>
        /// <param name="componentName"> Name of the component </param>
        /// <param name="componentUnitName"> Unit name/symbol of the component </param>
        /// <param name="description"> The description as found in the process library </param>
        public static IFunction CreateUnstructuredGridCellCoverage(string name, double defaultValue,
                                                                   string componentName, string componentUnitName,
                                                                   string description, string url = null)
        {
            var unstructuredGridCellCoverage =
                new UnstructuredGridCellCoverage(new UnstructuredGrid(), false) {Name = name};
            unstructuredGridCellCoverage.Attributes.Add(DESCRIPTION_ATTRIBUTE, description);

            unstructuredGridCellCoverage.Components[0].Unit = new Unit(componentUnitName, componentUnitName);
            unstructuredGridCellCoverage.Components[0].DefaultValue = defaultValue;
            unstructuredGridCellCoverage.Components[0].Name = componentName;
            unstructuredGridCellCoverage.Components[0].NoDataValue = -999d;

            return unstructuredGridCellCoverage;
        }

        /// <summary>
        /// Creates the function 'placeholder' for data available from hydro dynamics data.
        /// </summary>
        /// <param name="name"> Name of the new function </param>
        /// <param name="defaultValue"> Default value of the component </param>
        /// <param name="componentName"> Name of the component </param>
        /// <param name="componentUnitName"> Unit name/symbol of the component </param>
        /// <param name="description"> The description as found in the process library. </param>
        public static FunctionFromHydroDynamics CreateFunctionFromHydroDynamics(
            string name, double defaultValue, string componentName, string componentUnitName, string description,
            string url = null)
        {
            var function = new FunctionFromHydroDynamics
            {
                Name = name,
                FilePath = url
            };
            function.Attributes.Add(DESCRIPTION_ATTRIBUTE, description);
            var variable = new Variable<double>
            {
                Name = componentName,
                DefaultValue = defaultValue,
                Unit = new Unit(componentUnitName, componentUnitName)
            };

            function.Components.Add(variable);

            return function;
        }

        /// <summary>
        /// Returns the name of the first component of the provided function
        /// </summary>
        /// <remarks> "" is returned if the function does not contain any component </remarks>
        public static string GetComponentName(IFunction function)
        {
            return function.Components.Count > 0 ? function.Components[0].Name : "";
        }

        /// <summary>
        /// Returns the name of the unit of the first component of the provided function
        /// </summary>
        /// <remarks> "-" is returned if the function does not contain any component </remarks>
        public static string GetComponentUnitName(IFunction function)
        {
            return function.Components.Count > 0 ? function.Components[0].Unit.Name : "-";
        }

        public static string GetDescription(IFunction function)
        {
            string result;
            return function.Attributes.TryGetValue(DESCRIPTION_ATTRIBUTE, out result) ? result : null;
        }

        /// <summary>
        /// Sets the name of the unit of the first component of the provided function
        /// </summary>
        /// <remarks> No name is set if the function does not contain any component </remarks>
        public static void SetComponentUnitName(IFunction function, string unitName)
        {
            if (function.Components.Count > 0)
            {
                function.Components[0].Unit = new Unit(unitName, unitName);
            }
        }

        /// <summary>
        /// Returns the default value of the first component of the provided function
        /// </summary>
        /// <remarks> NaN is returned if the function does not contain any component or the first component is of the wrong type </remarks>
        public static double GetDefaultValue(IFunction function)
        {
            return function.Components.Count > 0 && function.Components[0].ValueType == typeof(double)
                       ? (double) function.Components[0].DefaultValue
                       : double.NaN;
        }

        /// <summary>
        /// Sets the default value of the first component of the provided function
        /// </summary>
        /// <remarks>
        /// No default value is set if the function does not contain any component or the first component is of the wrong
        /// type
        /// </remarks>
        public static void SetDefaultValue(IFunction function, double defaultValue)
        {
            if (function.Components.Count > 0 && function.Components[0].ValueType == typeof(double))
            {
                function.Components[0].DefaultValue = defaultValue;
            }
        }

        /// <summary>
        /// Returns the url value of the first component of the provided function
        /// </summary>
        /// <remarks> NaN is returned if the function does not contain any component or the first component is of the wrong type </remarks>
        public static string GetUrlValue(IFunction function)
        {
            var auxFunction = function as SegmentFileFunction;
            return auxFunction == null ? null : auxFunction.UrlPath;
        }

        /// <summary>
        /// Sets the default value of the first component of the provided function
        /// </summary>
        /// <remarks>
        /// No default value is set if the function does not contain any component or the first component is of the wrong
        /// type
        /// </remarks>
        public static void SetUrlValue(IFunction function, string urlValue)
        {
            var auxFunction = function as SegmentFileFunction;
            if (function.Components.Count > 0 && auxFunction != null)
            {
                auxFunction.UrlPath = urlValue;
            }
        }
    }
}