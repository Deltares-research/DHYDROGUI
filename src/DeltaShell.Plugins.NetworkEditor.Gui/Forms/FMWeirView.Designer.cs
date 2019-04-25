using DelftTools.Hydro.Structures;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms
{
    partial class FMWeirView
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
            this.bindingSourceWeir = new System.Windows.Forms.BindingSource(this.components);
            this.groupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.lateralCoefficientTextBox = new System.Windows.Forms.TextBox();
            this.bindingSourceFormula = new System.Windows.Forms.BindingSource(this.components);
            this.timeDependentLabel = new System.Windows.Forms.Label();
            this.crestLevelLabel = new System.Windows.Forms.Label();
            this.lateralContractionLabel = new System.Windows.Forms.Label();
            this.crestLevelCheckBox = new System.Windows.Forms.CheckBox();
            this.crestLevelContainer = new System.Windows.Forms.Panel();
            this.crestLevelTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.labelCrestWidth = new System.Windows.Forms.Label();
            this.textBoxCrestWidth = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.checkBoxUseCrestWidth = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceWeir)).BeginInit();
            this.groupBox.SuspendLayout();
            this.tableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceFormula)).BeginInit();
            this.crestLevelContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // bindingSourceWeir
            // 
            this.bindingSourceWeir.DataSource = typeof(DelftTools.Hydro.Structures.Weir);
            // 
            // groupBox
            // 
            this.groupBox.Controls.Add(this.tableLayoutPanel);
            this.groupBox.Location = new System.Drawing.Point(0, 0);
            this.groupBox.Name = "groupBox";
            this.groupBox.Size = new System.Drawing.Size(429, 154);
            this.groupBox.TabIndex = 0;
            this.groupBox.TabStop = false;
            this.groupBox.Text = "Weir properties";
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.AutoSize = true;
            this.tableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel.ColumnCount = 4;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.Controls.Add(this.lateralCoefficientTextBox, 1, 2);
            this.tableLayoutPanel.Controls.Add(this.timeDependentLabel, 3, 0);
            this.tableLayoutPanel.Controls.Add(this.crestLevelLabel, 0, 1);
            this.tableLayoutPanel.Controls.Add(this.lateralContractionLabel, 0, 2);
            this.tableLayoutPanel.Controls.Add(this.crestLevelCheckBox, 3, 1);
            this.tableLayoutPanel.Controls.Add(this.crestLevelContainer, 1, 1);
            this.tableLayoutPanel.Controls.Add(this.label1, 2, 1);
            this.tableLayoutPanel.Controls.Add(this.label2, 2, 2);
            this.tableLayoutPanel.Controls.Add(this.labelCrestWidth, 0, 3);
            this.tableLayoutPanel.Controls.Add(this.textBoxCrestWidth, 1, 3);
            this.tableLayoutPanel.Controls.Add(this.label3, 2, 3);
            this.tableLayoutPanel.Controls.Add(this.checkBoxUseCrestWidth, 0, 4);
            this.tableLayoutPanel.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 5;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.Size = new System.Drawing.Size(414, 121);
            this.tableLayoutPanel.TabIndex = 0;
            // 
            // lateralCoefficientTextBox
            // 
            this.lateralCoefficientTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lateralCoefficientTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceFormula, "LateralContraction", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.lateralCoefficientTextBox.Location = new System.Drawing.Point(156, 49);
            this.lateralCoefficientTextBox.Name = "lateralCoefficientTextBox";
            this.lateralCoefficientTextBox.Size = new System.Drawing.Size(145, 20);
            this.lateralCoefficientTextBox.TabIndex = 6;
            this.lateralCoefficientTextBox.Validated += new System.EventHandler(this.LateralCoefficientTextBoxValidated);
            // 
            // timeDependentLabel
            // 
            this.timeDependentLabel.AutoSize = true;
            this.timeDependentLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.timeDependentLabel.Location = new System.Drawing.Point(327, 0);
            this.timeDependentLabel.Name = "timeDependentLabel";
            this.timeDependentLabel.Size = new System.Drawing.Size(84, 20);
            this.timeDependentLabel.TabIndex = 2;
            this.timeDependentLabel.Text = "Time dependent";
            this.timeDependentLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // crestLevelLabel
            // 
            this.crestLevelLabel.AutoSize = true;
            this.crestLevelLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.crestLevelLabel.Location = new System.Drawing.Point(3, 20);
            this.crestLevelLabel.Name = "crestLevelLabel";
            this.crestLevelLabel.Size = new System.Drawing.Size(147, 26);
            this.crestLevelLabel.TabIndex = 3;
            this.crestLevelLabel.Text = GuiParameterNames.CrestLevel;
            this.crestLevelLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lateralContractionLabel
            // 
            this.lateralContractionLabel.AutoSize = true;
            this.lateralContractionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lateralContractionLabel.Location = new System.Drawing.Point(3, 46);
            this.lateralContractionLabel.Name = "lateralContractionLabel";
            this.lateralContractionLabel.Size = new System.Drawing.Size(147, 26);
            this.lateralContractionLabel.TabIndex = 4;
            this.lateralContractionLabel.Text = "Lateral contraction coefficient";
            this.lateralContractionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // crestLevelCheckBox
            // 
            this.crestLevelCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.crestLevelCheckBox.AutoSize = true;
            this.crestLevelCheckBox.Location = new System.Drawing.Point(361, 23);
            this.crestLevelCheckBox.Name = "crestLevelCheckBox";
            this.crestLevelCheckBox.Size = new System.Drawing.Size(15, 20);
            this.crestLevelCheckBox.TabIndex = 7;
            this.crestLevelCheckBox.UseVisualStyleBackColor = true;
            this.crestLevelCheckBox.CheckedChanged += new System.EventHandler(this.CrestLevelCheckBoxCheckedChanged);
            // 
            // crestLevelContainer
            // 
            this.crestLevelContainer.Controls.Add(this.crestLevelTextBox);
            this.crestLevelContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.crestLevelContainer.Location = new System.Drawing.Point(156, 23);
            this.crestLevelContainer.Name = "crestLevelContainer";
            this.crestLevelContainer.Size = new System.Drawing.Size(145, 20);
            this.crestLevelContainer.TabIndex = 8;
            // 
            // crestLevelTextBox
            // 
            this.crestLevelTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.crestLevelTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceWeir, "CrestLevel", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.crestLevelTextBox.Location = new System.Drawing.Point(0, 0);
            this.crestLevelTextBox.Name = "crestLevelTextBox";
            this.crestLevelTextBox.Size = new System.Drawing.Size(145, 20);
            this.crestLevelTextBox.TabIndex = 5;
            this.crestLevelTextBox.Validated += new System.EventHandler(this.CrestLevelTextBoxValidated);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(307, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(14, 26);
            this.label1.TabIndex = 9;
            this.label1.Text = "m";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(307, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(14, 26);
            this.label2.TabIndex = 10;
            this.label2.Text = "m";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelCrestWidth
            // 
            this.labelCrestWidth.AutoSize = true;
            this.labelCrestWidth.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelCrestWidth.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelCrestWidth.Location = new System.Drawing.Point(3, 72);
            this.labelCrestWidth.Name = "labelCrestWidth";
            this.labelCrestWidth.Size = new System.Drawing.Size(147, 26);
            this.labelCrestWidth.TabIndex = 11;
            this.labelCrestWidth.Text = GuiParameterNames.CrestWidth;
            this.labelCrestWidth.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxCrestWidth
            // 
            this.textBoxCrestWidth.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceWeir, "CrestWidth", true));
            this.textBoxCrestWidth.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxCrestWidth.Location = new System.Drawing.Point(156, 75);
            this.textBoxCrestWidth.Name = "textBoxCrestWidth";
            this.textBoxCrestWidth.Size = new System.Drawing.Size(145, 20);
            this.textBoxCrestWidth.TabIndex = 12;
            this.textBoxCrestWidth.Validated += new System.EventHandler(this.CrestWidthTextBoxValidated);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(307, 72);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(14, 26);
            this.label3.TabIndex = 13;
            this.label3.Text = "m";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // checkBoxUseCrestWidth
            // 
            this.checkBoxUseCrestWidth.AutoSize = true;
            this.checkBoxUseCrestWidth.Location = new System.Drawing.Point(3, 101);
            this.checkBoxUseCrestWidth.Name = "checkBoxUseCrestWidth";
            this.checkBoxUseCrestWidth.Size = new System.Drawing.Size(99, 17);
            this.checkBoxUseCrestWidth.TabIndex = 14;
            this.checkBoxUseCrestWidth.Text = "Use crest width";
            this.checkBoxUseCrestWidth.UseVisualStyleBackColor = true;
            this.checkBoxUseCrestWidth.CheckedChanged += new System.EventHandler(this.CheckBoxUseCrestWidthCheckedChanged);
            // 
            // FMWeirView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.groupBox);
            this.Name = "FMWeirView";
            this.Size = new System.Drawing.Size(432, 187);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceWeir)).EndInit();
            this.groupBox.ResumeLayout(false);
            this.groupBox.PerformLayout();
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceFormula)).EndInit();
            this.crestLevelContainer.ResumeLayout(false);
            this.crestLevelContainer.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.BindingSource bindingSourceWeir;
        private System.Windows.Forms.GroupBox groupBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.Label timeDependentLabel;
        private System.Windows.Forms.Label crestLevelLabel;
        private System.Windows.Forms.Label lateralContractionLabel;
        private System.Windows.Forms.TextBox crestLevelTextBox;
        private System.Windows.Forms.TextBox lateralCoefficientTextBox;
        private System.Windows.Forms.CheckBox crestLevelCheckBox;
        private System.Windows.Forms.Panel crestLevelContainer;
        private System.Windows.Forms.BindingSource bindingSourceFormula;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labelCrestWidth;
        private System.Windows.Forms.TextBox textBoxCrestWidth;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkBoxUseCrestWidth;
    }
}
