namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    partial class TracerDefinitionsEditor
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
            this.newTracerTextBox = new System.Windows.Forms.TextBox();
            this.addTracerDefinitionButton = new System.Windows.Forms.Button();
            this.itemsListBox = new DeltaShell.Plugins.FMSuite.Common.Gui.Editors.RemoveableItemsListBox();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // newTracerTextBox
            // 
            this.newTracerTextBox.Location = new System.Drawing.Point(3, 106);
            this.newTracerTextBox.Name = "newTracerTextBox";
            this.newTracerTextBox.Size = new System.Drawing.Size(120, 20);
            this.newTracerTextBox.TabIndex = 1;
            this.newTracerTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tracerTextBox_KeyPress);
            // 
            // addTracerDefinitionButton
            // 
            this.addTracerDefinitionButton.Cursor = System.Windows.Forms.Cursors.Default;
            this.addTracerDefinitionButton.Image = global::DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties.Resources.Plus;
            this.addTracerDefinitionButton.Location = new System.Drawing.Point(129, 105);
            this.addTracerDefinitionButton.Margin = new System.Windows.Forms.Padding(3, 3, 23, 3);
            this.addTracerDefinitionButton.Name = "addTracerDefinitionButton";
            this.addTracerDefinitionButton.Size = new System.Drawing.Size(35, 20);
            this.addTracerDefinitionButton.TabIndex = 2;
            this.addTracerDefinitionButton.UseVisualStyleBackColor = true;
            this.addTracerDefinitionButton.Click += new System.EventHandler(this.addTracerDefinitionButton_Click);
            // 
            // itemsListBox
            // 
            this.itemsListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.itemsListBox.IntegralHeight = false;
            this.itemsListBox.Location = new System.Drawing.Point(3, 3);
            this.itemsListBox.Name = "itemsListBox";
            this.itemsListBox.Size = new System.Drawing.Size(161, 96);
            this.itemsListBox.TabIndex = 0;
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // TracerDefinitionsEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.addTracerDefinitionButton);
            this.Controls.Add(this.newTracerTextBox);
            this.Controls.Add(this.itemsListBox);
            this.Name = "TracerDefinitionsEditor";
            this.Size = new System.Drawing.Size(187, 129);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Common.Gui.Editors.RemoveableItemsListBox itemsListBox;
        private System.Windows.Forms.TextBox newTracerTextBox;
        private System.Windows.Forms.Button addTracerDefinitionButton;
        private System.Windows.Forms.ErrorProvider errorProvider;
    }
}
