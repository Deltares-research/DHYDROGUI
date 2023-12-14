using System.Windows.Forms;
using DelftTools.Controls;
using DeltaShell.Plugins.FMSuite.Common.Gui.Editors;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    sealed partial class FlowBoundaryConditionPropertiesControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        private void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.verticalInterpolationComboBox = new System.Windows.Forms.ComboBox();
            this.reflectionParameterTextBox = new System.Windows.Forms.TextBox();
            this.reflectionUnitLabel = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.factorTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.offsetTextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.offsetUnitLabel = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.thatcherTimeSpanEditor = new DelftTools.Controls.Swf.TimeSpanEditor();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(2, 81);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(128, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Vertical interpolation type:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(2, 112);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(108, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Reflection parameter:";
            // 
            // verticalInterpolationComboBox
            // 
            this.verticalInterpolationComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.verticalInterpolationComboBox.FormattingEnabled = true;
            this.verticalInterpolationComboBox.Location = new System.Drawing.Point(160, 78);
            this.verticalInterpolationComboBox.Name = "verticalInterpolationComboBox";
            this.verticalInterpolationComboBox.Size = new System.Drawing.Size(140, 21);
            this.verticalInterpolationComboBox.TabIndex = 6;
            // 
            // reflectionParameterTextBox
            // 
            this.reflectionParameterTextBox.Location = new System.Drawing.Point(160, 109);
            this.reflectionParameterTextBox.Name = "reflectionParameterTextBox";
            this.reflectionParameterTextBox.Size = new System.Drawing.Size(140, 20);
            this.reflectionParameterTextBox.TabIndex = 7;
            // 
            // reflectionUnitLabel
            // 
            this.reflectionUnitLabel.AutoSize = true;
            this.reflectionUnitLabel.Location = new System.Drawing.Point(301, 112);
            this.reflectionUnitLabel.Name = "reflectionUnitLabel";
            this.reflectionUnitLabel.Size = new System.Drawing.Size(16, 13);
            this.reflectionUnitLabel.TabIndex = 8;
            this.reflectionUnitLabel.Text = "[-]";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(2, 194);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(40, 13);
            this.label5.TabIndex = 15;
            this.label5.Text = "Factor:";
            // 
            // factorTextBox
            // 
            this.factorTextBox.Location = new System.Drawing.Point(46, 191);
            this.factorTextBox.Name = "factorTextBox";
            this.factorTextBox.Size = new System.Drawing.Size(90, 20);
            this.factorTextBox.TabIndex = 16;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(157, 194);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(38, 13);
            this.label6.TabIndex = 17;
            this.label6.Text = "Offset:";
            // 
            // offsetTextBox
            // 
            this.offsetTextBox.Location = new System.Drawing.Point(210, 191);
            this.offsetTextBox.Name = "offsetTextBox";
            this.offsetTextBox.Size = new System.Drawing.Size(90, 20);
            this.offsetTextBox.TabIndex = 18;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(136, 194);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(16, 13);
            this.label7.TabIndex = 19;
            this.label7.Text = "[-]";
            // 
            // offsetUnitLabel
            // 
            this.offsetUnitLabel.AutoSize = true;
            this.offsetUnitLabel.Location = new System.Drawing.Point(301, 194);
            this.offsetUnitLabel.Name = "offsetUnitLabel";
            this.offsetUnitLabel.Size = new System.Drawing.Size(16, 13);
            this.offsetUnitLabel.TabIndex = 20;
            this.offsetUnitLabel.Text = "[-]";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(2, 138);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(140, 13);
            this.label8.TabIndex = 22;
            this.label8.Text = "Thatcher-Harleman time lag:";
            // 
            // thatcherTimeSpanEditor
            // 
            this.thatcherTimeSpanEditor.IncludeDays = false;
            this.thatcherTimeSpanEditor.IncludeTensOfSeconds = false;
            this.thatcherTimeSpanEditor.InsertKeyMode = System.Windows.Forms.InsertKeyMode.Overwrite;
            this.thatcherTimeSpanEditor.Location = new System.Drawing.Point(160, 135);
            this.thatcherTimeSpanEditor.Mask = "##\\:##\\:##";
            this.thatcherTimeSpanEditor.Name = "thatcherTimeSpanEditor";
            this.thatcherTimeSpanEditor.Size = new System.Drawing.Size(140, 20);
            this.thatcherTimeSpanEditor.TabIndex = 24;
            this.thatcherTimeSpanEditor.Text = "000000";
            this.thatcherTimeSpanEditor.TextMaskFormat = System.Windows.Forms.MaskFormat.IncludePromptAndLiterals;
            this.thatcherTimeSpanEditor.Value = System.TimeSpan.Parse("00:00:00");
            // 
            // FlowBoundaryConditionPropertiesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.thatcherTimeSpanEditor);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.offsetUnitLabel);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.offsetTextBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.factorTextBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.reflectionUnitLabel);
            this.Controls.Add(this.reflectionParameterTextBox);
            this.Controls.Add(this.verticalInterpolationComboBox);
            this.Controls.Add(this.label3);
            this.Name = "FlowBoundaryConditionPropertiesControl";
            this.Size = new System.Drawing.Size(325, 229);
            this.Controls.SetChildIndex(this.label3, 0);
            this.Controls.SetChildIndex(this.verticalInterpolationComboBox, 0);
            this.Controls.SetChildIndex(this.reflectionParameterTextBox, 0);
            this.Controls.SetChildIndex(this.reflectionUnitLabel, 0);
            this.Controls.SetChildIndex(this.label4, 0);
            this.Controls.SetChildIndex(this.label5, 0);
            this.Controls.SetChildIndex(this.factorTextBox, 0);
            this.Controls.SetChildIndex(this.label6, 0);
            this.Controls.SetChildIndex(this.offsetTextBox, 0);
            this.Controls.SetChildIndex(this.label7, 0);
            this.Controls.SetChildIndex(this.offsetUnitLabel, 0);
            this.Controls.SetChildIndex(this.label8, 0);
            this.Controls.SetChildIndex(this.thatcherTimeSpanEditor, 0);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox verticalInterpolationComboBox;
        private System.Windows.Forms.TextBox reflectionParameterTextBox;
        private System.Windows.Forms.Label reflectionUnitLabel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox factorTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox offsetTextBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label offsetUnitLabel;
        private System.Windows.Forms.Label label8;
        private DelftTools.Controls.Swf.TimeSpanEditor thatcherTimeSpanEditor;
    }
}
