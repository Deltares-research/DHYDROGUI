using DelftTools.Hydro;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    public class GeometryHelperTest
    {
        [Test]
        public void CalculateGeometryTest()
        {
            BranchGeometry branchGeometry = new BranchGeometry();
            INode startNode = new HydroNode {Geometry = new Point(0.0, 0.0)};
            INode endNode = new HydroNode {Geometry = new Point(100.0, 0.0)};

            IGeometry geometry = GeometryHelper.CalculateGeometry(true, branchGeometry, startNode, endNode);
            Assert.AreEqual(100.0, geometry.Length, 1.0e-6);
            Assert.AreEqual(2, geometry.Coordinates.Length);
        }

        /// <summary>
        /// very simple test data; curvepoint exactly in center branch as written by Sobek 2.xx. This curvepoint
        /// will be eliminated
        /// </summary>
        [Test]
        public void CalculateGeometryWithCurvePointInCenter()
        {
            BranchGeometry branchGeometry = new BranchGeometry();
            branchGeometry.CurvingPoints.Add(new CurvingPoint(2500.0, 90));
            INode startNode = new HydroNode { Geometry = new Point(150000.0, 490000.0) };
            INode endNode = new HydroNode { Geometry = new Point(155000.0, 490000.0) };

            IGeometry geometry = GeometryHelper.CalculateGeometry(true, branchGeometry, startNode, endNode);
            Assert.AreEqual(5000.0, geometry.Length, 1.0e-6);
            Assert.AreEqual(2, geometry.Coordinates.Length);
        }

        /// <summary>
        /// very simple test data; curvepoint exactly in center branch as written by Sobek 2.xx. This curvepoint
        /// will be eliminated
        /// </summary>
        [Test]
        public void CalculateGeometryWithCurvePointOffCenter()
        {
            BranchGeometry branchGeometry = new BranchGeometry();
            branchGeometry.CurvingPoints.Add(new CurvingPoint(2000.0, 90));
            INode startNode = new HydroNode { Geometry = new Point(150000.0, 490000.0) };
            INode endNode = new HydroNode { Geometry = new Point(155000.0, 490000.0) };

            IGeometry geometry = GeometryHelper.CalculateGeometry(true, branchGeometry, startNode, endNode);
            Assert.AreEqual(5000.0, geometry.Length, 1.0e-6);
            Assert.AreEqual(3, geometry.Coordinates.Length);
            Assert.AreEqual(150000.0, geometry.Coordinates[0].X, 1.0e-6);
            Assert.AreEqual(490000.0, geometry.Coordinates[0].Y, 1.0e-6);
            Assert.AreEqual(150000.0 + (2 * 2000.0), geometry.Coordinates[1].X, 1.0e-6);
            Assert.AreEqual(490000.0, geometry.Coordinates[1].Y, 1.0e-6);
            Assert.AreEqual(155000.0, geometry.Coordinates[2].X, 1.0e-6);
            Assert.AreEqual(490000.0, geometry.Coordinates[2].Y, 1.0e-6);
        }

        /// <summary>
        /// TOOLS-2233 related test: Reimplement offset check in NetworkCoverage
        /// chainage check is not the problem but in some cases the length of the channel in the
        /// sobek network.tp file and the calculated geometry did not match.
        /// source BYPASS model branch OVK79, case 2
        /// The channel has a curvepoint near the startnode. Removing this curvepoint also removed the discrepancy 
        /// where the length in network.tp was nearly unchanged -> error must be in GeometryHelper.CalculateGeometry
        /// </summary>
        [Test]
        public void CalculateGeometryWithVwerySmallChainage()
        {
            BranchGeometry branchGeometry = new BranchGeometry();
            branchGeometry.CurvingPoints.Add(new CurvingPoint(4.01218851156465E-03, 18.152706106776));
            branchGeometry.CurvingPoints.Add(new CurvingPoint(64.9873479287832, 17.260552556273));
            branchGeometry.CurvingPoints.Add(new CurvingPoint(152.7634048979, 15.7253094362602));
            branchGeometry.CurvingPoints.Add(new CurvingPoint(176.029776739941, 15.7526790331388));
            branchGeometry.CurvingPoints.Add(new CurvingPoint(192.040370741753, 32.7707795865409));
            INode startNode = new HydroNode { Geometry = new Point(187148.303375, 508462.526375) };
            INode endNode = new HydroNode { Geometry = new Point(187216.302875, 508657.566) };

            IGeometry geometry = GeometryHelper.CalculateGeometry(true, branchGeometry, startNode, endNode);
            Assert.AreEqual(207.581326318883, geometry.Length, 1.0e-6);
        }

        [Test]
        public void SimpleSobekReBranch()
        {
            var branchGeometry = new BranchGeometry();
            INode startNode = new HydroNode { Geometry = new Point(0.0, 0.0) };
            INode endNode = new HydroNode { Geometry = new Point(141.421, 341.421) };
            branchGeometry.CurvingPoints.Add(new CurvingPoint(0, 0));
            branchGeometry.CurvingPoints.Add(new CurvingPoint(100, 45));
            branchGeometry.CurvingPoints.Add(new CurvingPoint(400, 0));
            var geometry = GeometryHelper.CalculateGeometry(false, branchGeometry, startNode, endNode);
            Assert.AreEqual(400, geometry.Length, 1.0e-2);
            Assert.AreEqual(4, geometry.Coordinates.Length);
        }
    }
}
