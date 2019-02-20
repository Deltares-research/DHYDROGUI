using System;
using System.Collections.Generic;
using System.Xml.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xml;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    [Entity(FireOnCollectionChange=false)]
    public abstract class RtcBaseObject : Unique<long>, INameable, ICloneable, ICopyFrom
    {
        public string Name { get; set; }
        public string LongName { get; set; }
        protected string XmlTag { get; set; }

        /// <summary>
        /// Converts the information of the RtcBaseObject needed for writing the tools config file to an xml element.
        /// </summary>
        /// <param name="xNamespace">The x namespace.</param>
        /// <param name="prefix">The control group name.</param>
        /// <returns>The Xml Element.</returns>
        public abstract XElement ToXml(XNamespace xNamespace, string prefix);

        public virtual XElement ToXmlReference(XNamespace xNamespace, string prefix)
        {
            return new XElement(xNamespace + "trigger", new XElement(xNamespace + "ruleReference", GetXmlNameWithTag(prefix)));
        }

        /// <summary>
        /// Override this if the object needs to write a time series to the importSeries
        /// node in the rtcDataConfig.xml file.
        /// The content of the time series added to the timeseries_import.xml file.
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public virtual IEnumerable<IXmlTimeSeries> XmlImportTimeSeries(string prefix, DateTime start, DateTime stop, TimeSpan step)
        {
            yield break;
        }

        /// <summary>
        /// Override this if the rule needs to write a time series to the exportSeries
        /// node in the rtcDataConfig.xml file.
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public virtual IEnumerable<IXmlTimeSeries> XmlExportTimeSeries(string prefix)
        {
            yield break;
        }

        public virtual object Clone()
        {
            var rtcBaseObject = (RtcBaseObject)Activator.CreateInstance(GetType());
            rtcBaseObject.CopyFrom(this);
            return rtcBaseObject;
        }

        public virtual void CopyFrom(object source)
        {
            var rtcBaseObject = source as RtcBaseObject;
            if (rtcBaseObject != null)
            {
                Name = rtcBaseObject.Name;
                LongName = rtcBaseObject.LongName;
            }
        }

        /// <summary>
        /// Gets the XML name with the XML tag.
        /// Example: "[TimeRule]ControlGroup1/TimeRule1"
        /// </summary>
        /// <param name="prefix">The prefix</param>
        /// <returns>The XML name</returns>
        public string GetXmlNameWithTag(string prefix)
        {
            return XmlTag + GetXmlNameWithoutTag(prefix);
        }

        /// <summary>
        /// Gets the XML name without the XML tag.
        /// Example: "ControlGroup1/TimeRule1"
        /// </summary>
        /// <param name="prefix">The prefix</param>
        /// <returns>The XML name</returns>
        public string GetXmlNameWithoutTag(string prefix)
        {
            return prefix + Name;
        }
    }


    /// <summary>
    /// RtcXmlTags used for writing and reading xml files.
    /// </summary>
    public class RtcXmlTag
    {
        public const string DirectionalCondition = "[DirectionalCondition]";
        public const string FactorRule = "[FactorRule]";
        public const string HydraulicRule = "[HydraulicRule]";
        public const string IntervalRule = "[IntervalRule]";
        public const string LookupSignal = "[LookupSignal]";
        public const string PIDRule = "[PID]";
        public const string RelativeTimeRule = "[RelativeTimeRule]";
        public const string StandardCondition = "[StandardCondition]";
        public const string TimeCondition = "[TimeCondition]";
        public const string TimeRule = "[TimeRule]";

        public const string Input = "[Input]";
        public const string Output = "[Output]";

        public const string OutputAsInput = "[AsInputFor]";

        public const string Status = "[Status]";

        public const string SP = "[SP]"; // set point
        public const string IP = "[IP]"; // integral part
        public const string DP = "[DP]"; // differential part
        public const string Delayed = "[Delayed]";
        public const string Signal = "[Signal]";

        public static IList<string> ComponentTags = new List<string>()
        {
            DirectionalCondition,
            FactorRule,
            HydraulicRule,
            IntervalRule,
            LookupSignal,
            PIDRule,
            RelativeTimeRule,
            StandardCondition,
            TimeCondition,
            TimeRule
        };

        public static IList<string> ConnectionPointTags = new List<string>
        {
            Input,
            Output
        };
    }
}
