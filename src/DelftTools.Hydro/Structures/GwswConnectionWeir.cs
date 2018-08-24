using System.Linq;

namespace DelftTools.Hydro.Structures
{
    public class GwswConnectionWeir : Weir
    {
        public GwswConnectionWeir(string name) : base(name)
        {
        }

        public string SourceCompartmentId { get; set; }

        public string TargetCompartmentId { get; set; }

        public override void AddToHydroNetwork(IHydroNetwork hydroNetwork)
        {
            base.AddToHydroNetwork(hydroNetwork);
            var weir = hydroNetwork.SewerConnections.FirstOrDefault(
                sc => sc.BranchFeatures.Count == 1
                      && sc.BranchFeatures[0].Name == Name
                      && sc.BranchFeatures[0] is IWeir)?.BranchFeatures.FirstOrDefault() as IWeir;

            if (weir != null)
            {
                CopyPropertyValuesToExistingWeir(weir);
                return;
            }

            var sewerConnection = GetNewSewerConnectionWithWeir();
            sewerConnection.AddToHydroNetwork(hydroNetwork);
        }

        private ISewerConnection GetNewSewerConnectionWithWeir()
        {
            var sewerConnection = new SewerConnection(Name)
            {
                SourceCompartmentName = SourceCompartmentId,
                TargetCompartmentName = TargetCompartmentId
            };
            
            sewerConnection.BranchFeatures.Add(this);
            return sewerConnection;
        }

        private void CopyPropertyValuesToExistingWeir(IWeir weir)
        {
            weir.FlowDirection = FlowDirection;
        }
    }
}
