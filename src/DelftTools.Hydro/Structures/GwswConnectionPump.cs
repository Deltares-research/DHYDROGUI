using System.Linq;

namespace DelftTools.Hydro.Structures
{
    public class GwswConnectionPump : Pump
    {
        public GwswConnectionPump(string name) : base(name)
        {
        }

        public string SourceCompartmentId { get; set; }

        public string TargetCompartmentId { get; set; }

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
            var sewerConnection = new SewerConnection(Name)
            {
                SourceCompartmentName = SourceCompartmentId,
                TargetCompartmentName = TargetCompartmentId
            };
            
            sewerConnection.BranchFeatures.Add(this);
            return sewerConnection;
        }

        private void CopyPropertyValuesToExistingPump(IPump pump)
        {
            pump.DirectionIsPositive = DirectionIsPositive;
        }
    }
}