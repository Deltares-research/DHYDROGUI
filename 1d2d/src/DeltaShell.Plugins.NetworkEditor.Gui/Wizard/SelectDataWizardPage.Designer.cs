namespace DeltaShell.Plugins.NetworkEditor.Gui.Wizard
{
    partial class SelectDataWizardPage
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
            this.groupBoxNetworkFeature = new System.Windows.Forms.GroupBox();
            this.buttonLoadMappingFile = new System.Windows.Forms.Button();
            this.btnRelatedTables = new System.Windows.Forms.Button();
            this.comboBoxDiscriminatorValue = new System.Windows.Forms.ComboBox();
            this.btnAddToList = new System.Windows.Forms.Button();
            this.comboBoxDiscriminatorColumn = new System.Windows.Forms.ComboBox();
            this.comboBoxTable = new System.Windows.Forms.ComboBox();
            this.lblDiscriminatorValue = new System.Windows.Forms.Label();
            this.lblDiscriminator = new System.Windows.Forms.Label();
            this.lblTypeOfFeature = new System.Windows.Forms.Label();
            this.maskedTextBoxNumberOfLevels = new System.Windows.Forms.MaskedTextBox();
            this.lblNumberOfLevels = new System.Windows.Forms.Label();
            this.lblTable = new System.Windows.Forms.Label();
            this.lblFile = new System.Windows.Forms.Label();
            this.comboBoxFeatureType = new System.Windows.Forms.ComboBox();
            this.btnSelectFile = new System.Windows.Forms.Button();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.groupBoxImportList = new System.Windows.Forms.GroupBox();
            this.tableViewImportList = new DelftTools.Controls.Swf.Table.TableView();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.groupBoxNetworkFeature.SuspendLayout();
            this.groupBoxImportList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tableViewImportList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxNetworkFeature
            // 
            this.groupBoxNetworkFeature.Controls.Add(this.buttonLoadMappingFile);
            this.groupBoxNetworkFeature.Controls.Add(this.btnRelatedTables);
            this.groupBoxNetworkFeature.Controls.Add(this.comboBoxDiscriminatorValue);
            this.groupBoxNetworkFeature.Controls.Add(this.btnAddToList);
            this.groupBoxNetworkFeature.Controls.Add(this.comboBoxDiscriminatorColumn);
            this.groupBoxNetworkFeature.Controls.Add(this.comboBoxTable);
            this.groupBoxNetworkFeature.Controls.Add(this.lblDiscriminatorValue);
            this.groupBoxNetworkFeature.Controls.Add(this.lblDiscriminator);
            this.groupBoxNetworkFeature.Controls.Add(this.lblTypeOfFeature);
            this.groupBoxNetworkFeature.Controls.Add(this.maskedTextBoxNumberOfLevels);
            this.groupBoxNetworkFeature.Controls.Add(this.lblNumberOfLevels);
            this.groupBoxNetworkFeature.Controls.Add(this.lblTable);
            this.groupBoxNetworkFeature.Controls.Add(this.lblFile);
            this.groupBoxNetworkFeature.Controls.Add(this.comboBoxFeatureType);
            this.groupBoxNetworkFeature.Controls.Add(this.btnSelectFile);
            this.groupBoxNetworkFeature.Controls.Add(this.txtPath);
            this.groupBoxNetworkFeature.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxNetworkFeature.Location = new System.Drawing.Point(0, 0);
            this.groupBoxNetworkFeature.Name = "groupBoxNetworkFeature";
            this.groupBoxNetworkFeature.Size = new System.Drawing.Size(710, 229);
            this.groupBoxNetworkFeature.TabIndex = 6;
            this.groupBoxNetworkFeature.TabStop = false;
            this.groupBoxNetworkFeature.Text = "Select features to import";
            // 
            // buttonLoadMappingFile
            // 
            this.buttonLoadMappingFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonLoadMappingFile.Location = new System.Drawing.Point(18, 191);
            this.buttonLoadMappingFile.Name = "buttonLoadMappingFile";
            this.buttonLoadMappingFile.Size = new System.Drawing.Size(208, 24);
            this.buttonLoadMappingFile.TabIndex = 38;
            this.buttonLoadMappingFile.Text = "Load mapping file of GIS-importers";
            this.buttonLoadMappingFile.UseVisualStyleBackColor = true;
            this.buttonLoadMappingFile.Click += new System.EventHandler(this.buttonLoadMappingFile_Click);
            // 
            // btnRelatedTables
            // 
            this.btnRelatedTables.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRelatedTables.Enabled = false;
            this.btnRelatedTables.Image = Properties.Resources.Relational;
            this.btnRelatedTables.Location = new System.Drawing.Point(542, 79);
            this.btnRelatedTables.Name = "btnRelatedTables";
            this.btnRelatedTables.Size = new System.Drawing.Size(29, 23);
            this.btnRelatedTables.TabIndex = 37;
            this.btnRelatedTables.UseVisualStyleBackColor = true;
            this.btnRelatedTables.Click += new System.EventHandler(this.btnRelatedTables_Click);
            // 
            // comboBoxDiscriminatorValue
            // 
            this.comboBoxDiscriminatorValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxDiscriminatorValue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDiscriminatorValue.Enabled = false;
            this.comboBoxDiscriminatorValue.FormattingEnabled = true;
            this.comboBoxDiscriminatorValue.Location = new System.Drawing.Point(155, 136);
            this.comboBoxDiscriminatorValue.Name = "comboBoxDiscriminatorValue";
            this.comboBoxDiscriminatorValue.Size = new System.Drawing.Size(416, 21);
            this.comboBoxDiscriminatorValue.TabIndex = 36;
            // 
            // btnAddToList
            // 
            this.btnAddToList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddToList.Enabled = false;
            this.btnAddToList.Location = new System.Drawing.Point(588, 191);
            this.btnAddToList.Name = "btnAddToList";
            this.btnAddToList.Size = new System.Drawing.Size(107, 23);
            this.btnAddToList.TabIndex = 35;
            this.btnAddToList.Text = "Add to import list";
            this.btnAddToList.UseVisualStyleBackColor = true;
            this.btnAddToList.Click += new System.EventHandler(this.btnAddToList_Click);
            // 
            // comboBoxDiscriminatorColumn
            // 
            this.comboBoxDiscriminatorColumn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxDiscriminatorColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDiscriminatorColumn.Enabled = false;
            this.comboBoxDiscriminatorColumn.FormattingEnabled = true;
            this.comboBoxDiscriminatorColumn.Location = new System.Drawing.Point(155, 108);
            this.comboBoxDiscriminatorColumn.Name = "comboBoxDiscriminatorColumn";
            this.comboBoxDiscriminatorColumn.Size = new System.Drawing.Size(416, 21);
            this.comboBoxDiscriminatorColumn.TabIndex = 34;
            this.comboBoxDiscriminatorColumn.SelectedIndexChanged += new System.EventHandler(this.comboBoxDiscriminatorColumn_SelectedIndexChanged);
            // 
            // comboBoxTable
            // 
            this.comboBoxTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxTable.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTable.Enabled = false;
            this.comboBoxTable.FormattingEnabled = true;
            this.comboBoxTable.Location = new System.Drawing.Point(156, 79);
            this.comboBoxTable.Name = "comboBoxTable";
            this.comboBoxTable.Size = new System.Drawing.Size(375, 21);
            this.comboBoxTable.TabIndex = 33;
            this.comboBoxTable.SelectedIndexChanged += new System.EventHandler(this.comboBoxTable_SelectedIndexChanged);
            // 
            // lblDiscriminatorValue
            // 
            this.lblDiscriminatorValue.AutoSize = true;
            this.lblDiscriminatorValue.Location = new System.Drawing.Point(15, 139);
            this.lblDiscriminatorValue.Name = "lblDiscriminatorValue";
            this.lblDiscriminatorValue.Size = new System.Drawing.Size(58, 13);
            this.lblDiscriminatorValue.TabIndex = 32;
            this.lblDiscriminatorValue.Text = "Filter value";
            // 
            // lblDiscriminator
            // 
            this.lblDiscriminator.AutoSize = true;
            this.lblDiscriminator.Location = new System.Drawing.Point(15, 111);
            this.lblDiscriminator.Name = "lblDiscriminator";
            this.lblDiscriminator.Size = new System.Drawing.Size(66, 13);
            this.lblDiscriminator.TabIndex = 30;
            this.lblDiscriminator.Text = "Filter column";
            // 
            // lblTypeOfFeature
            // 
            this.lblTypeOfFeature.AutoSize = true;
            this.lblTypeOfFeature.Location = new System.Drawing.Point(15, 26);
            this.lblTypeOfFeature.Name = "lblTypeOfFeature";
            this.lblTypeOfFeature.Size = new System.Drawing.Size(48, 13);
            this.lblTypeOfFeature.TabIndex = 28;
            this.lblTypeOfFeature.Text = "Features";
            // 
            // maskedTextBoxNumberOfLevels
            // 
            this.maskedTextBoxNumberOfLevels.Enabled = false;
            this.maskedTextBoxNumberOfLevels.Location = new System.Drawing.Point(156, 165);
            this.maskedTextBoxNumberOfLevels.Mask = "0";
            this.maskedTextBoxNumberOfLevels.Name = "maskedTextBoxNumberOfLevels";
            this.maskedTextBoxNumberOfLevels.Size = new System.Drawing.Size(70, 20);
            this.maskedTextBoxNumberOfLevels.TabIndex = 27;
            this.maskedTextBoxNumberOfLevels.Text = "3";
            // 
            // lblNumberOfLevels
            // 
            this.lblNumberOfLevels.AutoSize = true;
            this.lblNumberOfLevels.Location = new System.Drawing.Point(15, 168);
            this.lblNumberOfLevels.Name = "lblNumberOfLevels";
            this.lblNumberOfLevels.Size = new System.Drawing.Size(114, 13);
            this.lblNumberOfLevels.TabIndex = 26;
            this.lblNumberOfLevels.Text = "Number of levels (WH)";
            // 
            // lblTable
            // 
            this.lblTable.AutoSize = true;
            this.lblTable.Location = new System.Drawing.Point(15, 82);
            this.lblTable.Name = "lblTable";
            this.lblTable.Size = new System.Drawing.Size(34, 13);
            this.lblTable.TabIndex = 25;
            this.lblTable.Text = "Table";
            // 
            // lblFile
            // 
            this.lblFile.AutoSize = true;
            this.lblFile.Location = new System.Drawing.Point(15, 54);
            this.lblFile.Name = "lblFile";
            this.lblFile.Size = new System.Drawing.Size(23, 13);
            this.lblFile.TabIndex = 24;
            this.lblFile.Text = "File";
            // 
            // comboBoxFeatureType
            // 
            this.comboBoxFeatureType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxFeatureType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFeatureType.Enabled = false;
            this.comboBoxFeatureType.FormattingEnabled = true;
            this.comboBoxFeatureType.Location = new System.Drawing.Point(156, 23);
            this.comboBoxFeatureType.Name = "comboBoxFeatureType";
            this.comboBoxFeatureType.Size = new System.Drawing.Size(415, 21);
            this.comboBoxFeatureType.TabIndex = 18;
            this.comboBoxFeatureType.SelectedIndexChanged += new System.EventHandler(this.comboBoxFeatureType_SelectedIndexChanged);
            // 
            // btnSelectFile
            // 
            this.btnSelectFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectFile.Enabled = false;
            this.btnSelectFile.Location = new System.Drawing.Point(542, 50);
            this.btnSelectFile.Name = "btnSelectFile";
            this.btnSelectFile.Size = new System.Drawing.Size(29, 23);
            this.btnSelectFile.TabIndex = 15;
            this.btnSelectFile.Text = "...";
            this.btnSelectFile.UseVisualStyleBackColor = true;
            this.btnSelectFile.Click += new System.EventHandler(this.btnSelectFile_Click);
            // 
            // txtPath
            // 
            this.txtPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPath.Location = new System.Drawing.Point(156, 51);
            this.txtPath.Name = "txtPath";
            this.txtPath.ReadOnly = true;
            this.txtPath.Size = new System.Drawing.Size(375, 20);
            this.txtPath.TabIndex = 14;
            // 
            // groupBoxImportList
            // 
            this.groupBoxImportList.Controls.Add(this.tableViewImportList);
            this.groupBoxImportList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxImportList.Location = new System.Drawing.Point(0, 229);
            this.groupBoxImportList.Name = "groupBoxImportList";
            this.groupBoxImportList.Size = new System.Drawing.Size(710, 309);
            this.groupBoxImportList.TabIndex = 7;
            this.groupBoxImportList.TabStop = false;
            this.groupBoxImportList.Text = "Import features list";
            // 
            // tableViewImportList
            // 
            this.tableViewImportList.AllowAddNewRow = false;
            this.tableViewImportList.AllowDeleteRow = true;
            this.tableViewImportList.AutoGenerateColumns = true;
            this.tableViewImportList.ColumnAutoWidth = false;
            this.tableViewImportList.DisplayCellFilter = null;
            this.tableViewImportList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableViewImportList.HeaderHeigth = -1;
            this.tableViewImportList.InputValidator = null;
            this.tableViewImportList.InvalidCellBackgroundColor = System.Drawing.Color.Tomato;
            this.tableViewImportList.InvalidCellFilter = null;
            this.tableViewImportList.Location = new System.Drawing.Point(3, 16);
            this.tableViewImportList.MultipleCellEdit = true;
            this.tableViewImportList.MultiSelect = true;
            this.tableViewImportList.Name = "tableViewImportList";
            this.tableViewImportList.Padding = new System.Windows.Forms.Padding(10);
            this.tableViewImportList.ReadOnly = false;
            this.tableViewImportList.ReadOnlyCellBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
            this.tableViewImportList.ReadOnlyCellFilter = null;
            this.tableViewImportList.ReadOnlyCellForeColor = System.Drawing.Color.LightGray;
            this.tableViewImportList.RowSelect = false;
            this.tableViewImportList.RowValidator = null;
            this.tableViewImportList.ShowRowNumbers = false;
            this.tableViewImportList.Size = new System.Drawing.Size(704, 290);
            this.tableViewImportList.TabIndex = 8;
            this.tableViewImportList.UseCenteredHeaderText = false;
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // SelectDataWizardPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.groupBoxImportList);
            this.Controls.Add(this.groupBoxNetworkFeature);
            this.Name = "SelectDataWizardPage";
            this.Size = new System.Drawing.Size(710, 538);
            this.groupBoxNetworkFeature.ResumeLayout(false);
            this.groupBoxNetworkFeature.PerformLayout();
            this.groupBoxImportList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tableViewImportList)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxNetworkFeature;
        private System.Windows.Forms.Button btnSelectFile;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.ComboBox comboBoxFeatureType;
        private System.Windows.Forms.Label lblNumberOfLevels;
        private System.Windows.Forms.Label lblTable;
        private System.Windows.Forms.Label lblFile;
        private System.Windows.Forms.MaskedTextBox maskedTextBoxNumberOfLevels;
        private System.Windows.Forms.Label lblTypeOfFeature;
        private System.Windows.Forms.Label lblDiscriminatorValue;
        private System.Windows.Forms.Label lblDiscriminator;
        private System.Windows.Forms.Button btnAddToList;
        private System.Windows.Forms.ComboBox comboBoxDiscriminatorColumn;
        private System.Windows.Forms.ComboBox comboBoxTable;
        private System.Windows.Forms.GroupBox groupBoxImportList;
        private DelftTools.Controls.Swf.Table.TableView tableViewImportList;
        private System.Windows.Forms.ComboBox comboBoxDiscriminatorValue;
        private System.Windows.Forms.Button btnRelatedTables;
        private System.Windows.Forms.Button buttonLoadMappingFile;
        private System.Windows.Forms.ErrorProvider errorProvider1;
    }
}
