using Client;
using Server;

namespace LoginWindow
{
    public partial class FrmLogin : Form
    {
        public FrmLogin()
        {
            InitializeComponent();
        }

        private void FrmLogin_Load(object sender, EventArgs e)
        {

        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            // �������� ��� ������ � IP-����� �������
            string playerName = tbName.Text.Trim();
            string serverIp = tbIp.Text.Trim();

            // ���������, ��� ���� ���������
            if (string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(serverIp))
            {
                MessageBox.Show("����������, ������� ��� � IP-����� �������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // �������� ������������ � �������
            try
            {
                // ������� ������ � ������������ � �������
                var gameClient = new GameClient(serverIp, 5000); // ���� 5000 �� ���������
                await gameClient.ConnectAsync(playerName);

                // ���� ����������� �������, ��������� �������� ����� ����
                var mainForm = new FrmMain(gameClient, playerName);
                mainForm.Show();

                // ��������� ����� ������
                this.Hide();
            }
            catch (Exception ex)
            {
                // ���� ����������� �� �������, ���������� ��������� �� ������
                MessageBox.Show($"�� ������� ������������ � �������: {ex.Message}", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
