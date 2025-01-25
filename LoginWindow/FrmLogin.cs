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
                MessageBox.Show("Пожалуйста, введите имя и IP-адрес сервера.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var gameClient = new GameClient(serverIp, 2024);

                // Подписываемся на событие OnJoinResponse
                gameClient.OnJoinResponse += (joinResponse) =>
                {
                    // Открываем основную форму игры
                    var mainForm = new FrmMain(gameClient, playerName, joinResponse.FieldSize);
                    mainForm.Show();

                    // Закрываем форму логина
                    this.Hide();
                };

                await gameClient.Connect(playerName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось подключиться к серверу: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
