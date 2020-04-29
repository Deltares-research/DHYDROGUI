using System.Windows;
using DelftTools.Utils.Guards;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.MapView
{
    /// <summary>
    /// <see cref="ActualSizeObserver"/> allows for observing the ActualHeight and
    /// ActualWidth properties. These properties are considered read-only and
    /// as such it is not possible to bind them one way to source, in order to
    /// push the values to the view model. This attached behaviour circumvents
    /// this problem.
    /// </summary>
    public static class ActualSizeObserver
    {
        /// <summary>
        /// The observe property.
        /// </summary>
        /// <remarks>
        /// When set to true, the ObservedWidth and ObservedHeight changes are
        /// pushed synced whenever a SizeChanged event occurs.
        /// </remarks>
        public static readonly DependencyProperty ObserveProperty = DependencyProperty.RegisterAttached(
            "Observe",
            typeof(bool),
            typeof(ActualSizeObserver),
            new FrameworkPropertyMetadata(OnObserveChanged));

        /// <summary>
        /// The observed width property.
        /// </summary>
        /// <remarks>
        /// This property is synced with the ActualWidth property.
        /// </remarks>
        public static readonly DependencyProperty ObservedWidthProperty = DependencyProperty.RegisterAttached(
            "ObservedWidth",
            typeof(double),
            typeof(ActualSizeObserver));

        /// <summary>
        /// The observed height property
        /// </summary>
        /// <remarks>
        /// This property is synced with the ActualHeight property.
        /// </remarks>
        public static readonly DependencyProperty ObservedHeightProperty = DependencyProperty.RegisterAttached(
            "ObservedHeight",
            typeof(double),
            typeof(ActualSizeObserver));

        /// <summary>
        /// Gets whether the size is observed.
        /// </summary>
        /// <param name="frameworkElement">The framework element.</param>
        /// <returns>
        /// Whether the size is observed.
        /// </returns>
        public static bool GetObserve(FrameworkElement frameworkElement)
        {
            Ensure.NotNull(frameworkElement, nameof(frameworkElement));
            return (bool)frameworkElement.GetValue(ObserveProperty);
        }

        /// <summary>
        /// Sets whether the size is observed.
        /// </summary>
        /// <param name="frameworkElement">The framework element.</param>
        /// <param name="observe">if set to <c>true</c> [observe] size changes.</param>
        public static void SetObserve(FrameworkElement frameworkElement, bool observe)
        {
            Ensure.NotNull(frameworkElement, nameof(frameworkElement));
            frameworkElement.SetValue(ObserveProperty, observe);
        }

        /// <summary>
        /// Gets the actual width of the observed framework element.
        /// </summary>
        /// <param name="frameworkElement">The framework element.</param>
        /// <returns>
        /// The actual width of the observed framework element.
        /// </returns>
        public static double GetObservedWidth(FrameworkElement frameworkElement)
        {
            Ensure.NotNull(frameworkElement, nameof(frameworkElement));
            return (double)frameworkElement.GetValue(ObservedWidthProperty);
        }

        /// <summary>
        /// Sets the actual width of the observed framework element.
        /// </summary>
        /// <param name="frameworkElement">The framework element.</param>
        /// <param name="observedWidth">Width of the observed framework element.</param>
        public static void SetObservedWidth(FrameworkElement frameworkElement, double observedWidth)
        {
            Ensure.NotNull(frameworkElement, nameof(frameworkElement));
            frameworkElement.SetValue(ObservedWidthProperty, observedWidth);
        }

        /// <summary>
        /// Gets the actual height of the observed framework element.
        /// </summary>
        /// <param name="frameworkElement">The framework element.</param>
        /// <returns>
        /// The actual height of the observed framework element.
        /// </returns>
        public static double GetObservedHeight(FrameworkElement frameworkElement)
        {
            Ensure.NotNull(frameworkElement, nameof(frameworkElement));
            return (double)frameworkElement.GetValue(ObservedHeightProperty);
        }

        /// <summary>
        /// Sets the actual height of the observed framework element.
        /// </summary>
        /// <param name="frameworkElement">The framework element.</param>
        /// <param name="observedHeight">Height of the observed framework element.</param>
        public static void SetObservedHeight(FrameworkElement frameworkElement, double observedHeight)
        {
            Ensure.NotNull(frameworkElement, nameof(frameworkElement));
            frameworkElement.SetValue(ObservedHeightProperty, observedHeight);
        }

        private static void OnObserveChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var frameworkElement = (FrameworkElement)dependencyObject;

            if ((bool)e.NewValue)
            {
                frameworkElement.SizeChanged += OnFrameworkElementSizeChanged;
                UpdateObservedSizesForFrameworkElement(frameworkElement);
            }
            else
            {
                frameworkElement.SizeChanged -= OnFrameworkElementSizeChanged;
            }
        }

        private static void OnFrameworkElementSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateObservedSizesForFrameworkElement((FrameworkElement)sender);
        }

        private static void UpdateObservedSizesForFrameworkElement(FrameworkElement frameworkElement)
        {
            frameworkElement.SetCurrentValue(ObservedWidthProperty, frameworkElement.ActualWidth);
            frameworkElement.SetCurrentValue(ObservedHeightProperty, frameworkElement.ActualHeight);
        }
    }
}