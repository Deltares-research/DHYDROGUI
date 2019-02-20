using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Handlers;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xsd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    /// <summary>This class is responsible for building complete controlgroups from the data config and tools config XML files.</summary>
    public class RealTimeControlDataAndToolsConfigXmlReader
    {
        private readonly ILogHandler logHandler;

        public RealTimeControlDataAndToolsConfigXmlReader(ILogHandler logHandler)
        {
            this.logHandler = logHandler;
        }

        /// <summary>Reads the specified data in the data config and tools config file.</summary>
        /// <param name="dataConfigFilePath">The data configuration file path.</param>
        /// <param name="toolsConfigFilePath">The tools configuration file path.</param>
        /// <param name="modelTimeStep">The model time step.</param>
        /// <returns>A list of controlgroups defined in the tools config file.</returns>
        public IList<IControlGroup> Read(string dataConfigFilePath, string toolsConfigFilePath, TimeSpan modelTimeStep)
        {
            var controlGroups = new List<IControlGroup>();

            var delftConfigXmlParser = new DelftConfigXmlFileParser(logHandler);
            RTCDataConfigXML dataConfigObject;
            try
            {
                dataConfigObject = delftConfigXmlParser.Read<RTCDataConfigXML>(dataConfigFilePath);
            }
            catch (FileNotFoundException e)
            {
                logHandler.ReportError(e.Message);
                return controlGroups;
            }

            RtcToolsConfigXML toolsConfigObject;
            try
            {
                toolsConfigObject = delftConfigXmlParser.Read<RtcToolsConfigXML>(toolsConfigFilePath);
            }
            catch (FileNotFoundException e)
            {
                logHandler.ReportError(e.Message);
                return controlGroups;
            }

            var allElements = dataConfigObject.importSeries.timeSeries.Concat(dataConfigObject.exportSeries.timeSeries).ToList();
            var ruleElements = toolsConfigObject.rules;
            var conditionElements = toolsConfigObject.triggers;

            var dataConfigConverter = new RealTimeControlDataConfigXmlConverter(logHandler);
            var connectionPoints = dataConfigConverter.CreateConnectionPointsFromXmlElements(allElements).ToList();
            if (connectionPoints.Count == 0)
            {
                logHandler.ReportErrorFormat(Resources.RealTimeControlDataConfigXmlReader_Read_Could_not_read_connection_points_from_file___0___, dataConfigFilePath);
                return controlGroups;
            }

            var toolsConfigConverter = new RealTimeControlToolsConfigXmlConverter(logHandler);
            controlGroups = toolsConfigConverter.CreateControlGroupsFromXmlElementIDs(ruleElements).ToList();
            if (controlGroups.Count == 0)
            {
                logHandler.ReportErrorFormat(Resources.RealTimeControlDataConfigXmlReader_Read_Could_not_read_control_groups_from_file___0___, toolsConfigFilePath);
                return controlGroups;
            }

            var signalElements = new List<RuleXML>();

            toolsConfigConverter.SeparateSignalsFromRules(ruleElements, signalElements);

            toolsConfigConverter.CreateRulesFromXmlElementsAndAddToControlGroup(ruleElements, controlGroups);
            toolsConfigConverter.CreateConditionsFromXmlElementsAndAddToControlGroup(conditionElements, controlGroups);
            toolsConfigConverter.CreateSignalsFromXmlElementsAndAddToControlGroup(signalElements,
                controlGroups);
                
			var toolsConfigComponentConnector = new RealTimeControlToolsConfigComponentConnector(logHandler);
            toolsConfigComponentConnector.ConnectRules(ruleElements, controlGroups, connectionPoints);
            toolsConfigComponentConnector.ConnectConditions(conditionElements, controlGroups, connectionPoints);
            toolsConfigComponentConnector.ConnectSignals(signalElements, controlGroups, connectionPoints);
            
            var timeSeriesElements = allElements.Select(e => e.PITimeSeries).Where(t => t.locationId != null).ToList();
            
            var dataConfigSetter = new RealTimeControlDataConfigXmlSetter(logHandler);
            dataConfigSetter.SetInterpolationAndExtrapolationRtcComponents(timeSeriesElements, controlGroups);

            var hydraulicRules = controlGroups.SelectMany(g => g.Rules.OfType<HydraulicRule>()).Where(r => !(r is FactorRule)).ToList();
            dataConfigSetter.SetTimeLagOnHydraulicRules(allElements, hydraulicRules, modelTimeStep);

            return controlGroups;
        }
    }
}
