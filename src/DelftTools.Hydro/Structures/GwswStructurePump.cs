using System.Linq;

namespace DelftTools.Hydro.Structures
{
    public class GwswStructurePump : Pump
    {
        public GwswStructurePump(string name) : base(name)
        {
        }
        
        public override void AddToHydroNetwork(IHydroNetwork hydroNetwork)
        {
            base.AddToHydroNetwork(hydroNetwork);
            var pump = hydroNetwork.SewerConnections.FirstOrDefault(
                sc => sc.BranchFeatures.Count == 1
                      && sc.BranchFeatures[0].Name == Name
                      && sc.BranchFeatures[0] is IPump)?.BranchFeatures.FirstOrDefault() as IPump;

            if (pump != null)
            {
                CopyPropertyValuesToExistingPump(pump);
                return;
            }

            var sewerConnection = GetNewSewerConnectionWithPump();
            sewerConnection.AddToHydroNetwork(hydroNetwork);
        }

        private ISewerConnection GetNewSewerConnectionWithPump()
        {
            var sewerConnection = new SewerConnection(Name);
            sewerConnection.BranchFeatures.Add(this);
            return sewerConnection;
        }

        private void CopyPropertyValuesToExistingPump(IPump pump)
        {
            pump.Capacity = Capacity;
            pump.StartSuction = StartSuction;
            pump.StopSuction = StopSuction;
            pump.StartDelivery = StartDelivery;
            pump.StopDelivery = StopDelivery;
        }
    }
}
