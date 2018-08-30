using DelftTools.Hydro.CrossSections;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.CrossSectionDefinitionGenerators;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class CrossSectionDefinitionIniCategoryGeneratorFactoryTest
    {
        [Test]
        public void GivenRoundShape_WhenGettingIniCategoryGenerator_ThenItIsOfTheCorrectType()
        {
            var generator = CrossSectionDefinitionIniCategoryGeneratorFactory.GetIniCategoryGenerator(CrossSectionStandardShapeType.Circle);
            Assert.IsTrue(generator is CrossSectionDefinitionIniCategoryGeneratorCircle);
        }

        [Test]
        public void GivenEllipticalShape_WhenGettingIniCategoryGenerator_ThenItIsOfTheCorrectType()
        {
            var generator = CrossSectionDefinitionIniCategoryGeneratorFactory.GetIniCategoryGenerator(CrossSectionStandardShapeType.Elliptical);
            Assert.IsTrue(generator is CrossSectionDefinitionIniCategoryGeneratorElliptical);
        }

        [Test]
        public void GivenRectangleShape_WhenGettingIniCategoryGenerator_ThenItIsOfTheCorrectType()
        {
            var generator = CrossSectionDefinitionIniCategoryGeneratorFactory.GetIniCategoryGenerator(CrossSectionStandardShapeType.Rectangle);
            Assert.IsTrue(generator is CrossSectionDefinitionIniCategoryGeneratorRectangle);
        }

        [Test]
        public void GivenEggShape_WhenGettingIniCategoryGenerator_ThenItIsOfTheCorrectType()
        {
            var generator = CrossSectionDefinitionIniCategoryGeneratorFactory.GetIniCategoryGenerator(CrossSectionStandardShapeType.Egg);
            Assert.IsTrue(generator is CrossSectionDefinitionIniCategoryGeneratorEgg);
        }

        [Test]
        public void GivenArchShape_WhenGettingIniCategoryGenerator_ThenItIsOfTheCorrectType()
        {
            var generator = CrossSectionDefinitionIniCategoryGeneratorFactory.GetIniCategoryGenerator(CrossSectionStandardShapeType.Arch);
            Assert.IsTrue(generator is CrossSectionDefinitionIniCategoryGeneratorArch);
        }

        [Test]
        public void GivenCunetteShape_WhenGettingIniCategoryGenerator_ThenItIsOfTheCorrectType()
        {
            var generator = CrossSectionDefinitionIniCategoryGeneratorFactory.GetIniCategoryGenerator(CrossSectionStandardShapeType.Cunette);
            Assert.IsTrue(generator is CrossSectionDefinitionIniCategoryGeneratorCunette);
        }

        [Test]
        public void GivenSteelCunetteShape_WhenGettingIniCategoryGenerator_ThenItIsOfTheCorrectType()
        {
            var generator = CrossSectionDefinitionIniCategoryGeneratorFactory.GetIniCategoryGenerator(CrossSectionStandardShapeType.SteelCunette);
            Assert.IsTrue(generator is CrossSectionDefinitionIniCategoryGeneratorSteelCunette);
        }

        [Test]
        public void GivenTrapeziumShape_WhenGettingIniCategoryGenerator_ThenItIsOfTheCorrectType()
        {
            var generator = CrossSectionDefinitionIniCategoryGeneratorFactory.GetIniCategoryGenerator(CrossSectionStandardShapeType.Trapezium);
            Assert.IsTrue(generator is CrossSectionDefinitionIniCategoryGeneratorTrapezium);
        }
    }
}