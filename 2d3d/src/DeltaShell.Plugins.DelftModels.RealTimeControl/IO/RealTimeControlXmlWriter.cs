using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Shell.Core.Workflow;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xml;
using DeltaShell.Plugins.DelftModels.RealTimeControl.XmlValidation;
using log4net;
using static DeltaShell.Plugins.DelftModels.RealTimeControl.RealTimeControlXmlFiles;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO
{
    public class RealTimeControlXmlWriter : IRealTimeControlXmlWriter
    {
        public const string RtcToolsConfigXsd = "rtcToolsConfig.xsd";
        public const string RtcRuntimeConfigxsd = "rtcRuntimeConfig.xsd";
        public const string PiTimeseriesxsd = "pi_timeseries.xsd";
        public const string RtcDataConfigXsd = "rtcDataConfig.xsd";
        public const string TreeVectorxsd = "treeVector.xsd";
        
        private static readonly XNamespace Fns = "http://www.wldelft.nl/fews";
        private static readonly XNamespace Pi = "http://www.wldelft.nl/fews/PI";
        private static readonly XNamespace Xsi = "http://www.w3.org/2001/XMLSchema-instance";
        private static readonly XNamespace OpenDa = "http://www.openda.org";

        private static ILog Log = LogManager.GetLogger(typeof(RealTimeControlXmlWriter));

        /// <inheritdoc />
        public void WriteToXml(RealTimeControlModel model, string directory)
        {
            Ensure.NotNull(model, nameof(model));
            Ensure.NotNullOrEmpty(directory, nameof(directory));
            
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($@"Directory '{directory}' does not exist.");
            }
            
            model.RefreshInitialState();
            model.SetTimeLagHydraulicRulesToTimeSteps(model.ControlGroups, model.TimeStep);

            CopyXsds(directory);

            WriteRuntimeConfigXml(model, directory);
            WriteToolsConfigXml(model, directory);
            WriteTimeSeriesXml(model, directory);
            WriteDataConfigXml(model, directory);
            WriteStateVectorXml(model, directory);
        }

        private static void CopyXsds(string copyToDirectory)
        {
            foreach (string xsdFile in Directory.GetFiles(DimrApiDataSet.RtcXsdDirectory).Where(f => f.EndsWith("xsd")))
            {
                File.Copy(xsdFile, copyToDirectory + Path.DirectorySeparatorChar + Path.GetFileName(xsdFile), true);
            }
        }

        private static void WriteRuntimeConfigXml(RealTimeControlModel model, string directory)
        {
            GetRuntimeConfigXml(directory, model, model.LimitMemory, model.LogLevel).Save(Path.Combine(directory, XmlRuntime));
        }
        
        public static XDocument GetRuntimeConfigXml(string xsdPath, RealTimeControlModel realTimeControlModel, bool limitMemory, int logLevel)
        {
            var xmlValidator = new Validator(new List<string> { xsdPath + Path.DirectorySeparatorChar + RtcRuntimeConfigxsd });

            XDocument xDocument = GetRuntimeConfigXDocument(xsdPath);

            if (xDocument.Root != null)
            {
                xDocument.Root.Add(GetXmlRuntimeFromModel(realTimeControlModel));
                xDocument.Root.Add(GetXmlForLimitedMemoryOption(limitMemory));
                // check if we are running in 'debug' mode (from tests)
                if (logLevel > 3)
                {
                    xDocument.Root.Add(GetXmlForLoggingOptions(logLevel));
                }

                // check if we want to write restart files
                if (realTimeControlModel.WriteRestart)
                {
                    xDocument.Root.Add(GetXmlRestartStateFromModel(realTimeControlModel));
                }
            }

            xmlValidator.Validate(xDocument);
            return xDocument;
        }
        
        private static void WriteToolsConfigXml(RealTimeControlModel model, string directory)
        {
            GetToolsConfigXml(directory, model.ControlGroups, model.WriteRestart || model.UseRestart).Save(Path.Combine(directory, XmlTools));
        }

        public static XDocument GetToolsConfigXml(string xsdPath, IList<ControlGroup> controlGroups, bool includeExtraStatesForRestart = false)
        {
            if (xsdPath == string.Empty)
            {
                xsdPath = DimrApiDataSet.RtcXsdDirectory;
            }

            var xmlValidator = new Validator(new List<string> { xsdPath + Path.DirectorySeparatorChar + RtcToolsConfigXsd });
            XDocument xDocument = GetToolsConfigXDocument(xsdPath);

            if (xDocument.Root != null)
            {
                xDocument.Root.Add(GetXmlRulesFromControlGroups(controlGroups, includeExtraStatesForRestart));
                xDocument.Root.Add(GetTriggersElementFromControlGroups(controlGroups));
            }

            AddUnitDelayComponents(xDocument, controlGroups);

            xmlValidator.Validate(xDocument);
            return xDocument;
        }
        
        private static void WriteDataConfigXml(RealTimeControlModel model, string directory)
        {
            string timeSeriesPathFileName = File.Exists(Path.Combine(directory, RealTimeControlXmlFiles.XmlTimeSeries)) ? RealTimeControlXmlFiles.XmlTimeSeries : null;
            GetDataConfigXml(directory, model, model.ControlGroups, timeSeriesPathFileName).Save(Path.Combine(directory, XmlData));
        }

        public static XDocument GetDataConfigXml(string xsdPath, ITimeDependentModel timeDependentModel, IList<ControlGroup> controlGroups, string timeSeriesPathFileName)
        {
            if (xsdPath == string.Empty)
            {
                xsdPath = DimrApiDataSet.RtcXsdDirectory;
            }

            var schemas = new List<string> { xsdPath + Path.DirectorySeparatorChar + RtcDataConfigXsd };

            var xmlValidator = new Validator(schemas);

            XDocument xDocument = GetDataConfigXDocument(xsdPath);

            if (xDocument.Root != null)
            {
                GetXmlInputsFromControlGroups(timeDependentModel, xDocument.Root, controlGroups, timeSeriesPathFileName);
                xDocument.Root.Add(GetXmlOutputsFromControlGroups(controlGroups));
            }

            xmlValidator.Validate(xDocument);
            return xDocument;
        }

        private static void WriteTimeSeriesXml(RealTimeControlModel model, string directory)
        {
            GetTimeSeriesXml(directory, model, model.ControlGroups)?.Save(Path.Combine(directory, RealTimeControlXmlFiles.XmlTimeSeries));
        }
        
        public static XDocument GetTimeSeriesXml(string xsdPath, ITimeDependentModel timeDependentModel, IList<ControlGroup> controlGroups)
        {
            var xmlValidator =
                new Validator(new List<string> { xsdPath + Path.DirectorySeparatorChar + PiTimeseriesxsd });
            XDocument xDocument = GetTimeSeriesXDocument(xsdPath);
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
        
        private static void WriteStateVectorXml(RealTimeControlModel model, string directory)
        {
            if (!model.UseRestart)
            {
                GetStateVectorXml(directory, model.ControlGroups).Save(Path.Combine(directory, XmlImportState));
            }
        }

        public static XDocument GetStateVectorXml(string xsdPath, IList<ControlGroup> controlGroups)
        {
            var xmlValidator = new Validator(new List<string> { xsdPath + Path.DirectorySeparatorChar + TreeVectorxsd });

            XDocument xDocument = GetStateVectorXDocument(xsdPath);

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

        public static XElement GetTimeStepXElement(XNamespace xNamespace, TimeSpan timeStep, string timestepName = "timeStep", bool noAttributes = false)
        {
            var units = new[]
            {
                new
                {
                    unit = "week",
                    multiplier = 7 * 24 * 60 * 60
                },
                new
                {
                    unit = "day",
                    multiplier = 24 * 60 * 60
                },
                new
                {
                    unit = "hour",
                    multiplier = 60 * 60
                },
                new
                {
                    unit = "minute",
                    multiplier = 60
                }
            };

            double seconds = timeStep.TotalSeconds;
            var unit = new
            {
                unit = "second",
                multiplier = 1
            };
            for (var i = 0; i < units.Length; i++)
            {
                if (seconds % units[i].multiplier == 0)
                {
                    unit = units[i];
                    break;
                }
            }

            if (noAttributes)
            {
                return new XElement(xNamespace + timestepName, seconds);
            }

            return new XElement(xNamespace + timestepName, new XAttribute("unit", unit.unit), new XAttribute("multiplier", seconds / unit.multiplier));
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
            string schemaLocation = Path.Combine(xsdPath, xsd);
            schemaLocation = schemaLocation.Replace(" ", "%20");

            node.Add(new XAttribute(XNamespace.Xmlns + "xsi", Xsi.NamespaceName));
            if (addRtcNameSpave)
            {
                node.Add(new XAttribute(XNamespace.Xmlns + "rtc", xNamespace.NamespaceName));
            }

            node.Add(new XAttribute("xmlns", xNamespace.NamespaceName));
            node.Add(new XAttribute(Xsi + "schemaLocation", xNamespace.NamespaceName + " " + schemaLocation));
            return node;
        }

        private static XDocument GetRuntimeConfigXDocument(string xsdPath)
        {
            var xDocument = new XDocument();

            var xDeclaration = new XDeclaration("1.0", "UTF-8", "yes");
            xDocument.Declaration = xDeclaration;
            XElement xElement = AddHeader(Fns, true, xsdPath, "rtcRuntimeConfig.xsd", new XElement(Fns + "rtcRuntimeConfig"));
            xDocument.Add(xElement);
            return xDocument;
        }

        private static XDocument GetToolsConfigXDocument(string xsdPath)
        {
            var xDocument = new XDocument();

            var xDeclaration = new XDeclaration("1.0", "UTF-8", "yes");
            xDocument.Declaration = xDeclaration;
            XElement xElement = AddHeader(Fns, true, xsdPath, "rtcToolsConfig.xsd", new XElement(Fns + "rtcToolsConfig",
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
            XElement xElement = AddHeader(Pi, false, xsdPath, "pi_timeseries.xsd", new XElement(Pi + "TimeSeries"));
            xDocument.Add(xElement);
            return xDocument;
        }

        private static XDocument GetStateVectorXDocument(string xsdPath)
        {
            var xDocument = new XDocument();

            var xDeclaration = new XDeclaration("1.0", "UTF-8", "yes");
            xDocument.Declaration = xDeclaration;
            XElement xElement = AddHeader(OpenDa, false, xsdPath, "treeVector.xsd", new XElement(OpenDa + "treeVectorFile"));
            xDocument.Add(xElement);
            return xDocument;
        }

        private static XElement GetTriggersElementFromControlGroups(IEnumerable<ControlGroup> controlGroups)
        {
            var triggersElement = new XElement(Fns + "triggers");
            foreach (ControlGroup group in controlGroups)
            {
                IEnumerable<RtcBaseObject> startObjects = ControlGroupHelper
                                                          .RetrieveTriggerObjects(group)
                                                          .Where(t => t is ConditionBase ||
                                                                      t is MathematicalExpression);
                foreach (RtcBaseObject startObject in startObjects)
                {
                    RtcSerializerBase serializer = SerializerCreator.CreateSerializerType(startObject);
                    triggersElement.Add(serializer.ToXml(Fns, GetGroupNameWithSeparator(group.Name)));
                }
            }

            return triggersElement.HasElements ? triggersElement : null;
        }

        private static string GetGroupNameWithSeparator(string groupName)
        {
            return groupName + "/";
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

            foreach (ControlGroup group in controlGroups)
            {
                string groupNameWithSeparator = GetGroupNameWithSeparator(group.Name);
                foreach (SignalBase signal in group.Signals)
                {
                    if (signal is LookupSignal)
                    {
                        signal.StoreAsRule = true;
                        RtcSerializerBase serializer = SerializerCreator.CreateSerializerType(signal);
                        rulesElementXmlContents.Add(serializer.ToXml(Fns, groupNameWithSeparator).First());
                    }
                }

                foreach (RuleBase rule in group.Rules)
                {
                    // RTCTools needs and extra state variable for controllers that use the previous state
                    // when determining the new state.
                    // If a restart file has to be written, the extra state variable is needed for all controllers,
                    // to be able to store a complete state.
                    if (includeExtraStatesForRestart || rule is PIDRule || rule is IntervalRule || rule is RelativeTimeRule)
                    {
                        var outputSerializer = new OutputSerializer(rule.Outputs.First());
                        string outputXmlName = outputSerializer.GetXmlName();
                        if (!oneMemoryBackupPerOutput.Contains(outputXmlName) && group.Conditions.Any(c => c.TrueOutputs.Contains(rule) || c.FalseOutputs.Contains(rule)))
                        {
                            // RTCTools needs extra rule for this controller
                            rulesElementXmlGlobalContents.Add(GetGlobalRuleForPIDMemoryBackup(outputXmlName, rule.Name));
                            oneMemoryBackupPerOutput.Add(outputXmlName);
                        }
                    }

                    // add support for standard setpint or lookup table
                    string setPointId = RtcXmlTag.SP + groupNameWithSeparator + rule.Name;
                    foreach (SignalBase signal in group.Signals)
                    {
                        if (signal is LookupSignal)
                        {
                            foreach (RuleBase ruleBase in signal.RuleBases)
                            {
                                if (ruleBase.IsLinkedFromSignal() && ruleBase.Name == rule.Name)
                                {
                                    setPointId = RtcXmlTag.Signal + groupNameWithSeparator + signal.Name;
                                }
                            }
                        }
                    }

                    foreach (IInput input in rule.Inputs)
                    {
                        input.SetPoint = setPointId;
                    }

                    RtcSerializerBase serializer = SerializerCreator.CreateSerializerType(rule);
                    rulesElementXmlContents.Add(serializer.ToXml(Fns, groupNameWithSeparator).First());
                }
            }

            rulesElement.Add(rulesElementXmlGlobalContents.Concat(rulesElementXmlContents));

            return rulesElement.HasElements ? rulesElement : null;
        }

        /// <summary>
        /// </summary>
        /// <param name="timeDependentModel"></param>
        /// example
        /// <startDate date="1999-04-15" time="01:00:00"/>
        /// <endDate date="1999-06-15" time="00:00:00"/>
        /// <timeStep unit="minute" multiplier="15"/>
        /// <numberEnsembles>1</numberEnsembles>
        /// xsd
        /// <complexType name="UserDefinedRuntimeConfigComplexType">
        ///     <sequence>
        ///         <element name="startDate" type="rtc:DateTimeComplexType"/>
        ///         <element name="endDate" type="rtc:DateTimeComplexType"/>
        ///         <element name="timeStep" type="rtc:TimeStepComplexType"/>
        ///         <element name="numberEnsembles" type="int" default="1" minOccurs="0"/>
        ///     </sequence>
        /// </complexType>
        /// The time unit element has three attributes, unit and divider and multiplier.
        /// the unit is second, minute, hour, week, month year.
        /// <returns></returns>
        private static XElement GetXmlRuntimeFromModel(ITimeDependentModel timeDependentModel)
        {
            var periodElement = new XElement(Fns + "period");
            var userDefinedElement = new XElement(Fns + "userDefined");
            userDefinedElement.Add(DateTimeToXElement("startDate", timeDependentModel.StartTime));
            userDefinedElement.Add(DateTimeToXElement("endDate", timeDependentModel.StopTime));

            userDefinedElement.Add(GetTimeStepXElement(Fns, timeDependentModel.TimeStep));

            periodElement.Add(userDefinedElement);

            return periodElement;
        }

        private static XElement GetXmlRestartStateFromModel(RealTimeControlModel realTimeControlModel)
        {
            var restartStateFromModel = new XElement(Fns + "stateFiles");
            restartStateFromModel.Add(DateTimeToXElement("startDate", realTimeControlModel.SaveStateStartTime));
            restartStateFromModel.Add(DateTimeToXElement("endDate", realTimeControlModel.SaveStateStopTime));
            restartStateFromModel.Add(GetTimeStepXElement(Fns, realTimeControlModel.SaveStateTimeStep, "stateTimeStep", true));

            return restartStateFromModel;
        }

        private static XElement GetXmlForLimitedMemoryOption(bool limitMemory)
        {
            if (!limitMemory)
            {
                limitMemory = true;
                Log.Warn("Depricated option \"Limited Memory\" of D-RTC model is set to True");
            }

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

        private static XElement DateTimeToXElement(string tag, DateTime dateTime)
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
        ///     <sequence>
        ///         <element name="elementId" type="string"/>
        ///         <element name="quantityId" type="rtc:quantityIdEnumStringType"/>
        ///     </sequence>
        /// </complexType>
        /// <simpleType name="quantityIdEnumStringType">
        ///     <restriction base="string">
        ///         <enumeration value="Water level"/>
        ///         <enumeration value="Crest level"/>
        ///         <enumeration value="Discharge"/>
        ///     </restriction>
        /// </simpleType>
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

            foreach (ControlGroup group in controlGroups)
            {
                string groupNameWithSeparator = GetGroupNameWithSeparator(group.Name);

                foreach (Input input in group.Inputs)
                {
                    if (inputItems.Contains(input.Name))
                    {
                        // avoid duplicates
                        continue;
                    }

                    inputItems.Add(input.Name);
                    var serializer = new InputSerializer(input);
                    var tempElement = new XElement(Fns + "timeSeries", new XAttribute("id", serializer.GetXmlName(string.Empty)));

                    if (input.IsConnected)
                    {
                        string inputLocationNameWithoutHash = input.LocationName.Replace("##", "~~");
                        tempElement.Add(new XElement(Fns + "OpenMIExchangeItem",
                                                     new XElement(Fns + "elementId", inputLocationNameWithoutHash),
                                                     new XElement(Fns + "quantityId", input.ParameterName),
                                                     new XElement(Fns + "unit", "m")
                                        ));
                    }

                    import.Add(tempElement);
                }

                foreach (ConditionBase conditionBase in group.Conditions)
                {
                    var serializer = SerializerCreator.CreateSerializerType<ConditionSerializerBase>(conditionBase);
                    foreach (XElement importTimeSeries in serializer.ToDataConfigImportSeries(groupNameWithSeparator, Fns))
                    {
                        string key = importTimeSeries.Attribute("id").Value;

                        if (serieNames.Contains(key))
                        {
                            continue;
                        }

                        import.Add(importTimeSeries);

                        serieNames.Add(key);
                    }
                }

                foreach (RuleBase ruleBase in group.Rules)
                {
                    // some rule require their output item also as input. The user will not have to make this connection. RTCTools
                    // does require an explicit reference. This allows future implementations to user other exchange itens as input
                    // used by RelativeTimeRule and PIDRule
                    var serializer = SerializerCreator.CreateSerializerType<RuleSerializerBase>(ruleBase);
                    import.Add(serializer.OutputAsInputToDataConfigXml(Fns));

                    if (ruleBase is IntervalRule intervalRule &&
                        intervalRule.IntervalType == IntervalRule.IntervalRuleIntervalType.Signal)
                    {
                        continue;
                    }

                    // add tines series that are part of the rules to the xml
                    foreach (IXmlTimeSeries timeSeries in serializer.XmlImportTimeSeries(groupNameWithSeparator, timeDependentModel.StartTime, timeDependentModel.StopTime, timeDependentModel.TimeStep))
                    {
                        import.Add(timeSeries.GetTimeSeriesXElementForDataConfigFile(Fns, false));
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
                                                   new XElement(Fns + "timeSeriesFile", "timeseries_export.xml"),
                                                   new XElement(Fns + "useBinFile", "false")
                                      )
            );

            // check if item has already been writtem and if yes skip
            var outputItems = new HashSet<string>();
            var seriesNames = new HashSet<string>();

            foreach (ControlGroup group in controlGroups)
            {
                string groupNameWithSeparator = GetGroupNameWithSeparator(group.Name);

                foreach (Output output in group.Outputs)
                {
                    string nameWithoutHashSigns = output.Name.Replace("##", "~~");
                    if (outputItems.Contains(nameWithoutHashSigns))
                    {
                        continue;
                    }

                    outputItems.Add(nameWithoutHashSigns);

                    var serializer = new OutputSerializer(output);
                    var openMi = new XElement(Fns + "timeSeries", new XAttribute("id", serializer.GetXmlName()));
                    if (output.IsConnected)
                    {
                        openMi.Add(new XElement(Fns + "OpenMIExchangeItem",
                                                new XElement(Fns + "elementId", output.LocationName),
                                                new XElement(Fns + "quantityId", output.ParameterName),
                                                new XElement(Fns + "unit", "m")));
                    }

                    export.Add(openMi);
                }

                foreach (ConditionBase conditionBase in group.Conditions)
                {
                    var serializer = SerializerCreator.CreateSerializerType<ConditionSerializerBase>(conditionBase);
                    foreach (XElement exportTimeSeries in serializer.ToDataConfigExportSeries(Fns, groupNameWithSeparator))
                    {
                        string key = exportTimeSeries.Attribute("id").Value;

                        if (seriesNames.Contains(key))
                        {
                            continue;
                        }

                        export.Add(exportTimeSeries);

                        seriesNames.Add(key);
                    }
                }

                foreach (RuleBase ruleBase in group.Rules)
                {
                    RtcSerializerBase serializer = SerializerCreator.CreateSerializerType(ruleBase);
                    foreach (IXmlTimeSeries timeSeries in serializer.XmlExportTimeSeries(groupNameWithSeparator))
                    {
                        export.Add(timeSeries.GetTimeSeriesXElementForDataConfigFile(Fns, true));
                    }
                }

                foreach (SignalBase signal in group.Signals)
                {
                    RtcSerializerBase serializer = SerializerCreator.CreateSerializerType(signal);
                    foreach (IXmlTimeSeries timeSeries in serializer.XmlExportTimeSeries(groupNameWithSeparator))
                    {
                        export.Add(timeSeries.GetTimeSeriesXElementForDataConfigFile(Fns, true));
                    }
                }

                foreach (MathematicalExpression expression in group.MathematicalExpressions)
                {
                    var serializer = new MathematicalExpressionSerializer(expression);
                    foreach (XElement element in serializer.GetDataConfigXmlElements(Fns,groupNameWithSeparator))
                    {
                        export.Add(element);
                    }
                }
            }

            AddHydraulicRulesWithTimeLagAsTimeSerieToDataConfig(export, controlGroups.SelectMany(controlGroup => controlGroup.Rules.OfType<HydraulicRule>().Where(r => r.TimeLagInTimeSteps > 0)));

            // if no timeseries where added add the root node, else the XSD validation breaks
            if (!export.Nodes().Any())
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
            foreach (Output output in controlGroups.SelectMany(controlGroup => controlGroup.Outputs))
            {
                if (names.Contains(output.Name))
                {
                    continue;
                }
                
                names.Add(output.Name);
                var serializer = new OutputSerializer(output);
                states.Add(new XElement(OpenDa + "treeVectorLeaf",
                                        new XAttribute("id", serializer.GetXmlName()),
                                        new XElement(OpenDa + "vector", output.Value)));
            }

            return states;
        }

        /// <summary>
        /// Write timeseries data for the rules that need an external times series to a xml file (pi_timeseries.xsd).
        /// The definition of this time series is written to rtcDataConfig.xml
        /// Time series in rtcDataConfig.xml that are of type PITimeSeries (opposed to OpenMIExchangeItem)
        /// have data in this xml file.
        /// </summary>
        private static void GetXmlTimeSeriesFromControlGroups(XElement root, IEnumerable<ControlGroup> controlGroups, ITimeDependentModel timeDependentModel)
        {
            var seriesNames = new HashSet<string>();
            foreach (ControlGroup group in controlGroups)
            {
                string groupNameWithSeparator = GetGroupNameWithSeparator(group.Name);
                foreach (RuleBase ruleBase in group.Rules)
                {
                    if (ruleBase is PIDRule pidRule
                        && pidRule.PidRuleSetpointType != PIDRule.PIDRuleSetpointTypes.TimeSeries)
                    {
                        if (pidRule.TimeSeries.Time.Values.Count != 0)
                        {
                            Log.WarnFormat(Resources.RealTimeControlXmlWriter_GetXmlTimeSeriesFromControlGroups_PIDRule__0__time_series_will_not_be_included_in_the_DIMR_XML_as_Set_Point_Type_is_not_TimeSeries, pidRule.Name);
                        }

                        continue;
                    }

                    if (ruleBase is IntervalRule intervalRule && intervalRule.IntervalType == IntervalRule.IntervalRuleIntervalType.Signal)
                    {
                        Log.WarnFormat(Resources.RealTimeControlXmlWriter_GetXmlTimeSeriesFromControlGroups_IntervalRule__0__time_series_will_not_be_included_in_the_DIMR_XML_as_Set_Point_Type_is_Signal, intervalRule.Name);
                        continue;
                    }

                    RtcSerializerBase serializer = SerializerCreator.CreateSerializerType(ruleBase);
                    foreach (IXmlTimeSeries timeSeries in serializer.XmlImportTimeSeries(groupNameWithSeparator, timeDependentModel.StartTime, timeDependentModel.StopTime, timeDependentModel.TimeStep))
                    {
                        string key = groupNameWithSeparator + timeSeries.LocationId + "/" + timeSeries.ParameterId;
                        if (seriesNames.Contains(key))
                        {
                            continue;
                        }

                        root.Add(timeSeries.GetTimeSeriesXElementForTimeSeriesFile(Pi, timeDependentModel.TimeStep));

                        seriesNames.Add(key);
                    }
                }

                foreach (ConditionBase conditionBase in group.Conditions)
                {
                    RtcSerializerBase serializer = SerializerCreator.CreateSerializerType(conditionBase);
                    foreach (IXmlTimeSeries timeSeries in serializer.XmlImportTimeSeries(@groupNameWithSeparator, timeDependentModel.StartTime, timeDependentModel.StopTime, timeDependentModel.TimeStep))
                    {
                        string key = groupNameWithSeparator + timeSeries.LocationId + "/" + timeSeries.ParameterId;

                        if (seriesNames.Contains(key))
                        {
                            continue;
                        }

                        root.Add(timeSeries.GetTimeSeriesXElementForTimeSeriesFile(Pi, timeDependentModel.TimeStep));

                        seriesNames.Add(key);
                    }
                }
            }
        }

        /// <summary>
        /// TimeLags are presented by a UnitDelay component in ToolsConfig
        /// </summary>
        /// <param name="xDocument">The <see cref="XDocument"/> to which the UnitDelayComponents are added. </param>
        /// <param name="controlGroups">The <see cref="ControlGroup"/> that are traversed to save the UnitDelayComponents from. </param>
        private static void AddUnitDelayComponents(XDocument xDocument, IList<ControlGroup> controlGroups)
        {
            List<HydraulicRule> timeLagHydraulicRules = controlGroups.SelectMany(controlGroup => controlGroup.Rules.OfType<HydraulicRule>().Where(r => r.TimeLagInTimeSteps > 0)).ToList();
            IEnumerable<string> inputNames = timeLagHydraulicRules.SelectMany(timeLagHydraulicRule => timeLagHydraulicRule.Inputs.OfType<Input>().Select(input =>
            {
                var serializer = new InputSerializer(input);
                return serializer.GetXmlName(string.Empty);
            }).Distinct());

            var xElementComponents = new XElement(Fns + "components");

            var passedNames = new HashSet<string>();
            foreach (string inputName in inputNames)
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
                                                                 new XAttribute("id", RtcXmlTag.Delayed + inputName),
                                                                 new XElement(Fns + "input", new XElement(Fns + "x", inputName)),
                                                                 new XElement(Fns + "output", new XElement(Fns + "yVector", RtcXmlTag.Delayed + inputName))
                                                    )
                                       ));
            }

            IEnumerable<DirectionalCondition> directionalConditions =
                controlGroups.SelectMany(controlGroup => controlGroup.Conditions.OfType<DirectionalCondition>());

            foreach (DirectionalCondition directionalCondition in directionalConditions)
            {
                IInput conditionInput = directionalCondition.Input;
                if (conditionInput is Input input)
                {
                    var inputSerializer = new InputSerializer(input);
                    string inputXmlName = inputSerializer.GetXmlName(string.Empty);
                    if (passedNames.Contains(inputXmlName))             //avoid double elements
                    {
                        continue;
                    }

                    passedNames.Add(inputXmlName);

                    var serializer = new DirectionalConditionSerializer(directionalCondition);
                    xElementComponents.Add(new XElement(Fns + "component",
                                                        new XElement(Fns + "unitDelay",
                                                                     new XAttribute("id", RtcXmlTag.Delayed + inputXmlName),
                                                                     new XElement(Fns + "input", new XElement(Fns + "x", inputXmlName)),
                                                                     new XElement(Fns + "output", new XElement(Fns + "y", serializer.GetLaggedInputName(string.Empty)))
                                                        )
                                           ));
                }
            }

            if (xElementComponents.HasElements)
            {
                XElement xElementGeneral = xDocument.Root.Elements().First();
                xElementGeneral.AddAfterSelf(xElementComponents);
            }
        }

        private static void AddHydraulicRulesWithTimeLagAsTimeSerieToDataConfig(XElement exportSeries, IEnumerable<HydraulicRule> hydraulicRulesWithLimeLag)
        {
            var dictInputAndHydraulicRuleWithBiggestDelay = new Dictionary<string, HydraulicRule>();
            foreach (HydraulicRule hydraulicRule in hydraulicRulesWithLimeLag)
            {
                foreach (Input input in hydraulicRule.Inputs.OfType<Input>())
                {
                    var serializer = new InputSerializer(input);
                    string inputXmlName = serializer.GetXmlName(string.Empty);
                    if (dictInputAndHydraulicRuleWithBiggestDelay.ContainsKey(inputXmlName))
                    {
                        if (hydraulicRule.TimeLagInTimeSteps > dictInputAndHydraulicRuleWithBiggestDelay[inputXmlName].TimeLagInTimeSteps)
                        {
                            dictInputAndHydraulicRuleWithBiggestDelay[inputXmlName] = hydraulicRule;
                        }
                    }
                    else
                    {
                        dictInputAndHydraulicRuleWithBiggestDelay.Add(inputXmlName, hydraulicRule);
                    }
                }
            }

            foreach (HydraulicRule hydraulicRule in dictInputAndHydraulicRuleWithBiggestDelay.Values)
            {
                IInput ruleInput = hydraulicRule.Inputs.First();
                if (ruleInput is Input input)
                {
                    var serializer = new InputSerializer(input);
                    var timeSeriesElement = new XElement(Fns + "timeSeries",
                                                         new XAttribute("id", RtcXmlTag.Delayed + serializer.GetXmlName(string.Empty)),
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
}