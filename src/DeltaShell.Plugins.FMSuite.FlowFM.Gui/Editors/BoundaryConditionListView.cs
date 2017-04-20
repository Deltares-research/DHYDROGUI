using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Table;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using GeoAPI.Extensions.Feature;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    public partial class BoundaryConditionListView : UserControl, ILayerEditorView
    {
        private readonly TableView boundaryConditionTableView;
        private IEventedList<FlowBoundaryCondition> boundaryConditions; 
        private IEventedList<BoundaryConditionSet> data;

        public BoundaryConditionListView()
        {
            InitializeComponent();
            boundaryConditionTableView = new TableView
            {
                AllowDeleteRow = true,
                AllowAddNewRow = false,
                AllowDrop = false,
                AutoGenerateColumns = false
            };

            boundaryConditionTableView.BestFitColumns();

            boundaryConditionTableView.AddColumn(FeaturePropertyName, FeaturePropertyDescription, true, 100);
            boundaryConditionTableView.AddColumn(QuantityPropertyName, QuantityPropertyDescription, true, 100);
            boundaryConditionTableView.AddColumn(ForcingTypePropertyName, ForcingTypePropertyDescription, true, 100);
            boundaryConditionTableView.AddColumn(FactorPropertyName, FactorPropertyDescription, false, 100);
            boundaryConditionTableView.AddColumn(OffsetPropertyName, OffsetPropertyDescription, false, 100);

            boundaryConditionTableView.SelectionChanged += TableViewOnSelectionChanged;

            Controls.Add(boundaryConditionTableView);
        }

        private void TableViewOnSelectionChanged(object sender, TableSelectionChangedEventArgs e)
        {
            if (SelectedFeaturesChanged != null)
            {
                SelectedFeaturesChanged(this, e);
            }
        }

        public Action<IFeature> ZoomToFeature { get; set; }

        public Action<object> OpenViewMethod { get; set; }

        public object Data
        {
            get { return data; }
            set
            {
                if (data != null)
                {
                    UnSubscribeToData();
                }
                data = value as IEventedList<BoundaryConditionSet>;
                if (data != null)
                {
                    boundaryConditions =
                        new EventedList<FlowBoundaryCondition>(
                            data.SelectMany(d => d.BoundaryConditions).OfType<FlowBoundaryCondition>());

                    SubscribeToData();

                    boundaryConditionTableView.Data = boundaryConditions;
                }
                else
                {
                    boundaryConditionTableView.Data = null;
                }
            }
        }

        private void OnBoundaryConditionRemoved(object sender, NotifyCollectionChangingEventArgs e)
        {
            if (e.Action == NotifyCollectionChangeAction.Remove)
            {
                foreach (var boundaryConditionSet in data)
                {
                    boundaryConditionSet.BoundaryConditions.Remove(e.Item as IBoundaryCondition);
                }
            }
        }

        private void SubscribeToData()
        {
            data.CollectionChanged += OnBoundaryConditionSetsChanged;
            boundaryConditions.CollectionChanged += OnBoundaryConditionRemoved;
            foreach (var boundaryConditionSet in data)
            {
                boundaryConditionSet.BoundaryConditions.CollectionChanged += OnBoundaryConditionSetsChanged;
            }
        }

        private void UnSubscribeToData()
        {
            data.CollectionChanged -= OnBoundaryConditionSetsChanged;
            boundaryConditions.CollectionChanged -= OnBoundaryConditionRemoved;
            foreach (var boundaryConditionSet in data)
            {
                boundaryConditionSet.BoundaryConditions.CollectionChanged -= OnBoundaryConditionSetsChanged;
            }
        }
        
        [InvokeRequired]
        private void OnBoundaryConditionSetsChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangeAction.Add:
                    var item = e.Item as FlowBoundaryCondition;
                    if (item != null)
                    {
                        boundaryConditions.Add(item);
                    }
                    else
                    {
                        var set = e.Item as BoundaryConditionSet;
                        if (set != null)
                        {
                            foreach (
                                var flowBoundaryCondition in
                                    set.BoundaryConditions.OfType<FlowBoundaryCondition>())
                            {
                                boundaryConditions.Add(flowBoundaryCondition);
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangeAction.Remove:
                    var condition = e.Item as FlowBoundaryCondition;
                    if (condition != null)
                    {
                        boundaryConditions.Remove(condition);
                    }
                    else
                    {
                        var set = e.Item as BoundaryConditionSet;
                        if (set != null)
                        {
                            foreach (
                                var flowBoundaryCondition in
                                    set.BoundaryConditions.OfType<FlowBoundaryCondition>())
                            {
                                boundaryConditions.Remove(flowBoundaryCondition);
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangeAction.Replace:
                    if (e.Item is FlowBoundaryCondition || e.Item is BoundaryConditionSet)
                    {
                        throw new NotImplementedException("Replacing boundary conditions is not supported");
                    }
                    break;
            }
        }

        public Image Image
        {
            get { return boundaryConditionTableView.Image; }
            set { boundaryConditionTableView.Image = value; }
        }
        public void EnsureVisible(object item)
        {
            boundaryConditionTableView.EnsureVisible(item);
        }

        public ViewInfo ViewInfo { get; set; }

        public IEnumerable<IFeature> SelectedFeatures
        {
            get
            {
                var selectedBcs = boundaryConditionTableView.SelectedRowsIndices.Select(
                    i => boundaryConditions[boundaryConditionTableView.GetDataSourceIndexByRowIndex(i)]);
                return
                    selectedBcs.Select(bc => data.FirstOrDefault(bcs => bcs.BoundaryConditions.Contains(bc))).Distinct();
            }
            set
            {
                var selectedBcsets = value.OfType<BoundaryConditionSet>();
                var selectedBcs =
                    selectedBcsets.SelectMany(bcs => bcs.BoundaryConditions)
                        .OfType<FlowBoundaryCondition>()
                        .Select(bc => boundaryConditions.IndexOf(bc)).Except(new[] {-1});

                boundaryConditionTableView.SelectRows(selectedBcs.ToArray());
            }
        }
        
        public event EventHandler SelectedFeaturesChanged;
        
        public ILayer Layer { get; set; }
        
        public void OnActivated(){}

        public void OnDeactivated(){}
    }
}
