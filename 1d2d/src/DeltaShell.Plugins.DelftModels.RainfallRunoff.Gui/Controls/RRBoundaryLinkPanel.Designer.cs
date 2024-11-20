using System.ComponentModel;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls
{
    partial class RRBoundaryLinkPanel
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
            this.panelLink = new System.Windows.Forms.Panel();
            this.valuesFromLinkedNodeChkBox = new System.Windows.Forms.CheckBox();
            this.bindingSourceViewModel = new System.Windows.Forms.BindingSource(this.components);
            this.lblLinked = new System.Windows.Forms.Label();
            this.panelLink.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceViewModel)).BeginInit();
            this.SuspendLayout();
            // 
            // panelLink
            // 
            this.panelLink.BackColor = System.Drawing.Color.Transparent;
            this.panelLink.Controls.Add(this.valuesFromLinkedNodeChkBox);
            this.panelLink.Controls.Add(this.lblLinked);
            this.panelLink.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelLink.Location = new System.Drawing.Point(0, 0);
            this.panelLink.Margin = new System.Windows.Forms.Padding(4);
            this.panelLink.Name = "panelLink";
            this.panelLink.Padding = new System.Windows.Forms.Padding(13, 12, 13, 12);
            this.panelLink.Size = new System.Drawing.Size(972, 78);
            this.panelLink.TabIndex = 2;
            // 
            // valuesFromLinkedNodeChkBox
            // 
            this.valuesFromLinkedNodeChkBox.AutoSize = true;
            this.valuesFromLinkedNodeChkBox.BackColor = System.Drawing.Color.Transparent;
            this.valuesFromLinkedNodeChkBox.Checked = true;
            this.valuesFromLinkedNodeChkBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.valuesFromLinkedNodeChkBox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.bindingSourceViewModel, "UseWaterLevelFromLinkedNode", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.valuesFromLinkedNodeChkBox.Location = new System.Drawing.Point(32, 40);
            this.valuesFromLinkedNodeChkBox.Margin = new System.Windows.Forms.Padding(4);
            this.valuesFromLinkedNodeChkBox.Name = "valuesFromLinkedNodeChkBox";
            this.valuesFromLinkedNodeChkBox.Size = new System.Drawing.Size(314, 21);
            this.valuesFromLinkedNodeChkBox.TabIndex = 5;
            this.valuesFromLinkedNodeChkBox.Text = "Use water level value(s) from the linked node";
            this.valuesFromLinkedNodeChkBox.UseVisualStyleBackColor = false;
            // 
            // bindingSourceViewModel
            // 
            this.bindingSourceViewModel.DataSource = typeof(DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.UnpavedDataViewModel);
            // 
            // lblLinked
            // 
            this.lblLinked.AutoSize = true;
            this.lblLinked.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLinked.Location = new System.Drawing.Point(12, 12);
            this.lblLinked.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblLinked.Name = "lblLinked";
            this.lblLinked.Size = new System.Drawing.Size(296, 17);
            this.lblLinked.TabIndex = 4;
            this.lblLinked.Text = "This catchment is linked to a 1D branch";
            // 
            // RRBoundaryLinkPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.panelLink);
            this.Location = new System.Drawing.Point(15, 15);
            this.Name = "RRBoundaryLinkPanel";
            this.Size = new System.Drawing.Size(972, 78);
            this.panelLink.ResumeLayout(false);
            this.panelLink.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceViewModel)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.BindingSource bindingSourceViewModel;

        private System.Windows.Forms.CheckBox valuesFromLinkedNodeChkBox;
        private System.Windows.Forms.Panel panelLink;
        private System.Windows.Forms.Label lblLinked;

        #endregion
    }
}