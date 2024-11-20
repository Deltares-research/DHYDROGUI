using System;
using DelftTools.Functions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extentions;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils
{
    /// <summary>
    /// Factory for creating function type creators
    /// </summary>
    public static class FunctionTypeCreatorFactory
    {
        /// <summary>
        /// Creates a function type creator for constant functions (in the context of WaterQualityModel1D)
        /// </summary>
        public static IFunctionTypeCreator CreateConstantCreator()
        {
            return CreateNewFunctionTypeCreator("Constant", f => f.IsConst(), WaterQualityFunctionFactory.CreateConst);
        }

        /// <summary>
        /// Creates a function type creator for time series (in the context of WaterQualityModel1D)
        /// </summary>
        public static IFunctionTypeCreator CreateTimeseriesCreator()
        {
            return CreateNewFunctionTypeCreator("Time series", f => f.IsTimeSeries(),
                                                WaterQualityFunctionFactory.CreateTimeSeries);
        }

        /// <summary>
        /// Creates a function type creator for time series (in the context of WaterQualityModel1D)
        /// </summary>
        public static IFunctionTypeCreator CreateSegmentFileCreator()
        {
            return CreateNewFunctionTypeCreator("Segment function", f => f.IsSegmentFile(),
                                                WaterQualityFunctionFactory.CreateSegmentFunction);
        }

        /// <summary>
        /// Creates a function type creator for network coverages
        /// </summary>
        public static IFunctionTypeCreator CreateNetworkCoverageCreator()
        {
            return CreateNewFunctionTypeCreator("Coverage", f => f.IsNetworkCoverage(),
                                                WaterQualityFunctionFactory.CreateNetworkCoverage);
        }

        /// <summary>
        /// Creates a function type creator for UnstructuredGrid coverages
        /// </summary>
        public static IFunctionTypeCreator CreateUnstructuredGridCoverageCreator()
        {
            return CreateNewFunctionTypeCreator("Coverage", f => f.IsUnstructuredGridCellCoverage(),
                                                WaterQualityFunctionFactory.CreateUnstructuredGridCellCoverage);
        }

        /// <summary>
        /// Creates a function type creator for data coming from hydro dynamics data.
        /// </summary>
        /// <param name="isAllowedFunction">
        /// Function that should return true if the creator
        /// should be applicable for a given <see cref="IFunction"/> and return false when
        /// it's not.
        /// </param>
        /// <param name="getFilePath">
        /// Function that should return the filepath corresponding
        /// to the converted function.
        /// </param>
        public static IFunctionTypeCreator CreateFunctionFromHydroDynamicsCreator(
            Func<IFunction, bool> isAllowedFunction,
            Func<IFunction, string> getFilePath)
        {
            return CreateNewFunctionTypeCreator("From hydro data",
                                                f => f.IsFromHydroDynamics(),
                                                delegate(
                                                    string name,
                                                    double defaultValue,
                                                    string compName,
                                                    string unitName,
                                                    string description,
                                                    string url)
                                                {
                                                    FunctionFromHydroDynamics functionFromHydroDynamics =
                                                        WaterQualityFunctionFactory.CreateFunctionFromHydroDynamics(name,
                                                                                                                    defaultValue,
                                                                                                                    compName,
                                                                                                                    unitName,
                                                                                                                    description,
                                                                                                                    url);
                                                    functionFromHydroDynamics.FilePath = getFilePath(functionFromHydroDynamics);

                                                    return functionFromHydroDynamics;
                                                },
                                                isAllowedFunction);
        }

        /// <summary>
        /// Creates a new function type creator.
        /// </summary>
        /// <param name="name"> The name of the creator. </param>
        /// <param name="checkFunction">
        /// The check function, which verifies if the passed function
        /// is of the type this creator creates (returning true) or not (returning false).
        /// </param>
        /// <param name="createFunction">
        /// The <see cref="IFunction"/> creation method, where:
        /// <list type="bullet">
        ///     <item> 1st argument: function name </item>
        ///     <item> 2nd argument: default value </item>
        ///     <item> 3rd argument: component name </item>
        ///     <item> 4th argument: component unit name </item>
        /// </list>
        /// </param>
        /// <param name="isAllowedFunction">
        /// The function to determine if the creator can be
        /// used for a given function or not.
        /// </param>
        private static FunctionTypeCreator CreateNewFunctionTypeCreator(
            string name, Func<IFunction, bool> checkFunction,
            Func<string, double, string, string, string, string, IFunction> createFunction,
            Func<IFunction, bool> isAllowedFunction = null)
        {
            return new FunctionTypeCreator(name, checkFunction,
                                           f => createFunction(f.Name,
                                                               WaterQualityFunctionFactory.GetDefaultValue(f),
                                                               WaterQualityFunctionFactory.GetComponentName(f),
                                                               WaterQualityFunctionFactory.GetComponentUnitName(f),
                                                               WaterQualityFunctionFactory.GetDescription(f),
                                                               WaterQualityFunctionFactory.GetUrlValue(f)),
                                           WaterQualityFunctionFactory.GetDefaultValue,
                                           WaterQualityFunctionFactory.SetDefaultValue,
                                           WaterQualityFunctionFactory.GetComponentUnitName,
                                           WaterQualityFunctionFactory.SetComponentUnitName,
                                           WaterQualityFunctionFactory.GetUrlValue,
                                           WaterQualityFunctionFactory.SetUrlValue,
                                           isAllowedFunction ?? (f => true));
        }
    }
}