using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Functions;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific.TimeSeriesGeneration;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="TimeDependentSpatiallyVaryingParametersViewModel{TSpreading}"/> defines the view model for the TimeDependentParametersView
    /// of spatially varying wave boundaries.
    /// </summary>
    /// <typeparam name="TSpreading">The type of the spreading.</typeparam>
    public class TimeDependentSpatiallyVaryingParametersViewModel<TSpreading> : TimeDependentParametersViewModel
        where TSpreading : IBoundaryConditionSpreading, new()
    {
        private readonly IGenerateSeries generateSeries;
        private readonly IReadOnlyDictionary<SupportPoint, TimeDependentParameters<TSpreading>> supportPointToParametersMapping;

        /// <summary>
        /// Creates a new <see cref="TimeDependentSpatiallyVaryingParametersViewModel{TSpreading}"/>.
        /// </summary>
        /// <param name="generateSeries">The generate series.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="supportPointToParametersMapping">The support point to parameters mapping.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public TimeDependentSpatiallyVaryingParametersViewModel(IGenerateSeries generateSeries,
                                                                TimeDependentParameters<TSpreading> parameters, 
                                                                IReadOnlyDictionary<SupportPoint, TimeDependentParameters<TSpreading>> supportPointToParametersMapping)
                                                                
        {
            Ensure.NotNull(generateSeries, nameof(generateSeries));
            Ensure.NotNull(parameters, nameof(parameters));
            Ensure.NotNull(supportPointToParametersMapping, nameof(supportPointToParametersMapping));

            this.generateSeries = generateSeries;
            ObservedParameters = parameters;
            this.supportPointToParametersMapping = supportPointToParametersMapping;

            TimeDependentParametersFunctions = new[] {ObservedParameters.WaveEnergyFunction.UnderlyingFunction};
        }

        /// <summary>
        /// Gets the observed parameters.
        /// </summary>
        public TimeDependentParameters<TSpreading> ObservedParameters { get; }

        public override IEnumerable<IFunction> TimeDependentParametersFunctions { get; }

        protected override void GenerateSeries() => 
            generateSeries.Execute(ObservedParameters.WaveEnergyFunction, 
                                   supportPointToParametersMapping.Values
                                                                  .Where(p => p != ObservedParameters)
                                                                  .Select(p => p.WaveEnergyFunction));
    }
}