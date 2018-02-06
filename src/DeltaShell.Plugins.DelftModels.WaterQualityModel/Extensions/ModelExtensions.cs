using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using log4net;

using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Extentions
{
    public static class ModelExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ModelExtensions));
        
        /// <summary>
        /// Adds a text document to the <param name="model"/> output with the content of the file described by <param name="filePath"/>
        /// </summary>
        /// <param name="model">The water quality model to add the text document to</param>
        /// <param name="dataItemTag">The name of the output text document in the water quality model</param>
        /// <param name="filePath">The path to the file to read</param>
        /// <param name="insertIndex">The data item index at which the text document must be inserted</param>
        /// <remarks>The <paramref name="insertIndex"/> is ignored when a text document with <paramref name="dataItemTag"/> already exists</remarks>
        public static void AddTextDocument(this WaterQualityModel model, string dataItemTag, string filePath, int insertIndex = -1)
        {
            if (!File.Exists(filePath))
            {
                Log.WarnFormat("Could not add {0} ({1})", dataItemTag, filePath);
                return;
            }
            
            var dataItem = ((IModel) model).DataItems.FirstOrDefault(di => di.Tag == dataItemTag);
            if (dataItem == null)
            {
                var textDocumentFromFile = ((Func<string, TextDocumentBase>) CreateTextDocumentFromFile)(filePath);
                dataItem = new DataItem(textDocumentFromFile, WaterQualityModel.GetDataItemNameFromTag(dataItemTag), textDocumentFromFile.GetType(), DataItemRole.Output,
                    dataItemTag);

                if (insertIndex > ((IModel) model).DataItems.Count || insertIndex < 0)
                {
                    ((IModel) model).DataItems.Add(dataItem);
                }
                else
                {
                    ((IModel) model).DataItems.Insert(insertIndex, dataItem);
                }
            }
            else
            {
                ((Action<IDataItem>) UpdateTextFromFileDocument)(dataItem);
            }
        }

        private static void UpdateTextFromFileDocument(IDataItem dataItem)
        {
            var value = dataItem.Value as TextDocumentFromFile;
            if (value != null)
            {
                // Reopen the file but keep same name for DataItem:
                var originalDataItemName = dataItem.Name;

                var path = value.Path;
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
            return model.AllDataItems.Where(di => di.Role.HasFlag(DataItemRole.Output) && di.Value is UnstructuredGridCellCoverage)
                .Select(di => ((UnstructuredGridCellCoverage)di.Value));
        }
        
        /// <summary>
        /// Setups the model data folder structure.
        /// </summary>
        /// <param name="model">The model to be configured.</param>
        /// <param name="projectDataDir">The directory where the project data is stored (often *.dsproj_data).</param>
        public static void SetupModelDataFolderStructure(this WaterQualityModel model, string projectDataDir)
        {
            // Folder layout will be as follows:
            // +--- <projectDataDir>
            //     \--- <waq_model_name>_output          (Explicit work directory)
            //     +--- <waq_model_name>
            //         \--- output                       (Target where model output is written to)
            //         +--- boundary_data_tables         (Folder storing DataTables for boundaries)
            //         +--- load_data_tables             (Folder storing DataTables for loads)

            var name = string.IsNullOrWhiteSpace(model.Name) ? Path.GetTempFileName() : model.Name;
            model.ModelDataDirectory = Path.Combine(projectDataDir, name.Replace(" ", "_"));
            var modelWorkFolder = model.ModelDataDirectory + "_output";
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