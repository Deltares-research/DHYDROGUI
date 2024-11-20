using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Data;
using Netron.GraphLib;
using Netron.GraphLib.UI;
using TypeConverter = System.ComponentModel.TypeConverter;

namespace DeltaShell.Plugins.DelftModels.RTCShapes.Shapes
{
    public abstract class ShapeBase : Shape, IUnique<long>
    {
        protected Connector LeftNode;
        protected Connector RightNode;

        protected Connector TopNode;
        protected Connector BottomNode;
        private PointF lastPoint;
        private Cursor cursor;

        private StringAlignment stringAlignment;

        protected ShapeBase()
        {
            InitializeFont();
            Initialize();
            stringAlignment = StringAlignment.Center;
        }

        /// <summary>
        /// Automatically resize the shape to fit the Title
        /// </summary>
        public bool AutoResize
        {
            get
            {
                return RecalculateSize;
            }
            set
            {
                RecalculateSize = value;
            }
        }

        /// <summary>
        /// Title of the shape (text drawn on the shape)
        /// </summary>
        public string Title { get; set; }

        public new object Tag
        {
            get
            {
                return base.Tag;
            }
            set
            {
                if (base.Tag != null)
                {
                    ((INotifyPropertyChanged) base.Tag).PropertyChanged -= TagPropertyChanged;
                }

                base.Tag = value;
                if (base.Tag != null)
                {
                    ((INotifyPropertyChanged) base.Tag).PropertyChanged += TagPropertyChanged;
                    TagPropertyChanged(this, null);
                }
            }
        }

        public IEnumerable<Connector> HighLightedConnectors { get; set; }

        public virtual long Id { get; set; }

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

        public override void AddProperties()
        {
            base.AddProperties();
            //replace the default text editing with something more extended for a label
            Bag.Properties.Remove("Text");
            Bag.Properties.Add(new PropertySpec("Text", typeof(string), "Appearance", "The text attached to the entity",
                                                "[Not Set]", typeof(TextUIEditor), typeof(TypeConverter)));
            Bag.Properties.Add(new PropertySpec("Alignment", typeof(StringAlignment), "Graph",
                                                "Gets or sets the string alignment.", StringAlignment.Near));
        }

        public override PointF ConnectionPoint(Connector c)
        {
            if (c == TopNode)
            {
                return new PointF(Rectangle.Left + (Rectangle.Width / 2), Rectangle.Top);
            }

            if (c == BottomNode)
            {
                return new PointF(Rectangle.Left + (Rectangle.Width / 2), Rectangle.Bottom);
            }

            if (c == LeftNode)
            {
                return new PointF(Rectangle.Left, Rectangle.Top + (Rectangle.Height / 2));
            }

            if (c == RightNode)
            {
                return new PointF(Rectangle.Right, Rectangle.Top + (Rectangle.Height / 2));
            }

            return new PointF(0, 0);
        }

        public virtual Type GetEntityType()
        {
            return GetType();
        }

        protected abstract void Initialize();

        protected virtual void Recalculate(Graphics g)
        {
            if (!RecalculateSize)
            {
                return;
            }

            SizeF s = g.MeasureString(Title, Font);
            Rectangle = new RectangleF(Rectangle.X, Rectangle.Y, s.Width + 5, Math.Max(s.Height + 10, Rectangle.Height));
        }

        protected void PreRender(Graphics g)
        {
            if (!ShowLabel)
            {
                return;
            }

            PointF origin = Point.Empty;

            switch (stringAlignment)
            {
                case StringAlignment.Center:
                    origin = new PointF(Rectangle.X + (Rectangle.Width / 2), Rectangle.Y + (Rectangle.Height / 2));
                    break;
                case StringAlignment.Far:
                    origin = new PointF((Rectangle.X + Rectangle.Width) - 1, Rectangle.Y + 3);
                    break;
                case StringAlignment.Near:
                    origin = new PointF(Rectangle.X + 1, Rectangle.Y + 3);
                    break;
            }

            RenderText(Title, origin, g, Font.Size);
            if (HighLightedConnectors != null)
            {
                DrawConnectorsHighlights(HighLightedConnectors, g);
            }
        }

        protected void RenderText(string text, PointF origin, Graphics g, float fontSize)
        {
            PointF newOrigin = origin;

            if (stringAlignment == StringAlignment.Center)
            {
                var fontToMeasure = new Font(Font.FontFamily, fontSize, Font.Style);
                SizeF stringSize = g.MeasureString(text, fontToMeasure);
                newOrigin = new PointF(origin.X - (stringSize.Width / 2), origin.Y - (stringSize.Height / 2));
            }

            g.DrawString(text, Font, TextBrush, newOrigin.X, newOrigin.Y);
        }

        protected void DrawConnectorsHighlights(IEnumerable<Connector> connectors, Graphics g)
        {
            if (connectors != null)
            {
                var linePen = new Pen(Color.Red, 5);

                var rectangles = new RectangleF[connectors.AsList().Count];
                var countRectangles = 0;
                foreach (Connector connector in connectors)
                {
                    var rectangle = new RectangleF(connector.Location.X, connector.Location.Y, 2,
                                                   2);
                    rectangles[countRectangles] = rectangle;
                    countRectangles++;
                }

                g.DrawRectangles(linePen, rectangles);
            }
        }

        protected override void GetPropertyBagValue(object sender, PropertySpecEventArgs e)
        {
            base.GetPropertyBagValue(sender, e);
            switch (e.Property.Name)
            {
                case "Alignment":
                    e.Value = stringAlignment;
                    break;
            }
        }

        protected override void SetPropertyBagValue(object sender, PropertySpecEventArgs e)
        {
            base.SetPropertyBagValue(sender, e);
            switch (e.Property.Name)
            {
                case "Alignment":
                    stringAlignment = (StringAlignment) e.Value;
                    Invalidate();
                    break;
            }
        }

        protected virtual void UpdateColor(bool isLinked) {}

        private void InitializeFont()
        {
            Font = new Font("Verdana", 10f);
        }

        private void TagPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Tag is INameable nameable)
            {
                Title = nameable.Name;
            }

            //Don't want a reference to RTCObjects. We could also implement IDisplayName
            Type type = Tag.GetType();
            PropertyInfo property = type.GetProperty("LongName");
            if (property != null)
            {
                Text = (string) property.GetValue(Tag, null);
            }

            UpdateColor(Title != "[Not Set]");

            Invalidate();
        }
    }
}