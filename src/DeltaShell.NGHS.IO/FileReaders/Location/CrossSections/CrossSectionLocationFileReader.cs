using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using DeltaShell.NGHS.Utils.Extensions;
using DHYDRO.Common.IO.Ini;
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
        /// - Skips INI sections that do not have a "CrossSection" header.
        /// - Logs an error and skips the section when this section does not contain an "id", "branchId",
        ///   "chainage", "shift" or "definitionId" property, or when any of these properties is empty.
        /// - Logs an error and skips the section when this section  contains a "chainage" or "shift"
        ///   property that contain invalid doubles.
        /// </remarks>
        public IEnumerable<CrossSectionLocation> Read(string filePath)
        {
            if (!File.Exists(filePath))
            {
                yield break;
            }

            IEnumerable<IniSection> iniSections = delftIniReader.ReadDelftIniFile(filePath)
                                                                     .Where(IsCrossSectionIniSection);

            foreach (IniSection crossSectionIniSection in iniSections)
            {
                if (!TryReadString(crossSectionIniSection, LocationRegion.Id, out string id) ||
                    !TryReadString(crossSectionIniSection, LocationRegion.Definition, out string definitionId) ||
                    !TryReadDouble(crossSectionIniSection, LocationRegion.Chainage, out double chainage) ||
                    !TryReadString(crossSectionIniSection, LocationRegion.BranchId, out string branchId) ||
                    !TryReadDouble(crossSectionIniSection, LocationRegion.Shift, out double shift))
                {
                    continue;
                }

                var longName = crossSectionIniSection.ReadProperty<string>(LocationRegion.Name, true);

                yield return new CrossSectionLocation(id, longName, branchId, chainage, shift, definitionId);
            }
        }

        private static bool TryReadString(IniSection iniSection, ConfigurationSetting setting, out string value)
        {
            value = null;
            string key = setting.Key;

            IniProperty property = iniSection.GetProperty(key);
            if (property == null)
            {
                log.ErrorFormat(Resources.IniProperty_NotFound, key, iniSection.Name, iniSection.LineNumber);
                return false;
            }

            string propertyValue = property.Value;
            if (string.IsNullOrEmpty(propertyValue))
            {
                log.ErrorFormat(Resources.IniProperty_EmptyValue, key, iniSection.Name, property.LineNumber);
                return false;
            }

            value = propertyValue;
            return true;
        }

        private static bool TryReadDouble(IniSection iniSection, ConfigurationSetting setting, out double value)
        {
            value = double.NaN;

            if (!TryReadString(iniSection, setting, out string strValue))
            {
                return false;
            }

            if (!double.TryParse(strValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleValue))
            {
                int lineNumber = iniSection.GetProperty(setting.Key).LineNumber;
                log.ErrorFormat(Resources.IniProperty_InvalidDouble, setting.Key, iniSection.Name, lineNumber, strValue);
                return false;
            }

            value = doubleValue;
            return true;
        }

        private static bool IsCrossSectionIniSection(IniSection iniSection) => 
            iniSection.Name.EqualsCaseInsensitive(CrossSectionRegion.IniHeader);
    }
}