using System;
using DelftTools.Functions;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks
{
    /// <summary>
    /// Interface for the function of a <see cref="SourceAndSink"/>.
    /// </summary>
    public interface ISourceAndSinkFunction : IFunction
    {
        /// <summary>
        /// Removes the sediment fraction variable with the specified <paramref name="name"/> from the function.
        /// </summary>
        /// <param name="name">The name of the sediment fraction.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="name"/> is <c>null</c> or empty.
        /// </exception>
        void RemoveSedimentFraction(string name);

        /// <summary>
        /// Removes the tracer variable with the specified <paramref name="name"/> from the function.
        /// </summary>
        /// <param name="name">The name of the tracer.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="name"/> is <c>null</c> or empty.
        /// </exception>
        void RemoveTracer(string name);

        /// <summary>
        /// Adds a new tracer variable to the function.
        /// </summary>
        /// <param name="name">The name of the tracer.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="name"/> is <c>null</c> or empty.
        /// </exception>
        void AddTracer(string name);

        /// <summary>
        /// Adds a new sediment fraction variable to the function.
        /// </summary>
        /// <param name="name">The name of the sediment fraction.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="name"/> is <c>null</c> or empty.
        /// </exception>
        void AddSedimentFraction(string name);
    }
}