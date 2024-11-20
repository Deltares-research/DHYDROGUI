using DelftTools.Controls.Swf.Charting;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView
{
    partial class NetworkSideView
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
            DelftTools.Utils.TimeNavigatableLabelFormatProvider timeNavigatableLabelFormatProvider1 = new DelftTools.Utils.TimeNavigatableLabelFormatProvider();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetworkSideView));
            this.chartView = new DelftTools.Controls.Swf.Charting.ChartView();
            this.showCrossSections = new System.Windows.Forms.CheckBox();
            this.optionsPanel = new System.Windows.Forms.Panel();
            this.showStructures = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.optionsPanel.SuspendLayout();
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
            this.chartView.Size = new System.Drawing.Size(465, 349);
            this.chartView.TabIndex = 0;
            this.chartView.Title = "TeeChart";
            this.chartView.WheelZoom = true;
            // 
            // showCrossSections
            // 
            this.showCrossSections.AutoSize = true;
            this.showCrossSections.Dock = System.Windows.Forms.DockStyle.Left;
            this.showCrossSections.Location = new System.Drawing.Point(111, 0);
            this.showCrossSections.Name = "showCrossSections";
            this.showCrossSections.Size = new System.Drawing.Size(94, 19);
            this.showCrossSections.TabIndex = 1;
            this.showCrossSections.Text = "Cross sections";
            this.showCrossSections.UseVisualStyleBackColor = true;
            this.showCrossSections.CheckedChanged += new System.EventHandler(this.ShowCrossSectionsCheckedChanged);
            // 
            // optionsPanel
            // 
            this.optionsPanel.Controls.Add(this.showCrossSections);
            this.optionsPanel.Controls.Add(this.showStructures);
            this.optionsPanel.Controls.Add(this.label1);
            this.optionsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.optionsPanel.Location = new System.Drawing.Point(0, 349);
            this.optionsPanel.Name = "optionsPanel";
            this.optionsPanel.Size = new System.Drawing.Size(465, 19);
            this.optionsPanel.TabIndex = 2;
            // 
            // showStructures
            // 
            this.showStructures.AutoSize = true;
            this.showStructures.Checked = true;
            this.showStructures.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showStructures.Dock = System.Windows.Forms.DockStyle.Left;
            this.showStructures.Location = new System.Drawing.Point(37, 0);
            this.showStructures.Name = "showStructures";
            this.showStructures.Size = new System.Drawing.Size(74, 19);
            this.showStructures.TabIndex = 2;
            this.showStructures.Text = "Structures";
            this.showStructures.UseVisualStyleBackColor = true;
            this.showStructures.CheckedChanged += new System.EventHandler(this.ShowStructuresCheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Show:";
            // 
            // NetworkSideView
            // 
            this.Controls.Add(this.chartView);
            this.Controls.Add(this.optionsPanel);
            this.Name = "NetworkSideView";
            this.Size = new System.Drawing.Size(465, 368);
            this.optionsPanel.ResumeLayout(false);
            this.optionsPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private ChartView chartView;
        private System.Windows.Forms.CheckBox showCrossSections;
        private System.Windows.Forms.Panel optionsPanel;
        private System.Windows.Forms.CheckBox showStructures;
        private System.Windows.Forms.Label label1;
    }
}