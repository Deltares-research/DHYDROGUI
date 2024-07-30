using System;
using System.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.Common.ModelSchema
{
    /// <summary>
    /// A descriptor class for a property (key-value-comment pairs), mainly to be used in a Delft .ini formatted file.
    /// </summary>
    /// <seealso cref="ModelProperty"/>
    public abstract class ModelPropertyDefinition
    {
        private Func<IEnumerable<ModelProperty>, bool> isEnabled;
        private Func<IEnumerable<ModelProperty>, bool> isVisible;

        /// <summary>
        /// Creates a new enabled property definition.
        /// </summary>
        protected ModelPropertyDefinition()
        {
            isEnabled = IsTrue;
            isVisible = IsTrue;
        }

        /// <summary>
        /// The default implementation of <see cref="IsEnabled"/>.
        /// </summary>
        /// <param name="modelProperties">A lookup with all available properties, indexed on their <see cref="FilePropertyKey"/>.</param>
        /// <returns>True if this property is enabled; False when it's not.</returns>
        private static bool IsTrue(IEnumerable<ModelProperty> modelProperties)
        {
            return true;
        }

        /// <summary>
        /// The data type of this property.
        /// </summary>
        public Type DataType { get; set; }

        /// <summary>
        /// The INI section name to which this property belongs to.
        /// </summary>
        public string FileSectionName { get; set; }

        /// <summary>
        /// The key of the property as it occurs in the INI file.
        /// </summary>
        public string FilePropertyKey { get; set; }

        /// <summary>
        /// The schema category.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// The schema sub-category
        /// </summary>
        public string SubCategory { get; set; }

        /// <summary>
        /// Display text for this property.
        /// </summary>
        public string Caption { get; set; }
        
        /// <summary>
        /// The sorting index of this property in the Delft .ini file. A value of -1 indicates that the property
        /// is not included in the sorting. The default value is -1.
        /// </summary>
        public int SortIndex { get; set; } = -1;

        /// <summary>
        /// The default value of this property, represented in string format.
        /// </summary>
        public string DefaultValueAsString { get; set; }

        /// <summary>
        /// The minimum value allowed for this property, represented in string format.
        /// </summary>
        public string MinValueAsString { get; set; }

        /// <summary>
        /// The maximum value allowed for this property, represented in string format.
        /// </summary>
        public string MaxValueAsString { get; set; }

        /// <summary>
        /// Whether this property represents a file location.
        /// </summary>
        public bool IsFile { get; set; }

        /// <summary>
        /// Whether this property represents multiple file locations.
        /// </summary>
        public bool IsMultipleFile { get; set; }

        /// <summary>
        /// Whether this property can or cannot be changed.
        /// </summary>
        public bool ModelFileOnly { get; set; }

        /// <summary>
        /// Additional descriptive text for this property.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Indicates if this property occurs in the schema or not.
        /// </summary>
        public bool IsDefinedInSchema { get; set; }

        /// <summary>
        /// String expression of dependencies to enable this property.
        /// </summary>
        /// <remarks>When setting this property, use <see cref="DeltaShell.Plugins.FMSuite.Common.Dependency.Dependencies"/> to update <see cref="IsEnabled"/>.</remarks>
        public string EnabledDependencies { get; set; }

        /// <summary>
        /// String expression of dependencies make this property visible.
        /// </summary>
        /// <remarks>When setting this property, use <see cref="DeltaShell.Plugins.FMSuite.Common.Dependency.Dependencies"/> to update <see cref="IsVisible"/>.</remarks>
        public string VisibleDependencies { get; set; }

        /// <summary>
        /// Indicates if this property is enabled or not.
        /// Setting this to null reverts it to the default implementation.
        /// </summary>
        public Func<IEnumerable<ModelProperty>, bool> IsEnabled
        {
            get { return isEnabled; }
            set { isEnabled = value ?? IsTrue; }
        }

        public Func<IEnumerable<ModelProperty>, bool> IsVisible
        {
            get { return isVisible; }
            set { isVisible = value ?? IsTrue; }
        }

        public string DocumentationSection { get; set; }

        public int FromRevision { get; set; }

        public int UntilRevision { get; set; }
        public string Unit { get; set; }
        public IList<string> DefaultValueAsStringArray { get; set; }
        public string DefaultsIndexer { get; set; }
    }
}