using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests
{
    [TestFixture]
    public class SewerCrossSectionDefinitionGeneratorTest : SewerFeatureFactoryTestHelper
    {
        private const string ProfileId = "PRO1";
        private readonly string unknownMaterialValue = SewerProfileMapping.SewerProfileMaterial.Unknown.GetDescription();
        private readonly string concreteMaterialValue = SewerProfileMapping.SewerProfileMaterial.Concrete.GetDescription();

        [TestCase(SewerFeatureType.Node)]
        [TestCase(SewerFeatureType.Connection)]
        [TestCase(SewerFeatureType.Discharge)]
        [TestCase(SewerFeatureType.Distribution)]
        [TestCase(SewerFeatureType.Meta)]
        [TestCase(SewerFeatureType.Runoff)]
        [TestCase(SewerFeatureType.Structure)]
        [TestCase(SewerFeatureType.Surface)]
        public void GivenGwswElementWithElementTypeNameUnequalToSewerProfile_WhenGeneratingSewerProfile_ThenReturnNull(SewerFeatureType type)
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = type.ToString()
            };
            GenerateCrossSectionDefinitionAndCheckForNull<CircleCrossSectionShapeGenerator>(profileGwswElement);
            GenerateCrossSectionDefinitionAndCheckForNull<RectangleCrossSectionShapeGenerator>(profileGwswElement);
            GenerateCrossSectionDefinitionAndCheckForNull<EggCrossSectionShapeGenerator>(profileGwswElement);
            GenerateCrossSectionDefinitionAndCheckForNull<TrapezoidCrossSectionShapeGenerator>(profileGwswElement);
            GenerateCrossSectionDefinitionAndCheckForNull<ArchCrossSectionShapeGenerator>(profileGwswElement);
            GenerateCrossSectionDefinitionAndCheckForNull<CunetteCrossSectionShapeGenerator>(profileGwswElement);
            GenerateCrossSectionDefinitionAndCheckForNull<DefaultCrossSectionShapeGenerator>(profileGwswElement);
        }

        #region Circle sewer profile shape
        
        [TestCase(SewerProfileMapping.SewerProfileMaterial.CastIron, 1.7)]
        [TestCase(SewerProfileMapping.SewerProfileMaterial.Concrete, 1.7)]
        [TestCase(SewerProfileMapping.SewerProfileMaterial.Hdpe, 1.7)]
        [TestCase(SewerProfileMapping.SewerProfileMaterial.Masonry, 1.7)]
        [TestCase(SewerProfileMapping.SewerProfileMaterial.Polyester, 1.7)]
        [TestCase(SewerProfileMapping.SewerProfileMaterial.Polyvinylchlorid, 1.6)]
        [TestCase(SewerProfileMapping.SewerProfileMaterial.SheetMetal, 1.7)]
        [TestCase(SewerProfileMapping.SewerProfileMaterial.Steel, 1.7)]
        [TestCase(SewerProfileMapping.SewerProfileMaterial.StoneWare, 1.7)]
        public void GivenCircleProfileGwswElement_WhenGeneratingSewerProfileCircle_ThenReturnCircleShapeWithCorrectDiameter(SewerProfileMapping.SewerProfileMaterial material, double expectedDiameter)
        {
            var materialString = material.GetDescription();
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "1700", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileMaterial, materialString, string.Empty)
                }
            };
            GenerateCrossSectionDefinitionAndCheckShapeProperties<CircleCrossSectionShapeGenerator, CrossSectionStandardShapeCircle>(profileGwswElement, ProfileId, expectedDiameter, materialString);
        }

        [Test]
        public void GivenSewerProfileGwswElementWithoutWidthDefined_WhenGeneratingSewerProfileCircle_ThenReturnDefaultCircleSewerProfileAndLogMessage()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty)
                }
            };
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => GenerateCrossSectionDefinitionAndCheckShapeProperties<CircleCrossSectionShapeGenerator, CrossSectionStandardShapeCircle>(profileGwswElement, ProfileId, 0.160d, unknownMaterialValue),
                "Sewer profile 'PRO1' is missing its width. Default profile property values are used for this profile.");
        }

        #endregion

        #region Rectangle sewer profile shape

        [Test]
        public void GivenGwswElement_WhenGeneratingSewerProfileRectangle_ThenReturnRectangleShapeWithCorrectPropertyValues()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "2000", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "1200", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileMaterial, concreteMaterialValue, string.Empty)
                }
            };
            GenerateCrossSectionDefinitionAndCheckShapeProperties<RectangleCrossSectionShapeGenerator, CrossSectionStandardShapeRectangle>(profileGwswElement, ProfileId, 2.0, 1.2, concreteMaterialValue);
        }

        [TestCase(SewerProfileMapping.PropertyKeys.SewerProfileWidth)]
        [TestCase(SewerProfileMapping.PropertyKeys.SewerProfileHeight)]
        [TestCase("fakeKey")]
        public void GivenSewerProfileGwswElementWithoutHeightOrWidthDefined_WhenGeneratingSewerProfileRectangle_ThenReturnDefaultRectangleSewerProfileAndLogMessage(string key)
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(key, "2000", string.Empty, TypeDouble)
                }
            };
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => GenerateCrossSectionDefinitionAndCheckShapeProperties<RectangleCrossSectionShapeGenerator, CrossSectionStandardShapeRectangle>(profileGwswElement, ProfileId, 1.0, 1.0, unknownMaterialValue),
                "Sewer profile 'PRO1' is missing its width and/or height. Default profile property values are used for this profile.");
        }

        #endregion

        #region Egg sewer profile shape

        [Test]
        public void GivenGwswElement_WhenGeneratingSewerProfileEgg_ThenReturnEggShapeWithCorrectPropertyValues()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "250", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileMaterial, concreteMaterialValue, string.Empty)
                }
            };
            GenerateCrossSectionDefinitionAndCheckShapeProperties<EggCrossSectionShapeGenerator, CrossSectionStandardShapeEgg>(profileGwswElement, ProfileId, 0.25, 0.375, concreteMaterialValue);
        }

        [Test]
        public void GivenSewerProfileGwswElementWithoutWidthDefined_WhenGeneratingSewerProfileEgg_ThenReturnDefaultEggShapeAndLogMessage()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty)
                }
            };
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => GenerateCrossSectionDefinitionAndCheckShapeProperties<EggCrossSectionShapeGenerator, CrossSectionStandardShapeEgg>(profileGwswElement, ProfileId, 2.0, 3.0, unknownMaterialValue),
                "Sewer profile 'PRO1' is missing its width. Default profile property values are used for this profile.");
        }

        [Test]
        public void GivenGwswElementWithWidthAndHeightInCorrectProportion_WhenGeneratingSewerProfileEgg_ThenReturnEggShapeWithCorrectPropertyValues()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "250", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "375", string.Empty, TypeDouble)
                }
            };
            GenerateCrossSectionDefinitionAndCheckShapeProperties<EggCrossSectionShapeGenerator, CrossSectionStandardShapeEgg>(profileGwswElement, ProfileId, 0.25, 0.375, unknownMaterialValue);
        }

        [Test]
        public void GivenSewerProfileGwswElementWithWidthAndHeightNotInCorrectProportion_WhenGeneratingSewerProfileEgg_ThenLogMessageIsGivenToUser()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "250", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "400", string.Empty, TypeDouble)
                }
            };

            var generator = new EggCrossSectionShapeGenerator();
            TestHelper.AssertAtLeastOneLogMessagesContains(() => generator.Generate(profileGwswElement), 
                "The width and height of sewer profile 'PRO1' are not in the right proportion (2:3). Width is now 250 mm and height is now 375 mm.");
        }

        [Test]
        public void GivenSewerProfileGwswElementWithWidthAndHeightNotInCorrectProportion_WhenGeneratingSewerProfileEgg_ThenHeightFromGwswElementIsIgnored()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "250", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "375", string.Empty, TypeDouble)
                }
            };
            GenerateCrossSectionDefinitionAndCheckShapeProperties<EggCrossSectionShapeGenerator, CrossSectionStandardShapeEgg>(profileGwswElement, ProfileId, 0.25, 0.375, unknownMaterialValue);
        }

        #endregion

        #region Cunette sewer profile shape

        [Test]
        public void GivenGwswElement_WhenGeneratingSewerProfileCunette_ThenReturnCunetteShapeWithCorrectPropertyValues()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "2000", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileMaterial, concreteMaterialValue, string.Empty)
                }
            };
            GenerateCrossSectionDefinitionAndCheckShapeProperties<CunetteCrossSectionShapeGenerator, CrossSectionStandardShapeCunette>(profileGwswElement, ProfileId, 2.0, 1.268, concreteMaterialValue);
        }

        [Test]
        public void GivenGwswElementWithWidthAndHeightInCorrectProportion_WhenGeneratingSewerProfileCunette_ThenReturnCunetteShapeWithCorrectPropertyValues()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "2000", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "1268", string.Empty, TypeDouble)
                }
            };
            GenerateCrossSectionDefinitionAndCheckShapeProperties<CunetteCrossSectionShapeGenerator, CrossSectionStandardShapeCunette>(profileGwswElement, ProfileId, 2.0, 1.268, unknownMaterialValue);
        }

        [Test]
        public void GivenSewerProfileGwswElementWithoutWidthDefined_WhenGeneratingSewerProfileCunette_ThenReturnDefaultCunetteShapeAndLogMessage()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty)
                }
            };
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => GenerateCrossSectionDefinitionAndCheckShapeProperties<CunetteCrossSectionShapeGenerator, CrossSectionStandardShapeCunette>(profileGwswElement, ProfileId, 1.0, 0.634, unknownMaterialValue),
                "Sewer profile 'PRO1' is missing its width. Default profile property values are used for this profile.");
        }

        [Test]
        public void GivenSewerProfileGwswElementWithWidthAndHeightNotInCorrectProportion_WhenGeneratingSewerProfileCunette_ThenLogMessageIsGivenToUser()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "250", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "400", string.Empty, TypeDouble)
                }
            };

            var cunetteGenerator = new CunetteCrossSectionShapeGenerator();
            TestHelper.AssertAtLeastOneLogMessagesContains(() => cunetteGenerator.Generate(profileGwswElement),
                "The width and height of sewer profile 'PRO1' are not in the right proportion (1:0.634). Width is now 250 mm and height is now 158.5 mm.");
        }

        [Test]
        public void GivenSewerProfileGwswElementWithWidthAndHeightNotInCorrectProportion_WhenGeneratingSewerProfileCunette_ThenHeightFromGwswElementIsIgnored()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "250", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "400", string.Empty, TypeDouble)
                }
            };
            GenerateCrossSectionDefinitionAndCheckShapeProperties<CunetteCrossSectionShapeGenerator, CrossSectionStandardShapeCunette>(profileGwswElement, ProfileId, 0.25, 0.1585, unknownMaterialValue);
        }

        #endregion

        #region Arch sewer profile shape

        [Test]
        public void GivenGwswElement_WhenGeneratingSewerProfileArch_ThenReturnArchShapeWithCorrectPropertyValues()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "1200", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "2500", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileMaterial, concreteMaterialValue, string.Empty)
                }
            };

            GenerateCrossSectionDefinitionAndCheckShapeProperties<ArchCrossSectionShapeGenerator, CrossSectionStandardShapeArch>(profileGwswElement, ProfileId, 1.2, 2.5, concreteMaterialValue, 2.5);
        }

        [TestCase(SewerProfileMapping.PropertyKeys.SewerProfileWidth)]
        [TestCase(SewerProfileMapping.PropertyKeys.SewerProfileHeight)]
        public void GivenGwswElementWithMissingValues_WhenGeneratingSewerProfileArch_ThenReturnDefaultArchShapeAndLogMessage(string key)
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(key, "1000", string.Empty, TypeDouble)
                }
            };
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => GenerateCrossSectionDefinitionAndCheckShapeProperties<ArchCrossSectionShapeGenerator, CrossSectionStandardShapeArch>(profileGwswElement, ProfileId, 1.0, 2.0, unknownMaterialValue, 1.0),
                "Sewer profile 'PRO1' is missing its width and/or height. Default profile property values are used for this profile.");
        }

        #endregion

        #region Trapezoid sewer profile shape

        [TestCase("2,5", "1,5")]
        [TestCase("2.5", "1.5")]
        public void GivenGwswElement_WhenGeneratingSewerProfileTrapezoid_ThenReturnTrapezoidShapeWithCorrectPropertyValues(string slope1, string slope2)
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "1000", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "1000", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.Slope1, slope1, string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.Slope2, slope2, string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileMaterial, concreteMaterialValue, string.Empty)
                }
            };
            GenerateTrapezoidCrossSectionAndCheckShapeProperties<TrapezoidCrossSectionShapeGenerator, CrossSectionStandardShapeTrapezium>(profileGwswElement, ProfileId, 1.0, 2.0, 2.0, concreteMaterialValue);
        }

        [TestCase(SewerProfileMapping.PropertyKeys.Slope1)]
        [TestCase(SewerProfileMapping.PropertyKeys.Slope2)]
        public void GivenGwswElementWithOneMissingSlope_WhenGeneratingSewerProfileTrapezoid_ThenReturnTrapezoidWithPresentSlope(string slopeKey)
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "1000", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "1000", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(slopeKey, "2", string.Empty, TypeDouble)
                }
            };
            GenerateTrapezoidCrossSectionAndCheckShapeProperties<TrapezoidCrossSectionShapeGenerator, CrossSectionStandardShapeTrapezium>(profileGwswElement, ProfileId, 1.0, 2.0, 2.0, unknownMaterialValue);

        }

        [Test]
        public void GivenSewerProfileGwswElementWithMissingWidth_WhenGeneratingSewerProfileTrapezoid_ThenReturnDefaultTrapezoidSewerProfileAndLogMessage()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "1000", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.Slope1, "2,5", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.Slope2, "1,5", string.Empty, TypeDouble)
                }
            };
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => GenerateTrapezoidCrossSectionAndCheckShapeProperties<TrapezoidCrossSectionShapeGenerator, CrossSectionStandardShapeTrapezium>(profileGwswElement, ProfileId, 10.0, 2.0, 20.0, unknownMaterialValue),
                "Sewer profile 'PRO1' is missing its width, height and/or slope. Default profile property values are used for this profile.");
        }

        [Test]
        public void GivenSewerProfileGwswElementWithMissingHeight_WhenGeneratingSewerProfileTrapezoid_ThenReturnDefaultTrapezoidSewerProfile()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "1000", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.Slope1, "2,5", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.Slope2, "1,5", string.Empty, TypeDouble)
                }
            };
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => GenerateTrapezoidCrossSectionAndCheckShapeProperties<TrapezoidCrossSectionShapeGenerator, CrossSectionStandardShapeTrapezium>(profileGwswElement, ProfileId, 10.0, 2.0, 20.0, unknownMaterialValue),
                "Sewer profile 'PRO1' is missing its width, height and/or slope. Default profile property values are used for this profile.");
        }

        [Test]
        public void GivenSewerProfileGwswElementWithMissingSlopes_WhenGeneratingSewerProfileTrapezoid_ThenReturnDefaultTrapezoidSewerProfile()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "1000", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "1000", string.Empty, TypeDouble)
                }
            };
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => GenerateTrapezoidCrossSectionAndCheckShapeProperties<TrapezoidCrossSectionShapeGenerator, CrossSectionStandardShapeTrapezium>(profileGwswElement, ProfileId, 10.0, 2.0, 20.0, unknownMaterialValue),
                "Sewer profile 'PRO1' is missing its width, height and/or slope. Default profile property values are used for this profile.");
        }

        #endregion

        #region Default sewer profile shape

        [Test]
        public void GivenDefaultDefinitionGenerator_WhenGeneratingSewerProfile_ThenReturnDefaultDefinitionAlways()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "3000", "400", TypeDouble)
                }
            };
            GenerateCrossSectionDefinitionAndCheckShapeProperties<DefaultCrossSectionShapeGenerator, CrossSectionStandardShapeCircle>(profileGwswElement, ProfileId, 0.4, concreteMaterialValue);
        }

        [Test]
        public void GivenDefaultDefinitionGenerator_WhenGeneratingSewerProfile_ThenLogMessage()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty)
                }
            };
            var defaultGenerator = new DefaultCrossSectionShapeGenerator();
            var expectedMessage =
                "Shape was not defined for sewer profile 'PRO1' in 'Profiel.csv'. A default round profile with diameter of 400 mm is used for this profile.";
            TestHelper.AssertAtLeastOneLogMessagesContains(() => defaultGenerator.Generate(profileGwswElement), expectedMessage);
        }

        #endregion

        #region Test helpers

        private static void GenerateCrossSectionDefinitionAndCheckForNull<T>(GwswElement gwswElement) where T : ASewerCrossSectionShapeGenerator, new()
        {
            var generator = new T();
            var network = new HydroNetwork();
            generator.Generate(gwswElement);
            Assert.IsEmpty(network.SharedCrossSectionDefinitions);
        }

        private void GenerateCrossSectionDefinitionAndCheckShapeProperties<TGenerator, TShape>(GwswElement gwswElement, string expectedName, double expectedWidth, string expectedMaterial)
            where TGenerator : ASewerCrossSectionShapeGenerator, new()
            where TShape : CrossSectionStandardShapeBase, new()
        {
            GenerateCrossSectionDefinitionAndCheckShapeProperties<TGenerator, TShape>(gwswElement, expectedName, expectedWidth, double.NaN, double.NaN, double.NaN, double.NaN, expectedMaterial);
        }

        private void GenerateCrossSectionDefinitionAndCheckShapeProperties<TGenerator, TShape>(GwswElement gwswElement, string expectedName, double expectedWidth, double expectedHeight, string expectedMaterial, double expectedArcHeight = double.NaN)
            where TGenerator : ASewerCrossSectionShapeGenerator, new()
            where TShape : CrossSectionStandardShapeBase, new()
        {
            GenerateCrossSectionDefinitionAndCheckShapeProperties<TGenerator, TShape>(gwswElement, expectedName, expectedWidth, expectedHeight, expectedArcHeight, double.NaN, double.NaN, expectedMaterial);
        }

        private void GenerateTrapezoidCrossSectionAndCheckShapeProperties<TGenerator, TShape>(GwswElement gwswElement, string expectedName, double expectedBottomWidth, double expectedSlope, double expectedMaxFlowWidth, string expectedMaterial)
            where TGenerator : ASewerCrossSectionShapeGenerator, new()
            where TShape : CrossSectionStandardShapeBase, new()
        {
            GenerateCrossSectionDefinitionAndCheckShapeProperties<TGenerator, TShape>(gwswElement, expectedName, expectedBottomWidth, double.NaN, double.NaN, expectedSlope, expectedMaxFlowWidth, expectedMaterial);
        }

        private static void GenerateCrossSectionDefinitionAndCheckShapeProperties<TGenerator, TShape>(GwswElement gwswElement, string expectedName, double expectedWidth, double expectedHeight, double expectedArcHeight, double expectedSlope, double expectedMaxFlowWidth, string expectedMaterial)
            where TGenerator : ASewerCrossSectionShapeGenerator, new()
            where TShape : CrossSectionStandardShapeBase, new()
        {
            var createdShape = new TGenerator().Generate(gwswElement) as TShape;
            Assert.IsNotNull(createdShape);

            Assert.That(createdShape.Name, Is.EqualTo(expectedName));
            Assert.That(createdShape.MaterialName, Is.EqualTo(expectedMaterial));

            var csHeightWidthShape = createdShape as CrossSectionStandardShapeWidthHeightBase;
            if (csHeightWidthShape != null)
            {
                Assert.That(csHeightWidthShape.Width, Is.EqualTo(expectedWidth));
                Assert.That(csHeightWidthShape.Height, Is.EqualTo(expectedHeight));
            }

            if (typeof(TShape) == typeof(CrossSectionStandardShapeCircle))
            {
                var csRoundShape = createdShape as CrossSectionStandardShapeCircle;
                Assert.NotNull(csRoundShape);
                Assert.That(csRoundShape.Diameter, Is.EqualTo(expectedWidth));
            }
            else if (typeof(TShape) == typeof(CrossSectionStandardShapeArch))
            {
                var csArchShape = createdShape as CrossSectionStandardShapeArch;
                Assert.NotNull(csArchShape);
                Assert.That(csArchShape.ArcHeight, Is.EqualTo(expectedArcHeight));
            }
            else if (typeof(TShape) == typeof(CrossSectionStandardShapeTrapezium))
            {
                var csTrapezoidShape = createdShape as CrossSectionStandardShapeTrapezium;
                Assert.NotNull(csTrapezoidShape);
                Assert.That(csTrapezoidShape.BottomWidthB, Is.EqualTo(expectedWidth));
                Assert.That(csTrapezoidShape.Slope, Is.EqualTo(expectedSlope));
                Assert.That(csTrapezoidShape.MaximumFlowWidth, Is.EqualTo(expectedMaxFlowWidth));
            }
        }

        #endregion
    }
}