using System.Drawing;
using System.Net;

namespace Server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            int width = GetUserInput("Введите ширину игрового поля (например, 10):");
            int height = GetUserInput("Введите высоту игрового поля (например, 10):");
            Console.WriteLine("Запуск сервера...");
            var server = new GameServer(IPAddress.Any, 2024, new Size(width, height));
            await server.Start();
        }
        private static int GetUserInput(string prompt)
        {
            int value;
            while (true)
            {
                Console.WriteLine(prompt);
                string input = Console.ReadLine();

                if (int.TryParse(input, out value) && value > 0)
                {
                    return value;
                }

                Console.WriteLine("Некорректный ввод. Пожалуйста, введите положительное число.");
            }
        }
    }
}
