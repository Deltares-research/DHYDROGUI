using System.IO;

namespace DeltaShell.Plugins.NetworkEditor.IO
{
    public static class IoHelper
    {
        public static string GetFilePathToLocationInSameDirectory(string netFilePath, string fileName)
        {
            var directoryName = Path.GetDirectoryName(netFilePath);
            return directoryName != null ? Path.Combine(directoryName, fileName) : null;
        }
    }
}
