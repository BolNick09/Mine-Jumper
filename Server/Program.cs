using System.Net;

namespace Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Запуск сервера...");
            var server = new GameServer(IPAddress.Any, 2024);
            server.Start();

            Console.ReadLine(); // Чтобы сервер не завершал работу сразу
        }
    }
}
