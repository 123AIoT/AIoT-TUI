namespace Byoe.LogNet
{
    partial class FormLogNetView
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.userButton1 = new System.Windows.Forms.Button();
            this.logNetAnalysisControl1 = new Byoe.LogNet.LogNetAnalysisControl();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox1.Location = new System.Drawing.Point(83, 3);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(560, 23);
            this.textBox1.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(9, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 17);
            this.label1.TabIndex = 6;
            this.label1.Text = "文件路径：";
            // 
            // userButton1
            // 
            this.userButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.userButton1.BackColor = System.Drawing.Color.Transparent;
            this.userButton1.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.userButton1.Location = new System.Drawing.Point(664, 3);
            this.userButton1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.userButton1.Name = "userButton1";
            this.userButton1.Size = new System.Drawing.Size(95, 25);
            this.userButton1.TabIndex = 8;
            this.userButton1.Text = "文件选择";
            this.userButton1.UseVisualStyleBackColor = false;
            this.userButton1.Click += new System.EventHandler(this.userButton1_Click);
            // 
            // logNetAnalysisControl1
            // 
            this.logNetAnalysisControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logNetAnalysisControl1.Location = new System.Drawing.Point(12, 35);
            this.logNetAnalysisControl1.Name = "logNetAnalysisControl1";
            this.logNetAnalysisControl1.Size = new System.Drawing.Size(796, 509);
            this.logNetAnalysisControl1.TabIndex = 5;
            this.logNetAnalysisControl1.Load += new System.EventHandler(this.logNetAnalysisControl1_Load);
            // 
            // FormLogNetView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(814, 561);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.userButton1);
            this.Controls.Add(this.logNetAnalysisControl1);
            this.Name = "FormLogNetView";
            this.Text = "FormLogNetView";
            this.Load += new System.EventHandler(this.FormLogNetView_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button userButton1;
        private LogNetAnalysisControl logNetAnalysisControl1;
    }
}