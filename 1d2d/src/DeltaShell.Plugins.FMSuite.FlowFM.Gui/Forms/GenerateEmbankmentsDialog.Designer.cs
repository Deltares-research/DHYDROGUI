namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    partial class GenerateEmbankmentsDialog
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
            this.checkBoxGenerateLeftEmbankments = new System.Windows.Forms.CheckBox();
            this.checkBoxGenerateRightEmbankments = new System.Windows.Forms.CheckBox();
            this.radioButtonCrossSectionBased = new System.Windows.Forms.RadioButton();
            this.radioButtonConstantDistance = new System.Windows.Forms.RadioButton();
            this.constantDistanceTextBox = new DelftTools.Controls.Swf.Editors.NumEdit();
            this.checkBoxAutomaticMerge = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // OkButton
            // 
            this.OkButton.Location = new System.Drawing.Point(154, 139);
            this.OkButton.Margin = new System.Windows.Forms.Padding(12, 12, 3, 3);
            this.OkButton.Name = "OkButton";
            this.OkButton.Size = new System.Drawing.Size(75, 23);
            this.OkButton.TabIndex = 1;
            this.OkButton.Text = "OK";
            this.OkButton.UseVisualStyleBackColor = true;
            this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton.Location = new System.Drawing.Point(235, 139);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 2;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // checkBoxGenerateLeftEmbankments
            // 
            this.checkBoxGenerateLeftEmbankments.AutoSize = true;
            this.checkBoxGenerateLeftEmbankments.Checked = true;
            this.checkBoxGenerateLeftEmbankments.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxGenerateLeftEmbankments.Location = new System.Drawing.Point(12, 71);
            this.checkBoxGenerateLeftEmbankments.Name = "checkBoxGenerateLeftEmbankments";
            this.checkBoxGenerateLeftEmbankments.Size = new System.Drawing.Size(119, 17);
            this.checkBoxGenerateLeftEmbankments.TabIndex = 6;
            this.checkBoxGenerateLeftEmbankments.Text = "Generate left embankments";
            this.checkBoxGenerateLeftEmbankments.UseVisualStyleBackColor = true;
            // 
            // checkBoxGenerateRightEmbankments
            // 
            this.checkBoxGenerateRightEmbankments.AutoSize = true;
            this.checkBoxGenerateRightEmbankments.Checked = true;
            this.checkBoxGenerateRightEmbankments.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxGenerateRightEmbankments.Location = new System.Drawing.Point(12, 94);
            this.checkBoxGenerateRightEmbankments.Name = "checkBoxGenerateRightEmbankments";
            this.checkBoxGenerateRightEmbankments.Size = new System.Drawing.Size(125, 17);
            this.checkBoxGenerateRightEmbankments.TabIndex = 7;
            this.checkBoxGenerateRightEmbankments.Text = "Generate right embankments";
            this.checkBoxGenerateRightEmbankments.UseVisualStyleBackColor = true;
            // 
            // radioButtonCrossSectionBased
            // 
            this.radioButtonCrossSectionBased.AutoSize = true;
            this.radioButtonCrossSectionBased.Location = new System.Drawing.Point(12, 12);
            this.radioButtonCrossSectionBased.Name = "radioButtonCrossSectionBased";
            this.radioButtonCrossSectionBased.Size = new System.Drawing.Size(120, 17);
            this.radioButtonCrossSectionBased.TabIndex = 8;
            this.radioButtonCrossSectionBased.Text = "Cross-section based";
            this.radioButtonCrossSectionBased.UseVisualStyleBackColor = true;
            // 
            // radioButtonConstantDistance
            // 
            this.radioButtonConstantDistance.AutoSize = true;
            this.radioButtonConstantDistance.Checked = true;
            this.radioButtonConstantDistance.Location = new System.Drawing.Point(12, 35);
            this.radioButtonConstantDistance.Name = "radioButtonConstantDistance";
            this.radioButtonConstantDistance.Size = new System.Drawing.Size(178, 17);
            this.radioButtonConstantDistance.TabIndex = 8;
            this.radioButtonConstantDistance.TabStop = true;
            this.radioButtonConstantDistance.Text = "Constant distance to branch (m):";
            this.radioButtonConstantDistance.UseVisualStyleBackColor = true;
            this.radioButtonConstantDistance.CheckedChanged += new System.EventHandler(this.RadioButtonConstantDistanceCheckedChanged);
            // 
            // constantDistanceTextBox
            // 
            this.constantDistanceTextBox.InputType = DelftTools.Controls.Swf.Editors.NumEdit.NumEditType.Double;
            this.constantDistanceTextBox.Location = new System.Drawing.Point(219, 35);
            this.constantDistanceTextBox.Name = "constantDistanceTextBox";
            this.constantDistanceTextBox.Size = new System.Drawing.Size(91, 20);
            this.constantDistanceTextBox.TabIndex = 9;
            this.constantDistanceTextBox.Text = "50";
            this.constantDistanceTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // checkBoxAutomaticMerge
            // 
            this.checkBoxAutomaticMerge.AutoSize = true;
            this.checkBoxAutomaticMerge.Checked = true;
            this.checkBoxAutomaticMerge.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutomaticMerge.Location = new System.Drawing.Point(12, 117);
            this.checkBoxAutomaticMerge.Name = "checkBoxAutomaticMerge";
            this.checkBoxAutomaticMerge.Size = new System.Drawing.Size(143, 17);
            this.checkBoxAutomaticMerge.TabIndex = 10;
            this.checkBoxAutomaticMerge.Text = "Perform automatic merge";
            this.checkBoxAutomaticMerge.UseVisualStyleBackColor = true;
            // 
            // GenerateEmbankmentsDialog
            // 
            this.AcceptButton = this.OkButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(320, 173);
            this.Controls.Add(this.checkBoxAutomaticMerge);
            this.Controls.Add(this.constantDistanceTextBox);
            this.Controls.Add(this.radioButtonConstantDistance);
            this.Controls.Add(this.radioButtonCrossSectionBased);
            this.Controls.Add(this.checkBoxGenerateRightEmbankments);
            this.Controls.Add(this.checkBoxGenerateLeftEmbankments);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.OkButton);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GenerateEmbankmentsDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Generate Embankments";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OkButton;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.CheckBox checkBoxGenerateLeftEmbankments;
        private System.Windows.Forms.CheckBox checkBoxGenerateRightEmbankments;
        private System.Windows.Forms.RadioButton radioButtonCrossSectionBased;
        private System.Windows.Forms.RadioButton radioButtonConstantDistance;
        private DelftTools.Controls.Swf.Editors.NumEdit constantDistanceTextBox;
        private System.Windows.Forms.CheckBox checkBoxAutomaticMerge;
    }
}