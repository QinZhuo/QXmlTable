namespace XmlTable
{
    partial class Finder
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.findInput = new System.Windows.Forms.TextBox();
            this.findButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // findInput
            // 
            this.findInput.Location = new System.Drawing.Point(37, 60);
            this.findInput.Name = "findInput";
            this.findInput.Size = new System.Drawing.Size(215, 21);
            this.findInput.TabIndex = 0;
            // 
            // findButton
            // 
            this.findButton.Location = new System.Drawing.Point(286, 57);
            this.findButton.Name = "findButton";
            this.findButton.Size = new System.Drawing.Size(75, 23);
            this.findButton.TabIndex = 1;
            this.findButton.Text = "查找";
            this.findButton.UseVisualStyleBackColor = true;
            this.findButton.Click += new System.EventHandler(this.findButton_Click);
            // 
            // Finder
            // 
            this.AcceptButton = this.findButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(386, 143);
            this.Controls.Add(this.findButton);
            this.Controls.Add(this.findInput);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Finder";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Finder";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox findInput;
        private System.Windows.Forms.Button findButton;
    }
}