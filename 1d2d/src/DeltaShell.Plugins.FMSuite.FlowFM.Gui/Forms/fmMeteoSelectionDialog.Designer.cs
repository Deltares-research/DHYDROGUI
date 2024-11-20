using System;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    internal partial class FmMeteoSelectionDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.OkButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.PrecipitationRadioButton = new System.Windows.Forms.RadioButton();
            this.PrecipitationTypeComboBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // OkButton
            // 
            this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OkButton.Location = new System.Drawing.Point(118, 261);
            this.OkButton.Name = "OkButton";
            this.OkButton.Size = new System.Drawing.Size(75, 23);
            this.OkButton.TabIndex = 0;
            this.OkButton.Text = "OK";
            this.OkButton.UseVisualStyleBackColor = true;
            this.OkButton.Click += new System.EventHandler(this.OkButtonClick);
            // 
            // CancelButton
            // 
            this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton.Location = new System.Drawing.Point(199, 261);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 1;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButtonClick);
            // 
            // PrecipitationRadioButton
            // 
            this.PrecipitationRadioButton.AutoSize = true;
            this.PrecipitationRadioButton.Location = new System.Drawing.Point(12, 12);
            this.PrecipitationRadioButton.Name = "PrecipitationRadioButton";
            this.PrecipitationRadioButton.Size = new System.Drawing.Size(184, 17);
            this.PrecipitationRadioButton.TabIndex = 2;
            this.PrecipitationRadioButton.TabStop = true;
            this.PrecipitationRadioButton.Text = "Fm Meteo precipitation time series";
            this.PrecipitationRadioButton.UseVisualStyleBackColor = true;
            // 
            // PrecipitationTypeComboBox
            // 
            this.PrecipitationTypeComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.PrecipitationTypeComboBox.Items.AddRange(new object[] {
            DeltaShell.Plugins.FMSuite.Common.FeatureData.FmMeteoLocationType.Global,
            DeltaShell.Plugins.FMSuite.Common.FeatureData.FmMeteoLocationType.Feature,
            DeltaShell.Plugins.FMSuite.Common.FeatureData.FmMeteoLocationType.Polygon,
            DeltaShell.Plugins.FMSuite.Common.FeatureData.FmMeteoLocationType.Grid});
            this.PrecipitationTypeComboBox.Location = new System.Drawing.Point(12, 35);
            this.PrecipitationTypeComboBox.Name = "PrecipitationTypeComboBox";
            this.PrecipitationTypeComboBox.Size = new System.Drawing.Size(120, 21);
            this.PrecipitationTypeComboBox.TabIndex = 3;
            this.PrecipitationTypeComboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.PrecipitationTypeComboBox_DrawItem);
            this.PrecipitationTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.PrecipitationTypeComboBox_SelectedIndexChanged);
            // 
            // FmMeteoSelectionDialog
            // 
            this.AcceptButton = this.OkButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 296);
            this.Controls.Add(this.PrecipitationRadioButton);
            this.Controls.Add(this.PrecipitationTypeComboBox);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.OkButton);
            this.MaximumSize = new System.Drawing.Size(300, 334);
            this.MinimumSize = new System.Drawing.Size(300, 334);
            this.Name = "FmMeteoSelectionDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select meteo item";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OkButton;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.RadioButton PrecipitationRadioButton;
        private System.Windows.Forms.ComboBox PrecipitationTypeComboBox;
    }
}