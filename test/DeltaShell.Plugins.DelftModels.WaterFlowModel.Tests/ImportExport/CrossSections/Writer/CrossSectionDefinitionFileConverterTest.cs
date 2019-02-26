using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections.Writer;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.CrossSections.Writer
{
    [TestFixture]
    public class CrossSectionDefinitionFileConverterTest
    {
        private WaterFlowModel1D mockedModel;
        private HydroNetwork mockedNetwork;
        private CrossSectionDefinitionFileConverter converter;

        [SetUp]
        public void Setup()
        {
            mockedModel = MockRepository.GeneratePartialMock<WaterFlowModel1D>();
            mockedNetwork = MockRepository.GeneratePartialMock<HydroNetwork>();
            converter = MockRepository.GenerateMock<CrossSectionDefinitionFileConverter>();
        }

        [Test]
        public void GivenWaterFlow1DModelWithCrossSectionDependedUponSharedCrossSectionDefinition_WhenConverting_ThenIsShared2()
        {
            //Given
            CreateWaterFlow1DModelWith1SharedCrossSectionDefinition();

            // When
            var categories = converter.Convert(mockedModel).ToList();
            Assert.That(categories.Count(), Is.EqualTo(2));

            //Then
            var delftIniProperties = categories.ElementAt(1).Properties;
            Assert.That(delftIniProperties.Any(p => p.Name.Contains("isShared") && p.Value.Contains("1")));
        }

        private void CreateWaterFlow1DModelWith1SharedCrossSectionDefinition()
        {
            var mockedCrossSection = MockRepository.GenerateMock<ICrossSection>();
            mockedCrossSection.Expect(cs => cs.Branch).Return(new Branch());
            mockedCrossSection.Expect(cs => cs.Chainage).Return(2.0);

            var crossSectionDefinition = new CrossSectionDefinitionProxy(new CrossSectionDefinitionYZ());
            var crossSectionSectionType = new CrossSectionSectionType();
            crossSectionSectionType.Name = "Main";
            crossSectionDefinition.Sections.Add(new CrossSectionSection
            {
                SectionType = crossSectionSectionType,

                MinY = -1.0,
                MaxY = 2.0
            });
            crossSectionDefinition.Name = "csd1";
            Assert.That(crossSectionDefinition.IsProxy, Is.EqualTo(true));

            mockedCrossSection.Expect(mcsd => mcsd.UseSharedDefinition(new CrossSectionDefinitionYZ())).Repeat.Any();
            mockedCrossSection.Expect(mcs => mcs.Definition).Return(crossSectionDefinition).Repeat.Any();
            mockedNetwork.SharedCrossSectionDefinitions.Add(crossSectionDefinition);
            Assert.That(mockedNetwork.SharedCrossSectionDefinitions.Count, Is.EqualTo(1));
            Assert.That(mockedCrossSection.Definition, Is.Not.Null);

            mockedCrossSection.Definition.Name = "csd1";
            mockedCrossSection.ShareDefinitionAndChangeToProxy();

            var mockedCrossSection2 = MockRepository.GenerateMock<ICrossSection>();

            mockedCrossSection2.Expect(mcsd => mcsd.UseSharedDefinition(new CrossSectionDefinitionYZ())).Repeat.Any();
            mockedCrossSection2.Expect(mcs => mcs.Definition).Return(crossSectionDefinition).Repeat.Any();

            var node1 = MockRepository.GenerateMock<INode>();
            var node2 = MockRepository.GenerateMock<INode>();
            var network = WaterFlowModel1DTestHelper.CreateSimplerNetwork(out node1, out node2);
            var roughnessSection = new RoughnessSection(new CrossSectionSectionType(), network) {Name = "Main"};
            mockedModel.RoughnessSections.Add(roughnessSection);

            var mockedCulvert = MockRepository.GenerateMock<ICulvert>();
            mockedCulvert.Expect(m => m.CrossSectionDefinition)
                .Return(new CrossSectionDefinitionProxy(new CrossSectionDefinitionYZ())).Repeat.Any();
            mockedNetwork.Expect(m => m.Culverts).Return(new List<ICulvert>() {mockedCulvert}).Repeat.Any();
            mockedNetwork.Expect(m => m.CrossSections).Return(new List<ICrossSection>()
                {mockedCrossSection, mockedCrossSection2}).Repeat.Any();

            var bridge = MockRepository.GenerateMock<IBridge>();
            mockedNetwork.Expect(m => m.Bridges).Return(new List<IBridge>() {bridge}).Repeat.Any();

            mockedModel.Network = mockedNetwork;
            Assert.IsNotNull(mockedModel.Network.CrossSections);
            Assert.That(mockedModel.Network.CrossSections.Count, Is.EqualTo(2));
        }
    }
}
