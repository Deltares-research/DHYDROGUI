using System;
using System.Collections.Generic;
using DelftTools.Controls;
using DelftTools.Utils.Data;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms
{
    public class ControlGroupEditorViewContext : Unique<long>, IViewContext
    {
        public ControlGroupEditorViewContext()
        {
            ShapeList = new List<ShapeBase>();
        }

        public virtual ControlGroup ControlGroup { get; set; }

        public virtual bool AutoSize { get; set; }

        /// <summary>
        /// HACK: Control / Paintable is saved into database, DON'T DO IT, save style instead!
        /// </summary>
        public virtual IList<ShapeBase> ShapeList { get; set; }

        public virtual object Data
        {
            get
            {
                return ControlGroup;
            }
        }

        public virtual Type ViewType
        {
            get
            {
                return typeof(ControlGroupGraphView);
            }
        }

        public override string ToString()
        {
            return ControlGroup + " editor settings";
        }
    }
}