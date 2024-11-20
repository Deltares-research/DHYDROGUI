using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using DelftTools.Functions;
using DeltaShell.NGHS.Utils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Unpaved;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class SobekRRUnpavedImporter : PartialSobekImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRRUnpavedImporter)); 

        public override string DisplayName
        {
            get { return "Rainfall Runoff unpaved data"; }
        }

        public override SobekImporterCategories Category { get; } = SobekImporterCategories.RainfallRunoff;

        protected override void PartialImport()
        { 
            log.DebugFormat("Importing unpaved data ...");
            
            var rainfallRunoffModel = GetModel<RainfallRunoffModel>();
            var catchmentModelData = rainfallRunoffModel.GetAllModelData()
                                                        .OfType<UnpavedData>()
                                                        .ToDictionary(rra => rra.Name);
            ReadAndAddOrUpdateUnpavedAreas(GetFilePath(SobekFileNames.SobekRRUnpavedFileName), catchmentModelData);
        }

        private void ReadAndAddOrUpdateUnpavedAreas(string filePath, Dictionary<string, UnpavedData> catchmentModelData)
        {
            var formatSCTable = new DataTable();
            formatSCTable.Columns.Add(new DataColumn("Percentage", typeof(double)));
            formatSCTable.Columns.Add(new DataColumn("Height", typeof(double)));

            var pathAlf = filePath.Replace(".3B", ".Alf");
            var pathInf = filePath.Replace(".3B", ".Inf");
            var pathSep = filePath.Replace(".3B", ".Sep");
            var pathSto = filePath.Replace(".3B", ".Sto");
            var pathTbl = filePath.Replace(".3B", ".Tbl");

            var dicErnst = new SobekRRErnstReader().Read(pathAlf).ToDictionaryWithErrorDetails(pathAlf, item => item.Id, item => item);
            var dicAlfa = new SobekRRAlfaReader().Read(pathAlf).ToDictionaryWithErrorDetails(pathAlf, item => item.Id, item => item);
            var dicInf = new SobekRRInfiltrationReader().Read(pathInf).ToDictionaryWithErrorDetails(pathInf,item => item.Id, item => item);
            var dicSep = new SobekRRSeepageReader().Read(pathSep).ToDictionaryWithErrorDetails(pathSep,item => item.Id, item => item);
            var dicSto = new SobekRRStorageReader().Read(pathSto).ToDictionaryWithErrorDetails(pathSto,item => item.Id, item => item);
            var dicH0 = new SobekRRTableReader("H0_T").Read(pathTbl).ToDictionaryWithErrorDetails(pathTbl,item => item.TableName, item => item);
            var dicGroundWater = new SobekRRTableReader("IG_T").Read(pathTbl).ToDictionaryWithErrorDetails(pathTbl,item => item.TableName, item => item);
            var dicScurve = new SobekRRTableReader("SC_T", formatSCTable).Read(pathTbl).ToDictionaryWithErrorDetails(pathTbl,item => item.TableName, item => item);

            foreach (var sobekUnpaved in new SobekRRUnpavedReader().Read(filePath))
            {
                if (catchmentModelData.ContainsKey(sobekUnpaved.Id))
                {
                    var unpaved = catchmentModelData[sobekUnpaved.Id];

                    unpaved.CalculationArea = sobekUnpaved.CropAreas.Sum();
                    unpaved.TotalAreaForGroundWaterCalculations = sobekUnpaved.GroundWaterArea;
                    unpaved.UseDifferentAreaForGroundWaterCalculations = (unpaved.CalculationArea !=
                                                                          sobekUnpaved.GroundWaterArea);

                    //Surface level
                    if (sobekUnpaved.ScurveUsed)
                    {
                        log.WarnFormat("S-curve is not supported. The median of the s-curve ({0}) has been used",
                                       sobekUnpaved.ScurveTableName);
                        if (dicScurve.ContainsKey(sobekUnpaved.ScurveTableName))
                        {

                            var dataTable = dicScurve[sobekUnpaved.ScurveTableName];
                            var max = 0.0;
                            var level = 0.0;
                            foreach (DataRow row in dataTable.Rows)
                            {
                                if (Convert.ToDouble(row[0], CultureInfo.InvariantCulture) > max)
                                {
                                    level = Convert.ToDouble(row[1], CultureInfo.InvariantCulture);
                                }
                            }
                            unpaved.SurfaceLevel = level;
                        }
                        else
                        {
                            log.ErrorFormat("S-curve table {0} has not been found", sobekUnpaved.ScurveTableName);
                        }
                    }
                    else
                    {
                        unpaved.SurfaceLevel = sobekUnpaved.SurfaceLevel;
                    }

                    //Soil type
                    var soilType = sobekUnpaved.SoilType;
                    if (Enum.IsDefined(typeof(UnpavedEnums.SoilType), soilType))
                    {
                        unpaved.SoilType = (UnpavedEnums.SoilType)soilType;
                    }
                    else if (Enum.IsDefined(typeof(UnpavedEnums.SoilTypeCapsim), soilType))
                    {
                        unpaved.SoilTypeCapsim = (UnpavedEnums.SoilTypeCapsim)soilType;
                    }
                    else
                    {
                        log.ErrorFormat("Couldn't import soil type {0} of {1}", soilType, unpaved.Name);
                    }


                    //crop areas
                    for (var i = 0; i < sobekUnpaved.CropAreas.Length; i++)
                    {
                        unpaved.AreaPerCrop[(UnpavedEnums.CropType)Enum.ToObject(typeof(UnpavedEnums.CropType), i)] =
                            sobekUnpaved.CropAreas[i];
                    }

                    //drainage formule
                    IDrainageFormula drainageFormula;
                    switch (sobekUnpaved.ComputationOption)
                    {
                        case SobekUnpavedComputationOption.Ernst:
                            drainageFormula = new ErnstDrainageFormula();
                            SetErnstDrainageFormula((ErnstDrainageFormula)drainageFormula, dicErnst,
                                                    sobekUnpaved.ErnstId);
                            break;
                        case SobekUnpavedComputationOption.HellingaDeZeeuw:
                            drainageFormula = new DeZeeuwHellingaDrainageFormula();
                            SetAlfaDrainageFormula((DeZeeuwHellingaDrainageFormula)drainageFormula, dicAlfa,
                                                   sobekUnpaved.AlfaLevelId);
                            break;
                        case SobekUnpavedComputationOption.KrayenhoffVanDeLeur:
                            drainageFormula = new KrayenhoffVanDeLeurDrainageFormula
                            {
                                ResevoirCoefficient = sobekUnpaved.ReservoirCoefficient
                            };
                            break;
                        default:
                            drainageFormula = new ErnstDrainageFormula();
                            log.ErrorFormat(
                                "Computational option {0} of {1} is not supported. Drainage formula 'Ernst' has been set.",
                                sobekUnpaved.ComputationOption, sobekUnpaved.Id);
                            break;
                    }

                    unpaved.DrainageFormula = drainageFormula;

                    //meteo
                    unpaved.MeteoStationName = sobekUnpaved.MeteoStationId;
                    unpaved.AreaAdjustmentFactor = sobekUnpaved.AreaAjustmentFactor;

                    //Storage
                    if (dicSto.ContainsKey(sobekUnpaved.StorageId))
                    {
                        var storage = dicSto[sobekUnpaved.StorageId];
                        unpaved.MaximumLandStorage = storage.MaxLandStorage;
                        unpaved.InitialLandStorage = storage.InitialLandStorage;
                    }
                    else
                    {
                        log.ErrorFormat("Storage table {0} has not been found.", sobekUnpaved.StorageId);
                    }

                    //groundwater
                    unpaved.GroundWaterLayerThickness = sobekUnpaved.InitialDepthGroundwaterLayer;
                    unpaved.MaximumAllowedGroundWaterLevel = sobekUnpaved.MaximumGroundwaterLevel;
                    if (!String.IsNullOrEmpty(sobekUnpaved.InitialGroundwaterLevelTableId))
                    {
                        unpaved.InitialGroundWaterLevelSource = UnpavedEnums.GroundWaterSourceType.Series;
                        if (dicGroundWater.ContainsKey(sobekUnpaved.InitialGroundwaterLevelTableId))
                        {
                            unpaved.InitialGroundWaterLevelSeries = DataTableHelper.ConvertDataTableToTimeSeries(
                                    dicGroundWater[sobekUnpaved.InitialGroundwaterLevelTableId], "Groundwater");
                        }
                        else
                        {
                            unpaved.InitialGroundWaterLevelSeries = new TimeSeries();
                            log.ErrorFormat("Groundwater table {0} has not been found.",
                                            sobekUnpaved.InitialGroundwaterLevelTableId);
                        }
                    }
                    else
                    {
                        if (sobekUnpaved.InitialGroundwaterLevelFromBoundary)
                        {
                            unpaved.InitialGroundWaterLevelSource = UnpavedEnums.GroundWaterSourceType.FromLinkedNode;
                            unpaved.InitialGroundWaterLevelConstant = -1; //not used
                        }
                        else
                        {
                            unpaved.InitialGroundWaterLevelSource = UnpavedEnums.GroundWaterSourceType.Constant;
                            unpaved.InitialGroundWaterLevelConstant = sobekUnpaved.InitialGroundwaterLevelConstant;
                        }
                    }

                    //infiltration 
                    if (dicInf.ContainsKey(sobekUnpaved.InfiltrationId))
                    {
                        var infiltration = dicInf[sobekUnpaved.InfiltrationId];
                        unpaved.InfiltrationCapacity = infiltration.InfiltrationCapacity;
                    }
                    else
                    {
                        log.ErrorFormat("Infiltration table {0} has not been found.", sobekUnpaved.InfiltrationId);
                    }

                    ////Seepage
                    if (dicSep.ContainsKey(sobekUnpaved.SeepageId))
                    {
                        var seepage = dicSep[sobekUnpaved.SeepageId];
                        unpaved.SeepageH0HydraulicResistance = seepage.ResistanceValue;

                        switch (seepage.ComputationOption)
                        {
                            case SeepageComputationOption.Constant:
                                unpaved.SeepageConstant = seepage.Seepage;
                                unpaved.SeepageSource = UnpavedEnums.SeepageSourceType.Constant;
                                break;
                            case SeepageComputationOption.VariableH0:
                                if (dicH0.ContainsKey(seepage.H0TableName))
                                {
                                    unpaved.SeepageH0Series =
                                        DataTableHelper.ConvertDataTableToTimeSeries(dicH0[seepage.H0TableName], "H0");
                                    unpaved.SeepageSource = UnpavedEnums.SeepageSourceType.H0Series;
                                }
                                else
                                {
                                    log.ErrorFormat("H0 table {0} has not been found.", seepage.H0TableName);
                                }
                                break;
                            case SeepageComputationOption.VariableModFlow:
                                log.WarnFormat("Seepage computation option {0} is not supported yet.",
                                               seepage.ComputationOption);
                                unpaved.SeepageSource = UnpavedEnums.SeepageSourceType.Series;
                                unpaved.SeepageSeries =
                                    DataTableHelper.ConvertDataTableToTimeSeries(seepage.SaltTableConcentration,
                                                                                 "Seepage");
                                log.WarnFormat("Seepage computation option has been set to series");
                                break;
                            case SeepageComputationOption.TimeTable:
                            case SeepageComputationOption.TimeTableAndSaltConcentration:
                                unpaved.SeepageSource = UnpavedEnums.SeepageSourceType.Series;
                                unpaved.SeepageSeries =
                                    DataTableHelper.ConvertDataTableToTimeSeries(seepage.SaltTableConcentration,
                                                                                 "Seepage");
                                break;
                            default:
                                log.ErrorFormat("Seepage computation option {0} is not supported yet.",
                                                seepage.ComputationOption);
                                break;

                        }

                    }
                    else
                    {
                        log.ErrorFormat("Seepage table {0} has not been find.", sobekUnpaved.SeepageId);
                    }
                }
                else
                {
                    log.WarnFormat("Rainfall runoff area with id {0} has not been found. Item has been skipped...",
                                   sobekUnpaved.Id);
                }
            }
        }

        private void SetAlfaDrainageFormula(DeZeeuwHellingaDrainageFormula drainageFormula, Dictionary<string, SobekRRAlfa> dicAlfa, string alfaLevelId)
        {
            if (dicAlfa.ContainsKey(alfaLevelId))
            {
                var sobekRRAlfa = dicAlfa[alfaLevelId];

                drainageFormula.SurfaceRunoff = sobekRRAlfa.FactorSurface;
                drainageFormula.HorizontalInflow = sobekRRAlfa.FactorInfiltration;
                drainageFormula.InfiniteDrainageLevelRunoff = sobekRRAlfa.FactorLastLayer;

                var levels = new[] { sobekRRAlfa.Level1, sobekRRAlfa.Level2, sobekRRAlfa.Level3 };
                var values = new[]
                                 {
                                     sobekRRAlfa.FactorTopSoil, sobekRRAlfa.FactorSecondLayer,
                                     sobekRRAlfa.FactorThirdLayer
                                 };

                AssignLevelsAndValuesToDrainageFormula(drainageFormula, values, levels);
            }
            else
            {
                log.ErrorFormat("De Zeeuw-Hellinga drainage formula {0} has not been found", alfaLevelId);
            }
        }

        private static void AssignLevelsAndValuesToDrainageFormula(ErnstDeZeeuwHellingaDrainageFormulaBase drainageFormula, double[] values, double[] levels)
        {
            //When adding layers, sobek works from level 3 to level 1. We want to work from level 1 to 
            //level 3 (but both in the same order: descending from the surface). If only one level is 
            //active in sobek, it's stored as level 3, whereas it would be level 1 for us. So we have 
            //to shift the first non-zero layer to our level 1. In case all layers are used, there is 
            //no difference between Sobek and DS.

            var index =
                values.Select((v, i) => new { v, i }).Where(a => a.v != 0).Select(a => a.i).FirstOrDefault();

            if (index < levels.Length)
            {
                drainageFormula.LevelOneTo = levels[index];
                drainageFormula.LevelOneValue = values[index];
                drainageFormula.LevelOneEnabled = true;
            }
            index++;

            if (index < levels.Length)
            {
                drainageFormula.LevelTwoTo = levels[index];
                drainageFormula.LevelTwoValue = values[index];
                drainageFormula.LevelTwoEnabled = true;
            }
            index++;

            if (index < levels.Length)
            {
                drainageFormula.LevelThreeTo = levels[index];
                drainageFormula.LevelThreeValue = values[index];
                drainageFormula.LevelThreeEnabled = true;
            }
        }

        private void SetErnstDrainageFormula(ErnstDrainageFormula drainageFormula, Dictionary<string, SobekRRErnst> dicErnst, string ernstId)
        {
            if (dicErnst.ContainsKey(ernstId))
            {
                var sobekRRErnst = dicErnst[ernstId];

                drainageFormula.SurfaceRunoff = sobekRRErnst.ResistanceSurface;
                drainageFormula.HorizontalInflow = sobekRRErnst.ResistanceInfiltration;
                drainageFormula.InfiniteDrainageLevelRunoff = sobekRRErnst.ResistanceLayer4;

                var levels = new[] { sobekRRErnst.Level1, sobekRRErnst.Level2, sobekRRErnst.Level3 };
                var values = new[]
                                 {
                                     sobekRRErnst.ResistanceLayer1, sobekRRErnst.ResistanceLayer2,
                                     sobekRRErnst.ResistanceLayer3
                                 };
                AssignLevelsAndValuesToDrainageFormula(drainageFormula, values, levels);
            }
            else
            {
                log.ErrorFormat("Ernst drainage formula {0} has not been found", ernstId);
            }
        }

    }
}