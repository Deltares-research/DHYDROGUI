using System;
using System.Collections.Generic;
using System.ComponentModel;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.FMSuite.Common.Gui
{
    /// <summary>
    /// <see cref="NotifyPropertyChangedEventPropagator"/> is responsible for propagating
    /// notify property changed events from an observed object to some property changed
    /// action given some property name mapping.
    /// </summary>
    /// <remarks>
    /// The goal of this class is to reduce some common boiler plate of view models which
    /// observe entity classes.
    /// </remarks>
    /// <seealso cref="IDisposable" />
    public sealed class NotifyPropertyChangedEventPropagator : IDisposable
    {
        private readonly INotifyPropertyChanged observedObject;
        private Action<string> propertyChangedAction;
        private readonly IReadOnlyDictionary<string, string> propertyMapping;

        /// <summary>
        /// Creates a new <see cref="NotifyPropertyChangedEventPropagator"/>.
        /// </summary>
        /// <param name="observedObject">The observed object.</param>
        /// <param name="propertyChangedAction">The property changed action.</param>
        /// <param name="propertyMapping">The property mapping.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any argument is <c>null</c>.
        /// </exception>
        public NotifyPropertyChangedEventPropagator(INotifyPropertyChanged observedObject,
                                                    Action<string> propertyChangedAction,
                                                    IReadOnlyDictionary<string, string> propertyMapping)
        {
            Ensure.NotNull(observedObject, nameof(observedObject));
            Ensure.NotNull(propertyChangedAction, nameof(propertyChangedAction));
            Ensure.NotNull(propertyMapping, nameof(propertyMapping));

            this.observedObject = observedObject;
            this.propertyChangedAction = propertyChangedAction;
            this.propertyMapping = propertyMapping;

            Subscribe();
        }

        private void Subscribe() => observedObject.PropertyChanged += PropagatePropertyChanged;
        private void Unsubscribe() => observedObject.PropertyChanged -= PropagatePropertyChanged;

        private void PropagatePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!Equals(sender, observedObject) || !IsObservedProperty(e.PropertyName))
            {
                return;
            }

            propertyChangedAction(propertyMapping[e.PropertyName]);
        }

        private bool IsObservedProperty(string propertyName) =>
            propertyMapping.ContainsKey(propertyName);

        public void Dispose()
        {
            if (hasDisposed)
            {
                return;
            }

            Unsubscribe();
            propertyChangedAction = null;
            hasDisposed = true;
        }

        private bool hasDisposed = false;
    }
}