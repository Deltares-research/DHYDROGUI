using System;
using System.Windows;
using System.Windows.Media;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public class GwswGraphicsProvider : IGraphicsProvider
    {
        private readonly ResourceDictionary resources = new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/DeltaShell.NGHS.Common.Gui;component/Brushes.xaml")
        };

        public bool CanProvideDrawingGroupFor(object item)
        {
            if (item is ProjectTemplate projectTemplate)
            {
                return projectTemplate.Id == GWSWImporterApplicationPlugin.GWSWImportTemplateId;
            }

            return item is GwswFileImporter;
        }

        public DrawingGroup CreateDrawingGroupFor(object item)
        {
            if (item is GwswFileImporter || item is ProjectTemplate)
            {
                return (DrawingGroup) resources["ManholeDrawingGroup"];
            }

            return null;
        }
    }
}