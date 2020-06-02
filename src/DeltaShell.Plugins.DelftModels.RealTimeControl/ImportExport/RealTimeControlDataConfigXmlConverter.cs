using System.Collections.Generic;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xsd;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    /// <summary>
    /// Responsible for taking the objects that come from the data config xml file and converting them into connection points.
    /// </summary>
    public class RealTimeControlDataConfigXmlConverter
    {
        private readonly ILogHandler logHandler;

        public RealTimeControlDataConfigXmlConverter(ILogHandler logHandler)
        {
            this.logHandler = logHandler;
        }

        /// <summary>
        /// Creates the connection points from XML elements.
        /// </summary>
        /// <param name="elements">The elements.</param>
        /// <returns>A collection of connection points</returns>
        /// <remarks>If parameter elements is NULL, methods returns.</remarks>
        public IEnumerable<ConnectionPoint> CreateConnectionPointsFromXmlElements(IEnumerable<RTCTimeSeriesXML> elements)
        {
            if (elements == null)
            {
                yield break;
            }

            foreach (RTCTimeSeriesXML element in elements)
            {
                string id = element.id;

                string tag = RealTimeControlXmlReaderHelper.GetTagFromElementId(id);

                bool ignoreElement = !RtcXmlTag.ConnectionPointTags.Contains(tag) ||
                                     id.Contains(RtcXmlTag.OutputAsInput) ||
                                     id.Contains(RtcXmlTag.Delayed) ||
                                     element.OpenMIExchangeItem.elementId == null;

                if (ignoreElement)
                {
                    continue;
                }

                ConnectionPoint connectionPoint;

                switch (tag)
                {
                    case RtcXmlTag.Input:
                        connectionPoint = new Input();
                        break;
                    case RtcXmlTag.Output:
                        connectionPoint = new Output();
                        break;
                    default:
                        yield break;
                }

                // serves as a temporary name, used for coupler
                connectionPoint.Name = id;

                yield return connectionPoint;
            }
        }
    }
}