using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRBuiFileReader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRRBuiFileReader));
        private static readonly Regex regex = new Regex(@"\s+", RegexOptions.Compiled);

        public bool ReadBuiHeaderData(string filePath)
        {
            var buiFile = new ResultTextFile { Path = filePath };
            if (!File.Exists(buiFile.Path))
            {
                log.ErrorFormat("Could not find file {0}.", buiFile.Path);
                return false;
            }

            try
            {
                buiFile.Open(buiFile.Path);
                if (buiFile.IsEmpty() || !ReadBuiHeader(buiFile))
                {
                    log.ErrorFormat("Failed reading header from BUI file: {0}.", buiFile.Path);
                    return false;
                }
            }
            finally
            {
                buiFile.Close();
            }
            return true;
        }

        public IEnumerable<MeteoStationsMeasurement> ReadMeasurementData(string path)
        {
            var buiFile = new ResultTextFile {Path = path};
            if (!File.Exists(buiFile.Path))
            {
                log.ErrorFormat("Could not find file {0}.", buiFile.Path);
                yield break;
            }

            try
            {
                buiFile.Open(buiFile.Path);

                if (!ReadBuiHeader(buiFile)) yield break;

                if (!MeasurementTimes.Any())
                {
                    yield break;
                }

                var currentTime = MeasurementTimes.GetEnumerator();
                string rl;
                while ((rl = buiFile.ParseLine(true)) != null && currentTime.MoveNext())
                {   
                    var timeSlice = regex.Split(rl.Trim());
                    var stationValues = new List<double>(NumberOfStations);
                    foreach (var stationValue in timeSlice)
                    {
                        double value;
                        if (! Double.TryParse(stationValue, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                        {
                            log.ErrorFormat("Failed to parse string on line number {0} as a number.", buiFile.CurrentLineIndex);
                            value = double.NaN;
                        }

                        stationValues.Add(value);
                    }

                    if (stationValues.Count != NumberOfStations)
                    {
                        log.ErrorFormat("Error parsing meteo data from line number {0}",  buiFile.CurrentLineIndex);
                        continue;
                    }

                    yield return
                        new MeteoStationsMeasurement {TimeOfMeasurement = currentTime.Current, MeasuredValues = stationValues};                   
                   
                }
            }
            finally
            {
                buiFile.Close();
            }
        }

        private bool ReadBuiHeader(ResultTextFile buiFile)
        {
            UseDefaultDataSet = (int.Parse(buiFile.ParseLine(true)) == 1);
            NumberOfStations = int.Parse(buiFile.ParseLine(true));

            StationNames = new List<string>(NumberOfStations);
            for (int i = 0; i < NumberOfStations; i++)
            {
                StationNames.Add(buiFile.ParseLine(true).Replace("\'", ""));
            }

            // *Aantal gebeurtenissen (omdat het 1 bui betreft is dit altijd 1)
            // *en het aantal seconden per waarnemingstijdstap
            // 1  3600
            // 3600 = 60 * 60 = 1 hour
            MeasurementPeriodSeconds = int.Parse(buiFile.ParseLine(true).Split(new char[] {' '},
                                                                               StringSplitOptions.RemoveEmptyEntries)[1]);

            // *Het format is: yyyymmdd:hhmmss:ddhhmmss
            //  yyyymmdd:hhmmss:ddhhmmss
            // PB: this comment is misleading it should be read as yyyy mm dd hh mm ss dd hh mm ss
            //     where leading zero'line are omitted!
            //  1951 1 1 0 0 0 1 16 0 0
            string[] metaTimeData = buiFile.ParseLine(true).Split(new char[] {' '},
                                                                  StringSplitOptions.RemoveEmptyEntries);
            if (metaTimeData.Length != 10)
            {
                log.Error("Error parsing start time and/or timestep.");
                return false;
            }

            StartTime = new DateTime(int.Parse(metaTimeData[0]),
                                     int.Parse(metaTimeData[1]),
                                     int.Parse(metaTimeData[2]),
                                     int.Parse(metaTimeData[3]),
                                     int.Parse(metaTimeData[4]),
                                     int.Parse(metaTimeData[5]));

            StopTime = StartTime + new TimeSpan(int.Parse(metaTimeData[6]),
                                                int.Parse(metaTimeData[7]),
                                                int.Parse(metaTimeData[8]),
                                                int.Parse(metaTimeData[9]));

            return true;
        }

        public DateTime StartTime { get; private set; }
        public DateTime StopTime { get; private set; }
        private int MeasurementPeriodSeconds { get; set; } // in seconds
        public IEnumerable<DateTime> MeasurementTimes
        {
            get
            {
                if (!(MeasurementPeriodSeconds > 0))
                {
                    yield break;
                }
                var time = StartTime;
                var period = new TimeSpan(0, 0, MeasurementPeriodSeconds);
                while (time <= StopTime)
                {
                    yield return time;
                    time += period;
                }
            }
        }

        private int NumberOfStations { get; set; }
        private bool UseDefaultDataSet { get; set; }
        public List<string> StationNames { get; private set; }

    }
}