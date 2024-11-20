using System.Windows.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.StandardCrossSections
{
    partial class CrossSectionStandardShapeWidthHeightView
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
            this.labelWidth = new System.Windows.Forms.Label();
            this.labelHeight = new System.Windows.Forms.Label();
            this.textBoxWidth = new System.Windows.Forms.TextBox();
            this.textBoxHeight = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.checkboxIsClosedProfile = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.crossSectionStandardShapeWidthHeightBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.crossSectionStandardShapeWidthHeightBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // labelWidth
            // 
            this.labelWidth.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelWidth.AutoSize = true;
            this.labelWidth.Location = new System.Drawing.Point(3, 31);
            this.labelWidth.Name = "labelWidth";
            this.labelWidth.Size = new System.Drawing.Size(35, 13);
            this.labelWidth.TabIndex = 0;
            this.labelWidth.Text = "Width";
            // 
            // labelHeight
            // 
            this.labelHeight.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelHeight.AutoSize = true;
            this.labelHeight.Location = new System.Drawing.Point(3, 6);
            this.labelHeight.Name = "labelHeight";
            this.labelHeight.Size = new System.Drawing.Size(38, 13);
            this.labelHeight.TabIndex = 1;
            this.labelHeight.Text = "Height";
            // 
            // textBoxWidth
            // 
            this.textBoxWidth.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.crossSectionStandardShapeWidthHeightBindingSource, "Width", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxWidth.Location = new System.Drawing.Point(95, 28);
            this.textBoxWidth.Name = "textBoxWidth";
            this.textBoxWidth.Size = new System.Drawing.Size(86, 20);
            this.textBoxWidth.TabIndex = 3;
            // 
            // textBoxHeight
            // 
            this.textBoxHeight.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.crossSectionStandardShapeWidthHeightBindingSource, "Height", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxHeight.Location = new System.Drawing.Point(95, 3);
            this.textBoxHeight.Name = "textBoxHeight";
            this.textBoxHeight.Size = new System.Drawing.Size(86, 20);
            this.textBoxHeight.TabIndex = 4;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.label2, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.textBoxHeight, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.textBoxWidth, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.checkboxIsClosedProfile, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.labelHeight, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelWidth, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label1, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(205, 87);
            this.tableLayoutPanel1.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(187, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(15, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "m";
            // 
            // checkboxIsClosedProfile
            // 
            this.checkboxIsClosedProfile.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.checkboxIsClosedProfile, 2);
            this.checkboxIsClosedProfile.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.crossSectionStandardShapeWidthHeightBindingSource, "Closed", true, DataSourceUpdateMode.OnPropertyChanged));
            this.checkboxIsClosedProfile.Location = new System.Drawing.Point(95, 53);
            this.checkboxIsClosedProfile.Name = "checkboxIsClosedProfile";
            this.checkboxIsClosedProfile.Size = new System.Drawing.Size(101, 17);
            this.checkboxIsClosedProfile.TabIndex = 5;
            this.checkboxIsClosedProfile.Text = "Is Closed Profile";
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(187, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(15, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "m";
            // 
            // crossSectionStandardShapeWidthHeightBindingSource
            // 
            this.crossSectionStandardShapeWidthHeightBindingSource.DataSource = typeof(DelftTools.Hydro.CrossSections.StandardShapes.CrossSectionStandardShapeWidthHeightBase);
            // 
            // CrossSectionStandardShapeWidthHeightView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "CrossSectionStandardShapeWidthHeightView";
            this.Size = new System.Drawing.Size(205, 87);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.crossSectionStandardShapeWidthHeightBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelWidth;
        private System.Windows.Forms.Label labelHeight;
        private System.Windows.Forms.TextBox textBoxWidth;
        private System.Windows.Forms.TextBox textBoxHeight;
        private System.Windows.Forms.BindingSource crossSectionStandardShapeWidthHeightBindingSource;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkboxIsClosedProfile;
    }
}
