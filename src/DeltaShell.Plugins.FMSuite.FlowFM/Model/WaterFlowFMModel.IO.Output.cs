using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Editing;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Model
{
    public partial class WaterFlowFMModel
    {
        #region Output

        public const string DiaFileDataItemTag = "DiaFile";

        #region (Dis)connect output

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

        protected virtual void ReconnectOutputFiles(string outputDirectory)
        {
            string mapFilePath = Path.Combine(outputDirectory, ModelDefinition.MapFileName);
            string hisFilePath = Path.Combine(outputDirectory, ModelDefinition.HisFileName);
            string classMapFilePath = Path.Combine(outputDirectory, ModelDefinition.ClassMapFileName);
            string waqFilePath = Path.Combine(outputDirectory, DelwaqOutputDirectoryName);
            string snappedFolderPath = Path.Combine(outputDirectory, SnappedFeaturesDirectoryName);

            ReconnectOutputFiles(mapFilePath, hisFilePath, classMapFilePath, waqFilePath, snappedFolderPath);
        }

        private void ReconnectOutputFiles(string mapFilePath, string hisFilePath, string classMapFilePath,
                                          string waqFolderPath, string snappedFolderPath, bool switchTo = false)
        {
            bool existsMapFile = File.Exists(mapFilePath);
            bool existsHisFile = File.Exists(hisFilePath);
            bool existsClassMapFile = File.Exists(classMapFilePath);
            bool existsWaqFolder = Directory.Exists(waqFolderPath);
            bool existsSnappedFolder = Directory.Exists(snappedFolderPath);

            if (!existsMapFile && !existsHisFile && !existsClassMapFile && !existsWaqFolder && !existsSnappedFolder)
            {
                return;
            }

            FireImportProgressChanged(this, "Reading output files - Reading Map file", 1, 2);
            BeginEdit(new DefaultEditAction("Reconnect output files"));

            // deal with issue that kernel doesn't understand any coordinate systems other than RD & WGS84 :
            if (existsMapFile)
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

            if (existsHisFile)
            {
                ReportProgressText("Reading his file");
                FireImportProgressChanged(this, "Reading output files - Reading His file", 1, 2);
                if (switchTo && OutputHisFileStore != null)
                {
                    OutputHisFileStore.Path = hisFilePath;
                }
                else
                {
                    var generalStructures = Area.Weirs.Where(w => w.WeirFormula is GeneralStructureWeirFormula);
                    var gates = Area.Weirs.Where(w => w.WeirFormula is GatedWeirFormula);
                    var simpleWeirs = Area.Weirs.Where(w => w.WeirFormula is SimpleWeirFormula);

                    var waterFlowFmModelDto = new WaterFlowFMModelDTO()
                    {
                        CoordinateSystem = this.CoordinateSystem,
                        ObservationPoints = Area.ObservationPoints,
                        ObservationCrossSections = Area.ObservationCrossSections,
                        SimpleWeirs = simpleWeirs,
                        GeneralStructureWeirs = generalStructures,
                        GatedWeirs = gates,
                        Pumps = Area.Pumps,
                    };
                    OutputHisFileStore = new FMHisFileFunctionStore(hisFilePath, waterFlowFmModelDto);
                }
            }

            if (existsClassMapFile)
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

            if (existsWaqFolder)
            {
                DelwaqOutputDirectoryPath = waqFolderPath;
            }

            if (existsSnappedFolder)
            {
                OutputSnappedFeaturesPath = snappedFolderPath;
            }

            OutputIsEmpty = false;

            EndEdit();
        }

        #endregion (Dis)connect output

        #region Save output files

        /// <summary>
        /// Saves the output by either moving or copying the source output to the target output directory.
        /// </summary>
        /// <remarks> When a file is locked, we report an error and return. </remarks>
        private void SaveOutput()
        {
            if (string.IsNullOrEmpty(currentOutputDirectoryPath))
            {
                return;
            }

            var sourceOutputDirectory = new DirectoryInfo(currentOutputDirectoryPath);
            if (!sourceOutputDirectory.Exists)
            {
                currentOutputDirectoryPath = PersistentOutputDirectoryPath;
                return;
            }

            var targetOutputDirectory = new DirectoryInfo(PersistentOutputDirectoryPath);
            string sourceOutputDirectoryPath = sourceOutputDirectory.FullName;
            string targetOutputDirectoryPath = targetOutputDirectory.FullName;

            bool sourceIsWorkingDir = sourceOutputDirectoryPath == WorkingOutputDirectoryPath;

            if (OutputIsEmpty && !HasOpenFunctionStores)
            {
                CleanDirectory(PersistentOutputDirectoryPath);

                if (sourceIsWorkingDir)
                {
                    CleanDirectory(WorkingDirectoryPath);
                }

                currentOutputDirectoryPath = PersistentOutputDirectoryPath;

                return;
            }

            if (sourceOutputDirectoryPath == targetOutputDirectoryPath)
            {
                return;
            }

            //copy all files and subdirectories from source directory "output" to persistent directory "output"
            if (!FileUtils.IsDirectoryEmpty(sourceOutputDirectoryPath))
            {
                FileUtils.CreateDirectoryIfNotExists(targetOutputDirectoryPath);

                if (sourceIsWorkingDir)
                {
                    List<string> lockedFiles = GetLockedFiles(WorkingDirectoryPath).ToList();

                    if (lockedFiles.Any())
                    {
                        ReportLockedFiles(lockedFiles);
                        return;
                    }

                    CleanDirectory(targetOutputDirectoryPath);
                    MoveAllContentDirectory(sourceOutputDirectory, targetOutputDirectoryPath);
                }
                else
                {
                    CleanDirectory(targetOutputDirectoryPath);
                    FileUtils.CopyAll(sourceOutputDirectory, targetOutputDirectory, string.Empty);
                }
            }

            string waqOutputDir = Path.Combine(PersistentOutputDirectoryPath, DelwaqOutputDirectoryName);
            string snappedOutputDir = Path.Combine(PersistentOutputDirectoryPath, SnappedFeaturesDirectoryName);
            ReconnectOutputFiles(MapFilePath, HisFilePath, ClassMapFilePath, waqOutputDir, snappedOutputDir, true);

            if (sourceIsWorkingDir)
            {
                CleanDirectory(WorkingDirectoryPath);
            }

            currentOutputDirectoryPath = PersistentOutputDirectoryPath;
        }

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
                        var textDocument = new TextDocument(true) { Name = diaFileName };
                        logDataItem = new DataItem(textDocument, DataItemRole.Output, DiaFileDataItemTag);
                        DataItems.Add(logDataItem);
                    }

                    string log = DiaFileReader.Read(diaFilePath);
                    ((TextDocument)logDataItem.Value).Content = log;
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

        private void MoveAllContentDirectory(DirectoryInfo sourceDirectory, string targetDirectoryPath)
        {
            foreach (FileInfo file in sourceDirectory.EnumerateFiles())
            {
                MoveFile(file, targetDirectoryPath);
            }

            bool onSameVolume = Directory.GetDirectoryRoot(sourceDirectory.FullName)
                                         .Equals(Directory.GetDirectoryRoot(targetDirectoryPath));

            foreach (DirectoryInfo directory in sourceDirectory.EnumerateDirectories())
            {
                MoveDirectory(directory, targetDirectoryPath, onSameVolume);
            }
        }

        private static void ReportLockedFiles(IEnumerable<string> filePaths)
        {
            string separator = Environment.NewLine + "- ";
            string lockedFilesMessage = separator + string.Join(separator, filePaths);
            Log.Error("There are one or more files locked, please close the following file(s) and save again:" +
                      lockedFilesMessage);
        }

        private IEnumerable<string> GetLockedFiles(string sourceDirectoryPath)
        {
            var sourceDirectory = new DirectoryInfo(sourceDirectoryPath);

            foreach (FileInfo file in sourceDirectory.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                string path = file.FullName;
                string parentDirectoryName = Path.GetFileName(Path.GetDirectoryName(path));

                // Snapped feature files are locked when the map in the GUI is open, so we ignore and copy snapped files instead.
                if (parentDirectoryName != SnappedFeaturesDirectoryName && FileUtils.IsFileLocked(path))
                {
                    yield return path;
                }
            }
        }

        private static void MoveFile(FileInfo file, string targetDirectoryPath)
        {
            string targetPath = Path.Combine(targetDirectoryPath, file.Name);
            file.MoveTo(targetPath);
        }

        private void MoveDirectory(DirectoryInfo sourceDirectoryInfo, string targetParentDirectoryPath,
                                   bool onSameVolume)
        {
            var targetDirectoryInfo =
                new DirectoryInfo(Path.Combine(targetParentDirectoryPath, sourceDirectoryInfo.Name));

            if (onSameVolume && sourceDirectoryInfo.Name != SnappedFeaturesDirectoryName)
            {
                sourceDirectoryInfo.MoveTo(targetDirectoryInfo.FullName);
            }
            else
            {
                FileUtils.CopyAll(sourceDirectoryInfo, targetDirectoryInfo, string.Empty);
            }
        }

        /// <summary>
        /// Removes all files and directories from the directory.
        /// </summary>
        /// <param name="directoryPath"> The directory path of the directory that needs to be cleaned. </param>
        private static void CleanDirectory(string directoryPath)
        {
            var directoryInfo = new DirectoryInfo(directoryPath);

            if (!directoryInfo.Exists)
            {
                return;
            }

            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo directory in directoryInfo.EnumerateDirectories())
            {
                try
                {
                    directory.Delete(true);
                }
                // Do NOT remove: when File Explorer is opened in the directory, an IO exeption is thrown.
                // There is no way of checking for this case, so we have to catch it. The second time it is called, it works fine.
                // https://stackoverflow.com/questions/329355/cannot-delete-directory-with-directory-deletepath-true
                catch (IOException)
                {
                    directory.Delete(true);
                }
            }
        }

        private void SetOutputDirAndWaqDirProperty()
        {
            WaterFlowFMProperty outputDirProperty = ModelDefinition.GetModelProperty(KnownProperties.OutputDir);

            string existingOutputDir = outputDirProperty.GetValueAsString();
            if (!existingOutputDir.StartsWith(OutputDirectoryName))
            {
                outputDirProperty.SetValueAsString(OutputDirectoryName);
                Log.InfoFormat("Running this model requires the OutputDirectory to be overwritten to: {0}",
                               OutputDirectoryName);
            }

            if (!SpecifyWaqOutputInterval)
            {
                return;
            }

            string relativeDWaqOutputDirectory = Path.Combine(OutputDirectoryName, DelwaqOutputDirectoryName);
            WaterFlowFMProperty waqOutputDirProperty = ModelDefinition.GetModelProperty(KnownProperties.WaqOutputDir);
            waqOutputDirProperty.SetValueAsString(relativeDWaqOutputDirectory);
        }

        private void ClearOutputDirAndWaqDirProperty()
        {
            ModelDefinition.GetModelProperty(KnownProperties.OutputDir).SetValueAsString(string.Empty);
            ModelDefinition.GetModelProperty(KnownProperties.WaqOutputDir).SetValueAsString(string.Empty);
        }

        #endregion Save output files

        #endregion Output
    }
}