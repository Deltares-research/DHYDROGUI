using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using DelftTools.Controls;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Collections;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms
{
    /// <summary>
    /// Interaction logic for RefreshMainSectionWidthsDialog.xaml
    /// </summary>
    public partial class RefreshMainSectionWidthsDialog : IView
    {
        public RefreshMainSectionWidthsDialog()
        {
            InitializeComponent();
        }

        private IEnumerable<ICrossSection> crossSections;

        public void EnsureVisible(object item)
        {
        }

        object IView.Data
        {
            get { return Data; }
            set { Data = (IEnumerable<ICrossSection>)value; }
        }

        public string Text { get; set; }
        public Image Image { get; set; }
        public bool Visible { get; }
        public ViewInfo ViewInfo { get; set; }

        public IEnumerable<ICrossSection> Data
        {
            get { return crossSections; }
            set { crossSections = value; }
        }



        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            crossSections.ForEach(cs =>
            {
                var crossSectionDefZw = cs.Definition as CrossSectionDefinitionZW;
                if (crossSectionDefZw != null) crossSectionDefZw.RefreshSectionsWidths();

                var crossSectionDefProxy = cs.Definition as CrossSectionDefinitionProxy;
                if (crossSectionDefProxy == null) return;

                crossSectionDefZw = crossSectionDefProxy.InnerDefinition as CrossSectionDefinitionZW;
                if (crossSectionDefZw != null) crossSectionDefZw.RefreshSectionsWidths();
            });
            Close();
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}
