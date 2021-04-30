using System;
using System.Windows;
using System.Windows.Media;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.GraphicsProviders
{
    public class HydroModelGuiGraphicsProvider : IGraphicsProvider
    {
        private readonly ResourceDictionary resources = new ResourceDictionary
        {
            MergedDictionaries =
            {
                new ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/DeltaShell.NGHS.Common.Gui;component/Brushes.xaml")
                },
                new ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/DelftTools.Controls.Wpf;component/DeltaresStyleDictionairy.xaml")
                }
            }
        };

        public bool CanProvideDrawingGroupFor(object item)
        {
            if (item is ModelInfo modelInfo)
            {
                return modelInfo.Name == DelftTools.Shell.Core.Properties.Resources.HydroModelApplicationPlugin_GetModelInfos__1D_2D_Integrated_Model + " (RHU)";
            }

            if (item is ProjectTemplate projectTemplate)
            {
                return projectTemplate.Id == HydroModelApplicationPlugin.RHUINTEGRATEDMODEL_TEMPLATE_ID;
            }

            if (item is DHydroConfigXmlImporter)
            {
                return true;
            }

            return false;
        }

        public DrawingGroup CreateDrawingGroupFor(object item)
        {
            if ((item is ModelInfo modelInfo && modelInfo.Name == DelftTools.Shell.Core.Properties.Resources.HydroModelApplicationPlugin_GetModelInfos__1D_2D_Integrated_Model + " (RHU)") ||
                (item is ProjectTemplate projectTemplate && projectTemplate.Id == HydroModelApplicationPlugin.RHUINTEGRATEDMODEL_TEMPLATE_ID))
            {
                return (DrawingGroup) resources["HydroModelDrawingGroup"];
            }

            if (item is DHydroConfigXmlImporter)
            {
                return (DrawingGroup)resources["DeltaresLogoDrawing"];
            }

            return null;
        }
    }
}