namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView.WeirFormulaViews
{
    partial class RiverWeirFormulaView
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
            this.textBoxCorrectionPos = new System.Windows.Forms.TextBox();
            this.bindingSourceRiverWeir = new System.Windows.Forms.BindingSource(this.components);
            this.textBoxSubmergePos = new System.Windows.Forms.TextBox();
            this.textBoxCorrectionNeg = new System.Windows.Forms.TextBox();
            this.textBoxSubmergeNeg = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.buttonFlowReduction = new System.Windows.Forms.Button();
            this.buttonReductionReverse = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceRiverWeir)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxCorrectionPos
            // 
            this.textBoxCorrectionPos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxCorrectionPos.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceRiverWeir, "CorrectionCoefficientPos", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxCorrectionPos.Location = new System.Drawing.Point(113, 32);
            this.textBoxCorrectionPos.Name = "textBoxCorrectionPos";
            this.textBoxCorrectionPos.Size = new System.Drawing.Size(49, 20);
            this.textBoxCorrectionPos.TabIndex = 0;
            this.textBoxCorrectionPos.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // bindingSourceRiverWeir
            // 
            this.bindingSourceRiverWeir.DataSource = typeof(DelftTools.Hydro.Structures.WeirFormula.RiverWeirFormula);
            // 
            // textBoxSubmergePos
            // 
            this.textBoxSubmergePos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSubmergePos.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceRiverWeir, "SubmergeLimitPos", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxSubmergePos.Location = new System.Drawing.Point(113, 60);
            this.textBoxSubmergePos.Name = "textBoxSubmergePos";
            this.textBoxSubmergePos.Size = new System.Drawing.Size(49, 20);
            this.textBoxSubmergePos.TabIndex = 0;
            this.textBoxSubmergePos.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxCorrectionNeg
            // 
            this.textBoxCorrectionNeg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxCorrectionNeg.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceRiverWeir, "CorrectionCoefficientNeg", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxCorrectionNeg.Location = new System.Drawing.Point(168, 32);
            this.textBoxCorrectionNeg.Name = "textBoxCorrectionNeg";
            this.textBoxCorrectionNeg.Size = new System.Drawing.Size(49, 20);
            this.textBoxCorrectionNeg.TabIndex = 0;
            this.textBoxCorrectionNeg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxSubmergeNeg
            // 
            this.textBoxSubmergeNeg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSubmergeNeg.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceRiverWeir, "SubmergeLimitNeg", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxSubmergeNeg.Location = new System.Drawing.Point(168, 60);
            this.textBoxSubmergeNeg.Name = "textBoxSubmergeNeg";
            this.textBoxSubmergeNeg.Size = new System.Drawing.Size(49, 20);
            this.textBoxSubmergeNeg.TabIndex = 0;
            this.textBoxSubmergeNeg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(104, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Correction";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(104, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Submerge";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 91);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(104, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Reduction";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.textBoxSubmergePos, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.textBoxSubmergeNeg, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.textBoxCorrectionNeg, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.textBoxCorrectionPos, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label4, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label5, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonFlowReduction, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.buttonReductionReverse, 2, 3);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(11, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(220, 112);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(113, 7);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(49, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Flow";
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(168, 7);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(49, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Reverse";
            // 
            // buttonFlowReduction
            // 
            this.buttonFlowReduction.Location = new System.Drawing.Point(113, 87);
            this.buttonFlowReduction.Name = "buttonFlowReduction";
            this.buttonFlowReduction.Size = new System.Drawing.Size(49, 22);
            this.buttonFlowReduction.TabIndex = 2;
            this.buttonFlowReduction.Text = "...";
            this.buttonFlowReduction.UseVisualStyleBackColor = true;
            this.buttonFlowReduction.Click += new System.EventHandler(this.buttonFlowReduction_Click);
            // 
            // buttonReductionReverse
            // 
            this.buttonReductionReverse.Location = new System.Drawing.Point(168, 87);
            this.buttonReductionReverse.Name = "buttonReductionReverse";
            this.buttonReductionReverse.Size = new System.Drawing.Size(49, 22);
            this.buttonReductionReverse.TabIndex = 3;
            this.buttonReductionReverse.Text = "...";
            this.buttonReductionReverse.UseVisualStyleBackColor = true;
            this.buttonReductionReverse.Click += new System.EventHandler(this.buttonReductionReverse_Click);
            // 
            // RiverWeirFormulaView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "RiverWeirFormulaView";
            this.Size = new System.Drawing.Size(239, 126);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceRiverWeir)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxCorrectionPos;
        private System.Windows.Forms.TextBox textBoxSubmergePos;
        private System.Windows.Forms.TextBox textBoxCorrectionNeg;
        private System.Windows.Forms.TextBox textBoxSubmergeNeg;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.BindingSource bindingSourceRiverWeir;
        private System.Windows.Forms.Button buttonFlowReduction;
        private System.Windows.Forms.Button buttonReductionReverse;
    }
}