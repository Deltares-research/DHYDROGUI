using DelftTools.Controls.Swf.Table;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    partial class FlowBoundaryConditionDataView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FlowBoundaryConditionDataView));
            this.noDataLabel = new System.Windows.Forms.Label();
            this.helpProvider1 = new System.Windows.Forms.HelpProvider();
            this.boundaryDataListBox = new System.Windows.Forms.CheckedListBox();
            this.boundaryDataSplitContainer = new System.Windows.Forms.SplitContainer();
            this.functionView = new DeltaShell.Plugins.CommonTools.Gui.Forms.Functions.FunctionView();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.drawButton = new System.Windows.Forms.Button();
            this.fileImportButton = new System.Windows.Forms.Button();
            this.fileExportButton = new System.Windows.Forms.Button();
            this.genDataButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.boundaryDataSplitContainer)).BeginInit();
            this.boundaryDataSplitContainer.Panel1.SuspendLayout();
            this.boundaryDataSplitContainer.Panel2.SuspendLayout();
            this.boundaryDataSplitContainer.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // noDataLabel
            // 
            this.noDataLabel.AutoSize = true;
            this.noDataLabel.Location = new System.Drawing.Point(0, 0);
            this.noDataLabel.Name = "noDataLabel";
            this.noDataLabel.Size = new System.Drawing.Size(0, 13);
            this.noDataLabel.TabIndex = 21;
            // 
            // boundaryDataListBox
            // 
            this.boundaryDataListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.boundaryDataListBox.FormattingEnabled = true;
            this.boundaryDataListBox.Location = new System.Drawing.Point(0, 0);
            this.boundaryDataListBox.Name = "boundaryDataListBox";
            this.boundaryDataListBox.Size = new System.Drawing.Size(241, 388);
            this.boundaryDataListBox.TabIndex = 23;
            // 
            // boundaryDataSplitContainer
            // 
            this.boundaryDataSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.boundaryDataSplitContainer.Location = new System.Drawing.Point(0, 38);
            this.boundaryDataSplitContainer.Name = "boundaryDataSplitContainer";
            // 
            // boundaryDataSplitContainer.Panel1
            // 
            this.boundaryDataSplitContainer.Panel1.Controls.Add(this.boundaryDataListBox);
            // 
            // boundaryDataSplitContainer.Panel2
            // 
            this.boundaryDataSplitContainer.Panel2.Controls.Add(this.noDataLabel);
            this.boundaryDataSplitContainer.Panel2.Controls.Add(this.functionView);
            this.boundaryDataSplitContainer.Size = new System.Drawing.Size(1036, 388);
            this.boundaryDataSplitContainer.SplitterDistance = 241;
            this.boundaryDataSplitContainer.TabIndex = 24;
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
            this.functionView.Image = ((System.Drawing.Image)(resources.GetObject("functionView.Image")));
            this.functionView.Location = new System.Drawing.Point(0, 0);
            this.functionView.MaxSeries = 10;
            this.functionView.Name = "functionView";
            this.functionView.ShowChartView = true;
            this.functionView.ShowTableView = true;
            this.functionView.Size = new System.Drawing.Size(791, 388);
            this.functionView.TabIndex = 22;
            this.functionView.ViewInfo = null;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.Controls.Add(this.drawButton, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.fileImportButton, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.fileExportButton, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.genDataButton, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1036, 38);
            this.tableLayoutPanel1.TabIndex = 21;
            // 
            // drawButton
            // 
            this.drawButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.drawButton.Image = ((System.Drawing.Image)(resources.GetObject("drawButton.Image")));
            this.drawButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.drawButton.Location = new System.Drawing.Point(831, 3);
            this.drawButton.Name = "drawButton";
            this.drawButton.Size = new System.Drawing.Size(202, 32);
            this.drawButton.TabIndex = 17;
            this.drawButton.Text = "Combined BC view...";
            this.drawButton.UseVisualStyleBackColor = true;
            this.drawButton.Click += new System.EventHandler(this.DrawButtonClick);
            // 
            // fileImportButton
            // 
            this.fileImportButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.fileImportButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileImportButton.Image = global::DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties.Resources.document_import;
            this.fileImportButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.fileImportButton.Location = new System.Drawing.Point(417, 3);
            this.fileImportButton.Name = "fileImportButton";
            this.fileImportButton.Size = new System.Drawing.Size(201, 32);
            this.fileImportButton.TabIndex = 19;
            this.fileImportButton.Text = "Import from files...";
            this.fileImportButton.UseVisualStyleBackColor = true;
            this.fileImportButton.Click += new System.EventHandler(this.FileImportButtonClick);
            // 
            // fileExportButton
            // 
            this.fileExportButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileExportButton.Image = global::DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties.Resources.document_export;
            this.fileExportButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.fileExportButton.Location = new System.Drawing.Point(624, 3);
            this.fileExportButton.Name = "fileExportButton";
            this.fileExportButton.Size = new System.Drawing.Size(201, 32);
            this.fileExportButton.TabIndex = 20;
            this.fileExportButton.Text = "Export to files...";
            this.fileExportButton.UseVisualStyleBackColor = true;
            this.fileExportButton.Click += new System.EventHandler(this.FileExportButtonClick);
            // 
            // genDataButton
            // 
            this.genDataButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.genDataButton.Image = ((System.Drawing.Image)(resources.GetObject("genDataButton.Image")));
            this.genDataButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.genDataButton.Location = new System.Drawing.Point(3, 3);
            this.genDataButton.Name = "genDataButton";
            this.genDataButton.Size = new System.Drawing.Size(201, 32);
            this.genDataButton.TabIndex = 16;
            this.genDataButton.UseVisualStyleBackColor = true;
            // 
            // FlowBoundaryConditionDataView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.boundaryDataSplitContainer);
            this.Controls.Add(this.tableLayoutPanel1);
            this.helpProvider1.SetHelpKeyword(this, " ");
            this.Name = "FlowBoundaryConditionDataView";
            this.helpProvider1.SetShowHelp(this, true);
            this.Size = new System.Drawing.Size(1036, 426);
            this.boundaryDataSplitContainer.Panel1.ResumeLayout(false);
            this.boundaryDataSplitContainer.Panel2.ResumeLayout(false);
            this.boundaryDataSplitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.boundaryDataSplitContainer)).EndInit();
            this.boundaryDataSplitContainer.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button genDataButton;
        private System.Windows.Forms.Button drawButton;
        private System.Windows.Forms.Label noDataLabel;
        private CommonTools.Gui.Forms.Functions.FunctionView functionView;
        private System.Windows.Forms.HelpProvider helpProvider1;
        private System.Windows.Forms.CheckedListBox boundaryDataListBox;
        private System.Windows.Forms.SplitContainer boundaryDataSplitContainer;
        private System.Windows.Forms.Button fileExportButton;
        private System.Windows.Forms.Button fileImportButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
