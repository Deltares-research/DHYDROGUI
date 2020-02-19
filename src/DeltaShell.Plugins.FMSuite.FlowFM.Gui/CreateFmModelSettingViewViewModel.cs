using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using DelftTools.Controls.Wpf.Commands;
using DelftTools.Shell.Core;
using GeoAPI.Extensions.CoordinateSystems;
using SharpMap;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui
{
    public class CreateFmModelSettingViewViewModel : INotifyPropertyChanged
    {
        private readonly ICommand applyTemplateCommand;
        private readonly ICommand cancelTemplateCommand;
        private CollectionView coordinateSystemsView;
        private string filterText;
        private ProjectTemplate projectTemplate;

        public CreateFmModelSettingViewViewModel()
        {
            applyTemplateCommand = new RelayCommand((o) =>
            {
                ExecuteProjectTemplate?.Invoke(FmModelSettings);
            }, o => ExecuteProjectTemplate != null);

            cancelTemplateCommand = new RelayCommand((o) =>
            {
                CancelProjectTemplate?.Invoke();
            }, o => CancelProjectTemplate != null);

            coordinateSystemsView = (CollectionView)CollectionViewSource.GetDefaultView(Map.CoordinateSystemFactory.SupportedCoordinateSystems);
            coordinateSystemsView.Filter = null;

            FmModelSettings.CoordinateSystem = Map.CoordinateSystemFactory.SupportedCoordinateSystems.FirstOrDefault(c => c.AuthorityCode == 28992);
        }

        /// <summary>
        /// <see cref="ProjectTemplate"/> to display
        /// </summary>
        public ProjectTemplate ProjectTemplate
        {
            get { return projectTemplate; }
            set
            {
                projectTemplate = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Command for applying the <see cref="ProjectTemplate"/>
        /// </summary>
        public ICommand ApplyTemplateCommand
        {
            get { return applyTemplateCommand; }
        }

        /// <summary>
        /// Command for canceling the view
        /// </summary>
        public ICommand CancelTemplateCommand
        {
            get { return cancelTemplateCommand; }
        }

        /// <summary>
        /// Action for executing the <see cref="ProjectTemplate"/>
        /// </summary>
        public Action<object> ExecuteProjectTemplate { get; set; }

        /// <summary>
        /// Action for canceling the view
        /// </summary>
        public Action CancelProjectTemplate { get; set; }

        public FmModelSettings FmModelSettings { get; } = new FmModelSettings();

        public CollectionView CoordinateSystems
        {
            get { return coordinateSystemsView; }

        }

        public string FilterText
        {
            get { return filterText; }
            set
            {
                filterText = value?.ToLower();
                coordinateSystemsView.Filter = o =>
                    ((ICoordinateSystem) o).Name.ToLower().Contains(filterText) ||
                    ((ICoordinateSystem) o).AuthorityCode.ToString().Contains(filterText);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}