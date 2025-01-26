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
        public List<Player> Players { get; } // НЕпотокобезопасная коллекция игроков, исправление багов
        public GameField GameField { get; } // Игровое поле
        private Player currentPlayer; // Текущий игрок

        public GameSession(Size fieldSize)
        {
            Players = new List<Player>();
            this.GameField = new GameField(fieldSize);
        }

        public async Task StartGame()
        {
            Console.WriteLine("Начало новой игровой сессии.");
            await SendInitialState();

            //// Устанавливаем первого игрока как текущего
            //currentPlayer = Players.First();

            //while (true)
            //{
            //    Player opponent = Players.First(player => player.Id != currentPlayer.Id);

            //    // Получаем ход от текущего игрока
            //    MoveMessage move = await ReceiveMove(currentPlayer);

            //    // Обработка хода
            //    bool isExploded = GameField.TryRevealCell(move.RevealX, move.RevealY, currentPlayer.Id);
            //    if (isExploded)
            //    {
            //        await NotifyGameOver(currentPlayer, opponent);
            //        break;
            //    }

            //    // Установка мины
            //    GameField.PlaceMine(move.MineX, move.MineY, currentPlayer.Id);

            //    // Передача хода сопернику
            //    currentPlayer = opponent;

            //    // Отправка обновлений состояния игры
            //    await SendGameState(currentPlayer, opponent);
            //}
        }

        public async Task SendInitialState()
        {
            // Получаем игроков из ConcurrentBag
            Player[] playersArray = Players.ToArray(); // Преобразуем ConcurrentBag в массив для удобства

            // Отправка начального состояния игры первому игроку
            GameStateMessage initialStatePlayer1 = new GameStateMessage
            {
                PlayerId = playersArray[0].Id,
                StrField = CellState.SerializeField(GameField.GetFieldState(playersArray[0].Id)),
                IsGameOver = false,
                CurrentPlayerId = playersArray[0].Id
            };
            try
            {
                await playersArray[0].Client.SendJson(new Message { GameState = initialStatePlayer1 });
            }
            catch (Exception ex)
            {
                string str = ex.Message;
            }


            // Отправка начального состояния игры второму игроку
            GameStateMessage initialStatePlayer2 = new GameStateMessage
            {
                PlayerId = playersArray[1].Id,
                StrField = CellState.SerializeField(GameField.GetFieldState(playersArray[1].Id)),
                IsGameOver = false,
                CurrentPlayerId = playersArray[0].Id
            };
            await playersArray[1].Client.SendJson(new Message { GameState = initialStatePlayer2 });
        }

        private async Task<MoveMessage> ReceiveMove(Player player)
        {
            // Получаем ход от игрока
            Message message = await player.Client.ReceiveJson<Message>();

            if (message?.Move != null)
            {
                return message.Move;
            }

            throw new InvalidOperationException("Получено некорректное сообщение.");
        }

        public async Task SendGameState(Player currentPlayer, Player opponent)
        {
            GameStateMessage gameStateForCurrent = new GameStateMessage
            {
                PlayerId = currentPlayer.Id,
                StrField = CellState.SerializeField(GameField.GetFieldState(currentPlayer.Id)),
                IsGameOver = false,
                CurrentPlayerId = opponent.Id // Указываем, чей сейчас ход
            };
            await currentPlayer.Client.SendJson(new Message { GameState = gameStateForCurrent });

            GameStateMessage gameStateForOpponent = new GameStateMessage
            {
                PlayerId = opponent.Id,
                StrField = CellState.SerializeField(GameField.GetFieldState(opponent.Id)),
                IsGameOver = false,
                CurrentPlayerId = opponent.Id 
            };
            await opponent.Client.SendJson(new Message { GameState = gameStateForOpponent });
        }


        public async Task NotifyGameOver(Player loser, Player winner)
        {
            // Сбрасываем IsMyTurn для обоих игроков
            loser.IsMyTurn = false;
            winner.IsMyTurn = false;

            // Уведомляем проигравшего
            Message loseMessage = new Message
            {
                GameState = new GameStateMessage
                {
                    IsGameOver = true,
                    WinnerId = winner.Id
                }
            };
            await loser.Client.SendJson(loseMessage);

            // Уведомляем победителя
            Message winMessage = new Message
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
