using DeltaShell.NGHS.Common.Utils;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.Utils
{
    [TestFixture]
    public class MigrationHelperTest
    {
        [Test]
        [TestCase(null, null, false)]
        [TestCase("", null, false)]
        [TestCase("someKey WithSpaces=AValue;someOtherKeyWithoutSpaces=AnotherValue", null, false)]
        [TestCase("Data Source=/a/path/to/a/database", "/a/path/to/a/database", true)]
        [TestCase(@"someKey WithSpaces=AValue;Data Source=\a\path\to\a\database;someOtherKeyWithoutSpaces=AnotherValue", @"\a\path\to\a\database", true)]
        [TestCase(@"someKey WithSpaces=AValue;Data Source=C:\a\path\to\a\database;someOtherKeyWithoutSpaces=AnotherValue", @"C:\a\path\to\a\database", true)]
        public void TryParseDatabasePath_ExpectedResults(string connectionString, string expectedDatabasePath, bool couldParse)
        {
            // Call
            bool hasParsed = MigrationHelper.TryParseDatabasePath(connectionString, out string databasePath);

            // Assert
            Assert.That(hasParsed, Is.EqualTo(couldParse));
            Assert.That(databasePath, Is.EqualTo(expectedDatabasePath));
        }
    }
}