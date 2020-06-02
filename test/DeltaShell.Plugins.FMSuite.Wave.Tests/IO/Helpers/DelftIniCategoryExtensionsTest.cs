using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers
{
    [TestFixture]
    public class DelftIniCategoryExtensionsTest
    {
        [TestCaseSource(nameof(GetAddSpatialPropertyArgumentNullCases))]
        public void AddSpatialProperty_ArgumentNull_ThrowsArgumentNullException(DelftIniCategory category, string propertyName,
                                                                                string expectedParamName)
        {
            // Call
            void Call() => category.AddSpatialProperty(propertyName, 0);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParamName));
        }

        private IEnumerable<TestCaseData> GetAddSpatialPropertyArgumentNullCases()
        {
            yield return new TestCaseData(null, "property_name", "category");
            yield return new TestCaseData(new DelftIniCategory(""), null, "propertyName");
        }

        [TestCaseSource(nameof(GetAddSpatialDataCases))]
        public void AddSpatialProperty_AddsCorrectPropertyToCategory(double value, string expectedPropValue)
        {
            // Setup
            var category = new DelftIniCategory("");
            const string propertyName = "property_name";

            // Call
            category.AddSpatialProperty(propertyName, value);

            // Assert
            DelftIniProperty property = category.Properties.Single();
            Assert.That(property.Name, Is.EqualTo(propertyName));
            Assert.That(property.Comment, Is.Empty);
            Assert.That(property.Value, Is.EqualTo(expectedPropValue));
        }

        private IEnumerable<TestCaseData> GetAddSpatialDataCases()
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