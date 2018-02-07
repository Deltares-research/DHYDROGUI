using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.XmlValidation;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public static class RealTimeControlXmlWriter
    {
        private static readonly XNamespace Fns = "http://www.wldelft.nl/fews";
        private static readonly XNamespace Pi = "http://www.wldelft.nl/fews/PI";
        private static readonly XNamespace Xsi = "http://www.w3.org/2001/XMLSchema-instance";
        private static readonly XNamespace OpenDa = "http://www.openda.org";

        public const string RtcToolsConfigXsd = "rtcToolsConfig.xsd";
        public const string RtcRuntimeConfigxsd = "rtcRuntimeConfig.xsd";
        public const string PiTimeseriesxsd = "pi_timeseries.xsd";
        public const string RtcDataConfigXsd = "rtcDataConfig.xsd";
        public const string TreeVectorxsd = "treeVector.xsd";

        private static ILog Log = LogManager.GetLogger(typeof(RealTimeControlXmlWriter));

        public static void CopyXsds(string copyToDirectory)
        {
            foreach (var xsdFile in Directory.GetFiles(DimrApiDataSet.RtcToolsDllPath).ToList().Where(f => f.EndsWith("xsd")))
            {
                File.Copy(xsdFile, copyToDirectory + Path.DirectorySeparatorChar + Path.GetFileName(xsdFile), true);
            }
        }

        /// <summary>
        /// Generate a header for the xml
        /// </summary>
        /// <param name="xNamespace"></param>
        /// <param name="addRtcNameSpave">
        /// Namesnape for rtc (rtc:) is not necessary for all xml's.
        /// </param>
        /// <param name="xsdPath"></param>
        /// <param name="xsd"></param>
        /// <param name="node">
        /// ParentNode to which header attributes will be applied..
        /// </param>
        /// <returns></returns>
        private static XElement AddHeader(XNamespace xNamespace, bool addRtcNameSpave, string xsdPath, string xsd, XElement node)
        {
            var schemaLocation = Path.Combine(xsdPath, xsd);
            schemaLocation = schemaLocation.Replace(" ","%20");

            node.Add(new XAttribute(XNamespace.Xmlns + "xsi", Xsi.NamespaceName));
            if (addRtcNameSpave)
            {
                node.Add(new XAttribute(XNamespace.Xmlns + "rtc", xNamespace.NamespaceName));
            }
            node.Add(new XAttribute("xmlns", xNamespace.NamespaceName));
            node.Add(new XAttribute(Xsi + "schemaLocation", xNamespace.NamespaceName + " " + schemaLocation));
            return node;
        }

        private static XDocument GetRuntimeXDocument(string xsdPath)
        {
            var xDocument = new XDocument();

            var xDeclaration = new XDeclaration("1.0", "UTF-8", "yes");
            xDocument.Declaration = xDeclaration;
            var xElement = AddHeader(Fns, true, xsdPath, "rtcRuntimeConfig.xsd", new XElement(Fns + "rtcRuntimeConfig"));
            xDocument.Add(xElement);
            return xDocument;
        }

        private static XDocument GetToolsConfigXDocument(string xsdPath)
        {
            var xDocument = new XDocument();

            var xDeclaration = new XDeclaration("1.0", "UTF-8", "yes");
            xDocument.Declaration = xDeclaration;
            var xElement = AddHeader(Fns, true, xsdPath, "rtcToolsConfig.xsd", new XElement(Fns + "rtcToolsConfig",
                                                                                   new XElement(Fns + "general",
                                                                                                new XElement(Fns + "description", "RTC Model DeltaShell"),
                                                                                                new XElement(Fns + "poolRoutingScheme", "Theta"),
                                                                                                new XElement(Fns + "theta", "0.5")
                                                                                       )));
            xDocument.Add(xElement);

            return xDocument;
        }

        private static XDocument GetDataConfigXDocument(string xsdPath)
        {
            var xDocument = new XDocument();

            var xDeclaration = new XDeclaration("1.0", "UTF-8", "yes");
            xDocument.Declaration = xDeclaration;
            xDocument.Add(AddHeader(Fns, true, xsdPath, "rtcDataConfig.xsd", new XElement(Fns + "rtcDataConfig")));
            return xDocument;
        }

        private static XDocument GetTimeSeriesXDocument(string xsdPath)
        {
            var xDocument = new XDocument();

            var xDeclaration = new XDeclaration("1.0", "UTF-8", "yes");
            xDocument.Declaration = xDeclaration;
            var xElement = AddHeader(Pi, false, xsdPath, "pi_timeseries.xsd", new XElement(Pi + "TimeSeries"));
            xDocument.Add(xElement);
            return xDocument;
        }

        private static XDocument GetStateVectorXDocument(string xsdPath)
        {
            var xDocument = new XDocument();

            var xDeclaration = new XDeclaration("1.0", "UTF-8", "yes");
            xDocument.Declaration = xDeclaration;
            var xElement = AddHeader(OpenDa, false, xsdPath, "treeVector.xsd", new XElement(OpenDa + "treeVectorFile"));
            xDocument.Add(xElement);
            return xDocument;
        }

        public static XDocument GetRuntimeXml(string xsdPath, ITimeDependentModel timeDependentModel, bool limitMemory, int logLevel)
        {
            var xmlValidator = new Validator(new List<string> { xsdPath + Path.DirectorySeparatorChar + RtcRuntimeConfigxsd });

            var xDocument = GetRuntimeXDocument(xsdPath);

            if (xDocument.Root != null)
            {
                xDocument.Root.Add(GetXmlRuntimeFromModel(timeDependentModel));
                xDocument.Root.Add(GetXmlForLimitedMemoryOption(limitMemory));
                // check if we are running in 'debug' mode (from tests)
                if (logLevel > 3)
                {
                    xDocument.Root.Add(GetXmlForLoggingOptions(logLevel));
                }
            }

            xmlValidator.Validate(xDocument);
            return xDocument;
        }

        public static XDocument GetToolsConfigXml(string xsdPath, IList<ControlGroup> controlGroups, bool includeExtraStatesForRestart=false)
        {
            if (xsdPath == string.Empty)
            {
                xsdPath = DimrApiDataSet.RtcToolsDllPath;
            }
            var xmlValidator = new Validator(new List<string> { xsdPath + Path.DirectorySeparatorChar + RtcToolsConfigXsd });
            var xDocument = GetToolsConfigXDocument(xsdPath);

            if (xDocument.Root != null)
            {
                // xDocument.Root.Add(GetXmlSignalsFromControlGroups(controlGroups)); // Will be used later
                xDocument.Root.Add(GetXmlRulesFromControlGroups(controlGroups, includeExtraStatesForRestart));
                xDocument.Root.Add(GetXmlConditionsFromControlGroups(controlGroups));
            }

            AddUnitDelayComponents(xDocument, controlGroups);

            xmlValidator.Validate(xDocument);
            return xDocument;
        }

        public static XDocument GetDataConfigXml(string xsdPath, ITimeDependentModel timeDependentModel, IList<ControlGroup> controlGroups, string timeSeriesPathFileName)
        {
            if (xsdPath == string.Empty)
            {
                xsdPath = DimrApiDataSet.RtcToolsDllPath;
            }
            var schemas = new List<string> { xsdPath + Path.DirectorySeparatorChar + RtcDataConfigXsd };

            var xmlValidator = new Validator(schemas);

            var xDocument = GetDataConfigXDocument(xsdPath);

            if (xDocument.Root != null)
            {
                GetXmlInputsFromControlGroups(timeDependentModel, xDocument.Root, controlGroups, timeSeriesPathFileName);
                xDocument.Root.Add(GetXmlOutputsFromControlGroups(controlGroups));
            }

            xmlValidator.Validate(xDocument);
            return xDocument;
        }

        public static XDocument GetTimeSeriesXml(string xsdPath, ITimeDependentModel timeDependentModel, IList<ControlGroup> controlGroups)
        {
            var xmlValidator =
                new Validator(new List<string> {xsdPath + Path.DirectorySeparatorChar + PiTimeseriesxsd});
            var xDocument = GetTimeSeriesXDocument(xsdPath);
            if (xDocument.Root != null)
            {
                GetXmlTimeSeriesFromControlGroups(xDocument.Root, controlGroups, timeDependentModel);
            }
            if (xDocument.Root.Nodes().Any())
            {
                xmlValidator.Validate(xDocument);
                return xDocument;
            }

            return null;
        }

        public static XDocument GetStateVectorXml(string xsdPath, IList<ControlGroup> controlGroups)
        {
            var xmlValidator = new Validator(new List<string> { xsdPath + Path.DirectorySeparatorChar + TreeVectorxsd });

            var xDocument = GetStateVectorXDocument(xsdPath);

            if (xDocument.Root != null)
            {
                var treeVector = new XElement(OpenDa + "treeVector");
                treeVector.Add(GetXmlStateFromControlGroups(controlGroups));
                if (treeVector.Nodes().Any())
                {
                    xDocument.Root.Add(treeVector);
                }
            }

            xmlValidator.Validate(xDocument);
            return xDocument;
        }


        private static XElement GetXmlConditionsFromControlGroups(IEnumerable<ControlGroup> controlGroups)
        {
            var triggersElement = new XElement(Fns + "triggers");
            foreach (var group in controlGroups)
            {
                //find start condition for each output
                foreach (var output in group.Outputs)
                {
                    var startObject = ControlGroupHelper.StartObjectsForOutput(group, output).FirstOrDefault();
                    if (startObject is ConditionBase)
                    {
                        var gf = group.Name;
                        var condition = (ConditionBase) startObject;
                        triggersElement.Add(condition.ToXml(Fns, gf));
                    }
                }
            }

            return triggersElement.HasElements ? triggersElement: null;
        }

        // RTCTools needs extra rules for each pid-controller that is controlled by a condition. This is because if a pid-controller
        // is switched off, and switched on at a later moment in time, its value at switch-off time must be retained as starting value
        // at switch-on time
        private static XElement GetGlobalRuleForPIDMemoryBackup(string xmlName, string ruleID)
        {
            var rulesElement = new XElement(Fns + "rule");

            rulesElement.Add(new XElement(Fns + "unitDelay", new XAttribute("id", ruleID + "_unitDelay"),
                                            new XElement(Fns + "input",
                                                        new XElement(Fns + "x", xmlName)),
                                            new XElement(Fns + "output",
                                                        new XElement(Fns + "y", xmlName))));
            return rulesElement;
        }

        private static XElement GetXmlRulesFromControlGroups(IEnumerable<ControlGroup> controlGroups, bool includeExtraStatesForRestart)
        {
            var oneMemoryBackupPerOutput = new HashSet<string>();
            var rulesElement = new XElement(Fns + "rules");

            // Create two lists so we can selectively add the elements we find in the loop to one of the lists. After, we create one list with
            // the elements in a specific order ('global' elements which should appear at the top, and the remaining elements below them).
            var rulesElementXmlGlobalContents = new List<XElement>();
            var rulesElementXmlContents = new List<XElement>();

            foreach (var group in controlGroups)
            {

                foreach (var signal in group.Signals)
                {
                    if (signal is LookupSignal)
                    {
                        string gf = group.Name;
                        signal.StoreAsRule = true;
                        rulesElementXmlContents.Add(signal.ToXml(Fns, gf));
                    }
                }

                foreach (var rule in group.Rules)
                {
                    // RTCTools needs and extra state variable for controllers that use the previous state
                    // when determining the new state.
                    // If a restart file has to be written, the extra state varable is needed for all controllers,
                    // to be able to store a complete state.
                    if (includeExtraStatesForRestart || rule is PIDRule || rule is IntervalRule || rule is RelativeTimeRule)
                    {
                        var outputXmlName = rule.Outputs.First().XmlName;
                        if (!oneMemoryBackupPerOutput.Contains(outputXmlName) && group.Conditions.Any(c => (c.TrueOutputs.Contains(rule) || c.FalseOutputs.Contains(rule))))
                        {
                            // RTCTools needs extra rule for this controller
                            rulesElementXmlGlobalContents.Add(GetGlobalRuleForPIDMemoryBackup(outputXmlName, rule.Name));
                            oneMemoryBackupPerOutput.Add(outputXmlName);
                        }
                    }

                    // add support for standard setpint or lookup table
                    var setPointId = group.Name + rule.Name + "_SP";
                    foreach (var signal in group.Signals)
                    {
                        if (signal is LookupSignal)
                        {
                            foreach (var ruleBase in signal.RuleBases)
                            {
                                if (ruleBase.IsLinkedFromSignal() && ruleBase.Name == rule.Name)
                                {
                                    setPointId = group.Name + signal.Name;
                                }
                            }
                        }
                    }

                    foreach (var input in rule.Inputs)
                    {
                        input.SetPoint = setPointId;
                    }

                    var gf = group.Name;
                    rulesElementXmlContents.Add(rule.ToXml(Fns, gf));
                }
            }

            rulesElement.Add(rulesElementXmlGlobalContents.Concat(rulesElementXmlContents));

            return rulesElement.HasElements ? rulesElement : null;
        }

        private static XElement GetXmlSignalsFromControlGroups(IEnumerable<ControlGroup> controlGroups)
        {
            var signalsElement = new XElement(Fns + "signals");

            // Create two lists so we can selectively add the elements we find in the loop to one of the lists. After, we create one list with
            // the elements in a specific order ('global' elements which should appear at the top, and the remaining elements below them).
            var signalsElementXmlGlobalContents = new List<XElement>();
            var signalsElementXmlContents = new List<XElement>();

            foreach (var group in controlGroups)
            {
                foreach (var signal in group.Signals)
                {
                    if (!(signal is LookupSignal))
                    {
                        string gf = group.Name;
                        signalsElementXmlContents.Add(signal.ToXml(Fns, gf));
                    }
                }
            }

            signalsElement.Add(signalsElementXmlGlobalContents.Concat(signalsElementXmlContents));

            return signalsElement.HasElements ? signalsElement : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeDependentModel"></param>
        /// example
        /// <startDate date="1999-04-15" time="01:00:00"/>
        /// <endDate date="1999-06-15" time="00:00:00"/>
        /// <timeStep unit="minute" multiplier="15"/>
        /// <numberEnsembles>1</numberEnsembles>
        /// xsd
        /// 	<complexType name="UserDefinedRuntimeConfigComplexType">
        /// 			<sequence>
        /// 				<element name="startDate" type="rtc:DateTimeComplexType"/>
        /// 				<element name="endDate" type="rtc:DateTimeComplexType"/>
        /// 				<element name="timeStep" type="rtc:TimeStepComplexType"/>
        /// 				<element name="numberEnsembles" type="int" default="1" minOccurs="0"/>
        /// 			</sequence>
        /// 	</complexType>
        ///  The time unit element has three attributes, unit and divider and multiplier.
        ///  the unit is second, minute, hour, week, month year.
        /// <returns></returns>
        private static XElement GetXmlRuntimeFromModel(ITimeDependentModel timeDependentModel)
        {
            var periodElement = new XElement(Fns + "period");
            var userDefinedElement = new XElement(Fns + "userDefined");
            userDefinedElement.Add(DateTimeToXElement("startDate", timeDependentModel.StartTime));
            userDefinedElement.Add(DateTimeToXElement("endDate", timeDependentModel.StopTime));

            userDefinedElement.Add(TimeStepToXml(Fns, timeDependentModel.TimeStep));

            periodElement.Add(userDefinedElement);

            return periodElement;
        }

        private static XElement GetXmlForLimitedMemoryOption(bool limitMemory)
        {
            var modelElement = new XElement(Fns + "mode", new XElement(Fns + "simulation", new XElement(Fns + "limitedMemory",
                limitMemory.ToString(CultureInfo.InvariantCulture).ToLower())));
            return modelElement;
        }

        private static XElement GetXmlForLoggingOptions(int logLevel)
        {
            var loggingElement = new XElement(Fns + "logging");
            loggingElement.Add(new XElement(Fns + "logLevel", logLevel));
            loggingElement.Add(new XElement(Fns + "flushing", logLevel > 3 ? "true" : "false"));
            return loggingElement;
        }

        public static XElement TimeStepToXml(XNamespace xNamespace, TimeSpan timeStep)
        {
            var units = new[]
                            {
                                new {unit = "week", multiplier = 7*24*60*60}, new {unit = "day", multiplier = 24*60*60},
                                new {unit = "hour", multiplier = 60*60}, new {unit = "minute", multiplier = 60}
                            };

            var seconds = timeStep.TotalSeconds;
            var unit = new {unit = "second", multiplier = 1};
            for (var i = 0; i < units.Length; i++)
            {
                if (seconds % units[i].multiplier == 0)
                {
                    unit = units[i];
                    break;
                }
            }
            return new XElement(xNamespace + "timeStep", new XAttribute("unit", unit.unit), new XAttribute("multiplier", seconds / unit.multiplier));
        }

        public static XElement DateTimeToXElement(string tag, DateTime dateTime)
        {
            return new XElement(Fns + tag,
                                new XAttribute("date", string.Format("{0:0000}-{1:00}-{2:00}",
                                                                     dateTime.Year,
                                                                     dateTime.Month,
                                                                     dateTime.Day)),
                                new XAttribute("time", string.Format("{0:00}:{1:00}:{2:00}",
                                                                     dateTime.Hour,
                                                                     dateTime.Minute,
                                                                     dateTime.Second)));
        }


        /// <summary>
        /// from rtcDataConfig.xsd:
        /// <complexType name="OpenMIExchangeItemComplexType">
        ///   <sequence>
        ///    <element name="elementId" type="string"/>
        ///    <element name="quantityId" type="rtc:quantityIdEnumStringType"/>
        ///   </sequence>
        /// </complexType>
        /// <simpleType name="quantityIdEnumStringType">
        ///   <restriction base="string">
        ///     <enumeration value="Water level"/>
        ///     <enumeration value="Crest level"/>
        ///     <enumeration value="Discharge"/>
        ///   </restriction>
        /// </simpleType>
        /// 
        /// todo: quantityId is enum this is blocker for rtc / waterflow integration
        /// </summary>
        /// <param name="timeDependentModel"></param>
        /// <param name="root"></param>
        /// <param name="controlGroups"></param>
        /// <param name="timeSeriesPathFileName"></param>
        /// <returns></returns>
        private static void GetXmlInputsFromControlGroups(ITimeDependentModel timeDependentModel, XElement root, IEnumerable<ControlGroup> controlGroups, string timeSeriesPathFileName)
        {
            var import = new XElement(Fns + "importSeries");
            if (timeSeriesPathFileName != null)
            {
                import.Add(new XElement(Fns + "PITimeSeriesFile",
                               new XElement(Fns + "timeSeriesFile", timeSeriesPathFileName),
                               new XElement(Fns + "useBinFile", "false")));
            }

            // check if item has already been writtem and if yes skip
            var inputItems = new HashSet<string>();

            var serieNames = new HashSet<string>();

            foreach (var group in controlGroups)
            {
                foreach (var input in group.Inputs)
                {
                    if (inputItems.Contains(input.Name))
                    {
                        // avoid duplicates
                        continue;
                    }
                    inputItems.Add(input.Name);
                    var tempElement = new XElement(Fns + "timeSeries", new XAttribute("id", input.XmlName));

                    if (input.IsConnected)
                    {
                        string inputLocationNameWithoutHash = input.LocationName.Replace("##","~~");
                        tempElement.Add(new XElement(Fns + "OpenMIExchangeItem",
                                                     new XElement(Fns + "elementId", inputLocationNameWithoutHash),
                                                     new XElement(Fns + "quantityId", input.ParameterName),
                                                     new XElement(Fns + "unit", "m")
                                                     ));
                    }
                    import.Add(tempElement);
                }

                foreach (var conditionBase in group.Conditions)
                {
                    foreach (var importTimeSeries in conditionBase.ToDataConfigImportSeries(group.Name, Fns))
                    {
                        var key = importTimeSeries.Attribute("id").Value;

                        if (serieNames.Contains(key))
                        {
                            continue;
                        }

                        import.Add(importTimeSeries);

                        serieNames.Add(key);

                    }
                }

                foreach (var ruleBase in group.Rules)
                {
                    // some rule require their output item also as input. The user will not have to make this connection. RTCTools
                    // does require an explicit reference. This allows future implementations to user other exchange itens as input
                    // used by RelativeTimeRule and PIDRule
                    import.Add(ruleBase.OutputAsInputToDataConfigXml(Fns));
                    // add tines series that are part of the rules to the xml
                    foreach (var timeSeries in ruleBase.XmlImportTimeSeries(@group.Name, timeDependentModel.StartTime, timeDependentModel.StopTime, timeDependentModel.TimeStep))
                    {
                        import.Add(timeSeries.ToDataConfigXml(Fns, false));
                    }
                }

                // if no timeseries where added add the root node, else the XSD validation breaks
                if (!import.Nodes().Any())
                {
                    import.Add(new XElement(Fns + "timeSeries", new XAttribute("id", "Undefined")));
                }
            }
            root.Add(import);
        }

        private static XElement GetXmlOutputsFromControlGroups(IEnumerable<ControlGroup> controlGroups)
        {
            var export = new XElement(Fns + "exportSeries",
                 new XElement(Fns + "CSVTimeSeriesFile", ""), //handy for debug 
                            new XElement(Fns + "PITimeSeriesFile",
                                     new XElement(Fns + "timeSeriesFile","timeseries_export.xml"),
                                     new XElement(Fns + "useBinFile","false")
                                     )
                            );

            // check if item has already been writtem and if yes skip
            var outputItems = new HashSet<string>();
            var seriesNames = new HashSet<string>();

            foreach (var group in controlGroups)
            {
                foreach (var output in group.Outputs)
                {
                    string nameWithoutHashSigns = output.Name.Replace("##", "~~");
                    if (outputItems.Contains(nameWithoutHashSigns))
                    {
                        continue;
                    }
                    outputItems.Add(nameWithoutHashSigns);

                    var openMi = new XElement(Fns + "timeSeries", new XAttribute("id", output.XmlName));
                    if (output.IsConnected)
                    {
                        string locationNameWithoutHashSigns = output.LocationName;
                        openMi.Add(new XElement(Fns + "OpenMIExchangeItem",
                                                     new XElement(Fns + "elementId", locationNameWithoutHashSigns),
                                                     new XElement(Fns + "quantityId", output.ParameterName),
                                                     new XElement(Fns + "unit", "m")));
                    }
                    export.Add(openMi);
                }
                foreach (var conditionBase in group.Conditions)
                {
                    foreach (var exportTimeSeries in conditionBase.ToDataConfigExportSeries(Fns, group.Name))
                    {
                        var key = exportTimeSeries.Attribute("id").Value;

                        if (seriesNames.Contains(key))
                        {
                            continue;
                        }

                        export.Add(exportTimeSeries);

                        seriesNames.Add(key);
                    }
                }

                foreach (var ruleBase in group.Rules)
                {
                    foreach (var timeSeries in ruleBase.XmlExportTimeSeries(@group.Name))
                    {
                        export.Add(timeSeries.ToDataConfigXml(Fns, true));
                    }
                }

                foreach (var signal in group.Signals)
                {
                    foreach (var timeSeries in signal.XmlExportTimeSeries(@group.Name))
                    {
                        export.Add(timeSeries.ToDataConfigXml(Fns, true));
                    }
                }
            }

            AddHydraulicRulesWithTimeLagAsTimeSerieToDataConfig(export, controlGroups.SelectMany(controlGroup => controlGroup.Rules.OfType<HydraulicRule>().Where(r => r.TimeLagInTimeSteps > 0)));

            // if no timeseries where added add the root node, else the XSD validation breaks
            if (export.Nodes().Count() == 0)
            {
                export.Add(new XElement(Fns + "timeSeries", new XAttribute("id", string.Empty)));
            }

            return export;
        }

        /// <summary>
        /// Returns a XElement with the initial state of the RTC calculation.
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<XElement> GetXmlStateFromControlGroups(IEnumerable<ControlGroup> controlGroups)
        {
            var names = new HashSet<string>(); // skip duplicates
            var states = new List<XElement>();
            foreach (var group in controlGroups)
            {
                foreach (var output in group.Outputs)
                {
                    if (names.Contains(output.Name))
                    {
                        continue;
                    }
                    names.Add(output.Name);

                    states.Add(new XElement(OpenDa + "treeVectorLeaf",
                                            new XAttribute("id", output.XmlName),
                                            new XElement(OpenDa + "vector", output.Value)));
                }
                foreach (var ruleBase in group.Rules)
                {
                    foreach (var state in ruleBase.ToImportState(OpenDa))
                    {
                        var name = state.Attributes().First(a => a.Name == "id").Value;
                        if (names.Contains(name))
                        {
                            continue;
                        }
                        names.Add(name);
                        states.Add(state);
                    }
                }
            }
            return states;
        }

        /// <summary>
        /// Write timeseries data for the rules that need an external times series to a xml file (pi_timeseries.xsd).
        /// The definition of this time series is written to rtcDataConfig.xml
        /// Time series in rtcDataConfig.xml that are of type PITimeSeries (opposed to OpenMIExchangeItem) 
        /// have data in this xml file.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="controlGroups"></param>
        /// <param name="timeDependentModel"></param>
        /// <returns></returns>
        private static void GetXmlTimeSeriesFromControlGroups(XElement root, IEnumerable<ControlGroup> controlGroups, ITimeDependentModel timeDependentModel)
        {
            var seriesNames = new HashSet<string>();
            foreach (var group in controlGroups)
            {
                foreach (var ruleBase in group.Rules)
                {
                    var ruleAsPid = ruleBase as PIDRule;
                    if (ruleAsPid != null && ruleAsPid.PidRuleSetpointType == PIDRule.PIDRuleSetpointType.Constant)
                    {
                        Log.WarnFormat(Resources.RealTimeControlXmlWriter_GetXmlTimeSeriesFromControlGroups_PIDRule__0__time_series_will_not_be_included_in_the_DIMR_XML_as_Set_Point_Type_is_Constant, ruleAsPid.Name);
                        continue;
                    }
                    foreach (var timeSeries in ruleBase.XmlImportTimeSeries(@group.Name, timeDependentModel.StartTime, timeDependentModel.StopTime, timeDependentModel.TimeStep))
                    {
                        var key = group.Name + "_" + timeSeries.LocationId + "_" + timeSeries.ParameterId;
                        if (seriesNames.Contains(key))
                        {
                            continue;
                        }

                        root.Add(timeSeries.ToTimeSeriesXml(Pi, timeDependentModel.TimeStep));

                        seriesNames.Add(key);
                    }
                }
                foreach (var conditionBase in group.Conditions)
                {
                    foreach (var timeSeries in conditionBase.XmlImportTimeSeries(@group.Name, timeDependentModel.StartTime, timeDependentModel.StopTime, timeDependentModel.TimeStep))
                    {
                        var key = group.Name + "_" + timeSeries.LocationId + "_" + timeSeries.ParameterId;

                        if (seriesNames.Contains(key))
                        {
                            continue;
                        }

                        root.Add(timeSeries.ToTimeSeriesXml(Pi, timeDependentModel.TimeStep));

                        seriesNames.Add(key);
                    }
                }
            }
        }


        /// <summary>
        /// TimeLags are presented by a UnitDelay component in ToolsConfig
        /// </summary>
        /// <param name="controlGroups"></param>
        private static void AddUnitDelayComponents(XDocument xDocument, IList<ControlGroup> controlGroups)
        {
            var timeLagHydraulicRules = controlGroups.SelectMany(controlGroup => controlGroup.Rules.OfType<HydraulicRule>().Where(r => r.TimeLagInTimeSteps > 0)).ToList();
            var inputNames = timeLagHydraulicRules.SelectMany(timeLagHydraulicRule => timeLagHydraulicRule.Inputs.OfType<Input>().Select(i => i.XmlName)).Distinct();

            var xElementComponents = new XElement(Fns + "components");

            var passedNames = new HashSet<string>();
            foreach (var inputName in inputNames)
            {
                //<component>
                //    <unitDelay id="multipleDelay">
                //        <input>
                //            <x>input</x> 
                //        </input>
                //        <output>
                //            <yVector>delayedInput</yVector> 
                //        </output>
                //  </unitDelay>
                //</component>

                if (passedNames.Contains(inputName)) //avoid double elements
                {
                    continue;
                }
                passedNames.Add(inputName);

                xElementComponents.Add(new XElement(Fns + "component",
                                    new XElement(Fns + "unitDelay",
                                        new XAttribute("id", inputName + "Delay"),
                                        new XElement(Fns + "input", new XElement(Fns + "x", inputName)),
                                        new XElement(Fns + "output", new XElement(Fns + "yVector", "delayed" + inputName))
                                    )
                                ));
            }

            var directionalConditions =
                controlGroups.SelectMany(controlGroup => controlGroup.Conditions.OfType<DirectionalCondition>());

            foreach (var directionalCondition in directionalConditions)
            {
                if (passedNames.Contains(directionalCondition.Input.XmlName)) //avoid double elements
                {
                    continue;
                }
                passedNames.Add(directionalCondition.Input.XmlName);

                xElementComponents.Add(new XElement(Fns + "component",
                                    new XElement(Fns + "unitDelay",
                                        new XAttribute("id", directionalCondition.Input.XmlName + "Delay"),
                                        new XElement(Fns + "input", new XElement(Fns + "x", directionalCondition.Input.XmlName)),
                                        new XElement(Fns + "output", new XElement(Fns + "y", directionalCondition.GetLaggedInputName()))
                                    )
                                ));
            }

            if(xElementComponents.HasElements)
            {
                var xElementGeneral = xDocument.Root.Elements().First();
                xElementGeneral.AddAfterSelf(xElementComponents);
            }

        }

        private static void AddHydraulicRulesWithTimeLagAsTimeSerieToDataConfig(XElement exportSeries, IEnumerable<HydraulicRule> hydraulicRulesWithLimeLag)
        {
            Dictionary<string, HydraulicRule> dictInputAndHydraulicRuleWithBiggestDelay = new Dictionary<string, HydraulicRule>();
            foreach (var hydraulicRule in hydraulicRulesWithLimeLag)
            {
                foreach (var input in hydraulicRule.Inputs)
                {
                    if(dictInputAndHydraulicRuleWithBiggestDelay.ContainsKey(input.XmlName))
                    {
                        if(hydraulicRule.TimeLagInTimeSteps > dictInputAndHydraulicRuleWithBiggestDelay[input.XmlName].TimeLagInTimeSteps)
                        {
                            dictInputAndHydraulicRuleWithBiggestDelay[input.XmlName] = hydraulicRule;
                        }
                    }
                    else
                    {
                        dictInputAndHydraulicRuleWithBiggestDelay.Add(input.XmlName, hydraulicRule);
                    }
                }
            }

            foreach (var hydraulicRule in dictInputAndHydraulicRuleWithBiggestDelay.Values)
            {
                var input = hydraulicRule.Inputs.First();
                var timeSeriesElement = new XElement(Fns + "timeSeries",
                                            new XAttribute("id", "delayed"+ input.XmlName),
                                            new XAttribute("vectorLength", hydraulicRule.TimeLagInTimeSteps - 1));

                var piTimeSeriesElement = new XElement(Fns + "PITimeSeries",
                                                       new XElement(Fns + "locationId", input.LocationName),
                                                       new XElement(Fns + "parameterId", input.ParameterName));
                timeSeriesElement.Add(piTimeSeriesElement);

                exportSeries.Add(timeSeriesElement);
            }
        }
    }
}
