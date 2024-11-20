using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf
{
    /// <summary>
    /// Class which synchronizes the updates between a <see cref="INotifyPropertyChanged"/>
    /// and <see cref="WpfGuiProperty"/>.
    /// </summary>
    public sealed class NotifyPropertyChangedWpfGuiPropertySynchronizer : IDisposable
    {
        private readonly INotifyPropertyChanged observable;
        private readonly List<PropertyChangedEventHandler> eventSubscribers;

        /// <summary>
        /// Creates a new instance of <see cref="NotifyPropertyChangedWpfGuiPropertySynchronizer"/>.
        /// </summary>
        /// <param name="observable">
        /// The <see cref="INotifyPropertyChanged"/> to synchronize
        /// the updates with.
        /// </param>
        public NotifyPropertyChangedWpfGuiPropertySynchronizer(INotifyPropertyChanged observable)
        {
            if (observable == null)
            {
                throw new ArgumentNullException(nameof(observable));
            }

            this.observable = observable;
            eventSubscribers = new List<PropertyChangedEventHandler>();
        }

        /// <summary>
        /// Synchronizes a collection of <see cref="WpfGuiProperty"/> to the updates of the observable of this class.
        /// </summary>
        /// <param name="properties">
        /// The collection of <see cref="WpfGuiProperty"/> to synchronize.
        /// </param>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="properties"/> is <c> null </c>. </exception>
        public void SynchronizeProperties(IEnumerable<WpfGuiProperty> properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            foreach (WpfGuiProperty guiProperty in properties)
            {
                PropertyChangedEventHandler eventHandler = (sender, args) => { guiProperty.RaisePropertyChangedEvents(); };
                observable.PropertyChanged += eventHandler;
                eventSubscribers.Add(eventHandler);
            }
        }

        public void Dispose()
        {
            foreach (PropertyChangedEventHandler eventSubscriber in eventSubscribers)
            {
                observable.PropertyChanged -= eventSubscriber;
            }
        }
    }
}