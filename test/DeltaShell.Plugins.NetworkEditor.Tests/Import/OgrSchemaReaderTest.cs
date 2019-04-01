using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Import;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import
{
    [TestFixture]
    [Category(TestCategory.X86)]
    public class OgrSchemaReaderTest
    {
        [Test]
        public void ReadMdbAndGetColumnNames()
        {
            var path = TestHelper.GetTestFilePath("HydroBaseCF_Basis.mdb");
            var ogrSchemaReader = new OgrSchemaReader();
            ogrSchemaReader.Path = path;
            ogrSchemaReader.OpenConnection();
            var lstColumnNamesNames = ogrSchemaReader.GetColumnNames("Cross_section_definition");
            ogrSchemaReader.CloseConnection();

            Assert.AreEqual(19, lstColumnNamesNames.Count);
            Assert.IsTrue(lstColumnNamesNames.Contains("OBJECTID"));
        }

        [Test]
        public void ReadMdbAndGetDistinctValues()
        {
            var path = TestHelper.GetTestFilePath("HydroBaseCF_Basis.mdb");
            var ogrSchemaReader = new OgrSchemaReader();
            ogrSchemaReader.Path = path;
            ogrSchemaReader.OpenConnection();
            var lstDistinctValues = ogrSchemaReader.GetDistinctValues("Culvert", "TYPE");
            ogrSchemaReader.CloseConnection();

            Assert.AreEqual(2, lstDistinctValues.Count);
            Assert.IsTrue(lstDistinctValues.Contains("ROND"));
            Assert.IsTrue(lstDistinctValues.Contains("rechthoek"));
        }
    }
}