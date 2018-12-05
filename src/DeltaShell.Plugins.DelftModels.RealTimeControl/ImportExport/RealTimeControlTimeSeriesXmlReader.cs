using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using log4net;
using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public static class RealTimeControlTimeSeriesXmlReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RealTimeControlTimeSeriesXmlReader));

        public static void Read(string timeSeriesFilePath, IList<ControlGroup> controlGroups )
        {
            var timeSeriesObject = (TimeSeriesCollectionComplexType)DelftConfigXmlFileParser.Read(timeSeriesFilePath);
            RealTimeControlTimeSeriesConnector.ConnectTimeSeries(timeSeriesObject.series, controlGroups );
        }
    }
}
