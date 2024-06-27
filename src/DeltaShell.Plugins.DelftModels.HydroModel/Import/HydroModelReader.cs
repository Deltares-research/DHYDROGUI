using DelftTools.Shell.Core.Services;
using Deltares.Infrastructure.Logging;
using DeltaShell.Dimr.DimrXsd;
using DeltaShell.NGHS.IO.FileReaders;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Import
{
    /// <summary>
    /// Reader for the Integrated model
    /// </summary>
    public sealed class HydroModelReader : IHydroModelReader
    {
        private readonly IFileImportService fileImportService;

        /// <summary>
        /// Initializes a new instance of the <see cref="HydroModelReader"/> class.
        /// </summary>
        /// <param name="fileImportService">Provides file importers for importing sub-models</param>
        public HydroModelReader(IFileImportService fileImportService)
        {
            this.fileImportService = fileImportService;
        }

        /// <summary>
        /// Reads a <see cref="HydroModel"/> (Integrated model) from the specified path.
        /// </summary>
        /// <param name="path">Path to the Dimr.xml</param>
        /// <returns>Read <see cref="HydroModel"/></returns>
        public HydroModel Read(string path)
        {
            var logHandler = new LogHandler("import of the Hydro Model");

            if (path == null)
            {
                return null;
            }

            var delftConfigXmlParser = new DelftConfigXmlFileParser(logHandler);
            var dataObject = delftConfigXmlParser.Read<dimrXML>(path);

            var hydroModelConverter = new HydroModelConverter(logHandler, fileImportService);
            HydroModel hydroModel = hydroModelConverter.Convert(dataObject, path);

            logHandler.LogReport();

            return hydroModel;
        }
    }
}