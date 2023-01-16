namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    partial class WeirView
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.useVelocityHeightCheckBox = new System.Windows.Forms.CheckBox();
            this.bindingSourceWeir = new System.Windows.Forms.BindingSource(this.components);
            this.LowerEdgeLevelTimeDependentCheckBox = new System.Windows.Forms.CheckBox();
            this.OpenGateOpeningTimeSeriesButton = new System.Windows.Forms.Button();
            this.OpenLowerEdgeLevelTimeSeriesButton = new System.Windows.Forms.Button();
            this.OpenCrestLevelTimeSeriesButton = new System.Windows.Forms.Button();
            this.TimeDependentLabel = new System.Windows.Forms.Label();
            this.groupBoxGate = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.labelLowerEdgeLevel = new System.Windows.Forms.Label();
            this.textBoxLowerEdgeLevel = new System.Windows.Forms.TextBox();
            this.labelGateOpening = new System.Windows.Forms.Label();
            this.LowerEdgeLevelLabel = new System.Windows.Forms.Label();
            this.GateOpeningUnitLabel = new System.Windows.Forms.Label();
            this.textBoxGateOpening = new System.Windows.Forms.TextBox();
            this.GateHeightLabel = new System.Windows.Forms.Label();
            this.textBoxGateHeight = new System.Windows.Forms.TextBox();
            this.GateHeightUnitLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxWeirFormula = new System.Windows.Forms.ComboBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.textBoxMaxNeg = new System.Windows.Forms.TextBox();
            this.checkBoxMaxNeg = new System.Windows.Forms.CheckBox();
            this.labelUnitMaxNeg = new System.Windows.Forms.Label();
            this.textBoxMaxPos = new System.Windows.Forms.TextBox();
            this.checkBoxAllowPositiveFlow = new System.Windows.Forms.CheckBox();
            this.checkBoxMaxPos = new System.Windows.Forms.CheckBox();
            this.checkBoxAllowNegativeFlow = new System.Windows.Forms.CheckBox();
            this.labelFlowDirection = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.labelCrestLevel = new System.Windows.Forms.Label();
            this.textBoxCrestLevel = new System.Windows.Forms.TextBox();
            this.textBoxCrestWidth = new System.Windows.Forms.TextBox();
            this.labelCrestWidth = new System.Windows.Forms.Label();
            this.textBoxOffsetY = new System.Windows.Forms.TextBox();
            this.labelOffsetY = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.CrestLevelUnitLabel = new System.Windows.Forms.Label();
            this.CrestWidthUnitLabel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.labelGeometry = new System.Windows.Forms.Label();
            this.CrestLevelTimeDependentCheckBox = new System.Windows.Forms.CheckBox();
            this.labelUnitMaxPos = new System.Windows.Forms.Label();
            this.comboBoxCrestShape = new System.Windows.Forms.ComboBox();
            this.labelCrestShape = new System.Windows.Forms.Label();
            this.groupBoxFormula = new System.Windows.Forms.GroupBox();
            this.panelFormula = new System.Windows.Forms.Panel();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceWeir)).BeginInit();
            this.groupBoxGate.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBoxFormula.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.useVelocityHeightCheckBox);
            this.groupBox1.Controls.Add(this.LowerEdgeLevelTimeDependentCheckBox);
            this.groupBox1.Controls.Add(this.OpenGateOpeningTimeSeriesButton);
            this.groupBox1.Controls.Add(this.OpenLowerEdgeLevelTimeSeriesButton);
            this.groupBox1.Controls.Add(this.OpenCrestLevelTimeSeriesButton);
            this.groupBox1.Controls.Add(this.TimeDependentLabel);
            this.groupBox1.Controls.Add(this.groupBoxGate);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.comboBoxWeirFormula);
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Controls.Add(this.textBoxMaxNeg);
            this.groupBox1.Controls.Add(this.checkBoxMaxNeg);
            this.groupBox1.Controls.Add(this.labelUnitMaxNeg);
            this.groupBox1.Controls.Add(this.textBoxMaxPos);
            this.groupBox1.Controls.Add(this.checkBoxAllowPositiveFlow);
            this.groupBox1.Controls.Add(this.checkBoxMaxPos);
            this.groupBox1.Controls.Add(this.checkBoxAllowNegativeFlow);
            this.groupBox1.Controls.Add(this.labelFlowDirection);
            this.groupBox1.Controls.Add(this.tableLayoutPanel1);
            this.groupBox1.Controls.Add(this.labelUnitMaxPos);
            this.groupBox1.Controls.Add(this.comboBoxCrestShape);
            this.groupBox1.Controls.Add(this.labelCrestShape);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(568, 254);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Weir Properties";
            // 
            // useVelocityHeightCheckBox
            // 
            this.useVelocityHeightCheckBox.AutoSize = true;
            this.useVelocityHeightCheckBox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.bindingSourceWeir, "UseVelocityHeight", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.useVelocityHeightCheckBox.Location = new System.Drawing.Point(392, 89);
            this.useVelocityHeightCheckBox.Name = "useVelocityHeightCheckBox";
            this.useVelocityHeightCheckBox.Size = new System.Drawing.Size(116, 17);
            this.useVelocityHeightCheckBox.TabIndex = 41;
            this.useVelocityHeightCheckBox.Text = "Use velocity height";
            this.useVelocityHeightCheckBox.UseVisualStyleBackColor = true;
            // 
            // LowerEdgeLevelTimeDependentCheckBox
            // 
            this.LowerEdgeLevelTimeDependentCheckBox.AutoSize = true;
            this.LowerEdgeLevelTimeDependentCheckBox.Location = new System.Drawing.Point(306, 183);
            this.LowerEdgeLevelTimeDependentCheckBox.Name = "LowerEdgeLevelTimeDependentCheckBox";
            this.LowerEdgeLevelTimeDependentCheckBox.Size = new System.Drawing.Size(15, 14);
            this.LowerEdgeLevelTimeDependentCheckBox.TabIndex = 11;
            this.LowerEdgeLevelTimeDependentCheckBox.UseVisualStyleBackColor = true;
            this.LowerEdgeLevelTimeDependentCheckBox.CheckedChanged += new System.EventHandler(this.GateTimeDependentCheckBox_CheckedChanged);
            // 
            // OpenGateOpeningTimeSeriesButton
            // 
            this.OpenGateOpeningTimeSeriesButton.Location = new System.Drawing.Point(164, 191);
            this.OpenGateOpeningTimeSeriesButton.Name = "OpenGateOpeningTimeSeriesButton";
            this.OpenGateOpeningTimeSeriesButton.Size = new System.Drawing.Size(87, 23);
            this.OpenGateOpeningTimeSeriesButton.TabIndex = 40;
            this.OpenGateOpeningTimeSeriesButton.Text = "Time series...";
            this.OpenGateOpeningTimeSeriesButton.UseVisualStyleBackColor = true;
            this.OpenGateOpeningTimeSeriesButton.Click += new System.EventHandler(this.OpenGateOpeningTimeSeriesButton_Click);
            // 
            // OpenLowerEdgeLevelTimeSeriesButton
            // 
            this.OpenLowerEdgeLevelTimeSeriesButton.Location = new System.Drawing.Point(164, 166);
            this.OpenLowerEdgeLevelTimeSeriesButton.Name = "OpenLowerEdgeLevelTimeSeriesButton";
            this.OpenLowerEdgeLevelTimeSeriesButton.Size = new System.Drawing.Size(87, 23);
            this.OpenLowerEdgeLevelTimeSeriesButton.TabIndex = 39;
            this.OpenLowerEdgeLevelTimeSeriesButton.Text = "Time series...";
            this.OpenLowerEdgeLevelTimeSeriesButton.UseVisualStyleBackColor = true;
            this.OpenLowerEdgeLevelTimeSeriesButton.Click += new System.EventHandler(this.OpenLowerEdgeLevelTimeSeriesButton_Click);
            // 
            // OpenCrestLevelTimeSeriesButton
            // 
            this.OpenCrestLevelTimeSeriesButton.Location = new System.Drawing.Point(164, 70);
            this.OpenCrestLevelTimeSeriesButton.Name = "OpenCrestLevelTimeSeriesButton";
            this.OpenCrestLevelTimeSeriesButton.Size = new System.Drawing.Size(87, 23);
            this.OpenCrestLevelTimeSeriesButton.TabIndex = 37;
            this.OpenCrestLevelTimeSeriesButton.Text = "Time series...";
            this.OpenCrestLevelTimeSeriesButton.UseVisualStyleBackColor = true;
            this.OpenCrestLevelTimeSeriesButton.Click += new System.EventHandler(this.OpenCrestLevelTimeSeriesButton_Click);
            // 
            // TimeDependentLabel
            // 
            this.TimeDependentLabel.AutoSize = true;
            this.TimeDependentLabel.Location = new System.Drawing.Point(270, 48);
            this.TimeDependentLabel.Name = "TimeDependentLabel";
            this.TimeDependentLabel.Size = new System.Drawing.Size(84, 13);
            this.TimeDependentLabel.TabIndex = 36;
            this.TimeDependentLabel.Text = "Time dependent";
            // 
            // groupBoxGate
            // 
            this.groupBoxGate.Controls.Add(this.tableLayoutPanel2);
            this.groupBoxGate.Location = new System.Drawing.Point(6, 148);
            this.groupBoxGate.Name = "groupBoxGate";
            this.groupBoxGate.Size = new System.Drawing.Size(322, 99);
            this.groupBoxGate.TabIndex = 1;
            this.groupBoxGate.TabStop = false;
            this.groupBoxGate.Text = "Gate";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel2.Controls.Add(this.labelLowerEdgeLevel, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.textBoxLowerEdgeLevel, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.labelGateOpening, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.LowerEdgeLevelLabel, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.GateOpeningUnitLabel, 2, 1);
            this.tableLayoutPanel2.Controls.Add(this.textBoxGateOpening, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.GateHeightLabel, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.textBoxGateHeight, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.GateHeightUnitLabel, 2, 2);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(316, 80);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // labelLowerEdgeLevel
            // 
            this.labelLowerEdgeLevel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLowerEdgeLevel.AutoSize = true;
            this.labelLowerEdgeLevel.Location = new System.Drawing.Point(3, 6);
            this.labelLowerEdgeLevel.Name = "labelLowerEdgeLevel";
            this.labelLowerEdgeLevel.Size = new System.Drawing.Size(144, 13);
            this.labelLowerEdgeLevel.TabIndex = 0;
            this.labelLowerEdgeLevel.Text = "Lower edge level";
            // 
            // textBoxLowerEdgeLevel
            // 
            this.textBoxLowerEdgeLevel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLowerEdgeLevel.Location = new System.Drawing.Point(153, 3);
            this.textBoxLowerEdgeLevel.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
            this.textBoxLowerEdgeLevel.Name = "textBoxLowerEdgeLevel";
            this.textBoxLowerEdgeLevel.Size = new System.Drawing.Size(77, 20);
            this.textBoxLowerEdgeLevel.TabIndex = 1;
            this.textBoxLowerEdgeLevel.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxLowerEdgeLevel.Validated += new System.EventHandler(this.TextBoxLowerEdgeLevelValidated);
            // 
            // labelGateOpening
            // 
            this.labelGateOpening.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelGateOpening.AutoSize = true;
            this.labelGateOpening.Location = new System.Drawing.Point(3, 32);
            this.labelGateOpening.Name = "labelGateOpening";
            this.labelGateOpening.Size = new System.Drawing.Size(144, 13);
            this.labelGateOpening.TabIndex = 2;
            this.labelGateOpening.Text = "Gate opening";
            // 
            // LowerEdgeLevelLabel
            // 
            this.LowerEdgeLevelLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.LowerEdgeLevelLabel.AutoSize = true;
            this.LowerEdgeLevelLabel.Location = new System.Drawing.Point(253, 6);
            this.LowerEdgeLevelLabel.Name = "LowerEdgeLevelLabel";
            this.LowerEdgeLevelLabel.Size = new System.Drawing.Size(15, 13);
            this.LowerEdgeLevelLabel.TabIndex = 9;
            this.LowerEdgeLevelLabel.Text = "m";
            // 
            // GateOpeningUnitLabel
            // 
            this.GateOpeningUnitLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.GateOpeningUnitLabel.AutoSize = true;
            this.GateOpeningUnitLabel.Location = new System.Drawing.Point(253, 32);
            this.GateOpeningUnitLabel.Name = "GateOpeningUnitLabel";
            this.GateOpeningUnitLabel.Size = new System.Drawing.Size(15, 13);
            this.GateOpeningUnitLabel.TabIndex = 10;
            this.GateOpeningUnitLabel.Text = "m";
            // 
            // textBoxGateOpening
            // 
            this.textBoxGateOpening.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxGateOpening.ReadOnly = true;
            this.textBoxGateOpening.Location = new System.Drawing.Point(153, 29);
            this.textBoxGateOpening.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
            this.textBoxGateOpening.Name = "textBoxGateOpening";
            this.textBoxGateOpening.Size = new System.Drawing.Size(77, 20);
            this.textBoxGateOpening.TabIndex = 1;
            this.textBoxGateOpening.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxGateOpening.Validated += new System.EventHandler(this.TextBoxGateOpeningValidated);
            // 
            // GateHeightLabel
            // 
            this.GateHeightLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.GateHeightLabel.AutoSize = true;
            this.GateHeightLabel.Location = new System.Drawing.Point(3, 59);
            this.GateHeightLabel.Name = "GateHeightLabel";
            this.GateHeightLabel.Size = new System.Drawing.Size(62, 13);
            this.GateHeightLabel.TabIndex = 11;
            this.GateHeightLabel.Text = "Gate height";
            // 
            // textBoxGateHeight
            // 
            this.textBoxGateHeight.Location = new System.Drawing.Point(153, 55);
            this.textBoxGateHeight.Name = "textBoxGateHeight";
            this.textBoxGateHeight.Size = new System.Drawing.Size(78, 20);
            this.textBoxGateHeight.TabIndex = 12;
            this.textBoxGateHeight.Validated += new System.EventHandler(this.TextBoxGateHeightValidated);
            this.textBoxGateHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // GateHeightUnitLabel
            // 
            this.GateHeightUnitLabel.AutoSize = true;
            this.GateHeightUnitLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.GateHeightUnitLabel.Location = new System.Drawing.Point(253, 52);
            this.GateHeightUnitLabel.Name = "GateHeightUnitLabel";
            this.GateHeightUnitLabel.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.GateHeightUnitLabel.Size = new System.Drawing.Size(15, 28);
            this.GateHeightUnitLabel.TabIndex = 13;
            this.GateHeightUnitLabel.Text = "m";
            this.GateHeightUnitLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.AutoEllipsis = true;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Structure type";
            // 
            // comboBoxWeirFormula
            // 
            this.comboBoxWeirFormula.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxWeirFormula.FormattingEnabled = true;
            this.comboBoxWeirFormula.Location = new System.Drawing.Point(159, 19);
            this.comboBoxWeirFormula.Name = "comboBoxWeirFormula";
            this.comboBoxWeirFormula.Size = new System.Drawing.Size(289, 21);
            this.comboBoxWeirFormula.TabIndex = 4;
            this.comboBoxWeirFormula.SelectedIndexChanged += new System.EventHandler(this.ComboBoxWeirFormulaSelectedIndexChanged);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panel1.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.panel1.Location = new System.Drawing.Point(380, 48);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(4, 199);
            this.panel1.TabIndex = 35;
            // 
            // textBoxMaxNeg
            // 
            this.textBoxMaxNeg.Location = new System.Drawing.Point(460, 196);
            this.textBoxMaxNeg.Name = "textBoxMaxNeg";
            this.textBoxMaxNeg.Size = new System.Drawing.Size(35, 20);
            this.textBoxMaxNeg.TabIndex = 8;
            this.textBoxMaxNeg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxMaxNeg.Validated += new System.EventHandler(this.TextBoxMaxNegValidated);
            // 
            // checkBoxMaxNeg
            // 
            this.checkBoxMaxNeg.AutoSize = true;
            this.checkBoxMaxNeg.Location = new System.Drawing.Point(408, 199);
            this.checkBoxMaxNeg.Name = "checkBoxMaxNeg";
            this.checkBoxMaxNeg.Size = new System.Drawing.Size(46, 17);
            this.checkBoxMaxNeg.TabIndex = 9;
            this.checkBoxMaxNeg.Text = "Max";
            this.checkBoxMaxNeg.UseVisualStyleBackColor = true;
            this.checkBoxMaxNeg.CheckedChanged += new System.EventHandler(this.CheckBoxMaxNegCheckedChanged);
            // 
            // labelUnitMaxNeg
            // 
            this.labelUnitMaxNeg.AutoSize = true;
            this.labelUnitMaxNeg.Location = new System.Drawing.Point(501, 200);
            this.labelUnitMaxNeg.Name = "labelUnitMaxNeg";
            this.labelUnitMaxNeg.Size = new System.Drawing.Size(28, 13);
            this.labelUnitMaxNeg.TabIndex = 7;
            this.labelUnitMaxNeg.Text = "m³/s";
            // 
            // textBoxMaxPos
            // 
            this.textBoxMaxPos.Location = new System.Drawing.Point(460, 150);
            this.textBoxMaxPos.Name = "textBoxMaxPos";
            this.textBoxMaxPos.Size = new System.Drawing.Size(35, 20);
            this.textBoxMaxPos.TabIndex = 1;
            this.textBoxMaxPos.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxMaxPos.Validated += new System.EventHandler(this.TextBoxMaxPosValidated);
            // 
            // checkBoxAllowPositiveFlow
            // 
            this.checkBoxAllowPositiveFlow.AutoSize = true;
            this.checkBoxAllowPositiveFlow.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.bindingSourceWeir, "AllowPositiveFlow", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxAllowPositiveFlow.Location = new System.Drawing.Point(391, 130);
            this.checkBoxAllowPositiveFlow.Name = "checkBoxAllowPositiveFlow";
            this.checkBoxAllowPositiveFlow.Size = new System.Drawing.Size(63, 17);
            this.checkBoxAllowPositiveFlow.TabIndex = 2;
            this.checkBoxAllowPositiveFlow.Text = "Positive";
            this.checkBoxAllowPositiveFlow.UseVisualStyleBackColor = true;
            // 
            // checkBoxMaxPos
            // 
            this.checkBoxMaxPos.AutoSize = true;
            this.checkBoxMaxPos.Location = new System.Drawing.Point(408, 153);
            this.checkBoxMaxPos.Name = "checkBoxMaxPos";
            this.checkBoxMaxPos.Size = new System.Drawing.Size(46, 17);
            this.checkBoxMaxPos.TabIndex = 2;
            this.checkBoxMaxPos.Text = "Max";
            this.checkBoxMaxPos.UseVisualStyleBackColor = true;
            this.checkBoxMaxPos.CheckedChanged += new System.EventHandler(this.CheckBoxMaxPosCheckedChanged);
            // 
            // checkBoxAllowNegativeFlow
            // 
            this.checkBoxAllowNegativeFlow.AutoSize = true;
            this.checkBoxAllowNegativeFlow.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.bindingSourceWeir, "AllowNegativeFlow", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxAllowNegativeFlow.Location = new System.Drawing.Point(392, 176);
            this.checkBoxAllowNegativeFlow.Name = "checkBoxAllowNegativeFlow";
            this.checkBoxAllowNegativeFlow.Size = new System.Drawing.Size(69, 17);
            this.checkBoxAllowNegativeFlow.TabIndex = 2;
            this.checkBoxAllowNegativeFlow.Text = "Negative";
            this.checkBoxAllowNegativeFlow.UseVisualStyleBackColor = true;
            // 
            // labelFlowDirection
            // 
            this.labelFlowDirection.AutoSize = true;
            this.labelFlowDirection.Location = new System.Drawing.Point(388, 109);
            this.labelFlowDirection.Name = "labelFlowDirection";
            this.labelFlowDirection.Size = new System.Drawing.Size(109, 13);
            this.labelFlowDirection.TabIndex = 0;
            this.labelFlowDirection.Text = "Allowed flow direction";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel1.Controls.Add(this.labelCrestLevel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.textBoxCrestLevel, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.textBoxCrestWidth, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.labelCrestWidth, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.textBoxOffsetY, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.labelOffsetY, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label9, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.CrestLevelUnitLabel, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.CrestWidthUnitLabel, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.label4, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.labelGeometry, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.CrestLevelTimeDependentCheckBox, 3, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(9, 43);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(316, 99);
            this.tableLayoutPanel1.TabIndex = 6;
            // 
            // labelCrestLevel
            // 
            this.labelCrestLevel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCrestLevel.AutoSize = true;
            this.labelCrestLevel.Location = new System.Drawing.Point(3, 29);
            this.labelCrestLevel.Name = "labelCrestLevel";
            this.labelCrestLevel.Size = new System.Drawing.Size(144, 13);
            this.labelCrestLevel.TabIndex = 0;
            this.labelCrestLevel.Text = "Crest level";
            // 
            // textBoxCrestLevel
            // 
            this.textBoxCrestLevel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxCrestLevel.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceWeir, "CrestLevel", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxCrestLevel.Location = new System.Drawing.Point(153, 27);
            this.textBoxCrestLevel.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
            this.textBoxCrestLevel.Name = "textBoxCrestLevel";
            this.textBoxCrestLevel.Size = new System.Drawing.Size(77, 20);
            this.textBoxCrestLevel.TabIndex = 1;
            this.textBoxCrestLevel.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxCrestWidth
            // 
            this.textBoxCrestWidth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxCrestWidth.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceWeir, "CrestWidth", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxCrestWidth.Location = new System.Drawing.Point(153, 51);
            this.textBoxCrestWidth.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
            this.textBoxCrestWidth.Name = "textBoxCrestWidth";
            this.textBoxCrestWidth.Size = new System.Drawing.Size(77, 20);
            this.textBoxCrestWidth.TabIndex = 1;
            this.textBoxCrestWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // labelCrestWidth
            // 
            this.labelCrestWidth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCrestWidth.AutoSize = true;
            this.labelCrestWidth.Location = new System.Drawing.Point(3, 53);
            this.labelCrestWidth.Name = "labelCrestWidth";
            this.labelCrestWidth.Size = new System.Drawing.Size(144, 13);
            this.labelCrestWidth.TabIndex = 0;
            this.labelCrestWidth.Text = "Crest width";
            // 
            // textBoxOffsetY
            // 
            this.textBoxOffsetY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOffsetY.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceWeir, "OffsetY", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxOffsetY.Location = new System.Drawing.Point(153, 75);
            this.textBoxOffsetY.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
            this.textBoxOffsetY.Name = "textBoxOffsetY";
            this.textBoxOffsetY.ReadOnly = true;
            this.textBoxOffsetY.Size = new System.Drawing.Size(77, 20);
            this.textBoxOffsetY.TabIndex = 1;
            this.textBoxOffsetY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // labelOffsetY
            // 
            this.labelOffsetY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelOffsetY.AutoSize = true;
            this.labelOffsetY.Location = new System.Drawing.Point(3, 79);
            this.labelOffsetY.Name = "labelOffsetY";
            this.labelOffsetY.Size = new System.Drawing.Size(144, 13);
            this.labelOffsetY.TabIndex = 0;
            this.labelOffsetY.Text = "Y offset";
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(3, 5);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(144, 13);
            this.label9.TabIndex = 3;
            this.label9.Text = "Crest shape (Cross-sectional)";
            // 
            // CrestLevelUnitLabel
            // 
            this.CrestLevelUnitLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.CrestLevelUnitLabel.AutoSize = true;
            this.CrestLevelUnitLabel.Location = new System.Drawing.Point(253, 29);
            this.CrestLevelUnitLabel.Name = "CrestLevelUnitLabel";
            this.CrestLevelUnitLabel.Size = new System.Drawing.Size(33, 13);
            this.CrestLevelUnitLabel.TabIndex = 6;
            this.CrestLevelUnitLabel.Text = "m AD";
            // 
            // CrestWidthUnitLabel
            // 
            this.CrestWidthUnitLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.CrestWidthUnitLabel.AutoSize = true;
            this.CrestWidthUnitLabel.Location = new System.Drawing.Point(253, 53);
            this.CrestWidthUnitLabel.Name = "CrestWidthUnitLabel";
            this.CrestWidthUnitLabel.Size = new System.Drawing.Size(15, 13);
            this.CrestWidthUnitLabel.TabIndex = 7;
            this.CrestWidthUnitLabel.Text = "m";
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(253, 79);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(15, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "m";
            // 
            // labelGeometry
            // 
            this.labelGeometry.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelGeometry.AutoSize = true;
            this.labelGeometry.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelGeometry.Location = new System.Drawing.Point(153, 5);
            this.labelGeometry.Name = "labelGeometry";
            this.labelGeometry.Size = new System.Drawing.Size(65, 13);
            this.labelGeometry.TabIndex = 9;
            this.labelGeometry.Text = "Rectangle";
            // 
            // CrestLevelTimeDependentCheckBox
            // 
            this.CrestLevelTimeDependentCheckBox.AutoSize = true;
            this.CrestLevelTimeDependentCheckBox.Location = new System.Drawing.Point(293, 27);
            this.CrestLevelTimeDependentCheckBox.Name = "CrestLevelTimeDependentCheckBox";
            this.CrestLevelTimeDependentCheckBox.Size = new System.Drawing.Size(15, 14);
            this.CrestLevelTimeDependentCheckBox.TabIndex = 10;
            this.CrestLevelTimeDependentCheckBox.UseVisualStyleBackColor = true;
            this.CrestLevelTimeDependentCheckBox.CheckedChanged += new System.EventHandler(this.CrestLevelTimeDependentCheckBox_CheckedChanged);
            // 
            // labelUnitMaxPos
            // 
            this.labelUnitMaxPos.AutoSize = true;
            this.labelUnitMaxPos.Location = new System.Drawing.Point(501, 154);
            this.labelUnitMaxPos.Name = "labelUnitMaxPos";
            this.labelUnitMaxPos.Size = new System.Drawing.Size(28, 13);
            this.labelUnitMaxPos.TabIndex = 0;
            this.labelUnitMaxPos.Text = "m³/s";
            // 
            // comboBoxCrestShape
            // 
            this.comboBoxCrestShape.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCrestShape.FormattingEnabled = true;
            this.comboBoxCrestShape.Location = new System.Drawing.Point(389, 64);
            this.comboBoxCrestShape.Name = "comboBoxCrestShape";
            this.comboBoxCrestShape.Size = new System.Drawing.Size(158, 21);
            this.comboBoxCrestShape.TabIndex = 4;
            this.comboBoxCrestShape.SelectedIndexChanged += new System.EventHandler(this.ComboBoxCrestShapeSelectedIndexChanged);
            // 
            // labelCrestShape
            // 
            this.labelCrestShape.AutoSize = true;
            this.labelCrestShape.Location = new System.Drawing.Point(389, 48);
            this.labelCrestShape.Name = "labelCrestShape";
            this.labelCrestShape.Size = new System.Drawing.Size(129, 13);
            this.labelCrestShape.TabIndex = 3;
            this.labelCrestShape.Text = "Crest shape (Longitudinal)";
            // 
            // groupBoxFormula
            // 
            this.groupBoxFormula.AutoSize = true;
            this.groupBoxFormula.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBoxFormula.Controls.Add(this.panelFormula);
            this.groupBoxFormula.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxFormula.Location = new System.Drawing.Point(0, 254);
            this.groupBoxFormula.Name = "groupBoxFormula";
            this.groupBoxFormula.Size = new System.Drawing.Size(568, 217);
            this.groupBoxFormula.TabIndex = 0;
            this.groupBoxFormula.TabStop = false;
            this.groupBoxFormula.Text = "Specific weir properties";
            // 
            // panelFormula
            // 
            this.panelFormula.AutoSize = true;
            this.panelFormula.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panelFormula.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelFormula.Location = new System.Drawing.Point(3, 16);
            this.panelFormula.Name = "panelFormula";
            this.panelFormula.Size = new System.Drawing.Size(562, 198);
            this.panelFormula.TabIndex = 5;
            // 
            // WeirView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBoxFormula);
            this.Controls.Add(this.groupBox1);
            this.Name = "WeirView";
            this.Size = new System.Drawing.Size(568, 471);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceWeir)).EndInit();
            this.groupBoxGate.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.groupBoxFormula.ResumeLayout(false);
            this.groupBoxFormula.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBoxFormula;
        private System.Windows.Forms.BindingSource bindingSourceWeir;
        private System.Windows.Forms.Label labelCrestWidth;
        private System.Windows.Forms.TextBox textBoxCrestWidth;
        private System.Windows.Forms.Label labelCrestLevel;
        private System.Windows.Forms.TextBox textBoxCrestLevel;
        private System.Windows.Forms.Label labelOffsetY;
        private System.Windows.Forms.TextBox textBoxOffsetY;
        private System.Windows.Forms.CheckBox checkBoxAllowNegativeFlow;
        private System.Windows.Forms.CheckBox checkBoxAllowPositiveFlow;
        protected System.Windows.Forms.ComboBox comboBoxWeirFormula;
        protected System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxLowerEdgeLevel;
        private System.Windows.Forms.Label labelLowerEdgeLevel;
        protected System.Windows.Forms.ComboBox comboBoxCrestShape;
        protected System.Windows.Forms.Label labelCrestShape;
        private System.Windows.Forms.TextBox textBoxGateOpening;
        private System.Windows.Forms.TextBox textBoxMaxPos;
        private System.Windows.Forms.Label labelFlowDirection;
        private System.Windows.Forms.Panel panelFormula;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox checkBoxMaxPos;
        private System.Windows.Forms.Label labelUnitMaxPos;
        private System.Windows.Forms.TextBox textBoxMaxNeg;
        private System.Windows.Forms.CheckBox checkBoxMaxNeg;
        private System.Windows.Forms.Label labelUnitMaxNeg;
        private System.Windows.Forms.Label CrestLevelUnitLabel;
        private System.Windows.Forms.Label CrestWidthUnitLabel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label LowerEdgeLevelLabel;
        private System.Windows.Forms.Label GateOpeningUnitLabel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBoxGate;
        protected System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label labelGateOpening;
        private System.Windows.Forms.Label labelGeometry;
        private System.Windows.Forms.Button OpenCrestLevelTimeSeriesButton;
        private System.Windows.Forms.Label TimeDependentLabel;
        private System.Windows.Forms.CheckBox CrestLevelTimeDependentCheckBox;
        private System.Windows.Forms.CheckBox LowerEdgeLevelTimeDependentCheckBox;
        private System.Windows.Forms.Button OpenLowerEdgeLevelTimeSeriesButton;
        protected System.Windows.Forms.Button OpenGateOpeningTimeSeriesButton;
        private System.Windows.Forms.CheckBox useVelocityHeightCheckBox;
        protected System.Windows.Forms.Label GateHeightLabel;
        protected System.Windows.Forms.TextBox textBoxGateHeight;
        protected System.Windows.Forms.Label GateHeightUnitLabel;
    }
}