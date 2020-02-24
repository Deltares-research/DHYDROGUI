using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.IO;

namespace DeltaShell.Plugins.DelftModels.HydroModel
{
    /// <summary>
    /// Partial class dedicated to isolate the logic in case we are
    /// dealing with coupling of FileBased models
    /// </summary>
    public partial class HydroModel
    {
        // place to save link infos in per hydro model
        private readonly IList<ModelExchangeInfo> modelExchangeInfos = new List<ModelExchangeInfo>();
        
        #region Logic for (un)linking/saving RTC-FlowFM filebased coupling

        /// <summary>
        /// Save the links between models to a different file.
        /// Only used for rtc and flow now.
        /// TODO: Use abstraction.
        /// </summary>
        public virtual void SaveLinks()
        {
            IModel flowModel, rtcModel;
            if (TryGetFmAndRtcModel(out flowModel, out rtcModel))
            {
                modelExchangeInfos.Clear();
                modelExchangeInfos.AddRange(GetExchangeInfo(flowModel, rtcModel, false));
            }
        }

        /// <summary>
        /// Unlink data items and remember them in linkInfos.
        /// Can be restored after saving to the database with <see cref="RelinkDataItems"/>.
        /// </summary>
        public virtual void UnlinkAndRememberDataItems()
        {
            IModel flowModel, rtcModel;
            if (TryGetFmAndRtcModel(out flowModel, out rtcModel))
            {
                modelExchangeInfos.Clear();
                modelExchangeInfos.AddRange(GetExchangeInfo(flowModel, rtcModel, true));
            }
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
                    IDataItem sourceItem = sourceItems.FirstOrDefault(di => Equals(ModelExchange.GetExchangeIdentifier(di), exchange.SourceName));

                    string targetName = exchange.TargetName.Trim();

                    if (exchangeInfo.TargetModelName == "FlowFM")
                    {
                        targetName = HydroModelHelper.UpdateOldNamesOfStructuresComponentsToNewNamesIfNeeded(targetName);
                    }

                    IDataItem targetItem = targetItems.FirstOrDefault(di => Equals(ModelExchange.GetExchangeIdentifier(di), targetName));

                    if (sourceItem == null || targetItem == null)
                    {
                        continue;
                    }

                    // link:
                    targetItem.LinkTo(sourceItem);
                }
            }
        }

        /// <summary>
        /// Load links when building model from file
        /// </summary>
        public virtual void LoadLinks()
        {
            modelExchangeInfos.Clear();
            modelExchangeInfos.AddRange(couplingFile.Read(couplingFile.FilePath));
        }

        public virtual string CouplingFilePath { get { return couplingFile != null ? couplingFile.FilePath : null; } }

        private bool TryGetFmAndRtcModel(out IModel flowModel, out IModel rtcModel)
        {
            flowModel = null;
            rtcModel = null;

            // TODO: Make this work upon abstractions
            foreach (var model in Models)
            {
                if (model.GetEntityType().Name.Equals("WaterFlowFMModel"))
                {
                    flowModel = model;
                }
                else if (model.GetEntityType().Name.Equals("RealTimeControlModel"))
                {
                    rtcModel = model;
                }
            }

            return flowModel != null && rtcModel != null;
        }

        public static IEnumerable<IDataItem> GetDataItems(IModel model, DataItemRole role)
        {
            if (model is IFileBased)
            {
                return model.GetChildDataItemLocations(role).SelectMany(model.GetChildDataItems);
            }
            else
            {
                return model.AllDataItems.Where(di => (di.Role & role) == role);
            }
        }

        /// <summary>
        /// finds the currently linked dataitems of the models, stores them in exhangeInfo, and when unlink is
        /// true, the linkage is broken. Currently only used for flowfm + rtc
        /// </summary>
        /// <param name="flowModel"></param>
        /// <param name="rtcModel"></param>
        /// <param name="unlink"></param>
        /// <returns></returns>
        private static IEnumerable<ModelExchangeInfo> GetExchangeInfo(IModel flowModel, IModel rtcModel, bool unlink)
        {
            List<IDataItem> flowInputItems = GetDataItems(flowModel, DataItemRole.Input).ToList();
            List<IDataItem> flowOutputItems = GetDataItems(flowModel, DataItemRole.Output).ToList();

            List<IDataItem> rtcInputItems = GetDataItems(rtcModel, DataItemRole.Input).ToList();
            IReadOnlyDictionary<IDataItem, string> rtcInputParameterNameMapping = GetDataItemNameMapping(rtcInputItems);

            List<IDataItem> rtcOutputItems = GetDataItems(rtcModel, DataItemRole.Output).ToList();
            IReadOnlyDictionary<IDataItem, string> rtcOutputParameterNameMapping = GetDataItemNameMapping(rtcOutputItems);

            var exchangeInfoList = new List<ModelExchangeInfo>();
            var exchangeInfo = new ModelExchangeInfo(flowModel, rtcModel);
            foreach (var flowOutputItem in flowOutputItems)
            {
                foreach (var rtcInputItem in rtcInputItems)
                {
                    if (rtcInputItem.LinkedTo != null && rtcInputItem.LinkedTo.Equals(flowOutputItem))
                    {
                        exchangeInfo.Exchanges.Add(new ModelExchange(flowOutputItem, rtcInputItem));
                        if (unlink)
                        {
                            rtcInputItem.Unlink();
                            RestoreDataItemName(rtcInputParameterNameMapping, rtcInputItem);
                        }
                    }
                }
            }
            exchangeInfoList.Add(exchangeInfo);

            exchangeInfo = new ModelExchangeInfo(rtcModel, flowModel);
            foreach (var rtcOutputItem in rtcOutputItems)
            {
                foreach (var flowInputItem in flowInputItems)
                {
                    if (flowInputItem.LinkedTo != null && flowInputItem.LinkedTo.Equals(rtcOutputItem))
                    {
                        exchangeInfo.Exchanges.Add(new ModelExchange(rtcOutputItem, flowInputItem));
                        if (unlink)
                        {
                            flowInputItem.Unlink();
                            RestoreDataItemName(rtcOutputParameterNameMapping, rtcOutputItem);
                        }
                    }
                }
            }
            exchangeInfoList.Add(exchangeInfo);

            return exchangeInfoList;
        }

        /// <summary>
        /// Gets the mapping between the <see cref="IDataItem"/> and the name that is associated with it.
        /// </summary>
        /// <param name="dataItems">The collection of <see cref="IDataItem"/> to retrieve  the mapping from.</param>
        /// <returns>A mapping between the <see cref="IDataItem"/> and the parameter name it maps.</returns>
        private static IReadOnlyDictionary<IDataItem, string> GetDataItemNameMapping(IEnumerable<IDataItem> dataItems)
        {
            // Filter the data items on INameable elements
            IEnumerable<IDataItem> filteredDataItems = dataItems.Where(item => item.ValueConverter?.OriginalValue is INameable);

            var mapping = new Dictionary<IDataItem, string>();
            foreach (IDataItem item in filteredDataItems)
            {
                var connectionPoint = (INameable) item.ValueConverter.OriginalValue;
                mapping.Add(item, connectionPoint.Name);
            }
            
            return mapping;
        }

        /// <summary>
        /// Restores the name of the <paramref name="dataItem"/> if applicable.
        /// </summary>
        /// <param name="mapping">The mapping to retrieve the name from.</param>
        /// <param name="dataItem">The <see cref="IDataItem"/> to restore the name for.</param>
        private static void RestoreDataItemName(IReadOnlyDictionary<IDataItem, string> mapping, IDataItem dataItem)
        {
            if (mapping.ContainsKey(dataItem))
            {
                var originalValue = (INameable) dataItem.ValueConverter.OriginalValue;
                originalValue.Name = mapping[dataItem];
            }
        }

        #endregion RTC-FlowFM coupling

        #region Logic for Model saving, loading, and copying

        private ModelCouplingFile couplingFile { get; set; }

        private void OnSave()
        {
            ModelSave();
        }

        private void OnAddedToProject(string filePath)
        {
            if (couplingFile == null)
            {
                couplingFile = new ModelCouplingFile { FilePath = filePath };
            }
            ModelSaveTo(filePath, false);
        }

        private void OnCopyTo(string filePath)
        {
            ModelSaveTo(filePath, false);
        }

        private void OnSwitchTo(string filePath)
        {
            if (couplingFile == null)
            {
                BuildModelFromFile(filePath);
            }
            else
            {
                couplingFile.FilePath = filePath;
            }
        }

        private void BuildModelFromFile(string filePath)
        {
            couplingFile = new ModelCouplingFile { FilePath = filePath };
            LoadLinks();
        }

        private void ModelSave()
        {
            ModelSaveTo(couplingFile.FilePath, true);
        }

        private void ModelSaveTo(string filePath, bool switchTo)
        {
            var targetDir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            // write exchanges
            couplingFile.Write(filePath, modelExchangeInfos);
            if (switchTo)
                couplingFile.FilePath = filePath;
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
                    OnSave();
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
            OnAddedToProject(GetJSONPathFromDeltaShellPath(path));
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
            OnCopyTo(GetJSONPathFromDeltaShellPath(destinationPath));
        }

        void IFileBased.SwitchTo(string newPath)
        {
            path = newPath;
            OnSwitchTo(GetJSONPathFromDeltaShellPath(newPath));
            IsOpen = true;
        }

        void IFileBased.Delete()
        {
        }

        private string GetJSONPathFromDeltaShellPath(string dsPath)
        {
            // dsproj_data/<model name>/<model name>.mdw
            return Path.Combine(Path.GetDirectoryName(dsPath), Path.Combine(Name, Name + ".json"));
        }

        #endregion
    }
}
