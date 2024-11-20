using System;
using System.Collections.Specialized;
using System.ComponentModel;
using DelftTools.Hydro.CrossSections;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView
{
    
    public class CrossSectionViewModel : INotifyPropertyChanged
    {
        private readonly ICrossSection crossSection;
        

        public CrossSectionViewModel(ICrossSection crossSection)
        {
            this.crossSection = crossSection;
            crossSection.HydroNetwork.SharedCrossSectionDefinitions.CollectionChanged += CrossSectionDefinitionsCollectionChanged;
            ((INotifyPropertyChanged)crossSection.HydroNetwork.SharedCrossSectionDefinitions).PropertyChanged += CrossSectionDefinitionsPropertyChanged;
        }

        private void CrossSectionDefinitionsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (SharedDefinitionsChanged != null)
            {
                SharedDefinitionsChanged(this, EventArgs.Empty);
            }
            FirePropertyChanged(nameof(CanSelectSharedDefinitions));
        }

        void CrossSectionDefinitionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (SharedDefinitionsChanged != null)
            {
                SharedDefinitionsChanged(this, EventArgs.Empty);
            }
            FirePropertyChanged(nameof(CanSelectSharedDefinitions));
        }

        public bool UseSharedDefinition
        {
            get { return crossSection.Definition.IsProxy; }
        }

        public bool UseLocalDefinition
        {
            get { return !crossSection.Definition.IsProxy; }
            set
            {
                //don't change for nothing..
                if (UseLocalDefinition == value)
                {
                    return;
                }

                if (value)
                {
                    crossSection.MakeDefinitionLocal();    
                }
                else
                {
                    var selectedSharedDefinition = crossSection.HydroNetwork.SharedCrossSectionDefinitions[0];
                    crossSection.UseSharedDefinition(selectedSharedDefinition);
                }
                FirePropertyChanged(nameof(UseLocalDefinition));
                FirePropertyChanged(nameof(UseSharedDefinition));
            }
        }
        
        public void SetSharedDefinition(int idx)
        {
            var sharedDefinition = crossSection.HydroNetwork.SharedCrossSectionDefinitions[idx];
            crossSection.UseSharedDefinition(sharedDefinition);
            crossSection.Definition.RefreshGeometry(); /* Refresh the geometry to avoid cached values */
        }

        private void FirePropertyChanged(string propname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propname));
            }
        }

        private CrossSectionDefinitionProxy ProxyDefinition
        {
            get { return crossSection.Definition as CrossSectionDefinitionProxy; }
        }
        
        public double LevelShift
        {
            get
            {
                if (ProxyDefinition != null)
                {
                    return ProxyDefinition.LevelShift;    
                }
                return 0.0;
            }
            set
            {
                
                //should not get here but you never know with binding.
                if (ProxyDefinition != null)
                {
                    ProxyDefinition.LevelShift = value;    
                }
            }
        }
        
        // use this to trigger data binding, this way we don't need to
        // subscribe here to CrossSection PropertyChanged events
        public void FireLevelShiftChanged()
        {
            FirePropertyChanged(nameof(LevelShift));
        }

        public bool CanShareDefinition
        {
            get { return !crossSection.Definition.IsProxy && IsShareableCrossSectionType; }
        }

        public bool IsShareableCrossSectionType
        {
            get { return !crossSection.Definition.GeometryBased; }
        }

        public bool CanSelectSharedDefinitions
        {
            get { return crossSection.HydroNetwork.SharedCrossSectionDefinitions.Count > 0; }
        }

        public ICrossSectionDefinition SharedDefinition
        {
            get
            {
                if (ProxyDefinition != null)
                {
                    return ProxyDefinition.InnerDefinition;
                }
                return null;
            }
        }
        public int SharedDefinitionIndex
        {
            get
            {
                if (ProxyDefinition != null)
                {
                    return crossSection.HydroNetwork.SharedCrossSectionDefinitions.IndexOf(ProxyDefinition.InnerDefinition);    
                }
                return 0;
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;


        
        public void ShareDefinition()
        {
            crossSection.ShareDefinitionAndChangeToProxy();
            FirePropertyChanged(nameof(UseLocalDefinition));
            FirePropertyChanged(nameof(UseSharedDefinition));
        }

        public event EventHandler SharedDefinitionsChanged;

        public void CreateNewSharedCrossSectionDefinition()
        {
            crossSection.MakeDefinitionLocal();
            crossSection.ShareDefinitionAndChangeToProxy();
            crossSection.Definition.RefreshGeometry(); /* Refresh the geometry to avoid cached values */
            FirePropertyChanged(nameof(UseLocalDefinition));
            FirePropertyChanged(nameof(UseSharedDefinition));
        }
    }
}