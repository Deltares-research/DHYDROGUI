using System;
using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Api;
using NUnit.Framework;

[TestFixture]
public class WaveModelApiTest
{
    [Test]
    [Category(TestCategory.Integration)]
    [Category(TestCategory.Slow)]
    public void RemoteWaveModelApiInitAndRunTest()
    {
        string oldDir = Directory.GetCurrentDirectory();
        try
        {
            var path = TestHelper.GetTestFilePath(@"obw\obw.mdw");
            var localPath = TestHelper.CreateLocalCopy(path);
            
            Directory.SetCurrentDirectory(Path.GetDirectoryName(localPath));
            // from mdw file:
            var refDate = new DateTime(2000, 7, 14);

            using (var api = new RemoteWaveModelApi(false) {ReferenceDateTime = refDate})
            {
                api.SetValues("mode", new[] { "stand-alone" });
                api.Initialize(Path.GetFileName(localPath));
                api.Update(3600.0);
                var apiCurrentTime = api.CurrentTime;
                Assert.That((apiCurrentTime-api.StartTime).TotalSeconds, Is.EqualTo(3600.0).Within(0.1));
                Assert.AreEqual(refDate + new TimeSpan(1, 0, 0), apiCurrentTime);
                api.Finish();
            }
        }
        finally
        {
            Directory.SetCurrentDirectory(oldDir);
        }
    }

    [Test]
    [Category(TestCategory.Integration)]
    [Category(TestCategory.Slow)]
    public void WaveModelApiInitAndRunTest()
    {
        if (!Environment.Is64BitProcess) return; // wave only runs in 64 bits!
        string oldDir = Directory.GetCurrentDirectory();
        try
        {
            var path = TestHelper.GetTestFilePath(@"obw\obw.mdw");
            var localPath = TestHelper.CreateLocalCopy(path);
            
            Directory.SetCurrentDirectory(Path.GetDirectoryName(localPath));
            // from mdw file:
            var refDate = new DateTime(2000, 7, 14);

            using (var api = new WaveModelApi {ReferenceDateTime = refDate})
            {
                api.SetValues("mode", new [] { "stand-alone" });
                api.Initialize(Path.GetFileName(localPath));
                api.Update(3600.0);
                var apiCurrentTime = api.CurrentTime;
                Assert.That((apiCurrentTime-api.StartTime).TotalSeconds, Is.EqualTo(3600.0).Within(0.1));
                Assert.AreEqual(refDate + new TimeSpan(1, 0, 0), apiCurrentTime);
                api.Finish();
            }
        }
        finally
        {
            Directory.SetCurrentDirectory(oldDir);
        }

        
    }
}