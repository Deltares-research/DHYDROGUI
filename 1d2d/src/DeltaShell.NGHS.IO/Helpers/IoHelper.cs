using System.IO;
using System.Linq;

namespace DeltaShell.NGHS.IO.Helpers
{
    public static class IoHelper
    {
        public static string GetFilePathToLocationInSameDirectory(string netFilePath, string fileName)
        {
            var directoryName = Path.GetDirectoryName(netFilePath);
            return directoryName != null ? Path.Combine(directoryName, fileName) : null;
        }
        public static bool IsValidTextFile(string path)
        {
            using (var stream = System.IO.File.Open(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
            using (var reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8))
            {
                var bytesRead = reader.ReadToEnd();
                reader.Close();
                return bytesRead.All(c => // Are all the characters either a:
                        c == (char)10  // New line
                        || c == (char)13 // Carriage Return
                        || c == (char)9 // Tab
                        || !char.IsControl(c) // Non-control (regular) character
                );
            }
        }
    }
}
