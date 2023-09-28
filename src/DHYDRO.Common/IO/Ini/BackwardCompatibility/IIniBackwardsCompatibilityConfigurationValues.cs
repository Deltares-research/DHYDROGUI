using System.Collections.Generic;

namespace DHYDRO.Common.IO.Ini.BackwardCompatibility
{
    /// <summary>
    /// <see cref="IIniBackwardsCompatibilityConfigurationValues"/> defines the legacy
    /// mappings and obsolete sections and properties.
    /// </summary>
    public interface IIniBackwardsCompatibilityConfigurationValues
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
        /// Gets the mapping of legacy section names to their up to date
        /// equivalents.
        /// </summary>
        /// <remarks>
        /// Note that all sections are assumed to be case-insensitive, as
        /// such it is required for all keys (and only the keys) in
        /// <see cref="LegacySectionMapping"/> to be written in lowercase,
        /// i.e. the following invariant should hold:
        /// FORALL p IN LegacySectionMapping.Keys: p == p.ToLower()
        /// </remarks>
        IReadOnlyDictionary<string, string> LegacySectionMapping { get; }

        /// <summary>
        /// Gets the INI property infos for unsupported property values.
        /// </summary>
        IEnumerable<IniPropertyInfo> UnsupportedPropertyValues { get; }
    }
}