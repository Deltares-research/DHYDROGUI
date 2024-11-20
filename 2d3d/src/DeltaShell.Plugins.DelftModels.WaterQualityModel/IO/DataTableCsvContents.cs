using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    public class DataTableCsvContents
    {
        public DataTableCsvContents()
        {
            Name = string.Empty;
            Interpolation = DataTableInterpolationType.Linear;
            DataRows = new List<LocationData>();
        }

        /// <summary>
        /// Name of the data table.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Type of interpolation applied on the data.
        /// </summary>
        public DataTableInterpolationType Interpolation { get; set; }

        /// <summary>
        /// The location specified time-dependent substance data.
        /// </summary>
        public IList<LocationData> DataRows { get; private set; }

        /// <summary>
        /// Gets or sets the include folder path where .usefors files are stored.
        /// </summary>
        public string UseforIncludeFolderPath { get; set; }

        /// <summary>
        /// Gets the name of the substance usefor file used as include for <see cref="CreateDataTableDelwaqFormat"/>.
        /// </summary>
        public string GetSubstanceUseforFileName()
        {
            return string.Format("{0}.usefors", Name);
        }

        /// <summary>
        /// Creates the default substance usefor file-contents used as include for <see cref="CreateDataTableDelwaqFormat"/>.
        /// </summary>
        public string CreateDefaultSubstanceUseforContents()
        {
            return string.Join(Environment.NewLine, DataRows.SelectMany(GetSubstancesForLocation)
                                                            .Distinct()
                                                            .Select(s => string.Format("USEFOR '{0}' '{0}'", s)));
        }

        /// <summary>
        /// Creates the data table contents represented in delwaq format.
        /// </summary>
        public string CreateDataTableDelwaqFormat()
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                foreach (LocationData locationData in DataRows)
                {
                    writer.WriteLine("DATA_ITEM");
                    writer.WriteLine("'{0}'", locationData.Name);
                    writer.WriteLine("CONCENTRATIONS");
                    writer.WriteLine("INCLUDE '{0}'",
                                     Path.Combine(UseforIncludeFolderPath, GetSubstanceUseforFileName()));
                    writer.WriteLine("TIME {0} DATA", Interpolation.ToString().ToUpper());
                    string[] substancesForLocation = GetSubstancesForLocation(locationData);
                    writer.WriteLine(string.Join(" ", substancesForLocation.Select(s => string.Format("'{0}'", s))));
                    foreach (KeyValuePair<DateTime, IDictionary<string, string>> substanceData in locationData
                        .TimeDependentSubstanceData)
                    {
                        KeyValuePair<DateTime, IDictionary<string, string>> data = substanceData;
                        IEnumerable<string> substanceDataValues = substancesForLocation.Select(substance =>
                                                                                                   data.Value.ContainsKey(substance)
                                                                                                       ? data.Value[substance].ToString(CultureInfo.InvariantCulture)
                                                                                                       : "-999");
                        writer.WriteLine("{0} {1}",
                                         substanceData.Key.ToString("yyyy/MM/dd-HH:mm:ss",
                                                                    CultureInfo.InvariantCulture),
                                         string.Join(" ", substanceDataValues));
                    }

                    writer.WriteLine();
                }

                return writer.ToString();
            }
        }

        private string[] GetSubstancesForLocation(LocationData locationData)
        {
            return locationData.TimeDependentSubstanceData.SelectMany(kvp => kvp.Value)
                               .Select(substanceData => substanceData.Key)
                               .Distinct()
                               .ToArray();
        }
    }

    /// <summary>
    /// Represents data read
    /// </summary>
    public class LocationData
    {
        private readonly IDictionary<DateTime, IDictionary<string, string>> data =
            new SortedDictionary<DateTime, IDictionary<string, string>>();

        public string Name { get; set; }

        /// <summary>
        /// Gets the sparse time dependent substance data.
        /// </summary>
        public IDictionary<DateTime, IDictionary<string, string>> TimeDependentSubstanceData => data;
    }
}