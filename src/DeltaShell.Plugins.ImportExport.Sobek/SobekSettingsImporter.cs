using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    public class SobekSettingsImporter : PartialSobekImporterBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekSettingsImporter));

        private SobekCaseSettings sobekCaseSettings;
        private WaterFlowFMModel waterFlowFMModel;
        
        public override string DisplayName
        {
            get { return "Model and case settings"; }
        }

        public override SobekImporterCategories Category { get; } = SobekImporterCategories.WaterFlow1D;

        protected override void PartialImport()
        {
            Log.DebugFormat("Importing model settings ...");

            waterFlowFMModel = GetModel<WaterFlowFMModel>();
            
            if (SobekType == DeltaShell.Sobek.Readers.SobekType.Sobek212)
            {
                ImportSobek212Settings();
            }
            else
            {
                ImportSobekReSettings();
                SetSobekReSettingsToFeatures();
            }

            ImportCaseSettingsFile();
            if (sobekCaseSettings != null)
            {
                // This happens in a few tests. 
                SetOutputSettings();
            }
        }

        private void ImportSobek212Settings()
        {
            var path = GetFilePath(SobekFileNames.SobekCaseSettingFileName);

            try
            {
                if (!File.Exists(path))
                {
                    Log.WarnFormat("Import of case settings skipped, file {0} does not exist.", path);
                    return;
                }
                sobekCaseSettings = SobekCaseSettingsReader.GetSobekCaseSettings(path);

                SetModelParameters();
            }
            catch (Exception exception)
            {
                Log.ErrorFormat("Error reading case settings {0}; reason {1}", path, exception.Message);
            }
        }

        private void ImportSobekReSettings()
        {
            var path = "";
            try
            {
                path = GetFilePath("DEFRUN.1");
                sobekCaseSettings = new SobekReDefRun1Reader().Read(path).First();
                waterFlowFMModel.StartTime = sobekCaseSettings.StartTime;
                waterFlowFMModel.StopTime = sobekCaseSettings.StopTime;
                waterFlowFMModel.TimeStep = sobekCaseSettings.TimeStep;
                waterFlowFMModel.OutputTimeStep = sobekCaseSettings.OutPutTimeStep;

                path = GetFilePath("DEFRUN.2");
                var sobekReDefRun2Reader = new SobekReDefRun2Reader {SobekCaseSettingsInstance = sobekCaseSettings};
                sobekReDefRun2Reader.Read(path).First();
                sobekCaseSettings = sobekReDefRun2Reader.SobekCaseSettingsInstance;
            }
            catch (Exception exception)
            {
                Log.ErrorFormat("Error reading case settings {0}; reason {1}", path, exception.Message);
            }
        }

        private void SetSobekReSettingsToFeatures()
        {
            foreach (var extraResistance in HydroNetwork.ExtraResistances)
            {
                extraResistance.ExtraResistanceType = 0;
            }
        }

        private void ImportCaseSettingsFile()
        {
            SobekCaseData sobekCaseData = GetSobekCaseData();
            if (sobekCaseData.WindDataPath == null)
            {
                Log.Error("No wind data available.");
                return;
            }
        }

        private SobekCaseData GetSobekCaseData()
        {
            string caseDataPath = GetFilePath(SobekFileNames.SobekCaseDescriptionFileName);
            if (!File.Exists(caseDataPath))
            {
                Log.WarnFormat("Sobek case data file [{0}] not found; skipping...", caseDataPath);
                return new SobekCaseData();
            }

            using (var stream = new FileStream(caseDataPath, FileMode.Open))
            {
                return SobekCaseDataReader.Read(stream, caseDataPath);
            }
        }

        private IEnumerable<DateTime> ReadMeasurementTimesFromBuiFile()
        {
            var sobekCaseData = GetSobekCaseData();
            if (sobekCaseData.BuiDataPath == null)
            {
                Log.Error("No precipitation data available.");
                return new List<DateTime>();
            }
            var buiFileReader = new SobekRRBuiFileReader();
            return buiFileReader.ReadBuiHeaderData(sobekCaseData.BuiDataPath) ? buiFileReader.MeasurementTimes : new List<DateTime>();
        }

        private void SetModelParameters()
        {
            // Simulation
            if (sobekCaseSettings.PeriodFromEvent)
            {
                DateTime startTime;
                DateTime stopTime;
                SobekMeteoDataImporterHelper.ReadTimersFromMeteo(ReadMeasurementTimesFromBuiFile().ToList(),
                                                                 sobekCaseSettings.StartTime,
                                                                 sobekCaseSettings.StopTime, out startTime,
                                                                 out stopTime);
                sobekCaseSettings.StartTime = startTime;
                sobekCaseSettings.StopTime = stopTime;
            }

            waterFlowFMModel.StartTime = sobekCaseSettings.StartTime;
            waterFlowFMModel.ReferenceTime = sobekCaseSettings.StartTime;
            waterFlowFMModel.StopTime = sobekCaseSettings.StopTime;
            waterFlowFMModel.TimeStep = sobekCaseSettings.TimeStep;
            waterFlowFMModel.OutputTimeStep = sobekCaseSettings.OutPutTimeStep;
            waterFlowFMModel.ModelDefinition.GetModelProperty(KnownProperties.DtMax).Value = sobekCaseSettings.TimeStep.TotalSeconds;
            waterFlowFMModel.ModelDefinition.SetModelProperty(GuiProperties.HisOutputDeltaT, sobekCaseSettings.OutPutTimeStep.TotalSeconds.ToString(CultureInfo.InvariantCulture));
            /*
            SetCaseSettingsToParameterSettings("LateralLocation", sobekCaseSettings.LateralLocation);
            SetCaseSettingsToParameterSettings("NoNegativeQlatWhenThereIsNoWater", sobekCaseSettings.NoNegativeQlatWhenThereIsNoWater);
            SetCaseSettingsToParameterSettings("MaxLoweringCrossAtCulvert", sobekCaseSettings.MaxLoweringCrossAtCulvert);

            // Initial Conditions
            SetCaseSettingsToParameterSettings("InitialEmptyWells", sobekCaseSettings.InitialEmptyWells);

            // Flow Parameters
            SetCaseSettingsToParameterSettings("CourantNumber", sobekCaseSettings.CourantNumber);
            SetCaseSettingsToParameterSettings("MaxDegree", sobekCaseSettings.MaxDegree);
            SetCaseSettingsToParameterSettings("MaxIterations", sobekCaseSettings.MaxIterations);
            SetCaseSettingsToParameterSettings("DtMinimum", sobekCaseSettings.DtMinimum);
            SetCaseSettingsToParameterSettings("EpsilonValueVolume", sobekCaseSettings.EpsilonValueVolume);
            SetCaseSettingsToParameterSettings("EpsilonValueWaterDepth", sobekCaseSettings.EpsilonValueWaterDepth);
            SetCaseSettingsToParameterSettings("StructureDynamicsFactor", sobekCaseSettings.StructureDynamicsFactor);
            SetCaseSettingsToParameterSettings("RelaxationFactor", sobekCaseSettings.RelaxationFactor);
            SetCaseSettingsToParameterSettings("Rho", sobekCaseSettings.Rho);
            SetCaseSettingsToParameterSettings("ThresholdValueFlooding", sobekCaseSettings.ThresholdValueFlooding);
            SetCaseSettingsToParameterSettings("ThresholdValueFloodingFLS", sobekCaseSettings.ThresholdValueFloodingFLS);
            SetCaseSettingsToParameterSettings("Theta", sobekCaseSettings.Theta);
            SetCaseSettingsToParameterSettings("MinimumLength", sobekCaseSettings.MinimumLength);
            SetCaseSettingsToParameterSettings("AccurateVersusSpeed", sobekCaseSettings.AccurateVersusSpeed);
            SetCaseSettingsToParameterSettings("StructureInertiaDampingFactor", sobekCaseSettings.StructureInertiaDampingFactor);
            SetCaseSettingsToParameterSettings("MinimumSurfaceinNode", sobekCaseSettings.MinimumSurfaceinNode);
            SetCaseSettingsToParameterSettings("MinimumSurfaceatStreet", sobekCaseSettings.MinimumSurfaceatStreet);
            SetCaseSettingsToParameterSettings("ExtraResistanceGeneralStructure", sobekCaseSettings.ExtraResistanceGeneralStructure);
            SetCaseSettingsToParameterSettings("AccelerationTermFactor", sobekCaseSettings.AccelerationTermFactor);
            SetCaseSettingsToParameterSettings("UseTimeStepReducerStructures", sobekCaseSettings.UseTimeStepReducerStructures);

            if (sobekCaseSettings.Iadvec1D != null)
            {
                SetCaseSettingsToParameterSettings("Iadvec1D", sobekCaseSettings.Iadvec1D);
            }
            if (sobekCaseSettings.Limtyphu1D != null)
            {
                SetCaseSettingsToParameterSettings("Limtyphu1D", sobekCaseSettings.Limtyphu1D);
            }
            if (sobekCaseSettings.Momdilution1D != null)
            {
                SetCaseSettingsToParameterSettings("Momdilution1D", sobekCaseSettings.Momdilution1D);
            }
            */
            // RiverOptions
            //SetCaseSettingsToParameterSettings("TransitionHeightSD", ParameterCategory.AdvancedOptions, sobekCaseSettings.TransitionHeightSummerDike);
        }

        /// <summary>
        /// Use:
        ///  SetCaseSettingsToParameterSettings(Name, ParameterCategory, Value);
        /// or 
        ///  SetCaseSettingsToParameterSettings(Name, Value);
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="p"></param>
        /*private void SetCaseSettingsToParameterSettings(string parameterName, params object[] p)
        {
            var parameterCategory = (ParameterCategory?)((p.Length == 2) ? p[0] : null);
            var valueIndex = (p.Length == 2) ? 1 : 0;
            if (p[valueIndex] is double)
            {
                SetCaseSettingsToParameterSettings(parameterName, parameterCategory,
                                                   ((double)p[valueIndex]).ToString(CultureInfo.InvariantCulture));
            }
            else if (p[valueIndex] is bool)
            {
                SetCaseSettingsToParameterSettings(parameterName, parameterCategory,
                                                   ((bool)p[valueIndex]).ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                SetCaseSettingsToParameterSettings(parameterName, parameterCategory, p[valueIndex].ToString());
            }
        }

        private void SetCaseSettingsToParameterSettings(string parameterName, ParameterCategory? parameterCategory, string value)
        {
            ModelApiParameter parameter = waterFlowModel1D.GetModelApiParameter(parameterName, parameterCategory);
            if (parameter != null)
            {
                parameter.Value = value;
            }
            else
            {
                Log.Warn(String.Format("Case setting {0} can not be imported, model parameter setting not available.",
                                       parameterName));
            }
        }*/

        private void SetOutputSettings()
        {
            /* 
             * This mapping is based on the parameters.xml file. It's imperfect, but as good as it gets: not all 
             * parameters in the SOBEK2 settings.dat file can be converted to an output setting in SOBEK3, and 
             * not all output settings in SOBEK3 have an equivalent in the SOBEK2 settings.dat file.
             */

            // Grid points
            // No salt options are found in the sobekCaseSettings yet. 
            ConditionallyAddOutputSetting(sobekCaseSettings.TotalArea, QuantityType.TotalArea, ElementSet.GridpointsOnBranches);
            ConditionallyAddOutputSetting(sobekCaseSettings.TotalWidth, QuantityType.TotalWidth, ElementSet.GridpointsOnBranches);
            ConditionallyAddOutputSetting(sobekCaseSettings.Volume, QuantityType.Volume, ElementSet.GridpointsOnBranches);
            ConditionallyAddOutputSetting(sobekCaseSettings.WaterDepth, QuantityType.WaterDepth, ElementSet.GridpointsOnBranches);
            ConditionallyAddOutputSetting(sobekCaseSettings.WaterLevelOnResultsNodes, QuantityType.WaterLevel, ElementSet.GridpointsOnBranches);
            ConditionallyAddOutputSetting(sobekCaseSettings.LateralOnNodes, QuantityType.LateralAtNodes, ElementSet.GridpointsOnBranches);

            // Branches
            // No dispersion is found in the sobekCaseSettings yet. 
            ConditionallyAddOutputSetting(sobekCaseSettings.Chezy, QuantityType.FlowConv, ElementSet.ReachSegElmSet);    // Chezy in SOBEK 2 settings.dat also means outputting Conveyance
            ConditionallyAddOutputSetting(sobekCaseSettings.Chezy, QuantityType.FlowChezy, ElementSet.ReachSegElmSet);
            ConditionallyAddOutputSetting(sobekCaseSettings.DischargeOnResultsBranches, QuantityType.Discharge, ElementSet.ReachSegElmSet);
            ConditionallyAddOutputSetting(sobekCaseSettings.Froude, QuantityType.Froude, ElementSet.ReachSegElmSet);
            ConditionallyAddOutputSetting(sobekCaseSettings.VelocityOnResultsBranches, QuantityType.Velocity, ElementSet.ReachSegElmSet);
            ConditionallyAddOutputSetting(sobekCaseSettings.WaterLevelSlope, QuantityType.WaterLevelGradient, ElementSet.ReachSegElmSet);

            // Structures
            ConditionallyAddOutputSetting(sobekCaseSettings.CrestLevel, QuantityType.CrestLevel, ElementSet.Structures);
            ConditionallyAddOutputSetting(sobekCaseSettings.CrestWidth, QuantityType.CrestWidth, ElementSet.Structures);
            ConditionallyAddOutputSetting(sobekCaseSettings.DischargeOnResultsStructures, QuantityType.Discharge, ElementSet.Structures);
            ConditionallyAddOutputSetting(sobekCaseSettings.GateLowerEdgeLevel, QuantityType.GateLowerEdgeLevel, ElementSet.Structures);
            ConditionallyAddOutputSetting(sobekCaseSettings.Head, QuantityType.Head, ElementSet.Structures);
            ConditionallyAddOutputSetting(sobekCaseSettings.OpeningsArea, QuantityType.FlowArea, ElementSet.Structures);
            ConditionallyAddOutputSetting(sobekCaseSettings.PressureDifference, QuantityType.PressureDifference, ElementSet.Structures);
            ConditionallyAddOutputSetting(sobekCaseSettings.VelocityOnResultsStructures, QuantityType.Velocity, ElementSet.Structures);
            ConditionallyAddOutputSetting(sobekCaseSettings.WaterLevelOnResultsStructures, QuantityType.WaterlevelUp, ElementSet.Structures);    // One setting in SOBEK 2...
            ConditionallyAddOutputSetting(sobekCaseSettings.WaterLevelOnResultsStructures, QuantityType.WaterlevelDown, ElementSet.Structures);  // ...controls two settings in SOBEK 3!
            ConditionallyAddOutputSetting(sobekCaseSettings.WaterlevelOnCrest, QuantityType.WaterLevelAtCrest, ElementSet.Structures);
            ConditionallyAddOutputSetting(sobekCaseSettings.CrestlevelOpeningsHeight, QuantityType.GateOpeningHeight, ElementSet.Structures);

            // Pumps
            ConditionallyAddOutputSetting(sobekCaseSettings.PumpResults, QuantityType.SuctionSideLevel, ElementSet.Pumps);
            ConditionallyAddOutputSetting(sobekCaseSettings.PumpResults, QuantityType.DeliverySideLevel, ElementSet.Pumps);
            ConditionallyAddOutputSetting(sobekCaseSettings.PumpResults, QuantityType.PumpHead, ElementSet.Pumps);
            ConditionallyAddOutputSetting(sobekCaseSettings.PumpResults, QuantityType.ActualPumpStage, ElementSet.Pumps);
            ConditionallyAddOutputSetting(sobekCaseSettings.PumpResults, QuantityType.PumpCapacity, ElementSet.Pumps);
            ConditionallyAddOutputSetting(sobekCaseSettings.PumpResults, QuantityType.ReductionFactor, ElementSet.Pumps);
            ConditionallyAddOutputSetting(sobekCaseSettings.PumpResults, QuantityType.PumpDischarge, ElementSet.Pumps);

            // This is an odd one: it selects 15 output settings at once and is accessed differently. 
            if (sobekCaseSettings.RiverSubsectionParameters)
            {
                //outputSettings.SubSections = AggregationOptions.Current;
            }
        }

        private void ConditionallyAddOutputSetting(bool add, QuantityType quantityType, ElementSet elementSet)
        {
            /*
            if (add)
            {
                var engineParameter = outputSettings.GetEngineParameter(quantityType, elementSet);
                if (engineParameter != null)
                {
                    engineParameter.AggregationOptions = AggregationOptions.Current;
                }    
            }*/
        }

    }
}
