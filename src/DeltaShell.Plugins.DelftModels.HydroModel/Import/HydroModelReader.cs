using System.Collections.Generic;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Dimr;
using DeltaShell.NGHS.IO.FileReaders;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Import
{
    /// <summary>
    /// Reader for the Integrated model
    /// </summary>
    public static class HydroModelReader
    {
        public static ICompositeActivity Read(string path, List<IDimrModelFileImporter> fileImporters)
        {
            if (path == null) { return null;}
            
            var dataObject = DelftConfigXmlFileParser.Read(path);
            var hydroModel = HydroModelConverter.Convert(dataObject, path, fileImporters);

            return hydroModel;
        }
    }
}
