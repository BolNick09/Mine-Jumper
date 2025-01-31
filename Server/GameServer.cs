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
        private readonly Size fieldSize; 

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

                TcpClient client = await listener.AcceptTcpClientAsync();
                Console.WriteLine("Подключен новый клиент.");

                // Запускаем обработку сообщений для нового клиента
                // Без await, чтобы не блокировать поток
                _ = HandleClient(client); 
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            try
            {
                while (client.Connected)
                {
                    Message message = await client.ReceiveJson<Message>();
                    if (message != null)                    
                        await ProcessMessage(message, client);                    
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
            
            if (message.Join != null)// Обработка подключения            
                await HandleJoinMessage(message.Join, client);
            
            else if (message.Move != null)// Обработка хода            
                await HandleMove(message.Move); 
            
            else if (message.Leave != null)// Обработка отключения            
                await HandleLeaveMessage(message.Leave, client);
            
            else if (message.Chat != null)// Обработка чата 
            {
                Player? sender = activeSession?.Players.FirstOrDefault(p => p.Client == client);
                if (sender != null)               
                    await HandleChatMessage(message.Chat);                
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

            Message joinResponse = new Message//Отправка рег данных игроку
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
                _ = activeSession.StartGame();            
        }

        private async Task HandleMove(MoveMessage moveMessage)
        {
            // Находим игрока, который сделал ход
            Player? activePlayer = activeSession.Players.FirstOrDefault(player => player.Id == moveMessage.PlayerId);

            Console.WriteLine($"Игрок {activePlayer.Name} (ID: {activePlayer.Id}) сделал ход: разминирование ({moveMessage.RevealX}, {moveMessage.RevealY}), установка мины ({moveMessage.MineX}, {moveMessage.MineY}).");

            //Проверка на взрыв
            if (activeSession.GameField.TryRevealCell(moveMessage.RevealX, moveMessage.RevealY, activePlayer.Id))
            {
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
            Player? leavingPlayer = activeSession.Players.FirstOrDefault(player => player.Id == leaveMessage.PlayerId);
            Console.WriteLine($"Игрок {leavingPlayer.Name} (ID: {leavingPlayer.Id}) покинул игру.");
            activeSession.Players.Remove(leavingPlayer);
            
            Console.WriteLine("Игровая сессия завершена из-за выхода игрока.");
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
            activeSession = null;
            
            
        }

        private async Task HandleChatMessage(ChatMessage chatMessage)
        {
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
