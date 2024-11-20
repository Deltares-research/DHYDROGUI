using System.Collections.Generic;
using System.IO;
using DelftTools.Functions;
using DelftTools.Utils.IO;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Common.FeatureData
{
    public class SpiderWebWindField : IWindField, IFileBased
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SpiderWebWindField));

        private SpiderWebWindField(string filePath)
        {
            WindFilePath = filePath;
        }

        public string WindFilePath { get; set; }

        public WindQuantity Quantity => WindQuantity.VelocityVectorAirPressure;

        public IFunction Data => null;

        public string Name => "Spider web";

        public static SpiderWebWindField Create(string filePath)
        {
            return new SpiderWebWindField(filePath);
        }

        #region IFileBased

        public string Path
        {
            get => WindFilePath;
            set => WindFilePath = value;
        }

        public IEnumerable<string> Paths
        {
            get
            {
                yield return WindFilePath;
            }
        }

        public bool IsFileCritical => true;

        public bool IsOpen => Path != null;

        /// <summary>
        /// Make a copy of the file if it is located in the DeltaShell working directory
        /// </summary>
        public bool CopyFromWorkingDirectory { get; }

        public void CreateNew(string path)
        {
            if (!File.Exists(path))
            {
                File.Create(path);
            }

            Path = path;
        }

        public void Close()
        {
            Path = null;
        }

        public void Open(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(string.Format("Wind file {0} could not be found", path));
            }

            Path = path;
        }

        public void CopyTo(string destinationPath)
        {
            if (!File.Exists(Path))
            {
                Log.ErrorFormat("Could not find wind data file {0}", Path);
                return;
            }

            if (System.IO.Path.GetFullPath(Path) != System.IO.Path.GetFullPath(destinationPath))
            {
                File.Copy(Path, destinationPath, true);
            }
        }

        public void SwitchTo(string newPath)
        {
            Path = newPath;
        }

        public void Delete()
        {
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
        }

        #endregion
    }
}