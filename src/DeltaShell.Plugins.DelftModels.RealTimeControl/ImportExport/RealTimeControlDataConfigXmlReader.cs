using DelftTools.Utils.Collections.Extensions;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    static class RealTimeControlDataConfigXmlReader
    {
        public static IList<ConnectionPoint> Read(string dataConfigFilePath, IList<ControlGroup> controlGroups)
        {
            var dataConfigObject = (RTCDataConfigXML)DelftConfigXmlFileParser.Read(dataConfigFilePath);

            var importElements = dataConfigObject.importSeries.timeSeries;
            var exportElements = dataConfigObject.exportSeries.timeSeries;
            var allElements = importElements.Concat(exportElements).ToList();

            var createdControlGroups = RealTimeControlDataConfigXmlConverter.CreateControlGroupsFromXmlElementIDs(allElements);

            RealTimeControlDataConfigXmlConverter.CreateRulesFromXmlElementsAndAddToControlGroup(allElements, createdControlGroups);
            RealTimeControlDataConfigXmlConverter.CreateConditionsFromXmlElementsAndAddToControlGroup(allElements, createdControlGroups);

            var connectionPoints = RealTimeControlDataConfigXmlConverter.GetConnectionPointsFromXmlElements(allElements);

            RealTimeControlDataConfigXmlConverter.AddOutputAsInputForRelativeTimeRule(allElements, createdControlGroups, connectionPoints.OfType<Output>());

            controlGroups.AddRange(createdControlGroups);

            return connectionPoints;
        }
    }
}
