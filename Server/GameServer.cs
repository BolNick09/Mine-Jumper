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
        private GameSession activeSession; // Текущая активная сессия
        private readonly Size fieldSize; // Размер игрового поля

        public GameServer(IPAddress ipAddress, int port, Size fieldSize)
        {
            listener = new TcpListener(ipAddress, port);
            this.fieldSize = fieldSize;
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
                // Ожидание подключения нового клиента
                TcpClient client = await listener.AcceptTcpClientAsync();
                Console.WriteLine("Подключен новый клиент.");

                // Запускаем обработку сообщений для нового клиента
                _ = HandleClient(client); // Без await, чтобы не блокировать поток
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            try
            {
                while (client.Connected)
                {
                    // Получаем сообщение от клиента
                    Message message = await client.ReceiveJson<Message>();

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
                await HandleMove(message.Move); // Обработка хода
            }
            else if (message.Leave != null)
            {
                await HandleLeaveMessage(message.Leave, client);
            }
            else if (message.Chat != null)
            {
                Player? sender = activeSession?.Players.FirstOrDefault(p => p.Client == client);
                if (sender != null)
                {
                    await HandleChatMessage(message.Chat);
                }
            }
        }

        private async Task HandleJoinMessage(JoinMessage joinMessage, TcpClient client)
        {
            if (activeSession == null)            
                activeSession = new GameSession(fieldSize);
            

            int playerId = activeSession.Players.Count + 1;

            Player player = new Player
            {
                Id = playerId,
                Name = joinMessage.PlayerName,
                IsActive = true,
                Client = client
            };

            activeSession.Players.Add(player);

            // Отправляем ответ клиенту
            Message joinResponse = new Message
            {
                Join = new JoinMessage
                {
                    PlayerId = player.Id,
                    PlayerName = player.Name,
                    FieldSize = fieldSize
                }
            };
            await client.SendJson(joinResponse);

            if (activeSession.Players.Count == 2)
            {
                _ = activeSession.StartGame();
            }
        }

        private async Task HandleMove(MoveMessage moveMessage)
        {
            // Находим игрока, который сделал ход
            Player? activePlayer = activeSession.Players.FirstOrDefault(player => player.Id == moveMessage.PlayerId);

            Console.WriteLine($"Игрок {activePlayer.Name} (ID: {activePlayer.Id}) сделал ход: разминирование ({moveMessage.RevealX}, {moveMessage.RevealY}), установка мины ({moveMessage.MineX}, {moveMessage.MineY}).");

            // Обработка хода
            bool isExploded = activeSession.GameField.TryRevealCell(moveMessage.RevealX, moveMessage.RevealY, activePlayer.Id);
            if (isExploded)
            {
                // Игрок взорвался
                Console.WriteLine($"Игрок {activePlayer.Name} (ID: {activePlayer.Id}) взорвался.");
                Player opponentPlayer = activeSession.Players.First(p => p.Id != activePlayer.Id);
                await activeSession.NotifyGameOver(activePlayer, opponentPlayer); // Уведомляем о завершении игры
                return;
            }

            // Установка мины
            activeSession.GameField.PlaceMine(moveMessage.MineX, moveMessage.MineY, activePlayer.Id);
            try
            {
                // Передача хода сопернику
                Player nextPlayer = activeSession.Players.First(player => player.Id != activePlayer.Id);

                // Отправка обновлений состояния игры
                await activeSession.SendGameState(activePlayer, nextPlayer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке хода игрока: {ex.Message}");
            }
        }

        private async Task HandleLeaveMessage(LeaveMessage leaveMessage, TcpClient client)
        {
            // Находим игрока, который покидает игру
            Player? leavingPlayer = activeSession.Players.FirstOrDefault(player => player.Id == leaveMessage.PlayerId);
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
                Player? remainingPlayer = activeSession.Players.FirstOrDefault();
                if (remainingPlayer != null)
                {
                    Message gameOverMessage = new Message
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
                Player opponent = activeSession.Players.First(p => p.Id != leaveMessage.PlayerId);
                Message leaveNotification = new Message
                {
                    Chat = new ChatMessage
                    {
                        Text = $"{leavingPlayer.Name} покинул игру.",
                    }
                };
                await opponent.Client.SendJson(leaveNotification);
            }
        }

        private async Task HandleChatMessage(ChatMessage chatMessage)
        {
            // Рассылаем сообщение всем игрокам
            foreach (Player player in activeSession.Players)
            {
                Message message = new Message
                {
                    Chat = new ChatMessage
                    {
                        Text = chatMessage.Text,
                    }
                };
                await player.Client.SendJson(message);
            }
        }
    }
}
