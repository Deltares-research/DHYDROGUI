namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    partial class HeatFluxModelView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HeatFluxModelView));
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.radiationCheckBox = new System.Windows.Forms.CheckBox();
            this.tabbedMultipleFunctionView = new DeltaShell.Plugins.CommonTools.Gui.Forms.Functions.TabbedMultipleFunctionView();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.radiationCheckBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(697, 25);
            this.panel1.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.tabbedMultipleFunctionView);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 25);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(697, 351);
            this.panel2.TabIndex = 1;
            // 
            // radiationCheckBox
            // 
            this.radiationCheckBox.AutoSize = true;
            this.radiationCheckBox.Location = new System.Drawing.Point(8, 5);
            this.radiationCheckBox.Name = "radiationCheckBox";
            this.radiationCheckBox.Size = new System.Drawing.Size(129, 17);
            this.radiationCheckBox.TabIndex = 1;
            this.radiationCheckBox.Text = "Specify solar radiation";
            this.radiationCheckBox.UseVisualStyleBackColor = true;
            // 
            // tabbedMultipleFunctionView1
            // 
            this.tabbedMultipleFunctionView.ChartSeriesType = DelftTools.Controls.Swf.Charting.Series.ChartSeriesType.LineSeries;
            this.tabbedMultipleFunctionView.Data = null;
            this.tabbedMultipleFunctionView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabbedMultipleFunctionView.Image = ((System.Drawing.Image)(resources.GetObject("tabbedMultipleFunctionView1.Image")));
            this.tabbedMultipleFunctionView.Location = new System.Drawing.Point(0, 0);
            this.tabbedMultipleFunctionView.Name = "tabbedMultipleFunctionView";
            this.tabbedMultipleFunctionView.Size = new System.Drawing.Size(697, 351);
            this.tabbedMultipleFunctionView.SplitIntoTabs = false;
            this.tabbedMultipleFunctionView.TabIndex = 0;
            this.tabbedMultipleFunctionView.ViewInfo = null;
            // 
            // HeatFluxModelView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "HeatFluxModelView";
            this.Size = new System.Drawing.Size(697, 376);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox radiationCheckBox;
        private System.Windows.Forms.Panel panel2;
        private CommonTools.Gui.Forms.Functions.TabbedMultipleFunctionView tabbedMultipleFunctionView;
    }
}
