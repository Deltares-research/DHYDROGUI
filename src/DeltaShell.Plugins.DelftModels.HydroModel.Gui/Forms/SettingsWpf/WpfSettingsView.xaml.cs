using System.Linq;
using DelftTools.Controls;
using DelftTools.Hydro;
using Image = System.Drawing.Image;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf
{
    /// <summary>
    /// Initializes a WPF Settings view for a generic IHydroModel that gets its properties from a CSV.
    /// </summary>
    /// <seealso cref="System.Windows.Controls.UserControl" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    /// <seealso cref="DelftTools.Controls.IView" />
    public partial class WpfSettingsView : IView
    {
        private IHydroModel data;
        public WpfSettingsView()
        {
            InitializeComponent();
        }

        #region Implmentation of IView
        public string Text { get; set; }
        public bool Visible { get; }
        #endregion

        #region IView Members
        public object Data
        {
            get { return data; }
            set
            {
                data = (IHydroModel)value;

                if (data == null)
                {
                    DataContext = null;
                    return;
                }
                DataContext = new WpfSettingsViewModel{DataModel = data};
            }
        }
        
        public Image Image
        {
            get; set;
        }
        public ViewInfo ViewInfo { get; set; }

        /// <summary>
        /// Makes object visible in the view if possible
        /// </summary>
        /// <param name="item"></param>
        public void EnsureVisible(object item)
        {
            var settings = ((WpfSettingsViewModel)DataContext);
            if (item is string && settings != null)
            {
                var tabName = (item as string).ToLowerInvariant();
                var selectedTab =
                    settings.SettingsCategories.IndexOf( settings.SettingsCategories.FirstOrDefault( c => c.CategoryName
                        .ToLowerInvariant().Equals(tabName)));
                mainTabControl.SelectedIndex = selectedTab;
            }
        }

        #endregion

        #region Implementation of IDisposable
        public void Dispose()
        {
            //            throw new NotImplementedException();
        }
        #endregion

    }
}
