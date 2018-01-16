using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using NUnit.Framework;
using Rhino.Mocks;

namespace DelftTools.Hydro.Tests.Helpers
{
    [TestFixture]
    public class CrossSectionValidatorTest
    {
        [TestCase(20.0, false)]
        [TestCase(45.0, true)]
        [TestCase(50.0, false)]
        public void GivenCrossSectionDefinitionZWWithOnlyMainSection_WhenValidatingSections_ThenMainSectionIsValidWhenItsWidthIsEqualToMaxFlowWidth(double mainSectionWidth, bool expectedResult)
        {
            var csdZw = GetSimpleCrossSectionDefinitionZw();

            csdZw.AddSection(new CrossSectionSectionType { Name = CrossSectionDefinitionZW.MainSectionName }, mainSectionWidth);
            Assert.That(CrossSectionValidator.IsCrossSectionSectionValid(csdZw), Is.EqualTo(expectedResult));
        }

        private static CrossSectionDefinitionZW GetSimpleCrossSectionDefinitionZw()
        {
            var csdZw = new CrossSectionDefinitionZW();
            csdZw.ZWDataTable.AddCrossSectionZWRow(10.0, 50.0, 5.0);
            csdZw.ZWDataTable.AddCrossSectionZWRow(2.0, 30.0, 2.0);
            Assert.That(csdZw.FlowWidth(), Is.EqualTo(45.0));
            return csdZw;
        }

        [TestCase(20.0, 20.0, false)]
        [TestCase(0.0, 20.0, false)]
        [TestCase(20.0, 0.0, false)]
        [TestCase(45.0, 0.0, true)]
        [TestCase(0.0, 45.0, true)]
        [TestCase(30.0, 15.0, true)]
        [TestCase(45.0, 5.0, false)]
        public void GivenCrossSectionDefinitionZWWithMainSectionAndFloodPlain1_WhenValidatingSections_ThenSectionsAreValidWhenTheirTotalWidthIsEqualToMaxFlowWidth(double mainSectionWidth, double floodPlain1Width, bool expectedResult)
        {
            var csdZw = GetSimpleCrossSectionDefinitionZw();

            csdZw.AddSection(new CrossSectionSectionType { Name = CrossSectionDefinitionZW.MainSectionName }, mainSectionWidth);
            csdZw.AddSection(new CrossSectionSectionType { Name = CrossSectionDefinitionZW.Floodplain1SectionTypeName }, floodPlain1Width);

            Assert.That(CrossSectionValidator.IsCrossSectionSectionValid(csdZw), Is.EqualTo(expectedResult));
        }

        [TestCase(10.0, 20.0, 10.0, false)]
        [TestCase(10.0, 20.0, 0.0, false)]
        [TestCase(10.0, 0.0, 10.0, false)]
        [TestCase(0.0, 20.0, 10.0, false)]
        [TestCase(15.0, 20.0, 10.0, true)]
        [TestCase(15.0, 30.0, 0.0, true)]
        [TestCase(15.0, 0.0, 30.0, true)]
        [TestCase(0.0, 35.0, 10.0, true)]
        [TestCase(45.0, 0.0, 0.0, true)]
        [TestCase(0.0, 45.0, 0.0, true)]
        [TestCase(0.0, 0.0, 45.0, true)]
        [TestCase(10.0, 20.0, 45.0, false)]
        public void GivenCrossSectionDefinitionZWWithThreeSectionsDefined_WhenValidatingSections_ThenSectionsAreValidWhenTheirTotalWidthIsEqualToMaxFlowWidth(double mainSectionWidth, double floodPlain1Width, double floodPlain2Width, bool expectedResult)
        {
            var csdZw = GetSimpleCrossSectionDefinitionZw();

            csdZw.AddSection(new CrossSectionSectionType { Name = CrossSectionDefinitionZW.MainSectionName }, mainSectionWidth);
            csdZw.AddSection(new CrossSectionSectionType { Name = CrossSectionDefinitionZW.Floodplain1SectionTypeName }, floodPlain1Width);
            csdZw.AddSection(new CrossSectionSectionType { Name = CrossSectionDefinitionZW.Floodplain2SectionTypeName }, floodPlain2Width);

            Assert.That(CrossSectionValidator.IsCrossSectionSectionValid(csdZw), Is.EqualTo(expectedResult));
        }

    [Test]
        public void CrossSectionTypeTabulatedZWZeroWidthOptions()
        {
            // cross section of ZW type is valid if:
            // - no zero width is present [1]
            // - one zero width is present at the lowest or highest point [2]
            // - two zero widths are present: lowest and highest point (with at least one point in between) [3]
            var crossSectionDefinitionZw = new CrossSectionDefinitionZW();
            crossSectionDefinitionZw.ZWDataTable.AddCrossSectionZWRow(10.0, 5.0, 0.0);
            crossSectionDefinitionZw.ZWDataTable.AddCrossSectionZWRow( 2.0, 3.0, 0.0);
            Assert.IsTrue(CrossSectionValidator.IsFlowProfileValid(crossSectionDefinitionZw));  // [1]

            crossSectionDefinitionZw.ZWDataTable.Clear();
            crossSectionDefinitionZw.ZWDataTable.AddCrossSectionZWRow( 2.0, 0.0, 0.0);
            crossSectionDefinitionZw.ZWDataTable.AddCrossSectionZWRow(10.0, 5.0, 0.0);
            Assert.IsTrue(CrossSectionValidator.IsFlowProfileValid(crossSectionDefinitionZw));  // [2]

            crossSectionDefinitionZw.ZWDataTable.Clear();
            crossSectionDefinitionZw.ZWDataTable.AddCrossSectionZWRow(10.0, 0.0, 0.0);
            crossSectionDefinitionZw.ZWDataTable.AddCrossSectionZWRow( 2.0, 0.0, 0.0);
            Assert.IsFalse(CrossSectionValidator.IsFlowProfileValid(crossSectionDefinitionZw)); // false: no point in between

            crossSectionDefinitionZw.ZWDataTable.AddCrossSectionZWRow( 6.0, 10.0, 0.0);         // [3]
            Assert.IsTrue(CrossSectionValidator.IsFlowProfileValid(crossSectionDefinitionZw));

            crossSectionDefinitionZw.ZWDataTable.Clear();
            crossSectionDefinitionZw.ZWDataTable.AddCrossSectionZWRow(10.0, 0.0, 0.0);
            crossSectionDefinitionZw.ZWDataTable.AddCrossSectionZWRow( 6.0, 0.0, 0.0);
            crossSectionDefinitionZw.ZWDataTable.AddCrossSectionZWRow( 2.0, 0.0, 0.0);
            crossSectionDefinitionZw.ZWDataTable.AddCrossSectionZWRow(4.0, 10.0, 0.0);
            Assert.IsFalse(CrossSectionValidator.IsFlowProfileValid(crossSectionDefinitionZw));  // false: more than 2 zero widths
        }

        [Test]
        public void CrossSectionTypeZWShouldOnlyBeAcceptedOnOpenBranch()
        {
            var mocks = new MockRepository();
            var openBranch = mocks.Stub<IChannel>();
            var enclosedBranch = mocks.Stub<IPipe>();
            string errorMessage;

            var crossSection = new CrossSection(new CrossSectionDefinitionZW()) { Branch = openBranch };
            Assert.IsTrue(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSection, out errorMessage));
            Assert.AreEqual("", errorMessage);

            var crossSection2 = new CrossSection(new CrossSectionDefinitionZW()) { Branch = enclosedBranch };
            Assert.IsFalse(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSection2, out errorMessage));
            Assert.AreEqual("Cross-sections on enclosed branches are not supported.", errorMessage);

            // ZW cross-section as proxy should give exact same results
            var proxy = new CrossSectionDefinitionProxy(new CrossSectionDefinitionZW());
            var crossSectionWithProxy1 = new CrossSection(proxy) { Branch = openBranch };
            Assert.IsTrue(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSectionWithProxy1, out errorMessage));
            Assert.AreEqual("", errorMessage);

            var crossSectionWithProxy2 = new CrossSection(proxy) { Branch = enclosedBranch };
            Assert.IsFalse(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSectionWithProxy2, out errorMessage));
            Assert.AreEqual("Cross-sections on enclosed branches are not supported.", errorMessage);
        }

        [Test]
        public void CrossSectionTypeYZShouldOnlyBeAcceptedOnOpenBranch()
        {
            var mocks = new MockRepository();
            var openBranch = mocks.Stub<IChannel>();
            var enclosedBranch = mocks.Stub<IPipe>();
            string errorMessage;

            var crossSection = new CrossSection(new CrossSectionDefinitionYZ()) { Branch = openBranch };
            Assert.IsTrue(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSection, out errorMessage));
            Assert.AreEqual("", errorMessage);

            var crossSection2 = new CrossSection(new CrossSectionDefinitionYZ()) { Branch = enclosedBranch };
            Assert.IsFalse(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSection2, out errorMessage));
            Assert.AreEqual("Cross-sections on enclosed branches are not supported.", errorMessage);

            // YZ cross-section as proxy should give exact same results.
            var proxy = new CrossSectionDefinitionProxy(new CrossSectionDefinitionYZ());
            var crossSectionWithProxy1 = new CrossSection(proxy) { Branch = openBranch };
            Assert.IsTrue(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSectionWithProxy1, out errorMessage));
            Assert.AreEqual("", errorMessage);

            var crossSectionWithProxy2 = new CrossSection(proxy) { Branch = enclosedBranch };
            Assert.IsFalse(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSectionWithProxy2, out errorMessage));
            Assert.AreEqual("Cross-sections on enclosed branches are not supported.", errorMessage);
        }

        [Test]
        public void CrossSectionTypeXYZShouldOnlyBeAcceptedOnOpenBranch()
        {
            var mocks = new MockRepository();
            var openBranch = mocks.Stub<IChannel>();
            var enclosedBranch = mocks.Stub<IPipe>();
            string errormessage;

            var crossSection = new CrossSection(new CrossSectionDefinitionXYZ()) { Branch = openBranch };
            Assert.IsTrue(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSection, out errormessage));
            Assert.AreEqual("", errormessage);

            var crossSection2 = new CrossSection(new CrossSectionDefinitionXYZ()) { Branch = enclosedBranch };
            Assert.IsFalse(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSection2, out errormessage));
            Assert.AreEqual("Cross-sections on enclosed branches are not supported.", errormessage);

            // XYZ cross-section as proxy should give exact same results.
            var proxy = new CrossSectionDefinitionProxy(new CrossSectionDefinitionXYZ());
            var crossSectionWithProxy1 = new CrossSection(proxy) { Branch = openBranch };
            Assert.IsTrue(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSectionWithProxy1, out errormessage));
            Assert.AreEqual("", errormessage);

            var crossSectionWithProxy2 = new CrossSection(proxy) { Branch = enclosedBranch };
            Assert.IsFalse(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSectionWithProxy2, out errormessage));
            Assert.AreEqual("Cross-sections on enclosed branches are not supported.", errormessage);
        }

        [Test]
        public void CrossSectionTypeStandardTrapeziumhouldOnlyBeAcceptedOnOpenBranch()
        {
            var mocks = new MockRepository();
            var openBranch = mocks.Stub<IChannel>();
            var enclosedBranch = mocks.Stub<IPipe>();
            string errorMessage;

            var crossSection = new CrossSection(new CrossSectionDefinitionStandard { ShapeType = CrossSectionStandardShapeType.Trapezium }) { Branch = openBranch };
            Assert.IsTrue(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSection, out errorMessage));
            Assert.AreEqual("", errorMessage);

            var crossSection2 = new CrossSection(new CrossSectionDefinitionStandard { ShapeType = CrossSectionStandardShapeType.Trapezium }) { Branch = enclosedBranch };
            Assert.IsFalse(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSection2, out errorMessage));
            Assert.AreEqual("Cross-sections on enclosed branches are not supported.", errorMessage);

            // Trapezium as proxy should give exact same results.
            var proxy = new CrossSectionDefinitionProxy(new CrossSectionDefinitionStandard { ShapeType = CrossSectionStandardShapeType.Trapezium });
            var crossSectionWithProxy1 = new CrossSection(proxy) { Branch = openBranch };
            Assert.IsTrue(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSectionWithProxy1, out errorMessage));
            Assert.AreEqual("", errorMessage);

            var crossSectionWithProxy2 = new CrossSection(proxy) { Branch = enclosedBranch };
            Assert.IsFalse(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSectionWithProxy2, out errorMessage));
            Assert.AreEqual("Cross-sections on enclosed branches are not supported.", errorMessage);
        }

        [Test]
        public void CrossSectionTypeStandardRectangleShouldBeAcceptedOnOpenAndClosedBranches()
        {
            var mocks = new MockRepository();
            var openBranch = mocks.Stub<IChannel>();
            var enclosedBranch = mocks.Stub<IPipe>();
            string errorMessage;

            var crossSection = new CrossSection(new CrossSectionDefinitionStandard { ShapeType = CrossSectionStandardShapeType.Rectangle }) { Branch = openBranch };
            Assert.IsTrue(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSection, out errorMessage));
            Assert.AreEqual("", errorMessage);

            var crossSection2 = new CrossSection(new CrossSectionDefinitionStandard { ShapeType = CrossSectionStandardShapeType.Rectangle }) { Branch = enclosedBranch };
            Assert.IsTrue(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSection2, out errorMessage));
            Assert.AreEqual("", errorMessage);

            // Rectangle as proxy should give exact same results.
            var proxy = new CrossSectionDefinitionProxy(new CrossSectionDefinitionStandard { ShapeType = CrossSectionStandardShapeType.Rectangle });
            var crossSectionWithProxy1 = new CrossSection(proxy) { Branch = openBranch };
            Assert.IsTrue(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSectionWithProxy1, out errorMessage));
            Assert.AreEqual("", errorMessage);

            var crossSectionWithProxy2 = new CrossSection(proxy) { Branch = enclosedBranch };
            Assert.IsTrue(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSectionWithProxy2, out errorMessage));
            Assert.AreEqual("", errorMessage);
        }

        [Test]
        public void CrossSectionTypeStandardEllipticalShouldBeAcceptedOnOpenAndClosedBranches()
        {
            var mocks = new MockRepository();
            var openBranch = mocks.Stub<IChannel>();
            var enclosedBranch = mocks.Stub<IPipe>();
            string errorMessage;

            var crossSection = new CrossSection(new CrossSectionDefinitionStandard { ShapeType = CrossSectionStandardShapeType.Elliptical }) { Branch = openBranch };
            Assert.IsTrue(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSection, out errorMessage));
            Assert.AreEqual("", errorMessage);

            var crossSection2 = new CrossSection(new CrossSectionDefinitionStandard { ShapeType = CrossSectionStandardShapeType.Elliptical }) { Branch = enclosedBranch };
            Assert.IsTrue(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSection2, out errorMessage));
            Assert.AreEqual("", errorMessage);

            // Elliptical as proxy should give exact same results.
            var proxy = new CrossSectionDefinitionProxy(new CrossSectionDefinitionStandard { ShapeType = CrossSectionStandardShapeType.Elliptical });
            var crossSectionWithProxy1 = new CrossSection(proxy) { Branch = openBranch };
            Assert.IsTrue(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSectionWithProxy1, out errorMessage));
            Assert.AreEqual("", errorMessage);

            var crossSectionWithProxy2 = new CrossSection(proxy) { Branch = enclosedBranch };
            Assert.IsTrue(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSectionWithProxy2, out errorMessage));
            Assert.AreEqual("", errorMessage);
        }

        [Test]
        public void CrossSectionTypeStandardCunnetteShouldBeAcceptedOnOpenAndClosedBranches()
        {
            var mocks = new MockRepository();
            var openBranch = mocks.Stub<IChannel>();
            var enclosedBranch = mocks.Stub<IPipe>();
            string errorMessage;

            var crossSection = new CrossSection(new CrossSectionDefinitionStandard { ShapeType = CrossSectionStandardShapeType.Cunette }) { Branch = openBranch };
            Assert.IsTrue(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSection, out errorMessage));
            Assert.AreEqual("", errorMessage);

            var crossSection2 = new CrossSection(new CrossSectionDefinitionStandard { ShapeType = CrossSectionStandardShapeType.Cunette }) { Branch = enclosedBranch };
            Assert.IsTrue(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSection2, out errorMessage));
            Assert.AreEqual("", errorMessage);

            // Cunette as proxy should give exact same results.
            var proxy = new CrossSectionDefinitionProxy(new CrossSectionDefinitionStandard { ShapeType = CrossSectionStandardShapeType.Cunette });
            var crossSectionWithProxy1 = new CrossSection(proxy) { Branch = openBranch };
            Assert.IsTrue(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSectionWithProxy1, out errorMessage));
            Assert.AreEqual("", errorMessage);

            var crossSectionWithProxy2 = new CrossSection(proxy) { Branch = enclosedBranch };
            Assert.IsTrue(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSectionWithProxy2, out errorMessage));
            Assert.AreEqual("", errorMessage);
        }

        [Test]
        public void CrossSectionTypeStandardSteelCunnetteShouldBeAcceptedOnOpenAndClosedBranches()
        {
            var mocks = new MockRepository();
            var openBranch = mocks.Stub<IChannel>();
            var enclosedBranch = mocks.Stub<IPipe>();
            string errorMessage;

            var crossSection = new CrossSection(new CrossSectionDefinitionStandard { ShapeType = CrossSectionStandardShapeType.SteelCunette }) { Branch = openBranch };
            Assert.IsTrue(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSection, out errorMessage));
            Assert.AreEqual("", errorMessage);

            var crossSection2 = new CrossSection(new CrossSectionDefinitionStandard { ShapeType = CrossSectionStandardShapeType.SteelCunette }) { Branch = enclosedBranch };
            Assert.IsTrue(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSection2, out errorMessage));
            Assert.AreEqual("", errorMessage);

            // SteelCunette as proxy should give exact same results.
            var proxy = new CrossSectionDefinitionProxy(new CrossSectionDefinitionStandard { ShapeType = CrossSectionStandardShapeType.SteelCunette });
            var crossSectionWithProxy1 = new CrossSection(proxy) { Branch = openBranch };
            Assert.IsTrue(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSectionWithProxy1, out errorMessage));
            Assert.AreEqual("", errorMessage);

            var crossSectionWithProxy2 = new CrossSection(proxy) { Branch = enclosedBranch };
            Assert.IsTrue(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSectionWithProxy2, out errorMessage));
            Assert.AreEqual("", errorMessage);
        }

        [Test]
        public void CrossSectionTypeStandardArchShouldBeAcceptedOnOpenAndClosedBranches()
        {
            var mocks = new MockRepository();
            var openBranch = mocks.Stub<IChannel>();
            var enclosedBranch = mocks.Stub<IPipe>();
            string errorMessage;

            var crossSection = new CrossSection(new CrossSectionDefinitionStandard { ShapeType = CrossSectionStandardShapeType.Arch }) { Branch = openBranch };
            Assert.IsTrue(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSection, out errorMessage));
            Assert.AreEqual("",errorMessage);

            var crossSection2 = new CrossSection(new CrossSectionDefinitionStandard { ShapeType = CrossSectionStandardShapeType.Arch }) { Branch = enclosedBranch };
            Assert.IsTrue(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSection2, out errorMessage));
            Assert.AreEqual("", errorMessage);

            // Arch as proxy should give exact same results.
            var proxy = new CrossSectionDefinitionProxy(new CrossSectionDefinitionStandard { ShapeType = CrossSectionStandardShapeType.Arch });
            var crossSectionWithProxy1 = new CrossSection(proxy) { Branch = openBranch };
            Assert.IsTrue(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSectionWithProxy1, out errorMessage));
            Assert.AreEqual("", errorMessage);

            var crossSectionWithProxy2 = new CrossSection(proxy) { Branch = enclosedBranch };
            Assert.IsTrue(CrossSectionValidator.IsCrossSectionAllowedOnBranch(crossSectionWithProxy2, out errorMessage));
            Assert.AreEqual("", errorMessage);
        }
    }
}
