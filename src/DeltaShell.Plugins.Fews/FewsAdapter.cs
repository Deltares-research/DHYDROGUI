using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.ModelExchange.Queries;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.Fews.Assemblers;
using log4net;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.Fews
{
    public class FewsAdapter
    {
        const string RouteReachSegmentsSuffix = "_" + FunctionAttributes.StandardFeatureNames.ReachSegment + "s";
        const string RouteGridPointsSuffix = "_" + FunctionAttributes.StandardFeatureNames.GridPoint + "s";

        private static readonly ILog log = LogManager.GetLogger(typeof(FewsAdapter));

        private ITimeDependentModel model;
        private ExtendedQueryContext extendedQueryContext;

        private bool initializeExportActionsHasBeenPerformed;
        private bool exportingAll;

        private IApplication Application { get; set; }


        public FewsAdapter(IApplication application)
        {
            Application = application;
        }

        public void ExportAll(string filePath, ITimeDependentModel timeDependentModel)
        {
            var model = timeDependentModel as ModelBase;
            if (model == null || model.OutputIsEmpty)
            {
                log.Error("Cannot export, please run model before exporting.");
                return;
            }
            exportingAll = true;
            ExportCsvFile(filePath, timeDependentModel);
            ExportShapeFile(filePath, timeDependentModel);
            ExportLongitudinalProfileDefinitions(filePath, timeDependentModel);
            Finish();
        }

        public void ExportCsvFile(string filePath, ITimeDependentModel timeDependentModel)
        {
            // this is very bad, refactor SOBEK 3.0 to make it work nice (use common data types!)
            Environment.SetEnvironmentVariable("UGLY_FEWS_HACK", "true");

            InitializeExportAction(filePath, timeDependentModel);
            if (model == null) return;

            var queryResults = extendedQueryContext.GetAll();
            var lines = AggregationResult.ToSeperatedValues(queryResults);
            var outputFileName = filePath;
            File.WriteAllLines(outputFileName, lines);

            Environment.SetEnvironmentVariable("UGLY_FEWS_HACK", "false");
            if(!exportingAll) Finish();
        }

        public void ExportShapeFile(string filePath, ITimeDependentModel timeDependentModel)
        {
            InitializeExportAction(filePath, timeDependentModel);
            if (model == null) return;
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string nodeFileName = fileName + "_node_data_items";
            var nodeFeatureCollection = NodeFeatureCollectionAssembler.Assemble(extendedQueryContext.GetAll());
            if (nodeFeatureCollection.Count > 0)
                ShapeFileWriter.Create(Path.GetDirectoryName(filePath), nodeFileName, nodeFeatureCollection);

            try
            {
                string lineFileName = fileName + "_line_data_items";
                var linesFeatureCollection = LineFeatureCollectionAssembler.Assemble(extendedQueryContext.GetAll(), extendedQueryContext.Discretization);
                if (linesFeatureCollection.Count > 0)
                    ShapeFileWriter.Create(Path.GetDirectoryName(filePath), lineFileName, linesFeatureCollection);
            }
            catch (Exception ex)
            {
                log.Error("Error exporting shape file: " + ex.Message);
            }
            
            if (!exportingAll) Finish();
        }

        private void Finish()
        {
            exportingAll = false;
            initializeExportActionsHasBeenPerformed = false;
            Environment.SetEnvironmentVariable("UGLY_FEWS_HACK", "false");
        }

        private void InitializeExportAction(string inputFile, ITimeDependentModel timeDependentModel)
        {
            if (initializeExportActionsHasBeenPerformed) return;

            if (string.IsNullOrEmpty(inputFile) || inputFile.Trim() == "")
                throw new ArgumentNullException("inputFile");

            model = timeDependentModel;

            extendedQueryContext = new ExtendedQueryContext(Application.Project);
            
            initializeExportActionsHasBeenPerformed = true;
        }

        private void ExportLongitudinalProfileDefinitions(string filePath, ITimeDependentModel timeDependentModel)
        {
            InitializeExportAction(filePath, timeDependentModel);
            if (model == null) return;

            // write Routes                
            string[] profileLocTypes = new[] { FunctionAttributes.StandardFeatureNames.GridPoint, FunctionAttributes.StandardFeatureNames.ReachSegment };
            try
            {
                foreach (Route route in GetRoutes())
                {
                    foreach (string profileVar in profileLocTypes)
                    {
                        string parameterId = (profileVar == FunctionAttributes.StandardFeatureNames.GridPoint)
                            ? FunctionAttributes.StandardNames.WaterLevel
                            : FunctionAttributes.StandardNames.WaterDischarge;

                        var coverage =
                            GetOutputNetworkCoverages().FirstOrDefault(
                                c => c.GetParameterId() == parameterId);

                        if (coverage == null)
                        {
                            log.WarnFormat(
                                "No profile definition written for {0} (no network spatial data found for {1}",
                                profileVar, parameterId);
                            continue;
                        }

                        var profileDefinitionsAssembler = new BranchesComplexTypeAssembler
                        {
                            NetworkCoverage = coverage,
                            Route = route
                        };
                        BranchesComplexType profileDefinitionDto = new BranchesComplexType();
                        string locTypeExtension = profileVar.Equals(FunctionAttributes.StandardFeatureNames.ReachSegment)
                            ? RouteReachSegmentsSuffix
                            : RouteGridPointsSuffix;
                        profileDefinitionsAssembler.AssembleDto(locTypeExtension, profileDefinitionDto);
                        string profileDefinitionFilePath = "profile_definition_" + route.Name + "_" + profileVar +
                                                           "s.xml";
                        profileDefinitionFilePath = Path.Combine(Path.GetDirectoryName(filePath),
                            profileDefinitionFilePath);
                        profileDefinitionDto.geoDatum = coverage.CoordinateSystem != null
                            ? GetCoordinateSystemInFewGeoDatum(
                                (int) (coverage.CoordinateSystem.AuthorityCode & 0xFFFFFFFF))
                            : "UNKNOWN";

                        profileDefinitionDto.SaveToFile(profileDefinitionFilePath);
                    }
                }
            }
            catch(Exception ex)
            {
                log.Error("Error exporting longitudinal profile definitions file: " + ex.Message);
            }
            if (!exportingAll) Finish();
        }

        private static string GetCoordinateSystemInFewGeoDatum(int AuthorityCode)
        {
            if (!Enum.IsDefined(typeof(BranchesComplexType.geoDatumEnumStringType), AuthorityCode))
            {
                return "UNKNOWN EPSG Authority Code CODE : " + AuthorityCode;
            }
            
            return BranchesComplexType.GetXmlEnumAttributeValueFromEnum((BranchesComplexType.geoDatumEnumStringType)AuthorityCode);
        }

        /// <summary>
        /// Hack to get th route from the HydroNetworkEditorViewContext
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Route> GetRoutes()
        {
            int routeCount = 0;
            var routes = Application.Project.GetAllItemsRecursive().OfType<Route>();
            foreach (var route in routes)
            {
                routeCount++;
                yield return route;
            }

            if (routeCount > 0)
                yield break;

            // fallback if previous fails
            foreach (var item in Application.Project.GetAllItemsRecursive())
            {
                if (item.GetType().Name == "HydroNetworkEditorViewContext")
                {
                    foreach (var propertyInfo in item.GetType().GetProperties())
                    {
                        var map = propertyInfo.GetValue(item, null);
                        if (map != null && map.GetType().Name == "Map")
                        {
                            var layersPropInfo = map.GetType().GetProperty("Layers");
                            var layers = layersPropInfo.GetValue(map, null) as IEnumerable;
                            if (layers != null)
                            {
                                foreach (var layer in layers)
                                {
                                    if (layer.GetType().Name == "RouteGroupLayer")
                                    {
                                        var routePropertyInfo = layer.GetType().GetProperty("Route");
                                        yield return routePropertyInfo.GetValue(layer, null) as Route;
                                    }
                                }
                            }
                        }
                    }
                    break;
                }
            }
        }
   
        private IEnumerable<NetworkCoverage> GetOutputNetworkCoverages()
        {
            var parents = extendedQueryContext
                .GetAllByFeatureOwner<NetworkCoverage>()
                .Where(p => p.ExchangeType == ExchangeType.Output);

            // return no duplicates
            return new HashSet<NetworkCoverage>(parents.Select(r => r.FeatureOwner as NetworkCoverage));
        }
    }
}