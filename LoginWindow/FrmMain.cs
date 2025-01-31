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
using LoginWindow;
using MineJumperClassLibrary;

namespace Client
{
    public partial class FrmMain : Form
    {
        private GameClient gameClient;
        private string playerName;
        private FrmLogin frmLogin;
        public FrmMain(GameClient gameClient, string playerName, Size fieldSize, FrmLogin frmLogin)
        {
            InitializeComponent();

            this.gameClient = gameClient;
            this.playerName = playerName;
            this.frmLogin = frmLogin;

            InitializeGameField(fieldSize);

            gameClient.OnGameStateUpdated += HandleGameStateUpdate;
            gameClient.OnGameOver += HandleGameOver;

            gameClient.OnChatMessageReceived += (chatMessage) =>
            {
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
            //размеры gb
            int groupBoxWidth = fieldSize.Width * 20 + 20;  //+ отступы
            int groupBoxHeight = fieldSize.Height * 20 + 20;//+ отступы 
            gbPlayField.Size = new Size(groupBoxWidth, groupBoxHeight);

            //размеры всей формы
            this.Size = new Size(groupBoxWidth + 300, groupBoxHeight + 65);
            this.Text += $" Player: {playerName}";
            tbChat.Left = gbPlayField.Left + groupBoxWidth + 30;
            tbEnterChat.Left = gbPlayField.Left + groupBoxWidth + 30;

            tbChat.Width = this.Width - gbPlayField.Width - 60;
            tbEnterChat.Width = this.Width - gbPlayField.Width - 60;

            //Создание кнопок-игровых клеток
            for (int x = 0; x < fieldSize.Width; x++)
            {
                for (int y = 0; y < fieldSize.Height; y++)
                {
                    Button button = new Button
                    {
                        Size = new Size(20, 20), 
                        Location = new Point(10 + x * 20, 10 + y * 20), 
                        Tag = new Point(x, y), 
                        BackColor = SystemColors.Control 
                        //Image = Properties.Resources.imgNone
                    };
                    button.Click += Cell_Click; 
                    gbPlayField.Controls.Add(button);
                }
            }
        }

        private bool isFirstClick = true; // Флаг для первого клика
        private Point firstClickCoordinates; // Координаты первого нажатия
        private async void Cell_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            Point cellCoordinates = (Point)button.Tag;

            if (button.BackColor != SystemColors.Control)
                return;

            if (isFirstClick) // Первый клик — открытие клетки
            {
                
                firstClickCoordinates = cellCoordinates;
                isFirstClick = false;

                button.BackColor = Color.Green;
                //button.Image = Properties.Resources.imgFlag;
            }
            else // Второй клик — установка мины
            {
                
                isFirstClick = true;

                await gameClient.SendMove(firstClickCoordinates.X, firstClickCoordinates.Y, cellCoordinates.X, cellCoordinates.Y);
                button.BackColor = Color.Red;
                //button.Image = Properties.Resources.imgMine;
            }
        }
        private void HandleGameStateUpdate(GameStateMessage gameState)
        {
            //Установка очередности хода 
            gameClient.Player.IsMyTurn = gameClient.Player.Id == gameState.CurrentPlayerId;
            // Блокируем кнопки, если ход не текущего игрока
            UpdateButtonsState(gameClient.Player.Id == gameState.CurrentPlayerId);

            gameClient.FieldState = CellState.DeserializeField(gameState.StrField);
            for (int x = 0; x < gameClient.FieldState.GetLength(0); x++)
            {
                for (int y = 0; y < gameClient.FieldState.GetLength(1); y++)
                {
                    CellState cellState = gameClient.FieldState[x, y];
                    Button? button = gbPlayField.Controls
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
            foreach (Button button in gbPlayField.Controls.OfType<Button>())
            {
                button.Enabled = isEnabled;
            }
        }

        private void HandleGameOver(int winnerId)
        {
            // Показываем сообщение о завершении игры
            MessageBox.Show(winnerId == gameClient.Player.Id ? "Вы победили!" : "Вы проиграли.");
            this.Close();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {

        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            frmLogin.Close();
            this.Close();
        }
    }
}
