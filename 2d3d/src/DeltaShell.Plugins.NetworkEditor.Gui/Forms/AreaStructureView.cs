using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using DelftTools.Controls;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels;
using DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.Views;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms
{
    public partial class AreaStructureView : UserControl, IReusableView
    {
        private IStructureObject structure;
        private bool locked;

        public AreaStructureView()
        {
            InitializeComponent();
        }

        public IStructureObject Data
        {
            get => structure;
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

        public Control StructureControl { get; private set; }

        private void SetStructureName()
        {
            label2.Text = structure == null ? "No structure data set..." : structure.Name;
        }

        private void SetStructureTypeView()
        {
            foreach (IDisposable control in structureViewPanel.Controls.OfType<IDisposable>())
            {
                control.Dispose();
            }

            structureViewPanel.Controls.Clear();

            if (structure == null)
            {
                return;
            }

            if (structure is IPump pump)
            {
                StructureControl = new PumpView
                {
                    Dock = DockStyle.Fill,
                    Data = pump
                };
                structureViewPanel.AutoScrollMinSize = new Size(StructureControl.Width, StructureControl.Height);
                structureViewPanel.Controls.Add(StructureControl);
                return;
            }

            if (structure is IStructure weir)
            {
                var controlHost = new ElementHost
                {
                    Dock = DockStyle.Fill,
                    Child = new StructureView {DataContext = new StructureViewModel(weir)}
                };
                StructureControl = controlHost;
                structureViewPanel.AutoScrollMinSize = new Size(StructureControl.Width, StructureControl.Height);
                structureViewPanel.Controls.Add(StructureControl);
                return;
            }

            throw new NotSupportedException();
        }

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
            get => Data;
            set => Data = (IStructureObject) value;
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
            get => locked;
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