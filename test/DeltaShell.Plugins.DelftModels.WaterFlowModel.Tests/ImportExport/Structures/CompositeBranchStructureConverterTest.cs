using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Structures
{
    [TestFixture]
    public class CompositeBranchStructureConverterTest : StructureConverterTestHelper
    {
        private IHydroNetwork originalNetwork;
        private IList<IChannel> channels;

        [SetUp]
        public void SetUp()
        {
            originalNetwork = FileWriterTestHelper.SetupSimpleHydroNetworkWith2NodesAnd1Branch("node1", "node2", "branch");
            channels = originalNetwork.Channels.ToList();
        }

        [Test]
        public void WhenACompositeBranchStructureConverterIsConstructedWithNoArguments_ThenNoExceptionIsThrown()
        {
            Assert.That(new CompositeBranchStructureConverter(), Is.Not.Null);
        }

        [Test]
        public void GivenSomeFactoryAndSomeCompositeBranchStructureConverter_WhenACompositeBranchStructureConverterIsConstructed_ThenNoExceptionIsThrown()
        {
            Func<string, StructureConverter> someFactory = a => null;
            Func<DelftIniCategory, IStructure1D, IList<ICompositeBranchStructure>, ICompositeBranchStructure> someCompositeBranchStructureConverter = (a, b,c) => null;
           
            Assert.That(new CompositeBranchStructureConverter(someFactory, someCompositeBranchStructureConverter), Is.Not.Null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "getCompositeBranchStructureFunc cannot be null.")]
        public void GivenSomeFactoryAndANullCompositeBranchStructureConverter_WhenACompositeBranchStructureConverterIsConstructed_ThenAnArgumentExceptionIsThrown()
        {
            Func<string, StructureConverter> someFactory = a => null;

            var converter = new CompositeBranchStructureConverter(someFactory, null);
            Assert.IsNotNull(converter);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "getTypeConverterFunc cannot be null.")]
        public void GivenANullFactoryAndSomeCompositeBranchStructureConverter_WhenACompositeBranchStructureConverterIsConstructed_ThenAnArgumentExceptionIsThrown()
        {
            Func<DelftIniCategory, IStructure1D, IList<ICompositeBranchStructure>, ICompositeBranchStructure> someCompositeBranchStructureConverter = (a, b, c) => null;

            var converter = new CompositeBranchStructureConverter(null,someCompositeBranchStructureConverter);
            Assert.IsNotNull(converter);
        }

        [Test]
        public void GivenTwoCategoriesOnTheSameCompositeStructure_WhenImporting_ThenACompositeStructureShouldBeCreatedWithTheTwoStructures()
        {
            //Given
            var errorMessages = new List<string>();
            var categories = new List<DelftIniCategory>();

            var category = CreatePerfectCategory();


            categories.Add(category);

            var category2 = CreatePerfectCategory();
            category2.SetProperty(StructureRegion.Id.Key, "Weir2");


            categories.Add(category2);

            //When
            var compositeBranchStructures = new CompositeBranchStructureConverter().Convert(categories, channels, new List<ICrossSectionDefinition>(), new GroundLayerDataTransferObject[]{}, errorMessages);

            //Then
            Assert.AreEqual(1, compositeBranchStructures.Count);
            Assert.AreEqual(2, compositeBranchStructures[0].Structures.Count);
            Assert.AreEqual("Weir1", compositeBranchStructures[0].Structures[0].Name);
            Assert.AreEqual("Weir2", compositeBranchStructures[0].Structures[1].Name);
        }

        #region Reading Culvert Cross Section Definition

        [Test]
        public void GivenCulvertDelftIniCategoryWithMatchingCrossSectionDefinitionWithRoundShape_WhenConvertingToCompositeBranchStructure_ThenCulvertCrossSectionShapePropertiesAreAsExpected()
        {
            // Given
            const double crossSectionDefinitionDiameter = 5.0;
            var roundShape = new CrossSectionStandardShapeRound
            {
                Diameter = crossSectionDefinitionDiameter
            };

            // When
            var compositeBranchStructures = ConvertCulvertDelftIniCategoryToCompositeBranchStructures(roundShape, new GroundLayerDataTransferObject[0]);

            // Then
            var shape = GetCulvertShapeFromCompositeStructure<CrossSectionStandardShapeRound>(compositeBranchStructures);
            Assert.That(shape.Diameter, Is.EqualTo(crossSectionDefinitionDiameter));
        }

        [Test]
        public void GivenCulvertDelftIniCategoryWithMatchingCrossSectionDefinitionWithRectangleShape_WhenConvertingToCompositeBranchStructure_ThenCulvertCrossSectionShapePropertiesAreAsExpected()
        {
            // Given
            var rectangleShape = new CrossSectionStandardShapeRectangle
            {
                Width = CrossSectionDefinitionWidth,
                Height = CrossSectionDefinitionHeight
            };

            // When
            var compositeBranchStructures = ConvertCulvertDelftIniCategoryToCompositeBranchStructures(rectangleShape, new GroundLayerDataTransferObject[0]);

            // Then
            var shape = GetCulvertShapeFromCompositeStructure<CrossSectionStandardShapeRectangle>(compositeBranchStructures);
            Assert.That(shape.Width, Is.EqualTo(CrossSectionDefinitionWidth));
            Assert.That(shape.Height, Is.EqualTo(CrossSectionDefinitionHeight));
        }

        [Test]
        public void GivenCulvertDelftIniCategoryWithMatchingCrossSectionDefinitionWithEllipseShape_WhenConvertingToCompositeBranchStructure_ThenCulvertCrossSectionShapePropertiesAreAsExpected()
        {
            // Given
            var ellipticalShape = new CrossSectionStandardShapeElliptical
            {
                Width = CrossSectionDefinitionWidth,
                Height = CrossSectionDefinitionHeight
            };

            // When
            var compositeBranchStructures = ConvertCulvertDelftIniCategoryToCompositeBranchStructures(ellipticalShape, new GroundLayerDataTransferObject[0]);

            // Then
            var shape = GetCulvertShapeFromCompositeStructure<CrossSectionStandardShapeElliptical>(compositeBranchStructures);
            Assert.That(shape.Width, Is.EqualTo(CrossSectionDefinitionWidth));
            Assert.That(shape.Height, Is.EqualTo(CrossSectionDefinitionHeight));
        }

        [Test]
        public void GivenCulvertDelftIniCategoryWithMatchingCrossSectionDefinitionWithEggShape_WhenConvertingToCompositeBranchStructure_ThenCulvertCrossSectionShapePropertiesAreAsExpected()
        {
            // Given
            var eggShape = new CrossSectionStandardShapeEgg
            {
                Width = CrossSectionDefinitionWidth
            };

            // When
            var compositeBranchStructures = ConvertCulvertDelftIniCategoryToCompositeBranchStructures(eggShape, new GroundLayerDataTransferObject[0]);

            // Then
            var shape = GetCulvertShapeFromCompositeStructure<CrossSectionStandardShapeEgg>(compositeBranchStructures);
            Assert.That(shape.Width, Is.EqualTo(CrossSectionDefinitionWidth));
        }

        [Test]
        public void GivenCulvertDelftIniCategoryWithMatchingCrossSectionDefinitionWithArchShape_WhenConvertingToCompositeBranchStructure_ThenCulvertCrossSectionShapePropertiesAreAsExpected()
        {
            // Given
            var CrossSectionDefinitionArcHeight = 1.0;
            var archShape = new CrossSectionStandardShapeArch
            {
                Width = CrossSectionDefinitionWidth,
                Height = CrossSectionDefinitionHeight,
                ArcHeight = CrossSectionDefinitionArcHeight
            };

            // When
            var compositeBranchStructures = ConvertCulvertDelftIniCategoryToCompositeBranchStructures(archShape, new GroundLayerDataTransferObject[0]);

            // Then
            var shape = GetCulvertShapeFromCompositeStructure<CrossSectionStandardShapeArch>(compositeBranchStructures);
            Assert.That(shape.Width, Is.EqualTo(CrossSectionDefinitionWidth));
            Assert.That(shape.Height, Is.EqualTo(CrossSectionDefinitionHeight));
            Assert.That(shape.ArcHeight, Is.EqualTo(CrossSectionDefinitionArcHeight));
        }

        [Test]
        public void GivenCulvertDelftIniCategoryWithMatchingCrossSectionDefinitionWithCunetteShape_WhenConvertingToCompositeBranchStructure_ThenCulvertCrossSectionShapePropertiesAreAsExpected()
        {
            // Given
            var cunetteShape = new CrossSectionStandardShapeCunette
            {
                Width = CrossSectionDefinitionWidth
            };

            // When
            var compositeBranchStructures = ConvertCulvertDelftIniCategoryToCompositeBranchStructures(cunetteShape, new GroundLayerDataTransferObject[0]);

            // Then
            var shape = GetCulvertShapeFromCompositeStructure<CrossSectionStandardShapeCunette>(compositeBranchStructures);
            Assert.That(shape.Width, Is.EqualTo(CrossSectionDefinitionWidth));
        }

        [Test]
        public void GivenCulvertDelftIniCategoryWithMatchingCrossSectionDefinitionWithSteelCunetteShape_WhenConvertingToCompositeBranchStructure_ThenCulvertCrossSectionShapePropertiesAreAsExpected()
        {
            // Given
            const double crossSectionDefinitionRadius = 2.2;
            const double crossSectionDefinitionRadius1 = 3.3;
            const double crossSectionDefinitionRadius2 = 0.6;
            const double crossSectionDefinitionRadius3 = 0.9;
            const double crossSectionDefinitionAngle = 1.0;
            const double crossSectionDefinitionAngle1 = 3.3;
            var steelCunetteShape = new CrossSectionStandardShapeSteelCunette
            {
                Height = CrossSectionDefinitionHeight,
                RadiusR = crossSectionDefinitionRadius,
                RadiusR1 = crossSectionDefinitionRadius1,
                RadiusR2 = crossSectionDefinitionRadius2,
                RadiusR3 = crossSectionDefinitionRadius3,
                AngleA = crossSectionDefinitionAngle,
                AngleA1 = crossSectionDefinitionAngle1
            };

            // When
            var compositeBranchStructures = ConvertCulvertDelftIniCategoryToCompositeBranchStructures(steelCunetteShape, new GroundLayerDataTransferObject[0]);

            // Then
            var shape = GetCulvertShapeFromCompositeStructure<CrossSectionStandardShapeSteelCunette>(compositeBranchStructures);
            Assert.That(shape.Height, Is.EqualTo(CrossSectionDefinitionHeight));
            Assert.That(shape.RadiusR, Is.EqualTo(crossSectionDefinitionRadius));
            Assert.That(shape.RadiusR1, Is.EqualTo(crossSectionDefinitionRadius1));
            Assert.That(shape.RadiusR2, Is.EqualTo(crossSectionDefinitionRadius2));
            Assert.That(shape.RadiusR3, Is.EqualTo(crossSectionDefinitionRadius3));
            Assert.That(shape.AngleA, Is.EqualTo(crossSectionDefinitionAngle));
            Assert.That(shape.AngleA1, Is.EqualTo(crossSectionDefinitionAngle1));
        }

        [Test]
        public void GivenCulvertDelftIniCategoryWithMatchingGroundLayerData_WhenConvertingToCompositeBranchStructure_ThenCulvertCrossSectionGroundLayerPropertiesAreAsExpected()
        {
            // Given
            var rectangleShape = new CrossSectionStandardShapeRectangle
            {
                Width = CrossSectionDefinitionWidth,
                Height = CrossSectionDefinitionHeight
            };

            var groundLayerThickness = 2.5;
            var layerData = new GroundLayerDataTransferObject
            {
                CrossSectionDefinitionId = CulvertName,
                GroundLayerUsed = true,
                GroundLayerThickness = groundLayerThickness
            };
            var groundLayerData = new[] { layerData };

            // When
            var compositeBranchStructures = ConvertCulvertDelftIniCategoryToCompositeBranchStructures(rectangleShape, groundLayerData);

            // Then
            Assert.That(compositeBranchStructures.Count, Is.EqualTo(1));

            var compositeBranchStructure = compositeBranchStructures.FirstOrDefault();
            var branchFeatures = compositeBranchStructure?.Branch.BranchFeatures;
            var culvert = branchFeatures?.FirstOrDefault() as Culvert;
            Assert.IsNotNull(culvert);

            Assert.IsTrue(culvert.GroundLayerEnabled);
            Assert.That(culvert.GroundLayerThickness, Is.EqualTo(groundLayerThickness));
        }

        [Test]
        public void GivenCulvertDelftIniCategoryWithMatchingGroundLayerDataWithNoGroundLayerUsed_WhenConvertingToCompositeBranchStructure_ThenCulvertCrossSectionGroundLayerPropertiesAreAsExpected()
        {
            // Given
            var rectangleShape = new CrossSectionStandardShapeRectangle
            {
                Width = CrossSectionDefinitionWidth,
                Height = CrossSectionDefinitionHeight
            };

            var groundLayerThickness = 2.5;
            var layerData = new GroundLayerDataTransferObject
            {
                CrossSectionDefinitionId = CulvertName,
                GroundLayerUsed = false, // GroundLayer is not used
                GroundLayerThickness = groundLayerThickness
            };
            var groundLayerData = new[] { layerData };

            // When
            var compositeBranchStructures = ConvertCulvertDelftIniCategoryToCompositeBranchStructures(rectangleShape, groundLayerData);

            // Then
            Assert.That(compositeBranchStructures.Count, Is.EqualTo(1));

            var compositeBranchStructure = compositeBranchStructures.FirstOrDefault();
            var branchFeatures = compositeBranchStructure?.Branch.BranchFeatures;
            var culvert = branchFeatures?.FirstOrDefault() as Culvert;
            Assert.IsNotNull(culvert);

            Assert.IsFalse(culvert.GroundLayerEnabled);
            Assert.That(culvert.GroundLayerRoughness, Is.EqualTo(0.0));
            Assert.That(culvert.GroundLayerThickness, Is.EqualTo(0.0));
        }

        private IList<ICompositeBranchStructure> ConvertCulvertDelftIniCategoryToCompositeBranchStructures(ICrossSectionStandardShape shape, GroundLayerDataTransferObject[] groundLayerDataTransferObjects)
        {
            var crossSectionDefinitions = new List<ICrossSectionDefinition> {GetCrossSectionDefinition(CulvertName, shape) };            
            Network = new HydroNetwork();
            var categories = new List<DelftIniCategory> {GetCulvertCategoryWithBasicProperties()};
            var branches = new List<IChannel> {GetMockedChannel()};

            var converter = new CompositeBranchStructureConverter();
            var compositeBranchStructures =
                converter.Convert(categories, branches, crossSectionDefinitions, groundLayerDataTransferObjects, new List<string>());
            return compositeBranchStructures;
        }

        private static CrossSectionDefinitionStandard GetCrossSectionDefinition(string name, ICrossSectionStandardShape shape)
        {
            var crossSectionDefinitionRound = new CrossSectionDefinitionStandard(shape)
            {
                Name = name
            };
            return crossSectionDefinitionRound;
        }

        private static T GetCulvertShapeFromCompositeStructure<T>(ICollection<ICompositeBranchStructure> compositeBranchStructures)
            where T : class, ICrossSectionStandardShape
        {
            Assert.That(compositeBranchStructures.Count, Is.EqualTo(1));

            var compositeBranchStructure = compositeBranchStructures.FirstOrDefault();
            var branchFeatures = compositeBranchStructure?.Branch.BranchFeatures;
            var culvert = branchFeatures?.FirstOrDefault() as Culvert;
            Assert.That(branchFeatures?.Count, Is.EqualTo(1));
            Assert.IsNotNull(culvert, "CompositeBranchStructureConverter did not return a Culvert object");

            var crossSectionDefinition = culvert.CrossSectionDefinition as CrossSectionDefinitionStandard;
            var shape = crossSectionDefinition?.Shape as T;
            Assert.IsNotNull(shape, $"CompositeBranchStructureConverter did not return a Culvert object with a {typeof(T)} shape type");

            return shape;
        }

        [Test]
        public void GivenCulvertDelftIniCategoryWithMatchingCrossSectionDefinitionZw_WhenConvertingToCompositeBranchStructure_ThenCulvertHasTabulatedShapeType()
        {
            // Given
            var crossSectionDefinitionZw = new CrossSectionDefinitionZW
            {
                Name = CulvertName
            };
            crossSectionDefinitionZw.ZWDataTable.AddCrossSectionZWRow(10.0, 5.0, 0.0);
            crossSectionDefinitionZw.ZWDataTable.AddCrossSectionZWRow(15.0, 3.0, 2.0);

            // When
            var compositeBranchStructures = ConvertCulvertDelftIniCategoryToCompositeBranchStructures(crossSectionDefinitionZw);

            // Then
            var csDefinitionZw = GetCrossSectionDefinitionZwFromCulvertCompositeBranchStructure(compositeBranchStructures);
            Assert.That(csDefinitionZw, Is.EqualTo(crossSectionDefinitionZw));
        }

        private IList<ICompositeBranchStructure> ConvertCulvertDelftIniCategoryToCompositeBranchStructures(CrossSectionDefinitionZW crossSectionDefinition)
        {
            var crossSectionDefinitions = new List<ICrossSectionDefinition> { crossSectionDefinition };
            Network = new HydroNetwork();
            var categories = new List<DelftIniCategory> { GetCulvertCategoryWithBasicProperties() };
            var branches = new List<IChannel> { GetMockedChannel() };

            var converter = new CompositeBranchStructureConverter();
            var compositeBranchStructures =
                converter.Convert(categories, branches, crossSectionDefinitions, new GroundLayerDataTransferObject[] { }, new List<string>());
            return compositeBranchStructures;
        }

        private static CrossSectionDefinitionZW GetCrossSectionDefinitionZwFromCulvertCompositeBranchStructure(ICollection<ICompositeBranchStructure> compositeBranchStructures)
        {
            Assert.That(compositeBranchStructures.Count, Is.EqualTo(1));

            var compositeBranchStructure = compositeBranchStructures.FirstOrDefault();
            var branchFeatures = compositeBranchStructure?.Branch.BranchFeatures;
            var culvert = branchFeatures?.FirstOrDefault() as Culvert;
            Assert.That(branchFeatures?.Count, Is.EqualTo(1));
            Assert.IsNotNull(culvert, "CompositeBranchStructureConverter did not return a Culvert object");

            return culvert.CrossSectionDefinition as CrossSectionDefinitionZW;
        }

        private const string CulvertName = "myCulvert";
        private const string CulvertLongName = "myCulvert_longName";
        private const string ChainageAsString = "2.0";
        private const double CrossSectionDefinitionWidth = 2.0;
        private const double CrossSectionDefinitionHeight = 3.0;

        private static DelftIniCategory GetCulvertCategoryWithBasicProperties()
        {
            var category = new DelftIniCategory(StructureRegion.Header);
            category.AddProperty(StructureRegion.DefinitionType.Key, StructureRegion.StructureTypeName.Culvert);
            category.AddProperty(StructureRegion.Id.Key, CulvertName);
            category.AddProperty(StructureRegion.Name.Key, CulvertLongName);
            category.AddProperty(StructureRegion.BranchId.Key, BranchName);
            category.AddProperty(StructureRegion.CsDefId.Key, CulvertName);
            category.AddProperty(StructureRegion.Chainage.Key, ChainageAsString);
            category.AddProperty(StructureRegion.Compound.Key, "0");
            category.AddProperty(StructureRegion.AllowedFlowDir.Key, "0");
            category.AddProperty(StructureRegion.LeftLevel.Key, "0.0");
            category.AddProperty(StructureRegion.RightLevel.Key, "0.0");
            category.AddProperty(StructureRegion.Length.Key, "1.0");
            category.AddProperty(StructureRegion.InletLossCoeff.Key, "1.0");
            category.AddProperty(StructureRegion.OutletLossCoeff.Key, "1.0");
            category.AddProperty(StructureRegion.ValveOnOff.Key, "1");
            category.AddProperty(StructureRegion.IniValveOpen.Key, "0.0");
            category.AddProperty(StructureRegion.BedFrictionType.Key, "1");
            category.AddProperty(StructureRegion.BedFriction.Key, "45.0");
            category.AddProperty(StructureRegion.GroundFrictionType.Key, "1");
            category.AddProperty(StructureRegion.GroundFriction.Key, "45.0");

            return category;
        }

        #endregion

        #region Reading Bridge Cross Section Definition
        
        [Test]
        public void GivenBridgeDelftIniCategoryWithMatchingCrossSectionDefinitionWithRectangleShape_WhenConvertingToCompositeBranchStructure_ThenBridgeCrossSectionShapePropertiesAreAsExpected()
        {
            // Given
            var rectangleShape = new CrossSectionStandardShapeRectangle
            {
                Width = CrossSectionDefinitionWidth,
                Height = CrossSectionDefinitionHeight
            };

            // When
            var compositeBranchStructures = ConvertBridgeDelftIniCategoryToCompositeBranchStructures(rectangleShape, new GroundLayerDataTransferObject[0]);

            // Then
            var shape = GetBridgeShapeFromCompositeStructure<CrossSectionStandardShapeRectangle>(compositeBranchStructures);
            Assert.That(shape.Width, Is.EqualTo(CrossSectionDefinitionWidth));
            Assert.That(shape.Height, Is.EqualTo(CrossSectionDefinitionHeight));
        }

        private IList<ICompositeBranchStructure> ConvertBridgeDelftIniCategoryToCompositeBranchStructures(ICrossSectionStandardShape shape, GroundLayerDataTransferObject[] groundLayerDataTransferObjects)
        {
            var crossSectionDefinitions = new List<ICrossSectionDefinition> { GetCrossSectionDefinition(BridgeName, shape) };
            Network = new HydroNetwork();
            var categories = new List<DelftIniCategory> { GetBridgeCategoryWithBasicProperties() };
            var branches = new List<IChannel> { GetMockedChannel() };

            var converter = new CompositeBranchStructureConverter();
            var compositeBranchStructures =
                converter.Convert(categories, branches, crossSectionDefinitions, groundLayerDataTransferObjects, new List<string>());
            return compositeBranchStructures;
        }

        private static T GetBridgeShapeFromCompositeStructure<T>(ICollection<ICompositeBranchStructure> compositeBranchStructures)
            where T : class, ICrossSectionStandardShape
        {
            Assert.That(compositeBranchStructures.Count, Is.EqualTo(1));

            var compositeBranchStructure = compositeBranchStructures.FirstOrDefault();
            var branchFeatures = compositeBranchStructure?.Branch.BranchFeatures;
            var bridge = branchFeatures?.FirstOrDefault() as Bridge;
            Assert.That(branchFeatures?.Count, Is.EqualTo(1));
            Assert.IsNotNull(bridge, "CompositeBranchStructureConverter did not return a Bridge object");

            var crossSectionDefinition = bridge.CrossSectionDefinition as CrossSectionDefinitionStandard;
            var shape = crossSectionDefinition?.Shape as T;
            Assert.IsNotNull(shape, $"CompositeBranchStructureConverter did not return a Bridge object with a {typeof(T)} shape type");

            return shape;
        }

        [Test]
        public void GivenBridgeDelftIniCategoryWithMatchingCrossSectionDefinitionZw_WhenConvertingToCompositeBranchStructure_ThenBridgeHasTabulatedShapeType()
        {
            // Given
            var crossSectionDefinitionZw = new CrossSectionDefinitionZW
            {
                Name = BridgeName
            };
            crossSectionDefinitionZw.ZWDataTable.AddCrossSectionZWRow(10.0, 5.0, 0.0);
            crossSectionDefinitionZw.ZWDataTable.AddCrossSectionZWRow(15.0, 3.0, 2.0);

            // When
            var compositeBranchStructures = ConvertBridgeDelftIniCategoryToCompositeBranchStructures(crossSectionDefinitionZw);

            // Then
            var csDefinitionZw = GetCrossSectionDefinitionZwFromBridgeCompositeBranchStructure(compositeBranchStructures);
            Assert.That(csDefinitionZw, Is.EqualTo(crossSectionDefinitionZw));
        }

        private IList<ICompositeBranchStructure> ConvertBridgeDelftIniCategoryToCompositeBranchStructures(CrossSectionDefinitionZW crossSectionDefinition)
        {
            var crossSectionDefinitions = new List<ICrossSectionDefinition> { crossSectionDefinition };
            Network = new HydroNetwork();
            var categories = new List<DelftIniCategory> { GetBridgeCategoryWithBasicProperties() };
            var branches = new List<IChannel> { GetMockedChannel() };

            var converter = new CompositeBranchStructureConverter();
            var compositeBranchStructures =
                converter.Convert(categories, branches, crossSectionDefinitions, new GroundLayerDataTransferObject[] { }, new List<string>());
            return compositeBranchStructures;
        }

        private static CrossSectionDefinitionZW GetCrossSectionDefinitionZwFromBridgeCompositeBranchStructure(ICollection<ICompositeBranchStructure> compositeBranchStructures)
        {
            Assert.That(compositeBranchStructures.Count, Is.EqualTo(1));

            var compositeBranchStructure = compositeBranchStructures.FirstOrDefault();
            var branchFeatures = compositeBranchStructure?.Branch.BranchFeatures;
            var culvert = branchFeatures?.FirstOrDefault() as Bridge;
            Assert.That(branchFeatures?.Count, Is.EqualTo(1));
            Assert.IsNotNull(culvert, "CompositeBranchStructureConverter did not return a Bridge object");

            return culvert.CrossSectionDefinition as CrossSectionDefinitionZW;
        }

        [Test]
        public void GivenBridgePillarDelftIniCategoryWithMatchingCrossSectionDefinition_WhenConvertingToCompositeBranchStructure_ThenBridgePillarHasNullCrossSectionDefinition()
        {
            // Given
            var category = GetBridgeCategoryWithBasicProperties();
            category.SetProperty(StructureRegion.DefinitionType.Key, "bridgePillar");
            category.Properties.RemoveAllWhere(p => p.Name == StructureRegion.CsDefId.Key); // No cross section ID for bridge pillars

            Network = new HydroNetwork();
            var categories = new List<DelftIniCategory> { category };
            var branches = new List<IChannel> { GetMockedChannel() };

            // When
            var converter = new CompositeBranchStructureConverter();
            var compositeBranchStructures =
                converter.Convert(categories, branches, new List<ICrossSectionDefinition>(), new GroundLayerDataTransferObject[0], new List<string>());

            // Then
            Assert.That(compositeBranchStructures.Count, Is.EqualTo(1));
            var compositeBranchStructure = compositeBranchStructures.FirstOrDefault();
            var branchFeatures = compositeBranchStructure?.Branch.BranchFeatures;
            var bridge = branchFeatures?.FirstOrDefault() as Bridge;

            Assert.IsNotNull(bridge);
            Assert.That(bridge.BridgeType, Is.EqualTo(BridgeType.Pillar));
            Assert.IsNull(bridge.CrossSectionDefinition);
        }

        [Test]
        public void GivenBridgeDelftIniCategoryWithMatchingGroundLayerDataWithNoGroundLayerUsed_WhenConvertingToCompositeBranchStructure_ThenBridgeCrossSectionGroundLayerPropertiesAreAsExpected()
        {
            // Given
            var rectangleShape = new CrossSectionStandardShapeRectangle
            {
                Width = CrossSectionDefinitionWidth,
                Height = CrossSectionDefinitionHeight
            };

            var groundLayerThickness = 2.5;
            var layerData = new GroundLayerDataTransferObject
            {
                CrossSectionDefinitionId = BridgeName,
                GroundLayerUsed = false, // GroundLayer is not used
                GroundLayerThickness = groundLayerThickness
            };
            var groundLayerData = new[] { layerData };

            // When
            var compositeBranchStructures = ConvertBridgeDelftIniCategoryToCompositeBranchStructures(rectangleShape, groundLayerData);

            // Then
            Assert.That(compositeBranchStructures.Count, Is.EqualTo(1));

            var compositeBranchStructure = compositeBranchStructures.FirstOrDefault();
            var branchFeatures = compositeBranchStructure?.Branch.BranchFeatures;
            var bridge = branchFeatures?.FirstOrDefault() as Bridge;
            Assert.IsNotNull(bridge);

            Assert.IsFalse(bridge.GroundLayerEnabled);
            Assert.That(bridge.GroundLayerRoughness, Is.EqualTo(0.0));
            Assert.That(bridge.GroundLayerThickness, Is.EqualTo(0.0));
        }

        [Test]
        public void GivenBridgeDelftIniCategoryWithMatchingGroundLayerData_WhenConvertingToCompositeBranchStructure_ThenBridgeCrossSectionGroundLayerPropertiesAreAsExpected()
        {
            // Given
            var rectangleShape = new CrossSectionStandardShapeRectangle
            {
                Width = CrossSectionDefinitionWidth,
                Height = CrossSectionDefinitionHeight
            };

            var groundLayerThickness = 2.5;
            var layerData = new GroundLayerDataTransferObject
            {
                CrossSectionDefinitionId = BridgeName,
                GroundLayerUsed = true, // GroundLayer is not used
                GroundLayerThickness = groundLayerThickness
            };
            var groundLayerData = new[] { layerData };

            // When
            var compositeBranchStructures = ConvertBridgeDelftIniCategoryToCompositeBranchStructures(rectangleShape, groundLayerData);

            // Then
            Assert.That(compositeBranchStructures.Count, Is.EqualTo(1));

            var compositeBranchStructure = compositeBranchStructures.FirstOrDefault();
            var branchFeatures = compositeBranchStructure?.Branch.BranchFeatures;
            var bridge = branchFeatures?.FirstOrDefault() as Bridge;
            Assert.IsNotNull(bridge);

            Assert.IsTrue(bridge.GroundLayerEnabled);
            Assert.That(bridge.GroundLayerThickness, Is.EqualTo(groundLayerThickness));
        }

        private const string BridgeName = "myBridge";
        private const string BridgeLongName = "myBridge_longName";

        private static DelftIniCategory GetBridgeCategoryWithBasicProperties()
        {
            var category = new DelftIniCategory(StructureRegion.Header);
            category.AddProperty(StructureRegion.Id.Key, BridgeName);
            category.AddProperty(StructureRegion.BranchId.Key, BranchName);
            category.AddProperty(StructureRegion.Name.Key, BridgeLongName);
            category.AddProperty(StructureRegion.Chainage.Key, ChainageAsString);
            category.AddProperty(StructureRegion.Compound.Key, "0");
            category.AddProperty(StructureRegion.DefinitionType.Key, "bridge");
            category.AddProperty(StructureRegion.AllowedFlowDir.Key, "0");
            category.AddProperty(StructureRegion.BedLevel.Key, "0.0");
            category.AddProperty(StructureRegion.CsDefId.Key, BridgeName);
            category.AddProperty(StructureRegion.Length.Key, "1.0");
            category.AddProperty(StructureRegion.InletLossCoeff.Key, "1.0");
            category.AddProperty(StructureRegion.OutletLossCoeff.Key, "1.0");
            category.AddProperty(StructureRegion.BedFrictionType.Key, "1");
            category.AddProperty(StructureRegion.BedFriction.Key, "45.0");
            category.AddProperty(StructureRegion.GroundFrictionType.Key, "1");
            category.AddProperty(StructureRegion.GroundFriction.Key, "45.0");
            category.AddProperty(StructureRegion.PillarWidth.Key, "1.0");
            category.AddProperty(StructureRegion.FormFactor.Key, "1.0");

            return category;
        }

        #endregion

        [Test]
        public void GivenAnUnknownTypeForAStructure_WhenTheConverterFactoryIsCreatingAConverter_ThenAnErrorMessageShouldBeCreated()
        {
            //Given
            var mocks = new MockRepository();

            var errorMessages = new List<string>();
            var categories = new List<DelftIniCategory>();

            var category = CreatePerfectCategory();
            categories.Add(category);

            var someFactoryMock = mocks.DynamicMock<Func<string, StructureConverter>>();
            someFactoryMock.Expect(e => e.Invoke("weir"))
                .Return(null)
                .Repeat.AtLeastOnce();
            
            mocks.ReplayAll();
            
            // Used for the constructor, but will not be executed.
            Func<DelftIniCategory, IStructure1D, IList<ICompositeBranchStructure>, ICompositeBranchStructure> compositeBranchStructuresFunc = BasicStructuresOperations.CreateCompositeBranchStructuresIfNeeded;

            //When
            var converter = new CompositeBranchStructureConverter(someFactoryMock, compositeBranchStructuresFunc );
            converter.Convert(categories, channels, new List<ICrossSectionDefinition>(), new GroundLayerDataTransferObject[] { }, errorMessages);

            //Then
            mocks.VerifyAll();

            Assert.AreEqual(1, errorMessages.Count);
            var expectedMessage = 
                string.Format(Resources.CompositeBranchStructureConverter_CreationOfStructuresAndCompositeBranchStructures_A__0__is_found_in_the_structure_file__line__1___and_this_type_is_not_supported_during_an_import_,
                "weir", 55);
            Assert.AreEqual(expectedMessage, errorMessages[0]);
        }

        [Test]
        public void GivenNullForStructure_WhenCreatingThisStructure_ThenAnErrorMessageShouldBeCreated()
        {
            //Given
            var mocks = new MockRepository();

            var errorMessages = new List<string>();
            var categories = new List<DelftIniCategory>();

            var category = CreatePerfectCategory();
            categories.Add(category);

            var convertMock = mocks.StrictMock<IStructureConverter>();
            convertMock.Expect(e => e.ConvertToStructure1D(category, channels.FirstOrDefault()))
                .Return(null)
                .Repeat.AtLeastOnce();

            var someFactoryMock = mocks.DynamicMock<Func<string, IStructureConverter>>();
            someFactoryMock.Expect(e => e.Invoke("weir"))
                .Return(convertMock)
                .Repeat.AtLeastOnce();

            mocks.ReplayAll();

            // Used for the constructor, but will not be executed.
            Func<DelftIniCategory, IStructure1D, IList<ICompositeBranchStructure>, ICompositeBranchStructure> compositeBranchStructuresFunc = BasicStructuresOperations.CreateCompositeBranchStructuresIfNeeded;

            //When
            var converter = new CompositeBranchStructureConverter(someFactoryMock, compositeBranchStructuresFunc);
            converter.Convert(categories, channels, new List<ICrossSectionDefinition>(), new GroundLayerDataTransferObject[] { }, errorMessages);

            //Then

            mocks.VerifyAll();

            Assert.AreEqual(1, errorMessages.Count);
            Assert.AreEqual(
                "Failed to create a structure from the structures file (line 55)",
                errorMessages[0]);
        }

        [Test]
        public void GivenNullForCompositeBranchStructure_WhenCreatingThisCorrespondingCompositeBranchStructureForAStructure_ThenAnErrorMessageShouldBeCreated()
        {
            //Given
            var mocks = new MockRepository();

            var errorMessages = new List<string>();
            var categories = new List<DelftIniCategory>();
            
            var category = CreatePerfectCategory();
            categories.Add(category);
            
            var someStructure = mocks.DynamicMock<IStructure1D>();
            someStructure.Expect(s => s.Name).Return("Weir").Repeat.Any();
            
            var convertMock = mocks.DynamicMock<IStructureConverter>();
            convertMock.Expect(e => e.ConvertToStructure1D(category, channels.FirstOrDefault()))
                .Return(someStructure)
                .Repeat.AtLeastOnce();

            var someFactoryMock = mocks.DynamicMock<Func<string, IStructureConverter>>();
            someFactoryMock.Expect(e => e.Invoke("weir"))
                .Return(convertMock)
                .IgnoreArguments()
                .Repeat.AtLeastOnce();

            var someCompositeBranchStructureMock = mocks
                .DynamicMock<Func<DelftIniCategory, IStructure1D, IList<ICompositeBranchStructure>,
                    ICompositeBranchStructure>>();
            someCompositeBranchStructureMock.Expect(e => e.Invoke(null, null, null))
                .IgnoreArguments()
                .Return(null)
                .Repeat.AtLeastOnce();

            mocks.ReplayAll();
            
            //When
            var converter = new CompositeBranchStructureConverter(someFactoryMock, someCompositeBranchStructureMock);
            converter.Convert(categories, channels, new List<ICrossSectionDefinition>(), new GroundLayerDataTransferObject[] { }, errorMessages);

            //Then

            mocks.VerifyAll();

            Assert.AreEqual(1, errorMessages.Count);
            Assert.AreEqual("Failed to create structure Weir from the structures file (line 55)",
                errorMessages[0]);
        }

        private DelftIniCategory CreatePerfectCategory()
        {
            var category = new DelftIniCategory(StructureRegion.Header);

            category.AddProperty(StructureRegion.Id.Key, "Weir1");
            category.AddProperty(StructureRegion.BranchId.Key, "branch");
            category.AddProperty(StructureRegion.Chainage.Key, "50");
            category.AddProperty(StructureRegion.Compound.Key, "1");
            category.AddProperty(StructureRegion.CompoundName.Key, "Bla");
            category.AddProperty(StructureRegion.DefinitionType.Key, "weir");
            
            category.AddProperty(StructureRegion.CrestLevel.Key, "1.3");
            category.AddProperty(StructureRegion.CrestWidth.Key, "100");
            category.AddProperty(StructureRegion.DischargeCoeff.Key, "1.1");
            category.AddProperty(StructureRegion.LatDisCoeff.Key, "1.2");
            category.AddProperty(StructureRegion.AllowedFlowDir.Key, "0");

            category.LineNumber = 55;

            return category;
        }
    }
}