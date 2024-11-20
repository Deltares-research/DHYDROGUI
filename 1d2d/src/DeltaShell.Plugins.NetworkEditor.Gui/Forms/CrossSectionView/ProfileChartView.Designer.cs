namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView
{
    partial class ProfileChartView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DelftTools.Controls.Swf.Charting.Chart chart1 = new DelftTools.Controls.Swf.Charting.Chart();
            DelftTools.Utils.TimeNavigatableLabelFormatProvider timeNavigatableLabelFormatProvider1 = new DelftTools.Utils.TimeNavigatableLabelFormatProvider();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProfileChartView));
            this.chartView = new DelftTools.Controls.Swf.Charting.ChartView();
            this.SuspendLayout();
            // 
            // chartView
            // 
            chart1.Name = null;
            this.chartView.Chart = chart1;
            this.chartView.Data = chart1;
            timeNavigatableLabelFormatProvider1.CustomDateTimeFormatInfo = ((System.Globalization.DateTimeFormatInfo)(resources.GetObject("timeNavigatableLabelFormatProvider1.CustomDateTimeFormatInfo")));
            timeNavigatableLabelFormatProvider1.ShowRangeLabel = true;
            timeNavigatableLabelFormatProvider1.ShowUnits = true;
            this.chartView.DateTimeLabelFormatProvider = timeNavigatableLabelFormatProvider1;
            this.chartView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartView.Image = null;
            this.chartView.Location = new System.Drawing.Point(0, 0);
            this.chartView.Name = "chartView";
            this.chartView.AllowPanning = true;
            this.chartView.SelectedPointIndex = -1;
            this.chartView.Size = new System.Drawing.Size(150, 150);
            this.chartView.TabIndex = 0;
            this.chartView.Title = "TeeChart";
            this.chartView.WheelZoom = true;
            // 
            // CrossSectionViewProfileChart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chartView);
            this.Name = "CrossSectionViewProfileChart";
            this.ResumeLayout(false);

        }

        #endregion

        private DelftTools.Controls.Swf.Charting.ChartView chartView;
    }
}
