using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers
{
    /// <summary>
    /// Reads data from the Sobek mdb file SETTINGS.DAT
    /// </summary>
    public class SobekCaseSettingsReader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekCaseSettingsReader));

        private string settingsFile;
        private IniData source;

        /// <summary>
        /// Reads the settings from the SETTINGS.DAT file into a <see cref="SobekCaseSettings"/> object
        /// </summary>
        /// <param name="path">Path of the SETTINGS.DAT file</param>
        /// <returns>A <see cref="SobekCaseSettings"/> object with the data from the SETTINGS.DAT file</returns>
        public SobekCaseSettings GetSobekCaseSettings(string path)
        {
            settingsFile = path;

            using (FileStream stream = File.OpenRead(path))
            {
                var iniParser = new IniParser { Configuration = { AllowPropertyKeysWithSpaces = true } };
                source = iniParser.Parse(stream);
            }

            var settings = new SobekCaseSettings
            {
                StartTime = new DateTime(),
                StopTime = new DateTime(),
                TimeStep = new TimeSpan()
            };

            var simulationSettings = new Dictionary<string, Action<string>>
            {
                // use -1 correction for year,month and day to correct for the default date 01-01-0001
                { "BeginYear", v => settings.StartTime = settings.StartTime.AddYears(ConvertStringTo<int>(v) - 1) },
                { "BeginMonth", v => settings.StartTime = settings.StartTime.AddMonths(ConvertStringTo<int>(v) - 1) },
                { "BeginDay", v => settings.StartTime = settings.StartTime.AddDays(ConvertStringTo<int>(v) - 1) },
                { "BeginHour", v => settings.StartTime = settings.StartTime.AddHours(ConvertStringTo<int>(v)) },
                { "BeginMinute", v => settings.StartTime = settings.StartTime.AddMinutes(ConvertStringTo<int>(v)) },
                { "BeginSecond", v => settings.StartTime = settings.StartTime.AddSeconds(ConvertStringTo<int>(v)) },
                { "EndYear", v => settings.StopTime = settings.StopTime.AddYears(ConvertStringTo<int>(v) - 1) },
                { "EndMonth", v => settings.StopTime = settings.StopTime.AddMonths(ConvertStringTo<int>(v) - 1) },
                { "EndDay", v => settings.StopTime = settings.StopTime.AddDays(ConvertStringTo<int>(v) - 1) },
                { "EndHour", v => settings.StopTime = settings.StopTime.AddHours(ConvertStringTo<int>(v)) },
                { "EndMinute", v => settings.StopTime = settings.StopTime.AddMinutes(ConvertStringTo<int>(v)) },
                { "EndSecond", v => settings.StopTime = settings.StopTime.AddSeconds(ConvertStringTo<int>(v)) },
                { "PeriodFromEvent", v => settings.PeriodFromEvent = ConvertStringTo<bool>(v) },
                { "LateralLocation", v => settings.LateralLocation = ConvertStringTo<int>(v) },
                { "NoNegativeQlatWhenThereIsNoWater", v => settings.NoNegativeQlatWhenThereIsNoWater = ConvertStringTo<bool>(v) },
                { "MaxLoweringCrossAtCulvert", v => settings.MaxLoweringCrossAtCulvert = ConvertStringTo<double>(v) },
            };

            var timeStepSettings = new Dictionary<string, Action<string>>
            {
                { "SobekDays", v => settings.TimeStep = settings.TimeStep.Add(new TimeSpan(ConvertStringTo<int>(v), 0, 0, 0)) },
                { "SobekHours", v => settings.TimeStep = settings.TimeStep.Add(new TimeSpan(0, ConvertStringTo<int>(v), 0, 0)) },
                { "SobekMinutes", v => settings.TimeStep = settings.TimeStep.Add(new TimeSpan(0, 0, ConvertStringTo<int>(v), 0)) },
                { "SobekSeconds", v => settings.TimeStep = settings.TimeStep.Add(new TimeSpan(0, 0, 0, ConvertStringTo<int>(v))) }
            };

            var generalSettings = new Dictionary<string, Action<string>>
            {
                { "NrOfTimesteps", v => settings.OutPutTimeStep = TimeSpan.FromMilliseconds(settings.TimeStep.TotalMilliseconds * ConvertStringTo<int>(v)) },
                { "ActualValue", v => settings.ActualValue = ConvertStringTo<bool>(v) },
                { "MeanValue", v => settings.MeanValue = ConvertStringTo<bool>(v) },
                { "MaximumValue", v => settings.MaximumValue = ConvertStringTo<bool>(v) }
            };

            var nodeSettings = new Dictionary<string, Action<string>>
            {
                { "LevelFromStreetLevel", v => settings.Freeboard = ConvertStringTo<bool>(v) },
                { "TotalArea", v => settings.TotalArea = ConvertStringTo<bool>(v) },
                { "TotalWidth", v => settings.TotalWidth = ConvertStringTo<bool>(v) },
                { "NodeVolume", v => settings.Volume = ConvertStringTo<bool>(v) },
                { "WaterDepth", v => settings.WaterDepth = ConvertStringTo<bool>(v) },
                { "WaterLevel", v => settings.WaterLevelOnResultsNodes = ConvertStringTo<bool>(v) },
                { "LateralOnNodes", v => settings.LateralOnNodes = ConvertStringTo<bool>(v) } // LateralOnNodes?? 
            };

            var branchesSettings = new Dictionary<string, Action<string>>
            {
                { "Chezy", v => settings.Chezy = ConvertStringTo<bool>(v) },
                { "Discharge", v => settings.DischargeOnResultsBranches = ConvertStringTo<bool>(v) },
                { "Froude", v => settings.Froude = ConvertStringTo<bool>(v) },
                { "SubSections", v => settings.RiverSubsectionParameters = ConvertStringTo<bool>(v) },
                { "Velocity", v => settings.VelocityOnResultsBranches = ConvertStringTo<bool>(v) },
                { "WaterLevelSlope", v => settings.WaterLevelSlope = ConvertStringTo<bool>(v) },
                { "Wind", v => settings.Wind = ConvertStringTo<bool>(v) }
            };

            var structureSettings = new Dictionary<string, Action<string>>
            {
                { "CrestLevel", v => settings.CrestLevel = ConvertStringTo<bool>(v) },
                { "OpeningsWidth", v => settings.CrestWidth = ConvertStringTo<bool>(v) },
                { "Discharge", v => settings.DischargeOnResultsStructures = ConvertStringTo<bool>(v) }, //twice?
                { "GateOpeningsLevel", v => settings.GateLowerEdgeLevel = ConvertStringTo<bool>(v) },
                { "StructHead", v => settings.Head = ConvertStringTo<bool>(v) },
                { "OpeningsArea", v => settings.OpeningsArea = ConvertStringTo<bool>(v) },
                { "PressureDifference", v => settings.PressureDifference = ConvertStringTo<bool>(v) },
                { "StructVelocity", v => settings.VelocityOnResultsStructures = ConvertStringTo<bool>(v) }, //twoce?
                { "WaterLevel", v => settings.WaterLevelOnResultsStructures = ConvertStringTo<bool>(v) },   //twoce?
                { "WaterlevelOnCrest", v => settings.WaterlevelOnCrest = ConvertStringTo<bool>(v) },
                { "CrestlevelOpeningsHeight", v => settings.CrestlevelOpeningsHeight = ConvertStringTo<bool>(v) }
            };

            var pumpSettings = new Dictionary<string, Action<string>> { { "PumpResults", v => settings.PumpResults = ConvertStringTo<bool>(v) } };

            var initialConditionsSettings = new Dictionary<string, Action<string>>
            {
                { "FromNetter", v => settings.FromNetter = ConvertStringTo<bool>(v) },
                { "FromValuesSelected", v => settings.FromValuesSelected = ConvertStringTo<bool>(v) },
                { "FromRestart", v => settings.FromRestart = ConvertStringTo<bool>(v) },
                { "InitialLevel", v => settings.InitialLevel = ConvertStringTo<bool>(v) },
                { "InitialDepth", v => settings.InitialDepth = ConvertStringTo<bool>(v) },
                { "EmptyWells", v => settings.InitialEmptyWells = ConvertStringTo<bool>(v) },
                { "InitialFlowValue", v => settings.InitialFlowValue = ConvertStringTo<double>(v) },
                { "InitialLevelValue", v => settings.InitialLevelValue = ConvertStringTo<double>(v) },
                { "InitialDepthValue", v => settings.InitialDepthValue = ConvertStringTo<double>(v) },
            };

            var flowParameterSettings = new Dictionary<string, Action<string>>
            {
                { "CourantNumber", v => settings.CourantNumber = ConvertStringTo<double>(v) },
                { "MaxDegree", v => settings.MaxDegree = ConvertStringTo<int>(v) },
                { "MaxIterations", v => settings.MaxIterations = ConvertStringTo<int>(v) },
                { "DtMinimum", v => settings.DtMinimum = ConvertStringTo<double>(v) },
                { "EpsilonValueVolume", v => settings.EpsilonValueVolume = ConvertStringTo<double>(v) },
                { "EpsilonValueWaterDepth", v => settings.EpsilonValueWaterDepth = ConvertStringTo<double>(v) },
                { "StructureDynamicsFactor", v => settings.StructureDynamicsFactor = ConvertStringTo<double>(v) },
                { "RelaxationFactor", v => settings.RelaxationFactor = ConvertStringTo<double>(v) },
                { "Rho", v => settings.Rho = ConvertStringTo<double>(v) },
                { "Gravity", v => settings.GravityAcceleration = ConvertStringTo<double>(v) },
                { "ThresholdValueFlooding", v => settings.ThresholdValueFlooding = ConvertStringTo<double>(v) },
                { "ThresholdValueFloodingFLS", v => settings.ThresholdValueFloodingFLS = ConvertStringTo<double>(v) },
                { "Theta", v => settings.Theta = ConvertStringTo<double>(v) },
                { "MinimumLength", v => settings.MinimumLength = ConvertStringTo<double>(v) },
                { "AccurateVersusSpeed", v => settings.AccurateVersusSpeed = ConvertStringTo<int>(v) },
                { "StructureInertiaDampingFactor", v => settings.StructureInertiaDampingFactor = ConvertStringTo<double>(v) },
                { "MinimumSurfaceinNode", v => settings.MinimumSurfaceinNode = ConvertStringTo<double>(v) },
                { "MinimumSurfaceatStreet", v => settings.MinimumSurfaceatStreet = ConvertStringTo<double>(v) },
                { "ExtraResistanceGeneralStructure", v => settings.ExtraResistanceGeneralStructure = ConvertStringTo<double>(v) },
                { "AccelerationTermFactor", v => settings.AccelerationTermFactor = ConvertStringTo<double>(v) },
                { "UseTimeStepReducerStructures", v => settings.UseTimeStepReducerStructures = ConvertStringTo<bool>(v) },
                { "Iadvec1D", v => settings.Iadvec1D = ConvertStringTo<double?>(v) },
                { "Limtyphu1D", v => settings.Limtyphu1D = ConvertStringTo<double?>(v) },
                { "Momdilution1D", v => settings.Momdilution1D = ConvertStringTo<double?>(v) }
            };

            var riverSettings = new Dictionary<string, Action<string>> { { "TransitionHeightSD", v => settings.TransitionHeightSummerDike = ConvertStringTo<double>(v) } };

            var waterQualitySettings = new Dictionary<string, Action<string>>
            {
                { "MeasurementFile", v => settings.MeasurementFile = v },
                { "Fraction", v => settings.Fraction = ConvertStringTo<bool>(v) },
                { "HistoryOutputInterval", v => settings.HistoryOutputInterval = ConvertStringTo<int>(v) },
                { "BalanceOutputInterval", v => settings.BalanceOutputInterval = ConvertStringTo<int>(v) },
                { "HisPeriodFromSimulation", v => settings.HisPeriodFromSimulation = ConvertStringTo<bool>(v) },
                { "BalPeriodFromSimulation", v => settings.BalPeriodFromSimulation = ConvertStringTo<bool>(v) },
                { "PeriodFromFlow", v => settings.PeriodFromFlow = ConvertStringTo<bool>(v) },
                { "ActiveProcess", v => settings.ActiveProcess = ConvertStringTo<bool>(v) },
                { "UseOldQuantityResults", v => settings.UseOldQuantityResults = ConvertStringTo<bool>(v) },
                { "LumpProcessesContributions", v => settings.LumpProcessesContributions = ConvertStringTo<bool>(v) },
                { "LumpBoundaryContributions", v => settings.LumpBoundaryContributions = ConvertStringTo<bool>(v) },
                { "SumOfMonitoringAreas", v => settings.SumOfMonitoringAreas = ConvertStringTo<bool>(v) },
                { "SuppressTimeDependentOutput", v => settings.SuppressTimeDependentOutput = ConvertStringTo<bool>(v) },
                { "LumpInternalTransport", v => settings.LumpInternalTransport = ConvertStringTo<bool>(v) },
                { "MapOutputInterval", v => settings.MapOutputInterval = ConvertStringTo<int>(v) },
                { "MapPeriodFromSimulation", v => settings.MapPeriodFromSimulation = ConvertStringTo<bool>(v) },
                { "OutputLocationsType", v => settings.OutputLocationsType = ConvertStringTo<int>(v) },
                { "OutputHisVarType", v => settings.OutputHisVarType = ConvertStringTo<int>(v) },
                { "OutputHisMapType", v => settings.OutputHisMapType = ConvertStringTo<int>(v) },
                { "SubstateFile", v => settings.SubstateFile = v },
                { "SubstateFileOption", v => settings.SubstateFileOption = ConvertStringTo<int>(v) },
                { "UseTatcherHarlemanTimeLag", v => settings.UseTatcherHarlemanTimeLag = ConvertStringTo<bool>(v) },
                { "TatcherHarlemanTimeLag", v => settings.TatcherHarlemanTimeLag = ConvertStringTo<int>(v) }
            };

            SetIniValues("Simulation", simulationSettings);
            SetIniValues("Timesteps", timeStepSettings);
            SetIniValues("ResultsGeneral", generalSettings);
            SetIniValues("ResultsNodes", nodeSettings);
            SetIniValues("ResultsBranches", branchesSettings);
            SetIniValues("ResultsStructures", structureSettings);

            // Test Check Presence of Category, If not Assignment of presentKeys fails
            if (source.ContainsSection("ResultsPumps"))
            {
                SetIniValues("ResultsPumps", pumpSettings);
            }

            SetIniValues("InitialConditions", initialConditionsSettings);
            SetIniValues("Flow Parameters", flowParameterSettings);
            SetIniValues("River Options", riverSettings);
            SetIniValues("Water Quality", waterQualitySettings);

            return settings;
        }

        private void SetIniValues(string sectionName, Dictionary<string, Action<string>> settings)
        {
            IniSection section = source.FindSection(sectionName);
            if (section == null)
            {
                log.WarnFormat("Case settings for category [{0}] can not be found", sectionName);
                return;
            }

            List<string> presentKeys = settings.Keys.Where(section.ContainsProperty).ToList();
            List<string> missingKeys = settings.Keys.Except(presentKeys).ToList();

            foreach (string key in missingKeys)
            {
                log.Warn($"Settings file {settingsFile} does not contain case setting {key}.");
            }

            foreach (string key in presentKeys)
            {
                settings[key](section.GetPropertyValue(key));
            }
        }

        private static T ConvertStringTo<T>(string valueString)
        {
            Type type = typeof(T);

            if (type == typeof(double))
            {
                return (T)(object)double.Parse(valueString, CultureInfo.InvariantCulture);
            }

            if (type == typeof(double?))
            {
                if (!string.IsNullOrEmpty(valueString) && valueString.Trim().Length > 0)
                {
                    return (T)(object)double.Parse(valueString, CultureInfo.InvariantCulture);
                }

                return (T)(object)null;
            }

            if (type == typeof(int))
            {
                return (T)(object)Convert.ToInt32(valueString);
            }

            if (type == typeof(bool))
            {
                // -1 == true for boolean sobek settings
                return (T)(object)(Convert.ToInt32(valueString) == -1);
            }

            throw new NotImplementedException();
        }
    }
}