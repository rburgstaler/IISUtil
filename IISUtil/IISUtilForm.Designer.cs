namespace IISUtil
{
    partial class IISUtilForm
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
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btGetPossibleArguments = new System.Windows.Forms.Button();
            this.tbArguments = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(169, 37);
            this.button1.TabIndex = 0;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(12, 338);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(442, 180);
            this.textBox1.TabIndex = 1;
            this.textBox1.WordWrap = false;
            // 
            // btGetPossibleArguments
            // 
            this.btGetPossibleArguments.Location = new System.Drawing.Point(12, 71);
            this.btGetPossibleArguments.Name = "btGetPossibleArguments";
            this.btGetPossibleArguments.Size = new System.Drawing.Size(169, 44);
            this.btGetPossibleArguments.TabIndex = 2;
            this.btGetPossibleArguments.Text = "Get Possible Arguments";
            this.btGetPossibleArguments.UseVisualStyleBackColor = true;
            this.btGetPossibleArguments.Click += new System.EventHandler(this.btGetPossibleArguments_Click);
            // 
            // tbArguments
            // 
            this.tbArguments.Location = new System.Drawing.Point(12, 133);
            this.tbArguments.Multiline = true;
            this.tbArguments.Name = "tbArguments";
            this.tbArguments.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbArguments.Size = new System.Drawing.Size(442, 199);
            this.tbArguments.TabIndex = 3;
            this.tbArguments.WordWrap = false;
            // 
            // IISUtilForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(466, 530);
            this.Controls.Add(this.tbArguments);
            this.Controls.Add(this.btGetPossibleArguments);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button1);
            this.Name = "IISUtilForm";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btGetPossibleArguments;
        private System.Windows.Forms.TextBox tbArguments;
    }
}

