namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls
{
    partial class CatchmentAttributeCoverageView
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
            this.label1 = new System.Windows.Forms.Label();
            this.attributeComboBox = new System.Windows.Forms.ComboBox();
            this.secondaryAttributeComboBox = new System.Windows.Forms.ComboBox();
            this.lblSecondary = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.secondaryLayerEnabled = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(28, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Primary";
            // 
            // attributeComboBox
            // 
            this.attributeComboBox.FormattingEnabled = true;
            this.attributeComboBox.Location = new System.Drawing.Point(110, 25);
            this.attributeComboBox.Name = "attributeComboBox";
            this.attributeComboBox.Size = new System.Drawing.Size(280, 21);
            this.attributeComboBox.TabIndex = 0;
            this.attributeComboBox.SelectedIndexChanged += new System.EventHandler(this.AttributeComboBoxSelectedIndexChanged);
            // 
            // secondaryAttributeComboBox
            // 
            this.secondaryAttributeComboBox.FormattingEnabled = true;
            this.secondaryAttributeComboBox.Location = new System.Drawing.Point(110, 52);
            this.secondaryAttributeComboBox.Name = "secondaryAttributeComboBox";
            this.secondaryAttributeComboBox.Size = new System.Drawing.Size(280, 21);
            this.secondaryAttributeComboBox.TabIndex = 0;
            this.secondaryAttributeComboBox.SelectedIndexChanged += new System.EventHandler(this.SecondaryAttributeComboBoxSelectedIndexChanged);
            // 
            // lblSecondary
            // 
            this.lblSecondary.AutoSize = true;
            this.lblSecondary.Location = new System.Drawing.Point(28, 55);
            this.lblSecondary.Name = "lblSecondary";
            this.lblSecondary.Size = new System.Drawing.Size(58, 13);
            this.lblSecondary.TabIndex = 1;
            this.lblSecondary.Text = "Secondary";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.secondaryLayerEnabled);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.lblSecondary);
            this.groupBox1.Controls.Add(this.attributeComboBox);
            this.groupBox1.Controls.Add(this.secondaryAttributeComboBox);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(15);
            this.groupBox1.Size = new System.Drawing.Size(912, 87);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Attribute visualisation";
            // 
            // secondaryLayerEnabled
            // 
            this.secondaryLayerEnabled.AutoSize = true;
            this.secondaryLayerEnabled.Location = new System.Drawing.Point(7, 56);
            this.secondaryLayerEnabled.Name = "secondaryLayerEnabled";
            this.secondaryLayerEnabled.Size = new System.Drawing.Size(15, 14);
            this.secondaryLayerEnabled.TabIndex = 2;
            this.secondaryLayerEnabled.UseVisualStyleBackColor = true;
            this.secondaryLayerEnabled.CheckedChanged += new System.EventHandler(this.SecondaryLayerEnabledCheckedChanged);
            // 
            // CatchmentAttributeCoverageView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "CatchmentAttributeCoverageView";
            this.Size = new System.Drawing.Size(912, 664);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox attributeComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox secondaryAttributeComboBox;
        private System.Windows.Forms.Label lblSecondary;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox secondaryLayerEnabled;
    }
}
