using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms;
using SharpMap.Data.Providers;
using IEditableObject = DelftTools.Utils.Editing.IEditableObject;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui
{
    public class ControlGroupFeatureCollection : FeatureCollection
    {
        private readonly IEventedList<ControlGroup> controlGroups;
        private IEditableObject editableObjectRefresh;
        private IEventedList<Connection> connectionList;
        private IEventedList<ConnectionPoint> connectionPointList;
        private bool useConnections;

        public ControlGroupFeatureCollection(IEventedList<ControlGroup> controlGroups)
        {
            this.controlGroups = controlGroups;
            controlGroups.CollectionChanged += ControlGroupsCollectionChanged;

            connectionList = new EventedList<Connection>();
            connectionPointList = new EventedList<ConnectionPoint>();

            UseConnections = false;
        }

        public override IList Features
        {
            get
            {
                return useConnections ? (IList) connectionList : (IList) connectionPointList;
            }
            set {}
        }

        public bool UseConnections
        {
            get
            {
                return useConnections;
            }
            set
            {
                useConnections = value;
                FeatureType = useConnections ? typeof(Connection) : typeof(ConnectionPoint);
            }
        }

        public IEditableObject EditableObjectRefresh
        {
            get
            {
                return editableObjectRefresh;
            }
            set
            {
                if (editableObjectRefresh is INotifyPropertyChanged notifyPropertyChanged)
                {
                    notifyPropertyChanged.PropertyChanged -= EditableObjectPropertyChanged;
                }

                editableObjectRefresh = value;
                RefreshEventedList();
                RefreshCoordinateSystem();
                if (editableObjectRefresh is INotifyPropertyChanged notifyPropertyChanged2)
                {
                    notifyPropertyChanged2.PropertyChanged += EditableObjectPropertyChanged;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                EditableObjectRefresh = null;
                controlGroups.CollectionChanged -= ControlGroupsCollectionChanged;
            }

            base.Dispose(disposing);
        }

        private void ControlGroupsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Immediate action needed: Attribute table needs to be updated. 
            RefreshEventedList();
        }

        private void RefreshEventedList()
        {
            if (UseConnections)
            {
                connectionList.Clear();
                connectionList.AddRange(controlGroups.SelectMany(GetConnections));
            }
            else
            {
                connectionPointList.Clear();
                connectionPointList.AddRange(controlGroups.SelectMany(cg => cg.Inputs.Cast<ConnectionPoint>().Concat(cg.Outputs)));
            }
        }

        private IEnumerable<Connection> GetConnections(ControlGroup cg)
        {
            foreach (Output output in cg.Outputs.Where(output => output.IsConnected))
            {
                foreach (Input input in ControlGroupHelper.InputItemsForOutput(cg, output).Where(c => c.IsConnected))
                {
                    yield return new Connection(input, output);
                }
            }
        }

        private void RefreshCoordinateSystem()
        {
            var realTimeControlModel = editableObjectRefresh as IRealTimeControlModel;
            if (realTimeControlModel == null)
            {
                return;
            }

            if (!CoordinateSystem.EqualsTo(realTimeControlModel.CoordinateSystem))
            {
                CoordinateSystem = realTimeControlModel.CoordinateSystem;
            }
        }

        private void EditableObjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "IsEditing" || editableObjectRefresh.IsEditing)
            {
                RefreshEventedList();
            }

            if (e.PropertyName == "CoordinateSystem")
            {
                RefreshCoordinateSystem();
            }
        }
    }
}