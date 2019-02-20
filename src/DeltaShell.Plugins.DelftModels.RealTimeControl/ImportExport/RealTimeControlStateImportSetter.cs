using DeltaShell.NGHS.IO.Handlers;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xsd;
using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    /// <summary>
    /// Responsible for setting the state import data on the inputs and outputs.
    /// </summary>
    public class RealTimeControlStateImportSetter
    {
        private readonly ILogHandler logHandler;

        public RealTimeControlStateImportSetter(ILogHandler logHandler)
        {
            this.logHandler = logHandler;
        }

        /// <summary>
        /// Sets the state import data on the connection points.
        /// </summary>
        /// <param name="connectionPoints">The connection points.</param>
        /// <param name="connectionPointItems">The Tree Vector Leaf XML elements.</param>
        /// <remarks>If parameter connectionPoints or connectionPointItems is NULL, methods returns.</remarks>
        public void SetStateImportOnConnectionPoints(IList<ConnectionPoint> connectionPoints, IEnumerable<TreeVectorLeafXML> connectionPointItems)
        {
            foreach (var connectionPointItem in connectionPointItems)
            {
                var connectionPointName = connectionPointItem.id;

                var tag = RealTimeControlXmlReaderHelper.GetTagFromElementId(connectionPointName);

                var value = double.Parse(connectionPointItem.vector, System.Globalization.CultureInfo.InvariantCulture);

                ConnectionPoint connectionPoint = null;

                switch (tag)
                {
                    case RtcXmlTag.Input:
                        connectionPoint = connectionPoints.OfType<Input>()
                            .FirstOrDefault(i => i.Name == connectionPointName);
                        break;
                    case RtcXmlTag.Output:
                        connectionPoint = connectionPoints.OfType<Output>()
                            .FirstOrDefault(i => i.Name == connectionPointName);
                        break;
                }

                if (connectionPoint == null)
                {
                    logHandler.ReportWarningFormat(
                        Resources
                            .RealTimeControlStateImportXmlReader_Read_Could_not_find_output_with_name___0___that_is_referenced_in_file___1____Please_check_file___2__,
                        connectionPointName, RealTimeControlXMLFiles.XmlImportState, RealTimeControlXMLFiles.XmlData);
                    continue;
                }

                connectionPoint.Value = value;
            }
        }
    }
}
