using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace DeltaShell.NGHS.TestUtils
{
    /// <summary>
    /// Base test fixture to create enum value tests.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enum.</typeparam>
    [TestFixture]
    public abstract class EnumValuesTestFixture<TEnum>
    {
        /// <summary>
        /// Get the expected value for enum values.
        /// </summary>
        /// <value>
        /// The expected value for enum values.
        /// </value>
        protected abstract IDictionary<TEnum, int> ExpectedValueForEnumValues { get; }

        [Test]
        public void ConvertToInnerValueType_AllEnumValues_ReturnExpectedValues()
        {
            // Setup
            foreach (TEnum value in Enum.GetValues(typeof(TEnum)))
            {
                // Call
                object actual = Convert.ChangeType(value, typeof(int));

                // Assert
                Assert.That(ExpectedValueForEnumValues.ContainsKey(value),
                            $"Expected {value} to be in the defined enum values.");
                Assert.That(ExpectedValueForEnumValues[value], Is.EqualTo(actual),
                            $"Expected {value} to be converted to a different value:");
            }
        }
    }
}