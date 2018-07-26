using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.NetworkEditor.Gui.Commands;
using GeoAPI.Extensions.Feature;

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
            RemoveItem();
        }

        private void Escape(object o)
        {
            DeselectItem?.Invoke();
        }
        
        #region Add/remove methods

        private void RemoveItem()
        {
            var compartmentToRemove = SelectedItem as Compartment;
            if (compartmentToRemove != null)
            {
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

        private IFeature AddCompartment()
        {
            var newCompartment = SewerFactory.CreateNewCompartmentAndAddToManhole(network, manhole);
            return newCompartment;
        }

        private IFeature AddPump()
        {
            return AddNewSewerConnectionWithStructure<Pump>();
        }

        private IFeature AddOrifice()
        {
            return AddNewSewerConnectionWithStructure<Orifice>();
        }

        private IFeature AddWeir()
        {
            return AddNewSewerConnectionWithStructure<Weir>();
        }

        private IFeature AddNewSewerConnectionWithStructure<T>()
        {
            var connection = SewerFactory.CreateConnectionWithStructure<T>(manhole);
            if (connection == null) return null;

            foreach (var pointFeature in connection.BranchFeatures.OfType<IPointFeature>())
            {
                pointFeature.ParentPointFeature = manhole;
            }

            network.Branches.Add(connection);
            return connection.GetStructuresFromBranchFeatures().FirstOrDefault();
        }

        #endregion

        public IFeature AddShape(ShapeType item)
        {
            switch (item)
            {
                case ShapeType.Compartment:
                    return AddCompartment();
                case ShapeType.Pump:
                    return AddPump();                    
                case ShapeType.Weir:
                    return AddWeir();
                case ShapeType.Orifice:
                    return AddOrifice();
                default:
                    throw new ArgumentOutOfRangeException(nameof(item), item, null);
            }
        }
    }
}