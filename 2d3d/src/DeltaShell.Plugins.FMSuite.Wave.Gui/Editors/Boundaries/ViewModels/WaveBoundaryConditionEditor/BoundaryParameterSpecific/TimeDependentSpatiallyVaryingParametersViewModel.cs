using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific.TimeSeriesGeneration;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="TimeDependentSpatiallyVaryingParametersViewModel{TSpreading}"/> defines the view model for the
    /// TimeDependentParametersView
    /// of spatially varying wave boundaries.
    /// </summary>
    /// <typeparam name="TSpreading">The type of the spreading.</typeparam>
    public sealed class TimeDependentSpatiallyVaryingParametersViewModel<TSpreading> : TimeDependentParametersViewModel
        where TSpreading : IBoundaryConditionSpreading, new()
    {
        private readonly IGenerateSeries generateSeries;
        private readonly IReadOnlyDictionary<SupportPoint, TimeDependentParameters<TSpreading>> supportPointToParametersMapping;
        private IEnumerable<IFunction> timeDependentParametersFunctions;

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

            TimeDependentParametersFunctions = new[]
            {
                ObservedParameters.WaveEnergyFunction.UnderlyingFunction
            };
        }

        public override IEnumerable<IFunction> TimeDependentParametersFunctions
        {
            get => timeDependentParametersFunctions;
            protected set
            {
                if (Equals(value, timeDependentParametersFunctions))
                {
                    return;
                }

                timeDependentParametersFunctions = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the observed parameters.
        /// </summary>
        public TimeDependentParameters<TSpreading> ObservedParameters { get; }

        protected override void GenerateSeries()
        {
            // We temporarily set the TimeDependentParametersFunctions to a placeholder 
            // function when generating the time series. Generating the time series data
            // will generate a large number of events. Each event triggers a refresh of the
            // MultipleFunctionsView, which in turn leads to unacceptable performance.
            // By temporarily setting the view to a placeholder function, we will only trigger
            // one refresh when we set the TimeDependentParametersFunctions back, thus solving
            // the performance issue. Ideally this would be solved within the framework, however
            // the current state of the framework this would be too much of a risk.
            IEnumerable<IFunction> functionsToUpdate = TimeDependentParametersFunctions;
            TimeDependentParametersFunctions = new IFunction[]
            {
                new Function(),
            };

            generateSeries.Execute(ObservedParameters.WaveEnergyFunction,
                                   supportPointToParametersMapping.Values
                                                                  .Where(p => p != ObservedParameters)
                                                                  .Select(p => p.WaveEnergyFunction));
            TimeDependentParametersFunctions = functionsToUpdate;
        }
    }
}