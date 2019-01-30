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
        [TestCase(StructureRegion.StructureTypeName.Pump, typeof(PumpConverter))]
        [TestCase(StructureRegion.StructureTypeName.Culvert, typeof(CulvertConverter))]
        [TestCase(StructureRegion.StructureTypeName.InvertedSiphon, typeof(InvertedSiphonConverter))]
        [TestCase(StructureRegion.StructureTypeName.Siphon, typeof(SiphonConverter))]
        public void GivenAsType_WhenCreatingTheConverter_ThenTheCorrespondingConverterShouldBeCreated(string type, Type classConverter)
        {
            var converter = StructureConverterFactory.GetStructureConverter(type);

            Assert.That(converter.GetType(), Is.EqualTo(classConverter));
        }

        // Not yet implemented, see issue SOBEK3-1569
        [Test]
        [TestCase(StructureRegion.StructureTypeName.Gate)]
        [TestCase(StructureRegion.StructureTypeName.Bridge)]
        [TestCase(StructureRegion.StructureTypeName.BridgePillar)]
        [TestCase("SomeName")]

        public void GivenANotSupportedStructureAsType_WhenCreatingTheConverter_ThenAnExtraResistanceConverterShouldBeCreated(string type)
        {
            var converter = StructureConverterFactory.GetStructureConverter(type);
            
            Assert.IsNull(converter);
        }


    }
}