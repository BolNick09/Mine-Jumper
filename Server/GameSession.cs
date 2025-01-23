using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using TcpLib;
using MineJumperClassLibrary;

namespace Server
{
    public class GameSession
    {
        public List<Player> Players { get; } // Список игроков в сессии
        public GameField GameField { get; } // Игровое поле
        private int currentPlayerId = 1; // Текущий игрок (1 или 2)

        public GameSession(Player player1, Player player2, Size fieldSize)
        {
            // Инициализация списка игроков
            Players = new List<Player> { player1, player2 };

            // Инициализация игрового поля
            this.GameField = new GameField(fieldSize);
        }

        public async Task StartGame()
        {
            Console.WriteLine("Начало новой игровой сессии.");
            await SendInitialState();

            while (true)
            {
                var currentPlayer = Players.First(player => player.Id == currentPlayerId);
                var opponent = Players.First(player => player.Id != currentPlayerId);

                // Получаем ход от текущего игрока
                var move = await ReceiveMove(currentPlayer);

                // Обработка хода
                var isExploded = GameField.TryRevealCell(move.RevealX, move.RevealY, currentPlayerId);
                if (isExploded)
                {
                    await NotifyGameOver(currentPlayer, opponent);
                    break;
                }

                // Установка мины
                GameField.PlaceMine(move.MineX, move.MineY, currentPlayerId);

                // Передача хода сопернику
                currentPlayerId = currentPlayerId == 1 ? 2 : 1;

                // Отправка обновлений состояния игры
                await SendGameState(currentPlayer, opponent);
            }
        }

        public async Task SendInitialState()
        {
            // Отправка начального состояния игры
            var initialState = new GameStateMessage
            {
                PlayerId = Players[0].Id,
                Field = GameField.GetFieldState(Players[0].Id),
                IsGameOver = false
            };
            await Players[0].Client.SendJson(new Message { GameState = initialState });

            initialState.PlayerId = Players[1].Id;
            initialState.Field = GameField.GetFieldState(Players[1].Id);
            await Players[1].Client.SendJson(new Message { GameState = initialState });
        }

        private async Task<MoveMessage> ReceiveMove(Player player)
        {
            // Получаем ход от игрока
            var message = await player.Client.ReceiveJson<Message>();

            if (message?.Move != null)
            {
                return message.Move;
            }

            throw new InvalidOperationException("Получено некорректное сообщение.");
        }

        private async Task SendGameState(Player currentPlayer, Player opponent)
        {
            var gameStateForCurrent = new GameStateMessage
            {
                PlayerId = currentPlayer.Id,
                Field = GameField.GetFieldState(currentPlayer.Id),
                IsGameOver = false
            };
            await currentPlayer.Client.SendJson(new Message { GameState = gameStateForCurrent });

            var gameStateForOpponent = new GameStateMessage
            {
                PlayerId = opponent.Id,
                Field = GameField.GetFieldState(opponent.Id),
                IsGameOver = false
            };
            await opponent.Client.SendJson(new Message { GameState = gameStateForOpponent });
        }

        private async Task NotifyGameOver(Player loser, Player winner)
        {
            // Уведомляем проигравшего
            var loseMessage = new Message
            {
                GameState = new GameStateMessage
                {
                    IsGameOver = true,
                    WinnerId = winner.Id
                }
            };
            await loser.Client.SendJson(loseMessage);

            // Уведомляем победителя
            var winMessage = new Message
            {
                GameState = new GameStateMessage
                {
                    IsGameOver = true,
                    WinnerId = winner.Id
                }
            };
            await winner.Client.SendJson(winMessage);
        }

        public async Task ProcessMove(MoveMessage moveMessage)
        {
            // Находим игрока, который сделал ход
            var player = Players.FirstOrDefault(p => p.Id == moveMessage.PlayerId);
            if (player == null)
            {
                Console.WriteLine($"Игрок с ID {moveMessage.PlayerId} не найден.");
                return;
            }

            Console.WriteLine($"Игрок {player.Name} (ID: {player.Id}) сделал ход: разминирование ({moveMessage.RevealX}, {moveMessage.RevealY}), установка мины ({moveMessage.MineX}, {moveMessage.MineY}).");

            // Обработка хода
            var isExploded = GameField.TryRevealCell(moveMessage.RevealX, moveMessage.RevealY, player.Id);
            if (isExploded)
            {
                // Игрок взорвался
                Console.WriteLine($"Игрок {player.Name} (ID: {player.Id}) взорвался.");
                var localOpponent = Players.First(p => p.Id != player.Id);
                await NotifyGameOver(player, localOpponent); // Уведомляем о завершении игры
                return;
            }

            // Установка мины
            GameField.PlaceMine(moveMessage.MineX, moveMessage.MineY, player.Id);

            // Передача хода сопернику
            var opponent = Players.First(p => p.Id != player.Id);

            // Отправка обновлений состояния игры
            await SendGameState(player, opponent);
        }

        private async Task HandleChatMessage(ChatMessage chatMessage, Player sender)
        {
            // Рассылаем сообщение всем игрокам
            foreach (var player in Players)
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
