using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using log4net;
using System.Collections.Generic;
using System.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public static class RealTimeControlTimeSeriesXmlReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RealTimeControlTimeSeriesXmlReader));

        public static void Read(string timeSeriesFilePath, IList<ControlGroup> controlGroups )
        {
            if (!File.Exists(timeSeriesFilePath))
            {
                Log.ErrorFormat(Resources.RealTimeControlTimeSeriesXmlReader_Read_File___0___does_not_exist_, timeSeriesFilePath);
                return;
            }

            if (controlGroups == null) return;

            var timeSeriesObject = (TimeSeriesCollectionComplexType)DelftConfigXmlFileParser.Read(timeSeriesFilePath);
            RealTimeControlTimeSeriesConnector.ConnectTimeSeries(timeSeriesObject?.series, controlGroups );
        }
    }
}
