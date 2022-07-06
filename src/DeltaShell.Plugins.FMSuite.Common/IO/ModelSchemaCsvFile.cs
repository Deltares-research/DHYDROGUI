using System;
using System.Linq;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public class ModelSchemaCsvFile : FMSuiteFileBase
    {
        public const string DefaultGUIGroupID = "misc";
        public const string DefaultGUIGroupCaption = "Miscellaneous";
        const int NumberOfColumnsBeforeDescription = 15;

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
                    if (lineFields.Length >= 1 && !String.IsNullOrWhiteSpace(lineFields[0]) && !line.StartsWith("MduGroup"))
                    {
                        // add mdu group
                        schema.ModelDefinitionCategory.Add(lineFields[0].Trim(), new ModelPropertyGroup(lineFields[0].Trim()));
                    }

                    line = GetNextLine();
                }

                // Mdu properties
                while ((line = GetNextLine()) != null)
                {
                    var lineFields = line.Split(',');
                    if (lineFields.Length < NumberOfColumnsBeforeDescription)
                    {
                        continue;
                    }

                    bool atLeastOneColumnFilled = false;
                    for (int i = 0; i < NumberOfColumnsBeforeDescription; ++i)
                    {
                        lineFields[i] = lineFields[i].Trim();
                        atLeastOneColumnFilled |= !String.IsNullOrEmpty(lineFields[i]);
                    }

                    if (!atLeastOneColumnFilled)
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

                    int fromRev;
                    int toRev;
                    ParseRevisions(fromRevString, toRevString, out fromRev, out toRev);

                    var description = string.Join("",
                                                  lineFields.Skip(NumberOfColumnsBeforeDescription).Select(s => s.Trim('"')));

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
                        DefaultValueAsString = defaultField,
                        MinValueAsString = minField,
                        MaxValueAsString = maxField,
                        IsMultipleFile = typeField.ToLower().Equals("multipleentriesfilename"),
                        IsFile = typeField.ToLower().Equals("filename") || typeField.ToLower().Equals("multipleentriesfilename"),
                        ModelFileOnly = isReadOnly.ToLower().Equals("true"),
                        Description = description,
                        DocumentationSection = docSection,
                        EnabledDependencies = enabledDeps.ToLower(),
                        VisibleDependencies = visibleDeps.ToLower(),
                        FromRevision = fromRev,
                        UntilRevision = toRev,
                        IsDefinedInSchema = true
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