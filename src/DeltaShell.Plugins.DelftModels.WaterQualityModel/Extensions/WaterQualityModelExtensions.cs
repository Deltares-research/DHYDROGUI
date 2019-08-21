using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataItemMetaData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using log4net;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Extensions
{
    public static class WaterQualityModelExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterQualityModelExtensions));

        /// <summary>
        /// Adds a text document to the
        /// <param name="model" />
        /// output with the content of the file described by
        /// <param name="filePath" />
        /// </summary>
        /// <param name="model"> The water quality model to add the text document to </param>
        /// <param name="dataItemMetaData"> The metadata object that provides information about the data item to be created </param>
        /// <param name="filePath"> The path to the file to read </param>
        /// <param name="insertIndex"> The data item index at which the text document must be inserted </param>
        /// <remarks>
        /// The <paramref name="insertIndex" /> is ignored when a text document with <paramref name="dataItemMetaData" />
        /// already exists
        /// </remarks>
        public static void AddTextDocument(this WaterQualityModel model, ADataItemMetaData dataItemMetaData,
                                           string filePath, int insertIndex = -1)
        {
            if (!File.Exists(filePath))
            {
                Log.WarnFormat("Could not add {0} ({1})", dataItemMetaData.Name, filePath);
                return;
            }

            IDataItem dataItem = ((IModel) model).DataItems.FirstOrDefault(di => di.Tag == dataItemMetaData.Tag);
            if (dataItem == null)
            {
                TextDocumentBase textDocumentFromFile =
                    ((Func<string, TextDocumentBase>) CreateTextDocumentFromFile)(filePath);
                dataItem = new DataItem(textDocumentFromFile, dataItemMetaData.Name, textDocumentFromFile.GetType(),
                                        DataItemRole.Output,
                                        dataItemMetaData.Tag);

                if (insertIndex > ((IModel) model).DataItems.Count || insertIndex < 0)
                {
                    ((IModel) model).DataItems.Add(dataItem);
                }
                else
                {
                    ((IModel) model).DataItems.Insert(insertIndex, dataItem);
                }

                CleanupInvalidFiles(filePath, dataItem);
            }
            else
            {
                UpdateTextFromFileDocument(dataItem);
            }
        }

        /// <summary>
        /// Connects the output map files to the <paramref name="model"/>.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <remarks>
        /// If both the binary and NetCDF file exist in the output directory,
        /// then the NetCDF file is connected to the model, except when the
        /// convention is not supported.
        /// </remarks>
        public static void ConnectMapOutput(this WaterQualityModel model)
        {
            string outputDirectory = model.ModelSettings.OutputDirectory;

            string mapFilePath = Path.Combine(outputDirectory, "deltashell.map");
            if (File.Exists(mapFilePath))
            {
                model.MapFileFunctionStore.Path = mapFilePath;
            }

            string mapNetCdfFilePath = Path.Combine(outputDirectory, "deltashell_map.nc");
            if (File.Exists(mapNetCdfFilePath))
            {
                if (!NetCdfFileConventionChecker.HasSupportedConvention(mapNetCdfFilePath))
                {
                    Log.WarnFormat(Resources.WaterQualityModel_File_does_not_meet_supported_UGRID_1_0_or_newer_standard, Path.GetFileName(mapNetCdfFilePath));
                }
                else
                {
                    model.MapFileFunctionStore.Path = mapNetCdfFilePath;
                }
            }
        }

        private static void CleanupInvalidFiles(string filePath, IDataItem dataItem)
        {
            //D3DFMIQ-76
            /* When adding a TextDocumentFromFile - DataItem to the DataItems collection,
             * it gets updated with a temporary file that should not be created.
             * To avoid this behaviour we do hereby a cleanup of such files.
             */
            if (dataItem == null
                || (TextDocumentFromFile) dataItem.Value == null
                || ((TextDocumentFromFile) dataItem.Value).Path == filePath)
            {
                return;
            }

            string fileToRemove = ((TextDocumentFromFile) dataItem.Value).Path;
            ((TextDocumentFromFile) dataItem.Value).Path = filePath;
            File.Delete(fileToRemove);
        }

        private static void UpdateTextFromFileDocument(IDataItem dataItem)
        {
            var value = dataItem.Value as TextDocumentFromFile;
            if (value != null)
            {
                // Reopen the file but keep same name for DataItem:
                string originalDataItemName = dataItem.Name;

                string path = value.Path;
                value.Close();
                value.Open(path);

                dataItem.Name = originalDataItemName;
            }
        }

        private static TextDocumentFromFile CreateTextDocumentFromFile(string filePath)
        {
            var textDocumentFromFile = new TextDocumentFromFile(true);
            textDocumentFromFile.Open(filePath);
            return textDocumentFromFile;
        }

        /// <summary>
        /// Returns all output coverages of this model.
        /// </summary>
        public static IEnumerable<UnstructuredGridCellCoverage> GetOutputCoverages(this WaterQualityModel model)
        {
            return model.AllDataItems
                        .Where(di => di.Role.HasFlag(DataItemRole.Output) && di.Value is UnstructuredGridCellCoverage)
                        .Select(di => (UnstructuredGridCellCoverage) di.Value);
        }

        /// <summary>
        /// Setups the model data folder structure.
        /// </summary>
        /// <param name="model"> The model to be configured. </param>
        /// <param name="projectDataDir"> The directory where the project data is stored (often *.dsproj_data). </param>
        public static void SetupModelDataFolderStructure(this WaterQualityModel model, string projectDataDir)
        {
            // Folder layout will be as follows:
            // +--- <projectDataDir>
            //     \--- <waq_model_name>_output          (Explicit work directory)
            //     +--- <waq_model_name>
            //         \--- output                       (Target where model output is written to)
            //         +--- boundary_data_tables         (Folder storing DataTables for boundaries)
            //         +--- load_data_tables             (Folder storing DataTables for loads)

            string name = string.IsNullOrWhiteSpace(model.Name) ? Path.GetTempFileName() : model.Name;
            model.ModelDataDirectory = Path.Combine(projectDataDir, name.Replace(" ", "_"));
            string modelWorkFolder = model.ModelDataDirectory + "_output";
            if (model.ModelSettings != null)
            {
                model.ModelSettings.WorkDirectory = modelWorkFolder;
                model.ModelSettings.OutputDirectory = Path.Combine(model.ModelDataDirectory, "output");
            }

            if (model.BoundaryDataManager != null)
            {
                model.BoundaryDataManager.FolderPath = Path.Combine(model.ModelDataDirectory, "boundary_data_tables");
            }

            if (model.LoadsDataManager != null)
            {
                model.LoadsDataManager.FolderPath = Path.Combine(model.ModelDataDirectory, "load_data_tables");
            }
        }
    }
}