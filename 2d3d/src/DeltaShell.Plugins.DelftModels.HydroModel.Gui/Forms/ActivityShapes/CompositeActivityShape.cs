using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core.Workflow;
using Netron.GraphLib.Attributes;
using Netron.GraphLib.Interfaces;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.ActivityShapes
{
    [NetronGraphShape("Composite activity shape",
                      NetronLibraryKey,
                      "Activity shapes",
                      "DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.ActivityShapes.CompositeActivityShape",
                      "Composite activity.")]
    public class CompositeActivityShape : ActivityShapeBase
    {
        /// <summary>
        /// The key in the Netron library under which this shape can be found when the
        /// DeltaShell.Plugins.DelftModels.HydroModel.Gui assembly is added as shape library.
        /// </summary>
        public const string NetronLibraryKey = "sp90uvt689yu2349pu8y6-90e8ug-ue-5280";

        private readonly Dictionary<IActivity, RectangleF> cachedMeasurements;
        private readonly Dictionary<IActivity, ActivityShapeBase> cachedChildShapes;

        /// <summary>
        /// Default constructor, used for instantiations by <see cref="Netron.GraphLib.UI.GraphControl"/>.
        /// In code, please use <see cref="CompositeActivityShape(IGraphSite,string)"/>
        /// </summary>
        /// <Deprecated>
        /// Please use <see cref="CompositeActivityShape(IGraphSite)"/> or
        /// <see cref="CompositeActivityShape(IGraphSite,string)"/> instead.
        /// </Deprecated>
        [Obsolete("Used only for Netron.GraphLib.UI.GraphControl")]
        public CompositeActivityShape() { }

        /// <summary>
        /// Creates a shape for a composite activity for a given <see cref="Netron.GraphLib.Interfaces.IGraphSite"/>.
        /// </summary>
        /// <param name="graphControl">The control hosting this shape.</param>
        public CompositeActivityShape(IGraphSite graphControl) : this(graphControl, "Composite activity") { }

        /// <summary>
        /// Creates a shape for a composite activity with a given name for a given
        /// <see cref="Netron.GraphLib.Interfaces.IGraphSite"/>.
        /// </summary>
        /// <param name="graphControl">The control hosting this shape.</param>
        /// <param name="shapeText"><see cref="Netron.GraphLib.Entity.Text"/> of this shape.</param>
        public CompositeActivityShape(IGraphSite graphControl, string shapeText) : base(graphControl, shapeText)
        {
            cachedMeasurements = new Dictionary<IActivity, RectangleF>();
            cachedChildShapes = new Dictionary<IActivity, ActivityShapeBase>();
        }

        /// <summary>
        /// The activity associated with this shape.
        /// </summary>
        /// <exception cref="ArgumentException">When value is not <see cref="ICompositeActivity"/>.</exception>
        /// <remarks><see cref="Netron.GraphLib.Entity.Text"/> will match <see cref="ICompositeActivity.Name"/> of activity.</remarks>
        public override IActivity Activity
        {
            get
            {
                return base.Activity;
            }
            set
            {
                var compositeActivity = value as ICompositeActivity;
                if (value != null && compositeActivity == null)
                {
                    throw new ArgumentException("Value must be a ICompositeActivity", nameof(value));
                }

                Text = value != null ? value.Name : "Composite activity";

                base.Activity = value;

                CacheChildShapes(compositeActivity);
            }
        }

        public override void Paint(Graphics g)
        {
            // Measure and apply new size:
            SizeF requiredSize = MeasureSize(g, true);
            Rectangle = new RectangleF(Rectangle.X, Rectangle.Y, requiredSize.Width, requiredSize.Height);

            DrawShape(g, Rectangle);

            // Render nested child activities:
            if (Activity != null)
            {
                foreach (KeyValuePair<IActivity, ActivityShapeBase> activityAndShape in cachedChildShapes)
                {
                    activityAndShape.Value.Rectangle = cachedMeasurements[activityAndShape.Key];
                    activityAndShape.Value.Paint(g);
                }
            }
        }

        internal override SizeF GetRequiredSize(Graphics g)
        {
            return MeasureSize(g, false);
        }

        private void CacheChildShapes(ICompositeActivity compositeActivity)
        {
            // Clear child shapes:
            foreach (ActivityShapeBase activityShapeBase in cachedChildShapes.Values)
            {
                Site.Shapes.Remove(activityShapeBase);
            }

            cachedChildShapes.Clear();
            if (compositeActivity == null)
            {
                return;
            }

            // Create and cache child shapes:
            foreach (IActivity activity in compositeActivity.Activities)
            {
                ActivityShapeBase activityShapeBase = ShapeFactory.CreateShapeFromActivity(activity, Site);
                cachedChildShapes[activity] = activityShapeBase;
                Site.Shapes.Add(activityShapeBase);
                activityShapeBase.ZOrder = ZOrder - 2; // Might seem unintuitive, but is correct.
            }
        }

        private SizeF MeasureSize(Graphics g, bool prepareForPaint)
        {
            if (prepareForPaint)
            {
                cachedMeasurements.Clear();
            }

            // Determine minimum required size for base:
            SizeF requiredSize = base.GetRequiredSize(g);

            // Determine additional size requirements based on child activities:
            if (Activity != null)
            {
                var activitiesOrigin = new PointF(Rectangle.X, Rectangle.Y + requiredSize.Height);
                var activitiesSize = new Size(0, 0);
                foreach (KeyValuePair<IActivity, ActivityShapeBase> activityAndShape in cachedChildShapes)
                {
                    // Origin to start layout placement from:
                    var origin =
                        new PointF(activitiesOrigin.X + activitiesSize.Width + HorizontalPadding,
                                   activitiesOrigin.Y + VerticalPadding);

                    // Determine activity placeholder:
                    SizeF size = activityAndShape.Value.GetRequiredSize(g);
                    var activityRectangle = new RectangleF(
                        origin.X,
                        origin.Y,
                        size.Width + (2 * HorizontalPadding),
                        size.Height + (2 * VerticalPadding));

                    // For painting: Cache placeholder
                    if (prepareForPaint)
                    {
                        cachedMeasurements.Add(activityAndShape.Key, activityRectangle);
                    }

                    // Increase width due to horizontal layout placement:
                    activitiesSize.Width += (int)Math.Ceiling(activityRectangle.Width + HorizontalPadding);

                    // Take largest height of placeholders due to horizontal layout placement:
                    var height = (int)Math.Ceiling(activityRectangle.Height);
                    if (height > activitiesSize.Height)
                    {
                        activitiesSize.Height = height;
                    }
                }

                // Padding:
                activitiesSize.Height += VerticalPadding;
                activitiesSize.Width += HorizontalPadding;

                // Determine final required size:
                if (activitiesSize.Width > requiredSize.Width)
                {
                    requiredSize.Width = activitiesSize.Width;
                }

                requiredSize.Height += activitiesSize.Height;
            }

            return requiredSize;
        }
    }
}