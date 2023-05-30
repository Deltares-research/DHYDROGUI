using System;
using System.Windows.Forms;
using DelftTools.Controls.Swf;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    partial class UnpavedDataView
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
            this.seepageH0SeriesButton = new System.Windows.Forms.Button();
            this.bindingSourceUnpavedViewModel = new System.Windows.Forms.BindingSource(this.components);
            this.label24 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.seepageSeriesButton = new System.Windows.Forms.Button();
            this.label17 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.seepageConstantRadio = new System.Windows.Forms.RadioButton();
            this.seepageHydraulicResistance = new System.Windows.Forms.TextBox();
            this.bindingSourceUnpaved = new System.Windows.Forms.BindingSource(this.components);
            this.seepage = new System.Windows.Forms.TextBox();
            this.seepageH0SeriesRadio = new System.Windows.Forms.RadioButton();
            this.seepageSeriesRadio = new System.Windows.Forms.RadioButton();
            this.drainagePanel = new System.Windows.Forms.Panel();
            this.drainageComboPanel = new System.Windows.Forms.Panel();
            this.label25 = new System.Windows.Forms.Label();
            this.drainageComboBox = new DelftTools.Controls.Swf.BindableComboBox();
            this.infiltrationUnitComboBox = new DelftTools.Controls.Swf.BindableComboBox();
            this.label15 = new System.Windows.Forms.Label();
            this.infiltrationCapacity = new System.Windows.Forms.TextBox();
            this.storageUnitComboBox = new DelftTools.Controls.Swf.BindableComboBox();
            this.initialLandStorage = new System.Windows.Forms.TextBox();
            this.maximumLandStorage = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.groundwaterSeriesButton = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.groundwaterSeriesRadio = new System.Windows.Forms.RadioButton();
            this.groundwaterConstantRadio = new System.Windows.Forms.RadioButton();
            this.groundwaterLinkedNodeRadio = new System.Windows.Forms.RadioButton();
            this.label7 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.groundwaterConstant = new System.Windows.Forms.TextBox();
            this.maximumGroundwaterLevel = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groundwaterThickness = new System.Windows.Forms.TextBox();
            this.soilTypeComboBox = new DelftTools.Controls.Swf.BindableComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.surfaceLevel = new System.Windows.Forms.TextBox();
            this.groundWaterAreaPanel = new System.Windows.Forms.Panel();
            this.unitLabel = new System.Windows.Forms.Label();
            this.groundwaterArea = new System.Windows.Forms.TextBox();
            this.differentGroundwaterAreaCheckBox = new System.Windows.Forms.CheckBox();
            this.cropsHeaderPanel = new System.Windows.Forms.Panel();
            this.lblAreaPerCropType = new System.Windows.Forms.Label();
            this.unpavedTabControl = new System.Windows.Forms.TabControl();
            this.cropsTab = new System.Windows.Forms.TabPage();
            this.areaDictionaryEditor = new DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls.AreaDictionaryEditor();
            this.surfaceSoilTab = new System.Windows.Forms.TabPage();
            this.lblComment = new System.Windows.Forms.Label();
            this.capsimSoilTypeComboBox = new DelftTools.Controls.Swf.BindableComboBox();
            this.lblCapsimSoilType = new System.Windows.Forms.Label();
            this.groundwaterTab = new System.Windows.Forms.TabPage();
            this.storageInfiltrationTab = new System.Windows.Forms.TabPage();
            this.drainageTab = new System.Windows.Forms.TabPage();
            this.seepageTab = new System.Windows.Forms.TabPage();
            this.meteoTab = new System.Windows.Forms.TabPage();
            this.catchmentMeteoStationSelection1 = new DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.CatchmentMeteoStationSelection();
            this.waterlevelTab = new System.Windows.Forms.TabPage();
            this.rrBoundarySeriesView1 = new DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls.RRBoundarySeriesView();
            this.rrBoundaryLinkPanel = new DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls.RRBoundaryLinkPanel();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceUnpavedViewModel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceUnpaved)).BeginInit();
            this.drainageComboPanel.SuspendLayout();
            this.groundWaterAreaPanel.SuspendLayout();
            this.cropsHeaderPanel.SuspendLayout();
            this.unpavedTabControl.SuspendLayout();
            this.cropsTab.SuspendLayout();
            this.surfaceSoilTab.SuspendLayout();
            this.groundwaterTab.SuspendLayout();
            this.storageInfiltrationTab.SuspendLayout();
            this.drainageTab.SuspendLayout();
            this.seepageTab.SuspendLayout();
            this.meteoTab.SuspendLayout();
            this.waterlevelTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // seepageH0SeriesButton
            // 
            this.seepageH0SeriesButton.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourceUnpavedViewModel, "SeepageIsH0Series", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.seepageH0SeriesButton.Location = new System.Drawing.Point(301, 155);
            this.seepageH0SeriesButton.Margin = new System.Windows.Forms.Padding(4);
            this.seepageH0SeriesButton.Name = "seepageH0SeriesButton";
            this.seepageH0SeriesButton.Size = new System.Drawing.Size(93, 25);
            this.seepageH0SeriesButton.TabIndex = 5;
            this.seepageH0SeriesButton.Text = "...";
            this.seepageH0SeriesButton.UseVisualStyleBackColor = true;
            this.seepageH0SeriesButton.Click += new System.EventHandler(this.SeepageH0SeriesButtonClick);
            // 
            // bindingSourceUnpavedViewModel
            // 
            this.bindingSourceUnpavedViewModel.DataSource = typeof(DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.UnpavedDataViewModel);
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(403, 160);
            this.label24.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(42, 17);
            this.label24.TabIndex = 3;
            this.label24.Text = "m AD";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(103, 160);
            this.label23.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(136, 17);
            this.label23.TabIndex = 3;
            this.label23.Text = "Piezometric level H0";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(103, 127);
            this.label22.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(164, 17);
            this.label22.TabIndex = 3;
            this.label22.Text = "Hydraulic Resistance (C)";
            // 
            // seepageSeriesButton
            // 
            this.seepageSeriesButton.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourceUnpavedViewModel, "SeepageIsSeries", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.seepageSeriesButton.Location = new System.Drawing.Point(197, 62);
            this.seepageSeriesButton.Margin = new System.Windows.Forms.Padding(4);
            this.seepageSeriesButton.Name = "seepageSeriesButton";
            this.seepageSeriesButton.Size = new System.Drawing.Size(93, 25);
            this.seepageSeriesButton.TabIndex = 5;
            this.seepageSeriesButton.Text = "...";
            this.seepageSeriesButton.UseVisualStyleBackColor = true;
            this.seepageSeriesButton.Click += new System.EventHandler(this.SeepageSeriesButtonClick);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.ForeColor = System.Drawing.Color.DimGray;
            this.label17.Location = new System.Drawing.Point(33, 197);
            this.label17.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(414, 17);
            this.label17.TabIndex = 4;
            this.label17.Text = "Notice: positive = upward (inflow), negative = downward (outflow)";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(403, 127);
            this.label21.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(31, 17);
            this.label21.TabIndex = 3;
            this.label21.Text = "day";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(300, 37);
            this.label16.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(57, 17);
            this.label16.TabIndex = 3;
            this.label16.Text = "mm/day";
            // 
            // seepageConstantRadio
            // 
            this.seepageConstantRadio.AutoSize = true;
            this.seepageConstantRadio.Checked = true;
            this.seepageConstantRadio.Location = new System.Drawing.Point(37, 34);
            this.seepageConstantRadio.Margin = new System.Windows.Forms.Padding(4);
            this.seepageConstantRadio.Name = "seepageConstantRadio";
            this.seepageConstantRadio.Size = new System.Drawing.Size(85, 21);
            this.seepageConstantRadio.TabIndex = 0;
            this.seepageConstantRadio.TabStop = true;
            this.seepageConstantRadio.Text = "Constant";
            this.seepageConstantRadio.UseVisualStyleBackColor = true;
            this.seepageConstantRadio.CheckedChanged += new System.EventHandler(this.SeriesRadioCheckedChanged);
            // 
            // seepageHydraulicResistance
            // 
            this.seepageHydraulicResistance.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceUnpaved, "SeepageH0HydraulicResistance", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N2"));
            this.seepageHydraulicResistance.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourceUnpavedViewModel, "SeepageIsH0Series", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.seepageHydraulicResistance.Location = new System.Drawing.Point(301, 123);
            this.seepageHydraulicResistance.Margin = new System.Windows.Forms.Padding(4);
            this.seepageHydraulicResistance.Name = "seepageHydraulicResistance";
            this.seepageHydraulicResistance.Size = new System.Drawing.Size(92, 22);
            this.seepageHydraulicResistance.TabIndex = 2;
            // 
            // bindingSourceUnpaved
            // 
            this.bindingSourceUnpaved.DataSource = typeof(DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.UnpavedData);
            // 
            // seepage
            // 
            this.seepage.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceUnpaved, "SeepageConstant", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N2"));
            this.seepage.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourceUnpavedViewModel, "SeepageIsConstant", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.seepage.Location = new System.Drawing.Point(199, 33);
            this.seepage.Margin = new System.Windows.Forms.Padding(4);
            this.seepage.Name = "seepage";
            this.seepage.Size = new System.Drawing.Size(92, 22);
            this.seepage.TabIndex = 2;
            // 
            // seepageH0SeriesRadio
            // 
            this.seepageH0SeriesRadio.AutoSize = true;
            this.seepageH0SeriesRadio.Location = new System.Drawing.Point(37, 91);
            this.seepageH0SeriesRadio.Margin = new System.Windows.Forms.Padding(4);
            this.seepageH0SeriesRadio.Name = "seepageH0SeriesRadio";
            this.seepageH0SeriesRadio.Size = new System.Drawing.Size(149, 21);
            this.seepageH0SeriesRadio.TabIndex = 1;
            this.seepageH0SeriesRadio.TabStop = true;
            this.seepageH0SeriesRadio.Text = "Variable (H0-table)";
            this.seepageH0SeriesRadio.UseVisualStyleBackColor = true;
            this.seepageH0SeriesRadio.CheckedChanged += new System.EventHandler(this.SeriesRadioCheckedChanged);
            // 
            // seepageSeriesRadio
            // 
            this.seepageSeriesRadio.AutoSize = true;
            this.seepageSeriesRadio.Location = new System.Drawing.Point(37, 63);
            this.seepageSeriesRadio.Margin = new System.Windows.Forms.Padding(4);
            this.seepageSeriesRadio.Name = "seepageSeriesRadio";
            this.seepageSeriesRadio.Size = new System.Drawing.Size(126, 21);
            this.seepageSeriesRadio.TabIndex = 1;
            this.seepageSeriesRadio.TabStop = true;
            this.seepageSeriesRadio.Text = "Variable (table)";
            this.seepageSeriesRadio.UseVisualStyleBackColor = true;
            this.seepageSeriesRadio.CheckedChanged += new System.EventHandler(this.SeriesRadioCheckedChanged);
            // 
            // drainagePanel
            // 
            this.drainagePanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.drainagePanel.Location = new System.Drawing.Point(13, 64);
            this.drainagePanel.Margin = new System.Windows.Forms.Padding(4);
            this.drainagePanel.Name = "drainagePanel";
            this.drainagePanel.Size = new System.Drawing.Size(802, 283);
            this.drainagePanel.TabIndex = 5;
            // 
            // drainageComboPanel
            // 
            this.drainageComboPanel.Controls.Add(this.label25);
            this.drainageComboPanel.Controls.Add(this.drainageComboBox);
            this.drainageComboPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.drainageComboPanel.Location = new System.Drawing.Point(13, 12);
            this.drainageComboPanel.Margin = new System.Windows.Forms.Padding(4);
            this.drainageComboPanel.Name = "drainageComboPanel";
            this.drainageComboPanel.Size = new System.Drawing.Size(802, 52);
            this.drainageComboPanel.TabIndex = 4;
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(23, 17);
            this.label25.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(130, 17);
            this.label25.TabIndex = 3;
            this.label25.Text = "Computation option";
            // 
            // drainageComboBox
            // 
            this.drainageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.drainageComboBox.FormattingEnabled = true;
            this.drainageComboBox.Items.AddRange(new object[]
            {
                "De Zeeuw-Hellinga",
                "Ernst",
                "Krayenhoff van de Leur"
            });
            this.drainageComboBox.Location = new System.Drawing.Point(187, 12);
            this.drainageComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.drainageComboBox.Name = "drainageComboBox";
            this.drainageComboBox.Size = new System.Drawing.Size(317, 24);
            this.drainageComboBox.TabIndex = 0;
            this.drainageComboBox.SelectedValueChanged += new System.EventHandler(this.DrainageComboBoxSelectedValueChanged);
            // 
            // infiltrationUnitComboBox
            // 
            this.infiltrationUnitComboBox.DataBindings.Add(new System.Windows.Forms.Binding("SelectedItem", this.bindingSourceUnpavedViewModel, "InfiltrationCapacityUnit", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.infiltrationUnitComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.infiltrationUnitComboBox.FormattingEnabled = true;
            this.infiltrationUnitComboBox.Location = new System.Drawing.Point(319, 27);
            this.infiltrationUnitComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.infiltrationUnitComboBox.Name = "infiltrationUnitComboBox";
            this.infiltrationUnitComboBox.Size = new System.Drawing.Size(104, 24);
            this.infiltrationUnitComboBox.TabIndex = 3;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(37, 31);
            this.label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(125, 17);
            this.label15.TabIndex = 3;
            this.label15.Text = "Infiltration capacity";
            // 
            // infiltrationCapacity
            // 
            this.infiltrationCapacity.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceUnpavedViewModel, "InfiltrationCapacity", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N2"));
            this.infiltrationCapacity.Location = new System.Drawing.Point(203, 27);
            this.infiltrationCapacity.Margin = new System.Windows.Forms.Padding(4);
            this.infiltrationCapacity.Name = "infiltrationCapacity";
            this.infiltrationCapacity.Size = new System.Drawing.Size(92, 22);
            this.infiltrationCapacity.TabIndex = 4;
            // 
            // storageUnitComboBox
            // 
            this.storageUnitComboBox.DataBindings.Add(new System.Windows.Forms.Binding("SelectedItem", this.bindingSourceUnpavedViewModel, "StorageUnit", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.storageUnitComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.storageUnitComboBox.FormattingEnabled = true;
            this.storageUnitComboBox.Location = new System.Drawing.Point(419, 90);
            this.storageUnitComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.storageUnitComboBox.Name = "storageUnitComboBox";
            this.storageUnitComboBox.Size = new System.Drawing.Size(104, 24);
            this.storageUnitComboBox.TabIndex = 3;
            // 
            // initialLandStorage
            // 
            this.initialLandStorage.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceUnpavedViewModel, "InitialLandStorage", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N2"));
            this.initialLandStorage.Location = new System.Drawing.Point(304, 90);
            this.initialLandStorage.Margin = new System.Windows.Forms.Padding(4);
            this.initialLandStorage.Name = "initialLandStorage";
            this.initialLandStorage.Size = new System.Drawing.Size(92, 22);
            this.initialLandStorage.TabIndex = 2;
            // 
            // maximumLandStorage
            // 
            this.maximumLandStorage.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceUnpavedViewModel, "MaximumLandStorage", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N2"));
            this.maximumLandStorage.Location = new System.Drawing.Point(203, 90);
            this.maximumLandStorage.Margin = new System.Windows.Forms.Padding(4);
            this.maximumLandStorage.Name = "maximumLandStorage";
            this.maximumLandStorage.Size = new System.Drawing.Size(92, 22);
            this.maximumLandStorage.TabIndex = 2;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(303, 70);
            this.label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(40, 17);
            this.label14.TabIndex = 1;
            this.label14.Text = "Initial";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(201, 70);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(66, 17);
            this.label13.TabIndex = 1;
            this.label13.Text = "Maximum";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(37, 94);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(109, 17);
            this.label12.TabIndex = 1;
            this.label12.Text = "Storage on land";
            // 
            // groundwaterSeriesButton
            // 
            this.groundwaterSeriesButton.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourceUnpavedViewModel, "GroundWaterLevelIsSeries", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.groundwaterSeriesButton.Location = new System.Drawing.Point(200, 174);
            this.groundwaterSeriesButton.Margin = new System.Windows.Forms.Padding(4);
            this.groundwaterSeriesButton.Name = "groundwaterSeriesButton";
            this.groundwaterSeriesButton.Size = new System.Drawing.Size(93, 25);
            this.groundwaterSeriesButton.TabIndex = 4;
            this.groundwaterSeriesButton.Text = "...";
            this.groundwaterSeriesButton.UseVisualStyleBackColor = true;
            this.groundwaterSeriesButton.Click += new System.EventHandler(this.GroundwaterSeriesButtonClick);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(301, 153);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(110, 17);
            this.label11.TabIndex = 3;
            this.label11.Text = "m below surface";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(303, 63);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(42, 17);
            this.label9.TabIndex = 3;
            this.label9.Text = "m AD";
            // 
            // groundwaterSeriesRadio
            // 
            this.groundwaterSeriesRadio.AutoSize = true;
            this.groundwaterSeriesRadio.Location = new System.Drawing.Point(49, 176);
            this.groundwaterSeriesRadio.Margin = new System.Windows.Forms.Padding(4);
            this.groundwaterSeriesRadio.Name = "groundwaterSeriesRadio";
            this.groundwaterSeriesRadio.Size = new System.Drawing.Size(122, 21);
            this.groundwaterSeriesRadio.TabIndex = 1;
            this.groundwaterSeriesRadio.Text = "Pick from table";
            this.groundwaterSeriesRadio.UseVisualStyleBackColor = true;
            this.groundwaterSeriesRadio.CheckedChanged += new System.EventHandler(this.GroundwaterCheckedChanged);
            // 
            // groundwaterConstantRadio
            // 
            this.groundwaterConstantRadio.AutoSize = true;
            this.groundwaterConstantRadio.Checked = true;
            this.groundwaterConstantRadio.Location = new System.Drawing.Point(49, 148);
            this.groundwaterConstantRadio.Margin = new System.Windows.Forms.Padding(4);
            this.groundwaterConstantRadio.Name = "groundwaterConstantRadio";
            this.groundwaterConstantRadio.Size = new System.Drawing.Size(85, 21);
            this.groundwaterConstantRadio.TabIndex = 1;
            this.groundwaterConstantRadio.TabStop = true;
            this.groundwaterConstantRadio.Text = "Constant";
            this.groundwaterConstantRadio.UseVisualStyleBackColor = true;
            this.groundwaterConstantRadio.CheckedChanged += new System.EventHandler(this.GroundwaterCheckedChanged);
            // 
            // groundwaterLinkedNodeRadio
            // 
            this.groundwaterLinkedNodeRadio.AutoSize = true;
            this.groundwaterLinkedNodeRadio.Location = new System.Drawing.Point(49, 121);
            this.groundwaterLinkedNodeRadio.Margin = new System.Windows.Forms.Padding(4);
            this.groundwaterLinkedNodeRadio.Name = "groundwaterLinkedNodeRadio";
            this.groundwaterLinkedNodeRadio.Size = new System.Drawing.Size(351, 21);
            this.groundwaterLinkedNodeRadio.TabIndex = 1;
            this.groundwaterLinkedNodeRadio.Text = "Take from linked node (boundary or lateral source)";
            this.groundwaterLinkedNodeRadio.UseVisualStyleBackColor = true;
            this.groundwaterLinkedNodeRadio.CheckedChanged += new System.EventHandler(this.GroundwaterCheckedChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(303, 31);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(19, 17);
            this.label7.TabIndex = 3;
            this.label7.Text = "m";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(37, 95);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(78, 17);
            this.label10.TabIndex = 1;
            this.label10.Text = "Initial Level";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(37, 63);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(150, 17);
            this.label8.TabIndex = 1;
            this.label8.Text = "Maximum allowed level";
            // 
            // groundwaterConstant
            // 
            this.groundwaterConstant.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceUnpaved, "InitialGroundWaterLevelConstant", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N2"));
            this.groundwaterConstant.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourceUnpavedViewModel, "GroundWaterLevelIsConstant", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.groundwaterConstant.Location = new System.Drawing.Point(200, 146);
            this.groundwaterConstant.Margin = new System.Windows.Forms.Padding(4);
            this.groundwaterConstant.Name = "groundwaterConstant";
            this.groundwaterConstant.Size = new System.Drawing.Size(92, 22);
            this.groundwaterConstant.TabIndex = 2;
            // 
            // maximumGroundwaterLevel
            // 
            this.maximumGroundwaterLevel.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceUnpaved, "MaximumAllowedGroundWaterLevel", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N2"));
            this.maximumGroundwaterLevel.Location = new System.Drawing.Point(201, 59);
            this.maximumGroundwaterLevel.Margin = new System.Windows.Forms.Padding(4);
            this.maximumGroundwaterLevel.Name = "maximumGroundwaterLevel";
            this.maximumGroundwaterLevel.Size = new System.Drawing.Size(92, 22);
            this.maximumGroundwaterLevel.TabIndex = 2;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(37, 31);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(107, 17);
            this.label6.TabIndex = 1;
            this.label6.Text = "Layer thickness";
            // 
            // groundwaterThickness
            // 
            this.groundwaterThickness.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceUnpaved, "GroundWaterLayerThickness", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N2"));
            this.groundwaterThickness.Location = new System.Drawing.Point(201, 27);
            this.groundwaterThickness.Margin = new System.Windows.Forms.Padding(4);
            this.groundwaterThickness.Name = "groundwaterThickness";
            this.groundwaterThickness.Size = new System.Drawing.Size(92, 22);
            this.groundwaterThickness.TabIndex = 2;
            // 
            // soilTypeComboBox
            // 
            this.soilTypeComboBox.DataBindings.Add(new System.Windows.Forms.Binding("SelectedItem", this.bindingSourceUnpaved, "SoilType", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.soilTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.soilTypeComboBox.FormattingEnabled = true;
            this.soilTypeComboBox.Location = new System.Drawing.Point(201, 66);
            this.soilTypeComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.soilTypeComboBox.Name = "soilTypeComboBox";
            this.soilTypeComboBox.Size = new System.Drawing.Size(321, 24);
            this.soilTypeComboBox.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(37, 70);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 17);
            this.label3.TabIndex = 1;
            this.label3.Text = "Soil type";
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(37, 31);
            this.label27.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(90, 17);
            this.label27.TabIndex = 4;
            this.label27.Text = "Surface level";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(303, 31);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "m AD";
            // 
            // surfaceLevel
            // 
            this.surfaceLevel.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceUnpaved, "SurfaceLevel", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N2"));
            this.surfaceLevel.Location = new System.Drawing.Point(201, 27);
            this.surfaceLevel.Margin = new System.Windows.Forms.Padding(4);
            this.surfaceLevel.Name = "surfaceLevel";
            this.surfaceLevel.Size = new System.Drawing.Size(92, 22);
            this.surfaceLevel.TabIndex = 2;
            // 
            // groundWaterAreaPanel
            // 
            this.groundWaterAreaPanel.Controls.Add(this.unitLabel);
            this.groundWaterAreaPanel.Controls.Add(this.groundwaterArea);
            this.groundWaterAreaPanel.Controls.Add(this.differentGroundwaterAreaCheckBox);
            this.groundWaterAreaPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.groundWaterAreaPanel.Location = new System.Drawing.Point(13, 257);
            this.groundWaterAreaPanel.Margin = new System.Windows.Forms.Padding(4);
            this.groundWaterAreaPanel.Name = "groundWaterAreaPanel";
            this.groundWaterAreaPanel.Size = new System.Drawing.Size(802, 74);
            this.groundWaterAreaPanel.TabIndex = 6;
            // 
            // unitLabel
            // 
            this.unitLabel.AutoSize = true;
            this.unitLabel.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceUnpavedViewModel, "AreaUnitLabel", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.unitLabel.Location = new System.Drawing.Point(288, 39);
            this.unitLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.unitLabel.Name = "unitLabel";
            this.unitLabel.Size = new System.Drawing.Size(31, 17);
            this.unitLabel.TabIndex = 3;
            this.unitLabel.Text = "unit";
            // 
            // groundwaterArea
            // 
            this.groundwaterArea.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceUnpavedViewModel, "TotalAreaForGroundWaterCalculations", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N2"));
            this.groundwaterArea.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourceUnpaved, "UseDifferentAreaForGroundWaterCalculations", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.groundwaterArea.Location = new System.Drawing.Point(185, 36);
            this.groundwaterArea.Margin = new System.Windows.Forms.Padding(4);
            this.groundwaterArea.Name = "groundwaterArea";
            this.groundwaterArea.Size = new System.Drawing.Size(92, 22);
            this.groundwaterArea.TabIndex = 2;
            // 
            // differentGroundwaterAreaCheckBox
            // 
            this.differentGroundwaterAreaCheckBox.AutoSize = true;
            this.differentGroundwaterAreaCheckBox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.bindingSourceUnpaved, "UseDifferentAreaForGroundWaterCalculations", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.differentGroundwaterAreaCheckBox.Location = new System.Drawing.Point(25, 7);
            this.differentGroundwaterAreaCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.differentGroundwaterAreaCheckBox.Name = "differentGroundwaterAreaCheckBox";
            this.differentGroundwaterAreaCheckBox.Size = new System.Drawing.Size(326, 21);
            this.differentGroundwaterAreaCheckBox.TabIndex = 5;
            this.differentGroundwaterAreaCheckBox.Text = "Use different area for groundwater calculations";
            this.differentGroundwaterAreaCheckBox.UseVisualStyleBackColor = true;
            // 
            // cropsHeaderPanel
            // 
            this.cropsHeaderPanel.Controls.Add(this.lblAreaPerCropType);
            this.cropsHeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.cropsHeaderPanel.Location = new System.Drawing.Point(13, 12);
            this.cropsHeaderPanel.Margin = new System.Windows.Forms.Padding(4);
            this.cropsHeaderPanel.Name = "cropsHeaderPanel";
            this.cropsHeaderPanel.Size = new System.Drawing.Size(802, 37);
            this.cropsHeaderPanel.TabIndex = 3;
            // 
            // lblAreaPerCropType
            // 
            this.lblAreaPerCropType.AutoSize = true;
            this.lblAreaPerCropType.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceUnpavedViewModel, "AreaPerCropTypeLabel", true));
            this.lblAreaPerCropType.Location = new System.Drawing.Point(8, 11);
            this.lblAreaPerCropType.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAreaPerCropType.Name = "lblAreaPerCropType";
            this.lblAreaPerCropType.Size = new System.Drawing.Size(126, 17);
            this.lblAreaPerCropType.TabIndex = 1;
            this.lblAreaPerCropType.Text = "Area per crop type";
            // 
            // unpavedTabControl
            // 
            this.unpavedTabControl.Controls.Add(this.cropsTab);
            this.unpavedTabControl.Controls.Add(this.surfaceSoilTab);
            this.unpavedTabControl.Controls.Add(this.groundwaterTab);
            this.unpavedTabControl.Controls.Add(this.storageInfiltrationTab);
            this.unpavedTabControl.Controls.Add(this.drainageTab);
            this.unpavedTabControl.Controls.Add(this.seepageTab);
            this.unpavedTabControl.Controls.Add(this.meteoTab);
            this.unpavedTabControl.Controls.Add(this.waterlevelTab);
            this.unpavedTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.unpavedTabControl.Location = new System.Drawing.Point(0, 0);
            this.unpavedTabControl.Margin = new System.Windows.Forms.Padding(4);
            this.unpavedTabControl.Name = "unpavedTabControl";
            this.unpavedTabControl.SelectedIndex = 0;
            this.unpavedTabControl.Size = new System.Drawing.Size(836, 491);
            this.unpavedTabControl.TabIndex = 7;
            // 
            // cropsTab
            // 
            this.cropsTab.AutoScroll = true;
            this.cropsTab.Controls.Add(this.groundWaterAreaPanel);
            this.cropsTab.Controls.Add(this.areaDictionaryEditor);
            this.cropsTab.Controls.Add(this.cropsHeaderPanel);
            this.cropsTab.Location = new System.Drawing.Point(4, 25);
            this.cropsTab.Margin = new System.Windows.Forms.Padding(4);
            this.cropsTab.Name = "cropsTab";
            this.cropsTab.Padding = new System.Windows.Forms.Padding(13, 12, 13, 12);
            this.cropsTab.Size = new System.Drawing.Size(828, 462);
            this.cropsTab.TabIndex = 0;
            this.cropsTab.Text = "Crops";
            this.cropsTab.UseVisualStyleBackColor = true;
            // 
            // areaDictionaryEditor
            // 
            this.areaDictionaryEditor.Dock = System.Windows.Forms.DockStyle.Top;
            this.areaDictionaryEditor.Location = new System.Drawing.Point(13, 49);
            this.areaDictionaryEditor.Margin = new System.Windows.Forms.Padding(5);
            this.areaDictionaryEditor.Name = "areaDictionaryEditor";
            this.areaDictionaryEditor.Size = new System.Drawing.Size(802, 208);
            this.areaDictionaryEditor.TabIndex = 2;
            this.areaDictionaryEditor.TotalAreaLabel = "Total area";
            this.areaDictionaryEditor.UnitLabel = "unit";
            // 
            // surfaceSoilTab
            // 
            this.surfaceSoilTab.AutoScroll = true;
            this.surfaceSoilTab.Controls.Add(this.lblComment);
            this.surfaceSoilTab.Controls.Add(this.capsimSoilTypeComboBox);
            this.surfaceSoilTab.Controls.Add(this.lblCapsimSoilType);
            this.surfaceSoilTab.Controls.Add(this.soilTypeComboBox);
            this.surfaceSoilTab.Controls.Add(this.label3);
            this.surfaceSoilTab.Controls.Add(this.label27);
            this.surfaceSoilTab.Controls.Add(this.label1);
            this.surfaceSoilTab.Controls.Add(this.surfaceLevel);
            this.surfaceSoilTab.Location = new System.Drawing.Point(4, 25);
            this.surfaceSoilTab.Margin = new System.Windows.Forms.Padding(4);
            this.surfaceSoilTab.Name = "surfaceSoilTab";
            this.surfaceSoilTab.Padding = new System.Windows.Forms.Padding(33, 31, 33, 31);
            this.surfaceSoilTab.Size = new System.Drawing.Size(828, 462);
            this.surfaceSoilTab.TabIndex = 1;
            this.surfaceSoilTab.Text = "Surface & Soil";
            this.surfaceSoilTab.UseVisualStyleBackColor = true;
            // 
            // lblComment
            // 
            this.lblComment.AutoSize = true;
            this.lblComment.Location = new System.Drawing.Point(43, 158);
            this.lblComment.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblComment.Name = "lblComment";
            this.lblComment.Size = new System.Drawing.Size(518, 17);
            this.lblComment.TabIndex = 7;
            this.lblComment.Text = "*) If the model settings has been set to capsim the capsim soil types will be use" + "d. ";
            // 
            // capsimSoilTypeComboBox
            // 
            this.capsimSoilTypeComboBox.DataBindings.Add(new System.Windows.Forms.Binding("SelectedItem", this.bindingSourceUnpaved, "SoilTypeCapsim", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.capsimSoilTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.capsimSoilTypeComboBox.FormattingEnabled = true;
            this.capsimSoilTypeComboBox.Location = new System.Drawing.Point(203, 110);
            this.capsimSoilTypeComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.capsimSoilTypeComboBox.Name = "capsimSoilTypeComboBox";
            this.capsimSoilTypeComboBox.Size = new System.Drawing.Size(321, 24);
            this.capsimSoilTypeComboBox.TabIndex = 6;
            // 
            // lblCapsimSoilType
            // 
            this.lblCapsimSoilType.AutoSize = true;
            this.lblCapsimSoilType.Location = new System.Drawing.Point(39, 113);
            this.lblCapsimSoilType.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCapsimSoilType.Name = "lblCapsimSoilType";
            this.lblCapsimSoilType.Size = new System.Drawing.Size(117, 17);
            this.lblCapsimSoilType.TabIndex = 5;
            this.lblCapsimSoilType.Text = "CapSim soil type*";
            // 
            // groundwaterTab
            // 
            this.groundwaterTab.AutoScroll = true;
            this.groundwaterTab.Controls.Add(this.groundwaterSeriesButton);
            this.groundwaterTab.Controls.Add(this.label11);
            this.groundwaterTab.Controls.Add(this.label6);
            this.groundwaterTab.Controls.Add(this.label9);
            this.groundwaterTab.Controls.Add(this.groundwaterThickness);
            this.groundwaterTab.Controls.Add(this.groundwaterSeriesRadio);
            this.groundwaterTab.Controls.Add(this.maximumGroundwaterLevel);
            this.groundwaterTab.Controls.Add(this.groundwaterConstantRadio);
            this.groundwaterTab.Controls.Add(this.groundwaterConstant);
            this.groundwaterTab.Controls.Add(this.groundwaterLinkedNodeRadio);
            this.groundwaterTab.Controls.Add(this.label8);
            this.groundwaterTab.Controls.Add(this.label7);
            this.groundwaterTab.Controls.Add(this.label10);
            this.groundwaterTab.Location = new System.Drawing.Point(4, 25);
            this.groundwaterTab.Margin = new System.Windows.Forms.Padding(4);
            this.groundwaterTab.Name = "groundwaterTab";
            this.groundwaterTab.Padding = new System.Windows.Forms.Padding(33, 31, 33, 31);
            this.groundwaterTab.Size = new System.Drawing.Size(828, 462);
            this.groundwaterTab.TabIndex = 2;
            this.groundwaterTab.Text = "Groundwater";
            this.groundwaterTab.UseVisualStyleBackColor = true;
            // 
            // storageInfiltrationTab
            // 
            this.storageInfiltrationTab.AutoScroll = true;
            this.storageInfiltrationTab.Controls.Add(this.infiltrationUnitComboBox);
            this.storageInfiltrationTab.Controls.Add(this.storageUnitComboBox);
            this.storageInfiltrationTab.Controls.Add(this.label15);
            this.storageInfiltrationTab.Controls.Add(this.infiltrationCapacity);
            this.storageInfiltrationTab.Controls.Add(this.initialLandStorage);
            this.storageInfiltrationTab.Controls.Add(this.maximumLandStorage);
            this.storageInfiltrationTab.Controls.Add(this.label12);
            this.storageInfiltrationTab.Controls.Add(this.label14);
            this.storageInfiltrationTab.Controls.Add(this.label13);
            this.storageInfiltrationTab.Location = new System.Drawing.Point(4, 25);
            this.storageInfiltrationTab.Margin = new System.Windows.Forms.Padding(4);
            this.storageInfiltrationTab.Name = "storageInfiltrationTab";
            this.storageInfiltrationTab.Padding = new System.Windows.Forms.Padding(33, 31, 33, 31);
            this.storageInfiltrationTab.Size = new System.Drawing.Size(828, 462);
            this.storageInfiltrationTab.TabIndex = 3;
            this.storageInfiltrationTab.Text = "Storage & Infiltration";
            this.storageInfiltrationTab.UseVisualStyleBackColor = true;
            // 
            // drainageTab
            // 
            this.drainageTab.AutoScroll = true;
            this.drainageTab.Controls.Add(this.drainagePanel);
            this.drainageTab.Controls.Add(this.drainageComboPanel);
            this.drainageTab.Location = new System.Drawing.Point(4, 25);
            this.drainageTab.Margin = new System.Windows.Forms.Padding(4);
            this.drainageTab.Name = "drainageTab";
            this.drainageTab.Padding = new System.Windows.Forms.Padding(13, 12, 13, 12);
            this.drainageTab.Size = new System.Drawing.Size(828, 462);
            this.drainageTab.TabIndex = 4;
            this.drainageTab.Text = "Drainage";
            this.drainageTab.UseVisualStyleBackColor = true;
            // 
            // seepageTab
            // 
            this.seepageTab.AutoScroll = true;
            this.seepageTab.Controls.Add(this.seepageH0SeriesButton);
            this.seepageTab.Controls.Add(this.label24);
            this.seepageTab.Controls.Add(this.seepageConstantRadio);
            this.seepageTab.Controls.Add(this.label23);
            this.seepageTab.Controls.Add(this.seepageSeriesRadio);
            this.seepageTab.Controls.Add(this.label22);
            this.seepageTab.Controls.Add(this.seepageH0SeriesRadio);
            this.seepageTab.Controls.Add(this.seepageSeriesButton);
            this.seepageTab.Controls.Add(this.seepage);
            this.seepageTab.Controls.Add(this.label17);
            this.seepageTab.Controls.Add(this.seepageHydraulicResistance);
            this.seepageTab.Controls.Add(this.label21);
            this.seepageTab.Controls.Add(this.label16);
            this.seepageTab.Location = new System.Drawing.Point(4, 25);
            this.seepageTab.Margin = new System.Windows.Forms.Padding(4);
            this.seepageTab.Name = "seepageTab";
            this.seepageTab.Padding = new System.Windows.Forms.Padding(33, 31, 33, 31);
            this.seepageTab.Size = new System.Drawing.Size(828, 462);
            this.seepageTab.TabIndex = 5;
            this.seepageTab.Text = "Seepage";
            this.seepageTab.UseVisualStyleBackColor = true;
            // 
            // meteoTab
            // 
            this.meteoTab.Controls.Add(this.catchmentMeteoStationSelection1);
            this.meteoTab.Location = new System.Drawing.Point(4, 25);
            this.meteoTab.Margin = new System.Windows.Forms.Padding(4);
            this.meteoTab.Name = "meteoTab";
            this.meteoTab.Size = new System.Drawing.Size(828, 462);
            this.meteoTab.TabIndex = 7;
            this.meteoTab.Text = "Meteo";
            this.meteoTab.UseVisualStyleBackColor = true;
            // 
            // catchmentMeteoStationSelection1
            // 
            this.catchmentMeteoStationSelection1.CatchmentModelData = null;
            this.catchmentMeteoStationSelection1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.catchmentMeteoStationSelection1.Location = new System.Drawing.Point(0, 0);
            this.catchmentMeteoStationSelection1.Margin = new System.Windows.Forms.Padding(5);
            this.catchmentMeteoStationSelection1.MeteoStations = null;
            this.catchmentMeteoStationSelection1.Name = "catchmentMeteoStationSelection1";
            this.catchmentMeteoStationSelection1.Size = new System.Drawing.Size(828, 462);
            this.catchmentMeteoStationSelection1.TabIndex = 0;
            this.catchmentMeteoStationSelection1.UseMeteoStations = false;
            // 
            // waterlevelTab
            // 
            this.waterlevelTab.Controls.Add(this.rrBoundarySeriesView1);
            this.waterlevelTab.Controls.Add(this.rrBoundaryLinkPanel);
            this.waterlevelTab.Location = new System.Drawing.Point(4, 25);
            this.waterlevelTab.Margin = new System.Windows.Forms.Padding(4);
            this.waterlevelTab.Name = "waterlevelTab";
            this.waterlevelTab.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.waterlevelTab.Size = new System.Drawing.Size(828, 462);
            this.waterlevelTab.TabIndex = 6;
            this.waterlevelTab.Text = "Boundary Waterlevel";
            this.waterlevelTab.UseVisualStyleBackColor = true;
            // 
            // rrBoundarySeriesView1
            // 
            this.rrBoundarySeriesView1.Data = null;
            this.rrBoundarySeriesView1.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourceUnpavedViewModel, "EnableWaterLevelForm", true));
            this.rrBoundarySeriesView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rrBoundarySeriesView1.Image = null;
            this.rrBoundarySeriesView1.Location = new System.Drawing.Point(7, 84);
            this.rrBoundarySeriesView1.Margin = new System.Windows.Forms.Padding(5);
            this.rrBoundarySeriesView1.Name = "rrBoundarySeriesView1";
            this.rrBoundarySeriesView1.Size = new System.Drawing.Size(814, 372);
            this.rrBoundarySeriesView1.TabIndex = 0;
            this.rrBoundarySeriesView1.ViewInfo = null;
            // 
            // rrBoundaryLinkPanel
            // 
            this.rrBoundaryLinkPanel.AutoSize = true;
            this.rrBoundaryLinkPanel.BackColor = System.Drawing.Color.Transparent;
            this.rrBoundaryLinkPanel.Data = null;
            this.rrBoundaryLinkPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.rrBoundaryLinkPanel.Image = null;
            this.rrBoundaryLinkPanel.Location = new System.Drawing.Point(7, 6);
            this.rrBoundaryLinkPanel.Margin = new System.Windows.Forms.Padding(4);
            this.rrBoundaryLinkPanel.Name = "rrBoundaryLinkPanel";
            this.rrBoundaryLinkPanel.Size = new System.Drawing.Size(814, 78);
            this.rrBoundaryLinkPanel.TabIndex = 1;
            this.rrBoundaryLinkPanel.ViewInfo = null;
            // 
            // UnpavedDataView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.unpavedTabControl);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "UnpavedDataView";
            this.Size = new System.Drawing.Size(836, 491);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceUnpavedViewModel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceUnpaved)).EndInit();
            this.drainageComboPanel.ResumeLayout(false);
            this.drainageComboPanel.PerformLayout();
            this.groundWaterAreaPanel.ResumeLayout(false);
            this.groundWaterAreaPanel.PerformLayout();
            this.cropsHeaderPanel.ResumeLayout(false);
            this.cropsHeaderPanel.PerformLayout();
            this.unpavedTabControl.ResumeLayout(false);
            this.cropsTab.ResumeLayout(false);
            this.surfaceSoilTab.ResumeLayout(false);
            this.surfaceSoilTab.PerformLayout();
            this.groundwaterTab.ResumeLayout(false);
            this.groundwaterTab.PerformLayout();
            this.storageInfiltrationTab.ResumeLayout(false);
            this.storageInfiltrationTab.PerformLayout();
            this.drainageTab.ResumeLayout(false);
            this.seepageTab.ResumeLayout(false);
            this.seepageTab.PerformLayout();
            this.meteoTab.ResumeLayout(false);
            this.waterlevelTab.ResumeLayout(false);
            this.waterlevelTab.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TextBox surfaceLevel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblAreaPerCropType;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox groundwaterThickness;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox maximumGroundwaterLevel;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox groundwaterConstant;
        private System.Windows.Forms.RadioButton groundwaterSeriesRadio;
        private System.Windows.Forms.RadioButton groundwaterConstantRadio;
        private System.Windows.Forms.RadioButton groundwaterLinkedNodeRadio;
        private System.Windows.Forms.TextBox initialLandStorage;
        private System.Windows.Forms.TextBox maximumLandStorage;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox infiltrationCapacity;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.RadioButton seepageConstantRadio;
        private System.Windows.Forms.TextBox seepage;
        private System.Windows.Forms.RadioButton seepageSeriesRadio;
        private System.Windows.Forms.RadioButton seepageH0SeriesRadio;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Button seepageH0SeriesButton;
        private System.Windows.Forms.Button seepageSeriesButton;
        private System.Windows.Forms.BindingSource bindingSourceUnpaved;
        private System.Windows.Forms.Button groundwaterSeriesButton;
        private System.Windows.Forms.BindingSource bindingSourceUnpavedViewModel;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.TextBox seepageHydraulicResistance;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Panel drainageComboPanel;
        private System.Windows.Forms.Panel drainagePanel;
        private System.Windows.Forms.Label unitLabel;
        private System.Windows.Forms.TextBox groundwaterArea;
        private System.Windows.Forms.CheckBox differentGroundwaterAreaCheckBox;
        private System.Windows.Forms.Label label27;
        private AreaDictionaryEditor areaDictionaryEditor;
        private System.Windows.Forms.Panel cropsHeaderPanel;
        private System.Windows.Forms.Panel groundWaterAreaPanel;
        private System.Windows.Forms.TabControl unpavedTabControl;
        private System.Windows.Forms.TabPage cropsTab;
        private System.Windows.Forms.TabPage surfaceSoilTab;
        private System.Windows.Forms.TabPage groundwaterTab;
        private System.Windows.Forms.TabPage storageInfiltrationTab;
        private System.Windows.Forms.TabPage drainageTab;
        private System.Windows.Forms.TabPage seepageTab;
        private System.Windows.Forms.TabPage waterlevelTab;
        private DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls.RRBoundarySeriesView rrBoundarySeriesView1;
        private System.Windows.Forms.Label lblCapsimSoilType;
        private System.Windows.Forms.Label lblComment;
        private System.Windows.Forms.TabPage meteoTab;
        private CatchmentMeteoStationSelection catchmentMeteoStationSelection1;
        private DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls.RRBoundaryLinkPanel rrBoundaryLinkPanel;
        private BindableComboBox soilTypeComboBox;
        private BindableComboBox storageUnitComboBox;
        private BindableComboBox infiltrationUnitComboBox;
        private BindableComboBox drainageComboBox;
        private BindableComboBox capsimSoilTypeComboBox;

    }
}
