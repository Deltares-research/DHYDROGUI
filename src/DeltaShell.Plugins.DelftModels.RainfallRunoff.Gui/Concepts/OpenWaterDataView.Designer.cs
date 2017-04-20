namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    partial class OpenWaterDataView
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.meteoTab = new System.Windows.Forms.TabPage();
            this.catchmentMeteoStationSelection1 = new DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.CatchmentMeteoStationSelection();
            this.tabControl1.SuspendLayout();
            this.meteoTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.meteoTab);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(675, 150);
            this.tabControl1.TabIndex = 1;
            // 
            // meteoTab
            // 
            this.meteoTab.Controls.Add(this.catchmentMeteoStationSelection1);
            this.meteoTab.Location = new System.Drawing.Point(4, 22);
            this.meteoTab.Name = "meteoTab";
            this.meteoTab.Padding = new System.Windows.Forms.Padding(3);
            this.meteoTab.Size = new System.Drawing.Size(667, 124);
            this.meteoTab.TabIndex = 0;
            this.meteoTab.Text = "Meteo";
            this.meteoTab.UseVisualStyleBackColor = true;
            // 
            // catchmentMeteoStationSelection1
            // 
            this.catchmentMeteoStationSelection1.CatchmentModelData = null;
            this.catchmentMeteoStationSelection1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.catchmentMeteoStationSelection1.Location = new System.Drawing.Point(3, 3);
            this.catchmentMeteoStationSelection1.MeteoStations = null;
            this.catchmentMeteoStationSelection1.Name = "catchmentMeteoStationSelection1";
            this.catchmentMeteoStationSelection1.Size = new System.Drawing.Size(661, 118);
            this.catchmentMeteoStationSelection1.TabIndex = 0;
            this.catchmentMeteoStationSelection1.UseMeteoStations = false;
            // 
            // OpenWaterDataView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Name = "OpenWaterDataView";
            this.Size = new System.Drawing.Size(675, 150);
            this.tabControl1.ResumeLayout(false);
            this.meteoTab.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage meteoTab;
        private CatchmentMeteoStationSelection catchmentMeteoStationSelection1;

    }
}
