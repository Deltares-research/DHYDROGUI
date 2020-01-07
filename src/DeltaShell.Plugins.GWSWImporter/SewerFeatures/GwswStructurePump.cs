using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;

namespace DeltaShell.Plugins.ImportExport.GWSW.SewerFeatures
{
    public class GwswStructurePump : Pump
    {
        public GwswStructurePump(string name) : base(name)
        {
        }

        protected override ISewerConnection GetNewSewerConnectionWithPump()
        {
            var sewerConnection = new SewerConnection(Name);
            sewerConnection.AddStructureToBranch(this);
            return sewerConnection;
        }

        protected override void CopyPropertyValuesToExistingPump(IPump pump)
        {
            pump.Capacity = Capacity;
            pump.StartSuction = StartSuction;
            pump.StopSuction = StopSuction;
            pump.StartDelivery = StartDelivery;
            pump.StopDelivery = StopDelivery;
        }
    }
}
