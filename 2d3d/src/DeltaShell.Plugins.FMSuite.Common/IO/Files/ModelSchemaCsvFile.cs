using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Files
{
    public class ModelSchemaCsvFile : NGHSFileBase
    {
        private const int descriptionIndex = 15;
        private const int unitIndex = 16;

        /// <summary>
        /// Reads <see cref="ModelPropertyDefinition"/> objects from csv and returns them in
        /// <see cref="ModelPropertySchema{TDefinition}"/> object.
        /// </summary>
        /// <typeparam name="TDef">The type of <see cref="ModelPropertyDefinition"/>.</typeparam>
        /// <param name="filePath">The absolute path to the file.</param>
        /// <param name="fileGroupName">Name of the file group.</param>
        /// <returns>A <see cref="ModelPropertySchema{TDefinition}"/> containing data read from the file.</returns>
        /// <exception cref="FormatException">Thrown when the file format does not comply with the expected file format.</exception>
        public ModelPropertySchema<TDef> ReadModelSchema<TDef>(string filePath, string fileGroupName)
            where TDef : ModelPropertyDefinition, new()
        {
            FileGroupName = fileGroupName;

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                OpenInputFile(fileStream);
                try
                {
                    return ReadModelPropertySchema<TDef>();
                }
                finally
                {
                    CloseInputFile();
                }
            }
        }

        private string FileGroupName { get; set; }

        private ModelPropertySchema<TDef> ReadModelPropertySchema<TDef>() where TDef : ModelPropertyDefinition, new()
        {
            var schema = new ModelPropertySchema<TDef>();

            string line = GetNextLine();
            if (line == null || !line.StartsWith("GUIGroups"))
            {
                throw new FormatException(string.Format("Expectation GUIGroups on line {0} of file {1}", LineNumber,
                                                        InputFilePath));
            }

            ReadAndAddPredefinedGroups(schema);
            IEnumerable<KeyValuePair<string, TDef>> propertyDefinitionsByGuiGroup = ReadMduProperties<TDef>();

            propertyDefinitionsByGuiGroup.ForEach(kvp =>
            {
                TDef propertyDefinition = kvp.Value;
                string guiGroupName = kvp.Key;

                AddPropertyDefinitionToGuiGroup(schema, guiGroupName, propertyDefinition);
                schema.AddPropertyDefinition(propertyDefinition);
            });

            return schema;
        }

        private void ReadAndAddPredefinedGroups<TDef>(ModelPropertySchema<TDef> schema) where TDef : ModelPropertyDefinition, new()
        {
            IEnumerable<KeyValuePair<string, ModelPropertyGroup>> guiPropertyGroups = ReadGuiPropertyGroups();
            guiPropertyGroups.ForEach(kvp => { schema.GuiPropertyGroups.Add(kvp); });

            IEnumerable<KeyValuePair<string, ModelPropertyGroup>> modelDefinitionCategories = ReadModelDefinitionCategories();
            modelDefinitionCategories.ForEach(kvp => { schema.ModelDefinitionCategory.Add(kvp); });
        }

        private IEnumerable<KeyValuePair<string, ModelPropertyGroup>> ReadGuiPropertyGroups()
        {
            string line = GetNextLine();
            while (line != null && !line.StartsWith(FileGroupName))
            {
                line = line.Trim();
                string[] lineFields = line.Split(',');
                if (lineFields.Length >= 2 && !string.IsNullOrWhiteSpace(lineFields[0]))
                {
                    // gui group
                    yield return new KeyValuePair<string, ModelPropertyGroup>(
                        lineFields[0].Trim(), new ModelPropertyGroup(lineFields[1].Trim()));
                }

                line = GetNextLine();
            }

            if (line == null || !line.StartsWith(FileGroupName))
            {
                throw new FormatException(string.Format("Expectation GUIGroups on line {0} of file {1}", LineNumber,
                                                        InputFilePath));
            }
        }

        private IEnumerable<KeyValuePair<string, ModelPropertyGroup>> ReadModelDefinitionCategories()
        {
            string line = GetNextLine();
            while (line != null && !line.StartsWith(FileGroupName))
            {
                line = line.Trim();
                string[] lineFields = line.Split(',');
                if (lineFields.Length >= 1 && !string.IsNullOrWhiteSpace(lineFields[0]) && !line.StartsWith("MduGroup"))
                {
                    yield return new KeyValuePair<string, ModelPropertyGroup>(
                        lineFields[0].Trim(), new ModelPropertyGroup(lineFields[0].Trim()));
                }

                line = GetNextLine();
            }
        }

        private IEnumerable<KeyValuePair<string, TDef>> ReadMduProperties<TDef>() where TDef : ModelPropertyDefinition, new()
        {
            var regexRule = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            /* About the RegEx:

            , the separator to be found
            ?= Asserts that the following will be matched
            [^\"]* matches any number of characters not present on the list (the double comma)
            \" matches a double comma

            So, if an expression is found, where a comma is present BUT between double commas ("),
            then that expression will not be split.
             */

            string line;
            while ((line = GetNextLine()) != null)
            {
                List<string> lineFields = regexRule.Split(line).Select(lf => lf.Trim()).ToList();
                if (lineFields.Count < descriptionIndex || lineFields.All(string.IsNullOrEmpty))
                {
                    continue;
                }

                string guiGroupName = lineFields[2];
                yield return new KeyValuePair<string, TDef>(guiGroupName, ConvertToPropertyDefinition<TDef>(lineFields));
            }
        }

        private void AddPropertyDefinitionToGuiGroup<TDef>(ModelPropertySchema<TDef> schema, string guiGroupName,
                                                           TDef propertyDefinition) where TDef : ModelPropertyDefinition, new()
        {
            ModelPropertyGroup guiPropertyGroup = GetGuiPropertyGroupFromSchema(schema, guiGroupName);
            propertyDefinition.Category = guiPropertyGroup.Name;
            guiPropertyGroup.PropertyDefinitions.Add(propertyDefinition);
        }

        private ModelPropertyGroup GetGuiPropertyGroupFromSchema<TDef>(ModelPropertySchema<TDef> schema, string guiGroupName)
            where TDef : ModelPropertyDefinition, new()
        {
            string guiGroupId = string.IsNullOrEmpty(guiGroupName) ? "misc" : guiGroupName;
            if (!schema.GuiPropertyGroups.ContainsKey(guiGroupId))
            {
                throw new FormatException(string.Format("Invalid group id \"{0}\" on line {1} of file {2}",
                                                        guiGroupId, LineNumber, InputFilePath));
            }

            ModelPropertyGroup propertyGroup = schema.GuiPropertyGroups[guiGroupId];
            return propertyGroup;
        }

        private TDef ConvertToPropertyDefinition<TDef>(IList<string> lineFields)
            where TDef : ModelPropertyDefinition, new()
        {
            string defaultValueDependentOn = null;
            string defaultValues = null;

            string defaultField = lineFields[6];
            string[] defaultArray = defaultField.Split(':');

            if (defaultArray.Length > 1)
            {
                defaultValueDependentOn = defaultArray[0];
                defaultValues = defaultArray[1];
            }

            string fromRevString = lineFields[13];
            string toRevString = lineFields[14];
            ParseRevisions(fromRevString, toRevString, out int fromRev, out int toRev);

            string mduPropertyName = lineFields[1];
            string captionField = lineFields[4];
            string typeField = lineFields[5];
            Type dataType = FMParser.GetClrType(mduPropertyName, typeField, ref captionField,
                                                InputFilePath, LineNumber);

            if (string.IsNullOrEmpty(captionField))
            {
                captionField = mduPropertyName;
            }

            string mduGroupName = lineFields[0];
            string subCategoryField = lineFields[3];
            string minField = lineFields[7];
            string maxField = lineFields[8];
            string isReadOnly = lineFields[9];
            string enabledDeps = lineFields[10];
            string docSection = lineFields[12];
            string visibleDeps = lineFields[11];

            string description = lineFields.ElementAtOrDefault(descriptionIndex) != null
                                     ? lineFields[descriptionIndex].Trim('"')
                                     : string.Empty;

            string unit = lineFields.ElementAtOrDefault(unitIndex) != null
                              ? lineFields[unitIndex]
                              : string.Empty;

            var propertyDefinition = new TDef
            {
                FileSectionName = mduGroupName,
                FilePropertyKey = mduPropertyName.Trim('"'),
                SubCategory = subCategoryField,
                Caption = captionField.Trim('"'),
                DataType = dataType,
                DefaultValueAsString = defaultField,
                MultipleDefaultValuesAvailable = false,
                MinValueAsString = minField,
                MaxValueAsString = maxField,
                IsMultipleFile = typeField.ToLower().Equals("multipleentriesfilename"),
                IsFile = typeField.ToLower().Equals("filename") ||
                         typeField.ToLower().Equals("multipleentriesfilename"),
                ModelFileOnly = isReadOnly.ToLower().Equals("true"),
                Description = description,
                Unit = unit,
                DocumentationSection = docSection,
                EnabledDependencies = enabledDeps.ToLower(),
                VisibleDependencies = visibleDeps.ToLower(),
                FromRevision = fromRev,
                UntilRevision = toRev,
                IsDefinedInSchema = true
            };

            if (defaultValueDependentOn != null)
            {
                propertyDefinition.MultipleDefaultValues = defaultValues.Split('|');
                propertyDefinition.DefaultValueDependentOn = defaultValueDependentOn;
                propertyDefinition.MultipleDefaultValuesAvailable = true;
            }

            return propertyDefinition;
        }

        private static void ParseRevisions(string fromRevString, string toRevString, out int fromRev, out int toRev)
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