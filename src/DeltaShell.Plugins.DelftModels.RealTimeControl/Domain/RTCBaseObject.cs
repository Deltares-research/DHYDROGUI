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
        protected RtcBaseObject(string xmlTag)
        {
            XmlTag = xmlTag;
        }

        public string Name { get; set; }
        public string LongName { get; set; }
        protected string XmlTag { get; }

        /// <summary>
        /// todo refactor this; there not 1 xml attached to a RtcBaseObject
        /// For rules:
        /// implement to write xml for rule to rtcToolsConfig.xml file.
        /// </summary>
        /// <param name="xNamespace"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public abstract XElement ToXml(XNamespace xNamespace, string prefix);

        public virtual XElement ToXmlReference(XNamespace xNamespace, string prefix)
        {
            return new XElement(xNamespace + "trigger", new XElement(xNamespace + "ruleReference", prefix + "/" + Name));
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

        public class RtcXmlTag
        {
            public const string DirectionalCondition = "[DirectionalCondition]";
            public const string FactorRule = "[FactorRule]";
            public const string HydraulicRule = "[HydraulicRule]";
            public const string IntervalRule = "[IntervalRule]";
            public const string LookupSignal = "[LookupSignal]";
            public const string PIDRule = "[PIDRule]";
            public const string RelativeTimeRule = "[RelativeTimeRule]";
            public const string StandardCondition = "[StandardCondition]";
            public const string TimeCondition = "[TimeCondition]";
            public const string TimeRule = "[TimeRule]";

            public const string OutputAsInput = "[AsInputFor]";
            public const string Status = "[Status]";

        }
    }
}
