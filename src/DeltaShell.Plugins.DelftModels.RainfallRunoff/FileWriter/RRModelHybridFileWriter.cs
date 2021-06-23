using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter
{
    public class RRModelHybridFileWriter : IRRModelHybridFileWriter
    {
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
        private readonly Dictionary<string, double[]> precipitationPerStation = new Dictionary<string, double[]>();
        private readonly Dictionary<string, double[]> evaporationPerStation = new Dictionary<string, double[]>();
        private readonly Dictionary<string, double[]> temperaturePerStation = new Dictionary<string, double[]>();
        private int startDateMeteoData;
        private int startTimeMeteoData;
        private int timeStepInSeconds;
        private CultureInfo oldCulture;
        private Dictionary<string, List<DelftTools.Utils.Tuple<string, string>>> iniOptions;
        private readonly List<string> pavedIds = new List<string>();
        private readonly List<NodeType> typeForNodes = new List<NodeType>();
        private readonly List<string> unpavedIds = new List<string>();

        public RRModelHybridFileWriter()
        {
            SetIniFile();
        }
        public bool GenerateRRModelFiles()
        {
            WriteFixedFiles();

            File.WriteAllText("DELFT_3B.INI", iniFile);
            File.WriteAllText("DEFAULT.BUI", FlushPrecipitationData());
            File.WriteAllText("DEFAULT.EVP", FlushEvaporationData());
            File.WriteAllText("DEFAULT.TMP", FlushTemperatureData());

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
            var currentNamespace = GetType().Namespace;
            var assembly = GetType().Assembly;
            var embeddedResources = assembly.GetManifestResourceNames();

            var fixedResourcePrefix = String.Format("{0}.Fixed.", currentNamespace);

            foreach (var resourceName in embeddedResources.Where(n => n.StartsWith(fixedResourcePrefix)))
            {
                var resourceContents = assembly.GetManifestResourceStream(resourceName);
                if (resourceContents != null)
                {
                    var fileName = resourceName.Substring(fixedResourcePrefix.Length);
                    var streamReader = new StreamReader(resourceContents);
                    File.WriteAllText(fileName, streamReader.ReadToEnd());
                }
            }
        }

        private string FlushPrecipitationData()
        {
            var sb = new StringBuilder();

            var valuesOfFirstStation = precipitationPerStation.Values.FirstOrDefault();

            if (valuesOfFirstStation == null)
            {
                return "";
            }

            if (startDateMeteoData == 0)
            {
                throw new InvalidOperationException("SetMeteoDataStartTimeAndInterval must be called before initialize");
            }

            SwitchToInvariantCulture();

            sb.AppendLine("1");
            sb.AppendLine("*Aantal stations");
            sb.AppendLine(precipitationPerStation.Keys.Count.ToString());
            sb.AppendLine("*Namen van stations");
            foreach (var station in precipitationPerStation.Keys)
            {
                sb.AppendLine("'" + station + "'");
            }
            sb.AppendLine("*Aantal gebeurtenissen (omdat het 1 bui betreft is dit altijd 1)");
            sb.AppendLine("*en het aantal seconden per waarnemingstijdstap");
            sb.AppendLine("1 " + timeStepInSeconds);
            sb.AppendLine("*Elke commentaarregel wordt begonnen met een * (asteriks).");

            var numTimeSteps = valuesOfFirstStation.Length;
            var startDateTime = ConvertFromSobekDateTime(startDateMeteoData, startTimeMeteoData);
            var diff = TimeSpan.FromSeconds(numTimeSteps * timeStepInSeconds);

            sb.AppendLine(String.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}", startDateTime.Year, startDateTime.Month, startDateTime.Day,
                startDateTime.Hour, startDateTime.Minute, startDateTime.Second,
                (int)diff.TotalDays, diff.Hours, diff.Minutes, diff.Seconds));

            for (int i = 0; i < valuesOfFirstStation.Length; i++)
            {
                foreach (var station in precipitationPerStation.Keys)
                {
                    sb.Append(precipitationPerStation[station][i] + " ");
                }
                sb.AppendLine();
            }

            //extra timestep
            foreach (var station in precipitationPerStation.Keys)
            {
                sb.Append("0 ");
            }

            RestoreCulture();

            precipitationPerStation.Clear();

            return sb.ToString();
        }

        private string FlushEvaporationData()
        {
            var sb = new StringBuilder();

            var valuesOfFirstStation = evaporationPerStation.Values.FirstOrDefault();

            if (valuesOfFirstStation == null)
            {
                return "";
            }

            SwitchToInvariantCulture();

            sb.AppendLine("*Verdampingsfile");
            sb.AppendLine("*Meteo data: evaporation intensity in mm/day");
            sb.AppendLine("*First record: start date, data in mm/day");
            sb.AppendLine("*Datum (year month day), verdamping (mm/dag) voor elk weerstation");
            sb.AppendLine("*jaar maand dag verdamping[mm]");

            var currentDate = ConvertFromSobekDateTime(startDateMeteoData, startTimeMeteoData);
            for (int i = 0; i < valuesOfFirstStation.Length; i++)
            {
                sb.Append(String.Format("{0}    {1}    {2}    ",
                    currentDate.Year,
                    currentDate.Month,
                    currentDate.Day));

                foreach (var station in evaporationPerStation.Keys)
                {
                    sb.Append(evaporationPerStation[station][i] + " ");
                }
                sb.AppendLine();

                currentDate = currentDate.AddDays(1);
            }

            RestoreCulture();

            evaporationPerStation.Clear();

            return sb.ToString();
        }

        private string FlushTemperatureData()
        {
            var sb = new StringBuilder();

            var valuesOfFirstStation = temperaturePerStation.Values.FirstOrDefault();

            if (valuesOfFirstStation == null)
            {
                return "";
            }

            if (startDateMeteoData == 0)
            {
                throw new InvalidOperationException("SetMeteoDataStartTimeAndInterval must be called before initialize");
            }

            SwitchToInvariantCulture();

            sb.AppendLine("1");
            sb.AppendLine("*Aantal stations");
            sb.AppendLine(temperaturePerStation.Keys.Count.ToString());
            sb.AppendLine("*Namen van stations");
            foreach (var station in temperaturePerStation.Keys)
            {
                sb.AppendLine("'" + station + "'");
            }
            sb.AppendLine("*Aantal gebeurtenissen (omdat het 1 bui betreft is dit altijd 1)");
            sb.AppendLine("*en het aantal seconden per waarnemingstijdstap");
            sb.AppendLine("1 " + timeStepInSeconds);
            sb.AppendLine("*Elke commentaarregel wordt begonnen met een * (asteriks).");

            var numTimeSteps = valuesOfFirstStation.Length;
            var startDateTime = ConvertFromSobekDateTime(startDateMeteoData, startTimeMeteoData);
            var diff = TimeSpan.FromSeconds(numTimeSteps * timeStepInSeconds);

            sb.AppendLine(String.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}", startDateTime.Year, startDateTime.Month, startDateTime.Day,
                startDateTime.Hour, startDateTime.Minute, startDateTime.Second,
                (int)diff.TotalDays, diff.Hours, diff.Minutes, diff.Seconds));

            for (int i = 0; i < valuesOfFirstStation.Length; i++)
            {
                foreach (var station in temperaturePerStation.Keys)
                {
                    sb.Append(temperaturePerStation[station][i] + " ");
                }
                sb.AppendLine();
            }

            RestoreCulture();

            temperaturePerStation.Clear();

            return sb.ToString();
        }

        public void SwitchToInvariantCulture()
        {
            oldCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
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

        public void RestoreCulture()
        {
            Thread.CurrentThread.CurrentCulture = oldCulture;
        }

        public bool SetSimulationTimesAndGenerateIniFile(int startDate, int startTime, int endDate, int endTime, int timeStep, int outputTimeStep)
        {
            SwitchToInvariantCulture();

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

            RestoreCulture();

            iniFile = sb.ToString();

            SwitchToInvariantCulture();

            //todo: outputtimestep

            const string iniFileFormat = "TimestepSize={12}\r\n" +
                                         "StartTime='{0:00}/{1:00}/{2:00};{3:00}:{4:00}:{5:00}'\r\n" +
                                         "EndTime='{6:00}/{7:00}/{8:00};{9:00}:{10:00}:{11:00}'\r\n";

            iniFile += String.Format(iniFileFormat,
                startDateTime.Year, startDateTime.Month, startDateTime.Day,
                startDateTime.Hour, startDateTime.Minute, startDateTime.Second,
                endDateTime.Year, endDateTime.Month, endDateTime.Day,
                endDateTime.Hour, endDateTime.Minute, endDateTime.Second,
                timeStep);
     
            RestoreCulture();

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

            SwitchToInvariantCulture();

            pavedIds.Add(id);

            var storageId = id + "_storage";
            var dryWaterId = id + "_dwf";

            AddNodeInternal(id, NodeType.Paved, x, y);

            var capacityString = sewerCapacityIsFixed
                ? String.Format("qc 0 {0} {1}", rainfallSewerCapacity, dwfSewerCapacity)
                : String.Format("qc 1 '{0}_qc'", id);

            pavedData.Add(
                String.Format(
                    "PAVE id '{0}' ar {1} lv {2} sd '{3}' ss {4} {5} qo {9} {10} ms '{6}' {13}is 0 np {7} dw '{8}' ro {11} ru {12} pave",
                    id, area, streetLevel, storageId, (int) sewerType, capacityString, meteoId,
                    numberOfPeople, dryWaterId, (int) dwfSewerLink, (int) rainfallSewerLink,
                    (Math.Abs(runoffCoefficient) < double.Epsilon) ? 0 : 1, runoffCoefficient, GetAreaAdjustmentFactorString(areaAdjustmentFactor)));

            pavedStorage.Add(String.Format("STDF id '{0}' nm '{0}' ms {1} is {2} mr {3} {4} ir {5} {6} stdf", storageId,
                maximumStreetStorage, initialStreetStorage, maximumRainfallSewerStorage,
                maximumDwfSewerStorage, initialRainfallSewerStorage, initialDwfSewerStorage));

            var waterUseString = GetWaterUseString(dwfComputationOption, waterUsePerCapitaPerHourInDay);

            pavedDwf.Add(String.Format("DWA id '{0}' nm '{0}' do {1} {2} dwa", dryWaterId,
                (int) dwfComputationOption, waterUseString));

            RestoreCulture();

            return pavedData.Count; 
        }

        public int AddGreenhouse(string id, double[] areasPerGreenhouseClass, double surfaceLevel,
            double initialRoofStorage, double maximumRoofStorage, double siloCapacity,
            double siloPumpCapacity, bool greenhouseUseSiloArea, double greenhouseSiloArea,
            string meteoId, double areaAdjustmentFactor,
            double x, double y)
        {

            SwitchToInvariantCulture();

            AddNodeInternal(id, NodeType.Greenhouse, x, y);

            var siloId = (greenhouseUseSiloArea ? id : "-1");
            var greenhouseAreaConnectedToSilo = (greenhouseUseSiloArea ? greenhouseSiloArea : 0.0);
            
            greenhouseData.Add(
                String.Format(
                    "GRHS id '{0}' na 10  ar {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} sl {11} as {12} si '{13}' sd '{14}' ms '{15}' {16}is 0.0 grhs",
                    id, areasPerGreenhouseClass[0], areasPerGreenhouseClass[1], areasPerGreenhouseClass[2],
                    areasPerGreenhouseClass[3], areasPerGreenhouseClass[4], areasPerGreenhouseClass[5],
                    areasPerGreenhouseClass[6], areasPerGreenhouseClass[7], areasPerGreenhouseClass[8],
                    areasPerGreenhouseClass[9], surfaceLevel, greenhouseAreaConnectedToSilo, siloId, id, meteoId,
                    GetAreaAdjustmentFactorString(areaAdjustmentFactor))
            );

            if (greenhouseUseSiloArea)
            {
                greenhouseSiloData.Add(String.Format("SILO id '{0}' nm '{0}' sc {1} pc {2} silo", id, siloCapacity,
                    siloPumpCapacity));
            }

            greenhouseRoofStorageData.Add(String.Format("STDF id '{0}' nm '{0}' mk {1} ik {2} stdf", id,
                maximumRoofStorage, initialRoofStorage));

            RestoreCulture();

            return greenhouseData.Count;
        }

        public int AddOpenWater(string id, double area, string meteoId, double areaAdjustmentFactor,
            double x, double y)
        {
            SwitchToInvariantCulture();

            AddNodeInternal(id, NodeType.Openwater, x, y);
            
            openWaterData.Add(String.Format("OWRR id '{0}' ar {1} ms '{2}'{3} owrr", id, area, meteoId,
                GetAreaAdjustmentFactorString(areaAdjustmentFactor)));

            /*
            TODO: Openwater catchments are not fully / incorrectly implemented, see issue: SOBEK3-784
            TODO: Openwater seepage definitions should also be implemented (see other catchments for examples)
            TODO: Openwater tables target levels should also be implemented

            Implementation of openWaterData.Add(...), from Documentation: 
             
                OPWA id '1' ml 0.0 rl 0.0 al 2 na 6 ar 10000. 110000. 120000. 130000 14000. 150000.
                lv -1. -0.8 -0.6 -0.4 -0.2 0. bl -2.0 tl 0 -0.9 sp 'seep_1' ms 'meteostat1' is 75.0 opwa
             
            With:
               + id  -   node identification
               + al  -   area-level relation (only used by model edit) 1= constantarea, 2=interpolation, 3=lineair
               + na  -   number of area/level combinations. Default 6. NOT READ.
               + ar  -   6 values of area (in m2)
               + lv  -   6 corresponding values of level (m NAP) in increasing order
               + ml  -   maximum allowable level (m NAP)
               + rl  -   reference level (m NAP)
               + bl  -   bottom level (m NAP) Default 1 meter below lowest value from area-level relation.
               + sp  -   seepage definition.
               + ms  -   identification of the meteostation
               + is  -   initial salt concentration (mg/l)
               + tl  -   target level; constant or reference to a table.
                         tl 0 -0.9 = initial groundwater level as a constant, with value -0.9 m NAP.
                         tl 1 'Tlv-Table' = target open water levels as a table, with table id Tlv-Table.      

            TODO: Many of these properties do not yet exist for OpenWaterCatchments in DeltaShell

            */

            RestoreCulture();

            return openWaterData.Count;
        }

        public int AddSacramento(string id, double area, double[] parameters, double[] capacities, double hydrographStep,
            double[] hydroGraphValues, string meteoId,
            double x, double y)
        {
            SwitchToInvariantCulture();

            AddNodeInternal(id, NodeType.Sacramento, x, y);

            var ca_id = id + "_ca";
            var uh_id = id + "_uh";
            var op_id = id + "_op";

            sacramentoData.Add(String.Format("SACR id '{0}' ar {1} ms '{2}' ca '{3}' uh '{4}' op '{5}' sacr", id, area,
                meteoId, ca_id, uh_id, op_id));

            // TODO: add checks on arrays

            sacramentoData.Add(
                String.Format(
                    "OPAR id '{0}' zperc {1} rexp {2} pfree {3} rserv {4} pctim {5} adimp {6} sarva {7} side {8} ssout {9} pm {10} pt1 {11} pt2 {12} opar",
                    op_id, parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5],
                    parameters[6], parameters[7], parameters[8], parameters[9], parameters[10], parameters[11]));

            sacramentoData.Add(
                String.Format(
                    "CAPS id '{0}' uztwm {1} uztwc {2} uzfwm {3} uzfwc {4} lztwm {5} lztwc {6} lzfsm {7} lzfsc {8} lzfpm {9} lzfpc {10} uzk {11} lzsk {12} lzpk {13} caps",
                    ca_id, capacities[0], capacities[1], capacities[2], capacities[3], capacities[4], capacities[5],
                    capacities[6], capacities[7], capacities[8], capacities[9], capacities[10], capacities[11],
                    capacities[12]));

            sacramentoData.Add(String.Format("UNIH id '{0}' uh {1} dt {2} unih", uh_id,
                String.Join(" ", hydroGraphValues),
                hydrographStep));

            
            RestoreCulture();

            return sacramentoData.Count;
        }

        public int AddHbv(string id, double area, double surfaceLevel, double[] snowParameters, double[] soilParameters,
            double[] flowParameters, double[] hiniParameters, string meteoId, double areaAdjustmentFactor,
            string tempId, double x, double y)
        {
            SwitchToInvariantCulture();

            AddNodeInternal(id, NodeType.Hbv, x, y);

            var snow_id = id + "_sn";
            var soil_id = id + "_sl";
            var flow_id = id + "_fl";
            var hini_id = id + "_hini";

            sacramentoData.Add(
                String.Format(
                    "HBV id '{0}' ar {1} sl {2} snow '{3}' soil '{4}' flow '{5}' hini '{6}' ts '{7}' ms '{8}' {9} hbv",
                    id, area, surfaceLevel, snow_id, soil_id, flow_id, hini_id, tempId, meteoId,
                    GetAreaAdjustmentFactorString(areaAdjustmentFactor)));

            // TODO: add checks on arrays

            sacramentoData.Add(String.Format("SNOW id '{0}' nm '{1}' mc {2} sft {3} smt {4} tac {5} fe {6} fwf {7} snow",
                snow_id, snow_id, snowParameters[0], snowParameters[1], snowParameters[2],
                snowParameters[3], snowParameters[4], snowParameters[5]));

            sacramentoData.Add(String.Format("SOIL id '{0}' nm '{1}' be {2} fc {3} ef {4} soil", soil_id, soil_id,
                soilParameters[0], soilParameters[1], soilParameters[2]));

            sacramentoData.Add(String.Format("FLOW id '{0}' nm '{1}' kb {2} ki {3} kq {4} qt {5} mp {6} flow", flow_id, flow_id,
                flowParameters[0], flowParameters[1], flowParameters[2], flowParameters[3],
                flowParameters[4]));

            sacramentoData.Add(String.Format("HINI id '{0}' nm '{1}' ds {2} fw {3} sm {4} uz {5} lz {6} hini", hini_id, hini_id,
                hiniParameters[0], hiniParameters[1], hiniParameters[2], hiniParameters[3],
                hiniParameters[4]));

            RestoreCulture();
            
            return sacramentoData.Count;
        }

        public int AddWasteWaterTreatmentPlant(string id, double x, double y)
        {
            wwtp.Add(String.Format("WWTP id '{0}' tb 0 wwtp", id));
            return AddNodeInternal(id, NodeType.Wwtp, x, y);
        }

        public int AddBoundaryNode(string id, double initialWaterLevel, double x, double y)
        {
            //set initial water level (actual water level is passed during runtime)
            boundaries.Add(String.Format("BOUN id '{0}' bl 0 {1} is 0 boun", id, initialWaterLevel));  //-99 m AD as default (outside)
            return AddNodeInternal(id, NodeType.BoundaryOrLateral, x, y);
        }

        public void AddLink(string linkId, string from, string to)
        {
            SwitchToInvariantCulture();

            links.Add(String.Format("BRCH id '{0}' ri '-1' mt 1 '0' bt 17 ObID '3B_LINK' bn '{1}' en '{2}' brch",
                linkId, from, to));

            RestoreCulture();
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

            return String.Format("wc {0} wd {1} wh {2}", waterUsePerHourForConstant, waterUsePerDayForVariable, waterUsePercentagesString);
        }

        private int AddNodeInternal(string id, NodeType nodeType, double x, double y)
        {
            SwitchToInvariantCulture();

            string sobekNodeId = GetSobekNodeId(nodeType);
            nodes.Add(String.Format(
                "NODE id '{0}' nm '{0}' ri '-1' mt 1 {1} px {2} py {3} node", id, sobekNodeId, x, y));
            typeForNodes.Add(nodeType);

            RestoreCulture();

            return typeForNodes.Count(t => t == nodeType);
        }

        private static string GetAreaAdjustmentFactorString(double areaAdjustmentFactor)
        {
            var areaAdjustmentFactorString = Math.Abs(areaAdjustmentFactor - 1.0) < 0.000001 //if ~= 1.0
                ? ""
                : string.Format(" aaf {0} ", areaAdjustmentFactor);
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
            throw new NotImplementedException("Unknown node type: todo");
        }

        public void SetPavedVariablePumpCapacities(int iref, int[] dates, int[] times, double[] mixedCapacity, double[] dwfCapacity)
        {
            SwitchToInvariantCulture();

            var pavedId = pavedIds[iref - 1];
            var capacityTableId = pavedId + "_qc";

            pavedPumpCapacitiesTable.Add(
                String.Format("QC_T id '{0}' PDIN 1 1 '31536000' pdin\n{1}qc_t", capacityTableId, WriteTable(dates, times, mixedCapacity, dwfCapacity)));

            RestoreCulture();
        }

        private static string WriteTable(int[] dates, int[] times, double[] values, double[] values2=null)
        {
            if (dates.Length != values.Length || dates.Length != times.Length)
            {
                throw new ArgumentException(String.Format("Number of datetimes {0} and number of values {1} do not match", dates.Length, values.Length));
            }

            var tableString = new StringBuilder();
            tableString.Append("    TBLE\n");
            
            for (int i = 0; i < dates.Length; i++ )
            {
                var time = ConvertFromSobekDateTime(dates[i], times[i]);
                tableString.Append(String.Format("        '{0}/{1:00}/{2:00};{3:00}:{4:00}:{5:00}' {6}{7} <\n",
                    time.Year, time.Month,
                    time.Day, time.Hour, time.Minute, time.Second, values[i],
                    values2 != null ? "    "+values2[i] : ""));
            }

            tableString.Append("    tble\n");
            return tableString.ToString();
        }

        public void SetUnpavedConstantSeepage(int iref, double seepage)
        {
            SwitchToInvariantCulture();
            var seepageId = unpavedIds[iref - 1] + "_seepage";
            unpavedSeepage.Add(String.Format("SEEP id '{0}' nm '{0}' co {1} sp {2} ss 0 seep", seepageId, 1, seepage));
            RestoreCulture();
        }

        public void SetUnpavedVariableSeepage(int iref, SeepageComputationOption seepageComputationOption, double resistanceC, int[] h0Dates, int[] h0Times, double[] h0Table)
        {
            SwitchToInvariantCulture();
            var unpavedId = unpavedIds[iref - 1];
            var seepageId = unpavedId + "_seepage";
            var h0TableId = unpavedId + "_h0table";
            unpavedSeepage.Add(String.Format("SEEP id '{0}' nm '{0}' co {1} cv {2} h0 '{3}' ss 0 seep", seepageId, (int)seepageComputationOption, resistanceC, h0TableId));
            unpavedTable.Add(String.Format("H0_T id '{0}' PDIN 1 1 '31536000' pdin\n{1}h0_t", h0TableId, WriteTable(h0Dates, h0Times, h0Table)));
            RestoreCulture();
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

            SwitchToInvariantCulture();

            AddNodeInternal(id, NodeType.Unpaved, x, y);

            string drainage = GetDrainageId(drainageComputationOption, id);

            var seepageId = id + "_seepage";
            var storageId = id + "_storage";
            var infiltrationId = id + "_infilt";

            unpavedIds.Add(id);
            unpavedData.Add(
                String.Format(
                    "UNPV id '{0}' na 16 ar {1} lv {3} ga {2} co {4} su 0 sd '{5}' rc {6}{7} sp '{8}' ic '{9}' bt {10} ig 0 {11} mg {12} gl {14} ms '{13}' {15}is 0 unpv",
                    id, String.Join(" ", areasForKnownCropTypes.Select(a => a.ToString(CultureInfo.InvariantCulture)).ToArray()),
                    areaForGroundwaterCalculations, surfaceLevel, (int)drainageComputationOption, storageId,
                    reservoirCoefficient, drainage, seepageId, infiltrationId, soilType, initialGroundwaterLevel,
                    maximumAllowedGroundwater, meteoId, groundwaterLayerThickness, GetAreaAdjustmentFactorString(areaAdjustmentFactor)));

            unpavedStorage.Add(String.Format("STDF id '{0}' nm '{0}' ml {1} il {2} stdf", storageId, maximumLandStorage, initialLandStorage));
            
            unpavedInfiltration.Add(String.Format("INFC id '{0}' nm '{0}' ic {1} infc", infiltrationId, infiltrationCapacity));

            RestoreCulture();

            return unpavedData.Count;
        }

        private static string GetDrainageId(DrainageComputationOption drainageComputationOption, string id)
        {
            var drainage = "";
            switch (drainageComputationOption)
            {
                case DrainageComputationOption.DeZeeuwHellinga:
                    drainage = String.Format(" ad '{0}'", id + "_hellinga");
                    break;
                case DrainageComputationOption.Ernst:
                    drainage = String.Format(" ed '{0}'", id + "_ernst");
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

            SwitchToInvariantCulture();

            var ernstId = unpavedIds[iref-1] + "_ernst";

            unpavedHellingaErnst.Add(
                String.Format(
                    "ERNS id '{0}' nm '{0}' cvs {1} cvo {2} {3} {4} {5} cvi {6} lv {7} {8} {9} erns", ernstId, surfaceRunoff,
                    belowSurfaceDrainage[0], belowSurfaceDrainage[1], belowSurfaceDrainage[2], lastLayerRunoff, infiltration,
                    belowSurfaceLevels[0], belowSurfaceLevels[1], belowSurfaceLevels[2]));

            RestoreCulture();

            return 0; //?
        }

        public int SetDeZeeuwHellinga(int iref, double surfaceRunoff, double lastLayerRunoff, double infiltration, double[] belowSurfaceLevels, double[] belowSurfaceDrainage)
        {
            if (belowSurfaceDrainage.Length != 3 || belowSurfaceLevels.Length != 3)
            {
                throw new NotSupportedException("Only 3 layers is supported");
            }

            SwitchToInvariantCulture();

            var hellingaId = unpavedIds[iref-1] + "_hellinga";

            unpavedHellingaErnst.Add(
                String.Format(
                    "ALFA id '{0}' nm '{0}' af {1} {2} {3} {4} {5} {6} lv {7} {8} {9} alfa", hellingaId, surfaceRunoff,
                    belowSurfaceDrainage[0], belowSurfaceDrainage[1], belowSurfaceDrainage[2], lastLayerRunoff, infiltration,
                    belowSurfaceLevels[0], belowSurfaceLevels[1], belowSurfaceLevels[2]));

            RestoreCulture();

            return 0; //?
        }

        public void SetMeteoDataStartTimeAndInterval(int startDate, int startTime, int timeStepInSeconds)
        {
            startDateMeteoData = startDate;
            startTimeMeteoData = startTime;
            this.timeStepInSeconds = timeStepInSeconds;
        }

        public void AddPrecipitationStation(string name, double[] precipitation)
        {
            precipitationPerStation.Add(name, precipitation);
        }

        public void AddEvaporationStation(string name, double[] evaporationInMMPerDay)
        {
            evaporationPerStation.Add(name, evaporationInMMPerDay);
        }

        public void AddTemperatureStation(string name, double[] temperatures)
        {
            temperaturePerStation.Add(name, temperatures);
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
            SwitchToInvariantCulture();
            GenerateRRModelFiles();
            RestoreCulture();
        }

        public void AddIniOption(string category, string property, string value)
        {
            if (String.IsNullOrEmpty(category) || String.IsNullOrEmpty(property))
            {
                return;
            }

            if (iniOptions.Keys.Contains(category))
            {
                var val = iniOptions[category];
                var kvp = val.FirstOrDefault(v => v.First.Equals(property, StringComparison.InvariantCultureIgnoreCase));
                if (kvp != null)
                    kvp.Second = value;
                else  
                    val.Add(new DelftTools.Utils.Tuple<string, string>(property, value));
                return;
            }
            iniOptions.Add(category, new List<DelftTools.Utils.Tuple<string, string>>
            {
                new DelftTools.Utils.Tuple<string, string>(property, value)
            });
        }
    }
}