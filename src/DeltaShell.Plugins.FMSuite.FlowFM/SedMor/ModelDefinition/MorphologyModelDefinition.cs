using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.FlowFM.SedMor.ModelDefinition
{
    public class MorphologyModelDefinition
    {
        public IEventedList<MorphologyBoundary> Boundaries { get; private set; }

        public Dictionary<string, SedMorProperty> Properties { get; private set; }
        public ModelSchema<SedMorPropertyDefinition> ModelSchema { get; private set; }
        public IDictionary<string, SedMorPropertyDefinition> BoundarySchema { get; private set; }

        public MorphologyModelDefinition()
        {
            LoadSchemas();
            Boundaries = new EventedList<MorphologyBoundary>();

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
            const string morPropertiesCsvFileName = "mor-properties.csv";
            var assemblyLocation = GetType().Assembly.Location;
            var directoryInfo = new FileInfo(assemblyLocation).Directory;
            if (directoryInfo != null)
            {
                var path = directoryInfo.FullName;
                var propertiesDefinitionFile = Path.Combine(path, morPropertiesCsvFileName);
                ModelSchema = new ModelSchemaCsvFile().ReadModelSchema<SedMorPropertyDefinition>(
                    propertiesDefinitionFile, "MorGroup");

                // extract sediment entries from model schema
                BoundarySchema = ModelSchema.PropertyDefinitions.Where(pd => pd.Value.FileCategoryName == "Boundary")
                                            .ToDictionary(kv => kv.Key, kv => kv.Value);

                // remove sediment entries from model schema:
                foreach (var boundDef in BoundarySchema)
                    ModelSchema.PropertyDefinitions.Remove(boundDef);
                ModelSchema.GuiPropertyGroups.Remove("Boundary");
                ModelSchema.ModelDefinitionCategory.Remove("Boundary");
            }
            else
            {
                throw new Exception("Failed to load property definition file: " + morPropertiesCsvFileName);
            }
        }

        public double Bed
        {
            get { return (double)Properties[MorProperties.Bed].Value; }
            set { Properties[MorProperties.Bed].Value = value; }
        }
    }
}