using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DelftTools.Controls.Wpf.Commands;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Collections;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    public class RefreshMainSectionWidthsDialogViewModel : INotifyPropertyChanged
    {
        private ICollection<CrossSectionViewModel> crossSectionViewModels;
        private IEnumerable<ICrossSection> crossSections;

        public RefreshMainSectionWidthsDialogViewModel()
        {
            FixSelectedCrossSectionsCommand = new RelayCommand(SetSelectedCrossSectionsWidth, o => CrossSectionViewModels?.Any(c => c.Selected)?? false);
            SelectAllCommand = new RelayCommand(o => CrossSectionViewModels.ForEach(c => c.Selected = true), o => CrossSectionViewModels?.Any(c => !c.Selected) ?? false);
            DeSelectAllCommand = new RelayCommand(o => CrossSectionViewModels.ForEach(c => c.Selected = false), o => CrossSectionViewModels?.Any(c => c.Selected) ?? false);
        }

        public ICommand DeSelectAllCommand { get; set; }

        public ICommand SelectAllCommand { get; set; }

        public ICommand FixSelectedCrossSectionsCommand { get; set; }

        public Action AfterFix { get; set; }

        public IEnumerable<ICrossSection> CrossSections
        {
            get { return crossSections; }
            set
            {
                crossSections = value;
                CrossSectionViewModels = crossSections?.Select(c => new CrossSectionViewModel(c)).ToList();
            }
        }

        public ICollection<CrossSectionViewModel> CrossSectionViewModels
        {
            get { return crossSectionViewModels; }
            set
            {
                crossSectionViewModels = value;
                OnPropertyChanged();
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetSelectedCrossSectionsWidth(object o)
        {
            CrossSectionViewModels
                .Where(vm => vm.Selected)
                .Select(vm => vm.Section)
                .ForEach(cs =>
                {
                    var crossSectionDef = cs.Definition as CrossSectionDefinition;
                    if (crossSectionDef != null) crossSectionDef.RefreshSectionsWidths();

                    var crossSectionDefProxy = cs.Definition as CrossSectionDefinitionProxy;
                    if (crossSectionDefProxy == null) return;

                    crossSectionDef = crossSectionDefProxy.InnerDefinition as CrossSectionDefinition;
                    if (crossSectionDef != null) crossSectionDef.RefreshSectionsWidths();
                });

            AfterFix?.Invoke();
        }
    }
}