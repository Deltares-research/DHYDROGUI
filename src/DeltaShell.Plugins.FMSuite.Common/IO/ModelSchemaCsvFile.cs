using System;
using System.Linq;
using System.Text.RegularExpressions;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public class ModelSchemaCsvFile : FMSuiteFileBase
    {
        public const string DefaultGUIGroupID = "misc";
        public const string DefaultGUIGroupCaption = "Miscellaneous";
        const int DescriptionIndex = 15;
        const int UnitIndex = 16;

        /// <summary>
        /// Method for reading the csv's and creating the corresponding model schema with the property definitions.
        /// </summary>
        /// <typeparam name="TDef">The type of the definition.</typeparam>
        /// <param name="propertiesDefinitionFile">The properties definition file.</param>
        /// <param name="fileGroupName">Name of the file group.</param>
        /// <returns></returns>
        /// <exception cref="System.FormatException">
        /// </exception>
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
                if (line == null || !line.StartsWith(fileGroupName))
                {
                    throw new FormatException(String.Format("Expectation GUIGroups on line {0} of file {1}", LineNumber,
                                                      propertiesDefinitionFile));
                }

                // Mdu Groups
                line = GetNextLine();
                while (line != null && !line.StartsWith(fileGroupName))
                {
                    line = line.Trim();
                    var lineFields = line.Split(',');
                    if (lineFields.Length >= 1 && !String.IsNullOrWhiteSpace(lineFields[0]))
                    {
                        if (!line.StartsWith("MduGroup"))
                        {
                            // add mdu group
                            schema.ModelDefinitionCategory.Add(lineFields[0].Trim(), new ModelPropertyGroup(lineFields[0].Trim()));
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
                    var lineFields = regexRule.Split(line).Select(lf => lf.Trim()).ToList();
                    if (lineFields.Count < DescriptionIndex
                        || lineFields.All(String.IsNullOrEmpty))
                    {
                        // todo: report
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
                    var description = lineFields.ElementAtOrDefault(DescriptionIndex) != null ? lineFields[DescriptionIndex].Trim('"') : string.Empty;
                    var unit = lineFields.ElementAtOrDefault(UnitIndex) != null ? lineFields[UnitIndex] : string.Empty;

                    string defaultValueDependentOn = null;
                    string defaultValues = null;
                 
                    var defaultArray = defaultField.Split(':');

                    if (defaultArray.Length > 1)
                    { 
                        defaultValueDependentOn = defaultArray[0];
                        defaultValues = defaultArray[1];
                    }
                   
                    int fromRev;
                    int toRev;
                    ParseRevisions(fromRevString, toRevString, out fromRev, out toRev);

                    var dataType = FMParser.GetClrType(mduPropertyName, typeField, ref captionField,
                        propertiesDefinitionFile, LineNumber);

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