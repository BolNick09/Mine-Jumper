namespace Client
{
    partial class FrmMain
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
            tbChat = new TextBox();
            tbEnterChat = new TextBox();
            btnSendChat = new Button();
            gbPlayField = new GroupBox();
            SuspendLayout();
            // 
            // tbChat
            // 
            tbChat.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            tbChat.Location = new Point(418, 12);
            tbChat.Multiline = true;
            tbChat.Name = "tbChat";
            tbChat.ReadOnly = true;
            tbChat.Size = new Size(254, 178);
            tbChat.TabIndex = 0;
            // 
            // tbEnterChat
            // 
            tbEnterChat.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            tbEnterChat.Location = new Point(418, 197);
            tbEnterChat.Name = "tbEnterChat";
            tbEnterChat.Size = new Size(254, 23);
            tbEnterChat.TabIndex = 1;
            // 
            // btnSendChat
            // 
            btnSendChat.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSendChat.Location = new Point(528, 224);
            btnSendChat.Name = "btnSendChat";
            btnSendChat.Size = new Size(144, 23);
            btnSendChat.TabIndex = 2;
            btnSendChat.Text = "Отправить сообщение";
            btnSendChat.UseVisualStyleBackColor = true;
            btnSendChat.Click += btnSendChat_Click;
            // 
            // gbPlayField
            // 
            gbPlayField.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gbPlayField.Location = new Point(12, 12);
            gbPlayField.Name = "gbPlayField";
            gbPlayField.Size = new Size(400, 235);
            gbPlayField.TabIndex = 3;
            gbPlayField.TabStop = false;
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(684, 261);
            Controls.Add(gbPlayField);
            Controls.Add(btnSendChat);
            Controls.Add(tbEnterChat);
            Controls.Add(tbChat);
            Name = "FrmMain";
            Text = "Mine Jumper";
            FormClosing += FrmMain_FormClosing;
            Load += FrmMain_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox tbChat;
        private TextBox tbEnterChat;
        private Button btnSendChat;
        private GroupBox gbPlayField;
    }
}