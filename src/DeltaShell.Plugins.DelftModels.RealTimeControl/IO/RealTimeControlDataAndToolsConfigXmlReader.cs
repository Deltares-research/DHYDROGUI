using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xsd;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO
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

            RuleXML[] ruleElements = toolsConfigObject.rules.ToArray();
            TriggerXML[] triggerElements = toolsConfigObject.triggers.ToArray();

            IEnumerable<IRtcDataAccessObject<RtcBaseObject>> dataAccessObjects = RealTimeControlToolsConfigXmlConverter.ConvertToDataAccessObjects(ruleElements, triggerElements, logHandler);

            IGrouping<string, IRtcDataAccessObject<RtcBaseObject>>[] controlGroupGroups = dataAccessObjects.GroupBy(o => o.ControlGroupName).ToArray();
            if (!controlGroupGroups.Any())
            {
                logHandler.ReportErrorFormat(Resources.RealTimeControlDataConfigXmlReader_Read_Could_not_read_control_groups_from_file___0___, toolsConfigFilePath);
                return controlGroups;
            }

            foreach (IGrouping<string, IRtcDataAccessObject<RtcBaseObject>> group in controlGroupGroups)
            {
                var connector = new RealTimeControlToolsConfigComponentConnector(group.Key);
                IControlGroup controlGroup = connector.AssembleControlGroup(group.ToArray());

                controlGroups.Add(controlGroup);
            }

            List<RTCTimeSeriesXML> allElements = dataConfigObject.importSeries.timeSeries.Concat(dataConfigObject.exportSeries.timeSeries).ToList();
            List<PITimeSeriesXML> timeSeriesElements = allElements.Select(e => e.PITimeSeries).Where(t => t.locationId != null).ToList();

            var dataConfigSetter = new RealTimeControlDataConfigXmlSetter(logHandler);
            dataConfigSetter.SetInterpolationAndExtrapolationRtcComponents(timeSeriesElements, controlGroups);

            List<HydraulicRule> hydraulicRules = controlGroups
                                                 .SelectMany(g => g.Rules.OfType<HydraulicRule>())
                                                 .Where(r => !(r is FactorRule)).ToList();
            dataConfigSetter.SetTimeLagOnHydraulicRules(allElements, hydraulicRules, modelTimeStep);

            return controlGroups;
        }
    }
}