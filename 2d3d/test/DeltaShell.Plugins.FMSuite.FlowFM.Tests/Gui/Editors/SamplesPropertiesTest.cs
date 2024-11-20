using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Editors
{
    [TestFixture]
    public class SamplesPropertiesTest
    {
        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void IsReadonly_PropertyNullOrWhitespace_ThrowsException(string propertyName)
        {
            // Setup
            Samples data = GetSamples();
            var samplesProperties = new SamplesProperties() { Data = data };

            // Call
            void Call() => samplesProperties.IsReadonly(propertyName);

            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        [Test]
        public void IsReadonly_NoDataValues_ReturnsTrue()
        {
            // Setup
            var data = new Samples("randomName");
            var samplesProperties = new SamplesProperties() { Data = data };

            // Precondition
            Assert.That(data.HasData, Is.False);

            // Call
            bool isReadOnly = samplesProperties.IsReadonly("randomName");

            // Assert
            Assert.That(isReadOnly, Is.True);
        }

        [Test]
        public void GridCellAveragingMethodIsReadonlyWhenSpatialInterpolationMethodIsNotAveraging(
            [Values] SpatialInterpolationMethod interpolationMethod)
        {
            // Setup
            Samples data = GetSamples();
            data.InterpolationMethod = interpolationMethod;

            var samplesProperties = new SamplesProperties() { Data = data };

            // Call
            bool isReadOnly = samplesProperties.IsReadonly(nameof(samplesProperties.AveragingMethod));

            // Assert
            if (interpolationMethod == SpatialInterpolationMethod.Averaging)
            {
                Assert.That(isReadOnly, Is.False);
            }
            else
            {
                Assert.That(isReadOnly, Is.True);
            }
        }

        [Test]
        public void RelativeSearchCellSizeIsReadonlyWhenSpatialInterpolationMethodIsNotAveraging(
            [Values] SpatialInterpolationMethod interpolationMethod)
        {
            // Setup
            Samples data = GetSamples();
            data.InterpolationMethod = interpolationMethod;

            var samplesProperties = new SamplesProperties() { Data = data };

            // Call
            bool isReadOnly = samplesProperties.IsReadonly(nameof(samplesProperties.RelativeSearchCellSize));

            // Assert
            if (interpolationMethod == SpatialInterpolationMethod.Averaging)
            {
                Assert.That(isReadOnly, Is.False);
            }
            else
            {
                Assert.That(isReadOnly, Is.True);
            }
        }

        [Test]
        public void ExtrapolationToleranceIsReadonlyWhenSpatialInterpolationMethodIsNotTriangulation(
            [Values] SpatialInterpolationMethod interpolationMethod)
        {
            // Setup
            Samples data = GetSamples();
            data.InterpolationMethod = interpolationMethod;

            var samplesProperties = new SamplesProperties() { Data = data };

            // Call
            bool isReadOnly = samplesProperties.IsReadonly(nameof(samplesProperties.ExtrapolationTolerance));

            // Assert
            if (interpolationMethod == SpatialInterpolationMethod.Triangulation)
            {
                Assert.That(isReadOnly, Is.False);
            }
            else
            {
                Assert.That(isReadOnly, Is.True);
            }
        }

        private static Samples GetSamples()
        {
            var samples = new Samples("randomName");
            var pointValues = new[]
            {
                new PointValue
                {
                    X = 1,
                    Y = 2,
                    Value = 3
                },
                new PointValue
                {
                    X = 4,
                    Y = 5,
                    Value = 6
                }
            };

            samples.SetPointValues(pointValues);

            return samples;
        }
    }
}