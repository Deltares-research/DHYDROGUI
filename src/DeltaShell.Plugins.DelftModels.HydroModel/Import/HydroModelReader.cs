using System.Collections.Generic;
using DeltaShell.Dimr;
using DeltaShell.Dimr.DimrXsd;
using DeltaShell.NGHS.IO.FileReaders;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Import
{
    /// <summary>
    /// Reader for the Integrated model
    /// </summary>
    public static class HydroModelReader
    {
        /// <summary>
        /// Reads an <see cref="HydroModel"/> (Integrated model) from
        /// <param name="path"/>
        /// using the
        /// supplied
        /// <param name="fileImporters"/>
        /// for importing sub-models
        /// </summary>
        /// <param name="path">Path to the Dimr.xml</param>
        /// <param name="fileImporters">File importers for importing sub-models</param>
        /// <returns>Read <see cref="HydroModel"/></returns>
        public static HydroModel Read(string path, IList<IDimrModelFileImporter> fileImporters)
        {
            var logHandler = new LogHandler("import of the Hydro Model");

            if (path == null)
            {
                return null;
            }

            var delftConfigXmlParser = new DelftConfigXmlFileParser(logHandler);
            var dataObject = delftConfigXmlParser.Read<dimrXML>(path);

            var hydroModelConverter = new HydroModelConverter(logHandler);
            HydroModel hydroModel = hydroModelConverter.Convert(dataObject, path, fileImporters);

            logHandler.LogReport();

            return hydroModel;
        }
    }
}