using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using TcpLib;
using MineJumperClassLibrary;


using Message = MineJumperClassLibrary.Message;

namespace Client
{
    public class GameClient
    {
        private string serverIp;
        private int port; 

        public Player Player { get; private set; } // Информация о игроке
        public int CurrentTurnPlayerId { get; set; } // ID текущего игрока

        public CellState[,] FieldState { get; set; }

        // События для уведомления о получении данных от сервера
        public event Action<JoinMessage> OnJoinResponse; // Ответ от сервера на подключение
        public event Action<GameStateMessage> OnGameStateUpdated; // Обновление состояния игры
        public event Action<string> OnChatMessageReceived; // Имя игрока, текст сообщения
        public event Action<int> OnGameOver; // Завершение игры

        public GameClient(string serverIp, int port)
        {
            this.serverIp = serverIp;
            this.port = port;
        }

        public async Task Connect(string playerName)
        {
            try
            {
                TcpClient client = new TcpClient();
                await client.ConnectAsync(serverIp, port);
                MessageBox.Show("Подключение к серверу успешно.");

                Player = new Player
                {
                    Client = client,
                    Name = playerName,
                    IsActive = true
                };

                JoinMessage joinMessage = new JoinMessage { PlayerName = playerName };
                await Player.Client.SendJson(new Message { Join = joinMessage });

                Message response = await Player.Client.ReceiveJson<Message>();
                if (response?.Join != null)
                {
                    Player.Id = response.Join.PlayerId;
                    MessageBox.Show($"Игрок {Player.Name} (ID: {Player.Id}) успешно зарегистрирован.");

                    OnJoinResponse?.Invoke(response.Join);

                    _ = StartListening();
                }
                else
                {
                    throw new Exception("Не удалось получить подтверждение подключения от сервера.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подключении к серверу: {ex.Message}");
                throw;
            }
        }

        // Отправка хода на сервер
        public async Task SendMove(int revealX, int revealY, int mineX, int mineY)
        {
            MoveMessage moveMessage = new MoveMessage
            {
                PlayerId = Player.Id,
                RevealX = revealX,
                RevealY = revealY,
                MineX = mineX,
                MineY = mineY
            };
            try
            {
                await Player.Client.SendJson(new Message { Move = moveMessage });
            }
            catch (Exception ex)
            {
                MessageBox.Show (ex.ToString() );
            }
        }

        // Отправка сообщения в чат
        public async Task SendChatMessage(string text)
        {
            ChatMessage chatMessage = new ChatMessage
            {
                Text = text
            };
            await Player.Client.SendJson(new Message { Chat = chatMessage });
        }

        // Прослушивание сообщений от сервера
        private async Task StartListening()
        {
            try
            {
                while (Player.Client.Connected)
                {
                    Message? message = await Player.Client.ReceiveJson<Message>();
                    if (message?.GameState?.IsGameOver == true)                    
                        OnGameOver?.Invoke(message.GameState.WinnerId ?? 0);
                    
                    else if (message?.GameState != null)
                    {
                        CurrentTurnPlayerId = message.GameState.CurrentPlayerId;
                        OnGameStateUpdated?.Invoke(message.GameState);
                    }
                    else if (message?.Chat != null)
                    {
                        string chatMessage = message.Chat.Text;
                        OnChatMessageReceived?.Invoke(chatMessage);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при прослушивании сообщений: {ex.Message}");
            }
            finally
            {
                Player.Client.Close();
            }
        }
    }
}
