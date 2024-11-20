using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DelftTools.Controls.Wpf.Commands;
using DelftTools.Functions;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.Laterals.TimeSeriesGeneration;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.Laterals.ViewModels
{
    /// <summary>
    /// View model for the <see cref="Views.LateralDefinitionView"/>
    /// </summary>
    public sealed class LateralDefinitionViewModel : INotifyPropertyChanged
    {
        private readonly LateralDefinition lateralDefinition;
        private readonly ITimeSeriesGeneratorDialogService timeSeriesGeneratorDialogService;

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="LateralDefinitionViewModel"/> class.
        /// </summary>
        /// <param name="lateralDefinition">The lateral definition.</param>
        /// <param name="timeSeriesGeneratorDialogService"> The service to make use of the time series generator dialog.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="lateralDefinition"/> or <paramref name="timeSeriesGeneratorDialogService"/>
        /// is <c>null</c>.
        /// </exception>
        public LateralDefinitionViewModel(LateralDefinition lateralDefinition,
                                          ITimeSeriesGeneratorDialogService timeSeriesGeneratorDialogService)
        {
            Ensure.NotNull(lateralDefinition, nameof(lateralDefinition));
            Ensure.NotNull(timeSeriesGeneratorDialogService, nameof(timeSeriesGeneratorDialogService));

            this.lateralDefinition = lateralDefinition;
            this.timeSeriesGeneratorDialogService = timeSeriesGeneratorDialogService;
            Functions = new[] { lateralDefinition.Discharge.TimeSeries };
            GenerateTimeSeriesCommand = new RelayCommand(_ => GenerateSeries());
        }

        /// <summary>
        /// The functions to bind to the <see cref="Views.LateralDefinitionView.MultipleFunctionView"/>.
        /// </summary>
        public IEnumerable<IFunction> Functions { get; }

        /// <summary>
        /// Gets or sets the type of the discharge data.
        /// </summary>
        public ViewLateralDischargeType DischargeType
        {
            get => ConvertToViewLateralDischargeType(lateralDefinition.Discharge.Type);
            set
            {
                lateralDefinition.Discharge.Type = ConvertFromViewLateralDischargeType(value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the constant discharge.
        /// </summary>
        public double ConstantDischarge
        {
            get => lateralDefinition.Discharge.Constant;
            set
            {
                lateralDefinition.Discharge.Constant = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the unit of the discharge.
        /// </summary>
        public string DischargeUnit => lateralDefinition.Discharge.TimeSeries.DischargeComponent.Unit.Symbol;

        /// <summary>
        /// Gets the <see cref="ICommand"/> to generate the time series.
        /// </summary>
        public ICommand GenerateTimeSeriesCommand { get; }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void GenerateSeries()
        {
            DateTime start = DateTime.Today;
            TimeSpan timeStep = TimeSpan.FromDays(1);
            DateTime stop = start.Add(timeStep);
            timeSeriesGeneratorDialogService.Execute(start, stop, timeStep, lateralDefinition.Discharge.TimeSeries);
        }

        private static LateralDischargeType ConvertFromViewLateralDischargeType(ViewLateralDischargeType dischargeType)
        {
            switch (dischargeType)
            {
                case ViewLateralDischargeType.Constant:
                    return LateralDischargeType.Constant;
                case ViewLateralDischargeType.TimeSeries:
                    return LateralDischargeType.TimeSeries;
                case ViewLateralDischargeType.RealTime:
                    return LateralDischargeType.RealTime;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dischargeType), dischargeType, null);
            }
        }

        private static ViewLateralDischargeType ConvertToViewLateralDischargeType(LateralDischargeType dischargeType)
        {
            switch (dischargeType)
            {
                case LateralDischargeType.Constant:
                    return ViewLateralDischargeType.Constant;
                case LateralDischargeType.TimeSeries:
                    return ViewLateralDischargeType.TimeSeries;
                case LateralDischargeType.RealTime:
                    return ViewLateralDischargeType.RealTime;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dischargeType), dischargeType, null);
            }
        }
    }
}