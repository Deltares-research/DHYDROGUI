using System.Collections.Generic;
using System.Collections.ObjectModel;
using DHYDRO.Common.IO.Ini.BackwardCompatibility;

namespace DeltaShell.NGHS.IO.FileReaders.BackwardCompatibility
{
    /// <summary>
    /// <see cref="StructureFileBackwardsCompatibilityConfigurationValues"/> defines the obsolete and legacy INI sections
    /// and properties for the structure file.
    /// </summary>
    /// <seealso cref="IIniBackwardsCompatibilityConfigurationValues"/>
    public class StructureFileBackwardsCompatibilityConfigurationValues : IIniBackwardsCompatibilityConfigurationValues
    {
        private static readonly IList<IniPropertyInfo> unsupportedPropertyValues = new List<IniPropertyInfo>
        {
            new IniPropertyInfo("structure", "type", "extraresistance")
        };

        /// <inheritdoc/>
        public ISet<string> ObsoleteProperties { get; } = new HashSet<string>();

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, string> LegacyPropertyMapping { get; } = 
            new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, string> LegacySectionMapping { get; } = 
            new Dictionary<string, string>(new Dictionary<string, string>());

        /// <inheritdoc/>
        public IEnumerable<IniPropertyInfo> UnsupportedPropertyValues { get; } =
            new ReadOnlyCollection<IniPropertyInfo>(unsupportedPropertyValues);
    }
}