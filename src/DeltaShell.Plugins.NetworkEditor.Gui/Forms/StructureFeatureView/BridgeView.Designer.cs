using System;
using DelftTools.Hydro.Structures;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    partial class BridgeView
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
            this.pageSplitContainer = new System.Windows.Forms.SplitContainer();
            this.geometrySplitContainer = new System.Windows.Forms.SplitContainer();
            this.splitContainerGeometry = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.textBoxShift = new System.Windows.Forms.TextBox();
            this.bindingSourceBridge = new System.Windows.Forms.BindingSource(this.components);
            this.textBoxHeight = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.labelWidth = new System.Windows.Forms.Label();
            this.labelHeight = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxWidth = new System.Windows.Forms.TextBox();
            this.labelShift = new System.Windows.Forms.Label();
            this.tableViewTabulatedData = new DelftTools.Controls.Swf.Table.TableView();
            this.label8 = new System.Windows.Forms.Label();
            this.lblPillarWidth = new System.Windows.Forms.Label();
            this.textBoxShapeFactor = new System.Windows.Forms.TextBox();
            this.textBoxPillarWidth = new System.Windows.Forms.TextBox();
            this.lblShapeFactor = new System.Windows.Forms.Label();
            this.groupBoxGeometry = new System.Windows.Forms.GroupBox();
            this.bridgeTypeCombobox = new System.Windows.Forms.ComboBox();
            this.yOffsetPanel = new System.Windows.Forms.Panel();
            this.labelYOffset = new System.Windows.Forms.Label();
            this.textBoxYOffset = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.roughnessGroupBox = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label56 = new System.Windows.Forms.Label();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.label59 = new System.Windows.Forms.Label();
            this.label55 = new System.Windows.Forms.Label();
            this.labelGroundLayerFrictionUnit = new System.Windows.Forms.Label();
            this.label58 = new System.Windows.Forms.Label();
            this.textBoxGroundLayerRoughnessValue = new System.Windows.Forms.TextBox();
            this.textBoxGroundLayerThickness = new System.Windows.Forms.TextBox();
            this.label57 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.labelFrictionUnit = new System.Windows.Forms.Label();
            this.textBoxRoughnessValue = new System.Windows.Forms.TextBox();
            this.comboBoxFrictionType = new System.Windows.Forms.ComboBox();
            this.lblLength = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxOutletLoss = new System.Windows.Forms.TextBox();
            this.textBoxLength = new System.Windows.Forms.TextBox();
            this.checkBoxAllowNegativeFlow = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.checkBoxAllowPositiveFlow = new System.Windows.Forms.CheckBox();
            this.textBoxInletLoss = new System.Windows.Forms.TextBox();
            this.lblInlet = new System.Windows.Forms.Label();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pageSplitContainer)).BeginInit();
            this.pageSplitContainer.Panel1.SuspendLayout();
            this.pageSplitContainer.Panel2.SuspendLayout();
            this.pageSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.geometrySplitContainer)).BeginInit();
            this.geometrySplitContainer.Panel1.SuspendLayout();
            this.geometrySplitContainer.Panel2.SuspendLayout();
            this.geometrySplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerGeometry)).BeginInit();
            this.splitContainerGeometry.Panel1.SuspendLayout();
            this.splitContainerGeometry.Panel2.SuspendLayout();
            this.splitContainerGeometry.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceBridge)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tableViewTabulatedData)).BeginInit();
            this.groupBoxGeometry.SuspendLayout();
            this.yOffsetPanel.SuspendLayout();
            this.roughnessGroupBox.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.pageSplitContainer);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(657, 468);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " Bridge properties";
            // 
            // pageSplitContainer
            // 
            this.pageSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pageSplitContainer.Location = new System.Drawing.Point(3, 16);
            this.pageSplitContainer.Name = "pageSplitContainer";
            // 
            // pageSplitContainer.Panel1
            // 
            this.pageSplitContainer.Panel1.Controls.Add(this.geometrySplitContainer);
            this.pageSplitContainer.Panel1.Controls.Add(this.groupBoxGeometry);
            this.pageSplitContainer.Panel1.Controls.Add(this.yOffsetPanel);
            // 
            // pageSplitContainer.Panel2
            // 
            this.pageSplitContainer.Panel2.Controls.Add(this.roughnessGroupBox);
            this.pageSplitContainer.Panel2.Controls.Add(this.lblLength);
            this.pageSplitContainer.Panel2.Controls.Add(this.label1);
            this.pageSplitContainer.Panel2.Controls.Add(this.textBoxOutletLoss);
            this.pageSplitContainer.Panel2.Controls.Add(this.textBoxLength);
            this.pageSplitContainer.Panel2.Controls.Add(this.checkBoxAllowNegativeFlow);
            this.pageSplitContainer.Panel2.Controls.Add(this.label5);
            this.pageSplitContainer.Panel2.Controls.Add(this.label13);
            this.pageSplitContainer.Panel2.Controls.Add(this.checkBoxAllowPositiveFlow);
            this.pageSplitContainer.Panel2.Controls.Add(this.textBoxInletLoss);
            this.pageSplitContainer.Panel2.Controls.Add(this.lblInlet);
            this.pageSplitContainer.Size = new System.Drawing.Size(651, 449);
            this.pageSplitContainer.SplitterDistance = 314;
            this.pageSplitContainer.TabIndex = 36;
            // 
            // geometrySplitContainer
            // 
            this.geometrySplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.geometrySplitContainer.IsSplitterFixed = true;
            this.geometrySplitContainer.Location = new System.Drawing.Point(0, 111);
            this.geometrySplitContainer.Name = "geometrySplitContainer";
            // 
            // geometrySplitContainer.Panel1
            // 
            this.geometrySplitContainer.Panel1.Controls.Add(this.splitContainerGeometry);
            this.geometrySplitContainer.Panel1MinSize = 0;
            // 
            // geometrySplitContainer.Panel2
            // 
            this.geometrySplitContainer.Panel2.Controls.Add(this.label8);
            this.geometrySplitContainer.Panel2.Controls.Add(this.lblPillarWidth);
            this.geometrySplitContainer.Panel2.Controls.Add(this.textBoxShapeFactor);
            this.geometrySplitContainer.Panel2.Controls.Add(this.textBoxPillarWidth);
            this.geometrySplitContainer.Panel2.Controls.Add(this.lblShapeFactor);
            this.geometrySplitContainer.Panel2MinSize = 0;
            this.geometrySplitContainer.Size = new System.Drawing.Size(314, 338);
            this.geometrySplitContainer.SplitterDistance = 175;
            this.geometrySplitContainer.TabIndex = 37;
            // 
            // splitContainerGeometry
            // 
            this.splitContainerGeometry.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerGeometry.IsSplitterFixed = true;
            this.splitContainerGeometry.Location = new System.Drawing.Point(0, 0);
            this.splitContainerGeometry.Name = "splitContainerGeometry";
            // 
            // splitContainerGeometry.Panel1
            // 
            this.splitContainerGeometry.Panel1.Controls.Add(this.tableLayoutPanel1);
            this.splitContainerGeometry.Panel1MinSize = 0;
            // 
            // splitContainerGeometry.Panel2
            // 
            this.splitContainerGeometry.Panel2.Controls.Add(this.tableViewTabulatedData);
            this.splitContainerGeometry.Panel2MinSize = 0;
            this.splitContainerGeometry.Size = new System.Drawing.Size(175, 338);
            this.splitContainerGeometry.SplitterDistance = 82;
            this.splitContainerGeometry.TabIndex = 21;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 76F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 63F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 61F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.textBoxHeight, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label2, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.label4, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.labelWidth, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelHeight, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label3, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.textBoxWidth, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(192, 80);
            this.tableLayoutPanel1.TabIndex = 6;
            // bindingSourceBridge
            // 
            this.bindingSourceBridge.DataSource = typeof(DelftTools.Hydro.Structures.IBridge);
            // 
            // 
            // textBoxShift
            // 
            this.textBoxShift.Location = new System.Drawing.Point(93, 26);
            this.textBoxShift.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceBridge, nameof(IBridge.Shift), true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged, null, "N3"));
            this.textBoxShift.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxShift.Name = "textBoxShift";
            this.textBoxShift.Size = new System.Drawing.Size(85, 20);
            this.textBoxShift.TabIndex = 1;
            this.textBoxShift.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxHeight
            // 
            this.textBoxHeight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxHeight.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceBridge, "Height", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged, null, "N3"));
            this.textBoxHeight.Location = new System.Drawing.Point(79, 28);
            this.textBoxHeight.Name = "textBoxHeight";
            this.textBoxHeight.Size = new System.Drawing.Size(57, 20);
            this.textBoxHeight.TabIndex = 1;
            this.textBoxHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(142, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(15, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "m";
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(142, 58);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(15, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "m";
            // 
            // labelWidth
            // 
            this.labelWidth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelWidth.AutoSize = true;
            this.labelWidth.Location = new System.Drawing.Point(3, 6);
            this.labelWidth.Name = "labelWidth";
            this.labelWidth.Size = new System.Drawing.Size(70, 13);
            this.labelWidth.TabIndex = 0;
            this.labelWidth.Text = "Width";
            // 
            // labelHeight
            // 
            this.labelHeight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelHeight.AutoSize = true;
            this.labelHeight.Location = new System.Drawing.Point(3, 31);
            this.labelHeight.Name = "labelHeight";
            this.labelHeight.Size = new System.Drawing.Size(70, 13);
            this.labelHeight.TabIndex = 0;
            this.labelHeight.Text = "Height";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(142, 31);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(15, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "m";
            // 
            // textBoxWidth
            // 
            this.textBoxWidth.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceBridge, "Width", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged, null, "N3"));
            this.textBoxWidth.Location = new System.Drawing.Point(79, 3);
            this.textBoxWidth.Name = "textBoxWidth";
            this.textBoxWidth.Size = new System.Drawing.Size(57, 20);
            this.textBoxWidth.TabIndex = 1;
            this.textBoxWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // labelShift
            // 
            this.labelShift.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelShift.AutoSize = true;
            this.labelShift.Location = new System.Drawing.Point(3, 28);
            this.labelShift.Name = "labelShift";
            this.labelShift.Size = new System.Drawing.Size(84, 13);
            this.labelShift.TabIndex = 0;
            this.labelShift.Text = "Shift";
            // 
            // tableViewTabulatedData
            // 
            this.tableViewTabulatedData.AllowAddNewRow = true;
            this.tableViewTabulatedData.AllowDeleteRow = true;
            this.tableViewTabulatedData.AutoGenerateColumns = true;
            this.tableViewTabulatedData.ColumnAutoWidth = false;
            this.tableViewTabulatedData.DisplayCellFilter = null;
            this.tableViewTabulatedData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableViewTabulatedData.HeaderHeigth = -1;
            this.tableViewTabulatedData.InputValidator = null;
            this.tableViewTabulatedData.InvalidCellBackgroundColor = System.Drawing.Color.Tomato;
            this.tableViewTabulatedData.InvalidCellFilter = null;
            this.tableViewTabulatedData.Location = new System.Drawing.Point(0, 0);
            this.tableViewTabulatedData.MultipleCellEdit = true;
            this.tableViewTabulatedData.MultiSelect = true;
            this.tableViewTabulatedData.Name = "tableViewTabulatedData";
            this.tableViewTabulatedData.ReadOnly = false;
            this.tableViewTabulatedData.ReadOnlyCellBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
            this.tableViewTabulatedData.ReadOnlyCellFilter = null;
            this.tableViewTabulatedData.ReadOnlyCellForeColor = System.Drawing.Color.LightGray;
            this.tableViewTabulatedData.RowSelect = false;
            this.tableViewTabulatedData.RowValidator = null;
            this.tableViewTabulatedData.ShowRowNumbers = false;
            this.tableViewTabulatedData.Size = new System.Drawing.Size(89, 338);
            this.tableViewTabulatedData.TabIndex = 10;
            this.tableViewTabulatedData.UseCenteredHeaderText = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(154, 6);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(15, 13);
            this.label8.TabIndex = 23;
            this.label8.Text = "m";
            // 
            // lblPillarWidth
            // 
            this.lblPillarWidth.AutoSize = true;
            this.lblPillarWidth.Location = new System.Drawing.Point(3, 6);
            this.lblPillarWidth.Name = "lblPillarWidth";
            this.lblPillarWidth.Size = new System.Drawing.Size(83, 13);
            this.lblPillarWidth.TabIndex = 19;
            this.lblPillarWidth.Text = "Total pillar width";
            // 
            // textBoxShapeFactor
            //
            //Not yet implemented in the kernel
            //this.textBoxShapeFactor.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceBridge, "ShapeFactor", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged, null, "N3"));
            this.textBoxShapeFactor.Location = new System.Drawing.Point(97, 29);
            this.textBoxShapeFactor.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
            this.textBoxShapeFactor.Name = "textBoxShapeFactor";
            this.textBoxShapeFactor.Size = new System.Drawing.Size(51, 20);
            this.textBoxShapeFactor.TabIndex = 22;
            this.textBoxShapeFactor.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxPillarWidth
            // 
            //Not yet implemented in the kernel
            //this.textBoxPillarWidth.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceBridge, "PillarWidth", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged, null, "N2"));
            this.textBoxPillarWidth.Location = new System.Drawing.Point(97, 6);
            this.textBoxPillarWidth.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
            this.textBoxPillarWidth.Name = "textBoxPillarWidth";
            this.textBoxPillarWidth.Size = new System.Drawing.Size(51, 20);
            this.textBoxPillarWidth.TabIndex = 20;
            this.textBoxPillarWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxPillarWidth.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxPillarWidth_Validating);
            this.textBoxPillarWidth.Validated += new System.EventHandler(this.textBoxPillarWidth_Validated);
            // 
            // lblShapeFactor
            // 
            this.lblShapeFactor.AutoSize = true;
            this.lblShapeFactor.Location = new System.Drawing.Point(3, 33);
            this.lblShapeFactor.Name = "lblShapeFactor";
            this.lblShapeFactor.Size = new System.Drawing.Size(68, 13);
            this.lblShapeFactor.TabIndex = 21;
            this.lblShapeFactor.Text = "Shape factor";
            // 
            // groupBoxGeometry
            // 
            this.groupBoxGeometry.Controls.Add(this.tableLayoutPanel2);
            this.groupBoxGeometry.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxGeometry.Location = new System.Drawing.Point(0, 29);
            this.groupBoxGeometry.Name = "groupBoxGeometry";
            this.groupBoxGeometry.Size = new System.Drawing.Size(314, 82);
            this.groupBoxGeometry.TabIndex = 22;
            this.groupBoxGeometry.TabStop = false;
            this.groupBoxGeometry.Text = "Geometry of (cross-sectional) flow-area";
            // 
            // bridgeTypeCombobox
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.bridgeTypeCombobox, 2);
            this.bridgeTypeCombobox.DataBindings.Add(new System.Windows.Forms.Binding("SelectedValue", this.bindingSourceBridge, "BridgeType", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.bridgeTypeCombobox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.bridgeTypeCombobox.DataSource = Enum.GetValues(typeof(BridgeType));
            this.bridgeTypeCombobox.FormattingEnabled = true;
            this.bridgeTypeCombobox.Location = new System.Drawing.Point(3, 3);
            this.bridgeTypeCombobox.Name = "bridgeTypeCombobox";
            this.bridgeTypeCombobox.Size = new System.Drawing.Size(175, 21);
            this.bridgeTypeCombobox.TabIndex = 21;
            // 
            // yOffsetPanel
            // 
            this.yOffsetPanel.Controls.Add(this.labelYOffset);
            this.yOffsetPanel.Controls.Add(this.textBoxYOffset);
            this.yOffsetPanel.Controls.Add(this.label6);
            this.yOffsetPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.yOffsetPanel.Location = new System.Drawing.Point(0, 0);
            this.yOffsetPanel.Name = "yOffsetPanel";
            this.yOffsetPanel.Size = new System.Drawing.Size(314, 29);
            this.yOffsetPanel.TabIndex = 37;
            // 
            // labelYOffset
            // 
            this.labelYOffset.AutoSize = true;
            this.labelYOffset.Location = new System.Drawing.Point(8, 6);
            this.labelYOffset.Name = "labelYOffset";
            this.labelYOffset.Size = new System.Drawing.Size(45, 13);
            this.labelYOffset.TabIndex = 0;
            this.labelYOffset.Text = "Y Offset";
            // 
            // textBoxYOffset
            // 
            this.textBoxYOffset.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceBridge, "OffsetY", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged, null, "N3"));
            this.textBoxYOffset.Location = new System.Drawing.Point(59, 3);
            this.textBoxYOffset.Name = "textBoxYOffset";
            this.textBoxYOffset.ReadOnly = true;
            this.textBoxYOffset.Size = new System.Drawing.Size(56, 20);
            this.textBoxYOffset.TabIndex = 1;
            this.textBoxYOffset.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(121, 6);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(15, 13);
            this.label6.TabIndex = 9;
            this.label6.Text = "m";
            // 
            // roughnessGroupBox
            // 
            this.roughnessGroupBox.Controls.Add(this.groupBox2);
            this.roughnessGroupBox.Controls.Add(this.label57);
            this.roughnessGroupBox.Controls.Add(this.label12);
            this.roughnessGroupBox.Controls.Add(this.labelFrictionUnit);
            this.roughnessGroupBox.Controls.Add(this.textBoxRoughnessValue);
            this.roughnessGroupBox.Controls.Add(this.comboBoxFrictionType);
            this.roughnessGroupBox.Location = new System.Drawing.Point(7, 30);
            this.roughnessGroupBox.Name = "roughnessGroupBox";
            this.roughnessGroupBox.Size = new System.Drawing.Size(251, 92);
            this.roughnessGroupBox.TabIndex = 29;
            this.roughnessGroupBox.TabStop = false;
            this.roughnessGroupBox.Text = "Roughness";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label56);
            this.groupBox2.Controls.Add(this.checkBox2);
            this.groupBox2.Controls.Add(this.label59);
            this.groupBox2.Controls.Add(this.label55);
            this.groupBox2.Controls.Add(this.labelGroundLayerFrictionUnit);
            this.groupBox2.Controls.Add(this.label58);
            this.groupBox2.Controls.Add(this.textBoxGroundLayerRoughnessValue);
            this.groupBox2.Controls.Add(this.textBoxGroundLayerThickness);
            this.groupBox2.Location = new System.Drawing.Point(9, 81);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(233, 101);
            this.groupBox2.TabIndex = 33;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Ground layer";
            this.groupBox2.Visible = false;
            // 
            // label56
            // 
            this.label56.AutoSize = true;
            this.label56.Location = new System.Drawing.Point(155, 70);
            this.label56.Name = "label56";
            this.label56.Size = new System.Drawing.Size(15, 13);
            this.label56.TabIndex = 31;
            this.label56.Text = "m";
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBox2.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.bindingSourceBridge, "GroundLayerEnabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBox2.Location = new System.Drawing.Point(73, 22);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(15, 14);
            this.checkBox2.TabIndex = 32;
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // label59
            // 
            this.label59.AutoSize = true;
            this.label59.Location = new System.Drawing.Point(6, 22);
            this.label59.Name = "label59";
            this.label59.Size = new System.Drawing.Size(46, 13);
            this.label59.TabIndex = 25;
            this.label59.Text = "Enabled";
            // 
            // label55
            // 
            this.label55.AutoSize = true;
            this.label55.Location = new System.Drawing.Point(6, 45);
            this.label55.Name = "label55";
            this.label55.Size = new System.Drawing.Size(61, 13);
            this.label55.TabIndex = 25;
            this.label55.Text = "Roughness";
            // 
            // labelGroundLayerFrictionUnit
            // 
            this.labelGroundLayerFrictionUnit.AutoSize = true;
            this.labelGroundLayerFrictionUnit.Location = new System.Drawing.Point(155, 43);
            this.labelGroundLayerFrictionUnit.Name = "labelGroundLayerFrictionUnit";
            this.labelGroundLayerFrictionUnit.Size = new System.Drawing.Size(28, 13);
            this.labelGroundLayerFrictionUnit.TabIndex = 24;
            this.labelGroundLayerFrictionUnit.Text = "m³/s";
            // 
            // label58
            // 
            this.label58.AutoSize = true;
            this.label58.Location = new System.Drawing.Point(6, 70);
            this.label58.Name = "label58";
            this.label58.Size = new System.Drawing.Size(56, 13);
            this.label58.TabIndex = 30;
            this.label58.Text = "Thickness";
            // 
            // textBoxGroundLayerRoughnessValue
            // 
            this.textBoxGroundLayerRoughnessValue.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceBridge, "GroundLayerRoughness", true));
            this.textBoxGroundLayerRoughnessValue.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourceBridge, "GroundLayerEnabled", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.textBoxGroundLayerRoughnessValue.Location = new System.Drawing.Point(73, 42);
            this.textBoxGroundLayerRoughnessValue.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
            this.textBoxGroundLayerRoughnessValue.Name = "textBoxGroundLayerRoughnessValue";
            this.textBoxGroundLayerRoughnessValue.Size = new System.Drawing.Size(77, 20);
            this.textBoxGroundLayerRoughnessValue.TabIndex = 23;
            this.textBoxGroundLayerRoughnessValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxGroundLayerThickness
            // 
            this.textBoxGroundLayerThickness.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceBridge, "GroundLayerThickness", true));
            this.textBoxGroundLayerThickness.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourceBridge, "GroundLayerEnabled", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.textBoxGroundLayerThickness.Location = new System.Drawing.Point(73, 67);
            this.textBoxGroundLayerThickness.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
            this.textBoxGroundLayerThickness.Name = "textBoxGroundLayerThickness";
            this.textBoxGroundLayerThickness.Size = new System.Drawing.Size(77, 20);
            this.textBoxGroundLayerThickness.TabIndex = 29;
            this.textBoxGroundLayerThickness.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label57
            // 
            this.label57.AutoSize = true;
            this.label57.Location = new System.Drawing.Point(6, 23);
            this.label57.Name = "label57";
            this.label57.Size = new System.Drawing.Size(31, 13);
            this.label57.TabIndex = 21;
            this.label57.Text = "Type";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(6, 53);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(34, 13);
            this.label12.TabIndex = 21;
            this.label12.Text = "Value";
            // 
            // labelFrictionUnit
            // 
            this.labelFrictionUnit.AutoSize = true;
            this.labelFrictionUnit.Location = new System.Drawing.Point(165, 53);
            this.labelFrictionUnit.Name = "labelFrictionUnit";
            this.labelFrictionUnit.Size = new System.Drawing.Size(28, 13);
            this.labelFrictionUnit.TabIndex = 7;
            this.labelFrictionUnit.Text = "m³/s";
            // 
            // textBoxRoughnessValue
            // 
            this.textBoxRoughnessValue.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceBridge, "Friction", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxRoughnessValue.Location = new System.Drawing.Point(82, 46);
            this.textBoxRoughnessValue.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
            this.textBoxRoughnessValue.Name = "textBoxRoughnessValue";
            this.textBoxRoughnessValue.Size = new System.Drawing.Size(77, 20);
            this.textBoxRoughnessValue.TabIndex = 13;
            this.textBoxRoughnessValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // comboBoxFrictionType
            // 
            this.comboBoxFrictionType.DataBindings.Add(new System.Windows.Forms.Binding("SelectedValue", this.bindingSourceBridge, "FrictionType", true));
            this.comboBoxFrictionType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFrictionType.FormattingEnabled = true;
            this.comboBoxFrictionType.Location = new System.Drawing.Point(82, 15);
            this.comboBoxFrictionType.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
            this.comboBoxFrictionType.Name = "comboBoxFrictionType";
            this.comboBoxFrictionType.Size = new System.Drawing.Size(110, 21);
            this.comboBoxFrictionType.TabIndex = 11;
            this.comboBoxFrictionType.SelectedValueChanged += new System.EventHandler(this.ComboBoxFrictionTypeSelectedValueChanged);
            // 
            // lblLength
            // 
            this.lblLength.AutoSize = true;
            this.lblLength.Location = new System.Drawing.Point(15, 9);
            this.lblLength.Name = "lblLength";
            this.lblLength.Size = new System.Drawing.Size(40, 13);
            this.lblLength.TabIndex = 3;
            this.lblLength.Text = "Length";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 231);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(109, 13);
            this.label1.TabIndex = 18;
            this.label1.Text = "Allowed flow direction";
            // 
            // textBoxOutletLoss
            // 
            this.textBoxOutletLoss.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceBridge, "OutletLossCoefficient", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged, null, "N3"));
            this.textBoxOutletLoss.Location = new System.Drawing.Point(85, 298);
            this.textBoxOutletLoss.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
            this.textBoxOutletLoss.Name = "textBoxOutletLoss";
            this.textBoxOutletLoss.Size = new System.Drawing.Size(51, 20);
            this.textBoxOutletLoss.TabIndex = 18;
            this.textBoxOutletLoss.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxLength
            // 
            this.textBoxLength.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceBridge, "Length", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged, null, "N3"));
            this.textBoxLength.Location = new System.Drawing.Point(89, 6);
            this.textBoxLength.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
            this.textBoxLength.Name = "textBoxLength";
            this.textBoxLength.Size = new System.Drawing.Size(77, 20);
            this.textBoxLength.TabIndex = 8;
            this.textBoxLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // checkBoxAllowNegativeFlow
            // 
            this.checkBoxAllowNegativeFlow.AutoSize = true;
            this.checkBoxAllowNegativeFlow.DataBindings.Add(new System.Windows.Forms.Binding("CheckState", this.bindingSourceBridge, "AllowNegativeFlow", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxAllowNegativeFlow.Location = new System.Drawing.Point(147, 252);
            this.checkBoxAllowNegativeFlow.Name = "checkBoxAllowNegativeFlow";
            this.checkBoxAllowNegativeFlow.Size = new System.Drawing.Size(69, 17);
            this.checkBoxAllowNegativeFlow.TabIndex = 2;
            this.checkBoxAllowNegativeFlow.Text = "Negative";
            this.checkBoxAllowNegativeFlow.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 301);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 13);
            this.label5.TabIndex = 17;
            this.label5.Text = "Outlet loss";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(172, 9);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(15, 13);
            this.label13.TabIndex = 16;
            this.label13.Text = "m";
            // 
            // checkBoxAllowPositiveFlow
            // 
            this.checkBoxAllowPositiveFlow.AutoSize = true;
            this.checkBoxAllowPositiveFlow.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.bindingSourceBridge, "AllowPositiveFlow", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxAllowPositiveFlow.Location = new System.Drawing.Point(48, 252);
            this.checkBoxAllowPositiveFlow.Name = "checkBoxAllowPositiveFlow";
            this.checkBoxAllowPositiveFlow.Size = new System.Drawing.Size(63, 17);
            this.checkBoxAllowPositiveFlow.TabIndex = 2;
            this.checkBoxAllowPositiveFlow.Text = "Positive";
            this.checkBoxAllowPositiveFlow.UseVisualStyleBackColor = true;
            // 
            // textBoxInletLoss
            // 
            this.textBoxInletLoss.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceBridge, "InletLossCoefficient", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged, null, "N3"));
            this.textBoxInletLoss.Location = new System.Drawing.Point(85, 274);
            this.textBoxInletLoss.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
            this.textBoxInletLoss.Name = "textBoxInletLoss";
            this.textBoxInletLoss.Size = new System.Drawing.Size(51, 20);
            this.textBoxInletLoss.TabIndex = 16;
            this.textBoxInletLoss.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // lblInlet
            // 
            this.lblInlet.AutoSize = true;
            this.lblInlet.Location = new System.Drawing.Point(13, 277);
            this.lblInlet.Name = "lblInlet";
            this.lblInlet.Size = new System.Drawing.Size(48, 13);
            this.lblInlet.TabIndex = 1;
            this.lblInlet.Text = "Inlet loss";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.textBoxShift, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.bridgeTypeCombobox, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.labelShift, 0, 1);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(6, 24);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(181, 47);
            this.tableLayoutPanel2.TabIndex = 22;
            // 
            // BridgeView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "BridgeView";
            this.Size = new System.Drawing.Size(657, 468);
            this.groupBox1.ResumeLayout(false);
            this.pageSplitContainer.Panel1.ResumeLayout(false);
            this.pageSplitContainer.Panel2.ResumeLayout(false);
            this.pageSplitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pageSplitContainer)).EndInit();
            this.pageSplitContainer.ResumeLayout(false);
            this.geometrySplitContainer.Panel1.ResumeLayout(false);
            this.geometrySplitContainer.Panel2.ResumeLayout(false);
            this.geometrySplitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.geometrySplitContainer)).EndInit();
            this.geometrySplitContainer.ResumeLayout(false);
            this.splitContainerGeometry.Panel1.ResumeLayout(false);
            this.splitContainerGeometry.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerGeometry)).EndInit();
            this.splitContainerGeometry.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceBridge)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tableViewTabulatedData)).EndInit();
            this.groupBoxGeometry.ResumeLayout(false);
            this.yOffsetPanel.ResumeLayout(false);
            this.yOffsetPanel.PerformLayout();
            this.roughnessGroupBox.ResumeLayout(false);
            this.roughnessGroupBox.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBoxAllowPositiveFlow;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label labelShift;
        private System.Windows.Forms.TextBox textBoxShift;
        private System.Windows.Forms.TextBox textBoxWidth;
        private System.Windows.Forms.Label labelWidth;
        private System.Windows.Forms.TextBox textBoxHeight;
        private System.Windows.Forms.Label labelHeight;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labelYOffset;
        private System.Windows.Forms.TextBox textBoxYOffset;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblLength;
        private System.Windows.Forms.Label labelFrictionUnit;
        private System.Windows.Forms.CheckBox checkBoxAllowNegativeFlow;
        private DelftTools.Controls.Swf.Table.TableView tableViewTabulatedData;
        private System.Windows.Forms.ComboBox comboBoxFrictionType;
        private System.Windows.Forms.TextBox textBoxLength;
        private System.Windows.Forms.TextBox textBoxRoughnessValue;
        private System.Windows.Forms.TextBox textBoxOutletLoss;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxInletLoss;
        private System.Windows.Forms.Label lblInlet;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.BindingSource bindingSourceBridge;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.SplitContainer splitContainerGeometry;
        private System.Windows.Forms.GroupBox groupBoxGeometry;
        private System.Windows.Forms.SplitContainer pageSplitContainer;
        private System.Windows.Forms.Panel yOffsetPanel;
        private System.Windows.Forms.TextBox textBoxShapeFactor;
        private System.Windows.Forms.Label lblShapeFactor;
        private System.Windows.Forms.TextBox textBoxPillarWidth;
        private System.Windows.Forms.Label lblPillarWidth;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox bridgeTypeCombobox;
        private System.Windows.Forms.SplitContainer geometrySplitContainer;
        private System.Windows.Forms.GroupBox roughnessGroupBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label56;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.Label label59;
        private System.Windows.Forms.Label label55;
        private System.Windows.Forms.Label labelGroundLayerFrictionUnit;
        private System.Windows.Forms.Label label58;
        private System.Windows.Forms.TextBox textBoxGroundLayerRoughnessValue;
        private System.Windows.Forms.TextBox textBoxGroundLayerThickness;
        private System.Windows.Forms.Label label57;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    }
}