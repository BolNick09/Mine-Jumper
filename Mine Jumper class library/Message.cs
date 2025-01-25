using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json.Serialization;
using System.Drawing;

namespace MineJumperClassLibrary
{


    public class Message
    {
        [JsonPropertyName("join")]
        public JoinMessage? Join { get; set; }

        [JsonPropertyName("move")]
        public MoveMessage? Move { get; set; }

        [JsonPropertyName("leave")]
        public LeaveMessage? Leave { get; set; }

        [JsonPropertyName("gameState")]
        public GameStateMessage? GameState { get; set; }

        [JsonPropertyName("chat")]
        public ChatMessage? Chat { get; set; }
    }

    public class JoinMessage
    {
        [JsonPropertyName("playerId")]
        public int PlayerId { get; set; }

        [JsonPropertyName("playerName")]
        public string PlayerName { get; set; }

        [JsonPropertyName("fieldSize")]
        public Size FieldSize { get; set; }
    }

    public class MoveMessage
    {
        [JsonPropertyName("playerId")]
        public int PlayerId { get; set; }

        [JsonPropertyName("revealX")]
        public int RevealX { get; set; } // Координаты клетки для разминирования

        [JsonPropertyName("revealY")]
        public int RevealY { get; set; }

        [JsonPropertyName("mineX")]
        public int MineX { get; set; } // Координаты клетки для установки мины

        [JsonPropertyName("mineY")]
        public int MineY { get; set; }
    }

    public class LeaveMessage
    {
        [JsonPropertyName("playerId")]
        public int PlayerId { get; set; }
    }

    public class GameStateMessage
    {
        [JsonPropertyName("playerId")]
        public int PlayerId { get; set; }

        [JsonPropertyName("field")]
        public CellState[,] Field { get; set; } // Состояние игрового поля для конкретного игрока

        [JsonPropertyName("isGameOver")]
        public bool IsGameOver { get; set; }

        [JsonPropertyName("winnerId")]
        public int? WinnerId { get; set; } // ID победителя (если игра завершена)
    }

    public class ChatMessage
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } // Текст сообщения

        [JsonPropertyName("senderId")]
        public int? SenderId { get; set; } // ID отправителя (опционально)
    }
}
