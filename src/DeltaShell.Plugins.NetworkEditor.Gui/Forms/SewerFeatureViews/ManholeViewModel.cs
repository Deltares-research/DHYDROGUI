using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
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
                SelectedItem = null;
            }
        }

        public ObservableCollection<ShapeType> ShapeTypes { get; set; }

        public ICommand EscapeCommand { get; set; }

        public ICommand DeleteCommand { get; set; }

        public object SelectedItem { get; set; }

        public Action DeselectItem { get; set; }

        private void Delete(object o)
        {
            RemoveSelectedItem();
        }

        private void Escape(object o)
        {
            DeselectItem?.Invoke();
        }
        
        #region Add/remove methods

        private void RemoveSelectedItem()
        {
            var compartmentToRemove = SelectedItem as Compartment;
            if (compartmentToRemove != null)
            {
                if (!CanRemoveCompartment(compartmentToRemove)) return;

                // incoming pipes => replace target compartment
                foreach (var pipe in manhole.IncomingPipes().Where(p => p.TargetCompartment == compartmentToRemove))
                {
                    pipe.TargetCompartment = manhole.Compartments.FirstOrDefault(c => c != compartmentToRemove);
                }

                // outgoing pipes => replace source compartment
                foreach (var pipe in manhole.OutgoingPipes().Where(p => p.SourceCompartment == compartmentToRemove))
                {
                    pipe.SourceCompartment = manhole.Compartments.FirstOrDefault(c => c != compartmentToRemove);
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

        private bool CanRemoveCompartment(Compartment compartment)
        {
            var containsCompartment = manhole.Compartments.Contains(compartment);
            var hasMoreThanOneCompartment = manhole.Compartments.Count > 1;
            return containsCompartment && hasMoreThanOneCompartment;
        }

        private IFeature AddNewSewerConnectionWithStructure(ISewerConnection connection)
        {
            network.Branches.Add(connection);
            NamingHelper.MakeNamesUnique(network.Structures);
            return connection.GetStructuresFromBranchFeatures().FirstOrDefault();
        }

        #endregion

        public IFeature AddShape(ShapeType item)
        {
            switch (item)
            {
                case ShapeType.Compartment:
                    return SewerFactory.CreateNewCompartmentAndAddToManhole(network, manhole);
                case ShapeType.Pump:
                    ISewerConnection pumpConnection = SewerFactory.CreatePumpConnection(manhole);
                    return AddNewSewerConnectionWithStructure(pumpConnection);
                case ShapeType.Weir:
                    ISewerConnection weirConnection = SewerFactory.CreateWeirConnection(manhole);
                    return AddNewSewerConnectionWithStructure(weirConnection);
                case ShapeType.Orifice:
                    ISewerConnection orificeConnection = SewerFactory.CreateOrificeConnection(manhole);
                    return AddNewSewerConnectionWithStructure(orificeConnection);
                default:
                    throw new ArgumentOutOfRangeException(nameof(item), item, null);
            }
        }
    }
}