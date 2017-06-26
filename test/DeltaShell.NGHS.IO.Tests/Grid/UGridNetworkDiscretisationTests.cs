using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Grid;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class UGridNetworkDiscretisationTests
    {
        private const string UGRID_TEST_FOLDER = @"ugrid\";
        
        [Test]
        public void GivenNetCdfFileWhenRetrievingNetworkIdThenReturn1()
        {
            var netCdfFilePath = "Custom_Ugrid.nc";
            var testFilePath = TestHelper.GetTestFilePath(UGRID_TEST_FOLDER + netCdfFilePath);
            var uGridNd = new UGridNetworkDiscretisation(testFilePath);

            uGridNd.Initialize();
            Assert.That(uGridNd.GetNetworkId(1), Is.EqualTo(1));
        }
    }
}