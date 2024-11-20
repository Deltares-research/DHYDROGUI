namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    partial class BcFileImportDialog
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
            this.components = new System.ComponentModel.Container();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.overwriteCheckBox = new System.Windows.Forms.CheckBox();
            this.quantitiesListBox = new System.Windows.Forms.CheckedListBox();
            this.dataTypesListBox = new System.Windows.Forms.CheckedListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.deleteDataCheckBox = new System.Windows.Forms.CheckBox();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog1";
            this.openFileDialog.Multiselect = true;
            this.openFileDialog.RestoreDirectory = true;
            this.openFileDialog.Title = "Open file";
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(210, 376);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(81, 24);
            this.buttonOk.TabIndex = 0;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(297, 376);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancelClick);
            // 
            // overwriteCheckBox
            // 
            this.overwriteCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.overwriteCheckBox.AutoSize = true;
            this.overwriteCheckBox.Location = new System.Drawing.Point(12, 286);
            this.overwriteCheckBox.Name = "overwriteCheckBox";
            this.overwriteCheckBox.Size = new System.Drawing.Size(133, 17);
            this.overwriteCheckBox.TabIndex = 4;
            this.overwriteCheckBox.Text = "Overwrite existing data";
            this.overwriteCheckBox.UseVisualStyleBackColor = true;
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Quantities";
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
            this.splitContainer1.TabIndex = 9;
            // 
            // deleteDataCheckBox
            // 
            this.deleteDataCheckBox.AutoSize = true;
            this.deleteDataCheckBox.Location = new System.Drawing.Point(12, 309);
            this.deleteDataCheckBox.Name = "deleteDataCheckBox";
            this.deleteDataCheckBox.Size = new System.Drawing.Size(138, 17);
            this.deleteDataCheckBox.TabIndex = 10;
            this.deleteDataCheckBox.Text = "Delete existing data first";
            this.deleteDataCheckBox.UseVisualStyleBackColor = true;
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // BcFileImportDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 412);
            this.Controls.Add(this.deleteDataCheckBox);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.overwriteCheckBox);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.MinimumSize = new System.Drawing.Size(400, 450);
            this.Name = "BcFileImportDialog";
            this.Text = "bc file import";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        protected System.Windows.Forms.CheckBox overwriteCheckBox;
        protected System.Windows.Forms.CheckedListBox quantitiesListBox;
        protected System.Windows.Forms.CheckedListBox dataTypesListBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        protected System.Windows.Forms.CheckBox deleteDataCheckBox;
        private System.Windows.Forms.ErrorProvider errorProvider1;
    }
}