using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
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
            csDef.AddSection(new CrossSectionSectionType { Name = RoughnessDataSet.MainSectionTypeName }, 60.0);
            csDef.AddSection(new CrossSectionSectionType { Name = RoughnessDataSet.Floodplain1SectionTypeName }, 16.0);

            var mainSectionWidth = csDef.GetSectionWidth(RoughnessDataSet.MainSectionTypeName);
            Assert.That(mainSectionWidth, Is.EqualTo(60.0));

            var fp1SectionWidth = csDef.GetSectionWidth(RoughnessDataSet.Floodplain1SectionTypeName);
            Assert.That(fp1SectionWidth, Is.EqualTo(16.0));
        }

        [Test]
        public void GivenCrossSectionWithSections_WhenGettingSectionWidthByNameWithUnexistingName_ThenZeroIsReturned()
        {
            var csDef = new TestCrossSectionDefinition();
            var width = csDef.GetSectionWidth("NoExistingSectionTypeName");
            Assert.That(width, Is.EqualTo(0.0));
        }

        #endregion

    }
}
