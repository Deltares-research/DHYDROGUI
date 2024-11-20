using System;
using DelftTools.Utils.Data;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public class DryWeatherFlow : Unique<long>
    {
        private string dryWeatherFlowId;

        // NHibernate
        public DryWeatherFlow(){}

        public DryWeatherFlow(string id)
        { 
            DryWeatherFlowId = id;
        }

        public virtual string DryWeatherFlowId
        {
            get { return dryWeatherFlowId; }
            set
            {
                ValidateId(value, "The dry weather flow id cannot be empty.");
                dryWeatherFlowId = value;
            }
        } // VER_IDE (debiet.csv)

        private static void ValidateId(string id, string message)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(message);
            }
        }

        public virtual double NumberOfUnits { get; set; } // AVV_ENH (debiet.csv)

        
    }

}