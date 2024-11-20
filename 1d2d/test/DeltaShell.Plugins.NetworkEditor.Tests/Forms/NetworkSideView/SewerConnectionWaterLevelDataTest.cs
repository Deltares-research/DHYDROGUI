using System;
using System.Collections.Generic;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.SewerFeatures;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using GeoAPI.Extensions.Coverages;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.NetworkSideView
{
    public class SewerConnectionWaterLevelDataTest
    {
        [Test]
        public void Constructor_SegmentNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new SewerConnectionWaterLevelData(null, 1.0, 3.0, 2.0);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("branchSegment"));
        }

        [Test]
        [TestCaseSource(nameof(GetDoubleValueOutOfRangeCases))]
        public void Constructor_DoubleValueOutOfRange_ThrowsArgumentOutOfRangeException(double bottomLevel, double relativeOffSet, double waterLevel, string expParamName)
        {
            var branchSegment = Substitute.For<INetworkSegment>();

            // Call
            void Call() => new SewerConnectionWaterLevelData(branchSegment, bottomLevel, waterLevel, relativeOffSet);

            // Assert
            var e = Assert.Throws<ArgumentOutOfRangeException>(Call);
            Assert.That(e.ParamName, Is.EqualTo(expParamName));
        }

        [Test]
        [TestCase(-2.0, ValueLocation.BelowSewerConnection, -1.0)]
        [TestCase(-1.0, ValueLocation.InsideSewerConnection, -1.0)]
        [TestCase(0.0, ValueLocation.InsideSewerConnection, 0.0)]
        [TestCase(1.0, ValueLocation.InsideSewerConnection, 1.0)]
        [TestCase(2.0, ValueLocation.AboveSewerConnection, 1.0)]
        public void Constructor_InitializesInstanceCorrectly(double waterLevel, ValueLocation expValueLocation, double expWaterLevelInSewerConnection)
        {
            // Setup
            const double sewerConnectionBottomLevel = -1.0;
            const double sewerConnectionHeight = 2.0;
            const double relativeOffset = 5.0;

            var segment = Substitute.For<INetworkSegment>();
            var sewerConnection = Substitute.For<ISewerConnection>();
            ICrossSection crossSection = CreateCrossSection(sewerConnectionHeight);

            segment.Branch = sewerConnection;
            sewerConnection.CrossSection = crossSection;

            // Call
            var data = new SewerConnectionWaterLevelData(segment, sewerConnectionBottomLevel, waterLevel, relativeOffset);

            // Assert
            Assert.That(data.BranchSegment, Is.SameAs(segment));
            Assert.That(data.SewerConnection, Is.SameAs(sewerConnection));
            Assert.That(data.SewerConnectionBottomLevel, Is.EqualTo(sewerConnectionBottomLevel));
            Assert.That(data.SewerConnectionTopLevel, Is.EqualTo(sewerConnectionBottomLevel + sewerConnectionHeight));
            Assert.That(data.WaterLevelInSewerConnection, Is.EqualTo(expWaterLevelInSewerConnection));
            Assert.That(data.ValueLocation, Is.EqualTo(expValueLocation));
            Assert.That(data.WaterLevel, Is.EqualTo(waterLevel));
            Assert.That(data.RelativeOffset, Is.EqualTo(relativeOffset));
        }

        private static ICrossSection CreateCrossSection(double crossSectionDefinitionHeight)
        {
            var crossSection = Substitute.For<ICrossSection>();
            var crossSectionDefinition = Substitute.For<ICrossSectionDefinition>();

            crossSection.Definition.Returns(crossSectionDefinition);
            crossSectionDefinition.HighestPoint.Returns(crossSectionDefinitionHeight);

            return crossSection;
        }

        private static IEnumerable<TestCaseData> GetDoubleValueOutOfRangeCases()
        {
            yield return new TestCaseData(double.NaN, 2.0, 3.0, "bottomLevel");
            yield return new TestCaseData(double.NegativeInfinity, 2.0, 3.0, "bottomLevel");
            yield return new TestCaseData(double.PositiveInfinity, 2.0, 3.0, "bottomLevel");

            yield return new TestCaseData(1.0, -2.0, 3.0, "relativeOffSet");
            yield return new TestCaseData(1.0, double.NaN, 3.0, "relativeOffSet");
            yield return new TestCaseData(1.0, double.NegativeInfinity, 3.0, "relativeOffSet");
            yield return new TestCaseData(1.0, double.PositiveInfinity, 3.0, "relativeOffSet");

            yield return new TestCaseData(1.0, 2.0, double.NaN, "waterLevel");
            yield return new TestCaseData(1.0, 2.0, double.NegativeInfinity, "waterLevel");
            yield return new TestCaseData(1.0, 2.0, double.PositiveInfinity, "waterLevel");
        }
    }
}