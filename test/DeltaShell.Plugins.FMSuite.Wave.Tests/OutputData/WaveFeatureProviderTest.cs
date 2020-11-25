using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.OutputData
{
    [TestFixture]
    public class WaveFeatureProviderTest
    {
        [Test]
        public void Constructor_ModelNull_ThrowsArgumentNullException()
        {
            //Call
            void Call() => new WaveFeatureProvider(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("model"));
        }

        [Test]
        public void GetObservationPoints_ReturnsCorrectCollection()
        {
            // Setup
            var model = Substitute.For<IWaveModel>();
            var observationPoints = new EventedList<Feature2DPoint>
            {
                new Feature2DPoint(),
                new Feature2DPoint(),
                new Feature2DPoint()
            };
            model.ObservationPoints.Returns(observationPoints);
            var featureProvider = new WaveFeatureProvider(model);

            // Call
            List<Feature2D> result = featureProvider.ObservationPoints.ToList();

            // Assert
            CollectionAssert.AreEqual(observationPoints, result);
        }

        [TestCaseSource(nameof(ModelObservationPointsNullOrEmptyCases))]
        public void GetObservationPoints_ModelObservationPointsNullOrEmpty_ReturnsEmptyCollection(IEventedList<Feature2DPoint> observationPoints)
        {
            // Setup
            var model = Substitute.For<IWaveModel>();
            model.ObservationPoints.Returns(observationPoints);
            var featureProvider = new WaveFeatureProvider(model);

            // Call
            List<Feature2D> result = featureProvider.ObservationPoints.ToList();

            // Assert
            Assert.That(result, Is.Empty);
        }

        private static IEnumerable<IEventedList<Feature2DPoint>> ModelObservationPointsNullOrEmptyCases()
        {
            yield return null;
            yield return new EventedList<Feature2DPoint>();
        }
    }
}