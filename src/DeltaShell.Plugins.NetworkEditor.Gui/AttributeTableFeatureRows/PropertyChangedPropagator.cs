using System;
using System.ComponentModel;
using DelftTools.Utils;
using DelftTools.Utils.Guards;

namespace DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows
{
    /// <summary>
    /// Base class that propagates the <see cref="INotifyPropertyChanged.PropertyChanged"/> event of an object.
    /// </summary>
    public abstract class PropertyChangedPropagator : INotifyPropertyChange, IDisposable
    {
        private readonly INotifyPropertyChanged notifyPropertyChanged;

        /// <summary>
        /// Initialize a new instance of the <see cref="PropertyChangedPropagator"/> class.
        /// </summary>
        /// <param name="notifyPropertyChanged"> An objects that implements <see cref="INotifyPropertyChanged"/>. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="notifyPropertyChanged"/> is <c>null</c>.
        /// </exception>
        protected PropertyChangedPropagator(INotifyPropertyChanged notifyPropertyChanged)
        {
            Ensure.NotNull(notifyPropertyChanged, nameof(notifyPropertyChanged));

            this.notifyPropertyChanged = notifyPropertyChanged;
            this.notifyPropertyChanged.PropertyChanged += InvokePropertyChanged;
        }

        private void InvokePropertyChanged(object sender, PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <inheritdoc/>
        public event PropertyChangingEventHandler PropertyChanging;

        /// <inheritdoc/>
        [Browsable(false)] // makes sure this property is not shown in the table view
        public bool HasParent { get; set; }

        ~PropertyChangedPropagator()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                notifyPropertyChanged.PropertyChanged -= InvokePropertyChanged;
            }
        }
    }
}