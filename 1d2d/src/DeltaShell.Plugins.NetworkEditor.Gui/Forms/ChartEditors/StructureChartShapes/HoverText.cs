using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Controls.Swf.Charting;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes
{
    public class HoverText : IHoverText
    {
        public HoverText(string line1, string line2, IShapeFeature shapeFeature, Color color, HoverPosition hoverPosition, ArrowHeadPosition arrowHeadPosition)
        {
            ShowLine = true;
            BackColor = Color.Transparent;
            Line1 = line1;
            Line2 = line2;
            ShapeFeature = shapeFeature;
            ForeColor = color;
            HoverPosition = hoverPosition;
            ArrowHeadPosition = arrowHeadPosition;
        }

        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public HoverType HoverType { get; set; }

        /// <summary>
        /// Color used to draw text and lines
        /// </summary>
        public Color ForeColor { get; set; }

        /// <summary>
        /// Color used to fill space behind text
        /// </summary>
        public Color BackColor { get; set; }

        public IShapeFeature ShapeFeature { get; set; }
        public HoverPosition HoverPosition { get; set; }
        public ArrowHeadPosition ArrowHeadPosition { get; set; }
        public bool ShowLine { get; set; }

        public void Render(List<Rectangle> usedSpace, IChart chart, Graphics graphics)
        {
            SolidBrush brush = new SolidBrush(ForeColor);
            Pen pen = new Pen(ForeColor, 1);
            switch (HoverPosition)
            {
                case HoverPosition.Left:
                    DrawLeft(graphics, pen, brush, usedSpace, chart);
                    break;
                case HoverPosition.Top:
                    DrawTop(graphics, pen, brush, usedSpace, chart);
                    break;
                case HoverPosition.Right:
                    DrawRight(graphics, pen, brush, usedSpace, chart);
                    break;
                case HoverPosition.Bottom:
                    DrawBottom(graphics, pen, brush, usedSpace, chart);
                    break;
            }
            pen.Dispose();
            brush.Dispose();
        }

        private static void DrawArrowHead(Graphics g, Pen pen, int x, int y, ArrowHeadPosition arrowHeadPosition)
        {
            const int arrowLength = 4;
            const int arrowWidth = 2;
            const int interoffset = 1;

            switch (arrowHeadPosition)
            {
                case ArrowHeadPosition.Left:
                    g.DrawLines(pen, new []
                                         {
                                             new Point(x + arrowLength, y - arrowWidth), 
                                             new Point(x + interoffset, y),
                                             new Point(x + arrowLength, y + arrowWidth),
                                             new Point(x + arrowLength, y - arrowWidth)
                                         });
                    break;
                case ArrowHeadPosition.Top:
                    g.DrawLines(pen, new[]
                                         {
                                             new Point(x - arrowWidth, y + arrowLength), 
                                             new Point(x, y + interoffset),
                                             new Point(x + arrowWidth, y + arrowLength),
                                             new Point(x - arrowWidth, y + arrowLength)
                                         });
                    break;
                case ArrowHeadPosition.Right:
                    g.DrawLines(pen, new[]
                                         {
                                             new Point(x - arrowLength, y - arrowWidth), 
                                             new Point(x - interoffset, y),
                                             new Point(x - arrowLength, y + arrowWidth),
                                             new Point(x - arrowLength, y - arrowWidth)
                                         });
                    break;
                case ArrowHeadPosition.Down:
                    g.DrawLines(pen, new[]
                                         {
                                             new Point(x - arrowWidth, y - arrowLength), 
                                             new Point(x, y - interoffset),
                                             new Point(x + arrowWidth, y - arrowLength),
                                             new Point(x - arrowWidth, y - arrowLength)
                                         });
                    break;
            }
        }


        private void DrawTop(Graphics graphics, Pen pen, Brush brush, List<Rectangle> usedSpace, IChart chart)
        {
            var g = chart.Graphics;
            var bounds = ShapeFeature.GetBounds();
            var y = Math.Max(ChartCoordinateService.ToDeviceY(chart, chart.LeftAxis.Maximum), bounds.Top);

            using (var font = (Font)g.Font.Clone())
            {
                int from = bounds.Left;
                int to = bounds.Right;

                ArrowHeadPosition arrowHeadPosition;

                CalculateHorizontalMargin(ref from, ref to, (int)graphics.MeasureString("O", font).Height / 2, out arrowHeadPosition);
                bool cancel = false;
                CenterTextHorizontal(usedSpace, graphics, font, brush, Line1, bounds, ref y, ref cancel);
                if (!cancel)
                {
                    CenterTextHorizontal(usedSpace, graphics, font, brush, Line2, bounds, ref y, ref cancel);
                    if (ShowLine)
                    {
                        DrawHorizontalLine(graphics, pen, to, from, arrowHeadPosition, y);
                    }
                }
            }
        }

        private void DrawBottom(Graphics graphics, Pen pen, Brush brush, List<Rectangle> usedSpace, IChart chart)
        {
            var g = chart.Graphics;
            var bounds = ShapeFeature.GetBounds();
            var y = ChartCoordinateService.ToDeviceY(chart, chart.LeftAxis.Minimum);

            using (var font = (Font)g.Font.Clone())
            {
                int margin = (int)graphics.MeasureString("O", font).Height;

                int from = bounds.Left;
                int to = bounds.Right;
                ArrowHeadPosition arrowHeadPosition;

                CalculateHorizontalMargin(ref from, ref to, margin, out arrowHeadPosition);

                if (ShowLine)
                {
                    y -= 2*margin;
                }
                bool cancel = false;
                CenterTextHorizontal(usedSpace, graphics, font, brush, Line2, bounds, ref y, ref cancel);
                if (!cancel)
                {
                    CenterTextHorizontal(usedSpace, graphics, font, brush, Line1, bounds, ref y, ref cancel);
                    if (ShowLine)
                    {
                        DrawHorizontalLine(graphics, pen, to, from, arrowHeadPosition, y);
                    }
                }
            }
        }

        private void DrawLeft(Graphics graphics, Pen pen, Brush brush, List<Rectangle> usedSpace, IChart chart)
        {
            var g = chart.Graphics;
            var bounds = ShapeFeature.GetBounds();
            var x = bounds.Left + 5;

            using (var font = (Font)g.Font.Clone())
            {
                var bottom = ChartCoordinateService.ToDeviceY(chart, chart.LeftAxis.Minimum);
                if (bottom > bounds.Top)
                {
                    int from = bounds.Top;
                    int to = bounds.Bottom;
                    int y;
                    ArrowHeadPosition arrowHeadPosition;

                    CalculateVerticalMargin(ref from, ref to, bounds, (int)graphics.MeasureString(Line1, font).Height / 2, out y, out arrowHeadPosition);

                    bool cancel = false;
                    LeftText(usedSpace, graphics, font, brush, Line1, bounds, ref y, ref cancel);

                    if (!cancel)
                    {
                        LeftText(usedSpace, graphics, font, brush, Line2, bounds, ref y, ref cancel);
                        if (ShowLine)
                        {
                            DrawVerticalLine(graphics, pen, from, to, arrowHeadPosition, x);
                        }
                    }
                }
            }
        }

        private void DrawRight(Graphics graphics, Pen pen, Brush brush, List<Rectangle> usedSpace, IChart chart)
        {
            var g = chart.Graphics;
            var bounds = ShapeFeature.GetBounds();
            var x = bounds.Left + 5;

            using (var font = (Font)g.Font.Clone())
            {
                var bottom = ChartCoordinateService.ToDeviceY(chart, chart.LeftAxis.Minimum);
                if (bottom > bounds.Top)
                {
                    int from = bounds.Top;
                    int to = bounds.Bottom;
                    int y;
                    ArrowHeadPosition arrowHeadPosition;

                    CalculateVerticalMargin(ref from, ref to, bounds, (int)graphics.MeasureString(Line1, font).Height / 2, out y, out arrowHeadPosition);

                    bool cancel = false;
                    RightText(usedSpace, graphics, font, brush, Line1, bounds, ref y, ref cancel);

                    if (!cancel)
                    {
                        RightText(usedSpace, graphics, font, brush, Line2, bounds, ref y, ref cancel);
                        if (ShowLine)
                        {
                            DrawVerticalLine(graphics, pen, from, to, arrowHeadPosition, x);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the dimensions of the horizontal line given a margin. If there is no room for the defined arrowhead 
        /// it is removed
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="margin"></param>
        /// <param name="arrowHeadPosition"></param>
        private void CalculateHorizontalMargin(ref int from, ref int to, int margin, out ArrowHeadPosition arrowHeadPosition)
        {
            arrowHeadPosition = ArrowHeadPosition;
            if (to - from > 10 + (2 * margin))
            {
                if ((ArrowHeadPosition == ArrowHeadPosition.Left) || (ArrowHeadPosition == ArrowHeadPosition.LeftRight))
                {
                    from += margin;
                }
                if ((ArrowHeadPosition == ArrowHeadPosition.Right) || (ArrowHeadPosition == ArrowHeadPosition.LeftRight))
                {
                    to -= margin;
                }
            }
            else
            {
                arrowHeadPosition = ArrowHeadPosition.None;
            }
        }


        /// <summary>
        /// Calculates the dimensions of the vertical line given a margin. If there is no room for the defined arrowhead 
        /// it is removed
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="bounds"></param>
        /// <param name="margin"></param>
        /// <param name="y"></param>
        /// <param name="arrowHeadPosition"></param>
        private void CalculateVerticalMargin(ref int from, ref int to, Rectangle bounds, int margin, out int y, out ArrowHeadPosition arrowHeadPosition)
        {
            arrowHeadPosition = ArrowHeadPosition;
            if (to - from > 10 + (2 * margin))
            {
                if ((ArrowHeadPosition == ArrowHeadPosition.Top) || (ArrowHeadPosition == ArrowHeadPosition.TopDown))
                {
                    from += margin;
                }
                if ((ArrowHeadPosition == ArrowHeadPosition.Down) || (ArrowHeadPosition == ArrowHeadPosition.TopDown))
                {
                    to -= margin;
                }
            }
            else
            {
                arrowHeadPosition = ArrowHeadPosition.None;
            }
            y = (bounds.Top + ((bounds.Height)) / 2 - margin);
        }

        private static void DrawHorizontalLine(Graphics g, Pen pen, int to, int from, ArrowHeadPosition arrowHeadPosition, int y)
        {
            if ((arrowHeadPosition == ArrowHeadPosition.Left) || (arrowHeadPosition == ArrowHeadPosition.LeftRight))
            {
                DrawArrowHead(g, pen, from, y, ArrowHeadPosition.Left);
            }
            if ((arrowHeadPosition == ArrowHeadPosition.Right) || (arrowHeadPosition == ArrowHeadPosition.LeftRight))
            {
                DrawArrowHead(g, pen, to, y, ArrowHeadPosition.Right);
            }
            g.DrawLine(pen, from, y, to, y);
        }

        private static void DrawVerticalLine(Graphics g, Pen pen, int from, int to, ArrowHeadPosition arrowHeadPosition, int x)
        {
            if ((arrowHeadPosition == ArrowHeadPosition.Top) || (arrowHeadPosition == ArrowHeadPosition.TopDown))
            {
                DrawArrowHead(g, pen, x, from, ArrowHeadPosition.Top);
            }
            if ((arrowHeadPosition == ArrowHeadPosition.Down) || (arrowHeadPosition == ArrowHeadPosition.TopDown))
            {
                DrawArrowHead(g, pen, x, to, ArrowHeadPosition.Down);
            }
            g.DrawLine(pen, x, from, x, to);
        }

        /// <summary>
        /// Draws a left aligned text if it exists
        /// </summary>
        /// <param name="usedSpace"></param>
        /// <param name="g"></param>
        /// <param name="font"></param>
        /// <param name="brush"></param>
        /// <param name="text"></param>
        /// <param name="bounds"></param>
        /// <param name="y"></param>
        /// <param name="cancel"></param>
        private void LeftText(ICollection<Rectangle> usedSpace, Graphics g, Font font, Brush brush, string text, Rectangle bounds, ref int y, ref bool cancel)
        {
            if ((text == null) || (text.Trim() == ""))
            {
                return;
            }
            int x = bounds.Left + 10;
            var size = g.MeasureString(text, font);
            DrawTextInFreeSpace(text, g, font, brush, x, ref y, size, usedSpace, ref cancel);
        }

        /// <summary>
        /// Draws a left aligned text if it exists
        /// </summary>
        /// <param name="usedSpace"></param>
        /// <param name="g"></param>
        /// <param name="font"></param>
        /// <param name="brush"></param>
        /// <param name="text"></param>
        /// <param name="bounds"></param>
        /// <param name="y"></param>
        /// <param name="cancel"></param>
        private void RightText(ICollection<Rectangle> usedSpace, Graphics g, Font font, Brush brush, string text, Rectangle bounds, ref int y, ref bool cancel)
        {
            if ((text == null) || (text.Trim() == ""))
            {
                return;
            }
            var size = g.MeasureString(text, font);
            int x = (int) (bounds.Left - 10 - size.Width);
            DrawTextInFreeSpace(text, g, font, brush, x, ref y, size, usedSpace, ref cancel);
        }


        private void CenterTextHorizontal(ICollection<Rectangle> usedSpace, Graphics g, Font font, Brush brush, string text, Rectangle bounds, ref int y, ref bool cancel)
        {
            if ((text == null) || (text.Trim() == ""))
            {
                return;
            }
            var size = g.MeasureString(text, font);
            int x = (int)(bounds.Left + (bounds.Width - size.Width) / 2);
            DrawTextInFreeSpace(text, g, font, brush, x, ref y, size, usedSpace, ref cancel);
        }

        private void DrawTextInFreeSpace(string text, Graphics g, Font font, Brush brush, int x, ref int y, SizeF size, ICollection<Rectangle> usedSpace, ref bool cancel)
        {
            var rectangle = new Rectangle(x, y, (int)size.Width, (int)size.Height);
            foreach (Rectangle space in usedSpace)
            {
                if (!space.IntersectsWith(rectangle))
                {
                    continue;
                }
                cancel = true;
                return;
            }
            usedSpace.Add(rectangle);
            if (BackColor != Color.Transparent)
            {
                SolidBrush solidBrush = new SolidBrush(BackColor);
                g.FillRectangle(solidBrush, rectangle);
                g.DrawRectangle(Pens.Black, rectangle);
                solidBrush.Dispose();

            }
            g.DrawString(text, font, brush, x, y);
            y += (int)size.Height;
        }

    }
}