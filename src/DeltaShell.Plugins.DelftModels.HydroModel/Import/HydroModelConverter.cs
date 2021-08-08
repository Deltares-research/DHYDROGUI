using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Editing;
using DeltaShell.Dimr;
using DeltaShell.Dimr.DimrXsd;
using DeltaShell.NGHS.Common;
using DeltaShell.NGHS.Common.Extensions;
using DeltaShell.NGHS.Common.Logging;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Geometries;
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
                throw new ArgumentException("Cannot convert empty dimr data object.");
            }

            string rootFolder = Path.GetDirectoryName(path);
            var hydroModel = new HydroModel();

            hydroModel.BeginEdit(new DefaultEditAction("Importing full Dimr model"));
            var suspendClearOutput = false;
            try
            {
                suspendClearOutput = hydroModel.SuspendClearOutputOnInputChange;
                hydroModel.SuspendClearOutputOnInputChange = true;
                AddModels(hydroModel, fileImporters, dimrObject, rootFolder);

                List<IDimrModel> subModels = hydroModel.Activities.OfType<IDimrModel>().ToList();
                if (dimrObject.coupler != null)
                {
                    CoupleSubModels(hydroModel, dimrObject, subModels);
                }
            }
            finally
            {
                hydroModel.EndEdit();
                hydroModel.SuspendClearOutputOnInputChange = suspendClearOutput;
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
                        logHandler.ReportErrorFormat("Could not import sub model defined at location {0} to integrated model.", filePath);
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
                throw new ArgumentException(string.Format("The working directory is missing for component {0} in the dimr xml.",
                                                          component.name));
            }

            if (component.inputFile == null)
            {
                throw new ArgumentException(string.Format("The input file is missing for component {0} in the dimr xml.",
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
                    logHandler.ReportError("Could not import RTC model, the settings.json file should contain an xml directory.");
                    return null;
                }

                pathParts = new[]
                {
                    rootFolder,
                    workingDirectory,
                    xmlDirectory
                };
            }
            else
            {
                pathParts = new[]
                {
                    rootFolder,
                    workingDirectory,
                    fileName
                };
            }

            return Path.Combine(pathParts);
        }

        private void RenameSubModelWhenNeeded(IActivity subModel, string componentName)
        {
            if (subModel.Name == componentName)
            {
                return;
            }

            Log.DebugFormat("Renamed model {0} to {1}.", subModel.Name, componentName);
            subModel.Name = componentName;
        }

        private void SetHydroModelProperties(HydroModel hydroModel, IActivity subModel)
        {
            if (!(subModel is IHydroModel sourceModel) || sourceModel.Region == null)
            {
                hydroModel.Activities.Add(subModel);
                return;
            }
            sourceModel.ReplaceHydroModelRegion(hydroModel);
        }

        private void CoupleSubModels(HydroModel hydroModel, dimrXML dimrObject, IList<IDimrModel> subModels)
        {
            foreach (dimrCouplerXML dimrCouplerXml in dimrObject.coupler)
            {
                IDimrModel sourceModel = subModels.FirstOrDefault(m => m.Name.EqualsCaseInsensitive(dimrCouplerXml.sourceComponent));
                IDimrModel targetModel = subModels.FirstOrDefault(m => m.Name.EqualsCaseInsensitive(dimrCouplerXml.targetComponent));

                if (sourceModel == null || targetModel == null)
                {
                    logHandler.ReportErrorFormat("Could not couple models: \'{0}\' to \'{1}\'.",
                                                 dimrCouplerXml.sourceComponent, dimrCouplerXml.targetComponent);
                    continue;
                }

                CoupleModelsUsingDimrCouplerXml(sourceModel, targetModel, dimrCouplerXml.item, hydroModel.Region.Links);
            }

            foreach (IControllingModel controllingModel in subModels.OfType<IControllingModel>())
            {
                controllingModel.CleanUpModelAfterModelCoupling();
            }
        }

        private void CoupleModelsUsingDimrCouplerXml(IDimrModel sourceModel, IDimrModel targetModel, IEnumerable<dimrCoupledItemXML> dimrCouplerXml, IEnumerable<HydroLink> regionLinks)
        {
            var linkBySourceIemLookup = regionLinks
                                                  .GroupBy(l => l.Source)
                                                  .ToDictionary(l => l.Key, l => l.ToArray());

           sourceModel.DimrCoupling?.Prepare();
           targetModel.DimrCoupling?.Prepare();
            
            foreach (dimrCoupledItemXML couplerXml in dimrCouplerXml)
            {
                try
                {
                    if (string.IsNullOrEmpty(couplerXml.sourceName) || 
                        string.IsNullOrEmpty(couplerXml.targetName))
                    {
                        logHandler.ReportErrorFormat("Could not link an item from {0} to {1}", sourceModel.Name, targetModel.Name);
                        continue;
                    }

                    // DataItem linking
                    IDataItem sourceDataItem = sourceModel.GetDataItemsByItemString(couplerXml.sourceName, couplerXml.targetName)?.FirstOrDefault();
                    if (sourceDataItem != null)
                    {
                        IEnumerable<IDataItem> targetDataItems = targetModel.GetDataItemsByItemString(couplerXml.targetName, couplerXml.sourceName);
                        if (targetDataItems != null)
                        {
                            foreach (IDataItem targetDataItem in targetDataItems)
                            {
                                targetDataItem.LinkTo(sourceDataItem);
                            }
                            continue;
                        }
                    }

                    // HydroObject linking
                    IHydroObject sourceItem = sourceModel.DimrCoupling?.GetLinkHydroObjectByItemString(couplerXml.sourceName);
                    if (sourceItem == null)
                    {
                        logHandler.ReportErrorFormat("Could not link {0} to {1}", couplerXml.sourceName, couplerXml.targetName);
                        continue;
                    }

                    IHydroObject targetItem = targetModel.DimrCoupling?.GetLinkHydroObjectByItemString(couplerXml.targetName);

                    if (!sourceItem.CanLinkTo(targetItem) ||
                        linkBySourceIemLookup.TryGetValue(sourceItem, out var links) &&
                        links.Any(l => l.Target == targetItem))
                    {
                        continue;
                    }

                    var link = sourceItem.LinkTo(targetItem);
                    if (link.Geometry == null)
                    {
                        link.Geometry = new LineString(new[]
                        {
                            GetCoordinateForHydroObject(sourceItem),
                            GetCoordinateForHydroObject(targetItem)
                        });
                    }
                }
                catch (Exception e) when (e is NotSupportedException ||
                                          e is ArgumentException)
                {
                    logHandler.ReportError($"Could not link {couplerXml.sourceName} to {couplerXml.targetName}: {e.Message}");
                }
            }
            
            sourceModel.DimrCoupling?.End();
            targetModel.DimrCoupling?.End();
        }

        private static Coordinate GetCoordinateForHydroObject(IHydroObject obj)
        {
            var catchment = obj as Catchment;
            return catchment != null ? catchment.InteriorPoint.Coordinate : obj.Geometry.Coordinate;
        }
    }
}