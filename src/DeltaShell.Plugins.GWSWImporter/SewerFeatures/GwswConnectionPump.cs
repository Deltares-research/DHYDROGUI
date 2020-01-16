using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;

namespace DeltaShell.Plugins.ImportExport.GWSW.SewerFeatures
{
    public class GwswConnectionPump : Pump
    {
        public GwswConnectionPump(string name) : base(name)
        {
        }

        public string SourceCompartmentName { get; set; }

        public string TargetCompartmentName { get; set; }

        protected override ISewerConnection GetNewSewerConnectionWithPump(IHydroNetwork hydroNetwork)
        {
            var sewerConnection = new SewerConnection(Name);
            SetSewerConnectionProperties(sewerConnection);
            var composite = sewerConnection.AddStructureToBranch(this);
            composite.Name = HydroNetworkHelper.GetUniqueFeatureName(hydroNetwork, composite);
            return sewerConnection;
        }

        protected override void CopyPropertyValuesToExistingPump(IPump pump)
        {
            pump.DirectionIsPositive = DirectionIsPositive;
        }

        protected override void SetSewerConnectionProperties(ISewerConnection sewerConnection)
        {
            sewerConnection.Length = Length;
            sewerConnection.SourceCompartmentName = SourceCompartmentName;
            sewerConnection.TargetCompartmentName = TargetCompartmentName;
        }
    }
}