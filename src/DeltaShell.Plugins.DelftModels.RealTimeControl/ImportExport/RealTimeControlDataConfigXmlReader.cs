using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    static class RealTimeControlDataConfigXmlReader
    {
        public static IList<ConnectionPoint> Read(string dataConfigFilePath, IHydroModel targetModel, RealTimeControlModel rtcModel)
        {
            var dataConfigObject = (RTCDataConfigXML)DelftConfigXmlFileParser.Read(dataConfigFilePath);

            var importElements = dataConfigObject.importSeries.timeSeries;
            var exportElements = dataConfigObject.exportSeries.timeSeries;
            var allElements = importElements.Concat(exportElements).ToList();

            var controlGroups = RealTimeControlDataConfigXmlConverter.CreateControlGroupsFromXmlElementIDs(allElements);

            var rules = RealTimeControlDataConfigXmlConverter.GetAllRulesFromXmlElementsAndAddToControlGroup(allElements, controlGroups);
            var conditions = RealTimeControlDataConfigXmlConverter.GetAllConditionsFromXmlElementsAndAddToControlGroup(allElements, controlGroups);
            var inputs = (IList<Input>)RealTimeControlDataConfigXmlConverter.GetConnectionPointsFromXmlElements(importElements, RtcXmlTag.Input, targetModel);
            var outputs = (IList<Output>)RealTimeControlDataConfigXmlConverter.GetConnectionPointsFromXmlElements(exportElements, RtcXmlTag.Output, targetModel);

            RealTimeControlDataConfigXmlConverter.AddOutputAsInputForRelativeTimeRule(allElements,
                rules.OfType<RelativeTimeRule>().ToList(), outputs);

            rtcModel.ControlGroups.AddRange(controlGroups);

            var connectionPoints = new List<ConnectionPoint>();
            connectionPoints.AddRange(inputs);
            connectionPoints.AddRange(outputs);

            return connectionPoints;
        }
    }
}
