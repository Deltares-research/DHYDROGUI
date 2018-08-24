using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class SewerProfileDefinitionGenerationTest : SewerFeatureFactoryTestHelper
    {
        private const string ProfileId = "PRO1";

        [TestCase(SewerProfileMapping.SewerProfileType.Arch, 0.6, 0.0)]
        [TestCase(SewerProfileMapping.SewerProfileType.Circle, 0.0, 0.0)]
        [TestCase(SewerProfileMapping.SewerProfileType.Cunette, 0.2536, 0.0)]
        [TestCase(SewerProfileMapping.SewerProfileType.Egg, 0.6, 0.0)]
        [TestCase(SewerProfileMapping.SewerProfileType.Rectangle, 0.6, 0.0)]
        [TestCase(SewerProfileMapping.SewerProfileType.Trapezoid, 1.0, 2.0)]
        public void GivenSimpleSewerProfileDataRound_WhenCreatingWithFactory_ThenProfileIsCorrectlyReturned(SewerProfileMapping.SewerProfileType sewerProfileType, double expectedHeight, double expectedSlope)
        {
            var profileType = EnumDescriptionAttributeTypeConverter.GetEnumDescription(sewerProfileType);
            var profileGwswElement = GetSewerProfileGwswElement(ProfileId, profileType, "400", "600", "3.0", "1.0");
            CreateProfileAndCheckProperties(profileGwswElement, ProfileId, 0.4, expectedHeight, expectedSlope);
        }

        [Test]
        public void GivenGwswElementWithoutIdDefined_WhenCreatingSewerProfile_ThenLogMessageIsGiven()
        {
            var sewerProfileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString()
            };
            CreateProfileAndCheckForLogMessage(sewerProfileGwswElement, "Cannot import sewer profile(s) without profile id. Please check 'Profiel.csv' for empty profile id's");
        }

        [Test]
        public void GivenGwswElementWithUnrecognizedShapeDefined_WhenCreatingSewerProfile_ThenDefaultProfileIsReturned()
        {
            var expectedProfileId = "MyProfile";
            var sewerProfileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, expectedProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileShape, "UnrecognizedShape", "RND")
                }
            };
            
            var shape = CreateSewerFeature<CrossSectionStandardShapeRound>(sewerProfileGwswElement);
            Assert.IsNotNull(shape);
            Assert.That(shape.Diameter, Is.EqualTo(0.4));
        }

        [Test]
        public void GivenGwswElementWithoutShapeDefined_WhenCreatingSewerProfile_ThenNullValuesIsReturned()
        {
            var expectedProfileId = "MyProfile";
            var sewerProfileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, expectedProfileId, string.Empty)
                }
            };

            var shape = CreateSewerFeature<CrossSectionStandardShapeRound>(sewerProfileGwswElement);
            Assert.IsNotNull(shape);
            Assert.That(shape.Diameter, Is.EqualTo(0.4));
        }

        [Test]
        public void GivenGwswElementWithUnrecognizedShapeDefined_WhenCreatingSewerProfile_ThenLogMessageIsReturned()
        {
            var expectedProfileId = "MyProfile";
            var sewerProfileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, expectedProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileShape, "UnrecognizedShape", "RND")
                }
            };
            CreateProfileAndCheckForLogMessage(sewerProfileGwswElement, "Shape was not defined for sewer profile 'MyProfile' in 'Profiel.csv'. A default round profile with diameter of 400 mm is used for this profile.");
        }

        [TestCase(SewerProfileMapping.SewerProfileType.Rectangle)]
        [TestCase(SewerProfileMapping.SewerProfileType.Arch)]
        [TestCase(SewerProfileMapping.SewerProfileType.Circle)]
        [TestCase(SewerProfileMapping.SewerProfileType.Cunette)]
        [TestCase(SewerProfileMapping.SewerProfileType.Egg)]
        [TestCase(SewerProfileMapping.SewerProfileType.Trapezoid)]
        public void GivenValidGwswElementWithoutWidthDefined_WhenCreatingSewerProfile_ThenLogMessageIsReturned(SewerProfileMapping.SewerProfileType sewerProfileType)
        {
            var profileType = EnumDescriptionAttributeTypeConverter.GetEnumDescription(sewerProfileType);
            var sewerProfileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileShape, profileType, string.Empty)
                }
            };
            CreateProfileAndCheckForLogMessage(sewerProfileGwswElement, "Default profile property values are used for this profile.");
        }

        [TestCase(SewerProfileMapping.SewerProfileType.Rectangle)]
        [TestCase(SewerProfileMapping.SewerProfileType.Arch)]
        [TestCase(SewerProfileMapping.SewerProfileType.Trapezoid)]
        public void GivenValidGwswElementWithoutHeightDefined_WhenCreatingSewerProfile_ThenLogMessageIsReturned(SewerProfileMapping.SewerProfileType sewerProfileType)
        {
            var profileType = EnumDescriptionAttributeTypeConverter.GetEnumDescription(sewerProfileType);
            var sewerProfileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileShape, profileType, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "4000", string.Empty)
                }
            };
            CreateProfileAndCheckForLogMessage(sewerProfileGwswElement, "Default profile property values are used for this profile.");
        }
        
        [TestCase(SewerProfileMapping.SewerProfileType.Trapezoid)]
        public void GivenValidGwswElementWithoutSlopeDefined_WhenCreatingSewerProfile_ThenLogMessageIsReturnedForTrapezoidType(SewerProfileMapping.SewerProfileType sewerProfileType)
        {
            var profileType = EnumDescriptionAttributeTypeConverter.GetEnumDescription(sewerProfileType);
            var sewerProfileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileShape, profileType, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "4000", string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "2000", string.Empty)
                }
            };
            CreateProfileAndCheckForLogMessage(sewerProfileGwswElement, "Default profile property values are used for this profile.");
        }

        #region Test helpers

        private static void CreateProfileAndCheckProperties(GwswElement profileGwswElement, string expectedProfileId,
            double expectedWidth, double expectedHeight, double expectedSlope)
        {
            //var element = SewerFeatureFactory.CreateSewerEntities(new List<GwswElement>{ profileGwswElement });
            var createdShape = CreateSewerFeature<CrossSectionStandardShapeBase>(profileGwswElement);
            Assert.IsNotNull(createdShape);
            
            // Check shape and its properties
            var shapeType = createdShape.Type;
            switch (shapeType)
            {
                case CrossSectionStandardShapeType.Round:
                {
                    var csShape = createdShape as CrossSectionStandardShapeRound;
                    Assert.NotNull(csShape);
                    Assert.That(Math.Abs(csShape.Diameter - expectedWidth) < 0.0001);
                    break;
                }
                case CrossSectionStandardShapeType.Egg:
                case CrossSectionStandardShapeType.Rectangle:
                case CrossSectionStandardShapeType.Cunette:
                    CheckWidthHeightBasedShapeProperties(createdShape, expectedWidth, expectedHeight);
                    break;
                case CrossSectionStandardShapeType.Arch:
                {
                    var csShape = createdShape as CrossSectionStandardShapeArch;
                    Assert.NotNull(csShape);
                    Assert.That(Math.Abs(csShape.Width - expectedWidth) < 0.0001);
                    Assert.That(Math.Abs(csShape.Height - expectedHeight) < 0.0001);
                    Assert.That(Math.Abs(csShape.ArcHeight - expectedHeight) < 0.0001);
                    break;
                }
                case CrossSectionStandardShapeType.Trapezium:
                {
                    var csShape = createdShape as CrossSectionStandardShapeTrapezium;
                    Assert.NotNull(csShape);
                    Assert.That(Math.Abs(csShape.BottomWidthB - expectedWidth) < 0.0001);
                    Assert.That(Math.Abs(csShape.MaximumFlowWidth - expectedHeight) < 0.0001);
                    Assert.That(Math.Abs(csShape.Slope - expectedSlope) < 0.0001);
                    break;
                }
            }
        }

        private static void CreateProfileAndCheckForLogMessage(GwswElement sewerProfileGwswElement, string expectedMessage)
        {
            TestHelper.AssertAtLeastOneLogMessagesContains(() => SewerFeatureFactory.CreateSewerEntities(new List<GwswElement>{ sewerProfileGwswElement }),
                expectedMessage);
        }

        private static void CheckWidthHeightBasedShapeProperties(CrossSectionStandardShapeBase shape, double expectedWidth, double expectedHeight)
        {
            var csShape = shape as CrossSectionStandardShapeWidthHeightBase;
            Assert.NotNull(csShape);
            Assert.That(Math.Abs(csShape.Width - expectedWidth) < 0.0001);
            Assert.That(Math.Abs(csShape.Height - expectedHeight) < 0.0001);
        }

        #endregion
    }
}