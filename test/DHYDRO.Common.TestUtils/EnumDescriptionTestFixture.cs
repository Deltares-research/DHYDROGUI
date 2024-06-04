using System;
using System.Collections.Generic;
using DHYDRO.Common.Extensions;
using NUnit.Framework;

namespace DHYDRO.Common.TestUtils
{
    [TestFixture]
    public abstract class EnumDescriptionTestFixture<TEnum> where TEnum : Enum
    {
        /// <summary>
        /// Get the expected value for enum values.
        /// </summary>
        /// <value>
        /// The expected value for enum values.
        /// </value>
        protected abstract IDictionary<TEnum, string> ExpectedDescriptionForEnumValues { get; }

        [Test]
        public void ConvertToInnerValueType_AllEnumValues_ReturnExpectedValues()
        {
            // Setup
            foreach (TEnum value in Enum.GetValues(typeof(TEnum)))
            {
                // Call
                string actual = value.GetDescription();

                // Assert
                Assert.That(ExpectedDescriptionForEnumValues.ContainsKey(value),
                            $"Expected {value} to be in the defined enum values.");
                Assert.That(ExpectedDescriptionForEnumValues[value], Is.EqualTo(actual),
                            $"Expected {value} to have a different description:");
            }
        }
    }
}