using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests
{
    [TestFixture]
    public class DirectoryNameConstantsTest
    {
        [TestCase(DirectoryNameConstants.InputDirectoryName, "input")]
        [TestCase(DirectoryNameConstants.OutputDirectoryName, "output")]
        public void Field_HasCorrectValue(string resultValue, string expectedValue)
        {
            Assert.That(resultValue, Is.EqualTo(expectedValue),
                        $"Constant field within class {nameof(DirectoryNameConstants)} did not have correct value.");
        }
    }
}