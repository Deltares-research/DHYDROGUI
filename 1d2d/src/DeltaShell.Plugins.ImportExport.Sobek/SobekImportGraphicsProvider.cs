using System;
using System.Windows;
using System.Windows.Media;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    public class SobekImportGraphicsProvider : IGraphicsProvider
    {
        private readonly ResourceDictionary resources = new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/DeltaShell.NGHS.Common.Gui;component/Brushes.xaml")
        };

        public bool CanProvideDrawingGroupFor(object item)
        {
            if (item is ProjectTemplate projectTemplate)
            {
                return projectTemplate.Id == SobekImportApplicationPlugin.Sobek2ImportTemplateId;
            }

            if (item is SobekHydroModelImporter)
            {
                return true;
            }

            return false;
        }

        public DrawingGroup CreateDrawingGroupFor(object item)
        {
            if (item is ProjectTemplate || item is SobekHydroModelImporter)
            {
                return (DrawingGroup)resources["SobekImporterDrawing"];
            }
            
            return null;
        }
    }
}