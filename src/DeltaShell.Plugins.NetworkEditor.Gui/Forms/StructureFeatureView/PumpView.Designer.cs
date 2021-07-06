namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    partial class PumpView
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.txtSuctionOn = new System.Windows.Forms.TextBox();
            this.ipumpBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.txtDeliveryOn = new System.Windows.Forms.TextBox();
            this.checkBoxSuction = new System.Windows.Forms.CheckBox();
            this.checkBoxDelivery = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSuctionOff = new System.Windows.Forms.TextBox();
            this.txtDeliveryOff = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.controlLevelsGroupBox = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.buttonReduction = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.OpenCapacityTimeSeriesButton = new System.Windows.Forms.Button();
            this.UseTimeDependentLabel = new System.Windows.Forms.Label();
            this.useTimeDependentCapacityCheckBox = new System.Windows.Forms.CheckBox();
            this.lblConvertedCapacaties = new System.Windows.Forms.Label();
            this.yOffsetTextBox = new System.Windows.Forms.TextBox();
            this.yOffsetLabel = new System.Windows.Forms.Label();
            this.textBoxCapacity = new System.Windows.Forms.TextBox();
            this.capacityUnitLabel = new System.Windows.Forms.Label();
            this.labelCapacity = new System.Windows.Forms.Label();
            this.radioButtonNegative = new System.Windows.Forms.RadioButton();
            this.radioButtonPositive = new System.Windows.Forms.RadioButton();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ipumpBindingSource)).BeginInit();
            this.controlLevelsGroupBox.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtSuctionOn, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtDeliveryOn, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxSuction, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxDelivery, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label4, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.label2, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtSuctionOff, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtDeliveryOff, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.label5, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.label6, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.label7, 4, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(434, 85);
            this.tableLayoutPanel1.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(133, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Switch-on level:";
            // 
            // txtSuctionOn
            // 
            this.txtSuctionOn.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txtSuctionOn.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.ipumpBindingSource, "StartSuction", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.txtSuctionOn.Location = new System.Drawing.Point(133, 26);
            this.txtSuctionOn.Name = "txtSuctionOn";
            this.txtSuctionOn.Size = new System.Drawing.Size(117, 20);
            this.txtSuctionOn.TabIndex = 4;
            this.txtSuctionOn.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // ipumpBindingSource
            // 
            this.ipumpBindingSource.DataSource = typeof(DelftTools.Hydro.Structures.IPump);
            // 
            // txtDeliveryOn
            // 
            this.txtDeliveryOn.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txtDeliveryOn.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.ipumpBindingSource, "StartDelivery", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.txtDeliveryOn.Location = new System.Drawing.Point(133, 58);
            this.txtDeliveryOn.Name = "txtDeliveryOn";
            this.txtDeliveryOn.Size = new System.Drawing.Size(117, 20);
            this.txtDeliveryOn.TabIndex = 6;
            this.txtDeliveryOn.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // checkBoxSuction
            // 
            this.checkBoxSuction.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.checkBoxSuction.AutoSize = true;
            this.checkBoxSuction.Checked = true;
            this.checkBoxSuction.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSuction.Location = new System.Drawing.Point(3, 27);
            this.checkBoxSuction.Name = "checkBoxSuction";
            this.checkBoxSuction.Size = new System.Drawing.Size(90, 17);
            this.checkBoxSuction.TabIndex = 10;
            this.checkBoxSuction.Text = "Suction side :";
            this.checkBoxSuction.UseVisualStyleBackColor = true;
            this.checkBoxSuction.CheckedChanged += new System.EventHandler(this.CheckBoxCheckedChanged);
            // 
            // checkBoxDelivery
            // 
            this.checkBoxDelivery.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.checkBoxDelivery.AutoSize = true;
            this.checkBoxDelivery.Checked = true;
            this.checkBoxDelivery.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxDelivery.Location = new System.Drawing.Point(3, 60);
            this.checkBoxDelivery.Name = "checkBoxDelivery";
            this.checkBoxDelivery.Size = new System.Drawing.Size(92, 17);
            this.checkBoxDelivery.TabIndex = 10;
            this.checkBoxDelivery.Text = "Delivery side :";
            this.checkBoxDelivery.UseVisualStyleBackColor = true;
            this.checkBoxDelivery.CheckedChanged += new System.EventHandler(this.CheckBoxCheckedChanged);
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(263, 29);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(14, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "m";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.Red;
            this.label2.Location = new System.Drawing.Point(283, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Switch-off level:";
            // 
            // txtSuctionOff
            // 
            this.txtSuctionOff.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txtSuctionOff.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.ipumpBindingSource, "StopSuction", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.txtSuctionOff.Location = new System.Drawing.Point(283, 26);
            this.txtSuctionOff.Name = "txtSuctionOff";
            this.txtSuctionOff.Size = new System.Drawing.Size(120, 20);
            this.txtSuctionOff.TabIndex = 5;
            this.txtSuctionOff.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtDeliveryOff
            // 
            this.txtDeliveryOff.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txtDeliveryOff.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.ipumpBindingSource, "StopDelivery", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.txtDeliveryOff.Location = new System.Drawing.Point(283, 58);
            this.txtDeliveryOff.Name = "txtDeliveryOff";
            this.txtDeliveryOff.Size = new System.Drawing.Size(120, 20);
            this.txtDeliveryOff.TabIndex = 7;
            this.txtDeliveryOff.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(413, 29);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(15, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "m";
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(263, 62);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(14, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "m";
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(413, 62);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(15, 13);
            this.label7.TabIndex = 14;
            this.label7.Text = "m";
            // 
            // controlLevelsGroupBox
            // 
            this.controlLevelsGroupBox.Controls.Add(this.label8);
            this.controlLevelsGroupBox.Controls.Add(this.buttonReduction);
            this.controlLevelsGroupBox.Controls.Add(this.tableLayoutPanel1);
            this.controlLevelsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.controlLevelsGroupBox.Location = new System.Drawing.Point(0, 105);
            this.controlLevelsGroupBox.Name = "controlLevelsGroupBox";
            this.controlLevelsGroupBox.Size = new System.Drawing.Size(442, 141);
            this.controlLevelsGroupBox.TabIndex = 8;
            this.controlLevelsGroupBox.TabStop = false;
            this.controlLevelsGroupBox.Text = "Pump control levels";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 111);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(82, 13);
            this.label8.TabIndex = 9;
            this.label8.Text = "Reduction table";
            // 
            // buttonReduction
            // 
            this.buttonReduction.Location = new System.Drawing.Point(137, 107);
            this.buttonReduction.Name = "buttonReduction";
            this.buttonReduction.Size = new System.Drawing.Size(117, 21);
            this.buttonReduction.TabIndex = 8;
            this.buttonReduction.Text = "...";
            this.buttonReduction.UseVisualStyleBackColor = true;
            this.buttonReduction.Click += new System.EventHandler(this.buttonReduction_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.OpenCapacityTimeSeriesButton);
            this.groupBox2.Controls.Add(this.UseTimeDependentLabel);
            this.groupBox2.Controls.Add(this.useTimeDependentCapacityCheckBox);
            this.groupBox2.Controls.Add(this.lblConvertedCapacaties);
            this.groupBox2.Controls.Add(this.yOffsetTextBox);
            this.groupBox2.Controls.Add(this.yOffsetLabel);
            this.groupBox2.Controls.Add(this.textBoxCapacity);
            this.groupBox2.Controls.Add(this.capacityUnitLabel);
            this.groupBox2.Controls.Add(this.labelCapacity);
            this.groupBox2.Controls.Add(this.radioButtonNegative);
            this.groupBox2.Controls.Add(this.radioButtonPositive);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(442, 105);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Pump properties";
            // 
            // OpenCapacityTimeSeriesButton
            // 
            this.OpenCapacityTimeSeriesButton.Location = new System.Drawing.Point(66, 23);
            this.OpenCapacityTimeSeriesButton.Name = "OpenCapacityTimeSeriesButton";
            this.OpenCapacityTimeSeriesButton.Size = new System.Drawing.Size(100, 19);
            this.OpenCapacityTimeSeriesButton.TabIndex = 8;
            this.OpenCapacityTimeSeriesButton.Text = "Time series ...";
            this.OpenCapacityTimeSeriesButton.UseVisualStyleBackColor = true;
            this.OpenCapacityTimeSeriesButton.Visible = false;
            this.OpenCapacityTimeSeriesButton.Click += new System.EventHandler(this.OpenCapacityTimeSeriesButton_Click);
            // 
            // UseTimeDependentLabel
            // 
            this.UseTimeDependentLabel.AutoSize = true;
            this.UseTimeDependentLabel.Location = new System.Drawing.Point(171, 8);
            this.UseTimeDependentLabel.Name = "UseTimeDependentLabel";
            this.UseTimeDependentLabel.Size = new System.Drawing.Size(84, 13);
            this.UseTimeDependentLabel.TabIndex = 7;
            this.UseTimeDependentLabel.Text = "Time dependent";
            this.UseTimeDependentLabel.Visible = false;
            // 
            // useTimeDependentCapacityCheckBox
            // 
            this.useTimeDependentCapacityCheckBox.AutoSize = true;
            this.useTimeDependentCapacityCheckBox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.ipumpBindingSource, "UseCapacityTimeSeries", true));
            this.useTimeDependentCapacityCheckBox.Location = new System.Drawing.Point(206, 29);
            this.useTimeDependentCapacityCheckBox.Name = "useTimeDependentCapacityCheckBox";
            this.useTimeDependentCapacityCheckBox.Size = new System.Drawing.Size(15, 14);
            this.useTimeDependentCapacityCheckBox.TabIndex = 6;
            this.useTimeDependentCapacityCheckBox.UseVisualStyleBackColor = true;
            this.useTimeDependentCapacityCheckBox.Visible = false;
            this.useTimeDependentCapacityCheckBox.CheckedChanged += new System.EventHandler(this.useTimeDependentCapacityCheckBox_CheckedChanged);
            // 
            // lblConvertedCapacaties
            // 
            this.lblConvertedCapacaties.AutoSize = true;
            this.lblConvertedCapacaties.Location = new System.Drawing.Point(232, 26);
            this.lblConvertedCapacaties.Name = "lblConvertedCapacaties";
            this.lblConvertedCapacaties.Size = new System.Drawing.Size(28, 13);
            this.lblConvertedCapacaties.TabIndex = 5;
            this.lblConvertedCapacaties.Text = "(= ?)";
            // 
            // yOffsetTextBox
            // 
            this.yOffsetTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.ipumpBindingSource, "OffsetY", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged, null, "N3"));
            this.yOffsetTextBox.Location = new System.Drawing.Point(66, 72);
            this.yOffsetTextBox.Name = "yOffsetTextBox";
            this.yOffsetTextBox.ReadOnly = true;
            this.yOffsetTextBox.Size = new System.Drawing.Size(100, 20);
            this.yOffsetTextBox.TabIndex = 4;
            // 
            // yOffsetLabel
            // 
            this.yOffsetLabel.AutoSize = true;
            this.yOffsetLabel.Location = new System.Drawing.Point(6, 75);
            this.yOffsetLabel.Name = "yOffsetLabel";
            this.yOffsetLabel.Size = new System.Drawing.Size(45, 13);
            this.yOffsetLabel.TabIndex = 3;
            this.yOffsetLabel.Text = "Y Offset";
            // 
            // textBoxCapacity
            // 
            this.textBoxCapacity.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.ipumpBindingSource, "Capacity", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "F4"));
            this.textBoxCapacity.Location = new System.Drawing.Point(66, 23);
            this.textBoxCapacity.Name = "textBoxCapacity";
            this.textBoxCapacity.Size = new System.Drawing.Size(100, 20);
            this.textBoxCapacity.TabIndex = 2;
            this.textBoxCapacity.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxCapacity.TextChanged += new System.EventHandler(this.TextBoxCapacityTextChanged);
            // 
            // capacityUnitLabel
            // 
            this.capacityUnitLabel.AutoSize = true;
            this.capacityUnitLabel.Location = new System.Drawing.Point(172, 26);
            this.capacityUnitLabel.Name = "capacityUnitLabel";
            this.capacityUnitLabel.Size = new System.Drawing.Size(28, 13);
            this.capacityUnitLabel.TabIndex = 1;
            this.capacityUnitLabel.Text = "m³/s";
            // 
            // labelCapacity
            // 
            this.labelCapacity.AutoSize = true;
            this.labelCapacity.Location = new System.Drawing.Point(6, 26);
            this.labelCapacity.Name = "labelCapacity";
            this.labelCapacity.Size = new System.Drawing.Size(48, 13);
            this.labelCapacity.TabIndex = 1;
            this.labelCapacity.Text = "Capacity";
            // 
            // radioButtonNegative
            // 
            this.radioButtonNegative.AutoSize = true;
            this.radioButtonNegative.Checked = true;
            this.radioButtonNegative.Location = new System.Drawing.Point(134, 49);
            this.radioButtonNegative.Name = "radioButtonNegative";
            this.radioButtonNegative.Size = new System.Drawing.Size(68, 17);
            this.radioButtonNegative.TabIndex = 0;
            this.radioButtonNegative.TabStop = true;
            this.radioButtonNegative.Text = "Negative";
            this.radioButtonNegative.UseVisualStyleBackColor = true;
            // 
            // radioButtonPositive
            // 
            this.radioButtonPositive.AutoSize = true;
            this.radioButtonPositive.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.ipumpBindingSource, "DirectionIsPositive", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.radioButtonPositive.Location = new System.Drawing.Point(68, 49);
            this.radioButtonPositive.Name = "radioButtonPositive";
            this.radioButtonPositive.Size = new System.Drawing.Size(62, 17);
            this.radioButtonPositive.TabIndex = 0;
            this.radioButtonPositive.Text = "Positive";
            this.radioButtonPositive.UseVisualStyleBackColor = true;
            // 
            // PumpView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
            this.Controls.Add(this.controlLevelsGroupBox);
            this.Controls.Add(this.groupBox2);
            this.Name = "PumpView";
            this.Size = new System.Drawing.Size(442, 246);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ipumpBindingSource)).EndInit();
            this.controlLevelsGroupBox.ResumeLayout(false);
            this.controlLevelsGroupBox.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSuctionOn;
        private System.Windows.Forms.TextBox txtSuctionOff;
        private System.Windows.Forms.TextBox txtDeliveryOn;
        private System.Windows.Forms.TextBox txtDeliveryOff;
        private System.Windows.Forms.GroupBox controlLevelsGroupBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox textBoxCapacity;
        private System.Windows.Forms.Label labelCapacity;
        private System.Windows.Forms.RadioButton radioButtonNegative;
        private System.Windows.Forms.RadioButton radioButtonPositive;
        private System.Windows.Forms.Label capacityUnitLabel;
        private System.Windows.Forms.BindingSource ipumpBindingSource;
        private System.Windows.Forms.CheckBox checkBoxSuction;
        private System.Windows.Forms.CheckBox checkBoxDelivery;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button buttonReduction;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox yOffsetTextBox;
        private System.Windows.Forms.Label yOffsetLabel;
        private System.Windows.Forms.Label lblConvertedCapacaties;
        private System.Windows.Forms.Button OpenCapacityTimeSeriesButton;
        private System.Windows.Forms.Label UseTimeDependentLabel;
        private System.Windows.Forms.CheckBox useTimeDependentCapacityCheckBox;

    }
}