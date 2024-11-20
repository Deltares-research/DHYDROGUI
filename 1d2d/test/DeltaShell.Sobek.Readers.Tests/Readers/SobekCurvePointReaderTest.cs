using System;
using System.Linq;
using DeltaShell.Sobek.Readers.Readers;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekCurvePointReaderTest
    {
        [Test]
        public void SplitTest()
        {
            string curvePointsText =
                @"BRCH id 'OVK27' cp 1 ct bc" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"15.8704294206971 1.97518284371537 <" + Environment.NewLine +
                @"tble brch" + Environment.NewLine +
                @"" + Environment.NewLine +
                @"BRCH id 'OVK28' cp 1 ct bc " + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"8.09640000248551 53.1286870145263 <" + Environment.NewLine +
                @"tble brch";

            var branchGeometries = new SobekCurvePointReader().Parse(curvePointsText);
            Assert.AreEqual(2, branchGeometries.Count());
        }

        [Test]
        public void SimpleBranch()
        {
            string curvePointsText =
                @"BRCH id 'OVK9' cp 1 ct bc " + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"14.0894738191571 140.671108214563 <" + Environment.NewLine +
                @"158.916647719427 136.305987236561 <" + Environment.NewLine +
                @"tble brch";

            var branchGeometries = new SobekCurvePointReader().Parse(curvePointsText);
            Assert.AreEqual(1, branchGeometries.Count());
            var branchGeometry = branchGeometries.FirstOrDefault();
            Assert.AreEqual("OVK9", branchGeometry.BranchID);
            Assert.AreEqual(2, branchGeometry.CurvingPoints.Count);
            var curvePoint = branchGeometry.CurvingPoints[1];
            Assert.AreEqual(158.916647719427, curvePoint.Location, 1.0e-6);
            Assert.AreEqual(136.305987236561, curvePoint.Angle, 1.0e-6);
        }

        // TOOLS-2233 Reimplement offset check in NetworkCoverage
        [Test]
        public void TableWithScientificNotation()
        {
            string curvePointsText =
                @"BRCH id 'OVK79' cp 1 ct bc " + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"4.01218851156465e-03 18.152706106776E-03 <" + Environment.NewLine +
                @"64.9873479287832 17.260552556273 <" + Environment.NewLine +
                @"152.7634048979 15.7253094362602 <" + Environment.NewLine +
                @"176.029776739941 15.7526790331388 <" + Environment.NewLine +
                @"192.040370741753 32.7707795865409 <" + Environment.NewLine +
                @"tble brch";

            var branchGeometries = new SobekCurvePointReader().Parse(curvePointsText);
            var branchGeometry = branchGeometries.FirstOrDefault();
            Assert.AreEqual(5, branchGeometry.CurvingPoints.Count);
            var curvePoint = branchGeometry.CurvingPoints[0];
            Assert.AreEqual(4.01218851156465e-03, curvePoint.Location, 1.0e-6);
            Assert.AreEqual(18.152706106776E-03, curvePoint.Angle, 1.0e-6);
        }

        [Test]
        public void SobekReRijn301Betuwe()
        {
            string source =
                @"BRCH id 'RT2_012' cp 1 ct bc 'Branch Curving Points' PDIN 0 0 '' pdin CLTT 'Location [m]' 'Angle [deg]' cltt CLID '(null)' '(null)' clid TBLE " + Environment.NewLine +
                @"10 340 < " + Environment.NewLine +
                @"2450 340 < " + Environment.NewLine +
                @"2550 306 < " + Environment.NewLine +
                @"10000 306 < " + Environment.NewLine +
                @"tble" + Environment.NewLine +
                 @"brch";
            var branchGeometries = new SobekCurvePointReader().Parse(source);
            var branchGeometry = branchGeometries.FirstOrDefault();
            Assert.AreEqual(4, branchGeometry.CurvingPoints.Count);
        }

        [Test]
        public void BranchWithoutCurpePoints()
        {
            // SobekRe only
            // cp 0 means 0 curvepoints. Geometry will be determined by start and end node. Where
            // length from deftop.1 will be used
            const string source = @"BRCH id 'MM1_6' cp 0 ct bc brch";
            var branchGeometries = new SobekCurvePointReader().Parse(source);
            var branchGeometry = branchGeometries.FirstOrDefault();
            Assert.IsNotNull(branchGeometry);
            Assert.AreEqual(0, branchGeometry.CurvingPoints.Count);
        }
    }
}
