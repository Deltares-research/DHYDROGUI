namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.StandardCrossSections
{
    partial class CrossSectionStandardDataView
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
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxShapeType = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panelDataView = new System.Windows.Forms.Panel();
            this.labelLevelShift = new System.Windows.Forms.Label();
            this.textBoxLevelShift = new System.Windows.Forms.TextBox();
            this.bindingSourceStandardDefinition = new System.Windows.Forms.BindingSource(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceStandardDefinition)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Type";
            // 
            // comboBoxShapeType
            // 
            this.comboBoxShapeType.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.comboBoxShapeType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxShapeType.FormattingEnabled = true;
            this.comboBoxShapeType.Location = new System.Drawing.Point(65, 4);
            this.comboBoxShapeType.Name = "comboBoxShapeType";
            this.comboBoxShapeType.Size = new System.Drawing.Size(121, 21);
            this.comboBoxShapeType.TabIndex = 1;
            this.comboBoxShapeType.SelectedIndexChanged += new System.EventHandler(this.ComboBoxShapeTypeSelectedIndexChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22.96296F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 77.03704F));
            this.tableLayoutPanel1.Controls.Add(this.labelLevelShift, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.comboBoxShapeType, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.textBoxLevelShift, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(270, 59);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // panelDataView
            // 
            this.panelDataView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDataView.Location = new System.Drawing.Point(0, 59);
            this.panelDataView.Name = "panelDataView";
            this.panelDataView.Size = new System.Drawing.Size(270, 91);
            this.panelDataView.TabIndex = 2;
            // 
            // labelLevelShift
            // 
            this.labelLevelShift.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelLevelShift.AutoSize = true;
            this.labelLevelShift.Location = new System.Drawing.Point(3, 37);
            this.labelLevelShift.Name = "labelLevelShift";
            this.labelLevelShift.Size = new System.Drawing.Size(55, 13);
            this.labelLevelShift.TabIndex = 2;
            this.labelLevelShift.Text = "Level shift";
            // 
            // textBoxLevelShift
            // 
            this.textBoxLevelShift.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.textBoxLevelShift.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceStandardDefinition, "LevelShift", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxLevelShift.Location = new System.Drawing.Point(65, 34);
            this.textBoxLevelShift.Name = "textBoxLevelShift";
            this.textBoxLevelShift.Size = new System.Drawing.Size(100, 20);
            this.textBoxLevelShift.TabIndex = 3;
            // 
            // bindingSourceStandardDefinition
            // 
            this.bindingSourceStandardDefinition.DataSource = typeof(DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.StandardCrossSections.CrossSectionDefinitionStandardViewModel);
            // 
            // CrossSectionStandardDataView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelDataView);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MinimumSize = new System.Drawing.Size(270, 0);
            this.Name = "CrossSectionStandardDataView";
            this.Size = new System.Drawing.Size(270, 150);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceStandardDefinition)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxShapeType;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panelDataView;
        private System.Windows.Forms.Label labelLevelShift;
        private System.Windows.Forms.TextBox textBoxLevelShift;
        private System.Windows.Forms.BindingSource bindingSourceStandardDefinition;

    }
}
