using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui
{
    /// <summary>
    /// <see cref="CoordinateSystemAwareFeature2DCollection{TSource}"/> defines a
    /// <see cref="Feature2DCollection"/> which observes some <typeparamref name="TSource"/>
    /// and synchronizes its coordinate system with itself.
    /// </summary>
    /// <typeparam name="TSource">The source class of the coordinate system.</typeparam>
    /// <remarks>
    /// It is expected that the <typeparamref name="TSource"/> calls its
    /// <see cref="INotifyPropertyChanged.PropertyChanged"/> when its
    /// <see cref="IHasCoordinateSystem.CoordinateSystem"/> has changed.
    /// </remarks>
    public sealed class CoordinateSystemAwareFeature2DCollection<TSource> : Feature2DCollection
        where TSource : class, IHasCoordinateSystem, INotifyPropertyChanged
    {
        private TSource coordinateSystemSource;

        /// <summary>
        /// Gets or sets the <see cref="TSource"/> which is observed for changes in the
        /// coordinate system.
        /// </summary>
        public TSource CoordinateSystemSource
        {
            get => coordinateSystemSource;
            set
            {
                if (coordinateSystemSource != null)
                {
                    coordinateSystemSource.PropertyChanged -= OnCoordinateSystemSystemChanged;
                }

                coordinateSystemSource = value;

                if (coordinateSystemSource != null)
                {
                    coordinateSystemSource.PropertyChanged += OnCoordinateSystemSystemChanged;
                }
            }
        }

        /// <summary>
        /// Initializes this <see cref="CoordinateSystemAwareFeature2DCollection{TSource}"/>
        /// with the given parameters.
        /// </summary>
        /// <typeparam name="T">The type of features.</typeparam>
        /// <param name="observedFeatures">The observed features.</param>
        /// <param name="featureTypeName">The name of the features.</param>
        /// <param name="source">The source with which the coordinate system is synced.</param>
        /// <param name="modelName">The name of the model type.</param>
        /// <returns>The initialized <see cref="CoordinateSystemAwareFeature2DCollection{TSource}"/></returns>
        /// <remarks>
        /// The <paramref name="modelName"/> should be set with nameof, e.g.:
        /// <code>
        /// nameof(WaterFlowFMModel)
        /// </code>
        /// </remarks>
        public CoordinateSystemAwareFeature2DCollection<TSource> Init<T>(
            IEventedList<T> observedFeatures, 
            string featureTypeName, 
            TSource source,
            string modelName)
        {
            CoordinateSystemSource = source;
            Init(observedFeatures, featureTypeName, modelName, CoordinateSystemSource.CoordinateSystem);
            return this;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                // Ensures the subscriptions are cleaned up.
                CoordinateSystemSource = null;
            }
        }

        private void OnCoordinateSystemSystemChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.Equals(e.PropertyName, nameof(IHasCoordinateSystem.CoordinateSystem)))
            {
                CoordinateSystem = CoordinateSystemSource.CoordinateSystem;
            }
        }
    }
}