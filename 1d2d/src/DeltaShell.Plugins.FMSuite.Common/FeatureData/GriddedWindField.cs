using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Functions;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Common.IO;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Common.FeatureData
{
    public class GriddedWindField: IWindField, IFileBased
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (GriddedWindField));
        private WindQuantity quantity;

        public static GriddedWindField CreateXField(string filePath)
        {
            return new GriddedWindField(filePath) {Quantity = WindQuantity.VelocityX};
        }

        public static GriddedWindField CreateYField(string filePath)
        {
            return new GriddedWindField(filePath) { Quantity = WindQuantity.VelocityY };
        }

        public static GriddedWindField CreatePressureField(string filePath)
        {
            return new GriddedWindField(filePath) { Quantity = WindQuantity.AirPressure };
        }

        public static GriddedWindField CreateCurviField(string filePath, string gridFilePath)
        {
            return new GriddedWindField(filePath, gridFilePath) {Quantity = WindQuantity.VelocityVectorAirPressure};
        }

        private GriddedWindField(string windFilePath)
        {
            WindFilePath = windFilePath;
            GridFilePath = windFilePath;
            SeparateGridFile = false;
        }

        private GriddedWindField(string windFilePath, string gridFilePath)
        {
            WindFilePath = windFilePath;
            GridFilePath = gridFilePath;
            SeparateGridFile = true;
        }

        private static string CreateName(WindQuantity windQuantity)
        {
            switch (windQuantity)
            {
                case WindQuantity.VelocityX:
                    return "Gridded x-field";
                case WindQuantity.VelocityY:
                    return "Gridded y-field";
                case WindQuantity.AirPressure:
                    return "Gridded p-field";
                case WindQuantity.VelocityVector:
                    return "Gridded xy-field";
                case WindQuantity.VelocityVectorAirPressure:
                    return "Curvi-grid xyp";
                default:
                    throw new NotImplementedException("Wind quantity not supported");
            }
        }

        public bool SeparateGridFile { get; private set; }

        public WindQuantity Quantity
        {
            get { return quantity; }
            private set
            {
                quantity = value;
                UpdateName();
            }
        }

        private void UpdateName()
        {
            Name = CreateName(Quantity);
        }

        public IFunction Data
        {
            get { return null; }
        }

        public string Name { get; private set; }

        public string WindFilePath { get; set; }

        public string GridFilePath { get; set; }

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
                if (SeparateGridFile)
                {
                    yield return GridFilePath;
                }
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
            GridFilePath = null;
        }

        public void Open(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(string.Format("Wind file {0} could not be found", path));
            }
            if (SeparateGridFile)
            {
                var gridFilePath = GetCorrespondingGridFilePath(path);
                if (gridFilePath == null || !File.Exists(gridFilePath))
                {
                    throw new FileNotFoundException(string.Format("Corresponding grid file {0} could not be found", gridFilePath));
                }
                GridFilePath = gridFilePath;
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
            if (SeparateGridFile)
            {
                if (!File.Exists(GridFilePath))
                {
                    Log.ErrorFormat("Could not find wind grid file {0}", Path);
                    return;                    
                }
                var destGridFilePath = GetCorrespondingGridFilePath(destinationPath);
                if (destGridFilePath != null && System.IO.Path.GetFullPath(GridFilePath) != System.IO.Path.GetFullPath(destGridFilePath))
                {
                    File.Copy(GridFilePath, destGridFilePath, true);
                }
            }
        }

        public void SwitchTo(string newPath)
        {
            Path = newPath;
            GridFilePath = SeparateGridFile ? GetCorrespondingGridFilePath(newPath) : newPath;
        }

        public void Delete()
        {
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
            if (SeparateGridFile && File.Exists(GridFilePath))
            {
                File.Delete(GridFilePath);
            }
            Path = null;
            GridFilePath = null;
        }

        #endregion

        public static string GetCorrespondingGridFilePath(string filePath)
        {
            return WindFile.GetCorrespondingGridFilePath(filePath);
        }
    }
}
