using DelftTools.Functions;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils
{
    /// <summary>
    /// Function type creator
    /// </summary>
    public interface IFunctionTypeCreator
    {
        /// <summary>
        /// The name of the function type creator
        /// </summary>
        string FunctionTypeName { get; }

        /// <summary>
        /// Returns whether or not
        /// <param name="function"/>
        /// is of a specific type
        /// </summary>
        bool IsThisFunctionType(IFunction function);

        /// <summary>
        /// Transforms
        /// <param name="function"/>
        /// into a function of a specific type
        /// </summary>
        /// <returns> The transformed function </returns>
        IFunction TransformToFunctionType(IFunction function);

        /// <summary>
        /// Return the default value of
        /// <param name="function"/>
        /// </summary>
        /// <remarks> double.NaN is returned when <see cref="IsThisFunctionType"/> is false </remarks>
        double GetDefaultValueForFunction(IFunction function);

        /// <summary>
        /// Sets the default value of
        /// <param name="function"/>
        /// </summary>
        /// <remarks> Nothing is set when <see cref="IsThisFunctionType"/> is false </remarks>
        void SetDefaultValueForFunction(IFunction function, double defaultValue);

        /// <summary>
        /// Return the unit of
        /// <param name="function"/>
        /// </summary>
        /// <remarks> An empty string is returned when <see cref="IsThisFunctionType"/> is false </remarks>
        string GetUnitForFunction(IFunction function);

        /// <summary>
        /// Sets the unit of
        /// <param name="function"/>
        /// </summary>
        /// <remarks> Nothing is set when <see cref="IsThisFunctionType"/> is false </remarks>
        void SetUnitForFunction(IFunction function, string unit);

        /// <summary>
        /// Return the url of
        /// <param name="function"/>
        /// </summary>
        /// <remarks> An empty string is returned when <see cref="IsThisFunctionType"/> is false </remarks>
        string GetUrlForFunction(IFunction function);

        /// <summary>
        /// Sets the url of
        /// <param name="function"/>
        /// </summary>
        /// <remarks> Nothing is set when <see cref="IsThisFunctionType"/> is false </remarks>
        void SetUrlForFunction(IFunction function, string url);

        /// <summary>
        /// Determines whether this creator is allowed to be used for a specified function.
        /// </summary>
        /// <param name="function"> The function. </param>
        /// <returns> True if this creator can be used; False otherwise. </returns>
        bool IsAllowed(IFunction function);
    }
}