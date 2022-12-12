using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    /// <summary>
    /// ZW from Gis importer, used for Gis importers using ZW.
    /// </summary>
    public class ZwFromGisImporter
    {
        /// <summary>
        /// Label of level.
        /// </summary>
        public const string LblLevel = "Level ";

        /// <summary>
        /// Dictionary holding properties.
        /// </summary>
        /// <remarks>
        /// <b>Key</b> Property mapping name.<br/>
        /// <b>Value</b> Unit of property mapping.
        /// </remarks>
        private readonly Dictionary<string, string> propertyMappingsNamesAndUnit;

        /// <summary>
        /// Constructor of ZwFromGisImporter.
        /// </summary>
        /// <param name="propertyMappings">Dictionary holding properties.</param>
        public ZwFromGisImporter(Dictionary<string, string> propertyMappings)
        {
            Ensure.NotNull(propertyMappings, nameof(propertyMappings));
            propertyMappingsNamesAndUnit = propertyMappings;
        }

        /// <summary>
        /// Number of levels.
        /// </summary>
        public int NumberOfLevels { get; private set; }

        /// <summary>
        /// Method which updated the number of levels based on the amount of level labels found in the properties mapping.
        /// </summary>
        /// <param name="propertiesMapping">List of the mapped properties.</param>
        public void UpdateNumberOfLevels(List<PropertyMapping> propertiesMapping)
        {
            NumberOfLevels = propertiesMapping.Count(pm => pm.PropertyName.StartsWith(LblLevel));
        }

        /// <summary>
        /// Method which adds/removes the property mapping and number of levels based on the given levels.
        /// </summary>
        /// <param name="levels">Given levels.</param>
        /// <param name="propertiesMapping">List of the mapped properties.</param>
        public void MakeNumberOfLevelPropertiesMapping(int levels, List<PropertyMapping> propertiesMapping)
        {
            RemoveExistingLeveledPropertyMappings(levels, propertiesMapping);
            AddLeveledPropertyMappings(levels, propertiesMapping);
            UpdateNumberOfLevels(propertiesMapping);
        }

        private void AddLeveledPropertyMappings(int levels, List<PropertyMapping> propertiesMapping)
        {
            for (int i = NumberOfLevels; i < levels; i++)
            {
                int j = i + 1;

                //level
                var propertyMapping = new PropertyMapping(LblLevel + j, false, true) { PropertyUnit = "m" };
                propertiesMapping.Add(propertyMapping);

                foreach (KeyValuePair<string, string> propertyMapped in propertyMappingsNamesAndUnit)
                {
                    propertyMapping = new PropertyMapping($"{propertyMapped.Key} {j}", false, true) { PropertyUnit = propertyMapped.Value };
                    propertiesMapping.Add(propertyMapping);
                }
            }
        }

        private void RemoveExistingLeveledPropertyMappings(int levels, List<PropertyMapping> propertiesMapping)
        {
            for (int i = NumberOfLevels; i > levels; i--)
            {
                //level
                PropertyMapping propertyMapping = propertiesMapping.First(p => p.PropertyName == LblLevel + i);
                propertiesMapping.Remove(propertyMapping);

                foreach (KeyValuePair<string, string> propertyMapped in propertyMappingsNamesAndUnit)
                {
                    propertyMapping = propertiesMapping.First(p => p.PropertyName == $"{propertyMapped.Key} {i}");
                    propertiesMapping.Remove(propertyMapping);
                }
            }
        }
    }
}