namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    partial class FMPartitionExportDialogBase
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
            this.metisRadioButton = new System.Windows.Forms.RadioButton();
            this.polFileRadioButton = new System.Windows.Forms.RadioButton();
            this.domainsTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.contiguousDomainCheckBox = new System.Windows.Forms.CheckBox();
            this.polFileTextBox = new System.Windows.Forms.TextBox();
            this.polFileSelectButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.comboBoxSolverType = new System.Windows.Forms.ComboBox();
            this.labelSolverType = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // metisRadioButton
            // 
            this.metisRadioButton.AutoSize = true;
            this.metisRadioButton.Location = new System.Drawing.Point(35, 30);
            this.metisRadioButton.Name = "metisRadioButton";
            this.metisRadioButton.Size = new System.Drawing.Size(168, 17);
            this.metisRadioButton.TabIndex = 0;
            this.metisRadioButton.TabStop = true;
            this.metisRadioButton.Text = "Automatic partitioning (METIS)";
            this.metisRadioButton.UseVisualStyleBackColor = true;
            // 
            // polFileRadioButton
            // 
            this.polFileRadioButton.AutoSize = true;
            this.polFileRadioButton.Location = new System.Drawing.Point(35, 111);
            this.polFileRadioButton.Name = "polFileRadioButton";
            this.polFileRadioButton.Size = new System.Drawing.Size(133, 17);
            this.polFileRadioButton.TabIndex = 1;
            this.polFileRadioButton.TabStop = true;
            this.polFileRadioButton.Text = "Partitioning polygon file";
            this.polFileRadioButton.UseVisualStyleBackColor = true;
            // 
            // domainsTextBox
            // 
            this.domainsTextBox.Location = new System.Drawing.Point(166, 68);
            this.domainsTextBox.Name = "domainsTextBox";
            this.domainsTextBox.Size = new System.Drawing.Size(55, 20);
            this.domainsTextBox.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(59, 71);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Number of domains:";
            // 
            // contiguousDomainCheckBox
            // 
            this.contiguousDomainCheckBox.AutoSize = true;
            this.contiguousDomainCheckBox.Location = new System.Drawing.Point(232, 71);
            this.contiguousDomainCheckBox.Name = "contiguousDomainCheckBox";
            this.contiguousDomainCheckBox.Size = new System.Drawing.Size(121, 17);
            this.contiguousDomainCheckBox.TabIndex = 4;
            this.contiguousDomainCheckBox.Text = "Contiguous domains";
            this.contiguousDomainCheckBox.UseVisualStyleBackColor = true;
            // 
            // polFileTextBox
            // 
            this.polFileTextBox.Enabled = false;
            this.polFileTextBox.Location = new System.Drawing.Point(62, 152);
            this.polFileTextBox.Name = "polFileTextBox";
            this.polFileTextBox.Size = new System.Drawing.Size(255, 20);
            this.polFileTextBox.TabIndex = 5;
            // 
            // polFileSelectButton
            // 
            this.polFileSelectButton.Location = new System.Drawing.Point(323, 150);
            this.polFileSelectButton.Name = "polFileSelectButton";
            this.polFileSelectButton.Size = new System.Drawing.Size(30, 23);
            this.polFileSelectButton.TabIndex = 6;
            this.polFileSelectButton.Text = "...";
            this.polFileSelectButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(313, 246);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 7;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(232, 246);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 8;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // comboBoxSolverType
            // 
            this.comboBoxSolverType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSolverType.FormattingEnabled = true;
            this.comboBoxSolverType.Location = new System.Drawing.Point(100, 199);
            this.comboBoxSolverType.Name = "comboBoxSolverType";
            this.comboBoxSolverType.Size = new System.Drawing.Size(121, 21);
            this.comboBoxSolverType.TabIndex = 9;
            // 
            // labelSolverType
            // 
            this.labelSolverType.AutoSize = true;
            this.labelSolverType.Location = new System.Drawing.Point(32, 202);
            this.labelSolverType.Name = "labelSolverType";
            this.labelSolverType.Size = new System.Drawing.Size(60, 13);
            this.labelSolverType.TabIndex = 10;
            this.labelSolverType.Text = "Solver type";
            // 
            // FMPartitionExportDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(400, 281);
            this.Controls.Add(this.labelSolverType);
            this.Controls.Add(this.comboBoxSolverType);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.polFileSelectButton);
            this.Controls.Add(this.polFileTextBox);
            this.Controls.Add(this.contiguousDomainCheckBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.domainsTextBox);
            this.Controls.Add(this.polFileRadioButton);
            this.Controls.Add(this.metisRadioButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(406, 300);
            this.Name = "FMPartitionExportDialogBase";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Export to partition";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton metisRadioButton;
        private System.Windows.Forms.RadioButton polFileRadioButton;
        private System.Windows.Forms.TextBox domainsTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox contiguousDomainCheckBox;
        private System.Windows.Forms.TextBox polFileTextBox;
        private System.Windows.Forms.Button polFileSelectButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.ComboBox comboBoxSolverType;
        private System.Windows.Forms.Label labelSolverType;
    }
}