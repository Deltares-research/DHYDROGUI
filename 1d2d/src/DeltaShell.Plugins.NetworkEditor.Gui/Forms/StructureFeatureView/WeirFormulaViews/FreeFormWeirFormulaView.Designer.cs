using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView.WeirFormulaViews
{
    partial class FreeFormWeirFormulaView
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
            this.yzTableView = new YZTableView();
            this.textBoxDischargeCoefficient = new System.Windows.Forms.TextBox();
            this.bindingSourceFreeFormWeirFormula = new System.Windows.Forms.BindingSource(this.components);
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceFreeFormWeirFormula)).BeginInit();
            this.SuspendLayout();
            // 
            // yzTableView
            // 
            this.yzTableView.AllowAddRemove = true;
            this.yzTableView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.yzTableView.Data = null;
            this.yzTableView.Image = null;
            this.yzTableView.Location = new System.Drawing.Point(0, 35);
            this.yzTableView.Name = "yzTableView";
            this.yzTableView.ReadOnly = false;
            this.yzTableView.ReadOnlyYColumn = false;
            this.yzTableView.Size = new System.Drawing.Size(446, 239);
            this.yzTableView.TabIndex = 0;
            // 
            // textBoxDischargeCoefficient
            // 
            this.textBoxDischargeCoefficient.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceFreeFormWeirFormula, "DischargeCoefficient", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxDischargeCoefficient.Location = new System.Drawing.Point(158, 9);
            this.textBoxDischargeCoefficient.Name = "textBoxDischargeCoefficient";
            this.textBoxDischargeCoefficient.Size = new System.Drawing.Size(100, 20);
            this.textBoxDischargeCoefficient.TabIndex = 3;
            this.textBoxDischargeCoefficient.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // bindingSourceFreeFormWeirFormula
            // 
            this.bindingSourceFreeFormWeirFormula.DataSource = typeof(DelftTools.Hydro.Structures.WeirFormula.FreeFormWeirFormula);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(124, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Discharge Coefficient Ce";
            // 
            // FreeFormWeirFormulaView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textBoxDischargeCoefficient);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.yzTableView);
            this.Name = "FreeFormWeirFormulaView";
            this.Size = new System.Drawing.Size(446, 274);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceFreeFormWeirFormula)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private YZTableView yzTableView;
        private System.Windows.Forms.TextBox textBoxDischargeCoefficient;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.BindingSource bindingSourceFreeFormWeirFormula;
    }
}