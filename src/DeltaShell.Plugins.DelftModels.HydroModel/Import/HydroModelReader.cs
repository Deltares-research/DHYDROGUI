using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Collections;
using DeltaShell.Dimr;
using DeltaShell.Dimr.xsd;
using DeltaShell.NGHS.IO.FileReaders;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Import
{
    public static class HydroModelReader
    {
        public static ICompositeActivity Read(string path, Func<List<IDimrModelFileImporter>> fileImporters)
        {
            if (path == null) { return null;}
            if (fileImporters == null) { return null;}

            var importers = fileImporters.Invoke();
            var dataObject = DelftConfigXmlFileParser.Read(path);

            var hydroModel = HydroModelConverter.Convert(dataObject, path, importers);

            return hydroModel;
        }
    }

    public static class HydroModelConverter
    {
        private static IEnumerable<object> flow1DModels;
        private static WaterFlowModel1DActivityConverter waterFlowModel1DActivityConverter;

        private static WaterFlowModel1DActivityConverter FlowConverter
        {
            get
            {
                if (waterFlowModel1DActivityConverter == null)
                {
                    return new WaterFlowModel1DActivityConverter();
                }

                return waterFlowModel1DActivityConverter;
            }
        }

        public static HydroModel Convert(object dataObject, string path, List<IDimrModelFileImporter> fileImporters)
        {
            var dimrObject = (dimrXML) dataObject;
            var flowImporter = fileImporters.FirstOrDefault(fi=> fi.LibraryName == "cf_dll");
            var hasFlowLibrary = dimrObject.component.Any(c => c.library == "cf_dll");

            if (hasFlowLibrary)
            {
                flow1DModels = FlowConverter.Convert(dataObject, path, flowImporter);
            }

            HydroModel hydroModel = new HydroModel();
            flow1DModels.ForEach(fm => hydroModel.Activities.Add((IActivity)fm));

            return hydroModel;
        }
    }

    public class WaterFlowModel1DActivityConverter : ConverterStrategy
    {
        public override IEnumerable<object> Convert(object dataObject, string path, IDimrModelFileImporter importer)
        {
            var dimrObject = (dimrXML)dataObject;
            var components = dimrObject.component.ToList();
            var flowFmFiles = GetInputFilesNamesFromDataModel(components).ToList();
            var pathToFiles = path.Replace("dimr.xml", "dflow1d");
            var flowModels = new List<object>();
            importer.ProgressChanged = (name, step, steps) => { };

            flowFmFiles.ForEach(file => flowModels.Add(importer.ImportItem(Path.Combine(pathToFiles, "rijn-flow-model.md1d"))));

            //var flowModel = importer.ImportItem(Path.Combine(pathToFiles, "rijn-flow-model.md1d"));

            return flowModels;
        }

        private static IEnumerable<string> GetInputFilesNamesFromDataModel(List<dimrComponentXML> components)
        {
            var inputFileList = new List<string>();

            foreach (var component in components)
            {
                inputFileList.Add(component.inputFile);
            }

            return inputFileList;
        }
    }

    public abstract class ConverterStrategy : IConverterStrategy
    {
        public virtual IEnumerable<object> Convert(object dataObject, string path, IDimrModelFileImporter importers)
        {
            return null;
        }
    }

    public interface IConverterStrategy
    {
        IEnumerable<object> Convert(object dataObject, string path, IDimrModelFileImporter importers);
    }
}
