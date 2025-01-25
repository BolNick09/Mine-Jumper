namespace LoginWindow
{
    partial class FrmLogin
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
            label1 = new Label();
            tbName = new TextBox();
            btnConnect = new Button();
            label2 = new Label();
            tbIp = new TextBox();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(10, 7);
            label1.Name = "label1";
            label1.Size = new Size(119, 15);
            label1.TabIndex = 0;
            label1.Text = "Введите имя игрока:";
            // 
            // tbName
            // 
            tbName.Location = new Point(149, 4);
            tbName.Margin = new Padding(3, 2, 3, 2);
            tbName.Name = "tbName";
            tbName.Size = new Size(204, 23);
            tbName.TabIndex = 1;
            // 
            // btnConnect
            // 
            btnConnect.Font = new Font("Segoe UI", 8F);
            btnConnect.Location = new Point(358, 29);
            btnConnect.Margin = new Padding(3, 2, 3, 2);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(95, 20);
            btnConnect.TabIndex = 2;
            btnConnect.Text = "Подключиться";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(10, 32);
            label2.Name = "label2";
            label2.Size = new Size(113, 15);
            label2.TabIndex = 3;
            label2.Text = "Введите IP сервера:";
            // 
            // tbIp
            // 
            tbIp.Location = new Point(149, 29);
            tbIp.Margin = new Padding(3, 2, 3, 2);
            tbIp.Name = "tbIp";
            tbIp.Size = new Size(204, 23);
            tbIp.TabIndex = 4;
            tbIp.Text = "127.0.0.1";
            // 
            // FrmLogin
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(458, 56);
            Controls.Add(tbIp);
            Controls.Add(label2);
            Controls.Add(btnConnect);
            Controls.Add(tbName);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(3, 2, 3, 2);
            Name = "FrmLogin";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Логин";
            Load += FrmLogin_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox tbName;
        private Button btnConnect;
        private Label label2;
        private TextBox tbIp;
    }
}
