using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.NGHS.Utils;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class SobekControllersTriggersImporter : PartialSobekImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekControllersTriggersImporter));
        private IDictionary<string, SobekTrigger> sobekTriggers;
        private IDictionary<string, SobekController> sobekControllers;

        private const string displayName = "Controllers and triggers (conditions and rules)";
        public override string DisplayName
        {
            get { return displayName; }
        }

        public override SobekImporterCategories Category { get; } = SobekImporterCategories.Rtc;

        protected override void PartialImport()
        {
            log.DebugFormat("Importing Controllers and Triggers ...");
            var waterFlowModel1D = GetModel<WaterFlowFMModel>();

            RealTimeControlModel realTimeControlModel;

            try
            {
                realTimeControlModel = RealTimeControlModel;
            }
            catch
            {
                log.WarnFormat("To object is not a RealTimeControlModel. Importing controllers and triggers has been skipped");
                return;
            }

            var controlledStructures = ImportControlledStructures();

            if (controlledStructures.Count == 0)
            {
                //No controllers and triggers, just a plain WaterFlowModel1D model
                log.WarnFormat("No controllers and triggers are available.");
                return;
            }

            sobekControllers = ImportControllers(waterFlowModel1D.TimeStep, SobekType == DeltaShell.Sobek.Readers.SobekType.Sobek212);
            sobekTriggers = ImportTriggers();

            var structureLookup = HydroNetwork.Structures.ToDictionaryWithDuplicateLogging(nameof(HydroNetwork.Structures),s => s.Name, v => v, comparer:StringComparer.InvariantCultureIgnoreCase);

            foreach (var sobekStructureMapping in controlledStructures)
            {
                if (!structureLookup.TryGetValue(sobekStructureMapping.StructureId, out var structure) || structure == null)
                {
                    log.ErrorFormat("Unable to link controlled structure {0} to imported structure in model; skipped.", sobekStructureMapping.StructureId);
                }

                ControlGroupBuilder.CreateControlGroupForStructureAndAddToRtcModel(sobekStructureMapping,
                                                                                      structure,
                                                                                      waterFlowModel1D,
                                                                                      realTimeControlModel,
                                                                                      sobekControllers, sobekTriggers);
            }
        }

        private RealTimeControlModel RealTimeControlModel
        {
            get
            {
                var rtc = TargetObject as RealTimeControlModel;
                if (rtc != null)
                {
                    return rtc;
                }
                var comp = TargetObject as ICompositeActivity;
                if (comp != null)
                {
                    rtc = comp.Activities.OfType<RealTimeControlModel>().FirstOrDefault();
                    if (rtc != null)
                    {
                        return rtc;
                    }
                }

                if (TargetObject is Project)
                {
                    return ((Project)TargetObject).RootFolder.GetAllModelsRecursive().OfType<RealTimeControlModel>().FirstOrDefault();
                }

                throw new NotSupportedException("Rtc not found in TargetObject");
            }
        }

        private IList<SobekStructureMapping> ImportControlledStructures()
        {
            var path = GetFilePath(SobekFileNames.SobekStructuresFileName);
            if (!File.Exists(path))
            {
                log.ErrorFormat("File {0} doesn't exist. Reading controlled structures has been skipped...");
                return new List<SobekStructureMapping>();
            }

            var structures = new SobekStructureDatFileReader().Read(path);
            return structures.Where(s => s.ControllerIDs != null && s.ControllerIDs.Count > 0).ToList();
        }

        private IDictionary<string, SobekTrigger> ImportTriggers()
        {
            var path = GetFilePath(SobekFileNames.SobekTriggersFileName);
            if (!File.Exists(path))
            {
                log.ErrorFormat("File {0} doesn't exist. Reading triggers has been skipped...");
                return new Dictionary<string, SobekTrigger>();
            }

            var triggers = new SobekTriggerReader().Read(path);
            return triggers.ToDictionaryWithErrorDetails(path, t => t.Id);
        }

        private IDictionary<string, SobekController> ImportControllers(TimeSpan timeStep, bool sobek212Import)
        {

            var path = GetFilePath(SobekFileNames.SobekControllersFileName);
            if (!File.Exists(path))
            {
                log.ErrorFormat("File {0} doesn't exist. Reading controllers has been skipped...");
                return new Dictionary<string, SobekController>();
            }

            SobekControllerReader.Sobek2Import = sobek212Import;
            SobekControllerReader.TimeStepModel = timeStep;

            var controllers = new SobekControllerReader().Read(path);

            Dictionary<string, SobekController> dicControllers = new Dictionary<string, SobekController>();
            foreach (var sobekController in controllers)
            {
                if (!dicControllers.ContainsKey(sobekController.Id))
                {
                    dicControllers.Add(sobekController.Id, sobekController);
                }
            }
            return dicControllers;
        }

    }
}

