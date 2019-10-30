using System;
using System.Windows;
using System.Windows.Media;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;

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
            var modelInfo = item as ModelInfo;
            if (modelInfo == null) return false;
            /*var possibleModel = modelInfo.CreateModel(null);
            if (possibleModel == null) return false;
            return possibleModel is IWaterFlowFMModel;*/
            return modelInfo.Name == FlowFMApplicationPlugin.FlowFlexibleMeshModelModelInfoName;
        }

        public DrawingGroup CreateDrawingGroupFor(object item)
        {
            var modelInfo = item as ModelInfo;
            if (modelInfo == null) return null;
            if (modelInfo.Name == FlowFMApplicationPlugin.FlowFlexibleMeshModelModelInfoName)
            { 
                return (DrawingGroup)resources["FMModelDrawingGroup"];
            }
            /*var possibleModel = modelInfo.CreateModel(null);
            if (possibleModel == null) return null;
            if(possibleModel is IWaterFlowFMModel)
            { 
                return (DrawingGroup)resources["FMModelDrawingGroup"];
            }*/
            
            return null;
        }
    }
}