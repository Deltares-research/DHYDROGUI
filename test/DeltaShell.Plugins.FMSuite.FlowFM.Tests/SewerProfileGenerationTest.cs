using System;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class SewerProfileGenerationTest : SewerFeatureFactoryTestHelper
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



        #region Test helpers

        private static void CreateProfileAndCheckProperties(GwswElement profileGwswElement, string expectedProfileId,
            double expectedWidth, double expectedHeight, double expectedSlope)
        {
            var element = SewerFeatureFactory.CreateInstance(profileGwswElement);
            var sewerProfile = element as CrossSection;
            Assert.NotNull(sewerProfile);

            var csDefinition = sewerProfile.Definition as CrossSectionDefinitionStandard;
            Assert.NotNull(csDefinition);
            Assert.That(csDefinition.Name, Is.EqualTo(expectedProfileId));

            var shapeType = csDefinition.Shape.Type;
            if (shapeType == CrossSectionStandardShapeType.Round)
            {
                var csShape = csDefinition.Shape as CrossSectionStandardShapeRound;
                Assert.NotNull(csShape);
                Assert.That(Math.Abs(csShape.Diameter - expectedWidth) < 0.0001);
            }
            else if(shapeType == CrossSectionStandardShapeType.Egg)
            {
                CheckWidthHeightBasedShapeProperties<CrossSectionStandardShapeEgg>(csDefinition, expectedWidth,expectedHeight);
            }
            else if (shapeType == CrossSectionStandardShapeType.Rectangle)
            {
                CheckWidthHeightBasedShapeProperties<CrossSectionStandardShapeRectangle>(csDefinition, expectedWidth, expectedHeight);
            }
            else if (shapeType == CrossSectionStandardShapeType.Cunette)
            {
                CheckWidthHeightBasedShapeProperties<CrossSectionStandardShapeCunette>(csDefinition, expectedWidth, expectedHeight);
            }
            else if (shapeType == CrossSectionStandardShapeType.Arch)
            {
                var csShape = csDefinition.Shape as CrossSectionStandardShapeArch;
                Assert.NotNull(csShape);
                Assert.That(Math.Abs(csShape.Width - expectedWidth) < 0.0001);
                Assert.That(Math.Abs(csShape.Height - expectedHeight) < 0.0001);
                Assert.That(Math.Abs(csShape.ArcHeight - expectedHeight) < 0.0001);
            }
            else if (shapeType == CrossSectionStandardShapeType.Trapezium)
            {
                var csShape = csDefinition.Shape as CrossSectionStandardShapeTrapezium;
                Assert.NotNull(csShape);
                Assert.That(Math.Abs(csShape.BottomWidthB - expectedWidth) < 0.0001);
                Assert.That(Math.Abs(csShape.MaximumFlowWidth - expectedHeight) < 0.0001);
                Assert.That(Math.Abs(csShape.Slope - expectedSlope) < 0.0001);
            }
        }

        private static void CheckWidthHeightBasedShapeProperties<TShape>(CrossSectionDefinitionStandard csDefinition, double expectedWidth, double expectedHeight) 
            where TShape : CrossSectionStandardShapeWidthHeightBase
        {
            var csShape = csDefinition.Shape as TShape;
            Assert.NotNull(csShape);
            Assert.That(Math.Abs(csShape.Width - expectedWidth) < 0.0001);
            Assert.That(Math.Abs(csShape.Height - expectedHeight) < 0.0001);
        }

        #endregion
    }
}