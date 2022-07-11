using System;
using System.Collections.Generic;
using DHYDRO.Common.IO.BackwardCompatibility;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.BackwardCompatibility
{
    [TestFixture]
    public class DelftIniPropertyInfoTest
    {
        [Test]
        [TestCaseSource(nameof(Constructor_ArgNullCases))]
        public void Constructor_ArgNull_ThrowsArgumentNullException(string category, string property, string value, string expParamName)
        {
            // Call
            void Call() => new DelftIniPropertyInfo(category, property, value);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo(expParamName));
        }

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var info = new DelftIniPropertyInfo("some_category", "some_property", "some_value");

            // Assert
            Assert.That(info.Category, Is.EqualTo("some_category"));
            Assert.That(info.Property, Is.EqualTo("some_property"));
            Assert.That(info.Value, Is.EqualTo("some_value"));
        }

        private static IEnumerable<TestCaseData> Constructor_ArgNullCases()
        {
            yield return ToData(null, "some_property", "some_value", "category");
            yield return ToData("some_category", null, "some_value", "property");
            yield return ToData("some_category", "some_property", null, "value");

            TestCaseData ToData(string category, string property, string value, string expParamName)
                => new TestCaseData(category, property, value, expParamName).SetName(expParamName);
        }
    }
}