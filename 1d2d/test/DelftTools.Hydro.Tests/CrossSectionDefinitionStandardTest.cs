using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class CrossSectionDefinitionStandardTest
    {
        [Test]
        public void CreateDefault()
        {
            var css = new CrossSectionDefinitionStandard();

            Assert.AreEqual(css.Shape.GetType(), typeof(CrossSectionStandardShapeRectangle));

            var rectangle = css.Shape as CrossSectionStandardShapeRectangle;

            Assert.Greater(rectangle.Width, 0.0);
            Assert.Greater(rectangle.Height, 0.0);
        }

        [Test]
        public void MakeLocalAddsLevelShift()
        {
            var innerDefinition = new CrossSectionDefinitionStandard();
            innerDefinition.LevelShift = 2;

            var proxyDefinition = new CrossSectionDefinitionProxy(innerDefinition);
            var crossSection = new CrossSection(proxyDefinition);

            proxyDefinition.LevelShift = 5;

            //unproxy the cs should add up the level shift
            crossSection.MakeDefinitionLocal();

            Assert.IsTrue(crossSection.Definition is CrossSectionDefinitionStandard);
            Assert.AreEqual(7, ((CrossSectionDefinitionStandard)crossSection.Definition).LevelShift);
        }
    }
}