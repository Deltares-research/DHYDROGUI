using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class LdbFileTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadWriteReadHarlingebLdbTest()
        {
            var ldbFile = new LdbFile();
            var ldbPath = TestHelper.GetTestFilePath(@"harlingen\Harlingen_haven.ldb");

            var features = ldbFile.Read(ldbPath);
            Assert.IsNotNull(features);

            const string exportedFile = "Harlingen_haven_export.ldb";
            ldbFile.Write(exportedFile, features);
            Assert.IsTrue(File.Exists(exportedFile));

            var featuresAfterExport = ldbFile.Read(exportedFile);
            Assert.AreEqual(features.Count, featuresAfterExport.Count);
            Assert.AreEqual(features.Select(f => f.Name).ToList(), featuresAfterExport.Select(f => f.Name).ToList());
            Assert.AreEqual(features.Select(f => f.Geometry).ToList(),
                            featuresAfterExport.Select(f => f.Geometry).ToList());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ReadWriteReadArabSeaLdbTest()
        {
            var ldbFile = new LdbFile();
            var ldbPath = TestHelper.GetTestFilePath(@"landboundaries\ArabianSea.ldb");

            var features = ldbFile.Read(ldbPath);
            Assert.IsNotNull(features);

            const string exportedFile = "ArabianSea_export.ldb";
            ldbFile.Write(exportedFile, features);
            Assert.IsTrue(File.Exists(exportedFile));

            var featuresAfterExport = ldbFile.Read(exportedFile);
            Assert.AreEqual(features.Count, featuresAfterExport.Count);
            Assert.AreEqual(features.Select(f => f.Name).ToList(), featuresAfterExport.Select(f => f.Name).ToList());
            Assert.AreEqual(features.Select(f => f.Geometry).ToList(),
                            featuresAfterExport.Select(f => f.Geometry).ToList());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ReadWriteReadMississippiLdbTest()
        {
            var ldbFile = new LdbFile();
            var ldbPath = TestHelper.GetTestFilePath(@"landboundaries\mississippiCoast_utm_new.ldb");

            var features = ldbFile.Read(ldbPath);
            Assert.IsNotNull(features);
            Assert.AreEqual(851, features.Count);

            const string exportedFile = "mississippiCoast_export.ldb";
            ldbFile.Write(exportedFile, features);
            Assert.IsTrue(File.Exists(exportedFile));

            var featuresAfterExport = ldbFile.Read(exportedFile);
            Assert.AreEqual(features.Count, featuresAfterExport.Count);
            Assert.AreEqual(features.Select(f => f.Name).ToList(), featuresAfterExport.Select(f => f.Name).ToList());
            Assert.AreEqual(features.Select(f => f.Geometry).ToList(),
                            featuresAfterExport.Select(f => f.Geometry).ToList());
        }
    }
}
