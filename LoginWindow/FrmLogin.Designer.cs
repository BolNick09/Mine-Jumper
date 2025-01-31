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
            label1.Location = new Point(11, 9);
            label1.Name = "label1";
            label1.Size = new Size(152, 20);
            label1.TabIndex = 0;
            label1.Text = "Введите имя игрока:";
            // 
            // tbName
            // 
            tbName.Location = new Point(170, 5);
            tbName.Name = "tbName";
            tbName.Size = new Size(233, 27);
            tbName.TabIndex = 1;
            // 
            // btnConnect
            // 
            btnConnect.Font = new Font("Segoe UI", 8F);
            btnConnect.Location = new Point(409, 39);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(116, 27);
            btnConnect.TabIndex = 2;
            btnConnect.Text = "Подключиться";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(11, 43);
            label2.Name = "label2";
            label2.Size = new Size(145, 20);
            label2.TabIndex = 3;
            label2.Text = "Введите IP сервера:";
            // 
            // tbIp
            // 
            tbIp.Location = new Point(170, 39);
            tbIp.Name = "tbIp";
            tbIp.Size = new Size(233, 27);
            tbIp.TabIndex = 4;
            tbIp.Text = "127.0.0.1";
            // 
            // FrmLogin
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(537, 75);
            Controls.Add(tbIp);
            Controls.Add(label2);
            Controls.Add(btnConnect);
            Controls.Add(tbName);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
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
