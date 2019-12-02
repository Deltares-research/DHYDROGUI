using DelftTools.Hydro.Structures;

namespace DelftTools.Hydro.SewerFeatures
{
    public class GwswStructurePump : Pump
    {
        public GwswStructurePump(string name) : base(name)
        {
        }

        protected override ISewerConnection GetNewSewerConnectionWithPump(IHydroNetwork hydroNetwork)
        {
            var sewerConnection = new SewerConnection(Name);
            sewerConnection.AddToHydroNetwork(hydroNetwork);
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
