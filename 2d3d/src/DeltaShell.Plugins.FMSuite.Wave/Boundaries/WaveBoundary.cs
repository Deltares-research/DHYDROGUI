using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries
{
    /// <summary>
    /// <see cref="WaveBoundary"/> implements a WaveBoundary.
    /// </summary>
    /// <seealso cref="IWaveBoundary"/>
    public class WaveBoundary : IWaveBoundary
    {
        private string name;

        /// <summary>
        /// Creates a new instance of the <see cref="WaveBoundary"/>.
        /// </summary>
        /// <param name="name">The name of this new <see cref="WaveBoundary"/>.</param>
        /// <param name="geometricDefinition">The geometric definition.</param>
        /// <param name="conditionDefinition">The condition definition.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any argument is <c>null</c> or <paramref name="name"/> is empty.
        /// </exception>
        public WaveBoundary(string name,
                            IWaveBoundaryGeometricDefinition geometricDefinition,
                            IWaveBoundaryConditionDefinition conditionDefinition)
        {
            Name = !string.IsNullOrEmpty(name) ? name : throw new ArgumentNullException(nameof(name));
            GeometricDefinition = geometricDefinition ?? throw new ArgumentNullException(nameof(geometricDefinition));
            ConditionDefinition = conditionDefinition ?? throw new ArgumentNullException(nameof(conditionDefinition));
        }

        //
        public string Name
        {
            get => name;
            set => name = !string.IsNullOrEmpty(value)
                              ? value
                              : throw new ArgumentNullException(nameof(value));
        }

        public IWaveBoundaryGeometricDefinition GeometricDefinition { get; }
        public IWaveBoundaryConditionDefinition ConditionDefinition { get; }
    }
}