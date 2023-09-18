using System.Collections.Generic;
using System.Collections.ObjectModel;
using DHYDRO.Common.IO.BackwardCompatibility;

namespace DeltaShell.NGHS.IO.FileReaders.BackwardCompatibility
{
    /// <summary>
    /// <see cref="StructureFileBackwardsCompatibilityConfigurationValues"/> defines the obsolete and legacy INI sections
    /// and properties for the structure file.
    /// </summary>
    /// <seealso cref="IDelftIniBackwardsCompatibilityConfigurationValues"/>
    public class StructureFileBackwardsCompatibilityConfigurationValues : IDelftIniBackwardsCompatibilityConfigurationValues
    {
        private static readonly IList<DelftIniPropertyInfo> unsupportedPropertyValues = new List<DelftIniPropertyInfo>
        {
            new DelftIniPropertyInfo("structure", "type", "extraresistance")
        };

        /// <inheritdoc/>
        public ISet<string> ObsoleteProperties { get; } = new HashSet<string>();

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, string> LegacyPropertyMapping { get; } = 
            new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, string> LegacyCategoryMapping { get; } = 
            new Dictionary<string, string>(new Dictionary<string, string>());

        /// <inheritdoc/>
        public IEnumerable<DelftIniPropertyInfo> UnsupportedPropertyValues { get; } =
            new ReadOnlyCollection<DelftIniPropertyInfo>(unsupportedPropertyValues);
    }
}