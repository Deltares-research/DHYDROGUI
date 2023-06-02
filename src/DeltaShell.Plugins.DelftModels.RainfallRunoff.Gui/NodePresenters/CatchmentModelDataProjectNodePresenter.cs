using System.Drawing;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.NodePresenters
{
    public class CatchmentModelDataProjectNodePresenter : TreeViewNodePresenterBaseForPluginGui<CatchmentModelData>
    {
        private static readonly Image imgNotInUseConcept = Resources.NotInUseConcept;
        private static readonly Image imgPavedConcept = Resources.paved;
        private static readonly Image imgUnpavedConcept = Resources.unpaved;
        private static readonly Image imgGreenhouseConcept = Resources.greenhouse;
        private static readonly Image imgOpenWaterConcept = Resources.openwater;
        private static readonly Image imgSacramentoConcept = Resources.sacramento;
        private static readonly Image imgHbvConcept = Resources.hbv;
        private static readonly Image imgNwrwConcept = Resources.nwrw;

        public CatchmentModelDataProjectNodePresenter(GuiPlugin guiPlugin) : base(guiPlugin)
        {
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, CatchmentModelData nodeData)
        {
            node.Text = nodeData.Name;
            node.Image = GetImage(nodeData);
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            return null;
        }

        private static Image GetImage(CatchmentModelData catchmentModelData)
        {
            Image img = imgNotInUseConcept;
            if (catchmentModelData.GetType() == typeof (PavedData))
            {
                img = imgPavedConcept;
            }
            else if (catchmentModelData.GetType() == typeof (UnpavedData))
            {
                img = imgUnpavedConcept;
            }
            else if (catchmentModelData.GetType() == typeof (GreenhouseData))
            {
                img = imgGreenhouseConcept;
            }
            else if (catchmentModelData.GetType() == typeof (OpenWaterData))
            {
                img = imgOpenWaterConcept;
            }
            else if (catchmentModelData.GetType() == typeof(SacramentoData))
            {
                img = imgSacramentoConcept;
            }
            else if (catchmentModelData.GetType() == typeof(HbvData))
            {
                img = imgHbvConcept;
            }
            else if (catchmentModelData.GetType() == typeof(NwrwData))
            {
                img = imgNwrwConcept;
            }
            return img;
        }
    }
}