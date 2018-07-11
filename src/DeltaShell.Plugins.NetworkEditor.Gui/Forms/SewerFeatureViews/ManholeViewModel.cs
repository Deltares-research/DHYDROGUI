using System.Linq;
using System.Windows.Input;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.NetworkEditor.Gui.Commands;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    [Entity]
    public class ManholeViewModel
    {
        private IHydroNetwork network;
        private Manhole manhole;

        public ManholeViewModel()
        {
            AddCompartmentCommand = new RelayCommand(AddCompartment, CanAddCompartment);
            AddPumpCommand = new RelayCommand(AddPump, CanAddPump);
            AddOrificeCommand = new RelayCommand(AddOrifice, CanAddOrifice);
            AddWeirCommand = new RelayCommand(AddWeir, CanAddWeir);
            RemoveCompartmentCommand = new RelayCommand(RemoveItem, CanRemoveItem);
        }

        public Manhole Manhole
        {
            get { return manhole; }
            set
            {
                if (value == null)
                {
                    network = null;
                    UnsubscribeEvents();
                    return;
                }

                if (manhole != null)
                {
                    if (manhole.Compartments != null)
                    {
                        UnsubscribeEvents();
                    }
                }

                manhole = value;

                if (manhole.Compartments != null)
                {
                    DetermineSurfaceAndBottomLevels();
                    SubscribeEvents();
                }

                network = manhole.Network as IHydroNetwork;
            }
        }

        public ICommand AddCompartmentCommand { get; set; }

        public ICommand AddOrificeCommand { get; set; }

        public ICommand AddPumpCommand { get; set; }

        public ICommand AddWeirCommand { get; set; }

        public ICommand RemoveCompartmentCommand { get; set; }

        public double SurfaceLevel { get; set; }

        public double BottomLevel { set; get; }

        public object SelectedItem { get; set; }

        private void DetermineSurfaceAndBottomLevels()
        {
            SurfaceLevel = CalculateSurfaceLevel();
            BottomLevel = CalculateBottomLevel();
        }

        private double CalculateSurfaceLevel()
        {
            var compartments = manhole?.Compartments;
            if (compartments != null && compartments.Any())
            {
                return compartments.Max(c => c.SurfaceLevel);
            }

            return 0;
        }

        private double CalculateBottomLevel()
        {
            var compartments = manhole?.Compartments;
            if (compartments != null && compartments.Any())
            {
                return compartments.Min(c => c.BottomLevel);
            }

            return 0;
        }

        private void EventedListCollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            DetermineSurfaceAndBottomLevels();
        }

        private void SubscribeEvents()
        {
            manhole.Compartments.CollectionChanged += EventedListCollectionChanged;
        }

        private void UnsubscribeEvents()
        {
            manhole.Compartments.CollectionChanged -= EventedListCollectionChanged;
        }

        #region Add/remove methods

        private void RemoveItem(object obj)
        {
            var compartmentToRemove = SelectedItem as Compartment;
            if (compartmentToRemove != null)
            {
                // Remove compartment
                var sewerConnections = manhole.InternalConnections();
                foreach (var sewerConnection in sewerConnections)
                {
                    if (sewerConnection.SourceCompartment == compartmentToRemove)
                    {
                        sewerConnection.SourceCompartment = null;
                    }

                    if (sewerConnection.TargetCompartment == compartmentToRemove)
                    {
                        sewerConnection.TargetCompartment = null;
                    }
                }

                manhole.Compartments.Remove(compartmentToRemove);
            }

            var structure1D = SelectedItem as IStructure1D;
            if (structure1D != null)
            {
                var connectionsToRemove = manhole.InternalConnections().Where(connection => connection.BranchFeatures.Contains(structure1D)).ToList();
                foreach (var connection in connectionsToRemove)
                {
                    manhole.Network.Branches.Remove(connection);
                }
            }

            var orifice = SelectedItem as SewerConnectionOrifice;
            if (orifice != null)
            {
                manhole.Network.Branches.Remove(orifice);
            }
        }

        private bool CanRemoveItem(object obj)
        {
            return SelectedItem != null;
        }
        
        private void AddCompartment(object obj)
        {
            var newCompartment = SewerFactory.CreateNewCompartmentAndAddToManhole(network, manhole);
            TryConnectCompartmentToConnection(manhole, newCompartment);
        }

        private bool CanAddCompartment(object obj)
        {
            return Manhole?.Compartments != null;
        }

        private void AddPump(object obj)
        {
            AddNewSewerConnectionWithStructure<Pump>();
        }

        private bool CanAddPump(object obj)
        {
            return true;
        }

        private void AddOrifice(object obj)
        {
            AddNewSewerConnectionWithStructure<SewerConnectionOrifice>();
        }

        private bool CanAddOrifice(object obj)
        {
            return true;
        }

        private void AddWeir(object obj)
        {
            AddNewSewerConnectionWithStructure<Weir>();
        }

        private bool CanAddWeir(object obj)
        {
            return true;
        }

        private void AddNewSewerConnectionWithStructure<T>()
        {
            var connection = SewerFactory.CreateConnectionWithStructure<T>(manhole);
            if (connection == null) return;

            TryConnecTSewerConnectionToCompartments(connection, manhole);

            network.Branches.Add(connection);
        }

        private static void TryConnecTSewerConnectionToCompartments(ISewerConnection sewerConnection, Manhole manhole)
        {
            var currentConnections = manhole.InternalConnections().ToList();
 
            for (var i = 1; i < manhole.Compartments.Count; i++)
            {
                var sourceCompartment = manhole.Compartments[i - 1];
                var sourceAvailable = currentConnections.All(cc => cc.SourceCompartment != sourceCompartment);
                var targetCompartment = manhole.Compartments[i];
                var targetAvailable = currentConnections.All(cc => cc.SourceCompartment != targetCompartment);

                if (sourceAvailable && targetAvailable)
                {
                    sewerConnection.SourceCompartment = sourceCompartment;
                    sewerConnection.TargetCompartment = targetCompartment;
                    break;
                }
            }
        }

        private static void TryConnectCompartmentToConnection(Manhole parentManhole, Compartment newCompartment)
        {
            // Find sewer connection where this compartment can be a target
            var sewerConnections = parentManhole.InternalConnections().ToList();

            var sewerConnectionsWithoutTargetCompartments = sewerConnections.FirstOrDefault(sc => sc.TargetCompartment == null);
            var sewerConnectionWithoutSourceCompartment = sewerConnections.FirstOrDefault(sc => sc.SourceCompartment == null);
            if (sewerConnectionsWithoutTargetCompartments != null)
            {
                sewerConnectionsWithoutTargetCompartments.TargetCompartment = newCompartment;
            }
            if (sewerConnectionWithoutSourceCompartment == null) return;

            sewerConnectionWithoutSourceCompartment.SourceCompartment = newCompartment;
        }

        #endregion
    }
}