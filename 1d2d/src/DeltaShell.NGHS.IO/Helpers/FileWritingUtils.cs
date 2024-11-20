using System;
using System.IO;
using DeltaShell.NGHS.IO.FileWriters;

namespace DeltaShell.NGHS.IO.Helpers
{
    public static class FileWritingUtils
    {
        public static void ThrowIfFileNotExists(string filePath, string fileNameTargetPath, Action<string> writeAction)
        {
            writeAction(filePath);

            if (File.Exists(filePath)) return;
            throw new FileWritingException(String.Format("{0} is not written at location {1}.", Path.GetFileName(filePath), fileNameTargetPath));
        }
    }
}
