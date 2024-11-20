using System;
using System.Collections.Generic;
using DelftTools.Hydro.Validators;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Validators
{
    [TestFixture]
    public class ValidatedFeaturesTest
    {
        [Test]
        [TestCaseSource(nameof(ConstructorArgNullCases))]
        public void Constructor_ArgNull_ThrowsArgumentNullException(IComplexFeature region, IFeature[] feature, string expParamName)
        {
            // Call
            void Call() => new ValidatedFeatures(region, feature);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo(expParamName));
        }

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Setup
            var region = Substitute.For<IComplexFeature>();
            var features = new[]
            {
                Substitute.For<IFeature>(),
                Substitute.For<IFeature>()
            };

            // Call
            var validatedFeatures = new ValidatedFeatures(region, features);

            // Assert
            Assert.That(validatedFeatures.FeatureRegion, Is.SameAs(region));
            Assert.That(validatedFeatures.Features, Is.EqualTo(features));
        }

        [Test]
        public void GetEnvelope_ForFeatures_GetsCorrectEnvelope()
        {
            // Setup
            var feature1 = Substitute.For<IFeature>();
            var feature2 = Substitute.For<IFeature>();
            var feature3 = Substitute.For<IFeature>();

            feature1.Geometry.EnvelopeInternal.Returns(new Envelope(2, 4, 2, 4));
            feature2.Geometry.EnvelopeInternal.Returns(new Envelope(3, 5, 1, 3));
            feature3.Geometry.EnvelopeInternal.Returns(new Envelope(0, 2, 0, 2));

            var region = Substitute.For<IComplexFeature>();
            var validatedFeatures = new ValidatedFeatures(region, feature1, feature2, feature3);

            // Call
            Envelope envelope = validatedFeatures.GetEnvelope();

            // Assert
            Assert.That(envelope.MinX, Is.EqualTo(0));
            Assert.That(envelope.MaxX, Is.EqualTo(5));
            Assert.That(envelope.MinY, Is.EqualTo(0));
            Assert.That(envelope.MaxY, Is.EqualTo(4));
        }

        [Test]
        public void GetEnvelope_ForPointFeature_GetsCorrectEnvelope()
        {
            // Setup
            var feature = Substitute.For<IFeature>();
            feature.Geometry.EnvelopeInternal.Returns(new Envelope(0, 0, 0, 0));
            var region = Substitute.For<IComplexFeature>();
            var validatedFeatures = new ValidatedFeatures(region, feature);

            // Call
            Envelope envelope = validatedFeatures.GetEnvelope();

            // Assert
            Assert.That(envelope.MinX, Is.EqualTo(-10));
            Assert.That(envelope.MaxX, Is.EqualTo(10));
            Assert.That(envelope.MinY, Is.EqualTo(-10));
            Assert.That(envelope.MaxY, Is.EqualTo(10));
        }

        private static IEnumerable<TestCaseData> ConstructorArgNullCases()
        {
            yield return new TestCaseData(null, Array.Empty<IFeature>(), "region");
            yield return new TestCaseData(Substitute.For<IComplexFeature>(), null, "features");
        }
    }
}