using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xml;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.Export
{
    public class LookupSignalSerializer : SignalSerializerBase
    {
        private readonly LookupSignal lookupSignal;

        public LookupSignalSerializer(LookupSignal lookupSignal) : base(lookupSignal)
        {
            this.lookupSignal = lookupSignal;
        }

        protected override string XmlTag { get; } = RtcXmlTag.LookupSignal;

        public override IEnumerable<XElement> ToXml(XNamespace xNamespace, string prefix)
        {
            XElement result = base.ToXml(xNamespace, prefix).First();
            IEventedList<Record> table = new EventedList<Record>();
            foreach (object x in lookupSignal.Function.Arguments[0].Values)
            {
                table.Add(new Record
                {
                    X = (double) x,
                    Y = (double) lookupSignal.Function[x]
                });
            }

            List<XElement> xElementsInput =
                lookupSignal.Inputs.Select(input =>
                {
                    var serializer = new InputSerializer(input);
                    return serializer.ToXmlInputReference(xNamespace, "x");
                }).ToList();
            foreach (XElement xElementInput in xElementsInput)
            {
                XElement xElement = xElementInput.Elements().First();
                xElement.Add(new XAttribute("ref", "IMPLICIT"));
            }

            result.Add(new XElement(xNamespace + "lookupTable", new XAttribute("id", GetXmlNameWithTag(prefix)),
                                    new XElement(xNamespace + "table",
                                                 table.Select(record => record.ToXml(xNamespace))),
                                    new XElement(xNamespace + "interpolationOption",
                                                 lookupSignal.Interpolation == InterpolationType.Constant
                                                     ? "BLOCK"
                                                     : "LINEAR"),
                                    new XElement(xNamespace + "extrapolationOption",
                                                 lookupSignal.Extrapolation == ExtrapolationType.Constant
                                                     ? "BLOCK"
                                                     : "LINEAR"),
                                    xElementsInput,
                                    new XElement(xNamespace + "output",
                                                 new XElement(xNamespace + "y",
                                                              RtcXmlTag.Signal + GetXmlNameWithoutTag(prefix)))));
            yield return result;
        }

        public override IEnumerable<IXmlTimeSeries> XmlExportTimeSeries(string prefix)
        {
            yield return GetExportTimeSeries(RtcXmlTag.Signal + GetXmlNameWithoutTag(prefix));
        }

        /// <summary>
        /// Returns a IXmlTimeSeries that is written to rtcDataConfig.xml and only used internally by RTCTools.
        /// Only Name is required for this series.
        /// </summary>
        /// <param name="name"> </param>
        /// <returns> </returns>
        private static IXmlTimeSeries GetExportTimeSeries(string name)
        {
            return new XmlTimeSeries {Name = name};
        }
    }
}