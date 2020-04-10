using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Shell.Core;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Data;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    public class SubFileImporter : IFileImporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SubFileImporter));
        private const string UnitCharacters = @"[#A-Za-z0-9\s\(\)-\\/\.\+\<\>,\|_&;:\[\]\%\{\}]*";

        public string Name => "Substance Process Library";

        public string Category { get; private set; }
        public string Description => string.Empty;

        public Bitmap Image { get; private set; }

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(SubstanceProcessLibrary);
            }
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel => false;

        public string FileFilter => "Sub Files (*.sub)|*.sub";

        public bool OpenViewAfterImport => false;

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public object ImportItem(string path, object target)
        {
            Import(target as SubstanceProcessLibrary, path ?? DefaultFilePath);

            return target;
        }

        /// <summary>
        /// Default file path (used in <see cref="ImportItem" /> if no path parameter is provided)
        /// </summary>
        public string DefaultFilePath { private get; set; }

        /// <summary>
        /// Imports substance process library data from <paramref name="path" /> to <paramref name="substanceProcessLibrary" />
        /// </summary>
        public void Import(SubstanceProcessLibrary substanceProcessLibrary, string path)
        {
            // Check if the substance process library is set
            if (substanceProcessLibrary == null)
            {
                throw new InvalidOperationException(Resources.SubFileImporter_Substance_process_library_not_set);
            }

            // Check if the path is set
            if (string.IsNullOrEmpty(path))
            {
                Log.Warn(Resources.SubFileImporter_Path_not_set);
                return;
            }

            // Check if the file exists
            if (!File.Exists(path))
            {
                Log.Warn(Resources.SubFileImporter_File_not_found);
                return;
            }

            string subFileText = File.ReadAllText(path);

            if (!ShouldCancel)
            {
                ImportSubstances(substanceProcessLibrary.Substances, subFileText);
            }

            if (!ShouldCancel)
            {
                ImportParameters(substanceProcessLibrary.Parameters, subFileText);
            }

            if (!ShouldCancel)
            {
                ImportProcesses(substanceProcessLibrary.Processes, subFileText);
            }

            if (!ShouldCancel)
            {
                ImportOutputParameters(substanceProcessLibrary.OutputParameters, subFileText);
            }

            substanceProcessLibrary.Name =
                Path.GetFileNameWithoutExtension(path) + (ShouldCancel ? " (import cancelled)" : "");

            if (ShouldCancel)
            {
                Log.Warn("Sub file import cancelled");
                return;
            }

            Log.Info(string.Format(Resources.SubFileImporter_Import_Sub_file_successfully_imported_from___0_, path));
            substanceProcessLibrary.ImportedSubstanceFilePath = path;
        }

        private void ImportSubstances(IEventedList<WaterQualitySubstance> librarySubstances, string subFileText)
        {
            string substancePattern =
                @"substance\s*'(?<Name>" + RegularExpression.Characters + @")'\s*(?<Active>" +
                RegularExpression.Characters + @")\s*\n" +
                @"\s*description\s*'(?<Description>" + RegularExpression.ExtendedCharacters + @")'\s*\n" +
                @"\s*concentration-unit\s*'(?<ConcentrationUnit>" + UnitCharacters + @")'\s*\n" +
                @"\s*waste-load-unit\s*'(?<WasteLoadUnit>" + UnitCharacters + @")'\s*\n" +
                @"end-substance";

            IEnumerable<WaterQualitySubstance> substances = CreateWaterQualityModelElements(substancePattern, subFileText, GetSubstance);
            // If there are no substances, try parsing according to the new substance pattern where the definitions 
            // are defined on a single line.
            if (!substances.Any())
            {
                substancePattern =
                    @"substance\s*'(?<Name>" + RegularExpression.Characters + @")'\s*" +
                    "(?<Active>" + RegularExpression.Characters + @")\s*" +
                    @"description\s*'(?<Description>" + RegularExpression.ExtendedCharacters + @")'\s*" +
                    @"concentration-unit\s*'(?<ConcentrationUnit>" + UnitCharacters + @")'\s*" +
                    @"waste-load-unit\s*'(?<WasteLoadUnit>" + UnitCharacters + @")'\s*" +
                    @"end-substance";

                substances = CreateWaterQualityModelElements(substancePattern, subFileText, GetSubstance);
            }

            ImportElementInformation<WaterQualitySubstance> importInformation = GetImportInformation(librarySubstances, substances, Equals);
            WaterQualitySubstance[] substancesToRemove = librarySubstances.Except(importInformation.ExistingElements).ToArray();
            RemoveIrrelevantElements(librarySubstances, substancesToRemove, Resources.SubFileImporter_Substances_Name);
           
            AddNewElements(librarySubstances, importInformation.NewElements, Resources.SubFileImporter_Substances_Name);
        }

        private void ImportParameters(IEventedList<WaterQualityParameter> libraryParameters, string subFileText)
        {
            string parameterPattern = @"parameter\s*'(?<Name>" + RegularExpression.Characters + @")'\s*\n" +
                                      @"\s*description\s*'(?<Description>" +
                                      RegularExpression.ExtendedCharacters + @")'\s*\n" +
                                      @"\s*unit\s*'(?<Unit>" + UnitCharacters + @")'\s*\n" +
                                      @"\s*value\s*(?<Value>" + RegularExpression.Characters + @")\s*\n" +
                                      @"end-parameter";

            IEnumerable<WaterQualityParameter> parameters = CreateWaterQualityModelElements(parameterPattern, subFileText, GetParameter);
            // If there are no parameters, try parsing according to the new parameter pattern where the definitions 
            // are defined on a single line.
            if (!parameters.Any())
            {
                parameterPattern =
                    @"parameter\s*'(?<Name>" + RegularExpression.Characters + @")'\s*" +
                    @"description\s*'(?<Description>" + RegularExpression.ExtendedCharacters + @")'\s*" +
                    @"unit\s*'(?<Unit>" + UnitCharacters + @")'\s*" +
                    @"value\s*(?<Value>" + RegularExpression.Characters + @")\s*" +
                    @"end-parameter";

                parameters = CreateWaterQualityModelElements(parameterPattern, subFileText, GetParameter);
            }

            ImportElementInformation<WaterQualityParameter> importInformation = GetImportInformation(libraryParameters, parameters, Equals);
            WaterQualityParameter[] parametersToRemove = libraryParameters.Except(importInformation.ExistingElements).ToArray();
            RemoveIrrelevantElements(libraryParameters, parametersToRemove, Resources.SubFileImporter_Parameters_Name);
            
            AddNewElements(libraryParameters, importInformation.NewElements, Resources.SubFileImporter_Parameters_Name);
        }

        private void ImportProcesses(IEventedList<WaterQualityProcess> libraryProcesses, string subFileText)
        {
            const string processesPattern = @"active-processes\s*\n(?<Processes>.*)end-active-processes";
            const string processPattern = @"\s*name\s*'(?<Name>" + RegularExpression.Characters +
                                          @")'\s*'(?<Description>" + RegularExpression.ExtendedCharacters + @")'\s*\n";

            MatchCollection processesMatches = RegularExpression.GetMatches(processesPattern, subFileText);

            IEnumerable<WaterQualityProcess> processes = Enumerable.Empty<WaterQualityProcess>();
            if (processesMatches.Count > 0)
            {
                string content = processesMatches[0].Groups["Processes"].Value;
                processes = CreateWaterQualityModelElements(processPattern, content, GetProcess);
            }

            ImportElementInformation<WaterQualityProcess> importInformation = GetImportInformation(libraryProcesses, processes, Equals);
            WaterQualityProcess[] processesToRemove = libraryProcesses.Except(importInformation.ExistingElements).ToArray();
            RemoveIrrelevantElements(libraryProcesses, processesToRemove, Resources.SubFileImporter_Processes_Name);

            AddNewElements(libraryProcesses, importInformation.NewElements, Resources.SubFileImporter_Processes_Name);
        }

        private void ImportOutputParameters(IEventedList<WaterQualityOutputParameter> libraryOutputParameters, string subFileText)
        {
            const string outputParameterPattern = @"output\s*'(?<Name>" + RegularExpression.Characters + @")'\s*\n" +
                                                  @"\s*description\s*'(?<Description>" +
                                                  RegularExpression.ExtendedCharacters + @")'\s*\n" +
                                                  @"end-output";

            var newOutputParameters = new Collection<WaterQualityOutputParameter>();
            var existingOutputParameters = new Collection<WaterQualityOutputParameter>();
            IEnumerable<WaterQualityOutputParameter> outputs = CreateWaterQualityModelElements(outputParameterPattern, subFileText, GetOutputParameter);
            foreach (WaterQualityOutputParameter output in outputs)
            {
                // Ignore existing default output parameters
                if (IsExistingDefaultOutputParameter(output, libraryOutputParameters))
                {
                    continue;
                }

                WaterQualityOutputParameter existingOutput = libraryOutputParameters.FirstOrDefault(p => Equals(p, output));
                if (existingOutput == null)
                {
                    newOutputParameters.Add(output);
                }
                else
                {
                    existingOutputParameters.Add(existingOutput);
                }
            }

            // Remove all irrelevant output parameters
            WaterQualityOutputParameter[] outputParametersToRemove = libraryOutputParameters.Except(existingOutputParameters)
                                                                                            .Where(outputParameter => !IsDefaultOutputParameter(outputParameter))
                                                                                            .ToArray();
            RemoveIrrelevantElements(libraryOutputParameters, outputParametersToRemove, Resources.SubFileImporter_OutputParameters_Name);
            AddNewElements(libraryOutputParameters, newOutputParameters, Resources.SubFileImporter_OutputParameters_Name);

            AddDefaultOutputParameters(libraryOutputParameters);
        }

        /// <summary>
        /// Creates a collection of water quality model elements based on its input arguments.
        /// </summary>
        /// <typeparam name="TWaterQualityElement"> The type of element of the water quality model that needs to be created. </typeparam>
        /// <param name="pattern"> The pattern to extract the elements from the <paramref name="subFileContents" />. </param>
        /// <param name="subFileContents"> The contents of the file to extract the elements from. </param>
        /// <param name="createElementFunc"> The function to create the element based on the pattern match. </param>
        /// <returns> A collection of <typeparamref name="TWaterQualityElement" />. </returns>
        /// <remarks>
        /// The type constraint is currently loosely based on the <see cref="WaterQualitySubstance" />,
        /// <see cref="WaterQualitySubstance" />, <see cref="WaterQualityProcess" /> and <see cref="WaterQualityOutputParameter" />
        /// .
        /// Ideally speaking, a proper abstraction for these elements should be implemented to make this method more restrictive.
        /// </remarks>
        private static IEnumerable<TWaterQualityElement> CreateWaterQualityModelElements<TWaterQualityElement>(string pattern, string subFileContents,
                                                                                                               Func<Match, TWaterQualityElement> createElementFunc)
            where TWaterQualityElement : Unique<long>, INameable, ICloneable
        {
            MatchCollection matches = RegularExpression.GetMatches(pattern, subFileContents);

            var elements = new List<TWaterQualityElement>();
            foreach (Match match in matches)
            {
                elements.Add(createElementFunc(match));
            }

            return elements;
        }

        /// <summary>
        /// Retrieves the collection of new elements from <paramref name="newElements" />.
        /// </summary>
        /// <typeparam name="TWaterQualityElement"> The type of element of the water quality model. </typeparam>
        /// <param name="existingElements"> The elements that are already present. </param>
        /// <param name="newElements"> The elements to determine the new elements from. </param>
        /// <param name="getEqualityFunc">The function to determine the equality between two <typeparamref name="TWaterQualityElement"/>. </param>
        /// <returns> A collection of new elements. </returns>
        /// <remarks>
        /// The type constraint is currently loosely based on the <see cref="WaterQualitySubstance" />,
        /// <see cref="WaterQualitySubstance" />, <see cref="WaterQualityProcess" /> and <see cref="WaterQualityOutputParameter" />
        /// .
        /// Ideally speaking, a proper abstraction for these elements should be implemented to make this method more restrictive.
        /// </remarks>
        private static ImportElementInformation<TWaterQualityElement> GetImportInformation<TWaterQualityElement>(IEventedList<TWaterQualityElement> existingElements,
                                                                                                                 IEnumerable<TWaterQualityElement> newElements,
                                                                                                                 Func<TWaterQualityElement, TWaterQualityElement, bool> getEqualityFunc)
            where TWaterQualityElement : Unique<long>, INameable, ICloneable
        {
            var existingElementsDuringImport = new Collection<TWaterQualityElement>();
            var newElementsDuringImport = new Collection<TWaterQualityElement>();
            foreach (TWaterQualityElement element in newElements)
            {
                TWaterQualityElement existingElement = existingElements.FirstOrDefault(p => getEqualityFunc(p, element));
                if (existingElement != null)
                {
                    existingElementsDuringImport.Add(existingElement);
                }
                else
                {
                    newElementsDuringImport.Add(element);
                }
            }

            return new ImportElementInformation<TWaterQualityElement>(existingElementsDuringImport, newElementsDuringImport);
        }

        /// <summary>
        /// Removes irrelevant elements from the <paramref name="target"/>.
        /// </summary>
        /// <typeparam name="TWaterQualityElement">The type of element of the water quality model.</typeparam>
        /// <param name="target">The collection to remove elements from.</param>
        /// <param name="elementsToRemove">The collection of elements that should not be removed.</param>
        /// <param name="elementName">The name of the type of elements that are removed.</param>
        /// <remarks>
        /// The type constraint is currently loosely based on the <see cref="WaterQualitySubstance" />,
        /// <see cref="WaterQualitySubstance" />, <see cref="WaterQualityProcess" /> and <see cref="WaterQualityOutputParameter" />
        /// .
        /// Ideally speaking, a proper abstraction for these elements should be implemented to make this method more restrictive.
        /// </remarks>
        private void RemoveIrrelevantElements<TWaterQualityElement>(IEventedList<TWaterQualityElement> target, IEnumerable<TWaterQualityElement> elementsToRemove,
                                                                    string elementName)
            where TWaterQualityElement : Unique<long>, INameable, ICloneable
        {
            int nrOfElementsToRemove = elementsToRemove.Count();
            for (var i = 0; i < nrOfElementsToRemove; i++)
            {
                if (ShouldCancel)
                {
                    return;
                }

                UpdateProgress($"Removing irrelevant {elementName}.", i + 1, nrOfElementsToRemove);
                target.Remove(elementsToRemove.ElementAt(i));
            }
        }

        /// <summary>
        /// Adds new elements to the <paramref name="target"/>.
        /// </summary>
        /// <typeparam name="TWaterQualityElement">The type of element of the water quality model.</typeparam>
        /// <param name="target">The collection to add the elements to.</param>
        /// <param name="elementsToAdd">The elements that should be added.</param>
        /// <param name="elementName">The name of the type of elements that are added.</param>
        /// <remarks>
        /// The type constraint is currently loosely based on the <see cref="WaterQualitySubstance" />,
        /// <see cref="WaterQualitySubstance" />, <see cref="WaterQualityProcess" /> and <see cref="WaterQualityOutputParameter" />
        /// .
        /// Ideally speaking, a proper abstraction for these elements should be implemented to make this method more restrictive.
        /// </remarks>
        private void AddNewElements<TWaterQualityElement>(IEventedList<TWaterQualityElement> target, IEnumerable<TWaterQualityElement> elementsToAdd,
                                                          string elementName)
            where TWaterQualityElement : Unique<long>, INameable, ICloneable
        {
            var j = 0;
            int nrOfSubstancesToBeAdded = elementsToAdd.Count();
            foreach (TWaterQualityElement substance in elementsToAdd)
            {
                if (ShouldCancel)
                {
                    return;
                }

                UpdateProgress($"Importing new {elementName}.", j++, nrOfSubstancesToBeAdded);
                target.Add(substance);
            }
        }

        private void AddDefaultOutputParameters(IEventedList<WaterQualityOutputParameter> target)
        {
            // Add any default output parameter that is still missing
            var defaultOutputParameters = new[]
            {
                new WaterQualityOutputParameter
                {
                    Name = Resources.SubstanceProcessLibrary_OutputParameters_Volume,
                    Description = Resources.SubstanceProcessLibrary_OutputParameters_Volume_description
                },
                new WaterQualityOutputParameter
                {
                    Name = Resources.SubstanceProcessLibrary_OutputParameters_Surf,
                    Description = Resources.SubstanceProcessLibrary_OutputParameters_Surf_description
                },
                new WaterQualityOutputParameter
                {
                    Name = Resources.SubstanceProcessLibrary_OutputParameters_Temp,
                    Description = Resources.SubstanceProcessLibrary_OutputParameters_Temp_description
                },
                new WaterQualityOutputParameter
                {
                    Name = Resources.SubstanceProcessLibrary_OutputParameters_Rad,
                    Description = Resources.SubstanceProcessLibrary_OutputParameters_Rad_description
                }
            };

            // Filter the elements to add whether they are unique
            WaterQualityOutputParameter[] uniqueElementsToAdd = defaultOutputParameters.Where(element => !target.Any(item => Equals(item, element))).ToArray();
            AddNewElements(target, uniqueElementsToAdd, Resources.SubFileImporter_DefaultOutputParameters_Name);

        }

        private static WaterQualitySubstance GetSubstance(Match match)
        {
            return new WaterQualitySubstance
            {
                Name = match.Groups["Name"].Value,
                Active = match.Groups["Active"].Value.ToLower().StartsWith("active"),
                Description = match.Groups["Description"].Value,
                ConcentrationUnit = StripParentheses(match.Groups["ConcentrationUnit"].Value),
                WasteLoadUnit = StripParentheses(match.Groups["WasteLoadUnit"].Value)
            };
        }

        private static WaterQualityProcess GetProcess(Match match)
        {
            return new WaterQualityProcess
            {
                Name = match.Groups["Name"].Value,
                Description = match.Groups["Description"].Value
            };
        }

        private static WaterQualityParameter GetParameter(Match match)
        {
            double defaultValue;
            double.TryParse(match.Groups["Value"].Value, NumberStyles.Float, CultureInfo.InvariantCulture,
                            out defaultValue);

            return new WaterQualityParameter
            {
                Name = match.Groups["Name"].Value,
                Description = match.Groups["Description"].Value,
                Unit = StripParentheses(match.Groups["Unit"].Value),
                DefaultValue = defaultValue
            };
        }

        private static WaterQualityOutputParameter GetOutputParameter(Match match)
        {
            return new WaterQualityOutputParameter
            {
                Name = match.Groups["Name"].Value,
                Description = match.Groups["Description"].Value,
                ShowInHis = true,
                ShowInMap = true
            };
        }

        private void UpdateProgress(string phaseName, int currentStep, int maxStep)
        {
            if (ProgressChanged != null)
            {
                ProgressChanged(phaseName, currentStep, maxStep);
            }
        }

        private static bool IsExistingDefaultOutputParameter(WaterQualityOutputParameter waterQualityOutputParameter,
                                                             IEnumerable<WaterQualityOutputParameter> outputParameters)
        {
            return IsDefaultOutputParameter(waterQualityOutputParameter) &&
                   outputParameters.Any(op => op.Name == waterQualityOutputParameter.Name);
        }

        private static bool IsDefaultOutputParameter(WaterQualityOutputParameter waterQualityOutputParameter)
        {
            return waterQualityOutputParameter.Name == Resources.SubstanceProcessLibrary_OutputParameters_Volume ||
                   waterQualityOutputParameter.Name == Resources.SubstanceProcessLibrary_OutputParameters_Surf ||
                   waterQualityOutputParameter.Name == Resources.SubstanceProcessLibrary_OutputParameters_Temp ||
                   waterQualityOutputParameter.Name == Resources.SubstanceProcessLibrary_OutputParameters_Rad;
        }

        private static bool Equals(WaterQualitySubstance first, WaterQualitySubstance second)
        {
            if (!Equals(first.Name, second.Name))
            {
                return false;
            }

            if (!Equals(first.Description, second.Description))
            {
                return false;
            }

            if (!Equals(first.Active, second.Active))
            {
                return false;
            }

            if (!Equals(first.ConcentrationUnit, second.ConcentrationUnit))
            {
                return false;
            }

            if (!Equals(first.WasteLoadUnit, second.WasteLoadUnit))
            {
                return false;
            }

            return true;
        }

        private static bool Equals(WaterQualityProcess first, WaterQualityProcess second)
        {
            if (!Equals(first.Name, second.Name))
            {
                return false;
            }

            if (!Equals(first.Description, second.Description))
            {
                return false;
            }

            return true;
        }

        private static bool Equals(WaterQualityParameter first, WaterQualityParameter second)
        {
            if (!Equals(first.Name, second.Name))
            {
                return false;
            }

            if (!Equals(first.Description, second.Description))
            {
                return false;
            }

            if (!Equals(first.Unit, second.Unit))
            {
                return false;
            }

            return true;
        }

        private static bool Equals(WaterQualityOutputParameter first, WaterQualityOutputParameter second)
        {
            if (!Equals(first.Name, second.Name))
            {
                return false;
            }

            if (!Equals(first.Description, second.Description))
            {
                return false;
            }

            return true;
        }

        private static bool Equals(string first, string second)
        {
            if (first == null && second == null)
            {
                return true;
            }

            if (first == null)
            {
                return false;
            }

            return first.Equals(second, StringComparison.InvariantCultureIgnoreCase);
        }

        private static string StripParentheses(string unit)
        {
            if (unit.StartsWith("(") && unit.EndsWith(")"))
            {
                return unit.Substring(1, unit.Length - 2);
            }

            return unit;
        }

        /// <summary>
        /// Class to store the information about the imported elements.
        /// </summary>
        /// <typeparam name="TWaterQualityElement"> The type of water quality element it needs to store data from. </typeparam>
        private class ImportElementInformation<TWaterQualityElement>
            where TWaterQualityElement : Unique<long>, INameable, ICloneable
        {
            /// <summary>
            /// Creates a new instance of <see cref="ImportElementInformation{TWaterQualityElement}" />.
            /// </summary>
            /// <param name="existingElements"> The existing elements of the import. </param>
            /// <param name="newElements"> The new elements that are imported. </param>
            public ImportElementInformation(IEnumerable<TWaterQualityElement> existingElements, IEnumerable<TWaterQualityElement> newElements)
            {
                ExistingElements = existingElements;
                NewElements = newElements;
            }

            /// <summary>
            /// Gets the existing elements that were part of  the import.
            /// </summary>
            public IEnumerable<TWaterQualityElement> ExistingElements { get; }

            /// <summary>
            /// Gets the mew elements that were part of the import
            /// </summary>
            public IEnumerable<TWaterQualityElement> NewElements { get; }
        }
    }
}