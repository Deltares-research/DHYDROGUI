using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures
{
    public class StructureSchemaCsvFile : NGHSFileBase
    {
        private const int NumberOfColumnsBeforeDescription = 8;

        // Keep 'Redundant size specification' for fail fast change detection during compilation
        // ReSharper disable RedundantExplicitArraySize
        private static readonly string[] StructureHeaders = new string[NumberOfColumnsBeforeDescription + 1]
        {
            "structuretype",
            "attributename",
            "caption",
            "type",
            "default",
            "min",
            "max",
            "structurefileonly",
            "description"
        };
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

                string line = GetNextLine();
                if (line == null || !IsStructureSchemaHeader(line, StructureHeaders))
                {
                    throw new FormatException(string.Format("Structure file header expected on line {0} of file {1}",
                                                            LineNumber, propertiesDefinitionFile));
                }

                while ((line = GetNextLine()) != null)
                {
                    string[] lineFields = line.Split(',');
                    if (lineFields.Length < NumberOfColumnsBeforeDescription)
                    {
                        continue;
                    }

                    for (var i = 0; i < NumberOfColumnsBeforeDescription; ++i)
                    {
                        lineFields[i] = lineFields[i].Trim();
                    }

                    string structureType = lineFields[0];
                    string attributeName = lineFields[1];
                    string captionField = lineFields[2];
                    string typeField = lineFields[3];
                    string defaultField = lineFields[4];
                    string minField = lineFields[5];
                    string maxField = lineFields[6];
                    string isReadOnly = lineFields[7];
                    string description = string.Join("", lineFields.Skip(NumberOfColumnsBeforeDescription));

                    Type dataType = FMParser.GetClrType(attributeName, typeField, ref captionField,
                                                        propertiesDefinitionFile, LineNumber);

                    var propertyDefinition = new StructurePropertyDefinition
                    {
                        Category = structureType, // Category being 'refitted' to be used for property <-> structuretype association
                        FilePropertyKey = attributeName,
                        Caption = captionField,
                        DataType = dataType,
                        DefaultValueAsString = defaultField,
                        MinValueAsString = minField,
                        MaxValueAsString = maxField,
                        IsFile = typeField.ToLower().Equals("filename"),
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
            string[] headerItems = line.Trim().Split(new[]
            {
                ','
            });
            if (headerItems.Length != expectedHeaderItems.Count)
            {
                return false;
            }

            return !expectedHeaderItems.Where((t, i) => headerItems[i].ToLower() != t).Any();
        }
    }
}