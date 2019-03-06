using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections.Writer;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.CrossSections.Writer
{
    [TestFixture]
    public class RoughnessDataProcessorTest
    {
        private const string RoughnessSectionName1 = "myName1";
        private const string RoughnessSectionName2 = "myName2";
        private readonly CrossSectionSectionType sectionType1 = new CrossSectionSectionType
        {
            Name = RoughnessSectionName1
        };
        private readonly CrossSectionSectionType sectionType2 = new CrossSectionSectionType
        {
            Name = RoughnessSectionName2
        };


        [Test]
        public void GivenCrossSectionWithOneSectionMatchingWithRoughnessSection_WhenAddRoughnessDataToDataModel_ThenRoughnessNamesAndPositionsAreAsExpected()
        {
            // Given
            const double minY = 1.0;
            const double maxY = 2.0;

            var crossSection = GetCrossSectionWithOneSection(sectionType1, minY, maxY);
            var roughnessSections = GetRoughnessSections(sectionType1).ToList();

            // When
            var category = RoughnessDataProcessor.AddRoughnessDataToFileContent(new DelftIniCategory("myName"), crossSection, roughnessSections, false);

            // Then
            var sectionCount = category.GetPropertyValue(DefinitionRegion.SectionCount.Key);
            Assert.That(int.Parse(sectionCount), Is.EqualTo(1));

            var roughnessNames = category.GetPropertyValue(DefinitionRegion.RoughnessNames.Key).Split(';');
            Assert.That(roughnessNames.Length, Is.EqualTo(1));
            Assert.Contains(RoughnessSectionName1, roughnessNames);

            var roughnessPositions = category.GetPropertyValue(DefinitionRegion.RoughnessPositions.Key).Split(' ').Select(v => double.Parse(v, CultureInfo.InvariantCulture)).ToArray();
            Assert.That(roughnessPositions.Length, Is.EqualTo(2));
            Assert.Contains(minY, roughnessPositions);
            Assert.Contains(maxY, roughnessPositions);
        }

        [Test]
        public void GivenCrossSectionWithTwoSectionsMatchingWithRoughnessSections_WhenAddRoughnessDataToDataModel_ThenRoughnessNamesAndPositionsAreAsExpected()
        {
            // Given
            const double y1 = 0.0;
            const double y2 = 1.0;
            const double y3 = 2.0;

            var crossSection = GetCrossSectionWithTwoSections(y1, y2, y3);
            var roughnessSections = GetRoughnessSections(sectionType1, sectionType2).ToList();

            // When
            var category = RoughnessDataProcessor.AddRoughnessDataToFileContent(new DelftIniCategory("myName"), crossSection, roughnessSections, false);

            // Then
            var sectionCount = category.GetPropertyValue(DefinitionRegion.SectionCount.Key);
            Assert.That(int.Parse(sectionCount), Is.EqualTo(2));

            var roughnessNames = category.GetPropertyValue(DefinitionRegion.RoughnessNames.Key).Split(';');
            Assert.That(roughnessNames.Length, Is.EqualTo(2));
            Assert.Contains(RoughnessSectionName1, roughnessNames);
            Assert.Contains(RoughnessSectionName2, roughnessNames);

            var roughnessPositions = category.GetPropertyValue(DefinitionRegion.RoughnessPositions.Key).Split(' ').Select(v => double.Parse(v, CultureInfo.InvariantCulture)).ToArray();
            Assert.That(roughnessPositions.Length, Is.EqualTo(3));
            Assert.Contains(y1, roughnessPositions);
            Assert.Contains(y2, roughnessPositions);
            Assert.Contains(y3, roughnessPositions);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void GivenCrossSectionWithSectionNotMatchingWithAnyRoughnessSection_WhenAddRoughnessDataToDataModel_ThenInvalidOperationExceptionIsThrown()
        {
            // Given
            var crossSection = GetCrossSectionWithOneSection(sectionType1, 0.0, 1.0);
            var roughnessSections = GetRoughnessSections(sectionType2).ToList();

            // When - Then
            RoughnessDataProcessor.AddRoughnessDataToFileContent(new DelftIniCategory("myName"), crossSection, roughnessSections, false);
        }

        private static IEnumerable<RoughnessSection> GetRoughnessSections(params CrossSectionSectionType[] crossSectionSectionTypes)
        {
            var network = new HydroNetwork();
            foreach (var sectionSectionType in crossSectionSectionTypes)
            {
                yield return new RoughnessSection(sectionSectionType, network);
            }
        }

        private static CrossSection GetCrossSectionWithOneSection(CrossSectionSectionType crossSectionSectionType, double minY, double maxY)
        {
            var crossSectionSection = new CrossSectionSection {SectionType = crossSectionSectionType, MinY = minY, MaxY = maxY};
            var definitionRound = new CrossSectionDefinitionStandard(new CrossSectionStandardShapeRound());
            definitionRound.Sections.Add(crossSectionSection);

            var crossSection = new CrossSection(definitionRound)
            {
                Branch = new Branch {Length = 20.0},
                Chainage = 10.0
            };
            return crossSection;
        }

        private CrossSection GetCrossSectionWithTwoSections(params double[] borders)
        {
            Assert.That(borders.Length, Is.EqualTo(3));

            var crossSectionSection1 = new CrossSectionSection { SectionType = sectionType1, MinY = borders[0], MaxY = borders[1] };
            var crossSectionSection2 = new CrossSectionSection { SectionType = sectionType2, MinY = borders[1], MaxY = borders[2] };
            var definitionRound = new CrossSectionDefinitionStandard(new CrossSectionStandardShapeRound());
            definitionRound.Sections.Add(crossSectionSection1);
            definitionRound.Sections.Add(crossSectionSection2);

            var crossSection = new CrossSection(definitionRound)
            {
                Branch = new Branch { Length = 20.0 },
                Chainage = 10.0
            };
            return crossSection;
        }
    }
}