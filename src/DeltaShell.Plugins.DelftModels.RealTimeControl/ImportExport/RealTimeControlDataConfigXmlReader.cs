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

            var rules = RealTimeControlDataConfigXmlConverter.GetAllRulesFromXmlElementsAndAddToControlGroup(allElements, createdControlGroups);
            var conditions = RealTimeControlDataConfigXmlConverter.GetAllConditionsFromXmlElementsAndAddToControlGroup(allElements, createdControlGroups);
            var inputs = RealTimeControlDataConfigXmlConverter.GetInputsFromXmlElements(allElements);
            var outputs = RealTimeControlDataConfigXmlConverter.GetOutputsFromXmlElements(allElements);

            RealTimeControlDataConfigXmlConverter.AddOutputAsInputForRelativeTimeRule(allElements,
                rules.OfType<RelativeTimeRule>().ToList(), outputs);

            controlGroups.AddRange(createdControlGroups);

            var connectionPoints = new List<ConnectionPoint>();
            connectionPoints.AddRange(inputs);
            connectionPoints.AddRange(outputs);

            return connectionPoints;
        }
    }
}
