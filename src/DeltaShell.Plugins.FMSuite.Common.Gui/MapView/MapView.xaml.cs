using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using SharpMap.Api;
using Image = System.Drawing.Image;
using Size = System.Drawing.Size;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.MapView
{
    /// <summary>
    /// Interaction logic for MapView.xaml
    /// </summary>
    public partial class MapView : UserControl
    {
        /// <summary>
        /// The map property
        /// </summary>
        public static readonly DependencyProperty MapProperty =
            DependencyProperty.Register(nameof(Map),
                                        typeof(IMap),
                                        typeof(MapView),
                                        new PropertyMetadata(default(IMap), OnMapChanged));

        /// <summary>
        /// The should refresh property
        /// </summary>
        public static readonly DependencyProperty ShouldRefreshProperty =
            DependencyProperty.Register(nameof(ShouldRefresh),
                                        typeof(bool),
                                        typeof(MapView),
                                        new PropertyMetadata(default(bool), OnShouldRefresh));

        /// <summary>
        /// Initializes a new instance of the <see cref="MapView"/> class.
        /// </summary>
        public MapView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the map.
        /// </summary>
        public IMap Map
        {
            get => (IMap) GetValue(MapProperty);
            set => SetValue(MapProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether [should refresh].
        /// </summary>
        /// <value>
        /// <c>true</c> if [should refresh]; otherwise, <c>false</c>.
        /// </value>
        public bool ShouldRefresh
        {
            get => (bool) GetValue(ShouldRefreshProperty);
            set => SetValue(ShouldRefreshProperty, value);
        }

        private static void OnMapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(e.NewValue is IMap) ||
                !(d is MapView mapView))
            {
                return;
            }

            mapView.ResizeMap();
        }

        private static void OnShouldRefresh(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is MapView mapView))
            {
                return;
            }

            mapView.RefreshView();
        }

        private void ResizeMap()
        {
            var size = new Size((int) ActualWidth,
                                (int) ActualHeight);

            if (size.Width <= 0 || size.Height <= 0)
            {
                return;
            }

            Map.Size = size;
            RefreshView();
        }

        private void RefreshView()
        {
            Image mapImage = Map?.Render();

            if (mapImage != null)
            {
                Image.Source = ToBitmapImage(mapImage);
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Map == null)
            {
                return;
            }

            ResizeMap();
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
    }
}