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
        private string serverIp; // IP-адрес сервера
        private int port; // Порт сервера

        public Player Player { get; private set; } // Информация о текущем игроке
        public Player CurrentPlayer { get; set; } // Игрок, чей сейчас ход

        // События для уведомления о получении данных от сервера
        public event Action<JoinMessage> OnJoinResponse; // Ответ от сервера на подключение
        public event Action<GameStateMessage> OnGameStateUpdated; // Обновление состояния игры
        public event Action<int> OnGameOver; // Завершение игры

        public GameClient(string serverIp, int port)
        {
            this.serverIp = serverIp;
            this.port = port;
        }

        // Подключение к серверу и регистрация игрока
        public async Task Connect(string playerName)
        {
            try
            {
                var client = new TcpClient();
                await client.ConnectAsync(serverIp, port);
                Console.WriteLine("Подключение к серверу успешно.");

                Player = new Player
                {
                    Client = client,
                    Name = playerName,
                    IsActive = true
                };

                // Отправляем сообщение о подключении
                var joinMessage = new JoinMessage { PlayerName = playerName };
                await Player.Client.SendJson(new Message { Join = joinMessage });

                // Получаем ответ от сервера
                var response = await Player.Client.ReceiveJson<Message>();
                if (response?.Join != null)
                {
                    Player.Id = response.Join.PlayerId;
                    Console.WriteLine($"Игрок {Player.Name} (ID: {Player.Id}) успешно зарегистрирован.");

                    // Вызываем событие OnJoinResponse
                    OnJoinResponse?.Invoke(response.Join);

                    // Запускаем прослушивание сообщений
                    _ = StartListening();
                }
                else
                {
                    throw new Exception("Не удалось получить подтверждение подключения от сервера.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при подключении к серверу: {ex.Message}");
                throw;
            }
        }

        // Отправка хода на сервер
        public async Task SendMove(int revealX, int revealY, int mineX, int mineY)
        {
            var moveMessage = new MoveMessage
            {
                PlayerId = Player.Id,
                RevealX = revealX,
                RevealY = revealY,
                MineX = mineX,
                MineY = mineY
            };
            await Player.Client.SendJson(new Message { Move = moveMessage });
        }

        // Отправка сообщения в чат
        public async Task SendChatMessage(string text)
        {
            var chatMessage = new ChatMessage
            {
                Text = text,
                SenderId = Player.Id
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
                    var message = await Player.Client.ReceiveJson<Message>();

                    if (message?.GameState != null)
                    {
                        // Уведомляем о новом состоянии игры
                        OnGameStateUpdated?.Invoke(message.GameState);
                    }
                    else if (message?.GameState?.IsGameOver == true)
                    {
                        // Уведомляем о завершении игры
                        OnGameOver?.Invoke(message.GameState.WinnerId ?? 0);
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
