using System.Collections.Generic;
using DelftTools.Functions;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    /// <summary>
    /// <see cref="ITimeDependentFunctionSplitter"/> is responsible for splitting a Function
    /// into several separate function based on its arguments.
    /// </summary>
    public interface ITimeDependentFunctionSplitter
    {
        /// <summary>
        /// Split the provided <paramref name="function"/> into separate functions based on its
        /// arguments.
        /// </summary>
        /// <param name="function">The function to split.</param>
        /// <returns>A collection of new function, each corresponding with a single argument value.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="function"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when there is less than 2 arguments or the first argument is not a Time argument.
        /// </exception>
        ICollection<IFunction> SplitIntoFunctionsPerArgumentValue(IFunction function);

        /// <summary>
        /// Extract the series corresponding with the <paramref name="argumentValue"/> from the <paramref name="function"/>.
        /// </summary>
        /// <param name="function">The function to extract the series from.</param>
        /// <param name="argumentValue">The argument value corresponding with the extract series.</param>
        /// <returns>
        /// The extracted series as a <see cref="IFunction"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="function"/> or <paramref name="argumentValue"/> is <c>null</c>.
        /// </exception>
        IFunction ExtractSeriesForArgumentValue(IFunction function, object argumentValue);
    }
}