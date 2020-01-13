using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Annotations;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor
{
    /// <summary>
    /// <see cref="SupportPointViewModel" /> defines the view model for the support point view
    /// </summary>
    /// <seealso cref="INotifyPropertyChanged" />
    public class SupportPointViewModel : INotifyPropertyChanged
    {
        private readonly SupportPoint supportPoint;

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
            this.supportPoint = supportPoint;
        }

        private bool isEnabled;

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
            get => supportPoint.Distance;
            set
            {
                if (!(Math.Abs(supportPoint.Distance - value) > 1E-15))
                {
                    return;
                }

                supportPoint.Distance = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)

        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}