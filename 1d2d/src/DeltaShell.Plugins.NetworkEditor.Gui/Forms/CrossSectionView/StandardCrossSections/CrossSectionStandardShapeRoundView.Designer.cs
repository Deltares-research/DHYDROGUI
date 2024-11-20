namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.StandardCrossSections
{
    partial class CrossSectionStandardShapeRoundView
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
            this.labelDiameter = new System.Windows.Forms.Label();
            this.textBoxDiameter = new System.Windows.Forms.TextBox();
            this.bindingSourceShape = new System.Windows.Forms.BindingSource(this.components);
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceShape)).BeginInit();
            this.SuspendLayout();
            // 
            // labelDiameter
            // 
            this.labelDiameter.AutoSize = true;
            this.labelDiameter.Location = new System.Drawing.Point(4, 7);
            this.labelDiameter.Name = "labelDiameter";
            this.labelDiameter.Size = new System.Drawing.Size(49, 13);
            this.labelDiameter.TabIndex = 0;
            this.labelDiameter.Text = "Diameter";
            // 
            // textBoxDiameter
            // 
            this.textBoxDiameter.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceShape, "Diameter", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxDiameter.Location = new System.Drawing.Point(59, 4);
            this.textBoxDiameter.Name = "textBoxDiameter";
            this.textBoxDiameter.Size = new System.Drawing.Size(83, 20);
            this.textBoxDiameter.TabIndex = 1;
            // 
            // bindingSourceShape
            // 
            this.bindingSourceShape.DataSource = typeof(DelftTools.Hydro.CrossSections.StandardShapes.CrossSectionStandardShapeCircle);
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(144, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(23, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "m";
            // 
            // CrossSectionStandardShapeRoundView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxDiameter);
            this.Controls.Add(this.labelDiameter);
            this.Name = "CrossSectionStandardShapeRoundView";
            this.Size = new System.Drawing.Size(170, 83);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceShape)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelDiameter;
        private System.Windows.Forms.TextBox textBoxDiameter;
        private System.Windows.Forms.BindingSource bindingSourceShape;
        private System.Windows.Forms.Label label2;

    }
}
