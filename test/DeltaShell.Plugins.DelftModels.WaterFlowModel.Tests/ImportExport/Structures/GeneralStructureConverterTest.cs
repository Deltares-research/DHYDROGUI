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
    public class GeneralStructureConverterTest
    {
        private IHydroNetwork originalNetwork;
        private IList<IChannel> channelsList;

        [SetUp]
        public void SetUp()
        {
            originalNetwork = FileWriterTestHelper.SetupSimpleHydroNetworkWith2NodesAnd1Branch();
            channelsList = originalNetwork.Channels.ToList();
        }

        [Test]
        public void
            GivenAStructureBranchCategoryOfAGeneralStructureWithExtraResistance_WhenConvertingToAGeneralStructure_ThenAWeirOfThisTypeShouldBeCreated()
        {
            //Given
            var category = CreatePerfectGeneralStructureCategory();

            //When
            var converter = new GeneralStructureConverter();
            var structure = (Weir)converter.ConvertToStructure1D(category, channelsList);
            var weirFormula = structure.WeirFormula as GeneralStructureWeirFormula;

            //Then
            Assert.NotNull(weirFormula);

            Assert.AreEqual(75, weirFormula.WidthLeftSideOfStructure);
            Assert.AreEqual(76, weirFormula.WidthStructureLeftSide);

            Assert.AreEqual(77, weirFormula.WidthStructureCentre);
            Assert.AreEqual(77, structure.CrestWidth);

            Assert.AreEqual(78, weirFormula.WidthStructureRightSide);
            Assert.AreEqual(79, weirFormula.WidthRightSideOfStructure);


            Assert.AreEqual(2, weirFormula.BedLevelLeftSideOfStructure);
            Assert.AreEqual(3, weirFormula.BedLevelLeftSideStructure);

            Assert.AreEqual(4, weirFormula.BedLevelStructureCentre);
            Assert.AreEqual(4, structure.CrestLevel);

            Assert.AreEqual(5, weirFormula.BedLevelRightSideStructure);
            Assert.AreEqual(6, weirFormula.BedLevelRightSideOfStructure);


            Assert.That(weirFormula.GateOpening - 3 < double.Epsilon);


            Assert.AreEqual(0.1, weirFormula.PositiveFreeGateFlow);
            Assert.AreEqual(0.2, weirFormula.PositiveDrownedGateFlow);
            Assert.AreEqual(0.3, weirFormula.PositiveFreeWeirFlow);
            Assert.AreEqual(0.4, weirFormula.PositiveDrownedWeirFlow);
            Assert.AreEqual(0.5, weirFormula.PositiveContractionCoefficient);


            Assert.AreEqual(0.6, weirFormula.NegativeFreeGateFlow);
            Assert.AreEqual(0.7, weirFormula.NegativeDrownedGateFlow);
            Assert.AreEqual(0.8, weirFormula.NegativeFreeWeirFlow);
            Assert.AreEqual(0.9, weirFormula.NegativeDrownedWeirFlow);
            Assert.AreEqual(0.95, weirFormula.NegativeContractionCoefficient);


            Assert.AreEqual(0.25, weirFormula.ExtraResistance);
            Assert.IsTrue(weirFormula.UseExtraResistance);
        }

        [Test]
        public void
            GivenAStructureBranchCategoryOfAGeneralStructureWithoutExtraResistance_WhenConvertingToAGeneralStructure_ThenAWeirOfThisTypeShouldBeCreated()
        {
            //Given
            var category = CreatePerfectGeneralStructureCategory();
            category.SetProperty(StructureRegion.ExtraResistance.Key, "0");

            //When
            var converter = new GeneralStructureConverter();
            var structure = (Weir)converter.ConvertToStructure1D(category, channelsList);
            var weirFormula = structure.WeirFormula as GeneralStructureWeirFormula;

            //Then
            Assert.NotNull(weirFormula);
            
            Assert.AreEqual(0.0, weirFormula.ExtraResistance);
            Assert.IsFalse(weirFormula.UseExtraResistance);
        }

        [Test]
        [TestCase("widthleftW1")]
        [TestCase("widthleftWsdl")]
        [TestCase("widthcenter")]
        [TestCase("widthrightWsdr")]
        [TestCase("widthrightW2")]
        [TestCase("levelleftZb1")]
        [TestCase("levelleftZbsl")]
        [TestCase("levelcenter")]
        [TestCase("levelrightZbsr")]
        [TestCase("levelrightZb2")]
        [TestCase("gateheight")]
        [TestCase("pos_freegateflowcoeff")]
        [TestCase("pos_drowngateflowcoeff")]
        [TestCase("pos_freeweirflowcoeff")]
        [TestCase("pos_drownweirflowcoeff")]
        [TestCase("pos_contrcoeffreegate")]
        [TestCase("neg_freegateflowcoeff")]
        [TestCase("neg_drowngateflowcoeff")]
        [TestCase("neg_freeweirflowcoeff")]
        [TestCase("neg_drownweirflowcoeff")]
        [TestCase("neg_contrcoeffreegate")]
        [TestCase("extraresistance")]
        public void
            GivenAStructureBranchCategoryOfAGeneralStructureWithAMissingMandatoryParameter_WhenConvertingToAGeneralStructure_ThenAnExceptionShouldBeThrown(string propertyName)
        {
            //Given
            var category = CreatePerfectGeneralStructureCategory();

            var removeProperty = category.Properties.FirstOrDefault(p => p.Name == propertyName);
            category.RemoveProperty(removeProperty);

            //When
            var converter = new GeneralStructureConverter();
            
            Assert.That(() => converter.ConvertToStructure1D(category, channelsList), Throws
                .TypeOf<PropertyNotFoundInFileException>().With.Message.EqualTo(string.Format(
                    "Property {0} is not found in the file", propertyName)));
        }
    

        private DelftIniCategory CreatePerfectGeneralStructureCategory()
        {
            var category = new DelftIniCategory(StructureRegion.Header);

            category.AddProperty(StructureRegion.Id.Key, "GeneralStructure1");
            category.AddProperty(StructureRegion.BranchId.Key, "branch");
            category.AddProperty(StructureRegion.Chainage.Key, "50");
            category.AddProperty(StructureRegion.Name.Key, "GeneralStructure1");
            category.AddProperty(StructureRegion.DefinitionType.Key, StructureRegion.StructureTypeName.GeneralStructure);

            category.AddProperty(StructureRegion.WidthLeftW1.Key, "75");
            category.AddProperty(StructureRegion.WidthLeftWsdl.Key, "76");
            category.AddProperty(StructureRegion.WidthCenter.Key, "77");
            category.AddProperty(StructureRegion.WidthRightWsdr.Key, "78");
            category.AddProperty(StructureRegion.WidthRightW2.Key, "79");

            category.AddProperty(StructureRegion.LevelLeftZb1.Key, "2");
            category.AddProperty(StructureRegion.LevelLeftZbsl.Key, "3");
            category.AddProperty(StructureRegion.LevelCenter.Key, "4");
            category.AddProperty(StructureRegion.LevelRightZbsr.Key, "5");
            category.AddProperty(StructureRegion.LevelRightZb2.Key, "6");

            category.AddProperty(StructureRegion.GateHeight.Key, "7");

            category.AddProperty(StructureRegion.PosFreeGateFlowCoeff.Key, "0.1");
            category.AddProperty(StructureRegion.PosDrownGateFlowCoeff.Key, "0.2");
            category.AddProperty(StructureRegion.PosFreeWeirFlowCoeff.Key, "0.3");
            category.AddProperty(StructureRegion.PosDrownWeirFlowCoeff.Key, "0.4");
            category.AddProperty(StructureRegion.PosContrCoefFreeGate.Key, "0.5");

            category.AddProperty(StructureRegion.NegFreeGateFlowCoeff.Key, "0.6");
            category.AddProperty(StructureRegion.NegDrownGateFlowCoeff.Key, "0.7");
            category.AddProperty(StructureRegion.NegFreeWeirFlowCoeff.Key, "0.8");
            category.AddProperty(StructureRegion.NegDrownWeirFlowCoeff.Key, "0.9");
            category.AddProperty(StructureRegion.NegContrCoefFreeGate.Key, "0.95");

            category.AddProperty(StructureRegion.ExtraResistance.Key, "0.25");
            
            return category;
        }
    }
}