using DeltaShell.Plugins.SharpMapGis.Gui.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.Feature2DEditors
{
    partial class FixedWeirEditor
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
            this.tableView = new DelftTools.Controls.Swf.Table.TableView();
            this.boundaryGeometryPreview = new BoundaryGeometryPreview();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.tableView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableView
            // 
            this.tableView.AllowAddNewRow = false;
            this.tableView.AllowColumnFiltering = false;
            this.tableView.AllowColumnPinning = true;
            this.tableView.AllowColumnSorting = true;
            this.tableView.AllowDeleteRow = false;
            this.tableView.AutoGenerateColumns = true;
            this.tableView.ColumnAutoWidth = false;
            this.tableView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableView.EditButtons = true;
            this.tableView.HeaderHeigth = -1;
            this.tableView.IncludeHeadersOnCopy = false;
            this.tableView.InvalidCellBackgroundColor = System.Drawing.Color.Tomato;
            this.tableView.IsEndEditOnEnterKey = false;
            this.tableView.Location = new System.Drawing.Point(0, 0);
            this.tableView.MultipleCellEdit = true;
            this.tableView.MultiSelect = true;
            this.tableView.Name = "tableView";
            this.tableView.ReadOnly = false;
            this.tableView.ReadOnlyCellBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
            this.tableView.ReadOnlyCellForeColor = System.Drawing.Color.Black;
            this.tableView.RowHeight = -1;
            this.tableView.RowSelect = false;
            this.tableView.RowValidator = null;
            this.tableView.ShowImportExportToolbar = false;
            this.tableView.ShowRowNumbers = false;
            this.tableView.Size = new System.Drawing.Size(205, 287);
            this.tableView.TabIndex = 0;
            this.tableView.UseCenteredHeaderText = false;
            this.tableView.ViewInfo = null;
            // 
            // boundaryGeometryPreview1
            // 
            this.boundaryGeometryPreview.DataPoints = null;
            this.boundaryGeometryPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.boundaryGeometryPreview.Location = new System.Drawing.Point(0, 0);
            this.boundaryGeometryPreview.Name = "boundaryGeometryPreview";
            this.boundaryGeometryPreview.Size = new System.Drawing.Size(227, 287);
            this.boundaryGeometryPreview.TabIndex = 1;
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.tableView);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.boundaryGeometryPreview);
            this.splitContainer.Size = new System.Drawing.Size(436, 287);
            this.splitContainer.SplitterDistance = 205;
            this.splitContainer.TabIndex = 2;
            // 
            // FixedWeirEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer);
            this.Name = "FixedWeirEditor";
            this.Size = new System.Drawing.Size(436, 287);
            ((System.ComponentModel.ISupportInitialize)(this.tableView)).EndInit();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DelftTools.Controls.Swf.Table.TableView tableView;
        private BoundaryGeometryPreview boundaryGeometryPreview;
        private System.Windows.Forms.SplitContainer splitContainer;
    }
}
