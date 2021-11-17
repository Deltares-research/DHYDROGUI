using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Extensions;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using log4net;

namespace DeltaShell.NGHS.IO.FileReaders.Location.CrossSections
{
    /// <summary>
    /// A cross section location file reader (crsloc.ini files).
    /// </summary>
    public class CrossSectionLocationFileReader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CrossSectionLocationFileReader));
        private readonly DelftIniReader delftIniReader;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrossSectionLocationFileReader"/> class.
        /// </summary>
        /// <param name="delftIniReader"> The delft ini reader. </param>
        public CrossSectionLocationFileReader(DelftIniReader delftIniReader)
        {
            Ensure.NotNull(delftIniReader, nameof(delftIniReader));

            this.delftIniReader = delftIniReader;
        }

        /// <summary>
        /// Reads the collection of <see cref="CrossSectionLocation"/> from the file at the specified <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath"> The cross section location file path. </param>
        /// <returns>
        /// A collection of <see cref="CrossSectionLocation"/>.
        /// </returns>
        /// <remarks>
        /// - When the file at <paramref name="filePath"/> does not exist, an empty collection is returned.
        /// - Skips INI categories that do not have a "CrossSection" header.
        /// - Logs an error and skips the category when this category does not contain an "id", "branchId",
        ///   "chainage", "shift" or "definitionId" property, or when any of these properties is empty.
        /// - Logs an error and skips the category when this category  contains a "chainage" or "shift"
        ///   property that contain invalid doubles.
        /// </remarks>
        public IEnumerable<CrossSectionLocation> Read(string filePath)
        {
            if (!File.Exists(filePath))
            {
                yield break;
            }

            IEnumerable<DelftIniCategory> categories = delftIniReader.ReadDelftIniFile(filePath)
                                                                     .Where(IsCrossSectionCategory);

            foreach (DelftIniCategory crossSectionCategory in categories)
            {
                if (!TryReadString(crossSectionCategory, LocationRegion.Id, out string id) ||
                    !TryReadString(crossSectionCategory, LocationRegion.Definition, out string definitionId) ||
                    !TryReadDouble(crossSectionCategory, LocationRegion.Chainage, out double chainage) ||
                    !TryReadString(crossSectionCategory, LocationRegion.BranchId, out string branchId) ||
                    !TryReadDouble(crossSectionCategory, LocationRegion.Shift, out double shift))
                {
                    continue;
                }

                var longName = crossSectionCategory.ReadProperty<string>(LocationRegion.Name, true);

                yield return new CrossSectionLocation(id, longName, branchId, chainage, shift, definitionId);
            }
        }

        private static bool TryReadString(IDelftIniCategory category, ConfigurationSetting setting, out string value)
        {
            value = null;
            string key = setting.Key;

            IDelftIniProperty property = category.GetProperty(key);
            if (property == null)
            {
                log.ErrorFormat(Resources.IniProperty_NotFound, key, category.Name, category.LineNumber);
                return false;
            }

            string propertyValue = property.Value;
            if (string.IsNullOrEmpty(propertyValue))
            {
                log.ErrorFormat(Resources.IniProperty_EmptyValue, key, category.Name, property.LineNumber);
                return false;
            }

            value = propertyValue;
            return true;
        }

        private static bool TryReadDouble(IDelftIniCategory category, ConfigurationSetting setting, out double value)
        {
            value = double.NaN;

            if (!TryReadString(category, setting, out string strValue))
            {
                return false;
            }

            if (!double.TryParse(strValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleValue))
            {
                int lineNumber = category.GetProperty(setting.Key).LineNumber;
                log.ErrorFormat(Resources.IniProperty_InvalidDouble, setting.Key, category.Name, lineNumber, strValue);
                return false;
            }

            value = doubleValue;
            return true;
        }

        private static bool IsCrossSectionCategory(DelftIniCategory category) => 
            category.Name.EqualsCaseInsensitive(CrossSectionRegion.IniHeader);
    }
}