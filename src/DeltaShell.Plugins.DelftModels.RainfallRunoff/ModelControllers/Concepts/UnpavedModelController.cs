using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Unpaved;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers.Concepts
{
    public class UnpavedModelController : ConceptModelController<UnpavedData>
    {
        private void AddUnpaved(IRRModelHybridFileWriter writer, IRainfallRunoffModel model, UnpavedData unpavedData, IList<ModelLink> links)
        {
            string unpavedId = unpavedData.Catchment.Name;
            var cropAreas = new double[16];

            int index = 0;
            foreach (UnpavedEnums.CropType crop in unpavedData.AreaPerCrop.Keys)
            {
                cropAreas[index++] = unpavedData.AreaPerCrop[crop];
            }

            var krayenhoff = unpavedData.DrainageFormula as KrayenhoffVanDeLeurDrainageFormula;
            var reservoirCoefficient = krayenhoff != null ? krayenhoff.ResevoirCoefficient : 0.0;
            
            var initialLandStorageInMM = unpavedData.InitialLandStorage;

            var maximumLandStorageInMM = unpavedData.MaximumLandStorage;

            DrainageComputationOption drainageComputationOption = GetDrainageComputationOption(unpavedData);

            var initialGroundwaterLevel = GetInitialGroundwaterLevel(unpavedData, model);

            int soilType;
            if(model.CapSim)
            {
                soilType = GetCapsimSoilType(unpavedData);
            }
            else
            {
                soilType = GetSoilType(unpavedData);
            }

            var iref = writer.AddUnpaved(unpavedId, cropAreas, unpavedData.TotalAreaForGroundWaterCalculations,
                                           unpavedData.SurfaceLevel, drainageComputationOption,
                                           reservoirCoefficient, initialLandStorageInMM, maximumLandStorageInMM,
                                           unpavedData.InfiltrationCapacity,
                                           soilType,
                                           initialGroundwaterLevel,
                                           unpavedData.MaximumAllowedGroundWaterLevel,
                                           unpavedData.GroundWaterLayerThickness,
                                           GetMeteoId(model,unpavedData), GetAreaAdjustmentFactor(model, unpavedData), unpavedData.Catchment?.InteriorPoint?.X ?? 0d, unpavedData.Catchment?.InteriorPoint?.Y ?? 0d);

            SetSeepage(unpavedData, writer, model.StartTime, iref);
            SetDrainage(unpavedData, writer, drainageComputationOption, iref);

            links.Add(RainfallRunoffModelController.CreateModelLink(unpavedData.Catchment));
        }

        private double GetInitialGroundwaterLevel(UnpavedData unpavedData, IRainfallRunoffModel model)
        {
            switch(unpavedData.InitialGroundWaterLevelSource)
            {
                case UnpavedEnums.GroundWaterSourceType.Constant:
                    return unpavedData.InitialGroundWaterLevelConstant;
                case UnpavedEnums.GroundWaterSourceType.Series:
                    return unpavedData.InitialGroundWaterLevelSeries.Evaluate<double>(model.StartTime);
                case UnpavedEnums.GroundWaterSourceType.FromLinkedNode:
                    return unpavedData.SurfaceLevel - RootController.GetWaterLevelAtBoundary(unpavedData.Catchment);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void SetDrainage(UnpavedData unpavedData, IRRModelHybridFileWriter writer,
                                        DrainageComputationOption drainageComputationOption, int iref)
        {
            if (drainageComputationOption == DrainageComputationOption.KrayenhoffVdLeur)
            {
                return;
            }

            var deZeeuwErnst = (ErnstDeZeeuwHellingaDrainageFormulaBase) unpavedData.DrainageFormula;
            var values = new[] {deZeeuwErnst.LevelOneValue, deZeeuwErnst.LevelTwoValue, deZeeuwErnst.LevelThreeValue};
            var levels = new[] {deZeeuwErnst.LevelOneTo, deZeeuwErnst.LevelTwoTo, deZeeuwErnst.LevelThreeTo};
            var enabled = new[]
                {deZeeuwErnst.LevelOneEnabled, deZeeuwErnst.LevelTwoEnabled, deZeeuwErnst.LevelThreeEnabled};

            double[] sobekValues = null;
            double[] sobekLevels = null;

            ConvertDrainageLevelsToSobekRepresentation(levels, values, enabled, ref sobekLevels, ref sobekValues);

            if (drainageComputationOption == DrainageComputationOption.DeZeeuwHellinga)
            {
                writer.SetDeZeeuwHellinga(iref, deZeeuwErnst.SurfaceRunoff, deZeeuwErnst.InfiniteDrainageLevelRunoff,
                                            deZeeuwErnst.HorizontalInflow, sobekLevels, sobekValues);
            }
            else
            {
                writer.SetErnst(iref, deZeeuwErnst.SurfaceRunoff, deZeeuwErnst.InfiniteDrainageLevelRunoff,
                                  deZeeuwErnst.HorizontalInflow, sobekLevels, sobekValues);
            }
        }

        private static void ConvertDrainageLevelsToSobekRepresentation(double[] levels, double[] values, bool[] enabled,
                                                                       ref double[] sobekLevels,
                                                                       ref double[] sobekValues)
        {
            int numLevels = values.Length;

            sobekLevels = new double[numLevels];
            sobekValues = new double[numLevels];

            int lastEnabled = 0;
            for (int i = 0; i < numLevels; i++)
            {
                if (!enabled[i])
                {
                    values[i] = 0.0;
                }
                else
                {
                    lastEnabled = i;
                }
            }
            int shift = (numLevels - 1) - lastEnabled;

            // 100 -> 001 (shift=2)
            // 120 -> 012 (shift=1)
            // 123 -> 123 (shift=0)

            for (int i = 0; i < numLevels; i++)
            {
                int j = i - shift;
                if (j >= 0)
                {
                    sobekLevels[i] = levels[j];
                    sobekValues[i] = values[j];
                }
            }
        }

        private static void SetSeepage(UnpavedData unpavedData, IRRModelHybridFileWriter writer, DateTime startTime, int iref)
        {
            SeepageComputationOption seepageType = GetSeepageComputationOption(unpavedData);
            switch (unpavedData.SeepageSource)
            {
                case UnpavedEnums.SeepageSourceType.Constant:
                    writer.SetUnpavedConstantSeepage(iref, unpavedData.SeepageConstant);
                    break;
                case UnpavedEnums.SeepageSourceType.Series:
                    var seepage = unpavedData.SeepageSeries.Evaluate<double>(startTime);
                    writer.SetUnpavedConstantSeepage(iref, seepage);
                    break;
                case UnpavedEnums.SeepageSourceType.H0Series:
                    int[] dates =
                        unpavedData.SeepageH0Series.Time.Values.Select(RRModelEngineHelper.DateToInt).ToArray();
                    int[] times =
                        unpavedData.SeepageH0Series.Time.Values.Select(RRModelEngineHelper.TimeToInt).ToArray();

                    writer.SetUnpavedVariableSeepage(iref, seepageType, unpavedData.SeepageH0HydraulicResistance,
                                                       dates, times,
                                                       unpavedData.SeepageH0Series.Components[0].Values.OfType<double>()
                                                           .ToArray());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static int GetSoilType(UnpavedData unpavedData)
        {
            return (int) (unpavedData.SoilType);
        }

        private static int GetCapsimSoilType(UnpavedData unpavedData)
        {
            return (int)(unpavedData.SoilTypeCapsim);
        }

        private static SeepageComputationOption GetSeepageComputationOption(UnpavedData unpavedData)
        {
            switch (unpavedData.SeepageSource)
            {
                case UnpavedEnums.SeepageSourceType.Constant:
                    return SeepageComputationOption.Constant;
                case UnpavedEnums.SeepageSourceType.Series:
                    return SeepageComputationOption.Constant;
                case UnpavedEnums.SeepageSourceType.H0Series:
                    return SeepageComputationOption.VariableWithH0;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static DrainageComputationOption GetDrainageComputationOption(UnpavedData unpavedData)
        {
            if (unpavedData.DrainageFormula is DeZeeuwHellingaDrainageFormula)
            {
                return DrainageComputationOption.DeZeeuwHellinga;
            }
            if (unpavedData.DrainageFormula is ErnstDrainageFormula)
            {
                return DrainageComputationOption.Ernst;
            }
            if (unpavedData.DrainageFormula is KrayenhoffVanDeLeurDrainageFormula)
            {
                return DrainageComputationOption.KrayenhoffVdLeur;
            }
            throw new NotImplementedException("Unknown");
        }
        
        public override bool CanHandle(ElementSet elementSet)
        {
            return elementSet == ElementSet.UnpavedElmSet;
        }

        protected override void OnAddArea(IRainfallRunoffModel model, UnpavedData data, IList<ModelLink> links)
        {
            AddUnpaved(Writer, model, data, links);
        }
    }
}