using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Helpers
{
    [TestFixture]
    public class DelftIniCategoryExtensionsTest
    {
        [TestCaseSource(nameof(GetAddSpatialPropertyArgumentNullCases))]
        public void AddSpatialProperty_ArgumentNull_ThrowsArgumentNullException(IniSection section, string propertyKey,
                                                                                string expectedParamName)
        {
            // Call
            void Call() => section.AddSpatialProperty(propertyKey, 0);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParamName));
        }

        private static IEnumerable<TestCaseData> GetAddSpatialPropertyArgumentNullCases()
        {
            yield return new TestCaseData(null, "property", "section");
            yield return new TestCaseData(new IniSection("section"), null, "propertyKey");
        }

        [TestCaseSource(nameof(GetAddSpatialDataCases))]
        public void AddSpatialProperty_AddsCorrectPropertyToCategory(double value, string expectedPropValue)
        {
            // Setup
            var section = new IniSection("TestSection");
            const string propertyKey = "property_key";

            // Call
            section.AddSpatialProperty(propertyKey, value);

            // Assert
            IniProperty property = section.Properties.Single();
            Assert.That(property.Key, Is.EqualTo(propertyKey));
            Assert.That(property.Comment, Is.Empty);
            Assert.That(property.Value, Is.EqualTo(expectedPropValue));
        }

        private static IEnumerable<TestCaseData> GetAddSpatialDataCases()
        {
            var value = 1.1;
            var expectedStr = "1.1000000";
            yield return new TestCaseData(value, expectedStr);

            value = 12.123;
            expectedStr = "12.1230000";
            yield return new TestCaseData(value, expectedStr);

            value = 123.12345;
            expectedStr = "123.1234500";
            yield return new TestCaseData(value, expectedStr);

            value = 1234.1234567;
            expectedStr = "1234.1234567";
            yield return new TestCaseData(value, expectedStr);

            value = 12345.123456789;
            expectedStr = "12345.1234568";
            yield return new TestCaseData(value, expectedStr);

            value = 123456.123456750;
            expectedStr = "123456.1234568";
            yield return new TestCaseData(value, expectedStr);

            value = 1234567.123456749;
            expectedStr = "1234567.1234567";
            yield return new TestCaseData(value, expectedStr);
        }
    }
}