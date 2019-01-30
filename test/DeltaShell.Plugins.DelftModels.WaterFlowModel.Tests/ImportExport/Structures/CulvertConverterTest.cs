using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Structures
{
    [TestFixture]
    public class CulvertConverterTest : StructureConverterTestHelper
    {
        private const string CulvertName = "myCulvert";
        private const string CulvertLongName = "myCulvert_longName";
        private const string ChainageAsString = "2.0";

        [SetUp]
        public void Setup()
        {
            Network = mocks.DynamicMock<INetwork>();
        }

        #region Culvert

        [Test]
        public void GivenCulvertStructureIniCategoryWithMatchingBranch_WhenConvertingToStructure1D_ThenCulvertIsReturnedWithCommonPropertyValues()
        {
            // Given
            var category = GetStructureCategoryWithBasicProperties();
            var branch = GetMockedBranch();

            // When
            var culvert = ConvertAndCheckForNull<CulvertConverter, Culvert>(category, branch);

            // Then
            Assert.That(culvert.CulvertType, Is.EqualTo(CulvertType.Culvert));
            Assert.That(culvert.Name, Is.EqualTo(CulvertName));
            Assert.That(culvert.LongName, Is.EqualTo(CulvertLongName));
            Assert.That(culvert.Chainage, Is.EqualTo(ParseToDouble(ChainageAsString)));
            Assert.That(culvert.Geometry, Is.EqualTo(new Point(2, 0)));
            Assert.That(culvert.Branch, Is.EqualTo(branch));
            Assert.That(culvert.Network, Is.EqualTo(Network));

            mocks.VerifyAll();
        }

        [TestCase("0", FlowDirection.Both)]
        [TestCase("1", FlowDirection.Positive)]
        [TestCase("2", FlowDirection.Negative)]
        [TestCase("3", FlowDirection.None)]
        public void GivenCulvertStructureIniCategoryWithSpecificFlowDirection_WhenConvertingToStructure1D_ThenCulvertIsReturnedWithSpecificFlowDirectionPropertyValue
            (string valueAsString, FlowDirection expectedFlowDirection)
        {
            // Given
            var category = GetStructureCategoryWithBasicProperties();
            category.SetProperty(StructureRegion.AllowedFlowDir.Key, valueAsString);

            var branch = GetMockedBranch();

            // When
            var culvert = ConvertAndCheckForNull<CulvertConverter, Culvert>(category, branch);

            // Then
            Assert.That(culvert.CulvertType, Is.EqualTo(CulvertType.Culvert));
            Assert.That(culvert.FlowDirection, Is.EqualTo(expectedFlowDirection));

            mocks.VerifyAll();
        }

        [TestCase("0", false)]
        [TestCase("1", true)]
        public void GivenCulvertStructureIniCategoryWithSpecificIsGatedValue_WhenConvertingToStructure1D_ThenCulvertIsReturnedWithSpecificIsGatedPropertyValue
            (string valueAsString, bool expectedIsGatedValue)
        {
            // Given
            var category = GetStructureCategoryWithBasicProperties();
            category.SetProperty(StructureRegion.ValveOnOff.Key, valueAsString);

            var branch = GetMockedBranch();

            // When
            var culvert = ConvertAndCheckForNull<CulvertConverter, Culvert>(category, branch);

            // Then
            Assert.That(culvert.CulvertType, Is.EqualTo(CulvertType.Culvert));
            Assert.That(culvert.IsGated, Is.EqualTo(expectedIsGatedValue));

            mocks.VerifyAll();
        }

        [Test]
        public void GivenCulvertStructureIniCategoryWithSpecificPropertyValues_WhenConvertingToStructure1D_ThenCulvertIsReturnedWithSpecificPropertyValues()
        {
            // Given
            var inletLevel = "2.0";
            var outletLevel = "3.0";
            var length = "4.0";
            var inletLossCoefficient = "5.0";
            var outletLossCoefficient = "6.0";
            var gateInitialOpening = "7.0";

            var category = GetStructureCategoryWithBasicProperties();
            category.SetProperty(StructureRegion.LeftLevel.Key, inletLevel);
            category.SetProperty(StructureRegion.RightLevel.Key, outletLevel);
            category.SetProperty(StructureRegion.Length.Key, length);
            category.SetProperty(StructureRegion.InletLossCoeff.Key, inletLossCoefficient);
            category.SetProperty(StructureRegion.OutletLossCoeff.Key, outletLossCoefficient);
            category.SetProperty(StructureRegion.IniValveOpen.Key, gateInitialOpening);

            var branch = GetMockedBranch();

            // When
            var culvert = ConvertAndCheckForNull<CulvertConverter, Culvert>(category, branch);

            // Then
            Assert.That(culvert.CulvertType, Is.EqualTo(CulvertType.Culvert));
            Assert.That(culvert.InletLevel, Is.EqualTo(ParseToDouble(inletLevel)));
            Assert.That(culvert.OutletLevel, Is.EqualTo(ParseToDouble(outletLevel)));
            Assert.That(culvert.Length, Is.EqualTo(ParseToDouble(length)));
            Assert.That(culvert.InletLossCoefficient, Is.EqualTo(ParseToDouble(inletLossCoefficient)));
            Assert.That(culvert.OutletLossCoefficient, Is.EqualTo(ParseToDouble(outletLossCoefficient)));
            Assert.That(culvert.GateInitialOpening, Is.EqualTo(ParseToDouble(gateInitialOpening)));

            mocks.VerifyAll();
        }

        [Test]
        public void GivenCulvertStructureIniCategoryWithLossCoefficientFunctionEntries_WhenConvertingToStructure1D_ThenCulvertWithSpecificReductionTableValuesIsReturned()
        {
            // Given
            var category = GetStructureCategoryWithBasicProperties();
            category.AddProperty(StructureRegion.LossCoeffCount.Key, "2");
            category.AddProperty(StructureRegion.RelativeOpening.Key, "1.000 2.000");
            category.AddProperty(StructureRegion.LossCoefficient.Key, "3.000 4.000");

            var branch = GetMockedBranch();

            // When
            var culvert = ConvertAndCheckForNull<CulvertConverter, Culvert>(category, branch);

            // Then
            Assert.That(culvert.CulvertType, Is.EqualTo(CulvertType.Culvert));

            var lossCoefficientFunction = culvert.GateOpeningLossCoefficientFunction;
            var argumentValues = lossCoefficientFunction.Arguments[0].Values;
            Assert.That(argumentValues.Count, Is.EqualTo(2));
            Assert.That(argumentValues[0], Is.EqualTo(1.0));
            Assert.That(argumentValues[1], Is.EqualTo(2.0));

            var componentValues = lossCoefficientFunction.Components[0].Values;
            Assert.That(componentValues.Count, Is.EqualTo(2));
            Assert.That(componentValues[0], Is.EqualTo(3.0));
            Assert.That(componentValues[1], Is.EqualTo(4.0));

            mocks.VerifyAll();
        }
        
        [Test]
        public void GivenCulvertStructureIniCategoryWithoutLossCoefficientFunctionEntries_WhenConvertingToStructure1D_ThenCulvertWithoutLossCoefficientFunctionEntriesIsReturned()
        {
            // Given
            var category = GetStructureCategoryWithBasicProperties();

            var branch = GetMockedBranch();

            // When
            var culvert = ConvertAndCheckForNull<CulvertConverter, Culvert>(category, branch);

            // Then
            Assert.That(culvert.CulvertType, Is.EqualTo(CulvertType.Culvert));

            var lossCoefficientFunction = culvert.GateOpeningLossCoefficientFunction;
            var argumentValues = lossCoefficientFunction.Arguments[0].Values;
            Assert.That(argumentValues.Count, Is.EqualTo(0));

            var componentValues = lossCoefficientFunction.Components[0].Values;
            Assert.That(componentValues.Count, Is.EqualTo(0));

            mocks.VerifyAll();
        }

        [Test]
        public void GivenCulvertStructureIniCategoryWithIncorrectLossCoefficientFunctionEntries_WhenConvertingToStructure1D_ThenCulvertWithoutLossCoefficientFunctionEntriesIsReturned()
        {
            // Given
            var category = GetStructureCategoryWithBasicProperties();
            category.AddProperty(StructureRegion.LossCoeffCount.Key, "2");
            category.AddProperty(StructureRegion.RelativeOpening.Key, "1.000"); // Too less values for RelativeOpening
            category.AddProperty(StructureRegion.LossCoefficient.Key, "3.000 4.000");

            var branch = GetMockedBranch();

            // When
            var culvert = ConvertAndCheckForNull<CulvertConverter, Culvert>(category, branch);

            // Then
            Assert.That(culvert.CulvertType, Is.EqualTo(CulvertType.Culvert));

            var lossCoefficientFunction = culvert.GateOpeningLossCoefficientFunction;
            var argumentValues = lossCoefficientFunction.Arguments[0].Values;
            Assert.That(argumentValues.Count, Is.EqualTo(0));

            var componentValues = lossCoefficientFunction.Components[0].Values;
            Assert.That(componentValues.Count, Is.EqualTo(0));

            mocks.VerifyAll();
        }

        #endregion

        #region Inverted Siphon

        [Test]
        public void GivenInvertedSiphonStructureIniCategoryWithBendLossCoefficient_WhenConvertingToStructure1D_ThenCulvertWithSpecificBendLossCoefficientIsReturned()
        {
            // Given
            var bendLossCoefficient = "0.25";
            var category = GetStructureCategoryWithBasicProperties();
            category.AddProperty(StructureRegion.BendLossCoef.Key, bendLossCoefficient);

            var branch = GetMockedBranch();

            // When
            var invertedSiphon = ConvertAndCheckForNull<InvertedSiphonConverter, Culvert>(category, branch);

            // Then
            Assert.That(invertedSiphon.CulvertType, Is.EqualTo(CulvertType.InvertedSiphon));
            Assert.That(invertedSiphon.BendLossCoefficient, Is.EqualTo(ParseToDouble(bendLossCoefficient)));

            mocks.VerifyAll();
        }

        #endregion

        #region Siphon

        [Test]
        public void GivenSiphonStructureIniCategoryWithSiphonLevels_WhenConvertingToStructure1D_ThenCulvertWithSpecificSiphonLevelsIsReturned()
        {
            // Given
            var bendLossCoefficient = "0.25";
            var siphonOnLevel = "1.0";
            var siphonOffLevel = "2.0";

            var category = GetStructureCategoryWithBasicProperties();
            category.AddProperty(StructureRegion.BendLossCoef.Key, bendLossCoefficient);
            category.AddProperty(StructureRegion.TurnOnLevel.Key, siphonOnLevel);
            category.AddProperty(StructureRegion.TurnOffLevel.Key, siphonOffLevel);
            category.SetProperty(StructureRegion.AllowedFlowDir.Key, "1"); // Set To FlowDirection.Positive, because negative flow is not allowed for siphons.

            var branch = GetMockedBranch();

            // When
            var invertedSiphon = ConvertAndCheckForNull<SiphonConverter, Culvert>(category, branch);

            // Then
            Assert.That(invertedSiphon.CulvertType, Is.EqualTo(CulvertType.Siphon));
            Assert.That(invertedSiphon.BendLossCoefficient, Is.EqualTo(ParseToDouble(bendLossCoefficient)));
            Assert.That(invertedSiphon.SiphonOnLevel, Is.EqualTo(ParseToDouble(siphonOnLevel)));
            Assert.That(invertedSiphon.SiphonOffLevel, Is.EqualTo(ParseToDouble(siphonOffLevel)));

            mocks.VerifyAll();
        }

        #endregion

        private static IDelftIniCategory GetStructureCategoryWithBasicProperties()
        {
            var category = new DelftIniCategory(StructureRegion.Header);
            category.AddProperty(StructureRegion.Id.Key, CulvertName);
            category.AddProperty(StructureRegion.Name.Key, CulvertLongName);
            category.AddProperty(StructureRegion.Chainage.Key, ChainageAsString);
            category.AddProperty(StructureRegion.AllowedFlowDir.Key, "0");
            category.AddProperty(StructureRegion.LeftLevel.Key, "0.0");
            category.AddProperty(StructureRegion.RightLevel.Key, "0.0");
            category.AddProperty(StructureRegion.Length.Key, "1.0");
            category.AddProperty(StructureRegion.InletLossCoeff.Key, "1.0");
            category.AddProperty(StructureRegion.OutletLossCoeff.Key, "1.0");
            category.AddProperty(StructureRegion.ValveOnOff.Key, "1");
            category.AddProperty(StructureRegion.IniValveOpen.Key, "0.0");
            category.SetProperty(StructureRegion.ValveOnOff.Key, "0");

            return category;
        }
    }
}