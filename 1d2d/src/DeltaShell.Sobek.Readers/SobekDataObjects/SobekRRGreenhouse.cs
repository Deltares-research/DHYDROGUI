using System.Linq;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekRRGreenhouse: ISobekCatchment
    {
        public SobekRRGreenhouse()
        {
            AreaAjustmentFactor = 1.0;
        }

        public string Id 
        {
            get;
            set;
        }

        public double[] Areas
        {
            get;
            set;
        }

        public double SiloArea
        {
            get;
            set;
        }

        public double SurfaceLevel
        {
            get;
            set;
        }

        public string StorageOnRoofsId
        {
            get;
            set;
        }

        public string SiloId
        {
            get;
            set;
        }

        public string MeteoStationId
        {
            get;
            set;
        }

        /// <summary>
        /// Factor related to the meteo station data (default 1.0)
        /// </summary>
        public double AreaAjustmentFactor
        {
            get;
            set;
        }

        public double InitialSaltConcentration
        {
            get;
            set;
        }

        public double Area { get { return Areas.Sum(); } }
    }
}
