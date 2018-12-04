using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Collections;
using DeltaShell.Dimr;
using DeltaShell.Dimr.xsd;
using log4net;
using Newtonsoft.Json;

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
                var fileGroups = SortFileGroups(dimrObject);
                foreach (var fileGroup in fileGroups)
                {
                    var extension = fileGroup.Key;
                    var importer = fileImporters.FirstOrDefault(f => f.MasterFileExtension == extension);

                    if (importer == null)
                    {
                        LogUnknownImporter(extension);

                        continue;
                    }

                    importer.ProgressChanged = (name, step, steps) => { };

                    foreach (var fileName in fileGroup)
                    {
                        var pathParts = SortFileParts(rootFolder, importer, fileName);
                        var combinedPath = Path.Combine(pathParts);
                        var fullPath = Path.GetFullPath(combinedPath);
                        var subModel = importer.ImportItem(fullPath);

                        AddSubModels(subModel, hydroModel);

                        hydroModel.Activities.Add(subModel as IActivity);
                    }
                }
            }
            finally
            {
                hydroModel.EndEdit();
            }

            return hydroModel;
        }

        private static void LogUnknownImporter(string extension)
        {
            Log.Info($"No importer found for extension: {extension}");
        }

        private static void AddSubModels(object subModel, HydroModel hydroModel)
        {
            var hydroModelSubModel = subModel as IHydroModel;

            if (hydroModelSubModel != null)
            {
                hydroModel.Region.SubRegions.Add(hydroModelSubModel.Region);
            }
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

        private static IEnumerable<IGrouping<string, string>> SortFileGroups(dimrXML dimrObject)
        {
            var fileGroups = dimrObject.component.Select(c => c.inputFile).GroupBy(f => Path.GetExtension(f)?.TrimStart('.'));
            return fileGroups;
        }
    }

}