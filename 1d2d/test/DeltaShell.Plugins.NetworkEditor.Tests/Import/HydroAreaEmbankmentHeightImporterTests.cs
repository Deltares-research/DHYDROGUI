using System;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Import;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import
{
    [TestFixture]
    class HydroAreaEmbankmentHeightImporterTests
    {
        private string _embankmentsPath;
        private string _embankmentHeightsPath;
        private HydroArea _hydroArea;

        [SetUp]
        public void SetUp()
        {
            _embankmentsPath = TestHelper.GetTestFilePath("EmbankmentHeights_ShapeFiles/Omtrek-Maas.shp");
            _embankmentHeightsPath = TestHelper.GetTestFilePath("EmbankmentHeights_ShapeFiles/omtrekMaasPointz.shp");
            _hydroArea = new HydroArea();
        }

        [Test]
        public void TestImportItem_NoPath()
        {
            var heightImporter = new HydroAreaEmbankmentHeightImporter();
            Assert.Throws<InvalidOperationException>(() => heightImporter.ImportItem(""));
        }
        
        [Test]
        public void TestImportItem_NoTarget()
        {
            var heightImporter = new HydroAreaEmbankmentHeightImporter();
            Assert.Throws<InvalidOperationException>(() => heightImporter.ImportItem(_embankmentHeightsPath));
        }

        [Test]
        public void TestImportItem_WrongTargetType()
        {
            var heightImporter = new HydroAreaEmbankmentHeightImporter();
            Assert.Throws<InvalidOperationException>(() => heightImporter.ImportItem(_embankmentHeightsPath, new object()));
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void TestImportItem_SuccessfullyImportsZValues()
        {
            var embankmentImporter = new HydroAreaEmbankmentImporter();
            embankmentImporter.ImportItem(_embankmentsPath, _hydroArea);

            Assert.That((_hydroArea.Embankments[0].Geometry.Coordinates[0].Z).Equals(0));

            var heightImporter = new HydroAreaEmbankmentHeightImporter();
            heightImporter.ImportItem(_embankmentHeightsPath, _hydroArea);

            Assert.That(_hydroArea.Embankments[0].Geometry.Coordinates[0].Z > 0);
        }
    }
}