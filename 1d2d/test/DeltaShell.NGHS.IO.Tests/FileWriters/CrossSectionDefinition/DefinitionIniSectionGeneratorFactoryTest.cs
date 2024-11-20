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
    public class DefinitionIniSectionGeneratorFactoryTest
    {
        [Test]
        public void GivenRoundShape_WhenGettingIniSectionGenerator_ThenItIsOfTheCorrectType()
        {
            var generator = DefinitionIniSectionGeneratorFactory.GetCrossSectionDefinitionIniSectionGenerator(CrossSectionStandardShapeType.Circle);
            Assert.IsTrue(generator is DefinitionGeneratorCrossSectionDefinitionCircle);
        }

        [Test]
        public void GivenEllipticalShape_WhenGettingIniSectionGenerator_ThenItIsOfTheCorrectType()
        {
            var generator = DefinitionIniSectionGeneratorFactory.GetCrossSectionDefinitionIniSectionGenerator(CrossSectionStandardShapeType.Elliptical);
            Assert.IsTrue(generator is DefinitionGeneratorCrossSectionDefinitionElliptical);
        }

        [Test]
        public void GivenRectangleShape_WhenGettingIniSectionGenerator_ThenItIsOfTheCorrectType()
        {
            var generator = DefinitionIniSectionGeneratorFactory.GetCrossSectionDefinitionIniSectionGenerator(CrossSectionStandardShapeType.Rectangle);
            Assert.IsTrue(generator is DefinitionGeneratorCrossSectionDefinitionRectangle);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GivenRectangleShapeSectionGenerator_WhenCreatingSection_ThenIsItOpenOrClosedProfile(bool isClosed)
        {
            var sectionGenerator = DefinitionIniSectionGeneratorFactory.GetCrossSectionDefinitionIniSectionGenerator(CrossSectionStandardShapeType.Rectangle);
            var csdStd = (CrossSectionDefinitionStandard)CrossSectionDefinitionStandard.CreateDefault();
            Assert.That(csdStd.Shape, Is.InstanceOf<ICrossSectionStandardShapeOpenClosed>());
            var rect = (ICrossSectionStandardShapeOpenClosed) csdStd.Shape;
            rect.Closed = isClosed;
            var iniSection = sectionGenerator.CreateDefinitionRegion(csdStd, true, "");
            Assert.IsTrue(iniSection.Properties.Any(p => p.Key.Equals(DefinitionPropertySettings.Closed.Key, StringComparison.InvariantCultureIgnoreCase)));
            Assert.IsTrue(iniSection.ReadProperty<string>(DefinitionPropertySettings.Closed.Key).Equals(isClosed ? "yes" : "no", StringComparison.CurrentCultureIgnoreCase));

            var csdGenerator = DefinitionGeneratorFactory.GetDefinitionReaderCrossSection(CrossSectionRegion.CrossSectionDefinitionType.Rectangle);
            var readCsd = csdGenerator.ReadDefinition(iniSection) as CrossSectionDefinitionStandard;
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
        public void GivenEggShape_WhenGettingIniSectionGenerator_ThenItIsOfTheCorrectType()
        {
            var generator = DefinitionIniSectionGeneratorFactory.GetCrossSectionDefinitionIniSectionGenerator(CrossSectionStandardShapeType.Egg);
            Assert.IsTrue(generator is DefinitionGeneratorCrossSectionDefinitionEgg);
        }

        [Test]
        public void GivenArchShape_WhenGettingIniSectionGenerator_ThenItIsOfTheCorrectType()
        {
            var generator = DefinitionIniSectionGeneratorFactory.GetCrossSectionDefinitionIniSectionGenerator(CrossSectionStandardShapeType.Arch);
            Assert.IsTrue(generator is DefinitionGeneratorCrossSectionDefinitionArch);
        }

        [Test]
        public void GivenCunetteShape_WhenGettingIniSectionGenerator_ThenItIsOfTheCorrectType()
        {
            var generator = DefinitionIniSectionGeneratorFactory.GetCrossSectionDefinitionIniSectionGenerator(CrossSectionStandardShapeType.Cunette);
            Assert.IsTrue(generator is DefinitionGeneratorCrossSectionDefinitionCunette);
        }

        [Test]
        public void GivenSteelCunetteShape_WhenGettingIniSectionGenerator_ThenItIsOfTheCorrectType()
        {
            var generator = DefinitionIniSectionGeneratorFactory.GetCrossSectionDefinitionIniSectionGenerator(CrossSectionStandardShapeType.SteelCunette);
            Assert.IsTrue(generator is DefinitionGeneratorCrossSectionDefinitionSteelCunette);
        }

        [Test]
        public void GivenTrapeziumShape_WhenGettingIniSectionGenerator_ThenItIsOfTheCorrectType()
        {
            var generator = DefinitionIniSectionGeneratorFactory.GetCrossSectionDefinitionIniSectionGenerator(CrossSectionStandardShapeType.Trapezium);
            Assert.IsTrue(generator is DefinitionGeneratorCrossSectionDefinitionTrapezium);
        }
    }
}