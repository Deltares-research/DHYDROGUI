using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.CrossSectionDefinition
{
    [TestFixture]
    public class CrossSectionDefinitionIniCategoryGeneratorFactoryTest
    {
        [Test]
        public void GivenRoundShape_WhenGettingIniCategoryGenerator_ThenItIsOfTheCorrectType()
        {
            var generator = DefinitionIniCategoryGeneratorFactory.GetCrossSectionDefinitionIniCategoryGenerator(CrossSectionStandardShapeType.Circle);
            Assert.IsTrue(generator is DefinitionGeneratorCrossSectionDefinitionCircle);
        }

        [Test]
        public void GivenEllipticalShape_WhenGettingIniCategoryGenerator_ThenItIsOfTheCorrectType()
        {
            var generator = DefinitionIniCategoryGeneratorFactory.GetCrossSectionDefinitionIniCategoryGenerator(CrossSectionStandardShapeType.Elliptical);
            Assert.IsTrue(generator is DefinitionGeneratorCrossSectionDefinitionElliptical);
        }

        [Test]
        public void GivenRectangleShape_WhenGettingIniCategoryGenerator_ThenItIsOfTheCorrectType()
        {
            var generator = DefinitionIniCategoryGeneratorFactory.GetCrossSectionDefinitionIniCategoryGenerator(CrossSectionStandardShapeType.Rectangle);
            Assert.IsTrue(generator is DefinitionGeneratorCrossSectionDefinitionRectangle);
        }

        [Test]
        public void GivenEggShape_WhenGettingIniCategoryGenerator_ThenItIsOfTheCorrectType()
        {
            var generator = DefinitionIniCategoryGeneratorFactory.GetCrossSectionDefinitionIniCategoryGenerator(CrossSectionStandardShapeType.Egg);
            Assert.IsTrue(generator is DefinitionGeneratorCrossSectionDefinitionEgg);
        }

        [Test]
        public void GivenArchShape_WhenGettingIniCategoryGenerator_ThenItIsOfTheCorrectType()
        {
            var generator = DefinitionIniCategoryGeneratorFactory.GetCrossSectionDefinitionIniCategoryGenerator(CrossSectionStandardShapeType.Arch);
            Assert.IsTrue(generator is DefinitionGeneratorCrossSectionDefinitionArch);
        }

        [Test]
        public void GivenCunetteShape_WhenGettingIniCategoryGenerator_ThenItIsOfTheCorrectType()
        {
            var generator = DefinitionIniCategoryGeneratorFactory.GetCrossSectionDefinitionIniCategoryGenerator(CrossSectionStandardShapeType.Cunette);
            Assert.IsTrue(generator is DefinitionGeneratorCrossSectionDefinitionCunette);
        }

        [Test]
        public void GivenSteelCunetteShape_WhenGettingIniCategoryGenerator_ThenItIsOfTheCorrectType()
        {
            var generator = DefinitionIniCategoryGeneratorFactory.GetCrossSectionDefinitionIniCategoryGenerator(CrossSectionStandardShapeType.SteelCunette);
            Assert.IsTrue(generator is DefinitionGeneratorCrossSectionDefinitionSteelCunette);
        }

        [Test]
        public void GivenTrapeziumShape_WhenGettingIniCategoryGenerator_ThenItIsOfTheCorrectType()
        {
            var generator = DefinitionIniCategoryGeneratorFactory.GetCrossSectionDefinitionIniCategoryGenerator(CrossSectionStandardShapeType.Trapezium);
            Assert.IsTrue(generator is DefinitionGeneratorCrossSectionDefinitionTrapezium);
        }
    }
}