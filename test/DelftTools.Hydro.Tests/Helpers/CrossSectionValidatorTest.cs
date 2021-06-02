using System;
using System.Collections.Generic;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Tests.TestObjects;
using DelftTools.Utils.Reflection;
using GeoAPI.Geometries;
using NUnit.Framework;
using Rhino.Mocks;

namespace DelftTools.Hydro.Tests.Helpers
{
    [TestFixture]
    public class CrossSectionValidatorTest
    {
        private readonly Func<ICrossSectionDefinition, bool> checkSectionsTotalWidth = csd => CrossSectionValidator.AreCrossSectionsEqualToTheFlowWidth(csd);
        private readonly Func<ICrossSectionDefinition, bool> checkFloodPlain1AndFloodPlain2 = csd => CrossSectionValidator.AreFloodPlain1AndFloodPlain2WidthsValid(csd);

        #region CrossSectionDefinition SectionWidths

        [TestCase(false)]
        [TestCase(true)]
        public void GivenCrossSectionDefinitionWithoutSections_WhenValidatingSections_ThenSectionsAreAlwaysValid(bool useCsdProxy)
        {
            var csDef = GetSimpleCrossSectionDefinition();

            var validationResult = useCsdProxy
                ? ValidateWithProxyCrossSectionDefinition(csDef, checkSectionsTotalWidth)
                : checkSectionsTotalWidth(csDef);

            Assert.That(validationResult, Is.True);
        }

        [TestCase(20.0, false, false)]
        [TestCase(45.0, true, false)]
        [TestCase(50.0, false, false)]
        [TestCase(20.0, false, true)]
        [TestCase(45.0, true, true)]
        [TestCase(50.0, false, true)]
        public void GivenCrossSectionDefinitionWithOnlyMainSection_WhenValidatingSections_ThenMainSectionIsValidWhenItsWidthIsEqualToMaxFlowWidth(double mainSectionWidth, bool expectedResult, bool useCsdProxy)
        {
            var csDef = GetSimpleCrossSectionDefinition();
            csDef.AddSection(new CrossSectionSectionType { Name = RoughnessDataSet.MainSectionTypeName }, mainSectionWidth);

            var validationResult = useCsdProxy 
                ? ValidateWithProxyCrossSectionDefinition(csDef, checkSectionsTotalWidth) 
                : checkSectionsTotalWidth(csDef);

            Assert.That(validationResult, Is.EqualTo(expectedResult));
        }

        [TestCase(20.0, 20.0, false, false)]
        [TestCase(0.0, 20.0, false, false)]
        [TestCase(20.0, 0.0, false, false)]
        [TestCase(45.0, 0.0, true, false)]
        [TestCase(0.0, 45.0, true, false)]
        [TestCase(30.0, 15.0, true, false)]
        [TestCase(45.0, 5.0, false, false)]
        [TestCase(20.0, 20.0, false, true)]
        [TestCase(0.0, 20.0, false, true)]
        [TestCase(20.0, 0.0, false, true)]
        [TestCase(45.0, 0.0, true, true)]
        [TestCase(0.0, 45.0, true, true)]
        [TestCase(30.0, 15.0, true, true)]
        [TestCase(45.0, 5.0, false, true)]
        public void GivenCrossSectionDefinitionWithMainSectionAndFloodPlain1_WhenValidatingSections_ThenSectionsAreValidWhenTheirTotalWidthIsEqualToMaxFlowWidth(double mainSectionWidth, double floodPlain1Width, bool expectedResult, bool useCsdProxy)
        {
            var csDef = GetSimpleCrossSectionDefinition();
            csDef.AddSection(new CrossSectionSectionType { Name = RoughnessDataSet.MainSectionTypeName }, mainSectionWidth);
            csDef.AddSection(new CrossSectionSectionType { Name = RoughnessDataSet.Floodplain1SectionTypeName }, floodPlain1Width);

            var validationResult = useCsdProxy
                ? ValidateWithProxyCrossSectionDefinition(csDef, checkSectionsTotalWidth)
                : checkSectionsTotalWidth(csDef);

            Assert.That(validationResult, Is.EqualTo(expectedResult));
        }

        [TestCase(10.0, 20.0, 10.0, false, false)]
        [TestCase(10.0, 20.0, 0.0, false, false)]
        [TestCase(10.0, 0.0, 10.0, false, false)]
        [TestCase(0.0, 20.0, 10.0, false, false)]
        [TestCase(15.0, 20.0, 10.0, true, false)]
        [TestCase(15.0, 30.0, 0.0, true, false)]
        [TestCase(15.0, 0.0, 30.0, true, false)]
        [TestCase(0.0, 35.0, 10.0, true, false)]
        [TestCase(45.0, 0.0, 0.0, true, false)]
        [TestCase(0.0, 45.0, 0.0, true, false)]
        [TestCase(0.0, 0.0, 45.0, true, false)]
        [TestCase(10.0, 20.0, 45.0, false, false)]
        [TestCase(10.0, 20.0, 10.0, false, true)]
        [TestCase(10.0, 20.0, 0.0, false, true)]
        [TestCase(10.0, 0.0, 10.0, false, true)]
        [TestCase(0.0, 20.0, 10.0, false, true)]
        [TestCase(15.0, 20.0, 10.0, true, true)]
        [TestCase(15.0, 30.0, 0.0, true, true)]
        [TestCase(15.0, 0.0, 30.0, true, true)]
        [TestCase(0.0, 35.0, 10.0, true, true)]
        [TestCase(45.0, 0.0, 0.0, true, true)]
        [TestCase(0.0, 45.0, 0.0, true, true)]
        [TestCase(0.0, 0.0, 45.0, true, true)]
        [TestCase(10.0, 20.0, 45.0, false, true)]
        public void GivenCrossSectionDefinitionZWWithThreeSectionsDefined_WhenValidatingSections_ThenSectionsAreValidWhenTheirTotalWidthIsEqualToMaxFlowWidth(double mainSectionWidth, double floodPlain1Width, double floodPlain2Width, bool expectedResult, bool useCsdProxy)
        {
            var csDef = GetSimpleCrossSectionDefinition();
            csDef.AddSection(new CrossSectionSectionType { Name = RoughnessDataSet.MainSectionTypeName }, mainSectionWidth);
            csDef.AddSection(new CrossSectionSectionType { Name = RoughnessDataSet.Floodplain1SectionTypeName }, floodPlain1Width);
            csDef.AddSection(new CrossSectionSectionType { Name = RoughnessDataSet.Floodplain2SectionTypeName }, floodPlain2Width);

            var validationResult = useCsdProxy
                ? ValidateWithProxyCrossSectionDefinition(csDef, checkSectionsTotalWidth)
                : checkSectionsTotalWidth(csDef);

            Assert.That(validationResult, Is.EqualTo(expectedResult));
        }

        #endregion

        #region FloodPlain1 & FloodPlain2 validation

        [TestCase(10.0, 0.0, 3.0, false, false)]
        [TestCase(10.0, 3.0, 0.0, true, false)]
        [TestCase(10.0, 1.0, 3.0, true, false)]
        [TestCase(10.0, 0.0, 3.0, false, true)]
        [TestCase(10.0, 3.0, 0.0, true, true)]
        [TestCase(10.0, 1.0, 3.0, true, true)]
        public void WhenFloodPlain1WidthIsEqualToZeroAndFloodPlain2WidthIsLargerThanZero_ThenTheCrossSectionSectionsAreNotValid(double mainSectionWidth, double floodPlain1Width, double floodPlain2Width, bool expectedResult, bool useCsdProxy)
        {
            var csdZw = GetSimpleCrossSectionDefinitionZw();
            csdZw.AddSection(new CrossSectionSectionType { Name = RoughnessDataSet.MainSectionTypeName }, mainSectionWidth);
            csdZw.AddSection(new CrossSectionSectionType { Name = RoughnessDataSet.Floodplain1SectionTypeName }, floodPlain1Width);
            csdZw.AddSection(new CrossSectionSectionType { Name = RoughnessDataSet.Floodplain2SectionTypeName }, floodPlain2Width);

            var validationResult = useCsdProxy
                ? ValidateWithProxyCrossSectionDefinition(csdZw, checkFloodPlain1AndFloodPlain2)
                : checkFloodPlain1AndFloodPlain2(csdZw);

            Assert.That(validationResult, Is.EqualTo(expectedResult));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void GivenCrossSectionDefinitionWithLessThanThreeSections_WhenValidiatingFloodPlain1AndFloodPlain2_ThenCrossSectionPassesThisValidation(int numberOfSections)
        {
            var mocks = new MockRepository();
            var crossSectionDef = mocks.DynamicMock<ICrossSectionDefinition>();
            crossSectionDef.Expect(cs => cs.Sections.Count).Return(numberOfSections).Repeat.Any();
            mocks.ReplayAll();

            var validationResult = checkFloodPlain1AndFloodPlain2(crossSectionDef);
            Assert.That(validationResult, Is.EqualTo(true));

            mocks.VerifyAll();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void GivenCrossSectionDefinitionYZ_WhenValidiatingFloodPlain1AndFloodPlain2_ThenCrossSectionPassesThisValidation(bool useCsdProxy)
        {
            var crossSectionDefYz = GetSimpleCrossSectionDefinitionZw();
            crossSectionDefYz.AddSection(new CrossSectionSectionType { Name = RoughnessDataSet.MainSectionTypeName }, 10.0);
            crossSectionDefYz.AddSection(new CrossSectionSectionType { Name = RoughnessDataSet.Floodplain1SectionTypeName }, 20.0);
            crossSectionDefYz.AddSection(new CrossSectionSectionType { Name = RoughnessDataSet.Floodplain2SectionTypeName }, 15.0);

            var validationResult = useCsdProxy
                ? ValidateWithProxyCrossSectionDefinition(crossSectionDefYz, checkFloodPlain1AndFloodPlain2)
                : checkSectionsTotalWidth(crossSectionDefYz);

            Assert.IsTrue(validationResult);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void GivenCrossSectionDefinitionStandard_WhenValidiatingFloodPlain1AndFloodPlain2_ThenCrossSectionPassesThisValidation(bool useCsdProxy)
        {
            var crossSectionDefStandard = GetSimpleCrossSectionDefinition();
            crossSectionDefStandard.AddSection(new CrossSectionSectionType {Name = RoughnessDataSet.MainSectionTypeName}, 10.0);
            crossSectionDefStandard.AddSection(new CrossSectionSectionType {Name = RoughnessDataSet.Floodplain1SectionTypeName}, 20.0);
            crossSectionDefStandard.AddSection(new CrossSectionSectionType {Name = RoughnessDataSet.Floodplain2SectionTypeName}, 15.0);

            var validationResult = useCsdProxy
                ? ValidateWithProxyCrossSectionDefinition(crossSectionDefStandard, checkFloodPlain1AndFloodPlain2)
                : checkSectionsTotalWidth(crossSectionDefStandard);

            Assert.IsTrue(validationResult);
        }

        #endregion

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

        private static CrossSectionDefinitionZW GetSimpleCrossSectionDefinitionZw()
        {
            var csdZw = new CrossSectionDefinitionZW();
            csdZw.ZWDataTable.AddCrossSectionZWRow(10.0, 50.0, 5.0);
            csdZw.ZWDataTable.AddCrossSectionZWRow(2.0, 30.0, 2.0);
            Assert.That(csdZw.FlowWidth(), Is.EqualTo(45.0));
            return csdZw;
        }

        private static ICrossSectionDefinition GetSimpleCrossSectionDefinition()
        {
            var csDef = new TestCrossSectionDefinition { Name = "TestCrossSectionDefinition" };

            TypeUtils.SetField(csDef, "profile", new List<Coordinate>()
            {
                new Coordinate(0, 0),
                new Coordinate(15, -10.0),
                new Coordinate(30, -10.0),
                new Coordinate(45.000001, 0) // Note: ensure epsilon of 1e-5 is taken into account
            });

            Assert.That(csDef.FlowWidth(), Is.EqualTo(45.000001));
            return csDef;
        }

        private static bool ValidateWithProxyCrossSectionDefinition(ICrossSectionDefinition crossSectionDefinition, Func<ICrossSectionDefinition, bool> validate)
        {
            var proxy = new CrossSectionDefinitionProxy(crossSectionDefinition);
            return validate(proxy);
        }
    }
}
