using System;
using System.IO;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public static class WindFile
    {
        public static string GetCorrespondingGridFilePath(string filePath)
        {
            if (filePath == null)
            {
                return null;
            }

            try
            {
                string fullPath = Path.GetFullPath(filePath);
                var apwxwyFileReader = new ApwxwyFileReader(fullPath);
                string fname = apwxwyFileReader.ReadGridFileNameWithExtension();

                if (fname == null)
                {
                    return null;
                }

                string apwxwyDir = Path.GetDirectoryName(fullPath);
                if (apwxwyDir == null)
                {
                    return null;
                }

                return Path.Combine(apwxwyDir, fname);
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}