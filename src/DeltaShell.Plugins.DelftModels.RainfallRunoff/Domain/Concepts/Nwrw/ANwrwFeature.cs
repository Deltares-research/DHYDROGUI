using DelftTools.Utils.Aop;
using DHYDRO.Common.Logging;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    [Entity]
    public abstract class ANwrwFeature: INwrwFeature
    {
        protected readonly ILogHandler logHandler;

        /// <summary>
        /// Using constructor injection to retrieve log handler in all derived classes
        /// </summary>
        /// <param name="logHandler">Logger object to log (gwsw import) messages to</param>
        protected ANwrwFeature(ILogHandler logHandler)
        {
            this.logHandler = logHandler;
        }
        public string Name { get; set; } // AFV_IDE // UNI_IDE (2x) // (Default : "DefinitionName")
        public IGeometry Geometry { get; set; }
        public abstract void AddNwrwCatchmentModelDataToModel(RainfallRunoffModel rrModel, NwrwImporterHelper helper);
        public virtual void InitializeNwrwCatchmentModelData(NwrwData nwrwData)
        {
            //nothing to initialize
        }

    }
}