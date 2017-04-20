using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DelftTools.Functions;
using DelftTools.Functions.Generic;

using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extentions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Model;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    public abstract class IncludeFileFactoryBase
    {
        #region Block 1

        /// <summary>
        /// Create the T0 include content.
        /// The startTime could be the start time of the model,
        /// or the conversion-ref-time from the hyd-file.
        /// </summary>
        public string CreateT0Include(DateTime referenceTime)
        {
            return string.Format("'T0: {0}  (scu=       1s)'", referenceTime.ToString("yyyy.MM.dd HH:mm:ss",
                CultureInfo.InvariantCulture));
        }
        
        /// <summary>
        /// Create the list of substances with active and passive substances.
        /// </summary>
        public string CreateSubstanceListInclude(SubstanceProcessLibrary substanceProcessLibrary)
        {
            var activeSubstances = substanceProcessLibrary.ActiveSubstances.ToList();
            var inActiveSubstances = substanceProcessLibrary.InActiveSubstances.ToList();

            using (var writer = new StringWriter(new StringBuilder()))
            {
                writer.WriteLine("; number of active and inactive substances");
                writer.WriteLine("{0}             {1}",
                    activeSubstances.Count, inActiveSubstances.Count);

                WriteSubstanceList(writer, activeSubstances, 1, "active substances");
                WriteSubstanceList(writer, inActiveSubstances, activeSubstances.Count + 1, "passive substances");

                return writer.ToString();
            }
        }

        private void WriteSubstanceList(StringWriter writer, IList<WaterQualitySubstance> substances, int startingSubstanceCount, string comment)
        {
            writer.WriteLine("        ; {0}", comment);

            for (int index = 0; index < substances.Count; index++)
            {
                var substance = substances[index];

                writer.WriteLine("{0}            '{1}' ;{2}",
                    index + startingSubstanceCount,
                    substance.Name,
                    substance.Description);
            }
        }

        #endregion Block 1
        #region Block 2

        /// <summary>
        /// Write the general waq model settings for the run.
        /// </summary>
        public string CreateNumSettingsInclude(WaterQualityModelSettings waqSettings)
        {
            var integrationOptions = waqSettings.NoDispersionIfFlowIsZero ? 1 : 0;
            integrationOptions += waqSettings.NoDispersionOverOpenBoundaries ? 2 : 0;
            integrationOptions += waqSettings.UseFirstOrder ? 0 : 4;

            using (var writer = new StringWriter(new StringBuilder()))
            {
                writer.WriteLine("{0}.{1}{2} ; integration option",
                    (int)waqSettings.NumericalScheme,
                    integrationOptions, waqSettings.Balance ? 3 : 0);

                writer.WriteLine("; detailed balance options");
                if (waqSettings.Balance)
                {
                    if (waqSettings.BalanceUnit != BalanceUnit.Gram)
                    {
                        writer.WriteLine(waqSettings.BalanceUnit == BalanceUnit.GramPerSquareMeter
                            ? "BAL_UNITAREA"
                            : "BAL_UNITVOLUME");
                    }
                    writer.WriteLine("{0} {1} {2}",
                        waqSettings.LumpProcesses ? "BAL_LUMPPROCESSES" : "BAL_NOLUMPPROCESSES",
                        waqSettings.LumpTransport ? "BAL_LUMPTRANSPORT" : "BAL_NOLUMPTRANSPORT",
                        waqSettings.LumpLoads ? "BAL_LUMPLOADS" : "BAL_NOLUMPLOADS");

                    writer.WriteLine("{0} {1}",
                        waqSettings.SuppressSpace ? "BAL_SUPPRESSSPACE" : "BAL_NOSUPPRESSSPACE",
                        waqSettings.SuppressTime ? "BAL_SUPPRESSTIME" : "BAL_NOSUPPRESSTIME");
                }

                return writer.ToString();
            }
        }

        /// <summary>
        /// Create the include file with output timers.
        /// </summary>
        public string CreateOutputTimersInclude(WaterQualityModelSettings waqSettings)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                writer.WriteLine("; output control (see DELWAQ-manual)");
                writer.WriteLine("; yyyy/mm/dd-hh:mm:ss  yyyy/mm/dd-hh:mm:ss  dddhhmmss");
                writer.WriteLine("{0} for balance output", CreateDelwaqTimeSettingsInputString(waqSettings.BalanceStartTime, waqSettings.BalanceStopTime, waqSettings.BalanceTimeStep));
                writer.WriteLine("{0} for map output", CreateDelwaqTimeSettingsInputString(waqSettings.MapStartTime, waqSettings.MapStopTime, waqSettings.MapTimeStep));
                writer.WriteLine("{0} for his output", CreateDelwaqTimeSettingsInputString(waqSettings.HisStartTime, waqSettings.HisStopTime, waqSettings.HisTimeStep));

                return writer.ToString();
            }
        }

        /// <summary>
        /// Create the include file with the simulation time from the model.
        /// Start time, stop time, time step.
        /// </summary>
        public string CreateSimTimersInclude(WaqInitializationSettings initializationSettings)
        {
            return CreateDelwaqTimeSettingsInputString(initializationSettings.SimulationStartTime, initializationSettings.SimulationStopTime, initializationSettings.SimulationTimeStep, true);
        }

        /// <summary>
        /// Creates a formatted string based on a <see cref="startTime"/>, a <see cref="stopTime"/> and a <see cref="timeStep"/>
        /// </summary>
        /// <param name="startTime">The start time</param>
        /// <param name="stopTime">The stop time</param>
        /// <param name="timeStep">The time step</param>
        /// <param name="addEndLineCharacters">Whether or not to add end line characters (\n) after the parameters (also whether or not to add a timestep constant)</param>
        private string CreateDelwaqTimeSettingsInputString(DateTime startTime, DateTime stopTime, TimeSpan timeStep, bool addEndLineCharacters = false)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                if (addEndLineCharacters)
                {
                    writer.WriteLine("  {0} ; start time", DateTimeToString(startTime));
                    writer.WriteLine("  {0} ; stop time", DateTimeToString(stopTime));
                    writer.WriteLine("  0 ; timestep constant");
                    writer.Write("  {0} ; timestep", FormatTimeStep(timeStep));
                }
                else
                {
                    writer.Write("  {0}  {1}  {2} ;  start, stop and step",
                        DateTimeToString(startTime), DateTimeToString(stopTime), FormatTimeStep(timeStep));
                }

                return writer.ToString();
            }
        }

        private string DateTimeToString(DateTime dateTime)
        {
            return dateTime.ToString("yyyy/MM/dd-HH:mm:ss", CultureInfo.InvariantCulture);
        }

        private string FormatTimeStep(TimeSpan timeStep)
        {
            return timeStep.Days.ToString("000") + timeStep.Hours.ToString("00") + 
                   timeStep.Minutes.ToString("00") + timeStep.Seconds.ToString("00");
        }

        #endregion Block 2
        #region Block 7

        /// <summary>
        /// Creates the processes include file contents, stating which processes should be enabled.
        /// </summary>
        public string CreateProcessesInclude(SubstanceProcessLibrary substanceProcessLibrary)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                foreach (var waterQualityProcess in substanceProcessLibrary.Processes)
                {
                    writer.WriteLine("CONSTANTS 'ACTIVE_{0}' DATA 0", waterQualityProcess.Name);
                }
                return writer.ToString();
            }
        }

        protected static void WriteConstant(StringWriter writer, IFunction meteoParameter)
        {
            var defaultValue = WaterQualityFunctionFactory.GetDefaultValue(meteoParameter);

            writer.WriteLine("CONSTANTS '{0}' DATA {1}", 
                meteoParameter.Name,
                defaultValue.ToString(CultureInfo.InvariantCulture));
        }

        protected static void WriteTimeSeries(StringWriter writer, IFunction timeDependentFunction)
        {
            var timeVariable = timeDependentFunction.Arguments[0];
            var valueVariable = timeDependentFunction.Components[0];

            writer.WriteLine("FUNCTIONS");
            writer.WriteLine(timeDependentFunction.Name);
            writer.WriteLine(timeVariable.InterpolationType == InterpolationType.Linear
                ? "LINEAR DATA"
                : "DATA");

            for (var i = 0; i < timeVariable.Values.Count; i++)
            {
                writer.WriteLine("{0} {1}",
                    ((DateTime)timeVariable.Values[i]).ToString("yyyy/MM/dd-HH:mm:ss", CultureInfo.InvariantCulture),
                    ((double)valueVariable.Values[i]).ToString(CultureInfo.InvariantCulture));
            }
            writer.WriteLine();
        }

        /// <summary>
        /// Creates the constants include file contents, stating all constant parameter values.
        /// </summary>
        public string CreateConstantsInclude(IEnumerable<IFunction> processCoefficients)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                foreach (var constantProcessCoefficient in processCoefficients.Where(pc => pc.IsConst()))
                {
                    WriteConstant(writer, constantProcessCoefficient);
                }

                return writer.ToString();
            }
        }

        /// <summary>
        /// Creates the functions include file contents, stating all time-dependent parameter values.
        /// </summary>
        public string CreateFunctionsInclude(IEnumerable<IFunction> processCoefficients)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                foreach (var timeDependentProcessCoefficient in processCoefficients.Where(pc => pc.IsTimeSeries()))
                {
                    WriteTimeSeries(writer, timeDependentProcessCoefficient);
                }

                return writer.ToString();
            }
        }

        /// <summary>
        /// Creates the spatial process parameters include file contents.
        /// </summary>
        public abstract string CreateParametersInclude(WaqInitializationSettings initializationSettings);

        #endregion Block 7
        #region Block 8

        /// <summary>
        /// Creates the include file contents for initial conditions (constant and spatial).
        /// </summary>
        public string CreateInitialConditionsInclude(WaqInitializationSettings initializationSettings)
        {
            if (!HasInitialConditions(initializationSettings))
            {
                return "";
            }

            return CreateConstantInitialConditionsFileContents(initializationSettings) +
                   CreateSpatialInitialConditionsFileContents(initializationSettings);
        }

        private static string CreateConstantInitialConditionsFileContents(WaqInitializationSettings initializationSettings)
        {
            using (var writer = new StringWriter(new StringBuilder()))
            {
                writer.WriteLine("MASS/M2");

                var constantInitialConditions = initializationSettings.InitialConditions.Where(ic => ic.IsConst()).ToArray();
                if (constantInitialConditions.Length > 0)
                {
                    writer.WriteLine("INITIALS");
                    writer.WriteLine(string.Join(" ", 
                        constantInitialConditions.Select(ic => string.Format("'{0}'", ic.Name)))); // "'NH4' 'CBOD5' 'CBOD5_2' 'OXY'"
                    writer.WriteLine("DEFAULTS");
                    writer.WriteLine(string.Join(" ",
                        constantInitialConditions.Select(
                            ic => WaterQualityFunctionFactory.GetDefaultValue(ic).ToString(CultureInfo.InvariantCulture)))); // "1.2 2.3 3.4 4.5"
                }
                return writer.ToString();
            }
        }

        /// <summary>
        /// Determines whether there are initial conditions available or not.
        /// </summary>
        protected virtual bool HasInitialConditions(WaqInitializationSettings initializationSettings)
        {
            return initializationSettings.InitialConditions != null && 
                initializationSettings.InitialConditions.Count > 0;
        }

        /// <summary>
        /// Creates the initial conditions file contents with spatial components.
        /// </summary>
        protected abstract string CreateSpatialInitialConditionsFileContents(WaqInitializationSettings initializationSettings);

        #endregion Block 8
        #region Block 9

        /// <summary>
        /// Creates the list of his variables to include in the output parameters.
        /// </summary>
        public string CreateHisVarInclude(SubstanceProcessLibrary substanceProcessLibrary)
        {
            var hisOutputParameters = substanceProcessLibrary.OutputParameters.Where(op => op.ShowInHis).ToList();
            return WriteOutputIncludeParameters(hisOutputParameters, true);
        }

        /// <summary>
        /// Creates the list of map variables to include in the output parameters.
        /// </summary>
        public string CreateMapVarInclude(SubstanceProcessLibrary substanceProcessLibrary)
        {
            var mapOutputParameters = substanceProcessLibrary.OutputParameters.Where(op => op.ShowInMap).ToList();
            return WriteOutputIncludeParameters(mapOutputParameters, false);
        }

        /// <summary>
        /// Write an output parameter include.
        /// Starts with a 2, because this are additional parameters to the default parameter.
        /// The second nummer is the number of items listed.
        /// Then a list of parameters is defined between quotes. If <paramref name="addParameterType"/> is true, a second column is written with 'volume'.
        /// </summary>
        /// <param name="outputParameters">The output parameters.</param>
        /// <param name="addParameterType">If the parameter type should be included as a second column. 'volume' or ' '</param>
        private static string WriteOutputIncludeParameters(ICollection<WaterQualityOutputParameter> outputParameters, bool addParameterType)
        {
            using (StringWriter writer = new StringWriter(new StringBuilder()))
            {
                writer.WriteLine("2 ; perform default output and extra parameters listed below");
                writer.WriteLine("{0} ; number of parameters listed", outputParameters.Count);

                foreach (var parameter in outputParameters)
                {
                    if (addParameterType)
                    {
                        string parameterType = parameter.Name == "Volume" || parameter.Name == "Surf" ? " " : "volume";
                        writer.WriteLine(" '{0}' '{1}'", parameter.Name, parameterType);
                    }
                    else
                    {
                        writer.WriteLine(" '{0}'", parameter.Name);
                    }
                }

                return writer.ToString();
            }
        }

        #endregion Block 9
    }
}