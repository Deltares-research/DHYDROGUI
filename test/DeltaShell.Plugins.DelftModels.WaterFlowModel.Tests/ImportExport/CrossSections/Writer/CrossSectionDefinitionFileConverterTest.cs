using System;
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
        private CrossSectionDefinitionFileConverter mockedConverter;
        private ICrossSection mockedCrossSection;
        private ICulvert mockedCulvert;
        private IBridge mockedBridge;

        [SetUp]
        public void Setup()
        {
            mockedModel = MockRepository.GeneratePartialMock<WaterFlowModel1D>();



            mockedCrossSection = MockRepository.GenerateMock<ICrossSection>();
            mockedCrossSection.Expect(cs => cs.Branch).Return(new Branch());
            mockedCrossSection.Expect(cs => cs.Chainage).Return(2.0);
            mockedCrossSection.Expect(cs => cs.Name).Return("crossSection1");

            mockedCulvert = MockRepository.GenerateMock<ICulvert>();
            mockedCulvert.Expect(m => m.CrossSectionDefinition)
                .Return(new CrossSectionDefinitionProxy(new CrossSectionDefinitionYZ())).Repeat.Any();

            mockedBridge = MockRepository.GenerateMock<IBridge>();

            mockedNetwork = MockRepository.GeneratePartialMock<HydroNetwork>();
            mockedNetwork.Expect(m => m.Culverts).Return(new List<ICulvert>() { mockedCulvert }).Repeat.Any();
            mockedNetwork.Expect(m => m.Bridges).Return(new List<IBridge>() { mockedBridge }).Repeat.Any();

            mockedConverter = MockRepository.GeneratePartialMock<CrossSectionDefinitionFileConverter>();
        }

        [Test]
        public void GivenWaterFlow1DModelWithSharedCrossSectionDefinition_WhenConvertingToCrossSectionDefinitionCategory_ThenCrossSectionDefinitionPropertyIsShared()
        {
            //Given
            var roughnessSection = CreateRoughnessSection();
         
            mockedModel.RoughnessSections.Add(roughnessSection);
            CreateSharedCrossSectionDefinition();
            var dependentCrossSection = CreateCrossSectionDependentUponSharedCrossSection(mockedCrossSection.Definition);
            Assert.That(mockedNetwork.SharedCrossSectionDefinitions.Count, Is.EqualTo(1));
            Assert.That(dependentCrossSection.Definition.Name, Is.EqualTo("csd1"));
            Assert.That(mockedCrossSection.Name, Is.EqualTo("crossSection1"));

            var network = SetupMockedNetwork(mockedCrossSection, dependentCrossSection);
            mockedModel.Network = network;
            Assert.IsNotNull(mockedModel.Network.CrossSections);
            Assert.That(mockedModel.Network.CrossSections.Count, Is.EqualTo(2));
            Assert.That(mockedModel.Network.SharedCrossSectionDefinitions.Count, Is.EqualTo(1));
            Assert.That(mockedModel.Network.SharedCrossSectionDefinitions.Count, Is.EqualTo(1));

            var mockedCrossSectionSection = MockRepository.GeneratePartialMock<CrossSectionSection>();
            var mockedCrossSectionSectionType = MockRepository.GeneratePartialMock<CrossSectionSectionType>();
            mockedCrossSectionSection.SectionType = mockedCrossSectionSectionType;
            mockedCrossSectionSection.SectionType.Name = "Main";

            mockedCrossSection.Definition.Sections.Add(mockedCrossSectionSection);
            Assert.That(mockedCrossSection.Definition.Sections.Count, Is.EqualTo(2));
            // When
            var categories = mockedConverter.Convert(mockedModel).ToList();
            Assert.That(categories.Count(), Is.EqualTo(2));

            //Then
            var delftIniProperties = categories.ElementAt(1).Properties;
            Assert.That(delftIniProperties.Any(p => p.Name.Contains("isShared") && p.Value.Contains("1")));
        }

        [Test]
        public void GivenWaterFlow1DModelWithSharedCrossSectionDefinitionAndChangedBackToLocalDefinition_WhenConvertingToCrossSectionDefinitionCategory_ThenCrossSectionDefinitionIsRenamed()
        {
            CreateSharedCrossSectionDefinition();
            var dependentCrossSection = CreateCrossSectionDependentUponSharedCrossSection(mockedCrossSection.Definition);
            Assert.That(mockedNetwork.SharedCrossSectionDefinitions.Count, Is.EqualTo(1));
            Assert.That(dependentCrossSection.Definition.Name, Is.EqualTo("csd1"));
            Assert.That(mockedCrossSection.Name, Is.EqualTo("crossSection1"));
            Assert.That(mockedCrossSection.Definition.IsProxy, Is.EqualTo(true));

            mockedCrossSection.MakeDefinitionLocal();
            Assert.That(mockedNetwork.SharedCrossSectionDefinitions.Count, Is.EqualTo(1));
            Assert.That(dependentCrossSection.Definition.Name, Is.EqualTo("csd1"));
            Assert.That(mockedCrossSection.Definition.IsProxy, Is.EqualTo(false));


            var network = SetupMockedNetwork(mockedCrossSection, dependentCrossSection);
            mockedModel.Network = network;
            Assert.IsNotNull(mockedModel.Network.CrossSections);
            Assert.That(mockedModel.Network.CrossSections.Count, Is.EqualTo(2));
            Assert.That(mockedModel.Network.SharedCrossSectionDefinitions.Count, Is.EqualTo(1));
        }

        private static RoughnessSection CreateRoughnessSection()
        {
            var node1 = MockRepository.GenerateMock<INode>();
            var node2 = MockRepository.GenerateMock<INode>();
            var network = WaterFlowModel1DTestHelper.CreateSimplerNetwork(out node1, out node2);
            var roughnessSection = new RoughnessSection(new CrossSectionSectionType(), network) {Name = "Main"};

            return roughnessSection;
        }

        private void CreateSharedCrossSectionDefinition()
        {
            var crossSectionDefinition = new CrossSectionDefinitionProxy(new CrossSectionDefinitionYZ());
            var crossSectionSectionType = new CrossSectionSectionType();
            crossSectionSectionType.Name = "Main";
            crossSectionDefinition.Sections.Add(new CrossSectionSection
            {
                SectionType = crossSectionSectionType,

                MinY = -1.0,
                MaxY = 2.0
            });


            var innerDefinitionName = crossSectionDefinition.InnerDefinition.Name = "csd1";
            Assert.That(crossSectionDefinition.IsProxy, Is.EqualTo(true));
            Assert.That(innerDefinitionName, Is.EqualTo("csd1"));

            mockedCrossSection.Expect(mcsd => mcsd.UseSharedDefinition(crossSectionDefinition));
            mockedCrossSection.Expect(mcs => mcs.Definition).Return(crossSectionDefinition).Repeat.Any();
            mockedNetwork.SharedCrossSectionDefinitions.Add(crossSectionDefinition);
            Assert.That(mockedNetwork.SharedCrossSectionDefinitions.Count, Is.EqualTo(1));
            Assert.That(mockedCrossSection.Definition, Is.Not.Null);

            mockedCrossSection.Definition.Name = "csd1";
            mockedCrossSection.ShareDefinitionAndChangeToProxy();
        }

        private ICrossSection CreateCrossSectionDependentUponSharedCrossSection(ICrossSectionDefinition definition)
        {
            var mockedCrossSection2 = MockRepository.GenerateMock<ICrossSection>();

            mockedCrossSection2.Expect(mcsd => mcsd.UseSharedDefinition(definition)).Repeat.Any();
            mockedCrossSection2.Expect(mcs => mcs.Definition).Return(definition).Repeat.Any();

            return mockedCrossSection2;
        }

        private HydroNetwork SetupMockedNetwork(ICrossSection crossSection, ICrossSection crossSection2)
        {
            mockedNetwork.Expect(m => m.CrossSections).Return(new List<ICrossSection>()
                {crossSection, crossSection2}).Repeat.Any();

            return mockedNetwork;
        }
    }
}
