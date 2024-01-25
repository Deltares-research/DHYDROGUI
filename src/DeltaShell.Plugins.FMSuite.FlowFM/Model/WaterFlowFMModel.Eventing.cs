using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
using GeoAPI.Extensions.Feature;
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

        private void TracerDefinitionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var name = (string)e.GetRemovedOrAddedItem();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    OnTracerAdded(name);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    OnTracerRemoved(name);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotSupportedException("Renaming of tracer definitions is not yet supported");
                case NotifyCollectionChangedAction.Reset:
                    throw new NotSupportedException($"{nameof(EventedList<string>)} does not support ${NotifyCollectionChangedAction.Reset}");
                default:
                    throw new ArgumentOutOfRangeException(nameof(e));
            }
        }

        private void OnTracerAdded(string name)
        {
            SpatialData.AddTracer(UnstructuredGridCoverageFactory.CreateCellCoverage(name, Grid, defaultValue: 0d));
            foreach (SourceAndSink sourceAndSink in SourcesAndSinks)
            {
                sourceAndSink.Function.AddTracer(name);
            }
        }

        private void OnTracerRemoved(string name)
        {
            SpatialData.RemoveTracer(name);

            foreach (BoundaryConditionSet set in BoundaryConditionSets)
            {
                set.BoundaryConditions.RemoveAllWhere(bc => bc is FlowBoundaryCondition flowCondition &&
                                                            flowCondition.FlowQuantity == FlowBoundaryQuantityType.Tracer &&
                                                            Equals(flowCondition.TracerName, name));
            }

            foreach (SourceAndSink sourceAndSink in SourcesAndSinks)
            {
                sourceAndSink.Function.RemoveTracer(name);
            }
        }

        private void SedimentFractionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (sender)
            {
                case ISedimentFraction fraction:
                    OnSedimentFractionChanged(fraction, e.PropertyName);
                    break;
                case ISpatiallyVaryingSedimentProperty spatiallyVaryingSedimentProperty:
                    OnSpatiallyVaryingSedimentPropertyChanged(spatiallyVaryingSedimentProperty,
                                                              e.PropertyName);
                    break;
            }
        }

        private void OnSedimentFractionChanged(ISedimentFraction fraction,
                                               string propertyName)
        {
            switch (propertyName)
            {
                case nameof(ISedimentFraction.Name):
                    fraction.UpdateSpatiallyVaryingNames();
                    break;
                case nameof(ISedimentFraction.CurrentFormulaType):
                case nameof(ISedimentFraction.CurrentSedimentType):
                    SynchronizeSedimentFractionData(fraction);
                    break;
            }
        }

        private void SynchronizeSedimentFractionData(ISedimentFraction fraction)
        {
            fraction.UpdateSpatiallyVaryingNames();

            List<string> activeSpatiallyVarying = fraction.GetAllActiveSpatiallyVaryingPropertyNames();
            List<string> spatiallyVarying = fraction.GetAllSpatiallyVaryingPropertyNames();

            foreach (string name in spatiallyVarying.Except(activeSpatiallyVarying))
            {
                SpatialData.RemoveFraction(name);
            }

            foreach (string layerName in activeSpatiallyVarying)
            {
                AddToInitialFractions(layerName);
            }

            fraction.CompileAndSetVisibilityAndIfEnabled();
            fraction.SetTransportFormulaInCurrentSedimentType();
        }

        private void OnSpatiallyVaryingSedimentPropertyChanged(ISpatiallyVaryingSedimentProperty prop,
                                                               string propertyName)
        {
            if (propertyName != nameof(ISpatiallyVaryingSedimentProperty.IsSpatiallyVarying))
            {
                return;
            }

            if (prop.IsSpatiallyVarying)
            {
                AddToInitialFractions(prop.SpatiallyVaryingName);
            }
            else
            {
                SpatialData.RemoveFraction(prop.SpatiallyVaryingName);
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
                    SourcesAndSinks.ForEach(ss => ss.Function.AddSedimentFraction(sedimentFraction.Name));

                    if (BoundaryConditionSets == null)
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
                    layersToRemove.ForEach(SpatialData.RemoveFraction);
                    RemoveSedimentFractionFromBoundaryConditionSets(name);

                    SourcesAndSinks.ForEach(ss => ss.Function.RemoveSedimentFraction(sedimentFraction.Name));
                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException("Renaming of sediment fraction is not yet supported");
                case NotifyCollectionChangedAction.Reset:
                    throw new NotSupportedException($"{nameof(EventedList<string>)} does not support ${NotifyCollectionChangedAction.Reset}");
                default:
                    throw new ArgumentOutOfRangeException(nameof(e));
            }
        }

        private void OnModelDefinitionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var prop = (WaterFlowFMProperty)sender;
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
                    var bedLevelType = (UnstructuredGridFileHelper.BedLevelLocation)prop.Value;
                    BeginEdit("Updating Bathymetry coverage");
                    UpdateBathymetryCoverage(bedLevelType);
                    EndEdit();
                }

                if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.UseSalinity,
                                                                   StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching salinity process", KnownProperties.UseSalinity,
                                           o => UseSalinity = (bool)o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.UseMorSed,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching morphology process", GuiProperties.UseMorSed,
                                           o => UseMorSed = (bool)o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.WriteSnappedFeatures,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching write snapped features options",
                                           GuiProperties.WriteSnappedFeatures, o => WriteSnappedFeatures = (bool)o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.ISlope,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit("Switching Bed slope formulation");
                    EndEdit();
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.IHidExp,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit("Switching Hiding and exposure formulation");
                    EndEdit();
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.Kmx,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching 3D dynamics", KnownProperties.Kmx,
                                           o => UseDepthLayers = (int)o != 0);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.ICdtyp,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching wind formulation type", KnownProperties.ICdtyp,
                                           o => CdType = (int)o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.Temperature,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit("Switching heat flux model");
                    HeatFluxModelType = ModelDefinition.HeatFluxModel.Type;
                    EndEdit();
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.SecondaryFlow,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching secondary flow process", KnownProperties.SecondaryFlow,
                                           o => UseSecondaryFlow = (bool)o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.WriteHisFile,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching WriteHisFile", GuiProperties.WriteHisFile,
                                           o => WriteHisFile = (bool)o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.SpecifyHisStart,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching SpecifyHisStart", GuiProperties.SpecifyHisStart,
                                           o => SpecifyHisStart = (bool)o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.SpecifyHisStop,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching SpecifyHisStop", GuiProperties.SpecifyHisStop,
                                           o => SpecifyHisStop = (bool)o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.WriteMapFile,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching WriteMapFile", GuiProperties.WriteMapFile,
                                           o => WriteMapFile = (bool)o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.SpecifyMapStart,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching SpecifyMapStart", GuiProperties.SpecifyMapStart,
                                           o => SpecifyMapStart = (bool)o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.SpecifyMapStop,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching SpecifyMapStop", GuiProperties.SpecifyMapStop,
                                           o => SpecifyMapStop = (bool)o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.WriteClassMapFile,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching WriteClassMapFile", GuiProperties.WriteClassMapFile,
                                           o => WriteClassMapFile = (bool)o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.WriteRstFile,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching WriteRstFile", GuiProperties.WriteRstFile,
                                           o => WriteRstFile = (bool)o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.SpecifyRstStart,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching SpecifyRstStart", GuiProperties.SpecifyRstStart,
                                           o => SpecifyRstStart = (bool)o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.SpecifyRstStop,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching SpecifyRstStop", GuiProperties.SpecifyRstStop,
                                           o => SpecifyRstStop = (bool)o);
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
                                           o => SpecifyWaqOutputInterval = (bool)o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.SpecifyWaqOutputStartTime,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching Waq output start time", GuiProperties.SpecifyWaqOutputStartTime,
                                           o => SpecifyWaqOutputStartTime = (bool)o);
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.SpecifyWaqOutputStopTime,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    TriggerPropertyChanged("Switching Waq output end time", GuiProperties.SpecifyWaqOutputStopTime,
                                           o => SpecifyWaqOutputStopTime = (bool)o);
                }
                else if (PropertyIsDataAccessObject(prop))
                {
                    return;
                }

                BeginEdit("");
                MarkOutputOutOfSync();
                EndEdit();
            }
        }

        private static bool PropertyIsDataAccessObject(WaterFlowFMProperty prop) =>
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.RestartFile,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.RestartDateTime,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.LandBoundaryFile,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.ThinDamFile,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.FixedWeirFile,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.BridgePillarFile,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.ObsFile,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.ObsCrsFile,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.StructuresFile,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.EnclosureFile,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.DryPointsFile,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.PathsRelativeToParent,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.OutputDir,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.WaqOutputDir,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.ExtForceFile,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.BndExtForceFile,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.MorFile,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.SedFile,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.HisInterval,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.MapInterval,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.RstInterval,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.WaqInterval,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.ClassMapInterval,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.Version,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.GuiVersion,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.NetFile,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.StructuresFile,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.IniFieldFile,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.PartitionFile,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.ManholeFile,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.ProfdefFile,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.ProflocFile,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.WaterLevIniFile,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.TrtRou,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.TrtDef,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.TrtL,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.MapFile__Obsolete,
                                                           StringComparison.InvariantCultureIgnoreCase) ||
            prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.HisFile__Obsolete,
                                                           StringComparison.InvariantCultureIgnoreCase);

        private void TriggerPropertyChanged(string defaultEditActionName, string propertyName,
                                            Action<object> setPropertyAction)
        {
            BeginEdit(defaultEditActionName);

            // To trigger a property changed on the WaterFlowFmModel, this self assignment is necessary.
            object propertyValue = ModelDefinition.GetModelProperty(propertyName).Value;
            setPropertyAction(propertyValue);

            EndEdit();
        }

        private void SyncFractionsAndTracers(SourceAndSink sourceAndSink)
        {
            foreach (ISedimentFraction sedimentFraction in SedimentFractions)
            {
                sourceAndSink.Function.AddSedimentFraction(sedimentFraction.Name);
            }

            foreach (string tracer in TracerDefinitions)
            {
                sourceAndSink.Function.AddTracer(tracer);
            }
        }

        private void SyncInitialFractions(ISedimentFraction sedimentFraction)
        {
            foreach (string layerName in sedimentFraction.GetAllActiveSpatiallyVaryingPropertyNames())
            {
                AddToInitialFractions(layerName);
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
        private void OnModelDefinitionChanged()
        {
            HeatFluxModelType = ModelDefinition.HeatFluxModel.Type;
            WindFields = ModelDefinition.WindFields;
            UnsupportedFileBasedExtForceFileItems = ModelDefinition.UnsupportedFileBasedExtForceFileItems;
        }

        #region Coupling

        private void HydroAreaCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            MarkOutputOutOfSync();

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
                            throw new ArgumentOutOfRangeException(nameof(e));
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
                            throw new ArgumentOutOfRangeException(nameof(e));
                    }
                }
            }

            if (removedOrAddedItem is IGroupableFeature groupableFeature && e.Action != NotifyCollectionChangedAction.Remove && !Area.IsEditing)
            {
                groupableFeature.UpdateGroupName(this);
            }

            bool inputSender = removedOrAddedItem is IPump ||
                               removedOrAddedItem is IStructure;
            bool outputSender = removedOrAddedItem is ObservationCrossSection2D ||
                                removedOrAddedItem is GroupableFeature2DPoint;

            if (inputSender || outputSender)
            {
                var feature = (IFeature)removedOrAddedItem;
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
                        var oldFeature = (IFeature)e.OldItems[0];
                        RemoveAreaFeature(oldFeature);
                        AddAreaItem(feature, inputSender);
                        break;
                    default:
                        throw new NotImplementedException($"Action {e.Action} on feature collection not supported");
                }
            }
        }

        private void HydroAreaPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is IStructure weir && e.PropertyName == nameof(IStructure.Formula))
            {
                bool isInputSender = Area.Structures.Any(w => w.Name == weir.Name);
                UpdateAreaDataItems(weir, isInputSender);
            }

            if (updatingGroupName || Area.IsEditing)
            {
                return;
            }

            if (!(sender is IGroupableFeature groupableFeature))
            {
                MarkOutputOutOfSync();
                return;
            }

            if (e.PropertyName == nameof(IGroupableFeature.GroupName))
            {
                updatingGroupName = true; // prevent recursive calls

                groupableFeature.UpdateGroupName(this);

                if (groupableFeature.IsDefaultGroup)
                {
                    groupableFeature.IsDefaultGroup = false;
                }

                updatingGroupName = false;
                
                return;
            }

            if (e.PropertyName != nameof(IGroupableFeature.IsDefaultGroup))
            {
                MarkOutputOutOfSync();
            }
        }

        private void RemoveAreaFeature(IFeature feature)
        {
            if (areaDataItems.TryGetValue(feature, out List<IDataItem> dataItemsToBeRemoved))
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
                    new WaterFlowFMFeatureValueConverter(this, feature, quantity, string.Empty)
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
                ((INotifyCollectionChange)areaDataItem.Value).CollectionChanged += HydroAreaCollectionChanged;
                ((INotifyPropertyChanged)areaDataItem.Value).PropertyChanged += HydroAreaPropertyChanged;
            }
        }

        protected override void OnBeforeDataItemsSet()
        {
            base.OnBeforeDataItemsSet();

            areaDataItem = GetDataItemByTag(HydroAreaTag);
            if (areaDataItem != null)
            {
                ((INotifyCollectionChange)areaDataItem.Value).CollectionChanged -= HydroAreaCollectionChanged;
                ((INotifyPropertyChanged)areaDataItem.Value).PropertyChanged -= HydroAreaPropertyChanged;
            }
        }

        protected override void OnDataItemLinked(object sender, LinkedUnlinkedEventArgs<IDataItem> e)
        {
            // subscribe to newly linked hydro area:
            IDataItem areaDataItem = GetDataItemByTag(HydroAreaTag);
            if (Equals(e.Target, areaDataItem) && !e.Relinking)
            {
                ((INotifyCollectionChange)areaDataItem.Value).CollectionChanged += HydroAreaCollectionChanged;
                ((INotifyPropertyChanged)areaDataItem.Value).PropertyChanged += HydroAreaPropertyChanged;
            }

            base.OnDataItemLinked(sender, e);
        }

        protected override void OnDataItemUnlinking(object sender, LinkingUnlinkingEventArgs<IDataItem> e)
        {
            // unsubscribe from area before unlink
            areaDataItem = GetDataItemByTag(HydroAreaTag);
            if (Equals(e.Target, areaDataItem))
            {
                ((INotifyCollectionChange)areaDataItem.Value).CollectionChanged -= HydroAreaCollectionChanged;
                ((INotifyPropertyChanged)areaDataItem.Value).PropertyChanged -= HydroAreaPropertyChanged;
            }

            base.OnDataItemUnlinking(sender, e);
        }

        private void FMRegionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            MarkOutputOutOfSync();
        }

        protected override void OnInputCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // empty to override Delta Shell framework logic, like removing output when input has been changed. 
        }

        protected override void OnInputPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // empty to override Delta Shell framework logic, like removing output when input has been changed. 
        }

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

            listOfOutputRestartFiles.Clear();
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
                            feature => new BoundaryConditionSet { Feature = feature }));
            syncers.Add(new FeatureDataSyncer<Feature2D, SourceAndSink>(
                            Pipes,
                            SourcesAndSinks,
                            feature => new SourceAndSink { Feature = feature }));
            syncers.Add(new FeatureDataSyncer<Feature2D, Lateral>(
                            LateralFeatures,
                            Laterals,
                            feature => new Lateral { Feature = feature }));
        }

        private void ClearSyncers()
        {
            syncers.ForEach(s => s.Dispose());
            syncers.Clear();
        }

        #endregion
    }
}