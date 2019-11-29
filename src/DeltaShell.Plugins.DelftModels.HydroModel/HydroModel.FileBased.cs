using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.HydroModel.ModelExchanges;

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
        /// Unlink data items and remember them in linkInfos.
        /// Can be restored after saving to the database with <see cref="RelinkDataItems"/>.
        /// </summary>
        public virtual void UnlinkAndRememberDataItems(bool unlink = true)
        {
            if (!TryGetFmAndRtcModel(out var flowModel, out var rtcModel)) 
                return;

            modelExchangeInfos.Clear();
            modelExchangeInfos.AddRange(GetExchangeInfo(flowModel, rtcModel, unlink));
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

                    // link:
                    targetItem.LinkTo(sourceItem);
                }
            }
        }

        public virtual string CouplingFilePath
        {
            get { return couplingFilePath; }
        }

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
            return model is IFileBased
                ? model.GetChildDataItemLocations(role).SelectMany(model.GetChildDataItems)
                : model.AllDataItems.Where(di => (di.Role & role) == role);
        }

        /// <summary>
        /// finds the currently linked dataitems of the models, stores them in exhangeInfo, and when unlink is
        /// true, the linkage is broken. Currently only used for flowfm + rtc
        /// </summary>
        private static IEnumerable<ModelExchangeInfo> GetExchangeInfo(IModel flowModel, IModel rtcModel, bool unlink)
        {
            yield return GetModelExchangeInfo(rtcModel, flowModel, unlink);
            yield return GetModelExchangeInfo(flowModel, rtcModel, unlink);
        }

        private static ModelExchangeInfo GetModelExchangeInfo(IModel targetModel, IModel sourceModel, bool unlink)
        {
            var targetInputItems = GetDataItems(targetModel, DataItemRole.Input).ToList();
            var sourceOutputItems = GetDataItems(sourceModel, DataItemRole.Output).ToList();
            
            var exchangeInfo = new ModelExchangeInfo(sourceModel, targetModel);

            foreach (var sourceOutputItem in sourceOutputItems)
            {
                foreach (var targetInputItem in targetInputItems.Where(i => i.LinkedTo != null && i.LinkedTo.Equals(sourceOutputItem)))
                {
                    exchangeInfo.Exchanges.Add(new ModelExchange(sourceOutputItem, targetInputItem));

                    if (unlink)
                    {
                        targetInputItem.Unlink();
                    }
                }
            }

            return exchangeInfo;
        }

        #endregion RTC-FlowFM coupling

        #region Logic for Model saving, loading, and copying

        private string couplingFilePath;

        private void OnSave()
        {
            ModelSave();
        }

        private void OnAddedToProject(string filePath)
        {
            if (couplingFilePath == null)
            {
                couplingFilePath = filePath;
            }

            ModelSaveTo(filePath);
        }

        private void OnCopyTo(string filePath)
        {
            ModelSaveTo(filePath);
        }

        private void OnSwitchTo(string filePath)
        {
            if (couplingFilePath == null)
            {
                modelExchangeInfos.ReadFromJson(filePath);
            }

            couplingFilePath = filePath;
        }

        private void ModelSave()
        {
            ModelSaveTo(couplingFilePath);
        }

        private void ModelSaveTo(string filePath)
        {
            FileUtils.CreateDirectoryIfNotExists(Path.GetDirectoryName(filePath));

            // write exchanges
            modelExchangeInfos.WriteToJson(filePath);
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
            OnAddedToProject(GetJsonPathFromDeltaShellPath(path, Name));

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
            OnCopyTo(GetJsonPathFromDeltaShellPath(destinationPath, Name));
        }

        void IFileBased.SwitchTo(string newPath)
        {
            path = newPath;
            OnSwitchTo(GetJsonPathFromDeltaShellPath(newPath, Name));
            IsOpen = true;
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
    }
}
