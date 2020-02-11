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
            if (item is ModelInfo modelInfo)
            {
                return modelInfo.Name == FlowFMApplicationPlugin.FlowFlexibleMeshModelModelInfoName;
            }

            if (item is ProjectTemplate projectTemplate)
            {
                return projectTemplate.Id == "FMModel";
            }

            if (item is WaterFlowFMFileImporter)
            {
                return true;
            }

            return false;
        }

        public DrawingGroup CreateDrawingGroupFor(object item)
        {
            if ((item is ModelInfo modelInfo && modelInfo.Name == FlowFMApplicationPlugin.FlowFlexibleMeshModelModelInfoName) ||
                (item is ProjectTemplate projectTemplate && projectTemplate.Id == "FMModel") ||
                item is WaterFlowFMFileImporter)
            {
                return (DrawingGroup) resources["FMModelDrawingGroup"];
            }


            return null;
        }
    }
}