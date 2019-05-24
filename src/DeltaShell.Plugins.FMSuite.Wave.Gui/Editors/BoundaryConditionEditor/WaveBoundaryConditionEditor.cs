using System;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.BoundaryConditionEditor
{
    /// <summary>
    /// WaveBoundaryConditionEditor wraps generic BoundaryConditionEditor because it has
    /// different view Data: WaveBoundaryCondition and not BoundaryConditionSet
    /// </summary>
    public partial class WaveBoundaryConditionEditor : UserControl, ICompositeView, IReusableView
    {
        public WaveBoundaryConditionEditor()
        {
            InitializeComponent();
        }

        public Common.Gui.Editors.BoundaryConditionEditor BoundaryConditionEditor { get; private set; }

        private WaveBoundaryCondition waveBoundaryCondition;

        public object Data
        {
            get => waveBoundaryCondition;
            set
            {
                waveBoundaryCondition = value as WaveBoundaryCondition;
                if (waveBoundaryCondition != null)
                {
                    BoundaryConditionEditor.Data = new BoundaryConditionSet
                    {
                        Feature = waveBoundaryCondition.Feature,
                        BoundaryConditions = {waveBoundaryCondition}
                    };
                }
                else
                {
                    BoundaryConditionEditor.Data = null;
                }
            }
        }

        public Image Image
        {
            get => BoundaryConditionEditor.Image;
            set => BoundaryConditionEditor.Image = value;
        }

        public void EnsureVisible(object item)
        {
            BoundaryConditionEditor.EnsureVisible(item);
        }

        public ViewInfo ViewInfo { get; set; }

        public bool Locked
        {
            get => BoundaryConditionEditor.Locked;
            set => BoundaryConditionEditor.Locked = value;
        }

        public event EventHandler LockedChanged
        {
            add => BoundaryConditionEditor.LockedChanged += value;
            remove => BoundaryConditionEditor.LockedChanged -= value;
        }

        public IEventedList<IView> ChildViews => BoundaryConditionEditor.ChildViews;

        public bool HandlesChildViews => true;

        public void ActivateChildView(IView childView) {}
    }
}