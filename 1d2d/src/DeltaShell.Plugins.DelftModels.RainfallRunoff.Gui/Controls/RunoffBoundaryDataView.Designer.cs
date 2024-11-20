namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls
{
    partial class RunoffBoundaryDataView
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
            this.rrBoundarySeriesView1 = new DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls.RRBoundarySeriesView();
            this.SuspendLayout();
            // 
            // rrBoundarySeriesView1
            // 
            this.rrBoundarySeriesView1.Data = null;
            this.rrBoundarySeriesView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rrBoundarySeriesView1.Image = null;
            this.rrBoundarySeriesView1.Location = new System.Drawing.Point(0, 0);
            this.rrBoundarySeriesView1.Margin = new System.Windows.Forms.Padding(5);
            this.rrBoundarySeriesView1.Name = "rrBoundarySeriesView1";
            this.rrBoundarySeriesView1.Size = new System.Drawing.Size(1189, 837);
            this.rrBoundarySeriesView1.TabIndex = 0;
            this.rrBoundarySeriesView1.ViewInfo = null;
            // 
            // RunoffBoundaryDataView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.rrBoundarySeriesView1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "RunoffBoundaryDataView";
            this.Size = new System.Drawing.Size(1189, 837);
            this.ResumeLayout(false);
        }

        #endregion

        private DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls.RRBoundarySeriesView rrBoundarySeriesView1;
    }
}
