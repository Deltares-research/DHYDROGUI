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
            this.ipumpBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.labelCapacity = new System.Windows.Forms.Label();
            this.capacityUnitLabel = new System.Windows.Forms.Label();
            this.textBoxCapacity = new System.Windows.Forms.TextBox();
            this.lblConvertedCapacaties = new System.Windows.Forms.Label();
            this.useTimeDependentCapacityCheckBox = new System.Windows.Forms.CheckBox();
            this.UseTimeDependentLabel = new System.Windows.Forms.Label();
            this.OpenCapacityTimeSeriesButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.ipumpBindingSource)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
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
            // capacityUnitLabel
            // 
            this.capacityUnitLabel.AutoSize = true;
            this.capacityUnitLabel.Location = new System.Drawing.Point(172, 26);
            this.capacityUnitLabel.Name = "capacityUnitLabel";
            this.capacityUnitLabel.Size = new System.Drawing.Size(31, 13);
            this.capacityUnitLabel.TabIndex = 1;
            this.capacityUnitLabel.Text = "m3/s";
            // 
            // textBoxCapacity
            // 
            this.textBoxCapacity.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.ipumpBindingSource, "Capacity", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxCapacity.Location = new System.Drawing.Point(66, 23);
            this.textBoxCapacity.Name = "textBoxCapacity";
            this.textBoxCapacity.Size = new System.Drawing.Size(100, 20);
            this.textBoxCapacity.TabIndex = 2;
            this.textBoxCapacity.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxCapacity.TextChanged += new System.EventHandler(this.TextBoxCapacityTextChanged);
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
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.OpenCapacityTimeSeriesButton);
            this.groupBox2.Controls.Add(this.UseTimeDependentLabel);
            this.groupBox2.Controls.Add(this.useTimeDependentCapacityCheckBox);
            this.groupBox2.Controls.Add(this.lblConvertedCapacaties);
            this.groupBox2.Controls.Add(this.textBoxCapacity);
            this.groupBox2.Controls.Add(this.capacityUnitLabel);
            this.groupBox2.Controls.Add(this.labelCapacity);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(442, 55);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Pump properties";
            // 
            // PumpView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
            this.Controls.Add(this.groupBox2);
            this.Name = "PumpView";
            this.Size = new System.Drawing.Size(442, 246);
            ((System.ComponentModel.ISupportInitialize)(this.ipumpBindingSource)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.BindingSource ipumpBindingSource;
        private System.Windows.Forms.Label labelCapacity;
        private System.Windows.Forms.Label capacityUnitLabel;
        private System.Windows.Forms.TextBox textBoxCapacity;
        private System.Windows.Forms.Label lblConvertedCapacaties;
        private System.Windows.Forms.CheckBox useTimeDependentCapacityCheckBox;
        private System.Windows.Forms.Label UseTimeDependentLabel;
        private System.Windows.Forms.Button OpenCapacityTimeSeriesButton;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}