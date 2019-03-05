using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
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
        private CrossSectionSectionType crossSectionSectionType;
        private const string CrossSectionName = "crossSection1";

        [SetUp]
        public void Setup()
        {
            crossSectionSectionType = GetMockedCrossSectionSectionType();
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
            var flowModel = GetMockedWaterFlowModel();

            //When
            var converter = new CrossSectionDefinitionFileConverter();
            var categories = converter.Convert(flowModel).ToArray();

            //Then
            var properties = categories.ElementAt(1).Properties;
            Assert.That(categories.Count, Is.EqualTo(2));
            Assert.That(properties.Last().Name, Is.EqualTo(DefinitionRegion.IsShared.Key));
            Assert.That(properties.Last().Value, Is.EqualTo("1"));
        }

        private WaterFlowModel1D GetMockedWaterFlowModel()
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
