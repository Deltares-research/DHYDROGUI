using System;
using System.Collections.Generic;
using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xml;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.Export
{
    public abstract class RtcSerializerBase
    {
        private RtcBaseObject RtcObject { get; }

        protected RtcSerializerBase(RtcBaseObject rtcBaseObject)
        {
            RtcObject = rtcBaseObject;
        }

        protected abstract string XmlTag { get; }

        /// <summary>
        /// Converts the information of the RtcBaseObject needed for writing the tools config file to an xml element.
        /// </summary>
        /// <param name="xNamespace">The x namespace.</param>
        /// <param name="prefix">The control group name.</param>
        /// <returns>The Xml Element.</returns>
        public abstract IEnumerable<XElement> ToXml(XNamespace xNamespace, string prefix);

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

        protected string GetXmlNameWithTag(string prefix)
        {
            return XmlTag + GetXmlNameWithoutTag(prefix);
        }

        /// <summary>
        /// Gets the XML name without the XML tag.
        /// Example: "ControlGroup1/TimeRule1"
        /// </summary>
        /// <param name="prefix"> The prefix </param>
        /// <returns> The XML name </returns>
        public string GetXmlNameWithoutTag(string prefix)
        {
            return prefix + RtcObject.Name;
        }
    }
}