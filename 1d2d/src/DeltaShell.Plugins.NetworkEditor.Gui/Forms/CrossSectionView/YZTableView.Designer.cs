namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView
{
    partial class YZTableView
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
            this.tableViewYZ = new DelftTools.Controls.Swf.Table.TableView();
            ((System.ComponentModel.ISupportInitialize)(this.tableViewYZ)).BeginInit();
            this.SuspendLayout();
            // 
            // tableViewYZ
            // 
            this.tableViewYZ.AllowAddNewRow = true;
            this.tableViewYZ.AllowDeleteRow = true;
            this.tableViewYZ.AutoGenerateColumns = true;
            this.tableViewYZ.ColumnAutoWidth = false;
            this.tableViewYZ.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableViewYZ.InputValidator = null;
            this.tableViewYZ.InvalidCellBackgroundColor = System.Drawing.Color.Tomato;
            this.tableViewYZ.InvalidCellFilter = null;
            this.tableViewYZ.Location = new System.Drawing.Point(0, 0);
            this.tableViewYZ.MultipleCellEdit = true;
            this.tableViewYZ.Name = "tableViewYZ";
            this.tableViewYZ.ReadOnly = false;
            this.tableViewYZ.ReadOnlyCellFilter = null;
            this.tableViewYZ.ReadOnlyCellForeColor = System.Drawing.Color.LightGray;
            this.tableViewYZ.ShowRowNumbers = false;
            this.tableViewYZ.Size = new System.Drawing.Size(563, 340);
            this.tableViewYZ.TabIndex = 2;
            // 
            // CrossSectionRoughnessSectionsTableView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableViewYZ);
            this.Name = "CrossSectionViewSectionsTable";
            this.Size = new System.Drawing.Size(563, 340);
            ((System.ComponentModel.ISupportInitialize)(this.tableViewYZ)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DelftTools.Controls.Swf.Table.TableView tableViewYZ;
    }
}
