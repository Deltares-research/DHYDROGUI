using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Extensions;
using DeltaShell.Core;
using DeltaShell.Core.Services;
using DeltaShell.Plugins.Data.NHibernate.DelftTools.Shell.Core.Dao;
using Deltares.OpenMI2.Oatc.Sdk.Backbone;
using Deltares.OpenMI2.Oatc.Sdk.Backbone.Generic;
using Deltares.OpenMI2.Oatc.Sdk.Buffer;
using Deltares.OpenMI2.Oatc.Sdk.Spatial;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net.Config;
using OpenMI.Standard2;
using OpenMI.Standard2.TimeSpace;
using Coordinate = Deltares.OpenMI2.Oatc.Sdk.Backbone.Coordinate;
using ExchangeItemHelper = DelftTools.ModelExchange.ExchangeItemHelper;

namespace DeltaShell.OpenMI2Wrapper
{
    public class DeltaShellOpenMI2TimeSpaceComponent : ITimeSpaceComponent
    {
        private static IApplication _application;
        private static int projectCount;
        private Project deltaShellProject;
        private HybridProjectRepository hybridProjectRepository;

        private TimeDependentModelBase rootModel;
        private readonly List<IAdaptedOutputFactory> adaptedOutputFactories = new List<IAdaptedOutputFactory>();

        private readonly bool runningInFromInMemoryTest;
        private readonly ITimeDependentModel timeDependentModel;

        private const string DsProjFilePathKey = "DsProjFilePath";
        private const string ResultingDsProjFilePathKey = "ResultingDsProjFilePath";
        private const string DsProjModelNameKey = "DsProjModelName";
        private const string ModelIdKey = "ModelId";
        private const string ExchangeItemGroupsKey = "ExchangeItemGroups";
        private const string SplitSpecificElementSetsKey = "SplitSpecificElementSets";

        private string resultingDsProjPath;
        private readonly IList<string> splitSpecificElementSets = new List<string>();
        private ExchangeItemHelper.ExchangeItemGroupType exchangeItemGroups = ExchangeItemHelper.ExchangeItemGroupType.All;

        /// <summary>
        /// Constructor for unit testing purposes
        /// </summary>
        /// <param name="timeDependentModel">rootmodel (hydro, flow1, etc.) to be exposed by OpenMI wrapper</param>
        public DeltaShellOpenMI2TimeSpaceComponent(ITimeDependentModel timeDependentModel)
        {
            this.timeDependentModel = timeDependentModel;
            runningInFromInMemoryTest = true;
        }

        /// <summary>
        /// Default constructor (initialize method will read model from dsproj)
        /// </summary>
        public DeltaShellOpenMI2TimeSpaceComponent()
        {
            if (_application == null)
            {
                _application = GetRunningDSApplication();
            }

            Id = Caption = Description = "<none>";
            Arguments = new List<IArgument>
                            {
                                new ArgumentString(DsProjFilePathKey, ""),
                                new ArgumentString(ResultingDsProjFilePathKey, ""),
                                new ArgumentString(DsProjModelNameKey, ""),
                                new ArgumentString(ModelIdKey, ""),
                                new ArgumentString(ExchangeItemGroupsKey, ""),
                                new ArgumentString(SplitSpecificElementSetsKey, ""),
                            };
            Inputs = new List<IBaseInput>();
            Outputs = new List<IBaseOutput>();
            TimeExtent = new TimeSet();

            Status = LinkableComponentStatus.Created;
        }

        public string Caption { get; set; }

        public string Description { get; set; }

        public string Id { get; private set; }

        public IList<IArgument> Arguments { get; private set; }

        public IList<IBaseInput> Inputs { get; private set; }

        public IList<IBaseOutput> Outputs { get; private set; }

        public List<IAdaptedOutputFactory> AdaptedOutputFactories
        {
            get
            {
                return adaptedOutputFactories;
            }
        }

        public LinkableComponentStatus Status { get; private set; }

        public event EventHandler<LinkableComponentStatusChangeEventArgs> StatusChanged;

        public ITimeSet TimeExtent { get; private set; }

        /// <summary>
        /// For unit testing purposes
        /// </summary>
        public static Func<IEnumerable<ApplicationPlugin>> GetAdditionalPlugins { get; set; }

        public void Initialize()
        {
            Status = LinkableComponentStatus.Initializing;

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
                IDictionary<string, IArgument> argDict = Arguments.Dictionary();

                var dsProjPath = argDict.GetValue<string>(DsProjFilePathKey);
                if (String.IsNullOrEmpty(dsProjPath))
                {
                    Status = LinkableComponentStatus.Failed;
                    throw new Exception(String.Format("DeltaShell project file not specified"));
                }
                if (!File.Exists(dsProjPath))
                {
                    Status = LinkableComponentStatus.Failed;
                    throw new Exception(String.Format("DeltaShell project file '{0}' does not exist", dsProjPath));
                }

                resultingDsProjPath = argDict.GetValue<string>(ResultingDsProjFilePathKey);
                if (!String.IsNullOrEmpty(resultingDsProjPath))
                {
                    resultingDsProjPath = Path.GetFullPath(resultingDsProjPath);
                }
                var nHibernateProjectRepositoryFactory = new NHibernateProjectRepositoryFactory();
                foreach (var plugin in PluginManager.GetPlugins<IPlugin>())
                {
                    nHibernateProjectRepositoryFactory.AddPlugin(plugin);
                }
                hybridProjectRepository = new HybridProjectRepository(nHibernateProjectRepositoryFactory);
                deltaShellProject = hybridProjectRepository.Open(dsProjPath);
                if (deltaShellProject == null)
                {
                    Status = LinkableComponentStatus.Failed;
                    throw new Exception(String.Format("Could not open project file '{0}'", dsProjPath));
                }
                projectCount++;

                var modelName = argDict.GetValue<string>(DsProjModelNameKey);
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
                    Status = LinkableComponentStatus.Failed;
                    throw new Exception(String.Format("Model {0} not found in project file '{1}'", modelName ?? "",
                                                      dsProjPath));
                }

                if (rootModel == null)
                {
                    Status = LinkableComponentStatus.Failed;
                    throw new Exception(String.Format("Model {0} not found in project file '{1}'", modelName ?? "",
                                                      dsProjPath));
                }

                Id = argDict.GetValue<string>(ModelIdKey);
                if (String.IsNullOrEmpty(Id))
                {
                    Id = rootModel.Name;
                }

                string eiGroupsString = argDict.GetValue<string>(ExchangeItemGroupsKey);
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

                string splitSpecificElementSetsString = argDict.GetValue<string>(SplitSpecificElementSetsKey);
                if (!String.IsNullOrEmpty(splitSpecificElementSetsString))
                {
                    string[] elementSets = splitSpecificElementSetsString.Split(';');
                    foreach (string elementSetId in elementSets)
                    {
                        splitSpecificElementSets.Add(elementSetId.ToLower());
                    }
                }
            }
            Caption = Description = Id;
           
            try
            {
                rootModel.Initialize();
            }
            catch (Exception)
            {
                Status = LinkableComponentStatus.Failed;
                throw;
            }
            
            if(rootModel.Status != ActivityStatus.Initialized)
            {
                Status = LinkableComponentStatus.Failed;
                throw new Exception(String.Format("Failed to initialize model '{0}'", rootModel.Name ?? ""));
            }

            adaptedOutputFactories.Add(new TimeBufferFactory("OATC Time Buffering"));
            adaptedOutputFactories.Add(new SpatialAdaptedOutputFactory("OATC Element mapping"));

            CreateExchangeItems();

            ((TimeSet) TimeExtent).TimeHorizon = new Time(rootModel.StartTime.ToUniversalTime(),
                                                          rootModel.StopTime.ToUniversalTime());

            Status = LinkableComponentStatus.Initialized;
        }

        public string[] Validate()
        {
            if (Status==LinkableComponentStatus.Initialized)
            {
                Status = LinkableComponentStatus.Validating;

                // Will be implemented in IModel some day...: model.Validate()

                Status = LinkableComponentStatus.Valid;
            }
            return new string[0];
        }

        public void Prepare()
        {
            if (Status == LinkableComponentStatus.Initialized || Status == LinkableComponentStatus.Valid)
            {
                Status = LinkableComponentStatus.Preparing;
                
                foreach (IBaseInput input in Inputs)
                {
                    if (input.Provider != null)
                    {
                        DeltaShellOpenMI2Input deltaShellOpenMI2Input = input as DeltaShellOpenMI2Input;
                        if (deltaShellOpenMI2Input != null)
                        {
                            deltaShellOpenMI2Input.SetLink();
                        }
                    }
                }

                foreach (IBaseOutput output in Outputs)
                {
                    if (output.Consumers.Count() > 0)
                    {
                        DeltaShellOpenMI2SubOutput shellOpenMI2SubOutput = output as DeltaShellOpenMI2SubOutput;
                        if (shellOpenMI2SubOutput != null)
                        {
                            shellOpenMI2SubOutput.ParentOutputItem.HasSubOutputItems = true;
                        }
                    }
                }

                foreach (IBaseOutput output in Outputs)
                {
                    DeltaShellOpenMI2Output deltaShellOpenMI2Output = output as DeltaShellOpenMI2Output;
                    if (deltaShellOpenMI2Output != null)
                    {
                        if (deltaShellOpenMI2Output.Consumers.Count() > 0 || deltaShellOpenMI2Output.HasSubOutputItems)
                        {
                            deltaShellOpenMI2Output.SetLink();
                        }
                    }
                }

                CopyOutputValuesToExchangeItems(Outputs);

                Status = LinkableComponentStatus.Updated;
            }
        }

        public void Update(params IBaseOutput[] requiredOutput)
        {
            if (Status != LinkableComponentStatus.Initialized && Status != LinkableComponentStatus.Updated && Status != LinkableComponentStatus.Valid)
            {
                return;
            }
            Status = LinkableComponentStatus.WaitingForData;
            foreach (IBaseInput baseInput in Inputs)
            {
                var input = baseInput as DeltaShellOpenMI2Input;
                if (input == null)
                {
                    Status = LinkableComponentStatus.Failed;
                    throw new Exception(String.Format("Unexpected Output implementation '{0}'", baseInput.GetType().Name));
                }
                if (input.Provider != null)
                {
                    input.Values = input.Provider.GetValues(input);
                }
                if(input.Values != null)
                {
                    input.FeedValuesToDataItem(rootModel.CurrentTime);
                }
            }

            Status = LinkableComponentStatus.Updating;
            IList<IBaseOutput> actualOutputs = Outputs;
            if (requiredOutput != null && requiredOutput.Length > 0)
            {
                // For performance reasons, only the required output items were specified
                actualOutputs = requiredOutput.ToList();
            }
            try
            {
                rootModel.Execute();

            }
            catch (Exception)
            {
                Status=LinkableComponentStatus.Failed;
                throw;
            }

            if (rootModel.Status != ActivityStatus.Executed && rootModel.Status != ActivityStatus.Done) return;
            try
            {
                CopyOutputValuesToExchangeItems(actualOutputs);
            }
            catch (Exception)
            {
                Status = LinkableComponentStatus.Failed;
                throw;
            }
            Status = LinkableComponentStatus.Updated;
        }

        private void CopyOutputValuesToExchangeItems(IEnumerable<IBaseOutput> outputs)
        {
            // first update parent items, items that are invidually split per element in element set
            foreach (var output in outputs)
            {
                DeltaShellOpenMI2Output dsOutput = output as DeltaShellOpenMI2Output;
                if (dsOutput != null)
                {
                    dsOutput.GetValuesFromDataItem(rootModel.CurrentTime);
                }
            }
            foreach (var output in outputs)
            {
                DeltaShellOpenMI2SubOutput dsSubOutput = output as DeltaShellOpenMI2SubOutput;
                if (dsSubOutput != null)
                {
                    dsSubOutput.GetValuesFromParentItem(rootModel.CurrentTime);
                }
            }
        }

        public void Finish()
        {
            Status=LinkableComponentStatus.Finishing;

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

            foreach (DeltaShellOpenMI2Input input in Inputs)
            {
                if (input.Provider != null)
                {
                    input.RemoveLink();

                }
            }

            foreach (DeltaShellOpenMI2Output output in Outputs)
            {
                if (output.Consumers.Count() > 0)
                {
                    output.RemoveLink();
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

            rootModel.Finish();
            
            if (!runningInFromInMemoryTest)
            {
                if (resultingDsProjPath != null)
                {
                    hybridProjectRepository.SaveProjectAs(deltaShellProject, resultingDsProjPath);
                }
                else
                {
                    hybridProjectRepository.Save(deltaShellProject);
                }
                hybridProjectRepository.Close(deltaShellProject);
                hybridProjectRepository = null;
                deltaShellProject = null;
                projectCount--;
            }

            rootModel.Cleanup();

            if (projectCount == 0 && _application != null)
            {
                _application.Dispose();
                _application = null;
            }
            Status = LinkableComponentStatus.Finished;
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
                var inputDataItemLocations = model.GetChildDataItemLocations(DataItemRole.Input);
                foreach (IFeature dataItemLocation in inputDataItemLocations)
                {
                    var elementSet = DetermineElementSet(dataItemLocation);
                    var dataItems = model.GetChildDataItems(dataItemLocation);
                    foreach (var dataItem in dataItems)
                    {
                        if ((dataItem.Role & DataItemRole.Input) == DataItemRole.Input)
                        {
                            string quantityName = ExchangeItemHelper.DetermineQuantityName(dataItem, elementSet.Caption);
                            var id = DetermineId(dataItem, dataItemLocation);
                            if (ExchangeItemHelper.IncludeExchangeItemForSelectedEIGroup(
                                    exchangeItemGroups, dataItemLocation, quantityName, checkForInput: true))
                            {
                                string quantityUserName = ExchangeItemHelper.StandardNameToUserName(quantityName);
                                IQuantity quantity = new Quantity(DetermineUnit(dataItem), "standard_name: " + quantityName, quantityUserName); ;
                                Inputs.Add(new DeltaShellOpenMI2Input(dataItem, id, quantity, elementSet, this));
                            }
                        }
                    }
                }

                var outputDataItemLocations = model.GetChildDataItemLocations(DataItemRole.Output);
                foreach (IFeature dataItemLocation in outputDataItemLocations)
                {
                    var elementSet = DetermineElementSet(dataItemLocation);
                    var dataItems = model.GetChildDataItems(dataItemLocation);
                    foreach (IDataItem dataItem in dataItems)
                    {
                        if ( (dataItem.Role & DataItemRole.Output) == DataItemRole.Output)
                        {
                            string quantityName = ExchangeItemHelper.DetermineQuantityName(dataItem, elementSet.Caption);
                            var id = DetermineId(dataItem, dataItemLocation);
                            if (ExchangeItemHelper.IncludeExchangeItemForSelectedEIGroup(
                                    exchangeItemGroups, dataItemLocation, quantityName))
                            {
                                string quantityUserName = ExchangeItemHelper.StandardNameToUserName(quantityName);
                                IQuantity quantity = new Quantity(DetermineUnit(dataItem), "standard_name: " + quantityName, quantityUserName); ;
                                DeltaShellOpenMI2Output deltaShellOpenMI2Output = new DeltaShellOpenMI2Output(dataItem, id, quantity, elementSet, this);
                                Outputs.Add(deltaShellOpenMI2Output);
                                CreateSplitElementSetExhangeItems(deltaShellOpenMI2Output);
                            }
                        }
                    }
                }
            }
        }

        private void CreateSplitElementSetExhangeItems(DeltaShellOpenMI2Output parentOutputItem)
        {
            IElementSet parentElementSet = parentOutputItem.ElementSet();
            if (ElementSetTypeIsIdOrXyAndMustBeSplitted(parentElementSet))
            {
                List<IElementSet> singleElementSets = new List<IElementSet>();
                for (int elementIndex = 0; elementIndex < parentElementSet.ElementCount; elementIndex++)
                {
                    string subElementId = parentElementSet.Caption + "." + parentElementSet.GetElementId(elementIndex).Id;
                    ElementSet subElementSet =
                        new ElementSet(subElementId,
                                       subElementId,
                                       parentElementSet.ElementType,
                                       parentElementSet.SpatialReferenceSystemWkt);
                    singleElementSets.Add(subElementSet);
                    DeltaShellOpenMI2SubOutput deltaShellOpenMI2Output = new DeltaShellOpenMI2SubOutput(parentOutputItem, subElementSet, elementIndex, this);
                    Outputs.Add(deltaShellOpenMI2Output);
                }
            }
        }

        private bool ElementSetTypeIsIdOrXyAndMustBeSplitted(IElementSet elementSet)
        {
            if (elementSet.ElementType == ElementType.IdBased || elementSet.ElementType == ElementType.Point)
            {
                foreach (string splitSpecificElementSet in splitSpecificElementSets)
                {
                    if (splitSpecificElementSet.Equals(elementSet.Caption.ToLower()))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static IElementSet DetermineElementSet(IFeature dataItemLocation)
        {
            string elementSetName = dataItemLocation is INameable
                ? ((INameable) dataItemLocation).Name
                : (dataItemLocation.Attributes.ContainsKey("StandardFeatureName")
                    ? (string) dataItemLocation.Attributes["StandardFeatureName"]
                    : "");

            var elementSet = new ElementSet(elementSetName, elementSetName, ElementType.Point);

            IGeometry geometry = dataItemLocation.Geometry;

            if (geometry is IPoint)
            {
                GeoAPI.Geometries.Coordinate coordinate = dataItemLocation.Geometry.Centroid.Coordinate;
                var element = new Element(elementSetName);  
                element.AddVertex(new Coordinate(coordinate.X, coordinate.Y, coordinate.Z));
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
                        element.AddVertex(new Coordinate(networkLocation.Geometry.Coordinate.X, networkLocation.Geometry.Coordinate.Y));
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
                return OpenMI2QuantityHelper.CreateOrFindUnitByUnitString(unitName);
            }
            return new Unit("-", 1, 0);
        }

        private static string DetermineId(IDataItem dataItem, IFeature location = null)
        {
            if (location != null)
            {
                return location.GetEntityType().Name + " - " + dataItem.Name;
            }
            return dataItem.Value.GetType().Name + " - " + dataItem.Name;
        }

        private static DeltaShellApplication GetRunningDSApplication()
        {
            var oldWorkingDirectory = Directory.GetCurrentDirectory();
            var location = Path.GetDirectoryName(typeof(DeltaShellOpenMI2TimeSpaceComponent).Assembly.Location);
            Directory.SetCurrentDirectory(location);

            XmlConfigurator.Configure();

            var app = new DeltaShellApplication
            {
                ScriptRunner = { SkipDefaultLibraries = true },
                Settings = new NameValueCollection
                        {
                            {"pluginsDirectory", "../plugins"},
                            {"language", "en-US"}
                        }
            };

            if (GetAdditionalPlugins != null)
            {
                app.Plugins.AddRange(GetAdditionalPlugins());
            }

            app.Run();
            
            Directory.SetCurrentDirectory(oldWorkingDirectory); // change it back
            return app;
        }
    }

    internal class DeltaShellOpenMI2SubOutput : Output
    {
        private DeltaShellOpenMI2Output parentOutputItem;
        private readonly int elementIndex;

        public DeltaShellOpenMI2SubOutput(DeltaShellOpenMI2Output parentOutputItem, IElementSet elementSet, int elementIndex, ITimeSpaceComponent component) : base(component, CreateId(parentOutputItem.Quantity(), elementSet), parentOutputItem.ValueDefinition, elementSet)
        {
            this.parentOutputItem = parentOutputItem;
            this.elementIndex = elementIndex;
        }

        private static string CreateId(IQuantity quantity, IElementSet elementSet)
        {
            return elementSet.Caption  + " - " + quantity.Caption;
        }

        public DeltaShellOpenMI2Output ParentOutputItem
        {
            get { return parentOutputItem; }
        }

        public void GetValuesFromParentItem(DateTime currentTime)
        {
            IList<double> parentValues = parentOutputItem.GetDataItemValuesAsDoubles();
            if (parentValues == null)
            {
                throw new Exception("No computed values available for output item " + ToString());
            }
            Values = new TimeSpaceValueSet<double>(new[] { parentValues[elementIndex] });
        }
    }
}