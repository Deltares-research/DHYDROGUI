namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    partial class SourceAndSinkView
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
            this.functionView = new DeltaShell.Plugins.CommonTools.Gui.Forms.Functions.FunctionView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.areaUnitLabel = new System.Windows.Forms.Label();
            this.areaLabel = new System.Windows.Forms.Label();
            this.areaTextBox = new System.Windows.Forms.TextBox();
            this.includeMomentumCheckBox = new System.Windows.Forms.CheckBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // functionView
            // 
            this.functionView.ChartSeriesType = DelftTools.Controls.Swf.Charting.Series.ChartSeriesType.LineSeries;
            this.functionView.ChartViewOption = DeltaShell.Plugins.CommonTools.Gui.Forms.Charting.ChartViewOptions.AllSeries;
            this.functionView.CreateSeriesMethod = null;
            this.functionView.CurrentTime = null;
            this.functionView.Data = null;
            this.functionView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.functionView.Function = null;
            this.functionView.Functions = null;
            this.functionView.Image = null;
            this.functionView.Location = new System.Drawing.Point(0, 0);
            this.functionView.MaxSeries = 10;
            this.functionView.Name = "functionView";
            this.functionView.ShowChartView = true;
            this.functionView.ShowTableView = true;
            this.functionView.Size = new System.Drawing.Size(561, 319);
            this.functionView.TabIndex = 0;
            this.functionView.ViewInfo = null;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.areaUnitLabel);
            this.panel1.Controls.Add(this.areaLabel);
            this.panel1.Controls.Add(this.areaTextBox);
            this.panel1.Controls.Add(this.includeMomentumCheckBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(561, 27);
            this.panel1.TabIndex = 1;
            // 
            // areaUnitLabel
            // 
            this.areaUnitLabel.AutoSize = true;
            this.areaUnitLabel.Location = new System.Drawing.Point(113, 8);
            this.areaUnitLabel.Name = "areaUnitLabel";
            this.areaUnitLabel.Size = new System.Drawing.Size(18, 13);
            this.areaUnitLabel.TabIndex = 3;
            this.areaUnitLabel.Text = "m²";
            // 
            // areaLabel
            // 
            this.areaLabel.AutoSize = true;
            this.areaLabel.Location = new System.Drawing.Point(3, 8);
            this.areaLabel.Name = "areaLabel";
            this.areaLabel.Size = new System.Drawing.Size(29, 13);
            this.areaLabel.TabIndex = 2;
            this.areaLabel.Text = "Area";
            // 
            // areaTextBox
            // 
            this.areaTextBox.Location = new System.Drawing.Point(38, 5);
            this.areaTextBox.Name = "areaTextBox";
            this.areaTextBox.Size = new System.Drawing.Size(74, 20);
            this.areaTextBox.TabIndex = 1;
            // 
            // includeMomentumCheckBox
            // 
            this.includeMomentumCheckBox.AutoSize = true;
            this.includeMomentumCheckBox.Location = new System.Drawing.Point(150, 7);
            this.includeMomentumCheckBox.Name = "includeMomentumCheckBox";
            this.includeMomentumCheckBox.Size = new System.Drawing.Size(115, 17);
            this.includeMomentumCheckBox.TabIndex = 0;
            this.includeMomentumCheckBox.Text = "Include momentum";
            this.includeMomentumCheckBox.UseVisualStyleBackColor = true;
            this.includeMomentumCheckBox.CheckedChanged += new System.EventHandler(this.includeMomentumCheckBox_CheckedChanged);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.functionView);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 27);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(561, 319);
            this.panel2.TabIndex = 0;
            // 
            // errorProvider1
            // 
            this.errorProvider1.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.errorProvider1.ContainerControl = this;
            // 
            // SourceAndSinkView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "SourceAndSinkView";
            this.Size = new System.Drawing.Size(561, 346);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private CommonTools.Gui.Forms.Functions.FunctionView functionView;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label areaUnitLabel;
        private System.Windows.Forms.Label areaLabel;
        private System.Windows.Forms.TextBox areaTextBox;
        private System.Windows.Forms.CheckBox includeMomentumCheckBox;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ErrorProvider errorProvider1;
    }
}
