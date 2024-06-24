using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Forms;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.NGHS.Common.Gui.MapLayers;
using DeltaShell.NGHS.Common.Gui.PropertyGrid;
using DeltaShell.NGHS.Common.Gui.PropertyGrid.PropertyInfoCreation;
using DeltaShell.NGHS.Common.Gui.Validation;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.NetworkEditor.Gui.Export;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CompositeStructureView;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ProjectExplorer;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid.PropertyInfoCreation;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.Roughness;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView;
using DeltaShell.Plugins.NetworkEditor.Gui.GraphicsProviders;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using DeltaShell.Plugins.NetworkEditor.Gui.ProjectExplorer;
using DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation;
using DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts;
using DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features1D;
using DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features2D;
using DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.FeaturesRR;
using DeltaShell.Plugins.NetworkEditor.Gui.Wizard;
using DeltaShell.Plugins.NetworkEditor.Import;
using DeltaShell.Plugins.NetworkEditor.ImportExportCsv;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.CoverageViews;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using Mono.Addins;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using SharpMap;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.Layers;
using SharpMap.UI.Forms;
using ObservationPoint1DTableViewCreationContext = DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features1D.ObservationPointTableViewCreationContext;
using ObservationPoint2DTableViewCreationContext = DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features2D.ObservationPointTableViewCreationContext;
using PropertyInfo = DelftTools.Shell.Gui.PropertyInfo;

namespace DeltaShell.Plugins.NetworkEditor.Gui
{

    [Extension(typeof(IPlugin))]
    public class NetworkEditorGuiPlugin : GuiPlugin
    {
        private readonly GuiContainer guiContainer;
        private readonly TableViewInfoCreator tableViewInfoCreator;
        private HydroRegionTreeView hydroRegionTreeView;
        private bool settingGuiSelection;
        private ClonableToolStripMenuItem generateCalculationGridLocationsToolStripMenuItem;
        private ClonableToolStripMenuItem removeCalculationGridLocationsToolStripMenuItem;
        private ClonableToolStripMenuItem addNewHydroRegionToolStripMenuItem;
        private ClonableToolStripMenuItem convertCoordinateSystemToolStripMenuItem;
        private ContextMenuStrip calculationGridMenu;
        private ContextMenuStrip hydroRegionContextMenu;
        private ContextMenuStrip convertCoordinateSystemContextMenu;
        private readonly IMapLayerProvider networkEditorMapLayerProvider;
        private IGui gui;
        private IGraphicsProvider graphicsProvider;
        private readonly PropertyInfoCreator propertyInfoCreator;
        private bool disposed;

        public NetworkEditorGuiPlugin()
        {
            guiContainer = new GuiContainer();
            tableViewInfoCreator = new TableViewInfoCreator(guiContainer);
            propertyInfoCreator = new PropertyInfoCreator(guiContainer);
            InitializeComponent();
            Instance = this;
            networkEditorMapLayerProvider = new NetworkEditorMapLayerProvider();
        }

        public static NetworkEditorGuiPlugin Instance { get; private set; }

        public override string Name
        {
            get { return "Network (UI)"; }
        }

        public override string DisplayName
        {
            get { return "Hydro Region Plugin (UI)"; }
        }

        public override string Description
        {
            get { return "Provides network editing functionality"; }
        }

        public override string Version
        {
            get { return GetType().Assembly.GetName().Version.ToString(); }
        }

        public override string FileFormatVersion => "3.5.0.0";

        public override IGui Gui
        {
            get { return gui; }
            set
            {
                if (gui != null)
                {
                    gui.SelectionChanged -= GuiSelectionChanged;
                    gui.Application.ProjectClosing -= ApplicationProjectClosed;
                    gui.Application.ProjectOpened -= ApplicationProjectOpened;

                }

                gui = value;
                guiContainer.Gui = value;

                if (gui != null)
                {
                    gui.Application.ProjectClosing += ApplicationProjectClosed;
                    gui.Application.ProjectOpened += ApplicationProjectOpened;
                    gui.SelectionChanged += GuiSelectionChanged;
                }
            }
        }

        public override IGraphicsProvider GraphicsProvider
        {
            get { return graphicsProvider ?? (graphicsProvider = new NetworkEditorGraphicsProvider()); }
        }

        public override IRibbonCommandHandler RibbonCommandHandler
        {
            get { return new Ribbon(); }
        }

        public override IEnumerable<PropertyInfo> GetPropertyInfos()
        {
            yield return new PropertyInfo<VectorLayer, CrossSectionVectorLayerProperties>
            {
                AdditionalDataCheck = l => l.CustomRenderers.OfType<CrossSectionRenderer>().Any()
            };
            yield return new PropertyInfo<WmtsLayer, WmtsLayerProperties>();
            yield return propertyInfoCreator.Create(new CrossSectionPropertyInfoCreationContext());
            yield return new PropertyInfo<ICrossSectionDefinition, CrossSectionDefinitionProperties>();
            yield return propertyInfoCreator.Create(new HydroNodePropertyInfoCreationContext());
            yield return propertyInfoCreator.Create(new ManholePropertyInfoCreationContext());
            yield return propertyInfoCreator.Create(new ChannelPropertyInfoCreationContext());
            yield return new PropertyInfo<IHydroNetwork, HydroNetworkProperties>();
            yield return new PropertyInfo<IDrainageBasin, DrainageBasinProperties>();
            yield return new PropertyInfo<HydroRegion, HydroRegionProperties>();
            yield return new PropertyInfo<Discretization, DiscretizationProperties>();
            yield return new PropertyInfo<INetworkCoverage, NetworkCoverageProperties>();
            yield return new PropertyInfo<IFeatureCoverage, FeatureCoverageProperties>();
            yield return propertyInfoCreator.Create(new CompositeStructurePropertyInfoCreationContext());
            yield return propertyInfoCreator.Create(new WeirPropertyInfoCreationContext());
            yield return propertyInfoCreator.Create(new GatePropertyInfoCreationContext());
            yield return propertyInfoCreator.Create(new PumpPropertyInfoCreationContext());
            yield return propertyInfoCreator.Create(new BridgePropertyInfoCreationContext());
            yield return propertyInfoCreator.Create(new CulvertPropertyInfoCreationContext());
            yield return propertyInfoCreator.Create(new LateralSourcePropertyInfoCreationContext());
            yield return propertyInfoCreator.Create(new ObservationPointPropertyInfoCreationContext());
            yield return propertyInfoCreator.Create(new WasteWaterTreatmentPlantPropertyInfoCreationContext());
            yield return propertyInfoCreator.Create(new RunoffBoundaryPropertyInfoCreationContext());
            yield return new PropertyInfo<NetworkSegment, NetworkSegmentProperties>();
            yield return new PropertyInfo<CrossSectionSectionType, CrossSectionSectionTypeProperties>();
            yield return propertyInfoCreator.Create(new RetentionPropertyInfoCreationContext());
            yield return propertyInfoCreator.Create(new HydroLinkPropertyInfoCreationContext());
            yield return new PropertyInfo<HydroArea, HydroAreaProperties>();
            yield return new PropertyInfo<ReverseRoughnessSection, ReverseRoughnessSectionProperties>();
            yield return new PropertyInfo<RoughnessSection, RoughnessSectionPropertiesBase<RoughnessSection>>();
            yield return propertyInfoCreator.Create(new SewerConnectionPropertyInfoCreationContext());
            yield return propertyInfoCreator.Create(new CompartmentPropertyInfoCreationContext());
            yield return propertyInfoCreator.Create(new LeveeBreachPropertyInfoCreationContext());
        }


        public override IEnumerable<ViewInfo> GetViewInfoObjects()
        {
            yield return new ViewInfo<RoughnessSection, RoughnessSectionCoverageTableView>
            {
                
                CompositeViewType = typeof(ProjectItemMapView),
                GetCompositeViewData = o =>
                {
                    return Gui.Application.GetAllModelsInProject()
                        .OfType<IModelWithRoughnessSections>()
                        .FirstOrDefault(m => m.RoughnessSections.Contains(o));
                },
            };
            yield return new ViewInfo<RoughnessNetworkCoverage, RoughnessSection, RoughnessSectionCoverageTableView>
            {
                CompositeViewType = typeof(ProjectItemMapView),
                GetCompositeViewData = o => Gui.Application.DataItemService.GetDataItemByValue(Gui.Application.Project, o) ?? new DataItem(o),
                GetViewData = coverage => Gui.Application.GetAllModelsInProject()
                    .OfType<IModelWithRoughnessSections>()
                    .SelectMany(m => m.RoughnessSections)
                    .FirstOrDefault(rs => Equals(rs.RoughnessNetworkCoverage, coverage))
            };
            yield return new ViewInfo<CrossSectionFromCsvFileImporterBase, CrossSectionCsvImportWizard>();
            yield return new ViewInfo<IPump, PumpView>
                {
                    AdditionalDataCheck = o => o != null && o.Branch != null,
                    CompositeViewType = typeof(CompositeStructureView),
                    GetCompositeViewData = o => o.ParentStructure,
                };
            yield return new ViewInfo<IWeir, WeirView>
            {
                AdditionalDataCheck = o => o != null && o.Branch != null && !(o is IOrifice),
                CompositeViewType = typeof(CompositeStructureView),
                GetCompositeViewData = o => o.ParentStructure,
            };
            yield return new ViewInfo<IOrifice, OrificeView>
            {
                AdditionalDataCheck = o => o != null && o.Branch != null,
                CompositeViewType = typeof(CompositeStructureView),
                GetCompositeViewData = o => o.ParentStructure,
            };
            yield return new ViewInfo<IBridge, BridgeView>
            {
                CompositeViewType = typeof(CompositeStructureView),
                GetCompositeViewData = o => o.ParentStructure,
            };
            yield return new ViewInfo<ICulvert, CulvertViewWpf>
            {
                CompositeViewType = typeof(CompositeStructureView),
                GetCompositeViewData = o => o.ParentStructure,
            };
            yield return new ViewInfo<ICompositeBranchStructure, CompositeStructureView>
            {
                AfterCreate = (v, o) =>
                {
                    v.Presenter = new CompositeStructureViewPresenter
                    {
                        SelectionContainer = Gui,
                        CreateView = ob => Gui.DocumentViewsResolver.CreateViewForData(ob, info => info.CompositeViewType == typeof(CompositeStructureView))
                    };
                }
            };
            yield return new ViewInfo<IEnumerable<IWeir>, ILayer, VectorLayerAttributeTableView>
                {
                    Description = "Attribute Table",
                    AdditionalDataCheck = o => o.All(weir => weir.Branch != null),
                    CompositeViewType = typeof(ProjectItemMapView),
                    GetCompositeViewData = o => gui.Application.Project.GetAllItemsRecursive()
                                                    .OfType<IDataItem>()
                                                    .FirstOrDefault(d => d.Value is IHydroNetwork && 
                                                                         (((IHydroNetwork)d.Value).Weirs == o || ((IHydroNetwork)d.Value).Orifices == o)),
                    GetViewData = o =>
                        {
                            var centralMap = Gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault(v => v.MapView.GetLayerForData(o) != null);
                            return centralMap.MapView.GetLayerForData(o);
                        },
                    AfterCreate = (v, o) =>
                        {
                            // It seems that this Gui can be null while calling the function. Is that correct?
                            var centralMap = Gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault(vi => vi.MapView.GetLayerForData(o) != null);
                            if (centralMap == null) return;

                            v.DeleteSelectedFeatures = () => centralMap.MapView.MapControl.DeleteTool.DeleteSelection();
                            v.OpenViewMethod = ob => Gui.CommandHandler.OpenView(ob);
                            v.ZoomToFeature = feature => centralMap.MapView.EnsureVisible(feature);
                            var nameValidator = NameValidator.CreateDefault();
                            nameValidator.AddValidator(new UniqueNameValidator(o));
                            v.SetCreateFeatureRowFunction(feature => new WeirRow((IWeir)feature, nameValidator));
                            if(o is IEnumerable<IOrifice>)
                                v.TableView.Columns.ToDictionary(c => c.Name, c => c)[nameof(WeirRow.Formula)].Visible = false;
                        }
                };
            yield return new ViewInfo<IEnumerable<IGate>, ILayer, VectorLayerAttributeTableView>
            {
                Description = "Attribute Table",
                AdditionalDataCheck = o => o.All(gate => gate.Branch != null),
                CompositeViewType = typeof(ProjectItemMapView),
                GetCompositeViewData = o => gui.Application.Project.GetAllItemsRecursive()
                                                .OfType<IDataItem>()
                                                .FirstOrDefault(d => d.Value is IHydroNetwork &&
                                                                     ((IHydroNetwork)d.Value).Gates == o),
                GetViewData = o =>
                {
                    var centralMap = Gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault(v => v.MapView.GetLayerForData(o) != null);
                    return centralMap.MapView.GetLayerForData(o);
                },
                AfterCreate = (v, o) =>
                {
                    var centralMap = Gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault(vi => vi.MapView.GetLayerForData(o) != null);
                    if (centralMap == null) return;

                    v.DeleteSelectedFeatures = () => centralMap.MapView.MapControl.DeleteTool.DeleteSelection();
                    v.OpenViewMethod = ob => Gui.CommandHandler.OpenView(ob);
                    v.ZoomToFeature = feature => centralMap.MapView.EnsureVisible(feature);
                    var nameValidator = NameValidator.CreateDefault();
                    nameValidator.AddValidator(new UniqueNameValidator(o));
                    v.SetCreateFeatureRowFunction(feature => new GateRow((IGate)feature, nameValidator));
                }
            };

            yield return new ViewInfo<ICrossSection, CrossSectionView>
                {
                    Description = "Cross-section view",
                    AfterCreate = (v, o) =>
                        {
                            v.StatusMessage += (s, e) => Gui.MainWindow.StatusBarMessage = s as string;
                            v.EditClickedAction = (s, e) => Gui.CommandHandler.OpenView((e as SelectedItemChangedEventArgs)?.Item);
                            v.GetConveyanceCalculators = null;
                        }
                };
            yield return new ViewInfo<ICrossSectionDefinition, CrossSectionDefinitionView>
                {
                    AfterCreate = (v, o) =>
                        {
                            v.StatusMessage += (s, e) => Gui.MainWindow.StatusBarMessage = s as string;
                            
                            //get the network that has this definition.
                            var network = Gui.Application.Project.GetAllItemsRecursive().OfType<IHydroNetwork>().FirstOrDefault(n => n.SharedCrossSectionDefinitions.Contains(o));
                            var viewModel = CrossSectionDefinitionViewModelProvider.GetViewModel(o, network);
                            viewModel.IsCurrentlyOnChannel = network.SharedCrossSectionDefinitions.Contains(o) 
                                                             && network.CrossSections.Any(cs => cs.Definition.IsProxy && ((CrossSectionDefinitionProxy)cs.Definition).InnerDefinition == o);
                            v.ViewModel = viewModel;
                        }
                };
            yield return new ViewInfo<Route, NetworkSideView>
                {
                    Description = "Network side view",
                    AdditionalDataCheck = r => r.Locations.Values.Count > 1,
                    AfterCreate = (v, o) =>
                        {
                            var project = Gui.Application.Project;
                            var coverages = project.GetAllItemsRecursive().OfType<ICoverage>().Distinct();
                            var manager = new NetworkSideViewCoverageManager(o, project, coverages)
                                {
                                    OnRouteRemoved = () => Gui.DocumentViews.Remove(v)
                                };
                            v.DataController = new NetworkSideViewDataController(o, manager, GetModelNameForCoverage);
                        },
                };
            yield return new ViewInfo<HydroRegionFromGisImporter, ImportHydroNetworkFromGisWizardDialog>
                {
                    GetViewName = (v, o) => v.Title,
                    AfterCreate = (v, o) =>
                        {
                            // Reset the dialog with a HydroRegionFromGisImporter with HydroRegion set
                            var selectedDataItem = gui.Selection as IDataItem;
                            o.HydroRegion = selectedDataItem != null
                                                ? selectedDataItem.Value as IHydroRegion
                                                : gui.Selection as IHydroRegion;
                            v.Importer = o;
                        }
                };
            yield return new ViewInfo<NetworkCoverageFromGisImporter, ImportNetworkCoverageFromGisWizardDialog>
                {
                    GetViewName = (v, o) => v.Title,
                    AfterCreate = (v, o) => v.Importer = o
                };
            
            yield return new ViewInfo<Embankment, IGeometry, GeometryEditor>
            {
                GetViewData = (v) => v.Geometry,
                GetViewName = (v,g) => ((Embankment)v.Tag).Name,
                AfterCreate = (v, o) =>
                    {
                        v.Name = o.Name;
                        v.Tag = o;
                    } 
            };

            yield return new ViewInfo<IStructure1D, AreaStructureView>()
            {
                AdditionalDataCheck = o => o.Branch == null,
                Description = "Structure Editor"
            };

            yield return new ViewInfo<Route, CoverageTableView>
            {
                Description = "Map (spatial data)",
                AdditionalDataCheck = route => Gui.Application.DataItemService.GetDataItemByValue(Gui.Application.Project, route.Network) != null,
                CompositeViewType = typeof(ProjectItemMapView),
                GetCompositeViewData = route => Gui.Application.DataItemService.GetDataItemByValue(Gui.Application.Project, route.Network),
            };

            yield return tableViewInfoCreator.Create(new Pump2DTableViewCreationContext());
            yield return tableViewInfoCreator.Create(new Weir2DTableViewCreationContext());
            yield return tableViewInfoCreator.Create(new Gate2DTableViewCreationContext());
            
            yield return tableViewInfoCreator.Create(new LandBoundary2DTableViewCreationContext());
            yield return tableViewInfoCreator.Create(new DryPointTableViewCreationContext());
            yield return tableViewInfoCreator.Create(new DryAreaTableViewCreationContext());
            yield return tableViewInfoCreator.Create(new ThinDam2DTableViewCreationContext());
            yield return tableViewInfoCreator.Create(new FixedWeirTableViewCreationContext());
            yield return tableViewInfoCreator.Create(new LeveeBreachTableViewCreationContext());
            yield return tableViewInfoCreator.Create(new ObservationPoint2DTableViewCreationContext());
            yield return tableViewInfoCreator.Create(new ObservationCrossSection2DTableViewCreationContext());
            yield return tableViewInfoCreator.Create(new RoofAreaTableViewCreationContext());
            yield return tableViewInfoCreator.Create(new GullyTableViewCreationContext());
            yield return tableViewInfoCreator.Create(new EmbankmentTableViewCreationContext());
            yield return tableViewInfoCreator.Create(new EnclosureTableViewCreationContext());
            yield return tableViewInfoCreator.Create(new BridgePillarTableViewCreationContext());
            yield return new ViewInfo<IManhole, ManholeView>();
            yield return new ViewInfo<ICompartment, IManhole, ManholeView>
            {
                AdditionalDataCheck = o => o?.ParentManhole != null,
                GetViewData = (v) => v.ParentManhole,
                OnActivateView = (v, o) =>
                {
                    if (!(o is ICompartment compartment)) return;
                    v.EnsureVisible(compartment);
                },
            };
            yield return new ViewInfo<ISewerConnection, SewerConnectionView>
            {
                AdditionalDataCheck = sewerConnection =>
                {
                    if (sewerConnection is IPipe pipe)
                    {
                        var defaultSewerCrossSectionSectionType = pipe.CrossSection?.Definition?.Sections?.FirstOrDefault()?.SectionType;
                        var roughnessSectionModel = Gui.Application.GetAllModelsInProject().OfType<IModelWithNetwork>().FirstOrDefault(m => m.Network.Pipes.Contains(pipe)) as IModelWithRoughnessSections;
                        var sewerRoughnessSection = roughnessSectionModel?.RoughnessSections?.FirstOrDefault(rs => rs.Name == defaultSewerCrossSectionSectionType?.ToString());

                        return sewerRoughnessSection != null;
                    }

                    return true;
                },
                AfterCreate = (view, sewerConnection) =>
                {
                    var defaultSewerCrossSectionSectionType = sewerConnection.CrossSection?.Definition.Sections.FirstOrDefault()?.SectionType;
                    var roughnessSectionModel = Gui.Application.GetAllModelsInProject().OfType<IModelWithNetwork>().FirstOrDefault(m => m.Network.Pipes.Contains(sewerConnection)) as IModelWithRoughnessSections;
                    var sewerRoughnessSection = roughnessSectionModel?.RoughnessSections?.FirstOrDefault(rs => rs.Name == defaultSewerCrossSectionSectionType?.ToString());

                    view.DataContext = new SewerConnectionViewModel(sewerConnection, sewerRoughnessSection);
                    view.OpenView = o => Gui.CommandHandler.OpenView(o);
                }
            };

            yield return tableViewInfoCreator.Create(new PipeTableViewCreationContext());
            yield return tableViewInfoCreator.Create(new CompartmentTableViewCreationContext());
            yield return tableViewInfoCreator.Create(new OutletCompartmentTableViewCreationContext());
            yield return tableViewInfoCreator.Create(new RetentionTableViewCreationContext());

            yield return new ViewInfo<LeveeBreach, LeveeBreachView>();
            yield return new ValidatedFeaturesViewInfo(guiContainer);
            
            yield return tableViewInfoCreator.Create(new HydroNodeTableViewCreationContext());
            yield return tableViewInfoCreator.Create(new ChannelTableViewCreationContext());
            yield return tableViewInfoCreator.Create(new BridgeTableViewCreationContext());
            yield return tableViewInfoCreator.Create(new CompositeBranchStructureTableViewCreationContext());
            yield return tableViewInfoCreator.Create(new CrossSectionTableViewCreationContext());
            yield return tableViewInfoCreator.Create(new CulvertTableViewCreationContext());
            yield return tableViewInfoCreator.Create(new LateralSourceTableViewCreationContext());
            yield return tableViewInfoCreator.Create(new ManholeTableViewCreationContext());
            yield return tableViewInfoCreator.Create(new ObservationPoint1DTableViewCreationContext());
            yield return tableViewInfoCreator.Create(new SewerConnectionTableViewCreationContext());
            yield return tableViewInfoCreator.Create(new PumpTableViewCreationContext());
            
            yield return tableViewInfoCreator.Create(new HydroLinkTableViewCreationContext());

            yield return tableViewInfoCreator.Create(new RunoffBoundaryTableViewCreationContext());
            yield return tableViewInfoCreator.Create(new WasteWaterTreatmentPlantTableViewCreationContext());
        }
        
        public override IEnumerable<IFileExporter> GetFileExporters()
        {
            yield return new HydroRegionShapeFileExporter(Gui);
        }

        public override IMapLayerProvider MapLayerProvider
        {
            get { return networkEditorMapLayerProvider; }
        }

        public IView HydroRegionContents
        {
            get { return hydroRegionTreeView; }
        }

        private void InitializeComponent()
        {   
            generateCalculationGridLocationsToolStripMenuItem = new ClonableToolStripMenuItem
            {
                Image = Properties.Resources.GeneratePoints,
                Name = "generateCalculationGridLocationsToolStripMenuItem",
                Text = Properties.Resources.NetworkEditorGuiPlugin_InitializeComponent_Generate_Computational_Grid_Nodes___
            };

            removeCalculationGridLocationsToolStripMenuItem = new ClonableToolStripMenuItem
            {
                Image = Properties.Resources.RemovePoints,
                Name = "removeCalculationGridLocationsToolStripMenuItem",
                Text = Properties.Resources.NetworkEditorGuiPlugin_InitializeComponent_Remove_Computational_Grid_Nodes
            };
            addNewHydroRegionToolStripMenuItem = new ClonableToolStripMenuItem
            {
                Image = Properties.Resources.HydroRegion,
                Name = "addNewHydroRegionToolStripMenuItem",
                Text = Properties.Resources.NetworkEditorGuiPlugin_InitializeComponent_Add_Sub_Region
            };
            convertCoordinateSystemToolStripMenuItem = new ClonableToolStripMenuItem
            {
                Image = Properties.Resources.HydroRegion,
                Name = "convertCoordinateSystemToolStripMenuItem",
                Text = "Convert to Coordinate System..."
            };

            generateCalculationGridLocationsToolStripMenuItem.Click += GenerateCalculationGridLocationsToolStripMenuItemClick;
            removeCalculationGridLocationsToolStripMenuItem.Click += removeCalculationGridLocationsToolStripMenuItem_Click;
            addNewHydroRegionToolStripMenuItem.Click += AddNewHydroRegionToolStripMenuItemClick;
            convertCoordinateSystemToolStripMenuItem.Click += ConvertCoordinateSystemToolStripMenuItemClick;
            
            calculationGridMenu = new ContextMenuStrip { Name = "calculationGridMenu", Size = new Size(210, 48) };
            calculationGridMenu.Items.AddRange(new ToolStripItem[] {generateCalculationGridLocationsToolStripMenuItem, removeCalculationGridLocationsToolStripMenuItem});

            hydroRegionContextMenu = new ContextMenuStrip {Name = "addNewHydroRegion", Size = new Size(210, 48)};
            hydroRegionContextMenu.Items.AddRange(new ToolStripItem[] {addNewHydroRegionToolStripMenuItem});

            convertCoordinateSystemContextMenu = new ContextMenuStrip { Name = "convertCoordinateSystemMenu" };
            convertCoordinateSystemContextMenu.Items.AddRange(new ToolStripItem[] {convertCoordinateSystemToolStripMenuItem});
        }

        public override void Activate()
        {
            InitializeHydroRegionTreeView();

            if (Gui.DocumentViews.ActiveView != null)
            {
                SetActiveRegion(GetRegionFromActiveView());
            }

            
            if (Gui.Application.Project != null)
            {
                // if project already exist call registered handler
                ApplicationProjectOpened(Gui.Application.Project);
            }
            
            ImportBranchesFromSelectionMapTool.BeforeExecute += () => Gui.IsViewRemoveOnItemDeleteSuspended = true;
            ImportBranchesFromSelectionMapTool.AfterExecute += () => Gui.IsViewRemoveOnItemDeleteSuspended = false; 

            base.Activate();
        }

        public override void Deactivate()
        {
            if (!IsActive)
            {
                return;
            }

            if (hydroRegionTreeView != null)
            {
                hydroRegionTreeView.TreeView.DoubleClick -= TreeViewDoubleClick;
                hydroRegionTreeView.TreeView.SelectedNodeChanged -= TreeViewSelectedNodeChanged;
                hydroRegionTreeView.Dispose();
            }
            
            base.Deactivate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                if (hydroRegionTreeView != null)
                {
                    ((IView)hydroRegionTreeView).Data = null;
                    hydroRegionTreeView.Dispose();
                    hydroRegionTreeView = null;
                }

                Instance = null;
            }

            base.Dispose(disposing);
            disposed = true;
        }

        public override IMenuItem GetContextMenu(object sender, object data)
        {
            if (hydroRegionTreeView != null && sender != null && ((TreeNode)sender).TreeView == hydroRegionTreeView.TreeView)
            {
                if (data is HydroRegion)
                {
                    addNewHydroRegionToolStripMenuItem.Tag = data;
                    return new MenuItemContextMenuStripAdapter(hydroRegionContextMenu);
                }
                return hydroRegionTreeView.GetContextMenu(sender, data);
            }

            if (data is HydroRegion)
            {
                addNewHydroRegionToolStripMenuItem.Tag = data;
                return new MenuItemContextMenuStripAdapter(hydroRegionContextMenu);
            }

            if (data is IHydroNetwork)
            {
                convertCoordinateSystemToolStripMenuItem.Tag = data;
                return new MenuItemContextMenuStripAdapter(convertCoordinateSystemContextMenu);
            }

            if(data is IDiscretization)
            {
                generateCalculationGridLocationsToolStripMenuItem.Tag = data as IDiscretization;
                generateCalculationGridLocationsToolStripMenuItem.Enabled = true;

                removeCalculationGridLocationsToolStripMenuItem.Tag = data as IDiscretization;
                removeCalculationGridLocationsToolStripMenuItem.Enabled = (((IDiscretization)data).Locations.Values.Count > 0);

                return new MenuItemContextMenuStripAdapter(calculationGridMenu);
            }

            return null;
        }

        public override bool CanDrop(object source, object target)
        {
            if (source is IHydroNetwork && target is Map)
            {
                return true;
            }

            return false;
        }

        public override IEnumerable<ITreeNodePresenter> GetProjectTreeViewNodePresenters()
        {
            yield return new HydroRegionProjectTreeViewNodePresenter { GuiPlugin = this };
            yield return new HydroNetworkProjectTreeViewNodePresenter { GuiPlugin = this };
            yield return new DrainageBasinProjectTreeViewNodePresenter { GuiPlugin = this };
            yield return new HydroAreaProjectTreeViewNodePresenter { GuiPlugin = this };
            yield return new Feature2DPolygonTreeViewNodePresenter {GuiPlugin = this};
            yield return new Feature2DPointTreeViewNodePresenter { GuiPlugin = this };

            yield return new FeatureProjectTreeViewNodePresenter<LandBoundary2D>(HydroArea.LandBoundariesPluralName, Properties.Resources.landboundary) { GuiPlugin = this };
            yield return new FeatureProjectTreeViewNodePresenter<GroupablePointFeature>(HydroArea.DryPointsPluralName, Properties.Resources.dry_point) { GuiPlugin = this };
            yield return new FeatureProjectTreeViewNodePresenter<ThinDam2D>(HydroArea.ThinDamsPluralName, Properties.Resources.thindam) { GuiPlugin = this };
            yield return new FeatureProjectTreeViewNodePresenter<Feature2D>(HydroArea.LeveeBreachName, Properties.Resources.LeveeBreach) { GuiPlugin = this };
            yield return new FeatureProjectTreeViewNodePresenter<FixedWeir>(HydroArea.FixedWeirsPluralName, Properties.Resources.fixedweir) { GuiPlugin = this };
            yield return new FeatureProjectTreeViewNodePresenter<GroupableFeature2DPoint>(HydroArea.ObservationPointsPluralName, Properties.Resources.Observation) { GuiPlugin = this };
            yield return new FeatureProjectTreeViewNodePresenter<ObservationCrossSection2D>(HydroArea.ObservationCrossSectionsPluralName, Properties.Resources.observationcs2d) { GuiPlugin = this };
            yield return new FeatureProjectTreeViewNodePresenter<Pump2D>(HydroArea.PumpsPluralName, Properties.Resources.pump) { GuiPlugin = this };
            yield return new FeatureProjectTreeViewNodePresenter<Weir2D>(HydroArea.WeirsPluralName, Properties.Resources.Weir) { GuiPlugin = this };
            yield return new FeatureProjectTreeViewNodePresenter<Gate2D>(HydroArea.GatesPluralName, Properties.Resources.Gate) { GuiPlugin = this };
            yield return new FeatureProjectTreeViewNodePresenter<Embankment>(HydroArea.EmbankmentsPluralName, Properties.Resources.Embankment) { GuiPlugin = this };
            yield return new FeatureProjectTreeViewNodePresenter<BridgePillar>(HydroArea.BridgePillarsPluralName, Properties.Resources.BridgeSmall) { GuiPlugin = this };
            yield return new FeatureProjectTreeViewNodePresenter<GroupableFeature2DPolygon>(HydroArea.RoofAreaName, Properties.Resources.Roof) { GuiPlugin = this };
            yield return new FeatureProjectTreeViewNodePresenter<Gully>(HydroArea.GullyName, Properties.Resources.Gully) { GuiPlugin = this };
            yield return new RoughnessSectionNodePresenter { GuiPlugin = this };

        }

        public override IEnumerable<Assembly> GetPersistentAssemblies()
        {
            yield return typeof(WmtsGroupLayer).Assembly;
            yield return GetType().Assembly;
        }

        public void InitializeHydroRegionTreeView()
        {
            if ((hydroRegionTreeView == null) || (hydroRegionTreeView.IsDisposed))
            {
                hydroRegionTreeView = new HydroRegionTreeView(this);
                hydroRegionTreeView.Text = "Region";
                hydroRegionTreeView.TreeView.DoubleClick += TreeViewDoubleClick;
                hydroRegionTreeView.TreeView.SelectedNodeChanged += TreeViewSelectedNodeChanged;
            }
            
            Gui.ToolWindowViews.Add(hydroRegionTreeView, ViewLocation.Right);
            Gui.ToolWindowViews.ActiveView = hydroRegionTreeView;
        }

        public void SetActiveRegion(IHydroRegion region)
        {
            if (hydroRegionTreeView == null)
            {
                return;
            }

            if (hydroRegionTreeView.Region != region)
            {
                hydroRegionTreeView.Region = region;
            }
        }

        void NetworkSideViewSelectionChanged(object sender, SelectedItemChangedEventArgs e)
        {
            Gui.Selection = e.Item;
        }

        void ApplicationProjectOpened(Project project)
        {
            project.RootFolder.CollectionChanged += RootFolderCollectionChanged;
            project.RootFolder.PropertyChanged += RootFolderPropertyChanged;
            ((INotifyPropertyChanged)Gui.Application.Project).PropertyChanged += ProjectPropertyChanged;
        }

        private void ProjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is ReverseRoughnessSection && e.PropertyName == "UseNormalRoughness")
            {
                Gui.DocumentViews.OfType<ProjectItemMapView>().ForEach(v => v.RefreshModelLayers());
            }
        }

        private void RootFolderPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Gui == null || Gui.DocumentViews == null)
            {
                return;
            }

            // Update the network in the network layers of all relevant maps after changing a network coverage network
            var networkCoverage = sender as INetworkCoverage;
            if (networkCoverage != null && e.PropertyName == "Network")
            {
                // Try to find maps for open views
                var openMaps = Gui.DocumentViews.AllViews
                    .OfType<MapView>()
                    .Select(mv => mv.Map)
                    .Where(m => m.Layers.OfType<NetworkCoverageGroupLayer>()
                                    .Any(l => l.NetworkCoverage == networkCoverage));

                if (openMaps.Any())
                {
                    foreach (var openMap in openMaps)
                    {
                        RefreshNetworkMapLayers(networkCoverage, openMap);
                    }
                }
                else
                {
                    // Try to find maps in a view contexts
                    foreach (var viewContext in Gui.ViewContextManager.ProjectViewContexts.OfType<CoverageViewViewContext>().Where(cvvc => cvvc.Coverage == networkCoverage))
                    {
                        RefreshNetworkMapLayers(networkCoverage, viewContext.Map);
                    }
                }
            }
        }

        private void RefreshNetworkMapLayers(INetworkCoverage networkCoverage, IMap map)
        {
            var hydroNetworkMapLayer = map.Layers.OfType<HydroRegionMapLayer>().FirstOrDefault(l => l.Region is IHydroNetwork);
            if (hydroNetworkMapLayer != null)
            {
                map.Layers.Remove(hydroNetworkMapLayer);
            }

            if (networkCoverage.Network != null)
            {
                map.Layers.Add(MapLayerProviderHelper.CreateLayersRecursive(networkCoverage.Network,null, Gui.Plugins.Select(p => p.MapLayerProvider).ToList()));
                map.ZoomToExtents();
            }
        }

        /// <summary>
        /// Listen to changes in the project and close views 
        /// hack: this logic should be in ProjectExplorerGuiPlugin::Project_CollectionChanged or
        /// merged with logic there.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RootFolderCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!(e.GetRemovedOrAddedItem() is IProjectItem)) // NB IModel and IDataItem derive from IProjectItem
            {
                return;
            }
            object value;
            if ((e.GetRemovedOrAddedItem() is IDataItem)) // NB IModel and IDataItem derive from IProjectItem
            {
                value = ((IDataItem)e.GetRemovedOrAddedItem()).Value;
            }
            else
            {
                value = e.GetRemovedOrAddedItem();
            }
            //IEnumerable<object> 
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    // if network is removed - remove views for all items within the network (not obvious on a higher level since it has no knowledge about child items of the network)
                    if (value is INetwork)
                    {
                        RemoveNetworkViews((INetwork)value);
                        return;
                    }
                    if (value is IModel)
                    {
                        // only close views for networks 
                        var networks =
                            (e.GetRemovedOrAddedItem() as IProjectItem).GetAllItemsRecursive().Where(
                                i =>
                                ((i is IDataItem) && (((IDataItem) i).Value is INetwork) && (!((IDataItem) i).IsLinked)))
                                .ToList();
                        foreach (IDataItem networkDataItem in networks)
                        {
                            RemoveNetworkViews((INetwork)networkDataItem.Value);
                        }
                        return;
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;

                case NotifyCollectionChangedAction.Add:
                    break;
            }
        }

        private void RemoveNetworkViews(INetwork network)
        {
            var networkViews =
                Gui.DocumentViews.Where(v => v.Data is INetworkFeature && ((INetworkFeature) v.Data).Network == network)
                    .ToArray();
            foreach (var view in networkViews)
            {
                Gui.DocumentViews.Remove(view);
            }

            var csDefinitionViews =
                Gui.DocumentViews.Where(
                    v => v.Data is ICrossSectionDefinition &&
                         network.BranchFeatures.OfType<ICrossSection>()
                             .Any(
                                 cs =>
                                     cs.Definition is CrossSectionDefinitionProxy &&
                                     ((CrossSectionDefinitionProxy) cs.Definition).InnerDefinition.Equals(v.Data))).ToArray();

            foreach (var view in csDefinitionViews)
            {
                Gui.DocumentViews.Remove(view);
            }

            
            // remove network layer from all maps
            var mapViews = Gui.DocumentViews.AllViews.OfType<MapView>().ToArray();
            foreach (var view in mapViews)
            {
                //only remove layers from the deleted network
                var hydroNetworkMapLayers = view.Map.Layers.OfType<HydroRegionMapLayer>().Where(l=>l.Region == network).ToArray();
                foreach (var layer in hydroNetworkMapLayers)
                {
                    view.Map.Layers.Remove(layer);
                }
            }
        }

        private void ApplicationProjectClosed(Project project)
        {
            if (project != null)
            {
                project.RootFolder.CollectionChanged -= RootFolderCollectionChanged;
                project.RootFolder.PropertyChanged -= RootFolderPropertyChanged;
                ((INotifyPropertyChanged)project).PropertyChanged -= ProjectPropertyChanged;
                foreach (var network in project.RootFolder.GetAllItemsRecursive().OfType<INetwork>())
                {
                    RemoveNetworkViews(network);
                }
            }
            
            ((IView)hydroRegionTreeView).Data = null;
            HydroNetworkCopyAndPasteHelper.ReleaseCopiedNetworkFeature();
        }

        public override void OnViewAdded(IView view)
        {
            OnDocumentViewAdded(view);
        }

        public override void OnViewRemoved(IView view)
        {
            if (view is MapView)
            {
                var mapView = (MapView)view;
                HydroRegionEditorHelper.RemoveHydroRegionEditorMapTool(mapView.MapControl);
            }
            //if the view contains a mapview remove the tool from the mapcontrol..
            if (view is ICompositeView)
            {
                var mapView = ((ICompositeView)view).ChildViews.OfType<MapView>().FirstOrDefault();
                if (mapView != null)
                {
                    HydroRegionEditorHelper.RemoveHydroRegionEditorMapTool(mapView.MapControl);
                }
            }
            if (view is NetworkSideView)
            {
                var networkSideView = (NetworkSideView)view;
                networkSideView.SelectionChanged -= NetworkSideViewSelectionChanged;
            }
        }

        public override void OnActiveViewChanged(IView view)
        {
            SetActiveRegion(GetRegionFromActiveView());
        }

        private void OnDocumentViewAdded(IView view)
        {
            var coverageView = view as CoverageView;
            if (coverageView == null && (view is ICompositeView compositeView))
            {
                coverageView = compositeView.ChildViews.OfType<CoverageView>().FirstOrDefault();
            }

            if (coverageView != null)
            {
                var mapView = coverageView.ChildViews.OfType<MapView>().FirstOrDefault();
                if (mapView == null) return;
                var map = mapView.Map;

                // add network as a layer
                if (coverageView.Coverage is INetworkCoverage)
                {
                    var networkCoverage = (INetworkCoverage)coverageView.Coverage;
                    
                    if (!map.Layers.OfType<HydroRegionMapLayer>().Any())
                    {
                        AddRegionLayer((IHydroRegion) networkCoverage.Network, map);
                    }
                }
                else if (coverageView.Coverage is IFeatureCoverage)
                {
                    // add region
                    var featureCoverage = (IFeatureCoverage) coverageView.Coverage;
                    
                    if (!map.Layers.OfType<HydroRegionMapLayer>().Any())
                    {
                        var region = GetRegionForFeatureCoverage(featureCoverage);
                        if (region != null)
                        {
                            AddRegionLayer(region, map);
                        }
                    }
                }
            }

            if (view is NetworkSideView sideView)
            {
                sideView.SelectionChanged += NetworkSideViewSelectionChanged;
            }
        }

        private void AddRegionLayer(IRegion region, IMap map)
        {
            var layer = MapLayerProviderHelper.CreateLayersRecursive(region, null, Gui.Plugins.Select(p => p.MapLayerProvider).ToList());
            map.DoWithLayerRecursive(layer, l =>
                {
                    var layerToSet = l as Layer;
                    if (layerToSet == null)
                    {
                        return;
                    }

                    layerToSet.ReadOnly = true;
                });
            map.Layers.Add(layer);
        }

        private IRegion GetRegionForFeatureCoverage(IFeatureCoverage featureCoverage)
        {
            if (featureCoverage.Features.Any())
            {
                var firstFeature = featureCoverage.Features[0];
                var allRegions = Gui.Application.Project.GetAllItemsRecursive().OfType<IRegion>();
                var matchingRegion = allRegions.FirstOrDefault(hr => hr.GetDirectChildren().Contains(firstFeature));
                return GetRootRegion(matchingRegion);
            }
            return null;
        }

        private IRegion GetRootRegion(IRegion matchingRegion)
        {
            if (matchingRegion == null)
                return null;

            return matchingRegion.Parent != null
                ? GetRootRegion(matchingRegion.Parent)
                : matchingRegion;
        }

        internal static MapView GetFocusedMapView()
        {
            return Instance.Gui.DocumentViews.ActiveView.GetViewsOfType<MapView>().FirstOrDefault();
        }

        private IHydroRegion GetRegionFromActiveView()
        {
            var activeView = Gui.DocumentViews.ActiveView;
            if (activeView == null || activeView.Data == null)
            {
                return null; // strange bug
            }

            // in case active view is view of network such as cross section editor, 
            // network editor or map that contains a network set the network to treeview
            IHydroRegion region = null;

            if (activeView is MapView || activeView is ProjectItemMapView)
            {
                // when region is dragged onto an opened mapview
                var mapView = activeView is ProjectItemMapView
                                  ? ((ProjectItemMapView)activeView).MapView
                                  : (MapView) activeView;

                HydroRegionEditorHelper.AddHydroRegionEditorMapTool(mapView.MapControl);
                region = mapView.Map.GetAllVisibleLayers(true).OfType<HydroRegionMapLayer>().Select(l => l.Region).OfType<IHydroNetwork>().FirstOrDefault();
            }
            else if ((activeView is CrossSectionView))
            {
                var crossSection = activeView.Data as ICrossSection;
                if (crossSection != null)
                {
                    region = (IHydroRegion)crossSection.Network;
                }
            }
            else if ((activeView is CrossSectionDefinitionView))
            {
                region = (activeView as CrossSectionDefinitionView).ViewModel.HydroNetwork;
            }
            else if (activeView is CompositeStructureView)
            {
                var structureView = (CompositeStructureView)activeView;

                var structure = structureView.Data as IStructure1D;

                if (structure != null)
                {
                    region = (IHydroNetwork)structure.Network;
                }
            }
            else if (activeView is NetworkSideView)
            {
                var route = ((NetworkSideView) activeView).Data as Route;
                region = route != null ? route.Network as IHydroNetwork : null;
            }
            else if (activeView != null)
            {
                region = activeView.Data as IHydroNetwork;
            }

            return region;
        }

        private void GuiSelectionChanged(object sender, SelectedItemChangedEventArgs e)
        {
            if (settingGuiSelection)
                return;

            //show network if selected
            if (Gui.Selection is IDataItem)
            {
                var dataItem = (IDataItem)Gui.Selection;
                if (typeof(IHydroRegion).IsAssignableFrom(dataItem.ValueType))
                {
                    hydroRegionTreeView.Region = (IHydroRegion)dataItem.Value;
                    return;
                }
            }

            //no network selected, so get it from the active view
            SetActiveRegion(GetRegionFromActiveView());
        }

        private void TreeViewSelectedNodeChanged(object sender, EventArgs e)
        {
            // check if view is open for cross section belonging to specific Branch
            if (hydroRegionTreeView.TreeView.SelectedNode == null || settingGuiSelection)
            {
                return;
            }
            
            if (!hydroRegionTreeView.SynchronizingGuiSelection)
            {
                settingGuiSelection = true;
                // needed to clear Trackers in mapView for nodes that are not displayed in MapView
                Gui.Selection = null;

                Gui.Selection = hydroRegionTreeView.TreeView.SelectedNode.Tag;

                settingGuiSelection = false;
            }

            var crossSection = Gui.Selection as ICrossSection;

            if (crossSection == null) return;
            
            var crossSectionViews = Gui.DocumentViews.OfType<CrossSectionView>();

            foreach (var view in crossSectionViews.Where(v=>!v.Locked))
            {
                view.Data = crossSection;
            }
        }

        private void TreeViewDoubleClick(object sender, EventArgs e)
        {
            //open view for selected object
            Gui.CommandHandler.OpenViewForSelection();
        }

        private void GenerateCalculationGridLocationsToolStripMenuItemClick(object sender, EventArgs e)
        {
            var discretization = (IDiscretization)((ToolStripMenuItem)sender).Tag;
            var hydroNetwork = (IHydroNetwork) discretization.Network;

            // if the actievview is the discretization view allow generation of gridpoints 
            // for selected channel only.
            MapControl mapControl = null;

            if (Gui.DocumentViews.ActiveView is CoverageView)
            {
                var coverageView = (CoverageView) Gui.DocumentViews.ActiveView;
                if (coverageView.Coverage == discretization)
                {
                    var mapView = coverageView.ChildViews.OfType<MapView>().FirstOrDefault();

                    mapControl = mapView != null ? mapView.MapControl : null;
                }
            }

            IList<IChannel> selectedChannels = null;
            if (mapControl != null)
            {
                selectedChannels = mapControl.SelectedFeatures.OfType<IChannel>().Where(f =>Equals(f.Network, hydroNetwork)).ToList();
            }

            var hasRun = HydroNetworkEditorMapToolHelper.RunCalculationGridWizard(selectedChannels,discretization);
            if (hasRun)
            {
                Gui.CommandHandler.OpenView(discretization);
                var mapView = Gui.DocumentViews.ActiveView.GetViewsOfType<MapView>().FirstOrDefault();
                if (mapView != null)
                {
                    mapView.MapControl.SelectTool.RefreshSelection();
                }
            }
        }

        private void removeCalculationGridLocationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var discretization = ((ToolStripMenuItem)sender).Tag as IDiscretization;

            discretization.ClearRuralLocations();

            //this should not be necessary...but collectionchanged is not properly handled in map
            var mapView = Gui.DocumentViews.ActiveView.GetViewsOfType<MapView>().FirstOrDefault();
            if (mapView != null)
            {
                mapView.MapControl.SelectTool.RefreshSelection();
            }
        }

        private void AddNewHydroRegionToolStripMenuItemClick(object sender, EventArgs e)
        {
            var region = (HydroRegion)((ToolStripMenuItem)sender).Tag;
            Gui.CommandHandler.AddNewProjectItem(region);
        }

        private void ConvertCoordinateSystemToolStripMenuItemClick(object sender, EventArgs e)
        {
            var network = ((ToolStripMenuItem) sender).Tag as INetwork;
            if (network == null)
                throw new InvalidOperationException("Can not find network when converting the coordinate system.");

            if (NetworkCoordinateConvertor.Convert(network))
            {
                var mapView = GetFocusedMapView();
                if (mapView != null && mapView.Map != null)
                    mapView.Map.ZoomToExtents();
            }
        }

        private string GetModelNameForCoverage(ICoverage coverage)
        {
            var allModels = Gui.Application.GetAllModelsInProject();
            var models = allModels.Where(m => m.GetAllItemsRecursive().Any(obj => obj is IFunction &&
                                                                      coverage.IsEqualOrDescendant(obj as IFunction))).ToList();

            if (models.Count() > 1) //several matches, do more filtering
            {
                models = models.Where(m => !(m is CompositeModel)).ToList();
            }

            var model = models.FirstOrDefault();
            return model != null ? model.Name : "";
        }
    }
}