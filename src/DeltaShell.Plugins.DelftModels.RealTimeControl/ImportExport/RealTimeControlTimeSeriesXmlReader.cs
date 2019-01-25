using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xsd;
using log4net;
using System.Collections.Generic;
using System.IO;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public static class RealTimeControlTimeSeriesXmlReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RealTimeControlTimeSeriesXmlReader));

        public static void Read(string timeSeriesFilePath, IList<IControlGroup> controlGroups )
        {
            if (!File.Exists(timeSeriesFilePath))
            {
                Log.ErrorFormat(Resources.RealTimeControlTimeSeriesXmlReader_Read_File___0___does_not_exist_, timeSeriesFilePath);
                return;
            }

            if (controlGroups == null) return;

            var timeSeriesObject = DelftConfigXmlFileParser.Read<TimeSeriesCollectionComplexType>(timeSeriesFilePath);
            RealTimeControlTimeSeriesSetter.SetTimeSeries(timeSeriesObject?.series, controlGroups);
        }
    }
}
