using System.Collections.Generic;
using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export
{
    /// <summary>
    /// Serializer for a <see cref="DirectionalCondition"/>.
    /// </summary>
    /// <seealso cref="StandardConditionSerializer"/>
    public class DirectionalConditionSerializer : StandardConditionSerializer
    {
        private const string timeLagPostFix = "-1";
        private readonly DirectionalCondition directionalCondition;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectionalConditionSerializer"/> class.
        /// </summary>
        /// <param name="directionalCondition"> The directional condition to serialize. </param>
        public DirectionalConditionSerializer(DirectionalCondition directionalCondition) : base(directionalCondition)
        {
            this.directionalCondition = directionalCondition;
        }

        /// <summary>
        /// Gets the xml name of the lagged input.
        /// </summary>
        /// <returns> The xml name of the lagged input </returns>
        public string GetLaggedInputName()
        {
            return GetInputName() + timeLagPostFix;
        }

        /// <summary>
        /// Converts the directional connection to a collection of <see cref="XElement"/>
        /// to be written to the export series in the data config xml file.
        /// </summary>
        /// <param name="xNamespace"> The xml namespace. </param>
        /// <param name="prefix"> The prefix. </param>
        /// <returns> The collection of <see cref="XElement"/>. </returns>
        public override IEnumerable<XElement> ToDataConfigExportSeries(XNamespace xNamespace, string prefix)
        {
            foreach (XElement export in base.ToDataConfigExportSeries(xNamespace, prefix))
            {
                yield return export;
            }

            var timeSeriesElement = new XElement(xNamespace + "timeSeries", new XAttribute("id", GetLaggedInputName()));
            yield return timeSeriesElement;
        }

        protected override string XmlTag { get; } = RtcXmlTag.DirectionalCondition;

        /// <summary>
        /// Gets the x2 element for the condition element in the tools config xml file.
        /// </summary>
        /// <param name="xNamespace"> The xml namespace. </param>
        /// <returns> The x2 element. </returns>
        protected override XElement GetX2Element(XNamespace xNamespace)
        {
            return new XElement(xNamespace + "x2Series",
                                directionalCondition.Reference == string.Empty
                                    ? null
                                    : new XAttribute("ref", directionalCondition.Reference),
                                GetLaggedInputName());
        }
    }
}