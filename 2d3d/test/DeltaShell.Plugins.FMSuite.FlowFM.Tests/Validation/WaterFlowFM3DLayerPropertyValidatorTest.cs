using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation
{
    [TestFixture]
    public class WaterFlowFM3DLayerPropertyValidatorTest
    {
        [Test]
        [TestCaseSource(nameof(GetConstructorArgumentNullCases))]
        public void Constructor_ArgumentNull_ThrowsArgumentNullException(WaterFlowFMProperty propertyToValidate,
                                                                         IEnumerable<WaterFlowFMProperty> allProperties)
        {
            // Call
            void Call() => WaterFlowFM3DLayerPropertyValidator.Validate(propertyToValidate, allProperties);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void InvalidPropertyForValidator_ThrowsArgumentException()
        {
            // Setup
            var propertyDefinition = new WaterFlowFMPropertyDefinition()
            {
                DataType = typeof(int),
                IsEnabled = IsTrue,
                MduPropertyName = "ThisNameDoesNotExist"
            };
            var property = new WaterFlowFMProperty(propertyDefinition, "1");

            var allProperties = Substitute.For<IEnumerable<WaterFlowFMProperty>>();

            // Call
            void Call() => WaterFlowFM3DLayerPropertyValidator.Validate(property, allProperties);

            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        [Test]
        public void PropertyToValidateIsNotEnabled_ReturnsEmptyString()
        {
            // Setup
            var propertyDefinition = new WaterFlowFMPropertyDefinition()
            {
                DataType = typeof(int),
                IsEnabled = IsFalse
            };
            var property = new WaterFlowFMProperty(propertyDefinition, "1");

            var allProperties = Substitute.For<IEnumerable<WaterFlowFMProperty>>();

            // Call
            string errorMessage = WaterFlowFM3DLayerPropertyValidator.Validate(property, allProperties);

            // Assert
            Assert.That(errorMessage, Is.Empty);
        }

        [Test]
        public void DzTopNotBiggerThanZero_ReturnsErrorMessage()
        {
            // Setup
            var propertyDefinition = new WaterFlowFMPropertyDefinition()
            {
                DataType = typeof(double),
                IsEnabled = IsTrue,
                MduPropertyName = KnownProperties.DzTop
            };
            var dzTop = new WaterFlowFMProperty(propertyDefinition, "0");

            var allProperties = Substitute.For<IEnumerable<WaterFlowFMProperty>>();

            // Call
            string errorMessage = WaterFlowFM3DLayerPropertyValidator.Validate(dzTop, allProperties);

            // Assert
            var expectedMessage = $"Parameter {KnownProperties.DzTop} should be > 0.00.";
            Assert.That(errorMessage, Is.EqualTo(expectedMessage));
        }

        [Test]
        public void ValidDzTop_ReturnsEmptyString()
        {
            // Setup
            var propertyDefinition = new WaterFlowFMPropertyDefinition()
            {
                DataType = typeof(double),
                IsEnabled = IsTrue,
                MduPropertyName = KnownProperties.DzTop
            };
            var dzTop = new WaterFlowFMProperty(propertyDefinition, "0.00001");

            var allProperties = Substitute.For<IEnumerable<WaterFlowFMProperty>>();

            // Call
            string errorMessage = WaterFlowFM3DLayerPropertyValidator.Validate(dzTop, allProperties);

            // Assert
            Assert.That(errorMessage, Is.Empty);
        }

        [Test]
        [TestCase(KnownProperties.FloorLevTopLay)]
        [TestCase(KnownProperties.DzTopUniAboveZ)]
        public void PropertyNotSmallerThan0_ReturnsErrorMessage(string propertyName)
        {
            // Setup
            var propertyDefinition = new WaterFlowFMPropertyDefinition()
            {
                DataType = typeof(double),
                IsEnabled = IsTrue,
                MduPropertyName = propertyName
            };
            var property = new WaterFlowFMProperty(propertyDefinition, "0");

            var allProperties = Substitute.For<IEnumerable<WaterFlowFMProperty>>();

            // Call
            string errorMessage = WaterFlowFM3DLayerPropertyValidator.Validate(property, allProperties);

            // Assert
            var expectedMessage = $"Parameter {propertyName} should be < 0.00.";
            Assert.That(errorMessage, Is.EqualTo(expectedMessage));
        }

        [Test]
        [TestCase(KnownProperties.FloorLevTopLay)]
        [TestCase(KnownProperties.DzTopUniAboveZ)]
        public void ValidProperty_ReturnsEmptyString(string propertyName)
        {
            // Setup
            var propertyDefinition = new WaterFlowFMPropertyDefinition()
            {
                DataType = typeof(double),
                IsEnabled = IsTrue,
                MduPropertyName = propertyName
            };
            var property = new WaterFlowFMProperty(propertyDefinition, "-0.0001");

            var allProperties = Substitute.For<IEnumerable<WaterFlowFMProperty>>();

            // Call
            string errorMessage = WaterFlowFM3DLayerPropertyValidator.Validate(property, allProperties);

            // Assert
            Assert.That(errorMessage, Is.Empty);
        }

        [Test]
        public void WhenValidatingNumTopSig_KmxPropertyNotPresent_ThrowsArgumentException()
        {
            // Setup
            var propertyDefinition = new WaterFlowFMPropertyDefinition()
            {
                DataType = typeof(int),
                IsEnabled = IsTrue,
                MduPropertyName = KnownProperties.NumTopSig
            };
            var numTopSig = new WaterFlowFMProperty(propertyDefinition, "0");

            IEnumerable<WaterFlowFMProperty> allProperties = Enumerable.Empty<WaterFlowFMProperty>();

            // Call
            void Call() => WaterFlowFM3DLayerPropertyValidator.Validate(numTopSig, allProperties);

            // Assert
            var expectedMessage = $"The kmx property is required to validate {KnownProperties.NumTopSig}.";
            Assert.That(Call, Throws.ArgumentException.With.Message.EqualTo(expectedMessage));
        }

        [Test]
        public void NumTopSigLessThanZero_ReturnsArgumentException()
        {
            // Setup
            var propertyDefinition = new WaterFlowFMPropertyDefinition()
            {
                DataType = typeof(int),
                IsEnabled = IsTrue,
                MduPropertyName = KnownProperties.NumTopSig
            };
            var numTopSig = new WaterFlowFMProperty(propertyDefinition, "-1");

            const string kmxValue = "2";
            WaterFlowFMProperty kmxProperty = CreateKmxProperty(kmxValue);
            IEnumerable<WaterFlowFMProperty> allProperties = new[] { kmxProperty };

            // Call
            string errorMessage = WaterFlowFM3DLayerPropertyValidator.Validate(numTopSig, allProperties);

            // Assert
            var expectedMessage = $"Parameter {KnownProperties.NumTopSig} should be between 0 and {kmxValue} (the current value of {KnownProperties.Kmx}).";
            Assert.That(errorMessage, Is.EqualTo(errorMessage));
        }

        [Test]
        public void ValidNumTopSig_ReturnsEmptyString()
        {
            // Setup
            var propertyDefinition = new WaterFlowFMPropertyDefinition()
            {
                DataType = typeof(int),
                IsEnabled = IsTrue,
                MduPropertyName = KnownProperties.NumTopSig
            };
            var numTopSig = new WaterFlowFMProperty(propertyDefinition, "0");

            const string kmxValue = "2";
            WaterFlowFMProperty kmxProperty = CreateKmxProperty(kmxValue);
            IEnumerable<WaterFlowFMProperty> allProperties = new[] { kmxProperty };

            // Call
            string errorMessage = WaterFlowFM3DLayerPropertyValidator.Validate(numTopSig, allProperties);

            // Assert
            Assert.That(errorMessage, Is.Empty);
        }

        private static WaterFlowFMProperty CreateKmxProperty(string kmxValue)
        {
            var kmxPropertyDefinition = new WaterFlowFMPropertyDefinition()
            {
                DataType = typeof(int),
                MduPropertyName = KnownProperties.Kmx
            };

            return new WaterFlowFMProperty(kmxPropertyDefinition, kmxValue);
        }

        private static IEnumerable<TestCaseData> GetConstructorArgumentNullCases()
        {
            yield return new TestCaseData(null, Enumerable.Empty<WaterFlowFMProperty>());

            var propertyDefinition = new WaterFlowFMPropertyDefinition() { DataType = typeof(int) };
            var property = new WaterFlowFMProperty(propertyDefinition, "1");
            yield return new TestCaseData(property, null);
        }

        private static bool IsFalse(IEnumerable<ModelProperty> modelProperties)
        {
            return false;
        }

        private static bool IsTrue(IEnumerable<ModelProperty> modelProperties)
        {
            return true;
        }
    }
}