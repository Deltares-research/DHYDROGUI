namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView
{
    partial class SummerDikeView
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
            this.cbxHasSummerdike = new System.Windows.Forms.CheckBox();
            this.lblCrestLevel = new System.Windows.Forms.Label();
            this.lblFloodSurface = new System.Windows.Forms.Label();
            this.lblTotalSurface = new System.Windows.Forms.Label();
            this.lblFloodplainLevel = new System.Windows.Forms.Label();
            this.txtCrestLevel = new System.Windows.Forms.TextBox();
            this.txtFloodSurface = new System.Windows.Forms.TextBox();
            this.txtTotalSurface = new System.Windows.Forms.TextBox();
            this.txtFloodplainLevel = new System.Windows.Forms.TextBox();
            this.lblCrestLevelUnit = new System.Windows.Forms.Label();
            this.lblFloodSurfaceUnit = new System.Windows.Forms.Label();
            this.lblTotalSurfaceUnit = new System.Windows.Forms.Label();
            this.lblFloodplainLevelUnit = new System.Windows.Forms.Label();
            this.bindingSourceCrossSection = new System.Windows.Forms.BindingSource(this.components);
            this.editPanel = new System.Windows.Forms.Panel();
            this.errorPanel = new System.Windows.Forms.Panel();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceCrossSection)).BeginInit();
            this.editPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // cbxHasSummerdike
            // 
            this.cbxHasSummerdike.AutoSize = true;
            this.cbxHasSummerdike.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbxHasSummerdike.Location = new System.Drawing.Point(0, 0);
            this.cbxHasSummerdike.Name = "cbxHasSummerdike";
            this.cbxHasSummerdike.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.cbxHasSummerdike.Size = new System.Drawing.Size(297, 23);
            this.cbxHasSummerdike.TabIndex = 0;
            this.cbxHasSummerdike.Text = "Use summerdike";
            this.cbxHasSummerdike.UseVisualStyleBackColor = true;
            this.cbxHasSummerdike.CheckedChanged += new System.EventHandler(this.cbxHasSummerdike_CheckedChanged);
            // 
            // lblCrestLevel
            // 
            this.lblCrestLevel.AutoSize = true;
            this.lblCrestLevel.Location = new System.Drawing.Point(3, 3);
            this.lblCrestLevel.Name = "lblCrestLevel";
            this.lblCrestLevel.Size = new System.Drawing.Size(56, 13);
            this.lblCrestLevel.TabIndex = 1;
            this.lblCrestLevel.Text = "Crest level";
            // 
            // lblFloodSurface
            // 
            this.lblFloodSurface.AutoSize = true;
            this.lblFloodSurface.Location = new System.Drawing.Point(3, 28);
            this.lblFloodSurface.Name = "lblFloodSurface";
            this.lblFloodSurface.Size = new System.Drawing.Size(147, 13);
            this.lblFloodSurface.TabIndex = 2;
            this.lblFloodSurface.Text = "Flow area behind summerdike";
            // 
            // lblTotalSurface
            // 
            this.lblTotalSurface.AutoSize = true;
            this.lblTotalSurface.Location = new System.Drawing.Point(3, 54);
            this.lblTotalSurface.Name = "lblTotalSurface";
            this.lblTotalSurface.Size = new System.Drawing.Size(149, 13);
            this.lblTotalSurface.TabIndex = 3;
            this.lblTotalSurface.Text = "Total area behind summerdike";
            // 
            // lblFloodplainLevel
            // 
            this.lblFloodplainLevel.AutoSize = true;
            this.lblFloodplainLevel.Location = new System.Drawing.Point(3, 80);
            this.lblFloodplainLevel.Name = "lblFloodplainLevel";
            this.lblFloodplainLevel.Size = new System.Drawing.Size(106, 13);
            this.lblFloodplainLevel.TabIndex = 4;
            this.lblFloodplainLevel.Text = "Floodplain base level";
            // 
            // txtCrestLevel
            // 
            this.txtCrestLevel.Location = new System.Drawing.Point(172, 2);
            this.txtCrestLevel.Name = "txtCrestLevel";
            this.txtCrestLevel.Size = new System.Drawing.Size(62, 20);
            this.txtCrestLevel.TabIndex = 5;
            this.txtCrestLevel.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtFloodSurface
            // 
            this.txtFloodSurface.Location = new System.Drawing.Point(172, 27);
            this.txtFloodSurface.Name = "txtFloodSurface";
            this.txtFloodSurface.Size = new System.Drawing.Size(62, 20);
            this.txtFloodSurface.TabIndex = 6;
            this.txtFloodSurface.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtTotalSurface
            // 
            this.txtTotalSurface.Location = new System.Drawing.Point(172, 53);
            this.txtTotalSurface.Name = "txtTotalSurface";
            this.txtTotalSurface.Size = new System.Drawing.Size(62, 20);
            this.txtTotalSurface.TabIndex = 7;
            this.txtTotalSurface.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtFloodplainLevel
            // 
            this.txtFloodplainLevel.Location = new System.Drawing.Point(172, 79);
            this.txtFloodplainLevel.Name = "txtFloodplainLevel";
            this.txtFloodplainLevel.Size = new System.Drawing.Size(62, 20);
            this.txtFloodplainLevel.TabIndex = 8;
            this.txtFloodplainLevel.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // lblCrestLevelUnit
            // 
            this.lblCrestLevelUnit.AutoSize = true;
            this.lblCrestLevelUnit.Location = new System.Drawing.Point(242, 3);
            this.lblCrestLevelUnit.Name = "lblCrestLevelUnit";
            this.lblCrestLevelUnit.Size = new System.Drawing.Size(15, 13);
            this.lblCrestLevelUnit.TabIndex = 9;
            this.lblCrestLevelUnit.Text = "m";
            // 
            // lblFloodSurfaceUnit
            // 
            this.lblFloodSurfaceUnit.AutoSize = true;
            this.lblFloodSurfaceUnit.Location = new System.Drawing.Point(242, 28);
            this.lblFloodSurfaceUnit.Name = "lblFloodSurfaceUnit";
            this.lblFloodSurfaceUnit.Size = new System.Drawing.Size(21, 13);
            this.lblFloodSurfaceUnit.TabIndex = 10;
            this.lblFloodSurfaceUnit.Text = "m2";
            // 
            // lblTotalSurfaceUnit
            // 
            this.lblTotalSurfaceUnit.AutoSize = true;
            this.lblTotalSurfaceUnit.Location = new System.Drawing.Point(241, 54);
            this.lblTotalSurfaceUnit.Name = "lblTotalSurfaceUnit";
            this.lblTotalSurfaceUnit.Size = new System.Drawing.Size(21, 13);
            this.lblTotalSurfaceUnit.TabIndex = 11;
            this.lblTotalSurfaceUnit.Text = "m2";
            // 
            // lblFloodplainLevelUnit
            // 
            this.lblFloodplainLevelUnit.AutoSize = true;
            this.lblFloodplainLevelUnit.Location = new System.Drawing.Point(241, 80);
            this.lblFloodplainLevelUnit.Name = "lblFloodplainLevelUnit";
            this.lblFloodplainLevelUnit.Size = new System.Drawing.Size(15, 13);
            this.lblFloodplainLevelUnit.TabIndex = 12;
            this.lblFloodplainLevelUnit.Text = "m";
            // 
            // bindingSourceCrossSection
            // 
            this.bindingSourceCrossSection.AllowNew = false;
            // 
            // editPanel
            // 
            this.editPanel.Controls.Add(this.errorPanel);
            this.editPanel.Controls.Add(this.lblCrestLevel);
            this.editPanel.Controls.Add(this.lblFloodplainLevelUnit);
            this.editPanel.Controls.Add(this.lblFloodSurface);
            this.editPanel.Controls.Add(this.lblTotalSurfaceUnit);
            this.editPanel.Controls.Add(this.lblTotalSurface);
            this.editPanel.Controls.Add(this.lblFloodSurfaceUnit);
            this.editPanel.Controls.Add(this.lblFloodplainLevel);
            this.editPanel.Controls.Add(this.lblCrestLevelUnit);
            this.editPanel.Controls.Add(this.txtCrestLevel);
            this.editPanel.Controls.Add(this.txtFloodplainLevel);
            this.editPanel.Controls.Add(this.txtFloodSurface);
            this.editPanel.Controls.Add(this.txtTotalSurface);
            this.editPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.editPanel.Location = new System.Drawing.Point(0, 23);
            this.editPanel.Name = "editPanel";
            this.editPanel.Size = new System.Drawing.Size(297, 105);
            this.editPanel.TabIndex = 13;
            // 
            // errorPanel
            // 
            this.errorPanel.Location = new System.Drawing.Point(281, 30);
            this.errorPanel.Name = "errorPanel";
            this.errorPanel.Size = new System.Drawing.Size(11, 39);
            this.errorPanel.TabIndex = 14;
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // SummerDikeView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.editPanel);
            this.Controls.Add(this.cbxHasSummerdike);
            this.MinimumSize = new System.Drawing.Size(279, 128);
            this.Name = "SummerDikeView";
            this.Size = new System.Drawing.Size(297, 128);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceCrossSection)).EndInit();
            this.editPanel.ResumeLayout(false);
            this.editPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblCrestLevel;
        private System.Windows.Forms.Label lblFloodSurface;
        private System.Windows.Forms.Label lblTotalSurface;
        private System.Windows.Forms.Label lblFloodplainLevel;
        private System.Windows.Forms.Label lblCrestLevelUnit;
        private System.Windows.Forms.Label lblFloodSurfaceUnit;
        private System.Windows.Forms.Label lblTotalSurfaceUnit;
        private System.Windows.Forms.Label lblFloodplainLevelUnit;
        private System.Windows.Forms.BindingSource bindingSourceCrossSection;
        public System.Windows.Forms.TextBox txtCrestLevel;
        public System.Windows.Forms.TextBox txtFloodSurface;
        public System.Windows.Forms.TextBox txtTotalSurface;
        public System.Windows.Forms.TextBox txtFloodplainLevel;
        public System.Windows.Forms.CheckBox cbxHasSummerdike;
        private System.Windows.Forms.Panel editPanel;
        private System.Windows.Forms.ErrorProvider errorProvider;
        private System.Windows.Forms.Panel errorPanel;
    }
}
