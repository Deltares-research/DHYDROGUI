using DelftTools.Utils;
using DelftTools.Utils.Data;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.DataObjects.BoundaryData
{
    [TestFixture]
    public class DataTableTest
    {
        [Test]
        public void DefaultConstructor_ExpectedValues()
        {
            // setup

            // call
            var table = new DataTable();

            // assert
            Assert.IsInstanceOf<IUnique<long>>(table);
            Assert.IsInstanceOf<INameable>(table);
            Assert.AreEqual(string.Empty, table.Name);
            Assert.IsTrue(table.IsEnabled);
            Assert.IsNull(table.DataFile);
            Assert.IsNull(table.SubstanceUseforFile);
        }
    }
}