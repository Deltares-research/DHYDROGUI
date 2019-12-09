using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Core.Workflow.DataItems.ValueConverters;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.FunctionStores;
using DeltaShell.Plugins.NetworkEditor.Import;
using DeltaShell.Plugins.NetworkEditor.ImportExportCsv;
using GeoAPI.Extensions.Feature;
using Mono.Addins;

namespace DeltaShell.Plugins.NetworkEditor
{
    [Extension(typeof(IPlugin))]
    public class NetworkEditorApplicationPlugin : ApplicationPlugin
    {
        public override string Name
        {
            get { return "Network"; }
        }

        public override string DisplayName
        {
            get { return Properties.Resources.NetworkEditorApplicationPlugin_DisplayName_Hydro_Region_Plugin; }
        }

        public override string Description
        {
            get { return Properties.Resources.NetworkEditorApplicationPlugin_Description; }
        }

        public override string Version
        {
            get { return GetType().Assembly.GetName().Version.ToString(); }
        }

        public override string FileFormatVersion
        {
            get { return "3.5.2.0"; }
        }

        public override IEnumerable<Assembly> GetPersistentAssemblies()
        {
            yield return typeof (HydroRegion).Assembly;
            yield return typeof(ReadOnlyMapHisFileFunctionStore).Assembly;
            yield return GetType().Assembly;
        }

        public override void Activate()
        {
            base.Activate();

            Application.ProjectClosing += ApplicationOnProjectClosing;
            Application.ProjectOpened += ApplicationOnProjectOpened;
        }

        public override IEnumerable<IFileImporter> GetFileImporters()
        {
            yield return new BridgeFromGisImporter();
            yield return new CatchmentFromGisImporter();
            yield return new ChannelFromGisImporter();
            yield return new CrossSectionXYZFromGisImporter();
            yield return new CrossSectionYZFromGisImporter();
            yield return new CrossSectionZWFromGisImporter();
            yield return new CulvertFromGisImporter();
            yield return new HydroRegionFromGisImporter();
            yield return new LateralSourceFromGisImporter();
            yield return new ObservationPointFromGisImporter();
            yield return new PumpFromGisImporter();
            yield return new SimpleWeirFromGisImporter();
            yield return new CrossSectionYZFromCsvFileImporter();
            yield return new CrossSectionXYZFromCsvFileImporter();
            yield return new CrossSectionZWFromCsvFileImporter();
            yield return new NetworkCoverageFromGisImporter();
            yield return new HydroAreaEmbankmentImporter();
            yield return new HydroAreaEmbankmentHeightImporter();
        }

        public override IEnumerable<IFileExporter> GetFileExporters()
        {
            yield return new CrossSectionYZToCsvFileExporter();
            yield return new CrossSectionXYZToCsvFileExporter();
            yield return new CrossSectionZWToCsvFileExporter();
        }

        private void ApplicationOnProjectClosing(Project project)
        {
            ((INotifyCollectionChanged) Application.Project).CollectionChanged -= OnProjectCollectionChanged;
        }

        private void ApplicationOnProjectOpened(Project project)
        {
            ((INotifyCollectionChanged) Application.Project).CollectionChanged += OnProjectCollectionChanged;
            FixOwnersOnChildDataItems(project.RootFolder);
        }

        private void FixOwnersOnChildDataItems(Folder folder)
        {
            foreach (var item in folder.Items)
            {
                var subfolder = item as Folder;
                if (subfolder != null)
                    FixOwnersOnChildDataItems(subfolder);

                var dataItem = item as IDataItem;
                if (dataItem != null)
                    SetOwnerForDataItem(dataItem, folder);
            }
        }

        private void SetOwnerForDataItem(IDataItem dataItem, IDataItemOwner dataItemOwner)
        {
            dataItem.Owner = dataItemOwner;
            foreach(var child in dataItem.Children)
                SetOwnerForDataItem(child, dataItemOwner);
        }

        private void OnProjectCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HandleSubRegionCollectionChanged(sender, e);

            HandleRegionDataItemCollectionChanged(sender, e);
        }

        /// <summary>
        /// Add / remove data item in the project - handle sub-regions and/or data items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [EditAction]
        private void HandleRegionDataItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var regionDataItem = e.GetRemovedOrAddedItem() as IDataItem;

            if (regionDataItem == null || !(regionDataItem.Value is IHydroRegion))
            {
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if(!(sender is IEventedList<IProjectItem>))
                {
                    return;
                }

                AddChildRegionDataItems(regionDataItem);
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                // handle child data item removed
                var region = (IHydroRegion)regionDataItem.Value;
                if (region.Parent != null && regionDataItem.Parent != null && Equals(regionDataItem.Parent.Children, sender))
                {
                    region.Parent.SubRegions.Remove(region);
                }

                return;
            }

            throw new NotSupportedException(string.Format(Properties.Resources.NetworkEditorApplicationPlugin_HandleRegionDataItemCollectionChanged__0__is_not_supported_for_hydro_regions_in_the_project, e.Action));
        }
        
        /// <summary>
        /// Add / remove child data item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [EditAction]
        private void HandleSubRegionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var subRegions = sender as IEventedList<IRegion>;
            var subRegion = e.GetRemovedOrAddedItem() as IRegion;

            if (subRegions == null || subRegion == null || subRegion.Parent == null)
            {
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var parentRegionDataItem = Application.DataItemService.GetDataItemByValue(Application.Project, subRegion.Parent);
                AddChildRegionDataItems(parentRegionDataItem);
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var parentRegionDataItem = Application.DataItemService.GetDataItemByValue(Application.Project, subRegion.Parent);
                var regionDataItem = parentRegionDataItem.Children.FirstOrDefault(di => Equals(di.Value, subRegion));
                if (regionDataItem != null)
                {
                    parentRegionDataItem.Children.Remove(regionDataItem);
                }
                return;
            }

            throw new NotSupportedException(string.Format(Properties.Resources.NetworkEditorApplicationPlugin_HandleSubRegionCollectionChanged__0__is_not_supported_on_the_hydro_region_collection, e.Action));
        }

        private static void AddChildRegionDataItems(IDataItem regionDataItem)
        {
            var region = regionDataItem.Value as IRegion;

            if(region == null)
            {
                return;
            }

            foreach (var subRegion in region.SubRegions)
            {
                var existingDataItem = regionDataItem.Children.FirstOrDefault(childDataItem => childDataItem.Value == subRegion);
                if (existingDataItem == null)
                {
                    existingDataItem = CreateDataItemForSubRegion(subRegion, regionDataItem);
                    regionDataItem.Children.Add(existingDataItem);
                }
                AddChildRegionDataItems(existingDataItem);
            }
        }

        private static IDataItem CreateDataItemForSubRegion(IRegion subRegion, IDataItem parent)
        {
            return new DataItem
            {
                Name = subRegion.Name,
                Role = parent.Role,
                Parent = parent,
                ValueType = typeof(IRegion),
                ValueConverter = new AggregationValueConverter(subRegion),
                Owner = parent.Owner
            };
        }
    }
}