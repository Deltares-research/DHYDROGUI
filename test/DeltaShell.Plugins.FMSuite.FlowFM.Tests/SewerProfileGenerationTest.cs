using System;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class SewerProfileGenerationTest : SewerFeatureFactoryTestHelper
    {
        private const string ProfileId = "PRO1";

        [Test]
        public void GivenSimpleSewerProfileDataRound_WhenCreatingWithFactory_ThenProfileIsCorrectlyReturned()
        {
            var profileGwswElement = GetSewerProfileGwswElement(ProfileId, "RND", "1400", null, null, null);
            CreateProfileAndCheckProperties<CrossSectionStandardShapeRound>(profileGwswElement, ProfileId, 1.4, 0.0, 0.0);
        }

        [Test]
        public void GivenSimpleSewerProfileDataEgg_WhenCreatingWithFactory_ThenProfileIsCorrectlyReturned()
        {
            var profileGwswElement = GetSewerProfileGwswElement(ProfileId, "EIV", "400", null, null, null);
            CreateProfileAndCheckProperties<CrossSectionStandardShapeEgg>(profileGwswElement, ProfileId, 0.4, 0.6, 0.0);
        }

        [Test]
        public void GivenSimpleSewerProfileDataArch_WhenCreatingWithFactory_ThenProfileIsCorrectlyReturned()
        {
            var profileGwswElement = GetSewerProfileGwswElement(ProfileId, "HEU", "400", "600", "3.0", "1.0");
            CreateProfileAndCheckProperties<CrossSectionStandardShapeArch>(profileGwswElement, ProfileId, 0.4, 0.6, 0.0);
        }

        [Test]
        public void GivenSimpleSewerProfileDataCunette_WhenCreatingWithFactory_ThenProfileIsCorrectlyReturned()
        {
            var profileGwswElement = GetSewerProfileGwswElement(ProfileId, "MVR", "400", null, null, null);
            CreateProfileAndCheckProperties<CrossSectionStandardShapeCunette>(profileGwswElement, ProfileId, 0.4, 0.2536, 0.0);
        }

        [Test]
        public void GivenSimpleSewerProfileDataRectangle_WhenCreatingWithFactory_ThenProfileIsCorrectlyReturned()
        {
            var profileGwswElement = GetSewerProfileGwswElement(ProfileId, "RHK", "400", "600", null, null);
            CreateProfileAndCheckProperties<CrossSectionStandardShapeRectangle>(profileGwswElement, ProfileId, 0.4, 0.6, 0.0);
        }

        [Test]
        public void GivenSimpleSewerProfileDataTrapezoid_WhenCreatingWithFactory_ThenProfileIsCorrectlyReturned()
        {
            var profileGwswElement = GetSewerProfileGwswElement(ProfileId, "TPZ", "400", "600", "3.0", "1.0");
            CreateProfileAndCheckProperties<CrossSectionStandardShapeTrapezium>(profileGwswElement, ProfileId, 0.4, 1.0, 2.0);
        }

        private static void CreateProfileAndCheckProperties<T>(GwswElement profileGwswElement, string expectedProfileId,
            double expectedWidth, double expectedHeight, double expectedSlope)
            where T : CrossSectionStandardShapeBase
        {
            var element = SewerFeatureFactory.CreateInstance(profileGwswElement);
            var sewerProfile = element as CrossSection;
            Assert.NotNull(sewerProfile);

            var csDefinition = sewerProfile.Definition as CrossSectionDefinitionStandard;
            Assert.NotNull(csDefinition);
            Assert.That(csDefinition.Name, Is.EqualTo(expectedProfileId));
            
            if (typeof(T) == typeof(CrossSectionStandardShapeRound))
            {
                var csShape = csDefinition.Shape as CrossSectionStandardShapeRound;
                Assert.NotNull(csShape);
                Assert.That(Math.Abs(csShape.Diameter - expectedWidth) < 0.0001);
            }
            else if(typeof(T) == typeof(CrossSectionStandardShapeEgg))
            {
                CheckWidthHeightBasedShapeProperties<CrossSectionStandardShapeEgg>(csDefinition, expectedWidth,expectedHeight);
            }
            else if (typeof(T) == typeof(CrossSectionStandardShapeRectangle))
            {
                CheckWidthHeightBasedShapeProperties<CrossSectionStandardShapeRectangle>(csDefinition, expectedWidth, expectedHeight);
            }
            else if (typeof(T) == typeof(CrossSectionStandardShapeCunette))
            {
                CheckWidthHeightBasedShapeProperties<CrossSectionStandardShapeCunette>(csDefinition, expectedWidth, expectedHeight);
            }
            else if (typeof(T) == typeof(CrossSectionStandardShapeArch))
            {
                var csShape = csDefinition.Shape as CrossSectionStandardShapeArch;
                Assert.NotNull(csShape);
                Assert.That(Math.Abs(csShape.Width - expectedWidth) < 0.0001);
                Assert.That(Math.Abs(csShape.Height - expectedHeight) < 0.0001);
                Assert.That(Math.Abs(csShape.ArcHeight - expectedHeight) < 0.0001);
            }
            else if (typeof(T) == typeof(CrossSectionStandardShapeTrapezium))
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
    }
}