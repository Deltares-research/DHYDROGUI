using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Commands;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    public class LeveeBreachViewModel : INotifyPropertyChanged
    {
        private bool useActive;
        private LeveeBreach leveeBreach;
        private bool showGenerateTablePopup;

        public LeveeBreachViewModel()
        {
            ClosePopupCommand = new RelayCommand(ClosePopup);
            GenerateTableCommand = new RelayCommand(GenerateTable);
            LeveeBreach = new LeveeBreach();
        }

        public LeveeBreach LeveeBreach
        {
            get { return leveeBreach; }
            set
            {
                leveeBreach = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedGrowthFormula));
                OnPropertyChanged(nameof(LeveeBreachSettings));
                OnPropertyChanged(nameof(UseSand));
                OnPropertyChanged(nameof(UseClay));
                OnPropertyChanged(nameof(UseActive));
            }
        }

        public LeveeBreachGrowthFormula SelectedGrowthFormula
        {
            get { return LeveeBreach?.LeveeBreachFormula ?? LeveeBreachGrowthFormula.VdKnaap2000; }
            set
            {
                if (LeveeBreach == null) return;
                LeveeBreach.LeveeBreachFormula = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LeveeBreachSettings));
            }
        }

        public bool UseActive
        {
            get { return useActive; }
            set
            {
                useActive = value;
                OnPropertyChanged();
            }
        }

        public LeveeBreachSettings LeveeBreachSettings
        {
            get { return LeveeBreach?.GetLeveeBreachSettings(); }
            set { }
        }

        public bool ShowGenerateTablePopup
        {
            get { return showGenerateTablePopup; }
            set
            {
                showGenerateTablePopup = value;
                OnPropertyChanged();
            }
        }

        public bool UseSand
        {
            get { return GetLeveeMaterial() == LeveeMaterial.Sand; }
            set
            {
                if (!value) return;
                var settings = LeveeBreach?.GetLeveeBreachSettings() as LeveeBreachSettingsVdKnaap2000;
                if (settings == null) return;
                settings.LeveeMaterial = LeveeMaterial.Sand;
            }
        }

        public bool UseClay
        {
            get { return GetLeveeMaterial() == LeveeMaterial.Clay; }
            set
            {
                if (!value) return;
                var settings = LeveeBreach?.GetLeveeBreachSettings() as LeveeBreachSettingsVdKnaap2000;
                if (settings == null) return;
                settings.LeveeMaterial = LeveeMaterial.Clay;
            }
        }

        public ICommand GenerateTableCommand { get; set; }

        private void GenerateTable(object obj)
        {
            GenerateTable();
        }

        private void GenerateTable()
        {
            var settings = LeveeBreach?.GetLeveeBreachSettings() as LeveeBreachSettingsVdKnaap2000;
            if (settings == null) return;

            settings.BreachGrowthSettings = CreateDummySettings(); // TODO Implement actual method to create settings

            ClosePopup();
        }

        private static ObservableCollection<SomeSettingClass> CreateDummySettings()
        {
            return new ObservableCollection<SomeSettingClass>
            {
                new SomeSettingClass{ TimeSpan = new TimeSpan(0, 1, 0, 0), Height = 1.0, Width = 2.0},
                new SomeSettingClass{ TimeSpan = new TimeSpan(1, 2, 0, 0), Height = 2.0, Width = 3.0},
                new SomeSettingClass{ TimeSpan = new TimeSpan(2, 3, 0, 0), Height = 3.0, Width = 4.0},
                new SomeSettingClass{ TimeSpan = new TimeSpan(3, 4, 0, 0), Height = 4.0, Width = 5.0},
            };
        }

        public ICommand ClosePopupCommand { get; set; }

        private void ClosePopup(object obj)
        {
            ClosePopup();
        }

        private void ClosePopup()
        {
            ShowGenerateTablePopup = false;
        }

        private LeveeMaterial GetLeveeMaterial()
        {
            var settings = LeveeBreach?.GetLeveeBreachSettings() as LeveeBreachSettingsVdKnaap2000;
            return settings?.LeveeMaterial ?? LeveeMaterial.Sand;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}