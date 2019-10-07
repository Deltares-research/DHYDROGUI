using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Files
{
    public class ModelSchemaCsvFile : FMSuiteFileBase
    {
        private const int descriptionIndex = 15;
        private const int unitIndex = 16;

        /// <summary>
        /// Reads <see cref="ModelPropertyDefinition"/> objects from csv and returns them in <see cref="ModelSchema{TDef}"/> object.
        /// </summary>
        /// <typeparam name="TDef">The type of <see cref="ModelPropertyDefinition"/>.</typeparam>
        /// <param name="filePath">The absolute path to the file.</param>
        /// <param name="fileGroupName">Name of the file group.</param>
        /// <returns>A <see cref="ModelSchema{TDef}"/> containing data read from the file.</returns>
        /// <exception cref="FormatException">Thrown when the file format does not comply with the expected file format.</exception>
        public ModelSchema<TDef> ReadModelSchema<TDef>(string filePath, string fileGroupName)
            where TDef : ModelPropertyDefinition, new()
        {
            var schema = new ModelSchema<TDef>();

            OpenInputFile(filePath);
            try
            {
                string line = GetNextLine();
                if (line == null || !line.StartsWith("GUIGroups"))
                {
                    throw new FormatException(string.Format("Expectation GUIGroups on line {0} of file {1}", LineNumber,
                                                            filePath));
                }

                // GUI Groups
                line = GetNextLine();
                while (line != null && !line.StartsWith(fileGroupName))
                {
                    line = line.Trim();
                    string[] lineFields = line.Split(',');
                    if (lineFields.Length >= 2 && !string.IsNullOrWhiteSpace(lineFields[0]))
                    {
                        // gui group
                        schema.GuiPropertyGroups.Add(lineFields[0].Trim(),
                                                     new ModelPropertyGroup(lineFields[1].Trim()));
                    }

                    line = GetNextLine();
                }

                if (line == null || !line.StartsWith(fileGroupName))
                {
                    throw new FormatException(string.Format("Expectation GUIGroups on line {0} of file {1}", LineNumber,
                                                            filePath));
                }

                // Mdu Groups
                line = GetNextLine();
                while (line != null && !line.StartsWith(fileGroupName))
                {
                    line = line.Trim();
                    string[] lineFields = line.Split(',');
                    if (lineFields.Length >= 1 && !string.IsNullOrWhiteSpace(lineFields[0]))
                    {
                        if (!line.StartsWith("MduGroup"))
                        {
                            // add mdu group
                            schema.ModelDefinitionCategory.Add(lineFields[0].Trim(),
                                                               new ModelPropertyGroup(lineFields[0].Trim()));
                        }
                    }

                    line = GetNextLine();
                }

                // Mdu properties
                while ((line = GetNextLine()) != null)
                {
                    var regexRule = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
                    /*
                     About the RegEx:
                    
                    , the separator to be found
                    ?= Asserts that the following will be matched
                    [^\"]* matches any number of characters not present on the list (the double comma)
                    \" matches a double comma
                     
                    So, if an expression is found, where a comma is present BUT between double commas ("),
                    then that expression will not be split.
                     */
                    List<string> lineFields = regexRule.Split(line).Select(lf => lf.Trim()).ToList();
                    if (lineFields.Count < descriptionIndex
                        || lineFields.All(string.IsNullOrEmpty))
                    {
                        // todo: report
                        continue;
                    }

                    string mduGroupName = lineFields[0];
                    string mduPropertyName = lineFields[1];
                    string guiGroupName = lineFields[2];
                    string subCategoryField = lineFields[3];
                    string captionField = lineFields[4];
                    string typeField = lineFields[5];
                    string defaultField = lineFields[6];
                    string minField = lineFields[7];
                    string maxField = lineFields[8];
                    string isReadOnly = lineFields[9];
                    string enabledDeps = lineFields[10];
                    string visibleDeps = lineFields[11];
                    string docSection = lineFields[12];
                    string fromRevString = lineFields[13];
                    string toRevString = lineFields[14];
                    string description = lineFields.ElementAtOrDefault(descriptionIndex) != null
                                             ? lineFields[descriptionIndex].Trim('"')
                                             : string.Empty;
                    string unit = lineFields.ElementAtOrDefault(unitIndex) != null
                                      ? lineFields[unitIndex]
                                      : string.Empty;

                    string defaultValueDependentOn = null;
                    string defaultValues = null;

                    string[] defaultArray = defaultField.Split(':');

                    if (defaultArray.Length > 1)
                    {
                        defaultValueDependentOn = defaultArray[0];
                        defaultValues = defaultArray[1];
                    }

                    int fromRev;
                    int toRev;
                    ParseRevisions(fromRevString, toRevString, out fromRev, out toRev);

                    Type dataType = FMParser.GetClrType(mduPropertyName, typeField, ref captionField,
                                                        filePath, LineNumber);

                    string guiGroupId = string.IsNullOrEmpty(guiGroupName) ? "misc" : guiGroupName;
                    if (!schema.GuiPropertyGroups.ContainsKey(guiGroupId))
                    {
                        throw new FormatException(string.Format("Invalid group id \"{0}\" on line {1} of file {2}",
                                                                guiGroupId, LineNumber, filePath));
                    }

                    if (string.IsNullOrEmpty(captionField))
                    {
                        captionField = mduPropertyName;
                    }

                    ModelPropertyGroup propertyGroup = schema.GuiPropertyGroups[guiGroupId];

                    var propertyDefinition = new TDef
                    {
                        Category = propertyGroup.Name,
                        FileCategoryName = mduGroupName,
                        FilePropertyName = mduPropertyName.Trim('"'),
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
                        IsDefinedInSchema = true,
                    };

                    if (defaultValueDependentOn != null)
                    {
                        propertyDefinition.MultipleDefaultValues = defaultValues.Split('|');
                        propertyDefinition.DefaultValueDependentOn = defaultValueDependentOn;
                        propertyDefinition.MultipleDefaultValuesAvailable = true;
                    }

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
            finally
            {
                CloseInputFile();
            }

            return schema;
        }

        private void ParseRevisions(string fromRevString, string toRevString, out int fromRev, out int toRev)
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