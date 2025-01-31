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
        public int Id { get; set; } 
        public string Name { get; set; } 
        public bool IsActive { get; set; } 
        public bool IsMyTurn { get; set; } 
        public TcpClient Client { get; set; } 
    }
}
