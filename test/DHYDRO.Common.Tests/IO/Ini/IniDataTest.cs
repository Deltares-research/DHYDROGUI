using System;
using System.Collections.Generic;
using System.Linq;
using DHYDRO.Common.IO.Ini;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.Ini
{
    [TestFixture]
    public class IniDataTest
    {
        [Test]
        public void Constructor_InitializesProperties()
        {
            var iniData = new IniData();

            Assert.IsEmpty(iniData.Sections);
            Assert.AreEqual(0, iniData.SectionCount);
        }

        [Test]
        public void Constructor_IniDataIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new IniData(null));
        }

        [Test]
        public void Constructor_ValidIniData_InitializesProperties()
        {
            var section = new IniSection("Section1");
            section.AddProperty("TestKey", "TestValue");

            var iniData = new IniData();
            iniData.AddSection(section);

            var copiedIniData = new IniData(iniData);

            Assert.AreEqual(1, copiedIniData.SectionCount);

            IniSection copiedIniSection = copiedIniData.Sections.FirstOrDefault();
            Assert.NotNull(copiedIniSection);
            Assert.AreNotSame(section, copiedIniSection);
            Assert.AreEqual(section, copiedIniSection);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void AddSection_NameIsNullOrEmpty_ThrowsArgumentException(string name)
        {
            var iniData = new IniData();

            Assert.Throws<ArgumentException>(() => iniData.AddSection(name));
        }

        [Test]
        public void AddSection_ValidSectionName_AddsSection()
        {
            var iniData = new IniData();

            IniSection section = iniData.AddSection("TestSection");

            Assert.IsNotNull(section);
            Assert.AreEqual("TestSection", section.Name);
            Assert.IsTrue(iniData.Sections.Contains(section));
        }

        [Test]
        public void AddSection_SameSectionName_AddsSection()
        {
            var iniData = new IniData();

            IniSection section1 = iniData.AddSection("TestSection");
            IniSection section2 = iniData.AddSection("TestSection");

            IniSection[] expected = { section1, section2 };

            Assert.That(iniData.Sections, Is.EqualTo(expected));
        }

        [Test]
        public void AddSection_SectionIsNull_ThrowsArgumentNullException()
        {
            var iniData = new IniData();

            Assert.Throws<ArgumentNullException>(() => iniData.AddSection((IniSection)null));
        }

        [Test]
        public void AddSection_ValidSection_AddsSection()
        {
            var iniData = new IniData();
            var section = new IniSection("TestSection");

            iniData.AddSection(section);

            IniSection addedSection = iniData.Sections.FirstOrDefault();
            Assert.IsNotNull(addedSection);
            Assert.AreEqual("TestSection", addedSection.Name);
        }

        [Test]
        public void AddSection_SameSection_AddsSection()
        {
            var iniData = new IniData();
            var section1 = new IniSection("TestSection");
            var section2 = new IniSection("TestSection");

            iniData.AddSection(section1);
            iniData.AddSection(section2);

            IniSection[] expected = { section1, section2 };

            Assert.That(iniData.Sections, Is.EqualTo(expected));
        }

        [Test]
        public void AddSection_ValidSections_PreservesOrder()
        {
            var iniData = new IniData();
            var section1 = new IniSection("TestSection1");
            var section2 = new IniSection("TestSection2");
            var section3 = new IniSection("TestSection3");

            iniData.AddSection(section3);
            iniData.AddSection(section2);
            iniData.AddSection(section1);

            IniSection[] expected = { section3, section2, section1 };

            Assert.That(iniData.Sections, Is.EqualTo(expected));
        }

        [Test]
        public void AddMultipleSections_SectionsIsNull_ThrowsArgumentNullException()
        {
            var iniData = new IniData();

            Assert.Throws<ArgumentNullException>(() => iniData.AddMultipleSections(null));
        }

        [Test]
        public void AddMultipleSections_ValidSections_AddsSections()
        {
            var iniData = new IniData();
            var section1 = new IniSection("Section1");
            var section2 = new IniSection("Section2");

            IniSection[] sections = { section1, section2 };

            iniData.AddMultipleSections(sections);

            Assert.That(iniData.Sections, Is.EqualTo(sections));
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void ContainsSection_NameIsNullOrEmpty_ThrowsArgumentException(string name)
        {
            var iniData = new IniData();

            Assert.Throws<ArgumentException>(() => iniData.ContainsSection(name));
        }

        [Test]
        [TestCase("testsection")]
        [TestCase("TestSection")]
        [TestCase("TESTSECTION")]
        public void ContainsSection_ExistingCaseInsensitiveName_ReturnsTrue(string name)
        {
            var iniData = new IniData();
            iniData.AddSection("TestSection");

            bool result = iniData.ContainsSection(name);

            Assert.IsTrue(result);
        }

        [Test]
        public void ContainsSection_SectionDoesNotExist_ReturnsFalse()
        {
            var iniData = new IniData();
            iniData.AddSection("TestSection");

            bool result = iniData.ContainsSection("OtherSection");

            Assert.IsFalse(result);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void FindSection_NameIsNullOrEmpty_ThrowsArgumentException(string name)
        {
            var iniData = new IniData();

            Assert.Throws<ArgumentException>(() => iniData.FindSection(name));
        }

        [Test]
        [TestCase("testsection")]
        [TestCase("TestSection")]
        [TestCase("TESTSECTION")]
        public void FindSection_ExistingCaseInsensitiveName_ReturnsSection(string name)
        {
            var iniData = new IniData();
            iniData.AddSection("TestSection");

            IniSection foundSection = iniData.FindSection(name);

            Assert.NotNull(foundSection);
            Assert.AreEqual("TestSection", foundSection.Name);
        }

        [Test]
        public void FindSection_NonExistingName_ReturnsNull()
        {
            var iniData = new IniData();
            iniData.AddSection("TestSection");

            IniSection foundSection = iniData.FindSection("NonExistingName");

            Assert.Null(foundSection);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void GetAllSections_NameIsNullOrEmpty_ThrowsArgumentException(string name)
        {
            var iniData = new IniData();

            Assert.Throws<ArgumentException>(() => iniData.GetAllSections(name));
        }

        [Test]
        [TestCase("section1")]
        [TestCase("Section1")]
        [TestCase("SECTION1")]
        public void GetAllSections_ExistingCaseInsensitiveName_ReturnsMatchingSections(string name)
        {
            var iniData = new IniData();
            iniData.AddSection("Section1");
            iniData.AddSection("Section2");
            iniData.AddSection("Section1");

            IEnumerable<IniSection> foundSections = iniData.GetAllSections(name);

            Assert.NotNull(foundSections);
            Assert.AreEqual(2, foundSections.Count(s => s.Name == "Section1"));
        }

        [Test]
        public void GetAllSections_NonExistingName_ReturnsEmptyCollection()
        {
            var iniData = new IniData();
            iniData.AddSection("TestSection");

            IEnumerable<IniSection> foundSections = iniData.GetAllSections("NonExistingName");

            Assert.NotNull(foundSections);
            Assert.IsEmpty(foundSections);
        }

        [Test]
        public void RemoveSection_NullSection_ThrowsArgumentNullException()
        {
            var iniData = new IniData();

            Assert.Throws<ArgumentNullException>(() => iniData.RemoveSection(null));
        }

        [Test]
        public void RemoveSection_ExistingSection_RemovesSection()
        {
            var iniData = new IniData();
            var section = new IniSection("TestSection");

            iniData.AddSection(section);
            iniData.RemoveSection(section);

            Assert.IsEmpty(iniData.Sections);
        }

        [Test]
        public void RemoveSection_SameSectionDifferentInstance_RemovesSection()
        {
            var iniData = new IniData();
            var section1 = new IniSection("TestSection");
            var section2 = new IniSection("TestSection");

            iniData.AddSection(section1);
            iniData.RemoveSection(section2);

            Assert.IsEmpty(iniData.Sections);
        }

        [Test]
        public void RemoveSection_DifferentSection_DoesNotRemoveSection()
        {
            var iniData = new IniData();
            var section1 = new IniSection("TestSection");
            var section2 = new IniSection("OtherSection");

            iniData.AddSection(section1);
            iniData.RemoveSection(section2);

            IniSection[] expected = { section1 };

            Assert.That(iniData.Sections, Is.EqualTo(expected));
        }

        [Test]
        public void RemoveSection_ExistingSection_PreservesOrder()
        {
            var iniData = new IniData();
            IniSection section1 = iniData.AddSection("Section1");
            IniSection section2 = iniData.AddSection("Section2");
            IniSection section3 = iniData.AddSection("Section3");

            iniData.RemoveSection(section1);

            IniSection section4 = iniData.AddSection("Section4");
            IniSection[] expected = { section2, section3, section4 };

            Assert.That(iniData.Sections, Is.EqualTo(expected));
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void RemoveAllSections_NameIsNullOrEmpty_ThrowsArgumentException(string name)
        {
            var iniData = new IniData();

            Assert.Throws<ArgumentException>(() => iniData.RemoveAllSections(name));
        }

        [Test]
        [TestCase("section1")]
        [TestCase("Section1")]
        [TestCase("SECTION1")]
        public void RemoveAllSections_ExistingCaseInsensitiveName_RemovesMatchingSections(string name)
        {
            var iniData = new IniData();
            iniData.AddSection("Section1");
            iniData.AddSection("Section2");
            iniData.AddSection("Section1");

            iniData.RemoveAllSections(name);

            Assert.AreEqual(1, iniData.SectionCount);
            Assert.IsFalse(iniData.ContainsSection("Section1"));
        }

        [Test]
        public void RemoveAllSections_NonExistingName_DoesNothing()
        {
            var iniData = new IniData();
            iniData.AddSection("Section1");

            iniData.RemoveAllSections("NonExistingName");

            Assert.AreEqual(1, iniData.SectionCount);
        }

        [Test]
        public void RemoveAllSections_PredicateIsNullOrEmpty_ThrowsArgumentNullException()
        {
            var iniData = new IniData();

            Assert.Throws<ArgumentNullException>(() => iniData.RemoveAllSections((Predicate<IniSection>)null));
        }

        [Test]
        public void RemoveAllSections_PredicateMatches_RemovesMatchingSections()
        {
            var iniData = new IniData();
            iniData.AddSection("Section1");
            iniData.AddSection("Section2");
            iniData.AddSection("Section1");

            iniData.RemoveAllSections(section => section.IsNameEqualTo("Section1"));

            Assert.AreEqual(1, iniData.SectionCount);
            Assert.IsFalse(iniData.ContainsSection("Section1"));
        }

        [Test]
        public void RemoveAllSections_PredicateDoesNotMatch_DoesNothing()
        {
            var iniData = new IniData();
            iniData.AddSection("Section1");

            iniData.RemoveAllSections(property => false);

            Assert.AreEqual(1, iniData.SectionCount);
        }

        [Test]
        public void ClearSections_WithSections_RemovesAllSections()
        {
            var iniData = new IniData();
            iniData.AddSection("Section1");
            iniData.AddSection("Section2");

            iniData.ClearSections();

            Assert.IsEmpty(iniData.Sections);
        }

        [Test]
        public void ClearSections_WithoutSections_DoesNothing()
        {
            var iniData = new IniData();

            iniData.ClearSections();

            Assert.IsEmpty(iniData.Sections);
        }

        [Test]
        [TestCase("", "TestSection")]
        [TestCase(null, "TestSection")]
        [TestCase("TestSection", "")]
        [TestCase("TestSection", null)]
        public void RenameSections_NameIsNullOrEmpty_ThrowsArgumentException(string oldName, string newName)
        {
            var iniData = new IniData();

            Assert.Throws<ArgumentException>(() => iniData.RenameSections(oldName, newName));
        }

        [Test]
        [TestCase("name1")]
        [TestCase("Name1")]
        [TestCase("NAME1")]
        public void RenameSections_ExistingCaseInsensitiveName_NameRenamed(string oldName)
        {
            var iniData = new IniData();
            iniData.AddSection("Name1");
            iniData.AddSection("Name2");
            iniData.AddSection("Name1");

            iniData.RenameSections(oldName, "NewName");

            var expected = new[]
            {
                new IniSection("NewName"), 
                new IniSection("Name2"), 
                new IniSection("NewName")
            };

            Assert.That(iniData.Sections, Is.EqualTo(expected));
        }

        [Test]
        public void RenameSections_SectionDoesNotExist_NoChanges()
        {
            var iniData = new IniData();
            iniData.AddSection("Name1");
            iniData.AddSection("Name2");

            iniData.RenameSections("NonExistentName", "NewName");

            var expected = new[] { new IniSection("Name1"), new IniSection("Name2") };

            Assert.That(iniData.Sections, Is.EqualTo(expected));
        }

        [Test]
        public void Equals_ObjectAndIniData_ReturnsFalse()
        {
            var obj = new object();
            var iniData = new IniData();

            bool result = iniData.Equals(obj);

            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_IniDataAndNull_ReturnsFalse()
        {
            var iniData = new IniData();

            bool result = iniData.Equals(null);

            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_SameIniDataReference_ReturnsTrue()
        {
            var iniData = new IniData();

            bool result = iniData.Equals(iniData);

            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_SameIniDataCaseInsensitive_ReturnsTrue()
        {
            var iniData1 = new IniData();
            var iniData2 = new IniData();

            iniData1.AddSection("TestSection");
            iniData2.AddSection("TESTSECTION");

            bool result = iniData1.Equals(iniData2);

            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_DifferentIniData_ReturnsFalse()
        {
            var iniData1 = new IniData();
            var iniData2 = new IniData();

            iniData1.AddSection("TestSection");
            iniData2.AddSection("OtherSection");

            bool result = iniData1.Equals(iniData2);

            Assert.IsFalse(result);
        }

        [Test]
        public void GetHashCode_SameIniData_SameHashCode()
        {
            var iniData1 = new IniData();
            var iniData2 = new IniData();

            int hashCode1 = iniData1.GetHashCode();
            int hashCode2 = iniData2.GetHashCode();

            Assert.AreEqual(hashCode1, hashCode2);
        }
    }
}