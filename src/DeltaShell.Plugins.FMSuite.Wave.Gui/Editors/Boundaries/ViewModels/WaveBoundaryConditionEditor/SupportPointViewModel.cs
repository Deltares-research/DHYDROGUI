using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor
{
    /// <summary>
    /// <see cref="SupportPointViewModel" /> defines the view model for the support point view
    /// </summary>
    /// <seealso cref="INotifyPropertyChanged" />
    public class SupportPointViewModel : INotifyPropertyChanged
    {
        private bool isEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportPointViewModel" /> class.
        /// </summary>
        /// <param name="supportPoint"> The support point. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="supportPoint" /> is <c> null </c>.
        /// </exception>
        public SupportPointViewModel(SupportPoint supportPoint)
        {
            Ensure.NotNull(supportPoint, nameof(supportPoint));
            SupportPoint = supportPoint;
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
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (isEnabled == value)
                {
                    return;
                }

                isEnabled = value;
                OnPropertyChanged();
            }
        }

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
                if (!(Math.Abs(SupportPoint.Distance - value) > 1E-15))
                {
                    return;
                }

                SupportPoint.Distance = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)

        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}