using System;
using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Functions.Binding;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Common.Gui;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.ViewModels.TimeFrameEditor;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame.DeltaShell.Plugins.FMSuite.Wave.TimeFrame;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.ViewModels
{
    /// <summary>
    /// <see cref="TimeFrameEditorViewModel"/> provides the view model for the
    /// <see cref="Views.TimeFrameEditorView"/>.
    /// </summary>
    /// <seealso cref="INotifyPropertyChanged" />
    /// <seealso cref="IDisposable" />
    public sealed class TimeFrameEditorViewModel : INotifyPropertyChanged,
                                                   IDisposable
    {
        private readonly IReadOnlyDictionary<string, string> propertyMapping = new Dictionary<string, string>
        {
            {nameof(ITimeFrameData.HydrodynamicsInputDataType), nameof(HydrodynamicsInputDataType)},
            {nameof(ITimeFrameData.WindInputDataType), nameof(WindInputDataType)},
        };

        // Note this object takes care of the event propagation for this class.
        private readonly NotifyPropertyChangedEventPropagator eventPropagator;
        private readonly ITimeFrameData timeFrameData;

        /// <summary>
        /// Creates a new <see cref="TimeFrameEditorViewModel"/>.
        /// </summary>
        /// <param name="timeFrameData">The time frame data.</param>
        /// <param name="hydrodynamicsConstantsViewModel">The hydrodynamics constants view model.</param>
        /// <param name="windConstantsViewModel">The wind constants view model.</param>
        /// <param name="windFilesViewModel">The wind files view model.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public TimeFrameEditorViewModel(ITimeFrameData timeFrameData,
                                        HydrodynamicsConstantsViewModel hydrodynamicsConstantsViewModel,
                                        WindConstantsViewModel windConstantsViewModel,
                                        WindFilesViewModel windFilesViewModel)

        {
            Ensure.NotNull(timeFrameData, nameof(timeFrameData));
            Ensure.NotNull(hydrodynamicsConstantsViewModel, nameof(hydrodynamicsConstantsViewModel));
            Ensure.NotNull(windConstantsViewModel, nameof(windConstantsViewModel));
            Ensure.NotNull(windFilesViewModel, nameof(windFilesViewModel));

            this.timeFrameData = timeFrameData;
            HydrodynamicsConstantsViewModel = hydrodynamicsConstantsViewModel;
            WindConstantsViewModel = windConstantsViewModel;
            WindFilesViewModel = windFilesViewModel;

            DataFunctionBindingList = new FunctionBindingList(timeFrameData.TimeVaryingData);

            eventPropagator = new NotifyPropertyChangedEventPropagator((INotifyPropertyChanged)timeFrameData,
                                                                       OnPropertyChanged,
                                                                       propertyMapping);
        }

        /// <summary>
        /// Gets or sets the type of the hydrodynamics input data.
        /// </summary>
        public HydrodynamicsInputDataType HydrodynamicsInputDataType
        {
            get => timeFrameData.HydrodynamicsInputDataType;
            set
            {
                if (HydrodynamicsInputDataType != value)
                {
                    timeFrameData.HydrodynamicsInputDataType = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the type of the wind input data.
        /// </summary>
        public WindInputDataType WindInputDataType
        {
            get => timeFrameData.WindInputDataType;
            set
            {
                if (WindInputDataType != value)
                {
                    timeFrameData.WindInputDataType = value;
                }
            }
        }

        /// <summary>
        /// Gets the hydrodynamics constants view model.
        /// </summary>
        public HydrodynamicsConstantsViewModel HydrodynamicsConstantsViewModel { get; }

        /// <summary>
        /// Gets the wind constants view model.
        /// </summary>
        public WindConstantsViewModel WindConstantsViewModel { get; }

        /// <summary>
        /// Gets the wind files view model.
        /// </summary>
        public WindFilesViewModel WindFilesViewModel { get; }

        /// <summary>
        /// Gets or sets the data function binding list.
        /// </summary>
        public IFunctionBindingList DataFunctionBindingList { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            if (hasDisposed)
            {
                return;
            }

            eventPropagator.Dispose();
            HydrodynamicsConstantsViewModel?.Dispose();
            WindConstantsViewModel?.Dispose();
            WindFilesViewModel?.Dispose();
            (DataFunctionBindingList as IDisposable)?.Dispose();
        }

        private bool hasDisposed = false;
    }
}