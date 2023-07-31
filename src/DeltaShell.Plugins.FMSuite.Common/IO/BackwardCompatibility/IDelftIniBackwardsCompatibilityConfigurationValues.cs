using System.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility
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
        /// Gets the properties that are obsolete only when other properties are present as well.
        /// </summary>
        /// <remarks>
        /// Note that all properties are assumed to be case-insensitive, as
        /// such it is required for all properties in <see cref="ConditionalObsoleteProperties"/>
        /// to be written in lowercase, i.e. the following invariant should hold:
        /// FORALL p IN <see cref="ConditionalObsoleteProperties"/>: p == p.ToLower()
        ///
        /// The keys in the mapping are considered obsolete, only if the properties, represented
        /// by the values in the mapping, are also present.
        /// </remarks>
        IReadOnlyDictionary<string, string> ConditionalObsoleteProperties { get; }

        /// <summary>
        /// Gets the mapping of legacy property names to the data required to
        /// update them to their up to date equivalents.
        /// </summary>
        /// <remarks>
        /// Note that all properties are assumed to be case-insensitive, as
        /// such it is required for all keys (and only the keys) in
        /// <see cref="LegacyPropertyMapping"/> to be written in lowercase,
        /// i.e. the following invariant should hold:
        /// FORALL p IN LegacyPropertyMapping.Keys: p == p.ToLower()
        /// </remarks>
        IReadOnlyDictionary<string, NewPropertyData> LegacyPropertyMapping { get; }

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
    }
}