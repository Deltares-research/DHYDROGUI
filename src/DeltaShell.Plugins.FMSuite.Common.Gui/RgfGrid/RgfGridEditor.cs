using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using MessageBox = DelftTools.Controls.Swf.MessageBox;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.RgfGrid
{
    public enum CoordinateSystemType
    {
        Spherical,
        Cartesian
    }

    public static class RgfGridEditor
    {
        private enum GridType
        {
            FM,
            GRD
        }

        public const string MfeAppProcessName = "mfe_app";

        private const string GrdNetCdfKeyword = "RGF_NETCDF";
        private const string RgfGridConfigurationFileName = "rgfgrid.d3d";

        public static void OpenGrid(string gridPath,
                                    bool gridIsEmpty = false,
                                    IEnumerable<string> additionalPaths = null,
                                    ICoordinateSystem coordinateSystem = null)
        {
            OpenGrids(new[]
                      {
                          gridPath
                      }, new[]
                      {
                          gridIsEmpty
                      }, additionalPaths,
                      coordinateSystem: coordinateSystem);
        }

        public static void OpenGrid(string gridPath,
                                    bool gridIsEmpty,
                                    IEnumerable<IPolygon> polygons,
                                    string polFileName,
                                    ICoordinateSystem coordinateSystem = null)
        {
            OpenGrids(new[]
                      {
                          gridPath
                      }, new[]
                      {
                          gridIsEmpty
                      }, null, polygons, polFileName,
                      coordinateSystem: coordinateSystem);
        }

        public static void OpenGrids(string[] grids,
                                     bool[] gridIsEmpty,
                                     IEnumerable<string> additionalPaths = null,
                                     IEnumerable<IPolygon> polygons = null,
                                     string polFileName = null,
                                     CoordinateSystemType systemType = CoordinateSystemType.Cartesian,
                                     ICoordinateSystem coordinateSystem = null)
        {
            additionalPaths = additionalPaths ?? new string[0];

            if (grids == null || grids.Length == 0)
            {
                throw new ArgumentException("You must supply at least one grid path");
            }

            // create config file with paths
            string tempDir = FileUtils.CreateTempDirectory();

            string[] gridCopies = PrepareGridCopies(grids, gridIsEmpty, tempDir, systemType, coordinateSystem);

            List<string> copiedAdditionalPaths = CopyFilesToDirectory(tempDir, additionalPaths);

            //copy additional files (ldb, shape)
            CreateRgfGridConfigurationFile(tempDir, gridCopies, copiedAdditionalPaths, polygons, polFileName);

            CopyRgfGridBinariesTo(tempDir);

            var validGrid = false;
            while (!validGrid)
            {
                StartRgfGridFrom(tempDir, polygons, polFileName);
                validGrid = true;

                string extension = Path.GetExtension(grids[0]);
                if (extension != ".nc" && extension != ".grd")
                {
                    continue;
                }

                GridType validGridType = extension == ".nc" ? GridType.FM : GridType.GRD;
                gridCopies = TryGetValidGridCopies(validGridType, tempDir, gridCopies, copiedAdditionalPaths, ref validGrid);
            }

            if (gridCopies.Length == 0)
            {
                FileUtils.DeleteIfExists(tempDir);
                MessageBox.Show(
                    "Unable to reload grid file, no grid was referenced in the d3d file! Did you save the rgfgrid project?",
                    "No grid file found",
                    MessageBoxButtons.OK);
                return;
            }

            if (!File.Exists(gridCopies[0]))
            {
                MessageBox.Show(
                    string.Format("Unable to find grid file, no grid was found at path: {0}", gridCopies[0]),
                    "No grid file found",
                    MessageBoxButtons.OK);
                return;
            }

            if (FileUtils.WaitForFile(gridCopies[0]))
            {
                for (var i = 0; i < grids.Length; i++)
                {
                    File.Copy(gridCopies[i], grids[i], true); // copy the grids back to the expected location
                }

                FileUtils.DeleteIfExists(tempDir);
            }
            else
            {
                MessageBox.Show(
                    string.Format("Unable to reload grid file {0}, RGFGrid is locking it!", gridCopies[0]),
                    "Grid file locked",
                    MessageBoxButtons.OK);
            }
        }

        private static IList<string> GetMessageBoxTextsWhenInvalidGridType(GridType gridType)
        {
            var messageBoxTexts = new List<string>();
            var message = string.Empty;
            var header = string.Empty;

            var commonText = "Click Retry to re-open rgfgrid to perform this conversion. If you click Cancel, any grid changes you made will be lost.";

            switch (gridType)
            {
                case GridType.FM:
                    message = string.Format(
                        "You appear to have created a regular grid, however flexible mesh works with irregular (unstructured) grids. " +
                        "You can convert a regular grid to an irregular grid using rgfgrid with:{0}Operations->Convert Grid->Regular to Irregular{0}{0}" +
                        commonText,
                        Environment.NewLine);
                    header = "Structured grid instead of unstructured grid!";
                    break;
                case GridType.GRD:
                    message = string.Format(
                        "You appear to have created an irregular grid, however wave works with regular (structured) grids. " +
                        "You can convert an irregular grid to a regular grid using rgfgrid with:{0}Operations->Convert Grid->Irregular to Regular{0}{0}" +
                        commonText,
                        Environment.NewLine);
                    header = "Unstructured grid instead of structured grid!";
                    break;
            }

            messageBoxTexts.Add(message);
            messageBoxTexts.Add(header);
            return messageBoxTexts;
        }

        private static string[] TryGetValidGridCopies(GridType validGridType, string tempDir, string[] gridCopies, IList<string> copiedAdditionalPaths, ref bool happyWithGrid)
        {
            GridFile[] copies = GetLastGridFilesFromConfigurationFile(tempDir, gridCopies.Length);

            if (copies.Any() && copies[0].Type != validGridType)
            {
                IList<string> messageBoxTexts = GetMessageBoxTextsWhenInvalidGridType(validGridType);
                if (MessageBox.Show(messageBoxTexts[0], messageBoxTexts[1], MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry)
                {
                    var config = new RgfConfig() {AdditionalGeometryPaths = copiedAdditionalPaths.Select(Path.GetFileName).ToList()};

                    config.AddGridFileNames(new Tuple<string, string>(copies[0].FileName,
                                                                      copies[0].Type == GridType.FM ? RgfConfig.FMGridKeyword : RgfConfig.GrdKeyword));

                    new DelftIniWriter().WriteDelftIniFile(config.ToIniData(), Path.Combine(tempDir, RgfGridConfigurationFileName));

                    happyWithGrid = false;
                }
            }
            else
            {
                gridCopies = copies.Select(c => c.FilePath).ToArray();
            }

            return gridCopies;
        }

        private static string[] PrepareGridCopies(string[] grids, bool[] gridIsEmpty, string tempDir, CoordinateSystemType systemType, ICoordinateSystem coordinateSystem)
        {
            string firstGrid = grids[0];

            if (Path.GetExtension(firstGrid) == ".nc") // unstructured grid
            {
                if (grids.Length > 1)
                {
                    throw new InvalidOperationException("Expected only one unstructured grid");
                }

                string gridEditPath = Path.Combine(tempDir, gridIsEmpty[0]
                                                                ? "empty_grid_overwrite_this_net.nc"
                                                                : "save_grid_here_net.nc");

                if (gridIsEmpty[0])
                {
                    ConstructNewEmptyGridFile(gridEditPath, coordinateSystem);
                }
                else
                {
                    File.Copy(grids[0], gridEditPath);
                    UnstructuredGridFileHelper.WriteCoordinateSystemToFile(gridEditPath, coordinateSystem, true);
                }

                return new[]
                {
                    gridEditPath
                };
            }

            if (Path.GetExtension(firstGrid) == ".grd")
            {
                for (var i = 0; i < grids.Length; ++i)
                {
                    if (gridIsEmpty[i])
                    {
                        string content = "Coordinate System = " + systemType + Environment.NewLine +
                                         "      0      0" + Environment.NewLine +
                                         " 0 0 0";
                        File.WriteAllText(grids[i], content);
                    }
                }
            }

            return CopyFilesToDirectory(tempDir, grids).ToArray();
        }

        private static void ConstructNewEmptyGridFile(string path, ICoordinateSystem coordinateSystem)
        {
            UnstructuredGridFileHelper.WriteEmptyUnstructuredGridFile(path);
            UnstructuredGridFileHelper.WriteCoordinateSystemToFile(path, coordinateSystem, true);
        }

        private static void StartRgfGridFrom(string tempDir, IEnumerable<IPolygon> polygons = null, string polFileName = null)
        {
            string previousDir = Environment.CurrentDirectory;
            try
            {
                Environment.CurrentDirectory = tempDir;

                // start mfe app with rgfgrid
                var rgfGridProcess = new Process();
                rgfGridProcess.StartInfo = new ProcessStartInfo(string.Format(@".\bin\{0}.exe", MfeAppProcessName), "rgfgrid.dll rgfgrid");
                rgfGridProcess.StartInfo.UseShellExecute = false;
                rgfGridProcess.StartInfo.EnvironmentVariables["WL_PLUGINS_HOME"] = ".";

                if (rgfGridProcess.Start())
                {
                    if (polFileName == null || polygons == null)
                    {
                        CloseAnyWarningPopups(rgfGridProcess);
                    }

                    rgfGridProcess.WaitForExit();
                }
                else
                {
                    throw new InvalidOperationException("Could not start rgfgrid");
                }
            }
            finally
            {
                Environment.CurrentDirectory = previousDir;
            }
        }

        private static void CloseAnyWarningPopups(Process rgfGridProcess)
        {
            // wait for a window to show
            while (rgfGridProcess.MainWindowHandle == IntPtr.Zero)
            {
                Thread.Sleep(0);
            }

            // if it's a (the) warning popup, close it:
            if (rgfGridProcess.MainWindowTitle == "Warning")
            {
                rgfGridProcess.CloseMainWindow(); // send close signal to warning dialog
            }
        }

        private static void CopyRgfGridBinariesTo(string tempDir)
        {
            var rootDir = "";
            if (!Directory.Exists("rgfgrid")) // exists in tests, but not in real application
            {
                rootDir = Path.GetDirectoryName(typeof(RgfGridEditor).Assembly.Location);
            }

            const string environment = "x64";            // we no longer have rgfgrid in 32bits
            const string rgfGridNugetDir = "plugins-qt"; // folder name provided by rgfGrid nuget package

            FileUtils.CopyDirectory(Path.Combine(rootDir, rgfGridNugetDir, environment), tempDir);
        }

        private static List<string> CopyFilesToDirectory(string targetDir, IEnumerable<string> sourcePaths)
        {
            var copiedPaths = new List<string>();
            foreach (string sourcePath in sourcePaths)
            {
                string fileName = Path.GetFileName(sourcePath);
                string copiedPath = Path.Combine(targetDir, fileName);
                File.Copy(sourcePath, copiedPath);
                copiedPaths.Add(copiedPath);
            }

            return copiedPaths;
        }

        private static IEnumerable<GridFile> ParseRgfGridConfigurationFile(string configFilePath)
        {
            string currentDirectory = Path.GetDirectoryName(configFilePath);
            var grids = new List<GridFile>();
            string[] lines = File.ReadAllLines(configFilePath);
            for (var i = 0; i < lines.Length; i++)
            {
                if (i > 0 && lines[i].TrimStart().StartsWith("FileName"))
                {
                    string prevLine = lines[i - 1];
                    if (!prevLine.Contains('='))
                    {
                        continue;
                    }

                    string type = prevLine.Split(new[]
                    {
                        '='
                    })[1].Trim();
                    string fileName = lines[i].Split(new[]
                    {
                        '='
                    })[1].Trim();

                    grids.Add(new GridFile
                    {
                        FileName = fileName,
                        FilePath = Path.Combine(currentDirectory, fileName),
                        Type = type == RgfConfig.GrdKeyword || type == GrdNetCdfKeyword ? GridType.GRD : GridType.FM
                    });
                }
            }

            return grids;
        }

        private static GridFile[] GetLastGridFilesFromConfigurationFile(string tempDir, int amountToTake)
        {
            // in case the user imports a grid in rgfgrid, it doesn't replace the original grid but is added as a 2nd grid.
            // we want to load back only as many grids as we had, so as a rule of thumb, we load back the last X, so only 
            // the last one in case of one grid (FM).

            try
            {
                string projFile = Path.Combine(tempDir, RgfGridConfigurationFileName);
                IEnumerable<GridFile> gridsInConfig = ParseRgfGridConfigurationFile(projFile);

                // take the last X grids saved (assume this indicates load order)
                return gridsInConfig
                       .Reverse()
                       .Take(amountToTake)
                       .Reverse()
                       .ToArray();
            }
            catch (Exception)
            {
                MessageBox.Show(
                    "Unable to parse rgfgrid.d3d configuration file. Unable to reload grid(s) with different names.",
                    "Unable to parse rgfgrid.d3d file.",
                    MessageBoxButtons.OK);
                return new GridFile[0];
            }
        }

        private static void CreateRgfGridConfigurationFile(string targetDir,
                                                           IEnumerable<string> gridFilePaths,
                                                           IEnumerable<string> additionalPaths,
                                                           IEnumerable<IPolygon> polygons,
                                                           string polFileName)
        {
            var config = new RgfConfig()
            {
                AdditionalGeometryPaths = additionalPaths.Select(Path.GetFileName).ToList(),
                Polygons = polygons?.ToList(),
                PolFileName = polFileName
            };
            config.AddGridFileNames(gridFilePaths.Select(Path.GetFileName).ToArray());

            new DelftIniWriter().WriteDelftIniFile(config.ToIniData(),
                                                   Path.Combine(targetDir, RgfGridConfigurationFileName));

            if (polygons != null && polFileName != null)
            {
                WritePolFile(polygons, targetDir, polFileName);
            }
        }

        private static void WritePolFile(IEnumerable<IPolygon> polygons, string targetDir, string fileName)
        {
            // write polygons file
            List<Feature2D> features = polygons.Select((p, i) => new Feature2D
            {
                Geometry = p,
                Name = "polyline" + i.ToString()
            }).ToList();
            var polFile = new PolFile<Feature2DPolygon> {IncludeClosingCoordinate = true};

            polFile.Write(Path.Combine(targetDir, fileName), features);
        }

        private class GridFile
        {
            public GridType Type { get; set; }
            public string FileName { get; set; }
            public string FilePath { get; set; }
        }
    }
}