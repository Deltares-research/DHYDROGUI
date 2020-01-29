using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DeltaShell.NGHS.Common;
using DeltaShell.NGHS.Common.Eventing;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.SupportPoints
{
    /// <summary>
    /// <see cref="SupportPointViewModel" /> defines the view model for the support point view
    /// </summary>
    /// <seealso cref="INotifyPropertyChanged" />
    public class SupportPointViewModel : INotifyPropertyChanged
    {
        private const double comparisonTolerance = 1E-15;
        private readonly SupportPointDataComponentViewModel dataComponentViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportPointViewModel" /> class.
        /// </summary>
        /// <param name="supportPoint"> The support point. </param>
        /// <param name="isEditable"> Whether the view model should be editable.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="supportPoint" /> is <c> null </c>.
        /// </exception>
        public SupportPointViewModel(SupportPoint supportPoint, 
                                     SupportPointDataComponentViewModel dataComponentViewModel,
                                     bool isEditable = true) 
        {
            Ensure.NotNull(supportPoint, nameof(supportPoint));
            Ensure.NotNull(dataComponentViewModel, nameof(dataComponentViewModel));

            SupportPoint = supportPoint;
            IsEditable = isEditable;
            this.dataComponentViewModel = dataComponentViewModel;

            isEnabled = this.dataComponentViewModel.IsEnabledSupportPoint(SupportPoint);
        }

        /// <summary>
        /// Gets the support point.
        /// </summary>
        /// <value>
        /// The support point.
        /// </value>
        public SupportPoint SupportPoint { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        /// <c> true </c> if this instance is enabled; otherwise, <c> false </c>.
        /// </value>
        /// <remarks>
        /// Enabling the support point will trigger the corresponding condition data
        /// to be created, or removed.
        /// </remarks>
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (IsEnabled == value)
                {
                    return;
                }

                UpdateModelData(value);

                isEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool isEnabled;

        private void UpdateModelData(bool shouldBeEnabled)
        {
            if (!dataComponentViewModel.IsEnabled())
            {
                if (shouldBeEnabled)
                {
                    throw new InvalidOperationException("You cannot enable a SupportPoint when dealing with uniform data.");
                }

                return;
            }
            
            if (shouldBeEnabled)
            {
                EnableSupportPoint();
            }
            else
            {
                DisableSupportPoint();
            }
        }

        private void EnableSupportPoint() =>
            dataComponentViewModel.AddDefaultParameters(SupportPoint);

        private void DisableSupportPoint() => 
            dataComponentViewModel.RemoveParameters(SupportPoint);

        /// <summary>
        /// Gets a value indicating whether this instance is editable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is editable; otherwise, <c>false</c>.
        /// </value>
        public bool IsEditable { get; }

        /// <summary>
        /// Gets or sets the distance.
        /// </summary>
        /// <value>
        /// The distance.
        /// </value>
        public double Distance
        {
            get => SupportPoint.Distance;
            set
            {
                if (Math.Abs(SupportPoint.Distance - value) <= comparisonTolerance)
                {
                    return;
                }

                double originalValue = SupportPoint.Distance;
                SupportPoint.Distance = value;
                OnPropertyChanged(originalValue);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(object originalValue = null, [CallerMemberName] string propertyName = null)

        {
            PropertyChanged?.Invoke(this, new PropertyChangedExtendedEventArgs(propertyName, originalValue));
        }
    }
}