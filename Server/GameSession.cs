using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using TcpLib;
using Mine_Jumper_class_library;

namespace Server
{
    public class GameSession
    {
        public List<Player> Players { get; } // Список игроков в сессии
        private readonly GameField gameField; // Игровое поле
        private int currentPlayerId = 1; // Текущий игрок (1 или 2)

        public GameSession(Player player1, Player player2, Size fieldSize)
        {
            // Инициализация списка игроков
            Players = new List<Player> { player1, player2 };

            // Инициализация игрового поля
            this.gameField = new GameField(fieldSize);
        }

        public async Task StartGame()
        {
            Console.WriteLine("Начало новой игровой сессии.");
            await SendInitialState();

            while (true)
            {
                var currentPlayer = Players.First(p => p.Id == currentPlayerId);
                var opponent = Players.First(p => p.Id != currentPlayerId);

                // Получаем ход от текущего игрока
                var move = await currentPlayer.Client.ReceiveJson<MoveMessage>();

                // Обработка хода
                var isExploded = gameField.TryRevealCell(move.RevealX, move.RevealY, currentPlayerId);
                if (isExploded)
                {
                    await NotifyGameOver(currentPlayer, opponent);
                    break;
                }

                // Установка мины
                gameField.PlaceMine(move.MineX, move.MineY, currentPlayerId);

                // Передача хода сопернику
                currentPlayerId = currentPlayerId == 1 ? 2 : 1;

                // Отправка обновлений состояния игры
                await SendGameState(currentPlayer, opponent);
            }
        }

        private async Task SendInitialState()
        {
            // Отправка начального состояния игры
            var initialState = new GameStateMessage
            {
                PlayerId = 1,
                Field = gameField.GetFieldState(1),
                IsGameOver = false
            };
            await Players[0].Client.SendJson(new Message { GameState = initialState });

            initialState.PlayerId = 2;
            initialState.Field = gameField.GetFieldState(2);
            await Players[1].Client.SendJson(new Message { GameState = initialState });
        }

        private async Task NotifyGameOver(Player loser, Player winner)
        {
            var gameOverMessage = new GameStateMessage
            {
                IsGameOver = true,
                WinnerId = winner.Id
            };
            await loser.Client.SendJson(new Message { GameState = gameOverMessage });
            await winner.Client.SendJson(new Message { GameState = gameOverMessage });
        }

        private async Task SendGameState(Player currentPlayer, Player opponent)
        {
            var gameStateForCurrent = new GameStateMessage
            {
                PlayerId = currentPlayer.Id,
                Field = gameField.GetFieldState(currentPlayer.Id),
                IsGameOver = false
            };
            await currentPlayer.Client.SendJson(new Message { GameState = gameStateForCurrent });

            var gameStateForOpponent = new GameStateMessage
            {
                PlayerId = opponent.Id,
                Field = gameField.GetFieldState(opponent.Id),
                IsGameOver = false
            };
            await opponent.Client.SendJson(new Message { GameState = gameStateForOpponent });
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

    public class Move
    {
        public int X { get; set; } // Координаты клетки для разминирования
        public int Y { get; set; }
        public int MineX { get; set; } // Координаты клетки для установки мины
        public int MineY { get; set; }
    }
}
