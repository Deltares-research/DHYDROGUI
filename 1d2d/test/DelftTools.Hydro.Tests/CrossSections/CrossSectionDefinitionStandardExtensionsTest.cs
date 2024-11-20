using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.Extensions;
using DelftTools.Hydro.CrossSections.StandardShapes;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.CrossSections
{
    [TestFixture]
    public class CrossSectionDefinitionStandardExtensionsTest
    {
        private const double WidthValue = 2.2;
        private const double HeightValue = 1.3;
        private const double ArchHeightValue = 3.3;
        private const double SlopeValue = 2.5;
        private const double BottomWidthValue = 10.0;
        private const double MaxFlowWidthValue = 20.0;

        #region GetDiameter

        [Test]
        public void GivenNullCrossSectionDefinition_WhenGettingProfileDiameter_ThenReturnNaN()
        {
            CrossSectionDefinitionStandard csDef = null;
            Assert.That(csDef.GetProfileDiameter(), Is.EqualTo(double.NaN));
        }

        [Test]
        public void GivenCrossSectionDefinitionWithNullShape_WhenGettingProfileDiameter_ThenReturnNaN()
        {
            var csDef = new CrossSectionDefinitionStandard(null);
            Assert.That(csDef.GetProfileDiameter(), Is.EqualTo(double.NaN));
        }

        [Test]
        public void GivenCrossSectionDefinitionWithNonRoundShape_WhenGettingProfileDiameter_ThenReturnNaN()
        {
            var csDef = GetCsDefArchShape();
            Assert.That(csDef.GetProfileDiameter(), Is.EqualTo(double.NaN));
        }

        [Test]
        public void GivenCrossSectionDefinitionWithRoundShape_WhenGettingProfileDiameter_ThenReturnDiameter()
        {
            var csDef = GetCsDefRoundShape();
            Assert.That(csDef.GetProfileDiameter(), Is.EqualTo(WidthValue));
        }

        #endregion

        #region GetProfileWidth

        [Test]
        public void GivenNullCrossSectionDefinition_WhenGettingProfileWidth_ThenReturnNaN()
        {
            CrossSectionDefinitionStandard csDef = null;
            Assert.That(csDef.GetProfileWidth(), Is.EqualTo(double.NaN));
        }

        [Test]
        public void GivenCrossSectionDefinitionWithNullShape_WhenGettingProfileWidth_ThenReturnNaN()
        {
            var csDef = new CrossSectionDefinitionStandard(null);
            Assert.That(csDef.GetProfileWidth(), Is.EqualTo(double.NaN));
        }

        [Test]
        public void GivenCrossSectionDefinitionWithShapeWithoutWidth_WhenGettingProfileWidth_ThenReturnNaN()
        {
            var csDef = new CrossSectionDefinitionStandard(new CrossSectionStandardShapeCircle());
            Assert.That(csDef.GetProfileWidth(), Is.EqualTo(double.NaN));
        }

        [Test]
        public void GivenCrossSectionDefinitionWithArchShape_WhenGettingProfileWidth_ThenReturnWidth()
        {
            var csDef = GetCsDefArchShape();
            Assert.That(csDef.GetProfileWidth(), Is.EqualTo(WidthValue));
        }

        [Test]
        public void GivenCrossSectionDefinitionWithWidthHeightBaseShape_WhenGettingProfileWidth_ThenReturnWidth()
        {
            var csDef = GetCsDefRectangleShape();
            Assert.That(csDef.GetProfileWidth(), Is.EqualTo(WidthValue));
        }

        #endregion

        #region GetProfileHeight

        [Test]
        public void GivenNullCrossSectionDefinition_WhenGettingProfileHeight_ThenReturnNaN()
        {
            CrossSectionDefinitionStandard csDef = null;
            Assert.That(csDef.GetProfileHeight(), Is.EqualTo(double.NaN));
        }

        [Test]
        public void GivenCrossSectionDefinitionWithNullShape_WhenGettingProfileHeight_ThenReturnNaN()
        {
            var csDef = new CrossSectionDefinitionStandard(null);
            Assert.That(csDef.GetProfileHeight(), Is.EqualTo(double.NaN));
        }

        [Test]
        public void GivenCrossSectionDefinitionWithShapeWithoutHeight_WhenGettingProfileHeight_ThenReturnNaN()
        {
            var csDef = new CrossSectionDefinitionStandard(new CrossSectionStandardShapeCircle());
            Assert.That(csDef.GetProfileHeight(), Is.EqualTo(double.NaN));
        }

        [Test]
        public void GivenCrossSectionDefinitionWithArchShape_WhenGettingProfileHeight_ThenReturnHeight()
        {
            var csDef = new CrossSectionDefinitionStandard(new CrossSectionStandardShapeArch
            {
                Height = HeightValue
            });
            Assert.That(csDef.GetProfileHeight(), Is.EqualTo(HeightValue));
        }

        [Test]
        public void GivenCrossSectionDefinitionWithWidthHeightBaseShape_WhenGettingProfileHeight_ThenReturnHeight()
        {
            var csDef = GetCsDefRectangleShape();
            Assert.That(csDef.GetProfileHeight(), Is.EqualTo(HeightValue));
        }

        #endregion

        #region GetProfileArcHeight

        [Test]
        public void GivenNullCrossSectionDefinition_WhenGettingProfileArchHeight_ThenReturnNaN()
        {
            CrossSectionDefinitionStandard csDef = null;
            Assert.That(csDef.GetProfileArchHeight(), Is.EqualTo(double.NaN));
        }

        [Test]
        public void GivenCrossSectionDefinitionWithNullShape_WhenGettingProfileArchHeight_ThenReturnNaN()
        {
            var csDef = new CrossSectionDefinitionStandard(null);
            Assert.That(csDef.GetProfileArchHeight(), Is.EqualTo(double.NaN));
        }

        [Test]
        public void GivenCrossSectionDefinitionWithNonArchShape_WhenGettingProfileArchHeight_ThenReturnNaN()
        {
            var csDef = GetCsDefRectangleShape();
            Assert.That(csDef.GetProfileArchHeight(), Is.EqualTo(double.NaN));
        }

        [Test]
        public void GivenCrossSectionDefinitionWithShapeWithoutArchHeight_WhenGettingProfileArchHeight_ThenReturnNaN()
        {
            var csDef = new CrossSectionDefinitionStandard(new CrossSectionStandardShapeRectangle());
            Assert.That(csDef.GetProfileArchHeight(), Is.EqualTo(double.NaN));
        }

        [Test]
        public void GivenCrossSectionDefinitionWithArchShape_WhenGettingProfileArchHeight_ThenReturnArchHeight()
        {
            var csDef = GetCsDefArchShape();
            Assert.That(csDef.GetProfileArchHeight(), Is.EqualTo(ArchHeightValue));
        }

        #endregion

        #region GetProfileSlope

        [Test]
        public void GivenNullCrossSectionDefinition_WhenGettingProfileSlope_ThenReturnNaN()
        {
            CrossSectionDefinitionStandard csDef = null;
            Assert.That(csDef.GetProfileSlope(), Is.EqualTo(double.NaN));
        }

        [Test]
        public void GivenCrossSectionDefinitionWithNullShape_WhenGettingProfileSlope_ThenReturnNaN()
        {
            var csDef = new CrossSectionDefinitionStandard(null);
            Assert.That(csDef.GetProfileSlope(), Is.EqualTo(double.NaN));
        }

        [Test]
        public void GivenCrossSectionDefinitionWithNonTrapezoidShape_WhenGettingProfileSlope_ThenReturnNaN()
        {
            var csDef = GetCsDefRectangleShape();
            Assert.That(csDef.GetProfileSlope(), Is.EqualTo(double.NaN));
        }

        [Test]
        public void GivenCrossSectionDefinitionWithShapeWithoutSlope_WhenGettingProfileSlope_ThenReturnNaN()
        {
            var csDef = new CrossSectionDefinitionStandard(new CrossSectionStandardShapeTrapezium());
            Assert.That(csDef.GetProfileSlope(), Is.EqualTo(0.0));
        }

        [Test]
        public void GivenCrossSectionDefinitionTrapezoidWithSlope_WhenGettingProfileSlope_ThenReturnSlope()
        {
            var csDef = GetCsDefTrapezoidShape();
            Assert.That(csDef.GetProfileSlope(), Is.EqualTo(SlopeValue));
        }

        #endregion

        #region GetProfileBottomWidthB

        [Test]
        public void GivenNullCrossSectionDefinition_WhenGettingProfileBottomWidthB_ThenReturnNaN()
        {
            CrossSectionDefinitionStandard csDef = null;
            Assert.That(csDef.GetProfileBottomWidthB(), Is.EqualTo(double.NaN));
        }

        [Test]
        public void GivenCrossSectionDefinitionWithNullShape_WhenGettingProfileBottomWidthB_ThenReturnNaN()
        {
            var csDef = new CrossSectionDefinitionStandard(null);
            Assert.That(csDef.GetProfileBottomWidthB(), Is.EqualTo(double.NaN));
        }

        [Test]
        public void GivenCrossSectionDefinitionWithNonTrapezoidShape_WhenGettingProfileBottomWidthB_ThenReturnNaN()
        {
            var csDef = GetCsDefRectangleShape();
            Assert.That(csDef.GetProfileBottomWidthB(), Is.EqualTo(double.NaN));
        }

        [Test]
        public void GivenCrossSectionDefinitionWithShapeWithoutBottomWidthB_WhenGettingProfileBottomWidthB_ThenReturnNaN()
        {
            var csDef = new CrossSectionDefinitionStandard(new CrossSectionStandardShapeTrapezium());
            Assert.That(csDef.GetProfileBottomWidthB(), Is.EqualTo(0.0));
        }

        [Test]
        public void GivenCrossSectionDefinitionTrapezoidWithBottomWidthB_WhenGettingProfileBottomWidthB_ThenReturnBottomWidthB()
        {
            var csDef = GetCsDefTrapezoidShape();
            Assert.That(csDef.GetProfileBottomWidthB(), Is.EqualTo(BottomWidthValue));
        }

        #endregion

        #region GetProfileMaximumFlowWidth

        [Test]
        public void GivenNullCrossSectionDefinition_WhenGettingProfileMaximumFlowWidth_ThenReturnNaN()
        {
            CrossSectionDefinitionStandard csDef = null;
            Assert.That(csDef.GetProfileMaximumFlowWidth(), Is.EqualTo(double.NaN));
        }

        [Test]
        public void GivenCrossSectionDefinitionWithNullShape_WhenGettingProfileMaximumFlowWidth_ThenReturnNaN()
        {
            var csDef = new CrossSectionDefinitionStandard(null);
            Assert.That(csDef.GetProfileMaximumFlowWidth(), Is.EqualTo(double.NaN));
        }

        [Test]
        public void GivenCrossSectionDefinitionWithNonTrapezoidShape_WhenGettingProfileMaximumFlowWidth_ThenReturnNaN()
        {
            var csDef = GetCsDefRectangleShape();
            Assert.That(csDef.GetProfileMaximumFlowWidth(), Is.EqualTo(double.NaN));
        }

        [Test]
        public void GivenCrossSectionDefinitionWithShapeWithoutMaximumFlowWidth_WhenGettingProfileMaximumFlowWidth_ThenReturnNaN()
        {
            var csDef = new CrossSectionDefinitionStandard(new CrossSectionStandardShapeTrapezium());
            Assert.That(csDef.GetProfileMaximumFlowWidth(), Is.EqualTo(0.0));
        }

        [Test]
        public void GivenCrossSectionDefinitionTrapezoidWithMaximumFlowWidth_WhenGettingProfileMaximumFlowWidth_ThenReturnMaximumFlowWidth()
        {
            var csDef = GetCsDefTrapezoidShape();
            Assert.That(csDef.GetProfileMaximumFlowWidth(), Is.EqualTo(MaxFlowWidthValue));
        }

        #endregion

        #region Test helpers

        private static CrossSectionDefinitionStandard GetCsDefRectangleShape()
        {
            var csDef = new CrossSectionDefinitionStandard(new CrossSectionStandardShapeRectangle
            {
                Width = WidthValue,
                Height = HeightValue
            });
            return csDef;
        }

        private static CrossSectionDefinitionStandard GetCsDefArchShape()
        {
            var csDef = new CrossSectionDefinitionStandard(new CrossSectionStandardShapeArch
            {
                Width = WidthValue,
                Height = HeightValue,
                ArcHeight = ArchHeightValue
            });
            return csDef;
        }

        private static CrossSectionDefinitionStandard GetCsDefTrapezoidShape()
        {
            var csDef = new CrossSectionDefinitionStandard(new CrossSectionStandardShapeTrapezium
            {
                Slope = SlopeValue,
                BottomWidthB = BottomWidthValue,
                MaximumFlowWidth = MaxFlowWidthValue
            });
            return csDef;
        }

        private static CrossSectionDefinitionStandard GetCsDefRoundShape()
        {
            var csDef = new CrossSectionDefinitionStandard(new CrossSectionStandardShapeCircle
            {
                Diameter = WidthValue
            });
            return csDef;
        }

        #endregion
    }
}