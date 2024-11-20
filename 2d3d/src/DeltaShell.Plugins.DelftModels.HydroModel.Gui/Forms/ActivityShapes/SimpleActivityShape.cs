using System;
using DelftTools.Shell.Core.Workflow;
using Netron.GraphLib.Attributes;
using Netron.GraphLib.Interfaces;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.ActivityShapes
{
    [NetronGraphShape("Activity shape",
                      NetronLibraryKey,
                      "Activity shapes",
                      "DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.ActivityShapes.SimpleActivityShape",
                      "An activity.")]
    public class SimpleActivityShape : ActivityShapeBase
    {
        /// <summary>
        /// The key in the Netron library under which this shape can be found when the
        /// DeltaShell.Plugins.DelftModels.HydroModel.Gui assembly is added as shape library.
        /// </summary>
        public const string NetronLibraryKey = "oiwue598u205u-209u59-w-892-395-2452894";

        /// <summary>
        /// Default constructor, used for instantiations by <see cref="Netron.GraphLib.UI.GraphControl"/>.
        /// In code, please use <see cref="SimpleActivityShape(IGraphSite,string)"/>
        /// </summary>
        /// <Deprecated>
        /// Please use <see cref="SimpleActivityShape(IGraphSite)"/> or
        /// <see cref="SimpleActivityShape(IGraphSite,string)"/> instead.
        /// </Deprecated>
        [Obsolete("Used only for Netron.GraphLib.UI.GraphControl")]
        public SimpleActivityShape() {}

        /// <summary>
        /// Creates a basic shape for an <see cref="IActivity"/> for a given <see cref="Netron.GraphLib.Interfaces.IGraphSite"/>.
        /// </summary>
        /// <param name="graphControl">The control hosting this shape.</param>
        public SimpleActivityShape(IGraphSite graphControl)
            : base(graphControl) {}

        /// <summary>
        /// Creates a basic shape with a given name for an <see cref="IActivity"/> for a given
        /// <see cref="Netron.GraphLib.Interfaces.IGraphSite"/>.
        /// </summary>
        /// <param name="graphControl">The control hosting this shape.</param>
        /// <param name="shapeText"><see cref="Netron.GraphLib.Entity.Text"/> of this shape.</param>
        public SimpleActivityShape(IGraphSite graphControl, string shapeText)
            : base(graphControl, shapeText) {}

        /// <summary>
        /// The activity associated with this shape.
        /// </summary>
        /// <remarks><see cref="Netron.GraphLib.Entity.Text"/> will match <see cref="ICompositeActivity.Name"/> of activity.</remarks>
        public override IActivity Activity
        {
            get
            {
                return base.Activity;
            }
            set
            {
                Text = value != null
                           ? value.Name
                           : "[Not set]";
                base.Activity = value;
            }
        }
    }
}