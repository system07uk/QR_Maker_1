namespace QR_Maker_1
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            button1 = new Button();
            textBox1 = new TextBox();
            pictureBox1 = new PictureBox();
            comboBox1 = new ComboBox();
            label1 = new Label();
            label2 = new Label();
            BtnBatchGenerate = new Button();
            button2 = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(10, 13);
            button1.Name = "button1";
            button1.Size = new Size(122, 31);
            button1.TabIndex = 0;
            button1.Text = "경로/파일찾기";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(137, 13);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(703, 31);
            textBox1.TabIndex = 1;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = SystemColors.ActiveCaption;
            pictureBox1.Location = new Point(180, 117);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(500, 620);
            pictureBox1.TabIndex = 2;
            pictureBox1.TabStop = false;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(9, 153);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(120, 23);
            comboBox1.TabIndex = 3;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(180, 96);
            label1.Name = "label1";
            label1.Size = new Size(80, 15);
            label1.TabIndex = 4;
            label1.Text = "변환 대기중...";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(9, 179);
            label2.Name = "label2";
            label2.Size = new Size(136, 15);
            label2.TabIndex = 5;
            label2.Text = "산업 환경에서는 H 권장";
            // 
            // BtnBatchGenerate
            // 
            BtnBatchGenerate.Location = new Point(9, 88);
            BtnBatchGenerate.Name = "BtnBatchGenerate";
            BtnBatchGenerate.Size = new Size(122, 31);
            BtnBatchGenerate.TabIndex = 6;
            BtnBatchGenerate.Text = "CSV 일괄변환";
            BtnBatchGenerate.UseVisualStyleBackColor = true;
            BtnBatchGenerate.Click += BtnBatchGenerate_Click;
            // 
            // button2
            // 
            button2.Location = new Point(9, 50);
            button2.Name = "button2";
            button2.Size = new Size(122, 31);
            button2.TabIndex = 7;
            button2.Text = "URL로 생성";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(844, 741);
            Controls.Add(button2);
            Controls.Add(BtnBatchGenerate);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(comboBox1);
            Controls.Add(pictureBox1);
            Controls.Add(textBox1);
            Controls.Add(button1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "Form1";
            Text = "QR Maker";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private TextBox textBox1;
        private PictureBox pictureBox1;
        private ComboBox comboBox1;
        private Label label1;
        private Label label2;
        private Button BtnBatchGenerate;
        private Button button2;
    }
}
