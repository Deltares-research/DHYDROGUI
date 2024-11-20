namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView
{
    partial class ZWSectionsView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ZWSectionsView));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxMain = new System.Windows.Forms.TextBox();
            this.viewModelBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.textBoxFloodplain1 = new System.Windows.Forms.TextBox();
            this.textBoxFloodplain2 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.addSectionTypeMain = new System.Windows.Forms.Button();
            this.addSectionTypeFp1 = new System.Windows.Forms.Button();
            this.addSectionTypeFp2 = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.viewModelBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tableLayoutPanel1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(332, 111);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Section Widths";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 96F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.textBoxMain, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.textBoxFloodplain1, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.textBoxFloodplain2, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.label4, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.label5, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.label6, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.addSectionTypeMain, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.addSectionTypeFp1, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.addSectionTypeFp2, 3, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(4, 19);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(324, 88);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 6);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Main";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label3.Location = new System.Drawing.Point(4, 64);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 17);
            this.label3.TabIndex = 2;
            this.label3.Text = "FloodPlain2";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 35);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 17);
            this.label2.TabIndex = 1;
            this.label2.Text = "FloodPlain1 ";
            // 
            // textBoxMain
            // 
            this.textBoxMain.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.viewModelBindingSource, "MainWidth", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxMain.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.viewModelBindingSource, "MainEnabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.textBoxMain.Location = new System.Drawing.Point(164, 4);
            this.textBoxMain.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxMain.Name = "textBoxMain";
            this.textBoxMain.Size = new System.Drawing.Size(84, 22);
            this.textBoxMain.TabIndex = 3;
            // 
            // viewModelBindingSource
            // 
            this.viewModelBindingSource.DataSource = typeof(ZWSectionsViewModel);
            this.viewModelBindingSource.CurrentChanged += new System.EventHandler(this.viewModelBindingSource_CurrentChanged);
            this.viewModelBindingSource.BindingComplete += new System.Windows.Forms.BindingCompleteEventHandler(this.viewModelBindingSource_BindingComplete);
            // 
            // textBoxFloodplain1
            // 
            this.textBoxFloodplain1.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.viewModelBindingSource, "FloodPlain1Width", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxFloodplain1.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.viewModelBindingSource, "FloodPlain1Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.textBoxFloodplain1.Location = new System.Drawing.Point(164, 33);
            this.textBoxFloodplain1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxFloodplain1.Name = "textBoxFloodplain1";
            this.textBoxFloodplain1.Size = new System.Drawing.Size(84, 22);
            this.textBoxFloodplain1.TabIndex = 4;
            // 
            // textBoxFloodplain2
            // 
            this.textBoxFloodplain2.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.viewModelBindingSource, "FloodPlain2Width", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxFloodplain2.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.viewModelBindingSource, "FloodPlain2Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.textBoxFloodplain2.Location = new System.Drawing.Point(164, 62);
            this.textBoxFloodplain2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxFloodplain2.Name = "textBoxFloodplain2";
            this.textBoxFloodplain2.Size = new System.Drawing.Size(84, 22);
            this.textBoxFloodplain2.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(260, 6);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(19, 17);
            this.label4.TabIndex = 6;
            this.label4.Text = "m";
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(260, 35);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(19, 17);
            this.label5.TabIndex = 7;
            this.label5.Text = "m";
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(260, 64);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(19, 17);
            this.label6.TabIndex = 8;
            this.label6.Text = "m";
            // 
            // addSectionTypeMain
            // 
            this.addSectionTypeMain.DataBindings.Add(new System.Windows.Forms.Binding("Visible", this.viewModelBindingSource, "MainCanAdd", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.addSectionTypeMain.FlatAppearance.BorderSize = 0;
            this.addSectionTypeMain.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.addSectionTypeMain.Image = ((System.Drawing.Image)(resources.GetObject("addSectionTypeMain.Image")));
            this.addSectionTypeMain.Location = new System.Drawing.Point(295, 4);
            this.addSectionTypeMain.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.addSectionTypeMain.Name = "addSectionTypeMain";
            this.addSectionTypeMain.Size = new System.Drawing.Size(23, 21);
            this.addSectionTypeMain.TabIndex = 4;
            this.addSectionTypeMain.UseVisualStyleBackColor = true;
            this.addSectionTypeMain.Click += new System.EventHandler(this.addSectionTypeMain_Click);
            // 
            // addSectionTypeFp1
            // 
            this.addSectionTypeFp1.DataBindings.Add(new System.Windows.Forms.Binding("Visible", this.viewModelBindingSource, "FloodPlain1CanAdd", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.addSectionTypeFp1.FlatAppearance.BorderSize = 0;
            this.addSectionTypeFp1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.addSectionTypeFp1.Image = ((System.Drawing.Image)(resources.GetObject("addSectionTypeFp1.Image")));
            this.addSectionTypeFp1.Location = new System.Drawing.Point(295, 33);
            this.addSectionTypeFp1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.addSectionTypeFp1.Name = "addSectionTypeFp1";
            this.addSectionTypeFp1.Size = new System.Drawing.Size(23, 21);
            this.addSectionTypeFp1.TabIndex = 9;
            this.addSectionTypeFp1.UseVisualStyleBackColor = true;
            this.addSectionTypeFp1.Click += new System.EventHandler(this.addSectionTypeFp1_Click);
            // 
            // addSectionTypeFp2
            // 
            this.addSectionTypeFp2.DataBindings.Add(new System.Windows.Forms.Binding("Visible", this.viewModelBindingSource, "FloodPlain2CanAdd", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.addSectionTypeFp2.FlatAppearance.BorderSize = 0;
            this.addSectionTypeFp2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.addSectionTypeFp2.Image = ((System.Drawing.Image)(resources.GetObject("addSectionTypeFp2.Image")));
            this.addSectionTypeFp2.Location = new System.Drawing.Point(295, 62);
            this.addSectionTypeFp2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.addSectionTypeFp2.Name = "addSectionTypeFp2";
            this.addSectionTypeFp2.Size = new System.Drawing.Size(23, 21);
            this.addSectionTypeFp2.TabIndex = 10;
            this.addSectionTypeFp2.UseVisualStyleBackColor = true;
            this.addSectionTypeFp2.Click += new System.EventHandler(this.addSectionTypeFp2_Click);
            // 
            // toolTip
            // 
            this.toolTip.IsBalloon = true;
            // 
            // ZWSectionsView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximumSize = new System.Drawing.Size(332, 126);
            this.Name = "ZWSectionsView";
            this.Size = new System.Drawing.Size(332, 111);
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.viewModelBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox textBoxMain;
        private System.Windows.Forms.TextBox textBoxFloodplain1;
        private System.Windows.Forms.TextBox textBoxFloodplain2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.BindingSource viewModelBindingSource;
        private System.Windows.Forms.Button addSectionTypeMain;
        private System.Windows.Forms.Button addSectionTypeFp1;
        private System.Windows.Forms.Button addSectionTypeFp2;
        private System.Windows.Forms.ToolTip toolTip;

    }
}
