using System;
using System.IO;
using DelftTools.Utils.IO;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests
{
    public static class WaveTestHelper
    {
        public static string CreateLocalCopy(string mdwPath)
        {
            string dir = Path.GetDirectoryName(mdwPath);
            string lastDir = new DirectoryInfo(dir).Name;

            string newDir = Path.Combine(Environment.CurrentDirectory, lastDir);

            if (Directory.Exists(newDir))
            {
                try
                {
                    Directory.Delete(newDir, true);
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to delete directory before local copy: {0}", newDir);
                }
            }

            FileUtils.CopyDirectory(dir, newDir, ".svn");
            return Path.Combine(newDir, Path.GetFileName(mdwPath));
        }
    }
}