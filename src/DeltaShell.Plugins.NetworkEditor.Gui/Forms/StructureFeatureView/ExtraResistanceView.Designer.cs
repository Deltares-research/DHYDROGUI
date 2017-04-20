namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    partial class ExtraResistanceView
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
            this.buttonTable = new System.Windows.Forms.Button();
            this.extResFormula = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonTable
            // 
            this.buttonTable.Location = new System.Drawing.Point(119, 68);
            this.buttonTable.Name = "buttonTable";
            this.buttonTable.Size = new System.Drawing.Size(75, 23);
            this.buttonTable.TabIndex = 2;
            this.buttonTable.Text = "&Table";
            this.buttonTable.UseVisualStyleBackColor = true;
            this.buttonTable.Click += new System.EventHandler(this.buttonTable_Click);
            // 
            // extResFormula
            // 
            this.extResFormula.AutoSize = true;
            this.extResFormula.Location = new System.Drawing.Point(21, 22);
            this.extResFormula.Name = "extResFormula";
            this.extResFormula.Size = new System.Drawing.Size(255, 13);
            this.extResFormula.TabIndex = 3;
            this.extResFormula.Text = "Extra Resistance Formula: DELTA_h  =  KSI * Q * |Q|";
            // 
            // ExtraResistanceView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
            this.Controls.Add(this.extResFormula);
            this.Controls.Add(this.buttonTable);
            this.Name = "ExtraResistanceView";
            this.Size = new System.Drawing.Size(342, 144);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonTable;
        private System.Windows.Forms.Label extResFormula;
    }
}
