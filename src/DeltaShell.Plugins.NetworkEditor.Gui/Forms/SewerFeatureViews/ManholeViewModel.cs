using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
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
            EscapeCommand = new RelayCommand(Escape);
            DeleteCommand = new RelayCommand(Delete);

            ShapeTypes = new ObservableCollection<ShapeType>(Enum.GetValues(typeof(ShapeType)).OfType<ShapeType>());
        }
        
        public Manhole Manhole
        {
            get { return manhole; }
            set
            {
                if (value == null)
                {
                    network = null;
                    return;
                }

                manhole = value;
                network = manhole?.Network as IHydroNetwork;
            }
        }

        public ObservableCollection<ShapeType> ShapeTypes { get; set; }

        public ICommand EscapeCommand { get; set; }

        public ICommand DeleteCommand { get; set; }

    public object SelectedItem { get; set; }

        public Action DeselectItem { get; set; }

        private void Delete(object o)
        {
            RemoveItem(null);
        }

        private void Escape(object o)
        {
            DeselectItem?.Invoke();
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
            AddNewSewerConnectionWithStructure<Orifice>();
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

            foreach (var pointFeature in connection.BranchFeatures.OfType<IPointFeature>())
            {
                pointFeature.ParentPointFeature = manhole;
            }

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

        public void AddShape(ShapeType item, int index)
        {
            switch (item)
            {
                case ShapeType.Compartment:
                    AddCompartment(null);
                    break;
                case ShapeType.Pump:
                    AddPump(null);
                    break;
                case ShapeType.Weir:
                    AddWeir(null);
                    break;
                case ShapeType.Orifice:
                    AddOrifice(null);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(item), item, null);
            }
        }
    }
}