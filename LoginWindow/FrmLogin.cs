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
            string playerName = tbName.Text.Trim();
            string serverIp = tbIp.Text.Trim();

            if (string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(serverIp))
            {
                MessageBox.Show("����������, ������� ��� � IP-����� �������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var gameClient = new GameClient(serverIp, 2024);

                // ������������� �� ������� OnJoinResponse
                gameClient.OnJoinResponse += (joinResponse) =>
                {
                    // ��������� �������� ����� ����
                    var mainForm = new FrmMain(gameClient, playerName, joinResponse.FieldSize);
                    mainForm.Show();

                    // ��������� ����� ������
                    this.Hide();
                };

                await gameClient.Connect(playerName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"�� ������� ������������ � �������: {ex.Message}", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
