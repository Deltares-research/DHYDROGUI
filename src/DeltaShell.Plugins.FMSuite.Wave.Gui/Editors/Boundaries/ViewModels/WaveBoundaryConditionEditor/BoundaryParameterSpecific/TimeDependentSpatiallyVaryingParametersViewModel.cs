using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Functions;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
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
        private readonly GenerateSeries generateSeries = 
            new GenerateSeries(new GenerateSeriesDialogHelper());
        private readonly IReadOnlyDictionary<SupportPoint, TimeDependentParameters<TSpreading>> supportPointToParametersMapping;

        public TimeDependentSpatiallyVaryingParametersViewModel(TimeDependentParameters<TSpreading> parameters, 
                                                                IReadOnlyDictionary<SupportPoint, TimeDependentParameters<TSpreading>> supportPointToParametersMapping)
                                                                
        {
            Ensure.NotNull(parameters, nameof(parameters));
            Ensure.NotNull(supportPointToParametersMapping, nameof(supportPointToParametersMapping));

            ObservedParameters = parameters;
            this.supportPointToParametersMapping = supportPointToParametersMapping;
        }

        /// <summary>
        /// Gets the observed parameters.
        /// </summary>
        public TimeDependentParameters<TSpreading> ObservedParameters { get; }

        public override IEnumerable<IFunction> TimeDependentParametersFunctions =>
            new[] {ObservedParameters.WaveEnergyFunction.UnderlyingFunction};

        protected override void GenerateSeries(IWin32Window owner) => 
            generateSeries.Execute(owner, 
                                   ObservedParameters.WaveEnergyFunction, 
                                   supportPointToParametersMapping.Values.Select(p => p.WaveEnergyFunction));
    }
}