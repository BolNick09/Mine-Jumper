using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MineJumperClassLibrary
{
    public class Player
    {
        public int Id { get; set; } // Уникальный идентификатор игрока
        public string Name { get; set; } // Имя игрока
        public bool IsActive { get; set; } // Активен ли игрок (в игре или вышел)
        public bool IsMyTurn { get; set; } // Флаг, указывающий, чей сейчас ход
        public TcpClient Client { get; set; } // Сетевой клиент игрока
    }
}
