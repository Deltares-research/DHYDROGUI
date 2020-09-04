using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.Dimr;
using DeltaShell.Dimr.xsd;
using DeltaShell.NGHS.Common;
using DeltaShell.NGHS.Common.Logging;
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
        /// Converts <see cref="dimrObject"/> to a <see cref="HydroModel"/> using the
        /// <param name="fileImporters"/>
        /// for importing sub-models
        /// </summary>
        /// <param name="dimrObject">Parsed <see cref="dimrXML"/> object</param>
        /// <param name="path">Path to dimr.xml (used for finding sub-model folders)</param>
        /// <param name="fileImporters">List of file importers for importing sub-models</param>
        /// <returns>Converted <see cref="HydroModel"/></returns>
        /// <exception cref="ArgumentException">
        /// When
        /// <param name="dimrObject"/>
        /// is null
        /// </exception>
        public HydroModel Convert(dimrXML dimrObject, string path, IList<IDimrModelFileImporter> fileImporters)
        {
            if (dimrObject == null)
            {
                throw new ArgumentException(Resources.HydroModelConverter_Convert_Cannot_convert_empty_dimr_data_object);
            }

            string rootFolder = Path.GetDirectoryName(path);
            var hydroModel = new HydroModel();

            hydroModel.BeginEdit(new ImportingFullModelAction("Importing full Dimr model"));
            try
            {
                AddModels(hydroModel, fileImporters, dimrObject, rootFolder);

                List<IDimrModel> subModels = hydroModel.Activities.OfType<IDimrModel>().ToList();
                if (dimrObject.coupler != null)
                {
                    CoupleSubModels(dimrObject, subModels);
                }
            }
            finally
            {
                hydroModel.EndEdit();
            }

            return hydroModel;
        }

        private void AddModels(HydroModel hydroModel, ICollection<IDimrModelFileImporter> fileImporters, dimrXML dimrObject, string rootFolder)
        {
            foreach (dimrComponentXML component in dimrObject.component)
            {
                ValidateComponent(component);
            }

            IEnumerable<IGrouping<string, dimrComponentXML>> componentGroups = dimrObject.component
                                                                                         .GroupBy(component => Path.GetExtension(component.inputFile)?.TrimStart('.'));

            foreach (IGrouping<string, dimrComponentXML> componentGroup in componentGroups)
            {
                string extension = GetExtension(componentGroup);
                IDimrModelFileImporter importer = fileImporters.FirstOrDefault(f => f.MasterFileExtension == extension);
                if (importer == null)
                {
                    LogUnknownImporter(extension);
                    continue;
                }

                importer.ProgressChanged = (name, step, steps) => {};

                foreach (dimrComponentXML component in componentGroup)
                {
                    string filePath = GetFilePath(rootFolder, component.workingDir.Trim(), component.inputFile.Trim(), importer);

                    if (filePath == null)
                    {
                        continue;
                    }

                    object importedItem = importer.ImportItem(Path.GetFullPath(filePath));

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

        private static void ValidateComponent(dimrComponentXML component)
        {
            if (string.IsNullOrEmpty(component.workingDir))
            {
                throw new ArgumentException(string.Format(Resources.HydroModelConverter_AddModels_The_working_directory_is_missing_for_component__0__in_the_dimr_xml_,
                                                          component.name));
            }

            if (component.inputFile == null)
            {
                throw new ArgumentException(string.Format(Resources.HydroModelConverter_AddModels_The_input_file_is_missing_for_component__0__in_the_dimr_xml_,
                                                          component.name));
            }
        }

        private string GetExtension(IGrouping<string, dimrComponentXML> componentGroup)
        {
            string extension = componentGroup.Key;

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

        private string GetFilePath(string rootFolder, string workingDirectory, string inputFileName,
                                   IDimrModelFileImporter importer)
        {
            string fileName = GetFileName(inputFileName);
            string filePath = ComposeFilePath(rootFolder, workingDirectory, fileName, importer);
            return filePath;
        }

        private string GetFileName(string fileName)
        {
            return fileName.Equals(".")
                       ? "settings.json"
                       : fileName;
        }

        private string ComposeFilePath(string rootFolder, string workingDirectory, string fileName, IDimrModelFileImporter importer)
        {
            string[] pathParts;

            if (importer.MasterFileExtension.Equals("json"))
            {
                string pathToFile = Path.Combine(rootFolder, workingDirectory, fileName);
                string file = File.ReadAllText(pathToFile);
                var fileObject = JsonConvert.DeserializeObject<RtcXmlDirectoryLookup>(file);
                string xmlDirectory = fileObject.XmlDirectory;

                if (xmlDirectory == null)
                {
                    logHandler.ReportError(Resources.HydroModelConverter_ComposeFilePath_Could_not_import_RTC_model_the_settings_json_file_should_contain_an_xml_directory_);
                    return null;
                }

                pathParts = new[] {rootFolder, workingDirectory, xmlDirectory};
            }
            else
            {
                pathParts = new[] {rootFolder, workingDirectory, fileName};
            }

            return Path.Combine(pathParts);
        }

        private void RenameSubModelWhenNeeded(IActivity subModel, string componentName)
        {
            if (subModel.Name == componentName)
            {
                return;
            }

            Log.DebugFormat(Resources.HydroModelConverter_AddModels_Renamed_model__0__to__1_, subModel.Name, componentName);
            subModel.Name = componentName;
        }

        private void SetHydroModelProperties(HydroModel hydroModel, IActivity subModel)
        {
            if (subModel is IHydroModel hydroModelSubModel)
            {
                if (hydroModelSubModel.Region != null)
                {
                    hydroModel.Region.SubRegions.Add(hydroModelSubModel.Region);
                }
            }

            hydroModel.Activities.Add(subModel);
        }

        private void CoupleSubModels(dimrXML dimrObject, IList<IDimrModel> subModels)
        {
            foreach (dimrCouplerXML dimrCouplerXml in dimrObject.coupler)
            {
                IDimrModel sourceModel = subModels.FirstOrDefault(m => m.Name == dimrCouplerXml.sourceComponent);
                IDimrModel targetModel = subModels.FirstOrDefault(m => m.Name == dimrCouplerXml.targetComponent);

                if (sourceModel == null || targetModel == null)
                {
                    logHandler.ReportErrorFormat(Resources.HydroModelConverter_CoupleSubModels_Could_not_couple_models____0___to___1___,
                                                 dimrCouplerXml.sourceComponent, dimrCouplerXml.targetComponent);
                    continue;
                }

                CoupleModelsByDimrCouplerXml(sourceModel, targetModel, dimrCouplerXml.item);
            }

            foreach (ILinkedDataItemsModel linkedModel in subModels.OfType<ILinkedDataItemsModel>())
            {
                linkedModel.CleanUpModelAfterModelCoupling();
            }
        }

        private void CoupleModelsByDimrCouplerXml(IDimrModel sourceModel, IDimrModel targetModel, dimrCoupledItemXML[] dimrCouplerXml)
        {
            foreach (dimrCoupledItemXML couplerXml in dimrCouplerXml)
            {
                try
                {
                    if (string.IsNullOrEmpty(couplerXml.sourceName) || string.IsNullOrEmpty(couplerXml.targetName))
                    {
                        logHandler.ReportErrorFormat(Resources.HydroModelConverter_CoupleModelsByDimrCouplerXml_Could_not_link_an_item_from__0__to__1__,
                                                     sourceModel.Name, targetModel.Name);
                        continue;
                    }

                    IDataItem sourceDataItem = sourceModel.GetDataItemsByItemString(couplerXml.sourceName).FirstOrDefault();
                    IEnumerable<IDataItem> targetDataItems = targetModel.GetDataItemsByItemString(couplerXml.targetName);

                    if (sourceDataItem == null || targetDataItems == null)
                    {
                        logHandler.ReportErrorFormat(Resources.HydroModelConverter_CoupleModelsByDimrCouplerXml_Could_not_link__0__to__1__,
                                                     couplerXml.sourceName, couplerXml.targetName);
                        continue;
                    }

                    foreach (IDataItem targetDataItem in targetDataItems)
                    {
                        targetDataItem.LinkTo(sourceDataItem);
                    }
                }
                catch (Exception e) when (e is NotSupportedException ||
                                          e is ArgumentException)
                {
                    string mainMessage = string.Format(
                        Resources.HydroModelConverter_CoupleModelsByDimrCouplerXml_Could_not_link__0__to__1__,
                        couplerXml.sourceName, couplerXml.targetName);

                    logHandler.ReportError(string.Concat(mainMessage, $": {e.Message}"));
                }
            }
        }
    }
}