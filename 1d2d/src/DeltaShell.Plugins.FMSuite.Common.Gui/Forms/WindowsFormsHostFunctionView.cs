using System.Windows;
using System.Windows.Forms.Integration;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Forms
{
    public static class WindowsFormsHostFunctionView
    {
        public static readonly DependencyProperty DataProperty
            = DependencyProperty.RegisterAttached("Data", typeof(object), typeof(WindowsFormsHostFunctionView), new PropertyMetadata(PropertyChanged));

        public static object GetData(WindowsFormsHost o)
        {
            return (object)o.GetValue(DataProperty);
        }

        public static void SetData(WindowsFormsHost o, object value)
        {
            o.SetValue(DataProperty, value);
        }

        private static void PropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var windowsFormsHost = sender as WindowsFormsHost;
            if (windowsFormsHost == null) return;

            var functionView = windowsFormsHost.Child as FunctionView;
            if (functionView == null) return;

            functionView.Data = null;
            if (e.NewValue is Variable<double>) return; // skip constant functions

            functionView.Data = e.NewValue;
        }
    }
}