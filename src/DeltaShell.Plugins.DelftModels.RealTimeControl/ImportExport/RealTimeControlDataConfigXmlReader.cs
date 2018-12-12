using DelftTools.Utils.Collections.Extensions;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    static class RealTimeControlDataConfigXmlReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RealTimeControlDataConfigXmlReader));

        public static IList<ConnectionPoint> Read(string dataConfigFilePath, IList<ControlGroup> controlGroups)
        {
            if (!File.Exists(dataConfigFilePath))
            {
                Log.ErrorFormat(Resources.RealTimeControlDataConfigXmlReader_Read_File___0___does_not_exist_, dataConfigFilePath);
                return null;
            }
                
            if (controlGroups == null) return null;
         
            var dataConfigObject = (RTCDataConfigXML)DelftConfigXmlFileParser.Read(dataConfigFilePath);

            var importElements = dataConfigObject.importSeries.timeSeries;
            var exportElements = dataConfigObject.exportSeries.timeSeries;
            var allElements = importElements.Concat(exportElements).ToList();
            if (allElements.Count == 0)
            {
                Log.ErrorFormat(Resources.RealTimeControlDataConfigXmlReader_Read_File___0___seems_to_be_empty_, RealTimeControlXMLFiles.XmlData);
                return null;

            }

            var createdControlGroups = RealTimeControlDataConfigXmlConverter.CreateControlGroupsFromXmlElementIDs(allElements);
            if (createdControlGroups == null || createdControlGroups.Count == 0)
            {
                Log.ErrorFormat(Resources.RealTimeControlDataConfigXmlReader_Read_Could_not_read_control_groups_from_file___0___, RealTimeControlXMLFiles.XmlData);
                return null;
            }

            RealTimeControlDataConfigXmlConverter.CreateRulesFromXmlElementsAndAddToControlGroup(allElements, createdControlGroups);
            RealTimeControlDataConfigXmlConverter.CreateConditionsFromXmlElementsAndAddToControlGroup(allElements, createdControlGroups);

            var connectionPoints = RealTimeControlDataConfigXmlConverter.GetConnectionPointsFromXmlElements(allElements);
            if (connectionPoints == null || connectionPoints.Count == 0)
            {
                Log.ErrorFormat(Resources.RealTimeControlDataConfigXmlReader_Read_Could_not_read_connection_points_from_file___0___, RealTimeControlXMLFiles.XmlData);
                return null;
            }

            RealTimeControlDataConfigXmlConverter.AddOutputAsInputForRelativeTimeRule(allElements, createdControlGroups, connectionPoints.OfType<Output>().ToList());

            controlGroups.AddRange(createdControlGroups);

            return connectionPoints;
        }
    }
}
