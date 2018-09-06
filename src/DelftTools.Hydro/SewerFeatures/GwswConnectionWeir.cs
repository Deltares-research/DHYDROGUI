using DelftTools.Hydro.Structures;

namespace DelftTools.Hydro.SewerFeatures
{
    public class GwswConnectionWeir : Weir
    {
        public GwswConnectionWeir(string name) : base(name)
        {
        }

        public string SourceCompartmentName { get; set; }

        public string TargetCompartmentName { get; set; }

        protected override ISewerConnection GetNewSewerConnectionWithWeir()
        {
            var sewerConnection = new SewerConnection(Name);
            SetSewerConnectionProperties(sewerConnection);
            sewerConnection.AddStructureToBranch(this);

            return sewerConnection;
        }

        protected override void CopyPropertyValuesToExistingWeir(IWeir weir)
        {
            weir.FlowDirection = FlowDirection;
        }

        protected override void SetSewerConnectionProperties(ISewerConnection sewerConnection)
        {
            sewerConnection.Length = Length;
            sewerConnection.SourceCompartmentName = SourceCompartmentName;
            sewerConnection.TargetCompartmentName = TargetCompartmentName;
        }
    }
}
