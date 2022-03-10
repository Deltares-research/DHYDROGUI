using System;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;
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

        [TestCase(true)]
        [TestCase(false)]
        public void GivenRectangleShapeCategoryGenerator_WhenCreatingCategory_ThenIsItOpenOrClosedProfile(bool isClosed)
        {
            var categoryGenerator = DefinitionIniCategoryGeneratorFactory.GetCrossSectionDefinitionIniCategoryGenerator(CrossSectionStandardShapeType.Rectangle);
            var csdStd = (CrossSectionDefinitionStandard)CrossSectionDefinitionStandard.CreateDefault();
            Assert.That(csdStd.Shape, Is.InstanceOf<ICrossSectionStandardShapeOpenClosed>());
            var rect = (ICrossSectionStandardShapeOpenClosed) csdStd.Shape;
            rect.Closed = isClosed;
            var category = categoryGenerator.CreateDefinitionRegion(csdStd, true, "");
            Assert.IsTrue(category.Properties.Any(p => p.Name.Equals(DefinitionPropertySettings.Closed.Key, StringComparison.InvariantCultureIgnoreCase)));
            Assert.IsTrue(category.ReadProperty<string>(DefinitionPropertySettings.Closed.Key).Equals(isClosed ? "yes" : "no", StringComparison.CurrentCultureIgnoreCase));

            var csdGenerator = DefinitionGeneratorFactory.GetDefinitionReaderCrossSection(CrossSectionRegion.CrossSectionDefinitionType.Rectangle);
            var readCsd = csdGenerator.ReadDefinition(category) as CrossSectionDefinitionStandard;
            Assert.IsNotNull(readCsd);
            Assert.IsTrue(csdStd.Shape.GetTabulatedDefinition().RawData.ContentEquals(readCsd.Shape.GetTabulatedDefinition().RawData));
            Assert.That((csdStd.Shape as ICrossSectionStandardShapeOpenClosed).Closed, Is.EqualTo((readCsd.Shape as ICrossSectionStandardShapeOpenClosed).Closed));
            var stdCsdCoordinates = csdStd.Shape.Profile.ToArray();
            var readCsdCoordinates = readCsd.Shape.Profile.ToArray();
            Assert.That(stdCsdCoordinates,Is.EqualTo(readCsdCoordinates));
            Assert.That(csdStd.GetProfile().ToArray(),Is.EqualTo(readCsdCoordinates));
            Assert.That(stdCsdCoordinates.Length, Is.EqualTo(isClosed ? 6 : 4));
            Assert.That(readCsdCoordinates.Length, Is.EqualTo(isClosed ? 6 : 4));
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