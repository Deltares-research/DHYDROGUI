namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView.WeirFormulaViews
{
    partial class SimpleWeirFormulaView
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
            this.textBoxDischargeCoefficient = new System.Windows.Forms.TextBox();
            this.bindingSourceSimpleWeirFormula = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceSimpleWeirFormula)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(105, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Correction coefficient";
            // 
            // textBoxDischargeCoefficient
            // 
            this.textBoxDischargeCoefficient.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceSimpleWeirFormula, "CorrectionCoefficient", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxDischargeCoefficient.Location = new System.Drawing.Point(156, 3);
            this.textBoxDischargeCoefficient.Name = "textBoxDischargeCoefficient";
            this.textBoxDischargeCoefficient.Size = new System.Drawing.Size(100, 20);
            this.textBoxDischargeCoefficient.TabIndex = 1;
            this.textBoxDischargeCoefficient.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // bindingSourceSimpleWeirFormula
            // 
            this.bindingSourceSimpleWeirFormula.DataSource = typeof(DelftTools.Hydro.Structures.WeirFormula.SimpleWeirFormula);
            // 
            // SimpleWeirFormulaView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textBoxDischargeCoefficient);
            this.Controls.Add(this.label1);
            this.MinimumSize = new System.Drawing.Size(270, 58);
            this.Name = "SimpleWeirFormulaView";
            this.Size = new System.Drawing.Size(270, 58);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceSimpleWeirFormula)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxDischargeCoefficient;
        private System.Windows.Forms.BindingSource bindingSourceSimpleWeirFormula;
    }
}