using DelftTools.Hydro.Structures;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    partial class GateView
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
            this.groupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.lowerEdgeLevelLabel = new System.Windows.Forms.Label();
            this.sillLevelLabel = new System.Windows.Forms.Label();
            this.doorHeightLabel = new System.Windows.Forms.Label();
            this.doorHeightTextBox = new System.Windows.Forms.TextBox();
            this.openingDirectionLabel = new System.Windows.Forms.Label();
            this.openingDirectionComboBox = new System.Windows.Forms.ComboBox();
            this.openingWidthLabel = new System.Windows.Forms.Label();
            this.lowerEdgeLevelCheckBox = new System.Windows.Forms.CheckBox();
            this.openingWidthCheckBox = new System.Windows.Forms.CheckBox();
            this.timeDependentLabel = new System.Windows.Forms.Label();
            this.lowerEdgeLevelContainer = new System.Windows.Forms.Panel();
            this.lowerEdgeLevelTextBox = new System.Windows.Forms.TextBox();
            this.openingWidthContainer = new System.Windows.Forms.Panel();
            this.openingWidthTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.LabelSillWidth = new System.Windows.Forms.Label();
            this.sillWidthTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.checkBoxUseSillWidth = new System.Windows.Forms.CheckBox();
            this.sillLevelCheckBox = new System.Windows.Forms.CheckBox();
            this.sillLevelContainer = new System.Windows.Forms.Panel();
            this.sillLevelTextBox = new System.Windows.Forms.TextBox();
            this.bindingSourceGate = new System.Windows.Forms.BindingSource(this.components);
            this.groupBox.SuspendLayout();
            this.tableLayoutPanel.SuspendLayout();
            this.lowerEdgeLevelContainer.SuspendLayout();
            this.openingWidthContainer.SuspendLayout();
            this.sillLevelContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceGate)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox
            // 
            this.groupBox.Controls.Add(this.tableLayoutPanel);
            this.groupBox.Location = new System.Drawing.Point(0, 0);
            this.groupBox.Name = "groupBox";
            this.groupBox.Size = new System.Drawing.Size(419, 241);
            this.groupBox.TabIndex = 0;
            this.groupBox.TabStop = false;
            this.groupBox.Text = "Gate properties";
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 4;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.Controls.Add(this.lowerEdgeLevelLabel, 0, 4);
            this.tableLayoutPanel.Controls.Add(this.sillLevelLabel, 0, 1);
            this.tableLayoutPanel.Controls.Add(this.doorHeightLabel, 0, 2);
            this.tableLayoutPanel.Controls.Add(this.doorHeightTextBox, 1, 2);
            this.tableLayoutPanel.Controls.Add(this.openingDirectionLabel, 0, 3);
            this.tableLayoutPanel.Controls.Add(this.openingDirectionComboBox, 1, 3);
            this.tableLayoutPanel.Controls.Add(this.openingWidthLabel, 0, 5);
            this.tableLayoutPanel.Controls.Add(this.lowerEdgeLevelCheckBox, 3, 4);
            this.tableLayoutPanel.Controls.Add(this.openingWidthCheckBox, 3, 5);
            this.tableLayoutPanel.Controls.Add(this.timeDependentLabel, 3, 0);
            this.tableLayoutPanel.Controls.Add(this.lowerEdgeLevelContainer, 1, 4);
            this.tableLayoutPanel.Controls.Add(this.openingWidthContainer, 1, 5);
            this.tableLayoutPanel.Controls.Add(this.label1, 2, 1);
            this.tableLayoutPanel.Controls.Add(this.label2, 2, 5);
            this.tableLayoutPanel.Controls.Add(this.label3, 2, 2);
            this.tableLayoutPanel.Controls.Add(this.label4, 2, 4);
            this.tableLayoutPanel.Controls.Add(this.LabelSillWidth, 0, 6);
            this.tableLayoutPanel.Controls.Add(this.sillWidthTextBox, 1, 6);
            this.tableLayoutPanel.Controls.Add(this.label5, 2, 6);
            this.tableLayoutPanel.Controls.Add(this.checkBoxUseSillWidth, 0, 7);
            this.tableLayoutPanel.Controls.Add(this.sillLevelCheckBox, 3, 1);
            this.tableLayoutPanel.Controls.Add(this.sillLevelContainer, 1, 1);
            this.tableLayoutPanel.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.tableLayoutPanel.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 9;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(399, 219);
            this.tableLayoutPanel.TabIndex = 0;
            // 
            // lowerEdgeLevelLabel
            // 
            this.lowerEdgeLevelLabel.AutoSize = true;
            this.lowerEdgeLevelLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lowerEdgeLevelLabel.Location = new System.Drawing.Point(3, 101);
            this.lowerEdgeLevelLabel.Name = "lowerEdgeLevelLabel";
            this.lowerEdgeLevelLabel.Size = new System.Drawing.Size(138, 30);
            this.lowerEdgeLevelLabel.TabIndex = 6;
            this.lowerEdgeLevelLabel.Text = "Lower edge level";
            this.lowerEdgeLevelLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // sillLevelLabel
            // 
            this.sillLevelLabel.AutoSize = true;
            this.sillLevelLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sillLevelLabel.Location = new System.Drawing.Point(3, 20);
            this.sillLevelLabel.Name = "sillLevelLabel";
            this.sillLevelLabel.Size = new System.Drawing.Size(138, 28);
            this.sillLevelLabel.TabIndex = 0;
            this.sillLevelLabel.Text = "Sill level";
            this.sillLevelLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // doorHeightLabel
            // 
            this.doorHeightLabel.AutoSize = true;
            this.doorHeightLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.doorHeightLabel.Location = new System.Drawing.Point(3, 48);
            this.doorHeightLabel.Name = "doorHeightLabel";
            this.doorHeightLabel.Size = new System.Drawing.Size(138, 26);
            this.doorHeightLabel.TabIndex = 2;
            this.doorHeightLabel.Text = "Door height";
            this.doorHeightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // doorHeightTextBox
            // 
            this.doorHeightTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGate, "DoorHeight", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.doorHeightTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.doorHeightTextBox.Location = new System.Drawing.Point(147, 51);
            this.doorHeightTextBox.Name = "doorHeightTextBox";
            this.doorHeightTextBox.Size = new System.Drawing.Size(112, 20);
            this.doorHeightTextBox.TabIndex = 3;
            this.doorHeightTextBox.Validated += new System.EventHandler(this.DoorHeightTextBoxValidated);
            // 
            // openingDirectionLabel
            // 
            this.openingDirectionLabel.AutoSize = true;
            this.openingDirectionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.openingDirectionLabel.Location = new System.Drawing.Point(3, 74);
            this.openingDirectionLabel.Name = "openingDirectionLabel";
            this.openingDirectionLabel.Size = new System.Drawing.Size(138, 27);
            this.openingDirectionLabel.TabIndex = 4;
            this.openingDirectionLabel.Text = "Horizontal opening direction";
            this.openingDirectionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // openingDirectionComboBox
            // 
            this.openingDirectionComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.openingDirectionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.openingDirectionComboBox.FormattingEnabled = true;
            this.openingDirectionComboBox.Location = new System.Drawing.Point(147, 77);
            this.openingDirectionComboBox.Name = "openingDirectionComboBox";
            this.openingDirectionComboBox.Size = new System.Drawing.Size(112, 21);
            this.openingDirectionComboBox.TabIndex = 5;
            // 
            // openingWidthLabel
            // 
            this.openingWidthLabel.AutoSize = true;
            this.openingWidthLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.openingWidthLabel.Location = new System.Drawing.Point(3, 131);
            this.openingWidthLabel.Name = "openingWidthLabel";
            this.openingWidthLabel.Size = new System.Drawing.Size(138, 29);
            this.openingWidthLabel.TabIndex = 7;
            this.openingWidthLabel.Text = "Opening width";
            this.openingWidthLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lowerEdgeLevelCheckBox
            // 
            this.lowerEdgeLevelCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.lowerEdgeLevelCheckBox.AutoSize = true;
            this.lowerEdgeLevelCheckBox.Location = new System.Drawing.Point(336, 104);
            this.lowerEdgeLevelCheckBox.Name = "lowerEdgeLevelCheckBox";
            this.lowerEdgeLevelCheckBox.Size = new System.Drawing.Size(15, 24);
            this.lowerEdgeLevelCheckBox.TabIndex = 10;
            this.lowerEdgeLevelCheckBox.UseVisualStyleBackColor = true;
            this.lowerEdgeLevelCheckBox.CheckedChanged += new System.EventHandler(this.LowerEdgeLevelCheckBoxCheckedChanged);
            // 
            // openingWidthCheckBox
            // 
            this.openingWidthCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.openingWidthCheckBox.AutoSize = true;
            this.openingWidthCheckBox.Location = new System.Drawing.Point(336, 134);
            this.openingWidthCheckBox.Name = "openingWidthCheckBox";
            this.openingWidthCheckBox.Size = new System.Drawing.Size(15, 23);
            this.openingWidthCheckBox.TabIndex = 11;
            this.openingWidthCheckBox.UseVisualStyleBackColor = true;
            this.openingWidthCheckBox.CheckedChanged += new System.EventHandler(this.OpeningWidthCheckBoxCheckedChanged);
            // 
            // timeDependentLabel
            // 
            this.timeDependentLabel.AutoSize = true;
            this.timeDependentLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.timeDependentLabel.Location = new System.Drawing.Point(292, 0);
            this.timeDependentLabel.Name = "timeDependentLabel";
            this.timeDependentLabel.Size = new System.Drawing.Size(104, 20);
            this.timeDependentLabel.TabIndex = 12;
            this.timeDependentLabel.Text = "Time dependent";
            this.timeDependentLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lowerEdgeLevelContainer
            // 
            this.lowerEdgeLevelContainer.Controls.Add(this.lowerEdgeLevelTextBox);
            this.lowerEdgeLevelContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lowerEdgeLevelContainer.Location = new System.Drawing.Point(147, 104);
            this.lowerEdgeLevelContainer.Name = "lowerEdgeLevelContainer";
            this.lowerEdgeLevelContainer.Size = new System.Drawing.Size(112, 24);
            this.lowerEdgeLevelContainer.TabIndex = 15;
            // 
            // lowerEdgeLevelTextBox
            // 
            this.lowerEdgeLevelTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGate, "LowerEdgeLevel", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.lowerEdgeLevelTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lowerEdgeLevelTextBox.Location = new System.Drawing.Point(0, 0);
            this.lowerEdgeLevelTextBox.Name = "lowerEdgeLevelTextBox";
            this.lowerEdgeLevelTextBox.Size = new System.Drawing.Size(112, 20);
            this.lowerEdgeLevelTextBox.TabIndex = 8;
            this.lowerEdgeLevelTextBox.Validated += new System.EventHandler(this.LowerEdgeLevelTextBoxValidated);
            // 
            // openingWidthContainer
            // 
            this.openingWidthContainer.Controls.Add(this.openingWidthTextBox);
            this.openingWidthContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.openingWidthContainer.Location = new System.Drawing.Point(147, 134);
            this.openingWidthContainer.Name = "openingWidthContainer";
            this.openingWidthContainer.Size = new System.Drawing.Size(112, 23);
            this.openingWidthContainer.TabIndex = 16;
            // 
            // openingWidthTextBox
            // 
            this.openingWidthTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGate, "OpeningWidth", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.openingWidthTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.openingWidthTextBox.Location = new System.Drawing.Point(0, 0);
            this.openingWidthTextBox.Name = "openingWidthTextBox";
            this.openingWidthTextBox.Size = new System.Drawing.Size(112, 20);
            this.openingWidthTextBox.TabIndex = 9;
            this.openingWidthTextBox.Validated += new System.EventHandler(this.OpeningWidthTextBoxValidated);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(265, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(21, 28);
            this.label1.TabIndex = 17;
            this.label1.Text = "[m]";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(265, 131);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(21, 29);
            this.label2.TabIndex = 18;
            this.label2.Text = "[m]";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(265, 48);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(21, 26);
            this.label3.TabIndex = 19;
            this.label3.Text = "[m]";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(265, 101);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(21, 30);
            this.label4.TabIndex = 20;
            this.label4.Text = "[m]";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LabelSillWidth
            // 
            this.LabelSillWidth.AutoSize = true;
            this.LabelSillWidth.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LabelSillWidth.Location = new System.Drawing.Point(3, 160);
            this.LabelSillWidth.Name = "LabelSillWidth";
            this.LabelSillWidth.Size = new System.Drawing.Size(138, 26);
            this.LabelSillWidth.TabIndex = 21;
            this.LabelSillWidth.Text = "Sill width";
            this.LabelSillWidth.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // sillWidthTextBox
            // 
            this.sillWidthTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGate, "SillWidth", true));
            this.sillWidthTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sillWidthTextBox.Location = new System.Drawing.Point(147, 163);
            this.sillWidthTextBox.Name = "sillWidthTextBox";
            this.sillWidthTextBox.Size = new System.Drawing.Size(112, 20);
            this.sillWidthTextBox.TabIndex = 22;
            this.sillWidthTextBox.Validated += new System.EventHandler(this.SillWidthTextBoxValidated);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(265, 160);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(21, 26);
            this.label5.TabIndex = 23;
            this.label5.Text = "[m]";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // checkBoxUseSillWidth
            // 
            this.checkBoxUseSillWidth.AutoSize = true;
            this.checkBoxUseSillWidth.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxUseSillWidth.Location = new System.Drawing.Point(3, 189);
            this.checkBoxUseSillWidth.Name = "checkBoxUseSillWidth";
            this.checkBoxUseSillWidth.Size = new System.Drawing.Size(138, 17);
            this.checkBoxUseSillWidth.TabIndex = 25;
            this.checkBoxUseSillWidth.Text = "Use sill width";
            this.checkBoxUseSillWidth.UseVisualStyleBackColor = true;
            this.checkBoxUseSillWidth.CheckedChanged += new System.EventHandler(this.CheckBoxUseSillWidthCheckedChanged);
            // 
            // sillLevelCheckBox
            // 
            this.sillLevelCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.sillLevelCheckBox.AutoSize = true;
            this.sillLevelCheckBox.Location = new System.Drawing.Point(336, 23);
            this.sillLevelCheckBox.Name = "sillLevelCheckBox";
            this.sillLevelCheckBox.Size = new System.Drawing.Size(15, 22);
            this.sillLevelCheckBox.TabIndex = 26;
            this.sillLevelCheckBox.UseVisualStyleBackColor = true;
            this.sillLevelCheckBox.CheckedChanged += new System.EventHandler(this.SillLevelCheckBoxCheckedChanged);
            // 
            // sillLevelContainer
            // 
            this.sillLevelContainer.Controls.Add(this.sillLevelTextBox);
            this.sillLevelContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sillLevelContainer.Location = new System.Drawing.Point(147, 23);
            this.sillLevelContainer.Name = "sillLevelContainer";
            this.sillLevelContainer.Size = new System.Drawing.Size(112, 22);
            this.sillLevelContainer.TabIndex = 27;
            // 
            // sillLevelTextBox
            // 
            this.sillLevelTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGate, "SillLevel", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.sillLevelTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sillLevelTextBox.Location = new System.Drawing.Point(0, 0);
            this.sillLevelTextBox.Name = "sillLevelTextBox";
            this.sillLevelTextBox.Size = new System.Drawing.Size(112, 20);
            this.sillLevelTextBox.TabIndex = 1;
            this.sillLevelTextBox.Validated += new System.EventHandler(this.SillLevelTextBoxValidated);
            // 
            // bindingSourceGate
            // 
            this.bindingSourceGate.DataSource = typeof(DelftTools.Hydro.Structures.Gate);
            // 
            // GateView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.groupBox);
            this.Name = "GateView";
            this.Size = new System.Drawing.Size(425, 244);
            this.groupBox.ResumeLayout(false);
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            this.lowerEdgeLevelContainer.ResumeLayout(false);
            this.lowerEdgeLevelContainer.PerformLayout();
            this.openingWidthContainer.ResumeLayout(false);
            this.openingWidthContainer.PerformLayout();
            this.sillLevelContainer.ResumeLayout(false);
            this.sillLevelContainer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceGate)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.BindingSource bindingSourceGate;
        private System.Windows.Forms.GroupBox groupBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.Label sillLevelLabel;
        private System.Windows.Forms.TextBox sillLevelTextBox;
        private System.Windows.Forms.Label doorHeightLabel;
        private System.Windows.Forms.TextBox doorHeightTextBox;
        private System.Windows.Forms.Label openingDirectionLabel;
        private System.Windows.Forms.ComboBox openingDirectionComboBox;
        private System.Windows.Forms.Label lowerEdgeLevelLabel;
        private System.Windows.Forms.Label openingWidthLabel;
        private System.Windows.Forms.TextBox lowerEdgeLevelTextBox;
        private System.Windows.Forms.TextBox openingWidthTextBox;
        private System.Windows.Forms.CheckBox lowerEdgeLevelCheckBox;
        private System.Windows.Forms.CheckBox openingWidthCheckBox;
        private System.Windows.Forms.Label timeDependentLabel;
        private System.Windows.Forms.Panel lowerEdgeLevelContainer;
        private System.Windows.Forms.Panel openingWidthContainer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label LabelSillWidth;
        private System.Windows.Forms.TextBox sillWidthTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox checkBoxUseSillWidth;
        private System.Windows.Forms.CheckBox sillLevelCheckBox;
        private System.Windows.Forms.Panel sillLevelContainer;
    }
}
