using DelftTools.Hydro;
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

        protected override void CopyPropertyValuesToExistingPump(IPump pump)
        {
            pump.DirectionIsPositive = DirectionIsPositive;
        }

        protected override void SetSewerConnectionProperties(ISewerConnection sewerConnection, IHydroNetwork hydroNetwork, SewerImporterHelper helper)
        {
            sewerConnection.SourceCompartmentName = SourceCompartmentName;
            sewerConnection.TargetCompartmentName = TargetCompartmentName;
            sewerConnection.AddOrUpdateGeometry(hydroNetwork, helper);
            sewerConnection.Length = Length;
        }
    }
}