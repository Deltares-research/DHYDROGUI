namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    partial class LateralSourceMappingPage
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rbQ = new System.Windows.Forms.RadioButton();
            this.rbQH = new System.Windows.Forms.RadioButton();
            this.rbQT = new System.Windows.Forms.RadioButton();
            this.rbHT = new System.Windows.Forms.RadioButton();
            this.rbH = new System.Windows.Forms.RadioButton();
            this.csvDataSelectionControl = new DelftTools.Controls.Swf.Csv.CsvDataSelectionControl();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rbQ);
            this.groupBox1.Controls.Add(this.rbQH);
            this.groupBox1.Controls.Add(this.rbQT);
            this.groupBox1.Controls.Add(this.rbHT);
            this.groupBox1.Controls.Add(this.rbH);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(527, 76);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Select data type";
            // 
            // rbQ
            // 
            this.rbQ.AutoSize = true;
            this.rbQ.Location = new System.Drawing.Point(105, 23);
            this.rbQ.Name = "rbQ";
            this.rbQ.Size = new System.Drawing.Size(83, 17);
            this.rbQ.TabIndex = 4;
            this.rbQ.Text = "Q (constant)";
            this.rbQ.UseVisualStyleBackColor = true;
            // 
            // rbQH
            // 
            this.rbQH.AutoSize = true;
            this.rbQH.Location = new System.Drawing.Point(54, 23);
            this.rbQH.Name = "rbQH";
            this.rbQH.Size = new System.Drawing.Size(45, 17);
            this.rbQH.TabIndex = 3;
            this.rbQH.Text = "Q(h)";
            this.rbQH.UseVisualStyleBackColor = true;
            // 
            // rbQT
            // 
            this.rbQT.AutoSize = true;
            this.rbQT.Checked = true;
            this.rbQT.Location = new System.Drawing.Point(6, 23);
            this.rbQT.Name = "rbQT";
            this.rbQT.Size = new System.Drawing.Size(42, 17);
            this.rbQT.TabIndex = 2;
            this.rbQT.TabStop = true;
            this.rbQT.Text = "Q(t)";
            this.rbQT.UseVisualStyleBackColor = true;
            // 
            // rbHT
            // 
            this.rbHT.AutoSize = true;
            this.rbHT.Location = new System.Drawing.Point(6, 46);
            this.rbHT.Name = "rbHT";
            this.rbHT.Size = new System.Drawing.Size(42, 17);
            this.rbHT.TabIndex = 1;
            this.rbHT.Text = "H(t)";
            this.rbHT.UseVisualStyleBackColor = true;
            // 
            // rbH
            // 
            this.rbH.AutoSize = true;
            this.rbH.Location = new System.Drawing.Point(105, 46);
            this.rbH.Name = "rbH";
            this.rbH.Size = new System.Drawing.Size(83, 17);
            this.rbH.TabIndex = 0;
            this.rbH.Text = "H (constant)";
            this.rbH.UseVisualStyleBackColor = true;
            // 
            // csvDataSelectionControl
            // 
            this.csvDataSelectionControl.ColumnSelectionVisible = true;
            this.csvDataSelectionControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.csvDataSelectionControl.FilteringVisible = true;
            this.csvDataSelectionControl.Location = new System.Drawing.Point(0, 76);
            this.csvDataSelectionControl.Name = "csvDataSelectionControl";
            this.csvDataSelectionControl.Size = new System.Drawing.Size(527, 455);
            this.csvDataSelectionControl.TabIndex = 1;
            // 
            // FlowTimeSeriesCsvMappingPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.csvDataSelectionControl);
            this.Controls.Add(this.groupBox1);
            this.Name = "FlowTimeSeriesCsvMappingPage";
            this.Size = new System.Drawing.Size(527, 531);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private DelftTools.Controls.Swf.Csv.CsvDataSelectionControl csvDataSelectionControl;
        public System.Windows.Forms.RadioButton rbQH;
        public System.Windows.Forms.RadioButton rbHT;
        public System.Windows.Forms.RadioButton rbH;
        public System.Windows.Forms.RadioButton rbQ;
        public System.Windows.Forms.RadioButton rbQT;
    }
}
