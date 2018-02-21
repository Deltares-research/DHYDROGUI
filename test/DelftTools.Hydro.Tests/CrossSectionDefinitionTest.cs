using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Tests.TestObjects;
using DelftTools.Utils.Reflection;
using GeoAPI.Geometries;
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
            csDef.AddSection(new CrossSectionSectionType { Name = CrossSectionDefinition.MainSectionName }, 60.0);
            csDef.AddSection(new CrossSectionSectionType { Name = CrossSectionDefinitionZW.Floodplain1SectionTypeName }, 16.0);

            var mainSectionWidth = csDef.GetSectionWidth(CrossSectionDefinition.MainSectionName);
            Assert.That(mainSectionWidth, Is.EqualTo(60.0));

            var fp1SectionWidth = csDef.GetSectionWidth(CrossSectionDefinitionZW.Floodplain1SectionTypeName);
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

        #region RefreshSectionWidths

        [Test]
        public void TestRefreshSectionWidths_CreatesNewMainSectionIfNoSectionsArePresent()
        {
            var csDef = new TestCrossSectionDefinition();
            TypeUtils.SetField(csDef, "profile", new List<Coordinate>()
            {
                new Coordinate(0, 0),
                new Coordinate(40, -10.0),
                new Coordinate(60, -10.0),
                new Coordinate(100, 0)
            });

            Assert.IsTrue(!csDef.Sections.Any());

            csDef.RefreshSectionsWidths();
            var mainSection = csDef.Sections.FirstOrDefault(css => css.SectionType.Name == CrossSectionDefinition.MainSectionName);
            Assert.NotNull(mainSection);

            Assert.AreEqual(csDef.Left, mainSection.MinY);
            Assert.AreEqual(csDef.Right, mainSection.MaxY);
        }

        #endregion
    }
}
