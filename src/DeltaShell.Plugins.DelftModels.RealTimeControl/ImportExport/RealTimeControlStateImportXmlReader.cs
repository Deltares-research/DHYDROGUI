using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Handlers;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xsd;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    /// <summary>
    /// Responsible for reading the state import file and setting the state import data on the connection points.
    /// </summary>
    public class RealTimeControlStateImportXmlReader
    {
        private readonly ILogHandler logHandler;

        public RealTimeControlStateImportXmlReader(ILogHandler logHandler)
        {
            this.logHandler = logHandler;
        }

        /// <summary>
        /// Reads the specified state import file path and sets the state import data on the connection points.
        /// </summary>
        /// <param name="stateImportFilePath">The state import file path.</param>
        /// <param name="connectionPoints">The connection points (inputs and outputs).</param>
        /// <remarks>If parameter connectionPoints is NULL, methods returns.</remarks>
        public void Read(string stateImportFilePath, IList<ConnectionPoint> connectionPoints)
        {
            if (connectionPoints == null) return;

            var delftConfigXmlParser = new DelftConfigXmlFileParser(logHandler);

            TreeVectorFileXML stateImportObject;
            try
            {
                stateImportObject = delftConfigXmlParser.Read<TreeVectorFileXML>(stateImportFilePath);
            }
            catch (FileNotFoundException e)
            {
                logHandler.ReportError(e.Message);
                return;
            }

            var connectionPointItems = stateImportObject.treeVector.Items.OfType<TreeVectorLeafXML>().ToList();

            var stateImportSetter = new RealTimeControlStateImportSetter(logHandler);
            stateImportSetter.SetStateImportOnConnectionPoints(connectionPoints, connectionPointItems);
        }
    }
}
