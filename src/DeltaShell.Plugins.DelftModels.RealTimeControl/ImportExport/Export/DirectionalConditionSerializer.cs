using System.Collections.Generic;
using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.Export
{
    public class DirectionalConditionSerializer : StandardConditionSerializer
    {
        private readonly DirectionalCondition directionalCondition;
        private const string timeLagPostFix = "-1";

        public DirectionalConditionSerializer(DirectionalCondition directionalCondition) : base(directionalCondition)
        {
            this.directionalCondition = directionalCondition;
        }

        protected override string XmlTag { get; } = RtcXmlTag.DirectionalCondition;

        public override IEnumerable<XElement> ToDataConfigExportSeries(XNamespace xNamespace, string prefix)
        {
            foreach (XElement export in base.ToDataConfigExportSeries(xNamespace, prefix))
            {
                yield return export;
            }

            var timeSeriesElement = new XElement(xNamespace + "timeSeries", new XAttribute("id", GetLaggedInputName()));
            yield return timeSeriesElement;
        }

        protected override XElement GetX2Element(XNamespace xNamespace, string inputName)
        {
            return new XElement(xNamespace + "x2Series",
                                directionalCondition.Reference == string.Empty
                                    ? null
                                    : new XAttribute("ref", directionalCondition.Reference),
                                GetLaggedInputName());
        }

        public string GetLaggedInputName()
        {
            return GetInputName() + timeLagPostFix;
        }
    }
}