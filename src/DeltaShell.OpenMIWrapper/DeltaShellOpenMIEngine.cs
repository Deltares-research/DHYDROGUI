using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.ModelExchange;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Extensions;
using DeltaShell.Core;
using DeltaShell.Core.Services;
using DeltaShell.Plugins.Data.NHibernate.DelftTools.Shell.Core.Dao;
using Deltares.OpenMI.Oatc.Sdk.Backbone;
using Deltares.OpenMI.Oatc.Sdk.DevelopmentSupport;
using Deltares.OpenMI.Oatc.Sdk.Wrapper;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net.Config;
using OpenMI.Standard;
using TimeSpan = Deltares.OpenMI.Oatc.Sdk.Backbone.TimeSpan;

namespace DeltaShell.OpenMIWrapper
{
    public class DeltaShellOpenMIEngine : IEngine
    {
        private readonly ITimeDependentModel timeDependentModel;
        private const string DsProjFilePathKey = "DsProjFilePath";
        private const string ResultingDsProjFilePathKey = "ResultingDsProjFilePath";
        private const string DsProjModelNameKey1 = "ModelName";  // compatibility with Sobek-2 wrapper
        private const string DsProjModelNameKey2 = "DsProjModelName";
        private const string ModelIdKey = "ModelId";
        private const string ExchangeItemGroupsKey = "ExchangeItemGroups";
        private const string SeparateProcessKey = "SeparateProcess";
        private const string SeparateProcessKey2 = "Process"; // compatibility with Sobek-2 wrapper
        private const string SplitSpecificElementSetsKey = "SplitSpecificElementSets";
        private readonly IList<InputExchangeItem> inputs = new List<InputExchangeItem>();
        private readonly IList<OutputExchangeItem> outputs = new List<OutputExchangeItem>();
        
        private static IApplication _application;
        private static int projectCount;
        private Project deltaShellProject;
        private HybridProjectRepository _hybridProjectRepository;

        private static readonly string _componentId = "DeltaShell Model";
        private static readonly string _componentDescription = "DeltaShell (single or integrated) model";
        private string modelId;
        private string modelDescription;
        private TimeDependentModelBase rootModel;
        private TimeSpan timeHorizon;
        private bool dataItemLinksHaveBeenEstablished;
        private IEnumerable<ILink> incomingLinks;
        private IEnumerable<ILink> outgoingLinks;
        private readonly bool runningInFromInMemoryTest;
        private string resultingDsProjPath;

        private ExchangeItemHelper.ExchangeItemGroupType exchangeItemGroups = ExchangeItemHelper.ExchangeItemGroupType.All;
		private readonly IList<string> splitSpecificElementSets = new List<string>();
        private bool _runInSeparateProcess;

        /// <summary>
        /// Constructor for unit testing purposes
        /// </summary>
        /// <param name="timeDependentModel">rootmodel (hydro, flow1, etc.) to be exposed by OpenMI wrapper</param>
        public DeltaShellOpenMIEngine(ITimeDependentModel timeDependentModel)
        {
            this.timeDependentModel = timeDependentModel;
            runningInFromInMemoryTest = true;
        }

        /// <summary>
        /// Default empty constructor (initialize method will read model from dsproj)
        /// </summary>
        public DeltaShellOpenMIEngine()
        {
            if (_application == null)
            {
                _application = GetRunningDSApplication();
            }
        }

        /// <summary>
        /// For unit testing purposes
        /// </summary>
        public static Func<IEnumerable<ApplicationPlugin>> GetAdditionalPlugins { get; set; }

        public void Initialize(Hashtable properties)
        {
            if (runningInFromInMemoryTest)
            {
                rootModel = timeDependentModel as TimeDependentModelBase;
                if (rootModel == null)
                {
                    throw new Exception(String.Format("Unexpected model implementation {0} (model '{1}')",
                                                      timeDependentModel.GetType().Name, timeDependentModel.Name));
                }
            }
            else
            {
                string dsProjPath = null;
                string modelName = null;
                string eiGroupsString = null;

                foreach (DictionaryEntry property in properties)
                {
                    if (((string)property.Key).ToLower().Equals(DsProjFilePathKey.ToLower()))
                    {
                        dsProjPath = (string) property.Value;
                    }
                    else if (((string) property.Key).ToLower().Equals(DsProjModelNameKey1.ToLower()) ||
                             ((string) property.Key).ToLower().Equals(DsProjModelNameKey2.ToLower()))
                    {
                        modelName = (string) property.Value;
                    }
                    else if (((string)property.Key).ToLower().Equals(ModelIdKey.ToLower()))
                    {
                        modelId = modelDescription = (string)property.Value;
                    }
                    else if (((string)property.Key).ToLower().Equals(ResultingDsProjFilePathKey.ToLower()))
                    {
                        resultingDsProjPath = (string)property.Value;
                    }
                    else if (((string)property.Key).ToLower().Equals(ExchangeItemGroupsKey.ToLower()))
                    {
                        eiGroupsString = (string)property.Value;
                    }
                    else if (((string)property.Key).ToLower().Equals(SplitSpecificElementSetsKey.ToLower()))
                    {
                        string splitSpecificElementSetsString = (string)property.Value;
                        if (!String.IsNullOrEmpty(splitSpecificElementSetsString))
                        {
                            string[] elementSets = ((string)properties["SplitSpecificElementSets"]).Split(';');
                            foreach (string elementSetId in elementSets)
                            {
                                splitSpecificElementSets.Add(elementSetId.ToLower());
                            }
                        }
                    }
                    else if (((string)property.Key).ToLower().Equals(SeparateProcessKey.ToLower()) ||
                             ((string)property.Key).ToLower().Equals(SeparateProcessKey2.ToLower()))
                    {
                        if (!Boolean.TryParse((string) property.Value, out _runInSeparateProcess))
                        {
                            throw new Exception(String.Format("Invalid value ({0}) for {1} key", property.Value, property.Key));
                        }
                    }
                }

                if (String.IsNullOrEmpty(dsProjPath))
                {
                    throw new Exception(String.Format("DeltaShell project file not specified"));
                }
                if (!File.Exists(dsProjPath))
                {
                    throw new Exception(String.Format("DeltaShell project file '{0}' does not exist", dsProjPath));
                }

                var nHibernateProjectRepositoryFactory = new NHibernateProjectRepositoryFactory();
                foreach (var plugin in PluginManager.GetPlugins<IPlugin>())
                {
                    nHibernateProjectRepositoryFactory.AddPlugin(plugin);
                }
                _hybridProjectRepository = new HybridProjectRepository(nHibernateProjectRepositoryFactory);
                deltaShellProject = _hybridProjectRepository.Open(dsProjPath);
                if (deltaShellProject == null)
                {
                    throw new Exception(String.Format("Could not open project file '{0}'", dsProjPath));
                }
                projectCount++;

                if (!String.IsNullOrEmpty(modelName))
                {
                    rootModel = deltaShellProject.RootFolder.Models.OfType<TimeDependentModelBase>().FirstOrDefault(
                        m => m.Name.ToLower().Equals(modelName.ToLower()));
                }
                else
                {
                    rootModel = deltaShellProject.RootFolder.Models.OfType<TimeDependentModelBase>().FirstOrDefault();
                }

                if (rootModel == null)
                {
                    throw new Exception(String.Format("Model {0} not found in project file '{1}'", modelName ?? "",
                                                      dsProjPath));
                }
                if (!String.IsNullOrEmpty(resultingDsProjPath))
                {
                    resultingDsProjPath = Path.GetFullPath(resultingDsProjPath);
                }
                if (!String.IsNullOrEmpty(eiGroupsString))
                {
                    try
                    {
                        exchangeItemGroups =
                            (ExchangeItemHelper.ExchangeItemGroupType)
                            Enum.Parse(typeof(ExchangeItemHelper.ExchangeItemGroupType), eiGroupsString.ToLower());
                    }
                    catch (Exception)
                    {
                        throw new Exception("Unknown value for ExchangeItemGroups: " + eiGroupsString);
                    }
                }

            }

            if (modelId == null)
            {
                // model id not set in omi file, take name from model in dsproj
                modelId = modelDescription = rootModel.Name;
            }

            CreateExchangeItems();

            timeHorizon =
                new TimeSpan(
                    new TimeStamp(CalendarConverter.Gregorian2ModifiedJulian(rootModel.StartTime)),
                    new TimeStamp(CalendarConverter.Gregorian2ModifiedJulian(rootModel.StopTime)));
            
            rootModel.Initialize();

            if (rootModel.Status != ActivityStatus.Initialized)
            {
                throw new Exception(String.Format("Failed to initialize model '{0}'", rootModel.Name ?? ""));
            }
        }

        internal void Prepare(ILink[] incomingLinks, ILink[] outgoingLinks)
        {
            this.incomingLinks = incomingLinks;
            this.outgoingLinks = outgoingLinks;

            if (!dataItemLinksHaveBeenEstablished)
            {
                foreach (ILink incomingLink in incomingLinks)
                {
                    DeltaShellOpenMIInput deltaShellOpenMIInput = FindInputItem(incomingLink.TargetQuantity.ID, incomingLink.TargetElementSet.ID) as DeltaShellOpenMIInput;
                    if (deltaShellOpenMIInput != null)
                    {
                        deltaShellOpenMIInput.SetLink();
                    }
                }
                foreach (ILink outgingLink in outgoingLinks)
                {
                    DeltaShellOpenMIOutput deltaShellOpenMIOutput = FindOutputItem(outgingLink.SourceQuantity.ID, outgingLink.SourceElementSet.ID) as DeltaShellOpenMIOutput;
                    if (deltaShellOpenMIOutput != null)
                    {
                        deltaShellOpenMIOutput.SetLink();
                    }
                }
                dataItemLinksHaveBeenEstablished = true;
            }
        }

        public void Finish()
        {
            // 'Run time exchange' links that were added in the Prepare() method should be removed, because
            // otherwise they will be stored in the database (!@#$% object relational modeling).
            // However, removing links leads to setting OutputOutSync to true (!@#$%, this is needed indeed for
            // 'shared' items (network, etc.) but not for 'run time exchange' items).
            // So we keep track of the actual out of sync flag.
            Dictionary<ModelBase, bool> outputsOutOfSync = new Dictionary<ModelBase, bool>();
            outputsOutOfSync.Add(rootModel, rootModel.OutputOutOfSync);
            var hydroModel = rootModel as ICompositeActivity;
            if (hydroModel != null)
            {
                foreach (ModelBase model in hydroModel.Activities.OfType<ModelBase>())
                {
                    outputsOutOfSync.Add(model, model.OutputOutOfSync);
                }
            }

            foreach (ILink incomingLink in incomingLinks)
            {
                DeltaShellOpenMIInput deltaShellOpenMIInput = FindInputItem(incomingLink.TargetQuantity.ID, incomingLink.TargetElementSet.ID) as DeltaShellOpenMIInput;
                if (deltaShellOpenMIInput != null)
                {
                    deltaShellOpenMIInput.RemoveLink();
                }
            }
            foreach (ILink outgingLink in outgoingLinks)
            {
                DeltaShellOpenMIOutput deltaShellOpenMIOutput = FindOutputItem(outgingLink.SourceQuantity.ID, outgingLink.SourceElementSet.ID) as DeltaShellOpenMIOutput;
                if (deltaShellOpenMIOutput != null)
                {
                    deltaShellOpenMIOutput.RemoveLink();
                }
            }

            rootModel.OutputOutOfSync = outputsOutOfSync[rootModel];
            if (hydroModel != null)
            {
                foreach (ModelBase model in hydroModel.Activities.OfType<ModelBase>())
                {
                    model.OutputOutOfSync = outputsOutOfSync[model];
                }
            }

            rootModel.Cleanup();
            if (!runningInFromInMemoryTest)
            {
                if (resultingDsProjPath != null)
                {
                    _hybridProjectRepository.SaveProjectAs(deltaShellProject, resultingDsProjPath);
                }
                else
                {
                    _hybridProjectRepository.Save(deltaShellProject);
                }
                _hybridProjectRepository.Close(deltaShellProject);
                _hybridProjectRepository = null;
                deltaShellProject = null;
                projectCount--;
            }
            if (projectCount == 0 && _application != null)
            {
                _application.Dispose();
                _application = null;
            }
        }

        public void Dispose()
        {
            // no action
        }

        public bool PerformTimeStep()
        {
            rootModel.Execute();
            return true;
        }

        public ITime GetCurrentTime()
        {
            DateTime currentTime = rootModel.CurrentTime;
            var hydroModel = rootModel as ICompositeActivity;
            if (hydroModel != null)
            {
                foreach (ITimeDependentModel model in hydroModel.Activities.OfType<ITimeDependentModel>())
                {
                    var subModelCurrentTime = model.CurrentTime;
                    if (subModelCurrentTime > currentTime)
                    {
                        currentTime = subModelCurrentTime;
                    }
                }
            }

            return new TimeStamp(CalendarConverter.Gregorian2ModifiedJulian(currentTime));
        }

        public ITime GetInputTime(string quantityID, string elementSetID)
        {
            double timeStep = rootModel.TimeStep.Ticks / 10000000d / 86400d;
            return new TimeStamp(((ITimeStamp)GetCurrentTime()).ModifiedJulianDay + timeStep);
        }

        public ITimeStamp GetEarliestNeededTime()
        {
            return (ITimeStamp) GetCurrentTime();
        }

        public void SetValues(string quantityID, string elementSetID, IValueSet valueSet)
        {
            DeltaShellOpenMIInput dsInput = FindInputItem(quantityID, elementSetID) as DeltaShellOpenMIInput;
            if (dsInput != null)
            {
                dsInput.SetValues(GetCurrentTime(), valueSet);
            }
        }

        public IValueSet GetValues(string quantityID, string elementSetID)
        {
            IOutputExchangeItem dsOutput = FindOutputItem(quantityID, elementSetID);
            if (dsOutput is DeltaShellOpenMIOutput)
            {
                return ((DeltaShellOpenMIOutput)dsOutput).GetValues(GetCurrentTime());
            }
            if (dsOutput is DeltaShellOpenMISubOutput)
            {
                return ((DeltaShellOpenMISubOutput)dsOutput).GetValues(GetCurrentTime());
            }
            throw new Exception("Unknown Output Exchange Item type: " + dsOutput.GetType());
        }

        public double GetMissingValueDefinition()
        {
            return double.NaN;
        }

        public string GetComponentID()
        {
            return _componentId;
        }

        public string GetComponentDescription()
        {
            return _componentDescription;
        }

        public string GetModelID()
        {
            return modelId;
        }

        public string GetModelDescription()
        {
            return modelDescription;
        }

        public ITimeSpan GetTimeHorizon()
        {
            return timeHorizon;
        }

        public int GetInputExchangeItemCount()
        {
            return inputs.Count();
        }

        public int GetOutputExchangeItemCount()
        {
            return outputs.Count();
        }

        public OutputExchangeItem GetOutputExchangeItem(int exchangeItemIndex)
        {
            return outputs[exchangeItemIndex];
        }

        public InputExchangeItem GetInputExchangeItem(int exchangeItemIndex)
        {
            return inputs[exchangeItemIndex];
        }

        private void CreateExchangeItems()
        {
            IEnumerable<IModel> models = new List<IModel> {rootModel};
            var hydroModel = rootModel as ICompositeActivity;
            if (hydroModel != null)
            {
                models = hydroModel.Activities.OfType<IModel>();
            }

            foreach (IModel model in models)
            {
                IEnumerable<IFeature> inputDataItemLocations = model.GetChildDataItemLocations(DataItemRole.Input);
                foreach (IFeature dataItemLocation in inputDataItemLocations)
                {
                    IElementSet elementSet = DetermineElementSet(dataItemLocation);
                    IEnumerable<IDataItem> dataItems = model.GetChildDataItems(dataItemLocation);
                    foreach (IDataItem dataItem in dataItems)
                    {
                        if ((dataItem.Role & DataItemRole.Input) == DataItemRole.Input)
                        {
                            string quantityName = ExchangeItemHelper.DetermineQuantityName(dataItem, elementSet.ID);
                            if (ExchangeItemHelper.IncludeExchangeItemForSelectedEIGroup(exchangeItemGroups, dataItemLocation, quantityName, checkForInput: true))
                            {
                                string quantityUserName = ExchangeItemHelper.StandardNameToUserName(quantityName);
                                IQuantity quantity = new Quantity(DetermineUnit(dataItem), "standard_name: " + quantityName, quantityUserName); ;
                                inputs.Add(new DeltaShellOpenMIInput(dataItem, GetMissingValueDefinition(), quantity, elementSet));
                            }
                        }
                    }
                }

                IEnumerable<IFeature> outputDataItemLocations = model.GetChildDataItemLocations(DataItemRole.Output);
                foreach (IFeature dataItemLocation in outputDataItemLocations)
                {
                    IElementSet elementSet = DetermineElementSet(dataItemLocation);
                    IEnumerable<IDataItem> dataItems = model.GetChildDataItems(dataItemLocation);
                    foreach (IDataItem dataItem in dataItems)
                    {
                        if ((dataItem.Role & DataItemRole.Output) == DataItemRole.Output)
                        {
                            string quantityName = ExchangeItemHelper.DetermineQuantityName(dataItem, elementSet.ID);
                            if (ExchangeItemHelper.IncludeExchangeItemForSelectedEIGroup(exchangeItemGroups, dataItemLocation, quantityName))
                            {
                                string quantityUserName = ExchangeItemHelper.StandardNameToUserName(quantityName);
                                IQuantity quantity = new Quantity(DetermineUnit(dataItem), "standard_name: " + quantityName, quantityUserName); ;
                                DeltaShellOpenMIOutput deltaShellOpenMIOutput = new DeltaShellOpenMIOutput(dataItem, quantity, elementSet);
                                outputs.Add(deltaShellOpenMIOutput);
                                CreateSplitElementSetExhangeItems(deltaShellOpenMIOutput);
                            }
                        }
                    }
                }
            }
        }

        private void CreateSplitElementSetExhangeItems(DeltaShellOpenMIOutput parentOutputItem)
        {
            IElementSet parentElementSet = parentOutputItem.ElementSet;
            if (ElementSetTypeIsIdOrXyAndMustBeSplitted(parentElementSet))
            {
                List<IElementSet> singleElementSets = new List<IElementSet>();
                for (int elementIndex = 0; elementIndex < parentElementSet.ElementCount; elementIndex++)
                {
                    string subElementId = parentElementSet.ID + "." + parentElementSet.GetElementID(elementIndex);
                    ElementSet subElementSet =
                        new ElementSet(subElementId,
                                       subElementId,
                                       parentElementSet.ElementType,
                                       parentElementSet.SpatialReference);
                    singleElementSets.Add(subElementSet);
                    DeltaShellOpenMISubOutput deltaShellOpenMIOutput = new DeltaShellOpenMISubOutput(parentOutputItem, subElementSet, elementIndex);
                    outputs.Add(deltaShellOpenMIOutput);
                }
            }
        }

        private bool ElementSetTypeIsIdOrXyAndMustBeSplitted(IElementSet elementSet)
		{
            if (elementSet.ElementType == ElementType.IDBased || elementSet.ElementType == ElementType.XYPoint)
			{
	            foreach (string splitSpecificElementSet in splitSpecificElementSets)
	            {
	                if (splitSpecificElementSet.Equals(elementSet.ID.ToLower()))
	                {
	                    return true;
	                }
	            }
			}
			return false;
		}

        private IInputExchangeItem FindInputItem(string quantityID, string elementSetID)
        {
            foreach (IInputExchangeItem input in inputs)
            {
                if (input.Quantity.ID.Equals(quantityID) && input.ElementSet.ID.Equals(elementSetID))
                {
                    return input;
                }
            }
            throw new Exception(String.Format("Unknown input exchange item '{0}/{1}'", elementSetID, quantityID));
        }

        private IOutputExchangeItem FindOutputItem(string quantityID, string elementSetID)
        {
            foreach (OutputExchangeItem output in outputs)
            {
                if (output.Quantity.ID.Equals(quantityID) && output.ElementSet.ID.Equals(elementSetID))
                {
                    return output;
                }
            }
            throw new Exception(String.Format("Unknown output exchange item '{0}/{1}'", elementSetID, quantityID));
        }

        private static IElementSet DetermineElementSet(IFeature dataItemLocation)
        {
            string elementSetName = dataItemLocation is INameable
                ? ((INameable) dataItemLocation).Name
                : (dataItemLocation.Attributes.ContainsKey("StandardFeatureName")
                    ? (string) dataItemLocation.Attributes["StandardFeatureName"]
                    : "");

            var geometry = dataItemLocation.Geometry;

            var elementSet = new ElementSet(elementSetName, elementSetName, ElementType.XYPoint,
                                            new SpatialReference("-"));
            if (geometry is IPoint)
            {
                Coordinate coordinate = dataItemLocation.Geometry.Centroid.Coordinate;
                var element = new Element(elementSetName);
                element.AddVertex(new Vertex(coordinate.X, coordinate.Y, coordinate.Z));
                elementSet.AddElement(element);
            }
            else
            {
                var geometryCollection = geometry as IGeometryCollection;
                if (geometryCollection != null)
                {
                    if (!dataItemLocation.Attributes.ContainsKey("ElementType"))
                    {
                        throw new ArgumentException("Invalid type " + dataItemLocation.GetType(), "dataItemLocation");
                    }

                    if (!dataItemLocation.Attributes.ContainsKey("locations"))
                    {
                        throw new ArgumentException("Missing locations attribute : " + dataItemLocation.GetType(), "dataItemLocation");
                    }

                    var locationEnumerable = dataItemLocation.Attributes["locations"] as IEnumerable;
                    if (locationEnumerable == null)
                    {
                        throw new ArgumentException("locations attribute does not contain a network location array");
                    }

                    foreach (var networkLocation in locationEnumerable.OfType<INetworkLocation>())
                    {
                        var element = new Element(networkLocation.Branch.Name + "_" + networkLocation.Chainage.ToString("F3", CultureInfo.InvariantCulture));
                        elementSet.AddElement(element);
                    }
                }
            }
            return elementSet;
        }

        private static IUnit DetermineUnit(IDataItem dataItem)
        {
            string unitName = dataItem.GetUnitName();

            if (unitName != null)
            {
                return OpenMIQuantityHelper.CreateOrFindUnitByUnitString(unitName);
            }
            return new Unit("-", 1, 0);
        }

        private static DeltaShellApplication GetRunningDSApplication()
        {
            var oldWorkingDirectory = Directory.GetCurrentDirectory();
            var location = Path.GetDirectoryName(typeof(DeltaShellOpenMIEngine).Assembly.Location);
            Directory.SetCurrentDirectory(location);
            
            XmlConfigurator.Configure();

            var app = new DeltaShellApplication();


            if (GetAdditionalPlugins != null)
            {
                app.Plugins.AddRange(GetAdditionalPlugins());
            }

            app.Run();
            
            Directory.SetCurrentDirectory(oldWorkingDirectory); // change it back
            return app;
        }
    }

    internal class DeltaShellOpenMISubOutput : OutputExchangeItem
    {
        private readonly DeltaShellOpenMIOutput parentOutputItem;
        private readonly int elementIndex;

        public DeltaShellOpenMISubOutput(DeltaShellOpenMIOutput parentOutputItem, IElementSet elementSet, int elementIndex)
        {
            this.parentOutputItem = parentOutputItem;
            this.elementIndex = elementIndex;
            ElementSet = elementSet;
            Quantity = parentOutputItem.Quantity;
        }

        public IValueSet GetValues(ITime getCurrentTime)
        {
            double[] parentValues = parentOutputItem.GetDataItemValuesAsDoubles();
            if (parentValues == null)
            {
                throw new Exception("No computed values available for output item " + ToString());
            }
            return new ScalarSet(new[] {parentValues[elementIndex]});
        }
    }
}