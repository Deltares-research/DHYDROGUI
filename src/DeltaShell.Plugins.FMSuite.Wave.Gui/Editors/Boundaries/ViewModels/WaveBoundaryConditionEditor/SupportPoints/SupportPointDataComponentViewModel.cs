using System;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.SupportPoints
{
    /// <summary>
    /// <see cref="SupportPointDataComponentViewModel" /> implements the methods
    /// for the Support Point Editor to interact with the wave boundary condition.
    /// It provides the abstraction, such that the geometry does not need to know
    /// about the construction of the relevant parameters.
    /// </summary>
    public class SupportPointDataComponentViewModel
    {
        private readonly IBoundaryParametersFactory parametersFactory;
        private readonly IWaveBoundaryConditionDefinition conditionDefinition;
        private readonly IAnnounceSelectedSupportPointDataChanged announceSelectedSupportPointDataChanged;

        private SupportPoint selectedSupportPoint;

        /// <summary>
        /// Creates a new <see cref="SupportPointDataComponentViewModel" />.
        /// </summary>
        /// <param name="conditionDefinition">The condition definition</param>
        /// <param name="parametersFactory">The parameters factory.</param>
        /// <param name="announceSelectedSupportPointDataChanged"> </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public SupportPointDataComponentViewModel(IWaveBoundaryConditionDefinition conditionDefinition,
                                                  IBoundaryParametersFactory parametersFactory,
                                                  IAnnounceSelectedSupportPointDataChanged announceSelectedSupportPointDataChanged)
        {
            Ensure.NotNull(conditionDefinition, nameof(conditionDefinition));
            Ensure.NotNull(parametersFactory, nameof(parametersFactory));
            Ensure.NotNull(announceSelectedSupportPointDataChanged, nameof(announceSelectedSupportPointDataChanged));

            this.conditionDefinition = conditionDefinition;
            this.parametersFactory = parametersFactory;
            this.announceSelectedSupportPointDataChanged = announceSelectedSupportPointDataChanged;
        }

        /// <summary>
        /// Gets the observed data component.
        /// </summary>
        public ISpatiallyDefinedDataComponent ObservedDataComponent => conditionDefinition.DataComponent;

        /// <summary>
        /// Gets or sets the selected support point.
        /// </summary>
        public SupportPoint SelectedSupportPoint
        {
            get => selectedSupportPoint;
            set
            {
                selectedSupportPoint = value;
                AnnounceSelectedSupportPointDataChanged(SelectedSupportPoint);
            }
        }

        /// <summary>
        /// Determines whether this instance is enabled.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </returns>
        public bool IsEnabled() =>
            ObservedDataComponent is SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>> ||
            ObservedDataComponent is SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>> ||
            ObservedDataComponent is SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>> ||
            ObservedDataComponent is SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>> ||
            ObservedDataComponent is SpatiallyVaryingDataComponent<FileBasedParameters>;

        /// <summary>
        /// Determines whether the specified <paramref name="supportPoint" /> is enabled.
        /// </summary>
        /// <param name="supportPoint">The support point.</param>
        /// <returns>
        /// <c>true</c> if the specified support point is enabled; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="supportPoint" /> is <c>null</c>.
        /// </exception>
        public bool IsEnabledSupportPoint(SupportPoint supportPoint)
        {
            Ensure.NotNull(supportPoint, nameof(supportPoint));

            switch (ObservedDataComponent)
            {
                case SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>> constantComponent:
                    return constantComponent.Data.ContainsKey(supportPoint);
                case SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>> constantComponent:
                    return constantComponent.Data.ContainsKey(supportPoint);
                case SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>> timeDependentComponent:
                    return timeDependentComponent.Data.ContainsKey(supportPoint);
                case SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>> timeDependentComponent:
                    return timeDependentComponent.Data.ContainsKey(supportPoint);
                case SpatiallyVaryingDataComponent<FileBasedParameters> fileBasedComponent:
                    return fileBasedComponent.Data.ContainsKey(supportPoint);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Adds a set of default parameters linked to the provided <paramref name="supportPoint" />.
        /// </summary>
        /// <param name="supportPoint">The support point.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="supportPoint" /> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="ObservedDataComponent" /> is not supported.
        /// </exception>
        public void AddDefaultParameters(SupportPoint supportPoint)
        {
            Ensure.NotNull(supportPoint, nameof(supportPoint));

            switch (ObservedDataComponent)
            {
                case SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>> constantComponent:
                    constantComponent.AddParameters(supportPoint,
                                                    parametersFactory.ConstructDefaultConstantParameters<PowerDefinedSpreading>());
                    break;
                case SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>> constantComponent:
                    constantComponent.AddParameters(supportPoint,
                                                    parametersFactory.ConstructDefaultConstantParameters<DegreesDefinedSpreading>());
                    break;
                case SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>> timeDependentComponent:
                    timeDependentComponent.AddParameters(supportPoint,
                                                         parametersFactory.ConstructDefaultTimeDependentParameters<PowerDefinedSpreading>());
                    break;
                case SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>> timeDependentComponent:
                    timeDependentComponent.AddParameters(supportPoint,
                                                         parametersFactory.ConstructDefaultTimeDependentParameters<DegreesDefinedSpreading>());
                    break;
                case SpatiallyVaryingDataComponent<FileBasedParameters> fileBasedComponent:
                    fileBasedComponent.AddParameters(supportPoint,
                                                     parametersFactory.ConstructDefaultFileBasedParameters());
                    break;
                default:
                    throw new InvalidOperationException("Currently stored data component is not supported.");
            }

            if (supportPoint == SelectedSupportPoint)
            {
                AnnounceSelectedSupportPointDataChanged(SelectedSupportPoint);
            }
        }

        /// <summary>
        /// Removes the parameters associated with the <paramref name="supportPoint" />.
        /// </summary>
        /// <param name="supportPoint">The support point.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="supportPoint" /> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="ObservedDataComponent" /> is not supported.
        /// </exception>
        public void RemoveParameters(SupportPoint supportPoint)
        {
            Ensure.NotNull(supportPoint, nameof(supportPoint));

            switch (ObservedDataComponent)
            {
                case SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>> constantComponent:
                    constantComponent.RemoveSupportPoint(supportPoint);
                    break;
                case SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>> constantComponent:
                    constantComponent.RemoveSupportPoint(supportPoint);
                    break;
                case SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>> timeDependentComponent:
                    timeDependentComponent.RemoveSupportPoint(supportPoint);
                    break;
                case SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>> timeDependentComponent:
                    timeDependentComponent.RemoveSupportPoint(supportPoint);
                    break;
                case SpatiallyVaryingDataComponent<FileBasedParameters> fileBasedComponent:
                    fileBasedComponent.RemoveSupportPoint(supportPoint);
                    break;
                default:
                    throw new InvalidOperationException("Currently stored data component is not supported.");
            }

            if (supportPoint == SelectedSupportPoint)
            {
                AnnounceSelectedSupportPointDataChanged(SelectedSupportPoint);
            }
        }

        /// <summary>
        /// Replaces the old support point with the new support point.
        /// </summary>
        /// <param name="oldSupportPoint">The old support point.</param>
        /// <param name="newSupportPoint">The new support point.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="ObservedDataComponent" /> is not supported or
        /// <paramref name="oldSupportPoint" /> does not exists within the data component or
        /// <paramref name="newSupportPoint" /> already exists within the data.
        /// </exception>
        public void ReplaceSupportPoint(SupportPoint oldSupportPoint,
                                        SupportPoint newSupportPoint)
        {
            switch (ObservedDataComponent)
            {
                case SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>> constantComponent:
                    constantComponent.ReplaceSupportPoint(oldSupportPoint, newSupportPoint);
                    break;
                case SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>> constantComponent:
                    constantComponent.ReplaceSupportPoint(oldSupportPoint, newSupportPoint);
                    break;
                case SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>> timeDependentComponent:
                    timeDependentComponent.ReplaceSupportPoint(oldSupportPoint, newSupportPoint);
                    break;
                case SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>> timeDependentComponent:
                    timeDependentComponent.ReplaceSupportPoint(oldSupportPoint, newSupportPoint);
                    break;
                case SpatiallyVaryingDataComponent<FileBasedParameters> fileDependentComponent:
                    fileDependentComponent.ReplaceSupportPoint(oldSupportPoint, newSupportPoint);
                    break;
                default:
                    throw new InvalidOperationException("Currently stored data component is not supported.");
            }
        }

        private void AnnounceSelectedSupportPointDataChanged(SupportPoint supportPoint) =>
            announceSelectedSupportPointDataChanged.AnnounceSelectedSupportPointDataChanged(supportPoint);
    }
}