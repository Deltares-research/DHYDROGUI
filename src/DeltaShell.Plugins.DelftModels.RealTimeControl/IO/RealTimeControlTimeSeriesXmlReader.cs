using System.Collections.Generic;
using System.IO;
using DeltaShell.Dimr.RtcXsd;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO
{
    /// <summary>
    /// Responsible for reading the time series file path and setting the time series on the RTC objects.
    /// </summary>
    public class RealTimeControlTimeSeriesXmlReader
    {
        private readonly ILogHandler logHandler;

        public RealTimeControlTimeSeriesXmlReader(ILogHandler logHandler)
        {
            this.logHandler = logHandler;
        }

        /// <summary>
        /// Reads the specified time series file path.
        /// </summary>
        /// <param name="timeSeriesFilePath">The time series file path.</param>
        /// <param name="controlGroups">The control groups.</param>
        /// <remarks>If parameter controlGroups is NULL or timeSeriesFilePath does not exist, methods returns.</remarks>
        public void Read(string timeSeriesFilePath, IList<IControlGroup> controlGroups)
        {
            if (string.IsNullOrEmpty(timeSeriesFilePath) || !File.Exists(timeSeriesFilePath) || controlGroups == null)
            {
                return;
            }

            var delftConfigXmlParser = new DelftConfigXmlFileParser(logHandler);

            var timeSeriesObject = delftConfigXmlParser.Read<TimeSeriesCollectionComplexType>(timeSeriesFilePath);

            var timeSeriesSetter = new RealTimeControlTimeSeriesSetter(logHandler);
            timeSeriesSetter.SetTimeSeries(timeSeriesObject.series, controlGroups);
        }
    }
}