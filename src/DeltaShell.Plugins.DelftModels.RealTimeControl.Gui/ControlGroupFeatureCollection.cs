using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms;
using SharpMap.Data.Providers;
using IEditableObject = DelftTools.Utils.Editing.IEditableObject;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui
{
    public class ControlGroupFeatureCollection : FeatureCollection
    {
        private IEditableObject editableObjectRefresh;

        private readonly IEventedList<ControlGroup> controlGroups;
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

        private void ControlGroupsCollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            // Immediate action needed: Attribute table needs to be updated. 
            RefreshEventedList();
        }

        public bool UseConnections
        {
            get { return useConnections; }
            set
            {
                useConnections = value;
                FeatureType = useConnections ? typeof(Connection) : typeof(ConnectionPoint);
            }
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
            foreach (var output in cg.Outputs.Where(output => output.IsConnected))
            {
                foreach (var input in ControlGroupHelper.InputItemsForOutput(cg, output).Where(c => c.IsConnected))
                {
                    yield return new Connection(input, output);
                }
            }
        }

        public override IList Features
        {
            get { return useConnections ? (IList) connectionList : (IList) connectionPointList; }
            set { }
        }

        public IEditableObject EditableObjectRefresh
        {
            get { return editableObjectRefresh; }
            set 
            { 
                if (editableObjectRefresh is INotifyPropertyChanged)
                {
                    ((INotifyPropertyChanged)editableObjectRefresh).PropertyChanged -= EditableObjectPropertyChanged;
                }

                editableObjectRefresh = value;
                RefreshEventedList();

                if (editableObjectRefresh is INotifyPropertyChanged)
                {
                    ((INotifyPropertyChanged)editableObjectRefresh).PropertyChanged += EditableObjectPropertyChanged;
                }
            }
        }

        void EditableObjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "IsEditing" || editableObjectRefresh.IsEditing) return;
            RefreshEventedList();
        }

        public override void Dispose()
        {
            EditableObjectRefresh = null;

            controlGroups.CollectionChanged -= ControlGroupsCollectionChanged;
            base.Dispose();
        }
    }
}