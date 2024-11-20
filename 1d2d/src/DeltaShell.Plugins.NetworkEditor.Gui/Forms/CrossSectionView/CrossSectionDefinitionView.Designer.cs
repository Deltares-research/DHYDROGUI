using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.StandardCrossSections;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView
{
    partial class CrossSectionDefinitionView
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
            this.viewSplitContainer = new System.Windows.Forms.SplitContainer();
            this.leftSplitContainer = new System.Windows.Forms.SplitContainer();
            this.splitContainerCrossSectionData = new System.Windows.Forms.SplitContainer();
            this.tableGroupBox = new System.Windows.Forms.GroupBox();
            this.tableView = new DelftTools.Controls.Swf.Table.TableView();
            this.crossSectionStandardDataView1 = new CrossSectionStandardDataView();
            this.summerDikeView = new SummerDikeView();
            this.rightSplitContainer = new System.Windows.Forms.SplitContainer();
            this.crossSectionChart = new ProfileChartView();
            this.splitContainerSectionViews = new System.Windows.Forms.SplitContainer();
            this.crossSectionSectionsTable = new SectionsTableView();
            this.crossSectionViewZwSectionsView1 = new ZWSectionsView();
            this.viewSplitContainer.Panel1.SuspendLayout();
            this.viewSplitContainer.Panel2.SuspendLayout();
            this.viewSplitContainer.SuspendLayout();
            this.leftSplitContainer.Panel1.SuspendLayout();
            this.leftSplitContainer.Panel2.SuspendLayout();
            this.leftSplitContainer.SuspendLayout();
            this.splitContainerCrossSectionData.Panel1.SuspendLayout();
            this.splitContainerCrossSectionData.Panel2.SuspendLayout();
            this.splitContainerCrossSectionData.SuspendLayout();
            this.tableGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tableView)).BeginInit();
            this.rightSplitContainer.Panel1.SuspendLayout();
            this.rightSplitContainer.Panel2.SuspendLayout();
            this.rightSplitContainer.SuspendLayout();
            this.splitContainerSectionViews.Panel1.SuspendLayout();
            this.splitContainerSectionViews.Panel2.SuspendLayout();
            this.splitContainerSectionViews.SuspendLayout();
            this.SuspendLayout();
            // 
            // viewSplitContainer
            // 
            this.viewSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.viewSplitContainer.Name = "viewSplitContainer";
            // 
            // viewSplitContainer.Panel1
            // 
            this.viewSplitContainer.Panel1.Controls.Add(this.leftSplitContainer);
            // 
            // viewSplitContainer.Panel2
            // 
            this.viewSplitContainer.Panel2.Controls.Add(this.rightSplitContainer);
            this.viewSplitContainer.Size = new System.Drawing.Size(1068, 589);
            this.viewSplitContainer.SplitterDistance = 297;
            this.viewSplitContainer.TabIndex = 1;
            // 
            // leftSplitContainer
            // 
            this.leftSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.leftSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.leftSplitContainer.Name = "leftSplitContainer";
            this.leftSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // leftSplitContainer.Panel1
            // 
            this.leftSplitContainer.Panel1.Controls.Add(this.splitContainerCrossSectionData);
            // 
            // leftSplitContainer.Panel2
            // 
            this.leftSplitContainer.Panel2.Controls.Add(this.summerDikeView);
            this.leftSplitContainer.Size = new System.Drawing.Size(297, 589);
            this.leftSplitContainer.SplitterDistance = 435;
            this.leftSplitContainer.TabIndex = 0;
            // 
            // splitContainerCrossSectionData
            // 
            this.splitContainerCrossSectionData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerCrossSectionData.Location = new System.Drawing.Point(0, 0);
            this.splitContainerCrossSectionData.Name = "splitContainerCrossSectionData";
            // 
            // splitContainerCrossSectionData.Panel1
            // 
            this.splitContainerCrossSectionData.Panel1.Controls.Add(this.tableGroupBox);
            // 
            // splitContainerCrossSectionData.Panel2
            // 
            this.splitContainerCrossSectionData.Panel2.Controls.Add(this.crossSectionStandardDataView1);
            this.splitContainerCrossSectionData.Size = new System.Drawing.Size(297, 435);
            this.splitContainerCrossSectionData.SplitterDistance = 73;
            this.splitContainerCrossSectionData.TabIndex = 1;
            // 
            // tableGroupBox
            // 
            this.tableGroupBox.Controls.Add(this.tableView);
            this.tableGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableGroupBox.Location = new System.Drawing.Point(0, 0);
            this.tableGroupBox.Name = "tableGroupBox";
            this.tableGroupBox.Size = new System.Drawing.Size(73, 435);
            this.tableGroupBox.TabIndex = 1;
            this.tableGroupBox.TabStop = false;
            this.tableGroupBox.Text = "TODO";
            // 
            // tableView
            // 
            this.tableView.AllowAddNewRow = true;
            this.tableView.AllowColumnPinning = true;
            this.tableView.AllowColumnSorting = true;
            this.tableView.AllowDeleteRow = true;
            this.tableView.AutoGenerateColumns = true;
            this.tableView.ColumnAutoWidth = false;
            this.tableView.DisplayCellFilter = null;
            this.tableView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableView.HeaderHeigth = -1;
            this.tableView.InputValidator = null;
            this.tableView.InvalidCellBackgroundColor = System.Drawing.Color.Tomato;
            this.tableView.InvalidCellFilter = null;
            this.tableView.IsEndEditOnEnterKey = false;
            this.tableView.Location = new System.Drawing.Point(3, 16);
            this.tableView.Margin = new System.Windows.Forms.Padding(4);
            this.tableView.MultipleCellEdit = true;
            this.tableView.MultiSelect = true;
            this.tableView.Name = "tableView";
            this.tableView.ReadOnly = false;
            this.tableView.ReadOnlyCellBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
            this.tableView.ReadOnlyCellFilter = null;
            this.tableView.ReadOnlyCellForeColor = System.Drawing.Color.Black;
            this.tableView.RowHeight = -1;
            this.tableView.RowSelect = false;
            this.tableView.RowValidator = null;
            this.tableView.EditButtons = true;
            this.tableView.ShowRowNumbers = false;
            this.tableView.Size = new System.Drawing.Size(67, 416);
            this.tableView.TabIndex = 0;
            this.tableView.UseCenteredHeaderText = false;
            // 
            // crossSectionStandardDataView1
            // 
            this.crossSectionStandardDataView1.Data = null;
            this.crossSectionStandardDataView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.crossSectionStandardDataView1.Image = null;
            this.crossSectionStandardDataView1.Location = new System.Drawing.Point(0, 0);
            this.crossSectionStandardDataView1.MinimumSize = new System.Drawing.Size(270, 0);
            this.crossSectionStandardDataView1.Name = "crossSectionStandardDataView1";
            this.crossSectionStandardDataView1.Size = new System.Drawing.Size(270, 435);
            this.crossSectionStandardDataView1.TabIndex = 0;
            // 
            // summerDikeView
            // 
            this.summerDikeView.Data = null;
            this.summerDikeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.summerDikeView.Image = null;
            this.summerDikeView.Location = new System.Drawing.Point(0, 0);
            this.summerDikeView.Margin = new System.Windows.Forms.Padding(4);
            this.summerDikeView.MinimumSize = new System.Drawing.Size(279, 128);
            this.summerDikeView.Name = "summerDikeView";
            this.summerDikeView.Size = new System.Drawing.Size(297, 150);
            this.summerDikeView.TabIndex = 0;
            this.summerDikeView.Visible = false;
            // 
            // rightSplitContainer
            // 
            this.rightSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.rightSplitContainer.Name = "rightSplitContainer";
            this.rightSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // rightSplitContainer.Panel1
            // 
            this.rightSplitContainer.Panel1.Controls.Add(this.crossSectionChart);
            // 
            // rightSplitContainer.Panel2
            // 
            this.rightSplitContainer.Panel2.Controls.Add(this.splitContainerSectionViews);
            this.rightSplitContainer.Size = new System.Drawing.Size(767, 589);
            this.rightSplitContainer.SplitterDistance = 376;
            this.rightSplitContainer.TabIndex = 0;
            // 
            // crossSectionChart
            // 
            this.crossSectionChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.crossSectionChart.HistoryToolEnabled = false;
            this.crossSectionChart.Location = new System.Drawing.Point(0, 0);
            this.crossSectionChart.Margin = new System.Windows.Forms.Padding(4);
            this.crossSectionChart.Name = "crossSectionChart";
            this.crossSectionChart.Size = new System.Drawing.Size(767, 376);
            this.crossSectionChart.TabIndex = 1;
            // 
            // splitContainerSectionViews
            // 
            this.splitContainerSectionViews.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerSectionViews.Location = new System.Drawing.Point(0, 0);
            this.splitContainerSectionViews.Name = "splitContainerSectionViews";
            // 
            // splitContainerSectionViews.Panel1
            // 
            this.splitContainerSectionViews.Panel1.Controls.Add(this.crossSectionSectionsTable);
            // 
            // splitContainerSectionViews.Panel2
            // 
            this.splitContainerSectionViews.Panel2.Controls.Add(this.crossSectionViewZwSectionsView1);
            this.splitContainerSectionViews.Size = new System.Drawing.Size(767, 209);
            this.splitContainerSectionViews.SplitterDistance = 255;
            this.splitContainerSectionViews.TabIndex = 1;
            // 
            // crossSectionSectionsTable
            // 
            this.crossSectionSectionsTable.Data = null;
            this.crossSectionSectionsTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.crossSectionSectionsTable.Image = null;
            this.crossSectionSectionsTable.Location = new System.Drawing.Point(0, 0);
            this.crossSectionSectionsTable.Margin = new System.Windows.Forms.Padding(4);
            this.crossSectionSectionsTable.MaxY = 0D;
            this.crossSectionSectionsTable.MinY = 0D;
            this.crossSectionSectionsTable.Name = "crossSectionSectionsTable";
            this.crossSectionSectionsTable.Size = new System.Drawing.Size(255, 209);
            this.crossSectionSectionsTable.TabIndex = 0;
            // 
            // crossSectionViewZwSectionsView1
            // 
            this.crossSectionViewZwSectionsView1.Data = null;
            this.crossSectionViewZwSectionsView1.Image = null;
            this.crossSectionViewZwSectionsView1.Location = new System.Drawing.Point(3, 3);
            this.crossSectionViewZwSectionsView1.Margin = new System.Windows.Forms.Padding(4);
            this.crossSectionViewZwSectionsView1.MaximumSize = new System.Drawing.Size(249, 102);
            this.crossSectionViewZwSectionsView1.Name = "crossSectionViewZwSectionsView1";
            this.crossSectionViewZwSectionsView1.Size = new System.Drawing.Size(249, 102);
            this.crossSectionViewZwSectionsView1.TabIndex = 0;
            // 
            // CrossSectionDefinitionView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.viewSplitContainer);
            this.Name = "CrossSectionDefinitionView";
            this.Size = new System.Drawing.Size(1068, 589);
            this.viewSplitContainer.Panel1.ResumeLayout(false);
            this.viewSplitContainer.Panel2.ResumeLayout(false);
            this.viewSplitContainer.ResumeLayout(false);
            this.leftSplitContainer.Panel1.ResumeLayout(false);
            this.leftSplitContainer.Panel2.ResumeLayout(false);
            this.leftSplitContainer.ResumeLayout(false);
            this.splitContainerCrossSectionData.Panel1.ResumeLayout(false);
            this.splitContainerCrossSectionData.Panel2.ResumeLayout(false);
            this.splitContainerCrossSectionData.ResumeLayout(false);
            this.tableGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tableView)).EndInit();
            this.rightSplitContainer.Panel1.ResumeLayout(false);
            this.rightSplitContainer.Panel2.ResumeLayout(false);
            this.rightSplitContainer.ResumeLayout(false);
            this.splitContainerSectionViews.Panel1.ResumeLayout(false);
            this.splitContainerSectionViews.Panel2.ResumeLayout(false);
            this.splitContainerSectionViews.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer viewSplitContainer;
        private System.Windows.Forms.SplitContainer rightSplitContainer;
        private System.Windows.Forms.SplitContainer leftSplitContainer;
        private DelftTools.Controls.Swf.Table.TableView tableView;
        private System.Windows.Forms.GroupBox tableGroupBox;
        private SectionsTableView crossSectionSectionsTable;
        private SummerDikeView summerDikeView;
        private ProfileChartView crossSectionChart;
        private System.Windows.Forms.SplitContainer splitContainerSectionViews;
        private ZWSectionsView crossSectionViewZwSectionsView1;
        private System.Windows.Forms.SplitContainer splitContainerCrossSectionData;
        private StandardCrossSections.CrossSectionStandardDataView crossSectionStandardDataView1;

    }
}
