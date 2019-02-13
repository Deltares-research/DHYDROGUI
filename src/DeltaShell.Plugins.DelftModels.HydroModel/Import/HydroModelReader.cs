using System.Collections.Generic;
using DeltaShell.Dimr;
using DeltaShell.Dimr.xsd;
using DeltaShell.NGHS.IO.FileReaders;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Import
{
    /// <summary>
    /// Reader for the Integrated model
    /// </summary>
    public static class HydroModelReader
    {
        /// <summary>
        /// Reads an <see cref="HydroModel"/> (Integrated model) from <param name="path"/> using the
        /// supplied <param name="fileImporters"/> for importing sub-models
        /// </summary>
        /// <param name="path">Path to the Dimr.xml</param>
        /// <param name="fileImporters">File importers for importing sub-models</param>
        /// <returns>Read <see cref="HydroModel"/></returns>
        public static HydroModel Read(string path, IList<IDimrModelFileImporter> fileImporters)
        {
            if (path == null) { return null;}
            
            var dataObject = DelftConfigXmlFileParser.Read<dimrXML>(path);
            return HydroModelConverter.Convert(dataObject, path, fileImporters);
        }
    }
}
