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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.TimeZoneLabel = new System.Windows.Forms.Label();
            this.TimeZoneUnitLabel = new System.Windows.Forms.Label();
            this.TimeZoneTextBox = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // bcTypeLabel
            // 
            this.bcTypeLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.bcTypeLabel.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.bcTypeLabel, 2);
            this.bcTypeLabel.Location = new System.Drawing.Point(176, 8);
            this.bcTypeLabel.Name = "bcTypeLabel";
            this.bcTypeLabel.Size = new System.Drawing.Size(31, 13);
            this.bcTypeLabel.TabIndex = 12;
            this.bcTypeLabel.Text = "none";
            // 
            // forcingTypeLabel
            // 
            this.forcingTypeLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.forcingTypeLabel.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.forcingTypeLabel, 3);
            this.forcingTypeLabel.Location = new System.Drawing.Point(5, 34);
            this.forcingTypeLabel.Name = "forcingTypeLabel";
            this.forcingTypeLabel.Size = new System.Drawing.Size(72, 13);
            this.forcingTypeLabel.TabIndex = 11;
            this.forcingTypeLabel.Text = "Forcing Type:";
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.descriptionLabel.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.descriptionLabel, 3);
            this.descriptionLabel.Location = new System.Drawing.Point(5, 8);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(63, 13);
            this.descriptionLabel.TabIndex = 10;
            this.descriptionLabel.Text = "Description:";
            // 
            // dataTypeComboBox
            // 
            this.dataTypeComboBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tableLayoutPanel1.SetColumnSpan(this.dataTypeComboBox, 2);
            this.dataTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dataTypeComboBox.FormattingEnabled = true;
            this.dataTypeComboBox.Location = new System.Drawing.Point(176, 31);
            this.dataTypeComboBox.Name = "dataTypeComboBox";
            this.dataTypeComboBox.Size = new System.Drawing.Size(138, 21);
            this.dataTypeComboBox.TabIndex = 14;
            // 
            // verticalInterpolationTypeLabel
            // 
            this.verticalInterpolationTypeLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.verticalInterpolationTypeLabel.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.verticalInterpolationTypeLabel, 3);
            this.verticalInterpolationTypeLabel.Location = new System.Drawing.Point(5, 87);
            this.verticalInterpolationTypeLabel.Name = "verticalInterpolationTypeLabel";
            this.verticalInterpolationTypeLabel.Size = new System.Drawing.Size(128, 13);
            this.verticalInterpolationTypeLabel.TabIndex = 2;
            this.verticalInterpolationTypeLabel.Text = "Vertical interpolation type:";
            // 
            // reflectionParameterLabel
            // 
            this.reflectionParameterLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.reflectionParameterLabel.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.reflectionParameterLabel, 3);
            this.reflectionParameterLabel.Location = new System.Drawing.Point(5, 113);
            this.reflectionParameterLabel.Name = "reflectionParameterLabel";
            this.reflectionParameterLabel.Size = new System.Drawing.Size(108, 13);
            this.reflectionParameterLabel.TabIndex = 3;
            this.reflectionParameterLabel.Text = "Reflection parameter:";
            // 
            // verticalInterpolationComboBox
            // 
            this.verticalInterpolationComboBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tableLayoutPanel1.SetColumnSpan(this.verticalInterpolationComboBox, 2);
            this.verticalInterpolationComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.verticalInterpolationComboBox.FormattingEnabled = true;
            this.verticalInterpolationComboBox.Location = new System.Drawing.Point(176, 84);
            this.verticalInterpolationComboBox.Name = "verticalInterpolationComboBox";
            this.verticalInterpolationComboBox.Size = new System.Drawing.Size(138, 21);
            this.verticalInterpolationComboBox.TabIndex = 6;
            // 
            // reflectionParameterTextBox
            // 
            this.reflectionParameterTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tableLayoutPanel1.SetColumnSpan(this.reflectionParameterTextBox, 2);
            this.reflectionParameterTextBox.Location = new System.Drawing.Point(176, 110);
            this.reflectionParameterTextBox.Name = "reflectionParameterTextBox";
            this.reflectionParameterTextBox.Size = new System.Drawing.Size(138, 20);
            this.reflectionParameterTextBox.TabIndex = 7;
            // 
            // reflectionUnitLabel
            // 
            this.reflectionUnitLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.reflectionUnitLabel.AutoSize = true;
            this.reflectionUnitLabel.Location = new System.Drawing.Point(322, 113);
            this.reflectionUnitLabel.Name = "reflectionUnitLabel";
            this.reflectionUnitLabel.Size = new System.Drawing.Size(16, 13);
            this.reflectionUnitLabel.TabIndex = 8;
            this.reflectionUnitLabel.Text = "[-]";
            // 
            // factorLabel
            // 
            this.factorLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.factorLabel.AutoSize = true;
            this.factorLabel.Location = new System.Drawing.Point(5, 188);
            this.factorLabel.Name = "factorLabel";
            this.factorLabel.Size = new System.Drawing.Size(40, 13);
            this.factorLabel.TabIndex = 15;
            this.factorLabel.Text = "Factor:";
            // 
            // factorTextBox
            // 
            this.factorTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.factorTextBox.Location = new System.Drawing.Point(78, 185);
            this.factorTextBox.Name = "factorTextBox";
            this.factorTextBox.Size = new System.Drawing.Size(66, 20);
            this.factorTextBox.TabIndex = 16;
            // 
            // offsetLabel
            // 
            this.offsetLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.offsetLabel.AutoSize = true;
            this.offsetLabel.Location = new System.Drawing.Point(176, 188);
            this.offsetLabel.Name = "offsetLabel";
            this.offsetLabel.Size = new System.Drawing.Size(38, 13);
            this.offsetLabel.TabIndex = 17;
            this.offsetLabel.Text = "Offset:";
            // 
            // offsetTextBox
            // 
            this.offsetTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.offsetTextBox.Location = new System.Drawing.Point(249, 185);
            this.offsetTextBox.Name = "offsetTextBox";
            this.offsetTextBox.Size = new System.Drawing.Size(66, 20);
            this.offsetTextBox.TabIndex = 18;
            // 
            // factorUnitLabel
            // 
            this.factorUnitLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.factorUnitLabel.AutoSize = true;
            this.factorUnitLabel.Location = new System.Drawing.Point(151, 188);
            this.factorUnitLabel.Name = "factorUnitLabel";
            this.factorUnitLabel.Size = new System.Drawing.Size(16, 13);
            this.factorUnitLabel.TabIndex = 19;
            this.factorUnitLabel.Text = "[-]";
            // 
            // offsetUnitLabel
            // 
            this.offsetUnitLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.offsetUnitLabel.AutoSize = true;
            this.offsetUnitLabel.Location = new System.Drawing.Point(322, 188);
            this.offsetUnitLabel.Name = "offsetUnitLabel";
            this.offsetUnitLabel.Size = new System.Drawing.Size(16, 13);
            this.offsetUnitLabel.TabIndex = 20;
            this.offsetUnitLabel.Text = "[-]";
            // 
            // thatcherTimeSpanLabel
            // 
            this.thatcherTimeSpanLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.thatcherTimeSpanLabel.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.thatcherTimeSpanLabel, 3);
            this.thatcherTimeSpanLabel.Location = new System.Drawing.Point(5, 139);
            this.thatcherTimeSpanLabel.Name = "thatcherTimeSpanLabel";
            this.thatcherTimeSpanLabel.Size = new System.Drawing.Size(140, 13);
            this.thatcherTimeSpanLabel.TabIndex = 22;
            this.thatcherTimeSpanLabel.Text = "Thatcher-Harleman time lag:";
            // 
            // thatcherTimeSpanEditor
            // 
            this.thatcherTimeSpanEditor.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.thatcherTimeSpanEditor.IncludeDays = false;
            this.thatcherTimeSpanEditor.IncludeTensOfSeconds = false;
            this.thatcherTimeSpanEditor.InsertKeyMode = System.Windows.Forms.InsertKeyMode.Overwrite;
            this.thatcherTimeSpanEditor.Location = new System.Drawing.Point(176, 136);
            this.thatcherTimeSpanEditor.Mask = "##\\:##\\:##";
            this.thatcherTimeSpanEditor.Name = "thatcherTimeSpanEditor";
            this.thatcherTimeSpanEditor.Size = new System.Drawing.Size(66, 20);
            this.thatcherTimeSpanEditor.TabIndex = 24;
            this.thatcherTimeSpanEditor.Text = "000000";
            this.thatcherTimeSpanEditor.TextMaskFormat = System.Windows.Forms.MaskFormat.IncludePromptAndLiterals;
            this.thatcherTimeSpanEditor.Value = System.TimeSpan.Parse("00:00:00");
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 6;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel1.Controls.Add(this.descriptionLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.bcTypeLabel, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.dataTypeComboBox, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.verticalInterpolationComboBox, 3, 3);
            this.tableLayoutPanel1.Controls.Add(this.reflectionUnitLabel, 5, 4);
            this.tableLayoutPanel1.Controls.Add(this.thatcherTimeSpanEditor, 3, 5);
            this.tableLayoutPanel1.Controls.Add(this.reflectionParameterTextBox, 3, 4);
            this.tableLayoutPanel1.Controls.Add(this.forcingTypeLabel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.offsetUnitLabel, 5, 7);
            this.tableLayoutPanel1.Controls.Add(this.thatcherTimeSpanLabel, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.offsetTextBox, 4, 7);
            this.tableLayoutPanel1.Controls.Add(this.factorUnitLabel, 2, 7);
            this.tableLayoutPanel1.Controls.Add(this.offsetLabel, 3, 7);
            this.tableLayoutPanel1.Controls.Add(this.verticalInterpolationTypeLabel, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.reflectionParameterLabel, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.factorLabel, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.factorTextBox, 1, 7);
            this.tableLayoutPanel1.Controls.Add(this.TimeZoneLabel, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.TimeZoneUnitLabel, 5, 2);
            this.tableLayoutPanel1.Controls.Add(this.TimeZoneTextBox, 3, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.MaximumSize = new System.Drawing.Size(350, 250);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel1.RowCount = 8;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28531F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28531F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28816F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28531F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28531F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28531F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28531F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(350, 213);
            this.tableLayoutPanel1.TabIndex = 25;
            // 
            // TimeZoneLabel
            // 
            this.TimeZoneLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.TimeZoneLabel.AutoSize = true;
            this.TimeZoneLabel.Location = new System.Drawing.Point(5, 61);
            this.TimeZoneLabel.Name = "TimeZoneLabel";
            this.TimeZoneLabel.Size = new System.Drawing.Size(59, 13);
            this.TimeZoneLabel.TabIndex = 25;
            this.TimeZoneLabel.Text = "Time zone:";
            // 
            // TimeZoneUnitLabel
            // 
            this.TimeZoneUnitLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.TimeZoneUnitLabel.AutoSize = true;
            this.TimeZoneUnitLabel.Location = new System.Drawing.Point(322, 61);
            this.TimeZoneUnitLabel.Name = "TimeZoneUnitLabel";
            this.TimeZoneUnitLabel.Size = new System.Drawing.Size(16, 13);
            this.TimeZoneUnitLabel.TabIndex = 26;
            this.TimeZoneUnitLabel.Text = "[-]";
            // 
            // TimeZoneTextBox
            // 
            this.TimeZoneTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.TimeZoneTextBox.Location = new System.Drawing.Point(176, 57);
            this.TimeZoneTextBox.Name = "TimeZoneTextBox";
            this.TimeZoneTextBox.ReadOnly = true;
            this.TimeZoneTextBox.Size = new System.Drawing.Size(67, 20);
            this.TimeZoneTextBox.TabIndex = 27;
            this.TimeZoneTextBox.Tag = "TimeZone";
            this.TimeZoneTextBox.Text = "0,0";
            // 
            // FlowBoundaryConditionPropertiesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "FlowBoundaryConditionPropertiesControl";
            this.Size = new System.Drawing.Size(353, 216);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label TimeZoneLabel;
        private System.Windows.Forms.TextBox TimeZoneTextBox;

        private System.Windows.Forms.Label TimeZoneUnitLabel;

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;

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
        private System.Windows.Forms.Label factorLabel;
        private System.Windows.Forms.TextBox factorTextBox;
        private System.Windows.Forms.Label offsetLabel;
        private System.Windows.Forms.TextBox offsetTextBox;
        private System.Windows.Forms.Label factorUnitLabel;
        private System.Windows.Forms.Label offsetUnitLabel;
        private System.Windows.Forms.Label thatcherTimeSpanLabel;
        private DelftTools.Controls.Swf.TimeSpanEditor thatcherTimeSpanEditor;
    }
}
