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
                    Name = "Embedded",
                    ToolTipText = "Embedded 1D2D links for polder and manholes",
                    Type = LinkType.Embedded
                },
                new RibbonLink
                {
                    Name = "Lateral",
                    ToolTipText = "Lateral 1D2D links for rivers",
                    Type = LinkType.Lateral
                },
                new RibbonLink
                {
                    Name = "Roof-sewer",
                    ToolTipText = "Roof-sewer links for rainfall",
                    Type = LinkType.RoofSewer
                },
                new RibbonLink
                {
                    Name = "Inhabitants",
                    ToolTipText = "Inhabitants-sewer links for household wastewater",
                    Type = LinkType.InhabitantsSewer
                },
                new RibbonLink
                {
                    Name = "Gully-sewer",
                    ToolTipText = "Gully-sewer links for street in/outlet",
                    Type = LinkType.GullySewer
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

            public LinkType LinkType
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

        public LinkType Type { get; set; }
    }
}

