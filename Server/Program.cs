using System.Net;

namespace Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Запуск сервера...");
            var server = new GameServer(IPAddress.Any, 6666);
            server.Start();
            Console.WriteLine("Сервер запущен. Ожидание подключений...");

            Console.ReadLine(); // Чтобы сервер не завершал работу сразу
        }
    }
}
