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
        public JoinMessage? Join { get; set; }

        public MoveMessage? Move { get; set; }

        public LeaveMessage? Leave { get; set; }

        public GameStateMessage? GameState { get; set; }

        public ChatMessage? Chat { get; set; }
    }

    public class JoinMessage
    {
        public int PlayerId { get; set; }

        public string PlayerName { get; set; }

        public Size FieldSize { get; set; }
    }

    public class MoveMessage
    {
        public int PlayerId { get; set; }

        public int RevealX { get; set; } // Координаты клетки для разминирования

        public int RevealY { get; set; }

        public int MineX { get; set; } // Координаты клетки для установки мины

        public int MineY { get; set; }
    }

    public class LeaveMessage
    {
        public int PlayerId { get; set; }
    }

    public class GameStateMessage
    {
        public int PlayerId { get; set; }

        public int CurrentPlayerId { get; set; } // ID игрока, чей сейчас ход

        public String StrField { get; set; } // Состояние игрового поля для конкретного игрока

        public bool IsGameOver { get; set; }

        public int? WinnerId { get; set; } // ID победителя (если игра завершена)
    }

    public class ChatMessage
    {
        public string Text { get; set; } // Текст сообщения
    }
}
