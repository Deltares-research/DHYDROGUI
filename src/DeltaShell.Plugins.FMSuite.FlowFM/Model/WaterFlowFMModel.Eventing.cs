using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Editing;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Model
{
    public partial class WaterFlowFMModel
    {
        private bool updatingGroupName;

        #region Spatial data

        public bool InitialCoverageSetChanged { get; set; }

        #endregion

        public object WaveModel
        {
            // cannot actually return anything, because it's a dynamic enum
            get => null;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        private void SourcesAndSinksCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var sourceAndSink = e.GetRemovedOrAddedItem() as SourceAndSink;

            if (sourceAndSink == null)
            {
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                SyncFractionsAndTracers(sourceAndSink);
            }
        }

        private void BoundaryConditionSetsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IEnumerable<FlowBoundaryCondition> tracerBoundaryConditions = Enumerable.Empty<FlowBoundaryCondition>();

            object removedOrAddedItem = e.GetRemovedOrAddedItem();
            var boundaryConditionSet = removedOrAddedItem as BoundaryConditionSet;
            if (boundaryConditionSet == null)
            {
                var flowBoundaryCondition = removedOrAddedItem as FlowBoundaryCondition;
                if (flowBoundaryCondition != null &&
                    flowBoundaryCondition.FlowQuantity == FlowBoundaryQuantityType.Tracer)
                {
                    tracerBoundaryConditions = new List<FlowBoundaryCondition>
                    {
                        flowBoundaryCondition
                    };
                }
            }
            else
            {
                tracerBoundaryConditions = boundaryConditionSet.BoundaryConditions
                                                               .OfType<FlowBoundaryCondition>()
                                                               .Where(fbc => fbc.FlowQuantity ==
                                                                             FlowBoundaryQuantityType.Tracer);
            }

            foreach (FlowBoundaryCondition tracerBoundaryCondition in tracerBoundaryConditions)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        AddTracerToSourcesAndSink(tracerBoundaryCondition.TracerName);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        RemoveTracerFromSourcesAndSink(tracerBoundaryCondition.TracerName);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        throw new NotImplementedException("Renaming of Tracers is not yet supported");
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        SourcesAndSinks.ForEach(ss => ss.TracerNames.Clear());
                        return;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void TracerDefinitionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            object removedOrAddedItem = e.GetRemovedOrAddedItem();
            var name = (string) removedOrAddedItem;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddToInitialTracers(name);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    // sync the initial tracers
                    InitialTracers.RemoveAllWhere(tr => tr.Name == name);

                    // remove all boundary conditions with that tracer name
                    foreach (BoundaryConditionSet set in BoundaryConditionSets)
                    {
                        set.BoundaryConditions.RemoveAllWhere(bc =>
                        {
                            var flowCondition = bc as FlowBoundaryCondition;

                            if (flowCondition != null &&
                                flowCondition.FlowQuantity == FlowBoundaryQuantityType.Tracer &&
                                Equals(flowCondition.TracerName, removedOrAddedItem))
                            {
                                return true;
                            }

                            return false;
                        });
                    }

                    break;
                case NotifyCollectionChangedAction.Replace:
                    // can't rename yet
                    throw new NotImplementedException("Renaming of tracer definitions is not yet supported");
                    break;
                case NotifyCollectionChangedAction.Reset:
                    // sync the initial tracers
                    InitialTracers.Clear();

                    // remove all tracer boundary conditions
                    foreach (BoundaryConditionSet set in BoundaryConditionSets)
                    {
                        set.BoundaryConditions.RemoveAllWhere(bc =>
                        {
                            var flowCondition = bc as FlowBoundaryCondition;
                            return flowCondition != null &&
                                   flowCondition.FlowQuantity == FlowBoundaryQuantityType.Tracer;
                        });
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SedimentFractionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name")
            {
                var sedimentFraction = sender as ISedimentFraction;

                if (sedimentFraction != null)
                {
                    sedimentFraction.UpdateSpatiallyVaryingNames();
                }
            }

            if (e.PropertyName == "CurrentFormulaType"
                || e.PropertyName == "CurrentSedimentType")
            {
                var sedimentFraction = sender as ISedimentFraction;
                if (sedimentFraction != null)
                {
                    sedimentFraction.UpdateSpatiallyVaryingNames();
                    List<string> activeSpatiallyVarying = sedimentFraction.GetAllActiveSpatiallyVaryingPropertyNames();
                    List<string> spatiallyVarying = sedimentFraction.GetAllSpatiallyVaryingPropertyNames();
                    InitialFractions.RemoveAllWhere(
                        fr => spatiallyVarying.Contains(fr.Name) && !activeSpatiallyVarying.Contains(fr.Name));

                    foreach (string layerName in activeSpatiallyVarying)
                    {
                        AddToInitialFractions(layerName);
                    }

                    sedimentFraction.CompileAndSetVisibilityAndIfEnabled();

                    if (e.PropertyName == "CurrentFormulaType")
                    {
                        sedimentFraction.SetTransportFormulaInCurrentSedimentType();
                    }
                }

                return;
            }

            var prop = sender as ISpatiallyVaryingSedimentProperty;
            if (prop == null)
            {
                return;
            }

            if (e.PropertyName == "IsSpatiallyVarying")
            {
                if (prop.IsSpatiallyVarying)
                {
                    AddToInitialFractions(prop.SpatiallyVaryingName);
                }
                else
                {
                    InitialFractions.RemoveAllWhere(tr => tr.Name.Equals(prop.SpatiallyVaryingName));
                }
            }
        }

        private void SedimentFractionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var sedimentFraction = e.GetRemovedOrAddedItem() as ISedimentFraction;
            if (sedimentFraction == null)
            {
                return;
            }

            string name = sedimentFraction.Name;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    sedimentFraction.UpdateSpatiallyVaryingNames();
                    sedimentFraction.CompileAndSetVisibilityAndIfEnabled();
                    sedimentFraction.SetTransportFormulaInCurrentSedimentType();
                    SourcesAndSinks.ForEach(ss => ss.SedimentFractionNames.Add(sedimentFraction.Name));

                    if (InitialFractions == null || BoundaryConditionSets == null)
                    {
                        break;
                    }

                    // sync the initial fractions
                    SyncInitialFractions(sedimentFraction);
                    AddSedimentFractionToFlowBoundaryConditionFunction(name);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    // sync the initial fractions
                    List<string> layersToRemove = sedimentFraction.GetAllActiveSpatiallyVaryingPropertyNames();
                    InitialFractions.RemoveAllWhere(ifs => layersToRemove.Contains(ifs.Name));

                    // Remove dataItems for coverages related to Removed Fraction
                    DataItems.RemoveAllWhere(di => di.Value is UnstructuredGridCoverage &&
                                                   layersToRemove.Contains(di.Name));
                    RemoveSedimentFractionFromBoundaryConditionSets(name);

                    SourcesAndSinks.ForEach(ss => ss.SedimentFractionNames.Remove(sedimentFraction.Name));
                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException("Renaming of sediment fraction is not yet supported");
                    break;
                case NotifyCollectionChangedAction.Reset:
                    // sync the initial fractions
                    InitialFractions.Clear();

                    RemoveAllSedimentFractionsFromBoundaryConditionSets();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnModelDefinitionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var prop = (WaterFlowFMProperty) sender;
            if (e.PropertyName == nameof(prop.Value))
            {
                if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.FixedWeirScheme,
                                                                   StringComparison.InvariantCultureIgnoreCase))
                {
                    fixedWeirProperties.Values.ForEach(p => p.UpdateDataColumns(prop.GetValueAsString()));
                }

                if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.BedlevType,
                                                                   StringComparison.InvariantCultureIgnoreCase))
                {
                    var bedLevelType = (UnstructuredGridFileHelper.BedLevelLocation) prop.Value;
                    BeginEdit(new DefaultEditAction("Updating Bathymetry coverage"));
                    UpdateBathymetryCoverage(bedLevelType);
                    EndEdit();
                }

                if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.UseSalinity,
                                                                   StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching salinity process", KnownProperties.UseSalinity,
                                           o => UseSalinity = (bool) o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.UseMorSed,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching morphology process", GuiProperties.UseMorSed,
                                           o => UseMorSed = (bool) o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.WriteSnappedFeatures,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching write snapped features options",
                                           GuiProperties.WriteSnappedFeatures, o => WriteSnappedFeatures = (bool) o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.ISlope,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching Bed slope formulation"));
                    EndEdit();
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.IHidExp,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching Hiding and exposure formulation"));
                    EndEdit();
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.Kmx,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching 3D dynamics", KnownProperties.Kmx,
                                           o => UseDepthLayers = (bool) o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.ICdtyp,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching wind formulation type", KnownProperties.ICdtyp,
                                           o => CdType = (int) o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.Temperature,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching heat flux model"));
                    HeatFluxModelType = ModelDefinition.HeatFluxModel.Type;
                    EndEdit();
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.SecondaryFlow,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching secondary flow process", KnownProperties.SecondaryFlow,
                                           o => UseSecondaryFlow = (bool) o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.WriteHisFile,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching WriteHisFile", GuiProperties.WriteHisFile,
                                           o => WriteHisFile = (bool) o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.SpecifyHisStart,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching SpecifyHisStart", GuiProperties.SpecifyHisStart,
                                           o => SpecifyHisStart = (bool) o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.SpecifyHisStop,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching SpecifyHisStop", GuiProperties.SpecifyHisStop,
                                           o => SpecifyHisStop = (bool) o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.WriteMapFile,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching WriteMapFile", GuiProperties.WriteMapFile,
                                           o => WriteMapFile = (bool) o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.SpecifyMapStart,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching SpecifyMapStart", GuiProperties.SpecifyMapStart,
                                           o => SpecifyMapStart = (bool) o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.SpecifyMapStop,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching SpecifyMapStop", GuiProperties.SpecifyMapStop,
                                           o => SpecifyMapStop = (bool) o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.WriteClassMapFile,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching WriteClassMapFile", GuiProperties.WriteClassMapFile,
                                           o => WriteClassMapFile = (bool) o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.WriteRstFile,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching WriteRstFile", GuiProperties.WriteRstFile,
                                           o => WriteRstFile = (bool) o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.SpecifyRstStart,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching SpecifyRstStart", GuiProperties.SpecifyRstStart,
                                           o => SpecifyRstStart = (bool) o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.SpecifyRstStop,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching SpecifyRstStop", GuiProperties.SpecifyRstStop,
                                           o => SpecifyRstStop = (bool) o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.WaveModelNr,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching Waves Model Nr", KnownProperties.WaveModelNr, o => WaveModel = o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.Irov,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching Wall behavior type", KnownProperties.Irov, o => WaveModel = o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.SpecifyWaqOutputInterval,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching Waq output interval time", GuiProperties.SpecifyWaqOutputInterval,
                                           o => SpecifyWaqOutputInterval = (bool) o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.SpecifyWaqOutputStartTime,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching Waq output start time", GuiProperties.SpecifyWaqOutputStartTime,
                                           o => SpecifyWaqOutputStartTime = (bool) o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.SpecifyWaqOutputStopTime,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching Waq output end time", GuiProperties.SpecifyWaqOutputStopTime,
                                           o => SpecifyWaqOutputStopTime = (bool) o);
                }
            }
        }

        private void TriggerPropertyChanged(string defaultEditActionName, string propertyName,
                                            Action<object> setPropertyAction)
        {
            BeginEdit(new DefaultEditAction(defaultEditActionName));

            // To trigger a property changed on the WaterFlowFmModel, this self assignment is necessary.
            object propertyValue = ModelDefinition.GetModelProperty(propertyName).Value;
            setPropertyAction(propertyValue);

            EndEdit();
        }

        private void SyncFractionsAndTracers(SourceAndSink sourceAndSink)
        {
            SedimentFractions.ForEach(sf => sourceAndSink.SedimentFractionNames.Add(sf.Name));

            BoundaryConditionSets.ForEach(bcs =>
            {
                bcs.BoundaryConditions.ForEach(bc =>
                {
                    var flowCondition = bc as FlowBoundaryCondition;
                    if (flowCondition != null && flowCondition.FlowQuantity == FlowBoundaryQuantityType.Tracer)
                    {
                        string tracerName = flowCondition.TracerName;
                        if (!sourceAndSink.TracerNames.Contains(tracerName))
                        {
                            sourceAndSink.TracerNames.Add(tracerName);
                        }
                    }
                });
            });
        }

        private void RemoveTracerFromSourcesAndSink(string name)
        {
            if (BoundaryConditions.OfType<FlowBoundaryCondition>().All(bc => bc.TracerName != name))
            {
                SourcesAndSinks.ForEach(ss => ss.TracerNames.Remove(name));
            }
        }

        private void RemoveAllSedimentFractionsFromBoundaryConditionSets()
        {
            foreach (BoundaryConditionSet set in BoundaryConditionSets)
            {
                set.BoundaryConditions.RemoveAllWhere(bc =>
                {
                    var flowCondition = bc as FlowBoundaryCondition;
                    return flowCondition != null &&
                           (flowCondition.FlowQuantity == FlowBoundaryQuantityType.SedimentConcentration
                            || flowCondition.FlowQuantity == FlowBoundaryQuantityType.MorphologyBedLoadTransport);
                });
            }
        }

        private void SyncInitialFractions(ISedimentFraction sedimentFraction)
        {
            foreach (string layerName in sedimentFraction.GetAllActiveSpatiallyVaryingPropertyNames())
            {
                if (InitialFractions.FirstOrDefault(fr => fr.Name.Equals(layerName)) == null)
                {
                    AddToInitialFractions(layerName);
                }
            }
        }

        private void AddSedimentFractionToFlowBoundaryConditionFunction(string name)
        {
            foreach (BoundaryConditionSet set in BoundaryConditionSets)
            {
                foreach (IBoundaryCondition bc in set.BoundaryConditions)
                {
                    var flowCondition = bc as FlowBoundaryCondition;
                    if (flowCondition != null
                        && flowCondition.FlowQuantity == FlowBoundaryQuantityType.MorphologyBedLoadTransport)
                    {
                        foreach (IFunction point in bc.PointData)
                        {
                            flowCondition.AddSedimentFractionToFunction(point, name);
                        }
                    }
                }
            }
        }

        private void RemoveSedimentFractionFromBoundaryConditionSets(string name)
        {
            foreach (BoundaryConditionSet set in BoundaryConditionSets)
            {
                set.BoundaryConditions.RemoveAllWhere(bc =>
                {
                    var flowCondition = bc as FlowBoundaryCondition;

                    if (flowCondition != null &&
                        flowCondition.FlowQuantity == FlowBoundaryQuantityType.SedimentConcentration &&
                        Equals(flowCondition.SedimentFractionName, name))
                    {
                        return true;
                    }

                    return false;
                });

                foreach (IBoundaryCondition bc in set.BoundaryConditions)
                {
                    var flowCondition = bc as FlowBoundaryCondition;
                    if (flowCondition != null
                        && flowCondition.FlowQuantity == FlowBoundaryQuantityType.MorphologyBedLoadTransport)
                    {
                        foreach (IFunction point in bc.PointData)
                        {
                            flowCondition.RemoveSedimentFractionFromFunction(point, name);
                        }
                    }
                }

                set.BoundaryConditions.RemoveAllWhere(bc =>
                {
                    var flowCondition = bc as FlowBoundaryCondition;

                    if (flowCondition != null &&
                        flowCondition.FlowQuantity == FlowBoundaryQuantityType.MorphologyBedLoadTransport
                        && (flowCondition.SedimentFractionNames == null ||
                            flowCondition.SedimentFractionNames.Count == 0))
                    {
                        return true;
                    }

                    return false;
                });
            }
        }

        /// <summary>
        /// Sync properties that are both in the model and the model definition.
        /// </summary>
        [EditAction]
        private void OnModelDefinitionChanged()
        {
            HeatFluxModelType = ModelDefinition.HeatFluxModel.Type;
            WindFields = ModelDefinition.WindFields;
            UnsupportedFileBasedExtForceFileItems = ModelDefinition.UnsupportedFileBasedExtForceFileItems;
        }

        #region Spatial data

        private void SpatialDataLayersChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Equals(sender, InitialSalinity.Coverages))
            {
                AddOrRenameDataItems(InitialSalinity, WaterFlowFMModelDefinition.InitialSalinityDataItemName);
            }
            else
            {
                throw new ArgumentException("Unexpected layered spatial data: " + e.GetRemovedOrAddedItem());
            }
        }

        private void SpatialDataTracersChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Equals(sender, InitialTracers))
            {
                AddOrRenameTracerDataItems();
            }
            else
            {
                throw new ArgumentException("Unexpected layered spatial data: " + e.GetRemovedOrAddedItem());
            }
        }

        private void SpatialDataFractionsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Equals(sender, InitialFractions))
            {
                AddOrRenameFractionDataItems();

                // Invoke property changed, so Gui can update
                InitialCoverageSetChanged = true;
            }
            else
            {
                throw new ArgumentException("Unexpected layered spatial data: " + e.GetRemovedOrAddedItem());
            }
        }

        #endregion

        #region Coupling

        private void HydroAreaCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            object removedOrAddedItem = e.GetRemovedOrAddedItem();
            if (!isLoading)
            {
                var fixedWeir = removedOrAddedItem as FixedWeir;
                if (fixedWeir != null)
                {
                    ModelFeatureCoordinateData<FixedWeir> weirProperties = fixedWeirProperties.ContainsKey(fixedWeir)
                                                                               ? fixedWeirProperties[fixedWeir]
                                                                               : null;

                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            if (weirProperties == null)
                            {
                                fixedWeirProperties.Add(fixedWeir, CreateModelFeatureCoordinateDataFor(fixedWeir));
                            }

                            break;
                        case NotifyCollectionChangedAction.Remove:
                            if (weirProperties == null)
                            {
                                break;
                            }

                            fixedWeirProperties.Remove(weirProperties.Feature);
                            weirProperties.Dispose();

                            break;
                        case NotifyCollectionChangedAction.Replace:
                            if (weirProperties == null)
                            {
                                fixedWeirProperties.Add(fixedWeir, CreateModelFeatureCoordinateDataFor(fixedWeir));
                                break;
                            }

                            weirProperties.Feature = fixedWeir;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                var bridgePillar = removedOrAddedItem as BridgePillar;
                if (bridgePillar != null)
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            BridgePillarsDataModel.Add(
                                CreateModelFeatureCoordinateDataFor(bridgePillar));
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            ModelFeatureCoordinateData<BridgePillar> dataToRemove =
                                BridgePillarsDataModel.FirstOrDefault(
                                    d => d.Feature == bridgePillar);
                            if (dataToRemove == null)
                            {
                                break;
                            }

                            BridgePillarsDataModel.Remove(dataToRemove);
                            dataToRemove.Dispose();
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            ModelFeatureCoordinateData<BridgePillar> dataToUpdate =
                                BridgePillarsDataModel.FirstOrDefault(
                                    d => d.Feature == bridgePillar);
                            if (dataToUpdate == null)
                            {
                                BridgePillarsDataModel.Add(
                                    CreateModelFeatureCoordinateDataFor(bridgePillar));
                                break;
                            }

                            dataToUpdate.Feature = bridgePillar;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            var groupableFeature = removedOrAddedItem as IGroupableFeature;
            if (groupableFeature != null && e.Action != NotifyCollectionChangedAction.Remove && !Area.IsEditing)
            {
                groupableFeature.UpdateGroupName(this);
            }

            bool inputSender = removedOrAddedItem is Pump2D || removedOrAddedItem is Weir2D;
            bool outputSender = removedOrAddedItem is ObservationCrossSection2D ||
                                removedOrAddedItem is GroupableFeature2DPoint;

            if (inputSender || outputSender)
            {
                var feature = (IFeature) removedOrAddedItem;
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        AddAreaItem(feature, inputSender);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        RemoveAreaFeature(feature);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        foreach (KeyValuePair<IFeature, List<IDataItem>> areaDataItem in areaDataItems)
                        {
                            RemoveAreaFeature(areaDataItem.Key);
                        }

                        areaDataItems.Clear();
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        var oldFeature = (IFeature) e.OldItems[0];
                        RemoveAreaFeature(oldFeature);
                        AddAreaItem(feature, inputSender);
                        break;
                    default:
                        throw new NotImplementedException(
                            string.Format("Action {0} on feature collection not supported", e.Action));
                }
            }
        }

        private void HydroAreaPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var weir = sender as IWeir;
            if (weir != null)
            {
                if (e.PropertyName == nameof(Weir.WeirFormula))
                {
                    bool isInputSender = Area.Weirs.Any(w => w.Name == weir.Name);
                    UpdateAreaDataItems(weir, isInputSender);
                }
            }

            var groupableFeature = sender as IGroupableFeature;
            if (updatingGroupName || Area.IsEditing || groupableFeature == null ||
                e.PropertyName != nameof(IGroupableFeature.GroupName))
            {
                return;
            }

            updatingGroupName = true; // prevent recursive calls

            groupableFeature.UpdateGroupName(this);

            if (groupableFeature.IsDefaultGroup)
            {
                groupableFeature.IsDefaultGroup = false;
            }

            updatingGroupName = false;
        }

        private void RemoveAreaFeature(IFeature feature)
        {
            List<IDataItem> dataItemsToBeRemoved;
            if (areaDataItems.TryGetValue(feature, out dataItemsToBeRemoved))
            {
                foreach (IDataItem dataItem in dataItemsToBeRemoved)
                {
                    UnSubscribeFromDataItem(dataItem, true);
                    OnDataItemRemoved(dataItem);
                }
            }

            areaDataItems.Remove(feature);
        }

        private void AddAreaItem(IFeature feature, bool isInputSender)
        {
            List<IDataItem> listToAdd = GetDataItemListForFeature(feature, isInputSender);
            areaDataItems.Add(feature, listToAdd);
        }

        private void UpdateAreaDataItems(IFeature feature, bool isInputSender)
        {
            if (areaDataItems.TryGetValue(feature, out List<IDataItem> dataItemsDependentOnThisFeature))
            {
                List<IDataItem> listToReplace = GetDataItemListForFeature(feature, isInputSender);

                List<IDataItem> dataItemsLinkedToRTC =
                    dataItemsDependentOnThisFeature.Where(di => di.LinkedTo != null).ToList();

                foreach (IDataItem dataItem in dataItemsLinkedToRTC)
                {
                    Log.WarnFormat(
                        Resources
                            .WaterFlowFMModel_ChangingWeirFormulaWhenAlsoUsedInRTC_Structure_component__0__has_been_removed_from_RTC_Control_Group__1__due_to_type_change,
                        dataItem.Name + "_" + dataItem.Tag, dataItem.LinkedTo.Parent.Name);

                    OnDataItemRemoved(dataItem);
                }

                areaDataItems[feature] = listToReplace;
            }
        }

        private List<IDataItem> GetDataItemListForFeature(IFeature feature, bool isInputSender)
        {
            IEnumerable<string> quantities = QuantityGenerator.GetQuantitiesForFeature(feature, UseSalinity);
            return quantities.Select(quantity => new DataItem(feature)
            {
                Name = feature.ToString(),
                Tag = quantity,
                Role = isInputSender ? DataItemRole.Input : DataItemRole.Output,
                ValueType = typeof(double),
                ValueConverter =
                    new WaterFlowFMFeatureValueConverter(this, feature, quantity, string.Empty) // TODO: insert unit
            }).OfType<IDataItem>().ToList();
        }

        #endregion

        #region Overrides of TimeDependentModelBase

        protected override void OnAfterDataItemsSet()
        {
            base.OnAfterDataItemsSet();

            IDataItem areaDataItem = GetDataItemByTag(HydroAreaTag);
            if (areaDataItem != null)
            {
                ((INotifyCollectionChange) areaDataItem.Value).CollectionChanged += HydroAreaCollectionChanged;
                ((INotifyPropertyChanged) areaDataItem.Value).PropertyChanged += HydroAreaPropertyChanged;
            }
        }

        protected override void OnBeforeDataItemsSet()
        {
            base.OnBeforeDataItemsSet();

            areaDataItem = GetDataItemByTag(HydroAreaTag);
            if (areaDataItem != null)
            {
                ((INotifyCollectionChange) areaDataItem.Value).CollectionChanged -= HydroAreaCollectionChanged;
                ((INotifyPropertyChanged) areaDataItem.Value).PropertyChanged -= HydroAreaPropertyChanged;
            }
        }

        protected override void OnDataItemLinked(object sender, LinkedUnlinkedEventArgs<IDataItem> e)
        {
            // subscribe to newly linked hydro area:
            IDataItem areaDataItem = GetDataItemByTag(HydroAreaTag);
            if (Equals(e.Target, areaDataItem) && !e.Relinking)
            {
                ((INotifyCollectionChange) areaDataItem.Value).CollectionChanged += HydroAreaCollectionChanged;
                ((INotifyPropertyChanged) areaDataItem.Value).PropertyChanged += HydroAreaPropertyChanged;
            }

            base.OnDataItemLinked(sender, e);
        }

        protected override void OnDataItemUnlinking(object sender, LinkingUnlinkingEventArgs<IDataItem> e)
        {
            // unsubscribe from area before unlink
            areaDataItem = GetDataItemByTag(HydroAreaTag);
            if (Equals(e.Target, areaDataItem))
            {
                ((INotifyCollectionChange) areaDataItem.Value).CollectionChanged -= HydroAreaCollectionChanged;
                ((INotifyPropertyChanged) areaDataItem.Value).PropertyChanged -= HydroAreaPropertyChanged;
            }

            base.OnDataItemUnlinking(sender, e);
        }

        protected override void OnInputCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {}
        protected override void OnInputPropertyChanged(object sender, PropertyChangedEventArgs e) {}

        /// <summary>
        /// Called when [clear output]. Clears all output of the model.
        /// </summary>
        protected override void OnClearOutput()
        {
            if (OutputMapFileStore != null)
            {
                ClearFunctionStore(OutputMapFileStore);
                OutputMapFileStore = null;
            }

            if (OutputHisFileStore != null)
            {
                ClearFunctionStore(OutputHisFileStore);
                OutputHisFileStore = null;
            }

            if (OutputClassMapFileStore != null)
            {
                ClearFunctionStore(OutputClassMapFileStore);
                OutputClassMapFileStore = null;
            }

            RemoveOutputTextDocumentDataItem();
        }

        private void RemoveOutputTextDocumentDataItem()
        {
            IList<IDataItem> textDocumentDataItems = dataItems.Where(di => di.Role.HasFlag(DataItemRole.Output)
                                                                           && di.ValueType == typeof(TextDocument))
                                                              .ToList();

            foreach (IDataItem dataItem in textDocumentDataItems)
            {
                dataItems.Remove(dataItem);
            }
        }

        #endregion

        #region Syncers

        private readonly IList<IDisposable> syncers = new List<IDisposable>();

        private void InitializeSyncers()
        {
            syncers.Add(new FeatureDataSyncer<Feature2D, BoundaryConditionSet>(
                            Boundaries,
                            BoundaryConditionSets,
                            feature => new BoundaryConditionSet
                            {
                                Feature = feature
                            }));
            syncers.Add(new FeatureDataSyncer<Feature2D, SourceAndSink>(
                            Pipes,
                            SourcesAndSinks,
                            feature => new SourceAndSink
                            {
                                Feature = feature
                            }));
        }

        private void ClearSyncers()
        {
            syncers.ForEach(s => s.Dispose());
            syncers.Clear();
        }

        #endregion
    }
}