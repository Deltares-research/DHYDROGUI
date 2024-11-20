using DeltaShell.Plugins.NetworkEditor.Import;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import
{
    [TestFixture]
    public class MappingColumnTest
    {
        [Test]
        public void GivenMappingColumnWithoutTableNameDefined_ThenToStringReturnsOnlyColumnName()
        {
            var columnName = "MyColumn";
            var mappingColumn = new MappingColumn
            {
                ColumnName = columnName
            };
            Assert.That(mappingColumn.ToString(), Is.EqualTo(columnName));
        }

        [Test]
        public void GivenMappingColumnWithTableNameDefined_ThenToStringReturnsTableNameAndColumnName()
        {
            var mappingColumn = new MappingColumn
            {
                TableName = "MyTable",
                ColumnName = "MyColumn"
            };
            Assert.That(mappingColumn.ToString(), Is.EqualTo("MyTable.MyColumn"));
        }
    }
}