using System.ComponentModel;
using System.Windows.Forms;
using DeltaShell.Plugins.FMSuite.Common.Gui.Editors;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.boundaryConditionPropertiesPanel = new System.Windows.Forms.Panel();
            this.boundaryConditionSelectionPanel = new System.Windows.Forms.Panel();
            this.quantitiesComboBox = new System.Windows.Forms.ComboBox();
            this.addDefinitionButton = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.boundaryGeometryPreview = new DeltaShell.Plugins.SharpMapGis.Gui.Forms.BoundaryGeometryPreview();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.supportPointListBox = new DeltaShell.Plugins.FMSuite.Common.Gui.Editors.SupportPointListBox();
            this.verticalProfileControl = new DeltaShell.Plugins.FMSuite.Common.Gui.Editors.VerticalProfileControl();
            this.conditionsListBox = new DeltaShell.Plugins.FMSuite.Common.Gui.Editors.RemoveableItemsListBox();
            this.groupBox1.SuspendLayout();
            this.boundaryConditionSelectionPanel.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // categoryComboBox
            // 
            this.categoryComboBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.categoryComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.categoryComboBox.FormattingEnabled = true;
            this.categoryComboBox.Location = new System.Drawing.Point(5, 5);
            this.categoryComboBox.Margin = new System.Windows.Forms.Padding(0);
            this.categoryComboBox.Name = "categoryComboBox";
            this.categoryComboBox.Size = new System.Drawing.Size(170, 21);
            this.categoryComboBox.TabIndex = 3;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.boundaryConditionPropertiesPanel);
            this.groupBox1.Controls.Add(this.boundaryConditionSelectionPanel);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(569, 283);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Boundary Condition";
            // 
            // boundaryConditionPropertiesPanel
            // 
            this.boundaryConditionPropertiesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.boundaryConditionPropertiesPanel.Location = new System.Drawing.Point(183, 16);
            this.boundaryConditionPropertiesPanel.Name = "boundaryConditionPropertiesPanel";
            this.boundaryConditionPropertiesPanel.Padding = new System.Windows.Forms.Padding(5);
            this.boundaryConditionPropertiesPanel.Size = new System.Drawing.Size(383, 264);
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
            this.boundaryConditionSelectionPanel.Padding = new System.Windows.Forms.Padding(5);
            this.boundaryConditionSelectionPanel.Size = new System.Drawing.Size(180, 264);
            this.boundaryConditionSelectionPanel.TabIndex = 0;
            // 
            // quantitiesComboBox
            // 
            this.quantitiesComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.quantitiesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.quantitiesComboBox.DropDownWidth = 200;
            this.quantitiesComboBox.FormattingEnabled = true;
            this.quantitiesComboBox.Location = new System.Drawing.Point(5, 237);
            this.quantitiesComboBox.Name = "quantitiesComboBox";
            this.quantitiesComboBox.Size = new System.Drawing.Size(134, 21);
            this.quantitiesComboBox.TabIndex = 5;
            // 
            // addDefinitionButton
            // 
            this.addDefinitionButton.AllowDrop = true;
            this.addDefinitionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.addDefinitionButton.Image = global::DeltaShell.Plugins.FMSuite.Common.Gui.Properties.Resources.Plus;
            this.addDefinitionButton.Location = new System.Drawing.Point(145, 236);
            this.addDefinitionButton.Name = "addDefinitionButton";
            this.addDefinitionButton.Size = new System.Drawing.Size(30, 23);
            this.addDefinitionButton.TabIndex = 4;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.tableLayoutPanel1);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(0, 0);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(5);
            this.groupBox3.Size = new System.Drawing.Size(458, 283);
            this.groupBox3.TabIndex = 16;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Location";
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
            this.tableLayoutPanel1.Size = new System.Drawing.Size(448, 260);
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
            this.label3.Location = new System.Drawing.Point(152, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Geometry";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(301, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Layer";
            // 
            // boundaryGeometryPreview
            // 
            this.boundaryGeometryPreview.DataPoints = null;
            this.boundaryGeometryPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.boundaryGeometryPreview.Location = new System.Drawing.Point(152, 23);
            this.boundaryGeometryPreview.Name = "boundaryGeometryPreview";
            this.boundaryGeometryPreview.Size = new System.Drawing.Size(143, 234);
            this.boundaryGeometryPreview.TabIndex = 5;
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
            this.splitContainer1.Panel1.Controls.Add(this.panel2);
            this.splitContainer1.Panel1.Controls.Add(this.panel1);
            this.splitContainer1.Size = new System.Drawing.Size(1027, 625);
            this.splitContainer1.SplitterDistance = 283;
            this.splitContainer1.TabIndex = 17;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.groupBox3);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(569, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(458, 283);
            this.panel2.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(569, 283);
            this.panel1.TabIndex = 0;
            // 
            // supportPointListBox
            // 
            this.supportPointListBox.ContextMenuItems = null;
            this.supportPointListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.supportPointListBox.FormattingEnabled = true;
            this.supportPointListBox.Location = new System.Drawing.Point(3, 23);
            this.supportPointListBox.Name = "supportPointListBox";
            this.supportPointListBox.Size = new System.Drawing.Size(143, 234);
            this.supportPointListBox.TabIndex = 0;
            // 
            // verticalProfileControl
            // 
            this.verticalProfileControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.verticalProfileControl.Location = new System.Drawing.Point(301, 23);
            this.verticalProfileControl.ModelDepthLayerDefinition = null;
            this.verticalProfileControl.Name = "verticalProfileControl";
            this.verticalProfileControl.Size = new System.Drawing.Size(144, 234);
            this.verticalProfileControl.TabIndex = 6;
            // 
            // conditionsListBox
            // 
            this.conditionsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.conditionsListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.conditionsListBox.FormattingEnabled = true;
            this.conditionsListBox.IntegralHeight = false;
            this.conditionsListBox.Location = new System.Drawing.Point(5, 34);
            this.conditionsListBox.Name = "conditionsListBox";
            this.conditionsListBox.Size = new System.Drawing.Size(170, 147);
            this.conditionsListBox.TabIndex = 2;
            // 
            // BoundaryConditionEditor
            // 
            this.AutoScroll = true;
            this.AutoScrollMinSize = new System.Drawing.Size(1020, 570);
            this.Controls.Add(this.splitContainer1);
            this.Name = "BoundaryConditionEditor";
            this.Size = new System.Drawing.Size(1027, 625);
            this.groupBox1.ResumeLayout(false);
            this.boundaryConditionSelectionPanel.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ComboBox categoryComboBox;
        private Button addDefinitionButton;
        private RemoveableItemsListBox conditionsListBox;
        private GroupBox groupBox1;
        private GroupBox groupBox3;
        private TableLayoutPanel tableLayoutPanel1;
        private SplitContainer splitContainer1;
        private Label label3;
        private Label label2;
        private Panel boundaryConditionPropertiesPanel;
        private Panel boundaryConditionSelectionPanel;
        private ComboBox quantitiesComboBox;
        private Panel panel2;
        private Panel panel1;
        private Label label1;
        private SupportPointListBox supportPointListBox;
        private BoundaryGeometryPreview boundaryGeometryPreview;
        private VerticalProfileControl verticalProfileControl;
    }
}
