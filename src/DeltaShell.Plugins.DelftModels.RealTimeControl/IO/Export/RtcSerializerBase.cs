using System;
using System.Collections.Generic;
using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xml;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export
{
    /// <summary>
    /// Serializer for an <see cref="RtcBaseObject"/>.
    /// </summary>
    public abstract class RtcSerializerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RtcSerializerBase"/> class.
        /// </summary>
        /// <param name="rtcBaseObject">The RTC base object to serialize.</param>
        protected RtcSerializerBase(RtcBaseObject rtcBaseObject)
        {
            RtcObject = rtcBaseObject;
        }

        /// <summary>
        /// Converts the rtc object to a collection of <see cref="XElement"/>
        /// to be written to the tools config xml file.
        /// </summary>
        /// <param name="xNamespace"> The xml namespace. </param>
        /// <param name="prefix"> The prefix. </param>
        /// <returns> The collection of <see cref="XElement"/>. </returns>
        public abstract IEnumerable<XElement> ToXml(XNamespace xNamespace, string prefix);

        /// <summary>
        /// Converts the rtc object to an xml element used as a
        /// reference by other xml elements.
        /// </summary>
        /// <param name="xNamespace"> The xml namespace. </param>
        /// <param name="prefix"> The prefix. </param>
        /// <returns> The reference xml element. </returns>
        public virtual XElement ToXmlReference(XNamespace xNamespace, string prefix)
        {
            return new XElement(xNamespace + "trigger", new XElement(xNamespace + "ruleReference", GetXmlNameWithTag(prefix)));
        }

        /// <summary>
        /// Override this if the object needs to write a collection of <see cref="XElement"/>
        /// to the import series in the data config xml file
        /// and the time series import xml file.
        /// </summary>
        /// <param name="prefix"> The prefix. </param>
        /// <param name="start"> The start time of the model. </param>
        /// <param name="stop"> The stop time of the model. </param>
        /// <param name="step"> The time step of the model. </param>
        /// <returns> The collection of <see cref="XElement"/>. </returns>
        public virtual IEnumerable<IXmlTimeSeries> XmlImportTimeSeries(string prefix, DateTime start, DateTime stop, TimeSpan step)
        {
            yield break;
        }

        /// <summary>
        /// Override this if the rtc object needs to be converted to a
        /// collection of <see cref="XElement"/> to be written to
        /// export series in the data config xml file.
        /// </summary>
        /// <param name="prefix"> The prefix. </param>
        /// <returns> The collection of <see cref="XElement"/>. </returns>
        public virtual IEnumerable<IXmlTimeSeries> XmlExportTimeSeries(string prefix)
        {
            yield break;
        }

        /// <summary>
        /// Gets the XML tag that is used within the id of the object.
        /// </summary>
        protected abstract string XmlTag { get; }

        /// <summary>
        /// Gets the xml name with the tag.
        /// </summary>
        /// <param name="prefix"> The prefix. </param>
        /// <returns> The xml name with the tag. </returns>
        /// <example>
        /// "[TimeRule]ControlGroup1/TimeRule1"
        /// </example>
        protected string GetXmlNameWithTag(string prefix)
        {
            return XmlTag + GetXmlNameWithoutTag(prefix);
        }

        /// <summary>
        /// Gets the xml name without the tag.
        /// </summary>
        /// <param name="prefix"> The prefix. </param>
        /// <returns> The xml name without the tag. </returns>
        /// <example>
        /// "ControlGroup1/TimeRule1"
        /// </example>
        protected string GetXmlNameWithoutTag(string prefix)
        {
            return prefix + RtcObject.Name;
        }

        private RtcBaseObject RtcObject { get; }
    }
}