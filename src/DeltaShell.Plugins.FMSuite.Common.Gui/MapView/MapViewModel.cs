using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using DelftTools.Utils.Guards;
using SharpMap;
using SharpMap.Api;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.MapView
{
    /// <summary>
    /// <see cref="MapViewModel"/> provides the view model for a thinly wrapped <see cref="Map"/>,
    /// to be used in WPF views.
    /// </summary>
    /// <seealso cref="IDisposable"/>
    /// <seealso cref="INotifyPropertyChanged"/>
    public class MapViewModel : IDisposable, INotifyPropertyChanged
    {
        private BitmapImage mapImage;
        private bool hasDisposed = false;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Creates a new <see cref="MapViewModel"/> with a default <see cref="Map"/>.
        /// </summary>
        public MapViewModel() : this(new Map()) {}

        /// <summary>
        /// Creates a new <see cref="MapViewModel"/> with the specified <paramref name="map"/>.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <c>null</c>.
        /// </exception>
        public MapViewModel(IMap map)
        {
            Ensure.NotNull(map, nameof(map));

            Map = map;
            RefreshView();
        }

        /// <summary>
        /// Gets the map associated with this <see cref="MapViewModel"/>.
        /// </summary>
        public IMap Map { get; }

        /// <summary>
        /// Gets or sets the map image.
        /// </summary>
        public BitmapImage MapImage
        {
            get => mapImage;
            protected set
            {
                if (value == mapImage)
                {
                    return;
                }

                mapImage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the height of the map in pixels.
        /// </summary>
        /// <remarks>
        /// This value should be equal to the rounded up
        /// integer value of the ActualHeight of the MapView.
        /// </remarks>
        public int MapHeight
        {
            get => Map.Size.Height;
            set
            {
                if (Map.Size.Height == value || value <= 0)
                {
                    return;
                }

                ResizeMap(new Size(Map.Size.Width, value));
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the width of the map in pixels.
        /// </summary>
        /// <remarks>
        /// This value should be equal to the rounded up
        /// integer value of the ActualWidth of the MapView.
        /// </remarks>
        public int MapWidth
        {
            get => Map.Size.Width;
            set
            {
                if (Map.Size.Width == value || value <= 0)
                {
                    return;
                }

                ResizeMap(new Size(value, Map.Size.Height));
                OnPropertyChanged();
            }
        }

        public void ResizeMap(Size mapSize)
        {
            Map.Size = mapSize;
            RefreshView();
        }

        /// <summary>
        /// Refreshes the <see cref="MapImage"/> by rendering the current state
        /// of the <see cref="Map"/>
        /// </summary>
        public void RefreshView() =>
            MapImage = ToBitmapImage(Map.Render());

        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
            if (hasDisposed)
            {
                return;
            }

            if (disposing && Map is IDisposable disposableMap)
            {
                disposableMap.Dispose();
            }

            hasDisposed = true;
        }

        /// <summary>
        /// Called when /[property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static BitmapImage ToBitmapImage(Image image)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();

            var ms = new MemoryStream();
            image.Save(ms, ImageFormat.Png);
            ms.Seek(0, SeekOrigin.Begin);
            bitmapImage.StreamSource = ms;

            bitmapImage.EndInit();

            return bitmapImage;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="MapViewModel"/> class.
        /// </summary>
        ~MapViewModel()
        {
            Dispose(false);
        }
    }
}