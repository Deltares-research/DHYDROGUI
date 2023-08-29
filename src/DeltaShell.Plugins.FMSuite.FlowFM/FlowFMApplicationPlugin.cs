using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.Common.IO.ImportExport;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.ImportersExporters;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using Mono.Addins;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    [Extension(typeof(IPlugin))]
    public class FlowFMApplicationPlugin : ApplicationPlugin, IDataAccessListenersProvider
    {
        private static ILog Log = LogManager.GetLogger(typeof(FlowFMApplicationPlugin));
        private IApplication application;

        public override string Name => "Delft3D FM";

        public override string DisplayName => "D-Flow Flexible Mesh Plugin";

        public override string Description => Properties.Resources.FlowFMApplicationPlugin_Description;

        public override string Version => AssemblyUtils.GetAssemblyInfo(GetType().Assembly).Version;

        public override string FileFormatVersion => "1.4.0.0";

        public override IApplication Application
        {
            get => application;
            set
            {
                if (application != null)
                {
                    application.ProjectOpened -= Application_ProjectOpened;
                }

                application = value;

                if (application != null)
                {
                    application.ProjectOpened += Application_ProjectOpened;
                }
            }
        }

        public override IEnumerable<ModelInfo> GetModelInfos()
        {
            yield return new ModelInfo
            {
                Name = "Flow Flexible Mesh Model",
                Category = "1D / 2D / 3D Standalone Models",
                Image = Properties.Resources.unstrucModel,
                GetParentProjectItem = owner =>
                {
                    Folder rootFolder = Application?.Project?.RootFolder;
                    return ApplicationPluginHelper.FindParentProjectItemInsideProject(rootFolder, owner) ?? rootFolder;
                },
                AdditionalOwnerCheck = owner =>
                    !(owner is ICompositeActivity) // Allow "standalone" flow models
                    || !((ICompositeActivity)owner).Activities.OfType<WaterFlowFMModel>().Any() &&
                    owner is IHydroModel, // Don't allow multiple flow models in one composite activity
                CreateModel = owner => new WaterFlowFMModel { WorkingDirectoryPathFunc = () => Application.WorkDirectory }
            };
        }

        public override IEnumerable<IFileImporter> GetFileImporters()
        {
            yield return new WaterFlowFMFileImporter(() => Application.WorkDirectory);
            yield return new Area2DStructuresImporter { GetModelForArea = GetModelForArea };
            yield return new StructuresListImporter(StructuresListType.Pumps) { GetModelForList = GetModelForCollection };
            yield return new StructuresListImporter(StructuresListType.Weirs) { GetModelForList = GetModelForCollection };
            yield return new FMMapFileImporter();
            yield return new FMHisFileImporter();
            yield return new FMRestartFileImporter(GetWaterFlowFMModels);
            yield return new BcFileImporter();
            yield return new BcmFileImporter();
            yield return new SamplesImporter();
            yield return new GroupablePointCloudImporter
            {
                GetBaseFolder = list =>
                {
                    WaterFlowFMModel model = GetWaterFlowFMModels()
                        .FirstOrDefault(m => Equals(m.Area.DryPoints, list));
                    return model == null ? string.Empty : Path.GetDirectoryName(model.MduFilePath);
                }
            };

            yield return new PlizFileImporterExporter<FixedWeir, FixedWeir>
            {
                Mode = Feature2DImportExportMode.Import,
                CreateDelegate = delegate (List<Coordinate> points1, string name1)
                {
                    var feature1 = new FixedWeir
                    {
                        Name = name1,
                        Geometry = LineStringCreator.CreateLineString(points1)
                    };
                    return feature1;
                },
                EqualityComparer = new GroupableFeatureComparer<FixedWeir>(),
                AfterImportAction = featureList =>
                {
                    WaterFlowFMModel waterFlowFmModel = GetModelFor(featureList, a => a.FixedWeirs);
                    string scheme = waterFlowFmModel
                                    .ModelDefinition.GetModelProperty(KnownProperties.FixedWeirScheme)
                                    .GetValueAsString();
                    var warningMessages = new StringBuilder();

                    foreach (ModelFeatureCoordinateData<FixedWeir> fixedWeirsProperty in waterFlowFmModel
                        .FixedWeirsProperties)
                    {
                        FixedWeir fixedWeir = fixedWeirsProperty.Feature;
                        fixedWeirsProperty.UpdateDataColumns(scheme);

                        bool locationKeyFound = fixedWeir.Attributes.ContainsKey(Feature2D.LocationKey);
                        int indexKey = !locationKeyFound
                                           ? -1
                                           : fixedWeir.Attributes.Keys.ToList().IndexOf(Feature2D.LocationKey);

                        int numberFixedWeirAttributes =
                            !locationKeyFound ? fixedWeir.Attributes.Count : fixedWeir.Attributes.Count - 1;
                        int difference = Math.Abs(fixedWeirsProperty.DataColumns.Count - numberFixedWeirAttributes);

                        if (fixedWeirsProperty.DataColumns.Count < fixedWeir.Attributes.Count)
                        {
                            warningMessages.AppendLine($"Based on the Fixed Weir Scheme {scheme}, " +
                                                       $"there are too many column(s) defined for {fixedWeir} in the imported fixed weir file. " +
                                                       $"The last {difference} column(s) have been ignored");
                        }

                        if (fixedWeirsProperty.DataColumns.Count > fixedWeir.Attributes.Count)
                        {
                            warningMessages.AppendLine($"Based on the Fixed Weir Scheme {scheme}, " +
                                                       $"there are not enough column(s) defined for {fixedWeir} in the imported fixed weir file. " +
                                                       $"The last {difference} column(s) have been generated using default values");
                        }

                        for (var index = 0; index < fixedWeirsProperty.DataColumns.Count; index++)
                        {
                            if (index < fixedWeir.Attributes.Count)
                            {
                                if (index == indexKey)
                                {
                                    continue;
                                }

                                IDataColumn dataColumn = fixedWeirsProperty.DataColumns[index];
                                var attributeWithListOfLoadedData =
                                    fixedWeir.Attributes[PliFile<FixedWeir>.NumericColumnAttributesKeys[index]] as
                                        GeometryPointsSyncedList<double>;
                                dataColumn.ValueList = attributeWithListOfLoadedData?.ToList();
                            }
                            else
                            {
                                break;
                            }
                        }

                        fixedWeir.Attributes.Clear(); //To Do during last step of cleaning. Turn this on.
                    }

                    var message = warningMessages.ToString();
                    if (!string.IsNullOrEmpty(message))
                    {
                        Log.Warn(message);
                    }
                },
                AfterCreateAction = (featureList, fixedWeir) =>
                {
                    WaterFlowFMModel waterFlowFmModel = GetModelFor(featureList, a => a.FixedWeirs);
                    fixedWeir.UpdateGroupName(waterFlowFmModel);
                },
                GetEditableObject = target => GetModelFor(target, a => a.FixedWeirs).Area
            };

            yield return new PlizFileImporterExporter<BridgePillar, BridgePillar>
            {
                Mode = Feature2DImportExportMode.Import,
                CreateDelegate =
                    delegate (List<Coordinate> points, string name) { return MduFile.CreateDelegateBridgePillar(name, points); },
                EqualityComparer = new GroupableFeatureComparer<BridgePillar>(),
                AfterCreateAction = delegate (object featureList, BridgePillar bridgePillar)
                {
                    WaterFlowFMModel waterFlowFmModel = GetModelFor(featureList, a => a.BridgePillars);
                    if (waterFlowFmModel == null)
                    {
                        return;
                    }

                    bridgePillar.UpdateGroupName(waterFlowFmModel);

                    var modelFeatureCoordinateData =
                        new ModelFeatureCoordinateData<BridgePillar>() { Feature = bridgePillar };
                    modelFeatureCoordinateData.UpdateDataColumns();
                    MduFile.SetBridgePillarDataModel(waterFlowFmModel.BridgePillarsDataModel,
                                                     modelFeatureCoordinateData, bridgePillar);

                    bridgePillar.Attributes.Clear(); //To Do during last step of cleaning. Turn this on. 
                },
                GetEditableObject = target => GetModelFor(target, a => a.BridgePillars).Area
            };

            yield return new PliFileImporterExporter<ThinDam2D, ThinDam2D>
            {
                Mode = Feature2DImportExportMode.Import,
                EqualityComparer = new GroupableFeatureComparer<ThinDam2D>(),
                AfterCreateAction = (target, w) => w.UpdateGroupName(GetModelFor(target, a => a.ThinDams)),
                GetEditableObject = target => GetModelFor(target, a => a.ThinDams).Area
            };

            yield return new PliFileImporterExporter<ObservationCrossSection2D, ObservationCrossSection2D>
            {
                Mode = Feature2DImportExportMode.Import,
                EqualityComparer = new GroupableFeatureComparer<ObservationCrossSection2D>(),
                AfterCreateAction =
                    (target, w) => w.UpdateGroupName(GetModelFor(target, a => a.ObservationCrossSections)),
                GetEditableObject = target => GetModelFor(target, a => a.ObservationCrossSections).Area
            };
            yield return new PliFileImporterExporter<Structure, Structure>
            {
                Mode = Feature2DImportExportMode.Import,
                CreateFromFeature = f => new Structure() { Name = f.Name, Geometry = f.Geometry },
                GetFeature = w => new Structure()
                {
                    Name = w.Name,
                    Geometry = w.Geometry
                },
                EqualityComparer = new GroupableFeatureComparer<Structure>(),
                AfterCreateAction = (target, w) => w.UpdateGroupName(GetModelFor(target, a => a.Structures)),
                GetEditableObject = target => GetModelFor(target, a => a.Structures).Area
            };
            yield return new PliFileImporterExporter<Pump, Pump>
            {
                Mode = Feature2DImportExportMode.Import,
                CreateFromFeature = f => new Pump() { Name = f.Name, Geometry = f.Geometry },
                CreateDelegate = (points1, name1) =>
                {
                    var pump = new Pump()
                    {
                        Name = name1,
                        Geometry = LineStringCreator.CreateLineString(points1)
                    };
                    return pump;
                },
                GetFeature = w => new Pump()
                {
                    Name = w.Name,
                    Geometry = w.Geometry
                },
                EqualityComparer = new GroupableFeatureComparer<Pump>(),
                AfterCreateAction = (target, w) => w.UpdateGroupName(GetModelFor(target, a => a.Pumps)),
                GetEditableObject = target => GetModelFor(target, a => a.Pumps).Area
            };
            yield return new PliFileImporterExporter<SourceAndSink, Feature2D>
            {
                Mode = Feature2DImportExportMode.Import,
                CreateFromFeature = f => new SourceAndSink
                {
                    Area = 1.0,
                    Feature = f
                },
                GetFeature = s => s.Feature,
                CreateDelegate =
                    (points, name) =>
                        points.Count == 1
                            ? new Feature2DPoint
                            {
                                Name = name,
                                Geometry = new Point(points[0])
                            }
                            : new Feature2D
                            {
                                Name = name,
                                Geometry = LineStringCreator.CreateLineString(points)
                            }
            };
            yield return new PliFileImporterExporter<BoundaryConditionSet, Feature2D>
            {
                Mode = Feature2DImportExportMode.Import,
                CreateFromFeature = f => new BoundaryConditionSet { Feature = f },
                GetFeature = b => b.Feature
            };

            yield return new PointFileImporterExporter { Mode = Feature2DImportExportMode.Import };
            yield return new ObsFileImporterExporter<GroupableFeature2DPoint>
            {
                Mode = Feature2DImportExportMode.Import,
                EqualityComparer = new GroupableFeatureComparer<GroupableFeature2DPoint>(),
                AfterCreateAction = (target, w) => w.UpdateGroupName(GetModelFor(target, a => a.ObservationPoints)),
                GetEditableObject = target => GetModelFor(target, a => a.ObservationPoints).Area
            };
            yield return new PolFileImporterExporter
            {
                Mode = Feature2DImportExportMode.Import,
                EqualityComparer = new GroupableFeatureComparer<GroupableFeature2DPolygon>(),
                AfterCreateAction =
                    (target, w) => w.UpdateGroupName(GetModelFor(target, a => a.Enclosures, a => a.DryAreas)),
                GetEditableObject = target => GetModelFor(target, a => a.Enclosures, a => a.DryAreas).Area
            };
            yield return new LdbFileImporterExporter
            {
                Mode = Feature2DImportExportMode.Import,
                EqualityComparer = new GroupableFeatureComparer<LandBoundary2D>(),
                AfterCreateAction = (target, w) => w.UpdateGroupName(GetModelFor(target, a => a.LandBoundaries)),
                GetEditableObject = target => GetModelFor(target, a => a.LandBoundaries).Area
            };

            yield return new FlowFMNetFileImporter { GetModelForGrid = GetModelForGrid };
            yield return new TimFileImporter
            {
                WindFileImporter = false,
                GetModelForSourceAndSink = GetModelForSourceAndSink,
                GetModelForHeatFluxModel = GetModelForHeatFluxModel
            };
            yield return new TimFileImporter
            {
                WindFileImporter = true,
                GetModelForWindTimeSeries = GetModelForWindField
            };

            // ShapeFileImporter
            yield return ShapeFileImporterFactory.Construct<ILineString, LandBoundary2D>(
                ShapeFileImporterFactory.AfterFeatureCreateActions.TryAddName);
            yield return ShapeFileImporterFactory.Construct<IPoint, GroupablePointFeature>(); // DryPoints
            yield return ShapeFileImporterFactory.Construct<IPolygon, GroupableFeature2DPolygon>(
                ShapeFileImporterFactory.AfterFeatureCreateActions.TryAddName); // DryAreas | Enclosure
            yield return ShapeFileImporterFactory.Construct<ILineString, ThinDam2D>(
                ShapeFileImporterFactory.AfterFeatureCreateActions.TryAddName);
            yield return ShapeFileImporterFactory.Construct<ILineString, FixedWeir>(
                ShapeFileImporterFactory.AfterFeatureCreateActions.TryAddName);
            yield return ShapeFileImporterFactory.Construct<IPoint, GroupableFeature2DPoint>(
                ShapeFileImporterFactory.AfterFeatureCreateActions.TryAddName); // ObservationPoint
            yield return ShapeFileImporterFactory.Construct<ILineString, ObservationCrossSection2D>(
                ShapeFileImporterFactory.AfterFeatureCreateActions.TryAddName);
            yield return ShapeFileImporterFactory.Construct<ILineString, BridgePillar>(
                ShapeFileImporterFactory.AfterFeatureCreateActions.TryAddName);
            yield return ShapeFileImporterFactory.Construct<ILineString, Pump>(
                ShapeFileImporterFactory.AfterFeatureCreateActions.Chain<Pump>(
                    ShapeFileImporterFactory.AfterFeatureCreateActions.TryAddName,
                    ShapeFileImporterFactory.AfterFeatureCreateActions.TryAddCapacity));
            yield return ShapeFileImporterFactory.Construct<ILineString, Structure>(
                ShapeFileImporterFactory.AfterFeatureCreateActions.Chain<Structure>(
                    ShapeFileImporterFactory.AfterFeatureCreateActions.TryAddName,
                    ShapeFileImporterFactory.AfterFeatureCreateActions.TryAddWeirFormula,
                    ShapeFileImporterFactory.AfterFeatureCreateActions.TryAddCrestWidth,
                    ShapeFileImporterFactory.AfterFeatureCreateActions.TryAddCrestLevel));
        }

        public override IEnumerable<IFileExporter> GetFileExporters()
        {
            yield return new WaterFlowFMFileExporter();
            yield return new Area2DStructuresExporter { GetModelForArea = GetModelForArea };
            yield return new StructuresListExporter(StructuresListType.Pumps) { GetModelForList = GetModelForCollection };
            yield return new StructuresListExporter(StructuresListType.Weirs) { GetModelForList = GetModelForCollection };
            yield return new BcFileExporter { GetRefDateForBoundaryCondition = GetRefDateForBoundaryCondition };
            yield return new BcmFileExporter { GetRefDateForBoundaryCondition = GetRefDateForBoundaryCondition };
            yield return new SamplesExporter();
            yield return new PlizFileImporterExporter<FixedWeir, FixedWeir>
            {
                Mode = Feature2DImportExportMode.Export,
                BeforeExportActionDelegate = delegate (object featureList)
                {
                    WaterFlowFMModel waterFlowFmModel = GetModelFor(featureList, a => a.FixedWeirs);
                    var fixedWeirs = featureList as IEnumerable<FixedWeir>;
                    if (fixedWeirs == null || waterFlowFmModel == null)
                    {
                        return;
                    }

                    foreach (FixedWeir fixedWeir in fixedWeirs)
                    {
                        fixedWeir.Attributes = new DictionaryFeatureAttributeCollection();
                        ModelFeatureCoordinateData<FixedWeir> correspondingModelFeatureCoordinateData =
                            waterFlowFmModel.FixedWeirsProperties.FirstOrDefault(d => d.Feature == fixedWeir);

                        if (correspondingModelFeatureCoordinateData == null)
                        {
                            return;
                        }

                        for (var index = 0; index < correspondingModelFeatureCoordinateData.DataColumns.Count; index++)
                        {
                            if (!correspondingModelFeatureCoordinateData.DataColumns[index].IsActive)
                            {
                                break;
                            }

                            IList dataColumnWithData =
                                correspondingModelFeatureCoordinateData.DataColumns[index].ValueList;

                            GeometryPointsSyncedList<double> syncedList;
                            syncedList = new GeometryPointsSyncedList<double>
                            {
                                CreationMethod = (f, i) => 0.0,
                                Feature = fixedWeir
                            };
                            fixedWeir.Attributes[PliFile<FixedWeir>.NumericColumnAttributesKeys[index]] = syncedList;

                            for (var i = 0; i < dataColumnWithData.Count; ++i)
                            {
                                syncedList[i] = (double)dataColumnWithData[i];
                            }
                        }
                    }
                },
                AfterExportActionDelegate = delegate (object featureList)
                {
                    var fixedWeirs = featureList as IEnumerable<FixedWeir>;
                    if (fixedWeirs == null)
                    {
                        return;
                    }

                    fixedWeirs.ForEach(fw => fw.Attributes.Clear());
                }
            };
            yield return new PlizFileImporterExporter<BridgePillar, BridgePillar>
            {
                Mode = Feature2DImportExportMode.Export,
                BeforeExportActionDelegate = delegate (object featureList)
                {
                    WaterFlowFMModel waterFlowFmModel = GetModelFor(featureList, a => a.BridgePillars);
                    var bridgePillars = featureList as IEnumerable<BridgePillar>;
                    if (bridgePillars == null || waterFlowFmModel == null)
                    {
                        return;
                    }

                    IList<ModelFeatureCoordinateData<BridgePillar>> modelFeatureCoordinateDatas =
                        waterFlowFmModel.BridgePillarsDataModel;
                    MduFile.SetBridgePillarAttributes(bridgePillars, modelFeatureCoordinateDatas);
                },
                AfterExportActionDelegate = delegate (object featureList)
                {
                    var bridgePillars = featureList as IEnumerable<BridgePillar>;
                    if (bridgePillars == null)
                    {
                        return;
                    }

                    MduFile.CleanBridgePillarAttributes(bridgePillars);
                }
            };
            yield return new PliFileImporterExporter<ThinDam2D, ThinDam2D> { Mode = Feature2DImportExportMode.Export };
            yield return
                new PliFileImporterExporter<ObservationCrossSection2D, ObservationCrossSection2D> { Mode = Feature2DImportExportMode.Export };
            yield return new PliFileImporterExporter<Structure, Structure>
            {
                Mode = Feature2DImportExportMode.Export,
                CreateFromFeature = f => new Structure() { Name = f.Name, Geometry = f.Geometry },
                GetFeature = w => new Structure
                {
                    Name = w.Name,
                    Geometry = w.Geometry
                }
            };
            yield return new PliFileImporterExporter<Pump, Pump>
            {
                Mode = Feature2DImportExportMode.Export,
                CreateFromFeature = f => new Pump() { Name = f.Name, Geometry = f.Geometry },
                GetFeature = w => new Pump
                {
                    Name = w.Name,
                    Geometry = w.Geometry
                }
            };
            yield return new PliFileImporterExporter<SourceAndSink, Feature2D>
            {
                Mode = Feature2DImportExportMode.Export,
                CreateFromFeature = f => new SourceAndSink
                {
                    Area = 1.0,
                    Feature = f
                },
                GetFeature = s => s.Feature
            };
            yield return new PliFileImporterExporter<BoundaryConditionSet, Feature2D>
            {
                Mode = Feature2DImportExportMode.Export,
                CreateFromFeature = f => new BoundaryConditionSet { Feature = f },
                GetFeature = b => b.Feature
            };
            yield return new PointFileImporterExporter { Mode = Feature2DImportExportMode.Export };
            yield return new ObsFileImporterExporter<GroupableFeature2DPoint> { Mode = Feature2DImportExportMode.Export };
            yield return new PolFileImporterExporter { Mode = Feature2DImportExportMode.Export };
            yield return new LdbFileImporterExporter { Mode = Feature2DImportExportMode.Export };
            yield return new FlowFMNetFileExporter { GetModelForGrid = GetModelForGrid };
            yield return
                new FMModelPartitionExporter
                {
                    PolygonFile = null,
                    IsContiguous = true,
                    NumDomains = 1,
                    SolverType = 7
                };
            yield return
                new FMGridPartitionExporter
                {
                    GetModelForGrid = GetModelForGrid,
                    PolygonFile = null,
                    IsContiguous = true,
                    NumDomains = 1
                };
            yield return new GeometryZipExporter { GetModelForGrid = GetModelForGrid };
            yield return new TimFileExporter
            {
                GetModelForHeatFluxModel = GetModelForHeatFluxModel,
                GetModelForSourceAndSink = GetModelForSourceAndSink
            };
            yield return new ImportedNetFileXyzExporter { Output = GridPointsExporter.OutputMode.Vertices };
            yield return new ImportedNetFileXyzExporter { Output = GridPointsExporter.OutputMode.CellCenters };

            foreach (WindItemExporter windItemExporter in WindItemExporter.CreateExporters())
            {
                windItemExporter.ReferenceDateGetter = w => GetModelForWindField(w).ReferenceTime;
                yield return windItemExporter;
            }
        }

        public override IEnumerable<Assembly> GetPersistentAssemblies()
        {
            yield return typeof(WaterFlowFMModel).Assembly;
        }

        public IEnumerable<IDataAccessListener> CreateDataAccessListeners()
        {
            yield return new WaterFlowFMDataAccessListener();
        }

        private IEnumerable<WaterFlowFMModel> FlowModels =>
            Application != null
                ? GetWaterFlowFMModels()
                : Enumerable.Empty<WaterFlowFMModel>();

        private void Application_ProjectOpened(Project project)
        {
            project?.RootFolder.GetAllModelsRecursive().OfType<WaterFlowFMModel>().ForEach(
                m => m.WorkingDirectoryPathFunc =
                         () => application.WorkDirectory);
        }

        private WaterFlowFMModel GetModelFor<T>(object target, params Func<HydroArea, IEnumerable<T>>[] listSelectors)
            where T : IFeature, INameable
        {
            return Application?.Project?.RootFolder.GetAllModelsRecursive()
                              .OfType<WaterFlowFMModel>()
                              .FirstOrDefault(m => listSelectors.Any(s => Equals(s(m.Area), target)));
        }

        private WaterFlowFMModel GetModelForArea(HydroArea hydroArea)
        {
            return FlowModels.FirstOrDefault(m => Equals(m.Area, hydroArea));
        }

        private WaterFlowFMModel GetModelForHeatFluxModel(HeatFluxModel heatFluxModel)
        {
            return FlowModels.FirstOrDefault(m => Equals(m.ModelDefinition.HeatFluxModel, heatFluxModel));
        }

        private WaterFlowFMModel GetModelForSourceAndSink(SourceAndSink sourceAndSink)
        {
            return FlowModels.FirstOrDefault(m => m.SourcesAndSinks.Contains(sourceAndSink));
        }

        private WaterFlowFMModel GetModelForWindField(IWindField windField)
        {
            return FlowModels.FirstOrDefault(m => m.WindFields.Contains(windField));
        }

        private WaterFlowFMModel GetModelForCollection(IEnumerable list)
        {
            return FlowModels.FirstOrDefault(m => GetAvailableLists(m, list));
        }

        private DateTime? GetRefDateForBoundaryCondition(IBoundaryCondition boundaryCondition)
        {
            WaterFlowFMModel waterFlowFMModel = FlowModels.FirstOrDefault();
            return waterFlowFMModel == null ? (DateTime?)null : waterFlowFMModel.ReferenceTime;
        }

        private WaterFlowFMModel GetModelForGrid(UnstructuredGrid grid)
        {
            return FlowModels.FirstOrDefault(m => m.Grid.Equals(grid));
        }

        private static bool GetAvailableLists(WaterFlowFMModel model, IEnumerable list)
        {
            if (Equals(model.Area.Structures, list))
            {
                return true;
            }

            if (Equals(model.Area.Pumps, list))
            {
                return true;
            }
            // Add more if relevant...

            return false;
        }

        private IEnumerable<WaterFlowFMModel> GetWaterFlowFMModels() => Application.GetAllModelsInProject().OfType<WaterFlowFMModel>();
    }
}