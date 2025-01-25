using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using MineJumperClassLibrary;

namespace Client
{
    public partial class FrmMain : Form
    {
        private GameClient gameClient;
        private string playerName;
        public FrmMain(GameClient gameClient, string playerName, Size fieldSize)
        {
            InitializeComponent();

            this.gameClient = gameClient;
            this.playerName = playerName;

            // Инициализируем игровое поле
            InitializeGameField(fieldSize);

            // Подписываемся на события
            gameClient.OnGameStateUpdated += HandleGameStateUpdate;
            gameClient.OnGameOver += HandleGameOver;
        }        

        private async void BtnSendChat_Click(object sender, EventArgs e)
        {
            string message = tbEnterChat.Text.Trim();
            if (!string.IsNullOrEmpty(message))
            {
                await gameClient.SendChatMessage(message);
                tbEnterChat.Clear();
            }
        }

        private void InitializeGameField(Size fieldSize)
        {
            // Очищаем GroupBox перед созданием новых кнопок
            gbPlayField.Controls.Clear();

            // Устанавливаем размеры GroupBox
            int groupBoxWidth = fieldSize.Width * 20 + 20; // Ширина: количество кнопок * 20 + отступы
            int groupBoxHeight = fieldSize.Height * 20 + 20; // Высота: количество кнопок * 20 + отступы
            gbPlayField.Size = new Size(groupBoxWidth, groupBoxHeight);

            // Устанавливаем размеры формы
            this.Size = new Size(groupBoxWidth + 300, groupBoxHeight + 65);

            // Создаем кнопки и добавляем их в GroupBox
            for (int x = 0; x < fieldSize.Width; x++)
            {
                for (int y = 0; y < fieldSize.Height; y++)
                {
                    var button = new Button
                    {
                        Size = new Size(20, 20), // Размер кнопки
                        Location = new Point(10 + x * 20, 10 + y * 20), // Позиция кнопки
                        Tag = new Point(x, y), // Сохраняем координаты кнопки в Tag
                        //Image =  // Иконка по умолчанию (закрытая клетка)
                    };
                    button.Click += Cell_Click; // Обработчик клика по кнопке
                    gbPlayField.Controls.Add(button); // Добавляем кнопку в GroupBox
                }
            }
        }
        private async void Cell_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var cellCoordinates = (Point)button.Tag;

            // Отправляем ход на сервер
            await gameClient.SendMove(cellCoordinates.X, cellCoordinates.Y, cellCoordinates.X, cellCoordinates.Y);
        }
        private void HandleGameStateUpdate(GameStateMessage gameState)
        {
            // Обновляем интерфейс в соответствии с состоянием игры
            // Например, обновляем клетки на поле
        }

        private void HandleGameOver(int winnerId)
        {
            // Показываем сообщение о завершении игры
            MessageBox.Show(winnerId == gameClient.PlayerId ? "Вы победили!" : "Вы проиграли.");
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {

        }

        private async void btnSendChat_Click(object sender, EventArgs e)
        {
            string message = tbEnterChat.Text.Trim();
            if (!string.IsNullOrEmpty(message))
            {
                await gameClient.SendChatMessage(message);
                tbEnterChat.Clear();
            }
        }
    }
}
