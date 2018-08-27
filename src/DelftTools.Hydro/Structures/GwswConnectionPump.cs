namespace DelftTools.Hydro.Structures
{
    public class GwswConnectionPump : Pump
    {
        public GwswConnectionPump(string name) : base(name)
        {
        }

        public string SourceCompartmentId { get; set; }

        public string TargetCompartmentId { get; set; }
        
        protected override ISewerConnection GetNewSewerConnectionWithPump()
        {
            var sewerConnection = new SewerConnection(Name)
            {
                SourceCompartmentName = SourceCompartmentId,
                TargetCompartmentName = TargetCompartmentId
            };
            
            sewerConnection.AddStructureToBranch(this);
            return sewerConnection;
        }

        protected override void CopyPropertyValuesToExistingPump(IPump pump)
        {
            pump.DirectionIsPositive = DirectionIsPositive;
        }
    }
}