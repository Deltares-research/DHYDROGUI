using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Collections;
using DeltaShell.Dimr;
using DeltaShell.Dimr.xsd;
using DeltaShell.NGHS.IO.Handlers;
using DeltaShell.Plugins.DelftModels.HydroModel.Properties;
using log4net;
using Newtonsoft.Json;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Import
{
    /// <summary>
    /// Converts the components information from a dimr file into sub-models of the Integrated model.
    /// </summary>
    public class HydroModelConverter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HydroModelConverter));

        private readonly ILogHandler logHandler;

        public HydroModelConverter(ILogHandler logHandler)
        {
            this.logHandler = logHandler;
        }

        /// <summary>
        /// Converts <see cref="dimrObject"/> to a <see cref="HydroModel"/> using the <param name="fileImporters"/> 
        /// for importing sub-models
        /// </summary>
        /// <param name="dimrObject">Parsed <see cref="dimrXML"/> object</param>
        /// <param name="path">Path to dimr.xml (used for finding sub-model folders)</param>
        /// <param name="fileImporters">List of file importers for importing sub-models</param>
        /// <returns>Converted <see cref="HydroModel"/></returns>
        /// <exception cref="ArgumentException">When <param name="dimrObject"/> is null</exception>
        public HydroModel Convert(dimrXML dimrObject, string path, IList<IDimrModelFileImporter> fileImporters)
        {
            if (dimrObject == null)
            {
                throw new ArgumentException(Resources.HydroModelConverter_Convert_Cannot_convert_empty_dimr_data_object);
            }

            var rootFolder = Path.GetDirectoryName(path);
            var hydroModel = new HydroModel();

            hydroModel.BeginEdit(new ImportingFullModelAction("Importing full Dimr model"));
            try
            {
                AddModels(hydroModel, fileImporters, dimrObject, rootFolder);

                var subModels = hydroModel.Activities.OfType<IDimrModel>().ToList();
                if (dimrObject.coupler != null) CoupleSubModels(dimrObject, subModels);
            }
            finally
            {
                hydroModel.EndEdit();
            }

            return hydroModel;
        }

        private void AddModels(HydroModel hydroModel, ICollection<IDimrModelFileImporter> fileImporters, dimrXML dimrObject, string rootFolder)
        {
            var componentGroups = dimrObject.component
                .GroupBy(component => Path.GetExtension(component.inputFile)?.TrimStart('.'));

            foreach (var componentGroup in componentGroups)
            {
                var extension = GetExtension(componentGroup);
                var importer = fileImporters.FirstOrDefault(f => f.MasterFileExtension == extension);
                if (importer == null)
                {
                    LogUnknownImporter(extension);
                    continue;
                }

                importer.ProgressChanged = (name, step, steps) => { };

                foreach (var component in componentGroup)
                {
                    var filePath = GetFilePath(rootFolder, component.inputFile, importer);
                    var importedItem = importer.ImportItem(Path.GetFullPath(filePath));

                    if (!(importedItem is IActivity subModel))
                    {
                        logHandler.ReportErrorFormat(Resources.HydroModelConverter_AddModels_Could_not_import_sub_model_defined_at_location__0__to_integrated_model_, filePath);
                        continue;
                    }

                    RenameSubModelWhenNeeded(subModel, component.name);
                    SetHydroModelProperties(hydroModel, subModel);
                }
            }
        }

        private string GetExtension(IGrouping<string, dimrComponentXML> componentGroup)
        {
            var extension = componentGroup.Key;

            if (string.IsNullOrEmpty(extension))
            {
                extension = "json";
            }

            return extension;
        }

        private void LogUnknownImporter(string extension)
        {
            logHandler.ReportInfo($"No importer found for extension: {extension}");
        }

        private string GetFilePath(string rootFolder, string inputFileName, IDimrModelFileImporter importer)
        {
            var fileName = GetFileName(inputFileName);
            var filePath = ComposeFilePath(rootFolder, fileName, importer);
            return filePath;
        }

        private string GetFileName(string fileName)
        {
            return fileName.Equals(".") 
                ? "settings.json"
                : fileName;
        }

        private string ComposeFilePath(string rootFolder, string fileName, IDimrModelFileImporter importer)
        {
            string[] pathParts;

            if (importer.MasterFileExtension.Equals("json"))
            {
                var subFolder = importer.SubFolders.First();
                var pathToFile = Path.Combine(rootFolder, subFolder, fileName);
                var file = File.ReadAllText(pathToFile);
                var fileObject = JsonConvert.DeserializeObject<RtcXmlDirectoryLookup>(file);

                pathParts = new[] { rootFolder }
                    .Concat(importer.SubFolders)
                    .Plus(fileObject.XmlDirectory)
                    .ToArray();
            }
            else
            {
                pathParts = new[] {rootFolder}
                    .Concat(importer.SubFolders)
                    .Plus(fileName)
                    .ToArray();
            }

            return Path.Combine(pathParts);
        }

        private void RenameSubModelWhenNeeded(IActivity subModel, string componentName)
        {
            if (subModel.Name == componentName) return;

            Log.DebugFormat(Resources.HydroModelConverter_AddModels_Renamed_model__0__to__1_, subModel.Name, componentName);
            subModel.Name = componentName;
        }

        private void SetHydroModelProperties(HydroModel hydroModel, IActivity subModel)
        {
            if (subModel is IHydroModel hydroModelSubModel)
            {
                hydroModel.Region.SubRegions.Add(hydroModelSubModel.Region);
            }

            hydroModel.Activities.Add(subModel);
        }

        private void CoupleSubModels(dimrXML dimrObject, IList<IDimrModel> subModels)
        {
            foreach (var dimrCouplerXml in dimrObject.coupler)
            {
                var sourceModel = subModels.FirstOrDefault(m => m.Name == dimrCouplerXml.sourceComponent);
                var targetModel = subModels.FirstOrDefault(m => m.Name == dimrCouplerXml.targetComponent);

                if (sourceModel == null || targetModel == null)
                {
                    logHandler.ReportErrorFormat(Resources.HydroModelConverter_CoupleSubModels_Could_not_couple_models____0___to___1___,
                        dimrCouplerXml.sourceComponent, dimrCouplerXml.targetComponent);
                    continue;
                }

                CoupleModelsByDimrCouplerXml(sourceModel, targetModel, dimrCouplerXml.item);
            }
        }

        private void CoupleModelsByDimrCouplerXml(IDimrModel sourceModel, IDimrModel targetModel, dimrCoupledItemXML[] dimrCouplerXml)
        {
            foreach (var couplerXml in dimrCouplerXml)
            {
                try
                {
                    if (string.IsNullOrEmpty(couplerXml.sourceName) || string.IsNullOrEmpty(couplerXml.targetName))
                    {
                        logHandler.ReportError($"Could not link an item from {sourceModel.Name} to {targetModel.Name}");
                        continue;
                    }
                    
                    var sourceDataItem = sourceModel.GetDataItemByItemString(couplerXml.sourceName);
                    var targetDataItem = targetModel.GetDataItemByItemString(couplerXml.targetName);

                    if (sourceDataItem == null || targetDataItem == null)
                    {
                        logHandler.ReportError($"Could not link {couplerXml.sourceName} to {couplerXml.targetName}");
                        continue;
                    }

                    targetDataItem.LinkTo(sourceDataItem);
                }
                catch (Exception e) when (e is NotImplementedException ||
                                          e is ArgumentException)
                {
                    logHandler.ReportError(
                        $"Could not link {couplerXml.sourceName} to {couplerXml.targetName} : {e.Message}");
                }
            }
        }
    }

}