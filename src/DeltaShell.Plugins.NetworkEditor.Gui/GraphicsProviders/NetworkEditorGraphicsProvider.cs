using System;
using System.Windows;
using System.Windows.Media;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Reflection;

namespace DeltaShell.Plugins.NetworkEditor.Gui.GraphicsProviders
{
    public class NetworkEditorGraphicsProvider : IGraphicsProvider
    {
        private readonly ResourceDictionary resources = new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/DeltaShell.Plugins.NetworkEditor.Gui;component/GraphicsProviders/NetworkEditorGraphics.xaml")
        };

        public bool CanProvideDrawingGroupFor(object item)
        {
            var dataItemInfo = item as DataItemInfo;
            if (dataItemInfo == null) return false;

            return dataItemInfo.ValueType.Implements(typeof(IHydroNetwork)) || 
                   dataItemInfo.ValueType.Implements(typeof(IHydroRegion)) ||
                   dataItemInfo.ValueType.IsInstanceOfType(typeof(HydroArea))
                ;
        }

        public DrawingGroup CreateDrawingGroupFor(object item)
        {
            var dataItemInfo = item as DataItemInfo;
            if (dataItemInfo == null) return null;

            if (dataItemInfo.ValueType.Implements(typeof(IHydroNetwork)))
            {
                return (DrawingGroup)resources["NetworkDrawingGroup"];
            }
            if (dataItemInfo.ValueType.Implements(typeof(IHydroRegion)))
            {
                return (DrawingGroup)resources["RegionDrawingGroup"];
            }
            if (dataItemInfo.ValueType.IsInstanceOfType(typeof(HydroArea)))
            {
                return (DrawingGroup)resources["AreaDrawingGroup"];
            }

            return null;
        }
    }
}