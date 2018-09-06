using DelftTools.Hydro.Structures;

namespace DelftTools.Hydro.SewerFeatures
{
    public class GwswConnectionPump : Pump
    {
        public GwswConnectionPump(string name) : base(name)
        {
        }

        public string SourceCompartmentName { get; set; }

        public string TargetCompartmentName { get; set; }
        
        protected override ISewerConnection GetNewSewerConnectionWithPump()
        {
            var sewerConnection = new SewerConnection(Name);
            SetSewerConnectionProperties(sewerConnection);
            sewerConnection.AddStructureToBranch(this);

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