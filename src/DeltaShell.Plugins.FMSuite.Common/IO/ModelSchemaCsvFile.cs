using System;
using System.Linq;
using System.Text.RegularExpressions;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public class ModelSchemaCsvFile : FMSuiteFileBase
    {
        public const string DefaultGUIGroupID = "misc";
        public const string DefaultGUIGroupCaption = "Miscellaneous";
        private const string descriptionHeader = "Description";
        private readonly Regex csvParser;
        public ModelSchemaCsvFile()
        {
            csvParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))",RegexOptions.Compiled);
            
        }

        public ModelSchema<TDef> ReadModelSchema<TDef>(string propertiesDefinitionFile, string fileGroupName) 
            where TDef:ModelPropertyDefinition,new()
        {
            var schema = new ModelSchema<TDef>();

            OpenInputFile(propertiesDefinitionFile);

            try
            {
                var line = GetNextLine();
                if (line == null || !line.StartsWith("GUIGroups"))
                {
                    throw new FormatException(String.Format("Expectation GUIGroups on line {0} of file {1}", LineNumber,
                                                            propertiesDefinitionFile));
                }

                // GUI Groups
                line = GetNextLine();
                line = ReadGuiGroups(fileGroupName, line, schema);

                if (line == null || !line.StartsWith(fileGroupName))
                {
                    throw new FormatException(String.Format("Expectation GUIGroups on line {0} of file {1}", LineNumber,
                                                            propertiesDefinitionFile));
                }

                // Mdu Groups
                line = GetNextLine();
                line = ReadMduGroups(fileGroupName, line, schema);

                // Mdu properties
                ReadMduGroupsProperties(propertiesDefinitionFile, schema, GetIndexOfDescription(line));
            }
            finally
            {
                CloseInputFile();
            }

            return schema;
        }

        private static int GetIndexOfDescription(string line)
        {
            string[] lineFields = line.Split(',');
            return Array.FindIndex(lineFields, column => column == descriptionHeader);
        }

        private void ReadMduGroupsProperties<TDef>(string propertiesDefinitionFile, ModelSchema<TDef> schema, int indexOfDescription) where TDef : ModelPropertyDefinition, new()
        {
            string line;
            while ((line = GetNextLine()) != null)
            {
                var lineFields = csvParser.Split(line);
                if (RowHasLessThanExpectedAmountOfColumns(indexOfDescription, lineFields))
                {
                    continue;
                }

                if (RowHasOnlyEmptyColumns(indexOfDescription, lineFields))
                {
                    continue;
                }

                var mduGroupName = lineFields[0];
                var mduPropertyName = lineFields[1];
                var guiGroupName = lineFields[2];
                var subCategoryField = lineFields[3];
                var captionField = lineFields[4];
                var typeField = lineFields[5];
                var defaultField = lineFields[6];
                var minField = lineFields[7];
                var maxField = lineFields[8];
                var isReadOnly = lineFields[9];
                var enabledDeps = lineFields[10];
                var visibleDeps = lineFields[11];
                var docSection = lineFields[12];
                var fromRevString = lineFields[13];
                var toRevString = lineFields[14];
                var unit = GetUnitField(indexOfDescription, lineFields);
                var defaultsIndexerField = GetDefaultsIndexerField(indexOfDescription, lineFields);

                int fromRev;
                int toRev;
                ParseRevisions(fromRevString, toRevString, out fromRev, out toRev);

                string description = CreateDescriptionFromLastColumns(lineFields, indexOfDescription);

                var dataType = DataTypeValueParser.GetClrType(mduPropertyName, typeField, ref captionField, propertiesDefinitionFile, LineNumber);

                var guiGroupId = string.IsNullOrEmpty(guiGroupName) ? DefaultGUIGroupID : guiGroupName;
                if (!schema.GuiPropertyGroups.ContainsKey(guiGroupId))
                {
                    throw new FormatException(String.Format("Invalid group id \"{0}\" on line {1} of file {2}",
                                                            guiGroupId, LineNumber, propertiesDefinitionFile));
                }

                if (string.IsNullOrEmpty(captionField)) captionField = mduPropertyName;

                var propertyGroup = schema.GuiPropertyGroups[guiGroupId];

                var propertyDefinition = new TDef
                {
                    Category = propertyGroup.Name,
                    FileCategoryName = mduGroupName,
                    FilePropertyName = mduPropertyName,
                    SubCategory = subCategoryField,
                    Caption = captionField,
                    DataType = dataType,
                    DefaultValueAsString = GetDefaultValueAsString(defaultsIndexerField, defaultField),
                    MinValueAsString = minField,
                    MaxValueAsString = maxField,
                    IsMultipleFile = typeField.ToLower().Equals("multipleentriesfilename"),
                    IsFile = typeField.ToLower().Equals("filename") || typeField.ToLower().Equals("multipleentriesfilename"),
                    ModelFileOnly = isReadOnly.ToLower().Equals("true"),
                    Description = description,
                    DocumentationSection = docSection,
                    EnabledDependencies = enabledDeps.ToLower(),
                    VisibleDependencies = visibleDeps.ToLower(),
                    DefaultValueAsStringArray = defaultField.Split('|'),
                    DefaultsIndexer = defaultsIndexerField,
                    FromRevision = fromRev,
                    UntilRevision = toRev,
                    IsDefinedInSchema = true,
                    Unit = unit
                };

                propertyGroup.PropertyDefinitions.Add(propertyDefinition);

                schema.PropertyDefinitions.Add(mduPropertyName.ToLower(), propertyDefinition);

                // register the propertyDe to the group and to the lookup tables
                if (!schema.ModelDefinitionCategory.ContainsKey(mduGroupName))
                {
                    schema.ModelDefinitionCategory.Add(mduGroupName, new ModelPropertyGroup(mduGroupName));
                }

                schema.ModelDefinitionCategory[mduGroupName].AddPropertyDefinition(propertyDefinition);
            }
        }

        private static bool RowHasLessThanExpectedAmountOfColumns(int indexOfDescription, string[] lineFields)
        {
            return lineFields.Length < indexOfDescription;
        }

        private static bool RowHasOnlyEmptyColumns(int indexOfDescription, string[] lineFields)
        {
            bool atLeastOneColumnFilled = false;
            for (int i = 0; i < indexOfDescription; ++i)
            {
                lineFields[i] = lineFields[i].Trim();
                atLeastOneColumnFilled |= !String.IsNullOrEmpty(lineFields[i]);
            }

            return !atLeastOneColumnFilled;
        }
        
        private static string GetUnitField(int indexOfDescription, string[] lineFields)
        {
            const int expectedUnitField = 15;
            return indexOfDescription == expectedUnitField ? string.Empty : lineFields[expectedUnitField];
        }

        private static string CreateDescriptionFromLastColumns(string[] lineFields, int amountOfColumnsBeforeDescription)
        {
            return string.Join("", lineFields.Skip(amountOfColumnsBeforeDescription).Select(s => s.Trim('"')));
        }
        
        private static string GetDefaultsIndexerField(int indexOfDescription, string[] lineFields)
        {
            const int expectedDefaultsIndexerField = 16;
            return indexOfDescription <= expectedDefaultsIndexerField ? string.Empty : lineFields[expectedDefaultsIndexerField];
        }

        private static string GetDefaultValueAsString(string defaultsIndexerField, string defaultField)
        {
            if (string.IsNullOrEmpty(defaultsIndexerField))
            {
                return defaultField;
            }

            return defaultField.Contains('|') ? defaultField.Split('|').First() : defaultField;
        }

        private string ReadMduGroups<TDef>(string fileGroupName, string line, ModelSchema<TDef> schema) where TDef : ModelPropertyDefinition, new()
        {
            while (line != null && !line.StartsWith(fileGroupName))
            {
                line = line.Trim();
                var lineFields = line.Split(',');
                if (lineFields.Length >= 1 && !String.IsNullOrWhiteSpace(lineFields[0]) && !line.StartsWith("MduGroup"))
                {
                    // add mdu group
                    schema.ModelDefinitionCategory.Add(lineFields[0].Trim(), new ModelPropertyGroup(lineFields[0].Trim()));
                }

                line = GetNextLine();
            }

            return line;
        }

        private string ReadGuiGroups<TDef>(string fileGroupName, string line, ModelSchema<TDef> schema) where TDef : ModelPropertyDefinition, new()
        {
            while (line != null && !line.StartsWith(fileGroupName))
            {
                line = line.Trim();
                var lineFields = line.Split(',');
                if (lineFields.Length >= 2 && !String.IsNullOrWhiteSpace(lineFields[0]))
                {
                    // gui group
                    schema.GuiPropertyGroups.Add(lineFields[0].Trim(), new ModelPropertyGroup(lineFields[1].Trim()));
                }

                line = GetNextLine();
            }

            return line;
        }

        void ParseRevisions(string fromRevString, string toRevString, out int fromRev, out int toRev)
        {
            int.TryParse(fromRevString, out fromRev);
            int.TryParse(toRevString, out toRev);
            if (fromRev > toRev)
            {
                fromRev = 0;
                toRev = int.MaxValue;
            }
        }
    }
}