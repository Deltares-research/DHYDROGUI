using System;

namespace DeltaShell.Plugins.FMSuite.Wave.ModelDefinition
{
    /// <summary>
    /// <see cref="IReferenceDateTimeProvider"/> provides a generic interface
    /// to obtain a reference date of a model from other parts of the code.
    /// </summary>
    public interface IReferenceDateTimeProvider
    {
        /// <summary>
        /// Gets the model reference date time.
        /// </summary>
        DateTime ModelReferenceDateTime { get; }
    }
}