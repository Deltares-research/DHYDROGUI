using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    public sealed class FlowBoundaryConditionEditorController : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FlowBoundaryConditionEditorController));
        private WaterFlowFMModel model;
        private BoundaryConditionEditor editor;

        public IEnumerable<BoundaryConditionDataType> AllSupportedDataTypes
        {
            get
            {
                return SupportedProcessNames.SelectMany(GetVariablesForProcess)
                                            .SelectMany(GetSupportedDataTypesForVariable)
                                            .Distinct();
            }
        }

        public BoundaryConditionEditor Editor
        {
            private get
            {
                return editor;
            }
            set
            {
                editor = value;
                if (editor != null)
                {
                    editor.SupportedVerticalProfileTypes = SupportedVerticalProfileTypes.BoundaryConditionProfileTypes;
                    editor.SupportPointListBoxContextMenuItems = new ToolStripItem[]
                    {
                        new ToolStripMenuItem("Properties...", null, OnLocationPropertiesClick)
                    };
                    if (Model != null)
                    {
                        editor.ModelDepthLayerDefinition = Model.DepthLayerDefinition;
                        editor.DepthLayerControlVisible = Model.UseDepthLayers;
                    }
                }
            }
        }

        public IEnumerable<string> SupportedProcessNames
        {
            get
            {
                return SupportedFlowQuantities.Select(FlowBoundaryCondition.GetProcessNameForQuantity)
                                 .Where(IsActiveProcess)
                                 .ToList().Distinct();
            }
        }

        public WaterFlowFMModel Model
        {
            get
            {
                return model;
            }
            set
            {
                if (model != null)
                {
                    ((INotifyPropertyChanged) model).PropertyChanged -= OnModelPropertyChanged;
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

                    ((INotifyPropertyChanged) model).PropertyChanged += OnModelPropertyChanged;
                    ((INotifyCollectionChanged) model).CollectionChanged += OnModelCollectionChanged;
                }
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
                yield return FlowBoundaryQuantityType.Riemann;
                yield return FlowBoundaryQuantityType.Neumann;
                yield return FlowBoundaryQuantityType.Salinity;
                yield return FlowBoundaryQuantityType.Temperature;
                yield return FlowBoundaryQuantityType.SedimentConcentration;
                yield return FlowBoundaryQuantityType.Tracer;
                yield return FlowBoundaryQuantityType.MorphologyBedLevelPrescribed;
                yield return FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed;
                yield return FlowBoundaryQuantityType.MorphologyBedLoadTransport;
                yield return FlowBoundaryQuantityType.MorphologyNoBedLevelConstraint;
                yield return FlowBoundaryQuantityType.MorphologyBedLevelFixed;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void OnBoundaryConditionSelectionChanged(IBoundaryCondition boundaryCondition)
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
                SupportPointIndex = Editor.SelectedSupportPointIndex
            };

            Editor.SelectedSupportPointChanged += view.OnSupportPointChanged;
            view.RefreshBoundaryData();
            view.UpdateControl();
            Editor.BoundaryConditionDataView = view;
            Editor.ChildViews.Add(view);
        }

        public IEnumerable<string> GetVariablesForProcess(string category)
        {
            return GetQuantitiesForProcess(category).Select(FlowBoundaryCondition.GetVariableNameForQuantity);
        }

        public IEnumerable<string> GetAllowedVariablesFor(string category, BoundaryConditionSet boundaryConditions)
        {
            return
                GetAllowedQuantitiesFor(category, boundaryConditions).Intersect(SupportedFlowQuantities)
                                                                     .SelectMany(GetVariableNamesForQuantity);
        }

        public string GetVariableDescription(string variable, string category = null)
        {
            FlowBoundaryQuantityType flowBoundaryQuantityType;

            if (category != FlowBoundaryQuantityType.Tracer.GetDescription() &&                // Do not try to match Tracers to enum descriptions
                category != FlowBoundaryQuantityType.SedimentConcentration.GetDescription() && // Do not try to match Fraction names to enum descriptions
                Enum.TryParse(variable, out flowBoundaryQuantityType))
            {
                return FlowBoundaryCondition.GetDescription(flowBoundaryQuantityType);
            }
            return variable ?? "";
        }

        public IEnumerable<BoundaryConditionDataType> GetSupportedDataTypesForVariable(string variable)
        {
            FlowBoundaryQuantityType flowBoundaryQuantityType;

            if (Enum.TryParse(variable, out flowBoundaryQuantityType))
            {
                if (!flowBoundaryQuantityType.Equals(FlowBoundaryQuantityType.MorphologyBedLoadTransport)
                    || flowBoundaryQuantityType.Equals(FlowBoundaryQuantityType.MorphologyBedLoadTransport) && model.SedimentFractions.Count > 0)
                {
                    return FlowBoundaryCondition.GetSupportedDataTypesForQuantity(flowBoundaryQuantityType);
                }
                else
                {
                    if (flowBoundaryQuantityType.Equals(FlowBoundaryQuantityType.MorphologyBedLoadTransport) &&
                        model.SedimentFractions.Count == 0)
                    {
                        Log.Warn("First, at least a fraction must be created.");
                    }
                }
            }
            // tracers are a special case. They need to be checked with StartsWith.
            else if (model.TracerDefinitions.Contains(variable))
            {
                return FlowBoundaryCondition.GetSupportedDataTypesForQuantity(FlowBoundaryQuantityType.Tracer);
            }
            else if (model.SedimentFractions.Select(f => f.Name).Contains(variable))
            {
                return FlowBoundaryCondition.GetSupportedDataTypesForQuantity(FlowBoundaryQuantityType.SedimentConcentration);
            }

            return Enumerable.Empty<BoundaryConditionDataType>();
        }

        // Keep the 'correct' ordering: first flow bc's, then salinity and temperature.
        public void InsertBoundaryCondition(BoundaryConditionSet boundaryConditions, IBoundaryCondition boundaryCondition)
        {
            var flowBoundaryCondition = boundaryCondition as FlowBoundaryCondition;
            if (flowBoundaryCondition == null)
            {
                boundaryConditions.BoundaryConditions.Add(boundaryCondition);
            }
            else
            {
                if (FlowBoundaryCondition.AlwaysAllowedQuantities.Contains(flowBoundaryCondition.FlowQuantity))
                {
                    boundaryConditions.BoundaryConditions.Add(boundaryCondition);
                }
                else
                {
                    FlowBoundaryCondition firstTransportConstituent =
                        boundaryConditions.BoundaryConditions.OfType<FlowBoundaryCondition>()
                                          .FirstOrDefault(
                                              bc => FlowBoundaryCondition.AlwaysAllowedQuantities.Contains(bc.FlowQuantity));

                    if (firstTransportConstituent != null)
                    {
                        int index = boundaryConditions.BoundaryConditions.IndexOf(firstTransportConstituent);
                        boundaryConditions.BoundaryConditions.Insert(index, boundaryCondition);
                    }
                    else
                    {
                        boundaryConditions.BoundaryConditions.Add(boundaryCondition);
                    }
                }
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Model = null;
            }
        }

        private void OnModelCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // return if there is no editor.
            if (Editor == null || model == null)
            {
                return;
            }

            if (Equals(sender, model.TracerDefinitions)
                || Equals(sender, model.SedimentFractions))
            {
                Editor.RefreshQuantitiesComboBox();
            }
        }

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // return if there is no editor.
            if (Editor == null)
            {
                return;
            }

            if (e.PropertyName == nameof(Model.UseMorSed) ||
                e.PropertyName == nameof(Model.UseSalinity) ||
                e.PropertyName == nameof(Model.HeatFluxModelType))
            {
                Editor.RefreshAvailableCategories();
            }
            else if (e.PropertyName == nameof(Model.UseDepthLayers))
            {
                Editor.ModelDepthLayerDefinition = Model.DepthLayerDefinition;
                Editor.DepthLayerControlVisible = Model.UseDepthLayers;
                Editor.UpdateGeometryPanel();
            }
            else if (e.PropertyName == nameof(Model.DepthLayerDefinition))
            {
                Editor.ModelDepthLayerDefinition = Model.DepthLayerDefinition;
                Editor.UpdateGeometryPanel();
            }
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

        private bool IsActiveProcess(string process)
        {
            // Filter out Sediment, SedimentConcentration, Salinity and temperature if not active:
            if (process == FlowBoundaryCondition.GetProcessNameForQuantity(FlowBoundaryQuantityType.Salinity))
            {
                return Model != null && Model.UseSalinity;
            }

            if (process == FlowBoundaryCondition.GetProcessNameForQuantity(FlowBoundaryQuantityType.Temperature))
            {
                return Model != null && Model.UseTemperature;
            }

            if (process == FlowBoundaryCondition.GetProcessNameForQuantity(FlowBoundaryQuantityType.SedimentConcentration)
                || process == FlowBoundaryCondition.GetProcessNameForQuantity(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed)
                || process == FlowBoundaryCondition.GetProcessNameForQuantity(FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed)
                || process == FlowBoundaryCondition.GetProcessNameForQuantity(FlowBoundaryQuantityType.MorphologyBedLoadTransport)
                || process == FlowBoundaryCondition.GetProcessNameForQuantity(FlowBoundaryQuantityType.MorphologyNoBedLevelConstraint)
                || process == FlowBoundaryCondition.GetProcessNameForQuantity(FlowBoundaryQuantityType.MorphologyBedLevelFixed))
            {
                return Model != null && Model.UseMorSed;
            }

            return true;
        }

        private IEnumerable<FlowBoundaryQuantityType> GetQuantitiesForProcess(string process)
        {
            if (!IsActiveProcess(process))
            {
                yield break;
            }

            foreach (
                FlowBoundaryQuantityType quantity in
                SupportedFlowQuantities.Where(
                    quantity => FlowBoundaryCondition.GetProcessNameForQuantity(quantity) == process))
            {
                yield return quantity;
            }
        }

        private static IEnumerable<FlowBoundaryQuantityType> GetAllowedQuantitiesFor(string process,
                                                                                     BoundaryConditionSet boundaryConditions)
        {
            if (boundaryConditions == null)
            {
                return Enumerable.Empty<FlowBoundaryQuantityType>();
            }

            List<FlowBoundaryQuantityType> existingQuantities =
                boundaryConditions.BoundaryConditions.OfType<FlowBoundaryCondition>()
                                  .Select(fbc => fbc.FlowQuantity).Distinct().ToList();

            int count = existingQuantities.Except(FlowBoundaryCondition.AlwaysAllowedQuantities).Count() + 1;

            IEnumerable<FlowBoundaryQuantityType> validCombinationResults =
                FlowBoundaryCondition.ValidBoundaryConditionCombinations.Where(
                                         l => l.Count == count && l.Except(existingQuantities).Count() == 1)
                                     .SelectMany(l => l).Distinct();

            List<FlowBoundaryQuantityType> allowedQuantities = validCombinationResults.Concat(existingQuantities)
                                                                                      .Concat(FlowBoundaryCondition.AlwaysAllowedQuantities)
                                                                                      .Distinct()
                                                                                      .Where(q => FlowBoundaryCondition.GetProcessNameForQuantity(q) == process).ToList();

            if (boundaryConditions.BoundaryConditions
                                  .Where(bc => FlowBoundaryCondition.IsMorphologyBoundary(bc))
                                  .ToList()
                                  .Count >= 1)
            {
                allowedQuantities.RemoveAllWhere(q => FlowBoundaryCondition.IsMorphologyFlowQuantityType(q));
            }

            return allowedQuantities;
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
            else if (type == FlowBoundaryQuantityType.SedimentConcentration)
            {
                foreach (ISedimentFraction fraction in model.SedimentFractions.Where(sf => sf.CurrentSedimentType.Key != "bedload"))
                {
                    yield return fraction.Name;
                }
            }
            else
            {
                yield return FlowBoundaryCondition.GetVariableNameForQuantity(type);
            }
        }
    }
}