using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public class StructureSchemaCsvFile : FMSuiteFileBase
    {
        private const int NumberOfColumnsBeforeDescription = 8;
        // Keep 'Redundant size specification' for fail fast change detection during compilation
        // ReSharper disable RedundantExplicitArraySize
        private static readonly string[] StructureHeaders = new string[NumberOfColumnsBeforeDescription+1] { "structuretype", "attributename", "caption", "type", "default", "min", "max", "structurefileonly", "description" };
        // ReSharper restore RedundantExplicitArraySize

        public StructureSchema<ModelPropertyDefinition> ReadStructureSchema(string propertiesDefinitionFile)
        {
            var schema = new StructureSchema<ModelPropertyDefinition>();

            OpenInputFile(propertiesDefinitionFile);
            try
            {
                // Expected layout:
                // <Optional comments section>
                // <Comma seperated Structure headers>
                // <N rows will data for all columns>

                var line = GetNextLine();
                if (line == null || !IsStructureSchemaHeader(line, StructureHeaders))
                {
                    throw new FormatException(String.Format("Structure file header expected on line {0} of file {1}",
                                                            LineNumber, propertiesDefinitionFile));
                }

                while ((line = GetNextLine()) != null)
                {
                    var lineFields = line.Split(',');
                    if (lineFields.Length < NumberOfColumnsBeforeDescription)
                    {
                        continue;
                    }

                    for (int i = 0; i < NumberOfColumnsBeforeDescription; ++i)
                    {
                        lineFields[i] = lineFields[i].Trim();
                    }

                    var structureType = lineFields[0];
                    var attributeName = lineFields[1];
                    var captionField = lineFields[2];
                    var typeField = lineFields[3];
                    var defaultField = lineFields[4];
                    var minField = lineFields[5];
                    var maxField = lineFields[6];
                    var isReadOnly = lineFields[7];
                    var description = String.Join("", lineFields.Skip(NumberOfColumnsBeforeDescription));

                    var dataType = DataTypeValueParser.GetClrType(attributeName, typeField, ref captionField, propertiesDefinitionFile, LineNumber);

                    var propertyDefinition = new StructurePropertyDefinition
                        {
                            Category = structureType, // Category being 'refitted' to be used for property <-> structuretype association
                            FilePropertyName = attributeName,
                            Caption = captionField,
                            DataType = dataType,
                            DefaultValueAsString = defaultField,
                            MinValueAsString = minField,
                            MaxValueAsString = maxField,
                            ModelFileOnly = isReadOnly.ToLower().Equals("true"),
                            Description = description,
                            IsDefinedInSchema = true
                        };


                    // Add property -> structure type association to schema
                    if (!schema.StructurePropertyGroups.ContainsKey(structureType))
                    {
                        schema.StructurePropertyGroups.Add(structureType, new ModelPropertyGroup(structureType));
                    }
                    schema.StructurePropertyGroups[structureType].PropertyDefinitions.Add(propertyDefinition);
                }
            }
            finally
            {
                CloseInputFile();
            }
            return schema;
        }

        private static bool IsStructureSchemaHeader(string line, ICollection<string> expectedHeaderItems)
        {
            var headerItems = line.Trim().Split(new[] {','});
            if (headerItems.Length != expectedHeaderItems.Count) return false;

            return !expectedHeaderItems.Where((t, i) => headerItems[i].ToLower() != t).Any();
        }
    }
}