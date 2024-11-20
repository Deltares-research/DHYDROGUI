using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Converters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.DataAccessObjects;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Files.Fnm;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.FileWriters;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter
{
    public class RRModelHybridFileWriter : IRRModelHybridFileWriter
    {
        private const string sobek3BfnmFileName = "Sobek_3b.fnm";
        private static readonly FnmFileWriter fnmFileWriter = new FnmFileWriter();
        private static readonly EvaporationFileNameConverter evaporationFileNameConverter = new EvaporationFileNameConverter();
        private string iniFile;
        private readonly List<string> pavedData = new List<string>();
        private readonly List<string> pavedStorage = new List<string>();
        private readonly List<string> pavedDwf = new List<string>();
        private readonly List<string> pavedPumpCapacitiesTable = new List<string>();
        private readonly List<string> unpavedData = new List<string>();
        private readonly List<string> unpavedStorage = new List<string>();
        private readonly List<string> unpavedSeepage = new List<string>();
        private readonly List<string> unpavedInfiltration = new List<string>();
        private readonly List<string> unpavedTable = new List<string>();
        private readonly List<string> unpavedHellingaErnst = new List<string>();
        private readonly List<string> greenhouseData = new List<string>();
        private readonly List<string> greenhouseSiloData = new List<string>();
        private readonly List<string> greenhouseRoofStorageData = new List<string>();
        private readonly List<string> sacramentoData = new List<string>();
        private readonly List<string> sacramentoCapacities = new List<string>();
        private readonly List<string> sacramentoUnitHydrograph = new List<string>();
        private readonly List<string> sacramentoOtherParameters = new List<string>();
        private readonly List<string> nodes = new List<string>();
        private readonly List<string> links = new List<string>();
        private readonly List<string> boundaries = new List<string>();
        private readonly List<string> wwtp = new List<string>();
        private readonly List<string> openWaterData = new List<string>();
        private Dictionary<string, List<DelftTools.Utils.Tuple<string, string>>> iniOptions;
        private readonly List<string> pavedIds = new List<string>();
        private readonly List<NodeType> typeForNodes = new List<NodeType>();
        private readonly List<string> unpavedIds = new List<string>();
        public IOEvaporationMeteoDataSource EvaporationMeteoDataSource { get; set; }
        
        private readonly IManifestRetriever manifestRetriever = new ManifestRetriever();

        public RRModelHybridFileWriter()
        {
            SetIniFile();
        }

        private void WriteFnmFile()
        {
            string evaporationFileName = evaporationFileNameConverter.ToFileName(EvaporationMeteoDataSource);
            
            var fnmFile = new FnmFile
            {
                Evaporation = { FileName = evaporationFileName }
            };
            
            using (StreamWriter writer = File.CreateText(sobek3BfnmFileName))
            {
                fnmFileWriter.Write(fnmFile, writer);
            }
        }

        public bool GenerateRRModelFiles()
        {
            WriteFixedFiles();
            WriteFnmFile();

            File.WriteAllText("DELFT_3B.INI", iniFile);
            File.WriteAllText("Paved.3b", String.Join("\r\n", pavedData.ToArray()));
            File.WriteAllText("Paved.sto", String.Join("\r\n", pavedStorage.ToArray()));
            File.WriteAllText("Paved.dwa", String.Join("\r\n", pavedDwf.ToArray()));
            File.WriteAllText("Paved.tbl", String.Join("\r\n", pavedPumpCapacitiesTable.ToArray()));

            File.WriteAllText("Unpaved.3b", String.Join("\r\n", unpavedData.ToArray()));
            File.WriteAllText("Unpaved.alf", String.Join("\r\n", unpavedHellingaErnst.ToArray()));
            File.WriteAllText("Unpaved.sto", String.Join("\r\n", unpavedStorage.ToArray()));
            File.WriteAllText("Unpaved.sep", String.Join("\r\n", unpavedSeepage.ToArray()));
            File.WriteAllText("Unpaved.inf", String.Join("\r\n", unpavedInfiltration.ToArray()));
            File.WriteAllText("Unpaved.tbl", String.Join("\r\n", unpavedTable.ToArray()));

            File.WriteAllText("Greenhse.3b", String.Join("\r\n", greenhouseData.ToArray()));
            File.WriteAllText("Greenhse.rf", String.Join("\r\n", greenhouseRoofStorageData.ToArray()));
            File.WriteAllText("Greenhse.sil", String.Join("\r\n", greenhouseSiloData.ToArray()));

            File.WriteAllText("Sacrmnto.3b", String.Join("\r\n", sacramentoData.ToArray()));
            File.WriteAllText("Sacrmnto.cap", String.Join("\r\n", sacramentoCapacities.ToArray()));
            File.WriteAllText("Sacrmnto.uh", String.Join("\r\n", sacramentoUnitHydrograph.ToArray()));
            File.WriteAllText("Sacrmnto.oth", String.Join("\r\n", sacramentoOtherParameters.ToArray()));

            File.WriteAllText("OpenWate.3b", String.Join("\r\n", openWaterData.ToArray()));

            File.WriteAllText("WWTP.3b", String.Join("\r\n", wwtp.ToArray()));
            File.WriteAllText("WWTP.tbl", "");
            
            File.WriteAllText("3B_NOD.TP", String.Join("\r\n", nodes.ToArray()));
            File.WriteAllText("3B_LINK.TP", String.Join("\r\n", links.ToArray()));

            //empty files:
            File.WriteAllText("3b_rout.3b", "");
            File.WriteAllText("pluvius.tbl", "");
            return true;
        }

        private void WriteFixedFiles()
        {
            foreach (string resourceName in manifestRetriever.FixedResources)
            {
                using (Stream resourceStream = manifestRetriever.GetFixedStream(resourceName))
                using (var streamReader = new StreamReader(resourceStream))
                {
                    File.WriteAllText(resourceName, streamReader.ReadToEnd());
                }
            }
        }

        private static DateTime ConvertFromSobekDateTime(int date, int time)
        {
            int year, month, day, hour, minute, second;
            SplitSobekDateTimeFormat(date, out year, out month, out day);
            SplitSobekDateTimeFormat(time, out hour, out minute, out second);
            return new DateTime(year, month, day, hour, minute, second);
        }
        
        private static void SplitSobekDateTimeFormat(int date, out int year, out int month, out int day)
        {
            year = date/10000;
            var remainder = date - (year*10000);
            month = remainder/100;
            remainder -= (month*100);
            day = remainder;
        }

        public bool SetSimulationTimesAndGenerateIniFile(int startDate, int startTime, int endDate, int endTime, int timeStep, int outputTimeStep)
        {
            using (CultureUtils.SwitchToInvariantCulture())
            {
                var startDateTime = ConvertFromSobekDateTime(startDate, startTime);
                var endDateTime = ConvertFromSobekDateTime(endDate, endTime);

                var sb = new StringBuilder();
                foreach (var k in iniOptions.Keys.ToList())
                {
                    sb.AppendLine("[" + k + "]");
                    foreach (var v in iniOptions[k])
                    {
                        sb.AppendLine(v.First + "=" + v.Second);
                    }
                    sb.AppendLine();
                }
                
                iniFile = sb.ToString();

                iniFile += $"TimestepSize={timeStep}\r\n" + 
                           $"StartTime='{startDateTime.Year:00}/{startDateTime.Month:00}/{startDateTime.Day:00};{startDateTime.Hour:00}:{startDateTime.Minute:00}:{startDateTime.Second:00}'\r\n" + 
                           $"EndTime='{endDateTime.Year:00}/{endDateTime.Month:00}/{endDateTime.Day:00};{endDateTime.Hour:00}:{endDateTime.Minute:00}:{endDateTime.Second:00}'\r\n";
            }
            
            return true;
        }

        public int AddPaved(string id, double area, double streetLevel, double initialStreetStorage, double maximumStreetStorage, 
            double initialRainfallSewerStorage, double maximumRainfallSewerStorage, double initialDwfSewerStorage,
            double maximumDwfSewerStorage, SewerType sewerType, bool sewerCapacityIsFixed, double rainfallSewerCapacity, double dwfSewerCapacity, 
            LinkType rainfallSewerLink, LinkType dwfSewerLink,
            int numberOfPeople,
            DwfComputationOption dwfComputationOption, double[] waterUsePerCapitaPerHourInDay, double runoffCoefficient, string meteoId, double areaAdjustmentFactor,
            double x, double y)
        {
            if (waterUsePerCapitaPerHourInDay.Length != 24)
            {
                throw new ArgumentException("waterUsePerCapitaPerHourInDay should be length 24");
            }

            pavedIds.Add(id);

            var storageId = id + "_storage";
            var dryWaterId = id + "_dwf";

            AddNodeInternal(id, NodeType.Paved, x, y);

            using (CultureUtils.SwitchToInvariantCulture())
            {
                var capacityString = sewerCapacityIsFixed
                                         ? $"qc 0 {rainfallSewerCapacity} {dwfSewerCapacity}"
                                         : $"qc 1 '{id}_qc'";

                pavedData.Add(
                    $"PAVE id '{id}' ar {area} lv {streetLevel} sd '{storageId}' ss {(int) sewerType} {capacityString} qo {(int) dwfSewerLink} {(int) rainfallSewerLink} ms '{meteoId}' {GetAreaAdjustmentFactorString(areaAdjustmentFactor)}is 0 np {numberOfPeople} dw '{dryWaterId}' ro {((Math.Abs(runoffCoefficient) < double.Epsilon) ? 0 : 1)} ru {runoffCoefficient} pave");

                pavedStorage.Add($"STDF id '{storageId}' nm '{storageId}' ms {maximumStreetStorage} is {initialStreetStorage} mr {maximumRainfallSewerStorage} {maximumDwfSewerStorage} ir {initialRainfallSewerStorage} {initialDwfSewerStorage} stdf");

                var waterUseString = GetWaterUseString(dwfComputationOption, waterUsePerCapitaPerHourInDay);

                pavedDwf.Add($"DWA id '{dryWaterId}' nm '{dryWaterId}' do {(int) dwfComputationOption} {waterUseString} dwa");
            }
            
            return pavedData.Count; 
        }

        public int AddGreenhouse(string id, double[] areasPerGreenhouseClass, double surfaceLevel,
            double initialRoofStorage, double maximumRoofStorage, double siloCapacity,
            double siloPumpCapacity, bool greenhouseUseSiloArea, double greenhouseSiloArea,
            string meteoId, double areaAdjustmentFactor,
            double x, double y)
        {
            AddNodeInternal(id, NodeType.Greenhouse, x, y);

            var greenhouseAreaConnectedToSilo = (greenhouseUseSiloArea ? greenhouseSiloArea : 0.0);

            using (CultureUtils.SwitchToInvariantCulture())
            {
                greenhouseData.Add(
                    $"GRHS id '{id}' na 10  ar {areasPerGreenhouseClass[0]} {areasPerGreenhouseClass[1]} {areasPerGreenhouseClass[2]} {areasPerGreenhouseClass[3]} {areasPerGreenhouseClass[4]} {areasPerGreenhouseClass[5]} {areasPerGreenhouseClass[6]} {areasPerGreenhouseClass[7]} {areasPerGreenhouseClass[8]} {areasPerGreenhouseClass[9]} sl {surfaceLevel} as {greenhouseAreaConnectedToSilo} si '{id}' sd '{id}' ms '{meteoId}' {GetAreaAdjustmentFactorString(areaAdjustmentFactor)}is 0.0 grhs"
                );

                greenhouseSiloData.Add($"SILO id '{id}' nm '{id}' sc {siloCapacity} pc {siloPumpCapacity} silo");

                greenhouseRoofStorageData.Add($"STDF id '{id}' nm '{id}' mk {maximumRoofStorage} ik {initialRoofStorage} stdf");
            }

            return greenhouseData.Count;
        }

        public int AddOpenWater(string id, double area, string meteoId, double areaAdjustmentFactor,
            double x, double y)
        {
            AddNodeInternal(id, NodeType.Openwater, x, y);

            using (CultureUtils.SwitchToInvariantCulture())
            {
                openWaterData.Add($"OWRR id '{id}' ar {area} ms '{meteoId}'{GetAreaAdjustmentFactorString(areaAdjustmentFactor)} owrr");
            }

            return openWaterData.Count;
        }

        public int AddSacramento(string id, double area, double[] parameters, double[] capacities, double hydrographStep,
            double[] hydroGraphValues, string meteoId,
            double x, double y)
        {
            AddNodeInternal(id, NodeType.Sacramento, x, y);

            var ca_id = id + "_ca";
            var uh_id = id + "_uh";
            var op_id = id + "_op";
            
            using (CultureUtils.SwitchToInvariantCulture())
            {
                sacramentoData.Add($"SACR id '{id}' ar {area} ms '{meteoId}' ca '{ca_id}' uh '{uh_id}' op '{op_id}' sacr");

                sacramentoData.Add(
                    $"OPAR id '{op_id}' zperc {parameters[0]} rexp {parameters[1]} pfree {parameters[2]} rserv {parameters[3]} pctim {parameters[4]} adimp {parameters[5]} sarva {parameters[6]} side {parameters[7]} ssout {parameters[8]} pm {parameters[9]} pt1 {parameters[10]} pt2 {parameters[11]} opar");

                sacramentoData.Add(
                    $"CAPS id '{ca_id}' uztwm {capacities[0]} uztwc {capacities[1]} uzfwm {capacities[2]} uzfwc {capacities[3]} lztwm {capacities[4]} lztwc {capacities[5]} lzfsm {capacities[6]} lzfsc {capacities[7]} lzfpm {capacities[8]} lzfpc {capacities[9]} uzk {capacities[10]} lzsk {capacities[11]} lzpk {capacities[12]} caps");

                sacramentoData.Add($"UNIH id '{uh_id}' uh {String.Join(" ", hydroGraphValues)} dt {hydrographStep} unih");
            }

            return sacramentoData.Count;
        }

        public int AddHbv(string id, double area, double surfaceLevel, double[] snowParameters, double[] soilParameters,
            double[] flowParameters, double[] hiniParameters, string meteoId, double areaAdjustmentFactor,
            string tempId, double x, double y)
        {
            AddNodeInternal(id, NodeType.Hbv, x, y);
            
            var snow_id = id + "_sn";
            var soil_id = id + "_sl";
            var flow_id = id + "_fl";
            var hini_id = id + "_hini";

            using (CultureUtils.SwitchToInvariantCulture())
            {
                sacramentoData.Add(
                    $"HBV id '{id}' ar {area} sl {surfaceLevel} snow '{snow_id}' soil '{soil_id}' flow '{flow_id}' hini '{hini_id}' ts '{tempId}' ms '{meteoId}' {GetAreaAdjustmentFactorString(areaAdjustmentFactor)} hbv");

                sacramentoData.Add($"SNOW id '{snow_id}' nm '{snow_id}' mc {snowParameters[0]} sft {snowParameters[1]} smt {snowParameters[2]} tac {snowParameters[3]} fe {snowParameters[4]} fwf {snowParameters[5]} snow");

                sacramentoData.Add($"SOIL id '{soil_id}' nm '{soil_id}' be {soilParameters[0]} fc {soilParameters[1]} ef {soilParameters[2]} soil");

                sacramentoData.Add($"FLOW id '{flow_id}' nm '{flow_id}' kb {flowParameters[0]} ki {flowParameters[1]} kq {flowParameters[2]} qt {flowParameters[3]} mp {flowParameters[4]} flow");

                sacramentoData.Add($"HINI id '{hini_id}' nm '{hini_id}' ds {hiniParameters[0]} fw {hiniParameters[1]} sm {hiniParameters[2]} uz {hiniParameters[3]} lz {hiniParameters[4]} hini");
            }

            return sacramentoData.Count;
        }

        public int AddWasteWaterTreatmentPlant(string id, double x, double y)
        {
            wwtp.Add($"WWTP id '{id}' tb 0 wwtp");
            return AddNodeInternal(id, NodeType.Wwtp, x, y);
        }

        public int AddBoundaryNode(string id, double initialWaterLevel, double x, double y)
        {
            //set initial water level (actual water level is passed during runtime)
            boundaries.Add($"BOUN id '{id}' bl 0 {initialWaterLevel} is 0 boun");  //-99 m AD as default (outside)
            return AddNodeInternal(id, NodeType.BoundaryOrLateral, x, y);
        }

        public void AddLink(string linkId, string from, string to)
        {
            using (CultureUtils.SwitchToInvariantCulture())
            {
                links.Add($"BRCH id '{linkId}' ri '-1' mt 1 '0' bt 17 ObID '3B_LINK' bn '{@from}' en '{to}' brch");
            }
        }

        private static string GetWaterUseString(DwfComputationOption dwfComputationOption, double[] waterUsePerCapitaPerHourInDay)
        {
            //for all dwf's the absolute water use is 55l/day. The 120 is default (unused data)
            //
            //DWA id '1' nm 'inhab_const_dwf' do 1 wc 2.291667 wd 120 wh 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 dwa
            //DWA id '2' nm 'inhab_var_dwf'   do 2 wc 0        wd 55  wh 100 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 dwa
            //DWA id '3' nm '1_const_dwf'     do 3 wc 2.291667 wd 120 wh 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 dwa
            //DWA id '4' nm '1_var_dwf'       do 4 wc 0        wd 55  wh 100 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 dwa

            var totalWaterUsePerCapitaPerDay = waterUsePerCapitaPerHourInDay.Sum();

            var isVariable = (dwfComputationOption == DwfComputationOption.NumberOfInhabitantsTimesVariableDWF ||
                              dwfComputationOption == DwfComputationOption.VariableDWF);

            var waterUsePerHourForConstant = isVariable ? 0.0 : totalWaterUsePerCapitaPerDay/24.0;
            var waterUsePerDayForVariable = isVariable ? totalWaterUsePerCapitaPerDay : 0.0;

            var waterUsePercentagesString = GetWaterUsePercentagesString(dwfComputationOption, waterUsePerCapitaPerHourInDay, totalWaterUsePerCapitaPerDay);

            return $"wc {waterUsePerHourForConstant} wd {waterUsePerDayForVariable} wh {waterUsePercentagesString}";
        }

        private int AddNodeInternal(string id, NodeType nodeType, double x, double y)
        {
            using (CultureUtils.SwitchToInvariantCulture())
            {
                string sobekNodeId = GetSobekNodeId(nodeType);
                nodes.Add($"NODE id '{id}' nm '{id}' ri '-1' mt 1 {sobekNodeId} px {x} py {y} node");
                typeForNodes.Add(nodeType);
            }

            return typeForNodes.Count(t => t == nodeType);
        }

        private static string GetAreaAdjustmentFactorString(double areaAdjustmentFactor)
        {
            var areaAdjustmentFactorString = Math.Abs(areaAdjustmentFactor - 1.0) < 0.000001 //if ~= 1.0
                ? ""
                : $" aaf {areaAdjustmentFactor} ";
            return areaAdjustmentFactorString;
        }

        private static string GetWaterUsePercentagesString(DwfComputationOption dwfComputationOption, double[] waterUsePerCapitaPerHourInDay, double waterUsePerCapitaPerDay)
        {
            var isVariableMultiplier = (dwfComputationOption == DwfComputationOption.NumberOfInhabitantsTimesVariableDWF ||
                                        dwfComputationOption == DwfComputationOption.VariableDWF)
                ? 1.0
                : 0.0;

            var waterUsePercentages =
                waterUsePerCapitaPerHourInDay.Select(
                    wp => (Math.Abs(waterUsePerCapitaPerDay) < double.Epsilon ? 0 : isVariableMultiplier*100.0*(wp/waterUsePerCapitaPerDay)).
                        ToString(CultureInfo.InvariantCulture));

            return String.Join(" ", waterUsePercentages.ToArray());
        }

        private static string GetSobekNodeId(NodeType nodeType)
        {
            switch(nodeType)
            {
                case NodeType.Paved:
                    return "'1' nt 43 ObID '3B_PAVED'";
                case NodeType.Unpaved:
                    return "'2' nt 44 ObID '3B_UNPAVED'";
                case NodeType.Greenhouse:
                    return "'3' nt 45 ObID '3B_GREENHOUSE'";
                case NodeType.BoundaryOrLateral:
                    return "'6' nt 78 ObID 'SBK_SBK-3B-NODE'";
                case NodeType.Wwtp:
                    return "'14' nt 56 ObID '3B_WWTP'";
                case NodeType.Openwater:
                    return "'21' nt 67 ObID 'OW_PRECIP'";
                case NodeType.Sacramento:
                    return "'16' nt 54 ObID '3B_SACRAMENTO'";
                case NodeType.Hbv:
                    return "'19' nt 63 ObID '3B_HBV'";
            }
            throw new NotSupportedException("Unknown node type");

        }

        public void SetPavedVariablePumpCapacities(int iref, int[] dates, int[] times, double[] mixedCapacity, double[] dwfCapacity)
        {
            using (CultureUtils.SwitchToInvariantCulture())
            {
                var pavedId = pavedIds[iref - 1];
                var capacityTableId = pavedId + "_qc";

                pavedPumpCapacitiesTable.Add(
                    $"QC_T id '{capacityTableId}' PDIN 1 1 '31536000' pdin\n{WriteTable(dates, times, mixedCapacity, dwfCapacity)}qc_t");
            }
        }

        private static string WriteTable(int[] dates, int[] times, double[] values, double[] values2=null)
        {
            if (dates.Length != values.Length || dates.Length != times.Length)
            {
                throw new ArgumentException($"Number of datetimes {dates.Length} and number of values {values.Length} do not match");
            }

            var tableString = new StringBuilder();
            tableString.Append("    TBLE\n");
            
            for (int i = 0; i < dates.Length; i++ )
            {
                var time = ConvertFromSobekDateTime(dates[i], times[i]);
                tableString.Append($"        '{time.Year}/{time.Month:00}/{time.Day:00};{time.Hour:00}:{time.Minute:00}:{time.Second:00}' {values[i]}{(values2 != null ? "    " + values2[i] : "")} <\n");
            }

            tableString.Append("    tble\n");
            return tableString.ToString();
        }

        public void SetUnpavedConstantSeepage(int iref, double seepage)
        {
            using (CultureUtils.SwitchToInvariantCulture())
            {
                var seepageId = unpavedIds[iref - 1] + "_seepage";
                unpavedSeepage.Add($"SEEP id '{seepageId}' nm '{seepageId}' co {1} sp {seepage} ss 0 seep");
            }
        }

        public void SetUnpavedVariableSeepage(int iref, SeepageComputationOption seepageComputationOption, double resistanceC, int[] h0Dates, int[] h0Times, double[] h0Table)
        {
            using (CultureUtils.SwitchToInvariantCulture())
            {
                var unpavedId = unpavedIds[iref - 1];
                var seepageId = unpavedId + "_seepage";
                var h0TableId = unpavedId + "_h0table";
                unpavedSeepage.Add($"SEEP id '{seepageId}' nm '{seepageId}' co {(int)seepageComputationOption} cv {resistanceC} h0 '{h0TableId}' ss 0 seep");
                unpavedTable.Add($"H0_T id '{h0TableId}' PDIN 1 1 '31536000' pdin\n{WriteTable(h0Dates, h0Times, h0Table)}h0_t");
            }
        }

        public int AddUnpaved(string id, double[] areasForKnownCropTypes, double areaForGroundwaterCalculations, double surfaceLevel, 
            DrainageComputationOption drainageComputationOption, double reservoirCoefficient, double initialLandStorage, 
            double maximumLandStorage, double infiltrationCapacity, 
            int soilType, double initialGroundwaterLevel, double maximumAllowedGroundwater,
            double groundwaterLayerThickness,
            string meteoId, double areaAdjustmentFactor,
            double x, double y)
        {
            if (areasForKnownCropTypes.Length != 16)
            {
                throw new ArgumentException("Length for areasForKnownCropTypes should be 16");
            }

            AddNodeInternal(id, NodeType.Unpaved, x, y);

            using (CultureUtils.SwitchToInvariantCulture())
            {
                string drainage = GetDrainageId(drainageComputationOption, id);

                var seepageId = id + "_seepage";
                var storageId = id + "_storage";
                var infiltrationId = id + "_infilt";

                unpavedIds.Add(id);
                unpavedData.Add(
                    $"UNPV id '{id}' na 16 ar {String.Join(" ", areasForKnownCropTypes.Select(a => a.ToString(CultureInfo.InvariantCulture)).ToArray())} lv {surfaceLevel} ga {areaForGroundwaterCalculations} co {(int)drainageComputationOption} su 0 sd '{storageId}' rc {reservoirCoefficient}{drainage} sp '{seepageId}' ic '{infiltrationId}' bt {soilType} ig 0 {initialGroundwaterLevel} mg {maximumAllowedGroundwater} gl {groundwaterLayerThickness} ms '{meteoId}' {GetAreaAdjustmentFactorString(areaAdjustmentFactor)}is 0 unpv");

                unpavedStorage.Add($"STDF id '{storageId}' nm '{storageId}' ml {maximumLandStorage} il {initialLandStorage} stdf");

                unpavedInfiltration.Add($"INFC id '{infiltrationId}' nm '{infiltrationId}' ic {infiltrationCapacity} infc");
            }

            return unpavedData.Count;
        }

        private static string GetDrainageId(DrainageComputationOption drainageComputationOption, string id)
        {
            var drainage = "";
            switch (drainageComputationOption)
            {
                case DrainageComputationOption.DeZeeuwHellinga:
                    drainage = $" ad '{id + "_hellinga"}'";
                    break;
                case DrainageComputationOption.Ernst:
                    drainage = $" ed '{id + "_ernst"}'";
                    break;
                case DrainageComputationOption.KrayenhoffVdLeur:
                    drainage = "";
                    break;
            }
            return drainage;
        }

        public int SetErnst(int iref, double surfaceRunoff, double lastLayerRunoff, double infiltration, double[] belowSurfaceLevels, double[] belowSurfaceDrainage)
        {
            if (belowSurfaceDrainage.Length != 3 || belowSurfaceLevels.Length != 3)
            {
                throw new NotSupportedException("Only 3 layers is supported");
            }

            using (CultureUtils.SwitchToInvariantCulture())
            {
                var ernstId = unpavedIds[iref - 1] + "_ernst";

                unpavedHellingaErnst.Add(
                    $"ERNS id '{ernstId}' nm '{ernstId}' cvs {surfaceRunoff} cvo {belowSurfaceDrainage[0]} {belowSurfaceDrainage[1]} {belowSurfaceDrainage[2]} {lastLayerRunoff} cvi {infiltration} lv {belowSurfaceLevels[0]} {belowSurfaceLevels[1]} {belowSurfaceLevels[2]} erns");
            }

            return 0; //?
        }

        public int SetDeZeeuwHellinga(int iref, double surfaceRunoff, double lastLayerRunoff, double infiltration, double[] belowSurfaceLevels, double[] belowSurfaceDrainage)
        {
            if (belowSurfaceDrainage.Length != 3 || belowSurfaceLevels.Length != 3)
            {
                throw new NotSupportedException("Only 3 layers is supported");
            }

            using (CultureUtils.SwitchToInvariantCulture())
            {
                var hellingaId = unpavedIds[iref - 1] + "_hellinga";

                unpavedHellingaErnst.Add(
                    $"ALFA id '{hellingaId}' nm '{hellingaId}' af {surfaceRunoff} {belowSurfaceDrainage[0]} {belowSurfaceDrainage[1]} {belowSurfaceDrainage[2]} {lastLayerRunoff} {infiltration} lv {belowSurfaceLevels[0]} {belowSurfaceLevels[1]} {belowSurfaceLevels[2]} alfa");
            }

            return 0; //?
        }

        private void SetIniFile()
        {
            iniOptions = new Dictionary<string, List<DelftTools.Utils.Tuple<string, string>>>
            {
                {
                    "System",
                    new List<DelftTools.Utils.Tuple<string,string>>
                    {
                        new DelftTools.Utils.Tuple<string, string>("CaseName",""),
                        new DelftTools.Utils.Tuple<string, string>("Debugfile","-1"),
                        new DelftTools.Utils.Tuple<string, string>("Version","2.11")
                    }
                },
                {
                    "OutputOptions",
                    new List<DelftTools.Utils.Tuple<string, string>>
                    {
                        new DelftTools.Utils.Tuple<string, string>("OutputTimestep","1"),
                        new DelftTools.Utils.Tuple<string, string>("OutputEvent","0"),
                        new DelftTools.Utils.Tuple<string, string>("OutputOverall","1"),
                        new DelftTools.Utils.Tuple<string, string>("OutputDetail","1"),
                        new DelftTools.Utils.Tuple<string, string>("OutputScreen","0"),
                        new DelftTools.Utils.Tuple<string, string>("OutputOpenwater","-1"),
                        new DelftTools.Utils.Tuple<string, string>("OutputGroundwater","0"),
                        new DelftTools.Utils.Tuple<string, string>("OutputBoundary","1"),
                        new DelftTools.Utils.Tuple<string, string>("OutputAtTimestep","1"),
                        new DelftTools.Utils.Tuple<string, string>("OutputAtTimestepOption","1"),
                        new DelftTools.Utils.Tuple<string, string>("OutputRRPaved","-1"),
                        new DelftTools.Utils.Tuple<string, string>("OutputRRUnpaved","-1"),
                        new DelftTools.Utils.Tuple<string, string>("ExtendedBalance","-1"),
                        new DelftTools.Utils.Tuple<string, string>("OutputRRGreenhouse","-1"),
                        new DelftTools.Utils.Tuple<string, string>("OutputRROpenWater","-1"),
                        new DelftTools.Utils.Tuple<string, string>("OutputRRStructure","0"),
                        new DelftTools.Utils.Tuple<string, string>("OutputRRBoundary","-1"),
                        new DelftTools.Utils.Tuple<string, string>("OutputRRNWRW","-1"),
                        new DelftTools.Utils.Tuple<string, string>("OutputRRWWTP","-1"),
                        new DelftTools.Utils.Tuple<string, string>("OutputRRIndustry","0"),
                        new DelftTools.Utils.Tuple<string, string>("OutputRRSacramento","-1"),
                        new DelftTools.Utils.Tuple<string, string>("OutputRRBalance","-1"),
                        new DelftTools.Utils.Tuple<string, string>("OutputRRSalt","0"),
                        new DelftTools.Utils.Tuple<string, string>("OutputRRLinkFlows","-1"),
                    }
                },
                {
                    "Options",
                    new List<DelftTools.Utils.Tuple<string,string>>
                    {
                        new DelftTools.Utils.Tuple<string, string>("InitGwlOption","-1"),
                        new DelftTools.Utils.Tuple<string, string>("MaalstopModule","0"),
                        new DelftTools.Utils.Tuple<string, string>("NWRWContinuous","0"),
                        new DelftTools.Utils.Tuple<string, string>("RestartFileNameEachTimestepOption","0"),
                        new DelftTools.Utils.Tuple<string, string>("RestartFileNamePrefix","''"),
                    }
                },
                {
                    "TimeSettings",
                    new List<DelftTools.Utils.Tuple<string,string>>
                    {
                        new DelftTools.Utils.Tuple<string, string>("EvaporationFromHrs","7"),
                        new DelftTools.Utils.Tuple<string, string>("EvaporationToHrs","19"),
                        new DelftTools.Utils.Tuple<string, string>("PeriodFromEvent","0"),
                    }
                }
            };
        }

        public void WriteFiles()
        {
            using (CultureUtils.SwitchToInvariantCulture())
            {
                GenerateRRModelFiles();
            }
        }

        public void AddIniOption(string section, string property, string value)
        {
            if (String.IsNullOrEmpty(section) || String.IsNullOrEmpty(property))
            {
                return;
            }

            if (iniOptions.Keys.Contains(section))
            {
                var val = iniOptions[section];
                var kvp = val.FirstOrDefault(v => v.First.Equals(property, StringComparison.InvariantCultureIgnoreCase));
                if (kvp != null)
                    kvp.Second = value;
                else  
                    val.Add(new DelftTools.Utils.Tuple<string, string>(property, value));
                return;
            }
            iniOptions.Add(section, new List<DelftTools.Utils.Tuple<string, string>>
            {
                new DelftTools.Utils.Tuple<string, string>(property, value)
            });
        }
    }
}