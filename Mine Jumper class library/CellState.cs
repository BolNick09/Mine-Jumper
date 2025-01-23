using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Mine_Jumper_class_library
{
    public class CellState
    {
        [JsonPropertyName("isRevealed")]
        public bool IsRevealed { get; set; } // Открыта ли клетка

        [JsonPropertyName("hasMine")]
        public bool HasMine { get; set; } // Есть ли мина соперника

        [JsonPropertyName("isPlayerMine")]
        public bool IsPlayerMine { get; set; } // Есть ли мина игрока
    }
}
