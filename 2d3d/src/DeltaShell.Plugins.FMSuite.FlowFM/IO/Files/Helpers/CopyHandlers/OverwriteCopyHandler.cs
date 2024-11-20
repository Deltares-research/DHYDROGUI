using System;
using System.IO;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers.CopyHandlers
{
    /// <summary>
    /// <see cref="OverwriteCopyHandler"/> defines a copy operation which will
    /// overwrite the targetPath if it already exists.
    /// </summary>
    /// <seealso cref="ICopyHandler"/>
    public class OverwriteCopyHandler : ICopyHandler
    {
        public void Copy(string sourcePath, string targetPath)
        {
            try
            {
                File.Copy(sourcePath, targetPath, true);
            }
            catch (Exception e)
            {
                throw new FileCopyException(e.Message, e);
            }
        }
    }
}