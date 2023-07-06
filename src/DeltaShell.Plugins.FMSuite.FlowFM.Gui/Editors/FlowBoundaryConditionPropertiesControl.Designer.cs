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
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.bcTypeLabel = new System.Windows.Forms.Label();
            this.forcingTypeLabel = new System.Windows.Forms.Label();
            this.descriptionLabel = new System.Windows.Forms.Label();
            this.dataTypeComboBox = new System.Windows.Forms.ComboBox();
            this.verticalInterpolationTypeLabel = new System.Windows.Forms.Label();
            this.reflectionParameterLabel = new System.Windows.Forms.Label();
            this.verticalInterpolationComboBox = new System.Windows.Forms.ComboBox();
            this.reflectionParameterTextBox = new System.Windows.Forms.TextBox();
            this.reflectionUnitLabel = new System.Windows.Forms.Label();
            this.factorLabel = new System.Windows.Forms.Label();
            this.factorTextBox = new System.Windows.Forms.TextBox();
            this.offsetLabel = new System.Windows.Forms.Label();
            this.offsetTextBox = new System.Windows.Forms.TextBox();
            this.factorUnitLabel = new System.Windows.Forms.Label();
            this.offsetUnitLabel = new System.Windows.Forms.Label();
            this.thatcherTimeSpanLabel = new System.Windows.Forms.Label();
            this.thatcherTimeSpanEditor = new DelftTools.Controls.Swf.TimeSpanEditor();
            this.SuspendLayout();
            //
            // bcTypeLabel
            //
            this.bcTypeLabel.AutoSize = true;
            this.bcTypeLabel.Location = new System.Drawing.Point(157, 3);
            this.bcTypeLabel.Name = "bcTypeLabel";
            this.bcTypeLabel.Size = new System.Drawing.Size(31, 13);
            this.bcTypeLabel.TabIndex = 12;
            this.bcTypeLabel.Text = "none";
            //
            // forcingTypeLabel
            //
            this.forcingTypeLabel.AutoSize = true;
            this.forcingTypeLabel.Location = new System.Drawing.Point(2, 28);
            this.forcingTypeLabel.Name = "forcingTypeLabel";
            this.forcingTypeLabel.Size = new System.Drawing.Size(68, 13);
            this.forcingTypeLabel.TabIndex = 11;
            this.forcingTypeLabel.Text = "Forcing Type:";
            //
            // descriptionLabel
            //
            this.descriptionLabel.AutoSize = true;
            this.descriptionLabel.Location = new System.Drawing.Point(2, 3);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(63, 13);
            this.descriptionLabel.TabIndex = 10;
            this.descriptionLabel.Text = "Description:";
            //
            // dataTypeComboBox
            //
            this.dataTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dataTypeComboBox.FormattingEnabled = true;
            this.dataTypeComboBox.Location = new System.Drawing.Point(160, 25);
            this.dataTypeComboBox.Name = "dataTypeComboBox";
            this.dataTypeComboBox.Size = new System.Drawing.Size(150, 21);
            this.dataTypeComboBox.TabIndex = 14;
            //
            // verticalInterpolationTypeLabel
            //
            this.verticalInterpolationTypeLabel.AutoSize = true;
            this.verticalInterpolationTypeLabel.Location = new System.Drawing.Point(2, 57);
            this.verticalInterpolationTypeLabel.Name = "verticalInterpolationTypeLabel";
            this.verticalInterpolationTypeLabel.Size = new System.Drawing.Size(128, 13);
            this.verticalInterpolationTypeLabel.TabIndex = 2;
            this.verticalInterpolationTypeLabel.Text = "Vertical interpolation type:";
            //
            // reflectionParameterLabel
            //
            this.reflectionParameterLabel.AutoSize = true;
            this.reflectionParameterLabel.Location = new System.Drawing.Point(2, 88);
            this.reflectionParameterLabel.Name = "reflectionParameterLabel";
            this.reflectionParameterLabel.Size = new System.Drawing.Size(108, 13);
            this.reflectionParameterLabel.TabIndex = 3;
            this.reflectionParameterLabel.Text = "Reflection parameter:";
            //
            // verticalInterpolationComboBox
            //
            this.verticalInterpolationComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.verticalInterpolationComboBox.FormattingEnabled = true;
            this.verticalInterpolationComboBox.Location = new System.Drawing.Point(160, 54);
            this.verticalInterpolationComboBox.Name = "verticalInterpolationComboBox";
            this.verticalInterpolationComboBox.Size = new System.Drawing.Size(140, 21);
            this.verticalInterpolationComboBox.TabIndex = 6;
            //
            // reflectionParameterTextBox
            //
            this.reflectionParameterTextBox.Location = new System.Drawing.Point(160, 85);
            this.reflectionParameterTextBox.Name = "reflectionParameterTextBox";
            this.reflectionParameterTextBox.Size = new System.Drawing.Size(140, 20);
            this.reflectionParameterTextBox.TabIndex = 7;
            //
            // reflectionUnitLabel
            //
            this.reflectionUnitLabel.AutoSize = true;
            this.reflectionUnitLabel.Location = new System.Drawing.Point(301, 88);
            this.reflectionUnitLabel.Name = "reflectionUnitLabel";
            this.reflectionUnitLabel.Size = new System.Drawing.Size(16, 13);
            this.reflectionUnitLabel.TabIndex = 8;
            this.reflectionUnitLabel.Text = "[-]";
            //
            // factorLabel
            //
            this.factorLabel.AutoSize = true;
            this.factorLabel.Location = new System.Drawing.Point(2, 170);
            this.factorLabel.Name = "factorLabel";
            this.factorLabel.Size = new System.Drawing.Size(40, 13);
            this.factorLabel.TabIndex = 15;
            this.factorLabel.Text = "Factor:";
            //
            // factorTextBox
            //
            this.factorTextBox.Location = new System.Drawing.Point(46, 167);
            this.factorTextBox.Name = "factorTextBox";
            this.factorTextBox.Size = new System.Drawing.Size(90, 20);
            this.factorTextBox.TabIndex = 16;
            //
            // offsetLabel
            //
            this.offsetLabel.AutoSize = true;
            this.offsetLabel.Location = new System.Drawing.Point(157, 170);
            this.offsetLabel.Name = "offsetLabel";
            this.offsetLabel.Size = new System.Drawing.Size(38, 13);
            this.offsetLabel.TabIndex = 17;
            this.offsetLabel.Text = "Offset:";
            //
            // offsetTextBox
            //
            this.offsetTextBox.Location = new System.Drawing.Point(210, 167);
            this.offsetTextBox.Name = "offsetTextBox";
            this.offsetTextBox.Size = new System.Drawing.Size(90, 20);
            this.offsetTextBox.TabIndex = 18;
            //
            // factorUnitLabel
            //
            this.factorUnitLabel.AutoSize = true;
            this.factorUnitLabel.Location = new System.Drawing.Point(136, 170);
            this.factorUnitLabel.Name = "factorUnitLabel";
            this.factorUnitLabel.Size = new System.Drawing.Size(16, 13);
            this.factorUnitLabel.TabIndex = 19;
            this.factorUnitLabel.Text = "[-]";
            //
            // offsetUnitLabel
            //
            this.offsetUnitLabel.AutoSize = true;
            this.offsetUnitLabel.Location = new System.Drawing.Point(301, 170);
            this.offsetUnitLabel.Name = "offsetUnitLabel";
            this.offsetUnitLabel.Size = new System.Drawing.Size(16, 13);
            this.offsetUnitLabel.TabIndex = 20;
            this.offsetUnitLabel.Text = "[-]";
            //
            // thatcherTimeSpanLabel
            //
            this.thatcherTimeSpanLabel.AutoSize = true;
            this.thatcherTimeSpanLabel.Location = new System.Drawing.Point(2, 114);
            this.thatcherTimeSpanLabel.Name = "thatcherTimeSpanLabel";
            this.thatcherTimeSpanLabel.Size = new System.Drawing.Size(140, 13);
            this.thatcherTimeSpanLabel.TabIndex = 22;
            this.thatcherTimeSpanLabel.Text = "Thatcher-Harleman time lag:";
            //
            // thatcherTimeSpanEditor
            //
            this.thatcherTimeSpanEditor.IncludeDays = false;
            this.thatcherTimeSpanEditor.IncludeTensOfSeconds = false;
            this.thatcherTimeSpanEditor.InsertKeyMode = System.Windows.Forms.InsertKeyMode.Overwrite;
            this.thatcherTimeSpanEditor.Location = new System.Drawing.Point(160, 111);
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
            this.Controls.Add(this.dataTypeComboBox);
            this.Controls.Add(this.bcTypeLabel);
            this.Controls.Add(this.forcingTypeLabel);
            this.Controls.Add(this.descriptionLabel);
            this.Controls.Add(this.thatcherTimeSpanEditor);
            this.Controls.Add(this.thatcherTimeSpanLabel);
            this.Controls.Add(this.offsetUnitLabel);
            this.Controls.Add(this.factorUnitLabel);
            this.Controls.Add(this.offsetTextBox);
            this.Controls.Add(this.offsetLabel);
            this.Controls.Add(this.factorTextBox);
            this.Controls.Add(this.factorLabel);
            this.Controls.Add(this.reflectionParameterLabel);
            this.Controls.Add(this.reflectionUnitLabel);
            this.Controls.Add(this.reflectionParameterTextBox);
            this.Controls.Add(this.verticalInterpolationComboBox);
            this.Controls.Add(this.verticalInterpolationTypeLabel);
            this.Name = "FlowBoundaryConditionPropertiesControl";
            this.Size = new System.Drawing.Size(325, 204);
            this.Controls.SetChildIndex(this.verticalInterpolationTypeLabel, 0);
            this.Controls.SetChildIndex(this.verticalInterpolationComboBox, 0);
            this.Controls.SetChildIndex(this.reflectionParameterTextBox, 0);
            this.Controls.SetChildIndex(this.reflectionUnitLabel, 0);
            this.Controls.SetChildIndex(this.reflectionParameterLabel, 0);
            this.Controls.SetChildIndex(this.factorLabel, 0);
            this.Controls.SetChildIndex(this.factorTextBox, 0);
            this.Controls.SetChildIndex(this.offsetLabel, 0);
            this.Controls.SetChildIndex(this.offsetTextBox, 0);
            this.Controls.SetChildIndex(this.factorUnitLabel, 0);
            this.Controls.SetChildIndex(this.offsetUnitLabel, 0);
            this.Controls.SetChildIndex(this.thatcherTimeSpanLabel, 0);
            this.Controls.SetChildIndex(this.thatcherTimeSpanEditor, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label bcTypeLabel;
        private System.Windows.Forms.Label forcingTypeLabel;
        private System.Windows.Forms.Label descriptionLabel;
        private System.Windows.Forms.ComboBox dataTypeComboBox;
        private System.Windows.Forms.Label verticalInterpolationTypeLabel;
        private System.Windows.Forms.Label reflectionParameterLabel;
        private System.Windows.Forms.ComboBox verticalInterpolationComboBox;
        private System.Windows.Forms.TextBox reflectionParameterTextBox;
        private System.Windows.Forms.Label reflectionUnitLabel;
        private Label factorLabel;
        private TextBox factorTextBox;
        private Label offsetLabel;
        private TextBox offsetTextBox;
        private Label factorUnitLabel;
        private Label offsetUnitLabel;
        private Label thatcherTimeSpanLabel;
        private DelftTools.Controls.Swf.TimeSpanEditor thatcherTimeSpanEditor;
    }
}
