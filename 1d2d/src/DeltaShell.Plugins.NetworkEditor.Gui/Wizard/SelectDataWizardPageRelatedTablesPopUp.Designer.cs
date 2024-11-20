namespace DeltaShell.Plugins.NetworkEditor.Gui.Wizard
{
    partial class SelectDataWizardPageRelatedTablesPopUp
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
            this.groupBoxID = new System.Windows.Forms.GroupBox();
            this.comboBoxID = new System.Windows.Forms.ComboBox();
            this.labelID = new System.Windows.Forms.Label();
            this.groupBoxRelatedTables = new System.Windows.Forms.GroupBox();
            this.comboBoxForeignKey2 = new System.Windows.Forms.ComboBox();
            this.comboBoxForeignKey1 = new System.Windows.Forms.ComboBox();
            this.comboBoxRelatedTables2 = new System.Windows.Forms.ComboBox();
            this.comboBoxRelatedTables1 = new System.Windows.Forms.ComboBox();
            this.labelForeignKey = new System.Windows.Forms.Label();
            this.labelRelatedTables = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxID.SuspendLayout();
            this.groupBoxRelatedTables.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxID
            // 
            this.groupBoxID.Controls.Add(this.comboBoxID);
            this.groupBoxID.Controls.Add(this.labelID);
            this.groupBoxID.Location = new System.Drawing.Point(15, 17);
            this.groupBoxID.Name = "groupBoxID";
            this.groupBoxID.Size = new System.Drawing.Size(462, 57);
            this.groupBoxID.TabIndex = 0;
            this.groupBoxID.TabStop = false;
            this.groupBoxID.Text = "Select ID column of base table";
            // 
            // comboBoxID
            // 
            this.comboBoxID.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxID.FormattingEnabled = true;
            this.comboBoxID.Location = new System.Drawing.Point(239, 20);
            this.comboBoxID.Name = "comboBoxID";
            this.comboBoxID.Size = new System.Drawing.Size(206, 21);
            this.comboBoxID.TabIndex = 1;
            // 
            // labelID
            // 
            this.labelID.AutoSize = true;
            this.labelID.Location = new System.Drawing.Point(10, 23);
            this.labelID.Name = "labelID";
            this.labelID.Size = new System.Drawing.Size(55, 13);
            this.labelID.TabIndex = 0;
            this.labelID.Text = "ID column";
            // 
            // groupBoxRelatedTables
            // 
            this.groupBoxRelatedTables.Controls.Add(this.comboBoxForeignKey2);
            this.groupBoxRelatedTables.Controls.Add(this.comboBoxForeignKey1);
            this.groupBoxRelatedTables.Controls.Add(this.comboBoxRelatedTables2);
            this.groupBoxRelatedTables.Controls.Add(this.comboBoxRelatedTables1);
            this.groupBoxRelatedTables.Controls.Add(this.labelForeignKey);
            this.groupBoxRelatedTables.Controls.Add(this.labelRelatedTables);
            this.groupBoxRelatedTables.Location = new System.Drawing.Point(14, 96);
            this.groupBoxRelatedTables.Name = "groupBoxRelatedTables";
            this.groupBoxRelatedTables.Size = new System.Drawing.Size(462, 100);
            this.groupBoxRelatedTables.TabIndex = 1;
            this.groupBoxRelatedTables.TabStop = false;
            this.groupBoxRelatedTables.Text = "Select related tables";
            // 
            // comboBoxForeignKey2
            // 
            this.comboBoxForeignKey2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxForeignKey2.Enabled = false;
            this.comboBoxForeignKey2.FormattingEnabled = true;
            this.comboBoxForeignKey2.Location = new System.Drawing.Point(240, 63);
            this.comboBoxForeignKey2.Name = "comboBoxForeignKey2";
            this.comboBoxForeignKey2.Size = new System.Drawing.Size(216, 21);
            this.comboBoxForeignKey2.TabIndex = 5;
            // 
            // comboBoxForeignKey1
            // 
            this.comboBoxForeignKey1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxForeignKey1.Enabled = false;
            this.comboBoxForeignKey1.FormattingEnabled = true;
            this.comboBoxForeignKey1.Location = new System.Drawing.Point(240, 35);
            this.comboBoxForeignKey1.Name = "comboBoxForeignKey1";
            this.comboBoxForeignKey1.Size = new System.Drawing.Size(216, 21);
            this.comboBoxForeignKey1.TabIndex = 4;
            // 
            // comboBoxRelatedTables2
            // 
            this.comboBoxRelatedTables2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxRelatedTables2.FormattingEnabled = true;
            this.comboBoxRelatedTables2.Location = new System.Drawing.Point(14, 63);
            this.comboBoxRelatedTables2.Name = "comboBoxRelatedTables2";
            this.comboBoxRelatedTables2.Size = new System.Drawing.Size(216, 21);
            this.comboBoxRelatedTables2.TabIndex = 3;
            this.comboBoxRelatedTables2.SelectedIndexChanged += new System.EventHandler(this.comboBoxRelatedTables2_SelectedIndexChanged);
            // 
            // comboBoxRelatedTables1
            // 
            this.comboBoxRelatedTables1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxRelatedTables1.FormattingEnabled = true;
            this.comboBoxRelatedTables1.Location = new System.Drawing.Point(14, 36);
            this.comboBoxRelatedTables1.Name = "comboBoxRelatedTables1";
            this.comboBoxRelatedTables1.Size = new System.Drawing.Size(216, 21);
            this.comboBoxRelatedTables1.TabIndex = 2;
            this.comboBoxRelatedTables1.SelectedIndexChanged += new System.EventHandler(this.comboBoxRelatedTables1_SelectedIndexChanged);
            // 
            // labelForeignKey
            // 
            this.labelForeignKey.AutoSize = true;
            this.labelForeignKey.Location = new System.Drawing.Point(237, 19);
            this.labelForeignKey.Name = "labelForeignKey";
            this.labelForeignKey.Size = new System.Drawing.Size(62, 13);
            this.labelForeignKey.TabIndex = 1;
            this.labelForeignKey.Text = "Foreign key";
            // 
            // labelRelatedTables
            // 
            this.labelRelatedTables.AutoSize = true;
            this.labelRelatedTables.Location = new System.Drawing.Point(11, 20);
            this.labelRelatedTables.Name = "labelRelatedTables";
            this.labelRelatedTables.Size = new System.Drawing.Size(75, 13);
            this.labelRelatedTables.TabIndex = 0;
            this.labelRelatedTables.Text = "Related tables";
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(368, 218);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(102, 23);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(254, 218);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(102, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // SelectDataWizardPageRelatedTablesPopUp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(492, 259);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBoxRelatedTables);
            this.Controls.Add(this.groupBoxID);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectDataWizardPageRelatedTablesPopUp";
            this.Text = "Select related tables and foreign keys";
            this.groupBoxID.ResumeLayout(false);
            this.groupBoxID.PerformLayout();
            this.groupBoxRelatedTables.ResumeLayout(false);
            this.groupBoxRelatedTables.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxID;
        private System.Windows.Forms.ComboBox comboBoxID;
        private System.Windows.Forms.Label labelID;
        private System.Windows.Forms.GroupBox groupBoxRelatedTables;
        private System.Windows.Forms.Label labelForeignKey;
        private System.Windows.Forms.Label labelRelatedTables;
        private System.Windows.Forms.ComboBox comboBoxForeignKey2;
        private System.Windows.Forms.ComboBox comboBoxForeignKey1;
        private System.Windows.Forms.ComboBox comboBoxRelatedTables2;
        private System.Windows.Forms.ComboBox comboBoxRelatedTables1;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}