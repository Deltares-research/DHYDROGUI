using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Import;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import
{
    [TestFixture]
    [Category(TestCategory.X86)]
    public class OleDbSchemaReaderTest
    {
        [Test]
        [Ignore("Not possible yet")]
        public void ReadMdbAndGetTableNames()
        {
            var path = TestHelper.GetTestFilePath("HydroBaseCF_Basis.mdb");
            var oledbSchemaReader = new OgrSchemaReader();
            oledbSchemaReader.Path = path;
            oledbSchemaReader.OpenConnection();
            var lstTableNames = oledbSchemaReader.GetTableNames;
            oledbSchemaReader.CloseConnection();

            Assert.AreEqual(10, lstTableNames.Count);
            Assert.IsTrue(lstTableNames.Contains("Cross_section_definition"));
        }

        [Test]
        public void ReadMdbAndGetColumnNames()
        {
            var path = TestHelper.GetTestFilePath("HydroBaseCF_Basis.mdb");
            var oledbSchemaReader = new OleDbSchemaReader();
            oledbSchemaReader.Path = path;
            oledbSchemaReader.OpenConnection();
            var lstColumnNamesNames = oledbSchemaReader.GetColumnNames("Cross_section_definition");
            oledbSchemaReader.CloseConnection();

            Assert.AreEqual(19, lstColumnNamesNames.Count);
            Assert.IsTrue(lstColumnNamesNames.Contains("OBJECTID"));
        }

        [Test]
        public void ReadMdbAndGetDistinctValues()
        {
            var path = TestHelper.GetTestFilePath("HydroBaseCF_Basis.mdb");
            var oledbSchemaReader = new OleDbSchemaReader();
            oledbSchemaReader.Path = path;
            oledbSchemaReader.OpenConnection();
            var lstDistinctValues = oledbSchemaReader.GetDistinctValues("Culvert","TYPE");
            oledbSchemaReader.CloseConnection();

            Assert.AreEqual(2, lstDistinctValues.Count);
            Assert.IsTrue(lstDistinctValues.Contains("ROND"));
            Assert.IsTrue(lstDistinctValues.Contains("rechthoek"));
        }
    }
}
