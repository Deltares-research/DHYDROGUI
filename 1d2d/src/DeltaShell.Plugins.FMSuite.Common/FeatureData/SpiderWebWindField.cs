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

        public static SpiderWebWindField Create(string filePath)
        {
            return new SpiderWebWindField(filePath);
        }

        private SpiderWebWindField(string filePath)
        {
            WindFilePath = filePath;
        }

        public string WindFilePath { get; set; }

        public WindQuantity Quantity
        {
            get { return WindQuantity.VelocityVectorAirPressure; }
        }

        public IFunction Data
        {
            get { return null; }
        }

        public string Name
        {
            get { return "Spider web"; }
        }

        #region IFileBased

        public string Path
        {
            get { return WindFilePath; }
            set { WindFilePath = value; }
        }

        public IEnumerable<string> Paths
        {
            get
            {
                yield return WindFilePath;
            }
        }

        public bool IsFileCritical { get { return true; } }

        public bool IsOpen
        {
            get { return Path != null; }
        }

        public bool CopyFromWorkingDirectory { get; } = false;

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
