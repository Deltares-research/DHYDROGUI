using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.Dimr;
using DeltaShell.Dimr.DimrXsd;
using DeltaShell.NGHS.Utils;
using DeltaShell.NGHS.Utils.Extensions;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Services;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.NGHS.Common;
using DeltaShell.NGHS.Common.Extensions;
using DeltaShell.Plugins.DelftModels.HydroModel.Properties;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Import
{
    /// <summary>
    /// Converts the components information from a dimr file into sub-models of the Integrated model.
    /// </summary>
    public class HydroModelConverter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(HydroModelConverter));

        private readonly ILogHandler logHandler;
        private readonly IFileImportService fileImportService;

        public HydroModelConverter(ILogHandler logHandler, IFileImportService fileImportService)
        {
            this.logHandler = logHandler;
            this.fileImportService = fileImportService;
        }

        /// <summary>
        /// Converts <see cref="dimrObject"/> to a <see cref="HydroModel"/> using the file importers.
        /// </summary>
        /// <param name="dimrObject">Parsed <see cref="dimrXML"/> object</param>
        /// <param name="path">Path to dimr.xml (used for finding sub-model folders)</param>
        /// <param name="reportProgress">String to feedback to importer what importers are working on.</param>
        /// <returns>Converted <see cref="HydroModel"/></returns>
        /// <exception cref="ArgumentException">
        /// When
        /// <param name="dimrObject"/>
        /// is null
        /// </exception>
        public HydroModel Convert(dimrXML dimrObject, string path, Action<string> reportProgress = null)
        {
            if (dimrObject == null)
            {
                throw new ArgumentException("Cannot convert empty dimr data object.");
            }

            string rootFolder = Path.GetDirectoryName(path);
            var hydroModel = new HydroModel();

            using (hydroModel.InEditMode("Importing full Dimr model"))
            {
                reportProgress?.Invoke(Resources.HydroModelConverter_Convert_importing_on_hydromodel);
                hydroModel.DoWithPropertySet(nameof(hydroModel.SuspendClearOutputOnInputChange), true, () =>
                {
                    AddModels(hydroModel, dimrObject, rootFolder, reportProgress);

                    if (dimrObject.coupler == null)
                    {
                        return;
                    }

                    List<IDimrModel> subModels = hydroModel.Activities.OfType<IDimrModel>().ToList();
                    CoupleSubModels(hydroModel, dimrObject, subModels);
                });
            }

            return hydroModel;
        }

        private void AddModels(HydroModel hydroModel, dimrXML dimrObject, string rootFolder, Action<string> progressChanged = null)
        {
            foreach (dimrComponentXML component in dimrObject.component)
            {
                ValidateComponent(component);
            }

            IEnumerable<IGrouping<string, dimrComponentXML>> componentGroups = dimrObject.component.GroupBy(component => component.inputFile);

            foreach (IGrouping<string, dimrComponentXML> componentGroup in componentGroups)
            {
                IDimrModelFileImporter importer = GetFileImporterFor(componentGroup.Key);
                if (importer == null)
                {
                    logHandler.ReportError($"No importer found for input file: {componentGroup.Key}");
                    continue;
                }

                importer.ProgressChanged = (name, step, steps) => { progressChanged?.Invoke("importing for "+importer.Name+Environment.NewLine+name);};

                foreach (dimrComponentXML component in componentGroup)
                {
                    string workDir = component.workingDir.Trim();
                    string inputFile = component.inputFile.Trim();
                    string filePath = Path.Combine(rootFolder, workDir, inputFile);

                    object importedItem = importer.ImportItem(filePath);

                    if (!(importedItem is IActivity subModel))
                    {
                        logHandler.ReportErrorFormat("Could not import sub model defined at location {0} to integrated model.", filePath);
                        continue;
                    }

                    RenameSubModelWhenNeeded(subModel, component.name);
                    SetHydroModelProperties(hydroModel, subModel);
                }
            }
        }
        
        private IDimrModelFileImporter GetFileImporterFor(string inputFile)
        {
            return fileImportService.FileImporters
                                    .OfType<IDimrModelFileImporter>()
                                    .FirstOrDefault(importer => importer.CanImportDimrFile(inputFile));
        }

        private static void ValidateComponent(dimrComponentXML component)
        {
            if (string.IsNullOrEmpty(component.workingDir))
            {
                throw new ArgumentException($"The working directory is missing for component {component.name} in the dimr xml.");
            }

            if (component.inputFile == null)
            {
                throw new ArgumentException($"The input file is missing for component {component.name} in the dimr xml.");
            }
        }

        private void RenameSubModelWhenNeeded(IActivity subModel, string componentName)
        {
            if (subModel.Name == componentName)
            {
                return;
            }

            log.DebugFormat("Renamed model {0} to {1}.", subModel.Name, componentName);
            subModel.Name = componentName;
        }

        private void SetHydroModelProperties(HydroModel hydroModel, IActivity subModel)
        {
            if (!(subModel is IHydroModel sourceModel) || sourceModel.Region == null)
            {
                hydroModel.Activities.Add(subModel);
                return;
            }
            sourceModel.ReplaceHydroModelRegion(hydroModel);
        }

        private void CoupleSubModels(HydroModel hydroModel, dimrXML dimrObject, IList<IDimrModel> subModels)
        {
            foreach (dimrCouplerXML dimrCouplerXml in dimrObject.coupler)
            {
                IDimrModel sourceModel = subModels.FirstOrDefault(m => m.Name.EqualsCaseInsensitive(dimrCouplerXml.sourceComponent));
                IDimrModel targetModel = subModels.FirstOrDefault(m => m.Name.EqualsCaseInsensitive(dimrCouplerXml.targetComponent));

                if (sourceModel == null || targetModel == null)
                {
                    logHandler.ReportErrorFormat("Could not couple models: \'{0}\' to \'{1}\'.",
                                                 dimrCouplerXml.sourceComponent, dimrCouplerXml.targetComponent);
                    continue;
                }

                DoWithSuspendOutputChecks(new []{sourceModel, targetModel}, () =>
                {
                    CoupleModelsUsingDimrCouplerXml(sourceModel, targetModel, dimrCouplerXml.item, hydroModel.Region.Links);
                });
            }

            foreach (IControllingModel controllingModel in subModels.OfType<IControllingModel>())
            {
                controllingModel.CleanUpModelAfterModelCoupling();
            }
        }

        private static void DoWithSuspendOutputChecks(IEnumerable<IModel> models, Action action)
        {
            var properties = new[]
            {
                nameof(IModel.SuspendClearOutputOnInputChange),
                nameof(IModel.SuspendMarkOutputOutOfSyncOnInputChange)
            };

            var resultAction = action;
            foreach (IModel model in models)
            {
                foreach (var property in properties)
                {
                    Action tempAction = resultAction;
                    resultAction = () => model.DoWithPropertySet(property, true, () => tempAction());
                }
            }

            resultAction();
        }

        private void CoupleModelsUsingDimrCouplerXml(IDimrModel sourceModel, IDimrModel targetModel, IEnumerable<dimrCoupledItemXML> dimrCouplerXml, IEnumerable<HydroLink> regionLinks)
        {
            var linkBySourceIemLookup = regionLinks
                                                  .GroupBy(l => l.Source)
                                                  .ToDictionary(l => l.Key, l => l.ToArray());

           sourceModel.DimrCoupling?.Prepare();
           targetModel.DimrCoupling?.Prepare();
            
            foreach (dimrCoupledItemXML couplerXml in dimrCouplerXml)
            {
                try
                {
                    if (string.IsNullOrEmpty(couplerXml.sourceName) || 
                        string.IsNullOrEmpty(couplerXml.targetName))
                    {
                        logHandler.ReportErrorFormat("Could not link an item from {0} to {1}", sourceModel.Name, targetModel.Name);
                        continue;
                    }

                    // DataItem linking
                    IDataItem sourceDataItem = sourceModel.GetDataItemsByItemString(couplerXml.sourceName, couplerXml.targetName)?.FirstOrDefault();
                    if (sourceDataItem != null)
                    {
                        IEnumerable<IDataItem> targetDataItems = targetModel.GetDataItemsByItemString(couplerXml.targetName, couplerXml.sourceName);
                        if (targetDataItems != null)
                        {
                            foreach (IDataItem targetDataItem in targetDataItems)
                            {
                                targetDataItem.LinkTo(sourceDataItem);
                            }
                            continue;
                        }
                    }

                    // HydroObject linking
                    IList<IHydroObject> sourceItems = sourceModel.DimrCoupling?.GetLinkHydroObjectsByItemString(couplerXml.sourceName).ToList();
                    if (sourceItems == null || !sourceItems.Any())
                    {
                        logHandler.ReportErrorFormat($"Model {sourceModel.ShortName} does not contain source item {couplerXml.sourceName} (to be linked to {couplerXml.targetName})");
                        continue;
                    }

                    IList<IHydroObject> targetItems = targetModel.DimrCoupling?.GetLinkHydroObjectsByItemString(couplerXml.targetName).ToList();
                    if (targetItems == null || !targetItems.Any())
                    {
                        logHandler.ReportErrorFormat($"Model {targetModel.ShortName} does not contain target item {couplerXml.targetName} (to be linked from {couplerXml.sourceName})");
                        continue;
                    }

                    foreach (IHydroObject sourceItem in sourceItems)
                    {
                        foreach (IHydroObject targetItem in targetItems)
                        {
                            var linkAlreadyPresent = linkBySourceIemLookup.TryGetValue(sourceItem, out var links) &&
                                                     links.Any(l => l.Target == targetItem);

                            if (!linkAlreadyPresent)
                            {
                                sourceModel.DimrCoupling.CreateLink(sourceItem, targetItem);
                            }
                        }
                    }
                }
                catch (Exception e) when (e is NotSupportedException ||
                                          e is ArgumentException)
                {
                    logHandler.ReportError($"Could not link {couplerXml.sourceName} to {couplerXml.targetName}: {e.Message}");
                }
            }
            
            sourceModel.DimrCoupling?.End();
            targetModel.DimrCoupling?.End();
        }
    }
}