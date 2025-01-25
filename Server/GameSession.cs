using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using TcpLib;
using MineJumperClassLibrary;
using System.Collections.Concurrent;

namespace Server
{
    public class GameSession
    {
        public ConcurrentBag<Player> Players { get; } // Потокобезопасная коллекция игроков
        public GameField GameField { get; } // Игровое поле
        private int currentPlayerId = 1; // Текущий игрок (1 или 2)

        public GameSession(Player player1, Player player2, Size fieldSize)
        {
            // Инициализация списка игроков
            Players = new ConcurrentBag<Player> { player1, player2 };

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
            // Получаем игроков из ConcurrentBag
            var playersArray = Players.ToArray(); // Преобразуем ConcurrentBag в массив для удобства

            // Отправка начального состояния игры первому игроку
            var initialStatePlayer1 = new GameStateMessage
            {
                PlayerId = playersArray[0].Id,
                Field = GameField.GetFieldState(playersArray[0].Id),
                IsGameOver = false
            };
            await playersArray[0].Client.SendJson(new Message { GameState = initialStatePlayer1 });

            // Отправка начального состояния игры второму игроку
            var initialStatePlayer2 = new GameStateMessage
            {
                PlayerId = playersArray[1].Id,
                Field = GameField.GetFieldState(playersArray[1].Id),
                IsGameOver = false
            };
            await playersArray[1].Client.SendJson(new Message { GameState = initialStatePlayer2 });
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

        public async Task SendGameState(Player currentPlayer, Player opponent)
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

        public async Task NotifyGameOver(Player loser, Player winner)
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
    }
}
