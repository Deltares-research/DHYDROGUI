using System;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Structures
{
    [TestFixture]
    public class StructureConverterFactoryTest
    {
        [Test]
        [TestCase(StructureRegion.StructureTypeName.Weir, typeof(WeirConverter))]
        [TestCase(StructureRegion.StructureTypeName.UniversalWeir, typeof(UniversalWeirConverter))]
        [TestCase(StructureRegion.StructureTypeName.AdvancedWeir, typeof(AdvancedWeirConverter))]
        [TestCase(StructureRegion.StructureTypeName.GeneralStructure, typeof(GeneralStructureConverter))]
        [TestCase(StructureRegion.StructureTypeName.Orifice, typeof(OrificeConverter))]
        [TestCase(StructureRegion.StructureTypeName.RiverWeir, typeof(RiverWeirConverter))]
        [TestCase(StructureRegion.StructureTypeName.ExtraResistanceStructure, typeof(ExtraResistanceConverter))]
        public void GivenAsType_WhenCreatingTheConverter_ThenTheCorrespondingConverterShouldBeCreated(string type, System.Type classConverter )
        {
            var converter = StructureConverterFactory.GetSpecificConverter(type);

            Assert.AreEqual(classConverter, converter.GetType());
        }

        // Not yet implemented, see issue SOBEK3-1569
        [Test]
        [TestCase(StructureRegion.StructureTypeName.Pump)]
        [TestCase(StructureRegion.StructureTypeName.Gate)]
        [TestCase(StructureRegion.StructureTypeName.Culvert)]
        [TestCase(StructureRegion.StructureTypeName.InvertedSiphon)]
        [TestCase(StructureRegion.StructureTypeName.Siphon)]
        [TestCase(StructureRegion.StructureTypeName.Bridge)]
        [TestCase(StructureRegion.StructureTypeName.BridgePillar)]
        [TestCase("SomeName")]

        public void GivenANotSupportedStructureAsType_WhenCreatingTheConverter_ThenAnExtraResistanceConverterShouldBeCreated(string type)
        {
            var converter = StructureConverterFactory.GetSpecificConverter(type);
            
            Assert.IsNull(converter);
        }


    }
}