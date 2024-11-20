using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.DataAccessObjects;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter
{
    public interface IRRModelHybridFileWriter
    {
        #region Model

        void WriteFiles();
        void AddIniOption(string section, string property, string value);

        bool SetSimulationTimesAndGenerateIniFile(int startDate, int startTime, int endDate, int endTime, int timeStep, int outputTimeStep);
        #endregion

        #region Paved

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="area">m2</param>
        /// <param name="streetLevel">m AD</param>
        /// <param name="initialStreetStorage">mm</param>
        /// <param name="maximumStreetStorage">mm</param>
        /// <param name="initialRainfallSewerStorage">mm</param>
        /// <param name="maximumRainfallSewerStorage">mm</param>
        /// <param name="initialDwfSewerStorage">mm</param>
        /// <param name="maximumDwfSewerStorage">mm</param>/// 
        /// <param name="sewerType">0=mixed, 1=separated,  2=improved separated</param>
        /// <param name="sewerCapacityIsFixed"></param>
        /// <param name="rainfallSewerCapacity">m3/s</param>
        /// <param name="dwfSewerCapacity">m3/s</param>
        /// <param name="dwfSewerLink"></param>
        /// <param name="numberOfPeople"></param>
        /// <param name="dwfComputationOption">dwf/dwa</param>
        /// <param name="waterUsePerCapitaPerHourInDay">array of 24 hours, avg #liter per hour per capita for a day</param>
        /// <param name="rainfallSewerLink"></param>
        /// <param name="runoffCoefficient"> </param>
        /// <param name="meteoId">meteo station id</param>
        /// <param name="areaAdjustmentFactor">calculation area adjustment (scaling) factor when calculating 
        /// effective precipitation / evaporation, 1.0 means no adjustment</param>
        /// <returns>paved internal id</returns>
        int AddPaved(string id, double area, double streetLevel,
            double initialStreetStorage,
            double maximumStreetStorage,
            double initialRainfallSewerStorage,
            double maximumRainfallSewerStorage,
            double initialDwfSewerStorage,
            double maximumDwfSewerStorage,
            SewerType sewerType,
            bool sewerCapacityIsFixed, double rainfallSewerCapacity, double dwfSewerCapacity,
            LinkType rainfallSewerLink, LinkType dwfSewerLink,
            int numberOfPeople, DwfComputationOption dwfComputationOption,
            double[] waterUsePerCapitaPerHourInDay,
            double runoffCoefficient,
            string meteoId,
            double areaAdjustmentFactor,
            double x, double y);

        void SetPavedVariablePumpCapacities(int iref, int[] dates, int[] times, double[] mixedCapacity, double[] dwfCapacity);

        #endregion

        #region Unpaved

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unpavedId"></param>
        /// <param name="seepage">mm/day</param>
        void SetUnpavedConstantSeepage(int iref, double seepage);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unpavedId"></param>
        /// <param name="seepageComputationOption">..</param>
        /// <param name="resistanceC"></param>
        /// <param name="h0Dates"></param>
        /// <param name="h0Times"></param>
        /// <param name="h0Table"></param>
        void SetUnpavedVariableSeepage(int iref, SeepageComputationOption seepageComputationOption, double resistanceC,
            int[] h0Dates, int[] h0Times, double[] h0Table);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="areasForKnownCropTypes">16 areas (cropfrac), m2</param>
        /// <param name="areaForGroundwaterCalculations">m2</param>
        /// <param name="surfaceLevel">m AD</param>
        /// <param name="drainageComputationOption">1=Hellinga de Zeeuw (default), 2=Krayenhoff van de Leur, 3=Ernst, details set through separate calls</param>
        /// <param name="reservoirCoefficient">day</param>
        /// <param name="initialLandStorage">mm</param>
        /// <param name="maximumLandStorage">mm</param>
        /// <param name="infiltrationCapacity">mm/hour (?)</param>
        /// <param name="soilType">const list BERGCOEF / BERGCOEF.CAP</param>
        /// <param name="initialGroundwaterLevel">m below surface</param>
        /// <param name="maximumAllowedGroundwater">m AD</param>
        /// <param name="groundwaterLayerThickness"></param>
        /// <param name="meteoId">meteo station id</param>
        /// <param name="areaAdjustmentFactor">calculation area adjustment (scaling) factor when calculating 
        /// effective precipitation / evaporation, 1.0 means no adjustment</param>
        /// <returns></returns>
        int AddUnpaved(string id, double[] areasForKnownCropTypes, double areaForGroundwaterCalculations,
            double surfaceLevel, DrainageComputationOption drainageComputationOption,
            double reservoirCoefficient,
            double initialLandStorage, double maximumLandStorage,
            double infiltrationCapacity, int soilType, double initialGroundwaterLevel,
            double maximumAllowedGroundwater, double groundwaterLayerThickness,
            string meteoId, double areaAdjustmentFactor,
            double x, double y);

        /// <summary>
        /// Sets Ernst data for unpaved node
        /// </summary>
        /// <param name="iref">internal id unpaved node</param>
        /// <param name="surfaceRunoff">day</param>
        /// <param name="lastLayerRunoff">day</param>
        /// <param name="infiltration">day</param>
        /// <param name="numLayers">number of layers</param>
        /// <param name="belowSurfaceLevels">m below surface for up to 3 layers</param>
        /// <param name="belowSurfaceDrainage">'day' for up to 3 layers</param>
        /// <returns></returns>
        int SetErnst(int iref, double surfaceRunoff, double lastLayerRunoff, double infiltration,
            double[] belowSurfaceLevels, double[] belowSurfaceDrainage);

        /// <summary>
        /// Sets Hellinga vd Leur data for unpaved node
        /// </summary>
        /// <param name="iref">internal id unpaved node</param>
        /// <param name="surfaceRunoff">1/day</param>
        /// <param name="lastLayerRunoff">1/day</param>
        /// <param name="infiltration">1/day</param>
        /// <param name="numLayers">number of layers</param>
        /// <param name="belowSurfaceLevels">m below surface for up to 3 layers</param>
        /// <param name="belowSurfaceDrainage">'1/day' for up to 3 layers</param>
        /// <returns></returns>
        int SetDeZeeuwHellinga(int iref, double surfaceRunoff, double lastLayerRunoff, double infiltration,
            double[] belowSurfaceLevels, double[] belowSurfaceDrainage);
        #endregion

        #region Greenhouse

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="areasPerGreenhouseClass">areas per greenhouse type</param>
        /// <param name="surfaceLevel">m AD</param>
        /// <param name="initialRoofStorage">mm</param>
        /// <param name="maximumRoofStorage">mm</param>
        /// <param name="siloCapacity">m3/ha</param>
        /// <param name="siloPumpCapacity">m3/s</param>
        /// <param name="greenhouseUseSiloArea">bool</param>
        /// <param name="greenhouseSiloArea">m2</param>
        /// <param name="meteoId">meteo station id</param>
        /// <param name="areaAdjustmentFactor">calculation area adjustment (scaling) factor when calculating 
        /// effective precipitation / evaporation, 1.0 means no adjustment</param>
        /// <returns></returns>
        int AddGreenhouse(string id, double[] areasPerGreenhouseClass, double surfaceLevel,
            double initialRoofStorage, double maximumRoofStorage,
            double siloCapacity, double siloPumpCapacity,
            bool greenhouseUseSiloArea, double greenhouseSiloArea, string meteoId, double areaAdjustmentFactor,
            double x, double y);
        #endregion

        #region Open water

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="area">m2</param>
        /// <param name="meteoId">meteo station id</param>
        /// <param name="areaAdjustmentFactor">calculation area adjustment (scaling) factor when calculating 
        /// effective precipitation / evaporation, 1.0 means no adjustment</param>
        /// <returns></returns>
        int AddOpenWater(string id, double area, string meteoId, double areaAdjustmentFactor,
            double x, double y);

        #endregion

        #region Sacramento

        int AddSacramento(string id, double area, double[] parameters, double[] capacities, double hydrographStep,
            double[] hydroGraphValues, string meteoId, double x, double y);

        #endregion

        #region HBV

        int AddHbv(string id, double area, double surfaceLevel, double[] snowParameters, double[] soilParameters,
            double[] flowParameters, double[] hiniParameters, string meteoId, double areaAdjustmentFactor,
            string tempId, double x, double y);

        #endregion

        #region RR Init
        int AddWasteWaterTreatmentPlant(string id, double x, double y);
        int AddBoundaryNode(string id, double initialWaterLevel, double x, double y); //lateral or boundary
        void AddLink(string linkId, string from, string to);
        bool GenerateRRModelFiles();
        #endregion

        #region Meteo

        /// <summary>
        /// The evaporation meteo data source.
        /// </summary>
        IOEvaporationMeteoDataSource EvaporationMeteoDataSource { get; set; }

        #endregion
    }
}