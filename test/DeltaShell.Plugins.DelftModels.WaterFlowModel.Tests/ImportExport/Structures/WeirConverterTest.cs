using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Structures
{
    [TestFixture]
    public class WeirConverterTest
    {
        private IHydroNetwork originalNetwork;
        private IList<IChannel> channelsList;

        [SetUp]
        public void SetUp()
        {
            originalNetwork = FileWriterTestHelper.SetupSimpleHydroNetworkWith2NodesAnd1Branch();
            channelsList = originalNetwork.Channels.ToList();

        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void GivenAStructureBranchCategoryOfASimpleWeir_WhenConvertingToASimpleWeir_ThenAWeirOfThisTypeShouldBeCreated()
        {
            //Given
            var category = new DelftIniCategory(StructureRegion.Header);

            category.AddProperty(StructureRegion.Id.Key, "Weir1");
            category.AddProperty(StructureRegion.BranchId.Key, "branch");
            category.AddProperty(StructureRegion.Chainage.Key, "50");
            category.AddProperty(StructureRegion.Name.Key, "Weir1");
            category.AddProperty(StructureRegion.DefinitionType.Key, StructureRegion.StructureTypeName.Weir);

            category.AddProperty(StructureRegion.CrestLevel.Key, "2.3");
            category.AddProperty(StructureRegion.CrestWidth.Key, " 100.0");
            category.AddProperty(StructureRegion.DischargeCoeff.Key, "1.0");
            category.AddProperty(StructureRegion.LatDisCoeff.Key, "1.0");
            category.AddProperty(StructureRegion.AllowedFlowDir.Key, "0");

            //When
            var converter = new WeirConverter();
            var structure = (Weir)converter.ConvertToStructure1D(category, channelsList);
            var weirFormula = structure.WeirFormula as SimpleWeirFormula;

            //Then
            Assert.NotNull(weirFormula);
            Assert.AreEqual(2.3, structure.CrestLevel);
            Assert.AreEqual(100.0, structure.CrestWidth);
            Assert.AreEqual(1.0, weirFormula.DischargeCoefficient);
            Assert.AreEqual(1.0, weirFormula.LateralContraction);
            Assert.AreEqual(0, (int)structure.FlowDirection);
        }
    }
}