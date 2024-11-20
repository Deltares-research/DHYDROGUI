using DelftTools.Hydro.CrossSections;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.ProfileMutators
{
    public class ProxyProfileMutator : ICrossSectionProfileMutator
    {
        private readonly CrossSectionDefinitionProxy crossSectionDefinition;
        private readonly ICrossSectionProfileMutator innerMutator;

        public ProxyProfileMutator(CrossSectionDefinitionProxy crossSectionDefinition,ICrossSectionProfileMutator innerMutator)
        {
            this.crossSectionDefinition = crossSectionDefinition;
            this.innerMutator = innerMutator;
        }

        public void MovePoint(int index, double y, double z)
        {
            innerMutator.MovePoint(index, y, z);
        }

        public void AddPoint(double y, double z)
        {
            innerMutator.AddPoint(y, z);
        }

        public void DeletePoint(int index)
        {
            innerMutator.DeletePoint(index);
        }

        public bool CanDelete
        {
            get { return innerMutator.CanDelete; }
        }

        public bool CanAdd
        {
            get { return innerMutator.CanAdd; }
        }

        public bool CanMove
        {
            get { return innerMutator.CanMove; }
        }

        public bool ClipHorizontal
        {
            get { return innerMutator.ClipHorizontal; }
        }

        public bool ClipVertical
        {
            get { return innerMutator.ClipVertical; }
        }

        public bool FixHorizontal
        {
            get { return innerMutator.FixHorizontal; }
        }

        public bool FixVertical
        {
            get { return innerMutator.FixVertical; }
        }

        
        
    }
}