using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataItemMetaData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using log4net;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Extensions
{
    public static class WaterQualityModelExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterQualityModelExtensions));

        /// <summary>
        /// Adds a text document to the
        /// <param name="model"/>
        /// output with the content of the file described by
        /// <param name="filePath"/>
        /// </summary>
        /// <param name="model"> The water quality model to add the text document to </param>
        /// <param name="dataItemMetaData"> The metadata object that provides information about the data item to be created </param>
        /// <param name="filePath"> The path to the file to read </param>
        public static void AddTextDocument(this WaterQualityModel model, ADataItemMetaData dataItemMetaData, string filePath)
        {
            if (!File.Exists(filePath))
            {
                Log.WarnFormat("Could not add {0} ({1})", dataItemMetaData.Name, filePath);
                return;
            }

            string fileContent = File.ReadAllText(filePath);

            IDataItem dataItem = model.DataItems.FirstOrDefault(di => di.Tag == dataItemMetaData.Tag);
            if (dataItem == null)
            {
                var textDocument = new TextDocument(true) {Content = fileContent};
                dataItem = new DataItem(textDocument, dataItemMetaData.Name, textDocument.GetType(),
                                        DataItemRole.Output,
                                        dataItemMetaData.Tag);

                model.DataItems.Add(dataItem);
            }
            else
            {
                if (dataItem.Value is TextDocument textDocument)
                {
                    textDocument.Content = fileContent;
                }
            }
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

            string name = string.IsNullOrWhiteSpace(model.Name) ? Path.GetRandomFileName() : model.Name;
            model.ModelDataDirectory = Path.Combine(projectDataDir, name.Replace(" ", "_"));
            if (model.ModelSettings != null)
            {
                model.ModelSettings.OutputDirectory =
                    Path.Combine(model.ModelDataDirectory, FileConstants.OutputDirectoryName);
            }

            if (model.BoundaryDataManager != null)
            {
                model.BoundaryDataManager.FolderPath =
                    Path.Combine(model.ModelDataDirectory, FileConstants.BoundaryDataDirectoryName);
            }

            if (model.LoadsDataManager != null)
            {
                model.LoadsDataManager.FolderPath =
                    Path.Combine(model.ModelDataDirectory, FileConstants.LoadsDataDirectoryName);
            }
        }
    }
}