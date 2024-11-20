using System.Windows.Forms;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Editors
{
    partial class BoundaryConditionPropertiesControl
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
            this.bcTypeLabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.dataTypeComboBox = new System.Windows.Forms.ComboBox();
            this.TimeZoneLabel = new System.Windows.Forms.Label();
            this.TimeZoneUnitLabel = new System.Windows.Forms.Label();
            this.TimeZoneTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // bcTypeLabel
            // 
            this.bcTypeLabel.AutoSize = true;
            this.bcTypeLabel.Location = new System.Drawing.Point(157, 3);
            this.bcTypeLabel.Name = "bcTypeLabel";
            this.bcTypeLabel.Size = new System.Drawing.Size(31, 13);
            this.bcTypeLabel.TabIndex = 12;
            this.bcTypeLabel.Text = "none";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(2, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Forcing type:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Description:";
            // 
            // dataTypeComboBox
            // 
            this.dataTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dataTypeComboBox.FormattingEnabled = true;
            this.dataTypeComboBox.Location = new System.Drawing.Point(160, 25);
            this.dataTypeComboBox.Name = "dataTypeComboBox";
            this.dataTypeComboBox.Size = new System.Drawing.Size(140, 21);
            this.dataTypeComboBox.TabIndex = 14;
            // 
            // TimeZoneLabel
            // 
            this.TimeZoneLabel.AutoSize = true;
            this.TimeZoneLabel.Location = new System.Drawing.Point(2, 55);
            this.TimeZoneLabel.Name = "TimeZoneLabel";
            this.TimeZoneLabel.Size = new System.Drawing.Size(59, 13);
            this.TimeZoneLabel.TabIndex = 31;
            this.TimeZoneLabel.Text = "Time zone:";
            // 
            // TimeZoneUnitLabel
            // 
            this.TimeZoneUnitLabel.AutoSize = true;
            this.TimeZoneUnitLabel.Location = new System.Drawing.Point(300, 55);
            this.TimeZoneUnitLabel.Name = "TimeZoneUnitLabel";
            this.TimeZoneUnitLabel.Size = new System.Drawing.Size(16, 13);
            this.TimeZoneUnitLabel.TabIndex = 32;
            this.TimeZoneUnitLabel.Text = "[-]";
            // 
            // TimeZoneTextBox
            // 
            this.TimeZoneTextBox.Location = new System.Drawing.Point(160, 52);
            this.TimeZoneTextBox.Name = "TimeZoneTextBox";
            this.TimeZoneTextBox.ReadOnly = true;
            this.TimeZoneTextBox.Size = new System.Drawing.Size(67, 20);
            this.TimeZoneTextBox.TabIndex = 33;
            this.TimeZoneTextBox.Tag = "TimeZone";
            this.TimeZoneTextBox.Text = "0,0";
            // 
            // BoundaryConditionPropertiesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TimeZoneLabel);
            this.Controls.Add(this.TimeZoneUnitLabel);
            this.Controls.Add(this.TimeZoneTextBox);
            this.Controls.Add(this.dataTypeComboBox);
            this.Controls.Add(this.bcTypeLabel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.MinimumSize = new System.Drawing.Size(316, 204);
            this.Name = "BoundaryConditionPropertiesControl";
            this.Size = new System.Drawing.Size(316, 226);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label TimeZoneLabel;
        private System.Windows.Forms.Label TimeZoneUnitLabel;
        private System.Windows.Forms.TextBox TimeZoneTextBox;

        #endregion

        private System.Windows.Forms.Label bcTypeLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox dataTypeComboBox;
    }
}
