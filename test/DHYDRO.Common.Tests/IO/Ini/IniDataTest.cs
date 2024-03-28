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

            Assert.That(iniData.Sections, Is.Empty);
            Assert.That(iniData.SectionCount, Is.EqualTo(0));
        }

        [Test]
        public void Constructor_IniDataIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => _ = new IniData(null), Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_ValidIniData_InitializesProperties()
        {
            var section = new IniSection("Section1");
            section.AddProperty("TestKey", "TestValue");

            var iniData = new IniData();
            iniData.AddSection(section);

            var copiedIniData = new IniData(iniData);

            Assert.That(copiedIniData.SectionCount, Is.EqualTo(1));

            IniSection copiedIniSection = copiedIniData.Sections.FirstOrDefault();
            Assert.That(copiedIniSection, Is.Not.Null);
            Assert.That(copiedIniSection, Is.Not.SameAs(section));
            Assert.That(copiedIniSection, Is.EqualTo(section));
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void AddSection_NameIsNullOrEmpty_ThrowsArgumentException(string name)
        {
            var iniData = new IniData();

            Assert.That(() => iniData.AddSection(name), Throws.ArgumentException);
        }

        [Test]
        public void AddSection_ValidSectionName_AddsSection()
        {
            var iniData = new IniData();

            IniSection section = iniData.AddSection("TestSection");

            Assert.That(section, Is.Not.Null);
            Assert.That(section.Name, Is.EqualTo("TestSection"));
            Assert.That(iniData.Sections.Contains(section), Is.True);
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

            Assert.That(() => iniData.AddSection((IniSection)null), Throws.ArgumentNullException);
        }

        [Test]
        public void AddSection_ValidSection_AddsSection()
        {
            var iniData = new IniData();
            var section = new IniSection("TestSection");

            iniData.AddSection(section);

            IniSection addedSection = iniData.Sections.FirstOrDefault();
            Assert.That(addedSection, Is.Not.Null);
            Assert.That(addedSection.Name, Is.EqualTo("TestSection"));
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

            Assert.That(() => iniData.AddMultipleSections(null), Throws.ArgumentNullException);
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

            Assert.That(() => iniData.ContainsSection(name), Throws.ArgumentException);
        }

        [Test]
        [TestCase("testsection")]
        [TestCase("TestSection")]
        [TestCase("TESTSECTION")]
        public void ContainsSection_ExistingCaseInsensitiveName_ReturnsTrue(string name)
        {
            var iniData = new IniData();
            iniData.AddSection("TestSection");

            bool containsSection = iniData.ContainsSection(name);

            Assert.That(containsSection, Is.True);
        }

        [Test]
        public void ContainsSection_SectionDoesNotExist_ReturnsFalse()
        {
            var iniData = new IniData();
            iniData.AddSection("TestSection");

            bool containsSection = iniData.ContainsSection("OtherSection");

            Assert.That(containsSection, Is.False);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void FindSection_NameIsNullOrEmpty_ThrowsArgumentException(string name)
        {
            var iniData = new IniData();

            Assert.That(() => iniData.FindSection(name), Throws.ArgumentException);
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

            Assert.That(foundSection, Is.Not.Null);
            Assert.That(foundSection.Name, Is.EqualTo("TestSection"));
        }

        [Test]
        public void FindSection_NonExistingName_ReturnsNull()
        {
            var iniData = new IniData();
            iniData.AddSection("TestSection");

            IniSection foundSection = iniData.FindSection("NonExistingName");

            Assert.That(foundSection, Is.Null);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void GetAllSections_NameIsNullOrEmpty_ThrowsArgumentException(string name)
        {
            var iniData = new IniData();

            Assert.That(() => iniData.GetAllSections(name), Throws.ArgumentException);
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

            Assert.That(foundSections, Has.Exactly(2).Matches<IniSection>(x => x.Name == "Section1"));
        }

        [Test]
        public void GetAllSections_NonExistingName_ReturnsEmptyCollection()
        {
            var iniData = new IniData();
            iniData.AddSection("TestSection");

            IEnumerable<IniSection> foundSections = iniData.GetAllSections("NonExistingName");

            Assert.That(foundSections, Is.Empty);
        }

        [Test]
        public void RemoveSection_NullSection_ThrowsArgumentNullException()
        {
            var iniData = new IniData();

            Assert.That(() => iniData.RemoveSection(null), Throws.ArgumentNullException);
        }

        [Test]
        public void RemoveSection_ExistingSection_RemovesSection()
        {
            var iniData = new IniData();
            var section = new IniSection("TestSection");

            iniData.AddSection(section);
            iniData.RemoveSection(section);

            Assert.That(iniData.Sections, Is.Empty);
        }

        [Test]
        public void RemoveSection_SameSectionDifferentInstance_RemovesSection()
        {
            var iniData = new IniData();
            var section1 = new IniSection("TestSection");
            var section2 = new IniSection("TestSection");

            iniData.AddSection(section1);
            iniData.RemoveSection(section2);

            Assert.That(iniData.Sections, Is.Empty);
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

            Assert.That(() => iniData.RemoveAllSections(name), Throws.ArgumentException);
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

            bool containsSection = iniData.ContainsSection("Section1");

            Assert.That(containsSection, Is.False);
            Assert.That(iniData.SectionCount, Is.EqualTo(1));
        }

        [Test]
        public void RemoveAllSections_NonExistingName_DoesNothing()
        {
            var iniData = new IniData();
            iniData.AddSection("Section1");

            iniData.RemoveAllSections("NonExistingName");

            Assert.That(iniData.SectionCount, Is.EqualTo(1));
        }

        [Test]
        public void RemoveAllSections_PredicateIsNullOrEmpty_ThrowsArgumentNullException()
        {
            var iniData = new IniData();

            Assert.That(() => iniData.RemoveAllSections((Predicate<IniSection>)null), Throws.ArgumentNullException);
        }

        [Test]
        public void RemoveAllSections_PredicateMatches_RemovesMatchingSections()
        {
            var iniData = new IniData();
            iniData.AddSection("Section1");
            iniData.AddSection("Section2");
            iniData.AddSection("Section1");

            iniData.RemoveAllSections(section => section.IsNameEqualTo("Section1"));

            bool containsSection = iniData.ContainsSection("Section1");

            Assert.That(iniData.SectionCount, Is.EqualTo(1));
            Assert.That(containsSection, Is.False);
        }

        [Test]
        public void RemoveAllSections_PredicateDoesNotMatch_DoesNothing()
        {
            var iniData = new IniData();
            iniData.AddSection("Section1");

            iniData.RemoveAllSections(property => false);

            Assert.That(iniData.SectionCount, Is.EqualTo(1));
        }

        [Test]
        public void ClearSections_WithSections_RemovesAllSections()
        {
            var iniData = new IniData();
            iniData.AddSection("Section1");
            iniData.AddSection("Section2");

            iniData.ClearSections();

            Assert.That(iniData.Sections, Is.Empty);
        }

        [Test]
        public void ClearSections_WithoutSections_DoesNothing()
        {
            var iniData = new IniData();

            iniData.ClearSections();

            Assert.That(iniData.Sections, Is.Empty);
        }

        [Test]
        [TestCase("", "TestSection")]
        [TestCase(null, "TestSection")]
        [TestCase("TestSection", "")]
        [TestCase("TestSection", null)]
        public void RenameSections_NameIsNullOrEmpty_ThrowsArgumentException(string oldName, string newName)
        {
            var iniData = new IniData();

            Assert.That(() => iniData.RenameSections(oldName, newName), Throws.ArgumentException);
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

            bool equals = iniData.Equals(obj);

            Assert.That(equals, Is.False);
        }

        [Test]
        public void Equals_IniDataAndNull_ReturnsFalse()
        {
            var iniData = new IniData();

            bool equals = iniData.Equals(null);

            Assert.That(equals, Is.False);
        }

        [Test]
        public void Equals_SameIniDataReference_ReturnsTrue()
        {
            var iniData = new IniData();

            bool equals = iniData.Equals(iniData);

            Assert.That(equals, Is.True);
        }

        [Test]
        public void Equals_SameIniDataCaseInsensitive_ReturnsTrue()
        {
            var iniData1 = new IniData();
            var iniData2 = new IniData();

            iniData1.AddSection("TestSection");
            iniData2.AddSection("TESTSECTION");

            bool equals = iniData1.Equals(iniData2);

            Assert.That(equals, Is.True);
        }

        [Test]
        public void Equals_DifferentIniData_ReturnsFalse()
        {
            var iniData1 = new IniData();
            var iniData2 = new IniData();

            iniData1.AddSection("TestSection");
            iniData2.AddSection("OtherSection");

            bool equals = iniData1.Equals(iniData2);

            Assert.That(equals, Is.False);
        }

        [Test]
        public void GetHashCode_SameIniData_SameHashCode()
        {
            var iniData1 = new IniData();
            var iniData2 = new IniData();

            int hashCode1 = iniData1.GetHashCode();
            int hashCode2 = iniData2.GetHashCode();

            Assert.That(hashCode2, Is.EqualTo(hashCode1));
        }
    }
}