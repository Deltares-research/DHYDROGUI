using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Threading;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView
{
    /// <summary>
    /// big ugly hack..partially for undo redo. Why ThreadsafeBindingList got involved I have no idea, but I don't want to
    /// worry about it either
    /// </summary>
    [Obsolete("D3DFMIQ-1923 remove cross section")]
    public class SectionsBindingList : ThreadsafeBindingList<CrossSectionSection>
    {
        public SectionsBindingList(SynchronizationContext context)
            : base(context) { }

        public SectionsBindingList(SynchronizationContext context, IList<CrossSectionSection> list)
            : base(context, list) { }

        public SectionsBindingList(IList<CrossSectionSection> list) : base(SynchronizationContext.Current, list) { }

        public Action<CrossSectionSection> BeforeAddItem { get; set; }

        protected override void OnAddingNew(AddingNewEventArgs e)
        {
            base.OnAddingNew(e);
            if (BeforeAddItem != null)
            {
                e.NewObject = new CrossSectionSection();
                BeforeAddItem((CrossSectionSection)e.NewObject);
            }
        }
    }
}