using System;
using System.Windows;
using System.Windows.Media;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.GraphicsProviders
{
    public class FmGuiGraphicsProvider : IGraphicsProvider
    {
        private readonly ResourceDictionary resources = new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/DeltaShell.Plugins.FMSuite.FlowFM.Gui;component/GraphicsProviders/FMGuiGraphics.xaml")
        };

        public bool CanProvideDrawingGroupFor(object item)
        {
            switch (item)
            {
                case Type type when type == typeof(WaterFlowFMModel):
                    return true;
                case ModelInfo modelInfo:
                    return modelInfo.Name == FlowFMApplicationPlugin.FlowFlexibleMeshModelModelInfoName;
                case ProjectTemplate projectTemplate:
                    return projectTemplate.Id == "FMModel";
                case FlowFMApplicationPlugin _:
                    return true;
                case FlowFMGuiPlugin _:
                    return true;
                default:
                    return item is WaterFlowFMFileImporter;
            }
        }

        public DrawingGroup CreateDrawingGroupFor(object item)
        {
            if ((item is ModelInfo modelInfo && modelInfo.Name == FlowFMApplicationPlugin.FlowFlexibleMeshModelModelInfoName) ||
                (item is ProjectTemplate projectTemplate && projectTemplate.Id == "FMModel") ||
                item is WaterFlowFMFileImporter ||
                item is FlowFMApplicationPlugin ||
                item is FlowFMGuiPlugin ||
                (item is Type type && type== typeof(WaterFlowFMModel)))
            {
                return (DrawingGroup) resources["FMModelDrawingGroup"];
            }
            
            return null;
        }
    }
}