using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MineJumperClassLibrary
{
    public class CellState
    {
        [JsonPropertyName("isRevealed")]
        public bool IsRevealed { get; set; } // Открыта ли клетка

        [JsonPropertyName("hasMine")]
        public bool HasMine { get; set; } // Есть ли мина соперника

        [JsonPropertyName("isPlayerMine")]
        public bool IsPlayerMine { get; set; } // Есть ли мина игрока

        // Метод для преобразования 2-мерного массива CellState в строку (JSON)
        public static string SerializeField(CellState[,] field)
        {
            List<List<CellState>> list = new List<List<CellState>>();

            for (int i = 0; i < field.GetLength(0); i++)
            {
                List<CellState> row = new List<CellState>();
                for (int j = 0; j < field.GetLength(1); j++)
                {
                    row.Add(field[i, j]);
                }
                list.Add(row);
            }

            return JsonSerializer.Serialize(list);
        }

        // Метод для преобразования строки (JSON) обратно в 2-мерный массив CellState
        public static CellState[,] DeserializeField(string json)
        {
            List<List<CellState>>? list = JsonSerializer.Deserialize<List<List<CellState>>>(json);
            int rows = list.Count;
            int cols = list[0].Count;

            CellState[,] field = new CellState[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    field[i, j] = list[i][j];
                }
            }

            return field;
        }
    }
}
