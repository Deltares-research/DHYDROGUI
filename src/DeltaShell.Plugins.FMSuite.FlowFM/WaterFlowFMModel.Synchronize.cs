using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public partial class WaterFlowFMModel
    {
        private void OnModelDefinitionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is WaterFlowFMProperty prop) || e.PropertyName != nameof(prop.Value))
            {
                return;
            }

            string mduPropertyName = prop.PropertyDefinition.MduPropertyName;

            bool IsParameter(string s) => string.Equals(mduPropertyName, s, StringComparison.InvariantCultureIgnoreCase);

            if (IsParameter(GuiProperties.InitialConditionGlobalQuantity1D) ||
                IsParameter(GuiProperties.InitialConditionGlobalQuantity2D))
            {
                if (InitialWaterLevel == null)
                {
                    return;
                }

                if (IsParameter(GuiProperties.InitialConditionGlobalQuantity2D))
                {
                    var type = (InitialConditionQuantity)(int)prop.Value;
                    InitialWaterLevel.Name = type == InitialConditionQuantity.WaterLevel
                                                 ? WaterFlowFMModelDefinition.InitialWaterLevelDataItemName
                                                 : WaterFlowFMModelDefinition.InitialWaterDepthDataItemName;
                }

                OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(InitialCoverageSetChanged)));
            }
            if (IsParameter(KnownProperties.FixedWeirScheme))
            {
                allFixedWeirsAndCorrespondingProperties?.ForEach(p => p.UpdateDataColumns(prop.GetValueAsString()));
            }
            if (IsParameter(KnownProperties.BedlevType))
            {
                BeginEdit("Updating Bathymetry coverage");
                UpdateBathymetryCoverage((UGridFileHelper.BedLevelLocation)prop.Value);
                EndEdit();
            }
            else if (IsParameter(KnownProperties.UseSalinity))
            {
                BeginEdit("Switching salinity process");
                OnPropertyChanged(nameof(UseSalinity));
                BoundaryConditions1D?.ForEach(bc => bc.UseSalt = UseSalinity);
                LateralSourcesData?.ForEach(lat => lat.UseSalt = UseSalinity);
                UpdateDataItemsForObservationPoints();
                EndEdit();
            }
            else if (IsParameter(GuiProperties.UseMorSed))
            {
                BeginEdit("Switching morphology process");
                OnPropertyChanged(nameof(UseMorSed));
                EndEdit();
            }
            else if (IsParameter(GuiProperties.WriteSnappedFeatures))
            {
                BeginEdit("Switching write snapped features options");
                OnPropertyChanged(nameof(WriteSnappedFeatures));
                EndEdit();
            }
            else if (IsParameter(KnownProperties.ISlope))
            {
                BeginEdit("Switching Bed slope formulation");
                EndEdit();
            }
            else if (IsParameter(KnownProperties.IHidExp))
            {
                BeginEdit("Switching Hiding and exposure formulation");
                EndEdit();
            }
            else if (IsParameter(KnownProperties.Kmx))
            {
                BeginEdit("Switching 3D dynamics");
                OnPropertyChanged(nameof(UseDepthLayers));
                EndEdit();
            }
            else if (IsParameter(KnownProperties.ICdtyp))
            {
                BeginEdit("Switching wind formulation type");
                OnPropertyChanged(nameof(CdType));
                EndEdit();
            }
            else if (IsParameter(KnownProperties.Temperature))
            {
                BeginEdit("Switching heat flux model");
                HeatFluxModelType = ModelDefinition.HeatFluxModel.Type;
                EndEdit();
                UpdateDataItemsForObservationPoints();
            }
            else if (IsParameter(KnownProperties.SecondaryFlow))
            {
                BeginEdit("Switching secondary flow process");
                OnPropertyChanged(nameof(UseSecondaryFlow));
                EndEdit();
            }
            else if (IsParameter(GuiProperties.WriteHisFile))
            {
                BeginEdit("Switching WriteHisFile");
                OnPropertyChanged(nameof(WriteHisFile));
                EndEdit();
            }
            else if (IsParameter(GuiProperties.SpecifyHisStart))
            {
                BeginEdit("Switching SpecifyHisStart");
                OnPropertyChanged(nameof(SpecifyHisStart));
                EndEdit();
            }
            else if (IsParameter(GuiProperties.SpecifyHisStop))
            {
                BeginEdit("Switching SpecifyHisStop");
                OnPropertyChanged(nameof(SpecifyHisStop));
                EndEdit();
            }
            else if (IsParameter(GuiProperties.WriteMapFile))
            {
                BeginEdit("Switching WriteMapFile");
                OnPropertyChanged(nameof(WriteMapFile));
                EndEdit();
            }
            else if (IsParameter(GuiProperties.SpecifyMapStart))
            {
                BeginEdit("Switching SpecifyMapStart");
                OnPropertyChanged(nameof(SpecifyMapStart));
                EndEdit();
            }
            else if (IsParameter(GuiProperties.SpecifyMapStop))
            {
                BeginEdit("Switching SpecifyMapStop");
                OnPropertyChanged(nameof(SpecifyMapStop));
                EndEdit();
            }
            else if (IsParameter(GuiProperties.WriteRstFile))
            {
                BeginEdit("Switching WriteRstFile");
                OnPropertyChanged(nameof(WriteRstFile));
                EndEdit();
            }
            else if (IsParameter(GuiProperties.SpecifyRstStart))
            {
                BeginEdit("Switching SpecifyRstStart");
                OnPropertyChanged(nameof(SpecifyRstStart));
                EndEdit();
            }
            else if (IsParameter(GuiProperties.SpecifyRstStop))
            {
                BeginEdit("Switching SpecifyRstStop");
                OnPropertyChanged(nameof(SpecifyRstStop));
                EndEdit();
            }
            else if (IsParameter(KnownProperties.WaveModelNr))
            {
                BeginEdit("Switching Waves Model Nr");
                OnPropertyChanged(nameof(WaveModel));
                EndEdit();
            }
            else if (IsParameter(KnownProperties.Irov))
            {
                BeginEdit("Switching Wall behavior type");
                OnPropertyChanged(nameof(WaveModel));
                EndEdit();
            }
            else if (IsParameter(GuiProperties.SpecifyWaqOutputInterval))
            {
                BeginEdit("Switching Waq output interval time");
                OnPropertyChanged(nameof(SpecifyWaqOutputInterval));
                EndEdit();
            }
            else if (IsParameter(GuiProperties.SpecifyWaqOutputStartTime))
            {
                BeginEdit("Switching Waq output start time");
                OnPropertyChanged(nameof(SpecifyWaqOutputStartTime));
                EndEdit();
            }
            else if (IsParameter(GuiProperties.SpecifyWaqOutputStopTime))
            {
                BeginEdit("Switching Waq output end time");
                OnPropertyChanged(nameof(SpecifyWaqOutputStopTime));
                EndEdit();
            }
            else if (IsParameter(GuiProperties.WriteClassMapFile))
            {
                BeginEdit("Switching WriteClassMapFile");
                OnPropertyChanged(nameof(WriteClassMapFile));
                EndEdit();
            }
            else if (IsParameter(GuiProperties.StartTime))
            {
                BeginEdit("Changing start time");
                base.StartTime = StartTime;
                OnPropertyChanged(nameof(StartTime));
                EndEdit();
            }
            else if (IsParameter(GuiProperties.StopTime))
            {
                BeginEdit("Changing stop time");
                base.StopTime = StopTime;
                OnPropertyChanged(nameof(StopTime));
                EndEdit();
            }
            else if (IsParameter(KnownProperties.DtUser))
            {
                BeginEdit("Changing timestep");
                base.TimeStep = TimeStep;
                OnPropertyChanged(nameof(TimeStep));
                EndEdit();
            }
            else
            {
                BeginEdit("");
                EndEdit();
            }
        }
        
        private void UpdateDataItemsForObservationPoints()
        {
            GroupableFeature2DPoint[] observationPointsToUpdate = 
                areaDataItems.Keys.OfType<GroupableFeature2DPoint>().ToArray();

            foreach (IFeature feature in observationPointsToUpdate)
            {
                RemoveAreaFeature(feature);
                AddAreaItem(feature);
            }
        }

        private void OnFMModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            MarkDirty();
            
            if (!(sender is WaterFlowFMModel)) return;
            
            if (e.PropertyName == nameof(Name) && fmRegion.Name != Name)
            {
                fmRegion.Name = Name;
                if (!OutputIsEmpty)
                {
                    Log.WarnFormat(Resources.WaterFlowFMModel__0__has_changed__clearing_results_, Name);
                    OnClearOutput();
                }
            }
        }
        
        private void OnFMModelCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            MarkDirty();
        }

        protected override void OnInputCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
        }

        protected override void OnInputPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // [TOOLS-22813] Override OnInputPropertyChanged to stop base class (ModelBase) from clearing the output
        }

        private void SedimentFractionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var sedimentFraction = e.GetRemovedOrAddedItem() as ISedimentFraction;
            if (sedimentFraction == null)
                return;
            var name = sedimentFraction.Name;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    sedimentFraction.UpdateSpatiallyVaryingNames();
                    sedimentFraction.CompileAndSetVisibilityAndIfEnabled();
                    sedimentFraction.SetTransportFormulaInCurrentSedimentType();
                    SourcesAndSinks.ForEach(ss => ss.SedimentFractionNames.Add(sedimentFraction.Name));

                    if (InitialFractions == null || BoundaryConditionSets == null) break;

                    // sync the initial fractions
                    SyncInitialFractions(sedimentFraction);
                    AddSedimentFractionToFlowBoundaryConditionFunction(name);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    // sync the initial fractions
                    var layersToRemove = sedimentFraction.GetAllActiveSpatiallyVaryingPropertyNames();
                    InitialFractions.RemoveAllWhere(ifs => layersToRemove.Contains(ifs.Name));

                    // Remove dataItems for coverages related to Removed Fraction
                    DataItems.RemoveAllWhere(di => di.Value is UnstructuredGridCoverage && layersToRemove.Contains(di.Name));
                    RemoveSedimentFractionFromBoundaryConditionSets(name);

                    SourcesAndSinks.ForEach(ss => ss.SedimentFractionNames.Remove(sedimentFraction.Name));
                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException("Renaming of sediment fraction is not yet supported");
                case NotifyCollectionChangedAction.Reset:
                    // sync the initial fractions
                    InitialFractions.Clear();

                    RemoveAllSedimentFractionsFromBoundaryConditionSets();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SedimentFractionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Name":
                    {
                        if (sender is ISedimentFraction sedimentFraction)
                        {
                            sedimentFraction.UpdateSpatiallyVaryingNames();
                        }

                        break;
                    }
                case "CurrentFormulaType":
                case "CurrentSedimentType":
                    {
                        if (!(sender is ISedimentFraction sedimentFraction))
                            return;

                        sedimentFraction.UpdateSpatiallyVaryingNames();
                        var activeSpatiallyVarying = sedimentFraction.GetAllActiveSpatiallyVaryingPropertyNames();
                        var spatiallyVarying = sedimentFraction.GetAllSpatiallyVaryingPropertyNames();
                        InitialFractions.RemoveAllWhere(
                            fr => spatiallyVarying.Contains(fr.Name) && !activeSpatiallyVarying.Contains(fr.Name));

                        foreach (var layerName in activeSpatiallyVarying)
                        {
                            AddToIntialFractions(layerName);
                        }

                        sedimentFraction.CompileAndSetVisibilityAndIfEnabled();

                        if (e.PropertyName == "CurrentFormulaType")
                        {
                            sedimentFraction.SetTransportFormulaInCurrentSedimentType();
                        }
                        return;
                    }
                case "IsSpatiallyVarying":
                    {
                        if (!(sender is ISpatiallyVaryingSedimentProperty prop)) return;

                        if (prop.IsSpatiallyVarying)
                        {
                            AddToIntialFractions(prop.SpatiallyVaryingName);
                        }
                        else
                        {
                            InitialFractions.RemoveAllWhere(tr => tr.Name.Equals(prop.SpatiallyVaryingName));
                        }

                        break;
                    }
            }
        }

        private void SourcesAndSinksCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var sourceAndSink = e.GetRemovedOrAddedItem() as SourceAndSink;

            if (sourceAndSink == null)
                return;

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                SyncFractionsAndTracers(sourceAndSink);
            }

            var feature = (IFeature)sourceAndSink.Feature;
            if (feature != null && HasValidDataItemRole(feature))
            {
                HandleFeatureCollectionChanged(e, feature);
            }
        }

        private void HandleFeatureCollectionChanged(NotifyCollectionChangedEventArgs e, IFeature feature)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddAreaItem(feature);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveAreaFeature(feature);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var areaAsDataItem in areaDataItems)
                    {
                        RemoveAreaFeature(areaAsDataItem.Key);
                    }
                    areaDataItems.Clear();
                    break;
                case NotifyCollectionChangedAction.Replace:
                    var oldFeature = e.OldItems?.OfType<IFeature>().FirstOrDefault();
                    RemoveAreaFeature(oldFeature);
                    AddAreaItem(feature);
                    break;
                default:
                    throw new NotImplementedException(
                        String.Format("Action {0} on feature collection not supported", e.Action));
            }
        }

        private bool HasValidDataItemRole(IFeature feature)
        {
            return GetDataItemRole(feature) != DataItemRole.None;
        }

        private void BoundaryConditionSetsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var tracerBoundaryConditions = Enumerable.Empty<FlowBoundaryCondition>();

            var boundaryConditionSet = e.GetRemovedOrAddedItem() as BoundaryConditionSet;
            if (boundaryConditionSet == null)
            {
                var flowBoundaryCondition = e.GetRemovedOrAddedItem() as FlowBoundaryCondition;
                if (flowBoundaryCondition != null && flowBoundaryCondition.FlowQuantity == FlowBoundaryQuantityType.Tracer)
                {
                    tracerBoundaryConditions = new List<FlowBoundaryCondition>() { flowBoundaryCondition };
                }
            }
            else
            {
                tracerBoundaryConditions = boundaryConditionSet.BoundaryConditions
                    .OfType<FlowBoundaryCondition>()
                    .Where(fbc => fbc.FlowQuantity == FlowBoundaryQuantityType.Tracer);
            }

            foreach (var tracerBoundaryCondition in tracerBoundaryConditions)
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
                        throw new NotSupportedException("Renaming of Tracers is not yet supported");

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
            var name = (string)e.GetRemovedOrAddedItem();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    // sync the initial tracers
                    InitialTracers.Add(CreateUnstructuredGridCellCoverage(name, Grid));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    // sync the initial tracers
                    InitialTracers.RemoveAllWhere(tr => tr.Name == name);

                    // remove all boundary conditions with that tracer name
                    foreach (var set in BoundaryConditionSets)
                    {
                        set.BoundaryConditions.RemoveAllWhere(bc =>
                        {
                            var flowCondition = bc as FlowBoundaryCondition;

                            if (flowCondition != null &&
                                flowCondition.FlowQuantity == FlowBoundaryQuantityType.Tracer &&
                                Equals(flowCondition.TracerName, e.GetRemovedOrAddedItem()))
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
                case NotifyCollectionChangedAction.Reset:
                    // sync the initial tracers
                    InitialTracers.Clear();

                    // remove all tracer boundary conditions
                    foreach (var set in BoundaryConditionSets)
                    {
                        set.BoundaryConditions.RemoveAllWhere(bc =>
                        {
                            var flowCondition = bc as FlowBoundaryCondition;
                            return flowCondition != null && flowCondition.FlowQuantity == FlowBoundaryQuantityType.Tracer;
                        });
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnWaterFlowFm1D2DLinkPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is Link1D2D) || !e.PropertyName.Equals("Geometry"))
                return;

            //update indexes
            var link = (Link1D2D) sender;
            var firstCoordinate = link.Geometry?.Coordinates.First();
            var lastCoordinate = link.Geometry?.Coordinates.Last();
            link.DiscretisationPointIndex = Links1D2DHelper.FindCalculationPointIndex(firstCoordinate, NetworkDiscretization, link.SnapToleranceUsed);
            link.FaceIndex = Links1D2DHelper.FindCellIndex(lastCoordinate, Grid);
        }

        private void HydroAreaCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            object removedOrAddedItem = e.GetRemovedOrAddedItem();
            
            if (!isLoading)
            {
                var fixedWeir = removedOrAddedItem as FixedWeir;
                if (fixedWeir != null)
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            if (allFixedWeirsAndCorrespondingProperties.FirstOrDefault(d => d.Feature == fixedWeir) == null)
                            {
                                allFixedWeirsAndCorrespondingProperties.Add(
                                    CreateModelFeatureCoordinateDataFor(fixedWeir));
                            }

                            break;
                        case NotifyCollectionChangedAction.Remove:
                            var dataToRemove =
                                allFixedWeirsAndCorrespondingProperties.FirstOrDefault(d => d.Feature == fixedWeir);
                            if (dataToRemove == null) break;

                            allFixedWeirsAndCorrespondingProperties.Remove(dataToRemove);
                            dataToRemove.Dispose();
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            var dataToUpdate =
                                allFixedWeirsAndCorrespondingProperties.FirstOrDefault(d => d.Feature == fixedWeir);
                            if (dataToUpdate == null)
                            {
                                allFixedWeirsAndCorrespondingProperties.Add(
                                    CreateModelFeatureCoordinateDataFor(fixedWeir));
                                break;
                            }

                            dataToUpdate.Feature = fixedWeir;
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
                            var dataToRemove =
                                BridgePillarsDataModel.FirstOrDefault(
                                    d => d.Feature == bridgePillar);
                            if (dataToRemove == null) break;

                            BridgePillarsDataModel.Remove(dataToRemove);
                            dataToRemove.Dispose();
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            var dataToUpdate =
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

            if (removedOrAddedItem is ILeveeBreach leveeBreach)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        CreatePointFeatureOfThisLeveeBreach(
                            leveeBreach,
                            LeveeBreachPointLocationType.BreachLocation,
                            leveeBreach.BreachLocation);
                        if (leveeBreach.WaterLevelFlowLocationsActive)
                        {
                            CreatePointFeatureOfThisLeveeBreach(
                                leveeBreach,
                                LeveeBreachPointLocationType.WaterLevelUpstreamLocation,
                                leveeBreach.WaterLevelUpstreamLocation);
                            CreatePointFeatureOfThisLeveeBreach(
                                leveeBreach,
                                LeveeBreachPointLocationType.WaterLevelDownstreamLocation,
                                leveeBreach.WaterLevelDownstreamLocation);
                        }

                        break;
                    case NotifyCollectionChangedAction.Remove:
                        var supportPoint2DFeatures = ((IEventedList<Feature2D>)Area.LeveeBreaches)
                            .Where(f2d => f2d.Attributes != null &&
                                          f2d.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_FEATURE) &&
                                          ((ILeveeBreach)f2d.Attributes[LeveeBreach.LEVEE_BREACH_FEATURE])
                                          .Equals(leveeBreach)).ToList();
                        foreach (var supportPoint2DFeature in supportPoint2DFeatures)
                        {
                            supportPoint2DFeature.PropertyChanged -= Feature2DPointOnPropertyChanged;
                            Area.LeveeBreaches.Remove(supportPoint2DFeature);
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:

                        if (e.OldItems is IEventedList<Feature2D> oldFeature2Ds)
                        {
                            foreach (var f2d in oldFeature2Ds)
                            {
                                if (f2d.Attributes != null &&
                                    f2d.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_FEATURE))
                                {
                                    f2d.PropertyChanged -= Feature2DPointOnPropertyChanged;
                                }
                            }
                        }


                        if (e.NewItems is IEventedList<Feature2D> newFeature2Ds)
                        {
                            foreach (var f2d in newFeature2Ds)
                            {
                                if (f2d.Attributes != null &&
                                    f2d.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_FEATURE))
                                {
                                    f2d.PropertyChanged += Feature2DPointOnPropertyChanged;
                                }
                            }

                            foreach (var leveeFeature in newFeature2Ds.OfType<ILeveeBreach>())
                            {
                                CreatePointFeatureOfThisLeveeBreach(
                                    leveeFeature,
                                    LeveeBreachPointLocationType.BreachLocation,
                                    leveeFeature.BreachLocation);
                                if (leveeFeature.WaterLevelFlowLocationsActive)
                                {
                                    CreatePointFeatureOfThisLeveeBreach(
                                        leveeFeature,
                                        LeveeBreachPointLocationType.WaterLevelUpstreamLocation,
                                        leveeFeature.WaterLevelUpstreamLocation);
                                    CreatePointFeatureOfThisLeveeBreach(
                                        leveeFeature,
                                        LeveeBreachPointLocationType.WaterLevelDownstreamLocation,
                                        leveeFeature.WaterLevelDownstreamLocation);
                                }
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        if (e.OldItems is IEventedList<Feature2D> oldLeveeFeature2Ds)
                        {
                            foreach (var f2d in oldLeveeFeature2Ds)
                            {
                                if (f2d.Attributes != null &&
                                    f2d.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_FEATURE))
                                {
                                    f2d.PropertyChanged -= Feature2DPointOnPropertyChanged;
                                }
                            }
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var groupableFeature = removedOrAddedItem as IGroupableFeature;
            if (groupableFeature != null && e.Action != NotifyCollectionChangedAction.Remove && !Area.IsEditing)
            {
                groupableFeature.UpdateGroupName(this);
            }

            if (removedOrAddedItem is IFeature feature && HasValidDataItemRole(feature))
            {
                HandleFeatureCollectionChanged(e, feature);
            }
        }

        private void HydroAreaPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (sender)
            {
                case IWeir weir when e.PropertyName == nameof(weir.WeirFormula):
                    {
                        UpdateAreaDataItems(weir);
                        break;
                    }
                case ILeveeBreach leveeBreach when e.PropertyName == nameof(leveeBreach.WaterLevelFlowLocationsActive):
                    {
                        if (leveeBreach.WaterLevelFlowLocationsActive)
                        {
                            CreatePointFeatureOfThisLeveeBreach(
                                leveeBreach,
                                LeveeBreachPointLocationType.WaterLevelUpstreamLocation,
                                leveeBreach.WaterLevelUpstreamLocation);
                            CreatePointFeatureOfThisLeveeBreach(
                                leveeBreach,
                                LeveeBreachPointLocationType.WaterLevelDownstreamLocation,
                                leveeBreach.WaterLevelDownstreamLocation);
                        }
                        else
                        {
                            var supportPoint2DFeatures = Area.LeveeBreaches
                                .Where(f2d => f2d.Attributes != null &&
                                              f2d.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_FEATURE) &&
                                              ((ILeveeBreach)f2d.Attributes[LeveeBreach.LEVEE_BREACH_FEATURE])
                                              .Equals(leveeBreach)).ToList();
                            foreach (var supportPoint2DFeature in supportPoint2DFeatures)
                            {
                                if (supportPoint2DFeature.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_POINT_LOCATION_TYPE) &&
                                    (LeveeBreachPointLocationType)supportPoint2DFeature.Attributes[LeveeBreach.LEVEE_BREACH_POINT_LOCATION_TYPE] == LeveeBreachPointLocationType.BreachLocation)
                                    continue;

                                supportPoint2DFeature.PropertyChanged -= Feature2DPointOnPropertyChanged;
                                Area.LeveeBreaches.Remove(supportPoint2DFeature);
                            }
                        }

                        break;
                    }
            }

            if (updatingGroupName || Area.IsEditing || !(sender is IGroupableFeature groupableFeature) ||
                e.PropertyName != nameof(groupableFeature.GroupName)) return;

            updatingGroupName = true;// prevent recursive calls

            groupableFeature.UpdateGroupName(this);

            if (groupableFeature.IsDefaultGroup)
            {
                groupableFeature.IsDefaultGroup = false;
            }

            updatingGroupName = false;
        }

        private void Feature2DPointOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is Feature2DPoint feature2DPoint &&
                feature2DPoint.Attributes != null &&
                feature2DPoint.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_FEATURE) &&
                feature2DPoint.Attributes[LeveeBreach.LEVEE_BREACH_FEATURE] is LeveeBreach leveeBreach &&
                feature2DPoint.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_POINT_LOCATION_TYPE))
            {
                var type = (LeveeBreachPointLocationType)feature2DPoint.Attributes[LeveeBreach.LEVEE_BREACH_POINT_LOCATION_TYPE];
                const double tolerance = 1e-5d;
                var locationX = feature2DPoint.X;
                var locationY = feature2DPoint.Y;
                switch (type)
                {
                    case LeveeBreachPointLocationType.BreachLocation:
                        if (Math.Abs(leveeBreach.BreachLocationX - locationX) > tolerance)
                            leveeBreach.BreachLocationX = locationX;
                        if (Math.Abs(leveeBreach.BreachLocationY - locationY) > tolerance)
                            leveeBreach.BreachLocationY = locationY;
                        break;
                    case LeveeBreachPointLocationType.WaterLevelUpstreamLocation:
                        if (Math.Abs(leveeBreach.WaterLevelUpstreamLocationX - locationX) > tolerance)
                            leveeBreach.WaterLevelUpstreamLocationX = locationX;
                        if (Math.Abs(leveeBreach.WaterLevelUpstreamLocationY - locationY) > tolerance)
                            leveeBreach.WaterLevelUpstreamLocationY = locationY;
                        break;
                    case LeveeBreachPointLocationType.WaterLevelDownstreamLocation:

                        if (Math.Abs(leveeBreach.WaterLevelDownstreamLocationX - locationX) > tolerance)
                            leveeBreach.WaterLevelDownstreamLocationX = locationX;
                        if (Math.Abs(leveeBreach.WaterLevelDownstreamLocationY - locationY) > tolerance)
                            leveeBreach.WaterLevelDownstreamLocationY = locationY;
                        break;
                }
            }
        }

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

        protected override void OnDataItemLinked(object sender, LinkedUnlinkedEventArgs<IDataItem> e)
        {
            // subscribe to newly linked hydro area:
            var areaDataItem = GetDataItemByTag(WaterFlowFMModelDataSet.HydroAreaTag);
            if (Equals(e.Target, areaDataItem) && !e.Relinking)
            {
                ((INotifyCollectionChange)areaDataItem.Value).CollectionChanged += HydroAreaCollectionChanged;
                ((INotifyPropertyChanged)areaDataItem.Value).PropertyChanged += HydroAreaPropertyChanged;
            }

            var networkDataItem = GetDataItemByTag(WaterFlowFMModelDataSet.NetworkTag);
            if (Equals(e.Target, networkDataItem) && !e.Relinking)
            {
                SubscribeToNetwork(networkDataItem.Value as IHydroNetwork);
            }

            base.OnDataItemLinked(sender, e);
        }

        protected override void OnDataItemUnlinking(object sender, LinkingUnlinkingEventArgs<IDataItem> e)
        {
            // unsubscribe from area before unlink
            areaDataItem = GetDataItemByTag(WaterFlowFMModelDataSet.HydroAreaTag);
            if (Equals(e.Target, areaDataItem))
            {
                ((INotifyCollectionChange)areaDataItem.Value).CollectionChanged -= HydroAreaCollectionChanged;
                ((INotifyPropertyChanged)areaDataItem.Value).PropertyChanged -= HydroAreaPropertyChanged;
            }

            var networkDataItem = GetDataItemByTag(WaterFlowFMModelDataSet.NetworkTag);
            if (Equals(e.Target, networkDataItem))
            {
                UnSubscribeFromNetwork(networkDataItem.Value as IHydroNetwork);
            }

            base.OnDataItemUnlinking(sender, e);
        }
    }
}
