using System;
using System.Windows;
using System.Windows.Media;
using DelftTools.Hydro.Properties;
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
                return modelInfo.Name == Resources.HydroModelGuiGraphicsProvider_CanProvideDrawingGroupFor_1D_2D_Integrated_Model_RHU;
            }

            if (item is ProjectTemplate projectTemplate)
            {
                return projectTemplate.Id == HydroModelApplicationPlugin.RHUINTEGRATEDMODEL_TEMPLATE_ID ||
                       projectTemplate.Id == HydroModelApplicationPlugin.DimrProjectTemplateId;
            }

            if (item is DHydroConfigXmlImporter)
            {
                return true;
            }

            return false;
        }

        public DrawingGroup CreateDrawingGroupFor(object item)
        {
            if (item is ModelInfo modelInfo && modelInfo.Name == Resources.HydroModelGuiGraphicsProvider_CanProvideDrawingGroupFor_1D_2D_Integrated_Model_RHU)
            {
                return (DrawingGroup) resources["HydroModelDrawingGroup"];
            }

            if (item is ProjectTemplate projectTemplate)
            {
                if (projectTemplate.Id == HydroModelApplicationPlugin.RHUINTEGRATEDMODEL_TEMPLATE_ID)
                {
                    return (DrawingGroup)resources["HydroModelDrawingGroup"];
                }

                if (projectTemplate.Id == HydroModelApplicationPlugin.DimrProjectTemplateId)
                {
                    return (DrawingGroup)resources["DeltaresIconDrawing"];
                }
            }

            if (item is DHydroConfigXmlImporter)
            {
                return (DrawingGroup)resources["DeltaresIconDrawing"];
            }

            return null;
        }
    }
}