using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Collections;
using DeltaShell.Dimr;
using DeltaShell.Dimr.xsd;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Import
{
    /// <summary>
    /// Converts the components information from a dimr file into sub-models of the Integrated model.
    /// </summary>
    public static class HydroModelConverter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HydroModelConverter));

        public static HydroModel Convert(object dataObject, string path, List<IDimrModelFileImporter> fileImporters)
        {
            var dimrObject = (dimrXML)dataObject;
            var rootFolder = Path.GetDirectoryName(path);
            var hydroModel = new HydroModel();

            hydroModel.BeginEdit(new ImportingFullModelAction("Importing full Dimr model"));
            try
            {
                AddModels(fileImporters, dimrObject, rootFolder, hydroModel);

                var subModels = hydroModel.Activities.OfType<IDimrModel>().ToList();
                foreach (var dimrCouplerXml in dimrObject.coupler)
                {
                    var sourceModel = subModels.FirstOrDefault(m => m.Name == dimrCouplerXml.sourceComponent);
                    var targetModel = subModels.FirstOrDefault(m => m.Name == dimrCouplerXml.targetComponent);

                    if (sourceModel == null || targetModel == null)
                    {
                        Log.Error($"Could not couple models: '{dimrCouplerXml.sourceComponent}' to '{dimrCouplerXml.targetComponent}'.");
                        continue;
                    }

                    CoupleModelsByDimrCouplerXml(sourceModel, targetModel, dimrCouplerXml.item);
                }
            }
            finally
            {
                hydroModel.EndEdit();
            }

            return hydroModel;
        }

        private static void CoupleModelsByDimrCouplerXml(IDimrModel sourceModel, IDimrModel targetModel, dimrCoupledItemXML[] dimrCouplerXml)
        {
            foreach (var couplerXml in dimrCouplerXml)
            {
                try
                {
                    var sourceDataitem = sourceModel.GetDataItemByItemString(couplerXml.sourceName);
                    var targetDataitem = targetModel.GetDataItemByItemString(couplerXml.targetName);

                    if (sourceDataitem == null || targetDataitem == null)
                    {
                        Log.Error($"Could not link {couplerXml.sourceName} to {couplerXml.targetName}");
                        continue;
                    }

                    sourceDataitem.LinkTo(targetDataitem);
                }
                catch (NotImplementedException exception)
                {
                    Log.Error($"Could not link {couplerXml.sourceName} to {couplerXml.targetName} : {exception.Message}");
                }
            }
        }

        private static void AddModels(List<IDimrModelFileImporter> fileImporters, dimrXML dimrObject, string rootFolder, HydroModel hydroModel)
        {
            var componentGroups = dimrObject.component
                .GroupBy(component => Path.GetExtension(component.inputFile)?.TrimStart('.'));

            foreach (var componentGroup in componentGroups)
            {
                var extension = componentGroup.Key;

                if (string.IsNullOrEmpty(extension))
                {
                    extension = "json";
                }
                var importer = fileImporters.FirstOrDefault(f => f.MasterFileExtension == extension);

                if (importer == null)
                {
                    LogUnknownImporter(extension);

                    continue;
                }

                importer.ProgressChanged = (name, step, steps) => { };

                foreach (var component in componentGroup)
                {
                    string[] pathParts;

                    var fileName = component.inputFile;

                    pathParts = SortFileParts(rootFolder, importer, FileNameIsUnknown(fileName) ? "settings.json" : fileName);

                    var combinedPath = Path.Combine(pathParts);
                    var fullPath = Path.GetFullPath(combinedPath);
                    var importedItem = importer.ImportItem(fullPath);
                    var subModel = importedItem as IActivity;
                    if (subModel == null)
                    {
                        Log.Error($"Could not add {subModel.Name} to integrated model."); 
                        continue;
                    }

                    if (subModel.Name != component.name)
                    {
                        Log.Debug($"Renamed model {subModel.Name} to {component.name}");
                        subModel.Name = component.name;
                    }

                    var hydroModelSubModel = subModel as IHydroModel;
                    if (hydroModelSubModel != null)
                    {
                        hydroModel.Region.SubRegions.Add(hydroModelSubModel.Region);
                        var controls = dimrObject.control;
                        var control = (dimrParallelXML)controls.ElementAt(0);
                        var startGroup = (dimrStartGroupXML)control.Items.ElementAt(0);
                        var time = startGroup.time;
                        var startTime = time.Split(' ')[0];
                        var timeStep = time.Split(' ')[1];
                        var stopTime = time.Split(' ')[2];

                        var dateTimeStart = new TimeSpan(0,0,int.Parse(startTime),0);
                        var dateTimeStop = new TimeSpan(0,0,int.Parse(stopTime),0);
                        var dateTimeTimeStep = new TimeSpan(0, 0, int.Parse(timeStep), 0);

                        hydroModel.StartTime.TimeOfDay.Add(dateTimeStart);
                        hydroModel.StopTime.TimeOfDay.Add(dateTimeStop);
                        hydroModel.TimeStep = dateTimeTimeStep;
                    }

                    hydroModel.Activities.Add(subModel);
                }
            }
        }

        private static bool FileNameIsUnknown(string fileName)
        {
            return fileName.Equals(".");
        }

        private static void LogUnknownImporter(string extension)
        {
            Log.Info($"No importer found for extension: {extension}");
        }

        private static string[] SortFileParts(string rootFolder, IDimrModelFileImporter importer, string fileName)
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
                    .Plus(fileObject.xmlDir)
                    .ToArray();

                return pathParts;
            }

            pathParts = new[] { rootFolder }
                .Concat(importer.SubFolders)
                .Plus(fileName)
                .ToArray();
            return pathParts;
        }
    }

}