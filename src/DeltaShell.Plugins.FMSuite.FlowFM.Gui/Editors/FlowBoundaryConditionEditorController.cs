using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.Gui.Editors;
using DeltaShell.Plugins.FMSuite.Common.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    public class FlowBoundaryConditionEditorController : BoundaryConditionEditorController
    {
        private WaterFlowFMModel model;

        public override BoundaryConditionEditor Editor
        {
            protected get
            {
                return base.Editor;
            }
            set
            {
                base.Editor = value;
                if (Editor != null)
                {
                    Editor.SupportedVerticalProfileTypes = SupportedVerticalProfileTypes.BoundaryConditionProfileTypes;
                    Editor.SupportPointListBoxContextMenuItems = new ToolStripItem[]
                        {
                            new ToolStripMenuItem("Properties...", null, OnLocationPropertiesClick)
                        };
                    if (Model != null)
                    {
                        Editor.ModelDepthLayerDefinition = Model.DepthLayerDefinition;
                        Editor.DepthLayerControlVisible = Model.UseDepthLayers;
                    }
                }
            }
        }

        public WaterFlowFMModel Model
        {
            get { return model; }
            set
            {
                if (model != null)
                {
                    ((INotifyPropertyChanged)model).PropertyChanged -= OnModelPropertyChanged;
                    ((INotifyCollectionChanged) model).CollectionChanged -= OnModelCollectionChanged;
                }
                model = value;
                if (model != null)
                {
                    if (Editor != null)
                    {
                        Editor.ModelDepthLayerDefinition = model.DepthLayerDefinition;
                        Editor.DepthLayerControlVisible = model.UseDepthLayers;
                    }
                    ((INotifyPropertyChanged)model).PropertyChanged += OnModelPropertyChanged;
                    ((INotifyCollectionChanged)model).CollectionChanged += OnModelCollectionChanged;
                }
            }
        }

        private void OnModelCollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            // return if there is no editor.
            if (Editor == null || model == null) return;

            if (Equals(sender, model.TracerDefinitions))
            {
                Editor.RefreshQuantitiesComboBox();
            }
        }

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // return if there is no editor.
            if (Editor == null) return;

            if (e.PropertyName == TypeUtils.GetMemberName(() => Model.UseSalinity) ||
                e.PropertyName == TypeUtils.GetMemberName(() => Model.HeatFluxModelType))
            {
                Editor.RefreshAvailableCategories();
            }
            else if (e.PropertyName == TypeUtils.GetMemberName(() => Model.UseDepthLayers))
            {
                Editor.ModelDepthLayerDefinition = Model.DepthLayerDefinition;
                Editor.DepthLayerControlVisible = Model.UseDepthLayers;
                Editor.UpdateGeometryPanel();
            }
            else if (e.PropertyName == TypeUtils.GetMemberName(() => Model.DepthLayerDefinition))
            {
                Editor.ModelDepthLayerDefinition = Model.DepthLayerDefinition;
                Editor.UpdateGeometryPanel();
            }
        }

        public override void OnBoundaryConditionSelectionChanged(IBoundaryCondition boundaryCondition)
        {
            var previousView = Editor.BoundaryConditionDataView as FlowBoundaryConditionDataView;
            if (previousView != null)
            {
                Editor.SelectedSupportPointChanged -= previousView.OnSupportPointChanged;
                previousView.BoundaryCondition = null;
                previousView.BoundaryConditionSet = null;
                previousView.Model = null;
            }
            var view = new FlowBoundaryConditionDataView
                {
                    Model = Model,
                    BoundaryConditionSet = (BoundaryConditionSet) Editor.Data,
                    BoundaryCondition = boundaryCondition,
                    SupportPointIndex = Editor.SelectedSupportPointIndex,
                };
            
            Editor.SelectedSupportPointChanged += view.OnSupportPointChanged;
            view.RefreshBoundaryData();
            view.UpdateControl();
            Editor.BoundaryConditionDataView = view;
            Editor.ChildViews.Add(view);
        }

        private void OnLocationPropertiesClick(object sender, EventArgs eventArgs)
        {
            var dialog = new SupportPointPropertiesForm(Editor.BoundaryConditionSet, Editor.SelectedSupportPointIndex,
                                                        Model == null ? null : Model.CoordinateSystem);
               
            if (dialog.ShowDialog(Editor) == DialogResult.OK)
            {
                Editor.UpdateSupportPointLabel(Editor.SelectedSupportPointIndex);
            }
        }

        public static IEnumerable<FlowBoundaryQuantityType> SupportedFlowQuantities
        {
            get
            {
                yield return FlowBoundaryQuantityType.WaterLevel;
                yield return FlowBoundaryQuantityType.Discharge;
                yield return FlowBoundaryQuantityType.Velocity;
                yield return FlowBoundaryQuantityType.NormalVelocity;
                yield return FlowBoundaryQuantityType.TangentVelocity;
//                yield return FlowBoundaryQuantityType.VelocityVector;
                yield return FlowBoundaryQuantityType.Riemann;
//                yield return FlowBoundaryQuantityType.RiemannVelocity;
                yield return FlowBoundaryQuantityType.Neumann;
//                yield return FlowBoundaryQuantityType.Outflow;
                yield return FlowBoundaryQuantityType.Salinity;
                yield return FlowBoundaryQuantityType.Temperature;
                yield return FlowBoundaryQuantityType.Tracer;
            }
        }

        public override IEnumerable<string> SupportedProcessNames
        {
            get
            {
                return SupportedFlowQuantities.Select(FlowBoundaryCondition.GetProcessNameForQuantity)
                                              .Where(IsActiveProcess)
                                              .ToList().Distinct();
            }
        }

        private bool IsActiveProcess(string process)
        {
            // Filter out Salinity and temperature if not active:
            if (process == FlowBoundaryCondition.GetProcessNameForQuantity(FlowBoundaryQuantityType.Salinity))
            {
                return (Model != null && Model.UseSalinity);
            }
            if (process == FlowBoundaryCondition.GetProcessNameForQuantity(FlowBoundaryQuantityType.Temperature))
            {
                return (Model != null && Model.UseTemperature);
            }
            return true;
        }

        private IEnumerable<FlowBoundaryQuantityType> GetQuantitiesForProcess(string process)
        {
            if (!IsActiveProcess(process)) yield break;

            foreach (
                var quantity in
                    SupportedFlowQuantities.Where(
                        quantity => FlowBoundaryCondition.GetProcessNameForQuantity(quantity) == process))
            {
                yield return quantity;
            }
        }

        public override IEnumerable<string> GetVariablesForProcess(string category)
        {
            return GetQuantitiesForProcess(category).Select(FlowBoundaryCondition.GetVariableNameForQuantity);
        }

        private static IEnumerable<FlowBoundaryQuantityType> GetAllowedQuantitiesFor(string process,
            BoundaryConditionSet boundaryConditions)
        {
            if (boundaryConditions == null)
            {
                return Enumerable.Empty<FlowBoundaryQuantityType>();
            }

            var existingQuantities =
                boundaryConditions.BoundaryConditions.OfType<FlowBoundaryCondition>()
                    .Select(fbc => fbc.FlowQuantity).Distinct().ToList();

            var count = existingQuantities.Except(FlowBoundaryCondition.AlwaysAllowedQuantities).Count() + 1;

            var validCombinationResults =
                FlowBoundaryCondition.ValidBoundaryConditionCombinations.Where(
                    l => l.Count == count && l.Except(existingQuantities).Count() == 1)
                    .SelectMany(l => l).Distinct();

            return
                validCombinationResults.Concat(existingQuantities)
                    .Concat(FlowBoundaryCondition.AlwaysAllowedQuantities)
                    .Distinct()
                    .Where(q => FlowBoundaryCondition.GetProcessNameForQuantity(q) == process);
        }

        public override IEnumerable<string> GetAllowedVariablesFor(string category, BoundaryConditionSet boundaryConditions)
        {
            return
                GetAllowedQuantitiesFor(category, boundaryConditions).Intersect(SupportedFlowQuantities)
                    .SelectMany(GetVariableNamesForQuantity);
        }

        private IEnumerable<string> GetVariableNamesForQuantity(FlowBoundaryQuantityType type)
        {
            if (type == FlowBoundaryQuantityType.Tracer)
            {
                foreach (string tracerDefinition in model.TracerDefinitions)
                {
                    yield return tracerDefinition;
                }
            }
            else
            {
                yield return FlowBoundaryCondition.GetVariableNameForQuantity(type);
            }
        }

        public override string GetVariableDescription(string variable)
        {
            FlowBoundaryQuantityType flowBoundaryQuantityType;

            return Enum.TryParse(variable, out flowBoundaryQuantityType)
                       ? FlowBoundaryCondition.GetDescription(flowBoundaryQuantityType)
                       : base.GetVariableDescription(variable);
        }


        public override IEnumerable<BoundaryConditionDataType> GetSupportedDataTypesForVariable(string variable)
        {
            FlowBoundaryQuantityType flowBoundaryQuantityType;

            if (Enum.TryParse(variable, out flowBoundaryQuantityType))
            {
                return FlowBoundaryCondition.GetSupportedDataTypesForQuantity(flowBoundaryQuantityType);
            }
            // tracers are a special case. They need to be checked with StartsWith.
            else if (model.TracerDefinitions.Contains(variable))
            {
                return FlowBoundaryCondition.GetSupportedDataTypesForQuantity(FlowBoundaryQuantityType.Tracer);
            }
            else return Enumerable.Empty<BoundaryConditionDataType>();
        }

        public override void InsertBoundaryCondition(BoundaryConditionSet boundaryConditionSet, IBoundaryCondition boundaryCondition)
        {
            var flowBoundaryCondition = boundaryCondition as FlowBoundaryCondition;
            if (flowBoundaryCondition == null)
            {
                base.InsertBoundaryCondition(boundaryConditionSet, boundaryCondition);
            }
            else
            {
                if (FlowBoundaryCondition.AlwaysAllowedQuantities.Contains(flowBoundaryCondition.FlowQuantity))
                {
                    boundaryConditionSet.BoundaryConditions.Add(boundaryCondition);
                }
                else
                {
                    var firstTransportConstituent =
                        boundaryConditionSet.BoundaryConditions.OfType<FlowBoundaryCondition>()
                            .FirstOrDefault(
                                bc => FlowBoundaryCondition.AlwaysAllowedQuantities.Contains(bc.FlowQuantity));


                    if (firstTransportConstituent != null)
                    {
                        var index = boundaryConditionSet.BoundaryConditions.IndexOf(firstTransportConstituent);
                        boundaryConditionSet.BoundaryConditions.Insert(index, boundaryCondition);
                    }
                    else
                    {
                        boundaryConditionSet.BoundaryConditions.Add(boundaryCondition);
                    }
                }
            }
        }

        public override void Dispose()
        {
            Model = null;
            base.Dispose();
        }
    }
}
