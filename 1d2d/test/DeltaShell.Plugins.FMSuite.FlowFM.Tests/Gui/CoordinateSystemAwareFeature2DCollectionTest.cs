using System.ComponentModel;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using GeoAPI.Extensions.CoordinateSystems;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    public class CoordinateSystemAwareFeature2DCollectionTest
    {
        [Test]
        public void Init_ExpectedResults()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            var observedFeature = new EventedList<Link1D2D>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            model.CoordinateSystem.Returns(coordinateSystem);

            var collection = new CoordinateSystemAwareFeature2DCollection<IWaterFlowFMModel>();

            // Call
            collection.Init(observedFeature, "1d2dLink", model, nameof(WaterFlowFMModel));

            // Assert
            Assert.That(collection.CoordinateSystemSource, Is.SameAs(model));
            Assert.That(collection.Features, Is.SameAs(observedFeature));
            Assert.That(collection.ModelName, Is.SameAs(nameof(WaterFlowFMModel)));
            Assert.That(collection.CoordinateSystem, Is.SameAs(coordinateSystem));
        }

        [Test]
        public void SourceCoordinateSystemChanged_UpdatesTheContainingCollection()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            var observedFeature = new EventedList<Link1D2D>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            model.CoordinateSystem.Returns(coordinateSystem);

            var collection = new CoordinateSystemAwareFeature2DCollection<IWaterFlowFMModel>();
            collection.Init(observedFeature, "1d2dLink", model, nameof(WaterFlowFMModel));

            var newCoordinateSystem = Substitute.For<ICoordinateSystem>();
            
            // Call
            model.CoordinateSystem.Returns(newCoordinateSystem);
            model.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(
                model, new PropertyChangedEventArgs(nameof(model.CoordinateSystem)));

            // Assert
            Assert.That(collection.CoordinateSystem, Is.SameAs(newCoordinateSystem));
        }

        [Test]
        public void DisconnectedSource_DoesNotUpdateTheCollection()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            var observedFeature = new EventedList<Link1D2D>();
            var initialCoordinateSystem = Substitute.For<ICoordinateSystem>();
            model.CoordinateSystem.Returns(initialCoordinateSystem);

            var collection = new CoordinateSystemAwareFeature2DCollection<IWaterFlowFMModel>();
            collection.Init(observedFeature, "1d2dLink", model, nameof(WaterFlowFMModel));

            // disconnection coordinate system source.
            collection.CoordinateSystemSource = null;

            var newCoordinateSystem = Substitute.For<ICoordinateSystem>();
            // Call
            model.CoordinateSystem.Returns(newCoordinateSystem);
            model.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(
                model, new PropertyChangedEventArgs(nameof(model.CoordinateSystem)));

            // Assert
            Assert.That(collection.CoordinateSystem, Is.SameAs(initialCoordinateSystem));
        }
    }
}