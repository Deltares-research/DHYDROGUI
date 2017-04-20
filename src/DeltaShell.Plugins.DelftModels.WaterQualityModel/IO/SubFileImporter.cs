using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Shell.Core;
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

        public string Name
        {
            get { return "Substance Process Library"; }
        }

        public string Category { get; private set; }
        
        public Bitmap Image { get; private set; }

        public IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof(SubstanceProcessLibrary); }
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel
        {
            get { return false; }
        }

        public string FileFilter
        {
            get { return "Sub Files (*.sub)|*.sub"; }
        }

        public bool OpenViewAfterImport { get { return false; } }

        public string TargetDataDirectory { get; set; }
        
        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public object ImportItem(string path, object target)
        {
            Import(target as SubstanceProcessLibrary, path ?? DefaultFilePath);

            return target;
        }

        /// <summary>
        /// Default file path (used in <see cref="ImportItem"/> if no path parameter is provided)
        /// </summary>
        public string DefaultFilePath { private get; set; }

        /// <summary>
        /// Imports substance process library data from <paramref name="path"/> to <paramref name="substanceProcessLibrary"/>
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

            var subFileText = File.ReadAllText(path);

            if (!ShouldCancel) ImportSubstances(substanceProcessLibrary, subFileText);
            if (!ShouldCancel) ImportParameters(substanceProcessLibrary, subFileText);
            if (!ShouldCancel) ImportProcesses(substanceProcessLibrary, subFileText);
            if (!ShouldCancel) ImportOutputParameters(substanceProcessLibrary, subFileText);

            substanceProcessLibrary.Name = Path.GetFileNameWithoutExtension(path) + (ShouldCancel ? " (import cancelled)" : "");

            if (ShouldCancel)
            {
                Log.Warn("Sub file import cancelled");
            }
            else
            {
                Log.Info("Sub file successfully imported");
            }
        }

        private void ImportSubstances(SubstanceProcessLibrary substanceProcessLibrary, string subFileText)
        {
            const string substancePattern =
                @"substance\s*'(?<Name>" + RegularExpression.Characters + @")'\s*(?<Active>" + RegularExpression.Characters + @")\s*\n" +
                @"\s*description\s*'(?<Description>" + RegularExpression.ExtendedCharacters + @")'\s*\n" +
                @"\s*concentration-unit\s*'(?<ConcentrationUnit>" + UnitCharacters + @")'\s*\n" +
                @"\s*waste-load-unit\s*'(?<WasteLoadUnit>" + UnitCharacters + @")'\s*\n" +
                @"end-substance";

            var newSubstanceVariables = new Collection<WaterQualitySubstance>();
            var existingSubstanceVariables = new Collection<WaterQualitySubstance>();
            var substanceMatches = RegularExpression.GetMatches(substancePattern, subFileText);

            foreach (Match match in substanceMatches)
            {
                var newSubstanceVariable = GetSubstance(match);
                var existingSubstanceVariable = substanceProcessLibrary.Substances.FirstOrDefault(sv => Equals(sv, newSubstanceVariable));

                if (existingSubstanceVariable == null)
                {
                    newSubstanceVariables.Add(newSubstanceVariable);
                }
                else
                {
                    existingSubstanceVariables.Add(existingSubstanceVariable);
                }
            }

            // Remove all irrelevant substance variables
            var substancesToRemove = substanceProcessLibrary.Substances.Where(sub => !existingSubstanceVariables.Contains(sub)).ToArray();
            for (var i = 0; i < substancesToRemove.Length; i++)
            {
                if (ShouldCancel) return;
                UpdateProgress("Removing irrelevant substances", i+1, substancesToRemove.Length);

                substanceProcessLibrary.Substances.Remove(substancesToRemove[i]);
            }

            // Add all new substance variables
            for (var i = 0; i < newSubstanceVariables.Count; i++)
            {
                if (ShouldCancel) return;
                UpdateProgress("Importing new substances", i + 1, newSubstanceVariables.Count);
                substanceProcessLibrary.Substances.Add(newSubstanceVariables[i]);
            }
        }

        private void ImportParameters(SubstanceProcessLibrary substanceProcessLibrary, string subFileText)
        {
            const string parameterPattern = @"parameter\s*'(?<Name>" + RegularExpression.Characters + @")'\s*\n" +
                                            @"\s*description\s*'(?<Description>" + RegularExpression.ExtendedCharacters + @")'\s*\n" +
                                            @"\s*unit\s*'(?<Unit>" + UnitCharacters + @")'\s*\n" +
                                            @"\s*value\s*(?<Value>" + RegularExpression.Characters + @")\s*\n" +
                                            @"end-parameter";

            var newParameters = new Collection<WaterQualityParameter>();
            var existingParameters = new Collection<WaterQualityParameter>();
            var parameterMatches = RegularExpression.GetMatches(parameterPattern, subFileText);

            foreach (Match match in parameterMatches)
            {
                var newParameter = GetParameter(match);
                var existingParameter = substanceProcessLibrary.Parameters.FirstOrDefault(p => Equals(p, newParameter));

                if (existingParameter == null)
                {
                    newParameters.Add(newParameter);
                }
                else
                {
                    existingParameters.Add(existingParameter);
                }
            }

            // Remove all irrelevant parameters
            var parametersToRemove = substanceProcessLibrary.Parameters.Where(param => !existingParameters.Contains(param)).ToArray();
            for (var i = 0; i < parametersToRemove.Length; i++)
            {
                if (ShouldCancel) return;
                UpdateProgress("Removing irrelevant parameters", i + 1, parametersToRemove.Length);

                substanceProcessLibrary.Parameters.Remove(parametersToRemove[i]);
            }

            // Add all new parameters
            for (var i = 0; i < newParameters.Count; i++)
            {
                if (ShouldCancel) return;
                UpdateProgress("Importing new parameters", i + 1, newParameters.Count);
                substanceProcessLibrary.Parameters.Add(newParameters[i]);
            }
        }

        private void ImportProcesses(SubstanceProcessLibrary substanceProcessLibrary, string subFileText)
        {
            const string processesPattern = @"active-processes\s*\n(?<Processes>.*)end-active-processes";
            const string processPattern = @"\s*name\s*'(?<Name>" + RegularExpression.Characters + @")'\s*'(?<Description>" + RegularExpression.ExtendedCharacters + @")'\s*\n";

            var newProcesses = new Collection<WaterQualityProcess>();
            var existingProcesses = new Collection<WaterQualityProcess>();
            var processesMatches = RegularExpression.GetMatches(processesPattern, subFileText);

            if (processesMatches.Count > 0)
            {
                var processMatches = RegularExpression.GetMatches(processPattern, processesMatches[0].Groups["Processes"].Value);

                foreach (Match match in processMatches)
                {
                    var newProcess = GetProcess(match);
                    var existingProcess = substanceProcessLibrary.Processes.FirstOrDefault(p => Equals(p, newProcess));

                    if (existingProcess == null)
                    {
                        newProcesses.Add(newProcess);
                    }
                    else
                    {
                        existingProcesses.Add(existingProcess);
                    }
                }
            }

            // Remove all irrelevant processes
            var processesToRemove = substanceProcessLibrary.Processes.Where(proc => !existingProcesses.Contains(proc)).ToArray();
            for (var i = 0; i < processesToRemove.Length; i++)
            {
                if (ShouldCancel) return;
                UpdateProgress("Removing irrelevant processes", i + 1, processesToRemove.Length);

                substanceProcessLibrary.Processes.Remove(processesToRemove[i]);
            }

            // Add all new processes
            for (var i = 0; i < newProcesses.Count; i++)
            {
                if (ShouldCancel) return;
                UpdateProgress("Importing new processes", i + 1, newProcesses.Count);
                substanceProcessLibrary.Processes.Add(newProcesses[i]);
            }
        }

        private void ImportOutputParameters(SubstanceProcessLibrary substanceProcessLibrary, string subFileText)
        {
            const string outputParameterPattern = @"output\s*'(?<Name>" + RegularExpression.Characters + @")'\s*\n" +
                                                  @"\s*description\s*'(?<Description>" + RegularExpression.ExtendedCharacters + @")'\s*\n" +
                                                  @"end-output";

            var newOutputParameters = new Collection<WaterQualityOutputParameter>();
            var existingOutputParameters = new Collection<WaterQualityOutputParameter>();
            var outputParameterMatches = RegularExpression.GetMatches(outputParameterPattern, subFileText);

            foreach (Match match in outputParameterMatches)
            {
                var newOutputParameter = GetOutputParameter(match);
                if (IsExistingDefaultOutputParameter(newOutputParameter, substanceProcessLibrary.OutputParameters)) continue; // Skip any existing default output parameter
   
                var existingOutputParameter = substanceProcessLibrary.OutputParameters.FirstOrDefault(op => Equals(op, newOutputParameter));
                if (existingOutputParameter == null)
                {
                    newOutputParameters.Add(newOutputParameter);
                }
                else
                {
                    existingOutputParameters.Add(existingOutputParameter);
                }
            }

            // Remove all irrelevant output parameters
            var outputParametersToRemove =
                substanceProcessLibrary.OutputParameters.Where(
                    outputParameter =>
                    !IsDefaultOutputParameter(outputParameter) && !existingOutputParameters.Contains(outputParameter))
                                       .ToArray();
            for (var i = 0; i < outputParametersToRemove.Length; i++)
            {
                if (ShouldCancel) return;
                UpdateProgress("Removing irrelevant output parameters", i + 1, outputParametersToRemove.Length);

                substanceProcessLibrary.OutputParameters.Remove(outputParametersToRemove[i]);
            }

            // Add all new parameters
            for (var i = 0; i < newOutputParameters.Count; i++)
            {
                if (ShouldCancel) return;
                UpdateProgress("Importing new output parameters", i + 1, newOutputParameters.Count);
                substanceProcessLibrary.OutputParameters.Add(newOutputParameters[i]);
            }

            // Add any default output parameter that is still missing (Tuple: name, description)
            var defaultOutputParameters = new[]
                {
                    new Tuple<string, string>(Resources.SubstanceProcessLibrary_OutputParameters_Volume,
                                              Resources.SubstanceProcessLibrary_OutputParameters_Volume_description),
                    new Tuple<string, string>(Resources.SubstanceProcessLibrary_OutputParameters_Surf,
                                              Resources.SubstanceProcessLibrary_OutputParameters_Surf_description),
                    new Tuple<string, string>(Resources.SubstanceProcessLibrary_OutputParameters_Temp,
                                              Resources.SubstanceProcessLibrary_OutputParameters_Temp_description),
                    new Tuple<string, string>(Resources.SubstanceProcessLibrary_OutputParameters_Rad,
                                              Resources.SubstanceProcessLibrary_OutputParameters_Rad_description)
                };
            for (var i = 0; i < defaultOutputParameters.Length; i++)
            {
                if (ShouldCancel) return;
                UpdateProgress("Removing irrelevant output parameters", i + 1, defaultOutputParameters.Length);

                if (substanceProcessLibrary.OutputParameters.All(op => op.Name != defaultOutputParameters[i].Item1))
                {
                    substanceProcessLibrary.OutputParameters.Add(new WaterQualityOutputParameter
                    {
                        Name = defaultOutputParameters[i].Item1,
                        Description = defaultOutputParameters[i].Item2,
                    });
                }
            }
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
            double.TryParse(match.Groups["Value"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out defaultValue);

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
            if (ProgressChanged != null) ProgressChanged(phaseName, currentStep, maxStep);
        }

        private static bool IsExistingDefaultOutputParameter(WaterQualityOutputParameter waterQualityOutputParameter, IEnumerable<WaterQualityOutputParameter> outputParameters)
        {
            return IsDefaultOutputParameter(waterQualityOutputParameter) && outputParameters.Any(op => op.Name == waterQualityOutputParameter.Name);
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
            if (!Equals(first.Name, second.Name)) return false;
            if (!Equals(first.Description, second.Description)) return false;
            if (!Equals(first.Active, second.Active)) return false;
            if (!Equals(first.ConcentrationUnit, second.ConcentrationUnit)) return false;
            if (!Equals(first.WasteLoadUnit, second.WasteLoadUnit)) return false;            

            return true;
        }

        private static bool Equals(WaterQualityProcess first, WaterQualityProcess second)
        {
            if (!Equals(first.Name, second.Name)) return false;
            if (!Equals(first.Description, second.Description)) return false;

            return true;
        }

        private static bool Equals(WaterQualityParameter first, WaterQualityParameter second)
        {
            if (!Equals(first.Name, second.Name)) return false;
            if (!Equals(first.Description, second.Description)) return false;
            if (!Equals(first.Unit, second.Unit)) return false;

            return true;
        }

        private static bool Equals(WaterQualityOutputParameter first, WaterQualityOutputParameter second)
        {
            if (!Equals(first.Name, second.Name)) return false;
            if (!Equals(first.Description, second.Description)) return false;

            return true;
        }

        private static bool Equals(string first, string second)
        {
            if (first == null && second == null) return true;
            if (first == null) return false;
 
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
    }
}