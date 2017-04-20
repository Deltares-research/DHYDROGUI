using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Polder;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.Polder
{
    public partial class PolderPieChart : UserControl
    {
        private PolderConcept data;
        private int margin = 5;

        public PolderPieChart()
        {
            InitializeComponent();
            DoubleBuffered = true;
        }

        public PolderConcept Data
        {
            get { return data; }
            set
            {
                if (data != null)
                {
                    ((INotifyPropertyChanged) data).PropertyChanged -= PolderPropertyChanged;
                }
                data = value;
                if (data != null)
                {
                    ((INotifyPropertyChanged) data).PropertyChanged += PolderPropertyChanged;
                }
            }
        }

        public double TotalArea { get; set; }

        private void PolderPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Data == null)
                return;

            double usedArea = Data.PavedArea + Data.UnpavedArea + Data.OpenWaterArea + Data.GreenhouseArea;
            double maxArea = Math.Max(TotalArea, usedArea);

            if (maxArea <= 0)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            float startAngle = -90f; //start at top

            FillPie(e.Graphics, Brushes.Red, (float) (Data.PavedArea/maxArea), ref startAngle);
            FillPie(e.Graphics, Brushes.Green, (float) (Data.UnpavedArea/maxArea), ref startAngle);
            FillPie(e.Graphics, Brushes.Goldenrod, (float) (Data.GreenhouseArea/maxArea), ref startAngle);
            FillPie(e.Graphics, Brushes.RoyalBlue, (float) (Data.OpenWaterArea/maxArea), ref startAngle);
            FillPie(e.Graphics, SystemBrushes.ControlLight, (float) ((maxArea - usedArea)/maxArea), ref startAngle);
        }

        private void FillPie(Graphics g, Brush brush, float percentage, ref float startAngle)
        {
            float angle = (360*percentage);
            g.FillPie(brush, margin, margin, Width - 2*margin, Height - 2*margin, startAngle, angle);
            startAngle += angle;
        }
    }
}