namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView.WeirFormulaViews
{
    partial class GeneralStructureWeirFormulaView
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
            this.textboxFreeGatePos = new System.Windows.Forms.TextBox();
            this.bindingSourceGeneralStructure = new System.Windows.Forms.BindingSource(this.components);
            this.textboxDrownedGatePos = new System.Windows.Forms.TextBox();
            this.textboxFreeGateNeg = new System.Windows.Forms.TextBox();
            this.textboxDrownedGateNeg = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.textBoxFreeWeirFlowPos = new System.Windows.Forms.TextBox();
            this.textBoxFreeWeirFlowNeg = new System.Windows.Forms.TextBox();
            this.textBoxDrownedWeirFlowPos = new System.Windows.Forms.TextBox();
            this.textBoxDrownedWeirFlowNeg = new System.Windows.Forms.TextBox();
            this.textBoxContractionCoefNeg = new System.Windows.Forms.TextBox();
            this.textBoxContractionCoefPos = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label15 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.textBoxLevelUpstream1 = new System.Windows.Forms.TextBox();
            this.textBoxLevelUpstream2 = new System.Windows.Forms.TextBox();
            this.textBoxWidthCrest = new System.Windows.Forms.TextBox();
            this.textBoxLevelCrest = new System.Windows.Forms.TextBox();
            this.textBoxWidthDownstream1 = new System.Windows.Forms.TextBox();
            this.textBoxLevelDownstream1 = new System.Windows.Forms.TextBox();
            this.textBoxLevelDownstream2 = new System.Windows.Forms.TextBox();
            this.textBoxWidthDownstream2 = new System.Windows.Forms.TextBox();
            this.textBoxWidthUpstream1 = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.textBoxWidthUpstream2 = new System.Windows.Forms.TextBox();
            this.checkBoxExtraResistance = new System.Windows.Forms.CheckBox();
            this.textBoxExtraResistance = new System.Windows.Forms.TextBox();
            this.textBoxCrestLength = new System.Windows.Forms.TextBox();
            this.textBoxGateOpeningWidth = new System.Windows.Forms.TextBox();
            this.labelCrestLength = new System.Windows.Forms.Label();
            this.labelGateOpeningWidth = new System.Windows.Forms.Label();
            this.comboBoxGateOpeningDirection = new System.Windows.Forms.ComboBox();
            this.labelGateOpeningDirection = new System.Windows.Forms.Label();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceGeneralStructure)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // textboxFreeGatePos
            // 
            this.textboxFreeGatePos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textboxFreeGatePos.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGeneralStructure, "PositiveFreeGateFlow", true));
            this.textboxFreeGatePos.Location = new System.Drawing.Point(133, 27);
            this.textboxFreeGatePos.Name = "textboxFreeGatePos";
            this.textboxFreeGatePos.Size = new System.Drawing.Size(46, 20);
            this.textboxFreeGatePos.TabIndex = 0;
            this.textboxFreeGatePos.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // bindingSourceGeneralStructure
            // 
            this.bindingSourceGeneralStructure.DataSource = typeof(DelftTools.Hydro.Structures.WeirFormula.GeneralStructureWeirFormula);
            // 
            // textboxDrownedGatePos
            // 
            this.textboxDrownedGatePos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textboxDrownedGatePos.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGeneralStructure, "PositiveDrownedGateFlow", true));
            this.textboxDrownedGatePos.Location = new System.Drawing.Point(133, 51);
            this.textboxDrownedGatePos.Name = "textboxDrownedGatePos";
            this.textboxDrownedGatePos.Size = new System.Drawing.Size(46, 20);
            this.textboxDrownedGatePos.TabIndex = 2;
            this.textboxDrownedGatePos.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textboxFreeGateNeg
            // 
            this.textboxFreeGateNeg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textboxFreeGateNeg.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGeneralStructure, "NegativeFreeGateFlow", true));
            this.textboxFreeGateNeg.Location = new System.Drawing.Point(185, 27);
            this.textboxFreeGateNeg.Name = "textboxFreeGateNeg";
            this.textboxFreeGateNeg.Size = new System.Drawing.Size(47, 20);
            this.textboxFreeGateNeg.TabIndex = 1;
            this.textboxFreeGateNeg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textboxDrownedGateNeg
            // 
            this.textboxDrownedGateNeg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textboxDrownedGateNeg.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGeneralStructure, "NegativeDrownedGateFlow", true));
            this.textboxDrownedGateNeg.Location = new System.Drawing.Point(185, 51);
            this.textboxDrownedGateNeg.Name = "textboxDrownedGateNeg";
            this.textboxDrownedGateNeg.Size = new System.Drawing.Size(47, 20);
            this.textboxDrownedGateNeg.TabIndex = 3;
            this.textboxDrownedGateNeg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(124, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Free gate flow";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(124, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Drowned gate flow";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 77);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(124, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Free weir flow";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55.55556F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22.22222F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22.22222F));
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.textboxDrownedGatePos, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.textboxDrownedGateNeg, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.textboxFreeGateNeg, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.textboxFreeGatePos, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label4, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label5, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label7, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.label8, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.textBoxFreeWeirFlowPos, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.textBoxFreeWeirFlowNeg, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.textBoxDrownedWeirFlowPos, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.textBoxDrownedWeirFlowNeg, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.textBoxContractionCoefNeg, 2, 5);
            this.tableLayoutPanel1.Controls.Add(this.textBoxContractionCoefPos, 1, 5);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(11, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66666F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66666F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(235, 147);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(133, 5);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(46, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Flow";
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(185, 5);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Reverse";
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 5);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(124, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "Coefficients";
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 101);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(124, 13);
            this.label7.TabIndex = 3;
            this.label7.Text = "Drowned weir flow";
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 127);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(124, 13);
            this.label8.TabIndex = 4;
            this.label8.Text = "Contraction coefficient";
            // 
            // textBoxFreeWeirFlowPos
            // 
            this.textBoxFreeWeirFlowPos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxFreeWeirFlowPos.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGeneralStructure, "PositiveFreeWeirFlow", true));
            this.textBoxFreeWeirFlowPos.Location = new System.Drawing.Point(133, 75);
            this.textBoxFreeWeirFlowPos.Name = "textBoxFreeWeirFlowPos";
            this.textBoxFreeWeirFlowPos.Size = new System.Drawing.Size(46, 20);
            this.textBoxFreeWeirFlowPos.TabIndex = 4;
            this.textBoxFreeWeirFlowPos.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxFreeWeirFlowNeg
            // 
            this.textBoxFreeWeirFlowNeg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxFreeWeirFlowNeg.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGeneralStructure, "NegativeFreeWeirFlow", true));
            this.textBoxFreeWeirFlowNeg.Location = new System.Drawing.Point(185, 75);
            this.textBoxFreeWeirFlowNeg.Name = "textBoxFreeWeirFlowNeg";
            this.textBoxFreeWeirFlowNeg.Size = new System.Drawing.Size(47, 20);
            this.textBoxFreeWeirFlowNeg.TabIndex = 5;
            this.textBoxFreeWeirFlowNeg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxDrownedWeirFlowPos
            // 
            this.textBoxDrownedWeirFlowPos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDrownedWeirFlowPos.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGeneralStructure, "PositiveDrownedWeirFlow", true));
            this.textBoxDrownedWeirFlowPos.Location = new System.Drawing.Point(133, 99);
            this.textBoxDrownedWeirFlowPos.Name = "textBoxDrownedWeirFlowPos";
            this.textBoxDrownedWeirFlowPos.Size = new System.Drawing.Size(46, 20);
            this.textBoxDrownedWeirFlowPos.TabIndex = 6;
            this.textBoxDrownedWeirFlowPos.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxDrownedWeirFlowNeg
            // 
            this.textBoxDrownedWeirFlowNeg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDrownedWeirFlowNeg.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGeneralStructure, "NegativeDrownedWeirFlow", true));
            this.textBoxDrownedWeirFlowNeg.Location = new System.Drawing.Point(185, 99);
            this.textBoxDrownedWeirFlowNeg.Name = "textBoxDrownedWeirFlowNeg";
            this.textBoxDrownedWeirFlowNeg.Size = new System.Drawing.Size(47, 20);
            this.textBoxDrownedWeirFlowNeg.TabIndex = 7;
            this.textBoxDrownedWeirFlowNeg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxContractionCoefNeg
            // 
            this.textBoxContractionCoefNeg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxContractionCoefNeg.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGeneralStructure, "NegativeContractionCoefficient", true));
            this.textBoxContractionCoefNeg.Location = new System.Drawing.Point(185, 123);
            this.textBoxContractionCoefNeg.Name = "textBoxContractionCoefNeg";
            this.textBoxContractionCoefNeg.Size = new System.Drawing.Size(47, 20);
            this.textBoxContractionCoefNeg.TabIndex = 9;
            this.textBoxContractionCoefNeg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxContractionCoefPos
            // 
            this.textBoxContractionCoefPos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxContractionCoefPos.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGeneralStructure, "PositiveContractionCoefficient", true));
            this.textBoxContractionCoefPos.Location = new System.Drawing.Point(133, 123);
            this.textBoxContractionCoefPos.Name = "textBoxContractionCoefPos";
            this.textBoxContractionCoefPos.Size = new System.Drawing.Size(46, 20);
            this.textBoxContractionCoefPos.TabIndex = 8;
            this.textBoxContractionCoefPos.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 6;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.Controls.Add(this.label15, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.label9, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.label16, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.label10, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.label11, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this.label12, 5, 0);
            this.tableLayoutPanel2.Controls.Add(this.textBoxLevelUpstream1, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.textBoxLevelUpstream2, 2, 1);
            this.tableLayoutPanel2.Controls.Add(this.textBoxWidthCrest, 2, 2);
            this.tableLayoutPanel2.Controls.Add(this.textBoxLevelCrest, 3, 1);
            this.tableLayoutPanel2.Controls.Add(this.textBoxWidthDownstream1, 4, 2);
            this.tableLayoutPanel2.Controls.Add(this.textBoxLevelDownstream1, 4, 1);
            this.tableLayoutPanel2.Controls.Add(this.textBoxLevelDownstream2, 5, 1);
            this.tableLayoutPanel2.Controls.Add(this.textBoxWidthDownstream2, 5, 2);
            this.tableLayoutPanel2.Controls.Add(this.textBoxWidthUpstream1, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.label13, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.textBoxWidthUpstream2, 2, 2);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(270, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(489, 71);
            this.tableLayoutPanel2.TabIndex = 3;
            // 
            // label15
            // 
            this.label15.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(3, 28);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(75, 13);
            this.label15.TabIndex = 3;
            this.label15.Text = "Level (m)";
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(84, 5);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(75, 13);
            this.label9.TabIndex = 5;
            this.label9.Text = "Upstream1";
            // 
            // label16
            // 
            this.label16.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(165, 5);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(75, 13);
            this.label16.TabIndex = 4;
            this.label16.Text = "Upstream2";
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(246, 5);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(75, 13);
            this.label10.TabIndex = 6;
            this.label10.Text = "Crest";
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(327, 5);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(75, 13);
            this.label11.TabIndex = 7;
            this.label11.Text = "Downstream1";
            // 
            // label12
            // 
            this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(408, 5);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(78, 13);
            this.label12.TabIndex = 8;
            this.label12.Text = "Downstream2";
            // 
            // textBoxLevelUpstream1
            // 
            this.textBoxLevelUpstream1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLevelUpstream1.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGeneralStructure, "BedLevelLeftSideOfStructure", true));
            this.textBoxLevelUpstream1.Location = new System.Drawing.Point(84, 26);
            this.textBoxLevelUpstream1.Name = "textBoxLevelUpstream1";
            this.textBoxLevelUpstream1.Size = new System.Drawing.Size(75, 20);
            this.textBoxLevelUpstream1.TabIndex = 10;
            this.textBoxLevelUpstream1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxLevelUpstream2
            // 
            this.textBoxLevelUpstream2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLevelUpstream2.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGeneralStructure, "BedLevelLeftSideStructure", true));
            this.textBoxLevelUpstream2.Location = new System.Drawing.Point(165, 26);
            this.textBoxLevelUpstream2.Name = "textBoxLevelUpstream2";
            this.textBoxLevelUpstream2.Size = new System.Drawing.Size(75, 20);
            this.textBoxLevelUpstream2.TabIndex = 11;
            this.textBoxLevelUpstream2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxWidthCrest
            // 
            this.textBoxWidthCrest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxWidthCrest.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGeneralStructure, "WidthStructureCentre", true));
            this.textBoxWidthCrest.Location = new System.Drawing.Point(246, 49);
            this.textBoxWidthCrest.Name = "textBoxWidthCrest";
            this.textBoxWidthCrest.Size = new System.Drawing.Size(75, 20);
            this.textBoxWidthCrest.TabIndex = 17;
            this.textBoxWidthCrest.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxLevelCrest
            // 
            this.textBoxLevelCrest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLevelCrest.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGeneralStructure, "BedLevelStructureCentre", true));
            this.textBoxLevelCrest.Location = new System.Drawing.Point(246, 26);
            this.textBoxLevelCrest.Name = "textBoxLevelCrest";
            this.textBoxLevelCrest.Size = new System.Drawing.Size(75, 20);
            this.textBoxLevelCrest.TabIndex = 12;
            this.textBoxLevelCrest.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxWidthDownstream1
            // 
            this.textBoxWidthDownstream1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxWidthDownstream1.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGeneralStructure, "WidthStructureRightSide", true));
            this.textBoxWidthDownstream1.Location = new System.Drawing.Point(327, 49);
            this.textBoxWidthDownstream1.Name = "textBoxWidthDownstream1";
            this.textBoxWidthDownstream1.Size = new System.Drawing.Size(75, 20);
            this.textBoxWidthDownstream1.TabIndex = 18;
            this.textBoxWidthDownstream1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxLevelDownstream1
            // 
            this.textBoxLevelDownstream1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLevelDownstream1.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGeneralStructure, "BedLevelRightSideStructure", true));
            this.textBoxLevelDownstream1.Location = new System.Drawing.Point(327, 26);
            this.textBoxLevelDownstream1.Name = "textBoxLevelDownstream1";
            this.textBoxLevelDownstream1.Size = new System.Drawing.Size(75, 20);
            this.textBoxLevelDownstream1.TabIndex = 13;
            this.textBoxLevelDownstream1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxLevelDownstream2
            // 
            this.textBoxLevelDownstream2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLevelDownstream2.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGeneralStructure, "BedLevelRightSideOfStructure", true));
            this.textBoxLevelDownstream2.Location = new System.Drawing.Point(408, 26);
            this.textBoxLevelDownstream2.Name = "textBoxLevelDownstream2";
            this.textBoxLevelDownstream2.Size = new System.Drawing.Size(78, 20);
            this.textBoxLevelDownstream2.TabIndex = 14;
            this.textBoxLevelDownstream2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxWidthDownstream2
            // 
            this.textBoxWidthDownstream2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxWidthDownstream2.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGeneralStructure, "WidthRightSideOfStructure", true));
            this.textBoxWidthDownstream2.Location = new System.Drawing.Point(408, 49);
            this.textBoxWidthDownstream2.Name = "textBoxWidthDownstream2";
            this.textBoxWidthDownstream2.Size = new System.Drawing.Size(78, 20);
            this.textBoxWidthDownstream2.TabIndex = 19;
            this.textBoxWidthDownstream2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxWidthUpstream1
            // 
            this.textBoxWidthUpstream1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxWidthUpstream1.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGeneralStructure, "WidthLeftSideOfStructure", true));
            this.textBoxWidthUpstream1.Location = new System.Drawing.Point(84, 49);
            this.textBoxWidthUpstream1.Name = "textBoxWidthUpstream1";
            this.textBoxWidthUpstream1.Size = new System.Drawing.Size(75, 20);
            this.textBoxWidthUpstream1.TabIndex = 15;
            this.textBoxWidthUpstream1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label13
            // 
            this.label13.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(3, 52);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(75, 13);
            this.label13.TabIndex = 9;
            this.label13.Text = "Width (m)";
            // 
            // textBoxWidthUpstream2
            // 
            this.textBoxWidthUpstream2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxWidthUpstream2.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGeneralStructure, "WidthStructureLeftSide", true));
            this.textBoxWidthUpstream2.Location = new System.Drawing.Point(165, 49);
            this.textBoxWidthUpstream2.Name = "textBoxWidthUpstream2";
            this.textBoxWidthUpstream2.Size = new System.Drawing.Size(75, 20);
            this.textBoxWidthUpstream2.TabIndex = 16;
            this.textBoxWidthUpstream2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // checkBoxExtraResistance
            // 
            this.checkBoxExtraResistance.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxExtraResistance.AutoSize = true;
            this.checkBoxExtraResistance.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.bindingSourceGeneralStructure, "UseExtraResistance", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxExtraResistance.Location = new System.Drawing.Point(307, 6);
            this.checkBoxExtraResistance.Name = "checkBoxExtraResistance";
            this.checkBoxExtraResistance.Size = new System.Drawing.Size(113, 17);
            this.checkBoxExtraResistance.TabIndex = 20;
            this.checkBoxExtraResistance.Text = "Extra resistance";
            this.checkBoxExtraResistance.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBoxExtraResistance.UseVisualStyleBackColor = true;
            // 
            // textBoxExtraResistance
            // 
            this.textBoxExtraResistance.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGeneralStructure, "ExtraResistance", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N3"));
            this.textBoxExtraResistance.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourceGeneralStructure, "UseExtraResistance", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.textBoxExtraResistance.Location = new System.Drawing.Point(426, 3);
            this.textBoxExtraResistance.Name = "textBoxExtraResistance";
            this.textBoxExtraResistance.Size = new System.Drawing.Size(76, 20);
            this.textBoxExtraResistance.TabIndex = 21;
            this.textBoxExtraResistance.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxCrestLength
            // 
            this.textBoxCrestLength.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxCrestLength.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGeneralStructure, "CrestLength", true));
            this.textBoxCrestLength.Location = new System.Drawing.Point(132, 4);
            this.textBoxCrestLength.Name = "textBoxCrestLength";
            this.textBoxCrestLength.Size = new System.Drawing.Size(46, 20);
            this.textBoxCrestLength.TabIndex = 22;
            this.textBoxCrestLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxGateOpeningWidth
            // 
            this.textBoxGateOpeningWidth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxGateOpeningWidth.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGeneralStructure, "GateOpeningWidth", true));
            this.textBoxGateOpeningWidth.Location = new System.Drawing.Point(132, 33);
            this.textBoxGateOpeningWidth.Name = "textBoxGateOpeningWidth";
            this.textBoxGateOpeningWidth.Size = new System.Drawing.Size(46, 20);
            this.textBoxGateOpeningWidth.TabIndex = 23;
            this.textBoxGateOpeningWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // labelCrestLength
            // 
            this.labelCrestLength.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCrestLength.AutoSize = true;
            this.labelCrestLength.Location = new System.Drawing.Point(3, 8);
            this.labelCrestLength.Name = "labelCrestLength";
            this.labelCrestLength.Size = new System.Drawing.Size(123, 13);
            this.labelCrestLength.TabIndex = 24;
            this.labelCrestLength.Text = "Crest length";
            // 
            // labelGateOpeningWidth
            // 
            this.labelGateOpeningWidth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelGateOpeningWidth.AutoSize = true;
            this.labelGateOpeningWidth.Location = new System.Drawing.Point(3, 37);
            this.labelGateOpeningWidth.Name = "labelGateOpeningWidth";
            this.labelGateOpeningWidth.Size = new System.Drawing.Size(123, 13);
            this.labelGateOpeningWidth.TabIndex = 25;
            this.labelGateOpeningWidth.Text = "Gate opening width";
            // 
            // comboBoxGateOpeningDirection
            // 
            this.comboBoxGateOpeningDirection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxGateOpeningDirection.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceGeneralStructure, "GateOpeningHorizontalDirection", true));
            this.comboBoxGateOpeningDirection.FormattingEnabled = true;
            this.comboBoxGateOpeningDirection.Location = new System.Drawing.Point(184, 63);
            this.comboBoxGateOpeningDirection.Name = "comboBoxGateOpeningDirection";
            this.comboBoxGateOpeningDirection.Size = new System.Drawing.Size(117, 21);
            this.comboBoxGateOpeningDirection.TabIndex = 26;
            // 
            // labelGateOpeningDirection
            // 
            this.labelGateOpeningDirection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelGateOpeningDirection.AutoSize = true;
            this.tableLayoutPanel3.SetColumnSpan(this.labelGateOpeningDirection, 2);
            this.labelGateOpeningDirection.Location = new System.Drawing.Point(3, 67);
            this.labelGateOpeningDirection.Name = "labelGateOpeningDirection";
            this.labelGateOpeningDirection.Size = new System.Drawing.Size(175, 13);
            this.labelGateOpeningDirection.TabIndex = 27;
            this.labelGateOpeningDirection.Text = "Gate opening horizontal direction";
            this.labelGateOpeningDirection.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 5;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 129F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 52F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 123F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 119F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 81F));
            this.tableLayoutPanel3.Controls.Add(this.textBoxCrestLength, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.textBoxExtraResistance, 4, 0);
            this.tableLayoutPanel3.Controls.Add(this.comboBoxGateOpeningDirection, 2, 2);
            this.tableLayoutPanel3.Controls.Add(this.checkBoxExtraResistance, 3, 0);
            this.tableLayoutPanel3.Controls.Add(this.labelGateOpeningDirection, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.labelCrestLength, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.textBoxGateOpeningWidth, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.labelGateOpeningWidth, 0, 1);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(11, 156);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 3;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33332F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(505, 89);
            this.tableLayoutPanel3.TabIndex = 28;
            // 
            // GeneralStructureWeirFormulaView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.tableLayoutPanel3);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "GeneralStructureWeirFormulaView";
            this.Size = new System.Drawing.Size(768, 252);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceGeneralStructure)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox textboxFreeGatePos;
        private System.Windows.Forms.TextBox textboxDrownedGatePos;
        private System.Windows.Forms.TextBox textboxFreeGateNeg;
        private System.Windows.Forms.TextBox textboxDrownedGateNeg;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.BindingSource bindingSourceGeneralStructure;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox textBoxFreeWeirFlowPos;
        private System.Windows.Forms.TextBox textBoxFreeWeirFlowNeg;
        private System.Windows.Forms.TextBox textBoxDrownedWeirFlowPos;
        private System.Windows.Forms.TextBox textBoxDrownedWeirFlowNeg;
        private System.Windows.Forms.TextBox textBoxContractionCoefNeg;
        private System.Windows.Forms.TextBox textBoxContractionCoefPos;
        private System.Windows.Forms.TextBox textBoxLevelUpstream1;
        private System.Windows.Forms.TextBox textBoxLevelUpstream2;
        private System.Windows.Forms.TextBox textBoxWidthCrest;
        private System.Windows.Forms.TextBox textBoxLevelCrest;
        private System.Windows.Forms.TextBox textBoxWidthDownstream1;
        private System.Windows.Forms.TextBox textBoxLevelDownstream1;
        private System.Windows.Forms.TextBox textBoxLevelDownstream2;
        private System.Windows.Forms.TextBox textBoxWidthDownstream2;
        private System.Windows.Forms.TextBox textBoxWidthUpstream1;
        private System.Windows.Forms.TextBox textBoxWidthUpstream2;
        private System.Windows.Forms.CheckBox checkBoxExtraResistance;
        private System.Windows.Forms.TextBox textBoxExtraResistance;
        private System.Windows.Forms.TextBox textBoxCrestLength;
        private System.Windows.Forms.TextBox textBoxGateOpeningWidth;
        private System.Windows.Forms.Label labelCrestLength;
        private System.Windows.Forms.Label labelGateOpeningWidth;
        private System.Windows.Forms.ComboBox comboBoxGateOpeningDirection;
        private System.Windows.Forms.Label labelGateOpeningDirection;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
    }
}