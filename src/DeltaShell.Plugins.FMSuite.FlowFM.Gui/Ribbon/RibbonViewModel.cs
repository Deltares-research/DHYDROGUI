using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.SharpMapGis.Gui.Commands;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Ribbon
{
    public class RibbonViewModel : INotifyPropertyChanged
    {
        private LinkType selectedLinkType;

        public RibbonViewModel()
        {

            ActivateAddBoundaryToolCommand = new RelayMapToolCommand
            {
                MapToolCommand = new MapToolCommand(FlowFMMapViewDecorator.BoundaryToolName) {LayerType = typeof (AreaLayer)}
            };
            ActivateAddSourceSinkToolCommand = new RelayMapToolCommand
            {
                MapToolCommand = new MapToolCommand(FlowFMMapViewDecorator.SourceAndSinkToolName) { LayerType = typeof(AreaLayer) }
            }; 
            ActivateAddSourceToolCommand = new RelayMapToolCommand
            {
                MapToolCommand = new MapToolCommand(FlowFMMapViewDecorator.SourceToolName) { LayerType = typeof(AreaLayer) }
            }; 
            ActivateReverseLineToolCommand = new RelayMapToolCommand
            {
                MapToolCommand = new MapToolCommand(FlowFMMapViewDecorator.Reverse2DLineToolName) { LayerType = typeof(AreaLayer) }
            }; 
            ActivateGenerateEmbankmentsToolCommand = new RelayMapToolCommand
            {
                MapToolCommand = new MapToolCommand(FlowFMMapViewDecorator.GenerateEmbankmentsToolName) { LayerType = typeof(AreaLayer) }
            }; 
            ActivateMergeEmbankmentsToolCommand = new RelayMapToolCommand
            {
                MapToolCommand = new MapToolCommand(FlowFMMapViewDecorator.MergeEmbankmentsToolName) { LayerType = typeof(AreaLayer) }
            };
            ActivateGridWizardToolCommand = new RelayMapToolCommand
            {
                MapToolCommand = new MapToolCommand(FlowFMMapViewDecorator.GridWizardToolName) { LayerType = typeof(AreaLayer) }
            };
            ActivateGenerateLinksToolCommand = new RelayMapToolCommand
            {
                MapToolCommand = new MapToolCommand(FlowFMMapViewDecorator.GenerateLinksToolName) { LayerType = typeof(AreaLayer) }
            };

            LinkTypes = new ObservableCollection<LinkType>
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
                    Name = "Inhabitants-sewer",
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

        public ObservableCollection<LinkType> LinkTypes { get; private set; }

        public LinkType SelectedLinkType
        {
            get { return selectedLinkType; }
            set
            {
                selectedLinkType = value;
                OnPropertyChanged(nameof(SelectedLinkType));
            }
        }

        public RelayMapToolCommand ActivateAddBoundaryToolCommand { get; private set; }
        public RelayMapToolCommand ActivateAddSourceSinkToolCommand { get; private set; }
        public RelayMapToolCommand ActivateAddSourceToolCommand { get; private set; }
        public RelayMapToolCommand ActivateReverseLineToolCommand { get; private set; }
        public RelayMapToolCommand ActivateGenerateEmbankmentsToolCommand { get; private set; }  
        public RelayMapToolCommand ActivateMergeEmbankmentsToolCommand { get; private set; }
        public RelayMapToolCommand ActivateGridWizardToolCommand { get; private set; }
        public RelayMapToolCommand ActivateGenerateLinksToolCommand { get; private set; }

        public RelayMapToolCommand ActivateAddLinkTool { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RefreshButtons()
        {
            new[]
            {
                ActivateAddBoundaryToolCommand,
                ActivateAddSourceSinkToolCommand,
                ActivateAddSourceToolCommand,
                ActivateReverseLineToolCommand,
                ActivateGenerateEmbankmentsToolCommand,
                ActivateMergeEmbankmentsToolCommand,
                ActivateGridWizardToolCommand,
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

