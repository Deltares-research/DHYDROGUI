using System.Linq;
using DelftTools.Hydro.Structures.WeirFormula;

namespace DelftTools.Hydro.Structures
{
    public class GwswStructureWeir : Weir
    {
        public GwswStructureWeir(string name) : base(name)
        {
        }

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
            var sewerConnection = new SewerConnection(Name);
            sewerConnection.BranchFeatures.Add(this);
            return sewerConnection;
        }

        private void CopyPropertyValuesToExistingWeir(IWeir weir)
        {
            weir.CrestWidth = CrestWidth;
            weir.CrestLevel = CrestLevel;
            weir.WeirFormula = new SimpleWeirFormula
            {
                DischargeCoefficient = ((SimpleWeirFormula)WeirFormula).DischargeCoefficient
            };
        }
    }
}
