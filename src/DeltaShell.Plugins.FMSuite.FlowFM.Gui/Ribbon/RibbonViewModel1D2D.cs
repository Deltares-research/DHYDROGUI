using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.RightsManagement;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.MapTools;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.SharpMapGis.Gui.Commands;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Ribbon
{
    [Entity]
    public class RibbonViewModel1D2D
    {
        private readonly ObservableCollection<RibbonLink> _linkTypes;
        private MapToolCommandGenerateLinksMapTool mapToolCommandGenerateLinksMapTool;
        private RibbonLink selectedRibbonLink;

        public RibbonViewModel1D2D()
        {
            mapToolCommandGenerateLinksMapTool = new MapToolCommandGenerateLinksMapTool(FlowFMMapViewDecorator.GenerateLinksToolName)
            {
                LayerType = typeof (AreaLayer)
            };

            ActivateGenerateLinksToolCommand = new RelayMapToolCommand
            {
                MapToolCommand = mapToolCommandGenerateLinksMapTool
            };

            _linkTypes = new ObservableCollection<RibbonLink>
            {
                new RibbonLink()
                {
                    Name = "Embedded",
                    ToolTipText = "Embedded 1D2D links for polder and manholes",
                    Type = GridApiDataSet.LinkType.Embedded
                },
                new RibbonLink()
                {
                    Name = "Lateral",
                    ToolTipText = "Lateral 1D2D links for rivers",
                    Type = GridApiDataSet.LinkType.Lateral
                },
                new RibbonLink()
                {
                    Name = "Roof-sewer",
                    ToolTipText = "Roof-sewer links for rainfall",
                    Type = GridApiDataSet.LinkType.RoofSewer
                },
                new RibbonLink()
                {
                    Name = "Inhabitants",
                    ToolTipText = "Inhabitants-sewer links for household wastewater",
                    Type = GridApiDataSet.LinkType.InhabitantsSewer
                },
                new RibbonLink()
                {
                    Name = "Gully-sewer",
                    ToolTipText = "Gully-sewer links for street in/outlet",
                    Type = GridApiDataSet.LinkType.GullySewer
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
                mapToolCommandGenerateLinksMapTool.LinkType = selectedRibbonLink.Type;
            }
        }

        public RelayMapToolCommand ActivateGenerateLinksToolCommand { get; private set; }

        public RelayMapToolCommand ActivateAddLinkTool { get; private set; }

        public void RefreshButtons()
        {
            new[]
            {
                ActivateGenerateLinksToolCommand
            }.ForEach(c => c.Refresh());
        }

        private class MapToolCommandGenerateLinksMapTool : MapToolCommand
        {
            public MapToolCommandGenerateLinksMapTool(string toolName) : base(toolName)
            {
            }

            public GridApiDataSet.LinkType LinkType
            {
                set
                {
                    var mapTool = MapTool as GenerateLinksMapTool;
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

        public GridApiDataSet.LinkType Type { get; set; }
    }
}

