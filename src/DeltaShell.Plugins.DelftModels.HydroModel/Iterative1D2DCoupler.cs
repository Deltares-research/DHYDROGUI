using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DelftTools.Utils.Validation;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.HydroModel.Api;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using SharpMap.Api;
using IUnit = DelftTools.Units.IUnit;

namespace DeltaShell.Plugins.DelftModels.HydroModel
{
    public class Iterative1D2DCoupler : CompositeActivity, IDisposable, IHydroModelWorkFlow, IDimrModel
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Iterative1D2DCoupler));

        private ITimeDependentModel flow1DModel;
        private ITimeDependentModel flow2DModel;

        private readonly IEventedList<Iterative1D2DCouplerLink> lineSegments = new EventedList<Iterative1D2DCouplerLink>();
        private bool featuresGenerated;
        private List<FeatureCoverage> linkCoverages;
        private IHydroModel hydroModel;
        public const string GridPropertyName = "Grid";
        public const string IsPartOf1D2DModelPropertyName = "IsPartOf1D2DModel";
        public const string DisableFlowNodeRenumberingPropertyName = "DisableFlowNodeRenumbering";
        
        public static string CellsToFeaturesName = "CellsToFeatures";

        private Iterative1D2DCouplerData iterative1D2DCouplerData;

        public Iterative1D2DCoupler()
        {
            Data = new Iterative1D2DCouplerData
            {
                Coupler = this,
                ForceDebugLoggingAction = ForceDebugLogging,
                RefreshMappingAction = RefreshMappings
            };
        }

        public IHydroModel HydroModel
        {
            get { return hydroModel; }
            set
            {
                if (hydroModel != null)
                {
                    ((INotifyPropertyChanged)hydroModel).PropertyChanged -= HydroModelPropertyChanged;
                    RemoveLogFile();
                }

                hydroModel = value;

                if (hydroModel != null)
                {
                    ((INotifyPropertyChanged)hydroModel).PropertyChanged += HydroModelPropertyChanged;
                }
            }
        }

        public virtual ITimeDependentModel Flow1DModel
        {
            get
            {
                return flow1DModel;
            }
            set
            {
                if (flow1DModel != null)
                {
                    var discretization = GetFirstModelDataItemValueByType<IDiscretization>(flow1DModel);
                    if (discretization != null)
                    {
                        ((INotifyPropertyChanged)discretization).PropertyChanged -= FlowModel1DDiscretizationChanged;
                    }
                }

                flow1DModel = value;

                if (flow1DModel != null)
                {
                    var discretization = GetFirstModelDataItemValueByType<IDiscretization>(flow1DModel);
                    if (discretization != null)
                    {
                        ((INotifyPropertyChanged)discretization).PropertyChanged += FlowModel1DDiscretizationChanged;
                    }
                }
            }
        }

        public virtual ITimeDependentModel Flow2DModel
        {
            get
            {
                return flow2DModel;
            }
            set
            {
                if (flow2DModel != null)
                {
                    ((INotifyPropertyChanged)flow2DModel).PropertyChanged -= FlowModel2DDiscretizationChanged;
                    var dimrModel = flow2DModel as IDimrModel;
                    if (dimrModel == null) return;
                    dimrModel.SetVar(new[] { false }, IsPartOf1D2DModelPropertyName);
                }

                flow2DModel = value;

                if (flow2DModel != null)
                {
                    ((INotifyPropertyChanged)flow2DModel).PropertyChanged += FlowModel2DDiscretizationChanged;
                    var dimrModel = flow2DModel as IDimrModel;
                    if (dimrModel == null) return;
                    dimrModel.SetVar(new[] { true }, IsPartOf1D2DModelPropertyName);
                    dimrModel.SetVar(new[] { true }, DisableFlowNodeRenumberingPropertyName);
                }
            }
        }

        public IEventedList<Iterative1D2DCouplerLink> Features
        {
            get
            {
                if (!featuresGenerated)
                {
                    RefreshMappings();
                }

                return lineSegments;
            } 
            set { throw new NotImplementedException(); }
        }

        public ICoordinateSystem CoordinateSystem
        {
            get
            {
                if (HydroModel == null || HydroModel.Region == null) return null;
                return HydroModel.Region.CoordinateSystem;
            }
        }

        private bool LogDebugMessages
        {
            get { return iterative1D2DCouplerData.Debug && HydroModelApplicationPlugin.IterativeCouplerAppender != null; }
        }

        public List<FeatureCoverage> LinkCoverages
        {
            get
            {
                if (linkCoverages == null)
                {
                    Generate1D2DLinkCoverages();
                }
                return linkCoverages;
            }
        }

        public IHydroModelWorkFlowData Data
        {
            get { return iterative1D2DCouplerData; }
            set
            {
                iterative1D2DCouplerData = (Iterative1D2DCouplerData) value;

                if (iterative1D2DCouplerData == null) return;

                iterative1D2DCouplerData.Coupler = this;
                iterative1D2DCouplerData.ForceDebugLoggingAction = ForceDebugLogging;
                iterative1D2DCouplerData.RefreshMappingAction = RefreshMappings;
                ForceDebugLogging(iterative1D2DCouplerData.Debug);
            }
        }

        protected override void OnInitialize()
        {
            linkCoverages = null;
            iterative1D2DCouplerData.OutputDataItems = null;
        }

        protected override void OnExecute()
        {
            
        }

        protected override void OnCleanUp()
        {
            base.OnCleanUp();

            if (!LogDebugMessages) return;

            HydroModelApplicationPlugin.IterativeCouplerAppender.Enabled = false;
            HydroModelApplicationPlugin.IterativeCouplerAppender.Messages.Clear();
        }

        public void Dispose()
        {
            Flow1DModel = null;
            Flow2DModel = null;
        }

        protected override void OnFinish()
        {
            if (RunsInInIntegratedModel) return;

            base.OnFinish();

            if (Activities.Any(a => a.Status == ActivityStatus.Failed))
            {
                Status = ActivityStatus.Failed;
                return;
            }

            Generate1D2DLinkCoverages();

            if (!LogDebugMessages || HydroModelApplicationPlugin.IterativeCouplerAppender.Messages.Count == 0) return;

            AddLogFile(string.Join("\r\n", HydroModelApplicationPlugin.IterativeCouplerAppender.Messages));
        }

        private static void ForceDebugLogging(bool debug)
        {
            // force debug logging for log4net Iterative1D2DCoupler logger
            ((Logger) (log.Logger)).Level = debug? Level.Debug : null;
        }

        private void AddLogFile(string text)
        {
            var tag = Name + "Tag";
            var dataItem = ((ModelBase) HydroModel).GetDataItemByTag(tag);
            if (dataItem == null)
            {
                dataItem = new DataItem(new TextDocument(true) {Name = "Log " + Name}, DataItemRole.Output,
                    tag);
                HydroModel.DataItems.Add(dataItem);
            }

            ((TextDocument) dataItem.Value).Content = text;
        }

        private void RemoveLogFile()
        {
            var oldTextDocument = ((ModelBase) HydroModel).GetDataItemByTag(Name + "Tag");
            if (oldTextDocument != null)
            {
                HydroModel.DataItems.Remove(oldTextDocument);
            }
        }

        private static T GetFirstModelDataItemValueByType<T>(ITimeDependentModel timeDependentModel)
        {
            return timeDependentModel.AllDataItems.Select(di => di.Value).OfType<T>().FirstOrDefault();
        }

        private void FlowModel1DDiscretizationChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "IsEditing") return;

            var discretization = GetFirstModelDataItemValueByType<IDiscretization>(flow1DModel);
            if (discretization.IsEditing && sender == discretization) return;

            Console.WriteLine("Sync links");
            RefreshMappings();
        }

        private void FlowModel2DDiscretizationChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender != Flow2DModel || e.PropertyName != GridPropertyName) return;

            Console.WriteLine("Sync links");
            RefreshMappings();
        }

        private void RefreshMappings(bool forceMapping = false)
        {
            if (!iterative1D2DCouplerData.Refresh1D2DLinks && !forceMapping)
            {
                log.Debug("Skipped refresh of 1d/2d links.");
                return; 
            }
            
            log.Info("Starting refreshing of 1d/2d links.");

            // only clear these if we are doing a refresh - the check above MUST come first (else you get a persistence error when saving)
            
            lineSegments.Clear();

            // TODO: unstruc. grid should become dataItem as well:
            var dimrModel = flow2DModel as IDimrModel;
            if (dimrModel != null)
            {
                var gridArray = dimrModel.GetVar(GridPropertyName) as UnstructuredGrid[];
                if (gridArray == null || gridArray.Length == 0 || gridArray[0] == null) 
                {
                    log.Error("No unstructured grid");
                    return;
                }
                var grid = gridArray[0];
                
                var discretization = GetFirstModelDataItemValueByType<IDiscretization>(Flow1DModel);
                if (discretization == null)
                {
                    log.Error("No network discretization");
                    return;
                }

                IList<Edge> edgesAlongEmbankments = null;
                try
                {
                    edgesAlongEmbankments = ((IGridOperationApi)Flow2DModel).GetLinkedCells().Select(i => grid.Edges[i - 1]).ToList();
                }
                catch (Exception e)
                {
                    log.Error("Unable to generate 1d/2d links");
                    return;
                }

                var gridPoints1D = discretization.Locations.GetValues<INetworkLocation>().ToList();

                var segments = edgesAlongEmbankments.ConvertMultiThreaded(e => CreateCouplerLink(grid, e, gridPoints1D));
                
                lineSegments.AddRange(segments);
            }
            featuresGenerated = true;

            log.Info("Refreshing of 1d/2d links finished.");
        }

        private static Iterative1D2DCouplerLink CreateCouplerLink(UnstructuredGrid grid, Edge edge, IList<INetworkLocation> gridPoints1D)
        {
            var cells = grid.VertexToCellIndices[edge.VertexFromIndex]
                .Concat(grid.VertexToCellIndices[edge.VertexToIndex])
                .Select(i => grid.Cells[i])
                .ToList();

            var edgeCell = cells.FirstOrDefault(c =>
                c.VertexIndices.Contains(edge.VertexFromIndex) &&
                c.VertexIndices.Contains(edge.VertexToIndex));

            if (edgeCell == null)
            {
                throw new Exception("Mapped 2d cell should be on border");
            }

            var edgeCenter = edge.GetEdgeCenter(grid);
            var closestPoint = gridPoints1D.Select((p, i) => new Tuple<int, INetworkLocation, double>(i, p, p.Geometry.Coordinate.Distance(edgeCenter)))
                    .OrderBy(t => t.Item3).FirstOrDefault();

            if (closestPoint == null)
            {
                log.Error("No closest point found, linking 1d computational grid points to 2d grid cells failed. Regenerate either or both grids.");
                return null;
            }

            // edge <=> 1d cell
            return new Iterative1D2DCouplerLink
                {
                    Name = string.Format("{0} - cell {1}", closestPoint.Item2.Name, grid.Cells.IndexOf(edgeCell)),
                    Geometry = new LineString(new[] {edgeCell.Center, closestPoint.Item2.Geometry.Coordinate}),
                    LinkEdge = edge
                };
        }

        private void Generate1D2DLinkCoverages()
        {
            var flow2dDimrModel = Flow2DModel as IDimrModel;
            if (flow2dDimrModel == null) return;

            var timeSeriesList = flow2dDimrModel.GetVar(CellsToFeaturesName) as ITimeSeries[];
            if (timeSeriesList == null) return;

            if (!Features.Any()) return;

            linkCoverages = new List<FeatureCoverage>();
            
            var edgeToFeature = Features.ToDictionary(f => f.LinkEdge);

            foreach (var timeSeries in timeSeriesList)
            {
                var unit = timeSeries.Components[0].Unit;
                var linkFeatureCoverage = CreateLinkFeatureCoverage(timeSeries.Name, Features, (unit != null ? (IUnit)unit.Clone() : null));
                
                // set times
                linkFeatureCoverage.Time.SkipUniqueValuesCheck = true;
                linkFeatureCoverage.Time.SetValues(timeSeries.Time.Values);
                linkFeatureCoverage.Time.SkipUniqueValuesCheck = false;

                foreach (FlowLink flowLink in timeSeries.Arguments[1].Values)
                {
                    if (!edgeToFeature.ContainsKey(flowLink.Edge)) continue;

                    var argumentFeature = edgeToFeature[flowLink.Edge];
                    var valuesToSet = timeSeries.GetValues<double>(new VariableValueFilter<FlowLink>(timeSeries.Arguments[1], flowLink));
                    linkFeatureCoverage.SetValues(valuesToSet, new VariableValueFilter<IFeature>(linkFeatureCoverage.FeatureVariable, argumentFeature));
                }

                linkCoverages.Add(linkFeatureCoverage);
            }
        }

        private FeatureCoverage CreateLinkFeatureCoverage(string name, IEventedList<Iterative1D2DCouplerLink> features, IUnit unit)
        {
            var featureCoverage = new FeatureCoverage(name)
                {
                    IsEditable = false,
                    IsTimeDependent = true,
                    Features = new EventedList<IFeature>(features),
                    CoordinateSystem = CoordinateSystem,
                };

            var featureVariable = new Variable<IFeature>("link"){IsAutoSorted = false};
            featureCoverage.Arguments.Add(featureVariable);
            featureCoverage.Time.InterpolationType = InterpolationType.Linear;
            featureCoverage.Components.Add(new Variable<double> {Name = name, InterpolationType = InterpolationType.Linear, Unit = unit});
            featureVariable.SetValues(Features);

            return featureCoverage;
        }

        //TODO: implement this function in OnExecute (do not remove this function)
        private void HydroModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (linkCoverages == null || HydroModel == null || sender != HydroModel.Region || e.PropertyName != TypeUtils.GetMemberName<IHydroRegion>(r => r.CoordinateSystem)) return;

            foreach (var featureCoverage in linkCoverages)
            {
                featureCoverage.CoordinateSystem = CoordinateSystem;
            }
        }

        #region Implementation of IDimrModel

        public virtual string GetItemString(IDataItem dataItem)
        {
            var feature = dataItem.GetFeature();

            var category = feature.GetFeatureCategory();
            if (category == null)
                return string.Empty;

            var dataItemName = ((INetworkFeature)((dataItem.ValueConverter).OriginalValue)).Name;

            var parameterName = dataItem.GetParameterName();

            string nameWithoutHashTags = dataItemName.Replace("##", "~~");
            var concatNames = new List<string>(new[] { category, nameWithoutHashTags, parameterName });

            concatNames.RemoveAll(s => s == null);

            return string.Join("/", concatNames);
        }

        public virtual string GetExporterPath(string directoryName)
        {
            return Path.Combine(directoryName, Path.GetFileName(InputFile));
        }

        public virtual string LibraryName { get { return "flow1d2d"; } }
        
        public virtual string InputFile
        {
            get { return Path.GetFileName(ShortName + ".ini"); }
        }

        public virtual string DirectoryName
        {
            get { return "1d2dcoupler"; }
        }

        public virtual bool IsMasterTimeStep
        {
            get { return true; }
        }

        public virtual string ShortName
        {
            get { return "1d2d"; }
        }

        public virtual Type ExporterType
        {
            get { return typeof (Iterative1D2DCouplerExporter); }
        }

        public virtual bool CanRunParallel
        {
            get { return false; }
        }

        public virtual string MpiCommunicatorString
        {
            get { return null; }
        }

        public virtual string KernelDirectoryLocation
        {
            get { return DimrApiDataSet.Iterative1D2DDllPath; }
        }

        public virtual void DisconnectOutput()
        {
            linkCoverages = null;
            iterative1D2DCouplerData.OutputDataItems = null;
        }

        public virtual void ConnectOutput(string outputPath)
        {
            Generate1D2DLinkCoverages();
        }

        public virtual DateTime CurrentTime { get; set; }

        public virtual DateTime StartTime { get { return Flow1DModel.StartTime; } }

        public new virtual ActivityStatus Status { get { return base.Status; } set { base.Status = value; } }
        
        [EditAction]
        public virtual bool RunsInInIntegratedModel{get; set;}

        public virtual ValidationReport Validate()
        {
            var flow1DDimrModel = Flow1DModel as IDimrModel;
            var flow2DDimrModel = Flow2DModel as IDimrModel;
            if (flow1DDimrModel == null || flow2DDimrModel == null) return null;

            var report = new ValidationReport(Name + " (Flow1D2D)", new[]
            {
                flow1DDimrModel.Validate(),
                flow2DDimrModel.Validate()
            });
            return report;
        }

        public virtual Array GetVar(string category, string itemName = null, string parameter = null)
        {
            return new double[] {};
        }

        public virtual void SetVar(Array values, string category, string itemName = null, string parameter = null)
        {
        }
        #endregion

        #region Implementation of IDataItemOwner

        public virtual bool CanRename(IDataItem item)
        {
            return false;
        }

        public virtual bool CanRemove(IDataItem item)
        {
            return false;
        }

        public virtual bool CanCopy(IDataItem item)
        {
            return false;
        }

        #endregion

        #region Implementation of IModel

        public virtual bool IsDataItemActive(IDataItem dataItem)
        {
            return false;
        }

        public virtual bool IsDataItemValid(IDataItem dataItem)
        {
            return false;
        }

        public virtual bool IsLinkAllowed(IDataItem source, IDataItem target)
        {
            return false;
        }

        public virtual IEnumerable<IFeature> GetChildDataItemLocations(DataItemRole role)
        {
            yield break;
        }

        public virtual IEnumerable<IDataItem> GetChildDataItems(IFeature location)
        {
            yield break;
        }

        public virtual void UpdateLink(object data)
        {
        }

        public virtual IDataItem GetDataItemByValue(object value)
        {
            return null;
        }

        public virtual void ClearOutput()
        {
            linkCoverages = new List<FeatureCoverage>();
            iterative1D2DCouplerData.OutputDataItems = null;
        }

        public virtual IEventedList<IDataItem> DataItems { get; set; }
        public virtual IEnumerable<IDataItem> AllDataItems { get { return Enumerable.Empty<IDataItem>(); } }
        public virtual string KernelVersions { get { return string.Empty; } }
        public virtual object Owner { get; set; }
        public virtual bool IsCopyable { get { return false; } }
        public virtual bool OutputOutOfSync { get; set; }
        public virtual string ExplicitWorkingDirectory { get; set; }
        public virtual bool SuspendClearOutputOnInputChange { get; set; }

        #endregion
    }
}
