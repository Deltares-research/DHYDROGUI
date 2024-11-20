
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters
{
    /// <summary>
    /// <see cref="FileBasedParameters"/> provides the file based parameters
    /// associated with a <see cref="IWaveBoundaryConditionDefinition"/>
    /// in the case of uniform data, or the parameters associated with a
    /// <see cref="GeometricDefinitions.SupportPoint"/> in the case of a spatially variant
    /// <see cref="IWaveBoundaryConditionDefinition"/>.
    /// </summary>
    /// <seealso cref="IForcingTypeDefinedParameters"/>
    public class FileBasedParameters : IForcingTypeDefinedParameters
    {
        /// <summary>
        /// Creates a new <see cref="FileBasedParameters"/>.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="filePath"/> is <c>null</c>.
        /// </exception>
        public FileBasedParameters(string filePath)
        {
            Ensure.NotNull(filePath, nameof(filePath));

            FilePath = filePath;
        }

        /// <summary>
        /// Gets or sets the path to the file containing the parameters.
        /// </summary>
        public string FilePath { get; set; }

        public void AcceptVisitor(IForcingTypeDefinedParametersVisitor visitor)
        {
            Ensure.NotNull(visitor, nameof(visitor));
            visitor.Visit(this);
        }
    }
}