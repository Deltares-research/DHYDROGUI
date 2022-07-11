using System.Collections.Generic;

namespace DHYDRO.Common.IO.BackwardCompatibility
{
    /// <summary>
    /// <see cref="IDelftIniBackwardsCompatibilityConfigurationValues"/> defines the legacy
    /// mappings and obsolete categories and properties.
    /// </summary>
    public interface IDelftIniBackwardsCompatibilityConfigurationValues
    {
        /// <summary>
        /// Gets the obsolete properties.
        /// </summary>
        /// <remarks>
        /// Note that all properties are assumed to be case-insensitive, as
        /// such it is required for all properties in <see cref="ObsoleteProperties"/>
        /// to be written in lowercase, i.e. the following invariant should hold:
        /// FORALL p IN <see cref="ObsoleteProperties"/>: p == p.ToLower()
        /// </remarks>
        ISet<string> ObsoleteProperties { get; }

        /// <summary>
        /// Gets the mapping of legacy property names to their up to date
        /// equivalents.
        /// </summary>
        /// <remarks>
        /// Note that all properties are assumed to be case-insensitive, as
        /// such it is required for all keys (and only the keys) in
        /// <see cref="LegacyPropertyMapping"/> to be written in lowercase,
        /// i.e. the following invariant should hold:
        /// FORALL p IN LegacyPropertyMapping.Keys: p == p.ToLower()
        /// </remarks>
        IReadOnlyDictionary<string, string> LegacyPropertyMapping { get; }

        /// <summary>
        /// Gets the mapping of legacy category names to their up to date
        /// equivalents.
        /// </summary>
        /// <remarks>
        /// Note that all categories are assumed to be case-insensitive, as
        /// such it is required for all keys (and only the keys) in
        /// <see cref="LegacyCategoryMapping"/> to be written in lowercase,
        /// i.e. the following invariant should hold:
        /// FORALL p IN LegacyCategoryMapping.Keys: p == p.ToLower()
        /// </remarks>
        IReadOnlyDictionary<string, string> LegacyCategoryMapping { get; }

        /// <summary>
        /// Gets the delft INI property infos for unsupported property values.
        /// </summary>
        IEnumerable<DelftIniPropertyInfo> UnsupportedPropertyValues { get; }
    }
}