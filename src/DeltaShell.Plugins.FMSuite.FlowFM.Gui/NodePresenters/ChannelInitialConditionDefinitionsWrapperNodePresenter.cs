using System;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.PresentationObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters
{
    public class ChannelInitialConditionDefinitionsWrapperNodePresenter : TreeViewNodePresenterBaseForPluginGui<ChannelInitialConditionDefinitionsWrapper>
    {
        public override bool CanRenameNode(ITreeNode node)
        {
            return false;
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, ChannelInitialConditionDefinitionsWrapper nodeData)
        {
            node.Image = Resources.waterLayers;
            if (parentNode.Tag is TreeFolder treeFolder && treeFolder.Parent is WaterFlowFMModel fmModel)
            {
                var property = fmModel.ModelDefinition.GetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D);
                
                if (property != null)
                {
                    if (int.TryParse(property.GetValueAsString(), out var quantity))
                    {
                        if (Enum.IsDefined(typeof(InitialConditionQuantity), quantity))
                        {
                            var quantityAsString = InitialConditionQuantityTypeConverter
                                .ConvertInitialConditionQuantityToString((InitialConditionQuantity) quantity);
                            node.Text = $"Channels - {quantityAsString}";
                        }
                        else
                        {
                            node.Text = $"Channels";
                        }
                    }
                    else
                    {
                        node.Text = $"Channels";
                    }
                }
                else
                {
                    node.Text = $"Channels";
                }
            }
            else
            {
                node.Text = $"Channels";
            }
            
        }

    }
}