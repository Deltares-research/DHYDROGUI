namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.StandardCrossSections
{
    partial class CrossSectionStandardShapeTrapeziumView
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
            this.labelSlope = new System.Windows.Forms.Label();
            this.textBoxSlope = new System.Windows.Forms.TextBox();
            this.bindingSourceTrapezium = new System.Windows.Forms.BindingSource(this.components);
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.textBoxMaxFlowWidth = new System.Windows.Forms.TextBox();
            this.textBoxBottomWidthB = new System.Windows.Forms.TextBox();
            this.labelMaxFlowWidth = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.trapeziumErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceTrapezium)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trapeziumErrorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // labelSlope
            // 
            this.labelSlope.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelSlope.AutoSize = true;
            this.labelSlope.Location = new System.Drawing.Point(3, 7);
            this.labelSlope.Name = "labelSlope";
            this.labelSlope.Size = new System.Drawing.Size(34, 13);
            this.labelSlope.TabIndex = 0;
            this.labelSlope.Text = "Slope";
            // 
            // textBoxSlope
            // 
            this.textBoxSlope.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceTrapezium, "Slope", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxSlope.Location = new System.Drawing.Point(95, 3);
            this.textBoxSlope.Name = "textBoxSlope";
            this.textBoxSlope.Size = new System.Drawing.Size(60, 20);
            this.textBoxSlope.TabIndex = 1;
            // 
            // bindingSourceTrapezium
            // 
            this.bindingSourceTrapezium.DataSource = typeof(DelftTools.Hydro.CrossSections.StandardShapes.CrossSectionStandardShapeTrapezium);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 39F));
            this.tableLayoutPanel1.Controls.Add(this.textBoxMaxFlowWidth, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.textBoxBottomWidthB, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.labelMaxFlowWidth, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.labelSlope, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.textBoxSlope, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.label3, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.label4, 2, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(223, 83);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // textBoxMaxFlowWidth
            // 
            this.textBoxMaxFlowWidth.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceTrapezium, "MaximumFlowWidth", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxMaxFlowWidth.Location = new System.Drawing.Point(95, 57);
            this.textBoxMaxFlowWidth.Name = "textBoxMaxFlowWidth";
            this.textBoxMaxFlowWidth.Size = new System.Drawing.Size(60, 20);
            this.textBoxMaxFlowWidth.TabIndex = 6;
            // 
            // textBoxBottomWidthB
            // 
            this.textBoxBottomWidthB.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceTrapezium, "BottomWidthB", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxBottomWidthB.Location = new System.Drawing.Point(95, 30);
            this.textBoxBottomWidthB.Name = "textBoxBottomWidthB";
            this.textBoxBottomWidthB.Size = new System.Drawing.Size(60, 20);
            this.textBoxBottomWidthB.TabIndex = 5;
            // 
            // labelMaxFlowWidth
            // 
            this.labelMaxFlowWidth.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelMaxFlowWidth.AutoSize = true;
            this.labelMaxFlowWidth.Location = new System.Drawing.Point(3, 62);
            this.labelMaxFlowWidth.Name = "labelMaxFlowWidth";
            this.labelMaxFlowWidth.Size = new System.Drawing.Size(80, 13);
            this.labelMaxFlowWidth.TabIndex = 4;
            this.labelMaxFlowWidth.Text = "Max. flow width";
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Bedwidth B";
            // 
            // trapeziumErrorProvider
            // 
            this.trapeziumErrorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.trapeziumErrorProvider.ContainerControl = this;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(187, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 26);
            this.label2.TabIndex = 7;
            this.label2.Text = "hor. / vert.";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(187, 34);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(15, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "m";
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(187, 62);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(15, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "m";
            // 
            // CrossSectionStandardShapeTrapeziumView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "CrossSectionStandardShapeTrapeziumView";
            this.Size = new System.Drawing.Size(223, 83);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceTrapezium)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trapeziumErrorProvider)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelSlope;
        private System.Windows.Forms.TextBox textBoxSlope;
        private System.Windows.Forms.BindingSource bindingSourceTrapezium;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxMaxFlowWidth;
        private System.Windows.Forms.TextBox textBoxBottomWidthB;
        private System.Windows.Forms.Label labelMaxFlowWidth;
        private System.Windows.Forms.ErrorProvider trapeziumErrorProvider;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;

    }
}
