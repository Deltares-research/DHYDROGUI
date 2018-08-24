using System.Linq;
using log4net;

namespace DelftTools.Hydro.Structures
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
                sc => sc.BranchFeatures.Count == 1
                      && sc.BranchFeatures[0].Name == Name
                      && sc.BranchFeatures[0] is IOrifice);
            if (sewerConnection != null)
            {
                CopyGwswConnectionOrificeSewerConnectionPropertyValuesToExistingSewerConnection(sewerConnection);
                if (sewerConnection.SourceCompartment == null || sewerConnection.TargetCompartment == null)
                {
                    ConnectToCompartments(sewerConnection, hydroNetwork);
                }
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
            sewerConnection.AddToHydroNetwork(hydroNetwork);
        }

        private void SetSourceAndTargetCompartmentNames(ISewerConnection sewerConnection)
        {
            sewerConnection.SourceCompartmentName = SourceCompartmentName;
            sewerConnection.TargetCompartmentName = TargetCompartmentName;
        }

        private void AddNewOrificeSewerConnectionToNetwork(IHydroNetwork hydroNetwork)
        {
            var sewerConnection = GetNewSewerConnectionWithOrifice();
            sewerConnection.BranchFeatures.Add(this);
            sewerConnection.AddToHydroNetwork(hydroNetwork);
        }

        private void AddOrificeToSewerConnection(ISewerConnection sewerConnection)
        {
            if (sewerConnection.BranchFeatures.Count > 0)
            {
                RemoveExistingBranchFeatures(sewerConnection);
            }
            sewerConnection.BranchFeatures.Add(this);
        }

        private void RemoveExistingBranchFeatures(ISewerConnection sewerConnection)
        {
            var branchFeature = sewerConnection.BranchFeatures[0];
            Log.Warn($"Overwriting branchfeature with name '{branchFeature.Name}' and type '{branchFeature.GetType()}' in sewer connection '{sewerConnection.Name}' with orifice '{Name}'");
            sewerConnection.BranchFeatures.Clear();
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
