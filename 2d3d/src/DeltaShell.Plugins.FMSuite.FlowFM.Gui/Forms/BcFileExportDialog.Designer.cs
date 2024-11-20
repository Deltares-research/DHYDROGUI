namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    partial class BcFileExportDialog
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.label1 = new System.Windows.Forms.Label();
            this.quantitiesListBox = new System.Windows.Forms.CheckedListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.dataTypesListBox = new System.Windows.Forms.CheckedListBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.exportModeComboBox = new System.Windows.Forms.ComboBox();
            this.fileModeLabel = new System.Windows.Forms.Label();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.quantitiesListBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Controls.Add(this.dataTypesListBox);
            this.splitContainer1.Size = new System.Drawing.Size(384, 275);
            this.splitContainer1.SplitterDistance = 189;
            this.splitContainer1.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Quantities";
            // 
            // quantitiesListBox
            // 
            this.quantitiesListBox.BackColor = System.Drawing.SystemColors.Control;
            this.quantitiesListBox.CheckOnClick = true;
            this.quantitiesListBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.quantitiesListBox.FormattingEnabled = true;
            this.quantitiesListBox.Location = new System.Drawing.Point(0, 31);
            this.quantitiesListBox.Margin = new System.Windows.Forms.Padding(8);
            this.quantitiesListBox.Name = "quantitiesListBox";
            this.quantitiesListBox.Size = new System.Drawing.Size(189, 244);
            this.quantitiesListBox.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Data types";
            // 
            // dataTypesListBox
            // 
            this.dataTypesListBox.BackColor = System.Drawing.SystemColors.Control;
            this.dataTypesListBox.CheckOnClick = true;
            this.dataTypesListBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dataTypesListBox.FormattingEnabled = true;
            this.dataTypesListBox.Location = new System.Drawing.Point(0, 31);
            this.dataTypesListBox.Margin = new System.Windows.Forms.Padding(8);
            this.dataTypesListBox.Name = "dataTypesListBox";
            this.dataTypesListBox.Size = new System.Drawing.Size(191, 244);
            this.dataTypesListBox.TabIndex = 6;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(299, 377);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 12;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancelClick);
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(212, 376);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(81, 24);
            this.buttonOk.TabIndex = 11;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // exportModeComboBox
            // 
            this.exportModeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.exportModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.exportModeComboBox.FormattingEnabled = true;
            this.exportModeComboBox.Location = new System.Drawing.Point(210, 299);
            this.exportModeComboBox.Name = "exportModeComboBox";
            this.exportModeComboBox.Size = new System.Drawing.Size(162, 21);
            this.exportModeComboBox.TabIndex = 13;
            // 
            // fileModeLabel
            // 
            this.fileModeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.fileModeLabel.AutoSize = true;
            this.fileModeLabel.Location = new System.Drawing.Point(155, 302);
            this.fileModeLabel.Name = "fileModeLabel";
            this.fileModeLabel.Size = new System.Drawing.Size(49, 13);
            this.fileModeLabel.TabIndex = 14;
            this.fileModeLabel.Text = "Export to";
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "bc";
            this.saveFileDialog.Title = "Export to file";
            // 
            // BcFileExportDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 412);
            this.Controls.Add(this.fileModeLabel);
            this.Controls.Add(this.exportModeComboBox);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.splitContainer1);
            this.MinimumSize = new System.Drawing.Size(400, 450);
            this.Name = "BcFileExportDialog";
            this.Text = "bc file export";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label1;
        protected System.Windows.Forms.CheckedListBox quantitiesListBox;
        private System.Windows.Forms.Label label2;
        protected System.Windows.Forms.CheckedListBox dataTypesListBox;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
        protected System.Windows.Forms.ComboBox exportModeComboBox;
        private System.Windows.Forms.Label fileModeLabel;
        protected System.Windows.Forms.SaveFileDialog saveFileDialog;
    }
}