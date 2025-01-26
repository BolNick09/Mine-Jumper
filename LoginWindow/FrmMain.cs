using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Resources;
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

            gameClient.OnChatMessageReceived += (chatMessage) =>
            {
                // Обновляем текстовое поле чата
                tbChat.AppendText($"{chatMessage}{Environment.NewLine}");
            };
        }

        private async void btnSendChat_Click(object sender, EventArgs e)
        {
            string message = $"{gameClient.Player.Name}: {tbEnterChat.Text}";
            if (!string.IsNullOrEmpty(message))
            {
                await gameClient.SendChatMessage(message);
                tbEnterChat.Clear();
            }
        }

        private void InitializeGameField(Size fieldSize)
        {
            // Устанавливаем размеры GroupBox
            int groupBoxWidth = fieldSize.Width * 20 + 20; // Ширина: количество кнопок * 20 + отступы
            int groupBoxHeight = fieldSize.Height * 20 + 20; // Высота: количество кнопок * 20 + отступы
            gbPlayField.Size = new Size(groupBoxWidth, groupBoxHeight);

            // Устанавливаем размеры формы
            this.Size = new Size(groupBoxWidth + 300, groupBoxHeight + 65);
            this.Text += $" Player: {playerName}";
            tbChat.Left = gbPlayField.Left + groupBoxWidth + 30;
            tbEnterChat.Left = gbPlayField.Left + groupBoxWidth + 30;

            tbChat.Width = this.Width - gbPlayField.Width - 60;
            tbEnterChat.Width = this.Width - gbPlayField.Width - 60;

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
                        BackColor = SystemColors.Control // Стандартный цвет кнопки
                    };
                    button.Click += Cell_Click; // Обработчик клика по кнопке
                    gbPlayField.Controls.Add(button); // Добавляем кнопку в GroupBox
                }
            }
        }

        private bool isFirstClick = true; // Флаг для первого клика
        private Point firstClickCoordinates; // Координаты первой клетки
        private async void Cell_Click(object sender, EventArgs e)
        {
            // Проверяем, наш ли ход
            if (gameClient.Player.Id != gameClient.CurrentTurnPlayerId)
            {
                MessageBox.Show("Сейчас не ваш ход.");
                return;
            }
            var button = (Button)sender;
            var cellCoordinates = (Point)button.Tag;

            if (button.BackColor != SystemColors.Control)
                return;

            if (isFirstClick)
            {
                // Первый клик — открытие клетки
                firstClickCoordinates = cellCoordinates;
                isFirstClick = false;

                // Временно помечаем клетку как открытую (для визуальной обратной связи)
                button.BackColor = Color.Green;
            }
            else
            {                
                // Второй клик — установка мины
                isFirstClick = true;

                // Отправляем ход на сервер
                await gameClient.SendMove(firstClickCoordinates.X, firstClickCoordinates.Y, cellCoordinates.X, cellCoordinates.Y);

                // Помечаем клетку с миной (для визуальной обратной связи)
                button.BackColor = Color.Red;
            }
        }
        private void HandleGameStateUpdate(GameStateMessage gameState)
        {
            //Установка очередности хода 
            gameClient.Player.IsMyTurn = gameClient.Player.Id == gameState.CurrentPlayerId;
            // Блокируем кнопки, если ход не текущего игрока
            UpdateButtonsState(gameClient.Player.Id == gameState.CurrentPlayerId);
            gameClient.FieldState = CellState.DeserializeField(gameState.StrField);
            // Обновляем интерфейс в соответствии с состоянием игры
            for (int x = 0; x < gameClient.FieldState.GetLength(0); x++)
            {
                for (int y = 0; y < gameClient.FieldState.GetLength(1); y++)
                {
                    var cellState = gameClient.FieldState[x, y];
                    var button = gbPlayField.Controls
                        .OfType<Button>()
                        .FirstOrDefault(b => ((Point)b.Tag).X == x && ((Point)b.Tag).Y == y);

                    if (button != null)
                    {
                        if (cellState.IsRevealed)
                        {
                            button.BackColor = Color.Green;
                        }
                        else if (cellState.IsPlayerMine)
                        {
                            button.BackColor = Color.Red;
                        }
                        else
                        {
                            button.BackColor = SystemColors.Control;
                        }
                    }
                }
            }
        }
        private void UpdateButtonsState(bool isEnabled)
        {
            foreach (var button in gbPlayField.Controls.OfType<Button>())
            {
                button.Enabled = isEnabled;
            }
        }

        private void HandleGameOver(int winnerId)
        {
            // Показываем сообщение о завершении игры
            MessageBox.Show(winnerId == gameClient.CurrentTurnPlayerId ? "Вы победили!" : "Вы проиграли.");
            this.Close();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {

        }        
    }
}
