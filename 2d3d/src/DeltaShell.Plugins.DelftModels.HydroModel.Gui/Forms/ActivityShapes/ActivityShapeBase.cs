using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DelftTools.Shell.Core.Workflow;
using Netron.GraphLib;
using Netron.GraphLib.Interfaces;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.ActivityShapes
{
    /// <summary>
    /// Base class for activity shapes.
    /// </summary>
    public abstract class ActivityShapeBase : Shape
    {
        protected const int HorizontalPadding = 5;
        protected const int VerticalPadding = 5;
        private IActivity activity;
        private Color shapeColor = Color.WhiteSmoke;
        private Brush backgroundBrush = new SolidBrush(Color.WhiteSmoke);
        private PointF lastPoint;
        private Cursor cursor;

        /// <summary>
        /// Default constructor, used for instantiations by <see cref="Netron.GraphLib.UI.GraphControl"/>.
        /// </summary>
        /// <Deprecated>
        /// Please use <see cref="ActivityShapeBase(IGraphSite)"/> or
        /// <see cref="ActivityShapeBase(IGraphSite,string)"/> instead.
        /// </Deprecated>
        [Obsolete("Used only for Netron.GraphLib.UI.GraphControl")]
        protected ActivityShapeBase() {}

        /// <summary>
        /// Creates an activity shape for a given <see cref="Netron.GraphLib.Interfaces.IGraphSite"/>.
        /// </summary>
        /// <param name="graphControl">The control hosting this shape.</param>
        protected ActivityShapeBase(IGraphSite graphControl)
            : this(graphControl, "[Not set]") {}

        /// <summary>
        /// Creates an activity with a set text for a given <see cref="Netron.GraphLib.Interfaces.IGraphSite"/>.
        /// </summary>
        /// <param name="graphControl">The control hosting this shape.</param>
        /// <param name="shapeText"><see cref="Netron.GraphLib.Entity.Text"/> of this shape.</param>
        protected ActivityShapeBase(IGraphSite graphControl, string shapeText) : base(graphControl)
        {
            Text = shapeText;
            Font = new Font("Verdana", 8f);
            Rectangle = new RectangleF(0, 0, 1, 1);
        }

        /// <summary>
        /// Background color of this shape.
        /// </summary>
        // Override to remove dependency on Layers of Netron.
        public override Color ShapeColor
        {
            get
            {
                return shapeColor;
            }
            set
            {
                shapeColor = value;
                backgroundBrush = new SolidBrush(shapeColor);
                Invalidate();
            }
        }

        /// <summary>
        /// The activity associated with this shape.
        /// </summary>
        public virtual IActivity Activity
        {
            get
            {
                return activity;
            }
            set
            {
                activity = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Resizes the shape to it's requested size.
        /// </summary>
        public void Inflate()
        {
            // Measure Text:
            SizeF requiredSize = GetRequiredSize(Site.Graphics);

            // Apply new requested size:
            Rectangle = new RectangleF(Rectangle.X, Rectangle.Y, requiredSize.Width, requiredSize.Height);
        }

        public override void Paint(Graphics g)
        {
            // Update sizing:
            Inflate();

            DrawShape(g, Rectangle);
        }

        public override Cursor GetCursor(PointF p)
        {
            if (lastPoint == p)
            {
                return cursor;
            }

            lastPoint = p;
            cursor = base.GetCursor(p);
            return cursor;
        }

        /// <summary>
        /// Background <see cref="Brush"/> definition, color determined by <see cref="ShapeColor"/>.
        /// </summary>
        // Override to remove dependency on Layers of Netron.
        protected override Brush BackgroundBrush
        {
            get
            {
                return backgroundBrush;
            }
        }

        protected void DrawShape(Graphics g, RectangleF rectangle)
        {
            Rectangle rec = System.Drawing.Rectangle.Round(rectangle);

            var linePen = new Pen(Color.Black, 1);
            linePen.EndCap = linePen.StartCap = LineCap.Round;

            var cornerRadius = 20;
            var gfxPath = new GraphicsPath();
            gfxPath.AddArc(rec.X, rec.Y, cornerRadius, cornerRadius, 180, 90);
            gfxPath.AddArc((rec.X + rec.Width) - cornerRadius, rec.Y, cornerRadius, cornerRadius, 270, 90);
            gfxPath.AddArc((rec.X + rec.Width) - cornerRadius, (rec.Y + rec.Height) - cornerRadius, cornerRadius, cornerRadius, 0, 90);
            gfxPath.AddArc(rec.X, (rec.Y + rec.Height) - cornerRadius, cornerRadius, cornerRadius, 90, 90);
            gfxPath.CloseAllFigures();

            var linearGradientBrush = new LinearGradientBrush(rec, ShapeColor, ControlPaint.LightLight(shapeColor), 90F);

            SmoothingMode smoothingMode = g.SmoothingMode;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillPath(linearGradientBrush, gfxPath);
            g.DrawPath(linePen, gfxPath);

            g.DrawString(Text, Font, TextBrush, Rectangle.X + HorizontalPadding, Rectangle.Y + VerticalPadding);

            g.SmoothingMode = smoothingMode;

            linePen.Dispose();
        }

        /// <summary>
        /// Measures the required dimensions to properly draw this shape and it's contents.
        /// </summary>
        /// <param name="g">Graphics reference to where it will be rendered.</param>
        /// <returns>Minimum required size.</returns>
        internal virtual SizeF GetRequiredSize(Graphics g)
        {
            SizeF requiredSize = g.MeasureString(Text, Font);
            requiredSize.Height += 2 * VerticalPadding;
            requiredSize.Width += 2 * HorizontalPadding;
            return requiredSize;
        }
    }
}