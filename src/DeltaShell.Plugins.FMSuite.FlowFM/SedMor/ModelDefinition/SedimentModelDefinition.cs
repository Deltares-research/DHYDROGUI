using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.FlowFM.SedMor.ModelDefinition
{
    public class SedimentModelDefinition
    {
        public IEventedList<Sediment> Sediments { get; private set; }

        public Dictionary<string, SedMorProperty> Properties { get; private set; }
        public ModelSchema<SedMorPropertyDefinition> ModelSchema { get; private set; }
        public IDictionary<string, SedMorPropertyDefinition> SedimentSchema { get; private set; }

        public SedimentModelDefinition()
        {
            LoadSchemas();
            Sediments = new EventedList<Sediment>();

            // initialize default properties:
            Properties = new Dictionary<string, SedMorProperty>();
            foreach (var propertyDefinition in ModelSchema.PropertyDefinitions)
            {
                Properties.Add(propertyDefinition.Key, new SedMorProperty(propertyDefinition.Value,
                                                                          propertyDefinition.Value.DefaultValueAsString));
            }
        }

        private void LoadSchemas()
        {
            const string sedPropertiesCsvFileName = "sed-properties.csv";
            var assemblyLocation = GetType().Assembly.Location;
            var directoryInfo = new FileInfo(assemblyLocation).Directory;
            if (directoryInfo != null)
            {
                var path = directoryInfo.FullName;
                var propertiesDefinitionFile = Path.Combine(path, sedPropertiesCsvFileName);
                ModelSchema = new ModelSchemaCsvFile().ReadModelSchema<SedMorPropertyDefinition>(
                    propertiesDefinitionFile, "SedGroup");

                // extract sediment entries from model schema
                SedimentSchema = ModelSchema.PropertyDefinitions.Where(pd => pd.Value.FileCategoryName == "Sediment")
                                            .ToDictionary(kv => kv.Key, kv => kv.Value);

                // remove sediment entries from model schema:
                foreach (var sedimentDef in SedimentSchema)
                    ModelSchema.PropertyDefinitions.Remove(sedimentDef);
                ModelSchema.GuiPropertyGroups.Remove("Sediment");
                ModelSchema.GuiPropertyGroups.Remove("SedimentAdvanced");
                ModelSchema.ModelDefinitionCategory.Remove("Sediment");
            }
            else
            {
                throw new Exception("Failed to load property definition file: " + sedPropertiesCsvFileName);
            }
        }

        public double ReferenceDensity
        {
            get { return (double)Properties[SedProperties.Cref].Value; }
            set { Properties[SedProperties.Cref].Value = value; }
        }
    }
}