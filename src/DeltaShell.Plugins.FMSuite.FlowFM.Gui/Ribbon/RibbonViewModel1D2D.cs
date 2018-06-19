using System.Collections.ObjectModel;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.SharpMapGis.Gui.Commands;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Ribbon
{
    [Entity]
    public class RibbonViewModel1D2D
    {
        private readonly ObservableCollection<LinkType> _linkTypes;

        public RibbonViewModel1D2D()
        {
            ActivateGenerateLinksToolCommand = new RelayMapToolCommand
            {
                MapToolCommand = new MapToolCommand(FlowFMMapViewDecorator.GenerateLinksToolName) { LayerType = typeof(AreaLayer) }
            };

            _linkTypes = new ObservableCollection<LinkType>
            {
                new LinkType()
                {
                    Name = "Embedded",
                    ToolTipText = "Embedded 1D2D links for polder and manholes",
                    Type = LinkTypeType.Embedded
                },
                new LinkType()
                {
                    Name = "Lateral",
                    ToolTipText = "Lateral 1D2D links for rivers",
                    Type = LinkTypeType.Lateral
                },
                new LinkType()
                {
                    Name = "Roof-sewer",
                    ToolTipText = "Roof-sewer links for rainfall",
                    Type = LinkTypeType.RoofSewer
                },
                new LinkType()
                {
                    Name = "Inhabitants",
                    ToolTipText = "Inhabitants-sewer links for household wastewater",
                    Type = LinkTypeType.InhabitantsSewer
                },
                new LinkType()
                {
                    Name = "Gully-sewer",
                    ToolTipText = "Gully-sewer links for street in/outlet",
                    Type = LinkTypeType.GullySewer
                }
            };

            SelectedLinkType = LinkTypes.First();
        }

        public ObservableCollection<LinkType> LinkTypes
        {
            get { return _linkTypes; }
        }

        public LinkType SelectedLinkType { get; set; }

        public RelayMapToolCommand ActivateGenerateLinksToolCommand { get; private set; }

        public RelayMapToolCommand ActivateAddLinkTool { get; private set; }

        public void RefreshButtons()
        {
            new[]
            {
                ActivateGenerateLinksToolCommand
            }.ForEach(c => c.Refresh());
        }
    }

    public class LinkType : INameable
    {
        public string Name { get; set; }
        public string ToolTipText { get; set; }
        public LinkTypeType Type { get; set; }
    }

    public enum LinkTypeType
    {
        Embedded,
        Lateral,
        RoofSewer,
        InhabitantsSewer,
        GullySewer
    }
}

