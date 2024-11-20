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
            this.components = new System.ComponentModel.Container();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.areaTab = new System.Windows.Forms.TabPage();
            this.lblAreaUnit = new System.Windows.Forms.Label();
            this.lblRunoffArea = new System.Windows.Forms.Label();
            this.runoffArea = new System.Windows.Forms.TextBox();
            this.openWaterDataViewModelBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.meteoTab = new System.Windows.Forms.TabPage();
            this.catchmentMeteoStationSelection1 = new DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.CatchmentMeteoStationSelection();
            this.tabControl1.SuspendLayout();
            this.areaTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.openWaterDataViewModelBindingSource)).BeginInit();
            this.meteoTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.areaTab);
            this.tabControl1.Controls.Add(this.meteoTab);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(675, 150);
            this.tabControl1.TabIndex = 1;
            // 
            // areaTab
            // 
            this.areaTab.Controls.Add(this.lblAreaUnit);
            this.areaTab.Controls.Add(this.lblRunoffArea);
            this.areaTab.Controls.Add(this.runoffArea);
            this.areaTab.Location = new System.Drawing.Point(4, 22);
            this.areaTab.Margin = new System.Windows.Forms.Padding(2);
            this.areaTab.Name = "areaTab";
            this.areaTab.Padding = new System.Windows.Forms.Padding(2);
            this.areaTab.Size = new System.Drawing.Size(667, 124);
            this.areaTab.TabIndex = 1;
            this.areaTab.Text = "Area";
            this.areaTab.UseVisualStyleBackColor = true;
            // 
            // lblAreaUnit
            // 
            this.lblAreaUnit.AutoSize = true;
            this.lblAreaUnit.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.openWaterDataViewModelBindingSource, "AreaUnitLabel", true));
            this.lblAreaUnit.Location = new System.Drawing.Point(155, 10);
            this.lblAreaUnit.Name = "lblAreaUnit";
            this.lblAreaUnit.Size = new System.Drawing.Size(24, 13);
            this.lblAreaUnit.TabIndex = 21;
            this.lblAreaUnit.Text = "unit";
            // 
            // lblRunoffArea
            // 
            this.lblRunoffArea.AutoSize = true;
            this.lblRunoffArea.Location = new System.Drawing.Point(10, 10);
            this.lblRunoffArea.Name = "lblRunoffArea";
            this.lblRunoffArea.Size = new System.Drawing.Size(63, 13);
            this.lblRunoffArea.TabIndex = 20;
            this.lblRunoffArea.Text = "Runoff area";
            // 
            // runoffArea
            // 
            this.runoffArea.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.openWaterDataViewModelBindingSource, "TotalAreaInUnit", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N2"));
            this.runoffArea.Location = new System.Drawing.Point(80, 8);
            this.runoffArea.MaximumSize = new System.Drawing.Size(70, 20);
            this.runoffArea.Name = "runoffArea";
            this.runoffArea.Size = new System.Drawing.Size(70, 20);
            this.runoffArea.TabIndex = 19;
            // 
            // openWaterDataViewModelBindingSource
            // 
            this.openWaterDataViewModelBindingSource.DataSource = typeof(DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.OpenWaterDataViewModel);
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
            this.catchmentMeteoStationSelection1.Margin = new System.Windows.Forms.Padding(4);
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
            this.areaTab.ResumeLayout(false);
            this.areaTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.openWaterDataViewModelBindingSource)).EndInit();
            this.meteoTab.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage meteoTab;
        private CatchmentMeteoStationSelection catchmentMeteoStationSelection1;
        private System.Windows.Forms.TabPage areaTab;
        private System.Windows.Forms.Label lblAreaUnit;
        private System.Windows.Forms.Label lblRunoffArea;
        private System.Windows.Forms.TextBox runoffArea;
        private System.Windows.Forms.BindingSource openWaterDataViewModelBindingSource;
    }
}
