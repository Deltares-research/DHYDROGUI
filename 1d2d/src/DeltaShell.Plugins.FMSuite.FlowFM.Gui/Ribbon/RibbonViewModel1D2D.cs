using System.Collections.ObjectModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.MapTools;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.SharpMapGis.Gui.Commands;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Ribbon
{
    [Entity]
    public class RibbonViewModel1D2D
    {
        private readonly ObservableCollection<RibbonLink> _linkTypes;
        private MapToolCommand1D2DLinksMapTool mapToolCommandAdd1D2DLinkMapTool;
        private MapToolCommand1D2DLinksMapTool mapToolCommandGenerate1D2DLinksMapTool;
        private RibbonLink selectedRibbonLink;

        public RibbonViewModel1D2D()
        {
            mapToolCommandGenerate1D2DLinksMapTool = new MapToolCommand1D2DLinksMapTool(FlowFMMapViewDecorator.GenerateLinksToolName)
            {
                LayerType = typeof (AreaLayer)
            };

            mapToolCommandAdd1D2DLinkMapTool = new MapToolCommand1D2DLinksMapTool(FlowFMMapViewDecorator.AddLinksToolName)
            {
                LayerType = typeof(AreaLayer)
            };

            ActivateGenerateLinksToolCommand = new RelayMapToolCommand
            {
                MapToolCommand = mapToolCommandGenerate1D2DLinksMapTool
            };

            ActivateAddLinksToolCommand = new RelayMapToolCommand
            {
                MapToolCommand = mapToolCommandAdd1D2DLinkMapTool
            };

            _linkTypes = new ObservableCollection<RibbonLink>
            {
                new RibbonLink
                {
                    Name = "Embedded 1-to-1",
                    ToolTipText = "Embedded 1D2D links for polder one to one",
                    Type = LinkGeneratingType.EmbeddedOneToOne
                },
                new RibbonLink
                {
                    Name = "Embedded 1-to-n",
                    ToolTipText = "Embedded 1D2D links for polder one to many",
                    Type = LinkGeneratingType.EmbeddedOneToMany
                },
                new RibbonLink
                {
                    Name = "Lateral",
                    ToolTipText = "Lateral 1D2D links for rivers",
                    Type = LinkGeneratingType.Lateral
                },
                new RibbonLink
                {
                    Name = "Gully-sewer",
                    ToolTipText = "Gully-sewer links for street in/outlet",
                    Type = LinkGeneratingType.GullySewer
                }
            };

            SelectedRibbonLink = LinkTypes.First();
        }

        public ObservableCollection<RibbonLink> LinkTypes
        {
            get { return _linkTypes; }
        }

        public RibbonLink SelectedRibbonLink
        {
            get
            {
                return selectedRibbonLink;
            }
            set
            {
                selectedRibbonLink = value;
                mapToolCommandGenerate1D2DLinksMapTool.LinkType = selectedRibbonLink.Type;
                mapToolCommandAdd1D2DLinkMapTool.LinkType = selectedRibbonLink.Type;
            }
        }

        public RelayMapToolCommand ActivateGenerateLinksToolCommand { get; private set; }

        public RelayMapToolCommand ActivateAddLinksToolCommand { get; private set; }

        public void RefreshButtons()
        {
            new[]
            {
                ActivateGenerateLinksToolCommand, ActivateAddLinksToolCommand
            }.ForEach(c => c.Refresh());
        }

        private class MapToolCommand1D2DLinksMapTool : MapToolCommand
        {
            public MapToolCommand1D2DLinksMapTool(string toolName) : base(toolName)
            {
            }

            public LinkGeneratingType LinkType
            {
                set
                {
                    var mapTool = MapTool as Base1D2DLinksMapTool;
                    if (mapTool != null) mapTool.LinkType = value;
                }
            }
        }
    }

    [Entity]
    public class RibbonLink : INameable
    {
        public string Name { get; set; }

        public string ToolTipText { get; set; }

        public LinkGeneratingType Type { get; set; }
    }
}

