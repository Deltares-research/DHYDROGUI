using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    // TODO: move it to DelftTools.Utils, Sobek is not a right place for such a generic test
    [TestFixture]
    public class StringUtilsTests
    {
        [Test]
        public void RemoveQuotes()
        {
            Assert.AreEqual("dd", StringUtils.RemoveQuotes("'dd'"));
            Assert.AreEqual("",StringUtils.RemoveQuotes("''"));

        }
    }
}