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
        private TcpClient client; // Клиент для подключения к серверу
        private string serverIp; // IP-адрес сервера
        private int port; // Порт сервера

        public int PlayerId { get; private set; } // ID игрока, полученный от сервера
        public string PlayerName { get; private set; } // Имя игрока
        public Size FieldSize { get; private set; } // Размер игрового поля

        // События для уведомления о получении данных от сервера
        public event Action<JoinMessage> OnJoinResponse; // Ответ от сервера на подключение
        public event Action<GameStateMessage> OnGameStateUpdated; // Обновление состояния игры
        public event Action<int> OnGameOver; // Завершение игры

        public GameClient(string serverIp, int port)
        {
            this.serverIp = serverIp;
            this.port = port;
            client = new TcpClient();
        }

        // Подключение к серверу и регистрация игрока
        public async Task ConnectAsync(string playerName)
        {
            try
            {
                // Подключаемся к серверу
                await client.ConnectAsync(serverIp, port);
                Console.WriteLine("Подключение к серверу успешно.");

                // Отправляем сообщение о подключении
                var joinMessage = new JoinMessage { PlayerName = playerName };
                await client.SendJson(new Message { Join = joinMessage });

                // Получаем ответ от сервера с подтверждением подключения
                var response = await client.ReceiveJson<Message>();
                if (response?.Join != null)
                {
                    PlayerId = response.Join.PlayerId;
                    PlayerName = response.Join.PlayerName;
                    FieldSize = response.Join.FieldSize;

                    // Уведомляем о получении ответа от сервера
                    OnJoinResponse?.Invoke(response.Join);

                    // Запускаем прослушивание сообщений от сервера
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
                PlayerId = PlayerId,
                RevealX = revealX,
                RevealY = revealY,
                MineX = mineX,
                MineY = mineY
            };
            await client.SendJson(new Message { Move = moveMessage });
        }

        // Отправка сообщения в чат
        public async Task SendChatMessage(string text)
        {
            var chatMessage = new ChatMessage
            {
                Text = text,
                SenderId = PlayerId
            };
            await client.SendJson(new Message { Chat = chatMessage });
        }

        // Прослушивание сообщений от сервера
        private async Task StartListening()
        {
            try
            {
                while (client.Connected)
                {
                    // Получаем сообщение от сервера
                    var message = await client.ReceiveJson<Message>();

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
                client.Close();
            }
        }
    }
}
