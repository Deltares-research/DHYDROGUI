using System.ComponentModel;
using System.Runtime.CompilerServices;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor
{
    /// <summary>
    /// <see cref="BoundaryDescriptionViewModel"/> defines the view model for the boundary description view.
    /// </summary>
    /// <seealso cref="INotifyPropertyChanged"/>
    public sealed class BoundaryDescriptionViewModel : INotifyPropertyChanged
    {
        private readonly IWaveBoundary observedBoundary;
        private readonly IViewDataComponentFactory dataComponentFactory;
        private readonly IAnnounceDataComponentChanged announceDataComponentChanged;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Creates a new instance of the <see cref="BoundaryDescriptionViewModel"/>.
        /// </summary>
        /// <param name="observedBoundary">The observed boundary.</param>
        /// <param name="dataComponentFactory">
        /// The <see cref="IViewDataComponentFactory"/> used to construct the data components.
        /// </param>
        /// <param name="announceDataComponentChanged">
        /// The <see cref="IAnnounceDataComponentChanged"/> used to signal the data component has changed.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any of the parameters is <c>null</c>.
        /// </exception>
        public BoundaryDescriptionViewModel(IWaveBoundary observedBoundary,
                                            IViewDataComponentFactory dataComponentFactory,
                                            IAnnounceDataComponentChanged announceDataComponentChanged)
        {
            Ensure.NotNull(observedBoundary, nameof(observedBoundary));
            Ensure.NotNull(dataComponentFactory, nameof(dataComponentFactory));
            Ensure.NotNull(announceDataComponentChanged, nameof(announceDataComponentChanged));

            this.observedBoundary = observedBoundary;
            this.dataComponentFactory = dataComponentFactory;
            this.announceDataComponentChanged = announceDataComponentChanged;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name
        {
            get => observedBoundary.Name;
            set
            {
                if (observedBoundary.Name == value)
                {
                    return;
                }

                observedBoundary.Name = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the type of the forcing.
        /// </summary>
        public ForcingViewType ForcingType
        {
            get => dataComponentFactory.GetForcingType(observedBoundary.ConditionDefinition.DataComponent);
            set
            {
                if (value == ForcingType)
                {
                    return;
                }

                observedBoundary.ConditionDefinition.DataComponent =
                    dataComponentFactory.ConstructBoundaryConditionDataComponent(value,
                                                                                 SpatialDefinition,
                                                                                 GetSpreadingViewType());
                OnPropertyChanged();
                announceDataComponentChanged.AnnounceDataComponentChanged();
            }
        }

        /// <summary>
        /// Gets or sets the spatial definition.
        /// </summary>
        public SpatialDefinitionViewType SpatialDefinition
        {
            get => dataComponentFactory.GetSpatialDefinition(observedBoundary.ConditionDefinition.DataComponent);
            set
            {
                if (value == SpatialDefinition)
                {
                    return;
                }

                observedBoundary.ConditionDefinition.DataComponent =
                    dataComponentFactory.ConstructBoundaryConditionDataComponent(ForcingType,
                                                                                 value,
                                                                                 GetSpreadingViewType());
                OnPropertyChanged();
                announceDataComponentChanged.AnnounceDataComponentChanged();
            }
        }

        private DirectionalSpreadingViewType GetSpreadingViewType() => dataComponentFactory.GetDirectionalSpreadingViewType(observedBoundary.ConditionDefinition.DataComponent);

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}