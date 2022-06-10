using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters
{
    internal class MeteoDataExporter : IFileExporter
    {
        private const string HeaderFormatPrecipitation =
            "* Use the default data set for other input (always 1) \n" +
            "1";

        private const string HeaderFormatPrecipitation2 =
            "* Number of events and the period in seconds \n" +
            "1 {0} \n" +
            "* The first record contains the start date and time (yyyy mm dd HH mm ss), " +
            "* followed by the length of the event (dd HH mm ss). \n" +
            "* The last part is the data for each time step.";

        private const string HeaderEvaporation =
            "* Evaporation file \n" +
            "* Format: \n" +
            "* year month day evaporation_intensity (mm/day)\n";

        private static readonly ILog log = LogManager.GetLogger(typeof(MeteoDataExporter));

        public string Name => "Meteo data exporter";

        public string Category => "";

        public string Description => Name;

        public string FileFilter => "SOBEK BUI/Evaporation/TMP files (*.BUI;*.EVP;*.GEM;*.PLV;*.TMP)|*.bui;*.evp;*.gem;*.plv;*.tmp";

        public Bitmap Icon { get; private set; }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof(MeteoData);
        }

        public bool CanExportFor(object item)
        {
            return true;
        }

        public bool Export(object item, string path)
        {
            var meteoData = item as MeteoData;
            switch (meteoData.Name)
            {
                case RainfallRunoffModelDataSet.PrecipitationName:
                case RainfallRunoffModelDataSet.TemperatureName:
                    return ExportPrecipitationTemperature(meteoData, path);
                case RainfallRunoffModelDataSet.EvaporationName:
                    return ExportEvaporation(meteoData, path);
                default:
                    throw new ArgumentException($"Error during export: can not identify type of meteo data {meteoData.Name}.");

            }
        }

        private static bool ExportPrecipitationTemperature(MeteoData meteoData, string path)
        {
            IFunction meteoDataDistributed = meteoData.MeteoDataDistributed.Data;
            if (meteoDataDistributed == null)
            {
                throw new ArgumentException($"{meteoData.Name}: Meteo data appears to be corrupt. Export has been aborted.");
            }

            using (var sw = new StreamWriter(path))
            {
                List<string> meteoStationNames = GetMeteoStationNames(meteoData, meteoDataDistributed);

                IList<DateTime> timeValues = meteoDataDistributed.Arguments[0].Values.Cast<DateTime>().ToList();
                if (!timeValues.Any())
                {
                    return true;
                }

                if (timeValues.Count == 1)
                {
                    log.Error($"{meteoData.Name}: cannot determine period, because only one value has been defined. Export has been aborted.");
                    return false;
                }

                sw.WriteLine("* Created: " + DateTime.Now);
                DateTime first = timeValues[0];
                TimeSpan timeStep = timeValues[1] - first;
                sw.WriteLine(HeaderFormatPrecipitation);

                // Write the meteo stations. 
                sw.WriteLine("*Aantal stations");
                sw.WriteLine(meteoStationNames.Count);
                sw.WriteLine("*Namen van stations");
                foreach (string meteoStationName in meteoStationNames)
                {
                    sw.WriteLine("'" + meteoStationName + "'");
                }

                sw.WriteLine(HeaderFormatPrecipitation2, Convert.ToInt32(timeStep.TotalSeconds));

                TimeSpan span = timeValues.Last() - first;
                var firstFormatted = first.ToString("yyyy MM dd HH mm ss");
                var spanFormatted = span.ToString(@"dd\ hh\ mm\ ss");
                sw.WriteLine(firstFormatted + " " + spanFormatted);

                var meteoValues = meteoDataDistributed.Components[0].Values as IMultiDimensionalArray<double>;
                var sb = new StringBuilder();
                for (var iTime = 0; iTime < meteoValues.Shape[0]; iTime++)
                {
                    if (meteoValues.Shape.Count() == 1)
                    {
                        // Global meteo definition
                        sb.AppendFormat(CultureInfo.InvariantCulture, "{0:0.00} ", meteoValues[iTime]);
                    }
                    else
                    {
                        // Per station or per catchment
                        for (var iStation = 0; iStation < meteoValues.Shape[1]; iStation++)
                        {
                            sb.AppendFormat(CultureInfo.InvariantCulture, "{0:0.000} ", meteoValues[iTime, iStation]);
                        }
                    }

                    sb.Append("\n");
                }

                sw.WriteLine(sb.ToString());
            }

            return true;
        }

        private static List<string> GetMeteoStationNames(MeteoData meteoData, IFunction meteoDataDistributed)
        {
            return meteoData.DataDistributionType == MeteoDataDistributionType.Global
                       ? new List<string> { MeteoData.GlobalMeteoName }
                       : meteoDataDistributed.Arguments[1].Values.Cast<object>().Select(v => v.ToString()).ToList();
        }

        private static bool ExportEvaporation(MeteoData meteoData, string path)
        {
            IFunction meteoDataDistributed = meteoData.MeteoDataDistributed.Data;
            if (meteoDataDistributed == null)
            {
                throw new ArgumentException("Meteo data appears to be corrupt. Export of meteo data has been aborted.");
            }

            DateTime[] times = meteoDataDistributed.Arguments[0].Values.Cast<DateTime>().ToArray();
            var evaporationValues = meteoDataDistributed.Components[0].Values as IMultiDimensionalArray<double>;

            using (var sw = new StreamWriter(path))
            {
                var sb = new StringBuilder();

                sb.Append("* Created: " + DateTime.Now.ToString());
                sb.Append(HeaderEvaporation);
                for (var iTime = 0; iTime < evaporationValues.Shape[0]; iTime++)
                {
                    sb.Append(times[iTime].ToString("yyyy MM dd"));
                    if (evaporationValues.Shape.Count() == 1)
                    {
                        // Global evaporation definition
                        sb.AppendFormat(CultureInfo.InvariantCulture, " {0:0.000}", evaporationValues[iTime]);
                    }
                    else
                    {
                        // Per station or per catchment
                        for (var iStation = 0; iStation < evaporationValues.Shape[1]; iStation++)
                        {
                            sb.AppendFormat(CultureInfo.InvariantCulture, " {0:0.000}", evaporationValues[iTime, iStation]);
                        }
                    }

                    sb.Append("\n");
                }

                if (evaporationValues.Shape[0] > 0)
                {
                    sw.Write(sb.ToString());
                }
            }

            return true;
        }
    }
}