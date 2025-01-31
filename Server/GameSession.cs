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
        public List<Player> Players { get; } // НЕпотокобезопасная коллекция игроков, исправление багов
        public GameField GameField { get; } // Игровое поле

        public GameSession(Size fieldSize)
        {
            Players = new List<Player>();
            this.GameField = new GameField(fieldSize);
        }

        public async Task StartGame()
        {
            Console.WriteLine("Начало новой игровой сессии.");
            await SendInitialState();            
        }

        public async Task SendInitialState()    // Отправка начального состояния игры
        {
            Player[] playersArray = Players.ToArray(); 
             
            GameStateMessage initialStatePlayer1 = new GameStateMessage
            {
                PlayerId = playersArray[0].Id,
                StrField = CellState.SerializeField(GameField.GetFieldState(playersArray[0].Id)),
                IsGameOver = false,
                CurrentPlayerId = playersArray[0].Id
            }; 
            GameStateMessage initialStatePlayer2 = new GameStateMessage
            {
                PlayerId = playersArray[1].Id,
                StrField = CellState.SerializeField(GameField.GetFieldState(playersArray[1].Id)),
                IsGameOver = false,
                CurrentPlayerId = playersArray[0].Id
            };
            await playersArray[0].Client.SendJson(new Message { GameState = initialStatePlayer1 });
            await playersArray[1].Client.SendJson(new Message { GameState = initialStatePlayer2 });
        }
        //Отправить состояние игры 
        public async Task SendGameState(Player currentPlayer, Player opponent)
        {
            GameStateMessage gameStateForCurrent = new GameStateMessage
            {
                PlayerId = currentPlayer.Id,
                StrField = CellState.SerializeField(GameField.GetFieldState(currentPlayer.Id)),
                IsGameOver = false,
                CurrentPlayerId = opponent.Id // Указываем, чей сейчас ход
            };           

            GameStateMessage gameStateForOpponent = new GameStateMessage
            {
                PlayerId = opponent.Id,
                StrField = CellState.SerializeField(GameField.GetFieldState(opponent.Id)),
                IsGameOver = false,
                CurrentPlayerId = opponent.Id 
            };
            await currentPlayer.Client.SendJson(new Message { GameState = gameStateForCurrent });
            await opponent.Client.SendJson(new Message { GameState = gameStateForOpponent });
        }
        public async Task NotifyGameOver(Player loser, Player winner)
        {
            // Уведомляем проигравшего
            Message loseMessage = new Message
            {
                GameState = new GameStateMessage
                {
                    IsGameOver = true,
                    WinnerId = winner.Id
                }
            };     
            // Уведомляем победителя
            Message winMessage = new Message
            {
                GameState = new GameStateMessage
                {
                    IsGameOver = true,
                    WinnerId = winner.Id
                }
            };
            await loser.Client.SendJson(loseMessage);
            await winner.Client.SendJson(winMessage);
        }
    }
}
