namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CompositeStructureView
{
    partial class StructureView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DelftTools.Controls.Swf.Charting.Chart chart1 = new DelftTools.Controls.Swf.Charting.Chart();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StructureView));
            this.chartView = new DelftTools.Controls.Swf.Charting.ChartView();
            this.SuspendLayout();
            // 
            // chartView
            // 
            this.chartView.Chart = chart1;
            this.chartView.Data = chart1;
            this.chartView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartView.Image = null;
            this.chartView.Location = new System.Drawing.Point(0, 0);
            this.chartView.Name = "chartView";
            this.chartView.AllowPanning = true;
            this.chartView.SelectedPointIndex = -1;
            this.chartView.Size = new System.Drawing.Size(601, 571);
            this.chartView.TabIndex = 0;
            this.chartView.WheelZoom = true;
            // 
            // StructureView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chartView);
            this.Name = "StructureView";
            this.Size = new System.Drawing.Size(601, 571);
            this.ResumeLayout(false);

        }

        #endregion

        private DelftTools.Controls.Swf.Charting.ChartView chartView;

    }
}