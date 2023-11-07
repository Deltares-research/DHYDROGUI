using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls.Swf.Charting.Series;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Charting;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.FMSuite.Common.Gui.Properties;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Editors
{
    public partial class BoundaryConditionEditor
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (this.Controller != null)
            {
                this.Controller.Dispose();
            }
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
            this.categoryComboBox = new System.Windows.Forms.ComboBox();
            this.boundaryConditionGroupBox = new System.Windows.Forms.GroupBox();
            this.boundaryConditionPropertiesPanel = new System.Windows.Forms.Panel();
            this.boundaryConditionSelectionPanel = new System.Windows.Forms.Panel();
            this.quantitiesComboBox = new System.Windows.Forms.ComboBox();
            this.addDefinitionButton = new System.Windows.Forms.Button();
            this.conditionsListBox = new DeltaShell.Plugins.FMSuite.Common.Gui.Editors.RemoveableItemsListBox();
            this.locationGroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.supportPointListBox = new DeltaShell.Plugins.FMSuite.Common.Gui.Editors.SupportPointListBox();
            this.boundaryGeometryPreview = new DeltaShell.Plugins.SharpMapGis.Gui.Forms.BoundaryGeometryPreview();
            this.verticalProfileControl = new DeltaShell.Plugins.FMSuite.Common.Gui.Editors.VerticalProfileControl();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.boundaryConditionGroupBox.SuspendLayout();
            this.boundaryConditionSelectionPanel.SuspendLayout();
            this.locationGroupBox.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // categoryComboBox
            // 
            this.categoryComboBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.categoryComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.categoryComboBox.FormattingEnabled = true;
            this.categoryComboBox.Location = new System.Drawing.Point(0, 0);
            this.categoryComboBox.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.categoryComboBox.Name = "categoryComboBox";
            this.categoryComboBox.Size = new System.Drawing.Size(180, 21);
            this.categoryComboBox.TabIndex = 3;
            // 
            // boundaryConditionGroupBox
            // 
            this.boundaryConditionGroupBox.Controls.Add(this.boundaryConditionPropertiesPanel);
            this.boundaryConditionGroupBox.Controls.Add(this.boundaryConditionSelectionPanel);
            this.boundaryConditionGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.boundaryConditionGroupBox.Location = new System.Drawing.Point(0, 0);
            this.boundaryConditionGroupBox.Name = "boundaryConditionGroupBox";
            this.boundaryConditionGroupBox.Size = new System.Drawing.Size(511, 280);
            this.boundaryConditionGroupBox.TabIndex = 14;
            this.boundaryConditionGroupBox.TabStop = false;
            this.boundaryConditionGroupBox.Text = "Boundary Condition";
            // 
            // boundaryConditionPropertiesPanel
            // 
            this.boundaryConditionPropertiesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.boundaryConditionPropertiesPanel.Location = new System.Drawing.Point(183, 16);
            this.boundaryConditionPropertiesPanel.Name = "boundaryConditionPropertiesPanel";
            this.boundaryConditionPropertiesPanel.Size = new System.Drawing.Size(325, 261);
            this.boundaryConditionPropertiesPanel.TabIndex = 5;
            // 
            // boundaryConditionSelectionPanel
            // 
            this.boundaryConditionSelectionPanel.Controls.Add(this.quantitiesComboBox);
            this.boundaryConditionSelectionPanel.Controls.Add(this.categoryComboBox);
            this.boundaryConditionSelectionPanel.Controls.Add(this.addDefinitionButton);
            this.boundaryConditionSelectionPanel.Controls.Add(this.conditionsListBox);
            this.boundaryConditionSelectionPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.boundaryConditionSelectionPanel.Location = new System.Drawing.Point(3, 16);
            this.boundaryConditionSelectionPanel.Name = "boundaryConditionSelectionPanel";
            this.boundaryConditionSelectionPanel.Size = new System.Drawing.Size(180, 261);
            this.boundaryConditionSelectionPanel.TabIndex = 0;
            // 
            // quantitiesComboBox
            // 
            this.quantitiesComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.quantitiesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.quantitiesComboBox.DropDownWidth = 200;
            this.quantitiesComboBox.FormattingEnabled = true;
            this.quantitiesComboBox.Location = new System.Drawing.Point(0, 239);
            this.quantitiesComboBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.quantitiesComboBox.Name = "quantitiesComboBox";
            this.quantitiesComboBox.Size = new System.Drawing.Size(134, 21);
            this.quantitiesComboBox.TabIndex = 5;
            // 
            // addDefinitionButton
            // 
            this.addDefinitionButton.AllowDrop = true;
            this.addDefinitionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.addDefinitionButton.Image = global::DeltaShell.Plugins.FMSuite.Common.Gui.Properties.Resources.Plus;
            this.addDefinitionButton.Location = new System.Drawing.Point(150, 238);
            this.addDefinitionButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.addDefinitionButton.Name = "addDefinitionButton";
            this.addDefinitionButton.Size = new System.Drawing.Size(30, 23);
            this.addDefinitionButton.TabIndex = 4;
            // 
            // conditionsListBox
            // 
            this.conditionsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.conditionsListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.conditionsListBox.FormattingEnabled = true;
            this.conditionsListBox.IntegralHeight = false;
            this.conditionsListBox.ItemHeight = 16;
            this.conditionsListBox.Location = new System.Drawing.Point(0, 29);
            this.conditionsListBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.conditionsListBox.Name = "conditionsListBox";
            this.conditionsListBox.Size = new System.Drawing.Size(180, 203);
            this.conditionsListBox.TabIndex = 2;
            // 
            // locationGroupBox
            // 
            this.locationGroupBox.Controls.Add(this.tableLayoutPanel1);
            this.locationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.locationGroupBox.Location = new System.Drawing.Point(511, 0);
            this.locationGroupBox.Name = "locationGroupBox";
            this.locationGroupBox.Padding = new System.Windows.Forms.Padding(5);
            this.locationGroupBox.Size = new System.Drawing.Size(516, 280);
            this.locationGroupBox.TabIndex = 16;
            this.locationGroupBox.TabStop = false;
            this.locationGroupBox.Text = "Location";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label3, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.supportPointListBox, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.boundaryGeometryPreview, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.verticalProfileControl, 2, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(5, 18);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(506, 257);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Support point";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(171, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Geometry";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(339, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Layer";
            // 
            // supportPointListBox
            // 
            this.supportPointListBox.ContextMenuItems = null;
            this.supportPointListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.supportPointListBox.FormattingEnabled = true;
            this.supportPointListBox.Location = new System.Drawing.Point(3, 23);
            this.supportPointListBox.Name = "supportPointListBox";
            this.supportPointListBox.Size = new System.Drawing.Size(162, 231);
            this.supportPointListBox.TabIndex = 0;
            // 
            // boundaryGeometryPreview
            // 
            this.boundaryGeometryPreview.DataPoints = null;
            this.boundaryGeometryPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.boundaryGeometryPreview.Location = new System.Drawing.Point(171, 23);
            this.boundaryGeometryPreview.Name = "boundaryGeometryPreview";
            this.boundaryGeometryPreview.Size = new System.Drawing.Size(162, 231);
            this.boundaryGeometryPreview.TabIndex = 5;
            // 
            // verticalProfileControl
            // 
            this.verticalProfileControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.verticalProfileControl.Location = new System.Drawing.Point(339, 23);
            this.verticalProfileControl.ModelDepthLayerDefinition = null;
            this.verticalProfileControl.Name = "verticalProfileControl";
            this.verticalProfileControl.Size = new System.Drawing.Size(164, 231);
            this.verticalProfileControl.TabIndex = 6;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.locationGroupBox);
            this.splitContainer1.Panel1.Controls.Add(this.boundaryConditionGroupBox);
            this.splitContainer1.Panel1MinSize = 280;
            this.splitContainer1.Size = new System.Drawing.Size(1027, 625);
            this.splitContainer1.SplitterDistance = 280;
            this.splitContainer1.TabIndex = 17;
            // 
            // BoundaryConditionEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoScrollMinSize = new System.Drawing.Size(1020, 570);
            this.Controls.Add(this.splitContainer1);
            this.Name = "BoundaryConditionEditor";
            this.Size = new System.Drawing.Size(1027, 625);
            this.boundaryConditionGroupBox.ResumeLayout(false);
            this.boundaryConditionSelectionPanel.ResumeLayout(false);
            this.locationGroupBox.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ComboBox categoryComboBox;
        private Button addDefinitionButton;
        private RemoveableItemsListBox conditionsListBox;
        private GroupBox boundaryConditionGroupBox;
        private GroupBox locationGroupBox;
        private TableLayoutPanel tableLayoutPanel1;
        private SplitContainer splitContainer1;
        private Label label3;
        private Label label2;
        private Panel boundaryConditionPropertiesPanel;
        private Panel boundaryConditionSelectionPanel;
        private ComboBox quantitiesComboBox;
        private Label label1;
        private SupportPointListBox supportPointListBox;
        private BoundaryGeometryPreview boundaryGeometryPreview;
        private VerticalProfileControl verticalProfileControl;
    }
}
