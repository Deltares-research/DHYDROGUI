using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.Editing;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.DelftModels.HydroModel.ModelExchanges;
using GeoAPI.Extensions.Feature;
using log4net;
using NetTopologySuite.Extensions.IO;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.DelftModels.HydroModel
{
    /// <summary>
    /// Partial class dedicated to isolate the logic in case we are
    /// dealing with coupling of FileBased models
    /// </summary>
    public partial class HydroModel
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(HydroModel));

        // place to save link infos in per hydro model
        private readonly IList<ModelExchangeInfo> modelExchangeInfos = new List<ModelExchangeInfo>();
        private readonly IList<RegionExchangeInfo> regionExchangeInfos = new List<RegionExchangeInfo>();

        #region Logic for (un)linking/saving RTC-FlowFM filebased coupling
        
        public virtual IHydroCoupling HydroCoupling => new HydroCoupling();

        /// <summary>
        /// Unlink data items and remember them in linkInfos.
        /// Can be restored after saving to the database with <see cref="RelinkDataItems"/>.
        /// </summary>
        public virtual void UnlinkAndRememberDataItems()
        {
            if (!TryGetFmAndRtcModel(out var flowModel, out var rtcModel)) 
                return;

            modelExchangeInfos.Clear();
            modelExchangeInfos.AddRange(GetExchangeInfo(flowModel, rtcModel));
        }

        public virtual void UnlinkAndRememberRegionLinks()
        {
            regionExchangeInfos.Clear();
            regionExchangeInfos.AddRange(GetRegionExchangeInfo());
        }

        /// <summary>
        /// re-establish the data links that were cut when the model was saved
        /// Can only be used once linkInfos is filled.
        /// </summary>
        public virtual void RelinkDataItems()
        {
            foreach (var exchangeInfo in modelExchangeInfos)
            {
                var sourceModel = Models.FirstOrDefault(m => Equals(ModelExchangeInfo.GetModelIdentifier(m), exchangeInfo.SourceModelName));
                var targetModel = Models.FirstOrDefault(m => Equals(ModelExchangeInfo.GetModelIdentifier(m), exchangeInfo.TargetModelName));

                if (sourceModel == null || targetModel == null) continue;

                var sourceItems = GetDataItems(sourceModel, DataItemRole.Output).ToList();
                var targetItems = GetDataItems(targetModel, DataItemRole.Input).ToList();

                foreach (var exchange in exchangeInfo.Exchanges)
                {
                    var sourceItem = sourceItems.FirstOrDefault(di => Equals(ModelExchange.GetExchangeIdentifier(di), exchange.SourceName));
                    var targetItem = targetItems.FirstOrDefault(di => Equals(ModelExchange.GetExchangeIdentifier(di), exchange.TargetName));

                    if (sourceItem == null || targetItem == null)
                    {
                        continue;
                    }

                    if (!targetItem.LinkedBy.Contains(targetItem))
                    {
                        // link:
                        targetItem.LinkTo(sourceItem);
                    }
                }
            }
        }

        public virtual void RelinkHydroRegionLinks()
        {
            PrepareHydroModelCoupling();
            
            if (!regionExchangeInfos.Any()) return;

            HashSet<(IHydroObject, IHydroObject)> linkConnections = CreateLinkConnectionsHashSet();

            try
            {
                Region.BeginEdit(new DefaultEditAction("Adding saved hydro links"));
                
                var regionObjectsLookup = new Dictionary<IHydroRegion, Dictionary<Type, Dictionary<string, IHydroObject>>>();
                foreach (IRegion region in Region.SubRegions)
                {
                    IHydroRegion hydroRegion = region as IHydroRegion;
                    if (hydroRegion != null)
                    {
                        Dictionary<Type, Dictionary<string, IHydroObject>> dictionary = new Dictionary<Type, Dictionary<string, IHydroObject>>();
                        foreach (var objects in hydroRegion.AllHydroObjects.GroupBy(ho => TypeUtils.Unproxy(ho).GetType()))
                        {
                            dictionary[objects.Key] = objects.ToDictionary(o => o.Name, StringComparer.InvariantCultureIgnoreCase);
                        }

                        regionObjectsLookup[hydroRegion] = dictionary;
                    }
                }

                var regionByNameLookup = Region.SubRegions.OfType<IHydroRegion>().ToDictionary(r => r.Name, StringComparer.InvariantCultureIgnoreCase);

                foreach (var regionExchangeInfo in regionExchangeInfos)
                {
                    var sourceRegionFound = regionByNameLookup.TryGetValue(regionExchangeInfo.SourceRegionName, out var sourceRegion);
                    var targetRegionFound = regionByNameLookup.TryGetValue(regionExchangeInfo.TargetRegionName, out var targetRegion);
                    
                    if (!sourceRegionFound || !targetRegionFound)
                    {
                        log.Error($"Could not restore links between {regionExchangeInfo.SourceRegionName} and {regionExchangeInfo.TargetRegionName}");
                        continue;
                    }

                    foreach (var regionExchange in regionExchangeInfo.Exchanges)
                    {
                        var sourceType = Type.GetType(regionExchange.SourceType);
                        Dictionary<string, IHydroObject> sourceHydroObjectTypes = null;
                        var hasSourceHydroObjectType = sourceType != null && regionObjectsLookup[sourceRegion].TryGetValue(sourceType, out sourceHydroObjectTypes);

                        IHydroObject source = null;
                        var hasSourceHydroObject = hasSourceHydroObjectType &&
                                                   sourceHydroObjectTypes.TryGetValue(regionExchange.SourceName,out source) && 
                                                   source.CanBeLinkSource;

                        var targetType = Type.GetType(regionExchange.TargetType);
                        Dictionary<string, IHydroObject> targetHydroObjectTypes = null;
                        var hasTargetHydroObjectType = targetType != null && regionObjectsLookup[targetRegion].TryGetValue(targetType, out targetHydroObjectTypes);

                        IHydroObject target = null;
                        var hasTargetHydroObject = hasTargetHydroObjectType && 
                                                   targetHydroObjectTypes.TryGetValue(regionExchange.TargetName,out target) && 
                                                   target.CanBeLinkTarget;
                        

                        if (!hasSourceHydroObject || !hasTargetHydroObject)
                        {
                            log.Error(
                                $"Could not restore link between {regionExchange.SourceName} ({regionExchangeInfo.SourceRegionName}) and {regionExchange.TargetName} ({regionExchangeInfo.TargetRegionName})");
                            continue;
                        }
                        
                        if (linkConnections.Contains((source, target)))
                        {
                            continue;
                        }
                        
                        IHydroCoupling hydroModelCoupling = GetHydroModelCouplings().FirstOrDefault(coupling => coupling.CanLink(source));
                        if (hydroModelCoupling == null)
                        {
                            continue;
                        }
                        
                        IHydroLink hydroLink = hydroModelCoupling.CreateLink(source,target);

                        if (hydroLink == null)
                        {
                            continue; 
                        }

                        hydroLink.Name = regionExchange.LinkName;

                        var sourceAsCatchment = source as Catchment;
                        if (sourceAsCatchment != null && Equals(sourceAsCatchment.CatchmentType, CatchmentType.NWRW))
                        {
                            source.Geometry = target.Geometry;
                            hydroLink.Geometry = new LineString(new[]
                            {
                                source.Geometry?.Coordinate,
                                target.Geometry?.Coordinate
                            });
                        }
                        else
                        {
                            hydroLink.Geometry = new WKTReader().Read(regionExchange.LinkGeometryWkt);
                        }
                        
                        linkConnections.Add((hydroLink.Source, hydroLink.Target));

                        if (sourceAsCatchment != null && sourceAsCatchment.ModelData != null)
                        {
                            sourceAsCatchment.ModelData.DoAfterLinking(target);
                        }
                    }
                }
            }
            finally
            {
                EndHydroModelCoupling();
                Region.EndEdit();
            }
        }

        private void EndHydroModelCoupling()
        {
            foreach (IHydroCoupling hydroModelCoupling in GetHydroModelCouplings())
            {
                hydroModelCoupling.End();
            }
        }

        private void PrepareHydroModelCoupling()
        {
            foreach (IHydroCoupling hydroModelCoupling in GetHydroModelCouplings())
            {
                hydroModelCoupling.Prepare();
            }
        }
        
        private IEnumerable<IHydroCoupling> GetHydroModelCouplings()
        {
            return Models.OfType<IHydroModel>()
                         .Select(hydroModel => hydroModel.HydroCoupling);
        }

        private IModel GetModelByTypeName(string typeName)
        {
            return Models.FirstOrDefault(model => model.GetEntityType().Name.Equals(typeName));
        }

        private bool TryGetFmAndRtcModel(out IModel flowModel, out IModel rtcModel)
        {
            flowModel = GetModelByTypeName("WaterFlowFMModel");
            rtcModel = GetModelByTypeName("RealTimeControlModel");
            return flowModel != null && rtcModel != null;
        }

        private static IEnumerable<IDataItem> GetDataItems(IModel model, DataItemRole role)
        {
            if (model is ICoupledModel coupledModel)
            {
                return coupledModel.GetDataItemsUsedForCouplingModel(role);
            }

            return model is IFileBased
                ? model.GetChildDataItemLocations(role).SelectMany(model.GetChildDataItems)
                : model.AllDataItems.Where(di => (di.Role & role) == role);
        }

        /// <summary>
        /// finds the currently linked dataitems of the models, stores them in exhangeInfo, and when unlink is
        /// true, the linkage is broken. Currently only used for flowfm + rtc
        /// </summary>
        private static IEnumerable<ModelExchangeInfo> GetExchangeInfo(IModel flowModel, IModel rtcModel)
        {
            yield return GetModelExchangeInfo(rtcModel, flowModel);
            yield return GetModelExchangeInfo(flowModel, rtcModel);
        }

        private static ModelExchangeInfo GetModelExchangeInfo(IModel targetModel, IModel sourceModel)
        {
            var targetInputItems = GetDataItems(targetModel, DataItemRole.Input).ToList();
            var sourceOutputItems = GetDataItems(sourceModel, DataItemRole.Output).ToList();

            var exchangeInfo = new ModelExchangeInfo(sourceModel, targetModel);

            foreach (var sourceOutputItem in sourceOutputItems)
            {
                var hiddenStateSource = sourceOutputItem.Hidden;
                try
                {
                    sourceOutputItem.Hidden = true; // for performance (bypasses view and sync logic)

                    foreach (var targetInputItem in targetInputItems.Where(i => i.LinkedTo != null && i.LinkedTo.Equals(sourceOutputItem)))
                    {
                        exchangeInfo.Exchanges.Add(new ModelExchange(sourceOutputItem, targetInputItem));

                        var hiddenStateTarget = targetInputItem.Hidden;
                        try
                        {
                            targetInputItem.Hidden = true; // for performance (bypasses view and sync logic)
                            targetInputItem.Unlink();
                        }
                        finally
                        {
                            targetInputItem.Hidden = hiddenStateTarget;
                        }
                    }
                }
                finally
                {
                    sourceOutputItem.Hidden = hiddenStateSource;
                }

            }

            return exchangeInfo;
        }

        private IEnumerable<RegionExchangeInfo> GetRegionExchangeInfo()
        {
            if (!Region.Links.Any())
            {
                return Enumerable.Empty<RegionExchangeInfo>();
            }

            var infos = new List<RegionExchangeInfo>();

            try
            {
                Region.BeginEdit(new DefaultEditAction("Removing hydro links"));

                var sourceRegionsLinksGrouping = Region.Links.Where(l => l.Source is Catchment).GroupBy(l => l.Source.Region);
                foreach (var sourceRegionGroup in sourceRegionsLinksGrouping)
                {
                    var sourceRegion = sourceRegionGroup.Key;
                    var targetRegionGrouping = sourceRegionGroup.GroupBy(l => l.Target.Region);

                    foreach (var targetRegionGroup in targetRegionGrouping)
                    {
                        var targetRegion = targetRegionGroup.Key;

                        infos.Add(new RegionExchangeInfo(sourceRegion, targetRegion)
                        {
                            Exchanges = targetRegionGroup.Select(l =>
                            {
                                var regionExchange = new RegionExchange(l);
                                Region.Links.Remove(l);
                                return regionExchange;
                            }).ToList()
                        });
                    }
                }
            }
            finally
            {
                Region.EndEdit();
            }

            return infos;
        }

        #endregion RTC-FlowFM coupling

        #region Logic for Model saving, loading, and copying

        private string modelCouplingFilePath;
        private string regionCouplingFilePath;

        private void ModelSaveTo(string modelExchangesFilePath, string regionExchangesFilePath)
        {
            FileUtils.CreateDirectoryIfNotExists(Path.GetDirectoryName(modelExchangesFilePath));
            FileUtils.CreateDirectoryIfNotExists(Path.GetDirectoryName(regionExchangesFilePath));

            // write exchanges
            modelExchangeInfos.WriteToJson(modelExchangesFilePath);
            regionExchangeInfos.WriteToJson(regionExchangesFilePath);
        }

        #endregion

        #region IFileBased and NHibernate

        //tells NHibernate we need to be saved
        private void MarkDirty()
        {
            unchecked { dirtyCounter++; }
        }
        private int dirtyCounter;

        private string path;

        string IFileBased.Path
        {
            get { return path; }
            set
            {
                if (path == value)
                    return;

                path = value;

                if (path == null)
                    return;

                if (path.StartsWith("$") && IsOpen)
                {
                    ModelSaveTo(modelCouplingFilePath, regionCouplingFilePath);
                }
            }
        }

        public virtual IEnumerable<string> Paths
        {
            get { yield return ((IFileBased)this).Path; }
        }

        public virtual bool IsFileCritical { get { return true; } }

        public virtual bool IsOpen { get; protected set; }

        void IFileBased.CreateNew(string path)
        {
            modelCouplingFilePath = GetJsonPathFromDeltaShellPath(path, Name);
            regionCouplingFilePath = GetJsonPathFromDeltaShellPath(path, GetRegionExchangesFileName());

            ModelSaveTo(modelCouplingFilePath, regionCouplingFilePath);
            
            this.path = path;
            IsOpen = true;
        }

        void IFileBased.Close()
        {
            IsOpen = false;
        }

        void IFileBased.Open(string path)
        {
            IsOpen = true;
        }

        void IFileBased.CopyTo(string destinationPath)
        {
            ModelSaveTo(GetJsonPathFromDeltaShellPath(destinationPath, Name), GetJsonPathFromDeltaShellPath(destinationPath, GetRegionExchangesFileName()));
        }

        void IFileBased.SwitchTo(string newPath)
        {
            path = newPath;
            
            var modelCouplingPath = GetJsonPathFromDeltaShellPath(newPath, Name);
            
            if (modelCouplingFilePath == null)
            {
                modelExchangeInfos.ReadFromJson(modelCouplingPath);
            }

            modelCouplingFilePath = modelCouplingPath;


            var regionCouplingPath = GetJsonPathFromDeltaShellPath(newPath, GetRegionExchangesFileName());

            if (regionCouplingFilePath == null)
            {
                regionExchangeInfos.ReadFromJson(regionCouplingPath);
            }

            regionCouplingFilePath = regionCouplingPath;

            IsOpen = true;
        }

        private string GetRegionExchangesFileName()
        {
            return Name + "RegionExchanges";
        }

        void IFileBased.Delete()
        {
        }

        private string GetJsonPathFromDeltaShellPath(string dsPath, string fileName)
        {
            // dsproj_data/<model name>/<model name>.json
            return Path.Combine(Path.GetDirectoryName(dsPath), Name, fileName + ".json");
        }

        #endregion
        
        private HashSet<(IHydroObject, IHydroObject)> CreateLinkConnectionsHashSet()
        {
            var hashSet = new HashSet<(IHydroObject, IHydroObject)>();
            foreach (HydroLink hydroLink in Region.Links)
            {
                hashSet.Add((hydroLink.Source, hydroLink.Target));
            }

            return hashSet;
        }
    }
}
