namespace DelftTools.Hydro.Structures
{
    public class GwswConnectionWeir : Weir
    {
        public GwswConnectionWeir(string name) : base(name)
        {
        }

        public string SourceCompartmentId { get; set; }

        public string TargetCompartmentId { get; set; }

        protected override ISewerConnection GetNewSewerConnectionWithWeir()
        {
            var sewerConnection = new SewerConnection(Name)
            {
                SourceCompartmentName = SourceCompartmentId,
                TargetCompartmentName = TargetCompartmentId
            };

            sewerConnection.AddStructureToBranch(this);
            return sewerConnection;
        }

        protected override void CopyPropertyValuesToExistingWeir(IWeir weir)
        {
            weir.FlowDirection = FlowDirection;
        }
    }
}
