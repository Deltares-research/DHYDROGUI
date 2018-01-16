using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.TestUtils;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class CrossSectionDefinitionExtensionsTest
    {
        private const string Section1Name = "section1";
        private const string Section2Name = "section2";
        private const string Section3Name = "section3";

        [Test]
        public void GivenCrossSectionDefinitionZwWithOneSection_WhenGettingTotalSectionsWidth_ThenResultIsCorrect()
        {
            var length = 20.0;
            var csDef = new CrossSectionDefinitionZW();
            csDef.AddSection(new CrossSectionSectionType { Name = Section1Name }, length);
            Assert.That(csDef.SectionsTotalWidth(), Is.EqualTo(length));
        }

        [Test]
        public void GivenCrossSectionDefinitionZwWithMultipleSections_WhenGettingTotalSectionsWidth_ThenResultIsCorrect()
        {
            var length1 = 20.0;
            var length2 = 55.0;
            var length3 = 89.0;
            var csDef = new CrossSectionDefinitionZW();
            csDef.AddSection(new CrossSectionSectionType { Name = Section1Name }, length1);
            csDef.AddSection(new CrossSectionSectionType { Name = Section2Name }, length2);
            csDef.AddSection(new CrossSectionSectionType { Name = Section3Name }, length3);
            Assert.That(csDef.SectionsTotalWidth(), Is.EqualTo(length1 + length2 + length3));
        }

        [Test]
        public void GivenCrossSectionDefinitionZw_WhenAddingSection_ThenSectionIsAddedCorrectly()
        {
            var csDef = new CrossSectionDefinitionZW();
            csDef.AddSection(new CrossSectionSectionType { Name = Section1Name }, 20.0);
            Assert.That(csDef.Sections.Count, Is.EqualTo(1));

            var section1 = csDef.Sections.FirstOrDefault();
            Assert.IsNotNull(section1);
            Assert.That(section1.MinY, Is.EqualTo(0.0));
            Assert.That(section1.MaxY, Is.EqualTo(10.0));
        }

        [Test]
        public void GivenCrossSectionDefinitionZw_WhenAddingMultipleSections_ThenSectionsAreAddedCorrectly()
        {
            var csDef = new CrossSectionDefinitionZW();
            csDef.AddSection(new CrossSectionSectionType { Name = Section1Name }, 20.0);
            csDef.AddSection(new CrossSectionSectionType { Name = Section2Name }, 55.0);
            csDef.AddSection(new CrossSectionSectionType { Name = Section3Name }, 5.0);
            Assert.That(csDef.Sections.Count, Is.EqualTo(3));

            var section1 = csDef.Sections.FirstOrDefault(s => s.SectionType.Name == Section1Name);
            Assert.IsNotNull(section1);
            Assert.That(section1.MinY, Is.EqualTo(0.0));
            Assert.That(section1.MaxY, Is.EqualTo(10.0));

            var section2 = csDef.Sections.FirstOrDefault(s => s.SectionType.Name == Section2Name);
            Assert.IsNotNull(section2);
            Assert.That(section2.MinY, Is.EqualTo(10.0));
            Assert.That(section2.MaxY, Is.EqualTo(37.5));

            var section3 = csDef.Sections.FirstOrDefault(s => s.SectionType.Name == Section3Name);
            Assert.IsNotNull(section3);
            Assert.That(section3.MinY, Is.EqualTo(37.5));
            Assert.That(section3.MaxY, Is.EqualTo(40.0));
        }

        [Test]
        public void GivenCrossSectionDefinitionZw_WhenTryingToAddSectionWithDuplicateName_ThenLogMessageAndSectionIsNotAdded()
        {
            var csDef = new CrossSectionDefinitionZW("myCrossSectionDefinition");
            csDef.AddSection(new CrossSectionSectionType { Name = Section1Name }, 20.0);
            Assert.That(csDef.Sections.Count, Is.EqualTo(1));

            var expectedMessage = "Could not add CrossSectionSection with duplicate name 'section1'";
            TestHelper.AssertAtLeastOneLogMessagesContains(() => csDef.AddSection(new CrossSectionSectionType { Name = Section1Name }, 13.0), expectedMessage);
            Assert.That(csDef.Sections.Count, Is.EqualTo(1));
        }

        [Test]
        public void GivenCrossSectionDefinitionZw_WhenTryingToAddSectionsWithNegativeWidth_ThenLogMessageAndSectionIsNotAdded()
        {
            var csDef = new CrossSectionDefinitionZW("myCrossSectionDefinition");
            var expectedMessage =
                "Could not add CrossSectionSection with negative length -2 to cross section definition 'myCrossSectionDefinition'";
            TestHelper.AssertAtLeastOneLogMessagesContains(() => csDef.AddSection(new CrossSectionSectionType { Name = Section1Name }, -2.0), expectedMessage);
            Assert.False(csDef.Sections.Any());
        }
    }
}