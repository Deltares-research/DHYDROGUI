using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public class Flow1DParameterGenerator
    {
        public IList<DelftIniCategory> GenerateParameterCategories(WaterFlowModel1D waterFlowModel1D)
        {
            if (waterFlowModel1D == null) return null;

            IList<DelftIniCategory> parameterGroups = new List<DelftIniCategory>();
            
            // Global values
            var globalValuesGroup = GenerateGlobalValues(waterFlowModel1D);

            // Initial Conditions
            var initialConditionsValuesGroup = GenerateInitialConditionsValues(waterFlowModel1D);

            // Time values
            var timeValuesGroup = GenerateTimeValues(waterFlowModel1D);
            
            //Sediment
            var sedimentValuesGroup = GenerateSedimentValues(waterFlowModel1D);
            
            //Specials
            var specialsValuesGroup = GenerateSpecialsValues(waterFlowModel1D);

            //Numerical Parameters
            var numericalParametersValuesGroup = GenerateNumericalParametersValues(waterFlowModel1D);
            
            //Simulation Options
            var simulationOptionsValuesGroup = GenerateSimulationOptionsValues(waterFlowModel1D);

            // TransportComputation Options
            var transportComputationOptionsValuesGroup = GenerateTransportComputationOptionsValues(waterFlowModel1D);
            
            // Advanced Options
            var advancedOptionsValuesGroup = GenerateAdvancedOptionsValues(waterFlowModel1D);
            
            //Salinity
            var salinityValuesGroup = GenerateSalinityValues(waterFlowModel1D);

            // Temperature
            var temperatureValuesGroup = GenerateTemperatureValues(waterFlowModel1D);

            // Morphology
            var morphologyValuesGroup = GenerateMorphologyValues(waterFlowModel1D);

            //Observation points
            var obsPointsGroup = GenerateObservationValues(waterFlowModel1D);

            //Restart Options
            var restartOptionsValuesGroup = GenerateRestartOptionsValues(waterFlowModel1D);


            parameterGroups.Add(globalValuesGroup);
            parameterGroups.Add(initialConditionsValuesGroup);
            parameterGroups.Add(timeValuesGroup);
            parameterGroups.Add(sedimentValuesGroup);
            parameterGroups.Add(specialsValuesGroup);
            parameterGroups.Add(numericalParametersValuesGroup);
            parameterGroups.Add(simulationOptionsValuesGroup);
            parameterGroups.Add(transportComputationOptionsValuesGroup);
            parameterGroups.Add(advancedOptionsValuesGroup);
            parameterGroups.Add(salinityValuesGroup);
            parameterGroups.Add(temperatureValuesGroup);
            parameterGroups.Add(morphologyValuesGroup);
            parameterGroups.Add(obsPointsGroup);
            parameterGroups.Add(restartOptionsValuesGroup);
            
            return parameterGroups;
        }

        private static DelftIniCategory GenerateTimeValues(WaterFlowModel1D waterFlowModel1D)
        {
            var timeValues = new DelftIniCategory(ModelDefinitionsRegion.TimeHeader);

            timeValues.AddProperty(ModelDefinitionsRegion.StartTime.Key, waterFlowModel1D.StartTime, ModelDefinitionsRegion.StartTime.Description);
            timeValues.AddProperty(ModelDefinitionsRegion.StopTime.Key, waterFlowModel1D.StopTime, ModelDefinitionsRegion.StopTime.Description);
            timeValues.AddProperty(ModelDefinitionsRegion.TimeStep.Key, waterFlowModel1D.TimeStep.TotalSeconds, ModelDefinitionsRegion.TimeStep.Description, ModelDefinitionsRegion.TimeStep.Format);

            var modelOutputSettings = waterFlowModel1D.OutputSettings;
            timeValues.AddProperty(ModelDefinitionsRegion.OutTimeStepGridPoints.Key, modelOutputSettings.GridOutputTimeStep.TotalSeconds, ModelDefinitionsRegion.OutTimeStepGridPoints.Description, ModelDefinitionsRegion.OutTimeStepGridPoints.Format);
            timeValues.AddProperty(ModelDefinitionsRegion.OutTimeStepStructures.Key, modelOutputSettings.StructureOutputTimeStep.TotalSeconds, ModelDefinitionsRegion.OutTimeStepStructures.Description, ModelDefinitionsRegion.OutTimeStepStructures.Format);
            
            return timeValues;
        }

        private DelftIniCategory GenerateTransportComputationOptionsValues(WaterFlowModel1D waterFlowModel1D)
        {
            var transportComputationValues = new DelftIniCategory(ModelDefinitionsRegion.TransportComputationValuesHeader);

            var tempIsBeingUsed = waterFlowModel1D.UseTemperature ? 1 : 0;
            transportComputationValues.AddProperty(ModelDefinitionsRegion.UseTemperature.Key, tempIsBeingUsed,ModelDefinitionsRegion.UseTemperature.Description);
            var density = waterFlowModel1D.DensityTypeParameter.Value;
            transportComputationValues.AddProperty(ModelDefinitionsRegion.Density.Key, density, ModelDefinitionsRegion.Density.Description);

            var heatTransferModel = waterFlowModel1D.TemperatureModelTypeParameter.Value;
            transportComputationValues.AddProperty(ModelDefinitionsRegion.HeatTransferModel.Key, heatTransferModel, ModelDefinitionsRegion.HeatTransferModel.Description);

            return transportComputationValues;
        }

        private DelftIniCategory GenerateAdvancedOptionsValues(WaterFlowModel1D waterFlowModel1D)
        {
            var advancedOptionsValues = new DelftIniCategory(ModelDefinitionsRegion.AdvancedOptionsHeader);

            // Value not used by ModelApi, so just get it from WFM1D
            var calculateDelwaqOutput = waterFlowModel1D.HydFileOutput ? 1 : 0;
            advancedOptionsValues.AddProperty(ModelDefinitionsRegion.CalculateDelwaqOutput.Key, calculateDelwaqOutput, ModelDefinitionsRegion.CalculateDelwaqOutput.Description);

            var extraResistanceGeneralStructure = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.ExtraResistanceGeneralStructure.Key);
            if (extraResistanceGeneralStructure != null)
            {
                advancedOptionsValues.AddProperty(ModelDefinitionsRegion.ExtraResistanceGeneralStructure.Key, extraResistanceGeneralStructure.Value, ModelDefinitionsRegion.ExtraResistanceGeneralStructure.Description);
            }

            var fillCulvertsWithGL = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.FillCulvertsWithGL.Key);
            if (fillCulvertsWithGL != null)
            {
                advancedOptionsValues.AddProperty(ModelDefinitionsRegion.FillCulvertsWithGL.Key, Convert.ToBoolean(fillCulvertsWithGL.Value) ? 1 : 0, ModelDefinitionsRegion.FillCulvertsWithGL.Description);
            }

            var lateralLocation = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.LateralLocation.Key);
            if (lateralLocation != null)
            {
                advancedOptionsValues.AddProperty(ModelDefinitionsRegion.LateralLocation.Key, lateralLocation.Value, ModelDefinitionsRegion.LateralLocation.Description);
            }
            
            var maxLoweringCrossAtCulvert = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.MaxLoweringCrossAtCulvert.Key);
            if (maxLoweringCrossAtCulvert != null)
            {
                advancedOptionsValues.AddProperty(ModelDefinitionsRegion.MaxLoweringCrossAtCulvert.Key, maxLoweringCrossAtCulvert.Value, ModelDefinitionsRegion.MaxLoweringCrossAtCulvert.Description);
            }
            
            var maxVolFact = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.MaxVolFact.Key);
            if (maxVolFact != null)
            {
                advancedOptionsValues.AddProperty(ModelDefinitionsRegion.MaxVolFact.Key, maxVolFact.Value, ModelDefinitionsRegion.MaxVolFact.Description);
            }

            var noNegativeQlatWhenThereIsNoWater = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.NoNegativeQlatWhenThereIsNoWater.Key);
            if (noNegativeQlatWhenThereIsNoWater != null)
            {
                advancedOptionsValues.AddProperty(ModelDefinitionsRegion.NoNegativeQlatWhenThereIsNoWater.Key, Convert.ToBoolean(noNegativeQlatWhenThereIsNoWater.Value)?1:0, ModelDefinitionsRegion.NoNegativeQlatWhenThereIsNoWater.Description);
            }
            
            var transitionHeightSd = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.TransitionHeightSD.Key);
            if (transitionHeightSd != null)
            {
                advancedOptionsValues.AddProperty(ModelDefinitionsRegion.TransitionHeightSD.Key, transitionHeightSd.Value, ModelDefinitionsRegion.TransitionHeightSD.Description);
            }

            var latitude = waterFlowModel1D.Latitude;
            advancedOptionsValues.AddProperty(ModelDefinitionsRegion.Latitude.Key, latitude, ModelDefinitionsRegion.Latitude.Description);

            var longitude = waterFlowModel1D.Longitude;
            advancedOptionsValues.AddProperty(ModelDefinitionsRegion.Longitude.Key, longitude, ModelDefinitionsRegion.Longitude.Description);

            return advancedOptionsValues;
        }

        private DelftIniCategory GenerateSimulationOptionsValues(WaterFlowModel1D waterFlowModel1D)
        {
            DelftIniCategory simulationOptionsValues = new DelftIniCategory(ModelDefinitionsRegion.SimulationOptionsValuesHeader);

            //TODO: Add allowablelargertimestep ???

            //TODO: Add allowabletimesteplimiter ???

            //TODO: Add AllowableVolumeError ???

            //TODO: Add AllowCrestLevelBelowBottom ???

            //TODO: Add Cflcheckalllinks ???

            //TODO: Add Channel ???

            //TODO: Add CheckFuru ???

            //TODO: Add CheckFuruMode ???
            
            var debug = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.Debug.Key);
            if (debug != null)
            {
                simulationOptionsValues.AddProperty(ModelDefinitionsRegion.Debug.Key, Convert.ToBoolean(debug.Value)?1:0, ModelDefinitionsRegion.Debug.Description);
            }

            var debugTime = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.DebugTime.Key);
            if (debugTime != null)
            {
                simulationOptionsValues.AddProperty(ModelDefinitionsRegion.DebugTime.Key, debugTime.Value, ModelDefinitionsRegion.DebugTime.Description);
            }

            //TODO: Add DepthsBelowBobs ???
            
            var dispMaxFactor = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.DispMaxFactor.Key);
            if (dispMaxFactor != null)
            {
                simulationOptionsValues.AddProperty(ModelDefinitionsRegion.DispMaxFactor.Key, dispMaxFactor.Value, ModelDefinitionsRegion.DispMaxFactor.Description);
            }

            var dumpInput = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.DumpInput.Key);
            if (dumpInput != null)
            {
                simulationOptionsValues.AddProperty(ModelDefinitionsRegion.DumpInput.Key, Convert.ToBoolean(dumpInput.Value)?1:0, ModelDefinitionsRegion.DumpInput.Description);
            }

            var iadvec1D = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.Iadvec1D.Key);
            if (iadvec1D != null)
            {
                simulationOptionsValues.AddProperty(ModelDefinitionsRegion.Iadvec1D.Key, iadvec1D.Value, ModelDefinitionsRegion.Iadvec1D.Description);
            }

            //TODO: Add Jchecknans ???

            //TODO: Add Junctionadvection ???

            //TODO: Add LaboratoryTest ???

            //TODO: Add LaboratoryTimeStep ???

            //TODO: Add LaboratoryTotalStep ???


            var limtyphu1D = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.Limtyphu1D.Key);
            if (limtyphu1D != null)
            {
                simulationOptionsValues.AddProperty(ModelDefinitionsRegion.Limtyphu1D.Key, limtyphu1D.Value, ModelDefinitionsRegion.Limtyphu1D.Description);
            }

            var loggingLevel = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.LoggingLevel.Key);
            if (loggingLevel != null)
            {
                simulationOptionsValues.AddProperty(ModelDefinitionsRegion.LoggingLevel.Key, loggingLevel.Value, ModelDefinitionsRegion.LoggingLevel.Description);
            }

            //TODO: Add LoggingLevel ???
            
            //TODO: Add Manhloss ???
            
            //TODO: Add ManholeLosses ???
            
            //TODO: Add MissingValue ???

            var momdilution1D = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.Momdilution1D.Key);
            if (momdilution1D != null)
            {
                simulationOptionsValues.AddProperty(ModelDefinitionsRegion.Momdilution1D.Key, momdilution1D.Value, ModelDefinitionsRegion.Momdilution1D.Description);
            }
            
            var Morphology = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.Morphology.Key);
            if (Morphology != null)
            {
                simulationOptionsValues.AddProperty(ModelDefinitionsRegion.Morphology.Key, Convert.ToBoolean(Morphology.Value) ?1:0, ModelDefinitionsRegion.Morphology.Description);
            }

            //TODO: Add PreissmannMinClosedManholes ???
            
            //TODO: Add QDrestart ???
            
            //TODO: Add River ???
            
            //TODO: Add Sewer ???
            
            //TODO: Add SiphonUpstreamThresholdSwitchOff ???
            
            //TODO: Add StrucAlfa ???
            
            //TODO: Add StructureDynamicsFactor ???
            
            //TODO: Add StructureStabilityFactor ???
            
            //TODO: Add ThresholdForSummerDike ???

            var timersOutputFrequency = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.TimersOutputFrequency.Key);
            if (timersOutputFrequency != null)
            {
                simulationOptionsValues.AddProperty(ModelDefinitionsRegion.TimersOutputFrequency.Key, timersOutputFrequency.Value, ModelDefinitionsRegion.TimersOutputFrequency.Description);
            }

            //TODO: Add use1d2dcoupling ???

            //TODO: Add UseEnergyHeadStructures ???

            var useTimers = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.UseTimers.Key);
            if (useTimers != null)
            {
                simulationOptionsValues.AddProperty(ModelDefinitionsRegion.UseTimers.Key, Convert.ToBoolean(useTimers.Value) ?1:0, ModelDefinitionsRegion.UseTimers.Description);
            }

            //TODO: Add Usevariableteta ???
            
            //TODO: Add VolumeCheck ???
            
            //TODO: Add VolumeCorrection ???
            
            //TODO: Add WaterQualityInUse ???

            var writeNetCdf = true; // always true - not currently configurable in the GUI
            if (writeNetCdf != null)
            {
                simulationOptionsValues.AddProperty(ModelDefinitionsRegion.WriteNetCDF.Key, Convert.ToBoolean(writeNetCdf) ? 1 : 0, ModelDefinitionsRegion.WriteNetCDF.Description);
            }

            return simulationOptionsValues;

        }

        private DelftIniCategory GenerateNumericalParametersValues(WaterFlowModel1D waterFlowModel1D)
        {
            DelftIniCategory numericalParametersValues = new DelftIniCategory(ModelDefinitionsRegion.NumericalParametersValuesHeader);

            var accelerationTermFactor = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.AccelerationTermFactor.Key);
            if (accelerationTermFactor != null)
            {
                numericalParametersValues.AddProperty(ModelDefinitionsRegion.AccelerationTermFactor.Key, accelerationTermFactor.Value, ModelDefinitionsRegion.AccelerationTermFactor.Description);
            }

            var accurateVersusSpeed = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.AccurateVersusSpeed.Key);
            if (accurateVersusSpeed != null)
            {
                numericalParametersValues.AddProperty(ModelDefinitionsRegion.AccurateVersusSpeed.Key, accurateVersusSpeed.Value, ModelDefinitionsRegion.AccurateVersusSpeed.Description);
            }
            
            var courantNumber = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.CourantNumber.Key);
            if (courantNumber != null)
            {
                numericalParametersValues.AddProperty(ModelDefinitionsRegion.CourantNumber.Key, courantNumber.Value, ModelDefinitionsRegion.CourantNumber.Description);
            }

            var dtMinimum = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.DtMinimum.Key);
            if (dtMinimum != null)
            {
                numericalParametersValues.AddProperty(ModelDefinitionsRegion.DtMinimum.Key, dtMinimum.Value, ModelDefinitionsRegion.DtMinimum.Description);
            }

            var epsilonValueVolume = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.EpsilonValueVolume.Key);
            if (epsilonValueVolume != null)
            {
                numericalParametersValues.AddProperty(ModelDefinitionsRegion.EpsilonValueVolume.Key, epsilonValueVolume.Value, ModelDefinitionsRegion.EpsilonValueVolume.Description);
            }

            var epsilonValueWaterDepth = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.EpsilonValueWaterDepth.Key);
            if (epsilonValueWaterDepth != null)
            {
                numericalParametersValues.AddProperty(ModelDefinitionsRegion.EpsilonValueWaterDepth.Key, epsilonValueWaterDepth.Value, ModelDefinitionsRegion.EpsilonValueWaterDepth.Description);
            }

            //TODO : Add FloodingDividedByDrying ???

            var gravity = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.Gravity.Key);
            if (gravity != null)
            {
                numericalParametersValues.AddProperty(ModelDefinitionsRegion.Gravity.Key, gravity.Value, ModelDefinitionsRegion.Gravity.Description);
            }

            var maxDegree = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.MaxDegree.Key);
            if (maxDegree != null)
            {
                numericalParametersValues.AddProperty(ModelDefinitionsRegion.MaxDegree.Key, maxDegree.Value, ModelDefinitionsRegion.MaxDegree.Description);
            }

            var maxIterations = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.MaxIterations.Key);
            if (maxIterations != null)
            {
                numericalParametersValues.AddProperty(ModelDefinitionsRegion.MaxIterations.Key, maxIterations.Value, ModelDefinitionsRegion.MaxIterations.Description);
            }

            //TODO : Add MaxTimeStep ???
            
            var minimumSurfaceatStreet = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.MinimumSurfaceatStreet.Key);
            if (minimumSurfaceatStreet != null)
            {
                numericalParametersValues.AddProperty(ModelDefinitionsRegion.MinimumSurfaceatStreet.Key, minimumSurfaceatStreet.Value, ModelDefinitionsRegion.MinimumSurfaceatStreet.Description);
            }

            var minimumSurfaceinNode = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.MinimumSurfaceinNode.Key);
            if (minimumSurfaceinNode != null)
            {
                numericalParametersValues.AddProperty(ModelDefinitionsRegion.MinimumSurfaceinNode.Key, minimumSurfaceinNode.Value, ModelDefinitionsRegion.MinimumSurfaceinNode.Description);
            }

            var minimumLength = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.MinimumLength.Key);
            if (minimumLength != null)
            {
                numericalParametersValues.AddProperty(ModelDefinitionsRegion.MinimumLength.Key, minimumLength.Value, ModelDefinitionsRegion.MinimumLength.Description);
            }

            var relaxationFactor = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.RelaxationFactor.Key);
            if (relaxationFactor != null)
            {
                numericalParametersValues.AddProperty(ModelDefinitionsRegion.RelaxationFactor.Key, relaxationFactor.Value, ModelDefinitionsRegion.RelaxationFactor.Description);
            }

            var rho = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.Rho.Key);
            if (rho != null)
            {
                numericalParametersValues.AddProperty(ModelDefinitionsRegion.Rho.Key, rho.Value, ModelDefinitionsRegion.Rho.Description);
            }

            var structureInertiaDampingFactor = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.StructureInertiaDampingFactor.Key);
            if (structureInertiaDampingFactor != null)
            {
                numericalParametersValues.AddProperty(ModelDefinitionsRegion.StructureInertiaDampingFactor.Key, structureInertiaDampingFactor.Value, ModelDefinitionsRegion.StructureInertiaDampingFactor.Description);
            }

            var theta = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.Theta.Key);
            if (theta != null)
            {
                numericalParametersValues.AddProperty(ModelDefinitionsRegion.Theta.Key, theta.Value, ModelDefinitionsRegion.Theta.Description);
            }

            var thresholdValueFlooding = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.ThresholdValueFlooding.Key);
            if (thresholdValueFlooding != null)
            {
                numericalParametersValues.AddProperty(ModelDefinitionsRegion.ThresholdValueFlooding.Key, thresholdValueFlooding.Value, ModelDefinitionsRegion.ThresholdValueFlooding.Description);
            }

            //TODO: Add UseOmp ???

            var useTimeStepReducerStructures = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.UseTimeStepReducerStructures.Key);
            if (useTimeStepReducerStructures != null)
            {
                int useReducer = -1;
                try
                {
                    useReducer = Convert.ToBoolean(useTimeStepReducerStructures.Value) ? 1 : 0;
                }
                catch (Exception)
                {
                    try
                    {
                        useReducer = Int32.Parse(useTimeStepReducerStructures.Value);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Could not determine boolean value for " + ModelDefinitionsRegion.UseTimeStepReducerStructures.Key);
                    }
                }
                numericalParametersValues.AddProperty(ModelDefinitionsRegion.UseTimeStepReducerStructures.Key, useReducer, ModelDefinitionsRegion.UseTimeStepReducerStructures.Description);
            }

            return numericalParametersValues;
        }

        private DelftIniCategory GenerateSpecialsValues(WaterFlowModel1D waterFlowModel1D)
        {
            DelftIniCategory specialsValuesGroup = new DelftIniCategory(ModelDefinitionsRegion.SpecialsValuesHeader);

            //TODO: Add DesignFactorDLG ???

            return specialsValuesGroup;

        }

        private DelftIniCategory GenerateSedimentValues(WaterFlowModel1D waterFlowModel1D)
        {
            DelftIniCategory resultsGeneralValuesGroup = new DelftIniCategory(ModelDefinitionsRegion.SedimentValuesHeader);
            
            //TODO: Add D50 ???

            //TODO: Add D90 ???

            //TODO: Add DepthUsedForSediment ???
            
            return resultsGeneralValuesGroup;
        }

        private DelftIniCategory GenerateInitialConditionsValues(WaterFlowModel1D waterFlowModel1D)
        {
            DelftIniCategory initialConditionsValuesGroup = new DelftIniCategory(ModelDefinitionsRegion.InitialConditionsValuesHeader);
            var emptyWells = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.InitialEmptyWells.Key);
            if (emptyWells != null)
            {
                initialConditionsValuesGroup.AddProperty(ModelDefinitionsRegion.InitialEmptyWells.Key, Convert.ToBoolean(emptyWells.Value) ? 1:0, ModelDefinitionsRegion.InitialEmptyWells.Description);
            }
            
            return initialConditionsValuesGroup;
        }

        private DelftIniCategory GenerateSalinityValues(WaterFlowModel1D waterFlowModel1D)
        {
            DelftIniCategory salinityValuesGroup = new DelftIniCategory(ModelDefinitionsRegion.SalinityValuesHeader);
            salinityValuesGroup.AddProperty(ModelDefinitionsRegion.SaltComputation.Key, waterFlowModel1D.UseSaltInCalculation ? 1:0,
                ModelDefinitionsRegion.SaltComputation.Description);
            var diffusionAtBoundaries = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.DiffusionAtBoundaries.Key);
            if (diffusionAtBoundaries != null)
            {
                salinityValuesGroup.AddProperty(ModelDefinitionsRegion.DiffusionAtBoundaries.Key, Convert.ToBoolean(diffusionAtBoundaries.Value) ? 1 : 0,
                    ModelDefinitionsRegion.SaltComputation.Description);
            }
            
            return salinityValuesGroup;
        }

        private DelftIniCategory GenerateTemperatureValues(WaterFlowModel1D waterFlowModel1D)
        {
            var temperatureValues = new DelftIniCategory(ModelDefinitionsRegion.TemperatureValuesHeader);

            var backgroundTemperature = waterFlowModel1D.BackgroundTemperature;
            temperatureValues.AddProperty(ModelDefinitionsRegion.BackgroundTemperature.Key, backgroundTemperature, ModelDefinitionsRegion.BackgroundTemperature.Description);

            var surfaceArea = waterFlowModel1D.SurfaceArea;
            temperatureValues.AddProperty(ModelDefinitionsRegion.SurfaceArea.Key, surfaceArea, ModelDefinitionsRegion.SurfaceArea.Description);

            var atmosphericPressure = waterFlowModel1D.AtmosphericPressure;
            temperatureValues.AddProperty(ModelDefinitionsRegion.AtmosphericPressure.Key, atmosphericPressure, ModelDefinitionsRegion.AtmosphericPressure.Description);

            var daltonNumber = waterFlowModel1D.DaltonNumber;
            temperatureValues.AddProperty(ModelDefinitionsRegion.DaltonNumber.Key, daltonNumber, ModelDefinitionsRegion.DaltonNumber.Description);

            var stantonNumber = waterFlowModel1D.StantonNumber;
            temperatureValues.AddProperty(ModelDefinitionsRegion.StantonNumber.Key, stantonNumber, ModelDefinitionsRegion.StantonNumber.Description);

            var heatCapacity = waterFlowModel1D.HeatCapacityWater;
            temperatureValues.AddProperty(ModelDefinitionsRegion.HeatCapacity.Key, heatCapacity, ModelDefinitionsRegion.HeatCapacity.Description);

            return temperatureValues;
        }

        private DelftIniCategory GenerateMorphologyValues(WaterFlowModel1D waterFlowModel1D)
        {
            var morphologyValues = new DelftIniCategory(ModelDefinitionsRegion.MorphologyValuesHeader);

            var calculateMorphology = waterFlowModel1D.UseMorphology ? 1 : 0;
            morphologyValues.AddProperty(ModelDefinitionsRegion.CalculateMorphology.Key, calculateMorphology, ModelDefinitionsRegion.CalculateMorphology.Description);

            var additionalOutput = waterFlowModel1D.AdditionalMorphologyOutput ? 1 : 0;
            morphologyValues.AddProperty(ModelDefinitionsRegion.AdditionalOutput.Key, additionalOutput, ModelDefinitionsRegion.AdditionalOutput.Description);

            var morphologyFilePath = string.IsNullOrEmpty(waterFlowModel1D.MorphologyPath) ? string.Empty : new FileInfo(waterFlowModel1D.MorphologyPath).Name;
            morphologyValues.AddProperty(ModelDefinitionsRegion.MorphologyInputFile.Key, morphologyFilePath, ModelDefinitionsRegion.MorphologyInputFile.Description);

            var sedimentFilePath = string.IsNullOrEmpty(waterFlowModel1D.SedimentPath) ? string.Empty : new FileInfo(waterFlowModel1D.SedimentPath).Name;
            morphologyValues.AddProperty(ModelDefinitionsRegion.SedimentInputFile.Key, sedimentFilePath, ModelDefinitionsRegion.SedimentInputFile.Description);
            
            return morphologyValues;
        }

        private DelftIniCategory GenerateObservationValues(WaterFlowModel1D waterFlowModel1D)
        {
            DelftIniCategory observationValues = new DelftIniCategory(ModelDefinitionsRegion.ObservationsHeader);
            ModelApiParameter interpolParameter = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.InterpolationType.Key);
            if (interpolParameter != null)
            {
                observationValues.AddProperty(ModelDefinitionsRegion.InterpolationType.Key, interpolParameter.Value,
                    ModelDefinitionsRegion.InterpolationType.Description);
            }
            
            return observationValues;
        }
        private DelftIniCategory GenerateRestartOptionsValues(WaterFlowModel1D waterFlowModel1D)
        {
            DelftIniCategory restartValues = new DelftIniCategory(ModelDefinitionsRegion.RestartHeader);
            if (waterFlowModel1D.UseSaveStateTimeRange)
            {
                restartValues.AddProperty(ModelDefinitionsRegion.RestartStartTime.Key, waterFlowModel1D.SaveStateStartTime, ModelDefinitionsRegion.RestartStartTime.Description);
                restartValues.AddProperty(ModelDefinitionsRegion.RestartStopTime.Key, waterFlowModel1D.SaveStateStopTime, ModelDefinitionsRegion.RestartStopTime.Description);
                restartValues.AddProperty(ModelDefinitionsRegion.RestartTimeStep.Key, int.Parse(waterFlowModel1D.SaveStateTimeStep.TotalSeconds.ToString(CultureInfo.InvariantCulture)), ModelDefinitionsRegion.RestartTimeStep.Description);
            }
            var useRestartParameter = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.UseRestart.Key);
            var useRestart = useRestartParameter != null ? Convert.ToBoolean(useRestartParameter.Value) : waterFlowModel1D.UseRestart;
            restartValues.AddProperty(ModelDefinitionsRegion.UseRestart.Key, useRestart ? 1 : 0, ModelDefinitionsRegion.UseRestart.Description);

            var writeRestartParameter = waterFlowModel1D.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.WriteRestart.Key);
            var writeRestart = writeRestartParameter != null ? Convert.ToBoolean(writeRestartParameter.Value) : waterFlowModel1D.WriteRestart;
            restartValues.AddProperty(ModelDefinitionsRegion.WriteRestart.Key, writeRestart ? 1 : 0, ModelDefinitionsRegion.WriteRestart.Description);


            return restartValues;
        }

        private static DelftIniCategory GenerateGlobalValues(WaterFlowModel1D waterFlowModel1D)
        {
            DelftIniCategory globalValuesGroup = new DelftIniCategory(ModelDefinitionsRegion.GlobalValuesHeader);
            int useDepth = waterFlowModel1D.InitialConditionsType == InitialConditionsType.Depth ? 1 : 0;

            if (useDepth == 1)
            {
                globalValuesGroup.AddProperty(ModelDefinitionsRegion.UseInitialWaterDepth.Key, useDepth,
                    ModelDefinitionsRegion.UseInitialWaterDepth.Description);
            }
            globalValuesGroup.AddProperty(ModelDefinitionsRegion.InitialWaterLevel.Key, waterFlowModel1D.DefaultInitialWaterLevel,
                ModelDefinitionsRegion.InitialWaterLevel.Description, ModelDefinitionsRegion.InitialWaterLevel.Format);
            globalValuesGroup.AddProperty(ModelDefinitionsRegion.InitialWaterDepth.Key, waterFlowModel1D.DefaultInitialDepth,
                ModelDefinitionsRegion.InitialWaterDepth.Description, ModelDefinitionsRegion.InitialWaterDepth.Format);
            globalValuesGroup.AddProperty(ModelDefinitionsRegion.InitialDischarge.Key, waterFlowModel1D.InitialFlow.DefaultValue,
                ModelDefinitionsRegion.InitialDischarge.Description, ModelDefinitionsRegion.InitialDischarge.Format);

            if (waterFlowModel1D.InitialSaltConcentration != null)
            {
                globalValuesGroup.AddProperty(ModelDefinitionsRegion.InitialSalinity.Key, waterFlowModel1D.InitialSaltConcentration.DefaultValue,
                    ModelDefinitionsRegion.InitialSalinity.Description, ModelDefinitionsRegion.InitialSalinity.Format);
            }

            if (waterFlowModel1D.DispersionCoverage != null)
            {
                globalValuesGroup.AddProperty(ModelDefinitionsRegion.Dispersion.Key, waterFlowModel1D.DispersionCoverage.DefaultValue,
                    ModelDefinitionsRegion.Dispersion.Description, ModelDefinitionsRegion.Dispersion.Format);
            }

            if (waterFlowModel1D.DispersionF3Coverage != null)
            {
                globalValuesGroup.AddProperty(ModelDefinitionsRegion.DispersionF3.Key, waterFlowModel1D.DispersionF3Coverage.DefaultValue,
                    ModelDefinitionsRegion.DispersionF3.Description, ModelDefinitionsRegion.DispersionF3.Format);
            }

            if (waterFlowModel1D.DispersionF4Coverage != null)
            {
                globalValuesGroup.AddProperty(ModelDefinitionsRegion.DispersionF4.Key, waterFlowModel1D.DispersionF4Coverage.DefaultValue,
                    ModelDefinitionsRegion.DispersionF4.Description, ModelDefinitionsRegion.DispersionF4.Format);
            }

            return globalValuesGroup;
        }
    }
}
