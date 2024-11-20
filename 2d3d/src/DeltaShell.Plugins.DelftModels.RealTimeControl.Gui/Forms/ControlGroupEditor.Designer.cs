namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms
{
    partial class ControlGroupEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ControlGroupEditor));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsbInput = new System.Windows.Forms.ToolStripButton();
            this.tsbMathExpression = new System.Windows.Forms.ToolStripButton();
            this.tsbCondition = new System.Windows.Forms.ToolStripButton();
            this.tsbRule = new System.Windows.Forms.ToolStripButton();
            this.tsbOutput = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonResize = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonMakeSameWidth = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonMakeSameHeight = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonAlignMiddle = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonAlignCenter = new System.Windows.Forms.ToolStripButton();
            this.tsbSignal = new System.Windows.Forms.ToolStripButton();
            this.graphControl = new DelftTools.Controls.Swf.Graph.GraphControl();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbInput,
            this.tsbMathExpression,
            this.tsbCondition,
            this.tsbRule,
            this.tsbOutput,
            this.toolStripButtonResize,
            this.toolStripSeparator1,
            this.toolStripButtonMakeSameWidth,
            this.toolStripButtonMakeSameHeight,
            this.toolStripButtonAlignMiddle,
            this.toolStripButtonAlignCenter,
            this.tsbSignal});
            this.toolStrip1.Location = new System.Drawing.Point(0, 539);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(620, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tsbInput
            // 
            this.tsbInput.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbInput.Image = ((System.Drawing.Image)(resources.GetObject("tsbInput.Image")));
            this.tsbInput.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbInput.Name = "tsbInput";
            this.tsbInput.Size = new System.Drawing.Size(23, 22);
            this.tsbInput.Text = "toolStripButton1";
            this.tsbInput.ToolTipText = "Adds an input location to the diagram";
            this.tsbInput.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnTsbInputClick);

            // Tool strip button Mathematical Expression
            this.tsbMathExpression.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbMathExpression.Image = ((System.Drawing.Image)(resources.GetObject("tsbMathExpression.Image")));
            this.tsbMathExpression.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbMathExpression.Name = "tsbMathExpression";
            this.tsbMathExpression.Size = new System.Drawing.Size(23, 22);
            this.tsbMathExpression.Text = "toolStripButton1";
            this.tsbMathExpression.ToolTipText = "Adds a mathematical expression to the diagram";
            this.tsbMathExpression.Click += new System.EventHandler(this.OnTsbMathExpressionSignalClick);

            // 
            // tsbCondition
            // 
            this.tsbCondition.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbCondition.Image = ((System.Drawing.Image)(resources.GetObject("tsbCondition.Image")));
            this.tsbCondition.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbCondition.Name = "tsbCondition";
            this.tsbCondition.Size = new System.Drawing.Size(23, 22);
            this.tsbCondition.Text = "toolStripButton1";
            this.tsbCondition.ToolTipText = "Adds a condition to the diagram";
            this.tsbCondition.Click += new System.EventHandler(this.OnTsbConditionClick);
            // 
            // tsbRule
            // 
            this.tsbRule.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbRule.Image = ((System.Drawing.Image)(resources.GetObject("tsbRule.Image")));
            this.tsbRule.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbRule.Name = "tsbRule";
            this.tsbRule.Size = new System.Drawing.Size(23, 22);
            this.tsbRule.Text = "toolStripButton1";
            this.tsbRule.ToolTipText = "Adds a rule to the diagram";
            this.tsbRule.Click += new System.EventHandler(this.OnTsbRuleClick);
            // 
            // tsbOutput
            // 
            this.tsbOutput.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbOutput.Image = ((System.Drawing.Image)(resources.GetObject("tsbOutput.Image")));
            this.tsbOutput.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbOutput.Name = "tsbOutput";
            this.tsbOutput.Size = new System.Drawing.Size(23, 22);
            this.tsbOutput.Text = "toolStripButton1";
            this.tsbOutput.ToolTipText = "Adds an output location to the diagram";
            this.tsbOutput.Click += new System.EventHandler(this.OnTsbOutputClick);
            // 
            // toolStripButtonResize
            // 
            this.toolStripButtonResize.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButtonResize.CheckOnClick = true;
            this.toolStripButtonResize.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonResize.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonResize.Image")));
            this.toolStripButtonResize.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonResize.Name = "toolStripButtonResize";
            this.toolStripButtonResize.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonResize.Text = "Auto size components";
            this.toolStripButtonResize.CheckedChanged += new System.EventHandler(this.ToolStripButtonResizeCheckedChanged);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButtonMakeSameWidth
            // 
            this.toolStripButtonMakeSameWidth.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButtonMakeSameWidth.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonMakeSameWidth.Enabled = false;
            this.toolStripButtonMakeSameWidth.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonMakeSameWidth.Image")));
            this.toolStripButtonMakeSameWidth.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonMakeSameWidth.Name = "toolStripButtonMakeSameWidth";
            this.toolStripButtonMakeSameWidth.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonMakeSameWidth.Text = "Make same width";
            this.toolStripButtonMakeSameWidth.Click += new System.EventHandler(this.ToolStripButtonMakeSameWidthClick);
            // 
            // toolStripButtonMakeSameHeight
            // 
            this.toolStripButtonMakeSameHeight.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButtonMakeSameHeight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonMakeSameHeight.Enabled = false;
            this.toolStripButtonMakeSameHeight.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonMakeSameHeight.Image")));
            this.toolStripButtonMakeSameHeight.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonMakeSameHeight.Name = "toolStripButtonMakeSameHeight";
            this.toolStripButtonMakeSameHeight.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonMakeSameHeight.Text = "Make same height";
            this.toolStripButtonMakeSameHeight.Click += new System.EventHandler(this.ToolStripButtonMakeSameHeightClick);
            // 
            // toolStripButtonAlignMiddle
            // 
            this.toolStripButtonAlignMiddle.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButtonAlignMiddle.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonAlignMiddle.Enabled = false;
            this.toolStripButtonAlignMiddle.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonAlignMiddle.Image")));
            this.toolStripButtonAlignMiddle.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonAlignMiddle.Name = "toolStripButtonAlignMiddle";
            this.toolStripButtonAlignMiddle.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonAlignMiddle.Text = "Align middle";
            this.toolStripButtonAlignMiddle.Click += new System.EventHandler(this.ToolStripButtonAlignMiddleClick);
            // 
            // toolStripButtonAlignCenter
            // 
            this.toolStripButtonAlignCenter.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButtonAlignCenter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonAlignCenter.Enabled = false;
            this.toolStripButtonAlignCenter.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonAlignCenter.Image")));
            this.toolStripButtonAlignCenter.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonAlignCenter.Name = "toolStripButtonAlignCenter";
            this.toolStripButtonAlignCenter.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonAlignCenter.Text = "Align centers";
            this.toolStripButtonAlignCenter.Click += new System.EventHandler(this.ToolStripButtonAlignCenterClick);
            // 
            // tsbSignal
            // 
            this.tsbSignal.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbSignal.Image = ((System.Drawing.Image)(resources.GetObject("tsbSignal.Image")));
            this.tsbSignal.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbSignal.Name = "tsbSignal";
            this.tsbSignal.Size = new System.Drawing.Size(23, 22);
            this.tsbSignal.Text = "toolStripButton1";
            this.tsbSignal.ToolTipText = "Adds a signal location to the diagram";
            this.tsbSignal.Click += new System.EventHandler(this.OnTsbSignalClick);
            // 
            // graphControl
            // 
            this.graphControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphControl.Location = new System.Drawing.Point(0, 0);
            this.graphControl.Name = "graphControl";
            this.graphControl.ReadOnly = false;
            this.graphControl.ScrollBars = true;
            this.graphControl.Size = new System.Drawing.Size(620, 539);
            this.graphControl.TabIndex = 2;

            // 
            // ControlGroupEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.graphControl);
            this.Controls.Add(this.toolStrip1);
            this.Name = "ControlGroupEditor";
            this.Size = new System.Drawing.Size(620, 564);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbInput;
        private System.Windows.Forms.ToolStripButton tsbMathExpression;
        private System.Windows.Forms.ToolStripButton tsbCondition;
        private System.Windows.Forms.ToolStripButton tsbRule;
        private System.Windows.Forms.ToolStripButton tsbOutput;
        private System.Windows.Forms.ToolStripButton toolStripButtonResize;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButtonAlignCenter;
        private System.Windows.Forms.ToolStripButton toolStripButtonAlignMiddle;
        private System.Windows.Forms.ToolStripButton toolStripButtonMakeSameWidth;
        private System.Windows.Forms.ToolStripButton toolStripButtonMakeSameHeight;
        private DelftTools.Controls.Swf.Graph.GraphControl graphControl;
        private System.Windows.Forms.ToolStripButton tsbSignal;
    }
}
