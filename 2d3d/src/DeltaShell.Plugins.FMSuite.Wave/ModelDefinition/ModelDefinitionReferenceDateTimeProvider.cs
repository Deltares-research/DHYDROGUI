using System;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.FMSuite.Wave.ModelDefinition
{
    /// <summary>
    /// <see cref="ModelDefinitionReferenceDateTimeProvider"/> implements <see cref="IReferenceDateTimeProvider"/>
    /// by wrapping the <see cref="WaveModelDefinition"/>.
    /// </summary>
    /// <seealso cref="IReferenceDateTimeProvider"/>
    public class ModelDefinitionReferenceDateTimeProvider : IReferenceDateTimeProvider
    {
        private readonly WaveModelDefinition modelDefinition;

        /// <summary>
        /// Creates a new <see cref="ModelDefinitionReferenceDateTimeProvider"/>.
        /// </summary>
        /// <param name="modelDefinition">The model definition.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="modelDefinition"/> is <c>null</c>.
        /// </exception>
        public ModelDefinitionReferenceDateTimeProvider(WaveModelDefinition modelDefinition)
        {
            Ensure.NotNull(modelDefinition, nameof(modelDefinition));
            this.modelDefinition = modelDefinition;
        }

        public DateTime ModelReferenceDateTime => modelDefinition.ModelReferenceDateTime;
    }
}