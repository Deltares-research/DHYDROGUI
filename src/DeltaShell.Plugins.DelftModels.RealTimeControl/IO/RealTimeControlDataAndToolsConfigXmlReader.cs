using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.Dimr.RtcXsd;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO
{
    /// <summary>
    /// This class is responsible for building complete controlgroups from the data config and tools config
    /// XML files.
    /// </summary>
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
            RTCDataConfigComplexType dataConfigObject;
            try
            {
                dataConfigObject = delftConfigXmlParser.Read<RTCDataConfigComplexType>(dataConfigFilePath);
            }
            catch (FileNotFoundException e)
            {
                logHandler.ReportError(e.Message);
                return controlGroups;
            }

            RtcToolsConfigComplexType toolsConfigObject;
            try
            {
                toolsConfigObject = delftConfigXmlParser.Read<RtcToolsConfigComplexType>(toolsConfigFilePath);
            }
            catch (FileNotFoundException e)
            {
                logHandler.ReportError(e.Message);
                return controlGroups;
            }

            RuleComplexType[] ruleElements = toolsConfigObject.rules?.ToArray() ?? new RuleComplexType[0];
            TriggerComplexType[] triggerElements = toolsConfigObject.triggers?.ToArray() ?? new TriggerComplexType[0];

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

            List<RTCTimeSeriesComplexType> allElements = dataConfigObject.importSeries.timeSeries.Concat(dataConfigObject.exportSeries.timeSeries).ToList();
            List<PITimeSeriesComplexType> timeSeriesElements = allElements.Select(e => e.PITimeSeries).Where(t => t?.locationId != null).ToList();

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