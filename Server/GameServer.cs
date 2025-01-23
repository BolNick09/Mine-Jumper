using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

using Mine_Jumper_class_library;
using TcpLib;
using System.Drawing;

namespace Server
{
    public class GameServer
    {
        private readonly TcpListener listener;
        private GameSession activeSession; // Текущая активная сессия

        public GameServer(IPAddress ipAddress, int port)
        {
            listener = new TcpListener(ipAddress, port);
        }

        public void Start()
        {
            listener.Start();
            Console.WriteLine("Сервер запущен. Ожидание подключений...");
            AcceptClients();
        }

        private void AcceptClients()
        {
            while (true)
            {
                var client = listener.AcceptTcpClient();
                Console.WriteLine("Подключен новый клиент.");

                // Если активной сессии нет, создаем новую
                if (activeSession == null)
                {
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
                    var player2Client = listener.AcceptTcpClient();
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
                    Task.Run(() => activeSession.StartGame());
                }
                else
                {
                    // Если игра уже начата, отправляем сообщение о том, что сервер занят
                    Console.WriteLine("Игра уже начата. Новые подключения невозможны.");
                    var busyMessage = new Message
                    {
                        Chat = new ChatMessage
                        {
                            Text = "Игра уже начата. Попробуйте позже.",
                            SenderId = null // Сообщение от сервера, поэтому SenderId = null
                        }
                    };
                    client.SendJson(busyMessage);
                    client.Close();
                }
            }
        }

        private async void HandleClient(TcpClient client)
        {
            try
            {
                while (client.Connected)
                {
                    // Получаем сообщение от клиента
                    var message = await client.ReceiveJson<Message>();

                    if (message != null)
                    {
                        ProcessMessage(message, client);
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

        private void ProcessMessage(Message message, TcpClient client)
        {
            if (message.Join != null)
            {
                HandleJoinMessage(message.Join, client);
            }
            else if (message.Move != null)
            {
                HandleMoveMessage(message.Move, client);
            }
            else if (message.Leave != null)
            {
                HandleLeaveMessage(message.Leave, client);
            }
            else if (message.Chat != null)
            {
                var sender = activeSession?.Players.FirstOrDefault(p => p.Client == client);
                if (sender != null)
                {
                    HandleChatMessage(message.Chat, sender);
                }
            }
        }

        private void HandleJoinMessage(JoinMessage joinMessage, TcpClient client)
        {
            Console.WriteLine($"Игрок {joinMessage.PlayerName} (ID: {joinMessage.PlayerId}) подключился.");
            //TODO Логика для регистрации игрока
        }

        private void HandleMoveMessage(MoveMessage moveMessage, TcpClient client)
        {
            Console.WriteLine($"Игрок {moveMessage.PlayerId} сделал ход: разминирование ({moveMessage.RevealX}, {moveMessage.RevealY}), установка мины ({moveMessage.MineX}, {moveMessage.MineY}).");
            //TODO Логика для обработки хода
        }

        private void HandleLeaveMessage(LeaveMessage leaveMessage, TcpClient client)
        {
            Console.WriteLine($"Игрок {leaveMessage.PlayerId} покинул игру.");
            //TODO Логика для удаления игрока
        }

        private async Task HandleChatMessage(ChatMessage chatMessage, Player sender)
        {
            //TODO Рассылаем сообщение всем игрокам
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
