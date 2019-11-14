using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Editing;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.IO.Readers;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Model
{
    public partial class WaterFlowFMModel
    {
        #region Implementation of IDimrModel

        /// <summary>
        /// Moves all content in the source directory into the target directory.
        /// </summary>
        /// <param name="sourceDirectory"> The source directory. </param>
        /// <param name="targetDirectoryPath"> The target directory path. </param>
        /// <remarks> <paramref name="sourceDirectory" /> should exist. </remarks>
        public virtual void ConnectOutput(string outputPath)
        {
            currentOutputDirectoryPath = outputPath;
            ReconnectOutputFiles(outputPath);
            ReadDiaFile(outputPath);
            ClearOutputDirAndWaqDirProperty();
        }

        /// <summary>
        /// Disconnects the output.
        /// </summary>
        public virtual void DisconnectOutput()
        {
            if (HasOpenFunctionStores)
            {
                BeginEdit(new DefaultEditAction("Disconnecting from output files"));

                if (OutputMapFileStore != null)
                {
                    OutputMapFileStore.Close();
                    OutputMapFileStore = null;
                }

                if (OutputHisFileStore != null)
                {
                    OutputHisFileStore.Close();
                    OutputHisFileStore = null;
                }

                if (OutputClassMapFileStore != null)
                {
                    OutputClassMapFileStore.Close();
                    OutputClassMapFileStore = null;
                }

                EndEdit();
            }

            OutputSnappedFeaturesPath = null;
        }

        /// <summary>
        /// Representation of the output directory for a D-Flow FM model.
        /// </summary>
        private class FmOutputDirectory
        {
            private readonly DirectoryInfo outputDirectoryInfo;

            /// <summary>
            /// Creates a new instance of <see cref="FmOutputDirectory"/>.
            /// </summary>
            /// <param name="directoryPath"></param>
            public FmOutputDirectory(string directoryPath)
            {
                outputDirectoryInfo = new DirectoryInfo(directoryPath);
            }

            /// <summary>
            /// Determines whether the output directory exists.
            /// </summary>
            public bool Exists => outputDirectoryInfo.Exists;

            /// <summary>
            /// Determines whether the output directory contains output.
            /// </summary>
            public bool ContainsOutput => File.Exists(MapFilePath)
                                          || File.Exists(HisFilePath)
                                          || File.Exists(ClassMapFilePath)
                                          || File.Exists(WaqOutputDirectoryPath)
                                          || File.Exists(SnappedOutputDirectoryPath);

            /// <summary>
            /// The file path to the map file.
            /// </summary>
            public string MapFilePath => FindFileThatEndsWith(outputDirectoryInfo.GetFiles(), FileConstants.MapFileExtension);

            /// <summary>
            /// The file path to the his file.
            /// </summary>
            public string HisFilePath => FindFileThatEndsWith(outputDirectoryInfo.GetFiles(), FileConstants.HisFileExtension);

            /// <summary>
            /// The file path to the class map file.
            /// </summary>
            public string ClassMapFilePath => FindFileThatEndsWith(outputDirectoryInfo.GetFiles(), FileConstants.ClassMapFileExtension);

            /// <summary>
            /// The path to the waq output directory.
            /// </summary>
            public string WaqOutputDirectoryPath => GetDirectoryPathStartingWith(FileConstants.PrefixDelwaqDirectoryName);

            /// <summary>
            /// The path to the snapped output directory.
            /// </summary>
            public string SnappedOutputDirectoryPath => GetDirectoryPathStartingWith(FileConstants.SnappedFeaturesDirectoryName);

            private string GetDirectoryPathStartingWith(string directoryNameStart)
            {
                return Directories.FirstOrDefault(d => d.Name.StartsWith(directoryNameStart, StringComparison.Ordinal))?.FullName;
            }

            private static string FindFileThatEndsWith(IEnumerable<FileInfo> files, string extension)
            {
                return files
                       .FirstOrDefault(f => f.Name.EndsWith(extension, StringComparison.Ordinal))?
                       .FullName;
            }

            private IEnumerable<DirectoryInfo> Directories => outputDirectoryInfo.GetDirectories();
        }

        protected virtual void ReconnectOutputFiles(string outputDirectoryPath, bool switchTo = false)
        {
            if (string.IsNullOrEmpty(outputDirectoryPath))
            {
                return;
            }

            var outputDirectory = new FmOutputDirectory(outputDirectoryPath);
            if (!outputDirectory.Exists || !outputDirectory.ContainsOutput)
            {
                return;
            }

            FireImportProgressChanged(this, "Reading output files - Reading Map file", 1, 2);
            BeginEdit(new DefaultEditAction("Reconnect output files"));

            ReconnectMapFile(outputDirectory.MapFilePath, switchTo);
            ReconnectHistoryFile(outputDirectory.HisFilePath, switchTo);
            ReconnectClassMapFile(outputDirectory.ClassMapFilePath, switchTo);
            ReconnectWaterQualityOutputDirectory(outputDirectory.WaqOutputDirectoryPath);
            ReconnectSnappedOutputDirectory(outputDirectory.SnappedOutputDirectoryPath);

            OutputIsEmpty = false;

            EndEdit();
        }

        private void ReconnectMapFile(string mapFilePath, bool switchTo)
        {
            // deal with issue that kernel doesn't understand any coordinate systems other than RD & WGS84 :
            if (mapFilePath != null)
            {
                ReportProgressText("Reading map file");
                ICoordinateSystem cs = UnstructuredGridFileHelper.GetCoordinateSystem(mapFilePath);

                // update map file coordinate system:
                if (CoordinateSystem != null && cs != CoordinateSystem)
                {
                    NetFile.WriteCoordinateSystem(mapFilePath, CoordinateSystem);
                }

                if (switchTo && OutputMapFileStore != null)
                {
                    OutputMapFileStore.Path = mapFilePath;
                }
                else
                {
                    OutputMapFileStore = new FMMapFileFunctionStore(this);
                    // don't change this to a property setter, because the timing is of great importance.
                    // elsewise, there will be no subscription to the read and Path triggers the Read().
                    OutputMapFileStore.Path = mapFilePath;
                }
            }
        }

        private void ReconnectHistoryFile(string hisFilePath, bool switchTo)
        {
            if (hisFilePath != null)
            {
                ReportProgressText("Reading his file");
                FireImportProgressChanged(this, "Reading output files - Reading His file", 1, 2);
                if (switchTo && OutputHisFileStore != null)
                {
                    OutputHisFileStore.Path = hisFilePath;
                }
                else
                {
                    OutputHisFileStore = new FMHisFileFunctionStore(hisFilePath, CoordinateSystem, Area);
                }
            }
        }

        private void ReconnectClassMapFile(string classMapFilePath, bool switchTo)
        {
            if (classMapFilePath != null)
            {
                ReportProgressText("Reading class map file");
                FireImportProgressChanged(this, "Reading output files - Reading Class Map file", 1, 2);
                if (switchTo && OutputClassMapFileStore != null)
                {
                    OutputClassMapFileStore.Path = classMapFilePath;
                }
                else
                {
                    OutputClassMapFileStore = new FMClassMapFileFunctionStore(classMapFilePath);
                }
            }
        }

        private void ReconnectWaterQualityOutputDirectory(string waqOutputDirectoryPath)
        {
            if (waqOutputDirectoryPath != null)
            {
                DelwaqOutputDirectoryPath = waqOutputDirectoryPath;
            }
        }

        private void ReconnectSnappedOutputDirectory(string snappedOutputDirectoryPath)
        {
            if (snappedOutputDirectoryPath != null)
            {
                OutputSnappedFeaturesPath = snappedOutputDirectoryPath;
            }
        }

        #endregion

        private void ReadDiaFile(string outputDirectory)
        {
            ReportProgressText("Reading dia file");
            string diaFileName = string.Format("{0}.dia", Name);

            string diaFilePath = Path.Combine(outputDirectory, diaFileName);
            if (File.Exists(diaFilePath))
            {
                try
                {
                    IDataItem logDataItem = DataItems.FirstOrDefault(di => di.Tag == DiaFileDataItemTag);
                    if (logDataItem == null)
                    {
                        // add logfile dataitem if not exists
                        var textDocument = new TextDocument(true) {Name = diaFileName};
                        logDataItem = new DataItem(textDocument, DataItemRole.Output, DiaFileDataItemTag);
                        DataItems.Add(logDataItem);
                    }

                    string log = DiaFileReader.Read(diaFilePath);
                    ((TextDocument) logDataItem.Value).Content = log;
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat(Resources.WaterFlowFMModel_ReadDiaFile_Error_reading_log_file___0____1_,
                                    diaFileName, ex.Message);
                }
            }
            else
            {
                Log.WarnFormat(
                    Resources.WaterFlowFMModel_ReadDiaFile_Could_not_find_log_file___0__at_expected_path___1_,
                    diaFileName, diaFilePath);
            }
        }
    }
}