using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Extensions;
using DeltaShell.Dimr;
using DeltaShell.Dimr.dimr_xsd;
using DeltaShell.Dimr.DimrXsd;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.HydroModel.Properties;
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
        /// <param name="reportProgress">String to feedback to importer what importers are working on.</param>
        /// <returns>
        /// The read <see cref="HydroModel"/>,
        /// in the case <paramref name="path"/> is <c>Null</c> or the Dimr.xml from <paramref name="path"/> is invalid <c>Null</c> is returned.
        /// </returns>
        public static HydroModel Read(string path, IList<IDimrModelFileImporter> fileImporters, Action<string> reportProgress = null)
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
            
            var hydroModelConverter = new HydroModelConverter(logHandler);
            HydroModel hydroModel = hydroModelConverter.Convert(dataObject, path, fileImporters, reportProgress);
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