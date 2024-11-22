﻿using System.Windows.Controls;
using System.Windows.Input;
using DelftTools.Controls;
using DeltaShell.NGHS.IO.DataObjects;
using Image = System.Drawing.Image;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Forms
{
    /// <summary>
    /// Interaction logic for Model1DLateralSourceDataViewWpf.xaml
    /// </summary>
    public partial class Model1DLateralSourceDataViewWpf : IView
    {
        private readonly Model1DLateralSourceDataViewWpfViewModel viewModel = new Model1DLateralSourceDataViewWpfViewModel();

        public Model1DLateralSourceDataViewWpf()
        {
            InitializeComponent();
            DataContext = viewModel;
        }      

        #region Implementation of IDisposable

        public void Dispose(){}

        #endregion

        #region Implementation of IView

        public void EnsureVisible(object item){}

        public object Data
        {
            get { return viewModel.Model1DLateralSourceData; }
            set { viewModel.Model1DLateralSourceData = (Model1DLateralSourceData)value; }
        }

        public string Text { get; set; }
        public Image Image { get; set; }
        public bool Visible { get; private set; }
        public ViewInfo ViewInfo { get; set; }

        #endregion

        private void OnSelectionFlowDataTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.UpdateFlowDataViewTab();
        }

        private void OnSelectionSalinityDataTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.UpdateSalinityDataViewTab();
        }
        
        private void OnSelectionTemperatureDataTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.UpdateTemperatureDataViewTab();
        }
        
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var tb = ((TextBox) sender);
            bool dot = tb.Text.IndexOf(".") < 0 && e.Text.Equals(".") && tb.Text.Length > 0;
            bool neg = tb.Text.IndexOf("-") < 0 && e.Text.Equals("-") && tb.Text.Length >= 0;
            e.Handled = !double.TryParse(e.Text, out double _) && !dot && !neg;
        }

        private void ProhibitSpaceKey(object sender, KeyEventArgs e)
        {
            // Prohibit space
            if (e.Key == Key.Space)
                e.Handled = true;
        }

    }
}
