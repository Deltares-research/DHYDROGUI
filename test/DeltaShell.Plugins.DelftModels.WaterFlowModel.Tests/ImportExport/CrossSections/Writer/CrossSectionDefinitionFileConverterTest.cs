using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
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
        private CrossSectionDefinitionFileConverter converter;
        private CrossSectionSectionType crossSectionSectionType;
        private const string CrossSectionName = "crossSection1";

        [SetUp]
        public void Setup()
        {
            crossSectionSectionType = GetMockedCrossSectionSectionType();
            converter = new CrossSectionDefinitionFileConverter();
        }

        private static CrossSectionSectionType GetMockedCrossSectionSectionType()
        {
            var crossSectionSectionType = MockRepository.GeneratePartialMock<CrossSectionSectionType>();
            crossSectionSectionType.Expect(s => s.Name).Return("Main").Repeat.Any();
            return crossSectionSectionType;
        }

        [Test]
        public void GivenWaterFlow1DModelWithSharedCrossSectionDefinitionAndChangedBackToLocalDefinition_WhenConvertingToCrossSectionDefinitionCategory_ThenIsSharedIsWrittenToProperty()
        {
            //Given
            var flowModel = GetMockedWaterFlowModelWithSharedCrossSectionDefinition();

            //When
            var categories = converter.Convert(flowModel).ToArray();

            //Then
            var properties = categories.ElementAt(1).Properties;
            Assert.That(categories.Count, Is.EqualTo(2));
            Assert.That(properties.Last().Name, Is.EqualTo(DefinitionRegion.IsShared.Key));
            Assert.That(properties.Last().Value, Is.EqualTo("1"));
        }

        [Test]
        public void GivenWaterFlow1DModelWithCrossSectionType_WhenConvertingToCrossSectionDefinitionCategory_ThenIdValueIsOfTypeAndGroundLayerIsUsed()
        {
            //Given
            var flowModel = GetMockedWaterFlowModelWithStandardCrossSectionType();

            //When
            var categories = converter.Convert(flowModel);

            //Then
            var properties = categories.ElementAt(1).Properties;
            var idValue = properties.ElementAt(0).Value;
            var groundLayerUsedName = properties.ElementAt(6).Name;
            var groundLayerUsedValue = properties.ElementAt(6).Value;
            Assert.That(flowModel.Network.CrossSections.ElementAt(0).CrossSectionType == CrossSectionType.Standard);
            Assert.That(idValue, Is.EqualTo("standardCrossSection"));
            Assert.That(groundLayerUsedName, Is.EqualTo("groundlayerUsed"));
            Assert.That(groundLayerUsedValue, Is.EqualTo("0"));
        }

        private static WaterFlowModel1D GetMockedWaterFlowModelWithStandardCrossSectionType()
        {
            var network = GetNetworkWithCulvertAndBridge();
            var model = MockRepository.GeneratePartialMock<WaterFlowModel1D>();

            model.Expect(mm => mm.Network).Return(network).Repeat.Any();

            return model;
        }

        private static IHydroNetwork GetNetworkWithCulvertAndBridge()
        {
            var standardCrossSection = GetCrossSectionWithStandardDefinition();
            var network = MockRepository.GenerateMock<IHydroNetwork>();
            network.Expect(mn => mn.CrossSections).Return(new List<ICrossSection>() {standardCrossSection}).Repeat.Any();
            network.Expect(mn => mn.SharedCrossSectionDefinitions)
                .Return(new EventedList<ICrossSectionDefinition>() { new CrossSectionDefinitionYZ() }).Repeat.Any();

            var culvert = MockRepository.GenerateMock<ICulvert>();
            culvert.Expect(c => c.CrossSectionDefinition).Return(new CrossSectionDefinitionYZ(standardCrossSection.Name)).Repeat
                .Any();
            network.Expect(mn => mn.Culverts).Return(new List<ICulvert>() {culvert}).Repeat.Any();

            var bridge = MockRepository.GenerateMock<IBridge>();
            bridge.Expect(b => b.CrossSectionDefinition).Return(new CrossSectionDefinitionYZ(standardCrossSection.Name)).Repeat
                .Any();
            network.Expect(mn => mn.Bridges).Return(new List<IBridge>() {bridge}).Repeat.Any();

            network.Expect(mn => mn.Structures).Return(new List<IStructure1D>() {culvert, bridge}).Repeat.Any();

            return network;
        }

        private static ICrossSection GetCrossSectionWithStandardDefinition()
        {
            var crossSectionDefinitionStandard = new CrossSectionDefinitionStandard() {Name = "standardCrossSection"};
            var standardCrossSection = MockRepository.GenerateMock<ICrossSection>();
            standardCrossSection.Expect(scs => scs.CrossSectionType).Return(CrossSectionType.Standard).Repeat.Any();
            standardCrossSection.Expect(scs => scs.Definition).Return(crossSectionDefinitionStandard).Repeat.Any();
            standardCrossSection.Expect(scs => scs.Name).Return(crossSectionDefinitionStandard.Name).Repeat.Any();

            return standardCrossSection;
        }

        private WaterFlowModel1D GetMockedWaterFlowModelWithSharedCrossSectionDefinition()
        {
            var network = GetMockedHydroNetwork();
            var roughnessSection = MockRepository.GeneratePartialMock<RoughnessSection>(crossSectionSectionType, network);

            var flowModel = MockRepository.GeneratePartialMock<WaterFlowModel1D>();
            flowModel.Expect(mm => mm.RoughnessSections).Return(new EventedList<RoughnessSection> {roughnessSection}).Repeat.Any();
            flowModel.Expect(mm => mm.Network).Return(network).Repeat.Any();

            return flowModel;
        }

        private HydroNetwork GetMockedHydroNetwork()
        {
            var crossSectionDefinitionYz = GetMockedCrossSectionDefinitionYz();
            var crossSectionDefinitionProxy = GetMockedCrossSectionDefinitionProxy(crossSectionDefinitionYz);

            var crossSection1 = GetCrossSectionOnBranchWithDefinition(crossSectionDefinitionProxy);
            var crossSection2 = GetMockedCrossSectionWithDefinition(crossSectionDefinitionProxy);
            var crossSections = new []{crossSection1, crossSection2};

            var node1 = MockRepository.GenerateMock<INode>();
            var node2 = MockRepository.GenerateMock<INode>();

            var network = MockRepository.GeneratePartialMock<HydroNetwork>();
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            network.Expect(n => n.CrossSections).Return(crossSections).Repeat.Any();
            network.Expect(n => n.SharedCrossSectionDefinitions).Return(new EventedList<ICrossSectionDefinition> { crossSectionDefinitionYz }).Repeat.Any();

            return network;
        }

        private static CrossSectionDefinitionYZ GetMockedCrossSectionDefinitionYz()
        {
            var crossSectionDefinitionYz = MockRepository.GeneratePartialMock<CrossSectionDefinitionYZ>();
            crossSectionDefinitionYz.Expect(csd => csd.Name).Return(CrossSectionName).Repeat.Any();

            return crossSectionDefinitionYz;
        }

        private CrossSectionDefinitionProxy GetMockedCrossSectionDefinitionProxy(ICrossSectionDefinition innerDefinition)
        {
            var crossSectionSection = GetMockedCrossSectionSection();
            var crossSectionDefinitionProxy = MockRepository.GenerateMock<CrossSectionDefinitionProxy>(innerDefinition);
            
            crossSectionDefinitionProxy.Expect(csd => csd.Sections).Return(new EventedList<CrossSectionSection>
            {
                crossSectionSection
            });
            crossSectionDefinitionProxy.Expect(csd => csd.Name).Return(CrossSectionName).Repeat.Any();
            crossSectionDefinitionProxy.Expect(csd => csd.IsProxy).Return(true).Repeat.Any();
            crossSectionDefinitionProxy.Expect(csd => csd.InnerDefinition).Return(innerDefinition).Repeat.Any();

            return crossSectionDefinitionProxy;
        }

        private static ICrossSection GetCrossSectionOnBranchWithDefinition(ICrossSectionDefinition crossSectionDefinition)
        {
            var crossSection = MockRepository.GenerateMock<ICrossSection>();
            crossSection.Expect(cs => cs.Branch).Return(new Branch()).Repeat.Any();
            crossSection.Expect(cs => cs.Definition).Return(crossSectionDefinition).Repeat.Any();

            return crossSection;
        }

        private static ICrossSection GetMockedCrossSectionWithDefinition(ICrossSectionDefinition crossSectionDefinition)
        {
            var crossSection = MockRepository.GenerateMock<ICrossSection>();
            crossSection.Expect(cs => cs.Definition).Return(crossSectionDefinition).Repeat.Any();

            return crossSection;
        }

        private CrossSectionSection GetMockedCrossSectionSection()
        {
            var crossSectionSection = MockRepository.GeneratePartialMock<CrossSectionSection>();
            crossSectionSection.Expect(css => css.SectionType).Return(crossSectionSectionType);

            return crossSectionSection;
        }
    }
}
