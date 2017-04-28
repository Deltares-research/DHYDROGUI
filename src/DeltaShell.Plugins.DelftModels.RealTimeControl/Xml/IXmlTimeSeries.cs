using System;
using System.Xml.Linq;
using DelftTools.Functions;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Xml
{
    public interface IXmlTimeSeries
    {
        string Name { get; set; }
        string LocationId { get; set; }
        string ParameterId { get; set; }
        DateTime StartTime { get; set; }
        DateTime EndTime { get; set; }
        TimeSpan TimeStep{get;set;}
        TimeSeries TimeSeries { get; set; }
        XElement ToDataConfigXml(XNamespace xNamespace, bool headerOnly);
        XElement ToTimeSeriesXml(XNamespace xNamespace, TimeSpan timeStep);
    }
}
