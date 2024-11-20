namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView.WeirFormulaViews
{
    partial class GatedWeirFormulaView
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
            this.textBoxLateralContraction = new System.Windows.Forms.TextBox();
            this.bindingSourceGatedWeirFormula = new System.Windows.Forms.BindingSource(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxContractionCoefficient = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceGatedWeirFormula)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxLateralContraction
            // 
            this.textBoxLateralContraction.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGatedWeirFormula, "LateralContraction", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxLateralContraction.Location = new System.Drawing.Point(155, 34);
            this.textBoxLateralContraction.Name = "textBoxLateralContraction";
            this.textBoxLateralContraction.Size = new System.Drawing.Size(100, 20);
            this.textBoxLateralContraction.TabIndex = 7;
            this.textBoxLateralContraction.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // bindingSourceGatedWeirFormula
            // 
            this.bindingSourceGatedWeirFormula.DataSource = typeof(DelftTools.Hydro.Structures.WeirFormula.GatedWeirFormula);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(114, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Lateral Contraction Cw";
            // 
            // textBoxContractionCoefficient
            // 
            this.textBoxContractionCoefficient.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGatedWeirFormula, "ContractionCoefficient", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxContractionCoefficient.Location = new System.Drawing.Point(155, 8);
            this.textBoxContractionCoefficient.Name = "textBoxContractionCoefficient";
            this.textBoxContractionCoefficient.Size = new System.Drawing.Size(100, 20);
            this.textBoxContractionCoefficient.TabIndex = 5;
            this.textBoxContractionCoefficient.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(123, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Contraction Coefficient μ";
            // 
            // GatedWeirFormulaView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textBoxLateralContraction);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxContractionCoefficient);
            this.Controls.Add(this.label1);
            this.MinimumSize = new System.Drawing.Size(274, 69);
            this.Name = "GatedWeirFormulaView";
            this.Size = new System.Drawing.Size(274, 69);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceGatedWeirFormula)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxLateralContraction;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxContractionCoefficient;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.BindingSource bindingSourceGatedWeirFormula;
    }
}
