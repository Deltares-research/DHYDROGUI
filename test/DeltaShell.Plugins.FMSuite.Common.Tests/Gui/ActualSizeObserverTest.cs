using System;
using DeltaShell.Plugins.FMSuite.Common.Gui.MapView;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.Gui
{
    [TestFixture]
    public class ActualSizeObserverTest
    {
        [Test]
        public void ObserveProperty_ExpectedResults()
        {
            Assert.That(ActualSizeObserver.ObserveProperty.Name, Is.EqualTo("Observe"));
            Assert.That(ActualSizeObserver.ObserveProperty.PropertyType, Is.EqualTo(typeof(bool)));
            Assert.That(ActualSizeObserver.ObserveProperty.OwnerType, Is.EqualTo(typeof(ActualSizeObserver)));
        }

        [Test]
        public void ObservedWidthProperty_ExpectedResults()
        {
            Assert.That(ActualSizeObserver.ObservedWidthProperty.Name, Is.EqualTo("ObservedWidth"));
            Assert.That(ActualSizeObserver.ObservedWidthProperty.PropertyType, Is.EqualTo(typeof(double)));
            Assert.That(ActualSizeObserver.ObservedWidthProperty.OwnerType, Is.EqualTo(typeof(ActualSizeObserver)));
        }

        [Test]
        public void ObservedHeightProperty_ExpectedResults()
        {
            Assert.That(ActualSizeObserver.ObservedHeightProperty.Name, Is.EqualTo("ObservedHeight"));
            Assert.That(ActualSizeObserver.ObservedHeightProperty.PropertyType, Is.EqualTo(typeof(double)));
            Assert.That(ActualSizeObserver.ObservedHeightProperty.OwnerType, Is.EqualTo(typeof(ActualSizeObserver)));
        }

        [Test]
        public void GetObserve_FrameWorkElementNull_ThrowsArgumentNullException()
        {
            void Call() => ActualSizeObserver.GetObserve(null);
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("frameworkElement"));
        }

        [Test]
        public void SetObserve_FrameWorkElementNull_ThrowsArgumentNullException()
        {
            void Call() => ActualSizeObserver.SetObserve(null, true);
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("frameworkElement"));
        }

        [Test]
        public void GetObservedWidth_FrameWorkElementNull_ThrowsArgumentNullException()
        {
            void Call() => ActualSizeObserver.GetObservedWidth(null);
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("frameworkElement"));
        }

        [Test]
        public void SetObservedWidth_FrameWorkElementNull_ThrowsArgumentNullException()
        {
            void Call() => ActualSizeObserver.SetObservedWidth(null, 1.0);
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("frameworkElement"));
        }

        [Test]
        public void GetObservedHeight_FrameWorkElementNull_ThrowsArgumentNullException()
        {
            void Call() => ActualSizeObserver.GetObservedHeight(null);
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("frameworkElement"));
        }

        [Test]
        public void SetObservedHeight_FrameWorkElementNull_ThrowsArgumentNullException()
        {
            void Call() => ActualSizeObserver.SetObservedHeight(null, 1.0);
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("frameworkElement"));
        }
    }
}