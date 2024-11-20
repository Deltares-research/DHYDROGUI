using System.Linq;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekRRUnpaved: ISobekCatchment
    {
        public SobekRRUnpaved()
        {
            AreaAjustmentFactor = 1.0;
        }

        public string Id
        {
            get; set;
        }

        public double[] CropAreas
        {
            get; set;
        }

        public double GroundWaterArea
        {
            get; set;
        }

        public double SurfaceLevel
        {
            get; set;
        }

        public SobekUnpavedComputationOption ComputationOption
        {
            get; set;
        }

        public double ReservoirCoefficient
        {
            get; set;
        }

        public bool ScurveUsed
        {
            get; set;
        }

        public string ScurveTableName
        {
            get; set;
        }

        public string StorageId
        {
            get; set;
        }

        /// <summary>
        /// for Hellinga de Zeeuw drainage formula only
        /// </summary>
        public string AlfaLevelId
        {
            get; set;
        }

        public string ErnstId
        {
            get; set;
        }

        public string SeepageId
        {
            get; set;
        }

        public string InfiltrationId
        {
            get; set;
        }

        /// <summary>
        /// from file BERGCOEF or BergCoef.Cap. Indices >100 are from Bergcoef.Cap.
        /// </summary>
        public int SoilType
        {
            get; set;
        }

        public double InitialGroundwaterLevelConstant
        {
            get; set;
        }

        public string InitialGroundwaterLevelTableId
        {
            get; set;
        }

        /// <summary>
        ///in m NAP
        /// </summary>
        public double MaximumGroundwaterLevel
        {
            get; set;
        }

        /// <summary>
        ///in meters (for salt computations) 
        /// </summary>
        public double InitialDepthGroundwaterLayer
        {
            get; set;
        }

        public string MeteoStationId
        {
            get; set;
        }

        /// <summary>
        /// Factor related to the meteo station data (default 1.0)
        /// </summary>
        public double AreaAjustmentFactor
        {
            get;
            set;
        }

        /// <summary>
        /// (mg/l) Default 100 mg/l
        /// </summary>
        public double InitialSaltConcentration
        {
            get; set;
        }

        public bool InitialGroundwaterLevelFromBoundary { get; set; }
        
        public double Area { get { return CropAreas.Sum(); } }
    }

    public enum SobekUnpavedComputationOption
    {
        HellingaDeZeeuw = 1,
        KrayenhoffVanDeLeur = 2,
        Ernst = 3
    }
}
