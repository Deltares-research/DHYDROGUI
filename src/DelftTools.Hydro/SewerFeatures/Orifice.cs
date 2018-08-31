using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using log4net;

namespace DelftTools.Hydro.SewerFeatures
{
    [Entity]
    public class Orifice : Gate, IOrifice
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Orifice));

        public Orifice() : this("Orifice")
        {
            
        }

        public Orifice(string name)
        {
            Name = name;
        }

        public double BottomLevel { get; set; }
        public double ContractionCoefficent { get; set; }
        public double MaxDischarge { get; set; }

        public virtual void AddToHydroNetwork(IHydroNetwork hydroNetwork)
        {
            var sewerConnection = hydroNetwork.SewerConnections.FirstOrDefault(
                sc => sc.BranchFeatures.Count >= 2
                      && sc.BranchFeatures[1].Name == Name
                      && sc.BranchFeatures[1] is IOrifice);
            var orifice = hydroNetwork.Orifices.FirstOrDefault(o => o.Name == Name);
            if (orifice != null)
            {
                CopyPropertyValuesToExistingOrifice(orifice);
                sewerConnection?.UpdateBranchFeatureGeometries();
                return;
            }

            sewerConnection = hydroNetwork.SewerConnections.FirstOrDefault(sc => sc.Name == Name);
            if (sewerConnection == null)
            {
                AddNewOrificeSewerConnectionToNetwork(hydroNetwork);
            }
            else
            {
                AddOrificeToSewerConnection(sewerConnection);
            }
        }

        protected void AddOrificeToSewerConnection(ISewerConnection sewerConnection)
        {
            if (sewerConnection.BranchFeatures.Count > 0)
            {
                RemoveExistingBranchFeatures(sewerConnection);
            }
            sewerConnection.AddStructureToBranch(this);
        }

        private void RemoveExistingBranchFeatures(ISewerConnection sewerConnection)
        {
            var branchFeature = sewerConnection.BranchFeatures[0];
            Log.Warn($"Overwriting branchfeature with name '{branchFeature.Name}' and type '{branchFeature.GetType()}' in sewer connection '{sewerConnection.Name}' with orifice '{Name}'");
            sewerConnection.BranchFeatures.Clear();
        }

        private void AddNewOrificeSewerConnectionToNetwork(IHydroNetwork hydroNetwork)
        {
            ISewerConnection sewerConnection = new SewerConnection(Name);
            sewerConnection.AddStructureToBranch(this);
            sewerConnection.AddToHydroNetwork(hydroNetwork);
        }

        private void CopyPropertyValuesToExistingOrifice(IOrifice orifice)
        {
            orifice.BottomLevel = BottomLevel;
            orifice.ContractionCoefficent = ContractionCoefficent;
            orifice.MaxDischarge = MaxDischarge;
        }
    }
}