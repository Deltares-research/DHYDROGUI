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
            Source = new Uri("pack://application:,,,/DeltaShell.Plugins.ImportExport.Sobek;component/SobekImportGraphics.xaml")
        };

        public bool CanProvideDrawingGroupFor(object item)
        {
            return item is ProjectTemplate projectTemplate && projectTemplate.Id == "Sobek2ImportTemplate";
        }

        public DrawingGroup CreateDrawingGroupFor(object item)
        {
            return (DrawingGroup) resources["ImporterDrawing"];
        }
    }
}