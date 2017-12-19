using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DelftTools.Controls.Wpf.Commands;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.ViewModels
{
    [Entity]
    public class DelimeterSelectorViewModel
    {
        public ObservableCollection<DelimeterOption> DelimeterOptions { get; set; }
        public Action<bool> CloseAction { get; set; }

        public bool TabChecked { get; set; }
        public bool CommaChecked { get; set; }
        public bool SemicolonChecked { get; set; }
        public bool SpaceChecked { get; set; }
        public bool OtherChecked { get; set; }
        public char OtherValue { get; set; }

        public char SelectedDelimeter { get; set; }

        #region

        public ICommand OnUpdateDelimeter
        {
            get { return new RelayCommand(param => UpdateDelimeter()); }
        }

        public DelimeterSelectorViewModel()
        {
            OtherValue = ' ';
            SemicolonChecked = true;
        }
        
        private void UpdateDelimeter()
        {
            if (TabChecked)
                SelectedDelimeter = '\t';
            if (CommaChecked)
                SelectedDelimeter = ',';
            if (SemicolonChecked)
                SelectedDelimeter = ';';
            if (SpaceChecked)
                SelectedDelimeter = ' ';
            if (OtherChecked)
                SelectedDelimeter = OtherValue;

            CloseAction?.Invoke(true);
        }

        public ICommand OnSetOptionChecked
        {
            get { return new RelayCommand(param => SetOptionChecked());}
        }

        public ICommand OnCancel
        {
            get { return new RelayCommand(param => CloseAction(false)); }
        }

        private void SetOptionChecked()
        {
            TabChecked = SelectedDelimeter == '\t';
            CommaChecked = SelectedDelimeter == ',';
            SemicolonChecked = SelectedDelimeter == ';';
            SpaceChecked = SelectedDelimeter == ' ';
            OtherChecked = false;

            if ( TabChecked || CommaChecked || SemicolonChecked || SpaceChecked) return;

            OtherChecked = true;
        }


        #endregion
    }

    public class DelimeterOption
    {
        public char Delimeter { get; set; }
        public string Name { get; set; }
    }
}