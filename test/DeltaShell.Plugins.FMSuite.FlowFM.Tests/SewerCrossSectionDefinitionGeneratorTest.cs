using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class SewerCrossSectionDefinitionGeneratorTest : SewerFeatureFactoryTestHelper
    {
        private const string ProfileId = "PRO1";

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
            GenerateCrossSectionDefinitionAndCheckForNull<CircleCrossSectionDefinitionGenerator>(profileGwswElement);
            GenerateCrossSectionDefinitionAndCheckForNull<RectangleCrossSectionDefinitionGenerator>(profileGwswElement);
            GenerateCrossSectionDefinitionAndCheckForNull<EggCrossSectionDefinitionGenerator>(profileGwswElement);
            GenerateCrossSectionDefinitionAndCheckForNull<TrapezoidCrossSectionDefinitionGenerator>(profileGwswElement);
            GenerateCrossSectionDefinitionAndCheckForNull<ArchCrossSectionDefinitionGenerator>(profileGwswElement);
            GenerateCrossSectionDefinitionAndCheckForNull<CunetteCrossSectionDefinitionGenerator>(profileGwswElement);
            GenerateCrossSectionDefinitionAndCheckForNull<DefaultCrossSectionDefinitionGenerator>(profileGwswElement);
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
            var materialString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(material);
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
            GenerateCrossSectionDefinitionAndCheckShapeProperties<CircleCrossSectionDefinitionGenerator, CrossSectionStandardShapeRound>(profileGwswElement, expectedDiameter);
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
                () => GenerateCrossSectionDefinitionAndCheckShapeProperties<CircleCrossSectionDefinitionGenerator, CrossSectionStandardShapeRound>(profileGwswElement, 0.160d),
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
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "1200", string.Empty, TypeDouble)
                }
            };
            GenerateCrossSectionDefinitionAndCheckShapeProperties<RectangleCrossSectionDefinitionGenerator, CrossSectionStandardShapeRectangle>(profileGwswElement, 2.0, 1.2);
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
                () => GenerateCrossSectionDefinitionAndCheckShapeProperties<RectangleCrossSectionDefinitionGenerator, CrossSectionStandardShapeRectangle>(profileGwswElement, 1.0, 1.0),
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
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "250", string.Empty, TypeDouble)
                }
            };
            GenerateCrossSectionDefinitionAndCheckShapeProperties<EggCrossSectionDefinitionGenerator, CrossSectionStandardShapeEgg>(profileGwswElement, 0.25, 0.375);
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
                () => GenerateCrossSectionDefinitionAndCheckShapeProperties<EggCrossSectionDefinitionGenerator, CrossSectionStandardShapeEgg>(profileGwswElement, 2.0, 3.0),
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
            GenerateCrossSectionDefinitionAndCheckShapeProperties<EggCrossSectionDefinitionGenerator, CrossSectionStandardShapeEgg>(profileGwswElement, 0.25, 0.375);
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

            var generator = new EggCrossSectionDefinitionGenerator();
            TestHelper.AssertAtLeastOneLogMessagesContains(() => generator.Generate(profileGwswElement, new HydroNetwork(), null), 
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
            GenerateCrossSectionDefinitionAndCheckShapeProperties<EggCrossSectionDefinitionGenerator, CrossSectionStandardShapeEgg>(profileGwswElement, 0.25, 0.375);
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
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "2000", string.Empty, TypeDouble)
                }
            };
            GenerateCrossSectionDefinitionAndCheckShapeProperties<CunetteCrossSectionDefinitionGenerator, CrossSectionStandardShapeCunette>(profileGwswElement, 2.0, 1.268);
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
            GenerateCrossSectionDefinitionAndCheckShapeProperties<CunetteCrossSectionDefinitionGenerator, CrossSectionStandardShapeCunette>(profileGwswElement, 2.0, 1.268);
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
                () => GenerateCrossSectionDefinitionAndCheckShapeProperties<CunetteCrossSectionDefinitionGenerator, CrossSectionStandardShapeCunette>(profileGwswElement, 1.0, 0.634),
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

            var cunetteGenerator = new CunetteCrossSectionDefinitionGenerator();
            TestHelper.AssertAtLeastOneLogMessagesContains(() => cunetteGenerator.Generate(profileGwswElement, new HydroNetwork()),
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
            GenerateCrossSectionDefinitionAndCheckShapeProperties<CunetteCrossSectionDefinitionGenerator, CrossSectionStandardShapeCunette>(profileGwswElement, 0.25, 0.1585);
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
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "2500", string.Empty, TypeDouble)
                }
            };

            GenerateCrossSectionDefinitionAndCheckShapeProperties<ArchCrossSectionDefinitionGenerator, CrossSectionStandardShapeArch>(profileGwswElement, 1.2, 2.5, 2.5);
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
                () => GenerateCrossSectionDefinitionAndCheckShapeProperties<ArchCrossSectionDefinitionGenerator, CrossSectionStandardShapeArch>(profileGwswElement, 1.0, 2.0, 1.0),
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
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.Slope2, slope2, string.Empty, TypeDouble)
                }
            };
            GenerateTrapezoidCrossSectionAndCheckShapeProperties<TrapezoidCrossSectionDefinitionGenerator, CrossSectionStandardShapeTrapezium>(profileGwswElement, 1.0, 2.0, 2.0);
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
            GenerateTrapezoidCrossSectionAndCheckShapeProperties<TrapezoidCrossSectionDefinitionGenerator, CrossSectionStandardShapeTrapezium>(profileGwswElement, 1.0, 2.0, 2.0);

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
                () => GenerateTrapezoidCrossSectionAndCheckShapeProperties<TrapezoidCrossSectionDefinitionGenerator, CrossSectionStandardShapeTrapezium>(profileGwswElement, 10.0, 2.0, 20.0),
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
                () => GenerateTrapezoidCrossSectionAndCheckShapeProperties<TrapezoidCrossSectionDefinitionGenerator, CrossSectionStandardShapeTrapezium>(profileGwswElement, 10.0, 2.0, 20.0),
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
                () => GenerateTrapezoidCrossSectionAndCheckShapeProperties<TrapezoidCrossSectionDefinitionGenerator, CrossSectionStandardShapeTrapezium>(profileGwswElement, 10.0, 2.0, 20.0),
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
            GenerateCrossSectionDefinitionAndCheckShapeProperties<DefaultCrossSectionDefinitionGenerator, CrossSectionStandardShapeRound>(profileGwswElement, 0.4);
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
            var defaultGenerator = new DefaultCrossSectionDefinitionGenerator();
            var expectedMessage =
                "Shape was not defined for sewer profile 'PRO1' in 'Profiel.csv'. A default round profile with diameter of 400 mm is used for this profile.";
            TestHelper.AssertAtLeastOneLogMessagesContains(() => defaultGenerator.Generate(profileGwswElement, new HydroNetwork()), expectedMessage);
        }

        #endregion

        #region Test helpers

        private static void GenerateCrossSectionDefinitionAndCheckForNull<T>(GwswElement gwswElement) where T : ASewerCrossSectionDefinitionGenerator, new()
        {
            var generator = new T();
            var network = new HydroNetwork();
            generator.Generate(gwswElement, network);
            Assert.IsEmpty(network.SharedCrossSectionDefinitions);
        }

        private void GenerateCrossSectionDefinitionAndCheckShapeProperties<TGenerator, TShape>(GwswElement gwswElement, double expectedWidth)
            where TGenerator : ASewerCrossSectionDefinitionGenerator, new()
            where TShape : CrossSectionStandardShapeBase, new()
        {
            GenerateCrossSectionDefinitionAndCheckShapeProperties<TGenerator, TShape>(gwswElement, expectedWidth, double.NaN, double.NaN, double.NaN, double.NaN);
        }

        private void GenerateCrossSectionDefinitionAndCheckShapeProperties<TGenerator, TShape>(GwswElement gwswElement, double expectedWidth, double expectedHeight, double expectedArcHeight = double.NaN)
            where TGenerator : ASewerCrossSectionDefinitionGenerator, new()
            where TShape : CrossSectionStandardShapeBase, new()
        {
            GenerateCrossSectionDefinitionAndCheckShapeProperties<TGenerator, TShape>(gwswElement, expectedWidth, expectedHeight, expectedArcHeight, double.NaN, double.NaN);
        }

        private void GenerateTrapezoidCrossSectionAndCheckShapeProperties<TGenerator, TShape>(GwswElement gwswElement, double expectedBottomWidth, double expectedSlope, double expectedMaxFlowWidth)
            where TGenerator : ASewerCrossSectionDefinitionGenerator, new()
            where TShape : CrossSectionStandardShapeBase, new()
        {
            GenerateCrossSectionDefinitionAndCheckShapeProperties<TGenerator, TShape>(gwswElement, expectedBottomWidth, double.NaN, double.NaN, expectedSlope, expectedMaxFlowWidth);
        }

        private static void GenerateCrossSectionDefinitionAndCheckShapeProperties<TGenerator, TShape>(GwswElement gwswElement, double expectedWidth, double expectedHeight, double expectedArcHeight, double expectedSlope, double expectedMaxFlowWidth)
            where TGenerator : ASewerCrossSectionDefinitionGenerator, new()
            where TShape : CrossSectionStandardShapeBase, new()
        {
            var csShape = GenerateValidCrossSectionAndReturnShape<TGenerator>(gwswElement) as TShape;
            Assert.NotNull(csShape);

            var csHeightWidthShape = csShape as CrossSectionStandardShapeWidthHeightBase;
            if (csHeightWidthShape != null)
            {
                Assert.That(csHeightWidthShape.Width, Is.EqualTo(expectedWidth));
                Assert.That(csHeightWidthShape.Height, Is.EqualTo(expectedHeight));
            }

            if (typeof(TShape) == typeof(CrossSectionStandardShapeRound))
            {
                var csRoundShape = csShape as CrossSectionStandardShapeRound;
                Assert.NotNull(csRoundShape);
                Assert.That(csRoundShape.Diameter, Is.EqualTo(expectedWidth));
            }
            else if (typeof(TShape) == typeof(CrossSectionStandardShapeArch))
            {
                var csArchShape = csShape as CrossSectionStandardShapeArch;
                Assert.NotNull(csArchShape);
                Assert.That(csArchShape.ArcHeight, Is.EqualTo(expectedArcHeight));
            }
            else if (typeof(TShape) == typeof(CrossSectionStandardShapeTrapezium))
            {
                var csTrapezoidShape = csShape as CrossSectionStandardShapeTrapezium;
                Assert.NotNull(csTrapezoidShape);
                Assert.That(csTrapezoidShape.BottomWidthB, Is.EqualTo(expectedWidth));
                Assert.That(csTrapezoidShape.Slope, Is.EqualTo(expectedSlope));
                Assert.That(csTrapezoidShape.MaximumFlowWidth, Is.EqualTo(expectedMaxFlowWidth));
            }
        }

        private static ICrossSectionStandardShape GenerateValidCrossSectionAndReturnShape<TGenerator>(GwswElement gwswElement)
            where TGenerator : ASewerCrossSectionDefinitionGenerator, new()
        {
            var generator = new TGenerator();
            var network = new HydroNetwork();
            generator.Generate(gwswElement, network);
            Assert.IsNotEmpty(network.SharedCrossSectionDefinitions);

            var csDefinition = network.SharedCrossSectionDefinitions.FirstOrDefault() as CrossSectionDefinitionStandard;
            Assert.NotNull(csDefinition);
            Assert.That(csDefinition.Name, Is.EqualTo(ProfileId));

            return csDefinition.Shape;
        }

        #endregion
    }
}