using System;
using System.Windows;
using System.Windows.Controls;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.MapView
{
    /// <summary>
    /// Interaction logic for MapView.xaml
    /// </summary>
    public partial class MapView : UserControl, IDisposable
    {
        public MapView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, 
                                          DependencyPropertyChangedEventArgs e)
        {
            if (!(Equals(sender, this) && e.NewValue is MapViewModel mapViewModel))
                return;

            mapViewModel.ResizeMap(new System.Drawing.Size((int)ActualWidth, 
                                                           (int)ActualHeight));
        }


        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && DataContext is IDisposable disposableContext)
            {
                disposableContext.Dispose();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
