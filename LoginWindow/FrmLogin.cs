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
            // Получаем имя игрока и IP-адрес сервера
            string playerName = tbName.Text.Trim();
            string serverIp = tbIp.Text.Trim();

            // Проверяем, что поля заполнены
            if (string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(serverIp))
            {
                MessageBox.Show("Пожалуйста, введите имя и IP-адрес сервера.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Пытаемся подключиться к серверу
            try
            {
                // Создаем клиент и подключаемся к серверу
                var gameClient = new GameClient(serverIp, 5000); // Порт 5000 по умолчанию
                await gameClient.ConnectAsync(playerName);

                // Если подключение успешно, открываем основную форму игры
                var mainForm = new FrmMain(gameClient, playerName);
                mainForm.Show();

                // Закрываем форму логина
                this.Hide();
            }
            catch (Exception ex)
            {
                // Если подключение не удалось, показываем сообщение об ошибке
                MessageBox.Show($"Не удалось подключиться к серверу: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
