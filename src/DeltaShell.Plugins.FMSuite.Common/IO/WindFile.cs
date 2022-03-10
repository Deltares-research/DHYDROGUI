using System;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public static class WindFile 
    {
        public static string GetCorrespondingGridFilePath(string filePath)
        {
            
            if (filePath == null) return null;
            try
            {
                var fullPath = System.IO.Path.GetFullPath(filePath);
                var apwxwyFileReader = new ApwxwyFileReader(fullPath);
                var fname = apwxwyFileReader.ReadGridFileNameWithExtension();

                if (fname == null) return null;

                var apwxwyDir = System.IO.Path.GetDirectoryName(fullPath);
                if (apwxwyDir == null) return null;

                return System.IO.Path.Combine(apwxwyDir, fname);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
