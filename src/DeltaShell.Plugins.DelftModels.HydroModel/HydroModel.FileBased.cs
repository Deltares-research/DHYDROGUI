using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.Common;

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
        /// </summary>
        public virtual void SaveLinks()
        {
            if (TryGetFmAndRtcModel(out IModel flowModel, out IModel rtcModel))
            {
                modelExchangeInfos.Clear();
                modelExchangeInfos.AddRange(GetModelExchangeInfos(flowModel, rtcModel));
            }
        }

        /// <summary>
        /// Unlink data items and remember them in linkInfos.
        /// Can be restored after saving to the database with <see cref="RelinkDataItems"/>.
        /// </summary>
        public virtual void UnlinkAndRememberDataItems()
        {
            if (TryGetFmAndRtcModel(out IModel flowModel, out IModel rtcModel))
            {
                modelExchangeInfos.Clear();
                modelExchangeInfos.AddRange(GetUnlinkedModelExchangeInfos(flowModel, rtcModel));
            }
        }

        /// <summary>
        /// re-establish the data links that were cut when the model was saved
        /// Can only be used once linkInfos is filled.
        /// </summary>
        public virtual void RelinkDataItems() =>
            modelExchangeInfos.ForEach(RelinkDataItems);

        private void RelinkDataItems(ModelExchangeInfo info)
        {
            ICoupledModel sourceModel = GetCoupledModel(info.SourceModelName);
            IDictionary<string, IDataItem> sourceItems =
                GetDataItemsUsedForCouplingModel(sourceModel, DataItemRole.Output);

            if (sourceItems == null || !sourceItems.Any())
            {
                return;
            }

            ICoupledModel targetModel = GetCoupledModel(info.TargetModelName);
            IDictionary<string, IDataItem> targetItems =
                GetDataItemsUsedForCouplingModel(targetModel, DataItemRole.Input);

            if (targetItems == null || !targetItems.Any())
            {
                return;
            }

            System.Tuple<bool, bool> sourceStatus = GetModelOutputEventStatus(sourceModel as IModel);
            System.Tuple<bool, bool> targetStatus = GetModelOutputEventStatus(targetModel as IModel);
            SuspendModelOutputEvents(sourceModel as IModel);
            SuspendModelOutputEvents(targetModel as IModel);

            foreach (ModelExchange modelExchange in info.Exchanges)
            {
                RelinkDataItemsInModelExchange(modelExchange,
                                               sourceItems,
                                               targetItems,
                                               targetModel);
            }

            UnsuspendModelOutputEvents(sourceModel as IModel, sourceStatus);
            UnsuspendModelOutputEvents(targetModel as IModel, targetStatus);
        }

        private static void RelinkDataItemsInModelExchange(ModelExchange exchange,
                                                           IDictionary<string, IDataItem> sourceItems,
                                                           IDictionary<string, IDataItem> targetItems,
                                                           ICoupledModel targetModel)
        {
            if (!sourceItems.TryGetValue(exchange.SourceName, out IDataItem sourceItem))
            {
                return;
            }

            string targetName = targetModel.GetUpToDateDataItemName(exchange.TargetName);
            if (!targetItems.TryGetValue(targetName, out IDataItem targetItem))
            {
                return;
            }

            targetItem.LinkTo(sourceItem);
        }

        private IDictionary<string, IDataItem> GetDataItemsUsedForCouplingModel(ICoupledModel model, DataItemRole role) =>
            model?.GetDataItemsUsedForCouplingModel(role)
                 .Where(di => di != null)
                 .ToDictionary(ModelExchange.GetExchangeIdentifier);

        private ICoupledModel GetCoupledModel(string modelName) =>
            Models.FirstOrDefault(m => Equals(ModelExchangeInfo.GetModelIdentifier(m), modelName)) as ICoupledModel;

        private static System.Tuple<bool, bool> GetModelOutputEventStatus(IModel model)
        {
            return new System.Tuple<bool, bool>(model.SuspendClearOutputOnInputChange, model.SuspendMarkOutputOutOfSyncOnInputChange);
        }

        private static void SuspendModelOutputEvents(IModel model)
        {
            model.SuspendClearOutputOnInputChange = true;
            model.SuspendMarkOutputOutOfSyncOnInputChange = true;
        }

        private static void UnsuspendModelOutputEvents(IModel model, System.Tuple<bool, bool> targetStatus)
        {
            bool originalSuspendClearOutput = targetStatus.Item1;
            bool originalSuspendOutOfSync = targetStatus.Item2;
            model.SuspendClearOutputOnInputChange = originalSuspendClearOutput;
            model.SuspendMarkOutputOutOfSyncOnInputChange = originalSuspendOutOfSync;
        }

        /// <summary>
        /// Load links when building model from file
        /// </summary>
        public virtual void LoadLinks()
        {
            modelExchangeInfos.Clear();
            modelExchangeInfos.AddRange(couplingFile.Read(couplingFile.FilePath));
        }

        private bool TryGetFmAndRtcModel(out IModel flowModel, out IModel rtcModel)
        {
            flowModel = null;
            rtcModel = null;

            foreach (IModel model in Models)
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

        public static IEnumerable<IDataItem> GetDataItemsUsedForCouplingModel(IModel model, DataItemRole role)
        {
            if (model is ICoupledModel coupledModel)
            {
                return coupledModel.GetDataItemsUsedForCouplingModel(role);
            }

            return Enumerable.Empty<IDataItem>();
        }

        /// <summary>
        /// Gets the collection of <see cref="ModelExchangeInfo"/> based on its input arguments.
        /// </summary>
        /// <param name="flowModel">The flow model to create the <see cref="ModelExchangeInfo"/> for.</param>
        /// <param name="rtcModel">The rtc model to create the <see cref="ModelExchangeInfo"/> for.</param>
        /// <returns>The collection of <see cref="ModelExchangeInfo"/>.</returns>
        private static IEnumerable<ModelExchangeInfo> GetModelExchangeInfos(IModel flowModel, IModel rtcModel)
        {
            return new[]
            {
                CreateModelExchangeInfo(flowModel, rtcModel),
                CreateModelExchangeInfo(rtcModel, flowModel)
            };
        }

        /// <summary>
        /// Creates a <see cref="ModelExchangeInfo"/>.
        /// </summary>
        /// <param name="inputModel">The <see cref="IModel"/> that serves as an input.</param>
        /// <param name="outputModel">The <see cref="IModel"/> that serves as an output.</param>
        /// <returns>A <see cref="ModelExchangeInfo"/>.</returns>
        private static ModelExchangeInfo CreateModelExchangeInfo(IModel inputModel, IModel outputModel)
        {
            var modelExchange = new ModelExchangeInfo(outputModel, inputModel);

            IEnumerable<IDataItem> inputDataItems = GetDataItemsUsedForCouplingModel(inputModel, DataItemRole.Input);
            IEnumerable<IDataItem> outputDataItems = GetDataItemsUsedForCouplingModel(outputModel, DataItemRole.Output);
            foreach (IDataItem linkedDataItem in GetLinkedDataInputItems(inputDataItems, outputDataItems))
            {
                modelExchange.Exchanges.Add(new ModelExchange(linkedDataItem.LinkedTo, linkedDataItem));
            }

            return modelExchange;
        }

        /// <summary>
        /// Gets the collection of <see cref="ModelExchangeInfo"/> based on its input arguments while breaking the linkage.
        /// </summary>
        /// <param name="flowModel">The flow model to create the <see cref="ModelExchangeInfo"/> for.</param>
        /// <param name="rtcModel">The rtc model to create the <see cref="ModelExchangeInfo"/> for.</param>
        /// <returns>The collection of <see cref="ModelExchangeInfo"/>.</returns>
        private static IEnumerable<ModelExchangeInfo> GetUnlinkedModelExchangeInfos(IModel flowModel, IModel rtcModel)
        {
            // Create the name mappings of the RTC components. The name will be temporarily set to their
            // default values when being unlinked during the save operation.
            IReadOnlyDictionary<IDataItem, string> rtcInputNameMapping = GetDataItemNameMapping(GetDataItemsUsedForCouplingModel(rtcModel, DataItemRole.Input));
            IReadOnlyDictionary<IDataItem, string> rtcOutputNameMapping = GetDataItemNameMapping(GetDataItemsUsedForCouplingModel(rtcModel, DataItemRole.Output));

            return new[]
            {
                CreateUnlinkedModelExchangeInfo(rtcModel, flowModel, rtcInputNameMapping, item => item),
                CreateUnlinkedModelExchangeInfo(flowModel, rtcModel, rtcOutputNameMapping, item => item.LinkedTo)
            };
        }

        /// <summary>
        /// Creates a <see cref="ModelExchangeInfo"/> while unlinking the data items of the <paramref name="inputModel"/>.
        /// </summary>
        /// <param name="inputModel">The <see cref="IModel"/> that serves as an input.</param>
        /// <param name="outputModel">The <see cref="IModel"/> that serves as an output.</param>
        /// <param name="dataItemNameMapping">The mapping between the <see cref="IDataItem"/> and its original name.</param>
        /// <param name="getDataItemFunc">The function to retrieve the data item to restore the name for.</param>
        /// <returns>A <see cref="ModelExchangeInfo"/>.</returns>
        private static ModelExchangeInfo CreateUnlinkedModelExchangeInfo(IModel inputModel, IModel outputModel,
                                                                         IReadOnlyDictionary<IDataItem, string> dataItemNameMapping,
                                                                         Func<IDataItem, IDataItem> getDataItemFunc)
        {
            System.Tuple<bool, bool> inputStatus = GetModelOutputEventStatus(inputModel);
            System.Tuple<bool, bool> outputStatus = GetModelOutputEventStatus(outputModel);
            SuspendModelOutputEvents(inputModel);
            SuspendModelOutputEvents(outputModel);

            var modelExchange = new ModelExchangeInfo(outputModel, inputModel);

            IEnumerable<IDataItem> inputDataItems = GetDataItemsUsedForCouplingModel(inputModel, DataItemRole.Input);
            IEnumerable<IDataItem> outputDataItems = GetDataItemsUsedForCouplingModel(outputModel, DataItemRole.Output);

            // Cache the linked data input items as due to the unlinking, changes might occur that affects
            // the collection of linked objects.
            IEnumerable<IDataItem> linkedDataInputItems = GetLinkedDataInputItems(inputDataItems, outputDataItems).ToArray();

            foreach (IDataItem linkedInputDataItem in linkedDataInputItems)
            {
                IDataItem dataItemToRestore = getDataItemFunc(linkedInputDataItem);
                modelExchange.Exchanges.Add(new ModelExchange(linkedInputDataItem.LinkedTo, linkedInputDataItem));
                linkedInputDataItem.Unlink();

                // Restore the name of the unlinked data item to its original value to prevent 
                // it from appearing as its default value during saving.
                RestoreDataItemName(dataItemNameMapping, dataItemToRestore);
            }

            UnsuspendModelOutputEvents(inputModel, inputStatus);
            UnsuspendModelOutputEvents(outputModel, outputStatus);

            return modelExchange;
        }

        /// <summary>
        /// Creates a <see cref="HashSet{T}"/> based on its input arguments.
        /// </summary>
        /// <param name="dataItems">The collection of <see cref="IDataItem"/> to create the set for.</param>
        /// <returns>A <see cref="HashSet{T}"/>.</returns>
        private static HashSet<IDataItem> CreateHashSet(IEnumerable<IDataItem> dataItems)
        {
            var set = new HashSet<IDataItem>();
            foreach (IDataItem dataItem in dataItems)
            {
                set.Add(dataItem);
            }

            return set;
        }

        /// <summary>
        /// Gets the collection of data input items that are linked to output data items.
        /// </summary>
        /// <param name="inputItems">The collection of input items to determine whether they are linked.</param>
        /// <param name="outputItems">
        /// The collection of output items that the <paramref name="inputItems"/>
        /// could be linked with.
        /// </param>
        /// <returns>A collection of <paramref name="inputItems"/> that are linked.</returns>
        private static IEnumerable<IDataItem> GetLinkedDataInputItems(IEnumerable<IDataItem> inputItems, IEnumerable<IDataItem> outputItems)
        {
            HashSet<IDataItem> outputSet = CreateHashSet(outputItems);
            return inputItems.Where(item => item.LinkedTo != null && outputSet.Contains(item.LinkedTo));
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
                var connectionPoint = (INameable)item.ValueConverter.OriginalValue;
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
                var originalValue = (INameable)dataItem.ValueConverter.OriginalValue;
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
            string targetDir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            // write exchanges
            couplingFile.Write(filePath, modelExchangeInfos);
            if (switchTo)
            {
                couplingFile.FilePath = filePath;
            }
        }

        #endregion

        #region IFileBased and NHibernate

        //tells NHibernate we need to be saved
        private void MarkDirty()
        {
            unchecked
            {
                dirtyCounter++;
            }
        }

        private int dirtyCounter;

        private string path;

        string IFileBased.Path
        {
            get
            {
                return path;
            }
            set
            {
                if (path == value)
                {
                    return;
                }

                path = value;

                if (path == null)
                {
                    return;
                }

                if (path.StartsWith("$") && IsOpen)
                {
                    OnSave();
                }
            }
        }

        public virtual IEnumerable<string> Paths
        {
            get
            {
                yield return ((IFileBased)this).Path;
            }
        }

        public virtual bool IsFileCritical
        {
            get
            {
                return true;
            }
        }

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
            // Nothing to be done, enforced through IFileBased
        }

        private string GetJSONPathFromDeltaShellPath(string dsPath)
        {
            // dsproj_data/<model name>/<model name>.mdw
            return Path.Combine(Path.GetDirectoryName(dsPath), Path.Combine(Name, Name + ".json"));
        }

        #endregion
    }
}