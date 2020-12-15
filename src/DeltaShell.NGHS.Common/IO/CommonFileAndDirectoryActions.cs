using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeltaShell.NGHS.Common.IO
{
    public static class CommonFileAndDirectoryActions
    {
        public static bool ClearFolderWithFileExceptions(string folderPath, IReadOnlyCollection<string> fileExceptions)
        {
            bool folderIsEmpty = true;
            var dir = new DirectoryInfo(folderPath);

            foreach (FileInfo fi in dir.GetFiles())
            {
                if (!fileExceptions.Contains(fi.FullName))
                {
                    File.Delete(fi.FullName);
                }
                else
                {
                    folderIsEmpty = false;
                }
            }

            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                if (ClearFolderWithFileExceptions(di.FullName, fileExceptions))
                {
                    di.Delete();
                }
                else
                {
                    folderIsEmpty = false;
                }
            }

            return folderIsEmpty;
        }
    }
}