using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Services;
using Deltares.Infrastructure.Logging;
using DeltaShell.Dimr.dimr_xsd;
using DeltaShell.Dimr.DimrXsd;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.HydroModel.Properties;

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
        /// <param name="reportProgress">String to feedback to importer what importers are working on.</param>
        /// <returns>
        /// The read <see cref="HydroModel"/>,
        /// in the case <paramref name="path"/> is <c>null</c> or the Dimr.xml from <paramref name="path"/> is invalid, <c>null</c> is returned.
        /// </returns>
        public HydroModel Read(string path, Action<string> reportProgress = null)
        {
            var logHandler = new LogHandler("import of the Hydro Model");

            if (path == null)
            {
                return null;
            }

            var delftConfigXmlParser = new DelftConfigXmlFileParser(logHandler);
            reportProgress?.Invoke(Resources.HydroModelReader_Read_Parsing_Dimr_xml_file);
            
            var dataObject = delftConfigXmlParser.Read<dimrXML>(path);

            var dimrXmlValidator = new DimrXmlValidator(logHandler);
            if (!dimrXmlValidator.IsValid(dataObject, path))
            {
                logHandler.LogReport();
                return null;
            }
            
            var hydroModelConverter = new HydroModelConverter(logHandler, fileImportService);
            HydroModel hydroModel = hydroModelConverter.Convert(dataObject, path, reportProgress);
            
            var allActivitiesRecursive = hydroModel.GetAllActivitiesRecursive<IHydroModel>().OfType<IHasCoordinateSystem>();
            var hasCoordinateSystem = allActivitiesRecursive.FirstOrDefault(a => a.CoordinateSystem != null);
            if (hasCoordinateSystem != null)
            {
                reportProgress?.Invoke(Resources.HydroModelReader_Read_Set_hydromodel_coordinate_system);
                hydroModel.CoordinateSystem = hasCoordinateSystem.CoordinateSystem;
            }
            
            logHandler.LogReport();

            return hydroModel;
        }
    }
}