using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

        public event EventHandler SelectedFeaturesChanged;

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
            boundaryConditionTableView.Dock = DockStyle.Fill;

            boundaryConditionTableView.SelectionChanged += TableViewOnSelectionChanged;

            Controls.Add(boundaryConditionTableView);
        }

        public Action<IFeature> ZoomToFeature { get; set; }

        public Action<object> OpenViewMethod { get; set; }

        public object Data
        {
            get
            {
                return data;
            }
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

        public Image Image
        {
            get
            {
                return boundaryConditionTableView.Image;
            }
            set
            {
                boundaryConditionTableView.Image = value;
            }
        }

        public ViewInfo ViewInfo { get; set; }

        public IEnumerable<IFeature> SelectedFeatures
        {
            get
            {
                IEnumerable<FlowBoundaryCondition> selectedBcs = boundaryConditionTableView.SelectedRowsIndices.Select(
                    i => boundaryConditions[boundaryConditionTableView.GetDataSourceIndexByRowIndex(i)]);
                return
                    selectedBcs.Select(bc => data.FirstOrDefault(bcs => bcs.BoundaryConditions.Contains(bc))).Distinct();
            }
            set
            {
                IEnumerable<BoundaryConditionSet> selectedBcsets = value.OfType<BoundaryConditionSet>();
                IEnumerable<int> selectedBcs =
                    selectedBcsets.SelectMany(bcs => bcs.BoundaryConditions)
                                  .OfType<FlowBoundaryCondition>()
                                  .Select(bc => boundaryConditions.IndexOf(bc)).Except(new[]
                                  {
                                      -1
                                  });

                boundaryConditionTableView.SelectRows(selectedBcs.ToArray());
            }
        }

        public ILayer Layer { get; set; }

        public void EnsureVisible(object item)
        {
            boundaryConditionTableView.EnsureVisible(item);
        }

        public void OnActivated()
        {
            // Nothing to be done, enforced through ILayerEditorView
        }

        public void OnDeactivated()
        {
            // Nothing to be done, enforced through ILayerEditorView
        }

        private void TableViewOnSelectionChanged(object sender, TableSelectionChangedEventArgs e)
        {
            if (SelectedFeaturesChanged != null)
            {
                SelectedFeaturesChanged(this, e);
            }
        }

        private void OnBoundaryConditionRemoved(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (BoundaryConditionSet boundaryConditionSet in data)
                {
                    boundaryConditionSet.BoundaryConditions.Remove(e.GetRemovedOrAddedItem() as IBoundaryCondition);
                }
            }
        }

        private void SubscribeToData()
        {
            data.CollectionChanged += OnBoundaryConditionSetsChanged;
            boundaryConditions.CollectionChanged += OnBoundaryConditionRemoved;
            foreach (BoundaryConditionSet boundaryConditionSet in data)
            {
                boundaryConditionSet.BoundaryConditions.CollectionChanged += OnBoundaryConditionSetsChanged;
            }
        }

        private void UnSubscribeToData()
        {
            data.CollectionChanged -= OnBoundaryConditionSetsChanged;
            boundaryConditions.CollectionChanged -= OnBoundaryConditionRemoved;
            foreach (BoundaryConditionSet boundaryConditionSet in data)
            {
                boundaryConditionSet.BoundaryConditions.CollectionChanged -= OnBoundaryConditionSetsChanged;
            }
        }

        [InvokeRequired]
        private void OnBoundaryConditionSetsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            object removedOrAddedItem = e.GetRemovedOrAddedItem();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    HandleAdd(removedOrAddedItem);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    HandleRemove(removedOrAddedItem);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    HandleReplace(e);
                    break;
            }
        }

        private void HandleAdd(object removedOrAddedItem)
        {
            var item = removedOrAddedItem as FlowBoundaryCondition;
            if (item != null)
            {
                boundaryConditions.Add(item);
            }
            else
            {
                var set = removedOrAddedItem as BoundaryConditionSet;
                if (set != null)
                {
                    foreach (FlowBoundaryCondition flowBoundaryCondition in set.BoundaryConditions.OfType<FlowBoundaryCondition>())
                    {
                        boundaryConditions.Add(flowBoundaryCondition);
                    }
                }
            }
        }

        private void HandleRemove(object removedOrAddedItem)
        {
            var condition = removedOrAddedItem as FlowBoundaryCondition;
            if (condition != null)
            {
                boundaryConditions.Remove(condition);
            }
            else
            {
                var set = removedOrAddedItem as BoundaryConditionSet;
                if (set != null)
                {
                    foreach (FlowBoundaryCondition flowBoundaryCondition in set.BoundaryConditions.OfType<FlowBoundaryCondition>())
                    {
                        boundaryConditions.Remove(flowBoundaryCondition);
                    }
                }
            }
        }
        
        private void HandleReplace(NotifyCollectionChangedEventArgs e)
        {
            foreach (object oldItem in e.OldItems)
            {
                HandleRemove(oldItem);
            }

            foreach (object newItem in e.NewItems)
            {
                HandleAdd(newItem);
            }
        }
    }
}