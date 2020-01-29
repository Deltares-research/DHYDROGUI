using DelftTools.Hydro;
using DeltaShell.NGHS.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DelftTools.Utils.Collections;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public class NwrwFileWriter : NGHSFileBase
    {
        private const string NWRW_3B_FILENAME = "pluvius.3b";
        private const string NWRW_ALG_FILENAME = "pluvius.alg";
        private const string NWRW_DWA_FILENAME = "pluvius.dwa";

        private const string DFEAULT_GENERAL_ID = "-1";
        private const string DEFAULT_INFILTRATION_FROM_DEPRESSIONS = "1";
        private const string DEFAULT_INFILTRATION_FROM_RUNOFF = "0";
        private const double DEFAULT_DOUBLE = 0.0;

        private const string NUMBER_OF_PEOPLE_TIMES_CONSTANT_DWA_PER_CAPITA_PER_HOUR = "1";
        private const string NUMBER_OF_PEOPLE_TIMES_VARIABLE_DWA_PER_CAPITA_PER_HOUR = "2";
        private const string ONE_TIMES_CONSTANT_DWA_PER_HOUR = "3";
        private const string ONE_TIMES_VARIABLE_DWA_PER_HOUR = "4";
        private const string USING_A_TABLE = "5";

        private static ILog Log = LogManager.GetLogger(typeof(NwrwFileWriter));

        private NwrwSurfaceType[] SurfaceTypesInCorrectOrder { get; } =
        {
            NwrwSurfaceType.ClosedPavedWithSlope,   // a1
            NwrwSurfaceType.ClosedPavedFlat,        // a2
            NwrwSurfaceType.ClosedPavedFlatStretch, // a3
            NwrwSurfaceType.OpenPavedWithSlope,     // a4
            NwrwSurfaceType.OpenPavedFlat,          // a5
            NwrwSurfaceType.OpenPavedFlatStretched, // a6
            NwrwSurfaceType.RoofWithSlope,          // a7
            NwrwSurfaceType.RoofFlat,               // a8
            NwrwSurfaceType.RoofFlatStretched,      // a9
            NwrwSurfaceType.UnpavedWithSlope,       // a10
            NwrwSurfaceType.UnpavedFlat,            // a11
            NwrwSurfaceType.UnpavedFlatStretched    // a12
        };


        public void WriteNwrwFiles(IHydroModel model, string path)
        {
            var rrModel = model as RainfallRunoffModel;
            if (rrModel == null || path == null) return;

            var nwrwDatas = rrModel.GetAllModelData().OfType<NwrwData>();
            var dryWeatherFlowDefinitions = rrModel.NwrwDryWeatherFlowDefinitions;

            WriteNwrw3bFile(nwrwDatas, path);
            WriteNwrwAlgFile(rrModel, path);
            WriteNwrwDwaFile(dryWeatherFlowDefinitions, path);
            //WriteNwrwTableFile(); // todo
        }

        private void WriteNwrw3bFile(IEnumerable<NwrwData> nwrwDatas, string path)
        {
            if (nwrwDatas == null || path == null) return;

            var filePath = Path.Combine(Path.GetFullPath(path), NWRW_3B_FILENAME);
            var listOfErrors = new List<string>();

            OpenOutputFile(filePath);
            try
            {
                foreach (NwrwData nwrwData in nwrwDatas)
                {
                    try
                    {
                        StringBuilder line = CreateNwrw3bLine(nwrwData);
                        WriteLine(line.ToString());
                    }
                    catch (Exception e)
                    {
                        listOfErrors.Add(e.Message + Environment.NewLine);
                    }
                }
            }
            finally
            {
                CloseOutputFile();
                if (listOfErrors.Any())
                    Log.ErrorFormat($"While writing to '{NWRW_3B_FILENAME}' we encountered the following errors: {Environment.NewLine}{string.Join(Environment.NewLine, listOfErrors)}");
            }
        }

        private void WriteNwrwAlgFile(RainfallRunoffModel rrModel, string path)
        {
            if (rrModel == null || path == null) return;
            var filePath = Path.Combine(Path.GetFullPath(path), NWRW_ALG_FILENAME);
            
            OpenOutputFile(filePath);
            try
            {
                StringBuilder line = CreateNwrwAlgLine(rrModel.NwrwDefinitions);
                WriteLine(line.ToString());
            }
            catch (Exception e)
            {
                Log.ErrorFormat($"While writing to '{NWRW_ALG_FILENAME}' we encountered the following error: {Environment.NewLine} {e.Message}");
            }
            finally
            {
                CloseOutputFile();
            }
        }

        private void WriteNwrwDwaFile(IEnumerable<NwrwDryWeatherFlowDefinition> dryWeatherFlowDefinitions, string path)
        {
            if (dryWeatherFlowDefinitions == null || path == null) return;
            
            var filePath = Path.Combine(Path.GetFullPath(path), NWRW_DWA_FILENAME);
            var listOfErrors = new List<string>();

            OpenOutputFile(filePath);
            try
            {
                foreach (NwrwDryWeatherFlowDefinition dryWeatherFlowDefinition in dryWeatherFlowDefinitions)
                {
                    try
                    {
                        StringBuilder line = CreateNwrwDwaLine(dryWeatherFlowDefinition);
                        WriteLine(line.ToString());
                    }
                    catch (Exception e)
                    {
                        listOfErrors.Add(e.Message + Environment.NewLine);
                    }
                }
            }
            finally
            {
                CloseOutputFile();
                if (listOfErrors.Any())
                    Log.ErrorFormat($"While writing to '{NWRW_DWA_FILENAME}' we encountered the following errors: {Environment.NewLine}{string.Join(Environment.NewLine, listOfErrors)}");
            }
        }



        #region .dwa
        private StringBuilder CreateNwrwDwaLine(NwrwDryWeatherFlowDefinition dryWeatherFlowDefinition)
        {
            StringBuilder line = new StringBuilder();

            AppendOpeningTagToDwaLine(line); // 'DWA'
            AppendIdToDwaLine(line, dryWeatherFlowDefinition); // 'id'
            AppendNameToDwaLine(line, dryWeatherFlowDefinition); // 'nm'
            
            switch (dryWeatherFlowDefinition.DistributionType)
            {
                case DwfDistributionType.Constant:
                    AppendConstantPropertiesToDwaLine(line, dryWeatherFlowDefinition);
                    break;
                case DwfDistributionType.Daily:
                    AppendDailyPropertiesToDwaLine(line, dryWeatherFlowDefinition);
                    break;
                case DwfDistributionType.Variable:
                    throw new ArgumentException($"'{nameof(DwfDistributionType.Variable)}' is not yet supported.");
                default:
                    throw new ArgumentException($"Invalid distribution type was provided.");
            } // 'do' 'wc' 'wd' 'wh'
            AppendClosingTagToDwaLine(line); // 'dwa'

            return line;
        }
        private void AppendConstantPropertiesToDwaLine(StringBuilder line, NwrwDryWeatherFlowDefinition dryWeatherFlowDefinition)
        {
            AppendDwfComputationOptionToDwaLine(line, NUMBER_OF_PEOPLE_TIMES_VARIABLE_DWA_PER_CAPITA_PER_HOUR); // 'do'
            AppendWaterUsePerCapitaAsConstantToDwaLine(line, dryWeatherFlowDefinition.DailyVolume); // 'wc'
            AppendWaterUsePerCapitaPerDayToDwaLine(line, DEFAULT_DOUBLE); // 'wd'
            AppendWaterUsePerHour(line, dryWeatherFlowDefinition); // 'wh'
        }
        private void AppendDailyPropertiesToDwaLine(StringBuilder line, NwrwDryWeatherFlowDefinition dryWeatherFlowDefinition)
        {
            AppendDwfComputationOptionToDwaLine(line, NUMBER_OF_PEOPLE_TIMES_CONSTANT_DWA_PER_CAPITA_PER_HOUR); // 'do'
            AppendWaterUsePerCapitaAsConstantToDwaLine(line, DEFAULT_DOUBLE); // 'wc'
            AppendWaterUsePerCapitaPerDayToDwaLine(line, dryWeatherFlowDefinition.DailyVolume); // 'wd'
            AppendWaterUsePerHour(line, dryWeatherFlowDefinition);  // 'wh'
        }
        private void AppendOpeningTagToDwaLine(StringBuilder line)
        {
            line.Append(NwrwKeywords.DwaOpeningKey);
            line.Append(" ");
        }
        private void AppendIdToDwaLine(StringBuilder line, NwrwDryWeatherFlowDefinition dryWeatherFlowDefinition)
        {
            line.Append(NwrwKeywords.IdKey);
            line.Append(" ");
            line.Append("'");
            line.Append(dryWeatherFlowDefinition.Name);
            line.Append("'");
            line.Append(" ");
        }
        private void AppendNameToDwaLine(StringBuilder line, NwrwDryWeatherFlowDefinition dryWeatherFlowDefinition)
        {
            line.Append(NwrwKeywords.NameKey);
            line.Append(" ");
            line.Append("'");
            line.Append(dryWeatherFlowDefinition.Name); // same as id
            line.Append("'");
            line.Append(" ");
        }
        private void AppendDwfComputationOptionToDwaLine(StringBuilder line, string option)
        {
            line.Append(NwrwKeywords.DwaComputationOptionKey); // do
            line.Append(" ");
            line.Append(option);
            line.Append(" ");
        }
        private void AppendWaterUsePerCapitaAsConstantToDwaLine(StringBuilder line, double waterUseConstant)
        {
            line.Append(NwrwKeywords.DwaWaterUsePerCapitaConstantValuePerHourKey);
            line.Append(" ");
            line.Append(waterUseConstant.ToString());
            line.Append(" ");
            
        }
        private void AppendWaterUsePerCapitaPerDayToDwaLine(StringBuilder line, double waterUseDaily)
        {
            line.Append(NwrwKeywords.DwaWaterUsePerCapitaPerDayKey);
            line.Append(" ");
            line.Append(waterUseDaily.ToString());
            line.Append(" ");
        }
        private void AppendWaterUsePerHour(StringBuilder line, NwrwDryWeatherFlowDefinition dryWeatherFlowDefinition)
        {
            line.Append(NwrwKeywords.DwaWaterUsePerCapitaPerHourKey);
            line.Append(" ");
            for (int i = 0; i < 23; i++)
            {
                line.Append(dryWeatherFlowDefinition.HourlyPercentageDailyVolume[i]);
                line.Append(" ");
            }
        }
        private void AppendClosingTagToDwaLine(StringBuilder line)
        {
            line.Append(NwrwKeywords.DwaClosingKey);
        }
        #endregion

        #region .alg
        private StringBuilder CreateNwrwAlgLine(IList<NwrwDefinition> nwrwDefinitions)
        {
            StringBuilder line = new StringBuilder();

            AppendOpeningTagToAlgLine(line);
            AppendIdToAlgLine(line);
            AppendNameToAlgLine(line);
            AppendRunoffDelayFactorToAlgLine(line, nwrwDefinitions);
            AppendMaximumStorageToAlgLine(line, nwrwDefinitions);
            AppendMaximumInfiltrationCapacityToAlgLine(line, nwrwDefinitions);
            AppendMinimumInfiltrationCapacityToAlgLine(line, nwrwDefinitions);
            AppendDecreaseInfiltrationCapacityToAlgLine(line, nwrwDefinitions);
            AppendIncreaseInfiltrationCapacityToAlgLine(line, nwrwDefinitions);
            AppendInfiltrationFromDepressionToAlgLine(line, nwrwDefinitions);
            AppendInfiltrationFromRunoffToAlgLine(line, nwrwDefinitions);
            AppendClosingTagToAlgLine(line);

            return line;
        }

        private void AppendOpeningTagToAlgLine(StringBuilder line)
        {
            // opening tag
            line.Append("PLVG");
            line.Append(" ");
        }

        private void AppendIdToAlgLine(StringBuilder line)
        {
            // id
            line.Append(NwrwKeywords.IdKey);
            line.Append(" ");
            line.Append("'");
            line.Append(DFEAULT_GENERAL_ID);
            line.Append("'");
            line.Append(" ");
        }

        private void AppendNameToAlgLine(StringBuilder line)
        {
            // name
            line.Append(NwrwKeywords.NameKey);
            line.Append(" ");
            line.Append("'");
            line.Append(String.Empty); // empty
            line.Append("'");
            line.Append(" ");
        }

        private void AppendRunoffDelayFactorToAlgLine(StringBuilder line, IList<NwrwDefinition> nwrwDefinitions)
        {
            // runoff-delay factor for 3 types of slopes (with slope, flat, flat stretched)
            line.Append(NwrwKeywords.RunoffDelayFactor);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.ClosedPavedWithSlope))?.RunoffSlope);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.ClosedPavedFlat))?.RunoffSlope);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.ClosedPavedFlatStretch))?.RunoffSlope);
            line.Append(" ");
        }

        private void AppendMaximumStorageToAlgLine(StringBuilder line, IList<NwrwDefinition> nwrwDefinitions)
        {
            // maximum storage for 12 types
            line.Append(NwrwKeywords.MaximumStorage);
            line.Append(" ");
            foreach (NwrwSurfaceType nwrwSurfaceType in SurfaceTypesInCorrectOrder)
            {
                line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(nwrwSurfaceType))?.SurfaceStorage);
                line.Append(" ");
            }
        }

        private void AppendMaximumInfiltrationCapacityToAlgLine(StringBuilder line, IList<NwrwDefinition> nwrwDefinitions)
        {
            // maximum infiltration capacity for 4 types of surface (closed paved, open paved, roofs, unpaved)
            line.Append(NwrwKeywords.MaximumInfiltrationCapacity);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.ClosedPavedWithSlope))?.InfiltrationCapacityMax);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.OpenPavedWithSlope))?.InfiltrationCapacityMax);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.RoofWithSlope))?.InfiltrationCapacityMax);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.UnpavedWithSlope))?.InfiltrationCapacityMax);
            line.Append(" ");
        }

        private void AppendMinimumInfiltrationCapacityToAlgLine(StringBuilder line, IList<NwrwDefinition> nwrwDefinitions)
        {
            // minimum infiltration capacity for 4 types of surface (closed paved, open paved, roofs, unpaved)
            line.Append(NwrwKeywords.MinimumInfiltrationCapacity);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.ClosedPavedWithSlope))?.InfiltrationCapacityMin);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.OpenPavedWithSlope))?.InfiltrationCapacityMin);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.RoofWithSlope))?.InfiltrationCapacityMin);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.UnpavedWithSlope))?.InfiltrationCapacityMin);
            line.Append(" ");
        }

        private void AppendDecreaseInfiltrationCapacityToAlgLine(StringBuilder line, IList<NwrwDefinition> nwrwDefinitions)
        {
            // decrease in infiltration capacity for 4 types of surface (closed paved, open paved, roofs, unpaved)
            line.Append(NwrwKeywords.DecreaseInInfiltrationCapacity);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.ClosedPavedWithSlope))?.InfiltrationCapacityReduction);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.OpenPavedWithSlope))?.InfiltrationCapacityReduction);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.RoofWithSlope))?.InfiltrationCapacityReduction);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.UnpavedWithSlope))?.InfiltrationCapacityReduction);
            line.Append(" ");
        }

        private void AppendIncreaseInfiltrationCapacityToAlgLine(StringBuilder line, IList<NwrwDefinition> nwrwDefinitions)
        {
            // increase in infiltration capacity for 4 types of surface (closed paved, open paved, roofs, unpaved)
            line.Append(NwrwKeywords.IncreaseInInfiltrationCapacity);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.ClosedPavedWithSlope))?.InfiltrationCapacityRecovery);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.OpenPavedWithSlope))?.InfiltrationCapacityRecovery);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.RoofWithSlope))?.InfiltrationCapacityRecovery);
            line.Append(" ");
            line.Append(nwrwDefinitions.FirstOrDefault(nd => nd.SurfaceType.Equals(NwrwSurfaceType.UnpavedWithSlope))?.InfiltrationCapacityRecovery);
            line.Append(" ");
        }

        private void AppendInfiltrationFromDepressionToAlgLine(StringBuilder line, IList<NwrwDefinition> nwrwDefinitions)
        {
            // option for infiltration from depressions
            line.Append(NwrwKeywords.InfiltrationFromDepressions);
            line.Append(" ");
            line.Append(DEFAULT_INFILTRATION_FROM_DEPRESSIONS);
            line.Append(" ");
        }

        private void AppendInfiltrationFromRunoffToAlgLine(StringBuilder line, IList<NwrwDefinition> nwrwDefinitions)
        {
            // option for infiltration from runoff
            line.Append(NwrwKeywords.InfiltrationFromRunoff);
            line.Append(" ");
            line.Append(DEFAULT_INFILTRATION_FROM_RUNOFF);
            line.Append(" ");
        }

        private void AppendClosingTagToAlgLine(StringBuilder line)
        {
            // closing tag
            line.Append("plvg");
        }

        #endregion
        
        #region .3b
        private StringBuilder CreateNwrw3bLine(NwrwData nwrwData)
        {
            StringBuilder line = new StringBuilder();

            AppendOpeningTagTo3bLine(line); // 'NWRW'
            AppendIdTo3bLine(line, nwrwData.NodeOrBranchId); // 'id'
            AppendSurfaceLevelTo3bLine(line, nwrwData.LateralSurface); // 'sl'
            AppendAreaTo3bLine(line, nwrwData.SurfaceLevelDict); // 'ar'
            AppendDryWeatherFlowsTo3bLine(line, nwrwData.DryWeatherFlows); // 'np' 'dw' 'np2' 'dw2'
            AppendMeteoStationIdTo3bLine(line, nwrwData.MeteoStationId); // 'ms'
            AppendSpecialAreasTo3bLine(line, nwrwData.NumberOfSpecialAreas, nwrwData.SpecialAreas); // 'na'
            AppendClosingTagTo3bLine(line); // 'nwrw'

            return line;
        }

        private void AppendOpeningTagTo3bLine(StringBuilder line)
        {
            // 'NWRW' opening keyword
            line.Append(NwrwKeywords.NwrwOpeningKey);
            line.Append(" ");
        }

        private void AppendIdTo3bLine(StringBuilder line, string id)
        {
            // 'id' + node identification
            line.Append(NwrwKeywords.IdKey);
            line.Append(" ");
            line.Append("'");
            line.Append(id);
            line.Append("'");
            line.Append(" ");
        }

        private void AppendSurfaceLevelTo3bLine(StringBuilder line, double surfaceLevel)
        {
            // 'sl' + surface level (in m) (optional input data)
            if (Math.Abs(surfaceLevel) > 0.001)
            {
                line.Append(NwrwKeywords.SurfaceLevelKey);
                line.Append(" ");
                line.Append(surfaceLevel);
                line.Append(" ");
            }
        }

        private void AppendAreaTo3bLine(StringBuilder line, IDictionary<NwrwSurfaceType, double> surfaceLevelDict)
        {
            // 'ar' + area (12 types) as combination of 3 kind of slopes and 4 types of surfaces
            line.Append(NwrwKeywords.AreaKey);
            line.Append(" ");

            

            foreach (NwrwSurfaceType surfaceType in SurfaceTypesInCorrectOrder)
            {
                if (surfaceLevelDict.ContainsKey(surfaceType))
                {
                    line.Append(surfaceLevelDict[surfaceType]);

                }
                else
                {
                    line.Append("0");
                }
                line.Append(" ");
            }
        }

        private void AppendDryWeatherFlowsTo3bLine(StringBuilder line,
            IList<DryWeatherFlow> nwrwDataDryWeatherFlows)
        {
            var numberOfDryWeatherFlows = nwrwDataDryWeatherFlows.Count;
            if (numberOfDryWeatherFlows >= 1)
            {
                line.Append(NwrwKeywords.FirstNumberOfUnitsKey);
                line.Append(" ");
                line.Append(nwrwDataDryWeatherFlows[0].NumberOfUnits);
                line.Append(" ");
                line.Append(NwrwKeywords.FirstDryWeatherFlowIdKey);
                line.Append(" ");
                line.Append("'");
                line.Append(nwrwDataDryWeatherFlows[0].DryWeatherFlowId);
                line.Append("'");
                line.Append(" ");
            }

            if (numberOfDryWeatherFlows >= 2)
            {
                line.Append(NwrwKeywords.SecondNumberOfUnitsKey);
                line.Append(" ");
                line.Append(nwrwDataDryWeatherFlows[1].NumberOfUnits);
                line.Append(" ");
                line.Append(NwrwKeywords.SecondDryWeatherFlowIdKey);
                line.Append(" ");
                line.Append("'");
                line.Append(nwrwDataDryWeatherFlows[1].DryWeatherFlowId);
                line.Append("'");
                line.Append(" ");
            }
        }

        private void AppendMeteoStationIdTo3bLine(StringBuilder line, string meteostationId)
        {
            // 'ms' + identification of the meteostation
            line.Append(NwrwKeywords.MeteostationIdKey);
            line.Append(" ");
            line.Append("'");
            line.Append(meteostationId);
            line.Append("'");
            line.Append(" ");
        }

        private void AppendSpecialAreasTo3bLine(StringBuilder line, int numberOfSpecialAreas, IList<NwrwSpecialArea> specialAreas)
        {
            if (numberOfSpecialAreas > 0)
            {
                AppendNumberOfSpecialAreasTo3bLine(line, numberOfSpecialAreas);
                AppendAllSpecialAreasTo3bLine(line, specialAreas);
            }
        }

        private void AppendNumberOfSpecialAreasTo3bLine(StringBuilder line, int numberOfSpecialAreas)
        {
            // 'na' + number of special areas with special inflow characteristics
            line.Append(NwrwKeywords.NumberOfSpecialAreasKey);
            line.Append(" ");
            line.Append(numberOfSpecialAreas);
            line.Append(" ");
        }

        private void AppendAllSpecialAreasTo3bLine(StringBuilder line, IList<NwrwSpecialArea> specialAreas)
        {
            // 'aa' + special area in m2 (for number of areas as specified after the 'na' keyword
            line.Append(NwrwKeywords.SpecialAreaKey);
            line.Append(" ");
            foreach (NwrwSpecialArea specialArea in specialAreas)
            {
                line.Append(specialArea.Area);
                line.Append(" ");
            }
        }

        private void AppendClosingTagTo3bLine(StringBuilder line)
        {
            line.Append("nwrw");
        }
        #endregion
    }
}
