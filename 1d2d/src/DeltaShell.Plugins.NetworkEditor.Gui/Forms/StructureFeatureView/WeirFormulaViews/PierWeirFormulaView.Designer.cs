namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView.WeirFormulaViews
{
    partial class PierWeirFormulaView
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
            this.textBoxAbutmentContractionPos = new System.Windows.Forms.TextBox();
            this.bindingSourcePierWeirFormula = new System.Windows.Forms.BindingSource(this.components);
            this.labelNumberOfPiers = new System.Windows.Forms.Label();
            this.textBoxNumberOfPiers = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxDesignHeadPos = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.textBoxUpstreamFacePos = new System.Windows.Forms.TextBox();
            this.textBoxPierContractionPos = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxAbutmentContractionNeg = new System.Windows.Forms.TextBox();
            this.textBoxDesignHeadNeg = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.textBoxUpstreamFaceNeg = new System.Windows.Forms.TextBox();
            this.textBoxPierContractionNeg = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourcePierWeirFormula)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 124);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(225, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Abutment contraction coefficient Ka";
            // 
            // textBoxAbutmentContractionPos
            // 
            this.textBoxAbutmentContractionPos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxAbutmentContractionPos.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePierWeirFormula, "AbutmentContractionPos", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxAbutmentContractionPos.Location = new System.Drawing.Point(234, 120);
            this.textBoxAbutmentContractionPos.Name = "textBoxAbutmentContractionPos";
            this.textBoxAbutmentContractionPos.Size = new System.Drawing.Size(57, 20);
            this.textBoxAbutmentContractionPos.TabIndex = 1;
            this.textBoxAbutmentContractionPos.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // bindingSourcePierWeirFormula
            // 
            this.bindingSourcePierWeirFormula.DataSource = typeof(DelftTools.Hydro.Structures.WeirFormula.PierWeirFormula);
            // 
            // labelNumberOfPiers
            // 
            this.labelNumberOfPiers.AutoSize = true;
            this.labelNumberOfPiers.Location = new System.Drawing.Point(14, 6);
            this.labelNumberOfPiers.Name = "labelNumberOfPiers";
            this.labelNumberOfPiers.Size = new System.Drawing.Size(81, 13);
            this.labelNumberOfPiers.TabIndex = 0;
            this.labelNumberOfPiers.Text = "Number of piers";
            // 
            // textBoxNumberOfPiers
            // 
            this.textBoxNumberOfPiers.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePierWeirFormula, "NumberOfPiers", true));
            this.textBoxNumberOfPiers.Location = new System.Drawing.Point(158, 3);
            this.textBoxNumberOfPiers.Name = "textBoxNumberOfPiers";
            this.textBoxNumberOfPiers.Size = new System.Drawing.Size(100, 20);
            this.textBoxNumberOfPiers.TabIndex = 1;
            this.textBoxNumberOfPiers.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 66);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(225, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "Design head of weir flow H0 ";
            // 
            // textBoxDesignHeadPos
            // 
            this.textBoxDesignHeadPos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDesignHeadPos.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePierWeirFormula, "DesignHeadPos", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxDesignHeadPos.Location = new System.Drawing.Point(234, 62);
            this.textBoxDesignHeadPos.Name = "textBoxDesignHeadPos";
            this.textBoxDesignHeadPos.Size = new System.Drawing.Size(57, 20);
            this.textBoxDesignHeadPos.TabIndex = 1;
            this.textBoxDesignHeadPos.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 37);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(225, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Upstream face  P";
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 95);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(225, 13);
            this.label8.TabIndex = 0;
            this.label8.Text = "Pier contraction coefficient Kp";
            // 
            // textBoxUpstreamFacePos
            // 
            this.textBoxUpstreamFacePos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxUpstreamFacePos.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePierWeirFormula, "UpstreamFacePos", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxUpstreamFacePos.Location = new System.Drawing.Point(234, 33);
            this.textBoxUpstreamFacePos.Name = "textBoxUpstreamFacePos";
            this.textBoxUpstreamFacePos.Size = new System.Drawing.Size(57, 20);
            this.textBoxUpstreamFacePos.TabIndex = 1;
            this.textBoxUpstreamFacePos.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxPierContractionPos
            // 
            this.textBoxPierContractionPos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPierContractionPos.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePierWeirFormula, "PierContractionPos", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxPierContractionPos.Location = new System.Drawing.Point(234, 91);
            this.textBoxPierContractionPos.Name = "textBoxPierContractionPos";
            this.textBoxPierContractionPos.Size = new System.Drawing.Size(57, 20);
            this.textBoxPierContractionPos.TabIndex = 1;
            this.textBoxPierContractionPos.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(234, 8);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(57, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Flow";
            // 
            // textBoxAbutmentContractionNeg
            // 
            this.textBoxAbutmentContractionNeg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxAbutmentContractionNeg.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePierWeirFormula, "AbutmentContractionNeg", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxAbutmentContractionNeg.Location = new System.Drawing.Point(297, 120);
            this.textBoxAbutmentContractionNeg.Name = "textBoxAbutmentContractionNeg";
            this.textBoxAbutmentContractionNeg.Size = new System.Drawing.Size(57, 20);
            this.textBoxAbutmentContractionNeg.TabIndex = 1;
            this.textBoxAbutmentContractionNeg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxDesignHeadNeg
            // 
            this.textBoxDesignHeadNeg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDesignHeadNeg.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePierWeirFormula, "DesignHeadNeg", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxDesignHeadNeg.Location = new System.Drawing.Point(297, 62);
            this.textBoxDesignHeadNeg.Name = "textBoxDesignHeadNeg";
            this.textBoxDesignHeadNeg.Size = new System.Drawing.Size(57, 20);
            this.textBoxDesignHeadNeg.TabIndex = 1;
            this.textBoxDesignHeadNeg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(297, 8);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(57, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "Reverse";
            // 
            // textBoxUpstreamFaceNeg
            // 
            this.textBoxUpstreamFaceNeg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxUpstreamFaceNeg.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePierWeirFormula, "UpstreamFaceNeg", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxUpstreamFaceNeg.Location = new System.Drawing.Point(297, 33);
            this.textBoxUpstreamFaceNeg.Name = "textBoxUpstreamFaceNeg";
            this.textBoxUpstreamFaceNeg.Size = new System.Drawing.Size(57, 20);
            this.textBoxUpstreamFaceNeg.TabIndex = 1;
            this.textBoxUpstreamFaceNeg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxPierContractionNeg
            // 
            this.textBoxPierContractionNeg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPierContractionNeg.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePierWeirFormula, "PierContractionNeg", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxPierContractionNeg.Location = new System.Drawing.Point(297, 91);
            this.textBoxPierContractionNeg.Name = "textBoxPierContractionNeg";
            this.textBoxPierContractionNeg.Size = new System.Drawing.Size(57, 20);
            this.textBoxPierContractionNeg.TabIndex = 1;
            this.textBoxPierContractionNeg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.Controls.Add(this.label5, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label8, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.textBoxPierContractionNeg, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label7, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label9, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.textBoxPierContractionPos, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.textBoxUpstreamFacePos, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.textBoxUpstreamFaceNeg, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.textBoxDesignHeadPos, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.textBoxDesignHeadNeg, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.textBoxAbutmentContractionPos, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.textBoxAbutmentContractionNeg, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.label2, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.label3, 3, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(11, 29);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(420, 145);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(360, 37);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "m";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(360, 66);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "m";
            // 
            // PierWeirFormulaView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.textBoxNumberOfPiers);
            this.Controls.Add(this.labelNumberOfPiers);
            this.Name = "PierWeirFormulaView";
            this.Size = new System.Drawing.Size(455, 195);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourcePierWeirFormula)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxAbutmentContractionPos;
        private System.Windows.Forms.Label labelNumberOfPiers;
        private System.Windows.Forms.TextBox textBoxNumberOfPiers;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxDesignHeadPos;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBoxUpstreamFacePos;
        private System.Windows.Forms.TextBox textBoxPierContractionPos;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxAbutmentContractionNeg;
        private System.Windows.Forms.TextBox textBoxDesignHeadNeg;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textBoxUpstreamFaceNeg;
        private System.Windows.Forms.TextBox textBoxPierContractionNeg;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.BindingSource bindingSourcePierWeirFormula;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;

    }
}