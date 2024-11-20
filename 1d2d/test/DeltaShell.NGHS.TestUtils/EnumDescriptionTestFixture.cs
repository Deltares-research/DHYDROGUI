using System;
using System.Collections.Generic;
using DelftTools.Utils.Reflection;
using NUnit.Framework;

namespace DeltaShell.NGHS.TestUtils
{
    [TestFixture]
    public abstract class EnumDescriptionTestFixture<TEnum>
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
                string actual = (value as Enum).GetDescription();

                // Assert
                Assert.That(ExpectedDescriptionForEnumValues.ContainsKey(value),
                            $"Expected {value} to be in the defined enum values.");
                Assert.That(ExpectedDescriptionForEnumValues[value], Is.EqualTo(actual),
                            $"Expected {value} to have a different description:");
            }
        }
    }
}