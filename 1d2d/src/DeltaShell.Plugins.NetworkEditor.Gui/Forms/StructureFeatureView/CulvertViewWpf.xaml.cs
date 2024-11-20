using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using Image = System.Drawing.Image;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{

    /// <summary>
    /// Interaction logic for CulvertViewWpf.xaml
    /// </summary>
    /// 
    public partial class CulvertViewWpf : IView
    {
        private ICulvert data;
        private bool settingViewModel;

        public CulvertViewWpf()
        {
            InitializeComponent();
            SetComboBoxes();
        }

        private void GateDialog_OnClick(object sender, RoutedEventArgs e)
        {
            var editFunctionDialog = new EditFunctionDialog {Text = "Gate-opening loss coefficient table for Culvert"};
            var dialogData = data.GateOpeningLossCoefficientFunction;
            editFunctionDialog.ColumnNames = new[] {"Gate height opening factor", "Loss coefficient"};
            editFunctionDialog.Data = dialogData;
            if (DialogResult.OK == editFunctionDialog.ShowDialog())
            {
                data.GateOpeningLossCoefficientFunction = dialogData;
            }
        }

        private void DataGridGeometryTabulated_AutoGeneratingColumn(object sender,
            DataGridAutoGeneratingColumnEventArgs e)
        {
            var acceptedColumns = new List<String>() {"z", "width", "storagewidth"};
            if (!acceptedColumns.Contains(e.Column.Header.ToString().ToLower()))
            {
                e.Column.Visibility = Visibility.Hidden;
            }
        }

        #region Comboboxes initializers

        private void SetComboBoxes()
        {
            SetComboWithEnum<CulvertFrictionType>(comboBoxRoughnessType);
            SetComboWithEnum<CulvertGeometryType>(comboBoxGeometryType);
            SetComboWithEnum<CulvertType>(comboBoxCulvertType);
        }

        private void SetComboWithEnum<T>(System.Windows.Controls.ComboBox comboBox)
        {
            comboBox.ItemsSource = Enum.GetValues(typeof(T)).Cast<object>().ToArray();
            comboBox.IsEnabled = true;
        }

        #endregion

        #region IView<ICulvert> Members

        public object Data
        {
            get { return data; }
            set
            {
                if (data != null)
                {
                    ((INotifyPropertyChanged) data).PropertyChanged -= OnPropertyChanged;
                }

                data = (ICulvert) value;

                if (data == null)
                {
                    DataContext = null;
                    return;
                }

                ((INotifyPropertyChanged) data).PropertyChanged += OnPropertyChanged;

                settingViewModel = true;
                DataContext = new CulvertViewWpfViewModel {Culvert = data};
                settingViewModel = false;
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (settingViewModel) return;
            settingViewModel = true;

            // full refresh
            ((CulvertViewWpfViewModel) DataContext).Culvert = data;
            settingViewModel = false;
        }

        public Image Image { get; set; }

        public void EnsureVisible(object item)
        {
        }

        public ViewInfo ViewInfo { get; set; }

        #endregion

        #region Implementation of IView

        public string Text { get; set; }
        public bool Visible { get; private set; }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            DataContext = null;
        }

        #endregion
    }
}
