using System;
using System.Collections.Generic;
using System.ComponentModel;
using DHYDRO.Common.Guards;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.Guards
{
    [TestFixture]
    public class EnsureTest
    {
        private static readonly Random random = new Random();

        [Test]
        public void NotNull_NullObj_ThrowsArgumentNullException()
        {
            // Arrange
            object obj = null;
            void Call() => Ensure.NotNull(obj, nameof(obj));

            // Act
            var exception = Assert.Throws<ArgumentNullException>(Call);

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo(nameof(obj)));
        }

        [Test]
        public void NotNull_NullObjWithMessage_ThrowsArgumentNullException()
        {
            // Arrange
            object obj = null;
            const string message = @"should not be null";
            void Call() => Ensure.NotNull(obj, nameof(obj), message);

            // Act
            var exception = Assert.Throws<ArgumentNullException>(Call);

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo(nameof(obj)));
            StringAssert.StartsWith(message, exception.Message);
        }

        [Test]
        public void NotNull_NonNullObjWithMessage_DoesNotThrowAnException()
        {
            // Arrange
            var obj = new object();
            const string message = @"should not be null";

            // Act
            Ensure.NotNull(obj, nameof(obj), message);

            // Assert
            Assert.IsTrue(true);
        }

        [Test]
        public void NotNull_NonNullObj_DoesNotThrowAnException()
        {
            // Arrange
            var obj = new object();
            void Call() => Ensure.NotNull(obj, nameof(obj));

            // Act Assert
            Assert.DoesNotThrow(Call);
        }

        [Test]
        public void NotNullOrEmpty_NotNullOrEmptyValue_ThrowsNothing()
        {
            // Call
            void Call() => Ensure.NotNullOrEmpty("not empty string", "the param name", "the message");

            // Assert
            Assert.DoesNotThrow(Call);
        }

        [Test]
        [Combinatorial]
        public void EnsureNotInfinity_ReturnsWhenValueNormal(
            [Values(-12314.34, -124, -0.1, 0.1, 0.0, 1.0, 11, 123.0, 12312.0)]
            double value)
        {
            Ensure.NotInfinity(value, nameof(value));
        }

        [Test]
        [Combinatorial]
        public void EnsureNotNaN_ReturnsWhenValueNormal(
            [Values(-123, -1234123.785, -0.1, 0.0, 1.0, 23, 1223.0, 124312.0)]
            double value)
        {
            Ensure.NotNaN(value, nameof(value));
        }

        [Test]
        public void IsDefined_NotDefined_ThrowsInvalidEnumArgumentException()
        {
            // Setup
            const string name = "value";

            // Call
            void Call() => Ensure.IsDefined((TestEnum)0, name);

            // Assert
            var exception = Assert.Throws<InvalidEnumArgumentException>(Call);
            Assert.That(exception.Message, Is.EqualTo(name));
        }

        [TestCase(null)]
        [TestCase("")]
        public void NotNullOrEmpty_NullOrEmptyValue_ThrowsArgumentException(string value)
        {
            // Call
            void Call() => Ensure.NotNullOrEmpty(value, "the param name");

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("the param name"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void NotNullOrEmpty_NullOrEmptyValueWithMessage_ThrowsArgumentException(string value)
        {
            // Call
            void Call() => Ensure.NotNullOrEmpty(value, "the param name", "the message");

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("the param name"));
            Assert.That(e.Message, Is.EqualTo("the message\r\nParameter name: the param name"));
        }

        [Test]
        public void NotNullOrWhiteSpace_NotNullOrWhiteSpaceValue_ThrowsNothing()
        {
            // Call
            void Call() => Ensure.NotNullOrWhiteSpace("not empty string", "the param name", "the message");

            // Assert
            Assert.DoesNotThrow(Call);
        }

        [Test]
        [TestCaseSource(nameof(NullOrWhiteSpaceValues))]
        public void NotNullOrWhiteSpace_NullOrWhiteSpaceValue_ThrowsArgumentException(string value)
        {
            // Call
            void Call() => Ensure.NotNullOrWhiteSpace(value, "the param name");

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("the param name"));
        }

        [Test]
        [TestCaseSource(nameof(NullOrWhiteSpaceValues))]
        public void NotNullOrWhiteSpace_NullOrWhiteSpaceValueWithMessage_ThrowsArgumentException(string value)
        {
            // Call
            void Call() => Ensure.NotNullOrWhiteSpace(value, "the param name", "the message");

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("the param name"));
            Assert.That(e.Message, Is.EqualTo("the message\r\nParameter name: the param name"));
        }

        [TestCase(double.NegativeInfinity)]
        [TestCase(double.PositiveInfinity)]
        public void EnsureNotInfinity_Throws_ArgumentOutOfRangeException_OnPlusMinusInfinity(double value)
        {
            // Arrange
            void Call() => Ensure.NotInfinity(value, nameof(value));

            // Act Assert
            Assert.Throws<ArgumentOutOfRangeException>(Call);
        }

        [TestCase(double.NegativeInfinity, @"Optional message")]
        [TestCase(double.PositiveInfinity, @"Optional message")]
        public void EnsureNotInfinityWithMessage_Throws_ArgumentOutOfRangeException_OnPlusMinusInfinity(double value, string message)
        {
            // Arrange
            void Call() => Ensure.NotInfinity(value, nameof(value), message);

            // Act
            var exception = Assert.Throws<ArgumentOutOfRangeException>(Call);

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo(nameof(value)));
            StringAssert.StartsWith(message, exception.Message);
        }

        [TestCase(double.NaN)]
        public void EnsureNotNaN_Throws_ArgumentOutOfRangeException_OnNaN(double value)
        {
            // Arrange
            void Call() => Ensure.NotNaN(value, nameof(value));

            // Act Assert
            Assert.Throws<ArgumentOutOfRangeException>(Call);
        }

        [TestCase(double.NaN, @"Optional message")]
        public void EnsureNotNaNWithMessage_Throws_ArgumentOutOfRangeException_OnNaN(double value, string message)
        {
            // Arrange
            void Call() => Ensure.NotNaN(value, nameof(value), message);

            // Act
            var exception = Assert.Throws<ArgumentOutOfRangeException>(Call);

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo(nameof(value)));
            StringAssert.StartsWith(message, exception.Message);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(TestEnum.A)]
        [TestCase(TestEnum.B)]
        [TestCase(TestEnum.C)]
        public void IsDefined_Defined_DoesNotThrow(object value)
        {
            // Setup
            const string name = "value";

            // Call
            void Call() => Ensure.IsDefined((TestEnum)value, name);

            // Assert
            Assert.DoesNotThrow(Call);
        }

        private enum TestEnum
        {
            A = 1,
            B = 2,
            C = 3
        }

        [TestCaseSource(nameof(NonNegativeValues))]
        public void NotNegative_NormalValue_Returns(double value)
        {
            // Setup
            const string paramName = "value";

            // Call
            void Call() => Ensure.NotNegative(value, paramName);

            // Assert
            Assert.DoesNotThrow(Call);
        }

        [TestCaseSource(nameof(NegativeValues))]
        public void NotNegative_NegativeValue_ThrowsArgumentOutOfRangeException(double value)
        {
            // Setup
            const string paramName = "value";

            // Call
            void Call() => Ensure.NotNegative(value, paramName);

            // Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(paramName));
        }

        [TestCaseSource(nameof(NegativeValues))]
        public void NotNegative_NegativeValue_WithMessage_ThrowsArgumentOutOfRangeException(double value)
        {
            // Setup
            const string paramName = "value";
            const string message = "optional_message";

            // Call
            void Call() => Ensure.NotNegative(value, paramName, message);

            // Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(paramName));
            StringAssert.StartsWith(message, exception.Message);
        }

        private static IEnumerable<double> NonNegativeValues()
        {
            yield return 0;
            yield return random.NextDouble();
            yield return double.MaxValue;
            yield return double.PositiveInfinity;
            yield return double.NaN;
        }

        private static IEnumerable<double> NegativeValues()
        {
            yield return -1 * random.NextDouble();
            yield return double.MinValue;
            yield return double.NegativeInfinity;
        }

        private static IEnumerable<string> NullOrWhiteSpaceValues()
        {
            yield return null;
            yield return "";
            yield return "   ";
            yield return "\t";
            yield return "\n";
            yield return "\r";
            yield return "\v";
        }
    }
}