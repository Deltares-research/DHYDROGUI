using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms
{
    public partial class AreaStructureView : UserControl, IReusableView
    {
        private IStructure1D structure;
        private bool locked;

        public AreaStructureView()
        {
            InitializeComponent();
        }

        public IStructure1D Data
        {
            get { return structure; }
            set
            {
                if (structure != null)
                {
                    var propertyChangedStructure = structure as INotifyPropertyChanged;
                    if (propertyChangedStructure != null)
                    {
                        propertyChangedStructure.PropertyChanged -= PropertyChangedHandler;
                    }
                }

                structure = value;

                SetStructureTypeView();
                SetStructureName();

                if (structure != null)
                {
                    var propertyChangedStructure = structure as INotifyPropertyChanged;
                    if (propertyChangedStructure != null)
                    {
                        propertyChangedStructure.PropertyChanged += PropertyChangedHandler;
                    }
                }
            }
        }

        private void SetStructureName()
        {
            label2.Text = structure == null ? "No structure data set..." : structure.Name;
        }

        private void SetStructureTypeView()
        {
            foreach (var control in structureViewPanel.Controls.OfType<IDisposable>())
            {
                control.Dispose();
            }
            structureViewPanel.Controls.Clear();

            if (structure == null) return;

            if (structure is IPump)
            {
                StructureControl = new PumpView
                {
                    Dock = DockStyle.Fill,
                    Data = (IPump) structure
                };
                structureViewPanel.AutoScrollMinSize = new Size(StructureControl.Width, StructureControl.Height);
                structureViewPanel.Controls.Add(StructureControl);
                return;
            }

            if (structure is IWeir)
            {
                var controlHost = new ElementHost
                {
                    Dock = DockStyle.Fill,
                    Child = new WeirViewWpf { Data = structure },
                };
                StructureControl = controlHost;
                structureViewPanel.AutoScrollMinSize = new Size(StructureControl.Width, StructureControl.Height);
                structureViewPanel.Controls.Add(StructureControl);
                return;
            }

            if (structure is IGate)
            {
                StructureControl = new GateView
                {
                    Dock = DockStyle.Fill,
                    Data = structure,
                };
                structureViewPanel.AutoScrollMinSize = new Size(StructureControl.Width, StructureControl.Height);
                structureViewPanel.Controls.Add(StructureControl);
                return;
            }
            throw new NotImplementedException();
        }

        public Control StructureControl { get; private set; }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name")
            {
                SetStructureName();
            }
        }

        #region IView

        object IView.Data
        {
            get { return Data; }
            set { Data = (IStructure1D) value; }
        }
        public Image Image { get; set; }
        public void EnsureVisible(object item)
        {
            // Do nothing
        }

        public ViewInfo ViewInfo { get; set; }

        #endregion

        #region IReusableView

        public bool Locked
        {
            get { return locked; }
            set
            {

                locked = value;
                if (LockedChanged != null)
                {
                    LockedChanged(this, new EventArgs());
                }
            }
        }

        public event EventHandler LockedChanged;

        #endregion
    }
}
