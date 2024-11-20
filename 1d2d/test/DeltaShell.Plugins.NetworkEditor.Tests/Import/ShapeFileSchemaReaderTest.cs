using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Import;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class ShapeFileSchemaReaderTest
    {
        [Test]
        public void ReadShapeWithSpecialCharactersInName()
        {
            var path = TestHelper.GetTestFilePath("1-watergangen-WGS.shp");
            ISchemaReader schemaReader = new ShapeFileSchemaReader();
            schemaReader.Path = path;
            schemaReader.OpenConnection();
            var lstColumnNamesNames = schemaReader.GetColumnNames(null);
            var lstDistinctValues = schemaReader.GetDistinctValues(null, "OVK_TYPE");
            schemaReader.CloseConnection();

            Assert.AreEqual(211,lstColumnNamesNames.Count);
            Assert.AreEqual(1, lstDistinctValues.Count);
        }

        [Test]
        public void ReadTwoShapeFiles()
        {
            var pathChannels = TestHelper.GetTestFilePath("HydroBaseCF_ShapeFiles/Channels.shp");
            var pathPumps = TestHelper.GetTestFilePath("HydroBaseCF_ShapeFiles/Pumps.shp");
            ISchemaReader schemaReader = new ShapeFileSchemaReader();
            schemaReader.Path = pathChannels;
            schemaReader.OpenConnection();
            var lstColumnNamesNames = schemaReader.GetColumnNames(null);
            Assert.IsTrue(lstColumnNamesNames.Contains("OVKIDENT"));
            schemaReader.CloseConnection();
            schemaReader.Path = pathPumps;
            schemaReader.OpenConnection();
            lstColumnNamesNames = schemaReader.GetColumnNames(null);
            schemaReader.CloseConnection();
            Assert.IsTrue(lstColumnNamesNames.Contains("KWKIDENT"));

        }

        [Test]
        public void ReadDbfAndGetColumnNames()
        {
            var path = TestHelper.GetTestFilePath("Stuwen.shp");
            var schemaReader = new ShapeFileSchemaReader {Path = path};
            schemaReader.OpenConnection();
            var lstColumnNamesNames = schemaReader.GetColumnNames(null);
            schemaReader.CloseConnection();

            Assert.AreEqual(47, lstColumnNamesNames.Count);
            Assert.IsTrue(lstColumnNamesNames.Contains("KSTREGEL"));
        }

        [Test]
        public void ReadDbfAndGetDistinctValues()
        {
            var path = TestHelper.GetTestFilePath("Stuwen.shp");
            var schemaReader = new ShapeFileSchemaReader {Path = path};
            schemaReader.OpenConnection();
            var lstDistinctValues = schemaReader.GetDistinctValues(null, "KSTREGEL");
            schemaReader.CloseConnection();

            Assert.AreEqual(3, lstDistinctValues.Count);
            Assert.IsTrue(lstDistinctValues.Contains("2"));
        }
    }
}
