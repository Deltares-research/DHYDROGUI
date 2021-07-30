using System.Threading;
using System.Windows;
using System.Windows.Forms.Integration;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections.Extensions;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    public static class WindowsFormsHostProfileView
    {
        public static readonly DependencyProperty PipeProperty = DependencyProperty.RegisterAttached("Pipe", typeof(IPipe), typeof(WindowsFormsHostProfileView), new PropertyMetadata(PipePropertyChanged));
        public static void SetPipe(DependencyObject element, IPipe value)
        {
            element.SetValue(PipeProperty, value);
        }
        public static IPipe GetPipe(DependencyObject element)
        {
            return (IPipe)element.GetValue(PipeProperty);
        }
        [InvokeRequired]
        private static void PipePropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var windowsFormsHost = sender as WindowsFormsHost;
            var profileChartView = windowsFormsHost?.Child as ProfileChartView;
            if (e.Property != PipeProperty) return;
            var pipe = e.NewValue as IPipe;
            if (pipe == null)
            {
                profileChartView?.SetData(null, null,null);
                return;
            }

            var crossSectionDefinition = pipe.CrossSection?.Definition.GetBaseDefinition();
            var crossSectionSections = new SectionsBindingList(SynchronizationContext.Current, crossSectionDefinition?.Sections);
            var network = pipe.Network as IHydroNetwork;
            var viewModel = CrossSectionDefinitionViewModelProvider.GetViewModel(crossSectionDefinition, network);
            profileChartView?.SetData(crossSectionDefinition, crossSectionSections, viewModel);
        }
    }
   public static class WindowsFormsHostCrossSectionView
    {
        public static readonly DependencyProperty SewerConnectionProperty = DependencyProperty.RegisterAttached("SewerConnection", typeof(ISewerConnection), typeof(WindowsFormsHostCrossSectionView), new PropertyMetadata(SewerConnectionPropertyChanged));
        public static void SetSewerConnection(DependencyObject element, ISewerConnection value)
        {
            element.SetValue(SewerConnectionProperty, value);
        }

        public static ISewerConnection GetSewerConnection(DependencyObject element)
        {
            return (ISewerConnection)element.GetValue(SewerConnectionProperty);
        }

        [InvokeRequired]
        private static void SewerConnectionPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!((sender as WindowsFormsHost)?.Child is CrossSectionPipeView crossSectionView))
            {
                return;
            }

            if (e.Property != SewerConnectionProperty) return;

            crossSectionView.Data = (e.NewValue as ISewerConnection)?.CrossSection;
        }
    }
}