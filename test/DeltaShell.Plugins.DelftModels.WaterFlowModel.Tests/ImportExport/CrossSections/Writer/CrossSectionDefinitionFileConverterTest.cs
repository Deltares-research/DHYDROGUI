using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Collections.Generic;
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
        private WaterFlowModel1D mockedModel;
        private HydroNetwork mockedNetwork;
        private CrossSectionDefinitionProxy mockedCrossSectionDefinitionProxy;
        private CrossSectionDefinitionYZ mockedCrossSectionDefinitionYz;
        private CrossSectionSectionType mockedCrossSectionSectionType;
        private CrossSectionSection mockedCrossSectionSection;
        private RoughnessSection mockedRoughnessSection;
        private ICrossSection mockedCrossSection1;
        private ICrossSection mockedCrossSection2;
        private INode mockedNode1;
        private INode mockedNode2;

        [SetUp]
        public void Setup()
        {
            converter = new CrossSectionDefinitionFileConverter();
            mockedCrossSection1 = MockRepository.GenerateMock<ICrossSection>();
            mockedCrossSection2 = MockRepository.GenerateMock<ICrossSection>();
            mockedNode1 = MockRepository.GenerateMock<INode>();
            mockedNode2 = MockRepository.GenerateMock<INode>();
            mockedModel = MockRepository.GeneratePartialMock<WaterFlowModel1D>();
            mockedNetwork = MockRepository.GeneratePartialMock<HydroNetwork>();
            mockedCrossSectionDefinitionYz = MockRepository.GeneratePartialMock<CrossSectionDefinitionYZ>();
            mockedCrossSectionDefinitionProxy = MockRepository.GenerateMock<CrossSectionDefinitionProxy>(mockedCrossSectionDefinitionYz);
            mockedCrossSectionSection = MockRepository.GeneratePartialMock<CrossSectionSection>();
            mockedCrossSectionSectionType = MockRepository.GeneratePartialMock<CrossSectionSectionType>();
        }

        [Test]
        public void GivenWaterFlow1DModelWithSharedCrossSectionDefinitionAndChangedBackToLocalDefinition_WhenConvertingToCrossSectionDefinitionCategory_ThenIsSharedIsWrittenToProperty()
        {
            //Given
            mockedCrossSectionSection.Expect(mcss => mcss.SectionType).Return(mockedCrossSectionSectionType);
            mockedCrossSectionSection.Expect(mcss => mcss.MinY).Return(-1.0);
            mockedCrossSectionSection.Expect(mcss => mcss.MaxY).Return(2.0);
            mockedCrossSectionSection.Expect(mcss => mcss.SectionType).Return(mockedCrossSectionSectionType);

            mockedCrossSectionDefinitionProxy.Expect(mcsdp => mcsdp.Sections).Return(new EventedList<CrossSectionSection>()
                {
                    mockedCrossSectionSection
                });

            mockedCrossSectionDefinitionYz.Expect(mcsdyz => mcsdyz.Name).Return("crossSection1").Repeat.Any();
            mockedCrossSectionDefinitionProxy.Expect(mcsdp => mcsdp.Name).Return("crossSection1").Repeat.Any();
            mockedCrossSectionDefinitionProxy.Expect(mcsdp => mcsdp.IsProxy).Return(true).Repeat.Any();
            mockedCrossSectionDefinitionProxy.Expect(mcsdp => mcsdp.InnerDefinition).Return(mockedCrossSectionDefinitionYz).Repeat.Any();
            
            mockedCrossSection1.Expect(cs => cs.Branch).Return(new Branch()).Repeat.Any();
            mockedCrossSection1.Expect(cs => cs.Chainage).Return(2.0).Repeat.Any();
            mockedCrossSection1.Expect(cs => cs.Name).Return("crossSection1").Repeat.Any();
            mockedCrossSection1.Expect(mcs1 => mcs1.Definition).Return(mockedCrossSectionDefinitionProxy).Repeat.Any();
            mockedCrossSection1.Expect(mcsd => mcsd.ShareDefinitionAndChangeToProxy()).Repeat.Any();
            mockedCrossSection1.Expect(mcsd => mcsd.UseSharedDefinition(mockedCrossSectionDefinitionProxy)).Repeat.Any();
           
            mockedCrossSection2.Expect(mcs2 => mcs2.UseSharedDefinition(mockedCrossSectionDefinitionProxy)).Repeat.Any();
            mockedCrossSection2.Expect(mcs2 => mcs2.Definition).Return(mockedCrossSectionDefinitionProxy).Repeat.Any();

            //set shared cross section back to local
            mockedCrossSection1.Expect(mcs1 => mcs1.MakeDefinitionLocal()).Repeat.Any();

            mockedNetwork.Nodes.Add(mockedNode1);
            mockedNetwork.Nodes.Add(mockedNode2);
            mockedNetwork.Expect(mcn => mcn.CrossSections)
                .Return(new List<ICrossSection>() {mockedCrossSection1, mockedCrossSection2}).Repeat.Any();
            mockedNetwork.Expect(mn => mn.SharedCrossSectionDefinitions)
                .Return(new EventedList<ICrossSectionDefinition>() { mockedCrossSectionDefinitionProxy}).Repeat.Any();

            mockedCrossSectionSectionType.Expect(mcsst => mcsst.Name).Return("Main").Repeat.Any();

            mockedRoughnessSection = MockRepository.GeneratePartialMock<RoughnessSection>(mockedCrossSectionSectionType, mockedNetwork);

            mockedModel.Expect(mm => mm.RoughnessSections)
                .Return(new EventedList<RoughnessSection>() {mockedRoughnessSection}).Repeat.Any();
            mockedModel.Expect(mm => mm.Network).Return(mockedNetwork).Repeat.Any();

            //When
            var categories = converter.Convert(mockedModel).ToList();

            //Then
            var properties = categories.ElementAt(1).Properties;
            Assert.That(categories.Count, Is.EqualTo(2));
            Assert.That(properties.Last().Name, Is.EqualTo("isShared"));
            Assert.That(properties.Last().Value, Is.EqualTo("1"));
        }
    }
}
