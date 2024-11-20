using DelftTools.Controls;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    partial class FmMeteoFieldListView
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.fmMeteoItemsListBox = new System.Windows.Forms.ListBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonRemove);
            this.panel1.Controls.Add(this.buttonAdd);
            this.panel1.Controls.Add(this.fmMeteoItemsListBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(5);
            this.panel1.Size = new System.Drawing.Size(146, 353);
            this.panel1.TabIndex = 0;
            // 
            // buttonRemove
            // 
            this.buttonRemove.Location = new System.Drawing.Point(74, 156);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(67, 23);
            this.buttonRemove.TabIndex = 2;
            this.buttonRemove.Text = "Remove";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.ButtonRemoveClick);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Location = new System.Drawing.Point(5, 156);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(67, 23);
            this.buttonAdd.TabIndex = 1;
            this.buttonAdd.Text = "Add...";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.ButtonAddClick);
            // 
            // fmMeteoItemsListBox
            // 
            this.fmMeteoItemsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fmMeteoItemsListBox.FormattingEnabled = true;
            this.fmMeteoItemsListBox.Location = new System.Drawing.Point(5, 5);
            this.fmMeteoItemsListBox.Margin = new System.Windows.Forms.Padding(5);
            this.fmMeteoItemsListBox.Name = "fmMeteoItemsListBox";
            this.fmMeteoItemsListBox.Size = new System.Drawing.Size(136, 147);
            this.fmMeteoItemsListBox.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(146, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(393, 353);
            this.panel2.TabIndex = 1;
            // 
            // FmMeteoFieldListView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.MinimumSize = new System.Drawing.Size(539, 353);
            this.Name = "FmMeteoFieldListView";
            this.Size = new System.Drawing.Size(539, 353);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.ListBox fmMeteoItemsListBox;

        public FmMeteoFieldListView()
        {
            ChildViews = new EventedList<IView>();
            InitializeComponent();
            fmMeteoItemsListBox.SelectedIndexChanged += FmMeteoItemsListBoxOnSelectedIndexChanged;
            fmMeteoItemsListBox.Format += FmMeteoItemsListBoxOnFormat;
            PopulateMeteoItemsListBox();
        }
    }
}
