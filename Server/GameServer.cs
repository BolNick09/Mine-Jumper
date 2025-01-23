using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Drawing;

using MineJumperClassLibrary;
using TcpLib;


namespace Server
{
    public class GameServer
    {
        private readonly TcpListener listener;
        private GameSession activeSession; 

        public GameServer(IPAddress ipAddress, int port)
        {
            listener = new TcpListener(ipAddress, port)
              ?? throw new ArgumentNullException(nameof(ipAddress));
        }

        public async Task Start()
        {
            listener.Start();
            Console.WriteLine("Сервер запущен. Ожидание подключений...");
            await AcceptClients();
        }

        private async Task AcceptClients()
        {
            while (true)
            {
                // Асинхронно ожидаем подключения нового клиента
                var client = await listener.AcceptTcpClientAsync();
                Console.WriteLine("Подключен новый клиент.");

                // Если игра уже начата, отправляем сообщение о том, что сервер занят
                if (activeSession != null)
                {                    
                    Console.WriteLine("Игра уже начата. Новые подключения невозможны.");
                    var busyMessage = new Message
                    {
                        Chat = new ChatMessage
                        {
                            Text = "Игра уже начата. Попробуйте позже.",
                            SenderId = null // Сообщение от сервера, поэтому SenderId = null
                        }
                    };
                    await client.SendJson(busyMessage);
                    client.Close();
                }

                // Регистрация первого игрока
                var player1 = new Player
                {
                    Id = 1,
                    Name = "Player 1",
                    IsActive = true,
                    Client = client
                };

                // Ожидаем подключения второго игрока
                Console.WriteLine("Ожидание второго игрока...");
                var player2Client = await listener.AcceptTcpClientAsync();
                Console.WriteLine("Второй игрок подключен.");

                // Регистрация второго игрока
                var player2 = new Player
                {
                    Id = 2,
                    Name = "Player 2",
                    IsActive = true,
                    Client = player2Client
                };

                // Создаем новую игровую сессию
                activeSession = new GameSession(player1, player2, new Size(10, 10));
                await activeSession.StartGame(); // Запускаем сессию в отдельной задаче

               _ = HandleClient(player1.Client);// _= Возвращаемый Task не нужен
               _ = HandleClient(player2.Client);

            }
        }

        private async Task HandleClient(TcpClient client)
        {
            try
            {
                while (client.Connected)
                {
                    // Получаем сообщение от клиента
                    var message = await client.ReceiveJson<Message>();

                    if (message != null)
                    {
                        await ProcessMessage(message, client);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке клиента: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }

        private async Task ProcessMessage(Message message, TcpClient client)
        {
            if (message.Join != null)
            {
                await HandleJoinMessage(message.Join, client);
            }
            else if (message.Move != null)
            {
                // Передаем ход в GameSession для обработки
                await activeSession.ProcessMove(message.Move);
            }
            else if (message.Leave != null)
            {
                await HandleLeaveMessage(message.Leave, client);
            }
            else if (message.Chat != null)
            {
                var sender = activeSession?.Players.FirstOrDefault(p => p.Client == client);
                if (sender != null)
                {
                    await HandleChatMessage(message.Chat, sender);
                }
            }
        }

        private async Task HandleJoinMessage(JoinMessage joinMessage, TcpClient client)
        {
            // Генерация уникального ID для игрока
            int playerId = activeSession.Players.Count + 1;

            // Создание объекта игрока
            var player = new Player
            {
                Id = playerId,
                Name = joinMessage.PlayerName,
                IsActive = true,
                Client = client
            };

            // Добавление игрока в сессию
            activeSession.Players.Add(player);

            Console.WriteLine($"Игрок {player.Name} (ID: {player.Id}) подключился.");

            // Отправка подтверждения подключения
            var joinResponse = new Message
            {
                Join = new JoinMessage
                {
                    PlayerId = player.Id,
                    PlayerName = player.Name
                }
            };
            await client.SendJson(joinResponse);
        }       

        private async Task HandleLeaveMessage(LeaveMessage leaveMessage, TcpClient client)
        {
            // Находим игрока, который покидает игру
            var leavingPlayer = activeSession.Players.FirstOrDefault(p => p.Id == leaveMessage.PlayerId);
            if (leavingPlayer == null)
            {
                Console.WriteLine($"Игрок с ID {leaveMessage.PlayerId} не найден.");
                return;
            }

            Console.WriteLine($"Игрок {leavingPlayer.Name} (ID: {leavingPlayer.Id}) покинул игру.");

            // Удаляем игрока из сессии
            activeSession.Players.Remove(leavingPlayer);

            // Если остался только один игрок, завершаем сессию
            if (activeSession.Players.Count < 2)
            {
                Console.WriteLine("Игровая сессия завершена из-за выхода игрока.");

                // Уведомляем оставшегося игрока о завершении игры
                var remainingPlayer = activeSession.Players.FirstOrDefault();
                if (remainingPlayer != null)
                {
                    var gameOverMessage = new Message
                    {
                        GameState = new GameStateMessage
                        {
                            IsGameOver = true,
                            WinnerId = null // Ничья, так как игра завершена досрочно
                        }
                    };
                    await remainingPlayer.Client.SendJson(gameOverMessage);
                }

                // Очищаем активную сессию
                activeSession = null;
            }
            else
            {
                // Уведомляем второго игрока о выходе соперника
                var opponent = activeSession.Players.First(p => p.Id != leaveMessage.PlayerId);
                var leaveNotification = new Message
                {
                    Chat = new ChatMessage
                    {
                        Text = $"{leavingPlayer.Name} покинул игру.",
                        SenderId = null // Сообщение от сервера
                    }
                };
                await opponent.Client.SendJson(leaveNotification);
            }
        }

        private async Task HandleChatMessage(ChatMessage chatMessage, Player sender)
        {
            // Рассылаем сообщение всем игрокам
            foreach (var player in activeSession.Players)
            {
                var message = new Message
                {
                    Chat = new ChatMessage
                    {
                        Text = chatMessage.Text,
                        SenderId = sender.Id
                    }
                };
                await player.Client.SendJson(message);
            }
        }
    }
}
