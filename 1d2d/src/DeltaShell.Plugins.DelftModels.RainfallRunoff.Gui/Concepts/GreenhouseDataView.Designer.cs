using System.Windows.Forms;
using DelftTools.Controls.Swf;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    partial class GreenhouseDataView
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
            this.greenhouseHeaderPanel = new System.Windows.Forms.Panel();
            this.lblAreaPerGreenhouseType = new System.Windows.Forms.Label();
            this.bindingSourceGreenhouseViewModel = new System.Windows.Forms.BindingSource(this.components);
            this.lblLevelUnit = new System.Windows.Forms.Label();
            this.lblLevel = new System.Windows.Forms.Label();
            this.surfaceLevel = new System.Windows.Forms.TextBox();
            this.bindingSourceGreenhouse = new System.Windows.Forms.BindingSource(this.components);
            this.lblAreaUnit = new System.Windows.Forms.Label();
            this.lblPumpCapacityUnit = new System.Windows.Forms.Label();
            this.tbPumpCapacity = new System.Windows.Forms.TextBox();
            this.lblPumpCapacity = new System.Windows.Forms.Label();
            this.lblSiloCapacityUnit = new System.Windows.Forms.Label();
            this.tbSiloCapacity = new System.Windows.Forms.TextBox();
            this.lblSiloCapacity = new System.Windows.Forms.Label();
            this.tbSubSoilStorageArea = new System.Windows.Forms.TextBox();
            this.cbUseSubsoilStorage = new System.Windows.Forms.CheckBox();
            this.storageUnitComboBox = new BindableComboBox();
            this.initialRoofStorage = new System.Windows.Forms.TextBox();
            this.maximumRoofStorage = new System.Windows.Forms.TextBox();
            this.lblStorageInitial = new System.Windows.Forms.Label();
            this.lblStorageMaximum = new System.Windows.Forms.Label();
            this.lblOnRoof = new System.Windows.Forms.Label();
            this.greenhouseTab = new System.Windows.Forms.TabControl();
            this.generalTab = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.areaDictionaryEditor = new DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls.AreaDictionaryEditor();
            this.storageTab = new System.Windows.Forms.TabPage();
            this.lblSubSoilStorageAreaUnit = new System.Windows.Forms.Label();
            this.meteoTab = new System.Windows.Forms.TabPage();
            this.catchmentMeteoStationSelection1 = new DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.CatchmentMeteoStationSelection();
            this.greenhouseHeaderPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceGreenhouseViewModel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceGreenhouse)).BeginInit();
            this.greenhouseTab.SuspendLayout();
            this.generalTab.SuspendLayout();
            this.panel1.SuspendLayout();
            this.storageTab.SuspendLayout();
            this.meteoTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // greenhouseHeaderPanel
            // 
            this.greenhouseHeaderPanel.Controls.Add(this.lblAreaPerGreenhouseType);
            this.greenhouseHeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.greenhouseHeaderPanel.Location = new System.Drawing.Point(20, 20);
            this.greenhouseHeaderPanel.Name = "greenhouseHeaderPanel";
            this.greenhouseHeaderPanel.Size = new System.Drawing.Size(681, 30);
            this.greenhouseHeaderPanel.TabIndex = 3;
            // 
            // lblAreaPerGreenhouseType
            // 
            this.lblAreaPerGreenhouseType.AutoSize = true;
            this.lblAreaPerGreenhouseType.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGreenhouseViewModel, "AreaPerGreenhouseTypeLabel", true));
            this.lblAreaPerGreenhouseType.Location = new System.Drawing.Point(6, 9);
            this.lblAreaPerGreenhouseType.Name = "lblAreaPerGreenhouseType";
            this.lblAreaPerGreenhouseType.Size = new System.Drawing.Size(129, 13);
            this.lblAreaPerGreenhouseType.TabIndex = 1;
            this.lblAreaPerGreenhouseType.Text = "Area per greenhouse type";
            // 
            // bindingSourceGreenhouseViewModel
            // 
            this.bindingSourceGreenhouseViewModel.DataSource = typeof(DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.GreenhouseDataViewModel);
            // 
            // lblLevelUnit
            // 
            this.lblLevelUnit.AutoSize = true;
            this.lblLevelUnit.Location = new System.Drawing.Point(196, 23);
            this.lblLevelUnit.Name = "lblLevelUnit";
            this.lblLevelUnit.Size = new System.Drawing.Size(33, 13);
            this.lblLevelUnit.TabIndex = 5;
            this.lblLevelUnit.Text = "m AD";
            // 
            // lblLevel
            // 
            this.lblLevel.AutoSize = true;
            this.lblLevel.Location = new System.Drawing.Point(3, 23);
            this.lblLevel.Name = "lblLevel";
            this.lblLevel.Size = new System.Drawing.Size(69, 13);
            this.lblLevel.TabIndex = 4;
            this.lblLevel.Text = "Surface level";
            // 
            // surfaceLevel
            // 
            this.surfaceLevel.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGreenhouse, "SurfaceLevel", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N2"));
            this.surfaceLevel.Location = new System.Drawing.Point(120, 20);
            this.surfaceLevel.Name = "surfaceLevel";
            this.surfaceLevel.Size = new System.Drawing.Size(70, 20);
            this.surfaceLevel.TabIndex = 1;
            // 
            // bindingSourceGreenhouse
            // 
            this.bindingSourceGreenhouse.DataSource = typeof(DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.GreenhouseData);
            // 
            // lblAreaUnit
            // 
            this.lblAreaUnit.AutoSize = true;
            this.lblAreaUnit.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGreenhouse, "TotalAreaUnit", true));
            this.lblAreaUnit.Location = new System.Drawing.Point(226, 74);
            this.lblAreaUnit.Name = "lblAreaUnit";
            this.lblAreaUnit.Size = new System.Drawing.Size(0, 13);
            this.lblAreaUnit.TabIndex = 14;
            // 
            // lblPumpCapacityUnit
            // 
            this.lblPumpCapacityUnit.AutoSize = true;
            this.lblPumpCapacityUnit.Location = new System.Drawing.Point(226, 128);
            this.lblPumpCapacityUnit.Name = "lblPumpCapacityUnit";
            this.lblPumpCapacityUnit.Size = new System.Drawing.Size(28, 13);
            this.lblPumpCapacityUnit.TabIndex = 12;
            this.lblPumpCapacityUnit.Text = "m³/s";
            // 
            // tbPumpCapacity
            // 
            this.tbPumpCapacity.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGreenhouse, "PumpCapacity", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N2"));
            this.tbPumpCapacity.Location = new System.Drawing.Point(146, 125);
            this.tbPumpCapacity.Name = "tbPumpCapacity";
            this.tbPumpCapacity.Size = new System.Drawing.Size(71, 20);
            this.tbPumpCapacity.TabIndex = 11;
            // 
            // lblPumpCapacity
            // 
            this.lblPumpCapacity.AutoSize = true;
            this.lblPumpCapacity.Location = new System.Drawing.Point(52, 128);
            this.lblPumpCapacity.Name = "lblPumpCapacity";
            this.lblPumpCapacity.Size = new System.Drawing.Size(77, 13);
            this.lblPumpCapacity.TabIndex = 10;
            this.lblPumpCapacity.Text = "Pump capacity";
            // 
            // lblSiloCapacityUnit
            // 
            this.lblSiloCapacityUnit.AutoSize = true;
            this.lblSiloCapacityUnit.Location = new System.Drawing.Point(226, 102);
            this.lblSiloCapacityUnit.Name = "lblSiloCapacityUnit";
            this.lblSiloCapacityUnit.Size = new System.Drawing.Size(35, 13);
            this.lblSiloCapacityUnit.TabIndex = 9;
            this.lblSiloCapacityUnit.Text = "m³/ha";
            // 
            // tbSiloCapacity
            // 
            this.tbSiloCapacity.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGreenhouse, "SiloCapacity", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N2"));
            this.tbSiloCapacity.Location = new System.Drawing.Point(146, 99);
            this.tbSiloCapacity.Name = "tbSiloCapacity";
            this.tbSiloCapacity.Size = new System.Drawing.Size(71, 20);
            this.tbSiloCapacity.TabIndex = 7;
            // 
            // lblSiloCapacity
            // 
            this.lblSiloCapacity.AutoSize = true;
            this.lblSiloCapacity.Location = new System.Drawing.Point(52, 102);
            this.lblSiloCapacity.Name = "lblSiloCapacity";
            this.lblSiloCapacity.Size = new System.Drawing.Size(67, 13);
            this.lblSiloCapacity.TabIndex = 6;
            this.lblSiloCapacity.Text = "Silo capacity";
            // 
            // tbSubSoilStorageArea
            // 
            this.tbSubSoilStorageArea.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourceGreenhouse, "UseSubsoilStorage", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.tbSubSoilStorageArea.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGreenhouseViewModel, "SubSoilStorageArea", true));
            this.tbSubSoilStorageArea.Location = new System.Drawing.Point(146, 70);
            this.tbSubSoilStorageArea.Name = "tbSubSoilStorageArea";
            this.tbSubSoilStorageArea.Size = new System.Drawing.Size(71, 20);
            this.tbSubSoilStorageArea.TabIndex = 5;
            // 
            // cbUseSubsoilStorage
            // 
            this.cbUseSubsoilStorage.AutoSize = true;
            this.cbUseSubsoilStorage.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.bindingSourceGreenhouse, "UseSubsoilStorage", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.cbUseSubsoilStorage.Location = new System.Drawing.Point(30, 73);
            this.cbUseSubsoilStorage.Name = "cbUseSubsoilStorage";
            this.cbUseSubsoilStorage.Size = new System.Drawing.Size(98, 17);
            this.cbUseSubsoilStorage.TabIndex = 4;
            this.cbUseSubsoilStorage.Text = "Subsoil storage";
            this.cbUseSubsoilStorage.UseVisualStyleBackColor = true;
            // 
            // storageUnitComboBox
            // 
            this.storageUnitComboBox.DataBindings.Add(new System.Windows.Forms.Binding("SelectedItem", this.bindingSourceGreenhouseViewModel, "StorageUnit", true, DataSourceUpdateMode.OnPropertyChanged));
            this.storageUnitComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.storageUnitComboBox.FormattingEnabled = true;
            this.storageUnitComboBox.Location = new System.Drawing.Point(299, 43);
            this.storageUnitComboBox.Name = "storageUnitComboBox";
            this.storageUnitComboBox.Size = new System.Drawing.Size(79, 21);
            this.storageUnitComboBox.TabIndex = 4;
            // 
            // initialRoofStorage
            // 
            this.initialRoofStorage.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGreenhouseViewModel, "InitialRoofStorage", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N2"));
            this.initialRoofStorage.Location = new System.Drawing.Point(223, 44);
            this.initialRoofStorage.Name = "initialRoofStorage";
            this.initialRoofStorage.Size = new System.Drawing.Size(70, 20);
            this.initialRoofStorage.TabIndex = 3;
            // 
            // maximumRoofStorage
            // 
            this.maximumRoofStorage.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGreenhouseViewModel, "MaximumRoofStorage", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N2"));
            this.maximumRoofStorage.Location = new System.Drawing.Point(147, 44);
            this.maximumRoofStorage.Name = "maximumRoofStorage";
            this.maximumRoofStorage.Size = new System.Drawing.Size(70, 20);
            this.maximumRoofStorage.TabIndex = 2;
            // 
            // lblStorageInitial
            // 
            this.lblStorageInitial.AutoSize = true;
            this.lblStorageInitial.Location = new System.Drawing.Point(222, 26);
            this.lblStorageInitial.Name = "lblStorageInitial";
            this.lblStorageInitial.Size = new System.Drawing.Size(31, 13);
            this.lblStorageInitial.TabIndex = 1;
            this.lblStorageInitial.Text = "Initial";
            // 
            // lblStorageMaximum
            // 
            this.lblStorageMaximum.AutoSize = true;
            this.lblStorageMaximum.Location = new System.Drawing.Point(146, 26);
            this.lblStorageMaximum.Name = "lblStorageMaximum";
            this.lblStorageMaximum.Size = new System.Drawing.Size(51, 13);
            this.lblStorageMaximum.TabIndex = 1;
            this.lblStorageMaximum.Text = "Maximum";
            // 
            // lblOnRoof
            // 
            this.lblOnRoof.AutoSize = true;
            this.lblOnRoof.Location = new System.Drawing.Point(23, 47);
            this.lblOnRoof.Name = "lblOnRoof";
            this.lblOnRoof.Size = new System.Drawing.Size(42, 13);
            this.lblOnRoof.TabIndex = 1;
            this.lblOnRoof.Text = "On roof";
            // 
            // greenhouseTab
            // 
            this.greenhouseTab.Controls.Add(this.generalTab);
            this.greenhouseTab.Controls.Add(this.storageTab);
            this.greenhouseTab.Controls.Add(this.meteoTab);
            this.greenhouseTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.greenhouseTab.Location = new System.Drawing.Point(0, 0);
            this.greenhouseTab.Name = "greenhouseTab";
            this.greenhouseTab.SelectedIndex = 0;
            this.greenhouseTab.Size = new System.Drawing.Size(729, 496);
            this.greenhouseTab.TabIndex = 4;
            // 
            // generalTab
            // 
            this.generalTab.AutoScroll = true;
            this.generalTab.Controls.Add(this.panel1);
            this.generalTab.Controls.Add(this.areaDictionaryEditor);
            this.generalTab.Controls.Add(this.greenhouseHeaderPanel);
            this.generalTab.Location = new System.Drawing.Point(4, 22);
            this.generalTab.Name = "generalTab";
            this.generalTab.Padding = new System.Windows.Forms.Padding(20);
            this.generalTab.Size = new System.Drawing.Size(721, 470);
            this.generalTab.TabIndex = 0;
            this.generalTab.Text = "General";
            this.generalTab.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblLevel);
            this.panel1.Controls.Add(this.surfaceLevel);
            this.panel1.Controls.Add(this.lblLevelUnit);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(20, 211);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(681, 45);
            this.panel1.TabIndex = 6;
            // 
            // areaDictionaryEditor
            // 
            this.areaDictionaryEditor.Dock = System.Windows.Forms.DockStyle.Top;
            this.areaDictionaryEditor.Location = new System.Drawing.Point(20, 50);
            this.areaDictionaryEditor.Name = "areaDictionaryEditor";
            this.areaDictionaryEditor.Size = new System.Drawing.Size(681, 161);
            this.areaDictionaryEditor.TabIndex = 2;
            this.areaDictionaryEditor.TotalAreaLabel = "Total area greenhouses";
            this.areaDictionaryEditor.UnitLabel = "unit";
            // 
            // storageTab
            // 
            this.storageTab.AutoScroll = true;
            this.storageTab.Controls.Add(this.lblSubSoilStorageAreaUnit);
            this.storageTab.Controls.Add(this.lblAreaUnit);
            this.storageTab.Controls.Add(this.lblOnRoof);
            this.storageTab.Controls.Add(this.lblPumpCapacityUnit);
            this.storageTab.Controls.Add(this.lblStorageMaximum);
            this.storageTab.Controls.Add(this.tbPumpCapacity);
            this.storageTab.Controls.Add(this.lblStorageInitial);
            this.storageTab.Controls.Add(this.lblPumpCapacity);
            this.storageTab.Controls.Add(this.maximumRoofStorage);
            this.storageTab.Controls.Add(this.lblSiloCapacityUnit);
            this.storageTab.Controls.Add(this.initialRoofStorage);
            this.storageTab.Controls.Add(this.tbSiloCapacity);
            this.storageTab.Controls.Add(this.storageUnitComboBox);
            this.storageTab.Controls.Add(this.lblSiloCapacity);
            this.storageTab.Controls.Add(this.cbUseSubsoilStorage);
            this.storageTab.Controls.Add(this.tbSubSoilStorageArea);
            this.storageTab.Location = new System.Drawing.Point(4, 22);
            this.storageTab.Name = "storageTab";
            this.storageTab.Padding = new System.Windows.Forms.Padding(20);
            this.storageTab.Size = new System.Drawing.Size(721, 470);
            this.storageTab.TabIndex = 1;
            this.storageTab.Text = "Storage";
            this.storageTab.UseVisualStyleBackColor = true;
            // 
            // lblSubSoilStorageAreaUnit
            // 
            this.lblSubSoilStorageAreaUnit.AutoSize = true;
            this.lblSubSoilStorageAreaUnit.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGreenhouseViewModel, "AreaUnitLabel", true));
            this.lblSubSoilStorageAreaUnit.Location = new System.Drawing.Point(226, 74);
            this.lblSubSoilStorageAreaUnit.Name = "lblSubSoilStorageAreaUnit";
            this.lblSubSoilStorageAreaUnit.Size = new System.Drawing.Size(36, 13);
            this.lblSubSoilStorageAreaUnit.TabIndex = 15;
            this.lblSubSoilStorageAreaUnit.Text = "stimpy";
            // 
            // meteoTab
            // 
            this.meteoTab.Controls.Add(this.catchmentMeteoStationSelection1);
            this.meteoTab.Location = new System.Drawing.Point(4, 22);
            this.meteoTab.Name = "meteoTab";
            this.meteoTab.Size = new System.Drawing.Size(721, 470);
            this.meteoTab.TabIndex = 2;
            this.meteoTab.Text = "Meteo";
            this.meteoTab.UseVisualStyleBackColor = true;
            // 
            // catchmentMeteoStationSelection1
            // 
            this.catchmentMeteoStationSelection1.CatchmentModelData = null;
            this.catchmentMeteoStationSelection1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.catchmentMeteoStationSelection1.Location = new System.Drawing.Point(0, 0);
            this.catchmentMeteoStationSelection1.MeteoStations = null;
            this.catchmentMeteoStationSelection1.Name = "catchmentMeteoStationSelection1";
            this.catchmentMeteoStationSelection1.Size = new System.Drawing.Size(721, 470);
            this.catchmentMeteoStationSelection1.TabIndex = 0;
            this.catchmentMeteoStationSelection1.UseMeteoStations = false;
            // 
            // GreenhouseDataView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.greenhouseTab);
            this.Name = "GreenhouseDataView";
            this.Size = new System.Drawing.Size(729, 496);
            this.greenhouseHeaderPanel.ResumeLayout(false);
            this.greenhouseHeaderPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceGreenhouseViewModel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceGreenhouse)).EndInit();
            this.greenhouseTab.ResumeLayout(false);
            this.generalTab.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.storageTab.ResumeLayout(false);
            this.storageTab.PerformLayout();
            this.meteoTab.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private AreaDictionaryEditor areaDictionaryEditor;
        private System.Windows.Forms.Panel greenhouseHeaderPanel;
        private System.Windows.Forms.Label lblAreaPerGreenhouseType;
        private System.Windows.Forms.TextBox surfaceLevel;
        private System.Windows.Forms.Label lblLevel;
        private System.Windows.Forms.Label lblLevelUnit;
        private System.Windows.Forms.BindingSource bindingSourceGreenhouse;
        private System.Windows.Forms.ComboBox storageUnitComboBox;
        private System.Windows.Forms.TextBox initialRoofStorage;
        private System.Windows.Forms.TextBox maximumRoofStorage;
        private System.Windows.Forms.Label lblStorageInitial;
        private System.Windows.Forms.Label lblStorageMaximum;
        private System.Windows.Forms.Label lblOnRoof;
        private System.Windows.Forms.TextBox tbSubSoilStorageArea;
        private System.Windows.Forms.CheckBox cbUseSubsoilStorage;
        private System.Windows.Forms.Label lblPumpCapacityUnit;
        private System.Windows.Forms.TextBox tbPumpCapacity;
        private System.Windows.Forms.Label lblPumpCapacity;
        private System.Windows.Forms.Label lblSiloCapacityUnit;
        private System.Windows.Forms.TextBox tbSiloCapacity;
        private System.Windows.Forms.Label lblSiloCapacity;
        private System.Windows.Forms.Label lblAreaUnit;
        private System.Windows.Forms.BindingSource bindingSourceGreenhouseViewModel;
        private System.Windows.Forms.TabControl greenhouseTab;
        private System.Windows.Forms.TabPage generalTab;
        private System.Windows.Forms.TabPage storageTab;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblSubSoilStorageAreaUnit;
        private System.Windows.Forms.TabPage meteoTab;
        private CatchmentMeteoStationSelection catchmentMeteoStationSelection1;

    }
}
