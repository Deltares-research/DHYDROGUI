using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DeltaShell.NGHS.Common.Extensions;
using DeltaShell.NGHS.Common.IO.RestartFiles;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public partial class WaterFlowFMModel
    {
        private void ReadDiaFile(string diaFilePath)
        {
            ReportProgressText("Reading dia file");
            FireImportProgressChanged(Resources.WaterFlowFMModel_ReadDiaFile_Reading_output_files___Reading_dia_file);
            string diaFileName = Path.GetFileName(diaFilePath);
            if (File.Exists(diaFilePath))
            {
                try
                {
                    IDataItem logDataItem = DataItems.FirstOrDefault(di => di.Tag == WaterFlowFMModelDataSet.DiaFileDataItemTag);
                    if (logDataItem == null)
                    {
                        // add logfile dataitem if not exists
                        var textDocument = new TextDocument(true) { Name = diaFileName };
                        logDataItem = new DataItem(textDocument, DataItemRole.Output, WaterFlowFMModelDataSet.DiaFileDataItemTag);
                        DataItems.Add(logDataItem);
                    }

                    string log = DiaFileReader.Read(diaFilePath);
                    ((TextDocument)logDataItem.Value).Content = log;
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat(Resources.WaterFlowFMModel_ReadDiaFile_Error_reading_log_file___0____1_, diaFileName, ex.Message);
                }
            }
            else
            {
                Log.WarnFormat(Resources.WaterFlowFMModel_ReadDiaFile_Could_not_find_log_file___0__at_expected_path___1_, diaFileName, diaFilePath);
            }
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
            
            FireImportProgressChanged(Resources.WaterFlowFMModel_ReconnectOutputFiles_Reading_output_files);

            using (this.InEditMode(DelftTools.Hydro.Properties.Resources.Reconnect_output_files_edit_action))
            {
                ReadDiaFile(outputDirectory.DiaFilePath);
                ReconnectMapFile(outputDirectory.MapFilePath, switchTo);
                ReconnectHistoryFile(outputDirectory.HisFilePath, switchTo);
                ReconnectClassMapFile(outputDirectory.ClassMapFilePath, switchTo);
                ReconnectFouFile(outputDirectory.FouFilePath, switchTo);
                ReconnectWaterQualityOutputDirectory(outputDirectory.WaqOutputDirectoryPath);
                ReconnectSnappedOutputDirectory(outputDirectory.SnappedOutputDirectoryPath);
                ReconnectRestartFiles(outputDirectory.RestartFilePaths);
                FireImportProgressChanged(Resources.WaterFlowFMModel_ReconnectOutputFiles_Reading_output_files___done);
                ReportProgressText();

                OutputIsEmpty = false;
            }
        }

        private void ReconnectMapFile(string mapFilePath, bool switchTo)
        {
            // deal with issue that kernel doesn't understand any coordinate systems other than RD & WGS84 :
            if (mapFilePath != null)
            {
                ReportProgressText("Reading map file");

                ICoordinateSystem cs;
                using (var ugridFile = new UGridFile(mapFilePath))
                {
                    cs = ugridFile.ReadCoordinateSystem();
                }

                // update map file coordinate system:
                if (!Grid.IsEmpty)
                {
                    FireImportProgressChanged(Resources.WaterFlowFMModel_ReconnectMapFile_Reading_output_files___Reading_map_2d_file);
                    if (CoordinateSystem != null && cs != CoordinateSystem)
                        NetFile.WriteCoordinateSystem(mapFilePath, CoordinateSystem);
                    if (switchTo && OutputMapFileStore != null)
                    {
                        OutputMapFileStore.SetPathWithoutLoadingData(mapFilePath);
                        Log.Debug($"Set the path of the output 2D map file function store to: {mapFilePath}");
                    }
                    else
                    {
                        OutputMapFileStore = new FMMapFileFunctionStore();

                        // don't change this to a property setter, because the timing is of great importance.
                        // elsewise, there will be no subscription to the read and Path triggers the Read().
                        try
                        {
                            Log.Debug($"Begin loading the output 2D map file data from: {mapFilePath}");
                            OutputMapFileStore.Path = mapFilePath;
                            Log.Debug($"End loading the output 2D map file data from: {mapFilePath}");
                        }
                        catch (Exception e)
                        {
                            Log.Error($"Error reading map file {e.Message}");
                            OutputMapFileStore = null;
                        }
                    }
                }

                if (Network != null && !Network.IsEdgesEmpty && !Network.IsVerticesEmpty)
                {
                    FireImportProgressChanged(Resources.WaterFlowFMModel_ReconnectMapFile_Reading_output_files___Reading_map_1d_file);

                    if (switchTo && Output1DFileStore != null)
                    {
                        Output1DFileStore.SetPathWithoutLoadingData(mapFilePath);
                        Log.Debug($"Set the path of the output 1D map file function store to: {mapFilePath}");
                    }
                    else
                    {
                        Output1DFileStore = new FM1DFileFunctionStore(Network);
                        // don't change this to a property setter, because the timing is of great importance.
                        // elsewise, there will be no subscription to the read and Path triggers the Read().
                        Log.Debug($"Begin loading the output 1D map file data from: {mapFilePath}");
                        Output1DFileStore.Path = mapFilePath;
                        Log.Debug($"End loading the output 1D map file data from: {mapFilePath}");
                    }
                }
            }
        }

        private void ReconnectHistoryFile(string hisFilePath, bool switchTo)
        {
            if (OutputMapFileStore != null && OutputMapFileStore.Grid == null)
            {
                Log.Warn("Associated output files are unsupported, these will not be loaded");
                OutputMapFileStore = null;
                return;
            }

            if (hisFilePath != null)
            {
                ReportProgressText("Reading his file");
                FireImportProgressChanged(Resources.WaterFlowFMModel_ReconnectHistoryFile_Reading_output_files___Reading_His_file);
                if (switchTo && OutputHisFileStore != null)
                {
                    OutputHisFileStore.SetPathWithoutLoadingData(hisFilePath);
                    Log.Debug($"Set the path of the output his file function store to: {hisFilePath}");
                }
                else
                {
                    OutputHisFileStore = new FMHisFileFunctionStore(Network, Area);
                    Log.Debug($"Begin loading the output his file data from: {hisFilePath}");
                    OutputHisFileStore.Path = hisFilePath;
                    Log.Debug($"End loading the output his file data from: {hisFilePath}");
                    OutputHisFileStore.CoordinateSystem = CoordinateSystem;
                }
            }
        }

        private void ReconnectClassMapFile(string classMapFilePath, bool switchTo)
        {
            if (classMapFilePath == null)
            {
                return;
            }

            ReportProgressText("Reading class map file");
            FireImportProgressChanged(Resources.WaterFlowFMModel_ReconnectClassMapFile_Reading_output_files___Reading_Class_Map_file);
            if (switchTo && OutputClassMapFileStore != null)
            {
                OutputClassMapFileStore.SetPathWithoutLoadingData(classMapFilePath);
                Log.Debug($"Set the path of the output class map file function store to: {classMapFilePath}");
            }
            else
            {
                Log.Debug($"Begin loading the output class map file data from: {classMapFilePath}");
                OutputClassMapFileStore = new FMClassMapFileFunctionStore(classMapFilePath);
                Log.Debug($"End loading the output class map file data from: {classMapFilePath}");
            }
        }

        private void ReconnectFouFile(string fouFilePath, bool switchTo)
        {
            if (fouFilePath == null)
            {
                return;
            }

            ReportProgressText("Reading fou file");
            FireImportProgressChanged(Resources.WaterFlowFMModel_ReconnectFouFile_Reading_output_files___Reading_Fou_file);
            if (switchTo && OutputFouFileStore != null)
            {
                OutputFouFileStore.Path = fouFilePath;
            }
            else
            {
                OutputFouFileStore = new FouFileFunctionStore {Path = fouFilePath};
            }
        }

        private void ReconnectWaterQualityOutputDirectory(string waqOutputDirectoryPath)
        {
            FireImportProgressChanged(Resources.WaterFlowFMModel_ReconnectWaterQualityOutputDirectory_Reading_output_files___Reconnect_WAQ_output_dir); 
            if (waqOutputDirectoryPath != null)
            {
                DelwaqOutputDirectoryPath = waqOutputDirectoryPath;
            }
        }
        
        private void ReconnectSnappedOutputDirectory(string snappedOutputDirectoryPath)
        {
            FireImportProgressChanged(Resources.WaterFlowFMModel_ReconnectSnappedOutputDirectory_Reading_output_files___Reconnect_snapped_output_dir);
            if (snappedOutputDirectoryPath != null)
            {
                OutputSnappedFeaturesPath = snappedOutputDirectoryPath;
            }
        }
        
        private void ReconnectRestartFiles(IEnumerable<string> restartFilePaths)
        {
            FireImportProgressChanged(Resources.WaterFlowFMModel_ReconnectRestartFiles_Reading_output_files___connect_restart_files);
            RestartOutput = restartFilePaths.Select(p => new RestartFile(p)).ToList();
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
            public bool ContainsOutput => File.Exists(DiaFilePath)
                                          || File.Exists(MapFilePath)
                                          || File.Exists(HisFilePath)
                                          || File.Exists(ClassMapFilePath)
                                          || File.Exists(WaqOutputDirectoryPath)
                                          || File.Exists(SnappedOutputDirectoryPath)
                                          || RestartFilePaths.Any();

            /// <summary>
            /// The file path to the diagnostics file.
            /// </summary>
            /// <remarks> Returns null in case the file was not found. </remarks>
            public string DiaFilePath => FindFileThatEndsWith(FileConstants.DiaFileExtension);
            
            /// <summary>
            /// The file path to the map file.
            /// </summary>
            /// <remarks> Returns null in case the file was not found. </remarks>
            public string MapFilePath => FindFileThatEndsWith(FileConstants.MapFileExtension);

            /// <summary>
            /// The file path to the his file.
            /// </summary>
            /// <remarks> Returns null in case the file was not found. </remarks>
            public string HisFilePath => FindFileThatEndsWith(FileConstants.HisFileExtension);

            /// <summary>
            /// The file path to the class map file.
            /// </summary>
            /// <remarks> Returns null in case the file was not found. </remarks>
            public string ClassMapFilePath => FindFileThatEndsWith(FileConstants.ClassMapFileExtension);

            /// <summary>
            /// The file path to the fou file.
            /// </summary>
            /// <remarks> Returns null in case the file was not found. </remarks>
            public string FouFilePath => FindFileThatEndsWith(FileConstants.FouFileExtension); 

            /// <summary>
            /// The path to the waq output directory.
            /// </summary>
            /// <remarks> Returns null in case the directory was not found. </remarks>
            public string WaqOutputDirectoryPath => GetDirectoryPathStartingWith(FileConstants.PrefixDelwaqDirectoryName);

            /// <summary>
            /// The path to the snapped output directory.
            /// </summary>
            /// <remarks> Returns null in case the directory was not found. </remarks>
            public string SnappedOutputDirectoryPath => GetDirectoryPathStartingWith(FileConstants.SnappedFeaturesDirectoryName);

            /// <summary>
            /// The paths of the restart files.
            /// </summary>
            public IEnumerable<string> RestartFilePaths => FindFilesThatEndWith(FileConstants.RestartFileExtension);

            private string GetDirectoryPathStartingWith(string directoryNameStart)
            {
                return outputDirectoryInfo.EnumerateDirectories()
                                          .FirstOrDefault(d => d.Name.StartsWith(directoryNameStart, StringComparison.Ordinal))?
                                          .FullName;
            }

            private string FindFileThatEndsWith(string extension)
            {
                return outputDirectoryInfo.EnumerateFiles()
                                          .FirstOrDefault(f => f.Name.EndsWith(extension, StringComparison.Ordinal))?
                                          .FullName;
            }

            private IEnumerable<string> FindFilesThatEndWith(string extension)
            {
                return outputDirectoryInfo.EnumerateFiles()
                                          .Where(f => f.Name.EndsWith(extension, StringComparison.Ordinal))?
                                          .Select(f => f.FullName);
            }
        }
    }
}