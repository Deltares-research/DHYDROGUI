namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView
{
    partial class SectionsTableView
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
            this.tableViewSections = new DelftTools.Controls.Swf.Table.TableView();
            ((System.ComponentModel.ISupportInitialize)(this.tableViewSections)).BeginInit();
            this.SuspendLayout();
            // 
            // tableViewSections
            // 
            this.tableViewSections.AllowAddNewRow = true;
            this.tableViewSections.AllowDeleteRow = true;
            this.tableViewSections.AutoGenerateColumns = true;
            this.tableViewSections.ColumnAutoWidth = false;
            this.tableViewSections.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableViewSections.HeaderHeigth = -1;
            this.tableViewSections.InputValidator = null;
            this.tableViewSections.InvalidCellBackgroundColor = System.Drawing.Color.Tomato;
            this.tableViewSections.InvalidCellFilter = null;
            this.tableViewSections.Location = new System.Drawing.Point(0, 0);
            this.tableViewSections.MultipleCellEdit = true;
            this.tableViewSections.Name = "tableViewSections";
            this.tableViewSections.ReadOnly = false;
            this.tableViewSections.ReadOnlyCellBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
            this.tableViewSections.ReadOnlyCellFilter = null;
            this.tableViewSections.ReadOnlyCellForeColor = System.Drawing.Color.Gray;
            this.tableViewSections.RowSelect = false;
            this.tableViewSections.ShowRowNumbers = false;
            this.tableViewSections.Size = new System.Drawing.Size(563, 340);
            this.tableViewSections.TabIndex = 2;
            this.tableViewSections.UseCenteredHeaderText = false;
            // 
            // CrossSectionViewBranchSectionsTable
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableViewSections);
            this.Name = "CrossSectionViewSectionsTable";
            this.Size = new System.Drawing.Size(563, 340);
            ((System.ComponentModel.ISupportInitialize)(this.tableViewSections)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DelftTools.Controls.Swf.Table.TableView tableViewSections;
    }
}
