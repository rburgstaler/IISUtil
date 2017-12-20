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
            this.panel1 = new System.Windows.Forms.Panel();
            this.tbOutput = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panel2 = new System.Windows.Forms.Panel();
            this.bt85Example = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btGetPossibleArguments = new System.Windows.Forms.Button();
            this.btRun = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.tbArguments = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tbOutput);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 417);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(828, 248);
            this.panel1.TabIndex = 5;
            // 
            // tbOutput
            // 
            this.tbOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbOutput.Location = new System.Drawing.Point(12, 23);
            this.tbOutput.Name = "tbOutput";
            this.tbOutput.Size = new System.Drawing.Size(804, 213);
            this.tbOutput.TabIndex = 7;
            this.tbOutput.Text = "";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Output";
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter1.Location = new System.Drawing.Point(0, 407);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(828, 10);
            this.splitter1.TabIndex = 6;
            this.splitter1.TabStop = false;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.bt85Example);
            this.panel2.Controls.Add(this.button2);
            this.panel2.Controls.Add(this.button1);
            this.panel2.Controls.Add(this.btGetPossibleArguments);
            this.panel2.Controls.Add(this.btRun);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(828, 59);
            this.panel2.TabIndex = 7;
            // 
            // bt85Example
            // 
            this.bt85Example.Location = new System.Drawing.Point(657, 12);
            this.bt85Example.Name = "bt85Example";
            this.bt85Example.Size = new System.Drawing.Size(134, 23);
            this.bt85Example.TabIndex = 7;
            this.bt85Example.Text = "IIS 8.5 Example";
            this.bt85Example.UseVisualStyleBackColor = true;
            this.bt85Example.Click += new System.EventHandler(this.bt85Example_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(567, 12);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 6;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(455, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 5;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btGetPossibleArguments
            // 
            this.btGetPossibleArguments.Location = new System.Drawing.Point(187, 12);
            this.btGetPossibleArguments.Name = "btGetPossibleArguments";
            this.btGetPossibleArguments.Size = new System.Drawing.Size(169, 37);
            this.btGetPossibleArguments.TabIndex = 4;
            this.btGetPossibleArguments.Text = "Get Possible Arguments";
            this.btGetPossibleArguments.UseVisualStyleBackColor = true;
            this.btGetPossibleArguments.Click += new System.EventHandler(this.btGetPossibleArguments_Click);
            // 
            // btRun
            // 
            this.btRun.Location = new System.Drawing.Point(12, 12);
            this.btRun.Name = "btRun";
            this.btRun.Size = new System.Drawing.Size(169, 37);
            this.btRun.TabIndex = 3;
            this.btRun.Text = "Run";
            this.btRun.UseVisualStyleBackColor = true;
            this.btRun.Click += new System.EventHandler(this.btRun_Click);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.label1);
            this.panel3.Controls.Add(this.tbArguments);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 59);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(828, 348);
            this.panel3.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Arguments";
            // 
            // tbArguments
            // 
            this.tbArguments.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbArguments.Location = new System.Drawing.Point(12, 24);
            this.tbArguments.Multiline = true;
            this.tbArguments.Name = "tbArguments";
            this.tbArguments.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbArguments.Size = new System.Drawing.Size(804, 318);
            this.tbArguments.TabIndex = 4;
            this.tbArguments.WordWrap = false;
            // 
            // IISUtilForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(828, 665);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.panel1);
            this.Name = "IISUtilForm";
            this.Text = "IIS Utility";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.IISUtilForm_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btGetPossibleArguments;
        private System.Windows.Forms.Button btRun;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TextBox tbArguments;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox tbOutput;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button bt85Example;

    }
}

