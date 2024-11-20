using System.IO;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests
{
    [TestFixture]
    public class DefaultModelSettingsTest
    {
        [Test]
        public void DefaultDeltaShellWorkingDirectoryTest()
        {
            Assert.AreEqual(Path.Combine(Path.GetTempPath(), "DeltaShell_Working_Directory"), DefaultModelSettings.DefaultDeltaShellWorkingDirectory);
        }
    }
}