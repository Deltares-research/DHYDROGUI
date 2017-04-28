using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Common.IO
{
    [TestFixture]
    public class DelftIniReaderTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ParseMdwFile()
        {
            var delftIniReader = new DelftIniReader();
            var mdwFilePath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd\tst.mdw");
            var categories = delftIniReader.ReadDelftIniFile(mdwFilePath);

            Assert.AreEqual(2, categories.Count(k => k.Name == "Domain"));
            Assert.AreEqual(2, categories.Count(k => k.Name == "Boundary"));
            Assert.AreEqual(3, categories.Count(k => k.Name == "TimePoint"));
            Assert.AreEqual(13, categories.Count);

            var innerDomain = categories.Where(c => c.Name == "Domain").ToList()[1];
            Assert.AreEqual(85, innerDomain.LineNumber);

            var bedLevelProperty = innerDomain.Properties.First(p => p.Name == "BedLevel");
            Assert.AreEqual(88, bedLevelProperty.LineNumber);
            Assert.AreEqual("inner.dep", bedLevelProperty.Value);

            Assert.AreEqual("36", innerDomain.GetPropertyValue("NDir"));
            Assert.AreEqual(null, innerDomain.GetPropertyValue("harazafraz"));
        }
    }
}