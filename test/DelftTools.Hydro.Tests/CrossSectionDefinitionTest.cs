using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Tests.TestObjects;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class CrossSectionDefinitionTest
    {
        #region GetSectionWidth

        [Test]
        public void GivenCrossSectionWithSections_WhenGettingSectionWidthByName_ThenCorrectSectionWidthIsReturned()
        {
            var csDef = new TestCrossSectionDefinition();
            csDef.AddSection(new CrossSectionSectionType {Name = CrossSectionDefinition.MainSectionName}, 60.0);
            csDef.AddSection(new CrossSectionSectionType {Name = CrossSectionDefinitionZW.Floodplain1SectionTypeName}, 16.0);

            double mainSectionWidth = csDef.GetSectionWidth(CrossSectionDefinition.MainSectionName);
            Assert.That(mainSectionWidth, Is.EqualTo(60.0));

            double fp1SectionWidth = csDef.GetSectionWidth(CrossSectionDefinitionZW.Floodplain1SectionTypeName);
            Assert.That(fp1SectionWidth, Is.EqualTo(16.0));
        }

        [Test]
        public void GivenCrossSectionWithSections_WhenGettingSectionWidthByNameWithUnexistingName_ThenZeroIsReturned()
        {
            var csDef = new TestCrossSectionDefinition();
            double width = csDef.GetSectionWidth("NoExistingSectionTypeName");
            Assert.That(width, Is.EqualTo(0.0));
        }

        #endregion
    }
}