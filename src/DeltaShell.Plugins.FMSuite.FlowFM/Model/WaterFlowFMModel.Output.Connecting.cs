using System;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Editing;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.IO.Readers;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
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
                    OutputHisFileStore = new FMHisFileFunctionStore(hisFilePath, CoordinateSystem, Area);
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