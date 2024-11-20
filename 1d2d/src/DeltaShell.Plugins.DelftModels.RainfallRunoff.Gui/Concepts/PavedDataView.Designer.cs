using DelftTools.Controls.Swf;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    partial class PavedDataView
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
            this.dryWeatherFlowOptionsGroupBox = new System.Windows.Forms.GroupBox();
            this.dryWeatherFlowOptionsTypeComboBox = new BindableComboBox();
            this.bindingSourcePaved = new System.Windows.Forms.BindingSource(this.components);
            this.lblDryWeatherFlowOptionsType = new System.Windows.Forms.Label();
            this.variableWaterUseFunctionButton = new System.Windows.Forms.Button();
            this.waterUseComboBox = new BindableComboBox();
            this.bindingSourcePavedViewModel = new System.Windows.Forms.BindingSource(this.components);
            this.waterUse = new System.Windows.Forms.TextBox();
            this.lblWaterUse = new System.Windows.Forms.Label();
            this.lblNumberOfInhabitants = new System.Windows.Forms.Label();
            this.numberOfInhabitants = new System.Windows.Forms.TextBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.storageUnitComboBox = new BindableComboBox();
            this.initialRoofStorage = new System.Windows.Forms.TextBox();
            this.maximumRoofStorage = new System.Windows.Forms.TextBox();
            this.lblStorageInitial = new System.Windows.Forms.Label();
            this.lblStorageMaximum = new System.Windows.Forms.Label();
            this.lblOnRoof = new System.Windows.Forms.Label();
            this.sewerTypeComboBox = new BindableComboBox();
            this.lblSewerType = new System.Windows.Forms.Label();
            this.mixedAndOrRainfallSewerPumpDischargeTargetGroupBox = new System.Windows.Forms.GroupBox();
            this.dryWeatherFlowPumpDischargeTargetcomboBox = new BindableComboBox();
            this.lblDryWeatherFlowPumpDischargeTarget = new System.Windows.Forms.Label();
            this.mixedAndOrRainfallPumpDischargeTargetcomboBox = new BindableComboBox();
            this.lblMixedAndOrRainfallPumpDischargeTarget = new System.Windows.Forms.Label();
            this.sewerPumpGroupBox = new System.Windows.Forms.GroupBox();
            this.lblUnit = new System.Windows.Forms.Label();
            this.dwfVariableCapacityButton = new System.Windows.Forms.Button();
            this.mixedVariableCapacityButton = new System.Windows.Forms.Button();
            this.sewerCapacityDryWeatherFlow = new System.Windows.Forms.TextBox();
            this.sewerCapacityMixedAndOrRainfall = new System.Windows.Forms.TextBox();
            this.lblCapacityDryWeatherFlow = new System.Windows.Forms.Label();
            this.lblCapacityMixedAndOrRainfall = new System.Windows.Forms.Label();
            this.sewerPumpCapacityUnitComboBox = new BindableComboBox();
            this.rbFixedCapacity = new System.Windows.Forms.RadioButton();
            this.rbVariableCapacity = new System.Windows.Forms.RadioButton();
            this.lblLevelUnit = new System.Windows.Forms.Label();
            this.lblLevel = new System.Windows.Forms.Label();
            this.surfaceLevel = new System.Windows.Forms.TextBox();
            this.lblAreaUnit = new System.Windows.Forms.Label();
            this.definitionOfSpillingGroupBox = new System.Windows.Forms.GroupBox();
            this.lblRunoffCoefficientUnit = new System.Windows.Forms.Label();
            this.runoffCoefficient = new System.Windows.Forms.TextBox();
            this.rbNoDelay = new System.Windows.Forms.RadioButton();
            this.rbUseRunoffCoefficient = new System.Windows.Forms.RadioButton();
            this.lblRunoffArea = new System.Windows.Forms.Label();
            this.runoffArea = new System.Windows.Forms.TextBox();
            this.pavedTabControl = new System.Windows.Forms.TabControl();
            this.generalTab = new System.Windows.Forms.TabPage();
            this.managementTab = new System.Windows.Forms.TabPage();
            this.storageTab = new System.Windows.Forms.TabPage();
            this.dryWeatherFlowTab = new System.Windows.Forms.TabPage();
            this.meteoTab = new System.Windows.Forms.TabPage();
            this.catchmentMeteoStationSelection1 = new DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.CatchmentMeteoStationSelection();
            this.dryWeatherFlowOptionsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourcePaved)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourcePavedViewModel)).BeginInit();
            this.mixedAndOrRainfallSewerPumpDischargeTargetGroupBox.SuspendLayout();
            this.sewerPumpGroupBox.SuspendLayout();
            this.definitionOfSpillingGroupBox.SuspendLayout();
            this.pavedTabControl.SuspendLayout();
            this.generalTab.SuspendLayout();
            this.managementTab.SuspendLayout();
            this.storageTab.SuspendLayout();
            this.dryWeatherFlowTab.SuspendLayout();
            this.meteoTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // dryWeatherFlowOptionsGroupBox
            // 
            this.dryWeatherFlowOptionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dryWeatherFlowOptionsGroupBox.Controls.Add(this.dryWeatherFlowOptionsTypeComboBox);
            this.dryWeatherFlowOptionsGroupBox.Controls.Add(this.lblDryWeatherFlowOptionsType);
            this.dryWeatherFlowOptionsGroupBox.Controls.Add(this.variableWaterUseFunctionButton);
            this.dryWeatherFlowOptionsGroupBox.Controls.Add(this.waterUseComboBox);
            this.dryWeatherFlowOptionsGroupBox.Controls.Add(this.waterUse);
            this.dryWeatherFlowOptionsGroupBox.Controls.Add(this.lblWaterUse);
            this.dryWeatherFlowOptionsGroupBox.Location = new System.Drawing.Point(28, 48);
            this.dryWeatherFlowOptionsGroupBox.Name = "dryWeatherFlowOptionsGroupBox";
            this.dryWeatherFlowOptionsGroupBox.Padding = new System.Windows.Forms.Padding(8);
            this.dryWeatherFlowOptionsGroupBox.Size = new System.Drawing.Size(665, 79);
            this.dryWeatherFlowOptionsGroupBox.TabIndex = 21;
            this.dryWeatherFlowOptionsGroupBox.TabStop = false;
            this.dryWeatherFlowOptionsGroupBox.Text = "Options";
            // 
            // dryWeatherFlowOptionsTypeComboBox
            // 
            this.dryWeatherFlowOptionsTypeComboBox.DataBindings.Add(new System.Windows.Forms.Binding("SelectedItem", this.bindingSourcePaved, "DryWeatherFlowOptions", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dryWeatherFlowOptionsTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dryWeatherFlowOptionsTypeComboBox.FormattingEnabled = true;
            this.dryWeatherFlowOptionsTypeComboBox.Location = new System.Drawing.Point(123, 22);
            this.dryWeatherFlowOptionsTypeComboBox.Name = "dryWeatherFlowOptionsTypeComboBox";
            this.dryWeatherFlowOptionsTypeComboBox.Size = new System.Drawing.Size(167, 21);
            this.dryWeatherFlowOptionsTypeComboBox.TabIndex = 25;
            this.dryWeatherFlowOptionsTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.dryWeatherFlowOptionsTypeComboBox_SelectedIndexChanged);
            // 
            // bindingSourcePaved
            // 
            this.bindingSourcePaved.DataSource = typeof(DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.PavedData);
            // 
            // lblDryWeatherFlowOptionsType
            // 
            this.lblDryWeatherFlowOptionsType.AutoSize = true;
            this.lblDryWeatherFlowOptionsType.Location = new System.Drawing.Point(8, 25);
            this.lblDryWeatherFlowOptionsType.Name = "lblDryWeatherFlowOptionsType";
            this.lblDryWeatherFlowOptionsType.Size = new System.Drawing.Size(31, 13);
            this.lblDryWeatherFlowOptionsType.TabIndex = 24;
            this.lblDryWeatherFlowOptionsType.Text = "Type";
            // 
            // variableWaterUseFunctionButton
            // 
            this.variableWaterUseFunctionButton.Location = new System.Drawing.Point(303, 49);
            this.variableWaterUseFunctionButton.Name = "variableWaterUseFunctionButton";
            this.variableWaterUseFunctionButton.Size = new System.Drawing.Size(60, 20);
            this.variableWaterUseFunctionButton.TabIndex = 23;
            this.variableWaterUseFunctionButton.Text = "...";
            this.variableWaterUseFunctionButton.UseVisualStyleBackColor = true;
            this.variableWaterUseFunctionButton.Click += new System.EventHandler(this.variableWaterUseFunctionButtonClick);
            // 
            // waterUseComboBox
            // 
            this.waterUseComboBox.DataBindings.Add(new System.Windows.Forms.Binding("SelectedItem", this.bindingSourcePavedViewModel, "WaterUseUnit", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.waterUseComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.waterUseComboBox.FormattingEnabled = true;
            this.waterUseComboBox.Location = new System.Drawing.Point(211, 48);
            this.waterUseComboBox.Name = "waterUseComboBox";
            this.waterUseComboBox.Size = new System.Drawing.Size(79, 21);
            this.waterUseComboBox.TabIndex = 21;
            // 
            // bindingSourcePavedViewModel
            // 
            this.bindingSourcePavedViewModel.DataSource = typeof(DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.PavedDataViewModel);
            // 
            // waterUse
            // 
            this.waterUse.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePavedViewModel, "WaterUse", true));
            this.waterUse.Location = new System.Drawing.Point(135, 49);
            this.waterUse.Name = "waterUse";
            this.waterUse.Size = new System.Drawing.Size(70, 20);
            this.waterUse.TabIndex = 20;
            // 
            // lblWaterUse
            // 
            this.lblWaterUse.AutoSize = true;
            this.lblWaterUse.Location = new System.Drawing.Point(8, 53);
            this.lblWaterUse.Name = "lblWaterUse";
            this.lblWaterUse.Size = new System.Drawing.Size(62, 13);
            this.lblWaterUse.TabIndex = 19;
            this.lblWaterUse.Text = "water use...";
            // 
            // lblNumberOfInhabitants
            // 
            this.lblNumberOfInhabitants.AutoSize = true;
            this.lblNumberOfInhabitants.Location = new System.Drawing.Point(28, 25);
            this.lblNumberOfInhabitants.Name = "lblNumberOfInhabitants";
            this.lblNumberOfInhabitants.Size = new System.Drawing.Size(110, 13);
            this.lblNumberOfInhabitants.TabIndex = 4;
            this.lblNumberOfInhabitants.Text = "Number of inhabitants";
            // 
            // numberOfInhabitants
            // 
            this.numberOfInhabitants.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePaved, "NumberOfInhabitants", true));
            this.numberOfInhabitants.Location = new System.Drawing.Point(151, 22);
            this.numberOfInhabitants.Name = "numberOfInhabitants";
            this.numberOfInhabitants.Size = new System.Drawing.Size(70, 20);
            this.numberOfInhabitants.TabIndex = 1;
            // 
            // textBox4
            // 
            this.textBox4.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePavedViewModel, "InitialSewerDryWeatherFlowStorage", true));
            this.textBox4.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourcePavedViewModel, "SewerTypeIsNotMixed", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.textBox4.Location = new System.Drawing.Point(248, 92);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(70, 20);
            this.textBox4.TabIndex = 10;
            // 
            // textBox5
            // 
            this.textBox5.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePavedViewModel, "MaximumSewerDryWeatherFlowStorage", true));
            this.textBox5.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourcePavedViewModel, "SewerTypeIsNotMixed", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.textBox5.Location = new System.Drawing.Point(172, 92);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(70, 20);
            this.textBox5.TabIndex = 9;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(28, 95);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(133, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "In sewer (dry weather flow)";
            // 
            // textBox2
            // 
            this.textBox2.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePavedViewModel, "InitialSewerMixedAndOrRainfallStorage", true));
            this.textBox2.Location = new System.Drawing.Point(248, 66);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(70, 20);
            this.textBox2.TabIndex = 7;
            // 
            // textBox3
            // 
            this.textBox3.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePavedViewModel, "MaximumSewerMixedAndOrRainfallStorage", true));
            this.textBox3.Location = new System.Drawing.Point(172, 66);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(70, 20);
            this.textBox3.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(28, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(118, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "In sewer (mixed/rainfall)";
            // 
            // storageUnitComboBox
            // 
            this.storageUnitComboBox.DataBindings.Add(new System.Windows.Forms.Binding("SelectedItem", this.bindingSourcePavedViewModel, "StorageUnit", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.storageUnitComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.storageUnitComboBox.FormattingEnabled = true;
            this.storageUnitComboBox.Location = new System.Drawing.Point(324, 41);
            this.storageUnitComboBox.Name = "storageUnitComboBox";
            this.storageUnitComboBox.Size = new System.Drawing.Size(79, 21);
            this.storageUnitComboBox.TabIndex = 4;
            // 
            // initialRoofStorage
            // 
            this.initialRoofStorage.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePavedViewModel, "InitialStreetStorage", true));
            this.initialRoofStorage.Location = new System.Drawing.Point(248, 42);
            this.initialRoofStorage.Name = "initialRoofStorage";
            this.initialRoofStorage.Size = new System.Drawing.Size(70, 20);
            this.initialRoofStorage.TabIndex = 3;
            // 
            // maximumRoofStorage
            // 
            this.maximumRoofStorage.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePavedViewModel, "MaximumStreetStorage", true));
            this.maximumRoofStorage.Location = new System.Drawing.Point(172, 42);
            this.maximumRoofStorage.Name = "maximumRoofStorage";
            this.maximumRoofStorage.Size = new System.Drawing.Size(70, 20);
            this.maximumRoofStorage.TabIndex = 2;
            // 
            // lblStorageInitial
            // 
            this.lblStorageInitial.AutoSize = true;
            this.lblStorageInitial.Location = new System.Drawing.Point(247, 14);
            this.lblStorageInitial.Name = "lblStorageInitial";
            this.lblStorageInitial.Size = new System.Drawing.Size(31, 13);
            this.lblStorageInitial.TabIndex = 1;
            this.lblStorageInitial.Text = "Initial";
            // 
            // lblStorageMaximum
            // 
            this.lblStorageMaximum.AutoSize = true;
            this.lblStorageMaximum.Location = new System.Drawing.Point(171, 14);
            this.lblStorageMaximum.Name = "lblStorageMaximum";
            this.lblStorageMaximum.Size = new System.Drawing.Size(51, 13);
            this.lblStorageMaximum.TabIndex = 1;
            this.lblStorageMaximum.Text = "Maximum";
            // 
            // lblOnRoof
            // 
            this.lblOnRoof.AutoSize = true;
            this.lblOnRoof.Location = new System.Drawing.Point(28, 45);
            this.lblOnRoof.Name = "lblOnRoof";
            this.lblOnRoof.Size = new System.Drawing.Size(50, 13);
            this.lblOnRoof.TabIndex = 1;
            this.lblOnRoof.Text = "On street";
            // 
            // sewerTypeComboBox
            // 
            this.sewerTypeComboBox.DataBindings.Add(new System.Windows.Forms.Binding("SelectedItem", this.bindingSourcePaved, "SewerType", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.sewerTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sewerTypeComboBox.FormattingEnabled = true;
            this.sewerTypeComboBox.Location = new System.Drawing.Point(151, 22);
            this.sewerTypeComboBox.Name = "sewerTypeComboBox";
            this.sewerTypeComboBox.Size = new System.Drawing.Size(155, 21);
            this.sewerTypeComboBox.TabIndex = 23;
            // 
            // lblSewerType
            // 
            this.lblSewerType.AutoSize = true;
            this.lblSewerType.Location = new System.Drawing.Point(28, 25);
            this.lblSewerType.Name = "lblSewerType";
            this.lblSewerType.Size = new System.Drawing.Size(60, 13);
            this.lblSewerType.TabIndex = 22;
            this.lblSewerType.Text = "Sewer type";
            // 
            // mixedAndOrRainfallSewerPumpDischargeTargetGroupBox
            // 
            this.mixedAndOrRainfallSewerPumpDischargeTargetGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mixedAndOrRainfallSewerPumpDischargeTargetGroupBox.Controls.Add(this.dryWeatherFlowPumpDischargeTargetcomboBox);
            this.mixedAndOrRainfallSewerPumpDischargeTargetGroupBox.Controls.Add(this.lblDryWeatherFlowPumpDischargeTarget);
            this.mixedAndOrRainfallSewerPumpDischargeTargetGroupBox.Controls.Add(this.mixedAndOrRainfallPumpDischargeTargetcomboBox);
            this.mixedAndOrRainfallSewerPumpDischargeTargetGroupBox.Controls.Add(this.lblMixedAndOrRainfallPumpDischargeTarget);
            this.mixedAndOrRainfallSewerPumpDischargeTargetGroupBox.Location = new System.Drawing.Point(28, 188);
            this.mixedAndOrRainfallSewerPumpDischargeTargetGroupBox.Name = "mixedAndOrRainfallSewerPumpDischargeTargetGroupBox";
            this.mixedAndOrRainfallSewerPumpDischargeTargetGroupBox.Padding = new System.Windows.Forms.Padding(8);
            this.mixedAndOrRainfallSewerPumpDischargeTargetGroupBox.Size = new System.Drawing.Size(631, 90);
            this.mixedAndOrRainfallSewerPumpDischargeTargetGroupBox.TabIndex = 20;
            this.mixedAndOrRainfallSewerPumpDischargeTargetGroupBox.TabStop = false;
            this.mixedAndOrRainfallSewerPumpDischargeTargetGroupBox.Text = "Pump discharge targets";
            // 
            // dryWeatherFlowPumpDischargeTargetcomboBox
            // 
            this.dryWeatherFlowPumpDischargeTargetcomboBox.DataBindings.Add(new System.Windows.Forms.Binding("SelectedItem", this.bindingSourcePaved, "DryWeatherFlowSewerPumpDischarge", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.dryWeatherFlowPumpDischargeTargetcomboBox.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourcePavedViewModel, "SewerTypeIsNotMixed", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.dryWeatherFlowPumpDischargeTargetcomboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dryWeatherFlowPumpDischargeTargetcomboBox.FormattingEnabled = true;
            this.dryWeatherFlowPumpDischargeTargetcomboBox.Location = new System.Drawing.Point(164, 51);
            this.dryWeatherFlowPumpDischargeTargetcomboBox.Name = "dryWeatherFlowPumpDischargeTargetcomboBox";
            this.dryWeatherFlowPumpDischargeTargetcomboBox.Size = new System.Drawing.Size(174, 21);
            this.dryWeatherFlowPumpDischargeTargetcomboBox.TabIndex = 26;
            // 
            // lblDryWeatherFlowPumpDischargeTarget
            // 
            this.lblDryWeatherFlowPumpDischargeTarget.AutoSize = true;
            this.lblDryWeatherFlowPumpDischargeTarget.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourcePavedViewModel, "SewerTypeIsNotMixed", true));
            this.lblDryWeatherFlowPumpDischargeTarget.Location = new System.Drawing.Point(12, 54);
            this.lblDryWeatherFlowPumpDischargeTarget.Name = "lblDryWeatherFlowPumpDischargeTarget";
            this.lblDryWeatherFlowPumpDischargeTarget.Size = new System.Drawing.Size(86, 13);
            this.lblDryWeatherFlowPumpDischargeTarget.TabIndex = 25;
            this.lblDryWeatherFlowPumpDischargeTarget.Text = "Dry weather flow";
            // 
            // mixedAndOrRainfallPumpDischargeTargetcomboBox
            // 
            this.mixedAndOrRainfallPumpDischargeTargetcomboBox.DataBindings.Add(new System.Windows.Forms.Binding("SelectedItem", this.bindingSourcePaved, "MixedAndOrRainfallSewerPumpDischarge", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.mixedAndOrRainfallPumpDischargeTargetcomboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.mixedAndOrRainfallPumpDischargeTargetcomboBox.FormattingEnabled = true;
            this.mixedAndOrRainfallPumpDischargeTargetcomboBox.Location = new System.Drawing.Point(164, 24);
            this.mixedAndOrRainfallPumpDischargeTargetcomboBox.Name = "mixedAndOrRainfallPumpDischargeTargetcomboBox";
            this.mixedAndOrRainfallPumpDischargeTargetcomboBox.Size = new System.Drawing.Size(174, 21);
            this.mixedAndOrRainfallPumpDischargeTargetcomboBox.TabIndex = 24;
            // 
            // lblMixedAndOrRainfallPumpDischargeTarget
            // 
            this.lblMixedAndOrRainfallPumpDischargeTarget.AutoSize = true;
            this.lblMixedAndOrRainfallPumpDischargeTarget.Location = new System.Drawing.Point(13, 27);
            this.lblMixedAndOrRainfallPumpDischargeTarget.Name = "lblMixedAndOrRainfallPumpDischargeTarget";
            this.lblMixedAndOrRainfallPumpDischargeTarget.Size = new System.Drawing.Size(70, 13);
            this.lblMixedAndOrRainfallPumpDischargeTarget.TabIndex = 19;
            this.lblMixedAndOrRainfallPumpDischargeTarget.Text = "Mixed/rainfall";
            // 
            // sewerPumpGroupBox
            // 
            this.sewerPumpGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sewerPumpGroupBox.Controls.Add(this.lblUnit);
            this.sewerPumpGroupBox.Controls.Add(this.dwfVariableCapacityButton);
            this.sewerPumpGroupBox.Controls.Add(this.mixedVariableCapacityButton);
            this.sewerPumpGroupBox.Controls.Add(this.sewerCapacityDryWeatherFlow);
            this.sewerPumpGroupBox.Controls.Add(this.sewerCapacityMixedAndOrRainfall);
            this.sewerPumpGroupBox.Controls.Add(this.lblCapacityDryWeatherFlow);
            this.sewerPumpGroupBox.Controls.Add(this.lblCapacityMixedAndOrRainfall);
            this.sewerPumpGroupBox.Controls.Add(this.sewerPumpCapacityUnitComboBox);
            this.sewerPumpGroupBox.Controls.Add(this.rbFixedCapacity);
            this.sewerPumpGroupBox.Controls.Add(this.rbVariableCapacity);
            this.sewerPumpGroupBox.Location = new System.Drawing.Point(28, 49);
            this.sewerPumpGroupBox.Name = "sewerPumpGroupBox";
            this.sewerPumpGroupBox.Padding = new System.Windows.Forms.Padding(8);
            this.sewerPumpGroupBox.Size = new System.Drawing.Size(631, 133);
            this.sewerPumpGroupBox.TabIndex = 19;
            this.sewerPumpGroupBox.TabStop = false;
            this.sewerPumpGroupBox.Text = "Sewer pump";
            // 
            // lblUnit
            // 
            this.lblUnit.AutoSize = true;
            this.lblUnit.Location = new System.Drawing.Point(14, 104);
            this.lblUnit.Name = "lblUnit";
            this.lblUnit.Size = new System.Drawing.Size(26, 13);
            this.lblUnit.TabIndex = 23;
            this.lblUnit.Text = "Unit";
            // 
            // dwfVariableCapacityButton
            // 
            this.dwfVariableCapacityButton.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourcePavedViewModel, "SewerTypeIsNotMixedAndSewerPumpIsVariableCapacity", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.dwfVariableCapacityButton.Location = new System.Drawing.Point(268, 70);
            this.dwfVariableCapacityButton.Name = "dwfVariableCapacityButton";
            this.dwfVariableCapacityButton.Size = new System.Drawing.Size(70, 20);
            this.dwfVariableCapacityButton.TabIndex = 22;
            this.dwfVariableCapacityButton.Text = "...";
            this.dwfVariableCapacityButton.UseVisualStyleBackColor = true;
            this.dwfVariableCapacityButton.Click += new System.EventHandler(this.dwfVariableCapacityButton_Click);
            // 
            // mixedVariableCapacityButton
            // 
            this.mixedVariableCapacityButton.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourcePavedViewModel, "SewerPumpCapacityIsVariable", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.mixedVariableCapacityButton.Location = new System.Drawing.Point(268, 47);
            this.mixedVariableCapacityButton.Name = "mixedVariableCapacityButton";
            this.mixedVariableCapacityButton.Size = new System.Drawing.Size(70, 20);
            this.mixedVariableCapacityButton.TabIndex = 22;
            this.mixedVariableCapacityButton.Text = "...";
            this.mixedVariableCapacityButton.UseVisualStyleBackColor = true;
            this.mixedVariableCapacityButton.Click += new System.EventHandler(this.groundwaterSeriesButtonClick);
            // 
            // sewerCapacityDryWeatherFlow
            // 
            this.sewerCapacityDryWeatherFlow.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourcePavedViewModel, "SewerTypeIsNotMixedAndSewerPumpIsFixedCapacity", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.sewerCapacityDryWeatherFlow.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePavedViewModel, "CapacityDryWeatherFlow", true));
            this.sewerCapacityDryWeatherFlow.Location = new System.Drawing.Point(164, 70);
            this.sewerCapacityDryWeatherFlow.Name = "sewerCapacityDryWeatherFlow";
            this.sewerCapacityDryWeatherFlow.Size = new System.Drawing.Size(70, 20);
            this.sewerCapacityDryWeatherFlow.TabIndex = 21;
            // 
            // sewerCapacityMixedAndOrRainfall
            // 
            this.sewerCapacityMixedAndOrRainfall.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourcePaved, "IsSewerPumpCapacityFixed", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.sewerCapacityMixedAndOrRainfall.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePavedViewModel, "CapacityMixedAndOrRainfall", true));
            this.sewerCapacityMixedAndOrRainfall.Location = new System.Drawing.Point(164, 47);
            this.sewerCapacityMixedAndOrRainfall.Name = "sewerCapacityMixedAndOrRainfall";
            this.sewerCapacityMixedAndOrRainfall.Size = new System.Drawing.Size(70, 20);
            this.sewerCapacityMixedAndOrRainfall.TabIndex = 20;
            // 
            // lblCapacityDryWeatherFlow
            // 
            this.lblCapacityDryWeatherFlow.AutoSize = true;
            this.lblCapacityDryWeatherFlow.Location = new System.Drawing.Point(11, 73);
            this.lblCapacityDryWeatherFlow.Name = "lblCapacityDryWeatherFlow";
            this.lblCapacityDryWeatherFlow.Size = new System.Drawing.Size(134, 13);
            this.lblCapacityDryWeatherFlow.TabIndex = 19;
            this.lblCapacityDryWeatherFlow.Text = "Capacity (dry weather flow)";
            // 
            // lblCapacityMixedAndOrRainfall
            // 
            this.lblCapacityMixedAndOrRainfall.AutoSize = true;
            this.lblCapacityMixedAndOrRainfall.Location = new System.Drawing.Point(11, 50);
            this.lblCapacityMixedAndOrRainfall.Name = "lblCapacityMixedAndOrRainfall";
            this.lblCapacityMixedAndOrRainfall.Size = new System.Drawing.Size(119, 13);
            this.lblCapacityMixedAndOrRainfall.TabIndex = 18;
            this.lblCapacityMixedAndOrRainfall.Text = "Capacity (mixed/rainfall)";
            // 
            // sewerPumpCapacityUnitComboBox
            // 
            this.sewerPumpCapacityUnitComboBox.DataBindings.Add(new System.Windows.Forms.Binding("SelectedItem", this.bindingSourcePavedViewModel, "PumpCapacityUnit", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.sewerPumpCapacityUnitComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sewerPumpCapacityUnitComboBox.FormattingEnabled = true;
            this.sewerPumpCapacityUnitComboBox.Location = new System.Drawing.Point(51, 101);
            this.sewerPumpCapacityUnitComboBox.Name = "sewerPumpCapacityUnitComboBox";
            this.sewerPumpCapacityUnitComboBox.Size = new System.Drawing.Size(79, 21);
            this.sewerPumpCapacityUnitComboBox.TabIndex = 17;
            // 
            // rbFixedCapacity
            // 
            this.rbFixedCapacity.AutoSize = true;
            this.rbFixedCapacity.Checked = true;
            this.rbFixedCapacity.Location = new System.Drawing.Point(11, 24);
            this.rbFixedCapacity.Name = "rbFixedCapacity";
            this.rbFixedCapacity.Size = new System.Drawing.Size(93, 17);
            this.rbFixedCapacity.TabIndex = 15;
            this.rbFixedCapacity.TabStop = true;
            this.rbFixedCapacity.Text = "Fixed capacity";
            this.rbFixedCapacity.UseVisualStyleBackColor = true;
            this.rbFixedCapacity.CheckedChanged += new System.EventHandler(this.SewerPumpCapacityRadioCheckedChanged);
            // 
            // rbVariableCapacity
            // 
            this.rbVariableCapacity.AutoSize = true;
            this.rbVariableCapacity.Location = new System.Drawing.Point(244, 24);
            this.rbVariableCapacity.Name = "rbVariableCapacity";
            this.rbVariableCapacity.Size = new System.Drawing.Size(106, 17);
            this.rbVariableCapacity.TabIndex = 16;
            this.rbVariableCapacity.Text = "Variable capacity";
            this.rbVariableCapacity.UseVisualStyleBackColor = true;
            this.rbVariableCapacity.CheckedChanged += new System.EventHandler(this.SewerPumpCapacityRadioCheckedChanged);
            // 
            // lblLevelUnit
            // 
            this.lblLevelUnit.AutoSize = true;
            this.lblLevelUnit.Location = new System.Drawing.Point(227, 59);
            this.lblLevelUnit.Name = "lblLevelUnit";
            this.lblLevelUnit.Size = new System.Drawing.Size(33, 13);
            this.lblLevelUnit.TabIndex = 5;
            this.lblLevelUnit.Text = "m AD";
            // 
            // lblLevel
            // 
            this.lblLevel.AutoSize = true;
            this.lblLevel.Location = new System.Drawing.Point(28, 59);
            this.lblLevel.Name = "lblLevel";
            this.lblLevel.Size = new System.Drawing.Size(69, 13);
            this.lblLevel.TabIndex = 4;
            this.lblLevel.Text = "Surface level";
            // 
            // surfaceLevel
            // 
            this.surfaceLevel.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePaved, "SurfaceLevel", true));
            this.surfaceLevel.Location = new System.Drawing.Point(151, 56);
            this.surfaceLevel.Name = "surfaceLevel";
            this.surfaceLevel.Size = new System.Drawing.Size(70, 20);
            this.surfaceLevel.TabIndex = 1;
            // 
            // lblAreaUnit
            // 
            this.lblAreaUnit.AutoSize = true;
            this.lblAreaUnit.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePavedViewModel, "AreaUnitLabel", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.lblAreaUnit.Location = new System.Drawing.Point(227, 25);
            this.lblAreaUnit.Name = "lblAreaUnit";
            this.lblAreaUnit.Size = new System.Drawing.Size(24, 13);
            this.lblAreaUnit.TabIndex = 18;
            this.lblAreaUnit.Text = "unit";
            // 
            // definitionOfSpillingGroupBox
            // 
            this.definitionOfSpillingGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.definitionOfSpillingGroupBox.Controls.Add(this.lblRunoffCoefficientUnit);
            this.definitionOfSpillingGroupBox.Controls.Add(this.runoffCoefficient);
            this.definitionOfSpillingGroupBox.Controls.Add(this.rbNoDelay);
            this.definitionOfSpillingGroupBox.Controls.Add(this.rbUseRunoffCoefficient);
            this.definitionOfSpillingGroupBox.Location = new System.Drawing.Point(31, 91);
            this.definitionOfSpillingGroupBox.Name = "definitionOfSpillingGroupBox";
            this.definitionOfSpillingGroupBox.Padding = new System.Windows.Forms.Padding(8);
            this.definitionOfSpillingGroupBox.Size = new System.Drawing.Size(662, 78);
            this.definitionOfSpillingGroupBox.TabIndex = 17;
            this.definitionOfSpillingGroupBox.TabStop = false;
            this.definitionOfSpillingGroupBox.Text = "Spilling definition";
            // 
            // lblRunoffCoefficientUnit
            // 
            this.lblRunoffCoefficientUnit.AutoSize = true;
            this.lblRunoffCoefficientUnit.Location = new System.Drawing.Point(219, 49);
            this.lblRunoffCoefficientUnit.Name = "lblRunoffCoefficientUnit";
            this.lblRunoffCoefficientUnit.Size = new System.Drawing.Size(34, 13);
            this.lblRunoffCoefficientUnit.TabIndex = 18;
            this.lblRunoffCoefficientUnit.Text = "1/min";
            // 
            // runoffCoefficient
            // 
            this.runoffCoefficient.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePaved, "RunoffCoefficient", true));
            this.runoffCoefficient.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourcePavedViewModel, "SplittingDefinitionUseRunoffCoefficient", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.runoffCoefficient.Enabled = false;
            this.runoffCoefficient.Location = new System.Drawing.Point(143, 46);
            this.runoffCoefficient.Name = "runoffCoefficient";
            this.runoffCoefficient.Size = new System.Drawing.Size(70, 20);
            this.runoffCoefficient.TabIndex = 17;
            // 
            // rbNoDelay
            // 
            this.rbNoDelay.AutoSize = true;
            this.rbNoDelay.Checked = true;
            this.rbNoDelay.Location = new System.Drawing.Point(11, 24);
            this.rbNoDelay.Name = "rbNoDelay";
            this.rbNoDelay.Size = new System.Drawing.Size(67, 17);
            this.rbNoDelay.TabIndex = 15;
            this.rbNoDelay.TabStop = true;
            this.rbNoDelay.Text = "No delay";
            this.rbNoDelay.UseVisualStyleBackColor = true;
            this.rbNoDelay.CheckedChanged += new System.EventHandler(this.SplittingDefinitionRadioCheckedChanged);
            // 
            // rbUseRunoffCoefficient
            // 
            this.rbUseRunoffCoefficient.AutoSize = true;
            this.rbUseRunoffCoefficient.Location = new System.Drawing.Point(11, 47);
            this.rbUseRunoffCoefficient.Name = "rbUseRunoffCoefficient";
            this.rbUseRunoffCoefficient.Size = new System.Drawing.Size(126, 17);
            this.rbUseRunoffCoefficient.TabIndex = 16;
            this.rbUseRunoffCoefficient.Text = "Use runoff coefficient";
            this.rbUseRunoffCoefficient.UseVisualStyleBackColor = true;
            this.rbUseRunoffCoefficient.CheckedChanged += new System.EventHandler(this.SplittingDefinitionRadioCheckedChanged);
            // 
            // lblRunoffArea
            // 
            this.lblRunoffArea.AutoSize = true;
            this.lblRunoffArea.Location = new System.Drawing.Point(28, 25);
            this.lblRunoffArea.Name = "lblRunoffArea";
            this.lblRunoffArea.Size = new System.Drawing.Size(63, 13);
            this.lblRunoffArea.TabIndex = 7;
            this.lblRunoffArea.Text = "Runoff area";
            // 
            // runoffArea
            // 
            this.runoffArea.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePavedViewModel, "TotalAreaInUnit", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N2"));
            this.runoffArea.Location = new System.Drawing.Point(151, 22);
            this.runoffArea.Name = "runoffArea";
            this.runoffArea.Size = new System.Drawing.Size(70, 20);
            this.runoffArea.TabIndex = 6;
            // 
            // pavedTabControl
            // 
            this.pavedTabControl.Controls.Add(this.generalTab);
            this.pavedTabControl.Controls.Add(this.managementTab);
            this.pavedTabControl.Controls.Add(this.storageTab);
            this.pavedTabControl.Controls.Add(this.dryWeatherFlowTab);
            this.pavedTabControl.Controls.Add(this.meteoTab);
            this.pavedTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pavedTabControl.Location = new System.Drawing.Point(0, 0);
            this.pavedTabControl.Name = "pavedTabControl";
            this.pavedTabControl.SelectedIndex = 0;
            this.pavedTabControl.Size = new System.Drawing.Size(729, 334);
            this.pavedTabControl.TabIndex = 19;
            // 
            // generalTab
            // 
            this.generalTab.AutoScroll = true;
            this.generalTab.Controls.Add(this.lblLevelUnit);
            this.generalTab.Controls.Add(this.lblAreaUnit);
            this.generalTab.Controls.Add(this.lblLevel);
            this.generalTab.Controls.Add(this.surfaceLevel);
            this.generalTab.Controls.Add(this.definitionOfSpillingGroupBox);
            this.generalTab.Controls.Add(this.lblRunoffArea);
            this.generalTab.Controls.Add(this.runoffArea);
            this.generalTab.Location = new System.Drawing.Point(4, 22);
            this.generalTab.Name = "generalTab";
            this.generalTab.Padding = new System.Windows.Forms.Padding(25);
            this.generalTab.Size = new System.Drawing.Size(721, 308);
            this.generalTab.TabIndex = 0;
            this.generalTab.Text = "General";
            this.generalTab.UseVisualStyleBackColor = true;
            // 
            // managementTab
            // 
            this.managementTab.AutoScroll = true;
            this.managementTab.Controls.Add(this.sewerTypeComboBox);
            this.managementTab.Controls.Add(this.lblSewerType);
            this.managementTab.Controls.Add(this.mixedAndOrRainfallSewerPumpDischargeTargetGroupBox);
            this.managementTab.Controls.Add(this.sewerPumpGroupBox);
            this.managementTab.Location = new System.Drawing.Point(4, 22);
            this.managementTab.Name = "managementTab";
            this.managementTab.Padding = new System.Windows.Forms.Padding(25);
            this.managementTab.Size = new System.Drawing.Size(721, 308);
            this.managementTab.TabIndex = 2;
            this.managementTab.Text = "Management";
            this.managementTab.UseVisualStyleBackColor = true;
            // 
            // storageTab
            // 
            this.storageTab.AutoScroll = true;
            this.storageTab.Controls.Add(this.lblOnRoof);
            this.storageTab.Controls.Add(this.textBox4);
            this.storageTab.Controls.Add(this.storageUnitComboBox);
            this.storageTab.Controls.Add(this.lblStorageInitial);
            this.storageTab.Controls.Add(this.textBox5);
            this.storageTab.Controls.Add(this.textBox2);
            this.storageTab.Controls.Add(this.initialRoofStorage);
            this.storageTab.Controls.Add(this.textBox3);
            this.storageTab.Controls.Add(this.label2);
            this.storageTab.Controls.Add(this.lblStorageMaximum);
            this.storageTab.Controls.Add(this.label3);
            this.storageTab.Controls.Add(this.maximumRoofStorage);
            this.storageTab.Location = new System.Drawing.Point(4, 22);
            this.storageTab.Name = "storageTab";
            this.storageTab.Padding = new System.Windows.Forms.Padding(25);
            this.storageTab.Size = new System.Drawing.Size(721, 308);
            this.storageTab.TabIndex = 3;
            this.storageTab.Text = "Storage";
            this.storageTab.UseVisualStyleBackColor = true;
            // 
            // dryWeatherFlowTab
            // 
            this.dryWeatherFlowTab.AutoScroll = true;
            this.dryWeatherFlowTab.Controls.Add(this.dryWeatherFlowOptionsGroupBox);
            this.dryWeatherFlowTab.Controls.Add(this.lblNumberOfInhabitants);
            this.dryWeatherFlowTab.Controls.Add(this.numberOfInhabitants);
            this.dryWeatherFlowTab.Location = new System.Drawing.Point(4, 22);
            this.dryWeatherFlowTab.Name = "dryWeatherFlowTab";
            this.dryWeatherFlowTab.Padding = new System.Windows.Forms.Padding(25);
            this.dryWeatherFlowTab.Size = new System.Drawing.Size(721, 308);
            this.dryWeatherFlowTab.TabIndex = 4;
            this.dryWeatherFlowTab.Text = "Dry Weather Flow";
            this.dryWeatherFlowTab.UseVisualStyleBackColor = true;
            // 
            // meteoTab
            // 
            this.meteoTab.Controls.Add(this.catchmentMeteoStationSelection1);
            this.meteoTab.Location = new System.Drawing.Point(4, 22);
            this.meteoTab.Name = "meteoTab";
            this.meteoTab.Size = new System.Drawing.Size(721, 308);
            this.meteoTab.TabIndex = 5;
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
            this.catchmentMeteoStationSelection1.Size = new System.Drawing.Size(721, 308);
            this.catchmentMeteoStationSelection1.TabIndex = 0;
            this.catchmentMeteoStationSelection1.UseMeteoStations = false;
            // 
            // PavedDataView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.pavedTabControl);
            this.Name = "PavedDataView";
            this.Size = new System.Drawing.Size(729, 334);
            this.dryWeatherFlowOptionsGroupBox.ResumeLayout(false);
            this.dryWeatherFlowOptionsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourcePaved)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourcePavedViewModel)).EndInit();
            this.mixedAndOrRainfallSewerPumpDischargeTargetGroupBox.ResumeLayout(false);
            this.mixedAndOrRainfallSewerPumpDischargeTargetGroupBox.PerformLayout();
            this.sewerPumpGroupBox.ResumeLayout(false);
            this.sewerPumpGroupBox.PerformLayout();
            this.definitionOfSpillingGroupBox.ResumeLayout(false);
            this.definitionOfSpillingGroupBox.PerformLayout();
            this.pavedTabControl.ResumeLayout(false);
            this.generalTab.ResumeLayout(false);
            this.generalTab.PerformLayout();
            this.managementTab.ResumeLayout(false);
            this.managementTab.PerformLayout();
            this.storageTab.ResumeLayout(false);
            this.storageTab.PerformLayout();
            this.dryWeatherFlowTab.ResumeLayout(false);
            this.dryWeatherFlowTab.PerformLayout();
            this.meteoTab.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblRunoffArea;
        private System.Windows.Forms.TextBox runoffArea;
        private System.Windows.Forms.Label lblLevelUnit;
        private System.Windows.Forms.Label lblLevel;
        private System.Windows.Forms.TextBox surfaceLevel;
        private System.Windows.Forms.BindingSource bindingSourcePaved;
        private System.Windows.Forms.GroupBox definitionOfSpillingGroupBox;
        private System.Windows.Forms.Label lblRunoffCoefficientUnit;
        private System.Windows.Forms.TextBox runoffCoefficient;
        private System.Windows.Forms.RadioButton rbNoDelay;
        private System.Windows.Forms.RadioButton rbUseRunoffCoefficient;
        private System.Windows.Forms.GroupBox sewerPumpGroupBox;
        private System.Windows.Forms.TextBox sewerCapacityDryWeatherFlow;
        private System.Windows.Forms.TextBox sewerCapacityMixedAndOrRainfall;
        private System.Windows.Forms.Label lblCapacityDryWeatherFlow;
        private System.Windows.Forms.Label lblCapacityMixedAndOrRainfall;
        private System.Windows.Forms.ComboBox sewerPumpCapacityUnitComboBox;
        private System.Windows.Forms.RadioButton rbFixedCapacity;
        private System.Windows.Forms.RadioButton rbVariableCapacity;
        private System.Windows.Forms.GroupBox mixedAndOrRainfallSewerPumpDischargeTargetGroupBox;
        private System.Windows.Forms.Label lblUnit;
        private System.Windows.Forms.Button mixedVariableCapacityButton;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox storageUnitComboBox;
        private System.Windows.Forms.TextBox initialRoofStorage;
        private System.Windows.Forms.TextBox maximumRoofStorage;
        private System.Windows.Forms.Label lblStorageInitial;
        private System.Windows.Forms.Label lblStorageMaximum;
        private System.Windows.Forms.Label lblOnRoof;
        private System.Windows.Forms.GroupBox dryWeatherFlowOptionsGroupBox;
        private System.Windows.Forms.Label lblNumberOfInhabitants;
        private System.Windows.Forms.TextBox numberOfInhabitants;
        private System.Windows.Forms.ComboBox waterUseComboBox;
        private System.Windows.Forms.TextBox waterUse;
        private System.Windows.Forms.Label lblWaterUse;
        private System.Windows.Forms.Button variableWaterUseFunctionButton;
        private System.Windows.Forms.BindingSource bindingSourcePavedViewModel;
        private System.Windows.Forms.ComboBox sewerTypeComboBox;
        private System.Windows.Forms.Label lblSewerType;
        private System.Windows.Forms.ComboBox dryWeatherFlowPumpDischargeTargetcomboBox;
        private System.Windows.Forms.Label lblDryWeatherFlowPumpDischargeTarget;
        private System.Windows.Forms.ComboBox mixedAndOrRainfallPumpDischargeTargetcomboBox;
        private System.Windows.Forms.Label lblMixedAndOrRainfallPumpDischargeTarget;
        private System.Windows.Forms.ComboBox dryWeatherFlowOptionsTypeComboBox;
        private System.Windows.Forms.Label lblDryWeatherFlowOptionsType;
        private System.Windows.Forms.Label lblAreaUnit;
        private System.Windows.Forms.TabControl pavedTabControl;
        private System.Windows.Forms.TabPage generalTab;
        private System.Windows.Forms.TabPage managementTab;
        private System.Windows.Forms.TabPage storageTab;
        private System.Windows.Forms.TabPage dryWeatherFlowTab;
        private System.Windows.Forms.Button dwfVariableCapacityButton;
        private System.Windows.Forms.TabPage meteoTab;
        private CatchmentMeteoStationSelection catchmentMeteoStationSelection1;
    }
}
