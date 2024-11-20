using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using DelftTools.Hydro.CrossSections;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    public class CrossSectionViewModel : INotifyPropertyChanged
    {
        private readonly ICrossSection section;
        private bool selected;

        public CrossSectionViewModel(ICrossSection section)
        {
            this.section = section;
            Selected = true;
        }

        public bool Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get { return section.Name; }
        }

        public string BranchName
        {
            get { return section.Branch.ToString(); }
        }

        public double Chainage
        {
            get { return section.Chainage; }
        }

        public CrossSectionType CrossSectionType
        {
            get { return section.CrossSectionType; }
        }
        public Image CrossSectionTypeImage
        {
            get { return CrossSectionNodePresenterIconHelper.GetIcon(CrossSectionType); }
        }

        public ICrossSection Section
        {
            get { return section; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}