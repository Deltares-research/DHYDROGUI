using System;
using System.Drawing;
using System.Linq;
using System.Windows.Media.Imaging;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.Gui.MapView;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.Gui.MapView
{
    [TestFixture]
    public class MapViewModelTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            var map = Substitute.For<IMap>();
            var image = new Bitmap(1, 1);
            map.Render().Returns(image);

            // Call
            using (var mapViewModel = new MapViewModel(map))
            {

                // Assert
                Assert.That(mapViewModel.Map, Is.SameAs(map));

                map.Received(1).Render();
                Assert.That(mapViewModel.MapImage, Is.Not.Null);
            }
        }

        [Test]
        public void Constructor_MapNull_ThrowsArgumentNullException()
        {
            void Call() => new MapViewModel(null);
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("map"));
        }

        [Test]
        public void MapHeight_Set_ExpectedResults()
        {
            // Setup
            var map = Substitute.For<IMap>();
            var image = new Bitmap(1, 1);
            map.Render().Returns(image);

            using (var mapViewModel = new MapViewModel(map))
            {
                BitmapImage initialImg = mapViewModel.MapImage;

                var propertyChangedObserver = new NotifyPropertyChangedTestObserver();
                mapViewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                map.ClearReceivedCalls();

                const int expectedHeight = 10;
                int expectedWidth = mapViewModel.MapWidth;

                // Call
                mapViewModel.MapHeight = expectedHeight;

                // Assert
                Assert.That(mapViewModel.MapHeight, Is.EqualTo(expectedHeight));

                map.Received(1).Size = new Size(expectedWidth, expectedHeight);
                map.Received(1).Render();

                Assert.That(mapViewModel.MapImage, Is.Not.SameAs(initialImg));
                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(2));
                Assert.That(propertyChangedObserver.Senders, Has.All.SameAs(mapViewModel));

                Assert.That(propertyChangedObserver.EventArgses
                                                   .Any(e => e.PropertyName == nameof(MapViewModel.MapImage)));
                Assert.That(propertyChangedObserver.EventArgses
                                                   .Any(e => e.PropertyName == nameof(MapViewModel.MapHeight)));
            }
        }

        [Test]
        public void MapHeight_Set_ValueEqualToOriginal_NoChanges()
        {
            // Setup
            var map = Substitute.For<IMap>();
            var image = new Bitmap(1, 1);
            map.Render().Returns(image);

            using (var mapViewModel = new MapViewModel(map))
            {
                map.ClearReceivedCalls();
                BitmapImage initialImg = mapViewModel.MapImage;

                var propertyChangedObserver = new NotifyPropertyChangedTestObserver();
                mapViewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                mapViewModel.MapHeight = mapViewModel.MapHeight;

                // Assert
                map.DidNotReceiveWithAnyArgs().Size = new Size(0, 0);
                map.DidNotReceiveWithAnyArgs().Render();

                Assert.That(mapViewModel.MapImage, Is.SameAs(initialImg));
                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));
            }
        }

        [Test]
        public void MapHeight_Set_ValueNegative_NoChanges()
        {
            // Setup
            var map = Substitute.For<IMap>();
            var image = new Bitmap(1, 1);
            map.Render().Returns(image);

            using (var mapViewModel = new MapViewModel(map))
            {
                map.ClearReceivedCalls();
                BitmapImage initialImg = mapViewModel.MapImage;

                var propertyChangedObserver = new NotifyPropertyChangedTestObserver();
                mapViewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                mapViewModel.MapHeight = int.MinValue;

                // Assert
                map.DidNotReceiveWithAnyArgs().Size = new Size(0, 0);
                map.DidNotReceiveWithAnyArgs().Render();

                Assert.That(mapViewModel.MapImage, Is.SameAs(initialImg));
                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));
            }
        }

        [Test]
        public void MapWidth_Set_ExpectedResults()
        {
            // Setup
            var map = Substitute.For<IMap>();
            var image = new Bitmap(1, 1);
            map.Render().Returns(image);

            using (var mapViewModel = new MapViewModel(map))
            {
                BitmapImage initialImg = mapViewModel.MapImage;

                var propertyChangedObserver = new NotifyPropertyChangedTestObserver();
                mapViewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                map.ClearReceivedCalls();

                const int expectedWidth = 10;
                int expectedHeight = mapViewModel.MapHeight;

                // Call
                mapViewModel.MapWidth = expectedWidth;

                // Assert
                Assert.That(mapViewModel.MapWidth, Is.EqualTo(expectedWidth));

                map.Received(1).Size = new Size(expectedWidth, expectedHeight);
                map.Received(1).Render();

                Assert.That(mapViewModel.MapImage, Is.Not.SameAs(initialImg));
                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(2));
                Assert.That(propertyChangedObserver.Senders, Has.All.SameAs(mapViewModel));

                Assert.That(propertyChangedObserver.EventArgses
                                                   .Any(e => e.PropertyName == nameof(MapViewModel.MapImage)));
                Assert.That(propertyChangedObserver.EventArgses
                                                   .Any(e => e.PropertyName == nameof(MapViewModel.MapWidth)));
            }
        }

        [Test]
        public void MapWidth_Set_ValueEqualToOriginal_NoChanges()
        {
            // Setup
            var map = Substitute.For<IMap>();
            var image = new Bitmap(1, 1);
            map.Render().Returns(image);

            using (var mapViewModel = new MapViewModel(map))
            {
                map.ClearReceivedCalls();
                BitmapImage initialImg = mapViewModel.MapImage;

                var propertyChangedObserver = new NotifyPropertyChangedTestObserver();
                mapViewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                mapViewModel.MapWidth = mapViewModel.MapWidth;

                // Assert
                map.DidNotReceiveWithAnyArgs().Size = new Size(0, 0);
                map.DidNotReceiveWithAnyArgs().Render();

                Assert.That(mapViewModel.MapImage, Is.SameAs(initialImg));
                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));
            }
        }

        [Test]
        public void MapWidth_Set_ValueNegative_NoChanges()
        {
            // Setup
            var map = Substitute.For<IMap>();
            var image = new Bitmap(1, 1);
            map.Render().Returns(image);

            using (var mapViewModel = new MapViewModel(map))
            {
                map.ClearReceivedCalls();
                BitmapImage initialImg = mapViewModel.MapImage;

                var propertyChangedObserver = new NotifyPropertyChangedTestObserver();
                mapViewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                mapViewModel.MapWidth = int.MinValue;

                // Assert
                map.DidNotReceiveWithAnyArgs().Size = new Size(0, 0);
                map.DidNotReceiveWithAnyArgs().Render();

                Assert.That(mapViewModel.MapImage, Is.SameAs(initialImg));
                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));
            }
        }

        [Test]
        public void Dispose_MapDisposable_MapDisposeCalled()
        {
            // Setup
            IMap map = Substitute.For<IMap, IDisposable>();
            
            var image = new Bitmap(1, 1);
            map.Render().Returns(image);

            var mapViewModel = new MapViewModel(map);

            // Call
            mapViewModel.Dispose();

            // Assert
            ((IDisposable) map).Received(1).Dispose();
        }

        [Test]
        public void DisposeCalledMultipleTimes_MapDisposable_MapDisposeCalledOnlyOnce()
        {
            // Setup
            IMap map = Substitute.For<IMap, IDisposable>();
            
            var image = new Bitmap(1, 1);
            map.Render().Returns(image);

            var mapViewModel = new MapViewModel(map);

            // Call
            mapViewModel.Dispose();
            mapViewModel.Dispose();
            mapViewModel.Dispose();

            // Assert
            ((IDisposable) map).Received(1).Dispose();
        }
    }
}