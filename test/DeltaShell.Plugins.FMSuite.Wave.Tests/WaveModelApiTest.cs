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
    public void WaveModelApiInitAndRunTest()
    {
        var waveDir = Path.GetDirectoryName(GetType().Assembly.Location);
        var d3dhomeDir = Path.Combine(waveDir, "Delft3D");

        var waveExeDir = Path.Combine(d3dhomeDir, @"win32\wave\bin");
        var swanExeDir = Path.Combine(d3dhomeDir, @"win32\swan\bin");
        var swanScriptDir = Path.Combine(d3dhomeDir, @"win32\swan\scripts");
        var pathToAdd = waveExeDir + ";" +
                        swanExeDir + ";" +
                        swanScriptDir;

        var oldD3D = Environment.GetEnvironmentVariable("D3D_HOME");
        var oldPath = Environment.GetEnvironmentVariable("PATH");

        // from mdw file:
        var refDate = new DateTime(2000, 7, 14);

        DateTime t;
        string oldDir = Directory.GetCurrentDirectory();
        try
        {
            Environment.SetEnvironmentVariable("D3D_HOME", d3dhomeDir);
            Environment.SetEnvironmentVariable("PATH", pathToAdd + ";" + oldPath);
            Environment.SetEnvironmentVariable("ARCH", "win32");

            var path = TestHelper.GetTestFilePath(@"obw\obw.mdw");
            var localPath = TestHelper.CreateLocalCopy(path);
            
            Directory.SetCurrentDirectory(Path.GetDirectoryName(localPath));

            using (var api = new RemoteWaveModelApi(true) {ReferenceDateTime = refDate})
            {
                api.SetVar("mode", "stand-alone");
                api.Initialize(Path.GetFileName(localPath));
                api.Update(3600.0);

                t = api.CurrentTime;
            }
        }
        finally
        {
            Environment.SetEnvironmentVariable("D3D_HOME", oldD3D);
            Environment.SetEnvironmentVariable("PATH", oldPath);
            Directory.SetCurrentDirectory(oldDir);
        }

        
        Assert.AreEqual(refDate + new TimeSpan(1,0,0), t);
    }
}