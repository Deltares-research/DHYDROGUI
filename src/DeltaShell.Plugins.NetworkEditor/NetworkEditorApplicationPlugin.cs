using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Import;
using DeltaShell.Plugins.NetworkEditor.ImportExportCsv;
using GeoAPI.Extensions.Feature;
using Mono.Addins;
using NetTopologySuite.IO;

namespace DeltaShell.Plugins.NetworkEditor
{
    [Extension(typeof(IPlugin))]
    public class NetworkEditorApplicationPlugin : ApplicationPlugin
    {
        static readonly WKTReader WktReader = new WKTReader();

        public readonly IList<IFileExporter> exporters = new List<IFileExporter>
            {
                new CrossSectionYZToCsvFileExporter(),
                new CrossSectionXYZToCsvFileExporter(),
                new CrossSectionZWToCsvFileExporter()
            };

        public override string Name
        {
            get { return Properties.Resources.NetworkEditorApplicationPlugin_GetDataItemInfos_Network; }
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

        public override string FileFormatVersion => "3.5.2.0";

        public override void Activate()
        {
            base.Activate();

            Application.ProjectService.ProjectClosing += ApplicationOnProjectClosing;
            Application.ProjectService.ProjectOpened += ApplicationOnProjectOpened;
            Application.ProjectService.ProjectCreated += ApplicationOnProjectOpened;
        }

        public override void Deactivate()
        {
            if (!IsActive)
            {
                return;
            }

            Application.ProjectService.ProjectClosing -= ApplicationOnProjectClosing;
            Application.ProjectService.ProjectOpened -= ApplicationOnProjectOpened;
            Application.ProjectService.ProjectCreated -= ApplicationOnProjectOpened;

            base.Deactivate();
        }

        public override IEnumerable<DataItemInfo> GetDataItemInfos()
        {
            yield return new DataItemInfo<HydroRegion>
                {
                    Name = Properties.Resources.NetworkEditorApplicationPlugin_GetDataItemInfos_Region,
                    Category = Properties.Resources.NetworkEditorApplicationPlugin_GetDataItemInfos_Hydro,
                    Image = Properties.Resources.HydroRegion,
                    CreateData = owner => new HydroRegion
                        {
                            Name = string.Format("{0}1", Properties.Resources.NetworkEditorApplicationPlugin_GetDataItemInfos_Region),
                            SubRegions =
                                {
                                    new HydroNetwork { Name = Properties.Resources.NetworkEditorApplicationPlugin_GetDataItemInfos_Network },
                                    new DrainageBasin { Name = Properties.Resources.NetworkEditorApplicationPlugin_GetDataItemInfos_Basin }
                                },
                        },
                    AddExampleData = data => AddExampleHydroRegionData(data)
                };

            yield return new DataItemInfo<HydroNetwork>
                {
                    Name = Properties.Resources.NetworkEditorApplicationPlugin_GetDataItemInfos_Network,
                    Category = Properties.Resources.NetworkEditorApplicationPlugin_GetDataItemInfos_Hydro,
                    Image = Properties.Resources.Network,
                    AdditionalOwnerCheck = owner => !(owner is HydroRegion)
                                                    || !((HydroRegion) owner).SubRegions.OfType<HydroNetwork>().Any(), // Support only a single network per region for now
                    CreateData = owner =>
                        {
                            var network = new HydroNetwork();

                            var hydroRegion = owner as HydroRegion;
                            if (hydroRegion != null)
                            {
                                network.Name = NamingHelper.GetUniqueName(string.Format("{0}{{0}}", Properties.Resources.NetworkEditorApplicationPlugin_GetDataItemInfos_Network), hydroRegion.SubRegions);

                                hydroRegion.SubRegions.Add(network);
                            }

                            var folder = owner as Folder;
                            if (folder != null)
                            {
                                network.Name = NamingHelper.GetUniqueName(string.Format("{0}{{0}}", Properties.Resources.NetworkEditorApplicationPlugin_GetDataItemInfos_Network), folder.Items);
                            }

                            return network;
                        },
                    AddExampleData = data => AddExampleHydroNetworkData(data)
                };

            yield return new DataItemInfo<IDrainageBasin>
                {
                    Name = Properties.Resources.NetworkEditorApplicationPlugin_GetDataItemInfos_Basin,
                    Category = Properties.Resources.NetworkEditorApplicationPlugin_GetDataItemInfos_Hydro,
                    Image = Properties.Resources.DrainageBasin,
                    AdditionalOwnerCheck = owner => !(owner is HydroRegion)
                                                    || !((HydroRegion) owner).SubRegions.OfType<IDrainageBasin>().Any(), // Support only a single basin per region for now
                    CreateData = owner =>
                        {
                            var drainageBasin = new DrainageBasin();

                            var hydroRegion = owner as HydroRegion;
                            if (hydroRegion != null)
                            {
                                drainageBasin.Name = NamingHelper.GetUniqueName(string.Format("{0}{{0}}", Properties.Resources.NetworkEditorApplicationPlugin_GetDataItemInfos_Basin), hydroRegion.SubRegions);

                                hydroRegion.SubRegions.Add(drainageBasin);
                            }

                            var folder = owner as Folder;
                            if (folder != null)
                            {
                                drainageBasin.Name = NamingHelper.GetUniqueName(string.Format("{0}{{0}}", Properties.Resources.NetworkEditorApplicationPlugin_GetDataItemInfos_Basin), folder.Items);
                            }

                            return drainageBasin;
                        },
                    AddExampleData = data => AddExampleDrainageBasinData(data)
                };
        }

        public override IEnumerable<IFileImporter> GetFileImporters()
        {
            yield return new BridgeRectangularFromGisImporter();
            yield return new BridgeZwFromGisImporter();
            yield return new BridgeYzFromGisImporter();
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
            return exporters;
        }

        private void ApplicationOnProjectClosing(object sender, EventArgs<Project> e)
        {
            ((INotifyCollectionChanged) e.Value).CollectionChanged -= OnProjectCollectionChanged;
        }

        private void ApplicationOnProjectOpened(object sender, EventArgs<Project> e)
        {
            Project project = e.Value;
            ((INotifyCollectionChanged) project).CollectionChanged += OnProjectCollectionChanged;
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
                var parentRegionDataItem = Application.DataItemService.GetDataItemByValue(Application.ProjectService.Project, subRegion.Parent);
                AddChildRegionDataItems(parentRegionDataItem);
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var parentRegionDataItem = Application.DataItemService.GetDataItemByValue(Application.ProjectService.Project, subRegion.Parent);
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
                Value = subRegion,
                Owner = parent.Owner
            };
        }

        private static void AddExampleHydroRegionData(IRegion hydroRegion)
        {
            var hydroNetwork = hydroRegion.SubRegions.OfType<IHydroNetwork>().First();
            var drainageBasin = hydroRegion.SubRegions.OfType<IDrainageBasin>().First();

            AddExampleHydroNetworkData(hydroNetwork);
            AddExampleDrainageBasinData(drainageBasin);

            drainageBasin.WasteWaterTreatmentPlants[0].LinkTo(hydroNetwork.LateralSources.FirstOrDefault());
        }

        private static void AddExampleHydroNetworkData(IHydroNetwork network)
        {
            var node1 = new HydroNode { Name = "Node1", Geometry = WktReader.Read("POINT(0 0)") };
            var node2 = new HydroNode { Name = "Node2", Geometry = WktReader.Read("POINT(10 0)") };
            var lateral = new LateralSource { Chainage = 5, Geometry = WktReader.Read("POINT(5 0)") };
            var channel1 = new Channel { Name = "Channel1", BranchFeatures = { lateral }, Source = node1, Target = node2, Geometry = WktReader.Read("LINESTRING(0 0, 10 0)") };

            network.Branches.Add(channel1);
            network.Nodes.AddRange(new[] { node1, node2 });
        }

        private static void AddExampleDrainageBasinData(IDrainageBasin drainageBasin)
        {
            var catchment = new Catchment { Name = "Catchment1", CatchmentType = CatchmentType.Unpaved, Geometry = WktReader.Read("POLYGON((0 6, 0 12, 10 12, 10 6, 0 6))"), Basin = drainageBasin};
            var plant = new WasteWaterTreatmentPlant { Name = "Plant1", Geometry = WktReader.Read("POINT(5 5)") };

            drainageBasin.Catchments.Add(catchment);
            drainageBasin.WasteWaterTreatmentPlants.Add(plant);

            catchment.LinkTo(plant);
        }
    }
}