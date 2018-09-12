using System.Linq;
using log4net;

namespace DelftTools.Hydro.SewerFeatures
{
    public class GwswConnectionOrifice : Orifice
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GwswConnectionOrifice));
        public GwswConnectionOrifice(string name) : base(name)
        {
        }

        public string SourceCompartmentName { get; set; }
        public string TargetCompartmentName { get; set; }
        public double LevelSource { get; set; }
        public double LevelTarget { get; set; }
        public SewerConnectionWaterType WaterType { get; set; }

        public override void AddToHydroNetwork(IHydroNetwork hydroNetwork)
        {
            var sewerConnection = hydroNetwork.SewerConnections.FirstOrDefault(
                sc => sc.BranchFeatures.Count >= 2
                      && sc.BranchFeatures[1].Name == Name
                      && sc.BranchFeatures[1] is IOrifice);
            if (sewerConnection != null)
            {
                if (sewerConnection.SourceCompartment == null || sewerConnection.TargetCompartment == null)
                {
                    ConnectToCompartments(sewerConnection, hydroNetwork);
                }
                CopyGwswConnectionOrificeSewerConnectionPropertyValuesToExistingSewerConnection(sewerConnection);
                sewerConnection.AddToHydroNetwork(hydroNetwork);
                sewerConnection.UpdateBranchFeatureGeometries();
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

        private void ConnectToCompartments(ISewerConnection sewerConnection, IHydroNetwork hydroNetwork)
        {
            hydroNetwork.Branches.Remove(sewerConnection);
            SetSourceAndTargetCompartmentNames(sewerConnection);
        }

        private void SetSourceAndTargetCompartmentNames(ISewerConnection sewerConnection)
        {
            sewerConnection.SourceCompartmentName = SourceCompartmentName;
            sewerConnection.TargetCompartmentName = TargetCompartmentName;
        }

        private void AddNewOrificeSewerConnectionToNetwork(IHydroNetwork hydroNetwork)
        {
            var sewerConnection = GetNewSewerConnectionWithOrifice();
            sewerConnection.AddToHydroNetwork(hydroNetwork);
        }
        
        private SewerConnection GetNewSewerConnectionWithOrifice()
        {
            var sewerConnection = new SewerConnection(Name)
            {
                LevelSource = LevelSource,
                LevelTarget = LevelTarget,
                Length = Length,
                WaterType = WaterType,
                SourceCompartmentName = SourceCompartmentName,
                TargetCompartmentName = TargetCompartmentName
            };
            sewerConnection.AddStructureToBranch(this);

            return sewerConnection;
        }

        private void CopyGwswConnectionOrificeSewerConnectionPropertyValuesToExistingSewerConnection(ISewerConnection sewerConnection)
        {
            sewerConnection.LevelSource = LevelSource;
            sewerConnection.LevelTarget = LevelTarget;
            sewerConnection.Length = Length;
            sewerConnection.WaterType = WaterType;
        }
    }
}
