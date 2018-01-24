using DelftTools.Hydro.CrossSections;
using DelftTools.Utils;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class CrossSectionSectionTest
    {
        private const double DefaultMinY = 0.0;
        private const double DefaultMaxY = 25.0;
        private const double DefaultWidth = 50.0;

        [Test]
        public void CrossSectionSectionReturnsCorrectWidth()
        {
            var section = GetDefaultCrossSectionSection(DefaultMinY, DefaultMaxY);
            Assert.That(section.Width, Is.EqualTo(DefaultWidth));
        }

        [TestCase(DefaultMinY, DefaultMaxY, 30.0)]
        [TestCase(DefaultMinY, DefaultMaxY, 0.0)]
        [TestCase(DefaultMinY, DefaultMaxY, -20.0)]
        [TestCase(25.0, 40.0, 30.0)]
        [TestCase(25.0, 40.0, 0.0)]
        [TestCase(25.0, 40.0, -20.0)]
        public void GivenCrossSectionSection_WhenAddingToSectionWidth_ThenPropertiesAreChangedCorrectly(double minY, double maxY, double addedWidth)
        {
            var defaultSectionWidth = 2 * (maxY - minY);
            var section = GetDefaultCrossSectionSection(minY, maxY);
            Assert.That(section.Width, Is.EqualTo(defaultSectionWidth));
            Assert.That(section.MinY, Is.EqualTo(minY));
            Assert.That(section.MaxY, Is.EqualTo(maxY));

            section.Width += addedWidth;
            Assert.That(section.Width, Is.EqualTo(defaultSectionWidth + addedWidth));
            Assert.That(section.MinY, Is.EqualTo(minY));
            Assert.That(section.MaxY, Is.EqualTo(maxY + addedWidth * 0.5));
        }

        #region Test Helpers

        private static CrossSectionSection GetDefaultCrossSectionSection(double minY, double maxY)
        {
            var section = new CrossSectionSection
            {
                MinY = minY,
                MaxY = maxY
            };
            return section;
        }

        #endregion

        [Test]
        [Ignore("WIP")]
        public void UnsubscribeOldSectionType()
        {
            var section = new CrossSectionSection();
            var old = new CrossSectionSectionType { Name = "old" };
            section.SectionType = old;

            var newType = new CrossSectionSectionType { Name = "new" };
            section.SectionType = newType;

            ((INotifyPropertyChange)section).PropertyChanging += (s, e) => Assert.Fail("FAAAAAAAAAAAAAAIL!");
            old.Name = "hoi";
        }
    }
}