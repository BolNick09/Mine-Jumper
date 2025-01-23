using System.Net.Sockets;
using System.Text.Json;

namespace TcpLib
{
    public static class Tcp
    {
        public static async Task SendInt32(this TcpClient client, int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);//int->byte[4]
            await client.GetStream().WriteAsync(bytes, 0, bytes.Length);//отправить 4 байта
        }

        public static async Task SendBytes(this TcpClient client, byte[] bytes)
        {
            await client.SendInt32(bytes.Length);//Сначала отправить длину
            await client.GetStream().WriteAsync(bytes);//затем содержимое
        }
        public static async Task SendString (this TcpClient client, string message)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message);
            await client.SendBytes(bytes);
        }
        //Вызвать так: await client.SendJson(new Student);
        public static async Task SendJson<T>(this TcpClient client, T item)
        {
            string json = JsonSerializer.Serialize(item);    //конвертировать в строку
            await client.SendString(json);                      //отправить строку
        }
        //Вызвать так: using Stream file = File.OpenRead("file.jpg");
        //              await client.SendFile(file);
        public static async Task SendFile (this TcpClient client, Stream file)
        {
            int length = (int)file.Length;
            await client.SendInt32(length);
            
            int sent = 0;
            byte[] buffer = new byte[1024];
            while (sent < length)
            {
                //зачерпнуть не более, чем осталось
                int read = await file.ReadAsync(buffer, 0, Math.Min(buffer.Length, length - sent));
                await client.GetStream().WriteAsync(buffer, 0, read);
                sent += read;
            }

        }

        //Вызвать так: int length = await client.ReceiveInt32();
        public static async Task<int> ReceiveInt32 (this TcpClient client)
        {
            byte[] bytes = new byte[sizeof(int)];
            await client.GetStream().ReadExactlyAsync(bytes, 0, bytes.Length);
            return BitConverter.ToInt32(bytes);
        }

        //Вызвать так: byte[] bytes = await client.ReceiveBytes();
        public static async Task<byte[]> ReceiveBytes (this TcpClient client)
        {
            int length = await client.ReceiveInt32();
            byte[] bytes = new byte[length];
            await client.GetStream().ReadExactlyAsync(bytes,0, length);
            return bytes;
        }

        public static async Task <string> ReceiveString (this TcpClient client)
        {
            byte[] bytes = await client.ReceiveBytes();
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        //Получить строку и конвертировать в объект
        public static async Task<T> ReceiveJson<T>(this TcpClient client) => 
            JsonSerializer.Deserialize<T>(await client.ReceiveString()) ?? throw new NullReferenceException();

        //Вызвать так: using Stream file = File.Create("file.jpg");
        //await client.ReceiveFile(file);
        public static async Task ReceiveFile (this TcpClient client, Stream stream)
        {
            int length = await client.ReceiveInt32 ();

            int pos = 0;
            byte[] buffer = new byte[1024];
            while (pos < length)
            {
                int remaining = Math.Min (length - pos, buffer.Length);
                int read = await client.GetStream().ReadAsync(buffer, 0, remaining);
                await stream.WriteAsync(buffer, 0, read);
                pos += read;
            }
        }

    }
}
