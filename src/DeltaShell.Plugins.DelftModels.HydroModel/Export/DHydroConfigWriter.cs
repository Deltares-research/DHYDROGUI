using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.HydroModel.Properties;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Export
{
    public class DHydroConfigWriter
    {
        private const string Encoding = "UTF-8";
        private const string Documentation = "documentation";
        private const string TeamName = "Deltares, Coupling Team";
        private const string FileVersion = "1.2";
        private const string SchemaLocation = "http://content.oss.deltares.nl/schemas/dimr-1.2.xsd";

        private static readonly XNamespace Xsi = "http://www.w3.org/2001/XMLSchema-instance";
        private static readonly XNamespace DHyd = "http://schemas.deltares.nl/dimr";

        private static readonly XName RootName = "dimrConfig";
        private const string defaultNumThreadsKey = "threads";
        private const int defaultNumThreads = 1;

        public DHydroConfigWriter()
        {
            modelCouplers = new List<IDimrConfigModelCoupler>();
            CoreCountDictionary = new Dictionary<IDimrModel, int>();

            //Create dictionary of parents.
            CouplerModelsDictionary = new Dictionary<IModel, IDimrModel>();
        }

        public List<IDimrConfigModelCoupler> modelCouplers { get; private set; }
        public IDictionary<IModel, IDimrModel> CouplerModelsDictionary { get; private set; }

        public IDictionary<IDimrModel, int> CoreCountDictionary { private get; set; }

        public XDocument CreateConfigDocument(ICompositeActivity workFlow)
        {
            if (!workFlow.Activities.Any())
            {
                throw new NotImplementedException(Resources.DHydroConfigWriter_CreateConfigDocument_Empty_model_cannot_generate_a_configuration_file_);
            }

            var xDocument = new XDocument {Declaration = new XDeclaration("1.0", Encoding, "yes")};
            XElement rootNode = CreateRootNode();
            xDocument.Add(rootNode);
            modelCouplers.Clear();
            rootNode.Add(CreateControlNode(workFlow));

            List<IDimrModel> allDHydroActivities = workFlow.GetAllActivitiesRecursive<IActivity>().Select(UnwrapActivity).OfType<IDimrModel>().ToList();

            foreach (IDimrModel dHydroActivity in allDHydroActivities)
            {
                CoreCountDictionary.TryGetValue(dHydroActivity, out int nodeCount);
                rootNode.Add(CreateComponentNode(dHydroActivity, nodeCount));
            }

            foreach (IDimrConfigModelCoupler modelCoupler in modelCouplers)
            {
                rootNode.Add(CreateCouplerNode(modelCoupler));
            }

            return xDocument;
        }

        private static XElement CreateRootNode()
        {
            var root = new XElement(DHyd + RootName.LocalName);
            root.Add(new XAttribute("xmlns", DHyd.NamespaceName));
            root.Add(new XAttribute(XNamespace.Xmlns + "xsi", Xsi.NamespaceName));
            root.Add(new XAttribute(Xsi + "schemaLocation", DHyd.NamespaceName + " " + SchemaLocation));
            var docElement = new XElement(DHyd + Documentation);
            docElement.Add(new XElement(DHyd + "fileVersion", FileVersion));
            docElement.Add(new XElement(DHyd + "createdBy", TeamName));
            docElement.Add(new XElement(DHyd + "creationDate", DateTime.UtcNow));
            root.Add(docElement);
            root.Add(new XComment(Resources.DHydroConfigDescription));
            return root;
        }

        private XElement CreateComponentNode(IDimrModel dimrModel, int numCores)
        {
            var component = new XElement(DHyd + "component");
            component.Add(new XAttribute("name", dimrModel.Name));
            component.Add(new XElement(DHyd + "library", dimrModel.LibraryName));
            if (numCores > 0)
            {
                component.Add(new XElement(DHyd + "process",
                                           string.Join(" ",
                                                       Enumerable.Range(0, numCores).Select(i => i.ToString(CultureInfo.InvariantCulture)))));
            }
            if (dimrModel.LibraryName.Equals("dflowfm"))
            {
                XElement setting = new XElement(DHyd + "setting");
                setting.Add(new XAttribute("key", defaultNumThreadsKey));
                setting.Add(new XAttribute("value", defaultNumThreads));
                component.Add(setting);
            }
            component.Add(new XElement(DHyd + "workingDir", dimrModel.DirectoryName));
            component.Add(new XElement(DHyd + "inputFile", dimrModel.InputFile));
            return component;
        }

        private XElement CreateControlNode(ICompositeActivity hydroModel)
        {
            var control = new XElement(DHyd + "control");
            if (hydroModel.CurrentWorkflow != null)
            {
                RecursivelyAddCompositeControlNodes(control, hydroModel.CurrentWorkflow);
            }

            return control;
        }

        private XElement CreateCouplerNode(IDimrConfigModelCoupler modelCoupler)
        {
            var coupler = new XElement(DHyd + "coupler");
            coupler.Add(new XAttribute("name", modelCoupler.Name));
            string sourceComponentName = modelCoupler.Source;
            string targetComponentName = modelCoupler.Target;
            coupler.Add(new XElement(DHyd + "sourceComponent", sourceComponentName));
            coupler.Add(new XElement(DHyd + "targetComponent", targetComponentName));

            foreach (DimrCoupleInfo coupleInfo in modelCoupler.CoupleInfos)
            {
                var itemNode = new XElement(DHyd + "item");
                var sourceNode = new XElement(DHyd + "sourceName", coupleInfo.Source);
                var targetNode = new XElement(DHyd + "targetName", coupleInfo.Target);
                itemNode.Add(sourceNode, targetNode);
                coupler.Add(itemNode);
            }

            if (modelCoupler.AddCouplerLoggerInfo)
            {
                var loggerNode = new XElement(DHyd + "logger");
                //set attributes.
                var workingDir = ".";
                var workingDirNode = new XElement(DHyd + "workingDir", workingDir);
                var outputFileNode = new XElement(DHyd + "outputFile", string.Concat(modelCoupler.Name, ".nc"));
                loggerNode.Add(workingDirNode, outputFileNode);
                coupler.Add(loggerNode);
            }

            return coupler;
        }

        private void CreateCouplers(ICompositeActivity compositeActivity)
        {
            List<ICompositeActivity> wrappedActivities = compositeActivity.GetActivitiesOfType<ICompositeActivity>().Where(ca => ca != compositeActivity).ToList();
            foreach (ICompositeActivity wrappedActivity in wrappedActivities)
            {
                IEnumerable<IActivity> unWrappedSubActivities = wrappedActivity.GetActivitiesOfType<IActivity>().Where(wa => wa != wrappedActivity);
                foreach (IActivity subActivity in unWrappedSubActivities)
                {
                    var sa = (IModel) subActivity;
                    var wa = (IDimrModel) wrappedActivity;
                    CouplerModelsDictionary.Add(sa, wa);
                }
            }

            List<IActivity> unWrappedActivities = compositeActivity.GetActivitiesOfType<IActivity>().Where(at => at != compositeActivity).ToList();
            foreach (IModel sourceModel in unWrappedActivities.OfType<IModel>())
            {
                foreach (IModel targetModel in unWrappedActivities.OfType<IModel>())
                {
                    if (Equals(sourceModel, targetModel))
                    {
                        continue;
                    }

                    /* Try to guess if it's a wrapped activity */
                    //Is the sourceModel a submodel?
                    IDimrModel wrapperTarget;
                    CouplerModelsDictionary.TryGetValue(targetModel, out wrapperTarget);
                    if (wrapperTarget != null)
                    {
                        //try to retrieve the already generated coupler from the coupler list
                        string couplerName = ((IDimrModel) sourceModel).ShortName + DimrConfigModelCouplerFactory.COUPLER_NAME_COMBINER + wrapperTarget.ShortName;
                        IDimrConfigModelCoupler wrapperCoupler = modelCouplers.FirstOrDefault(mc => mc.Name == couplerName);
                        if (wrapperCoupler != null)
                        {
                            //if retrieved, then update coupler
                            wrapperCoupler.UpdateModel(sourceModel, targetModel, null, wrapperTarget as CompositeActivity);
                            continue;
                        }
                    }

                    //Is the targetmodel a submodel?
                    IDimrModel wrapperSource;
                    CouplerModelsDictionary.TryGetValue(sourceModel, out wrapperSource);
                    if (wrapperSource != null)
                    {
                        //try to retrieve the already generated coupler from the coupler list
                        string couplerName = wrapperSource.ShortName + DimrConfigModelCouplerFactory.COUPLER_NAME_COMBINER + ((IDimrModel) targetModel).ShortName;
                        IDimrConfigModelCoupler wrapperCoupler = modelCouplers.FirstOrDefault(mc => mc.Name == couplerName);
                        if (wrapperCoupler != null)
                        {
                            //if retrieved, then update coupler
                            wrapperCoupler.UpdateModel(sourceModel, targetModel, wrapperSource as CompositeActivity, null);
                            continue;
                        }
                    }

                    /*** Generate a new coupler and add to list ***/
                    IDimrConfigModelCoupler modelCoupler = DimrConfigModelCouplerFactory.GetCouplerForModels(sourceModel, targetModel, wrapperSource as CompositeActivity, wrapperTarget as CompositeActivity);
                    if (!modelCoupler.CoupleInfos.Any())
                    {
                        continue;
                    }

                    string name = modelCoupler.Name;
                    int n = modelCouplers.Count(mc => mc.Name.StartsWith(name));
                    if (n > 0)
                    {
                        modelCoupler.Name = modelCoupler.Name + "_" + (n + 1);
                    }

                    modelCouplers.Add(modelCoupler);
                }
            }
        }

        private void RecursivelyAddControlNodes(XElement node, IActivity activity, DateTime? refTime)
        {
            var activityWrapper = activity as ActivityWrapper;
            if (activityWrapper != null)
            {
                RecursivelyAddControlNodes(node, ((ActivityWrapper) activity).Activity, refTime);
            }
            else
            {
                if (activity is ICompositeActivity compositeActivity && compositeActivity.Activities.Any())
                {
                    RecursivelyAddCompositeControlNodes(node, compositeActivity);
                }
                else
                {
                    if (activity is IDimrModel dHydroActivity)
                    {
                        if (dHydroActivity.IsMasterTimeStep)
                        {
                            var auxModel = activity as IModel;
                            /* If it's nested it will be in the dictonary and we don't need to include it in here */
                            if (auxModel == null || !CouplerModelsDictionary.ContainsKey(auxModel))
                            {
                                var startElement = new XElement(DHyd + "start");
                                startElement.Add(new XAttribute("name", dHydroActivity.Name));
                                node.Add(startElement);
                            }
                        }
                        else
                        {
                            var groupElement = new XElement(DHyd + "startGroup");
                            if (activity is ITimeDependentModel timeDependentModel && refTime != null)
                            {
                                var timeElement = new XElement(DHyd + "time");
                                double startTime = (timeDependentModel.StartTime - refTime.Value).TotalSeconds;
                                double stopTime = (timeDependentModel.StopTime - refTime.Value).TotalSeconds;
                                double timeStep = timeDependentModel.TimeStep.TotalSeconds;
                                timeElement.Add(string.Join(" ", new[]
                                {
                                    startTime,
                                    timeStep,
                                    stopTime
                                }));
                                groupElement.Add(timeElement);
                            }

                            foreach (IDimrConfigModelCoupler modelCoupler in modelCouplers.Where(mc => Equals(mc.Target, activity.Name)))
                            {
                                if (modelCoupler.SourceIsMasterTimeStep)
                                {
                                    var couplerElement = new XElement(DHyd + "coupler");
                                    couplerElement.Add(new XAttribute("name", modelCoupler.Name));
                                    groupElement.Add(couplerElement);
                                }
                            }

                            var startElement = new XElement(DHyd + "start");
                            startElement.Add(new XAttribute("name", dHydroActivity.Name));
                            groupElement.Add(startElement);
                            foreach (IDimrConfigModelCoupler modelCoupler in modelCouplers.Where(mc => Equals(mc.Source, activity.Name)))
                            {
                                var couplerElement = new XElement(DHyd + "coupler");
                                couplerElement.Add(new XAttribute("name", modelCoupler.Name));
                                groupElement.Add(couplerElement);
                            }

                            node.Add(groupElement);
                        }
                    }
                    else
                    {
                        throw new NotImplementedException("Activity of type " + activity.GetType() +
                                                          " cannot be serialized to xml.");
                    }
                }
            }
        }

        private void RecursivelyAddCompositeControlNodes(XElement control, ICompositeActivity compositeActivity)
        {
            if (compositeActivity is SequentialActivity || compositeActivity is ParallelActivity && compositeActivity.GetActivitiesOfType<IActivity>().Count(at => at != compositeActivity) == 1)
            {
                foreach (IActivity activity in compositeActivity.Activities)
                {
                    RecursivelyAddControlNodes(control, activity, null);
                }
            }
            else if (compositeActivity is ParallelActivity)
            {
                List<IActivity> unWrappedActivities = compositeActivity.GetActivitiesOfType<IActivity>().Where(at => at != compositeActivity).ToList();
                if (!unWrappedActivities.Any())
                {
                    return;
                }

                CreateCouplers(compositeActivity);
                var parallelBlock = new XElement(DHyd + "parallel");

                var masterActivity =
                    unWrappedActivities.OfType<ITimeDependentModel>().OfType<IDimrModel>().FirstOrDefault(a => a.IsMasterTimeStep) as
                        ITimeDependentModel; /*Only needed for the start time*/
                if (masterActivity == null)
                {
                    throw new NotImplementedException("Workflows without a master model cannot be serialized to d_hydro xml.");
                }

                List<IActivity> orderedActivities = unWrappedActivities.ToList();
                DateTime startTime = masterActivity.StartTime;
                foreach (IActivity activity in orderedActivities)
                {
                    RecursivelyAddControlNodes(parallelBlock, activity, startTime);
                }

                control.Add(parallelBlock);
            }
            else
            {
                throw new NotImplementedException("Composite activity of type " + compositeActivity.GetType() +
                                                  " cannot be serialized to xml.");
            }
        }

        private static IActivity UnwrapActivity(IActivity activity)
        {
            IActivity result = activity;
            while (result is ActivityWrapper wrapper)
            {
                result = wrapper.Activity;
            }

            return result;
        }
    }
}