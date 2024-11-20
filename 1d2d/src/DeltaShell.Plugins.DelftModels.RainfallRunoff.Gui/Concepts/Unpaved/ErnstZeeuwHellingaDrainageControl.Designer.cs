using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Unpaved;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.Unpaved
{
    partial class ErnstZeeuwHellingaDrainageControl
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
            this.surfaceValue = new System.Windows.Forms.TextBox();
            this.ernstDeZeeuwHellingaDrainageFormulaBaseBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.levelOneValue = new System.Windows.Forms.TextBox();
            this.levelTwoValue = new System.Windows.Forms.TextBox();
            this.levelThreeValue = new System.Windows.Forms.TextBox();
            this.levelFourValue = new System.Windows.Forms.TextBox();
            this.txtLevelOneTo = new System.Windows.Forms.TextBox();
            this.txtLevelTwoTo = new System.Windows.Forms.TextBox();
            this.txtLevelThreeTo = new System.Windows.Forms.TextBox();
            this.txtLevelOneFrom = new System.Windows.Forms.TextBox();
            this.txtLevelTwoFrom = new System.Windows.Forms.TextBox();
            this.txtLevelThreeFrom = new System.Windows.Forms.TextBox();
            this.txtLevelFourFrom = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.reactionFactorLbl = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.horizontalInflowValue = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.levelOneCkbox = new System.Windows.Forms.CheckBox();
            this.levelTwoCkbox = new System.Windows.Forms.CheckBox();
            this.levelThreeCkbox = new System.Windows.Forms.CheckBox();
            this.levelOnePanel = new System.Windows.Forms.Panel();
            this.levelTwoPanel = new System.Windows.Forms.Panel();
            this.levelThreePanel = new System.Windows.Forms.Panel();
            this.levelFourPanel = new System.Windows.Forms.Panel();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.ernstDeZeeuwHellingaDrainageFormulaBaseBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.levelOnePanel.SuspendLayout();
            this.levelTwoPanel.SuspendLayout();
            this.levelThreePanel.SuspendLayout();
            this.levelFourPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // surfaceValue
            // 
            this.surfaceValue.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.ernstDeZeeuwHellingaDrainageFormulaBaseBindingSource, "SurfaceRunoff", true));
            this.surfaceValue.Location = new System.Drawing.Point(146, 40);
            this.surfaceValue.Name = "surfaceValue";
            this.surfaceValue.Size = new System.Drawing.Size(100, 20);
            this.surfaceValue.TabIndex = 0;
            this.surfaceValue.Validated += new System.EventHandler(this.ValuesChanged);
            // 
            // ernstDeZeeuwHellingaDrainageFormulaBaseBindingSource
            // 
            this.ernstDeZeeuwHellingaDrainageFormulaBaseBindingSource.DataSource = typeof(ErnstDeZeeuwHellingaDrainageFormulaBase);
            // 
            // levelOneValue
            // 
            this.levelOneValue.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.ernstDeZeeuwHellingaDrainageFormulaBaseBindingSource, "LevelOneValue", true));
            this.levelOneValue.Location = new System.Drawing.Point(125, 3);
            this.levelOneValue.Name = "levelOneValue";
            this.levelOneValue.Size = new System.Drawing.Size(100, 20);
            this.levelOneValue.TabIndex = 2;
            this.levelOneValue.Validated += new System.EventHandler(this.ValuesChanged);
            // 
            // levelTwoValue
            // 
            this.levelTwoValue.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.ernstDeZeeuwHellingaDrainageFormulaBaseBindingSource, "LevelTwoValue", true));
            this.levelTwoValue.Location = new System.Drawing.Point(125, 2);
            this.levelTwoValue.Name = "levelTwoValue";
            this.levelTwoValue.Size = new System.Drawing.Size(100, 20);
            this.levelTwoValue.TabIndex = 4;
            this.levelTwoValue.Validated += new System.EventHandler(this.ValuesChanged);
            // 
            // levelThreeValue
            // 
            this.levelThreeValue.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.ernstDeZeeuwHellingaDrainageFormulaBaseBindingSource, "LevelThreeValue", true));
            this.levelThreeValue.Location = new System.Drawing.Point(125, 3);
            this.levelThreeValue.Name = "levelThreeValue";
            this.levelThreeValue.Size = new System.Drawing.Size(100, 20);
            this.levelThreeValue.TabIndex = 6;
            this.levelThreeValue.Validated += new System.EventHandler(this.ValuesChanged);
            // 
            // levelFourValue
            // 
            this.levelFourValue.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.ernstDeZeeuwHellingaDrainageFormulaBaseBindingSource, "InfiniteDrainageLevelRunoff", true));
            this.levelFourValue.Location = new System.Drawing.Point(125, 3);
            this.levelFourValue.Name = "levelFourValue";
            this.levelFourValue.Size = new System.Drawing.Size(100, 20);
            this.levelFourValue.TabIndex = 7;
            this.levelFourValue.Validated += new System.EventHandler(this.ValuesChanged);
            // 
            // txtLevelOneTo
            // 
            this.txtLevelOneTo.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.ernstDeZeeuwHellingaDrainageFormulaBaseBindingSource, "LevelOneTo", true));
            this.txtLevelOneTo.Location = new System.Drawing.Point(59, 3);
            this.txtLevelOneTo.Name = "txtLevelOneTo";
            this.txtLevelOneTo.Size = new System.Drawing.Size(40, 20);
            this.txtLevelOneTo.TabIndex = 1;
            this.txtLevelOneTo.Validating += new System.ComponentModel.CancelEventHandler(this.ToTextBoxValidating);
            this.txtLevelOneTo.Validated += new System.EventHandler(this.ToTextBoxValidated);
            // 
            // txtLevelTwoTo
            // 
            this.txtLevelTwoTo.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.ernstDeZeeuwHellingaDrainageFormulaBaseBindingSource, "LevelTwoTo", true));
            this.txtLevelTwoTo.Location = new System.Drawing.Point(59, 2);
            this.txtLevelTwoTo.Name = "txtLevelTwoTo";
            this.txtLevelTwoTo.Size = new System.Drawing.Size(40, 20);
            this.txtLevelTwoTo.TabIndex = 3;
            this.txtLevelTwoTo.Validating += new System.ComponentModel.CancelEventHandler(this.ToTextBoxValidating);
            this.txtLevelTwoTo.Validated += new System.EventHandler(this.ToTextBoxValidated);
            // 
            // txtLevelThreeTo
            // 
            this.txtLevelThreeTo.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.ernstDeZeeuwHellingaDrainageFormulaBaseBindingSource, "LevelThreeTo", true));
            this.txtLevelThreeTo.Location = new System.Drawing.Point(59, 3);
            this.txtLevelThreeTo.Name = "txtLevelThreeTo";
            this.txtLevelThreeTo.Size = new System.Drawing.Size(40, 20);
            this.txtLevelThreeTo.TabIndex = 5;
            this.txtLevelThreeTo.Validating += new System.ComponentModel.CancelEventHandler(this.ToTextBoxValidating);
            this.txtLevelThreeTo.Validated += new System.EventHandler(this.ToTextBoxValidated);
            // 
            // txtLevelOneFrom
            // 
            this.txtLevelOneFrom.Enabled = false;
            this.txtLevelOneFrom.Location = new System.Drawing.Point(3, 3);
            this.txtLevelOneFrom.Name = "txtLevelOneFrom";
            this.txtLevelOneFrom.Size = new System.Drawing.Size(40, 20);
            this.txtLevelOneFrom.TabIndex = 0;
            this.txtLevelOneFrom.Text = "0";
            // 
            // txtLevelTwoFrom
            // 
            this.txtLevelTwoFrom.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.ernstDeZeeuwHellingaDrainageFormulaBaseBindingSource, "LevelOneTo", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.txtLevelTwoFrom.Enabled = false;
            this.txtLevelTwoFrom.Location = new System.Drawing.Point(3, 2);
            this.txtLevelTwoFrom.Name = "txtLevelTwoFrom";
            this.txtLevelTwoFrom.Size = new System.Drawing.Size(40, 20);
            this.txtLevelTwoFrom.TabIndex = 0;
            // 
            // txtLevelThreeFrom
            // 
            this.txtLevelThreeFrom.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.ernstDeZeeuwHellingaDrainageFormulaBaseBindingSource, "LevelTwoTo", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.txtLevelThreeFrom.Enabled = false;
            this.txtLevelThreeFrom.Location = new System.Drawing.Point(3, 3);
            this.txtLevelThreeFrom.Name = "txtLevelThreeFrom";
            this.txtLevelThreeFrom.Size = new System.Drawing.Size(40, 20);
            this.txtLevelThreeFrom.TabIndex = 0;
            // 
            // txtLevelFourFrom
            // 
            this.txtLevelFourFrom.Enabled = false;
            this.txtLevelFourFrom.Location = new System.Drawing.Point(3, 3);
            this.txtLevelFourFrom.Name = "txtLevelFourFrom";
            this.txtLevelFourFrom.Size = new System.Drawing.Size(40, 20);
            this.txtLevelFourFrom.TabIndex = 0;
            this.txtLevelFourFrom.Text = "0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(80, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Surface";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(59, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Infinity";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(46, 6);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(10, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "-";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(46, 5);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(10, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "-";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(46, 6);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(10, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "-";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(46, 6);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(10, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "-";
            // 
            // reactionFactorLbl
            // 
            this.reactionFactorLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.reactionFactorLbl.Location = new System.Drawing.Point(146, 7);
            this.reactionFactorLbl.Name = "reactionFactorLbl";
            this.reactionFactorLbl.Size = new System.Drawing.Size(100, 30);
            this.reactionFactorLbl.TabIndex = 2;
            this.reactionFactorLbl.Text = "Reaction factor: [1/day]";
            // 
            // label8
            // 
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(21, 7);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(107, 30);
            this.label8.TabIndex = 2;
            this.label8.Text = "Drainage level: [m below surface]";
            // 
            // horizontalInflowValue
            // 
            this.horizontalInflowValue.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.ernstDeZeeuwHellingaDrainageFormulaBaseBindingSource, "HorizontalInflow", true));
            this.horizontalInflowValue.Location = new System.Drawing.Point(146, 188);
            this.horizontalInflowValue.Name = "horizontalInflowValue";
            this.horizontalInflowValue.Size = new System.Drawing.Size(100, 20);
            this.horizontalInflowValue.TabIndex = 8;
            this.horizontalInflowValue.Validated += new System.EventHandler(this.ValuesChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(21, 191);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(85, 13);
            this.label9.TabIndex = 1;
            this.label9.Text = "Horizontal Inflow";
            // 
            // errorProvider
            // 
            this.errorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.errorProvider.ContainerControl = this;
            // 
            // levelOneCkbox
            // 
            this.levelOneCkbox.AutoSize = true;
            this.levelOneCkbox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.ernstDeZeeuwHellingaDrainageFormulaBaseBindingSource, "LevelOneEnabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.levelOneCkbox.Location = new System.Drawing.Point(3, 69);
            this.levelOneCkbox.Name = "levelOneCkbox";
            this.levelOneCkbox.Size = new System.Drawing.Size(15, 14);
            this.levelOneCkbox.TabIndex = 9;
            this.levelOneCkbox.UseVisualStyleBackColor = true;
            this.levelOneCkbox.CheckedChanged += new System.EventHandler(this.CheckBoxCheckedChanged);
            // 
            // levelTwoCkbox
            // 
            this.levelTwoCkbox.AutoSize = true;
            this.levelTwoCkbox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.ernstDeZeeuwHellingaDrainageFormulaBaseBindingSource, "LevelTwoEnabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.levelTwoCkbox.Enabled = false;
            this.levelTwoCkbox.Location = new System.Drawing.Point(3, 95);
            this.levelTwoCkbox.Name = "levelTwoCkbox";
            this.levelTwoCkbox.Size = new System.Drawing.Size(15, 14);
            this.levelTwoCkbox.TabIndex = 9;
            this.levelTwoCkbox.UseVisualStyleBackColor = true;
            this.levelTwoCkbox.CheckedChanged += new System.EventHandler(this.CheckBoxCheckedChanged);
            // 
            // levelThreeCkbox
            // 
            this.levelThreeCkbox.AutoSize = true;
            this.levelThreeCkbox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.ernstDeZeeuwHellingaDrainageFormulaBaseBindingSource, "LevelThreeEnabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.levelThreeCkbox.Enabled = false;
            this.levelThreeCkbox.Location = new System.Drawing.Point(3, 121);
            this.levelThreeCkbox.Name = "levelThreeCkbox";
            this.levelThreeCkbox.Size = new System.Drawing.Size(15, 14);
            this.levelThreeCkbox.TabIndex = 9;
            this.levelThreeCkbox.UseVisualStyleBackColor = true;
            this.levelThreeCkbox.CheckedChanged += new System.EventHandler(this.CheckBoxCheckedChanged);
            // 
            // levelOnePanel
            // 
            this.levelOnePanel.Controls.Add(this.txtLevelOneFrom);
            this.levelOnePanel.Controls.Add(this.levelOneValue);
            this.levelOnePanel.Controls.Add(this.txtLevelOneTo);
            this.levelOnePanel.Controls.Add(this.label3);
            this.levelOnePanel.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.ernstDeZeeuwHellingaDrainageFormulaBaseBindingSource, "LevelOneEnabled", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.levelOnePanel.Enabled = false;
            this.levelOnePanel.Location = new System.Drawing.Point(21, 63);
            this.levelOnePanel.Name = "levelOnePanel";
            this.levelOnePanel.Size = new System.Drawing.Size(228, 25);
            this.levelOnePanel.TabIndex = 0;
            this.levelOnePanel.EnabledChanged += new System.EventHandler(this.PanelEnabledChanged);
            // 
            // levelTwoPanel
            // 
            this.levelTwoPanel.Controls.Add(this.txtLevelTwoFrom);
            this.levelTwoPanel.Controls.Add(this.levelTwoValue);
            this.levelTwoPanel.Controls.Add(this.txtLevelTwoTo);
            this.levelTwoPanel.Controls.Add(this.label4);
            this.levelTwoPanel.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.ernstDeZeeuwHellingaDrainageFormulaBaseBindingSource, "LevelTwoEnabled", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.levelTwoPanel.Enabled = false;
            this.levelTwoPanel.Location = new System.Drawing.Point(21, 89);
            this.levelTwoPanel.Name = "levelTwoPanel";
            this.levelTwoPanel.Size = new System.Drawing.Size(228, 25);
            this.levelTwoPanel.TabIndex = 10;
            this.levelTwoPanel.EnabledChanged += new System.EventHandler(this.PanelEnabledChanged);
            // 
            // levelThreePanel
            // 
            this.levelThreePanel.Controls.Add(this.txtLevelThreeFrom);
            this.levelThreePanel.Controls.Add(this.levelThreeValue);
            this.levelThreePanel.Controls.Add(this.txtLevelThreeTo);
            this.levelThreePanel.Controls.Add(this.label5);
            this.levelThreePanel.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.ernstDeZeeuwHellingaDrainageFormulaBaseBindingSource, "LevelThreeEnabled", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.levelThreePanel.Enabled = false;
            this.levelThreePanel.Location = new System.Drawing.Point(21, 115);
            this.levelThreePanel.Name = "levelThreePanel";
            this.levelThreePanel.Size = new System.Drawing.Size(228, 25);
            this.levelThreePanel.TabIndex = 0;
            this.levelThreePanel.EnabledChanged += new System.EventHandler(this.PanelEnabledChanged);
            // 
            // levelFourPanel
            // 
            this.levelFourPanel.Controls.Add(this.txtLevelFourFrom);
            this.levelFourPanel.Controls.Add(this.levelFourValue);
            this.levelFourPanel.Controls.Add(this.label2);
            this.levelFourPanel.Controls.Add(this.label6);
            this.levelFourPanel.Location = new System.Drawing.Point(21, 141);
            this.levelFourPanel.Name = "levelFourPanel";
            this.levelFourPanel.Size = new System.Drawing.Size(228, 25);
            this.levelFourPanel.TabIndex = 0;
            // 
            // pictureBox
            // 
            this.pictureBox.Location = new System.Drawing.Point(255, 7);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(277, 201);
            this.pictureBox.TabIndex = 11;
            this.pictureBox.TabStop = false;
            // 
            // ErnstZeeuwHellingaDrainageControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.levelFourPanel);
            this.Controls.Add(this.levelThreePanel);
            this.Controls.Add(this.levelTwoPanel);
            this.Controls.Add(this.levelOnePanel);
            this.Controls.Add(this.levelThreeCkbox);
            this.Controls.Add(this.levelTwoCkbox);
            this.Controls.Add(this.levelOneCkbox);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.reactionFactorLbl);
            this.Controls.Add(this.horizontalInflowValue);
            this.Controls.Add(this.surfaceValue);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label1);
            this.Name = "ErnstZeeuwHellingaDrainageControl";
            this.Size = new System.Drawing.Size(539, 219);
            this.Load += new System.EventHandler(this.ErnstZeeuwHellingaDrainageControlLoad);
            ((System.ComponentModel.ISupportInitialize)(this.ernstDeZeeuwHellingaDrainageFormulaBaseBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.levelOnePanel.ResumeLayout(false);
            this.levelOnePanel.PerformLayout();
            this.levelTwoPanel.ResumeLayout(false);
            this.levelTwoPanel.PerformLayout();
            this.levelThreePanel.ResumeLayout(false);
            this.levelThreePanel.PerformLayout();
            this.levelFourPanel.ResumeLayout(false);
            this.levelFourPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox surfaceValue;
        private System.Windows.Forms.TextBox levelOneValue;
        private System.Windows.Forms.TextBox levelTwoValue;
        private System.Windows.Forms.TextBox levelThreeValue;
        private System.Windows.Forms.TextBox levelFourValue;
        private System.Windows.Forms.TextBox txtLevelOneTo;
        private System.Windows.Forms.TextBox txtLevelTwoTo;
        private System.Windows.Forms.TextBox txtLevelThreeTo;
        private System.Windows.Forms.TextBox txtLevelOneFrom;
        private System.Windows.Forms.TextBox txtLevelTwoFrom;
        private System.Windows.Forms.TextBox txtLevelThreeFrom;
        private System.Windows.Forms.TextBox txtLevelFourFrom;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label reactionFactorLbl;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox horizontalInflowValue;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ErrorProvider errorProvider;
        private System.Windows.Forms.CheckBox levelThreeCkbox;
        private System.Windows.Forms.CheckBox levelTwoCkbox;
        private System.Windows.Forms.CheckBox levelOneCkbox;
        private System.Windows.Forms.Panel levelOnePanel;
        private System.Windows.Forms.Panel levelTwoPanel;
        private System.Windows.Forms.Panel levelThreePanel;
        private System.Windows.Forms.Panel levelFourPanel;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.BindingSource ernstDeZeeuwHellingaDrainageFormulaBaseBindingSource;


    }
}
