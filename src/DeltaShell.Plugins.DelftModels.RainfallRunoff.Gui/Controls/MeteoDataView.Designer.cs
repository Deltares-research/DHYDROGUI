namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls
{
    partial class MeteoDataView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        
        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MeteoDataView));
            this.pnlSelect = new System.Windows.Forms.Panel();
            this.generateBtn = new System.Windows.Forms.Button();
            this.cmbMeteoDataType = new System.Windows.Forms.ComboBox();
            this.lblMeteoDataType = new System.Windows.Forms.Label();
            this.pnlLine = new System.Windows.Forms.Panel();
            this.pnlView = new System.Windows.Forms.Panel();
            this.stationsListEditor = new DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls.MeteoStationsListEditor();
            this.pnlSelect.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlSelect
            // 
            this.pnlSelect.Controls.Add(this.generateBtn);
            this.pnlSelect.Controls.Add(this.cmbMeteoDataType);
            this.pnlSelect.Controls.Add(this.lblMeteoDataType);
            this.pnlSelect.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSelect.Location = new System.Drawing.Point(0, 0);
            this.pnlSelect.Name = "pnlSelect";
            this.pnlSelect.Size = new System.Drawing.Size(1171, 30);
            this.pnlSelect.TabIndex = 0;
            // 
            // generateBtn
            // 
            this.generateBtn.Image = ((System.Drawing.Image)(resources.GetObject("generateBtn.Image")));
            this.generateBtn.Location = new System.Drawing.Point(3, 3);
            this.generateBtn.Name = "generateBtn";
            this.generateBtn.Size = new System.Drawing.Size(205, 25);
            this.generateBtn.TabIndex = 2;
            this.generateBtn.Text = "Generate / modify time series...";
            this.generateBtn.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.generateBtn.UseVisualStyleBackColor = true;
            this.generateBtn.Click += new System.EventHandler(this.GenerateBtnClick);
            // 
            // cmbMeteoDataType
            // 
            this.cmbMeteoDataType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbMeteoDataType.FormattingEnabled = true;
            this.cmbMeteoDataType.Location = new System.Drawing.Point(956, 4);
            this.cmbMeteoDataType.Name = "cmbMeteoDataType";
            this.cmbMeteoDataType.Size = new System.Drawing.Size(206, 21);
            this.cmbMeteoDataType.TabIndex = 1;
            // 
            // lblMeteoDataType
            // 
            this.lblMeteoDataType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMeteoDataType.AutoSize = true;
            this.lblMeteoDataType.Location = new System.Drawing.Point(787, 7);
            this.lblMeteoDataType.Name = "lblMeteoDataType";
            this.lblMeteoDataType.Size = new System.Drawing.Size(138, 13);
            this.lblMeteoDataType.TabIndex = 0;
            this.lblMeteoDataType.Text = "Type of meteorological data";
            // 
            // pnlLine
            // 
            this.pnlLine.BackColor = System.Drawing.Color.DimGray;
            this.pnlLine.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlLine.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLine.Location = new System.Drawing.Point(0, 30);
            this.pnlLine.Name = "pnlLine";
            this.pnlLine.Size = new System.Drawing.Size(1171, 1);
            this.pnlLine.TabIndex = 1;
            // 
            // pnlView
            // 
            this.pnlView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlView.Location = new System.Drawing.Point(172, 31);
            this.pnlView.Name = "pnlView";
            this.pnlView.Size = new System.Drawing.Size(999, 630);
            this.pnlView.TabIndex = 2;
            // 
            // stationsListEditor
            // 
            this.stationsListEditor.Data = null;
            this.stationsListEditor.Dock = System.Windows.Forms.DockStyle.Left;
            this.stationsListEditor.Location = new System.Drawing.Point(0, 31);
            this.stationsListEditor.Name = "stationsListEditor";
            this.stationsListEditor.Size = new System.Drawing.Size(172, 630);
            this.stationsListEditor.TabIndex = 0;
            // 
            // MeteoDataView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlView);
            this.Controls.Add(this.stationsListEditor);
            this.Controls.Add(this.pnlLine);
            this.Controls.Add(this.pnlSelect);
            this.Name = "MeteoDataView";
            this.Size = new System.Drawing.Size(1171, 661);
            this.pnlSelect.ResumeLayout(false);
            this.pnlSelect.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlSelect;
        private System.Windows.Forms.Panel pnlLine;
        private System.Windows.Forms.Panel pnlView;
        private System.Windows.Forms.ComboBox cmbMeteoDataType;
        private System.Windows.Forms.Label lblMeteoDataType;
        private System.Windows.Forms.Button generateBtn;
        private MeteoStationsListEditor stationsListEditor;
    }
}
