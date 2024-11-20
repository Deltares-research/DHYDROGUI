using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core.Workflow;
using Netron.GraphLib.Attributes;
using Netron.GraphLib.Interfaces;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.ActivityShapes
{
    [NetronGraphShape("Sequential activity shape",
                      NetronLibraryKey,
                      "Activity shapes",
                      "DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.ActivityShapes.SequentialActivityShape",
                      "Sequential activity.")]
    public class SequentialActivityShape : ActivityShapeBase
    {
        /// <summary>
        /// The key in the Netron library under which this shape can be found when the
        /// DeltaShell.Plugins.DelftModels.HydroModel.Gui assembly is added as shape library.
        /// </summary>
        public const string NetronLibraryKey = "0-94u35v-et0-u45-3riot039u5095-32385-v";

        private readonly Dictionary<IActivity, RectangleF> cachedMeasurements;
        private readonly Dictionary<IActivity, ActivityShapeBase> cachedChildShapes;

        /// <summary>
        /// Default constructor, used for instantiations by <see cref="Netron.GraphLib.UI.GraphControl"/>.
        /// </summary>
        /// <Deprecated>Please use <see cref="SequentialActivityShape(IGraphSite)"/> instead.</Deprecated>
        [Obsolete("Used only for Netron.GraphLib.UI.GraphControl")]
        public SequentialActivityShape() { }

        /// <summary>
        /// Creates a shape for a sequential activity for a given <see cref="Netron.GraphLib.Interfaces.IGraphSite"/>.
        /// </summary>
        /// <param name="graphControl">The control hosting this shape.</param>
        public SequentialActivityShape(IGraphSite graphControl)
            : base(graphControl, "Sequential activity")
        {
            cachedMeasurements = new Dictionary<IActivity, RectangleF>();
            cachedChildShapes = new Dictionary<IActivity, ActivityShapeBase>();
        }

        /// <summary>
        /// Creates a shape for a sequential activity for a given <see cref="Netron.GraphLib.Interfaces.IGraphSite"/>.
        /// </summary>
        /// <param name="graphControl">The control hosting this shape.</param>
        /// <param name="shapeText">This parameter will be ignored.</param>
        /// <remarks>Text of this shape will always be <c>Sequential activity</c>.</remarks>
        public SequentialActivityShape(IGraphSite graphControl, string shapeText) : this(graphControl)
        {
            // This shape should always have the Text of "Sequential activity"
        }

        public override IActivity Activity
        {
            get
            {
                return base.Activity;
            }
            set
            {
                var sequentialActivity = value as SequentialActivity;
                if (value != null && sequentialActivity == null)
                {
                    throw new ArgumentException("Value must be a SequentialActivity", nameof(value));
                }

                base.Activity = value;

                CacheChildShapes(sequentialActivity);
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

        private void CacheChildShapes(SequentialActivity sequentialActivity)
        {
            // Clear child shapes:
            foreach (ActivityShapeBase activityShapeBase in cachedChildShapes.Values)
            {
                Site.Shapes.Remove(activityShapeBase);
            }

            cachedChildShapes.Clear();
            if (sequentialActivity == null)
            {
                return;
            }

            // Create and cache child shapes:
            foreach (IActivity activity in sequentialActivity.Activities)
            {
                ActivityShapeBase activityShapeBase = ShapeFactory.CreateShapeFromActivity(activity, Site);
                cachedChildShapes[activity] = activityShapeBase;
                Site.Shapes.Add(activityShapeBase);
                activityShapeBase.ZOrder = ZOrder - 1; // Might seem unintuitive, but is correct.
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

            if (Activity != null)
            {
                var parallelActivitiesOrigin = new PointF(Rectangle.X, Rectangle.Y + requiredSize.Height);
                var parallelActivitiesSize = new Size(0, 0);
                foreach (KeyValuePair<IActivity, ActivityShapeBase> activityAndShape in cachedChildShapes)
                {
                    // Origin to start layout placement from:
                    var origin =
                        new PointF(parallelActivitiesOrigin.X + HorizontalPadding,
                                   parallelActivitiesOrigin.Y + parallelActivitiesSize.Height + VerticalPadding);

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

                    // Increase height due to vertical layout placement:
                    parallelActivitiesSize.Height += (int)Math.Ceiling(activityRectangle.Height + VerticalPadding);

                    // Take largest width of placeholders due to vertical layout placement:
                    var width = (int)Math.Ceiling(activityRectangle.Width);
                    if (width > parallelActivitiesSize.Width)
                    {
                        parallelActivitiesSize.Width = width;
                    }
                }

                // Padding:
                parallelActivitiesSize.Height += VerticalPadding;
                parallelActivitiesSize.Width += HorizontalPadding;

                // Determine final required size:
                if (parallelActivitiesSize.Width > requiredSize.Width)
                {
                    requiredSize.Width = parallelActivitiesSize.Width;
                }

                requiredSize.Height += parallelActivitiesSize.Height;
            }

            return requiredSize;
        }
    }
}