using System;
using System.Collections.Generic;
using DHYDRO.Common.IO.Ini.BackwardCompatibility;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.Ini.BackwardCompatibility
{
    [TestFixture]
    public class IniPropertyInfoTest
    {
        [Test]
        [TestCaseSource(nameof(Constructor_ArgNullCases))]
        public void Constructor_ArgNull_ThrowsArgumentNullException(string section, string property, string value, string expParamName)
        {
            // Call
            void Call() => new IniPropertyInfo(section, property, value);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo(expParamName));
        }

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var info = new IniPropertyInfo("some_section", "some_property", "some_value");

            // Assert
            Assert.That(info.Section, Is.EqualTo("some_section"));
            Assert.That(info.Property, Is.EqualTo("some_property"));
            Assert.That(info.Value, Is.EqualTo("some_value"));
        }

        private static IEnumerable<TestCaseData> Constructor_ArgNullCases()
        {
            yield return ToData(null, "some_property", "some_value", "section");
            yield return ToData("some_section", null, "some_value", "property");
            yield return ToData("some_section", "some_property", null, "value");

            TestCaseData ToData(string section, string property, string value, string expParamName)
                => new TestCaseData(section, property, value, expParamName).SetName(expParamName);
        }
    }
}