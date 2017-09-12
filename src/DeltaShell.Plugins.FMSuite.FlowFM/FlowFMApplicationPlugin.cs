using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Geometries;
using Mono.Addins;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    [Extension(typeof(IPlugin))]
    public class FlowFMApplicationPlugin : ApplicationPlugin, IDataAccessListenersProvider
    {
        public static string PluginVersion; // 1.2
        public static string PluginName; // D-Flow Flexible Mesh Plugin
        
        public override string Name
        {
            get { return "Delft3D FM"; }
        }

        public override string DisplayName
        {
            get { return "D-Flow Flexible Mesh Plugin"; }
        }

        public override string Description
        {
            get { return Properties.Resources.FlowFMApplicationPlugin_Description; }
        }

        public override string Version
        {
            get { return GetType().Assembly.GetName().Version.ToString(); }
        }

        public override string FileFormatVersion
        {
            get { return "1.1.0.0"; }
        }

        public override void Activate()
        {
            base.Activate();
            PluginVersion = Version;
            PluginName = DisplayName;
        }

        public override IEnumerable<ModelInfo> GetModelInfos()
        {
            yield return new ModelInfo
                {
                    Name = "Flow Flexible Mesh Model",
                    Category = "1D / 2D / 3D Standalone Models",
                    Image = Properties.Resources.unstrucModel,
                    AdditionalOwnerCheck = owner => !(owner is ICompositeActivity) // Allow "standalone" flow models
                             || !((ICompositeActivity)owner).Activities.OfType<WaterFlowFMModel>().Any(), // Don't allow multiple flow models in one composite activity
                    CreateModel = owner => new WaterFlowFMModel()
                };
        }

        public override IEnumerable<IFileImporter> GetFileImporters()
        {
            yield return new WaterFlowFMFileImporter();
            yield return new Area2DStructuresImporter { GetModelForArea = GetModelForArea };
            yield return new StructuresListImporter(StructuresListType.Pumps) { GetModelForList = GetModelForCollection };
            yield return new StructuresListImporter(StructuresListType.Weirs) { GetModelForList = GetModelForCollection };
            yield return new StructuresListImporter(StructuresListType.Gates) { GetModelForList = GetModelForCollection };
            yield return new FMMapFileImporter();
            yield return new FMHisFileImporter();
            yield return new FMRstFileImporter(){GetFMModelForRestartState = GetFMModelForRestartState};
            yield return new BcFileImporter();
            yield return new BcmFileImporter();
            yield return new BoundaryConditionWpsImporter();

            yield return new PliFileImporterExporter<Embankment, Embankment>
            {
                Mode = Feature2DImportExportMode.Import,
                CreateDelegate = delegate(List<Coordinate> points, string name)
                {
                    points.ForEach(p => p.Z = 0.0);
                    return new Embankment {Name = name, Geometry = PliFile<Embankment>.CreatePolyLine(points)};
                },
            };
            yield return new PliFileImporterExporter<FixedWeir, FixedWeir>
            {
                Mode = Feature2DImportExportMode.Import,
                CreateDelegate = delegate(List<Coordinate> points, string name)
                {
                    var feature = new FixedWeir {Name = name, Geometry = PliFile<FixedWeir>.CreatePolyLine(points)};
                    feature.InitializeAttributes();
                    return feature;
                },
            };
            yield return new PliFileImporterExporter<ThinDam2D, ThinDam2D> { Mode = Feature2DImportExportMode.Import };
            yield return
                new PliFileImporterExporter<ObservationCrossSection2D, ObservationCrossSection2D>
                {
                    Mode = Feature2DImportExportMode.Import
                };
            yield return new PliFileImporterExporter<IWeir, Feature2D>
            {
                Mode = Feature2DImportExportMode.Import,
                CreateFromFeature = f => new Weir(f.Name, true) {Geometry = f.Geometry},
                GetFeature = w => new Feature2D {Name = w.Name, Geometry = w.Geometry}
            };
            yield return new PliFileImporterExporter<IPump, Feature2D>
            {
                Mode = Feature2DImportExportMode.Import,
                CreateFromFeature = f => new Pump(f.Name, true) { Geometry = f.Geometry },
                GetFeature = w => new Feature2D { Name = w.Name, Geometry = w.Geometry }
            };
            yield return new PliFileImporterExporter<IGate, Feature2D>
            {
                Mode = Feature2DImportExportMode.Import,
                CreateFromFeature = f => new Gate(f.Name) { Geometry = f.Geometry },
                GetFeature = w => new Feature2D { Name = w.Name, Geometry = w.Geometry }
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
                            : new Feature2D {Name = name, Geometry = PliFile<Feature2D>.CreatePolyLine(points)}
            };
            yield return new PliFileImporterExporter<BoundaryConditionSet, Feature2D>
            {
                Mode = Feature2DImportExportMode.Import,
                CreateFromFeature = f => new BoundaryConditionSet {Feature = f},
                GetFeature = b => b.Feature
            };

            yield return new PointFileImporterExporter { Mode = Feature2DImportExportMode.Import };
            yield return new PolFileImporterExporter {Mode = Feature2DImportExportMode.Import};
            yield return new LdbFileImporterExporter {Mode = Feature2DImportExportMode.Import};
            yield return new FlowFMNetFileImporter {GetModelForGrid = GetModelForGrid};
            yield return
                new TimFileImporter
                {
                    WindFileImporter = false,
                    GetModelForSourceAndSink = GetModelForSourceAndSink,
                    GetModelForHeatFluxModel = GetModelForHeatFluxModel,
                };
            yield return
                new TimFileImporter
                {
                    WindFileImporter = true,
                    GetModelForWindTimeSeries = GetModelForWindField
                };
        }

        private WaterFlowFMModel GetFMModelForRestartState(FileBasedRestartState fileBasedRestartState)
        {
            return
                Application.GetAllModelsInProject().OfType<WaterFlowFMModel>()
                    .FirstOrDefault(m => Equals(m.RestartInput, fileBasedRestartState));
        }

        public override IEnumerable<IFileExporter> GetFileExporters()
        {
            yield return new WaterFlowFMFileExporter();
            yield return new Area2DStructuresExporter { GetModelForArea = GetModelForArea };
            yield return new StructuresListExporter(StructuresListType.Pumps) { GetModelForList = GetModelForCollection };
            yield return new StructuresListExporter(StructuresListType.Weirs) { GetModelForList = GetModelForCollection };
            yield return new StructuresListExporter(StructuresListType.Gates) { GetModelForList = GetModelForCollection };
            yield return new BcFileExporter {GetRefDateForBoundaryCondition = GetRefDateForBoundaryCondition};
            yield return new BcmFileExporter {GetRefDateForBoundaryCondition = GetRefDateForBoundaryCondition};
            yield return new PliFileImporterExporter<Embankment, Embankment> { Mode = Feature2DImportExportMode.Export };
            yield return new PliFileImporterExporter<FixedWeir, FixedWeir> { Mode = Feature2DImportExportMode.Export };
            yield return new PliFileImporterExporter<ThinDam2D, ThinDam2D> { Mode = Feature2DImportExportMode.Export };
            yield return
                new PliFileImporterExporter<ObservationCrossSection2D, ObservationCrossSection2D>
                {
                    Mode = Feature2DImportExportMode.Export
                };
            yield return new PliFileImporterExporter<IWeir, Feature2D>
            {
                Mode = Feature2DImportExportMode.Export,
                CreateFromFeature = f => new Weir(f.Name, true) { Geometry = f.Geometry },
                GetFeature = w => new Feature2D { Name = w.Name, Geometry = w.Geometry }
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
            yield return new PolFileImporterExporter {Mode = Feature2DImportExportMode.Export};
            yield return new LdbFileImporterExporter {Mode = Feature2DImportExportMode.Export};
            yield return new FlowFMNetFileExporter {GetModelForGrid = GetModelForGrid};
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

        public override IEnumerable<System.Reflection.Assembly> GetPersistentAssemblies()
        {
            yield return typeof (WaterFlowFMModel).Assembly;
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
        {
            get { return Application != null ? Application.GetAllModelsInProject().OfType<WaterFlowFMModel>() : Enumerable.Empty<WaterFlowFMModel>(); }
        }

        private static bool GetAvailableLists(WaterFlowFMModel model, IEnumerable list)
        {
            if (Equals(model.Area.Weirs, list)) return true;
            if (Equals(model.Area.Gates, list)) return true;
            if (Equals(model.Area.Pumps, list)) return true;
            // Add more if relevant...

            return false;
        }


        public IEnumerable<IDataAccessListener> CreateDataAccessListeners()
        {
            yield return new WaterFlowFMDataAccessListener();
        }
    }
}