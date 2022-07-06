using System;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView
{
    /// <summary>
    /// Can this be something else than IBindinglist???
    /// </summary>
    public partial class ZWSectionsView : UserControl, IView
    {
        private ZWSectionsViewModel data;

        public ZWSectionsView()
        {
            InitializeComponent();
            toolTip.SetToolTip(addSectionTypeMain, "Add section type 'Main' to network");
            toolTip.SetToolTip(addSectionTypeFp1, "Add section type 'FloodPlain1' to network");
            toolTip.SetToolTip(addSectionTypeFp2, "Add section type 'FloodPlain2' to network");
        }

        public object Data
        {
            get { return data; }
            set
            {
                data = (ZWSectionsViewModel) value;
                
                if (data ==null)
                {
                    //bind to null gives exception
                    viewModelBindingSource.DataSource = typeof (ZWSectionsViewModel);
                }
                else
                {
                    viewModelBindingSource.DataSource = data;
                }
            }
        }

        public Image Image
        {
            get; set;
        }

        public void EnsureVisible(object item)
        {
        //    throw new NotImplementedException();
        }

        public ViewInfo ViewInfo { get; set; }

        private void addSectionTypeMain_Click(object sender, EventArgs e)
        {
            data.AddMainSectionType();
        }

        private void addSectionTypeFp1_Click(object sender, EventArgs e)
        {
            data.AddFp1SectionType();
        }

        private void addSectionTypeFp2_Click(object sender, EventArgs e)
        {
            data.AddFp2SectionType();
        }

        private void viewModelBindingSource_CurrentChanged(object sender, EventArgs e)
        {

        }

		// HACK: to display binding properties correctly when they
		// are modified in the ViewModel's setter
        private bool isResetting;
        private void viewModelBindingSource_BindingComplete(object sender, BindingCompleteEventArgs e)
        {
            if (e.BindingCompleteContext == BindingCompleteContext.ControlUpdate || isResetting) return;

            isResetting = true;
            try
            {
                var bindingSource = sender as BindingSource;
                if (bindingSource != null)
                {
                    bindingSource.ResetBindings(false);
                }
            }
            finally
            {
                isResetting = false;
            }
        }
    }
}
