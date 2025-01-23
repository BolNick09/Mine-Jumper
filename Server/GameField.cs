using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mine_Jumper_class_library;

namespace Server
{
    public class GameField
    {
        public Size Size { get; }
        private readonly bool[,] player1Mines; // Мины игрока 1
        private readonly bool[,] player2Mines; // Мины игрока 2
        private readonly bool[,] revealed;     // Разминированные клетки

        public GameField(Size size)
        {
            Size = size;
            player1Mines = new bool[size.Width, size.Height];
            player2Mines = new bool[size.Width, size.Height];
            revealed = new bool[size.Width, size.Height];
        }

        // Попытка разминировать клетку
        public bool TryRevealCell(int x, int y, int playerId)
        {
            if (x < 0 || x >= Size.Width || y < 0 || y >= Size.Height)
                throw new ArgumentOutOfRangeException("Некорректные координаты клетки.");

            if (revealed[x, y])
                return false; // Клетка уже разминирована

            revealed[x, y] = true;

            // Проверка, есть ли мина соперника в этой клетке
            var opponentMines = playerId == 1 ? player2Mines : player1Mines;
            return opponentMines[x, y]; // true, если игрок взорвался
        }

        // Установка мины
        public void PlaceMine(int x, int y, int playerId)
        {
            if (x < 0 || x >= Size.Width || y < 0 || y >= Size.Height)
                throw new ArgumentOutOfRangeException("Некорректные координаты клетки.");

            var playerMines = playerId == 1 ? player1Mines : player2Mines;
            playerMines[x, y] = true;
        }

        // Получение состояния поля для конкретного игрока
        public CellState[,] GetFieldState(int playerId)
        {
            var fieldState = new CellState[Size.Width, Size.Height];
            var opponentMines = playerId == 1 ? player2Mines : player1Mines;
            var playerMines = playerId == 1 ? player1Mines : player2Mines;

            for (int x = 0; x < Size.Width; x++)
            {
                for (int y = 0; y < Size.Height; y++)
                {
                    fieldState[x, y] = new CellState
                    {
                        IsRevealed = revealed[x, y],
                        HasMine = opponentMines[x, y],
                        IsPlayerMine = playerMines[x, y]
                    };
                }
            }

            return fieldState;
        }
    }
}
