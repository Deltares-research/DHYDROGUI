using System;
using System.Collections.Generic;
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
            // Setup
            object obj = null;
            void Call() => Ensure.NotNull(obj, nameof(obj));

            // Call
            var exception = Assert.Throws<ArgumentNullException>(Call);

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo(nameof(obj)));
        }

        [Test]
        public void NotNull_NonNullObj_DoesNotThrowAnException()
        {
            // Setup
            var obj = new object();
            void Call() => Ensure.NotNull(obj, nameof(obj));

            // Call | Assert
            Assert.DoesNotThrow(Call);
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
    }
}