using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor
{
    /// <summary>
    /// <see cref="BoundaryDescriptionViewModel"/> defines the view model for the boundary description view.
    /// </summary>
    /// <seealso cref="INotifyPropertyChanged" />
    public class BoundaryDescriptionViewModel : INotifyPropertyChanged
    {
        private readonly IWaveBoundary observedBoundary;
        private readonly IViewDataComponentFactory dataComponentFactory;

        /// <summary>
        /// Creates a new instance of the <see cref="BoundaryDescriptionViewModel"/>.
        /// </summary>
        /// <param name="observedBoundary">The observed boundary.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="observedBoundary"/> is <c>null</c>.
        /// </exception>
        public BoundaryDescriptionViewModel(IWaveBoundary observedBoundary, 
                                            IViewDataComponentFactory dataComponentFactory)
        {
            Ensure.NotNull(observedBoundary, nameof(observedBoundary));
            Ensure.NotNull(dataComponentFactory, nameof(dataComponentFactory));

            this.observedBoundary = observedBoundary;
            this.dataComponentFactory = dataComponentFactory;

            forcingType = 
                this.dataComponentFactory.GetForcingType(observedBoundary.ConditionDefinition
                                                                    .DataComponent);
            spatialDefinition = 
                this.dataComponentFactory.GetSpatialDefinition(observedBoundary.ConditionDefinition
                                                                          .DataComponent);
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
            get => forcingType;
            set
            {
                if (value == ForcingType)
                {
                    return;
                }

                forcingType = value;
                OnPropertyChanged();
            }
        }

        private ForcingViewType forcingType = ForcingViewType.Constant;

        /// <summary>
        /// Gets or sets the spatial definition.
        /// </summary>
        public SpatialDefinitionViewType SpatialDefinition
        {
            get => spatialDefinition;
            set
            {
                if (value == SpatialDefinition)
                {
                    return;
                }

                spatialDefinition = value;
                OnPropertyChanged();
            }
        }

        private SpatialDefinitionViewType spatialDefinition = SpatialDefinitionViewType.Uniform;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}