using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using BasicModelInterface;
using DelftTools.Functions;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public partial class WaterFlowFMModel
    {
        private double previousProgress = 0;
        private string progressText;

        [NoNotifyPropertyChange]
        public override DateTime StartTime
        {
            get => (DateTime) ModelDefinition.GetModelProperty(GuiProperties.StartTime).Value;
            set
            {
                ModelDefinition.GetModelProperty(GuiProperties.StartTime).Value = value;
                // This base model setting is made to make the base logic right
                base.StartTime = value;
            }
        }

        public override string ProgressText => string.IsNullOrEmpty(progressText) ? base.ProgressText : progressText;

        public override IEnumerable<IDataItem> AllDataItems
        {
            get
            {
                return base.AllDataItems.Concat(areaDataItems.Values.SelectMany(v => v));
            }
        }

        public override IBasicModelInterface BMIEngine => runner.Api;

        [NoNotifyPropertyChange]
        public override DateTime StopTime
        {
            get => (DateTime) ModelDefinition.GetModelProperty(GuiProperties.StopTime).Value;
            set
            {
                ModelDefinition.GetModelProperty(GuiProperties.StopTime).Value = value;
                // This base model setting is made to make the base logic right
                base.StopTime = value;
            }
        }

        public override TimeSpan TimeStep
        {
            get => (TimeSpan) ModelDefinition.GetModelProperty(KnownProperties.DtUser).Value;
            set
            {
                ModelDefinition.GetModelProperty(KnownProperties.DtUser).Value = value;
                // This base model setting is made to make the base logic right
                base.TimeStep = value;
            }
        }

        public override IProjectItem DeepClone()
        {
            string tempDir = FileUtils.CreateTempDirectory();
            string mduFileName = MduFilePath != null ? Path.GetFileName(MduFilePath) : "some_temp.mdu";
            string tempFilePath = Path.Combine(tempDir, mduFileName);
            ExportTo(tempFilePath, false);

            return new WaterFlowFMModel(tempFilePath);
        }

        /// <summary>
        /// Gets the direct children of the parent object
        /// </summary>
        /// <returns> </returns>
        public override IEnumerable<object> GetDirectChildren()
        {
            foreach (object item in base.GetDirectChildren())
            {
                yield return item;
            }

            foreach (Feature2D boundary in Boundaries)
            {
                yield return boundary;
            }

            foreach (Feature2D pipe in Pipes)
            {
                yield return pipe;
            }

            foreach (BoundaryConditionSet boundaryConditionSet in BoundaryConditionSets)
            {
                yield return boundaryConditionSet;
            }

            foreach (SourceAndSink sourcesAndSink in SourcesAndSinks)
            {
                yield return sourcesAndSink;
            }

            if (ModelDefinition.HeatFluxModel.MeteoData != null)
            {
                yield return ModelDefinition.HeatFluxModel;
            }

            yield return WindFields;

            foreach (IWindField windField in WindFields)
            {
                yield return windField;
            }

            //uncomment when required:
            //yield return Grid;

            yield return InitialSalinity;
            yield return Viscosity;
            yield return Diffusivity;
            yield return Roughness;
            yield return InitialWaterLevel;
            yield return InitialTemperature;
            yield return InitialTracers;
            yield return InitialFractions;

            //for QueryTimeSeries tool:
            if (OutputHisFileStore != null)
            {
                foreach (IFunction function in OutputHisFileStore.Functions)
                {
                    yield return function;
                }
            }

            if (OutputMapFileStore != null)
            {
                foreach (IFunction function in OutputMapFileStore.Functions)
                {
                    yield return function;
                }
            }

            if (OutputClassMapFileStore != null)
            {
                foreach (IFunction function in OutputClassMapFileStore.Functions)
                {
                    yield return function;
                }
            }
        }

        public override IEnumerable<IFeature> GetChildDataItemLocations(DataItemRole role)
        {
            if ((role & DataItemRole.Input) == DataItemRole.Input)
            {
                return InputFeatureCollections.OfType<IList>().SelectMany(l => l.OfType<IFeature>());
            }

            if ((role & DataItemRole.Output) == DataItemRole.Output)
            {
                return OutputFeatureCollections.OfType<IList>().SelectMany(l => l.OfType<IFeature>());
            }

            return Enumerable.Empty<IFeature>();
        }

        public override IEnumerable<IDataItem> GetChildDataItems(IFeature location)
        {
            if (location == null)
            {
                yield break;
            }

            List<IDataItem> items;
            areaDataItems.TryGetValue(location, out items);

            if (items == null)
            {
                yield break;
            }

            foreach (IDataItem di in items)
            {
                yield return di;
            }
        }

        protected override void OnInitialize()
        {
            previousProgress = 0;
            DataItems.RemoveAllWhere(di => di.Tag == DiaFileDataItemTag);

            ReportProgressText("Initializing");

            // Force fm kernel to write output to 'output' Directory
            SetOutputDirAndWaqDirProperty();

            if (Directory.Exists(WorkingOutputDirectoryPath))
            {
                DisconnectOutput();
                FileUtils.DeleteIfExists(WorkingOutputDirectoryPath);
                FileUtils.CreateDirectoryIfNotExists(WorkingOutputDirectoryPath);
            }

            runner.OnInitialize();

            ReportProgressText();
        }

        protected override void OnCleanup()
        {
            snapApiInErrorMode = false;
            base.OnCleanup();
            runner.OnCleanup();

            ReportProgressText();
        }

        protected override void OnExecute()
        {
            runner.OnExecute();
        }

        protected override void OnFinish()
        {
            runner.OnFinish();
            currentOutputDirectoryPath = WorkingOutputDirectoryPath;
        }

        protected override void OnProgressChanged()
        {
            // Only update gui for every 1 percent progress (performance)
            if (ProgressPercentage - previousProgress < 0.01)
            {
                return;
            }

            previousProgress = ProgressPercentage;
            runner.OnProgressChanged();
            base.OnProgressChanged();
        }

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

        // [TOOLS-22813] Override OnInputPropertyChanged to stop base class (ModelBase) from clearing the output
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
        }

        private IEnumerable<object> InputFeatureCollections
        {
            get
            {
                yield return Area.Pumps;
                yield return Area.Weirs;
            }
        }

        private IEnumerable<object> OutputFeatureCollections
        {
            get
            {
                yield return Area.ObservationPoints;
                yield return Area.ObservationCrossSections;
            }
        }

        private void ReportProgressText(string text = null)
        {
            progressText = text;
            base.OnProgressChanged();
        }
    }
}