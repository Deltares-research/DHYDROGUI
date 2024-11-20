using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using Deltares.Infrastructure.API.DependencyInjection;
using DeltaShell.NGHS.Common;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.NHibernate;
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
    public class FlowFMApplicationPlugin : ApplicationPlugin
    {
        public const string FlowFlexibleMeshModelModelInfoName = "Flow Flexible Mesh Model";
        public const string FM_MODEL_DEFAULT_PROJECT_TEMPLATE_ID = "FMModel";
        public const string FM_MODEL_MDU_IMPORT_PROJECT_TEMPLATE_ID = "FMModelMDUImport";

        private static readonly ILog log = LogManager.GetLogger(typeof(FlowFMApplicationPlugin));
        internal static string PluginVersion { get; } = typeof(FlowFMApplicationPlugin).Assembly.GetName().Version.ToString();
        internal static string PluginName { get; } = "D-Flow Flexible Mesh Plugin";

        private IApplication application;

        public override string Name => "Delft3D FM";

        public override string DisplayName => PluginName;

        public override string Description => Properties.Resources.FlowFMApplicationPlugin_Description;

        public override string Version => PluginVersion;

        public override string FileFormatVersion => "1.4.0.0";

        public override IApplication Application
        {
            get => application;
            set
            {
                if (application != null)
                {
                    Application.ProjectService.ProjectOpened -= Application_ProjectOpened;
                    Application.ProjectService.ProjectCreated -= Application_ProjectOpened;
                    Application.ProjectService.ProjectClosing -= Application_ProjectClosing;
                }

                application = value;

                if (application != null)
                {
                    Application.ProjectService.ProjectOpened += Application_ProjectOpened;
                    Application.ProjectService.ProjectCreated += Application_ProjectOpened;
                    Application.ProjectService.ProjectClosing += Application_ProjectClosing;
                }
            }
        }

        public override IEnumerable<ModelInfo> GetModelInfos()
        {
            yield return new ModelInfo
                {
                    Name = FlowFlexibleMeshModelModelInfoName,
                    Category = "1D / 2D / 3D Standalone Models",
                    Image = Properties.Resources.unstrucModel,
                    AdditionalOwnerCheck = owner => !(Application?.ProjectService.Project != null &&
                                                      Application.ProjectService.Project.RootFolder.GetAllModelsRecursive().OfType<WaterFlowFMModel>().Any()), // Don't allow multiple flow models
                    CreateModel = owner => new WaterFlowFMModel{ WorkingDirectoryPathFunc = () => Application?.WorkDirectory }
                };
        }

        public override IEnumerable<ProjectTemplate> ProjectTemplates()
        {
            yield return new ProjectTemplate
            {
                Id = FM_MODEL_DEFAULT_PROJECT_TEMPLATE_ID,
                Category = ProductCategories.NewTemplateCategory,
                Name = "FM model",
                Description = "Creates a new standalone flexible mesh model",
                ExecuteTemplateOpenView = (p, settings) =>
                {
                    var model = new WaterFlowFMModel();
                    if (settings is ModelSettings modelSettings)
                    {
                        model.Name = modelSettings.ModelName;
                        model.CoordinateSystem = modelSettings.CoordinateSystem;
                        if (modelSettings.UseModelNameForProject)
                        {
                            p.Name = model.Name;
                        }
                    }

                    p.RootFolder.Items.Add(model);

                    return model;
                }
            };
            yield return new ProjectTemplate
            {
                Id = FM_MODEL_MDU_IMPORT_PROJECT_TEMPLATE_ID,
                Category = ProductCategories.ImportTemplateCategory,
                Description = Properties.Resources.FlowFMApplicationPlugin_ProjectTemplates_Import_FM_mdu_as_model,
                Name = Properties.Resources.FlowFMApplicationPlugin_ProjectTemplates_DFlowFM_mdu_import,
                ExecuteTemplateOpenView = (project, o) =>
                {
                    if (!(o is string path) || !File.Exists(path))
                    {
                        return null;
                    }

                    var importer = new WaterFlowFMFileImporter(() => Application.WorkDirectory);

                    var fileImportActivity = new FileImportActivity(importer, project)
                    {
                        Files = new[]
                        {
                            path
                        }
                    };

                    fileImportActivity.OnImportFinished += (activity, importedObject, fileImporter) =>
                    {
                        project.RootFolder.Add(importedObject);
                    };

                    Application.ActivityRunner.Enqueue(fileImportActivity);
                    return fileImportActivity;
                }
            };
        }

        public override IEnumerable<IFileImporter> GetFileImporters()
        {
            yield return new LateralSourceImporter();
            yield return new WaterFlowFMFileImporter(() => Application?.WorkDirectory);
            yield return new WaterFlowFMIntoWaterFlowFMFileImporter();
            yield return new Area2DStructuresImporter { GetModelForArea = GetModelForArea };
            yield return new StructuresListImporter(StructuresListType.Pumps) { GetModelForList = GetModelForCollection };
            yield return new StructuresListImporter(StructuresListType.Weirs) { GetModelForList = GetModelForCollection };
            yield return new StructuresListImporter(StructuresListType.Gates) { GetModelForList = GetModelForCollection };
            yield return new FMMapFileImporter();
            yield return new FMHisFileImporter();
            yield return new BcFileImporter();
            yield return new BcFile1DImporter();
            yield return new BcmFileImporter();
            yield return new GroupablePointCloudImporter
            {
                GetRootDirectory = featureList => GetModelFor(featureList, a => a.DryPoints).GetModelDirectory(),
                GetBaseDirectory = featureList => GetModelFor(featureList, a => a.DryPoints).GetMduDirectory()
            };

            yield return new PliFileImporterExporter<Embankment, Embankment>
            {
                Mode = Feature2DImportExportMode.Import,
                CreateDelegate = delegate(List<Coordinate> points, string name)
                {
                    points.ForEach(p => p.Z = 0.0);
                    return new Embankment {Name = name, Geometry = PliFile<Embankment>.CreatePolyLineGeometry(points)};
                },
            };

            yield return new GisToFeature2DImporter<ILineString, Embankment>();

            yield return new PliFileImporterExporter<FixedWeir, FixedWeir>
            {
                Mode = Feature2DImportExportMode.Import,
                CreateDelegate = (points1, name1) => new FixedWeir
                {
                    Name = name1,
                    Geometry = PliFile<FixedWeir>.CreatePolyLineGeometry(points1)
                },
                EqualityComparer = new GroupableFeatureComparer<FixedWeir>(),
                AfterCreateAction = delegate(object featureList, FixedWeir fixedWeir)
                {
                    fixedWeir.UpdateGroupName(GetModelFor(featureList, a => a.FixedWeirs));
                    
                    var modelFeatureCoordinateData = new ModelFeatureCoordinateData<FixedWeir> {Feature = fixedWeir};
                    var scheme = GetModelFor(featureList, a => a.FixedWeirs).ModelDefinition
                        .GetModelProperty(KnownProperties.FixedWeirScheme).GetValueAsString();
                    modelFeatureCoordinateData.UpdateDataColumns(scheme);

                    var locationKeyFound = fixedWeir.Attributes.ContainsKey(Feature2D.LocationKey);
                    var indexKey = !locationKeyFound ? -1 : fixedWeir.Attributes.Keys.ToList().IndexOf(Feature2D.LocationKey);

                    var numberFixedWeirAttributes = !locationKeyFound ? fixedWeir.Attributes.Count : (fixedWeir.Attributes.Count - 1);

                    var difference = Math.Abs(modelFeatureCoordinateData.DataColumns.Count - numberFixedWeirAttributes);

                    
                    if (modelFeatureCoordinateData.DataColumns.Count < fixedWeir.Attributes.Count)
                    {
                        log.Warn($"Based on the Fixed Weir Scheme {scheme}, there are too many column(s) defined for {fixedWeir} in the imported fixed weir file. The last {difference} column(s) have been ignored");
                    }

                    if (modelFeatureCoordinateData.DataColumns.Count > fixedWeir.Attributes.Count)
                    {
                        log.Warn($"Based on the Fixed Weir Scheme {scheme}, there are not enough column(s) defined for {fixedWeir} in the imported fixed weir file. The last {difference} column(s) have been generated using default values");
                    }

                    for (var index = 0; index < modelFeatureCoordinateData.DataColumns.Count; index++)
                    {

                        if (index < fixedWeir.Attributes.Count)
                        {
                            if (index == indexKey) continue;

                            var dataColumn = modelFeatureCoordinateData.DataColumns[index];
                            var attributeWithListOfLoadedData =
                                fixedWeir.Attributes[PliFile<FixedWeir>.NumericColumnAttributesKeys[index]] as
                                    GeometryPointsSyncedList<double>;
                            dataColumn.ValueList = attributeWithListOfLoadedData.ToList();
                        }
                        else
                        {
                            break;
                        }
                    }
                    GetModelFor(featureList, a => a.FixedWeirs).FixedWeirsProperties.Add(modelFeatureCoordinateData);
                    
                        fixedWeir.Attributes.Clear(); //To Do during last step of cleaning. Turn this on. 
                    
                },

            GetEditableObject = target => GetModelFor(target, a => a.FixedWeirs).Area
            };

            yield return new GisToFeature2DImporter<ILineString, FixedWeir>();


            yield return new PlizFileImporterExporter<BridgePillar, BridgePillar>
            {
                Mode = Feature2DImportExportMode.Import,
                CreateDelegate = (points, name) => MduFile.CreateDelegateBridgePillar(name, points),
                EqualityComparer = new GroupableFeatureComparer<BridgePillar>(),
                AfterCreateAction = delegate (object featureList, BridgePillar bridgePillar)
                {
                    var waterFlowFmModel = GetModelFor(featureList, a => a.BridgePillars);
                    if (waterFlowFmModel == null) return;

                    bridgePillar.UpdateGroupName(waterFlowFmModel);

                    var modelFeatureCoordinateData = new ModelFeatureCoordinateData<BridgePillar> { Feature = bridgePillar };
                    modelFeatureCoordinateData.UpdateDataColumns();
                    MduFile.SetBridgePillarDataModel(waterFlowFmModel.BridgePillarsDataModel,modelFeatureCoordinateData,bridgePillar);


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

            yield return new GisToFeature2DImporter<ILineString, ThinDam2D>();

            yield return new PliFileImporterExporter<ObservationCrossSection2D, ObservationCrossSection2D>
                {
                    Mode = Feature2DImportExportMode.Import,
                    EqualityComparer = new GroupableFeatureComparer<ObservationCrossSection2D>() ,
                    AfterCreateAction = (target, w) => w.UpdateGroupName(GetModelFor(target, a => a.ObservationCrossSections)),
                    GetEditableObject = target => GetModelFor(target, a => a.ObservationCrossSections).Area
            };
            yield return new GisToFeature2DImporter<IPoint, ObservationCrossSection2D>();

            yield return new PliFileImporterExporter<Weir2D, Weir2D>
            {
                Mode = Feature2DImportExportMode.Import,
                CreateFromFeature = f => new Weir2D(f.Name, true) {Geometry = f.Geometry},
                GetFeature = w => new Weir2D {Name = w.Name, Geometry = w.Geometry},
                EqualityComparer = new GroupableFeatureComparer<Weir2D>(),
                AfterCreateAction = (target, w) => w.UpdateGroupName(GetModelFor(target, a => a.Weirs)),
                GetEditableObject = target => GetModelFor(target, a => a.Weirs).Area
            };

            yield return new PliFileImporterExporter<Pump2D, Pump2D>
            {
                Mode = Feature2DImportExportMode.Import,
                CreateFromFeature = f => new Pump2D(f.Name, true) { Geometry = f.Geometry },
                GetFeature = w => new Pump2D { Name = w.Name, Geometry = w.Geometry },
                EqualityComparer = new GroupableFeatureComparer<Pump2D>(),
                AfterCreateAction = (target, w) => w.UpdateGroupName(GetModelFor(target, a => a.Pumps)),
                GetEditableObject = target => GetModelFor(target, a => a.Pumps).Area
            };

            yield return new PliFileImporterExporter<Gate2D, Gate2D>
            {
                Mode = Feature2DImportExportMode.Import,
                CreateFromFeature = f => new Gate2D(f.Name) { Geometry = f.Geometry },
                GetFeature = w => new Gate2D { Name = w.Name, Geometry = w.Geometry },
                EqualityComparer = new GroupableFeatureComparer<Gate2D>(),
                AfterCreateAction = (target, w) => w.UpdateGroupName(GetModelFor(target, a => a.Gates)),
                GetEditableObject = target => GetModelFor(target, a => a.Gates).Area
            };

            yield return new PliFileImporterExporter<SourceAndSink, Feature2D>
            {
                Mode = Feature2DImportExportMode.Import,
                CreateFromFeature = f => new SourceAndSink {Area = 1.0, Feature = f},
                GetFeature = s => s.Feature,
                CreateDelegate =
                    (points, name) =>
                        points.Count == 1
                            ? new Feature2DPoint {Name = name, Geometry = new Point(points[0])}
                            : new Feature2D {Name = name, Geometry = PliFile<Feature2D>.CreatePolyLineGeometry(points)}
            };

            yield return new PliFileImporterExporter<BoundaryConditionSet, Feature2D>
            {
                Mode = Feature2DImportExportMode.Import,
                CreateFromFeature = f => new BoundaryConditionSet {Feature = f},
                GetFeature = b => b.Feature
            };

            yield return new PointFileImporterExporter
            {
                Mode = Feature2DImportExportMode.Import,
                EqualityComparer = new GroupableFeatureComparer<GroupableFeature2DPolygon>(),
                AfterCreateAction = (target, w) => w.UpdateGroupName(GetModelFor(target, a => a.Gullies)),
                GetEditableObject = target => GetModelFor(target, a => a.Gullies).Area
            };

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
                AfterCreateAction = (target, w) => w.UpdateGroupName(GetModelFor(target, a => a.Enclosures, a => a.DryAreas, a => a.RoofAreas)),
                GetEditableObject = target => GetModelFor(target, a => a.Enclosures, a => a.DryAreas, a => a.RoofAreas).Area
            };
            yield return new LdbFileImporterExporter
            {
                Mode = Feature2DImportExportMode.Import,
                EqualityComparer = new GroupableFeatureComparer<LandBoundary2D>(),
                AfterCreateAction = (target, w) => w.UpdateGroupName(GetModelFor(target, a => a.LandBoundaries)),
                GetEditableObject = target => GetModelFor(target, a => a.LandBoundaries).Area
            };

            yield return new GisToFeature2DImporter<ILineString, LandBoundary2D>();

            yield return new FlowFMNetFileImporter {GetModelForGrid = GetModelForGrid};

            yield return new TimFileImporter
                {
                    WindFileImporter = false,
                    GetModelForSourceAndSink = GetModelForSourceAndSink,
                    GetModelForHeatFluxModel = GetModelForHeatFluxModel,
                };
            yield return new TimFileImporter
                {
                    WindFileImporter = true,
                    GetModelForWindTimeSeries = GetModelForWindField
                };


            yield return new PliFileImporterExporter<Feature2D, Feature2D>
            {
                
                Mode = Feature2DImportExportMode.Import,
                CreateDelegate = delegate (List<Coordinate> points1, string name1)
                {
                    var feature1 = new LeveeBreach { Name = name1, Geometry = PliFile<Feature2D>.CreatePolyLineGeometry(points1) };
                    return feature1;
                },
                AfterCreateAction = (target, w) => (w as LeveeBreach)?.UpdateGroupName(GetModelFor(target, a => a.LeveeBreaches)),
                GetEditableObject = target => GetModelFor(target, a => a.LeveeBreaches).Area
            };

            yield return new GisToFeature2DImporter<ILineString, Feature2D>
            {
                CreateInstanceOfFeature2D = () => new LeveeBreach()
            };

            yield return new GisToFeature2DImporter<IPoint, Gully>();

            yield return new GisToFeature2DImporter<IPolygon, GroupableFeature2DPolygon>();
        }

        private WaterFlowFMModel GetModelFor<T>(object target, params Func<HydroArea, IEnumerable<T>>[] listSelectors) where T : IFeature
        {
            return Application?.ProjectService.Project?.RootFolder.GetAllModelsRecursive()
                .OfType<WaterFlowFMModel>()
                .FirstOrDefault(m => listSelectors.Any(s => Equals(s(m.Area),target)));
        }

        public override IEnumerable<IFileExporter> GetFileExporters()
        {
            yield return new FMModelFileExporter();
            yield return new Area2DStructuresExporter { GetModelForArea = GetModelForArea };
            yield return new StructuresListExporter(StructuresListType.Pumps) { GetModelForList = GetModelForCollection };
            yield return new StructuresListExporter(StructuresListType.Weirs) { GetModelForList = GetModelForCollection };
            yield return new StructuresListExporter(StructuresListType.Gates) { GetModelForList = GetModelForCollection };
            yield return new BcFileExporter {GetRefDateForBoundaryCondition = GetRefDateForBoundaryCondition};
            yield return new BcmFileExporter {GetRefDateForBoundaryCondition = GetRefDateForBoundaryCondition};
            yield return new PliFileImporterExporter<Embankment, Embankment> { Mode = Feature2DImportExportMode.Export };
            yield return new PliFileImporterExporter<FixedWeir, FixedWeir>
            {
                Mode = Feature2DImportExportMode.Export,
                BeforeExportActionDelegate = delegate (object featureList)
                {
                    var waterFlowFmModel = GetModelFor(featureList, a => a.FixedWeirs);
                    var fixedWeirs = featureList as IEnumerable<FixedWeir>;
                    if (fixedWeirs == null || waterFlowFmModel == null) return;

                    foreach (var fixedWeir in fixedWeirs)
                    {
                        fixedWeir.Attributes = new DictionaryFeatureAttributeCollection();
                        var correspondingModelFeatureCoordinateData = waterFlowFmModel?.FixedWeirsProperties.FirstOrDefault(d => d.Feature == fixedWeir);

                    if (correspondingModelFeatureCoordinateData == null) return;

                    for (var index = 0; index < correspondingModelFeatureCoordinateData.DataColumns.Count; index++)
                    {
                        if (!correspondingModelFeatureCoordinateData.DataColumns[index].IsActive) break;
                        var dataColumnWithData = correspondingModelFeatureCoordinateData.DataColumns[index].ValueList;

                        var syncedList = new GeometryPointsSyncedList<double>
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
                    if (fixedWeirs == null) return;

                    fixedWeirs.ForEach(fw => fw.Attributes.Clear());
                }
            };
            yield return new PlizFileImporterExporter<BridgePillar, BridgePillar>
            {
                Mode = Feature2DImportExportMode.Export,
                BeforeExportActionDelegate = delegate (object featureList)
                {
                    var waterFlowFmModel = GetModelFor(featureList, a => a.BridgePillars);
                    var bridgePillars = featureList as IEnumerable<BridgePillar>;
                    if (bridgePillars == null || waterFlowFmModel == null) return;

                    var modelFeatureCoordinateDatas = waterFlowFmModel.BridgePillarsDataModel;
                    MduFile.SetBridgePillarAttributes(bridgePillars, modelFeatureCoordinateDatas);
                },

                AfterExportActionDelegate = delegate (object featureList)
                {
                    var bridgePillars = featureList as IEnumerable<BridgePillar>;
                    if (bridgePillars == null) return;

                    MduFile.CleanBridgePillarAttributes(bridgePillars);
                },
            };
            yield return new PliFileImporterExporter<ThinDam2D, ThinDam2D> { Mode = Feature2DImportExportMode.Export };
            yield return new PliFileImporterExporter<ObservationCrossSection2D, ObservationCrossSection2D> { Mode = Feature2DImportExportMode.Export };
            yield return new PliFileImporterExporter<IWeir, Feature2D>
            {
                Mode = Feature2DImportExportMode.Export,
                CreateFromFeature = f => new Weir(f.Name, true) { Geometry = f.Geometry },
                GetFeature = w => new Feature2D
                {
                    Name = w.Name,
                    Geometry = w.Geometry
                }
            };
            yield return new PliFileImporterExporter<IPump, Feature2D>
            {
                Mode = Feature2DImportExportMode.Export,
                CreateFromFeature = f => new Pump(f.Name, true) { Geometry = f.Geometry },
                GetFeature = w => new Feature2D { Name = w.Name, Geometry = w.Geometry }
            };
            yield return new PliFileImporterExporter<IGate, Feature2D>
            {
                Mode = Feature2DImportExportMode.Export,
                CreateFromFeature = f => new Gate(f.Name) { Geometry = f.Geometry },
                GetFeature = w => new Feature2D { Name = w.Name, Geometry = w.Geometry }
            };
            yield return new PliFileImporterExporter<SourceAndSink, Feature2D>
            {
                Mode = Feature2DImportExportMode.Export,
                CreateFromFeature = f => new SourceAndSink { Area = 1.0, Feature = f },
                GetFeature = s => s.Feature
            };
            yield return new PliFileImporterExporter<BoundaryConditionSet, Feature2D>
            {
                Mode = Feature2DImportExportMode.Export,
                CreateFromFeature = f => new BoundaryConditionSet { Feature = f },
                GetFeature = b => b.Feature
            };
            yield return new PointFileImporterExporter { Mode = Feature2DImportExportMode.Export };
            yield return new ObsFileImporterExporter<GroupableFeature2DPoint> { Mode = Feature2DImportExportMode.Export};
            yield return new PolFileImporterExporter {Mode = Feature2DImportExportMode.Export};
            yield return new LdbFileImporterExporter {Mode = Feature2DImportExportMode.Export};
            yield return new FlowFMNetFileExporter {GetModelForGrid = GetModelForGrid};
            yield return new FMModelPartitionExporter
            {
                PolygonFile = null,
                IsContiguous = true,
                NumDomains = 1,
                SolverType = 7
            };
            yield return new FMGridPartitionExporter
            {
                GetModelForGrid = GetModelForGrid,
                PolygonFile = null,
                IsContiguous = true,
                NumDomains = 1
            };
            yield return new GeometryZipExporter {GetModelForGrid = GetModelForGrid};
            yield return new TimFileExporter
            {
                GetModelForHeatFluxModel = GetModelForHeatFluxModel,
                GetModelForSourceAndSink = GetModelForSourceAndSink
            };
            yield return new ImportedNetFileXyzExporter {Output = GridPointsExporter.OutputMode.Vertices};
            yield return new ImportedNetFileXyzExporter {Output = GridPointsExporter.OutputMode.CellCenters};
			
			foreach (var windItemExporter in WindItemExporter.CreateExporters())
            {
                windItemExporter.ReferenceDateGetter = w => GetModelForWindField(w).ReferenceTime;
                yield return windItemExporter;
            }
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
            var waterFlowFMModel = FlowModels.FirstOrDefault();
            return waterFlowFMModel == null ? (DateTime?) null : waterFlowFMModel.ReferenceTime;
        }

        private WaterFlowFMModel GetModelForGrid(UnstructuredGrid grid)
        {
            return FlowModels.FirstOrDefault(m => m.Grid.Equals(grid));
        }

        private IEnumerable<WaterFlowFMModel> FlowModels
            => Application != null
                   ? Application.ProjectService.Project.RootFolder.GetAllModelsRecursive().OfType<WaterFlowFMModel>()
                   : Enumerable.Empty<WaterFlowFMModel>();

        private static bool GetAvailableLists(WaterFlowFMModel model, IEnumerable list)
        {
            if (Equals(model.Area.Weirs, list)) return true;
            if (Equals(model.Area.Gates, list)) return true;
            if (Equals(model.Area.Pumps, list)) return true;
            // Add more if relevant...

            return false;
        }
        
        private void Application_ProjectOpened(object sender, EventArgs<Project> e)
        {
            Project project = e.Value;
            Application.ProjectService.Project.RootFolder.GetAllModelsRecursive().OfType<WaterFlowFMModel>().ForEach(m =>
            {
                m.WorkingDirectoryPathFunc = () => application.WorkDirectory;
                m.DimrRunner.FileExportService = Application.FileExportService;
            });
            
            project.CollectionChanging += OnProjectCollectionChanging;
        }

        private void Application_ProjectClosing(object sender, EventArgs<Project> e)
        {
            e.Value.CollectionChanging -= OnProjectCollectionChanging;
        }
        
        private void OnProjectCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            if (e.Action == NotifyCollectionChangeAction.Add && e.Item is WaterFlowFMModel model)
            {
                model.DimrRunner.FileExportService = Application.FileExportService;
            }
        }
        
        /// <inheritdoc/>
        public override void AddRegistrations(IDependencyInjectionContainer container)
        {
            container.Register<IDataAccessListenersProvider, FlowFMDataAccessListenersProvider>(LifeCycle.Transient);
        }
    }
}